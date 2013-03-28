using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Squared.Render.Convenience;
using Squared.Util;
using Squared.Util.Event;
using Squared.Render;
using Squared.Game;

namespace RLMS.Framework {
    [Flags]
    public enum MessageStyle : int {
        Default = 0x0,
        ManualClose = 0x0,
        AcceptsInput = 0x0,
        AutoClose = 0x1,
        Passive = 0x2
    }

    public class Message : IThreadedComponent, IDisposable {
        public const float BorderSize = 12f;
        public const float BackgroundOpacity = 0.75f;
        public const float HoldTimePerCharacter = 0.04f;
        public const float HoldTime = 2.25f;
        public const float FadeInTime = 0.2f;
        public const float FadeOutTime = 0.66f;

        public readonly Game Game;
        public string[] Pages = null;
        public int ActivePage = 0;
        public float OpenedWhen, HoldDuration;
        public Action OnClose = null;
        public MessageStyle Style;
        public Vector2? Alignment = null;
        public bool HideWhenPaused = false;

        public Message (Game game, string text, MessageStyle style)
            : this(game, new string[] { text }, style) {
        }

        public Message (Game game, string[] pages, MessageStyle style) {
            Game = game;
            Pages = pages;
            ActivePage = 0;
            HoldDuration = HoldTime + (HoldTimePerCharacter * Text.Length);
            OpenedWhen = (float)Time.Seconds;
            Style = style;
        }

        public string Text {
            get {
                return Pages[ActivePage];
            }
        }

        public void FadeOut () {
            OpenedWhen = (float)Time.Seconds - FadeInTime;
            HoldDuration = 0.0f;
            Style |= MessageStyle.AutoClose;
        }

        public void Dispose () {
            Game.Components.Remove(this);
        }

        public void Close () {
            if (OnClose != null)
                OnClose();

            Game.Components.Remove(this);
        }

        public void Shown () {
            if ((Style & MessageStyle.Passive) != MessageStyle.Passive)
                Game.InputControls.EventBus.Subscribe(null, null, EventListener);
        }

        public void Hidden () {
            if ((Style & MessageStyle.Passive) != MessageStyle.Passive)
                Game.InputControls.EventBus.Unsubscribe(null, null, EventListener);
        }

        protected void EventListener (EventInfo e) {
            if (HideWhenPaused && Game.Paused)
                return;

            e.Consume();

            int scroll = 0;

            if ((e.Source == Game.InputControls.Left) || (e.Source == Game.InputControls.Up))
                scroll = -1;
            else if ((e.Source == Game.InputControls.Right) || (e.Source == Game.InputControls.Down))
                scroll = 1;

            if (scroll != 0) {
                if (
                    (Pages.Length > 1) && (
                        (e.Type == "Press") ||
                        (e.Type == "RepeatPress")
                    )
                ) {
                    ActivePage = Arithmetic.Wrap(ActivePage + scroll, 0, Pages.Length - 1);
                }
            } else if (e.Type == "Press") {
                Close();
            }
        }

        public void Update () {
            if ((Style & MessageStyle.AutoClose) == MessageStyle.AutoClose) {
                var t = (float)Time.Seconds;

                if (t > (OpenedWhen + HoldDuration + FadeInTime + FadeOutTime))
                    Close();
            }
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (HideWhenPaused && Game.Paused)
                return;

            int layerOffset = 0;
            var t = (float)Time.Seconds - OpenedWhen;
            float opacity;

            if (t <= FadeInTime) {
                opacity = MathHelper.Lerp(0.0f, 1.0f, t / FadeInTime);
            } else if (t <= FadeInTime + HoldDuration) {
                opacity = 1.0f;
                layerOffset = 2;
            } else {
                if ((Style & MessageStyle.AutoClose) == MessageStyle.AutoClose) {
                    opacity = MathHelper.Lerp(1.0f, 0.0f, (t - (FadeInTime + HoldDuration)) / FadeOutTime);
                } else {
                    opacity = 1.0f;
                    layerOffset = 2;
                }
            }

            var size = Game.UIText.MeasureString(Text);

            var clippedSize = new Vector2(size.X, size.Y);
            if (clippedSize.X > 1000)
                clippedSize.X = 1000;

            var tl = new Vector2(
                (Game.ViewportWidth - clippedSize.X),
                (Game.ViewportHeight - clippedSize.Y)
            ) * Alignment.GetValueOrDefault(new Vector2(0.5f, 0.5f));
            var br = tl + clippedSize;
            var border = BorderSize;

            var colorInner = new Color(0.0f, 0.0f, 0.0f, BackgroundOpacity * opacity);
            var colorOuter = new Color(0.0f, 0.0f, 0.0f, 0.0f);

            renderer.FillRectangle(new Bounds(tl, br), colorInner);
            renderer.Layer += 1;
            renderer.DrawString(Game.UIText, Text, tl);
        }
    }
}
