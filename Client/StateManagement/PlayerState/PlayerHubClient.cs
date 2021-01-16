using KrzyWro.CAH.Client.Helpers;
using KrzyWro.CAH.Shared;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ClientMessages;
using KrzyWro.CAH.Shared.Contracts.ServerMessages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement.PlayerState
{
    public class PlayerHubClient : IPlayerHubClient
    {
        private readonly HubConnection _hubConnection;

        public PlayerHubClient(NavigationManager NavigationManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri(IPlayerHub.Path))
                .WithAutomaticReconnect()
                .Build();
        }

        public void OnConnected(Func<string, Task> action) => _hubConnection.Reconnected += action;
        public void OnDisconnected(Func<Exception, Task> action) => _hubConnection.Reconnecting += action;

        public async Task Init() => await _hubConnection.StartAsync();

        public void OnGreet(Func<Task> action) => _hubConnection.OnMessage<IGreetMessage>(action);

        public async Task SendRegisterPlayer(Player player) => await _hubConnection.SendMessageAsync<IPlayerHubRegisterPlayer, Guid, string>(player.Id, player.Name);
    }
}
