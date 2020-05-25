using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public class AppState
    {
        private readonly IAppLocalStorage _localStorage;
        private readonly IPlayerHubClient _playerHub;

        public AppEvents Events { get; } = new AppEvents();


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

        public bool ConnectionFailed { get; private set; } = false;

        public AppState(IAppLocalStorage localStorage, IPlayerHubClient playerHub)
        {
            _localStorage = localStorage;
            _playerHub = playerHub;
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
            await _playerHub.SendRequestQuestion();
        }

        private async Task RequestHand() => await _playerHub.SendRequestHand();

        public async Task SendAnswers()
        {
            if (SelectedAnswers.Count == CurrentQuestion.AnswerCards)
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.SendAnswer);
                await _playerHub.SendAnswers(SelectedAnswers.ToList());
                Hand = new List<AnswerModel>();
            }
        }

        public async Task PickAnswer()
        {
            CurrentState = CurrentState.ChangeState(Flow.Action.WaitForBestAnswer);
            await _playerHub.SendPickedBestAnswers(BestAnswer);
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
                CurrentState = CurrentState.ChangeState(Flow.Action.EnterGame);
                CurrentState = CurrentState.ChangeState(Flow.Action.ProceedToNextQuestion);
                await Events.StateChanged.RaiseAsync();
                await Events.ServerGreeting.RaiseAsync();
            });

            _playerHub.OnReceivingQuestion(async question =>
            {
                CurrentQuestion = question;
                await Events.OnQuestionRetrival.RaiseAsync();
            });

            _playerHub.OnReceivingHand(async hand =>
            {
                Hand = hand;
                await Events.OnHandRetrival.RaiseAsync();
            });

            _playerHub.OnWaitForOtherPlayers(async () =>
            {
                await Events.OnWaitForOtherPlayers.RaiseAsync();
            });

            _playerHub.OnWaitForBestAnswer(async hand =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.WaitForBestAnswer);
                PlayerAnswers = hand;
                await Events.OnWaitForBestPick.RaiseAsync();
            });

            _playerHub.OnSelectBestAnswer(async hand =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.PickBestAnswer);
                PlayerAnswers = hand;
                await Events.OnSelectBestAnswer.RaiseAsync();
            });

            _playerHub.OnBestAnswerPicked(async (answers, playerName) =>
            {
                CurrentState = CurrentState.ChangeState(Flow.Action.ProceedToBestAnswer);
                PlayerAnswers = new List<List<AnswerModel>>();
                BestAnswer = answers;
                BestAnswerPlayerName = playerName;
                await Events.OnBestPick.RaiseAsync();
            });
            _playerHub.OnRecevingScores(async scores =>
            {
                Scores = scores;
                await Events.OnScoresArrival.RaiseAsync();
            });
            _playerHub.OnReceivingRestoreSelectedAnswers(async hand =>
            {
                SelectedAnswers = hand;
                await Events.OnHandRetrival.RaiseAsync();
            });
        }

        public async Task Init()
        {
            Player = await _localStorage.GetPlayer();

            Events.ServerGreeting += RequestQuestion;
            Events.ServerGreeting += RequestHand;
            Events.ServerGreeting += _playerHub.SendRequestScores;

            if (await _localStorage.ShouldFirstRunSetup())
                CurrentState = CurrentState.ChangeState(Flow.Action.FirstRunSetup);
            else
            {
                await Events.PlayerNameChanged.RaiseAsync();
                await RegisterPlayer();
            }

            await InitPlayerHub();
        }

    }
}
