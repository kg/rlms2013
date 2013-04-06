using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using RLMS.States.Exploration;
using RLMS.States.Narrative;
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
        public readonly AreaView View;

        public readonly InputHandler Handler;

        private readonly Queue<Func<object>> PendingTasks = new Queue<Func<object>>();

        private bool Running = true;
        private bool ContentLoaded = false;

        private InputEventSubscription AcceptSubscription;

        public ExplorationState (Game game, Area area) {
            Game = game;
            Area = area;
            View = new AreaView();
            Handler = new InputHandler(this);

            AcceptSubscription = game.InputControls.Accept.AddListener(DefaultAcceptListener);

            EnterArea(area);
        }

        public Area Area {
            get;
            private set;
        }

        public bool IsTopmost {
            get;
            set;
        }

        public void Dispose () {
            AcceptSubscription.Dispose();
        }

        private bool DefaultAcceptListener (InputControl c, InputEvent e) {
            if (!IsTopmost)
                return false;

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

        protected IEnumerator<object> LoadContent () {
            ContentLoaded = true;

            yield break;
        }

        public IEnumerator<object> Main () {
            yield return LoadContent();

            while (Running || (PendingTasks.Count > 0)) {
                if (PendingTasks.Count > 0) {
                    var pt = PendingTasks.Dequeue();
                    yield return pt();
                }

                yield return new WaitForNextStep();
            }
        }

        public void EnqueueTask (Func<IEnumerator<object>> task) {
            PendingTasks.Enqueue(task);
        }

        private void EnterArea (Area newArea) {
            newArea.Initialize(this);
            Area = newArea;
            EnqueueTask(newArea.LoadContent);
            EnqueueTask(newArea.OnEnter);
        }

        public void ExitArea (Area newArea = null) {
            EnqueueTask(Area.OnExit);
            PendingTasks.Enqueue(() => {
                if (newArea == null)
                    Running = false;
                else
                    EnterArea(newArea);

                return null;
            });
        }

        public IEnumerable<Hotspot> HitTest (Vector2 position) {
            return HitTest(new Bounds(position, position));
        }

        public IEnumerable<Hotspot> HitTest (Bounds bounds) {
            foreach (var hotspot in Area.Hotspots)
                if (bounds.Intersects(hotspot.Bounds))
                    yield return hotspot;
        }

        public void Update () {
            View.Update();

            Handler.Update();
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (!ContentLoaded)
                return;

            View.Draw(frame, ref renderer);

            renderer.Layer += 1;

            Handler.Draw(ref renderer);
        }

        class HotspotOnClickFallbackScene : Scene {
            public readonly Hotspot Hotspot;

            public HotspotOnClickFallbackScene (Hotspot hotspot) {
                Hotspot = hotspot;
            }

            public override IEnumerator<object> Main () {
                yield return ShowSmartText("Hotspot '" + Hotspot.Name + "' has no OnClick handler.");

                yield return ShowAdvancePrompt();
            }
        }

        public class InputHandler {
            public readonly ExplorationState State;

            public InputHandler (ExplorationState state) {
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

                var hotspot = State.HitTest(position).LastOrDefault();
                if (hotspot != null) {
                    var onclick = hotspot.OnClick ?? 
                        (() => HotspotOnClickFallback(hotspot));

                    State.EnqueueTask(onclick);
                }

                return true;
            }

            private IEnumerator<object> HotspotOnClickFallback (Hotspot hotspot) {
                yield return Game.PlayScene(new HotspotOnClickFallbackScene(hotspot));
            }

            public void Draw (ref ImperativeRenderer renderer) {
                if (Game.InputControls.MouseLocation.HasValue) {
                    var hotspot = State.HitTest(Game.InputControls.MouseLocation.Value).LastOrDefault();
                    if (hotspot == null)
                        return;

                    renderer.FillRectangle(hotspot.Bounds, Color.White * 0.1f);

                    var textLayout = Game.UIText.LayoutString(hotspot.ToString(), null);
                    var offset = ((hotspot.Bounds.Size - textLayout.Size) * 0.5f) + hotspot.Bounds.TopLeft;
                    offset.X = (float)Math.Floor(offset.X);
                    offset.Y = (float)Math.Floor(offset.Y);

                    renderer.DrawMultiple(textLayout, offset + Vector2.One, multiplyColor: Color.Black, sortKey: 0);
                    renderer.DrawMultiple(textLayout, offset, sortKey: 1);
                }
            }

            public void Update () {
            }
        }
    }
}
