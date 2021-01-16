using KrzyWro.CAH.Server.Helpers;
using KrzyWro.CAH.Server.Services;
using KrzyWro.CAH.Shared.Contracts;
using KrzyWro.CAH.Shared.Contracts.ServerMessages.Games;
using KrzyWro.CAH.Shared.Dto;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Hubs
{
    public class LobbyHub : Hub, ILobbyHub
    {
        private readonly IGamesService _gamesService;
        public static ConcurrentDictionary<Guid, HashSet<string>> PlayerToConnections = new ConcurrentDictionary<Guid, HashSet<string>>();
        public static ConcurrentDictionary<string, Guid> ConnectionToPlayer = new ConcurrentDictionary<string, Guid>();

        public LobbyHub(IGamesService gamesService)
        {
            _gamesService = gamesService;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectionToPlayer.TryRemove(Context.ConnectionId, out var playerId);
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>(), (x, v) => { v.Remove(Context.ConnectionId); return v; });
            await base.OnDisconnectedAsync(exception);
        }

        public Task Join(Guid playerId)
        {
            PlayerToConnections.AddOrUpdate(playerId, x => new HashSet<string>() { Context.ConnectionId }, (x, v) => { v.Add(Context.ConnectionId); return v; });
            ConnectionToPlayer.AddOrUpdate(Context.ConnectionId, playerId, (x, v) => playerId);
            return Task.CompletedTask;
        }

        public async Task RequestGameList()
        {
            var list = await _gamesService.List();
            await Clients.Caller.SendMessageAsync<IGameSendList, List<TableEntry>>(list.Select(x => new TableEntry
            {
                GameId = x.Id
            }
            ).ToList());
        }

        public async Task RequestGameCreation()
        {
            var game = await _gamesService.Create();
            await Clients.All.SendMessageAsync<IGameCreated, TableEntry>(new TableEntry
            {
                GameId = game.Id
            });
        }
    }
}
