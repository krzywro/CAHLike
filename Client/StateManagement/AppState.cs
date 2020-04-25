using Blazored.LocalStorage;
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
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public class AppState
    {
        private readonly ILocalStorageService _localStorage;

        public AppEvents Events { get; } = new AppEvents();
        public HubConnection PlayerHubConnection;
        public async Task EnsurePlayerHubConnection() => await PlayerHubConnection?.StartAsync();

        public async Task RegisterPlayer()
        {
            var player = await _localStorage.GetItemAsync<Player>("player");
            if (player == null)
            {
                player = new Player { Id = Guid.NewGuid() };
                await _localStorage.SetItemAsync("player", player);
            }
            Player = player;
            await Events.PlayerNameChanged.RaiseAsync();
            await PlayerHubConnection?.SendMessageAsync<IPlayerHubRegisterPlayer, Guid, string>(player.Id, Player.Name);
            await PlayerHubConnection?.SendMessageAsync<IPlayerHubRequestScores>();
		}

		public async Task RequestQuestion()
        {
            PlayerAnswers.Clear();
            BestAnswer.Clear();
            BestAnswerPlayerName = string.Empty;
            _selectedAnswers.Clear();
            await Events.OnAnswerSelectionChange.RaiseAsync();
            await PlayerHubConnection?.SendMessageAsync<IPlayerHubRequestQuestion>();
        }

        public async Task RequestHand() => await PlayerHubConnection?.SendMessageAsync<IPlayerHubRequestHand>();

        public async Task SendAnswers()
        {
            await PlayerHubConnection?.SendMessageAsync<IPlayerHubSendAnswers, List<AnswerModel>>(SelectedAnswers.ToList());
            Hand = new List<AnswerModel>();
        }

        public async Task PickAnswer() => await PlayerHubConnection?.SendMessageAsync<IPlayerHubPickAnswer, List<AnswerModel>>(BestAnswer);

        private List<AnswerModel> _selectedAnswers = new List<AnswerModel>();
        public IReadOnlyList<AnswerModel> SelectedAnswers => _selectedAnswers;

        public List<List<AnswerModel>> PlayerAnswers { get; private set; } = new List<List<AnswerModel>>();
        public List<AnswerModel> BestAnswer { get; set; } = new List<AnswerModel>();
        public string BestAnswerPlayerName { get; private set; } = string.Empty;

        public int GetAnswerSelectionNumber(AnswerModel id) => _selectedAnswers.IndexOf(id) + 1;

        public async Task ToggleAnswer(AnswerModel id)
        {

            if (_selectedAnswers.Contains(id))
                _selectedAnswers.Remove(id);
            else if (_selectedAnswers.Count < CurrentQuestion.AnswerCards)
                _selectedAnswers.Add(id);

            await Events.OnAnswerSelectionChange.RaiseAsync();
        }

        public QuestionModel CurrentQuestion { get; private set; }
        public List<AnswerModel> Hand { get; private set; } = new List<AnswerModel>();

        public Player Player { get; private set; } = new Player();

        public IList<ScoreRow> Scores { get; private set; } = new List<ScoreRow>();

        public async Task SetPlayerName(string name)
        {
            Player.Name = name;
            await _localStorage.SetItemAsync("player", Player);
            await RegisterPlayer();
        }

        public AppState(ISyncLocalStorageService syncLocalStorage, ILocalStorageService localStorage, NavigationManager NavigationManager)
        {
            _localStorage = localStorage;

            var player = syncLocalStorage.GetItem<Player>("player");
            if (player == null)
            {
                player = new Player { Id = Guid.NewGuid(), Name = "Nowy gracz" };
                syncLocalStorage.SetItem("player", player);
            }
            Player = player;

            PlayerHubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/playerhub"))
                .WithAutomaticReconnect()
                .Build();

            PlayerHubConnection.OnMessage<IGreetMessage>(async () =>
            {
                await Events.ServerGreeting.RaiseAsync();
            });

            PlayerHubConnection.OnMessage<IQuestionMessage, QuestionModel>(async question =>
            {
                CurrentQuestion = question;
                await Events.OnQuestionRetrival.RaiseAsync();
            });

            PlayerHubConnection.OnMessage<IHandMessage, List<AnswerModel>>(async hand =>
            {
                Hand = hand;
                await Events.OnHandRetrival.RaiseAsync();
            });

            PlayerHubConnection.OnMessage<IWaitForOtherPlayers>(async () =>
            {
                await Events.OnWaitForOtherPlayers.RaiseAsync();
            });

            PlayerHubConnection.OnMessage<IWaitForBestAnswer, List<List<AnswerModel>>>(async hand =>
            {
                PlayerAnswers = hand;
                await Events.OnWaitForBestPick.RaiseAsync();
            });

            PlayerHubConnection.OnMessage<ISelectBestAnswer, List<List<AnswerModel>>>(async hand =>
            {
                PlayerAnswers = hand;
                await Events.OnSelectBestAnswer.RaiseAsync();
            });

            PlayerHubConnection.OnMessage<IBestAnswerPicked, List<AnswerModel>, string>(async (answers, playerName) =>
            {
                PlayerAnswers = new List<List<AnswerModel>>();
                BestAnswer = answers;
                BestAnswerPlayerName = playerName;
                await Events.OnBestPick.RaiseAsync();
            });
            PlayerHubConnection.OnMessage<ISendScores, List<ScoreRow>>(async scores =>
            {
                Scores = scores;
                await Events.OnScoresArrival.RaiseAsync();
            });
            PlayerHubConnection.OnMessage<IRestoreSelectedAnswers, List<AnswerModel>>(async hand =>
            {
                _selectedAnswers = hand;
                await Events.OnHandRetrival.RaiseAsync();
            });
        }

    }
}
