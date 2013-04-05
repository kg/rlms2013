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
    public class NarrativeState : IThreadedState {
        public readonly Game Game;

        public Scene CurrentScene;

        public readonly Textbox Textbox;

        private bool ContentLoaded = false;
        public bool AdvancePromptVisible = false;

        public Texture2D AdvancePromptIcon;

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
            yield return Game.ContentLoader.LoadContent<Texture2D>("advance").Bind(() => AdvancePromptIcon);

            yield return Textbox.LoadContent();

            ContentLoaded = true;
        }

        public IEnumerator<object> Main () {
            yield return LoadContent();

            while (CurrentScene != null) {
                var scene = CurrentScene;
                CurrentScene = null;
                yield return scene.Play(this);
                yield return new WaitForNextStep();
            }
        }

        public void Update () {
            Textbox.Update();
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;

            Textbox.Draw(frame, ref renderer);

            if (AdvancePromptVisible) {
                var advancePromptPosition = Textbox.Bounds.BottomRight;
                advancePromptPosition.Y -= Squared.Util.Arithmetic.Pulse((float)(Squared.Util.Time.Seconds * 0.66), 0f, 24f);
                renderer.Draw(AdvancePromptIcon, advancePromptPosition, origin: new Vector2(0.85f, 0.8f));
            }
        }
    }

    public static class NarrativeStateExtensions {
        public static SignalFuture Play (this Scene scene, NarrativeState state) {
            var result = new SignalFuture();
            scene.Initialize(state);
            state.Scheduler.Start(result, new SchedulableGeneratorThunk(scene.Main()), TaskExecutionPolicy.RunAsBackgroundTask);
            return result;
        }
    }
}
