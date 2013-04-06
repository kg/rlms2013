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
                {'\u2026', 0.35f},
                {'\u65E0', 0.2f},
            };
            var pauseCharList = pauseChars.Keys.ToArray();

            IFuture f = null;
            foreach (var word in words) {
                f = Textbox.AddText(word, speaker: speaker);

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
                Textbox.AdvancePromptVisible = true;
                var f = Controls.Accept.WaitForPress();
                f.RegisterOnComplete((_) => {
                    Textbox.AdvancePromptVisible = false;
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
}