using System;
using System.IO;
using System.Linq;
using DynamicGameAssets.PackData;
using HarmonyLib;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using OrnithologistsGuild.Game.Critters;
using OrnithologistsGuild.Game.Items;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace OrnithologistsGuild
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static DynamicGameAssets.IDynamicGameAssetsApi DGA;
        internal static ContentPack DGAContentPack;

        public static BirdieDef debug_AlwaysSpawn = null;
        public static PerchType? debug_PerchType = null;
        private static bool debug_EnableBirdWhisperer = false;
        public static Vector2? debug_BirdWhisperer = null;

        public static Mod Instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;

            this.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            // this.Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            this.Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (debug_EnableBirdWhisperer)
            {
                debug_BirdWhisperer = e.Cursor.Tile * Game1.tileSize;
                Monitor.Log($"Bird whisperer: {debug_BirdWhisperer.ToString()}");

                var birdie = Game1.player.currentLocation.critters.Where(c => c is BetterBirdie).FirstOrDefault();
                if (birdie != null)
                {
                    ((BetterBirdie)birdie).Frighten();
                }
            }
        }

        // private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        // {
        //     if (e.IsOneSecond)
        //     {
        //         this.Monitor.Log(string.Join(",", Game1.player.mailReceived.Select(m => m.ToString())));
        //     }
        // }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SaveDataManager.Load();
            Mail.Initialize();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Config
            ConfigManager.Initialize();

            // Translation
            I18n.Init(Helper.Translation);

            // Internal content
            ContentManager.Initialize();

            // Ornithologist's Guild content packs
            ContentPackManager.Initialize();
            if (ConfigManager.Config.LoadVanillaPack) ContentPackManager.LoadVanilla();
            if (ConfigManager.Config.LoadBuiltInPack) ContentPackManager.LoadBuiltIn();
            ContentPackManager.LoadExternal();

            // Dynamic Game Assets content pack
            DGA = Helper.ModRegistry.GetApi<DynamicGameAssets.IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            DGA.AddEmbeddedPack(this.ModManifest, Path.Combine(Helper.DirectoryPath, "assets", "dga"));
            DGAContentPack = DynamicGameAssets.Mod.GetPacks().First(cp => cp.GetManifest().UniqueID == ModManifest.UniqueID);

            // Save serializer
            var sc = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(JojaBinoculars));
            sc.RegisterSerializerType(typeof(AntiqueBinoculars));
            sc.RegisterSerializerType(typeof(ProBinoculars));
            sc.RegisterSerializerType(typeof(LifeList));

            // Harmony patches
            var harmony = new Harmony(this.ModManifest.UniqueID);

            LocationPatches.Initialize(this.Monitor);
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.GameLocation), nameof(StardewValley.GameLocation.addBirdies)),
               prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.addBirdies_Prefix))
            );

            TreePatches.Initialize(this.Monitor);
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.TerrainFeatures.Tree), nameof(StardewValley.TerrainFeatures.Tree.performUseAction)),
               prefix: new HarmonyMethod(typeof(TreePatches), nameof(TreePatches.performUseAction_Prefix))
            );

            // Console commands
            Helper.ConsoleCommands.Add("ogd", "Adds debug items to inventory", OnDebugCommand);
            Helper.ConsoleCommands.Add("ogs", "Consistently spawns specified birdie ID", OnDebugCommand);
            Helper.ConsoleCommands.Add("ogp", "Forces birdies to perch on specified perch type", OnDebugCommand);
            Helper.ConsoleCommands.Add("ogw", "Bird Whisperer: ask a bird (nicely) to go wherever you click", OnDebugCommand);
        }

        private void OnDebugCommand(string cmd, string[] args)
        {
            if (cmd.Equals("ogd"))
            {
                //Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(270, 32)); // Corn
                Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(770, 32)); // Mixed Seeds
                //Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(431, 32)); // Sunflower Seeds
                Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(832, 32)); // Pineapple

                //Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("WoodenHopper").ToItem());
                //Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("WoodenPlatform").ToItem());
                //Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("PlasticTube").ToItem());
                //Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("SeedHuller").ToItem());
                Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("StoneBath").ToItem());
                Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("HeatedStoneBath").ToItem());
                //Game1.player.addItemByMenuIfNecessary((Item)new LifeList());
                //Game1.player.addItemByMenuIfNecessary((Item)new JojaBinoculars());
                //Game1.player.addItemByMenuIfNecessary((Item)new AntiqueBinoculars());
                //Game1.player.addItemByMenuIfNecessary((Item)new ProBinoculars());
            }
            else if (cmd.Equals("ogs"))
            {
                BirdieDef birdieDef = ContentPackManager.BirdieDefs.Values.FirstOrDefault(birdieDef => birdieDef.ID.Equals(args.Length == 0 ? "HouseSparrow" : args[0], StringComparison.OrdinalIgnoreCase));
                if (birdieDef == null)
                {
                    Monitor.Log($"Birdie \"{args[0]}\" not found", LogLevel.Error);
                }

                debug_AlwaysSpawn = birdieDef;
            }
            else if (cmd.Equals("ogp"))
            {
                debug_PerchType = (PerchType)Enum.Parse(typeof(PerchType), args[0]);
            }
            else if (cmd.Equals("ogw"))
            {
                debug_EnableBirdWhisperer = true;
            }
        }
    }
}