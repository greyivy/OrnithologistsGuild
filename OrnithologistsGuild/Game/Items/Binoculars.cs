using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Microsoft.Xna.Framework;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;
using System.Linq;
using System;

namespace OrnithologistsGuild.Game.Items
{
    public class Binoculars : CustomObject
    {
        public int Range;

        private static readonly int AnimateDuration = 750;
        private int? AnimateElapsed;

        public Binoculars(CommonPackData data, int range) : base((ObjectPackData)data)
        {
            this.name = $"{data.ID}_Subclass";

            Range = range;
        }

        public override bool performUseAction(GameLocation location)
        {
            if (location == null || !location.IsOutdoors) return false;

            if (AnimateElapsed.HasValue) return false;
            AnimateElapsed = 0;

            return false;
        }

        private void UseBinoculars(GameLocation location, Farmer f)
        {
            List<string> alreadyIdentified = new List<string>();
            List<string> newlyIdentified = new List<string>();

            var actualRange = (Range + 0.5) * Game1.tileSize;
            var midPoint = f.position + new Vector2(0.5f * Game1.tileSize, -0.25f * Game1.tileSize);

            foreach (var critter in location.critters)
            {
                if (critter is BetterBirdie && Vector2.Distance(midPoint, critter.position) <= actualRange)
                {
                    var birdie = (BetterBirdie)critter;

                    if (birdie.IsFlying) continue;

                    if (DataManager.LifeListContains(birdie.Birdie))
                    {
                        alreadyIdentified.Add(birdie.Birdie.name);
                    }
                    else
                    {
                        DataManager.AddToLifeList(birdie.Birdie);

                        newlyIdentified.Add(birdie.Birdie.name);
                    }
                } else if (critter is Woodpecker && Vector2.Distance(midPoint, critter.position) <= actualRange)
                {
                    
                } else if (critter is Seagull && Vector2.Distance(midPoint, critter.position) <= actualRange)
                {

                } else if (critter is Crow && Vector2.Distance(midPoint, critter.position) <= actualRange)
                {

                } else if (critter is Owl)
                {
                    // TODO
                }

                // ... other critter types? Bird? PerchingBird?
            }

            if (alreadyIdentified.Any() || newlyIdentified.Any())
            {
                List<string> lines = new List<string>();
                if (newlyIdentified.Any()) lines.Add($"Newly identified: {string.Join(", ", newlyIdentified)}");
                if (alreadyIdentified.Any()) lines.Add($"Already identified: {string.Join(", ", alreadyIdentified)}");

                Game1.drawObjectDialogue(string.Join("^", lines));
            }
        }

        public override bool canStackWith(ISalable other)
        {
            return false;
        }

        public override void actionWhenBeingHeld(Farmer who)
        {
            who.setRunning(false);
            who.canOnlyWalk = true;

            base.actionWhenBeingHeld(who);
        }

        public override void actionWhenStopBeingHeld(Farmer who)
        {
            who.canOnlyWalk = false;
            if (Game1.options.autoRun)
            {
                who.setRunning(true);
            }

            base.actionWhenStopBeingHeld(who);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (AnimateElapsed.HasValue) {
                AnimateElapsed += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                if (AnimateElapsed <= AnimateDuration)
                {
                    var factor = Utilities.EaseOutSine(((float)AnimateElapsed.Value / (float)AnimateDuration));

                    var animatedRange = Utility.Lerp(0, Range * Game1.tileSize, factor);
                    var opacity = Utility.Lerp(0.7f, 0.1f, factor);

                    MonoGame.Primitives2D.DrawCircle(
                        spriteBatch,
                        objectPosition + new Vector2(0.5f * Game1.tileSize, 1.5f * Game1.tileSize),
                        animatedRange,
                        (int)animatedRange / 4,
                        Color.AliceBlue * opacity,
                        (Game1.tileSize / 16) * 2);
                }
                else
                {
                    // Animation complete
                    AnimateElapsed = null;

                    // TODO best place to put this? Is rendering done on a separate thread?
                    UseBinoculars(f.currentLocation, f);
                }
            }

            base.drawWhenHeld(spriteBatch, objectPosition, f);
        }
    }
}
