using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages.Table
{
    public interface ITableMasterRequestSelection : ITableServerMessage<List<List<AnswerModel>>>
    {
    }
}
