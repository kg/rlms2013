using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RLMS.States.Narrative {
    public class Speaker {
        public string Name;
        public Color Color;
        public string BlipSoundName;
    }

    public class SpeakerCollection : KeyedCollection<string, Speaker> {
        protected override string GetKeyForItem(Speaker item) {
            return item.Name;
        }
    }

    public static class Speakers {
        public static readonly SpeakerCollection ByName = new SpeakerCollection {
            new Speaker {
                Name = "Monologue",
                BlipSoundName = "Monologue",
                Color = new Color(235, 240, 245)
            },
            new Speaker {
                Name = "Callista",
                BlipSoundName = "Callista",
                Color = new Color(206, 66, 101)
            },
            new Speaker {
                Name = "Dad",
                BlipSoundName = "Dad",
                Color = new Color(43, 137, 196)
            },
            new Speaker {
                Name = "Steve",
                BlipSoundName = "Steve",
                Color = new Color(63, 220, 80)
            }
        };
    }
}
