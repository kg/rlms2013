using System;
using System.Collections.Generic;
using System.Linq;
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

        public Sleep Pause () {
            return new Sleep(Controls.Accept.State ? 0.2f : 0.7f);
        }

        public SignalFuture ShowAdvancePrompt () {
            State.AdvancePromptVisible = true;
            var f = Controls.Accept.WaitForPress();
            f.RegisterOnComplete((_) => {
                State.AdvancePromptVisible = false;
            });
            return f;
        }

        public abstract IEnumerator<object> Main ();
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
                Color = Colors.ByName[speaker];
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
                yield return Scene.Textbox.Sentence(Choice.Text, font: Choice.Font, speaker: Choice.Speaker, lineBreak: true);
                yield return new Sleep(0.2f);

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