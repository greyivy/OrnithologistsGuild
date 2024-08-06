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
                Dictionary<BirdieDef, float> flockBirdieDefs = ModEntry.debug_AlwaysSpawn == null ? GetRandomBirdieDefWithVariants(gameLocation) : new Dictionary<BirdieDef, float>() { { ModEntry.debug_AlwaysSpawn, 1f } };
                if (!flockBirdieDefs.Any()) return;

                var spawnLocations = BetterBirdie.GetRandomPositionsOrPerchesFor(gameLocation, flockBirdieDefs, mustBeOffscreen: !onScreen);
                SpawnBirdies(gameLocation, spawnLocations);
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
                var flockBirdieDefs = GetRandomFeederBirdieDefWithVariants(feederProperties, food);
                if (!flockBirdieDefs.Any()) return;

                var shouldAddBirdToFeeder = flocksAdded == 0 && Game1.random.NextDouble() < 0.65;
                // Ensure feeder is/isn't onscreen
                if (Utility.isOnScreen(feeder.TileLocation * Game1.tileSize, Game1.tileSize) != onScreen) shouldAddBirdToFeeder = false;

                var spawnLocations = BetterBirdie.GetRandomPositionsOrPerchesFor(location, flockBirdieDefs, mustBeOffscreen: true, tileAreaBound: feederRect, spawnType: SpawnType.Land);
                SpawnBirdies(location, shouldAddBirdToFeeder ? spawnLocations.Skip(1) : spawnLocations);
                if (spawnLocations.Any()) flocksAdded++;

                var perch = new Perch(feeder);
                if (shouldAddBirdToFeeder && perch.GetOccupant(location) == null)
                {
                    location.addCritter(new BetterBirdie(flockBirdieDefs.Keys.First(), Vector2.Zero, perch));
                }
            }
        }

        private static void SpawnBirdies(GameLocation location, IEnumerable<BirdieSpawn> spawns)
        {
            foreach (var spawn in spawns)
            {
                if (spawn.BirdiePosition.Perch != null)
                {
                    // Add perched bird
                    location.addCritter(new BetterBirdie(spawn.BirdieDef, Vector2.Zero, spawn.BirdiePosition.Perch, isFledgling: spawn.IsFledgling));
                }
                else
                {
                    var tile = Utilities.XY(spawn.BirdiePosition.Position) / Game1.tileSize;
                    location.addCritter(new BetterBirdie(spawn.BirdieDef, tile, isFledgling: spawn.IsFledgling));
                }
            }
        }

        private static Dictionary<BirdieDef, float> GetRandomBirdieDefWithVariants(GameLocation gameLocation)
        {
            var birdieDefWeights = ContentPackManager.BirdieDefs.Values
                .ToDictionary(
                    birdieDef => birdieDef,
                    birdieDef => birdieDef.GetContextualWeight(updateContext: true, gameLocation, debug: ModEntry.debug_Conditions));

            var birdieDef = Utilities.WeightedRandom(ContentPackManager.BirdieDefs.Values, birdieDef => birdieDefWeights[birdieDef]);
            if (birdieDef == null) return new Dictionary<BirdieDef, float>();

            return ContentPackManager.BirdieDefs.Values
                .Where(b => b.VariantIDs.Contains(birdieDef.ID) && birdieDefWeights[b] > 0)
                .Prepend(birdieDef)
                .ToDictionary(b => b, b => birdieDefWeights[b]);
        }

        private static Dictionary<BirdieDef, float> GetRandomFeederBirdieDefWithVariants(FeederProperties feederProperties, FoodDef foodDef)
        {
            var usualSuspects = ContentPackManager.BirdieDefs.Values.Where(birdieDef => birdieDef.CanPerchAt(feederProperties) && birdieDef.CanEat(foodDef));

            var birdieDefWeights = usualSuspects
                .ToDictionary(
                    birdieDef => birdieDef,
                    birdieDef => birdieDef.GetContextualWeight(updateContext: true, null, feederProperties, foodDef, debug: ModEntry.debug_Conditions));

            var birdieDef = Utilities.WeightedRandom(usualSuspects, birdieDef => birdieDefWeights[birdieDef]);
            if (birdieDef == null) return new Dictionary<BirdieDef, float>();

            return birdieDefWeights.Keys
                .Where(b => b.VariantIDs.Contains(birdieDef.ID) && birdieDefWeights[b] > 0)
                .Prepend(birdieDef)
                .ToDictionary(b => b, b => birdieDefWeights[b]);
        }
    }
}
