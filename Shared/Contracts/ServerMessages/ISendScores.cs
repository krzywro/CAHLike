using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages
{
    public interface ISendScores : IServerMessage<List<ScoreRow>>
    {
    }
}
