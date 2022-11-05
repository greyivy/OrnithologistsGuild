using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace OrnithologistsGuild
{
    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        public readonly int[] FallbackBirdTypes = new int[] { StardewValley.BellsAndWhistles.Birdie.blueBird, StardewValley.BellsAndWhistles.Birdie.brownBird };

        public Models.BirdieModel Birdie;
        public Models.FeederModel Perch;

        public BetterBirdie(Models.BirdieModel birdie, int tileX, int tileY, Models.FeederModel perch = null) : base(0, new Vector2(tileX * 64, tileY * 64))
        {
            Birdie = birdie;

            if (DataManager.BirdieAssetIds.ContainsKey(birdie.id))
            {
                baseFrame = 0;
                sprite = new AnimatedSprite(DataManager.BirdieAssetIds[birdie.id], baseFrame, 32, 32);
            } else
            {
                // Fallback to random vanilla bird
                baseFrame = FallbackBirdTypes[Game1.random.Next(0, FallbackBirdTypes.Length)];
                sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
            }

            flip = Game1.random.NextDouble() < 0.5;
            position.X += 32f;
            position.Y += 32f;
            startingPosition = position;
            flightOffset = (float)Game1.random.NextDouble() - 0.5f;
            State = BetterBirdieState.Pecking;

            this.Perch = perch;
            if (perch != null)
            {
                this.position.Y += perch.perchOffset;
                this.startingPosition.Y = this.position.Y;
            }
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            if (State == BetterBirdieState.FlyingAway)
            {
                base.draw(b);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (State != BetterBirdieState.FlyingAway)
            {
                base.draw(b);
            }
        }
    }
}

