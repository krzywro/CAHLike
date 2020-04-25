using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;

namespace KrzyWro.CAH.Shared.Contracts.ServerMessages
{
    public interface IWaitForBestAnswer : IServerMessage<List<List<AnswerModel>>>
    {
    }
}
