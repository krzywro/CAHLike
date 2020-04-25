using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages
{
    public interface IPlayerHubRequestScores : IClientMessage
    {
        Task RequestScores();
    }
}
