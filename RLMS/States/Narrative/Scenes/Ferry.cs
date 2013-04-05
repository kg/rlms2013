using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RLMS.Framework;
using Squared.Task;

namespace RLMS.States.Narrative.Scenes {
    public class FerryPilot : Narrative.Scene {
        public override IEnumerator<object> Main () {
            yield return Textbox.Sentence("\"I haven't seen you before. Sightseeing?\"", speaker: "Steve");

            yield return ShowAdvancePrompt();
        }
    }
}
