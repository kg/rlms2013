using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RLMS.Framework;
using Squared.Task;

namespace RLMS.States.Narrative.Scenes {
    public class Intro : Narrative.Scene {
        public override IEnumerator<object> Main () {
            yield return ShowSmartText(@"'Dad' was definitely not what you expected to see on your caller ID.
After your last conversation, it didn't seem likely you'd be talking to him again.
The silence on the other end of the line made you wonder if you were still right.");

            yield return new Branch(this) {
                {"'Hey Dad.'", HeyDad, "Callista"},
                {"'What's up?'", WhatsUp, "Callista"}
            };
        }

        public IEnumerator<object> HeyDad () {
            yield return ShowSmartText("He pauses for just slightly too long before responding: \n");
            yield return Pause(1f);

            yield return ShowSmartText("'Hi. I uh... figured you might still be looking for work?'\n", speaker: "Dad");
            yield return ShowSmartText("An easy guess, but still accurate.\n");
            yield return ShowSmartText("'Yeah, looking. Not that I'm too good at it.'", speaker: "Callista");

            yield return Part2();
        }

        public IEnumerator<object> WhatsUp () {
            yield return ShowSmartText("'Not... not much. Have you found any work yet?'\n", speaker: "Dad");
            yield return ShowSmartText("More diplomatic than you expected.\n");
            yield return ShowSmartText("'Nothing that much suits me, no.'", speaker: "Callista");

            yield return Part2();
        }

        public IEnumerator<object> Part2 () {
            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return Pause(0.66f);

            yield return ShowSmartText(
                "'So, uh... a friend called me today. He's got a part-time job that needs filling pretty quickly... " +
                "sounded like somebody quit in a hurry. It's a bit out of the way, but the pay is good.'\n", 
                speaker: "Dad"
            );

            yield return ShowSmartText("Huh. What's the catch? ");
            yield return ShowSmartText("'That sounds alright. What's the work? And where is it?'\n", speaker: "Callista");

            yield return Part3();
        }

        public IEnumerator<object> Part3 () {
            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return ShowSmartText("'He owns a lumber mill on an island off the coast, upstate.'\n", speaker: "Dad");
            yield return Pause(0.4f);

            yield return ShowSmartText("What. ");
            yield return Pause(0.6f);

            yield return ShowSmartText("'I'd... make a pretty terrible lumberjack, Dad.'\n", speaker: "Callista");

            yield return ShowSmartText("His sigh sounds particularly weary.\n");

            yield return ShowSmartText(
                "'I know, hear me out. He needs someone to keep the place running. " +
                "Set schedules, balance the books... You were always good at this stuff.'\n", 
                speaker: "Dad"
            );

            yield return ShowSmartText("You feel the distinct sensation of being backed into a corner.\n");

            yield return Part4();
        }

        public IEnumerator<object> Part4 () {
            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return ShowSmartText("'You spent enough time with me at work to know how to do this...'\n", speaker: "Dad");

            yield return ShowSmartText("That's a corner, alright. ");
            yield return Pause(0.3f);

            yield return ShowSmartText("'Yeah, I guess so.'\n", speaker: "Callista");

            yield return ShowSmartText("'I know it's not what you're looking for, but... could you give it a try? If things don't work out, I'm sure he'll understand.'\n", speaker: "Dad");

            yield return Pause(1.5f);
            yield return ShowAdvancePrompt();
        }
    }
}
