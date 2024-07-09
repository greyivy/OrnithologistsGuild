using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using OrnithologistsGuild.Game.Critters;
using OrnithologistsGuild.Models;
using StardewValley;

namespace OrnithologistsGuild
{
    public class BetterBirdieSpawner
    {
        public static void AddBirdies(GameLocation location, double chance = 0, bool onScreen = false)
        {
            // If AddBirdies is called before warp is complete, ContentPactcher conditions will not be up to date with the new location
            if (Game1.isWarping)
            {
                ModEntry.Instance.Monitor.Log("Deferring AddBirdies until after warp...");

                System.EventHandler<StardewModdingAPI.Events.WarpedEventArgs> handlerPlayerWarped = null;
                handlerPlayerWarped = delegate
                {
                    ModEntry.Instance.Monitor.Log("...warped, calling AddBirdies");
                    AddBirdies(location, chance, onScreen);

                    ModEntry.Instance.Helper.Events.Player.Warped -= handlerPlayerWarped;
                };

                ModEntry.Instance.Helper.Events.Player.Warped += handlerPlayerWarped;

                return;
            }

            if (!location.IsOutdoors) return;

            ModEntry.Instance.Monitor.Log($"AddBirdies onScreen={onScreen} chance={chance}");

            // First, get locations of all bird feeders
            foreach (var overlaidDict in location.Objects)
            {
                foreach (var obj in overlaidDict.Values.Where(obj => obj.IsFeeder()))
                {
                    // Only attract birds if there is food
                    if (obj.MinutesUntilReady > 0)
                    {
                        var foodDef = FoodDef.FromFeeder(obj);
                        if (foodDef != null)
                        {
                            var feederFlockChance = System.Math.Min(0.75, (chance * 1.5));

                            AddBirdiesNearFeeder(location, obj, foodDef, feederFlockChance, onScreen);
                        }
                    }
                }
            }

            if (chance > 0) AddRandomBirdies(location, chance, onScreen);
        }

        private static void AddRandomBirdies(GameLocation gameLocation, double chance, bool onScreen)
        {
            ModEntry.Instance.Monitor.Log($"AddRandomBirdies onScreen={onScreen} chance={chance}");

            // Chance to add another flock
            int flocksAdded = 0;
            int debug_AlwaysSpawn_Trial = 0;

            while ((ModEntry.debug_AlwaysSpawn != null && debug_AlwaysSpawn_Trial < 100 && flocksAdded == 0) || Game1.random.NextDouble() < chance / (flocksAdded + 1)) // Chance lowers after every flock
            {
                // Determine flock parameters
                BirdieDef flockBirdieDef = ModEntry.debug_AlwaysSpawn == null ? GetRandomBirdieDef(gameLocation) : ModEntry.debug_AlwaysSpawn;
                if (flockBirdieDef == null) return;

                var spawnLocations = BetterBirdie.GetRandomPositionsOrPerchesFor(gameLocation, flockBirdieDef, mustBeOffscreen: !onScreen);
                SpawnBirdies(gameLocation, flockBirdieDef, spawnLocations);
                if (spawnLocations.Any()) flocksAdded++;

                debug_AlwaysSpawn_Trial++;
            }
        }

        private static void AddBirdiesNearFeeder(GameLocation location, Object feeder, FoodDef food, double chance, bool onScreen)
        {
            ModEntry.Instance.Monitor.Log($"AddBirdiesNearFeeder onScreen={onScreen} chance={chance}");

            var feederProperties = feeder.GetFeederProperties();

            // Build a rectangle around the feeder based on the range
            var feederRect = Utility.getRectangleCenteredAt(feeder.TileLocation, (feederProperties.Range * 2) + 1);

            // Chance to add another flock
            int flocksAdded = 0;
            while (flocksAdded < feederProperties.MaxFlocks && Game1.random.NextDouble() < chance / (flocksAdded + 1)) // Chance lowers after every flock
            {
                // Determine flock parameters
                var flockBirdieDef = GetRandomFeederBirdieDef(feederProperties, food);
                if (flockBirdieDef == null) return;

                var shouldAddBirdToFeeder = flocksAdded == 0 && Game1.random.NextDouble() < 0.65;
                // Ensure feeder is/isn't onscreen
                if (Utility.isOnScreen(feeder.TileLocation * Game1.tileSize, Game1.tileSize) != onScreen) shouldAddBirdToFeeder = false;

                var spawnLocations = BetterBirdie.GetRandomPositionsOrPerchesFor(location, flockBirdieDef, mustBeOffscreen: true, tileAreaBound: feederRect, spawnType: SpawnType.Land);
                SpawnBirdies(location, flockBirdieDef, shouldAddBirdToFeeder ? spawnLocations.Skip(1) : spawnLocations);
                if (spawnLocations.Any()) flocksAdded++;

                var perch = new Perch(feeder);
                if (shouldAddBirdToFeeder && perch.GetOccupant(location) == null)
                {
                    location.addCritter(new BetterBirdie(flockBirdieDef, Vector2.Zero, perch));
                }
            }
        }

        private static void SpawnBirdies(GameLocation location, BirdieDef birdieDef, IEnumerable<(BirdiePosition BirdiePosition, bool IsFledgling)> spawns)
        {
            foreach (var spawn in spawns)
            {
                if (spawn.BirdiePosition.Perch != null)
                {
                    // Add perched bird
                    location.addCritter(new BetterBirdie(birdieDef, Vector2.Zero, spawn.BirdiePosition.Perch, isFledgling: spawn.IsFledgling));
                }
                else
                {
                    var tile = Utilities.XY(spawn.BirdiePosition.Position) / Game1.tileSize;
                    location.addCritter(new BetterBirdie(birdieDef, tile, isFledgling: spawn.IsFledgling));
                }
            }
        }

        private static BirdieDef GetRandomBirdieDef(GameLocation gameLocation)
        {
            return Utilities.WeightedRandom(ContentPackManager.BirdieDefs.Values, birdieDef => birdieDef.GetContextualWeight(updateContext: true, gameLocation, debug: ModEntry.debug_Conditions));
        }

        private static BirdieDef GetRandomFeederBirdieDef(FeederProperties feederProperties, FoodDef foodDef)
        {
            var usualSuspects = ContentPackManager.BirdieDefs.Values.Where(birdieDef => birdieDef.CanPerchAt(feederProperties) && birdieDef.CanEat(foodDef));

            return Utilities.WeightedRandom(usualSuspects, birdieDef => birdieDef.GetContextualWeight(updateContext: true, null, feederProperties, foodDef, debug: ModEntry.debug_Conditions));
        }
    }
}

