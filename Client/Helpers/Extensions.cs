using KrzyWro.CAH.Shared.Contracts.ClientMessages;
using KrzyWro.CAH.Shared.Contracts.ServerMessages;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
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

        public static Task SendMessageAsync<T>(this HubConnection hubConnection, CancellationToken cancellationToken = default) where T : IClientMessage
            => hubConnection.SendAsync(typeof(T).GetMethods()[0].Name, cancellationToken);

        public static Task SendMessageAsync<T, T1>(this HubConnection hubConnection, T1 arg1, CancellationToken cancellationToken = default) where T : IClientMessage<T1>
            => hubConnection.SendAsync(typeof(T).GetMethods()[0].Name, arg1, cancellationToken);

        public static Task SendMessageAsync<T, T1, T2>(this HubConnection hubConnection, T1 arg1, T2 arg2, CancellationToken cancellationToken = default) where T : IClientMessage<T1, T2>
            => hubConnection.SendAsync(typeof(T).GetMethods()[0].Name, arg1, arg2, cancellationToken);

        public static Task SendMessageAsync<T, T1, T2, T3>(this HubConnection hubConnection, T1 arg1, T2 arg2, T3 arg3, CancellationToken cancellationToken = default) where T : IClientMessage<T1, T2, T3>
            => hubConnection.SendAsync(typeof(T).GetMethods()[0].Name, arg1, arg2, arg3, cancellationToken);
    }
}
