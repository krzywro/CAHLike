using KrzyWro.CAH.Shared.Cards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Table
{
    public interface ITableMasterSendAnswers : ITablePlayerMessage<List<AnswerModel>>
    {
        Task SendMasterAnswers(Guid game, List<AnswerModel> answers);
    }
}
