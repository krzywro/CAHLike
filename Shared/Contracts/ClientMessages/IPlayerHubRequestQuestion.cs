using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages
{
    public interface IPlayerHubRequestQuestion : IClientMessage
    {
        Task RequestQuestion();
    }
}
