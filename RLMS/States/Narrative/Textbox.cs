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
        public const float BlippyInterval = 4;
        public const float WordWrapRightMargin = 96;
        public const float WordWrapIndent = 20;

        public class TextString {
            public string Speaker;
            public SpriteFont Font;
            public StringLayout Layout;
            public SignalFuture Future;
            public int Length;
            public bool IsFullyVisible;
            public bool LineBreakAfter;
            public bool IsMu;
        }

        public readonly NarrativeState State;
        public readonly List<TextString> Strings = new List<TextString>();
        public readonly Bounds Bounds;

        public int TotalCharacterCount = 0;
        public int DisplayedCharacterCount = 0;
        public float NextBlipTime = 0;

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

                if (s.IsMu) {
                    currentBlip = null;

                    if (!s.IsFullyVisible && isFullyVisible)
                        Blips["Mu"].Play(0.8f, (float)BlippyRNG.NextDouble(-0.1f, 0.1f), 0f);
                }

                s.IsFullyVisible = isFullyVisible;
            }

            var advanceSpeed = Game.InputControls.Accept.State ? 4 : 1;
            var prev = DisplayedCharacterCount;
            DisplayedCharacterCount = Math.Min(TotalCharacterCount, DisplayedCharacterCount + advanceSpeed);

            if ((DisplayedCharacterCount > prev) && (NextBlipTime > 0))
                NextBlipTime -= 1;

            if ((currentBlip != null) && (NextBlipTime <= 0)) {
                float blippyVolume = (currentBlip == Blips["Monologue"]) ? 0.4f : 0.75f;
                blippyVolume = (float)BlippyRNG.NextDouble(blippyVolume - 0.1f, blippyVolume);
                if (Game.InputControls.Accept.State)
                    blippyVolume *= 0.4f;

                currentBlip.Play(blippyVolume, (float)BlippyRNG.NextDouble(-0.07f, 0.070f), 0f);
                NextBlipTime += (float)BlippyRNG.NextDouble(BlippyInterval - 1, BlippyInterval + 2);
            }
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            renderer.FillRectangle(Bounds.Expand(4, 4), Color.Black * 0.9f);
            renderer.Layer += 1;
            renderer.OutlineRectangle(Bounds.Expand(5, 5), Color.White);
            renderer.Layer += 1;

            int charactersLeft = DisplayedCharacterCount;

            foreach (var s in Strings) {
                int charactersToDraw = Math.Min(charactersLeft, s.Layout.Count);
                if (s.IsMu) {
                    var offset = new Vector2((float)BlippyRNG.NextDouble(-1, 1), (float)BlippyRNG.NextDouble(-1, 1));
                    var scale = (float)BlippyRNG.NextDouble(0.95f, 1.05f);
                    var rotation = (float)BlippyRNG.NextDouble(-0.04f, 0.04f);

                    var dc = s.Layout.DrawCalls.Array[s.Layout.DrawCalls.Offset];
                    dc.Position += offset;
                    dc.ScaleF = scale;
                    dc.Rotation = rotation;
                    dc.MultiplyColor *= (float)BlippyRNG.NextDouble(0.7f, 1.1f);

                    renderer.Draw(ref dc);
                } else {
                    renderer.DrawMultiple(s.Layout.Slice(0, charactersToDraw));
                }
                charactersLeft -= s.Length;
            }

            renderer.Layer += 1;
        }

        public static string[] SplitWords (string text) {
            var result = new List<string>();
            char[] WrapCharacters = new [] { '.', ',', ' ', ':', ';', '-', '?', '\n', '\u2026', '\u65E0' };
            // Ensure ellipses use the unicode character, because it makes things easier.
            text = text.Replace("...", "\u2026");

            int pos = 0, nextPos = 0;

            while (pos < text.Length) {
                nextPos = text.IndexOfAny(WrapCharacters, pos);

                if (nextPos < pos) {
                    result.Add(text.Substring(pos));
                    break;
                }

                nextPos += 1;
                var trailingChars = nextPos - pos;

                // Tack single/double quotes onto the end of preceding words even if they have punctuation before them.
                // Otherwise we get annoying single blips for quotes after punctuation. Yuck.
                if (nextPos < text.Length) {
                    if ((text[nextPos] == '\'') || (text[nextPos] == '\"')) {
                        trailingChars += 1;
                        nextPos += 1;
                    }
                }

                result.Add(text.Substring(pos, trailingChars));
                pos = nextPos;
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
                    Length = word.Trim().Length,
                    IsMu = word.Contains("\u65E0")
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
