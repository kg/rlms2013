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
            Textbox.Clear();

            yield return ShowSmartText("A bored-looking young man walks over and strikes up a conversation.\n");
            yield return ShowSmartText("'I haven't seen you before. Sightseeing?'\n", speaker: "Steve");
            yield return ShowSmartText("As curious as he is bored, apparently. ");
            yield return ShowSmartText("'Work, actually.'\n", speaker: "Callista");
            yield return ShowSmartText("'Wow! That doesn't happen often. How long you planning to stay on the island?'\n", speaker: "Steve");
            yield return ShowSmartText("'A few months... probably.'", speaker: "Callista");

            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return ShowSmartText("'We'll be seeing a lot of each other, then. I'm your way back to the mainland. Name's Steve.'\n", speaker: "Steve");
            yield return ShowSmartText("'I'm 无. Nice to meet you.'", speaker: "Callista");

            yield return ShowAdvancePrompt();
        }
    }
}
