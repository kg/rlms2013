using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RLMS.Framework;
using Squared.Task;

namespace RLMS.States.Narrative.Scenes {
    public class FerryPilot : Scene {
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
            yield return HaveAnyQuestions(true);
        }

        public IEnumerator<object> HaveAnyQuestions (bool firstTime = false) {
            if (!firstTime) {
                yield return Pause(0.8f);
                yield return ShowSmartText("'Any other questions?'", speaker: "Steve");
            }

            yield return new Branch(this) {
                {"'Do you live on the island?'", DoYouLiveOnTheIsland, "Callista"},
                {"'How long does the ferry trip take?'", HowLongDoesTheFerryTake, "Callista"},
                {"'What's the weather like on the island?'", HowsTheWeather, "Callista"},
                {"'Any sights worth seeing?'", Sightseeing, "Callista"},
                {"'Nope, thanks!'", null, "Callista"}
            };
        }

        public IEnumerator<object> DoYouLiveOnTheIsland () {
            yield return ShowSmartText(
                "'Nope. I'd have to take another trip back after the last ferry. My family owns a house a few miles up the coast. Makes it pretty easy to get to work.'\n",
                speaker: "Steve"
            );

            yield return HaveAnyQuestions();
        }

        public IEnumerator<object> HowLongDoesTheFerryTake () {
            yield return ShowSmartText(
                "'Getting to the island takes about forty minutes. Getting back to the mainland takes about twenty.'\n",
                speaker: "Steve"
            );

            yield return new Branch(this) {
                {"'Why does it take so much longer to get there?'", WhySoMuchLonger, "Callista"},
                {"'Thanks.'", () => HaveAnyQuestions(), "Callista"},
            };
        }

        public IEnumerator<object> WhySoMuchLonger () {
            yield return ShowSmartText(
                "'The tides tend to be pushing against us on the way there. ",
                speaker: "Steve"
            );

            yield return Pause(0.8f);

            yield return ShowSmartText(
                "Something to do with the moon, maybe?'\n",
                speaker: "Steve"
            );

            yield return HaveAnyQuestions();
        }

        public IEnumerator<object> HowsTheWeather () {
            yield return ShowSmartText(
                "'Warmer than you'd expect! This time of year... It's probably around the sixties? During the summer it's pretty warm, but never hot enough to need air conditioning.'\n",
                speaker: "Steve"
            );

            yield return HaveAnyQuestions();
        }

        public IEnumerator<object> Sightseeing () {
            yield return ShowSmartText(
                "'From the ferry, you might be able to see some wildlife along the shore. Once you're there...'\n",
                speaker: "Steve"
            );

            yield return Pause(0.6f);

            yield return ShowSmartText(
                "He seems deep in thought. He must not get this question often.\n"
            );

            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return ShowSmartText(
                "'There isn't much to see, honestly. There's a chapel that has a certain beauty to it, and the view from the lighthouse is great at sunset.'\n",
                speaker: "Steve"
            );

            yield return Pause(0.4f);

            yield return ShowSmartText(
                "'I can't think of anything else.'\n",
                speaker: "Steve"
            );

            yield return HaveAnyQuestions();
        }
    }

    public class FerryDockEntered : Scene {
        public override IEnumerator<object> Main() {
            yield return ShowSmartText(
                "The ferry only runs twice a day, so of course you have to be here at six in the morning. No wonder the place is deserted."
            );

            yield return ShowAdvancePrompt();
        }
    }

    public class OnboardFerryEntered : Scene {
        public override IEnumerator<object> Main () {
            yield return ShowSmartText(
                "If the dust piled up on the seats is any indication, this ferry doesn't see many passengers..."
            );

            yield return ShowAdvancePrompt();

            yield return Pause(1.5f);

            State.ChangeScene<FerryPilot>();
        }
    }
}
