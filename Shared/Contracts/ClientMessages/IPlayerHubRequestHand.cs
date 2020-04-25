using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages
{
    public interface IPlayerHubRequestHand : IClientMessage
    {
        Task RequestHand();
    }
}
