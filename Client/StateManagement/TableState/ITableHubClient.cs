using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement.TableState
{
    public interface ITableHubClient
    {
        Task Init(Guid game);
        void OnConnected(Func<string, Task> action);
        void OnDisconnected(Func<Exception, Task> action);

        void OnTableMasterRequestSelection(Func<List<List<AnswerModel>>, Task> action);
        void OnTablePlayerJoined(Func<string, Task> action);
        void OnTablePlayerRestoreSelectedAnswers(Func<List<AnswerModel>, Task> action);
        void OnTablePlayerSendHand(Func<List<AnswerModel>, Task> action);
        void OnTablePlayerWaitForOtherPlayers(Func<Task> action);
        void OnTablePlayerWaitForSelection(Func<List<List<AnswerModel>>, Task> action);
        void OnTableSendBestAnswer(Func<List<AnswerModel>, string, Task> action);
        void OnTableSendQuestion(Func<QuestionModel, Task> action);
        void OnTableSendScores(Func<List<ScoreRow>, Task> action);

        Task Join(Guid playerId);
        Task SendRequestHand();
        Task SendRequestQuestion();
        Task SendRequestScores();
        Task MasterSendAnswers(List<AnswerModel> answers);
        Task PlayerSendAnswers(List<AnswerModel> answers);
    }
}