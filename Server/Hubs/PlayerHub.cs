using KrzyWro.CAH.Server.Services;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Hubs
{
    public class PlayerHub : Hub
    {
        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<string, Guid> ConnectionToPlayer = new ConcurrentDictionary<string, Guid>();

        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;
        private readonly IDeckService _deckService;

        private const string CachePlayerHandPrefix = "player_hand_";
        private const string CachePlayerAnswerPrefix = "player_answer_";

        public PlayerHub(ILogger<PlayerHub> logger, IDistributedCache cache, IDeckService deckService)
        {
            _logger = logger;
            _cache = cache;
            _deckService = deckService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionToPlayer.TryRemove(Context.ConnectionId, out var playerId);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(Context.ConnectionId); return v; });
            if (PlayerToConnections[playerId]?.Count == 0)
                PlayerToConnections.TryRemove(playerId, out _);
            _logger.LogInformation($"[Disconnected {Context.ConnectionId}] Player: {playerId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterPlayer(Guid playerId, string playerName)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>() { Context.ConnectionId }, (x, v) => { v.Add(Context.ConnectionId); return v; });
            ConnectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (x, v) => playerId);
            _logger.LogInformation($"[Connected {Context.ConnectionId}] Player: {playerId}");

            await Clients.Caller.SendAsync("Greet");
        }

        public async Task RequestQuestion()
        {
            var question = await _deckService.PeekQuestion();
            await Clients.Caller.SendAsync("GetQuestion", question);
        }

        public async Task RequestHand()
        {
            var playerHandString = await _cache.GetStringAsync($"{CachePlayerHandPrefix}{ConnectionToPlayer[Context.ConnectionId]}");
            var playerHand = string.IsNullOrEmpty(playerHandString)
                ? new List<AnswerModel>()
                : JsonSerializer.Deserialize<List<AnswerModel>>(playerHandString);

            while (playerHand.Count < 5)
            {
                var answer = await _deckService.PopAnswer();
                if (answer != null)
                    playerHand.Add(answer);
                else
                    break;
            }

            playerHandString = JsonSerializer.Serialize(playerHand);
            await _cache.SetStringAsync($"{CachePlayerHandPrefix}{ConnectionToPlayer[Context.ConnectionId]}", playerHandString);

            await Clients.Caller.SendAsync("GetHand", playerHand);
        }

        public async Task SendAnswers(List<AnswerModel> answers)
        {
            var collectedAnswersString = await _cache.GetStringAsync("collectedAnswers");
            var collectedAnswers = string.IsNullOrEmpty(collectedAnswersString)
                ? new List<Guid>()
                : JsonSerializer.Deserialize<List<Guid>>(collectedAnswersString);

            var playerHandString = await _cache.GetStringAsync($"{CachePlayerHandPrefix}{ConnectionToPlayer[Context.ConnectionId]}");
            var playerHand = JsonSerializer.Deserialize<List<AnswerModel>>(playerHandString);
            playerHand = playerHand.Where(hand => !answers.Select(a => a.Id).Contains(hand.Id)).ToList();
            playerHandString = JsonSerializer.Serialize(playerHand);
            await _cache.SetStringAsync($"{CachePlayerHandPrefix}{ConnectionToPlayer[Context.ConnectionId]}", playerHandString);

            await _cache.SetStringAsync($"{CachePlayerAnswerPrefix}{ConnectionToPlayer[Context.ConnectionId]}", JsonSerializer.Serialize(answers));

            collectedAnswers.Add(ConnectionToPlayer[Context.ConnectionId]);

            if (collectedAnswers.Count < PlayerToConnections.Count)
            {
                foreach (var connection in PlayerToConnections[ConnectionToPlayer[Context.ConnectionId]])
                {
                    await Clients.Client(connection).SendAsync("WaitForOtherPlayers");
                }
                collectedAnswersString = JsonSerializer.Serialize(collectedAnswers);
                await _cache.SetStringAsync("collectedAnswers", collectedAnswersString);
            }
            else
            {
                var pickerId = collectedAnswers.First();

                var collectedAnswersValues = new List<List<AnswerModel>>();
                foreach(var playerId in collectedAnswers)
                {
                    var value = await _cache.GetStringAsync($"{CachePlayerAnswerPrefix}{playerId}");
                    collectedAnswersValues.Add(JsonSerializer.Deserialize<List<AnswerModel>>(value));
                }

                foreach (var connection in PlayerToConnections.Where(x => x.Key != pickerId).SelectMany(x => x.Value))
                {
                    await Clients.Client(connection).SendAsync("WaitForBestAnswerPick", collectedAnswersValues);
                }

                foreach (var connection in PlayerToConnections[pickerId])
                {
                    await Clients.Client(connection).SendAsync("SelectBestAnswer", collectedAnswersValues);
                }
            }
        }

        public async Task PickAnswer(List<AnswerModel> answers)
        {
            await _deckService.PrepareNextQuestion();
            await Clients.All.SendAsync("BestAnswerPick", answers);
            await _cache.RemoveAsync("collectedAnswers");
        }
    }
}
