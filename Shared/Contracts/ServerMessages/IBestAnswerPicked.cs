using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages
{
    public interface IBestAnswerPicked : IServerMessage<List<AnswerModel>, string>
    {
    }
}
