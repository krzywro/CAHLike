using KrzyWro.CAH.Shared.Contracts.ServerMessages;
using KrzyWro.CAH.Shared.Contracts.ServerMessages.Table;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KrzyWro.CAH.Server.Helpers
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom => Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
    }

    static class Extensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

        public static Task SendMessageAsync<T>(this IClientProxy clientProxy) where T : IServerMessage
            => clientProxy.SendAsync(typeof(T).Name);

        public static Task SendMessageAsync<T, T1>(this IClientProxy clientProxy, T1 arg1) where T : IServerMessage<T1>
            => clientProxy.SendAsync(typeof(T).Name, arg1);

        public static Task SendMessageAsync<T, T1, T2>(this IClientProxy clientProxy, T1 arg1, T2 arg2) where T : IServerMessage<T1, T2>
            => clientProxy.SendAsync(typeof(T).Name, arg1, arg2);

        public static Task SendMessageAsync<T, T1, T2, T3>(this IClientProxy clientProxy, T1 arg1, T2 arg2, T3 arg3) where T : IServerMessage<T1, T2, T3>
            => clientProxy.SendAsync(typeof(T).Name, arg1, arg2, arg3);

        public static Task SendMessageAsync<T>(this IClientProxy clientProxy, Guid guid) where T : ITableServerMessage
            => clientProxy.SendAsync(typeof(T).Name, guid);

        public static Task SendMessageAsync<T, T1>(this IClientProxy clientProxy, Guid guid, T1 arg1) where T : ITableServerMessage<T1>
            => clientProxy.SendAsync(typeof(T).Name, guid, arg1);

        public static Task SendMessageAsync<T, T1, T2>(this IClientProxy clientProxy, Guid guid, T1 arg1, T2 arg2) where T : ITableServerMessage<T1, T2>
            => clientProxy.SendAsync(typeof(T).Name, guid, arg1, arg2);
    }
}
