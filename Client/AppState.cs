using Blazored.LocalStorage;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client
{
    public class AppState
    {
        private readonly ILocalStorageService _localStorage;
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
            PlayerNameChanged?.Invoke();
            await PlayerHubConnection?.SendAsync("RegisterPlayer", player.Id, Player.Name);
            await PlayerHubConnection?.SendAsync("RequestScores");
		}

		public async Task RequestQuestion()
        {
            PlayerAnswers = new List<List<AnswerModel>>();
            BestAnswer = new List<AnswerModel>();
            BestAnswerPlayerName = string.Empty;
            _selectedAnswers.Clear();
            NotifyStateChanged();
            await PlayerHubConnection?.SendAsync("RequestQuestion");
        }

        public async Task RequestHand() => await PlayerHubConnection?.SendAsync("RequestHand");

        public async Task SendAnswers()
        {
            await PlayerHubConnection?.SendAsync("SendAnswers", SelectedAnswers);
            Hand = new List<AnswerModel>();
        }

        public async Task PickAnswer() => await PlayerHubConnection?.SendAsync("PickAnswer", BestAnswer);

        private List<AnswerModel> _selectedAnswers = new List<AnswerModel>();
        public IReadOnlyList<AnswerModel> SelectedAnswers => _selectedAnswers;

        public List<List<AnswerModel>> PlayerAnswers { get; private set; } = new List<List<AnswerModel>>();
        public List<AnswerModel> BestAnswer { get; set; } = new List<AnswerModel>();
        public string BestAnswerPlayerName { get; private set; } = string.Empty;

        public int GetAnswerSelectionNumber(AnswerModel id) => _selectedAnswers.IndexOf(id) + 1;

        public void ToggleAnswer(AnswerModel id)
        {

            if (_selectedAnswers.Contains(id))
                _selectedAnswers.Remove(id);
            else if (_selectedAnswers.Count < CurrentQuestion.AnswerCards)
                _selectedAnswers.Add(id);

            NotifyStateChanged();
        }

        public QuestionModel CurrentQuestion { get; private set; }
        public List<AnswerModel> Hand { get; private set; } = new List<AnswerModel>();

        private void NotifyStateChanged() => OnAnswerSelectionChange?.Invoke();

        public Player Player { get; private set; } = new Player();

        public IDictionary<string, int> Scores { get; private set; } = new Dictionary<string, int>();

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
                player = new Player { Id = Guid.NewGuid() };
                syncLocalStorage.SetItem("player", player);
            }
            Player = player;


            PlayerHubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/playerhub"))
                .WithAutomaticReconnect()
                .Build();

            PlayerHubConnection.On("Greet", () =>
            {
                ServerGreeting?.Invoke();
            });

            PlayerHubConnection.On<QuestionModel>("GetQuestion", question =>
            {
                CurrentQuestion = question;
                OnQuestionRetrival?.Invoke();
            });

            PlayerHubConnection.On<List<AnswerModel>>("GetHand", hand =>
            {
                Hand = hand;
                OnHandRetrival?.Invoke();
            });

            PlayerHubConnection.On("WaitForOtherPlayers", () =>
            {
                OnWaitForOtherPlayers?.Invoke();
            });

            PlayerHubConnection.On<List<List<AnswerModel>>>("WaitForBestAnswerPick", hand =>
            {
                PlayerAnswers = hand;
                OnWaitForBestPick?.Invoke();
            });

            PlayerHubConnection.On<List<List<AnswerModel>>>("SelectBestAnswer", hand =>
            {
                PlayerAnswers = hand;
                OnSelectBestAnswer?.Invoke();
            });

            PlayerHubConnection.On<List<AnswerModel>, string>("BestAnswerPick", (answers, playerName) =>
            {
                PlayerAnswers = new List<List<AnswerModel>>();
                BestAnswer = answers;
                BestAnswerPlayerName = playerName;
                OnBestPick?.Invoke();
            });
            PlayerHubConnection.On<Dictionary<string, int>>("SendScores", scores =>
            {
                Scores = scores;
                OnScoresArrival?.Invoke();
            });
            PlayerHubConnection.On<List<AnswerModel>>("YourAnswers", hand =>
            {
                _selectedAnswers = hand;
                OnHandRetrival?.Invoke();
            });
        }

        #region Events
        public event Action PlayerNameChanged;
        public event Action ServerGreeting;
        public event Action OnAnswerSelectionChange;
        public event Action OnQuestionRetrival;
        public event Action OnHandRetrival;
        public event Action OnWaitForOtherPlayers;
        public event Action OnWaitForBestPick;
        public event Action OnSelectBestAnswer;
        public event Action OnBestPick;
        public event Action OnScoresArrival;
        #endregion
    }
}
