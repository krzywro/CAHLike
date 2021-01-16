using KrzyWro.CAH.Client.Helpers;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ClientMessages.Game;
using KrzyWro.CAH.Shared.Contracts.ServerMessages.Games;
using KrzyWro.CAH.Shared.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement.LobbyState
{
    public class LobbyHubClient : ILobbyHubClient
    {
        private readonly HubConnection _hubConnection;

        public LobbyHubClient(NavigationManager NavigationManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(ILobbyHub.Path))
                .WithAutomaticReconnect()
                .Build();
        }

        public void OnConnected(Func<string, Task> action) => _hubConnection.Reconnected += action;
        public void OnDisconnected(Func<Exception, Task> action) => _hubConnection.Reconnecting += action;

        public Task Init() => _hubConnection.StartAsync();

        public void OnGameCreated(Func<TableEntry, Task> action) => _hubConnection.OnMessage<IGameCreated, TableEntry>(action);

        public void OnRecivingGameList(Func<List<TableEntry>, Task> action) => _hubConnection.OnMessage<IGameSendList, List<TableEntry>>(action);

        public Task SendRequestGameList() => _hubConnection.SendMessageAsync<IRequestGameList>();

        public Task SendRequestGameCreation() => _hubConnection.SendMessageAsync<IRequestGameCreation>();

        public Task Join(Guid playerId) => _hubConnection.SendMessageAsync<ILobbyJoin, Guid>(playerId);
    }
}
