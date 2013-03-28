using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RLMS.States.Action {
    public abstract class Entity {
        public readonly ActionState State;

        protected Entity (ActionState state) {
            State = state;
        }

        public abstract void Update ();
    }
}
