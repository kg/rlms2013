using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RLMS.Framework;

namespace RLMS.States.Exploration {
    public abstract class Area {
        // BLEH. Don't want to require every class to define a constructor that just calls base()...
        public void Initialize (ExplorationState state) {
            State = state;
        }

        public ExplorationState State {
            get;
            private set;
        }

        public Game Game {
            get {
                return State.Game;
            }
        }

        public InputControls Controls {
            get {
                return State.Game.InputControls;
            }
        }

        public virtual IEnumerator<object> OnEnter () {
            yield break;
        }

        public virtual IEnumerator<object> OnExit () {
            yield break;
        }

        public static Type[] GetAllAreaTypes () {
            var tArea = typeof(Area);
            return (
                from t in Assembly.GetExecutingAssembly().GetTypes()
                where !t.IsAbstract && tArea.IsAssignableFrom(t)
                select t
            ).ToArray();
        }
    }
}
