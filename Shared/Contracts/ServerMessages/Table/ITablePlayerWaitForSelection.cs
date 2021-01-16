using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages.Table
{
    public interface ITablePlayerWaitForSelection : ITableServerMessage<List<List<AnswerModel>>>
    {
    }
}
