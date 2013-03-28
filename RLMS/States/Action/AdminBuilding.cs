using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Squared.Render.Convenience;

namespace RLMS.States.Action {
    public class AdminBuilding : Building {
        public AdminBuilding () 
            : base (
                Squared.Game.Bounds.FromPositionAndSize(
                    new Vector2(212, 346),
                    new Vector2(103, 162)
                )
            )
        {
        }

        public override Entity CreateRuntimeBuilding (ActionState state) {
            return new RuntimeAdminBuilding(state, this);
        }
    }

    public class RuntimeAdminBuilding : RuntimeBuilding<AdminBuilding> {
        public RuntimeAdminBuilding (ActionState state, AdminBuilding building)
            : base(state, building) {
        }

        public override void Update () {
        }

        public override void Draw (ref ImperativeRenderer renderer) {
            renderer.OutlineRectangle(Bounds, Color.White);
        }

        public override string ToString () {
            return "Administration Building";
        }
    }
}
