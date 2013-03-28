using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Render;
using Microsoft.Xna.Framework.Graphics;
using Squared.Render.Convenience;

namespace RLMS.Framework {
    public interface IThreadedComponent {
        void Shown ();
        void Hidden ();
        void Update ();
        void Draw (Frame frame, ref ImperativeRenderer renderer);
    }

    public interface IThreadedState {
        IEnumerator<object> Main ();

        void Update ();
        void Draw (Frame frame, ref ImperativeRenderer renderer);
    }
}
