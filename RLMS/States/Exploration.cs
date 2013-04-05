using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using Squared.Game;
using Squared.Game.Input;
using Squared.Render;
using Squared.Render.Convenience;
using RLMS.States.Action;
using Squared.Task;
using Squared.Util.Event;

namespace RLMS.States {
    public class ExplorationState : IThreadedState {
        public readonly Game Game;

        private bool ContentLoaded = false;

        public ExplorationState (Game game) {
            Game = game;

            // Game.InputControls.Accept.AddListener(DefaultAcceptListener);
        }

        /*
        private bool DefaultAcceptListener (InputControl c, InputEvent e) {
            var ml = Game.InputControls.MouseLocation;
            if (!ml.HasValue)
                return false;

            return Handler.HandleAccept(ml.Value, e);
        }
         */

        public EventBus EventBus {
            get {
                return Game.EventBus;
            }
        }

        protected IEnumerator<object> LoadContent () {
            ContentLoaded = true;

            yield break;
        }

        public IEnumerator<object> Main () {
            yield return LoadContent();

            while (true)
                yield return new WaitForNextStep();
        }

        public void Update () {
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;
        }
    }
}
