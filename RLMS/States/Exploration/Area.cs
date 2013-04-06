using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using RLMS.Framework;
using Squared.Game;
using Squared.Task;

namespace RLMS.States.Exploration {
    public abstract class Area {
        public readonly List<Hotspot> Hotspots = new List<Hotspot>();

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

        public AreaView View {
            get {
                return State.View;
            }
        }

        public virtual IEnumerator<object> LoadContent () {
            yield break;
        }

        public virtual IEnumerator<object> OnEnter () {
            yield break;
        }

        public virtual IEnumerator<object> OnExit () {
            yield break;
        }

        public Hotspot AddHotspot (string name, Bounds bounds, Func<IEnumerator<object>> onClick = null) {
            var hotspot = new Hotspot(name, bounds, onClick);
            Hotspots.Add(hotspot);

            return hotspot;
        }

        public void TravelToArea<T> () 
            where T : Area {

            TravelToArea(typeof(T));
        }

        private void TravelToArea (Type areaType) {
            TravelToArea((Area)Activator.CreateInstance(areaType));
        }

        public void TravelToArea (Area area) {
            State.ExitArea(area);
        }

        public static Type[] GetAllAreaTypes () {
            var tArea = typeof(Area);
            return (
                from t in Assembly.GetExecutingAssembly().GetTypes()
                where !t.IsAbstract && 
                    tArea.IsAssignableFrom(t) && 
                    t.GetConstructors().Any((c) => c.GetParameters().Length == 0)
                select t
            ).ToArray();
        }
    }
}
