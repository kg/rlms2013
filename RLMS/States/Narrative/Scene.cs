using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using RLMS.Framework;
using Squared.Task;

namespace RLMS.States.Narrative {
    public abstract class Scene {
        // BLEH. Don't want to require every scene class to define a constructor that just calls base()...
        public void Initialize (NarrativeState state) {
            State = state;
        }

        public NarrativeState State {
            get;
            private set;
        }

        public Game Game {
            get {
                return State.Game;
            }
        }

        public Textbox Textbox {
            get {
                return State.Textbox;
            }
        }

        public InputControls Controls {
            get {
                return State.Game.InputControls;
            }
        }

        public Sleep Pause (float duration = 0.7f) {
            float multiplier = Controls.Accept.State ? 0.25f : 1.0f;
            return new Sleep(multiplier * duration);
        }

        protected IEnumerator<object> FastAdvance () {
            yield return new Sleep(0.4);

            if (!Controls.Accept.State)
                yield return ShowAdvancePrompt();
        }

        public IEnumerator<object> ShowSmartText (string text, string speaker = "Monologue") {
            var words = Textbox.SplitWords(text);
            var pauseChars = new Dictionary<char, float> {
                {'\n', 0.1f},
                {'.', 0.5f},
                {'?', 0.5f},
                {'!', 0.5f},
                {':', 0.15f},
                {',', 0.07f},
                {'\u2026', 0.35f}
            };
            var pauseCharList = pauseChars.Keys.ToArray();

            IFuture f = null;
            foreach (var word in words) {
                f = Textbox.AddText(word, speaker = speaker);

                var pauseCharIndex = word.LastIndexOfAny(pauseCharList);
                if (pauseCharIndex >= 0) {
                    yield return f;
                    f = null;

                    var pauseChar = word[pauseCharIndex];
                    yield return Pause(pauseChars[pauseChar]);
                }
            }

            if (f != null)
                yield return f;
        }

        public IFuture ShowAdvancePrompt () {
            if (Controls.Accept.State) {
                return State.Scheduler.Start(FastAdvance());
            } else {
                State.AdvancePromptVisible = true;
                var f = Controls.Accept.WaitForPress();
                f.RegisterOnComplete((_) => {
                    State.AdvancePromptVisible = false;
                });
                return f;
            }
        }

        public abstract IEnumerator<object> Main ();

        public static Type[] GetAllSceneTypes () {
            var tScene = typeof(Scene);
            return (
                from t in Assembly.GetExecutingAssembly().GetTypes() 
                where !t.IsAbstract && tScene.IsAssignableFrom(t) 
                select t
            ).ToArray();
        }
    }

    public class Branch : ISchedulable, IEnumerable<Branch.BranchItem> {
        public class BranchItem : TextMenuItem {
            public readonly Branch Branch;
            public readonly Func<IEnumerator<object>> Task;
            public readonly string Speaker;
            public readonly Color Color;

            public BranchItem (Branch branch, string text, string speaker, Func<IEnumerator<object>> task) {
                Branch = branch;
                Text = text;
                Speaker = speaker;
                Color = Speakers.ByName[speaker].Color;
                Task = task;
            }

            public override void Initialize (Menu menu) {
                base.Initialize(menu);

                Handler = (_) => {
                    Branch.Choice = this;
                    menu.Cancel();
                };
            }

            public override void Draw (Game game, ref Squared.Render.Convenience.ImperativeRenderer renderer, Microsoft.Xna.Framework.Vector2 itemPosition, Microsoft.Xna.Framework.Vector2 itemSize, bool selected) {
                if (Text == null)
                    return;

                renderer.DrawString(
                    Font, Text, itemPosition,
                    Color * ((selected) ? 1f : 0.66f)
                );
            }
        }

        public readonly Scene Scene;
        public readonly List<BranchItem> Items = new List<BranchItem>();
        public BranchItem Choice; 

        public Branch (Scene scene) {
            Scene = scene;
        }

        public void Add (string text, Func<IEnumerator<object>> task, string speaker = "Monologue") {
            Items.Add(new BranchItem(this, text, speaker, task));
        }

        void ISchedulable.Schedule (TaskScheduler scheduler, IFuture future) {
            scheduler.Start(future, new SchedulableGeneratorThunk(ScheduleTask()), TaskExecutionPolicy.RunAsBackgroundTask);
        }

        IEnumerator<object> ScheduleTask () {
            if (Items.Count == 0)
                throw new InvalidOperationException("A branch must contain at least one option");

            Choice = null;

            yield return Menu.ShowNew(
                Scene.Game, "Dialogue Branch", Items,
                font: Scene.Textbox.DialogueFont
            );

            if (Choice != null) {
                Scene.Textbox.Clear();
                yield return Scene.Textbox.AddText(Choice.Text, font: Choice.Font, speaker: Choice.Speaker, lineBreak: true);
                yield return Scene.Pause(0.25f);

                yield return Choice.Task();
            }
        }

        IEnumerator<Branch.BranchItem> IEnumerable<Branch.BranchItem>.GetEnumerator () {
            return Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return Items.GetEnumerator();
        }
    }
}