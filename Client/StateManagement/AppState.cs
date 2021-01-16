using KrzyWro.CAH.Client.StateManagement.LobbyState;
using KrzyWro.CAH.Client.StateManagement.PlayerState;
using KrzyWro.CAH.Client.StateManagement.TableState;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using KrzyWro.CAH.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public class AppState
    {
        private readonly IAppLocalStorage _localStorage;
        private readonly IPlayerHubClient _playerHub;
        private readonly ILobbyHubClient _lobbyHub;
        private readonly ITableHubClient _tableHub;

        public AppEvents Events { get; } = new AppEvents();

        public IList<TableEntry> GameList { get; private set; } = new List<TableEntry>();

        public List<AnswerModel> SelectedAnswers { get; private set; } = new List<AnswerModel>();
        public List<List<AnswerModel>> PlayerAnswers { get; private set; } = new List<List<AnswerModel>>();
        public List<AnswerModel> BestAnswer { get; private set; } = new List<AnswerModel>();
        public string BestAnswerPlayerName { get; private set; } = string.Empty;
        public QuestionModel CurrentQuestion { get; private set; }
        public List<AnswerModel> Hand { get; private set; } = new List<AnswerModel>();
        public Player Player { get; private set; } = new Player();
        public IList<ScoreRow> Scores { get; private set; } = new List<ScoreRow>();

        public Flow.State CurrentState { get; private set; } = Flow.InitialState;

        public bool Connected { get; private set; } = true;
        public bool TableSetUp { get; private set; } = false;

        public bool ConnectionFailed { get; private set; } = false;

        public AppState(IAppLocalStorage localStorage, IPlayerHubClient playerHub, ILobbyHubClient lobbyHub, ITableHubClient tableHub)
        {
            _localStorage = localStorage;
            _playerHub = playerHub;
            _lobbyHub = lobbyHub;
            _tableHub = tableHub;
        }

        public async Task RegisterPlayer()
        {
            if(!ConnectionFailed)
                await _playerHub.SendRegisterPlayer(Player);
		}

        public async Task NextQuestion()
        {
            CurrentState = CurrentState.ChangeState(Flow.Action.ProceedToNextQuestion);
            await RequestQuestion();
            await RequestHand();
        }

        private async Task RequestQuestion()
        {
            PlayerAnswers.Clear();
            BestAnswer.Clear();
            BestAnswerPlayerName = string.Empty;
            SelectedAnswers.Clear();
            await Events.OnAnswerSelectionChange.RaiseAsync();
            await _tableHub.SendRequestQuestion();
        }

        public Task RequestGameList()
        {
            GameList.Clear();
            return _lobbyHub.SendRequestGameList();
        }

        private async Task RequestHand() => await _tableHub.SendRequestHand();

        public async Task SendAnswers()
        {
            if (SelectedAnswers.Count == CurrentQuestion.AnswerCards)
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.SendAnswer);
                await _tableHub.PlayerSendAnswers(SelectedAnswers.ToList());
                Hand = new List<AnswerModel>();
            }
        }

        public async Task PickAnswer()
        {
            CurrentState = CurrentState.ChangeState(Flow.Action.WaitForBestAnswer);
            await _tableHub.MasterSendAnswers(BestAnswer);
        }

        public int GetAnswerSelectionNumber(AnswerModel id) => SelectedAnswers.IndexOf(id) + 1;

        public async Task ToggleAnswer(AnswerModel id)
        {

            if (SelectedAnswers.Contains(id))
                SelectedAnswers.Remove(id);
            else if (SelectedAnswers.Count < CurrentQuestion.AnswerCards)
                SelectedAnswers.Add(id);

            await Events.OnAnswerSelectionChange.RaiseAsync();
        }
        public void ToggleBestAnswer(List<AnswerModel> answer)
        {
            if (CurrentState == Flow.State.SelectingBestAnswer)
                BestAnswer = answer;
        }

        public async Task SetPlayerName(string name)
        {
            await _localStorage.SetPlayerName(name);
            Player = await _localStorage.GetPlayer();

            if (CurrentState == Flow.State.FirstRunNamePicking)
                CurrentState = CurrentState.ChangeState(Flow.Action.PickName);

            await RegisterPlayer();
            await Events.PlayerNameChanged.RaiseAsync();
        }

        private async Task InitPlayerHub()
        {
            _playerHub.OnConnected(async x =>
            {
                Connected = true;
                await Events.StateChanged.RaiseAsync();
            });

            _playerHub.OnDisconnected(async x =>
            {
                Connected = false;
                await Events.StateChanged.RaiseAsync();
            });

            try
            {
                await _playerHub.Init();
            }
            catch
            {
                ConnectionFailed = true;
            }

            _playerHub.OnGreet(async () =>
            {
                Connected = true;
                await Events.StateChanged.RaiseAsync();
                await Events.ServerGreeting.RaiseAsync();
            });
        }

        private async Task InitLobbyHub()
        {
            if (Connected)
            {
                await _lobbyHub.Init();
                _lobbyHub.OnRecivingGameList(async games =>
                {
                    GameList = games;
                    if (games.Count == 0)
                        await _lobbyHub.SendRequestGameCreation();
                    await Events.OnGameEntryArrival.RaiseAsync();
                });
                _lobbyHub.OnGameCreated(async game =>
                {
                    GameList.Add(game);
                    await Events.OnGameEntryArrival.RaiseAsync();
                });
                await _lobbyHub.Join(Player.Id);
            }
        }

        public async Task InitTableHub(Guid gameId)
        {
            if (!Connected || TableSetUp)
                return;

            TableSetUp = true;
            CurrentState = CurrentState.ChangeState(Flow.Action.EnterGame);

            _tableHub.OnTableSendQuestion(async question =>
            {
                CurrentQuestion = question;
                await Events.OnQuestionRetrival.RaiseAsync();
            });

            _tableHub.OnTablePlayerSendHand(async hand =>
            {
                Hand = hand;
                await Events.OnHandRetrival.RaiseAsync();
            });

            _tableHub.OnTablePlayerWaitForOtherPlayers(async () =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.BecameMaster);
                await Events.OnWaitForOtherPlayers.RaiseAsync();
            });

            _tableHub.OnTablePlayerWaitForSelection(async hand =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.WaitForBestAnswer);
                PlayerAnswers = hand;
                await Events.OnWaitForBestPick.RaiseAsync();
            });

            _tableHub.OnTableMasterRequestSelection(async hand =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.PickBestAnswer);
                PlayerAnswers = hand;
                await Events.OnSelectBestAnswer.RaiseAsync();
            });

            _tableHub.OnTableSendBestAnswer(async (answers, playerName) =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.ProceedToBestAnswer);
                PlayerAnswers = new List<List<AnswerModel>>();
                BestAnswer = answers;
                BestAnswerPlayerName = playerName;
                await Events.OnBestPick.RaiseAsync();
            });
            _tableHub.OnTableSendScores(async scores =>
            {
                Scores = scores;
                await Events.OnScoresArrival.RaiseAsync();
            });
            _tableHub.OnTablePlayerRestoreSelectedAnswers(async hand =>
            {
                SelectedAnswers = hand;
                await Events.OnHandRetrival.RaiseAsync();
            });

            await _tableHub.Init(gameId);
            await _tableHub.Join(Player.Id);
            await NextQuestion();

            await Events.StateChanged.RaiseAsync();
        }

        public async Task Init()
        {
            Player = await _localStorage.GetPlayer();
            Events.ServerGreeting += RequestGameList;

            await InitPlayerHub();
            await InitLobbyHub();


            if (await _localStorage.ShouldFirstRunSetup())
                CurrentState = CurrentState.ChangeState(Flow.Action.FirstRunSetup);
            else
            {
                await Events.PlayerNameChanged.RaiseAsync();
                await RegisterPlayer();
            }
        }

    }
}
