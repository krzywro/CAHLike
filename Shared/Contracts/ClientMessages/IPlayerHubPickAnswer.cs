using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages
{
    public interface IPlayerHubPickAnswer : IClientMessage<List<AnswerModel>>
    {
        Task PickAnswer(List<AnswerModel> answers);
    }
}
