using KrzyWro.CAH.Client.Helpers;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ClientMessages.Table;
using KrzyWro.CAH.Shared.Contracts.ServerMessages.Table;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement.TableState
{
    public class TableHubClient : ITableHubClient
    {
        private readonly HubConnection _hubConnection;
        private Guid _game;

        public TableHubClient(NavigationManager NavigationManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(ITableHub.Path))
                .WithAutomaticReconnect()
                .Build();
        }

        public void OnConnected(Func<string, Task> action) => _hubConnection.Reconnected += action;
        public void OnDisconnected(Func<Exception, Task> action) => _hubConnection.Reconnecting += action;

        public Task Init(Guid game)
        {
            _game = game;
            if (_hubConnection.State == HubConnectionState.Connected)
                _hubConnection.StopAsync();
            return _hubConnection.StartAsync();
        }

        public void OnTableMasterNomination(Func<Task> action) => _hubConnection.OnMessage<ITableMasterNomination, Guid>(
             msgGame => msgGame == _game ? action() : Task.CompletedTask);

        public void OnTableMasterRequestSelection(Func<List<List<AnswerModel>>, Task> action) => _hubConnection.OnMessage<ITableMasterRequestSelection, Guid, List<List<AnswerModel>>>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public void OnTablePlayerJoined(Func<string, Task> action) => _hubConnection.OnMessage<ITablePlayerJoined, Guid, string>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public void OnTablePlayerRestoreSelectedAnswers(Func<List<AnswerModel>, Task> action) => _hubConnection.OnMessage<ITablePlayerRestoreSelectedAnswers, Guid, List<AnswerModel>>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public void OnTablePlayerSendHand(Func<List<AnswerModel>, Task> action) => _hubConnection.OnMessage<ITablePlayerSendHand, Guid, List<AnswerModel>>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public void OnTablePlayerWaitForOtherPlayers(Func<Task> action) => _hubConnection.OnMessage<ITablePlayerWaitForOtherPlayers, Guid>(
            msgGame => msgGame == _game ? action() : Task.CompletedTask);

        public void OnTablePlayerWaitForSelection(Func<List<List<AnswerModel>>, Task> action) => _hubConnection.OnMessage<ITablePlayerWaitForSelection, Guid, List<List<AnswerModel>>>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public void OnTableSendBestAnswer(Func<List<AnswerModel>, string, Task> action) => _hubConnection.OnMessage<ITableSendBestAnswer, Guid, List<AnswerModel>, string>(
            (msgGame, payload1, payload2) => msgGame == _game ? action(payload1, payload2) : Task.CompletedTask);

        public void OnTableSendQuestion(Func<QuestionModel, Task> action) => _hubConnection.OnMessage<ITableSendQuestion, Guid, QuestionModel>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public void OnTableSendScores(Func<List<ScoreRow>, Task> action) => _hubConnection.OnMessage<ITableSendScores, Guid, List<ScoreRow>>(
            (msgGame, payload) => msgGame == _game ? action(payload) : Task.CompletedTask);

        public Task Join(Guid playerId) => _hubConnection.SendMessageAsync<ITablePlayerJoin, Guid>(_game, playerId);

        public Task SendRequestHand() => _hubConnection.SendMessageAsync<ITablePlayerRequestHand>(_game);

        public Task SendRequestQuestion() => _hubConnection.SendMessageAsync<ITablePlayerRequestQuestion>(_game);

        public Task SendRequestScores() => _hubConnection.SendMessageAsync<ITablePlayerRequestScores>(_game);

        public Task MasterSendAnswers(List<AnswerModel> answers) => _hubConnection.SendMessageAsync<ITableMasterSendAnswers, List<AnswerModel>>(_game, answers);

        public Task PlayerSendAnswers(List<AnswerModel> answers) => _hubConnection.SendMessageAsync<ITablePlayerSendAnswers, List<AnswerModel>>(_game, answers);

    }
}
