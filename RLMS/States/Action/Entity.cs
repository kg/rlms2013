﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Squared.Game;
using Squared.Render.Convenience;

namespace RLMS.States.Action {
    public abstract class Entity {
        public readonly ActionState State;

        public RuntimeBuilding OccupiedBuilding; 

        public Vector2 Position;
        public Bounds Hitbox;

        protected Entity (ActionState state) {
            State = state;
        }

        public virtual Bounds Bounds {
            get {
                return new Bounds(
                    Position + Hitbox.TopLeft,
                    Position + Hitbox.BottomRight
                );
            }
        }

        public void Occupy (RuntimeBuilding building) {
            if (building == OccupiedBuilding)
                return;

            if (OccupiedBuilding != null) {
                OccupiedBuilding.Occupants.Remove(this);
            }

            OccupiedBuilding = building;

            if (building != null) {
                building.Occupants.Add(this);
                Position = building.Position + 
                    new Vector2(building.Bounds.Size.X * 0.5f, 0) +
                    new Vector2(0, Hitbox.Size.Y * building.Occupants.Count);
            }
        }

        public abstract void Update ();
        public abstract void Draw (ref ImperativeRenderer renderer);
    }
}
