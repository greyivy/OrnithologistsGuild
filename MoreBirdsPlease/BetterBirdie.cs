using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace MoreBirdsPlease
{
    public class BetterBirdie : StardewValley.BellsAndWhistles.Birdie
    {
        public readonly int[] FallbackBirdTypes = new int[] { StardewValley.BellsAndWhistles.Birdie.blueBird, StardewValley.BellsAndWhistles.Birdie.brownBird };

        public Models.BirdieModel Birdie;
        public Models.FeederModel Perch;

        private static Dictionary<string, string> LoadedAssets = new Dictionary<string, string>();

        public BetterBirdie(Models.BirdieModel birdie, int tileX, int tileY, Models.FeederModel perch = null) : base(tileX, tileY, 0)
        {
            Birdie = birdie;

            if (LoadedAssets.ContainsKey(birdie.id))
            {
                baseFrame = 0;
                sprite = new AnimatedSprite(LoadedAssets[birdie.id], baseFrame, 32, 32);
            } else
            {
                // Fallback to random vanilla bird
                baseFrame = FallbackBirdTypes[Game1.random.Next(0, FallbackBirdTypes.Length)];
                sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
            }

            this.Perch = perch;
            if (perch != null)
            {
                this.position.Y += perch.perchOffset;
                this.startingPosition.Y = this.position.Y;
            }
        }

        public static void LoadAssets(Mod mod)
        {
            foreach (var birdie in DataManager.Birdies)
            {
                var asset = $"assets/birdies/{birdie.id.ToString()}.png";

                try
                {
                    mod.Helper.ModContent.Load<Texture2D>(asset);
                    LoadedAssets.Add(birdie.id, mod.Helper.ModContent.GetInternalAssetName(asset).BaseName);

                    mod.Monitor.Log($"Loaded birdie asset {asset}", LogLevel.Debug);
                } catch
                {
                    mod.Monitor.Log($"Error loading birdie asset {asset}", LogLevel.Warn);
                }
            }
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            // TODO specific bird behavior/speed/etc. per Birdie?
            var result = base.update(time, environment);

            if (Perch != null && this.yOffset == 0)
            {
                // Bird likely not flying away -- keep stationary
                this.position = this.startingPosition;
            }

            return result;
        }
    }
}

