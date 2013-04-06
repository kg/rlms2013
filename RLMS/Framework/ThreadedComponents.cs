using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;

namespace RLMS.Framework {
    public class ThreadedComponents : IEnumerable<IThreadedComponent> {
        private HashSet<IThreadedComponent> Items = new HashSet<IThreadedComponent>();

        public void Add (IThreadedComponent component) {
            if (Items.Add(component))
                component.Shown();
        }

        public void Remove (IThreadedComponent component) {
            if (Items.Remove(component))
                component.Hidden();
        }

        public void Clear () {
            foreach (var item in Items)
                item.Hidden();

            Items.Clear();
        }

        public IEnumerator<IThreadedComponent> GetEnumerator () {
            return Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return Items.GetEnumerator();
        }
    }

    public class ThreadedStateStack : IEnumerable<IThreadedState> {
        private Dictionary<IThreadedState, IFuture> Futures = new Dictionary<IThreadedState, IFuture>();
        private List<IThreadedState> Stack = new List<IThreadedState>();
        private TaskScheduler Scheduler;

        public ThreadedStateStack (TaskScheduler scheduler) {
            Scheduler = scheduler;
        }

        public IFuture Push (IThreadedState state) {
            if (Futures.ContainsKey(state))
                throw new InvalidOperationException();

            if (Stack.Count > 0)
                Current.IsTopmost = false;

            state.IsTopmost = true;
            Stack.Add(state);

            var future = Scheduler.Start(
                state.Main(), TaskExecutionPolicy.RunWhileFutureLives
            );
            Futures[state] = future;

            future.RegisterOnComplete((_) => {
                Remove(state);
            });
            future.RegisterOnDispose((_) => {
                Remove(state);
            });

            return future;
        }

        internal void Remove (IThreadedState state) {
            if (!Futures.ContainsKey(state))
                return;

            Stack.Remove(state);
            if (Stack.Count > 0)
                Stack[Stack.Count - 1].IsTopmost = true;

            var f = Futures[state];
            Futures.Remove(state);

            f.Dispose();
            if (state is IDisposable)
                ((IDisposable)state).Dispose();
        }

        public void Pop () {
            var state = Stack.Last();
            Remove(state);
        }

        public int Count {
            get {
                return Stack.Count;
            }
        }

        public IThreadedState Current {
            get {
                if (Stack.Count == 0)
                    return null;

                return Stack[Stack.Count - 1];
            }
        }

        public IEnumerator<IThreadedState> GetEnumerator () {
            return Stack.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return Stack.GetEnumerator();
        }
    }
}
