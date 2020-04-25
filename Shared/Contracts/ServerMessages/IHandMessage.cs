using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages
{
    public interface IHandMessage : IServerMessage<List<AnswerModel>>
    {
    }
}
