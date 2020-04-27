using KrzyWro.CAH.Client.Helpers;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ClientMessages;
using KrzyWro.CAH.Shared.Contracts.ServerMessages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public class PlayerHubClient : IPlayerHubClient
    {
        private readonly HubConnection _hubConnection;

        public PlayerHubClient(NavigationManager NavigationManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(IPlayerHub.Path))
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task Init() => await _hubConnection.StartAsync();

        public void OnGreet(Func<Task> action) => _hubConnection.OnMessage<IGreetMessage>(action);
        public void OnReceivingQuestion(Func<QuestionModel, Task> action) => _hubConnection.OnMessage<IQuestionMessage, QuestionModel>(action);
        public void OnReceivingHand(Func<List<AnswerModel>, Task> action) => _hubConnection.OnMessage<IHandMessage, List<AnswerModel>>(action);
        public void OnWaitForOtherPlayers(Func<Task> action) => _hubConnection.OnMessage<IWaitForOtherPlayers>(action);
        public void OnWaitForBestAnswer(Func<List<List<AnswerModel>>, Task> action) => _hubConnection.OnMessage<IWaitForBestAnswer, List<List<AnswerModel>>>(action);
        public void OnSelectBestAnswer(Func<List<List<AnswerModel>>, Task> action) => _hubConnection.OnMessage<ISelectBestAnswer, List<List<AnswerModel>>>(action);
        public void OnBestAnswerPicked(Func<List<AnswerModel>, string, Task> action) => _hubConnection.OnMessage<IBestAnswerPicked, List<AnswerModel>, string>(action);
        public void OnRecevingScores(Func<List<ScoreRow>, Task> action) => _hubConnection.OnMessage<ISendScores, List<ScoreRow>>(action);
        public void OnReceivingRestoreSelectedAnswers(Func<List<AnswerModel>, Task> action) => _hubConnection.OnMessage<IRestoreSelectedAnswers, List<AnswerModel>>(action);

        public async Task SendRegisterPlayer(Player player) => await _hubConnection.SendMessageAsync<IPlayerHubRegisterPlayer, Guid, string>(player.Id, player.Name);
        public async Task SendRequestScores() => await _hubConnection.SendMessageAsync<IPlayerHubRequestScores>();
        public async Task SendRequestQuestion() => await _hubConnection.SendMessageAsync<IPlayerHubRequestQuestion>();
        public async Task SendRequestHand() => await _hubConnection.SendMessageAsync<IPlayerHubRequestHand>();
        public async Task SendAnswers(List<AnswerModel> answers) => await _hubConnection.SendMessageAsync<IPlayerHubSendAnswers, List<AnswerModel>>(answers);
        public async Task SendPickedBestAnswers(List<AnswerModel> answers) => await _hubConnection.SendMessageAsync<IPlayerHubPickAnswer, List<AnswerModel>>(answers);
    }
}
