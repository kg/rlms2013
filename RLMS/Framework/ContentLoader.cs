using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Task;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Squared.Util;
using System.Diagnostics;

namespace RLMS.Framework {
    public class ContentLoader : IDisposable {
        protected Thread _Thread;
        protected ContentManager _Content;
        protected TaskScheduler _Scheduler;
        protected IFuture _LoaderFuture;
        protected object _DeviceLock;
        protected BlockingQueue<Action> _WorkQueue = new BlockingQueue<Action>();

        public ContentLoader (ContentManager content, GraphicsDevice device, object deviceLock) {
            _Scheduler = new TaskScheduler();
            _Content = content;
            _DeviceLock = deviceLock;
            _LoaderFuture = _Scheduler.Start(LoaderTask(), TaskExecutionPolicy.RunAsBackgroundTask);
            _Thread = new Thread(ThreadFunc);
            _Thread.IsBackground = true;
            _Thread.Start();
        }

        protected void ThreadFunc () {
#if XBOX
            _Thread.SetProcessorAffinity(5);
#endif

            while (true) {
                _Scheduler.WaitForWorkItems();
                _Scheduler.Step();
            }
        }

        public TaskScheduler Scheduler {
            get {
                return _Scheduler;
            }
        }

        protected IEnumerator<object> BatchLoaderTask<T> (Future<Dictionary<string, T>> result, string[] filenames) {
            var dict = new Dictionary<string, T>();

            foreach (var filename in filenames) {
                var f = this.LoadContent<T>(filename);
                yield return f;
                if (f.Failed)
                    result.Fail(f.Error);
                else
                    dict.Add(Path.GetFileNameWithoutExtension(filename), f.Result);
            }

            result.Complete(dict);
        }

        public Future<Dictionary<string, T>> LoadBatch<T> (string sourceFolder) {
            var f = new Future<Dictionary<string, T>>();

            string contentFolder = Path.GetFullPath(_Content.RootDirectory) + "\\";
            sourceFolder = Path.Combine(contentFolder, sourceFolder);
            var filenames = System.IO.Directory.GetFiles(sourceFolder, "*.xnb").Select(
                (fn) => fn.Replace(contentFolder, "").Replace(".xnb", "")
            ).ToArray();

            var _ = _Scheduler.Start(BatchLoaderTask<T>(f, filenames));
            _.RegisterOnComplete((__) => {
                if (__.Failed) {
                    try {
                        f.SetResult(null, __.Error);
                    } catch {
                    }
                }
            });

            return f;
        }

        private static T LoadLocked<T> (ContentManager content, object deviceLock, string assetName) {
            lock (deviceLock)
                return content.Load<T>(assetName);
        }

        public Future<T> LoadContent<T> (string assetName) {
            var f = new Future<T>();

            var worker = (Action)(() => {
#if DEBUG
                long begin = Time.Ticks;
#endif

                try {
                    T result;
                    lock (_Content)
                        result = LoadLocked<T>(_Content, _DeviceLock, assetName);
                    f.SetResult(result, null);
                } catch (Exception ex) {
                    f.SetResult(default(T), ex);
                }

#if DEBUG
                long end = Time.Ticks;
                Debug.WriteLine(String.Format(
                    "Loaded asset '{0}' in {1:00.0000} sec(s).",
                    assetName, TimeSpan.FromTicks(end - begin).TotalSeconds)
                );
#endif
            });

            _WorkQueue.Enqueue(worker);

            return f;
        }

        protected IEnumerator<object> LoaderTask () {
            var sleep = new Sleep(0.03f);

            while (true) {
                var f = _WorkQueue.Dequeue();
                yield return f;

                var item = f.Result as Action;
                item();

                yield return sleep;
            }
        }

        public void Dispose () {
            _Thread.Abort();
            _LoaderFuture.Dispose();
            _Content.Dispose();
            _Scheduler.Dispose();
        }
    }
}
