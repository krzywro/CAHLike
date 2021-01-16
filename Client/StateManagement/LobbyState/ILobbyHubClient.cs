using KrzyWro.CAH.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement.LobbyState
{
    public interface ILobbyHubClient
    {
        Task Init();
        void OnConnected(Func<string, Task> action);
        void OnDisconnected(Func<Exception, Task> action);
        void OnGameCreated(Func<TableEntry, Task> action);
        void OnRecivingGameList(Func<List<TableEntry>, Task> action);

        Task Join(Guid playerId);
        Task SendRequestGameList();
        Task SendRequestGameCreation();
    }
}