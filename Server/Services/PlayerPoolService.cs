using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Services
{
    public interface IPlayerPoolService
    {
        Task RegisterPlayerConnection(string connectionId, Guid playerId, string playerName);
        Task DeregisterPlayerConnection(string connectionId);
        Task<string> GetName(Guid player);
        Task<Guid> GetPlayer(string connection);
        Task<HashSet<string>> GetConnections(Guid player);
        Task<bool> IsOnline(Guid player);
    }

    public class PlayerPoolService : IPlayerPoolService
    {

        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<Guid, string> PlayerToNames = new ConcurrentDictionary<Guid, string>();
        public static ConcurrentDictionary<string, Guid> ConnectionToPlayer = new ConcurrentDictionary<string, Guid>();

        private readonly ILogger _logger;

        public Task DeregisterPlayerConnection(string connectionId)
        {

            ConnectionToPlayer.TryRemove(connectionId, out var playerId);
            PlayerToNames.TryGetValue(playerId, out var playerName);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(connectionId); return v; });

            if (PlayerToConnections.TryGetValue(playerId, out var connections) && !connections.Any()) ;

            _logger.LogInformation($"[Disconnected {connectionId}] Player: {playerName} ({playerId})");
            return Task.CompletedTask;
        }

        public Task<HashSet<string>> GetConnections(Guid player)
        {
            PlayerToConnections.TryGetValue(player, out var playerConnections);
            return Task.FromResult(playerConnections);
        }

        public Task<string> GetName(Guid player)
        {
            PlayerToNames.TryGetValue(player, out var playerName);
            return Task.FromResult(playerName);
        }

        public Task<Guid> GetPlayer(string connection)
        {
            ConnectionToPlayer.TryGetValue(connection, out var player);
            return Task.FromResult(player);
        }

        public Task<bool> IsOnline(Guid player)
        {
            PlayerToConnections.TryGetValue(player, out var playerConnections);
            return Task.FromResult(playerConnections.Any());
        }

        public Task RegisterPlayerConnection(string connectionId, Guid playerId, string playerName)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>() { connectionId }, (x, v) => { v.Add(connectionId); return v; });
            PlayerToNames.AddOrUpdate(playerId, playerName, (x, v) => playerName);
            ConnectionToPlayer.AddOrUpdate(connectionId, playerId, (x, v) => playerId);
            _logger.LogInformation($"[Connected {connectionId}] Player: {playerName} ({playerId})");
            return Task.CompletedTask;
        }
    }
}
