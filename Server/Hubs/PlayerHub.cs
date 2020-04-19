using KrzyWro.CAH.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Hubs
{
    public class PlayerHub : Hub
    {
        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<string, Guid> ConnectionToPlayer = new ConcurrentDictionary<string, Guid>();

        private readonly ILogger _logger;

        public PlayerHub(ILogger<PlayerHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionToPlayer.TryRemove(Context.ConnectionId, out var playerId);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(Context.ConnectionId); return v; });
            if (PlayerToConnections[playerId]?.Count == 0)
                PlayerToConnections.TryRemove(playerId, out _);
            _logger.LogInformation($"[Disconnected {Context.ConnectionId}] Player: {playerId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task RegisterPlayer(Guid playerId, string playerName)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Add(Context.ConnectionId); return v; });
            ConnectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (x, v) => playerId);
            _logger.LogInformation($"[Connected {Context.ConnectionId}] Player: {playerId}");

            await Clients.Client(Context.ConnectionId).SendAsync("Greet");
        }


    }
}
