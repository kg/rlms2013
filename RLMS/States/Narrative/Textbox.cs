using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public const float WordWrapRightMargin = 96;
        public const float WordWrapIndent = 20;

        public class TextString {
            public string Speaker;
            public SpriteFont Font;
            public StringLayout Layout;
            public SignalFuture Future;
            public int Length;
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
                TotalCharacterCount += s.Length;

                int charactersToDraw = Math.Min(charactersLeft, s.Length);
                var isFullyVisible = (charactersToDraw == s.Length);
                var isVisible = (charactersToDraw > 0);
                charactersLeft -= charactersToDraw;

                if (isFullyVisible && (s.Future != null) && !s.Future.Completed)
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
                charactersLeft -= s.Length;
            }
        }

        public static string[] SplitWords (string text) {
            var result = new List<string>();
            char[] WrapCharacters = new [] { '.', ',', ' ', ':', ';', '-', '?', '\n', '\u2026' };
            text = text.Replace("...", "\u2026");

            int pos = 0, nextPos = 0;

            while (pos < text.Length) {
                nextPos = text.IndexOfAny(WrapCharacters, pos);

                if (nextPos < pos) {
                    result.Add(text.Substring(pos));
                    break;
                }

                result.Add(text.Substring(pos, nextPos - pos + 1));
                pos = nextPos + 1;
            }

            return result.ToArray();
        }

        public SignalFuture AddText (string text, string speaker = "Monologue", SpriteFont font = null, bool lineBreak = false) {
            var textPosition = Bounds.TopLeft + new Vector2(8, 0);
            float xOffset = 0;

            if (Strings.Count > 0) {
                var ls = Strings.Last();
                var lcb = ls.Layout.LastCharacterBounds.Translate(ls.Layout.Position);
                if (ls.LineBreakAfter) {
                    textPosition.Y = lcb.TopLeft.Y + ls.Font.LineSpacing;
                } else {
                    xOffset = lcb.BottomRight.X - ls.Layout.Position.X;
                    textPosition.Y = lcb.TopLeft.Y;
                }
            }

            bool italic = (speaker == "Monologue");
            var actualFont = font ?? (italic ? ItalicDialogueFont : DialogueFont);
            var color = Speakers.ByName[speaker].Color;

            var words = SplitWords(text);

            TextString s = null;

            float rightMargin = Bounds.Size.X - WordWrapRightMargin;

            foreach (var word in words) {
                var size = actualFont.MeasureString(word);

                // word wrap
                if ((xOffset + size.X) >= rightMargin) {
                    xOffset = WordWrapIndent;
                    textPosition.Y += actualFont.LineSpacing;
                }

                s = new TextString {
                    Speaker = speaker,
                    Font = actualFont,
                    Layout = actualFont.LayoutString(word, null, position: textPosition, xOffsetOfFirstLine: xOffset, color: color),
                    Length = word.Trim().Length
                };
                Strings.Add(s);

                xOffset = s.Layout.LastCharacterBounds.BottomRight.X;

                if (word.EndsWith("\n")) {
                    xOffset = 0;
                    textPosition.Y += actualFont.LineSpacing;
                }
            }

            if (s == null)
                return null;

            s.LineBreakAfter = (lineBreak || text.EndsWith("\n"));
            return s.Future = new SignalFuture();
        }

        public void Clear () {
            Strings.Clear();
            DisplayedCharacterCount = TotalCharacterCount = 0;
            NextBlipTime = 0;
        }
    }
}
