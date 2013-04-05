using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Squared.Game;
using Squared.Render;
using Squared.Render.Convenience;
using Squared.Task;

namespace RLMS.States.Narrative {
    public class Textbox {
        public const int BlippyInterval = 4;

        public class TextString {
            public string Speaker;
            public StringLayout Layout;
            public readonly SignalFuture Future = new SignalFuture();

            public bool LineBreakAfter;
        }

        public readonly NarrativeState State;
        public readonly List<TextString> Strings = new List<TextString>();
        public readonly Bounds Bounds;

        public int TotalCharacterCount = 0;
        public int DisplayedCharacterCount = 0;
        public int NextBlipTime = 0;

        public SpriteFont DialogueFont, ItalicDialogueFont, MenuFont;
        public Dictionary<string, SoundEffect> Blips;

        public readonly Random BlippyRNG = new Random();

        public Textbox (NarrativeState state) {
            State = state;

            var size = new Vector2(Game.ViewportWidth - 32, (Game.ViewportHeight - 32) * 0.4f);
            var pos = new Vector2(Game.ViewportWidth - 16, Game.ViewportHeight - 16) - size;
            Bounds = Bounds.FromPositionAndSize(pos, size);
        }

        public Game Game {
            get {
                return State.Game;
            }
        }

        public IEnumerator<object> LoadContent () {
            yield return Game.ContentLoader.LoadContent<SpriteFont>("Dialogue").Bind(() => DialogueFont);
            yield return Game.ContentLoader.LoadContent<SpriteFont>("DialogueItalic").Bind(() => ItalicDialogueFont);

            yield return Game.ContentLoader.LoadBatch<SoundEffect>("sounds/blips").Bind(() => Blips);

            MenuFont = DialogueFont;
        }

        public void Update () {
            TotalCharacterCount = 0;

            int charactersLeft = DisplayedCharacterCount;

            SoundEffect currentBlip = null;

            foreach (var s in Strings) {
                TotalCharacterCount += s.Layout.Count;

                int charactersToDraw = Math.Min(charactersLeft, s.Layout.Count);
                var isFullyVisible = (charactersToDraw == s.Layout.Count);
                var isVisible = (charactersToDraw > 0);
                charactersLeft -= charactersToDraw;

                if (isFullyVisible && !s.Future.Completed)
                    s.Future.SetResult(NoneType.None, null);

                if (!isFullyVisible) {
                    if (!Blips.TryGetValue(Speakers.ByName[s.Speaker].BlipSoundName, out currentBlip))
                        currentBlip = null;
                }
            }

            var advanceSpeed = Game.InputControls.Accept.State ? 4 : 1;
            var prev = DisplayedCharacterCount;
            DisplayedCharacterCount = Math.Min(TotalCharacterCount, DisplayedCharacterCount + advanceSpeed);

            if (DisplayedCharacterCount > prev)
                NextBlipTime -= 1;

            if ((currentBlip != null) && (NextBlipTime <= 0)) {
                float blippyVolume = (currentBlip == Blips["Monologue"]) ? 0.4f : 0.75f;
                if (Game.InputControls.Accept.State)
                    blippyVolume = 0.33f;

                currentBlip.Play(blippyVolume, (float)BlippyRNG.NextDouble(-0.055f, 0.055f), 0f);
                NextBlipTime = BlippyInterval;
            }
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            renderer.FillRectangle(Bounds.Expand(4, 4), Color.Black);
            renderer.Layer += 1;
            renderer.OutlineRectangle(Bounds.Expand(5, 5), Color.White);
            renderer.Layer += 1;

            int charactersLeft = DisplayedCharacterCount;

            foreach (var s in Strings) {
                int charactersToDraw = Math.Min(charactersLeft, s.Layout.Count);
                renderer.DrawMultiple(s.Layout.Slice(0, charactersToDraw));
                charactersLeft -= charactersToDraw;
            }
        }

        public SignalFuture Sentence (string text, string speaker = "Monologue", SpriteFont font = null, bool lineBreak = false) {
            var textPosition = Bounds.TopLeft;
            float xOffset = 0;

            if (Strings.Count > 0) {
                var ls = Strings.Last();
                var lcb = ls.Layout.LastCharacterBounds.Translate(ls.Layout.Position);
                if (ls.LineBreakAfter) {
                    textPosition.Y = lcb.BottomRight.Y;
                } else {
                    xOffset = lcb.BottomRight.X - ls.Layout.Position.X;
                    textPosition.Y = lcb.TopLeft.Y;
                }
            }

            bool italic = (speaker == "Monologue");
            var actualFont = font ?? (italic ? ItalicDialogueFont : DialogueFont);
            var s = new TextString {
                Speaker = speaker,
                Layout = actualFont.LayoutString(
                    text, null,
                    position: textPosition,
                    xOffsetOfFirstLine: xOffset,
                    lineBreakAtX: Bounds.Size.X,
                    color: Speakers.ByName[speaker].Color
                ),
                LineBreakAfter = lineBreak || text.EndsWith("\n")
            };
            Strings.Add(s);
            return s.Future;
        }

        public void Clear () {
            Strings.Clear();
            DisplayedCharacterCount = TotalCharacterCount = 0;
            NextBlipTime = 0;
        }
    }
}
