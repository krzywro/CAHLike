using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public interface IPlayerHubClient
    {
        Task Init();
        void OnBestAnswerPicked(Func<List<AnswerModel>, string, Task> action);
        void OnGreet(Func<Task> action);
        void OnReceivingHand(Func<List<AnswerModel>, Task> action);
        void OnReceivingQuestion(Func<QuestionModel, Task> action);
        void OnReceivingRestoreSelectedAnswers(Func<List<AnswerModel>, Task> action);
        void OnRecevingScores(Func<List<ScoreRow>, Task> action);
        void OnSelectBestAnswer(Func<List<List<AnswerModel>>, Task> action);
        void OnWaitForBestAnswer(Func<List<List<AnswerModel>>, Task> action);
        void OnWaitForOtherPlayers(Func<Task> action);
        Task SendAnswers(List<AnswerModel> answers);
        Task SendPickedBestAnswers(List<AnswerModel> answers);
        Task SendRegisterPlayer(Player player);
        Task SendRequestHand();
        Task SendRequestQuestion();
        Task SendRequestScores();
    }
}