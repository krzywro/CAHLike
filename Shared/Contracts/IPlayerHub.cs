using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts
{
    public interface IPlayerHub
    {
        Task RegisterPlayer(Guid playerId, string playerName);
        Task RequestQuestion();
        Task RequestScores();
        Task RequestHand();
        Task SendAnswers(List<AnswerModel> answers);
        Task PickAnswer(List<AnswerModel> answers);
    }
}
