﻿using System;
using System.Collections.Generic;
using System.Linq;
using DynamicGameAssets.Game;
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

                EventHandler<StardewModdingAPI.Events.WarpedEventArgs> handlerPlayerWarped = null;
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

            // TODO redo chance system
            // Original SDV source:
            // `double chance1 = Math.Max(0.15, Math.Min(0.5, (double) (this.map.Layers[0].LayerWidth * this.map.Layers[0].LayerHeight) / 15000.0));
            // ...followed by other chances for other types of birds, e.g. woodpecker (which is 1/5th of `chance1`)
            chance = Math.Clamp(chance, 0.15, 0.35);

            ModEntry.Instance.Monitor.Log($"AddBirdies onScreen={onScreen} chance={chance}");

            // First, get locations of all bird feeders
            foreach (var overlaidDict in location.Objects)
            {
                foreach (var obj in overlaidDict.Values)
                {
                    if (typeof(CustomBigCraftable).IsAssignableFrom(obj.GetType()))
                    {
                        var feeder = (CustomBigCraftable)obj;

                        // Only attract birds if there is food
                        if (feeder.MinutesUntilReady > 0)
                        {
                            var feederDef = FeederDef.FromFeeder(feeder);
                            if (feederDef != null)
                            {
                                var foodDef = FoodDef.FromFeeder(feeder);
                                if (foodDef != null)
                                {
                                    AddBirdiesNearFeeder(location, feeder, feederDef, foodDef, chance, onScreen);
                                }
                            }
                        }
                    }
                }
            }

            if (chance > 0) AddRandomBirdies(location, chance, onScreen);
        }

        private static void AddRandomBirdies(GameLocation location, double chance, bool onScreen)
        {
            ModEntry.Instance.Monitor.Log("AddRandomBirdies");

            BirdieDef flockBirdieDef = null;

            // Chance to add another flock
            int flocksAdded = 0;
            while ((ModEntry.debug_AlwaysSpawn != null && flocksAdded == 0) || Game1.random.NextDouble() < chance / (flocksAdded + 1)) // Chance lowers after every flock
            {
                // Determine flock parameters
                flockBirdieDef = ModEntry.debug_AlwaysSpawn == null ? GetRandomBirdieDef() : ModEntry.debug_AlwaysSpawn;
                if (flockBirdieDef == null) return;

                var spawnLocations = BetterBirdie.GetRandomPositionsOrPerchesFor(location, flockBirdieDef, mustBeOffscreen: !onScreen);
                SpawnBirdies(location, flockBirdieDef, spawnLocations);
                if (spawnLocations.Any()) flocksAdded++;
            }
        }

        private static void AddBirdiesNearFeeder(GameLocation location, CustomBigCraftable feeder, FeederDef feederDef, FoodDef food, double chance, bool onScreen)
        {
            ModEntry.Instance.Monitor.Log("AddBirdiesNearFeeder");

            // Build a rectangle around the feeder based on the range
            var feederRect = Utility.getRectangleCenteredAt(feeder.TileLocation, (feederDef.Range * 2) + 1);

            BirdieDef flockBirdieDef = null;

            // Chance to add another flock
            int flocksAdded = 0;
            while (flocksAdded < feederDef.MaxFlocks && Game1.random.NextDouble() < (chance * 2))
            {
                // Determine flock parameters
                flockBirdieDef = GetRandomFeederBirdieDef(feederDef, food);
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

        private static void SpawnBirdies(GameLocation location, BirdieDef birdieDef, IEnumerable<Tuple<Vector3, Perch>> spawnLocations)
        {
            foreach (var spawnLocation in spawnLocations)
            {
                if (spawnLocation.Item2 != null)
                {
                    // Add perched bird
                    location.addCritter(new BetterBirdie(birdieDef, Vector2.Zero, spawnLocation.Item2));
                }
                else
                {
                    var tile = Utilities.XY(spawnLocation.Item1) / Game1.tileSize;
                    location.addCritter(new BetterBirdie(birdieDef, tile));
                }
            }
        }

        private static BirdieDef GetRandomBirdieDef()
        {
            return Utilities.WeightedRandom(ContentPackManager.BirdieDefs.Values, birdieDef => birdieDef.GetContextualWeight(true));
        }

        private static BirdieDef GetRandomFeederBirdieDef(FeederDef feederDef, FoodDef foodDef)
        {
            var usualSuspects = ContentPackManager.BirdieDefs.Values.Where(birdieDef => birdieDef.CanPerchAt(feederDef) && birdieDef.CanEat(foodDef));

            return Utilities.WeightedRandom(usualSuspects, birdieDef => birdieDef.GetContextualWeight(true, feederDef, foodDef));
        }
    }
}

