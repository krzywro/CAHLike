using KrzyWro.CAH.Shared;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public interface IAppLocalStorage
    {
        Task<Player> GetPlayer();
        Task SetPlayerName(string newName);
        Task<bool> ShouldFirstRunSetup();
    }
}