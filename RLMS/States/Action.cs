using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using Squared.Game;
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

        public T Building<T> ()
            where T : RuntimeBuilding {

            return Entities.OfType<T>().First();
        }

        protected void SetupRuntimeState () {
            foreach (var b in Mill.Buildings)
                Entities.Add(b.CreateRuntimeBuilding(this));

            foreach (var lj in Mill.Roster)
                Entities.Add(lj.CreateRuntimeEntity(this));
        }

        protected IEnumerator<object> LoadContent () {
            yield return Game.ContentLoader.LoadContent<Texture2D>("lumbermill").Bind(() => Background);
            yield return Game.ContentLoader.LoadContent<Texture2D>("log").Bind(() => Log);

            ContentLoaded = true;
        }

        public IEnumerable<Entity> HitTest (Vector2 position) {
            return HitTest(new Bounds(position, position));
        }

        public IEnumerable<Entity> HitTest (Bounds bounds) {
            foreach (var entity in Entities)
                if (bounds.Intersects(entity.Bounds))
                    yield return entity;
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

            foreach (var entity in Entities)
                entity.Draw(ref renderer);

            if (Game.InputControls.MouseLocation.HasValue) {
                foreach (var entity in HitTest(Game.InputControls.MouseLocation.Value)) {
                    renderer.FillRectangle(entity.Bounds, Color.White * 0.5f);

                    renderer.DrawString(Game.UIText, entity.ToString(), entity.Bounds.TopRight);
                }
            }
        }
    }
}
