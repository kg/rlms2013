using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using Squared.Render;
using Squared.Render.Convenience;
using RLMS.States.Action;
using Squared.Task;
using Squared.Util.Event;

namespace RLMS.States {
    public class ActionState : IThreadedState {
        public readonly Game Game;

        // FIXME
        public readonly LumberMill Mill = new LumberMill();
        public readonly List<Entity> Entities = new List<Entity>();

        public Texture2D Background;
        public Texture2D Log;

        private bool ContentLoaded = false;

        public ActionState (Game game) {
            Game = game;

            SetupRuntimeState();
        }

        public EventBus EventBus {
            get {
                return Game.EventBus;
            }
        }

        protected void SetupRuntimeState () {
            foreach (var lj in Mill.Roster)
                Entities.Add(new RuntimeLumberjack(this, lj));
        }

        protected IEnumerator<object> LoadContent () {
            yield return Game.ContentLoader.LoadContent<Texture2D>("lumbermill").Bind(() => Background);
            yield return Game.ContentLoader.LoadContent<Texture2D>("log").Bind(() => Log);

            ContentLoaded = true;
        }

        public IEnumerator<object> Main () {
            yield return LoadContent();

            while (true)
                yield return new WaitForNextStep();
        }

        public void Update () {
            foreach (var entity in Entities)
                entity.Update();
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;

            renderer.Draw(Background, 0, 0);
            renderer.Layer += 1;
        }
    }
}
