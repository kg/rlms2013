using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Squared.Game;

namespace RLMS.States.Action {
    public abstract class Building {
        public Bounds Bounds;

        public Building (Bounds bounds) {
            Bounds = bounds;
        }

        public abstract Entity CreateRuntimeBuilding (ActionState state);
    }

    public abstract class RuntimeBuilding : Entity {
        public readonly HashSet<Entity> Occupants = new HashSet<Entity>();

        protected RuntimeBuilding (ActionState state)
            : base(state) {
        }

        public override void Update () {
            var pos = Position + new Vector2(Hitbox.Size.X * 0.5f, Hitbox.Size.Y * 0.1f);

            foreach (var occupant in Occupants) {
                occupant.Position = pos;
                pos += new Vector2(0, occupant.Hitbox.Size.Y);
            }
        }
    }

    public abstract class RuntimeBuilding<T> : RuntimeBuilding
        where T : Building
    {
        public readonly T Building;

        protected RuntimeBuilding (ActionState state, T building)
            : base(state) {

            Building = building;
            Position = building.Bounds.TopLeft;
            Hitbox = building.Bounds.Translate(-Position);
        }
    }
}
