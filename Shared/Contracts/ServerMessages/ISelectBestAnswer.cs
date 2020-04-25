using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages
{
    public interface ISelectBestAnswer : IServerMessage<List<List<AnswerModel>>>
    {
    }
}
