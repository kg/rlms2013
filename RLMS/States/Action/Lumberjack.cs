using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLMS.States.Action {
    public class Lumberjack {
        public readonly string Name;

        public Lumberjack (string name) {
            Name = name;
        }
    }

    public class RuntimeLumberjack : Entity {
        public readonly Lumberjack Prototype;

        public RuntimeLumberjack (ActionState state, Lumberjack prototype)
            : base (state) {
            Prototype = prototype;
        }

        public override void Update () {
        }
    }
}
