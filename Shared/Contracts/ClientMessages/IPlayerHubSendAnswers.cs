using KrzyWro.CAH.Shared.Cards;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages
{
    public interface IPlayerHubSendAnswers : IClientMessage<List<AnswerModel>>
    {
        Task SendAnswers(List<AnswerModel> answers);
    }
}
