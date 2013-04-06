using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using RLMS.States.Exploration;
using Squared.Game;
using Squared.Game.Input;
using Squared.Render;
using Squared.Render.Convenience;
using RLMS.States.Action;
using Squared.Task;
using Squared.Util.Event;

namespace RLMS.States {
    public class ExplorationState : IThreadedState, IDisposable {
        public readonly Game Game;
        public readonly Area Area;

        private bool ContentLoaded = false;

        private InputEventSubscription AcceptSubscription;

        public ExplorationState (Game game, Area area) {
            Game = game;
            Area = area;

            AcceptSubscription = game.InputControls.Accept.AddListener(DefaultAcceptListener);

            area.Initialize(this);
        }

        public void Dispose () {
            AcceptSubscription.Dispose();
        }

        private bool DefaultAcceptListener (InputControl c, InputEvent e) {
            var ml = Game.InputControls.MouseLocation;
            if (!ml.HasValue)
                return false;

            return true;
        }

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

            yield return Area.OnEnter();

            while (true)
                yield return new WaitForNextStep();

            yield return Area.OnExit();
        }

        public void Update () {
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;
        }
    }
}
