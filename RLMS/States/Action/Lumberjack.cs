using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Squared.Game;

namespace RLMS.States.Action {
    public class Lumberjack {
        public readonly string Name;

        public Lumberjack (string name) {
            Name = name;
        }

        public RuntimeLumberjack CreateRuntimeEntity (ActionState state) {
            return new RuntimeLumberjack(state, this);
        }
    }

    public class RuntimeLumberjack : Entity {
        public readonly Lumberjack Prototype;

        public RuntimeLumberjack (ActionState state, Lumberjack prototype)
            : base (state) {
            Prototype = prototype;

            var size = new Vector2(8, 8);
            Hitbox = new Bounds(-size, size);

            Occupy(state.Building<RuntimeAdminBuilding>());
        }

        public override void Update () {
        }

        public override void Draw (ref Squared.Render.Convenience.ImperativeRenderer renderer) {
            renderer.OutlineRectangle(Bounds, Color.White);
        }

        public override string ToString () {
            return Prototype.Name;
        }
    }
}
