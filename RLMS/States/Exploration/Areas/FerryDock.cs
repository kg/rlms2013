using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RLMS.States.Narrative.Scenes;
using Squared.Game;
using Squared.Task;

namespace RLMS.States.Exploration.Areas {
    public class FerryDock : Area {
        public Texture2D Background;

        public override IEnumerator<object> LoadContent () {
            yield return Game.ContentLoader.LoadContent<Texture2D>("backgrounds/FerryDock_placeholder").Bind(() => Background);
        }

        public override IEnumerator<object> OnEnter () {
            View.SetBackground(Background);

            yield return Game.PlayScene<FerryDockEntered>();

            AddHotspot(
                "Ferry",
                Bounds.FromPositionAndSize(
                    new Vector2(29, 0),
                    new Vector2(390, 412)
                ),
                BoardFerry
            );
        }

        public IEnumerator<object> BoardFerry () {
            TravelToArea<OnboardFerry>();

            yield break;
        }
    }
}
