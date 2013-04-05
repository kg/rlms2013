using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Squared.Render.Convenience;
using Squared.Util;
using Squared.Game;
using Squared.Task;
using Squared.Util.Event;
using Squared.Render;

namespace RLMS.Framework {
    public interface IMenuItem {
        void Initialize (Menu menu);
        void Draw (Game game, ref ImperativeRenderer renderer, Vector2 itemPosition, Vector2 itemSize, bool selected);
        Vector2 Measure (Game game);
        void HandleInput (Game game, EventInfo e);
    }

    public class TextMenuItem : IMenuItem, IEventSource {
        public string Text;
        public Action<TextMenuItem> Handler;
        public SpriteFont Font;

        public virtual void Initialize (Menu menu) {
            if (Font == null)
                Font = menu.Font;
        }

        public virtual void Draw (Game game, ref ImperativeRenderer renderer, Vector2 itemPosition, Vector2 itemSize, bool selected) {
            if (Text == null)
                return;

            renderer.DrawString(
                Font, Text, itemPosition, 
                (selected) ? Color.White : new Color(127, 127, 127, 255)
            );
        }

        public virtual Vector2 Measure (Game game) {
            if (Text == null)
                return Vector2.Zero;

            return Font.MeasureString(Text);
        }

        public virtual void HandleInput (Game game, EventInfo e) {
            if (e.Source == game.InputControls.Accept) {
                if (e.Type == "Press") {
                    if (Handler != null) {
                        game.EventBus.Broadcast(this, "MenuItemSelected", null);

                        Handler(this);
                    }
                }
            }
        }

        public override string ToString () {
            return String.Format("MenuItem {0}", Text);
        }

        string IEventSource.CategoryName {
            get {
                return "Menu";
            }
        }
    }

    public class Menu : IThreadedComponent, IEventSource, IDisposable {
        public const float BorderSize = 12f;
        public const float BackgroundOpacity = 0.75f;

        public readonly string Description;
        public event Action Cancelled;

        public readonly Game Game;
        public SpriteFont Font;
        public List<IMenuItem> Items = new List<IMenuItem>();
        public int SelectedIndex = 0;

        public Menu (Game game, string description, IFuture future, params IMenuItem[] items) {
            Game = game;
            Description = description;
            Font = game.UIText;

            Items.AddRange(items);
        }

        public static IEnumerator<object> ShowNew (Game game, string description, IEnumerable<object> items, SpriteFont font = null) {
            var f = new Future<string>();

            var menu = new Menu(game, description, f);
            menu.Font = font ?? menu.Font;

            foreach (var item in items) {
                if (item is IMenuItem) {
                    menu.Items.Add((IMenuItem)item);
                } else if (item is string) {
                    var menuItem = new TextMenuItem {
                        Text = (string)item,
                        Handler = (i) => {
                            menu.Close();
                            f.SetResult(i.Text, null);
                        }
                    };
                    menu.Items.Add(menuItem);
                } else {
                    throw new InvalidOperationException("Menu items must be strings or IMenuItem instances");
                }
            }

            menu.Cancelled += () => { f.SetResult(null, null); };

            while (!game.InputControls.Available)
                yield return new WaitForNextStep();

            game.Components.Add(menu);

            yield return f;

            yield return new Result(f.Result);
        }

        public void Shown () {
            foreach (var item in Items)
                item.Initialize(this);

            Game.InputControls.EventBus.Subscribe(null, null, EventListener);
            Game.InputControls.EventBus.Subscribe(null, "ControllerDisconnected", _ControllerDisconnected);

            Game.EventBus.Broadcast(this, "MenuOpened", null);
        }

        public void Hidden () {
            Game.InputControls.EventBus.Unsubscribe(null, null, EventListener);
            Game.InputControls.EventBus.Unsubscribe(null, "ControllerDisconnected", _ControllerDisconnected);

            Game.EventBus.Broadcast(this, "MenuClosed", null);
        }

        protected void EventListener (EventInfo e) {
            e.Consume();

            if ((e.Type == "Press") || (e.Type == "RepeatPress")) {
                if (e.Source == Game.InputControls.Cancel) {
                    Cancel();
                    return;
                } else if (e.Source == Game.InputControls.Up) {
                    SelectedIndex = SelectedIndex - 1;
                    if (SelectedIndex < 0)
                        SelectedIndex = Items.Count - 1;

                    Game.EventBus.Broadcast(this, "MenuCursorMoved", SelectedIndex);
                    return;
                } else if (e.Source == Game.InputControls.Down) {
                    SelectedIndex = SelectedIndex + 1;
                    if (SelectedIndex >= Items.Count)
                        SelectedIndex = 0;

                    Game.EventBus.Broadcast(this, "MenuCursorMoved", SelectedIndex);
                    return;
                }
            }

            var item = Items[SelectedIndex];
            item.HandleInput(Game, e);
        }

        protected void _ControllerDisconnected (EventInfo e) {
            Cancel();
        }

        public void Cancel () {
            if (this.Cancelled != null)
                this.Cancelled();

            Game.EventBus.Broadcast(this, "MenuCancelled", SelectedIndex);

            this.Close();
        }

        public void Dispose () {
            Game.Components.Remove(this);
        }

        public void Close () {
            Game.Components.Remove(this);
        }

        public static void DrawCursor (Game game, ref ImperativeRenderer renderer, Vector2 itemPosition, Vector2 menuSize, Vector2 itemSize) {
            var time = (float)Time.Seconds;
            var scale = Vector2.One;
            var offset = Arithmetic.Pulse(time, 0.6f, 0.75f);
            var origin = new Vector2(0.5f, 0.5f);
            var bounds = new Bounds(Vector2.Zero, Vector2.One);

            itemPosition.X -= (game.Cursor.Width * offset);
            itemPosition.Y += (itemSize.Y / 2.0f);
            var drawCall = new Squared.Render.BitmapDrawCall(
                game.Cursor, itemPosition, bounds, Color.White, scale, origin
            );

            renderer.Draw(ref drawCall);

            itemPosition.X += menuSize.X + (game.Cursor.Width * offset * 2.0f);
            drawCall.Position = itemPosition;
            drawCall.Mirror(true, false);

            renderer.Draw(ref drawCall);
        }

        protected Vector2 Measure () {
            var size = new Vector2(0, 0);

            foreach (var item in Items) {
                var itemSize = item.Measure(Game);
                size.X = Math.Max(size.X, itemSize.X);
                size.Y += itemSize.Y;
            }

            return size;
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            var size = Measure();
            var pos = new Vector2(Game.ViewportWidth - size.X, Game.ViewportHeight - size.Y) * new Vector2(0.5f, 0.5f);

            pos.X = (float)Math.Floor(pos.X);
            pos.Y = (float)Math.Floor(pos.Y);

            var menuBounds = new Bounds(                    
                pos - new Vector2(4, 4),
                pos + size + new Vector2(4, 4)
            );

            renderer.FillRectangle(
                menuBounds,
                new Color(0.0f, 0.0f, 0.0f, BackgroundOpacity)
            );
            renderer.Layer += 1;

            renderer.OutlineRectangle(
                menuBounds,
                Color.White
            );

            renderer.Layer += 1;

            for (int i = 0; i < Items.Count; i++) {
                var item = Items[i];
                var itemSize = item.Measure(Game);
                var selected = (i == SelectedIndex);

                item.Draw(Game, ref renderer, pos, itemSize, selected);

                if (selected)
                    DrawCursor(Game, ref renderer, pos, size, itemSize);

                pos.Y += itemSize.Y;
            }

            renderer.Layer += 1;
        }

        public void Update () {
            var ml = Game.InputControls.MouseLocation;
            if (!ml.HasValue)
                return;

            var size = Measure();
            var pos = new Vector2(Game.ViewportWidth - size.X, Game.ViewportHeight - size.Y) * new Vector2(0.5f, 0.5f);

            for (int i = 0; i < Items.Count; i++) {
                var item = Items[i];
                var itemSize = item.Measure(Game);
                var itemBounds = new Bounds(
                    pos, pos + itemSize
                );

                if (itemBounds.Contains(ml.Value))
                    SelectedIndex = i;

                pos.Y += itemSize.Y;
            }
        }

        public override string ToString () {
            return String.Format("{0}Menu", Description);
        }

        string IEventSource.CategoryName {
            get {
                return "Menu";
            }
        }
    }

    public class Picture : IThreadedComponent {
        public const float FadeInTime = 1.0f;

        public readonly Game Game;
        public Texture2D Image;
        public Vector2 Center;
        public long ShownWhen;

        public Picture (Game game, Texture2D image) {
            Game = game;

            Image = image;
            Center = new Vector2(game.ViewportWidth / 2.0f, game.ViewportHeight / 2.0f);
        }

        public void Shown () {
            ShownWhen = Time.Ticks;
        }

        public void Hidden () {
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            var now = Time.Ticks;
            float alpha = (float)(TimeSpan.FromTicks(now - ShownWhen).TotalSeconds / FadeInTime);
            var color = new Color(
                alpha, alpha, alpha, alpha
            );

            renderer.Draw(new Squared.Render.BitmapDrawCall(
                Image, Center, new Bounds(Vector2.Zero, Vector2.One), color, 
                Vector2.One, new Vector2(0.5f, 0.5f)
            ));
        }

        public void Update () {
        }
    }
}
