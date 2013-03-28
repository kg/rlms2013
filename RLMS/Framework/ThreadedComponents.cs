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

    public class ThreadedStateStack {
        private Dictionary<IThreadedState, IFuture> Futures = new Dictionary<IThreadedState, IFuture>();
        private Stack<IThreadedState> Stack = new Stack<IThreadedState>();
        private TaskScheduler Scheduler;

        public ThreadedStateStack (TaskScheduler scheduler) {
            Scheduler = scheduler;
        }

        public IFuture Push (IThreadedState state) {
            if (Futures.ContainsKey(state))
                throw new InvalidOperationException();

            Stack.Push(state);

            var future = Scheduler.Start(
                state.Main(), TaskExecutionPolicy.RunAsBackgroundTask
            );
            Futures[state] = future;

            return future;
        }

        public void Pop () {
            var state = Stack.Pop();

            Futures[state].Dispose();
            Futures.Remove(state);
        }

        public IThreadedState Current {
            get {
                return Stack.Peek();
            }
        }
    }
}
