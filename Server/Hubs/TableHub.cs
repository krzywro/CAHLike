using KrzyWro.CAH.Server.Helpers;
using KrzyWro.CAH.Server.Services;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ServerMessages.Table;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Hubs
{
    public class TableHub : Hub, ITableHub
    {
        private readonly IGamesService _gamesService;
        private readonly IPlayerPoolService _playerPoolService;
        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<string, Guid> ConnectionToPlayer = new ConcurrentDictionary<string, Guid>();


        public TableHub(IGamesService gamesService, IPlayerPoolService playerPoolService)
        {
            _gamesService = gamesService;
            _playerPoolService = playerPoolService;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionToPlayer.TryRemove(Context.ConnectionId, out var playerId);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(Context.ConnectionId); return v; });
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Join(Guid game, Guid playerId)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>() { Context.ConnectionId }, (x, v) => { v.Add(Context.ConnectionId); return v; });
            ConnectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (x, v) => playerId);
            var playerName = await _playerPoolService.GetName(playerId);
            var table = await _gamesService.Get(game);
            
            if (table.Players.Count == 0)
                table.CurrentMaster = playerId;

            if (table.Players.Contains(playerId))
                return;

            table.Players.Add(playerId);
            table.Scores.AddOrUpdate(playerId, 0, (x, v) => v);

            var hand = new List<AnswerModel>();
            for (int i = 0; i < 10; i++)
            {
                table.AnswerDeck.TryPop(out var card);
                hand.Add(card);
            }
            table.Hands.AddOrUpdate(playerId, hand, (x, v) => v);
            await Clients.Caller.SendMessageAsync<ITablePlayerJoined, string>(game, playerName);
            await SendScoresToAllClients(game);
        }

        public async Task RequestQuestion(Guid game)
        {
            var table = await _gamesService.Get(game);
            await Clients.Caller.SendMessageAsync<ITableSendQuestion, QuestionModel>(game, table.CurrentQuestion);
        }

        public async Task RequestScores(Guid game)
        {
            await Clients.Caller.SendMessageAsync<ITableSendScores, List<ScoreRow>>(game, await CreateScoresToSend(game));
        }

        public async Task RequestHand(Guid game)
        {
            var player = ConnectionToPlayer[Context.ConnectionId];
            var table = await _gamesService.Get(game);

            if (table.CollectedAnswers.ContainsKey(player))
            {
                table.CollectedAnswers.TryGetValue(player, out var awaitingAnswers);
                await Clients.Caller.SendMessageAsync<ITablePlayerRestoreSelectedAnswers, List<AnswerModel>>(game, awaitingAnswers);
                await Clients.Caller.SendMessageAsync<ITablePlayerWaitForOtherPlayers, Guid>(table.Id);
            }
            else if (table.CurrentMaster == player && table.CollectedAnswers.Count > 0 && table.CollectedAnswers.Count == table.Players.Count - 1)
            {
                await Clients.Caller.SendMessageAsync<ITableMasterRequestSelection, List<List<AnswerModel>>>(game, table.CollectedAnswers.Select(x => x.Value).ToList());
            }
            else if (table.CurrentMaster == player)
            {
                await Clients.Caller.SendMessageAsync<ITableMasterNomination, Guid>(table.Id);
            }
            else
            {
                table.Hands.TryGetValue(player, out var hand);
                while (hand.Count < 10)
                {
                    table.AnswerDeck.TryPop(out var answer);
                    if (answer != null)
                        hand.Add(answer);
                    else
                        break;
                }

                await Clients.Caller.SendMessageAsync<ITablePlayerSendHand, List<AnswerModel>>(game, hand);
            }
        }

        public async Task SendPlayerAnswers(Guid game, List<AnswerModel> answers)
        {
            var player = ConnectionToPlayer[Context.ConnectionId];
            var table = await _gamesService.Get(game);

            table.Hands[player] = table.Hands[player].Where(hand => !answers.Select(a => a.Id).Contains(hand.Id)).ToList();

            table.CollectedAnswers.AddOrUpdate(player, answers, (x, v) => v);


            if (table.CollectedAnswers.Count < table.Players.Count - 1)
            {
                var connections = PlayerToConnections[player];
                foreach (var connection in connections)
                {
                    await Clients.Client(connection).SendMessageAsync<ITablePlayerWaitForOtherPlayers, Guid>(game);
                }
            }
            else
            {
                var pickerId = table.CurrentMaster;

                var collectedAnswersValues = table.CollectedAnswers.Select(x => x.Value).ToList();

                foreach (var otherPlayer in table.Players.Where(x => x != pickerId))
                {
                    foreach (var connection in PlayerToConnections[otherPlayer])
                    {
                        await Clients.Client(connection).SendMessageAsync<ITablePlayerWaitForSelection, List<List<AnswerModel>>>(game, collectedAnswersValues);
                    }
                }

                foreach (var connection in PlayerToConnections[pickerId])
                {
                    await Clients.Client(connection).SendMessageAsync<ITableMasterRequestSelection, List<List<AnswerModel>>>(game, table.CollectedAnswers.Select(x => x.Value).ToList());
                }
            }
        }

        public async Task SendMasterAnswers(Guid game, List<AnswerModel> answers)
        {
            var table = await _gamesService.Get(game);
            table.CurrentQuestion = table.QuestionDeck.Pop();

            var winnerName = string.Empty;
            var answersGuids = answers.Select(x => x.Id).ToArray();

            foreach (var collectedAnswers in table.CollectedAnswers)
            {
                if (answersGuids.Intersect(collectedAnswers.Value.Select(x => x.Id)).Any())
                {
                    winnerName = await _playerPoolService.GetName(collectedAnswers.Key);
                    table.Scores.AddOrUpdate(collectedAnswers.Key, 1, (k, v) => v + 1);
                    break;
                }
            }

            table.PreviousMasters.Add(table.CurrentMaster);
            var candidate = table.Players.Except(table.PreviousMasters).FirstOrDefault();
            if (candidate == Guid.Empty)
            {
                table.PreviousMasters.Clear();
                candidate = table.Players.First();
            }
            table.CurrentMaster = candidate;
            table.CollectedAnswers.Clear();

            await Clients.All.SendMessageAsync<ITableSendBestAnswer, List<AnswerModel>, string>(game, answers, winnerName);
            await SendScoresToAllClients(game);
        }

        private async Task SendScoresToAllClients(Guid game)
            => await Clients.All.SendMessageAsync<ITableSendScores, List<ScoreRow>>(game, await CreateScoresToSend(game));

        private async Task<List<ScoreRow>> CreateScoresToSend(Guid game)
        {
            var table = await _gamesService.Get(game);
            var scores = new List<ScoreRow>();
            foreach (var entry in table.Scores.OrderByDescending(x => x.Value))
            {
                var name = await _playerPoolService.GetName(entry.Key);
                var online = PlayerToConnections[entry.Key].Any();
                scores.Add(new ScoreRow { Score = entry.Value, Online = online, PlayerName = name });
            }

            return scores;
        }
    }

    
}
