using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using System.Net.Http;
using KrzyWro.CAH.Client.StateManagement;
using KrzyWro.CAH.Client.StateManagement.LobbyState;
using KrzyWro.CAH.Client.StateManagement.PlayerState;
using KrzyWro.CAH.Client.StateManagement.TableState;

namespace KrzyWro.CAH.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddScoped<ILobbyHubClient, LobbyHubClient>();
            builder.Services.AddScoped<IPlayerHubClient, PlayerHubClient>();
            builder.Services.AddScoped<ITableHubClient, TableHubClient>();
            builder.Services.AddScoped<IAppLocalStorage, AppLocalStorage>();
            builder.Services.AddScoped<AppState>();

            var host = builder.Build();
            await host.Services.GetService<AppState>().Init();
            await host.RunAsync();
        }
    }
}
