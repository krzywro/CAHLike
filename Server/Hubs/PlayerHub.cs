using KrzyWro.CAH.Server.Helpers;
using KrzyWro.CAH.Server.Services;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ServerMessages;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Hubs
{
    public class PlayerHub : Hub, IPlayerHub
    {
        private readonly IPlayerPoolService _playerPoolService;
        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<string, Guid> ConnectionToPlayer = new ConcurrentDictionary<string, Guid>();

        public PlayerHub(IPlayerPoolService playerPoolService)
        {
            _playerPoolService = playerPoolService;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionToPlayer.TryRemove(Context.ConnectionId, out var playerId);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(Context.ConnectionId); return v; });
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterPlayer(Guid playerId, string playerName)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>() { Context.ConnectionId }, (x, v) => { v.Add(Context.ConnectionId); return v; });
            ConnectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (x, v) => playerId);
            await _playerPoolService.RegisterPlayer(playerId, playerName);
            await Clients.Caller.SendMessageAsync<IGreetMessage>();
        }
    }
}
