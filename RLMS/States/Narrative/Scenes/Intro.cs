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
            yield return Textbox.Sentence("'Dad' was definitely not what you expected to see on your caller ID.\n");
            yield return Pause();
            yield return Textbox.Sentence("After your last conversation, it didn't seem likely you'd be talking to him again.\n");
            yield return Pause();
            yield return Textbox.Sentence("The silence on the other end of the line made you wonder if you were still right.\n");

            yield return new Branch(this) {
                {"\"Hey Dad.\"", HeyDad, "Callista"},
                {"\"What's up?\"", WhatsUp, "Callista"}
            };
        }

        public IEnumerator<object> HeyDad () {
            yield return Textbox.Sentence("He pauses for just slightly too long before responding: \n");
            yield return new Sleep(1.6);

            yield return Textbox.Sentence("\"Hi. I uh... ", speaker: "Dad");
            yield return new Sleep(0.33f);
            yield return Textbox.Sentence("figured you might still be looking for work?\"\n", speaker: "Dad");
            yield return Pause();

            yield return Textbox.Sentence("An easy guess, but still accurate.\n");
            yield return Pause();
            yield return Textbox.Sentence("\"Yeah, looking. Not that I'm too good at it.\"", speaker: "Callista");

            yield return Part2();
        }

        public IEnumerator<object> WhatsUp () {
            yield return Textbox.Sentence("\"Not... ", speaker: "Dad");
            yield return new Sleep(0.33f);
            yield return Textbox.Sentence("not much. Have you found any work yet?\"\n", speaker: "Dad");
            yield return Pause();

            yield return Textbox.Sentence("More diplomatic than you expected.\n");
            yield return Textbox.Sentence("\"Nothing that much suits me, no.\"", speaker: "Callista");

            yield return Part2();
        }

        public IEnumerator<object> Part2 () {
            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return new Sleep(0.66f);

            yield return Textbox.Sentence("\"So, uh... ", speaker: "Dad");
            yield return new Sleep(0.4f);
            yield return Textbox.Sentence("a friend called me today. ", speaker: "Dad");
            yield return Pause();
            yield return Textbox.Sentence("He's got a part-time job that needs filling \n", speaker: "Dad");
            yield return Textbox.Sentence(" pretty quickly... ", speaker: "Dad");
            yield return new Sleep(0.33f);
            yield return Textbox.Sentence("sounded like somebody quit in a hurry. ", speaker: "Dad");
            yield return Pause();
            yield return Textbox.Sentence("It's a bit out \n", speaker: "Dad");
            yield return Textbox.Sentence(" of the way, but the pay is good.\" \n", speaker: "Dad");
            yield return Pause();

            yield return Textbox.Sentence("Huh. ");
            yield return Pause();
            yield return Textbox.Sentence("What's the catch? ");
            yield return Pause();
            yield return Textbox.Sentence("\"That sounds alright. What's the work? ", speaker: "Callista");
            yield return Pause();
            yield return Textbox.Sentence("And where is it?\"\n", speaker: "Callista");

            yield return Part3();
        }

        public IEnumerator<object> Part3 () {
            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return Textbox.Sentence("\"He owns some kind of lumber mill on an island off the coast, upstate.\"\n", speaker: "Dad");
            yield return new Sleep(0.6f);

            yield return Textbox.Sentence("What. ");
            yield return new Sleep(1.3f);

            yield return Textbox.Sentence("\"I'd... ", speaker: "Callista");
            yield return new Sleep(0.2f);
            yield return Textbox.Sentence("make a pretty terrible lumberjack, Dad.\"\n", speaker: "Callista");
            yield return Pause();

            yield return Textbox.Sentence("His sigh sounds particularly weary. \n");
            yield return Pause();

            yield return Textbox.Sentence("\"I know, hear me out. ", speaker: "Dad");
            yield return Pause();
            yield return Textbox.Sentence("He needs someone to keep the place running.\n", speaker: "Dad");
            yield return Pause();
            yield return Textbox.Sentence("Set schedules, balance the books... ", speaker: "Dad");
            yield return Pause();
            yield return Textbox.Sentence("You were always good at this stuff.\"\n", speaker: "Dad");
            yield return Pause();

            yield return Textbox.Sentence("You feel the distinct sensation of being backed into a corner. \n");

            yield return Part4();
        }

        public IEnumerator<object> Part4 () {
            yield return ShowAdvancePrompt();

            Textbox.Clear();

            yield return Textbox.Sentence("\"You spent enough time with me at work to know how to do this...\"\n", speaker: "Dad");
            yield return Pause();

            yield return Textbox.Sentence("That's a corner, alright. ");
            yield return new Sleep(0.6f);

            yield return Textbox.Sentence("\"Yeah, I guess so.\"\n", speaker: "Callista");
            yield return Pause();

            yield return Textbox.Sentence("\"I know it's not what you're looking for, but... ", speaker: "Dad");
            yield return new Sleep(0.4f);

            yield return Textbox.Sentence("could you give it a try? If things don't \n", speaker: "Dad");
            yield return Textbox.Sentence(" work out, I'm sure he'll understand.\"\n", speaker: "Dad");
            yield return Pause();

            yield return new Sleep(1);
            yield return ShowAdvancePrompt();
        }
    }
}
