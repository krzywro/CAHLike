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
        Task RegisterPlayer(Guid playerId, string playerName);
        Task<string> GetName(Guid player);
    }

    public class PlayerPoolService : IPlayerPoolService
    {
        public static ConcurrentDictionary<Guid, string> PlayerToNames = new ConcurrentDictionary<Guid, string>();

        public Task<string> GetName(Guid player)
        {
            PlayerToNames.TryGetValue(player, out var playerName);
            return Task.FromResult(playerName);
        }

        public Task RegisterPlayer(Guid playerId, string playerName)
        {
            PlayerToNames.AddOrUpdate(playerId, playerName, (x, v) => playerName);
            return Task.CompletedTask;
        }
    }
}
