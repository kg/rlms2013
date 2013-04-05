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
            yield return ShowSmartText("'I'm 无. Nice to meet you.'\n", speaker: "Callista");

            yield return ShowSmartText("'We have to wait a bit in case there are any other passengers. Have any questions?'", speaker: "Steve");
            yield return HaveAnyQuestions(false);
        }

        public IEnumerator<object> HaveAnyQuestions (bool clear = true) {
            if (clear) {
                Textbox.Clear();
                yield return ShowSmartText("'Any other questions?'", speaker: "Steve");
            }

            yield return new Branch(this) {
                {"'Do you live on the island?'", DoYouLiveOnTheIsland, "Callista"},
                {"'How long does the ferry trip take?'", HowLongDoesTheFerryTake, "Callista"},
                {"'What's the weather like on the island?'", HowsTheWeather, "Callista"},
                {"'Any sights worth seeing?'", Sightseeing, "Callista"},
            };
        }

        public IEnumerator<object> DoYouLiveOnTheIsland () {
            yield return HaveAnyQuestions();
        }

        public IEnumerator<object> HowLongDoesTheFerryTake () {
            yield return HaveAnyQuestions();
        }

        public IEnumerator<object> HowsTheWeather () {
            yield return HaveAnyQuestions();
        }

        public IEnumerator<object> Sightseeing () {
            yield return HaveAnyQuestions();
        }
    }
}
