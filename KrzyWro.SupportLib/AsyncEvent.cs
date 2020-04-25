using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KrzyWro.SupportLib
{
    public class AsyncEvent
    {
        private List<Func<Task>> tasks = new List<Func<Task>>();
        private List<Action> actions = new List<Action>();

        public static AsyncEvent operator +(AsyncEvent @event, Func<Task> task)
        {
            @event.tasks.Add(task);
            return @event;
        }
        public static AsyncEvent operator -(AsyncEvent @event, Func<Task> task)
        {
            @event.tasks.Remove(task);
            return @event;
        }
        public static AsyncEvent operator +(AsyncEvent @event, Action task)
        {
            @event.actions.Add(task);
            return @event;
        }
        public static AsyncEvent operator -(AsyncEvent @event, Action task)
        {
            @event.actions.Remove(task);
            return @event;
        }

        public async Task RaiseAsync()
        {
            actions.ForEach(x => x());
            await Task.WhenAll(tasks.Select(x => x())).ConfigureAwait(false);
        }
    }
}
