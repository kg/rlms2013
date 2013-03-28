using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Squared.Game;

namespace RLMS.States.Action {
    public class LumberMill {
        public readonly List<Lumberjack> Roster = new List<Lumberjack> {
            new Lumberjack("Bill"),
            new Lumberjack("Billy"),
            new Lumberjack("Billy Bob")
        };

        public readonly List<Building> Buildings = new List<Building>();
        public readonly AdminBuilding AdminBuilding;

        public LumberMill () {
            Buildings.Add(AdminBuilding = new AdminBuilding());
        }
    }
}
