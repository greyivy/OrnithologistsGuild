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
using OrnithologistsGuild.Game.Items;
using HarmonyLib;

// Notes:
// Automate mod support (see dynamic assets readme)
// Multiplayer... works! Each player gets their own birds, but they can be scared away by the other player. Not bad. Adding synced birds will take A LOT of work.

// TODO Planned features:
// - Bird baths
// - Custom mail backgrounds? Or color? https://www.nexusmods.com/stardewvalley/mods/1536
// - Specific bird behavior/speed/etc. per Birdie
// - Better bird models / sounds
// - Better weight/chance system taking luck into account
// - More sets of binoculars

namespace OrnithologistsGuild
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

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Register content pack
            dga = Helper.ModRegistry.GetApi<DynamicGameAssets.IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            dga.AddEmbeddedPack(this.ModManifest, Path.Combine(Helper.DirectoryPath, "assets", "dga"));
            dgaPack = DynamicGameAssets.Mod.GetPacks().First(cp => cp.GetManifest().UniqueID == ModManifest.UniqueID);

            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(Binoculars));
            sc.RegisterSerializerType(typeof(LifeList));

            Helper.ConsoleCommands.Add("og_debug", "Adds debug items to inventory", OnDebugCommand);

            var harmony = new Harmony(this.ModManifest.UniqueID);

            LocationPatches.Initialize(this.Monitor);
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.GameLocation), nameof(StardewValley.GameLocation.addBirdies)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.addBirdies_Prefix))
            );
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
            Game1.player.addItemByMenuIfNecessary((Item)new LifeList());
            Game1.player.addItemByMenuIfNecessary((Item)new Binoculars());
        }
    }
}