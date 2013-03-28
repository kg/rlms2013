using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLMS.States.Action {
    public class LumberMill {
        public readonly HashSet<Lumberjack> Roster = new HashSet<Lumberjack> {
            new Lumberjack("Bill"),
            new Lumberjack("Billy"),
            new Lumberjack("Billy Bob")
        };
    }
}
