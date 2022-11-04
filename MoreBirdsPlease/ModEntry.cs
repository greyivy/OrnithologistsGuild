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
using MoreBirdsPlease.Game.Items;

// Notes:
// Automate mod support (see dynamic assets readme)
// Multiplayer... works! Each player gets their own birds, but they can be scared away by the other player. Not bad. Adding synced birds will take A LOT of work.

// TODO Before v1:
// - Patch addbirdies to add specific BetterBirdies
// - Perch on tube feeder

// TODO Planned features:
// - Bird baths
// - A field guide that gets filled as you see new birds?
// - Custom mail backgrounds https://www.nexusmods.com/stardewvalley/mods/1536
// - Specific bird behavior/speed/etc. per Birdie

namespace MoreBirdsPlease
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static DynamicGameAssets.IDynamicGameAssetsApi dga;
        internal static ContentPack dgaPack;

        public static Mod instance;

        private List<string> previousActiveLocations = new List<string>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;

            this.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            this.Helper.Events.Player.Warped += Player_Warped;

            DataManager.Initialize(this);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            DataManager.InitializeSaveData();
            Mail.Initialize();
        }

        /*
        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {

        RESPAWNS BIRDS REGULARLY WHEN OFF SCREEN

            if (e.IsMultipleOf(60 * 5 /* 5s ))
            {

                            // see if any critters remain in their original position, else respawn Game1.player.currentLocation.critters[0].startingPosition

  
                            if (Utility.isOnScreen(bc.TileLocation * (float)Game1.tileSize, Game1.tileSize * feeder.range)) {
                                this.Monitor.Log("onsc");
                            } else
                            {
                                this.Monitor.Log("offsc");

                            }
                        }
                }
            }
        }
        */

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            ConsiderAddingBirds(e.NewLocation);
        }

        private void ConsiderAddingBirds(GameLocation location)
        {
            // First, get locations of all bird feeders
            foreach (var overlaidDict in location.Objects)
            {
                foreach (var obj in overlaidDict.Values)
                {
                    if (typeof(CustomBigCraftable).IsAssignableFrom(obj.GetType()))
                    {
                        var bigCraftable = (CustomBigCraftable)obj;

                        // Only attract birds if there is food
                        if (bigCraftable.MinutesUntilReady > 0)
                        {
                            var feeder = DataManager.Feeders.FirstOrDefault(feeder => feeder.id == bigCraftable.Id);
                            if (feeder != null)
                            {
                                var food = DataManager.Foods.FirstOrDefault(food => bigCraftable.TextureOverride.EndsWith($":{food.feederAssetIndex}"));
                                if (food != null)
                                {
                                    this.AddBirdsNearFeeder(location, bigCraftable.TileLocation, feeder, food);
                                }
                            }
                        }
                    }
                }
            }
        }

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

            // 65% change to add another flock
            int flocksAdded = 0;
            while (flocksAdded < feeder.maxFlocks && Game1.random.NextDouble() < 0.65)
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

                    bool birdAddedToFeeder = false;

                    // Spawn birdies
                    List<Critter> crittersToAdd = new List<Critter>();
                    for (int index = 0; index < flockSize; ++index)
                    {
                        if (!birdAddedToFeeder && Game1.random.NextDouble() < 0.65)
                        {
                            // Maybe a stationary birdie eating at the feeder
                            location.addCritter((Critter)new BetterBirdie(flockSpecies, (int)feederLocation.X, (int)feederLocation.Y, feeder));
                        } else
                        {
                            crittersToAdd.Add((Critter)new BetterBirdie(flockSpecies, -100, -100));
                        }
                    }

                    this.Helper.Reflection.GetMethod(location, "addCrittersStartingAtTile").Invoke(randomTile, crittersToAdd);

                    flocksAdded++;

                    break;
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Register content pack
            dga = Helper.ModRegistry.GetApi<DynamicGameAssets.IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            dga.AddEmbeddedPack(this.ModManifest, Path.Combine(Helper.DirectoryPath, "assets", "dga"));
            dgaPack = DynamicGameAssets.Mod.GetPacks().First(cp => cp.GetManifest().UniqueID == ModManifest.UniqueID);

            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(Binoculars));

            Helper.ConsoleCommands.Add("mbp_debug", "Adds debug items to inventory", OnDebugCommand);
        }

        private void OnDebugCommand(string cmd, string[] args)
        {
            // Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(270, 32)); // Corn
            // Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(770, 32)); // Mixed Seeds
            Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(431, 32)); // Sunflower Seeds
            // Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(832, 32)); // Pineapple

            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("WoodenHopper").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("WoodenPlatform").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("PlasticTube").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("SeedHuller").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)dgaPack.Find("SeedHuller").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)new Binoculars());
        }
    }
}