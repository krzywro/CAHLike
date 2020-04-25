﻿using KrzyWro.CAH.Server.Helpers;
using KrzyWro.CAH.Server.Services;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ServerMessages;
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
    public class PlayerHub : Hub, IPlayerHub
    {
        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<Guid, string> PlayerToNames = new ConcurrentDictionary<Guid, string>();
        public static ConcurrentDictionary<Guid, int> PlayerToScore = new ConcurrentDictionary<Guid, int>();
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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionToPlayer.TryRemove(Context.ConnectionId, out var playerId);
            PlayerToNames.TryGetValue(playerId, out var playerName);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(Context.ConnectionId); return v; });
            _logger.LogInformation($"[Disconnected {Context.ConnectionId}] Player: {playerName} ({playerId})");
            await SendScoresToAllClients(); 
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterPlayer(Guid playerId, string playerName)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>() { Context.ConnectionId }, (x, v) => { v.Add(Context.ConnectionId); return v; });
            PlayerToNames.AddOrUpdate(playerId, playerName, (x, v) => playerName);
            PlayerToScore.AddOrUpdate(playerId, 0, (x, v) => v);
            ConnectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (x, v) => playerId);
            _logger.LogInformation($"[Connected {Context.ConnectionId}] Player: {playerName} ({playerId})");
            await SendScoresToAllClients();
            await Clients.Caller.SendMessageAsync<IGreetMessage>();
        }

        public async Task RequestQuestion()
        {
            var question = await _deckService.PeekQuestion();
            await Clients.Caller.SendMessageAsync<IQuestionMessage, QuestionModel>(question);
        }
        public async Task RequestScores()
            => await Clients.Caller.SendMessageAsync<ISendScores, List<ScoreRow>>(CreateScoresToSend());

        public async Task RequestHand()
        {
            var collectedAnswersString = await _cache.GetStringAsync("collectedAnswers");
            var collectedAnswers = string.IsNullOrEmpty(collectedAnswersString)
                ? new List<Guid>()
                : JsonSerializer.Deserialize<List<Guid>>(collectedAnswersString);
            if(collectedAnswers.Contains(ConnectionToPlayer[Context.ConnectionId]))
            {
                var value = await _cache.GetStringAsync($"{CachePlayerAnswerPrefix}{ConnectionToPlayer[Context.ConnectionId]}");
                var awaitingAnswers = JsonSerializer.Deserialize<List<AnswerModel>>(value);
                await Clients.Caller.SendMessageAsync<IRestoreSelectedAnswers, List<AnswerModel>>(awaitingAnswers);
                if (collectedAnswers.IndexOf(ConnectionToPlayer[Context.ConnectionId]) == 0)
                {
                    var collectedAnswersValues = new List<List<AnswerModel>>();
                    foreach (var playerId in collectedAnswers)
                    {
                        var cacheValue = await _cache.GetStringAsync($"{CachePlayerAnswerPrefix}{playerId}");
                        collectedAnswersValues.Add(JsonSerializer.Deserialize<List<AnswerModel>>(cacheValue));
                    }
                    await Clients.Caller.SendMessageAsync<ISelectBestAnswer, List<List<AnswerModel>>>(collectedAnswersValues);
                }
                else
                    await Clients.Caller.SendMessageAsync<IWaitForOtherPlayers>();
                return;
            }


            var playerHandString = await _cache.GetStringAsync($"{CachePlayerHandPrefix}{ConnectionToPlayer[Context.ConnectionId]}");
            var playerHand = string.IsNullOrEmpty(playerHandString)
                ? new List<AnswerModel>()
                : JsonSerializer.Deserialize<List<AnswerModel>>(playerHandString);

            while (playerHand.Count < 10)
            {
                var answer = await _deckService.PopAnswer();
                if (answer != null)
                    playerHand.Add(answer);
                else
                    break;
            }

            playerHandString = JsonSerializer.Serialize(playerHand);
            await _cache.SetStringAsync($"{CachePlayerHandPrefix}{ConnectionToPlayer[Context.ConnectionId]}", playerHandString);

            await Clients.Caller.SendMessageAsync<IHandMessage, List<AnswerModel>>(playerHand);
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

            if (collectedAnswers.Count < PlayerToConnections.Count(x => x.Value.Any()))
            {
                foreach (var connection in PlayerToConnections[ConnectionToPlayer[Context.ConnectionId]])
                {
                    await Clients.Client(connection).SendMessageAsync<IWaitForOtherPlayers>();
                }
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
                    await Clients.Client(connection).SendMessageAsync<IWaitForBestAnswer, List<List<AnswerModel>>>(collectedAnswersValues);
                }

                foreach (var connection in PlayerToConnections[pickerId])
                {
                    await Clients.Client(connection).SendMessageAsync<ISelectBestAnswer, List<List<AnswerModel>>>(collectedAnswersValues);
                }
            }

            collectedAnswersString = JsonSerializer.Serialize(collectedAnswers);
            await _cache.SetStringAsync("collectedAnswers", collectedAnswersString);
        }

        public async Task PickAnswer(List<AnswerModel> answers)
        {
            await _deckService.PrepareNextQuestion();
            var collectedAnswersString = await _cache.GetStringAsync("collectedAnswers");
            var collectedAnswers = string.IsNullOrEmpty(collectedAnswersString)
                ? new List<Guid>()
                : JsonSerializer.Deserialize<List<Guid>>(collectedAnswersString);

            var playerName = string.Empty;
            var answersGuids = answers.Select(x => x.Id).ToArray();

            foreach (var playerId in collectedAnswers)
            {
                var value = await _cache.GetStringAsync($"{CachePlayerAnswerPrefix}{playerId}");
                var cachedAnswers = JsonSerializer.Deserialize<List<AnswerModel>>(value);
                if (answersGuids.Intersect(cachedAnswers.Select(x => x.Id)).Any())
                {
                    PlayerToNames.TryGetValue(playerId, out playerName);
                    PlayerToScore.AddOrUpdate(playerId, 1, (k, v) => v + 1);
                    break;
                }
            }

            await Clients.All.SendMessageAsync<IBestAnswerPicked, List<AnswerModel>, string>(answers, playerName);
            await SendScoresToAllClients();
            await _cache.RemoveAsync("collectedAnswers");
        }

        private async Task SendScoresToAllClients()
            => await Clients.All.SendMessageAsync<ISendScores, List<ScoreRow>>(CreateScoresToSend());

        private List<ScoreRow> CreateScoresToSend()
            => PlayerToScore
                .OrderByDescending(x => x.Value)
                .Select(x => new ScoreRow { PlayerName = PlayerToNames[x.Key], Score = x.Value, Online = PlayerToConnections[x.Key].Any() })
                .ToList();
    }
}
