using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages.Table
{
    public interface ITableSendScores : ITableServerMessage<List<ScoreRow>>
    {
    }
}
