using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Game
{
    public interface IRequestGameList : IClientMessage
    {
        Task RequestGameList();
    }
}
