using Blazored.LocalStorage;
using KrzyWro.CAH.Shared;
using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.StateManagement
{
    public class AppLocalStorage : IAppLocalStorage
    {
        private static class Keys
        {
            public const string Player = "player";
            public const string FirstRun = "firstRunDone";
        }

        private const string NewPlayer = "Nowy gracz";

        private readonly ILocalStorageService _localStorage;

        public AppLocalStorage(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task<Player> GetPlayer()
        {
            var player = await _localStorage.GetItemAsync<Player>(Keys.Player);
            if (player == null)
            {
                player = new Player { Id = Guid.NewGuid(), Name = NewPlayer };
                await _localStorage.SetItemAsync(Keys.Player, player);
            }

            return player;
        }

        public async Task SetPlayerName(string newName)
        {
            var player = await GetPlayer();
            player.Name = newName;
            await _localStorage.SetItemAsync(Keys.Player, player);
            await _localStorage.SetItemAsync(Keys.FirstRun, true);
        }

        public async Task<bool> ShouldFirstRunSetup()
        {
            var done = await _localStorage.GetItemAsync<bool?>(Keys.FirstRun);
            if (!done.HasValue)
            {
                done = false;
                await _localStorage.SetItemAsync(Keys.FirstRun, done);
            }

            return !done.Value;
        }
    }
}
