using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using RLMS.States.Narrative;
using Squared.Game;
using Squared.Game.Input;
using Squared.Render;
using Squared.Render.Convenience;
using RLMS.States.Action;
using Squared.Task;
using Squared.Util.Event;

namespace RLMS.States {
    public class NarrativeState : IThreadedState, IDisposable {
        public readonly Game Game;

        public Scene CurrentScene;

        public readonly Textbox Textbox;

        internal readonly HashSet<IDisposable> Disposables = new HashSet<IDisposable>();

        private bool ContentLoaded = false;

        public NarrativeState (Game game, Scene scene) {
            Game = game;
            CurrentScene = scene;

            Textbox = new Textbox(this);
        }

        public EventBus EventBus {
            get {
                return Game.EventBus;
            }
        }

        public TaskScheduler Scheduler {
            get {
                return Program.TaskScheduler;
            }
        }

        protected IEnumerator<object> LoadContent () {
            yield return Textbox.LoadContent();

            ContentLoaded = true;
        }

        public IEnumerator<object> Main () {
            yield return LoadContent();

            while (CurrentScene != null) {
                var scene = CurrentScene;
                CurrentScene = null;
                var playFuture = scene.Play(this);
                Disposables.Add(playFuture);
                try {
                    yield return playFuture;
                } finally {
                    Disposables.Remove(playFuture);
                }
                yield return new WaitForNextStep();
            }
        }

        public void Dispose () {
            CurrentScene = null;

            foreach (var disposable in Disposables)
                disposable.Dispose();

            Disposables.Clear();
        }

        public void Update () {
            Textbox.Update();
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;

            Textbox.Draw(frame, ref renderer);
        }
    }

    public static class NarrativeStateExtensions {
        public static SignalFuture Play (this Scene scene, NarrativeState state) {
            var result = new SignalFuture();
            scene.Initialize(state);
            state.Scheduler.Start(result, new SchedulableGeneratorThunk(scene.Main()), TaskExecutionPolicy.RunWhileFutureLives);
            state.Disposables.Add(result);
            return result;
        }
    }
}
