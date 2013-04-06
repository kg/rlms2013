using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squared.Game;

namespace RLMS.States.Exploration {
    public class Hotspot {
        public readonly string Name;
        public readonly Bounds Bounds;
        public readonly Func<IEnumerator<object>> OnClick;

        public Hotspot (string name, Bounds bounds, Func<IEnumerator<object>> onClick = null) {
            Name = name;
            Bounds = bounds;
            OnClick = onClick;
        }

        public override string ToString () {
            return Name;
        }
    }
}
