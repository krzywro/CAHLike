using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITablePlayerSendAnswers : ITablePlayerMessage<List<AnswerModel>>
    {
        Task SendPlayerAnswers(Guid game, List<AnswerModel> answers);
    }
}
