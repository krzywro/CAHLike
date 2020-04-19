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
        private readonly ISyncLocalStorageService _syncLocalStorage;
        public HubConnection PlayerHubConnection;
        public async Task EnsurePlayerHubConnection() => await PlayerHubConnection?.StartAsync();
        public async Task RegisterPlayer() => await PlayerHubConnection?.SendAsync("RegisterPlayer", PlayerId, Player.Name);

        private List<AnswerModel> _selectedAnswers = new List<AnswerModel>();
        public IReadOnlyList<AnswerModel> SelectedAnswers => _selectedAnswers;
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

        public event Action OnAnswerSelectionChange;
        private void NotifyStateChanged() => OnAnswerSelectionChange?.Invoke();

        public Player Player => new Player(PlayerId);

        public Guid PlayerId
        {
            get
            {
                var id = _syncLocalStorage.GetItem<Guid?>("playerId");
                if (id == null || id == Guid.Empty)
                {
                    id = Guid.NewGuid();
                    _syncLocalStorage.SetItem("playerId", id);
                }
                return id.Value;
            }
        }

        public event Action ServerGreeting;

        public AppState(ISyncLocalStorageService syncLocalStorage, NavigationManager NavigationManager)
        {
            _syncLocalStorage = syncLocalStorage;

            PlayerHubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/playerhub"))
                .WithAutomaticReconnect()
                .Build();

            PlayerHubConnection.On("Greet", () =>
            {
                ServerGreeting?.Invoke();
            });

            CurrentQuestion = new QuestionModel(Guid.NewGuid(), "Test pytania dłuższego lub krótszego", 1);
            Hand = new List<AnswerModel> {
                new AnswerModel(Guid.NewGuid(), "1"),
                new AnswerModel(Guid.NewGuid(), "2"),
                new AnswerModel(Guid.NewGuid(), "3"),
                new AnswerModel(Guid.NewGuid(), "4"),
                new AnswerModel(Guid.NewGuid(), "5"),
                new AnswerModel(Guid.NewGuid(), "6"),
                new AnswerModel(Guid.NewGuid(), "7"),
                new AnswerModel(Guid.NewGuid(), "8"),
                new AnswerModel(Guid.NewGuid(), "9"),
                new AnswerModel(Guid.NewGuid(), "10"),
            };
        }
    }
}
