using KrzyWro.CAH.Shared.Contracts.ServerMessages;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Client.Helpers
{
    public static class Extensions
    {
        public static IDisposable OnMessage<T>(this HubConnection hubConnection, Func<Task> handler) where T : IServerMessage
            => hubConnection.On(typeof(T).Name, handler);

        public static IDisposable OnMessage<T, T1>(this HubConnection hubConnection, Func<T1, Task> handler) where T : IServerMessage<T1>
            => hubConnection.On(typeof(T).Name, handler);

        public static IDisposable OnMessage<T, T1, T2>(this HubConnection hubConnection, Func<T1, T2, Task> handler) where T : IServerMessage<T1, T2>
            => hubConnection.On(typeof(T).Name, handler);

        public static IDisposable OnMessage<T, T1, T2, T3>(this HubConnection hubConnection, Func<T1, T2, T3, Task> handler) where T : IServerMessage<T1, T2, T3>
            => hubConnection.On(typeof(T).Name, handler);
    }
}
