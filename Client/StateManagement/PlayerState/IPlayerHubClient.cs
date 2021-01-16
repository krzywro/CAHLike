using KrzyWro.CAH.Shared;
using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement.PlayerState
{
    public interface IPlayerHubClient
    {
        Task Init();
        void OnConnected(Func<string, Task> action);
        void OnGreet(Func<Task> action);
        void OnDisconnected(Func<Exception, Task> action);
        Task SendRegisterPlayer(Player player);
    }
}