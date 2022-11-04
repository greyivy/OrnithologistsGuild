using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using xTile.Dimensions;
using DynamicGameAssets.PackData;
using System.IO;
using System.Collections;
using DynamicGameAssets.Game;
using SpaceShared.APIs;
using System.Threading.Channels;
using Microsoft.Xna.Framework.Graphics;

// TODO achievements for seeing birds? A field guide that gets filled as you see new birds? Mail framework mod with gifts!


namespace MoreBirdsPlease
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static DynamicGameAssets.IDynamicGameAssetsApi dga;
        internal static ContentPack dgaPack;

        public static Mod instance;

        private List<string> previousActiveLocations = new List<string>();

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;

            this.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            this.Helper.Events.Player.Warped += Player_Warped;

            // this.Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            DataManager.Initialize(this);
        }

        /*
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {

        RESPAWNS BIRDS REGULARLY WHEN OFF SCREEN

            if (e.IsMultipleOf(60 * 5 /* 5s ))
            {
                // First, get locations of all bird feeders
                foreach (var d in Game1.player.currentLocation.Objects)
                {
                    foreach (var o in d.Values)
                        if (typeof(CustomBigCraftable).IsAssignableFrom(o.GetType()))
                        {
                            var bc = (CustomBigCraftable)o;

                            // TODO check bird feeder ID, decide on range, attraction based on it

                            this.Monitor.Log(bc.Id + ": " + bc.TileLocation.ToString() + ", " + bc.MinutesUntilReady + ", " + bc.TextureOverride);

                            // could maybe use TextureOveride to determine type of food if set a different texture for each type of food
                            var feeder = new FeederDefinition()
                            {
                                range = 10,
                                maxFlocks = 2,
                                maxFlockSize = 5
                            };

                            var r = feeder.getRangeRect(bc.TileLocation);

                            // see if any critters remain in their original position, else respawn Game1.player.currentLocation.critters[0].startingPosition

                            //if (Utility.isOnScreen(new Vector2(r.Top, r.Left) * (float)Game1.tileSize, Game1.tileSize) ||
                            //    Utility.isOnScreen(new Vector2(r.Bottom, r.Left) * (float)Game1.tileSize, Game1.tileSize) ||
                            //    Utility.isOnScreen(new Vector2(r.Top, r.Right) * (float)Game1.tileSize, Game1.tileSize) ||
                            //    Utility.isOnScreen(new Vector2(r.Bottom, r.Right) * (float)Game1.tileSize, Game1.tileSize) ||
                            //    Utility.isOnScreen(bc.TileLocation * (float)Game1.tileSize, Game1.tileSize))
                            //{
                            if (Utility.isOnScreen(bc.TileLocation * (float)Game1.tileSize, Game1.tileSize * feeder.range)) {
                                this.Monitor.Log("onsc");
                            } else
                            {
                                this.Monitor.Log("offsc");

                            }



                            // Only attract birds if there is food
                            //if (bc.MinutesUntilReady > 0)
                            //{
                            //    this.AddBirdsNearFeeder(location, bc.TileLocation, feeder);
                            //}
                        }
                }
            }
        }
        */

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            ConsiderAddingBirds(e.NewLocation);
        }

        /* 
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // TODO ARE BIRDS SHARED BETWEEN PLAYERS?

            // Ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;

            CheckNewlyActiveLocations();
        }

        private void CheckNewlyActiveLocations()
        {
            // This doesn't work well because farms always stay loaded. Maybe we should just override addBirdies
            var activeLocations = this.Helper.Multiplayer.GetActiveLocations();
            var newlyActiveLocations = new List<GameLocation>();

            foreach (var location in activeLocations)
            {
                if (!previousActiveLocations.Contains(location.Name))
                {
                    newlyActiveLocations.Add(location);
                }
            }

            if (newlyActiveLocations.Any())
            {
                foreach (var location in newlyActiveLocations)
                {
                    ConsiderAddingBirds(location);
                }

                previousActiveLocations = activeLocations.Select(l => l.Name).ToList();
            }
        }
        */

        private void ConsiderAddingBirds(GameLocation location)
        {
            // First, get locations of all bird feeders
            foreach (var d in location.Objects)
            {
                foreach (var o in d.Values)
                    if (typeof(CustomBigCraftable).IsAssignableFrom(o.GetType()))
                    {
                        var bc = (CustomBigCraftable)o;

                        // TODO
                        this.Monitor.Log(bc.Id + ": " + bc.TileLocation.ToString() + ", " + bc.MinutesUntilReady + ", " + bc.TextureOverride);

                        // Only attract birds if there is food
                        if (bc.MinutesUntilReady > 0)
                        {
                            var feeder = DataManager.Feeders.FirstOrDefault(f => f.id == bc.Id);
                            if (feeder != null)
                            {
                                // TODO
                                this.Monitor.Log("FEEDER: " + feeder.id);

                                var food = DataManager.Foods.FirstOrDefault(f => bc.TextureOverride.EndsWith($":{f.feederAssetIndex}"));
                                if (food != null)
                                {
                                    // TODO
                                    this.Monitor.Log("FOOD: " + food.id);

                                    this.AddBirdsNearFeeder(location, bc.TileLocation, feeder, food);
                                }
                            }
                        }
                    }
            }
        }

        // TODO
        private static Microsoft.Xna.Framework.Rectangle GetFeederRangeRect(Models.FeederModel feeder, Vector2 feederLocation)
        {
            return new Microsoft.Xna.Framework.Rectangle((int)feederLocation.X - feeder.range, (int)feederLocation.Y - feeder.range, (feeder.range * 2) + 1, (feeder.range * 2) + 1);
        }

        private static Models.BirdieModel GetRandomBirdie(Models.FeederModel feeder, Models.FoodModel food)
        {
            var usualSuspects = DataManager.Birdies.Where(b => b.weightedFeeders.ContainsKey(feeder.type) && b.weightedFoods.ContainsKey(food.id)).ToList();

            // TODO optimize
            var weightedUsualSuspects = new List<Models.BirdieModel>();
            foreach (var birdie in usualSuspects)
            {
                weightedUsualSuspects.AddRange(Enumerable.Repeat(birdie, birdie.weightedFeeders[feeder.type] + birdie.weightedFoods[food.id]));
            }

            return weightedUsualSuspects[Game1.random.Next(0, weightedUsualSuspects.Count - 1)];
        }

        private void AddBirdsNearFeeder(GameLocation location, Vector2 feederLocation, Models.FeederModel feeder, Models.FoodModel food)
        {
            // No birdies past 8:00 PM (it's their bedtime), in the desert or railroad // TODO ensure they don't spawn indoors!
            if (Game1.timeOfDay >= 1800 || location is Desert || (location is Railroad)) return;

            // Build a rectangle around the feeder based on the range
            var feederRect = GetFeederRangeRect(feeder, feederLocation);
            this.Monitor.Log("Trying to spawn birds at " + feederRect.ToString());

            Models.BirdieModel flockSpecies = null;

            // 75% change to add another flock
            int flocksAdded = 0;
            while (flocksAdded < feeder.maxFlocks && Game1.random.NextDouble() < 0.75)
            {
                // Determine flock parameters
                flockSpecies = GetRandomBirdie(feeder, food);
                int flockSize = Game1.random.Next(1, flockSpecies.maxFlockSize + 1);

                // Try 50 times to find an empty patch within the feeder range
                for (int trial = 0; trial < 100; trial++)
                {
                    // Get a random tile within the feeder range
                    var randomTile = new Vector2(Game1.random.Next(feederRect.Left, feederRect.Right + 1), Game1.random.Next(feederRect.Top, feederRect.Bottom));

                    if (!location.isTileOnMap(randomTile)) continue;

                    // Get a 3x3 patch around the random tile 
                    // var randomRect = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 2, (int)randomTile.Y - 2, 5, 5); // TODO revert to 5x5 if needed
                    var randomRect = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 1, (int)randomTile.Y - 1, 3, 3);

                    if (!location.isAreaClear(randomRect)) continue;

                    this.Monitor.Log($"Found clear location at {randomRect}, adding flock of {flockSize} {flockSpecies.name} ({flockSpecies.id})");

                    // Spawn birdies
                    List<Critter> crittersToAdd = new List<Critter>();
                    for (int index = 0; index < flockSize; ++index)
                        crittersToAdd.Add((Critter)new BetterBirdie(flockSpecies, -100, -100));

                    this.Helper.Reflection.GetMethod(location, "addCrittersStartingAtTile").Invoke(randomTile, crittersToAdd);

                    flocksAdded++;

                    break;
                }
            }

            if (flockSpecies != null && Game1.random.NextDouble() < 0.75)
            {
                // Maybe a stationary birdie eating at the feeder
                location.addCritter((Critter)new BetterBirdie(flockSpecies, (int)feederLocation.X, (int)feederLocation.Y, feeder));
            }
        }

        // TODO
        private void AddBirdsInBath(GameLocation location, Vector2 bathLocation)
        {

        }

        //private void World_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        //{
        //    foreach (var o in new List<KeyValuePair<Vector2, StardewValley.Object>>(e.Added))
        //    {
        //        if (typeof(CustomBigCraftable).IsAssignableFrom(o.Value.GetType()))
        //        {
        //            var bc = (CustomBigCraftable)o.Value;

        //            this.Monitor.Log(bc.Id);
        //        }
        //    }
        //}

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Register content pack
            dga = Helper.ModRegistry.GetApi<DynamicGameAssets.IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            dga.AddEmbeddedPack(this.ModManifest, Path.Combine(Helper.DirectoryPath, "assets", "dga"));
            dgaPack = DynamicGameAssets.Mod.GetPacks().First(cp => cp.GetManifest().UniqueID == ModManifest.UniqueID);

            Helper.ConsoleCommands.Add("mbp_debug", "Adds debug items to inventory", OnDebugCommand);

            BetterBirdie.LoadAssets(this);
        }

        private void OnDebugCommand(string cmd, string[] args)
        {
            Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(270, 32)); // Corn
            Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(770, 32)); // Mixed Seeds
            Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(431, 32)); // Sunflower Seeds
            Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(832, 32)); // Pineapple

            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("WoodenHopper").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("WoodenPlatform").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("PlasticTube").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("SeedHuller").ToItem());
        }
    }
}