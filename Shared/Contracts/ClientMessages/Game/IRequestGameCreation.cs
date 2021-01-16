using System.Threading.Tasks;

namespace KrzyWro.CAH.Shared.Contracts.ClientMessages.Game
{
    public interface IRequestGameCreation : IClientMessage
    {
        Task RequestGameCreation();
    }
}
