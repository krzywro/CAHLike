using KrzyWro.SupportLib;

namespace KrzyWro.CAH.Client.StateManagement
{
    public class AppEvents
    {
        private readonly AsyncEvent stateChanged = new AsyncEvent();
        private readonly AsyncEvent playerNameChanged = new AsyncEvent();
        private readonly AsyncEvent serverGreeting = new AsyncEvent();
        private readonly AsyncEvent onQuestionRetrival = new AsyncEvent();
        private readonly AsyncEvent onAnswerSelectionChange = new AsyncEvent();
        private readonly AsyncEvent onHandRetrival = new AsyncEvent();
        private readonly AsyncEvent onWaitForOtherPlayers = new AsyncEvent();
        private readonly AsyncEvent onWaitForBestPick = new AsyncEvent();
        private readonly AsyncEvent onSelectBestAnswer = new AsyncEvent();
        private readonly AsyncEvent onBestPick = new AsyncEvent();
        private readonly AsyncEvent onScoresArrival = new AsyncEvent();

        public AsyncEvent StateChanged { get { return stateChanged; } set { } }
        public AsyncEvent PlayerNameChanged { get { return playerNameChanged; } set { } }
        public AsyncEvent ServerGreeting { get { return serverGreeting; } set { } }
        public AsyncEvent OnAnswerSelectionChange { get { return onAnswerSelectionChange; } set { } }
        public AsyncEvent OnQuestionRetrival { get { return onQuestionRetrival; } set { } }
        public AsyncEvent OnHandRetrival { get { return onHandRetrival; } set { } }
        public AsyncEvent OnWaitForOtherPlayers { get { return onWaitForOtherPlayers; } set { } }
        public AsyncEvent OnWaitForBestPick { get { return onWaitForBestPick; } set { } }
        public AsyncEvent OnSelectBestAnswer { get { return onSelectBestAnswer; } set { } }
        public AsyncEvent OnBestPick { get { return onBestPick; } set { } }
        public AsyncEvent OnScoresArrival { get { return onScoresArrival; } set { } }
    }
}
