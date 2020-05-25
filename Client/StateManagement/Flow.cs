using System;

namespace KrzyWro.CAH.Client.StateManagement
{
    public static class Flow
    {
        public enum State { Idle, EnteredAGame, PickingAnswer, WaitingForOtherPlayersAnswers, WaitingForBestAnswerSelection, SelectingBestAnswer, ViewingBestAnswer, FirstRunNamePicking }
        public enum Action { EnterGame, SendAnswer, PickBestAnswer, WaitForBestAnswer, ProceedToBestAnswer, ProceedToNextQuestion, LeaveGame, PickName, FirstRunSetup }

        public static State InitialState => State.Idle;

        public static State ChangeState(this State state, Action action)
            => (state, action) switch
            {
                (State.FirstRunNamePicking, Action.PickName) => State.Idle,
                (State.Idle, Action.EnterGame) => State.EnteredAGame,
                (State.Idle, Action.FirstRunSetup) => State.FirstRunNamePicking,
                (State.EnteredAGame, Action.ProceedToNextQuestion) => State.PickingAnswer,
                (State.PickingAnswer, Action.SendAnswer) => State.WaitingForOtherPlayersAnswers,
                (State.WaitingForOtherPlayersAnswers, Action.PickBestAnswer) => State.SelectingBestAnswer,
                (State.SelectingBestAnswer, Action.WaitForBestAnswer) => State.WaitingForBestAnswerSelection,
                (State.WaitingForOtherPlayersAnswers, Action.WaitForBestAnswer) => State.WaitingForBestAnswerSelection,
                (State.WaitingForBestAnswerSelection, Action.ProceedToBestAnswer) => State.ViewingBestAnswer,
                (State.ViewingBestAnswer, Action.ProceedToNextQuestion) => State.PickingAnswer,
                (_, Action.LeaveGame) => State.Idle,
                _ => throw new NotSupportedException($"{state} has no transition on {action}")
            };
    }
}
