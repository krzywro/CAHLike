using KrzyWro.CAH.Server.Helpers;
using KrzyWro.CAH.Server.Services;
using KrzyWro.CAH.Shared.Contracts.ServerMessages.Games;
using KrzyWro.CAH.Shared.Dto;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly IGamesService _gamesService;

        public LobbyHub(IGamesService gamesService)
        {
            _gamesService = gamesService;
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
            await Clients.Caller.SendMessageAsync<IGameCreated, TableEntry>(new TableEntry
            {
                GameId = game.Id
            });
        }
    }
}
