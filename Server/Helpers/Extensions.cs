using KrzyWro.CAH.Shared.Contracts.ServerMessages;
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

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
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

        public static async Task SendMessageAsync<T>(this IClientProxy clientProxy) where T : IServerMessage
            => await clientProxy.SendAsync(typeof(T).Name);

        public static async Task SendMessageAsync<T, T1>(this IClientProxy clientProxy, T1 arg1) where T : IServerMessage<T1>
            => await clientProxy.SendAsync(typeof(T).Name, arg1);

        public static async Task SendMessageAsync<T, T1, T2>(this IClientProxy clientProxy, T1 arg1, T2 arg2) where T : IServerMessage<T1, T2>
            => await clientProxy.SendAsync(typeof(T).Name, arg1, arg2);

        public static async Task SendMessageAsync<T, T1, T2, T3>(this IClientProxy clientProxy, T1 arg1, T2 arg2, T3 arg3) where T : IServerMessage<T1, T2, T3>
            => await clientProxy.SendAsync(typeof(T).Name, arg1, arg2, arg3);
    }
}
