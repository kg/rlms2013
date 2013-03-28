using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RLMS.Framework;
using Squared.Render;
using Squared.Render.Convenience;

namespace RLMS.States {
    public class Action : IThreadedState {
        public readonly Game Game;

        public Action (Game game) {
            Game = game;
        }

        public IEnumerator<object> Main () {
            yield break;
        }

        public void Update () {
        }

        public void Draw (Frame frame, ref ImperativeRenderer renderer) {
        }
    }
}
