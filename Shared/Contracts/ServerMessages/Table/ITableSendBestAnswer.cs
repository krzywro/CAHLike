using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages.Table
{
    public interface ITableSendBestAnswer : ITableServerMessage<List<AnswerModel>, string>
    {
    }
}
