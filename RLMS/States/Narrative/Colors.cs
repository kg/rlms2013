using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RLMS.States.Narrative {
    public static class Colors {
        public static readonly Color Monologue = new Color(235, 240, 245);
        public static readonly Color Callista = new Color(206, 66, 101);
        public static readonly Color Dad = new Color(43, 137, 196);

        public static readonly Dictionary<string, Color> ByName = new Dictionary<string, Color> {
            {"Monologue", Monologue},
            {"Callista", Callista},
            {"Dad", Dad}
        };
    }
}
