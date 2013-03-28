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
    public class ActionState : IThreadedState {
        public class InputHandler {
            public readonly ActionState State;

            protected Entity[] OrderingEntities = null;

            public InputHandler (ActionState state) {
                State = state;
            }

            protected Game Game {
                get {
                    return State.Game;
                }
            }

            public bool HandleAccept (Vector2 position, InputEvent evt) {
                if (evt.Type != InputEventType.Press)
                    return true;

                var entities = State.HitTest<RuntimeLumberjack>(position).ToArray();

                if ((OrderingEntities != null) && (entities.Length == 0)) {
                    var oe = OrderingEntities;
                    OrderingEntities = null;

                    foreach (var e in oe)
                        e.MoveTo(position);
                } else {
                    OrderingEntities = entities;
                }

                return true;
            }

            public void Draw (ref ImperativeRenderer renderer) {
                if (Game.InputControls.MouseLocation.HasValue) {
                    foreach (var entity in State.HitTest<Entity>(Game.InputControls.MouseLocation.Value)) {
                        renderer.FillRectangle(entity.Bounds, Color.White * 0.5f);

                        renderer.DrawString(Game.UIText, entity.ToString(), entity.Bounds.TopRight);
                    }
                }
            }

            public void Update () {
            }
        }

        public readonly Game Game;

        // FIXME
        public readonly LumberMill Mill = new LumberMill();
        public readonly List<Entity> Entities = new List<Entity>();

        public readonly InputHandler Handler;

        public Texture2D Background;
        public Texture2D Log;

        private bool ContentLoaded = false;

        public ActionState (Game game) {
            Game = game;
            Handler = new InputHandler(this);

            SetupRuntimeState();

            Game.InputControls.Accept.AddListener(DefaultAcceptListener);
        }

        private bool DefaultAcceptListener (InputControl c, InputEvent e) {
            var ml = Game.InputControls.MouseLocation;
            if (!ml.HasValue)
                return false;

            return Handler.HandleAccept(ml.Value, e);
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

        public IEnumerable<Entity> HitTest<T> (Vector2 position) 
            where T : Entity
        {
            return HitTest<T>(new Bounds(position, position));
        }

        public IEnumerable<Entity> HitTest<T> (Bounds bounds) 
            where T : Entity
        {
            foreach (var entity in Entities.OfType<T>())
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

            Handler.Update();
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;

            renderer.Draw(Background, 0, 0);
            renderer.Layer += 1;

            foreach (var entity in Entities)
                entity.Draw(ref renderer);

            renderer.Layer += 1;

            Handler.Draw(ref renderer);
        }
    }
}
