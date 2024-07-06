using System;
using System.Collections.Generic;

namespace OrnithologistsGuild.Game.Critters
{
    public class BetterBirdieBehavior
    {
        public int Weight;
        public Func<BetterBirdieTrigger> Action;
        public bool Immediate;

        public BetterBirdieBehavior(int weight, Func<BetterBirdieTrigger> action, bool immediate)
        {
            Weight = weight;
            Action = action;
            Immediate = immediate;
        }
    }

    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        private Func<BetterBirdieTrigger> flipAction => () =>
        {
            Flip();
            return BetterBirdieTrigger.Stop;
        };

        public List<BetterBirdieBehavior> GetContextualBehavior()
        {
            if (IsRoosting)
            {
                var shouldSleepLonger = Environment.IsRainingHere() || IsInNest;

                return new List<BetterBirdieBehavior> {
                    new BetterBirdieBehavior(shouldSleepLonger ? 25 : 5, () => BetterBirdieTrigger.Sleep, true),
                    new BetterBirdieBehavior(1, () => BetterBirdieTrigger.Relocate, true)
                };
            }

            if (IsInBath)
            {
                return new List<BetterBirdieBehavior> {
                    new BetterBirdieBehavior(5, () => BetterBirdieTrigger.Bathe, true),
                    new BetterBirdieBehavior(1, flipAction, true),
                    new BetterBirdieBehavior(1, () => BetterBirdieTrigger.Relocate, true)
                };
            }

            if (IsInWater)
            {
                return new List<BetterBirdieBehavior> {
                    new BetterBirdieBehavior(1000, () => BetterBirdieTrigger.Swim, false),
                    new BetterBirdieBehavior(100, () => BetterBirdieTrigger.Bathe, false),
                    new BetterBirdieBehavior(50, flipAction, false),
                    new BetterBirdieBehavior(25, () => BetterBirdieTrigger.Relocate, false),
                    new BetterBirdieBehavior(5, () => BetterBirdieTrigger.FlyAway, false)
                };
            }

            if (IsPerched)
            {
                return new List<BetterBirdieBehavior> {
                    new BetterBirdieBehavior(200, () => BetterBirdieTrigger.Peck, false),
                    new BetterBirdieBehavior(50, flipAction, false),
                    new BetterBirdieBehavior(25, () => BetterBirdieTrigger.Relocate, false),
                    new BetterBirdieBehavior(5, () => BetterBirdieTrigger.FlyAway, false)
                };
            }

            return new List<BetterBirdieBehavior> {
                new BetterBirdieBehavior(200, () => BetterBirdieTrigger.Walk, false),
                new BetterBirdieBehavior(200, () => BetterBirdieTrigger.Hop, false),
                new BetterBirdieBehavior(100, () => BetterBirdieTrigger.Peck, false),
                new BetterBirdieBehavior(50, flipAction, false),
                new BetterBirdieBehavior(25, () => BetterBirdieTrigger.Relocate, false),
                new BetterBirdieBehavior(5, () => BetterBirdieTrigger.FlyAway, false),
                // Birds who cannot perch can sleep on the ground
                new BetterBirdieBehavior(BirdieDef.PerchPreference > 0 ? 0 : (Environment.IsRainingHere() ? 200 : 25), () => BetterBirdieTrigger.Sleep, false)
            };
        }
    }
}

