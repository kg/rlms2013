using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Squared.Render;
using Squared.Render.Convenience;

namespace RLMS.States.Exploration {
    public class AreaView {
        private Texture2D CurrentBackground;

        public void SetBackground (Texture2D background) {
            CurrentBackground = background;
        }

        public void Update () {
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
            if (CurrentBackground != null)
                renderer.Draw(CurrentBackground, Vector2.Zero);

            renderer.Layer += 1;
        }
    }
}
