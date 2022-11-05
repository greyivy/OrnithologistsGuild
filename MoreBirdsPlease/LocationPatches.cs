using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using xTile.Dimensions;

namespace MoreBirdsPlease
{
    public class LocationPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        private static Models.BirdieModel GetRandomBirdie()
        {
            var usualSuspects = DataManager.Birdies.ToList();

            // TODO optimize
            var weightedUsualSuspects = new List<Models.BirdieModel>();
            foreach (var birdie in usualSuspects)
            {
                weightedUsualSuspects.AddRange(Enumerable.Repeat(birdie, birdie.weightedRandom));
            }

            return weightedUsualSuspects[Game1.random.Next(0, weightedUsualSuspects.Count - 1)];
        }

        public static bool addBirdies_Prefix(StardewValley.GameLocation __instance, double chance, bool onlyIfOnScreen = false)
        {
            try
            {
                Monitor.Log($"addBirdies_Prefix {chance.ToString()}");

                // No birdies past 8:00 PM (it's their bedtime), in the desert or railroad // TODO ensure they don't spawn indoors!
                if (Game1.timeOfDay >= 1800 || !__instance.IsOutdoors || __instance is Desert || (__instance is Railroad)) return false;

                Models.BirdieModel flockSpecies = null;

                // Chance to add another flock
                int flocksAdded = 0;
                while (Game1.random.NextDouble() < Math.Min(chance, (0.1 / (flocksAdded + 1)))) // Max 10% chance
                {
                    // Determine flock parameters
                    flockSpecies = GetRandomBirdie();
                    int flockSize = Game1.random.Next(1, flockSpecies.maxFlockSize + 1);

                    // Try 50 times to find an empty patch within the location
                    for (int trial = 0; trial < 100; trial++)
                    {
                        // Get a random tile within the feeder range
                        var randomTile = __instance.getRandomTile();

                        if (!onlyIfOnScreen || !Utility.isOnScreen(randomTile * 64f, 64))
                        {
                            // Get a 5x5 patch around the random tile
                            // Note that 5x5 is more than feeder's 3x3
                            var randomRect = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 1, (int)randomTile.Y - 1, 3, 3);

                            if (!__instance.isAreaClear(randomRect)) continue;

                            Monitor.Log($"Found clear location at {randomRect}, adding flock of {flockSize} {flockSpecies.name} ({flockSpecies.id})");

                            // Spawn birdies
                            List<Critter> crittersToAdd = new List<Critter>();
                            for (int index = 0; index < flockSize; ++index)
                            {
                                crittersToAdd.Add((Critter)new BetterBirdie(flockSpecies, -100, -100));
                            }

                            ModEntry.instance.Helper.Reflection.GetMethod(__instance, "addCrittersStartingAtTile").Invoke(randomTile, crittersToAdd);

                            flocksAdded++;

                            break;
                        }
                    }
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addBirdies_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}

