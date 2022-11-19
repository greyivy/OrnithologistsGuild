using System.IO;
using System.Linq;
using DynamicGameAssets.PackData;
using HarmonyLib;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game.Items;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

// Notes:
// Automate mod support (see dynamic assets readme)
// Multiplayer... works! Each player gets their own birds, but they can be scared away by the other player. Not bad. Adding synced birds will take A LOT of work.

// Planned features:
// - Bird baths
// - Custom mail backgrounds? Or color? https://www.nexusmods.com/stardewvalley/mods/1536
// - Binoculars work on Critters.Owl, Woodpecker, Crow/Magpie etc. (or even customize these)
// - disable tooltip for birdhouses
// - can kyle read and write? are the letters from him?
// - emote position for kyle not in the right place. this affects a few things but i'm not sure how to patch the draw method.
// - ridgeside village map issues
// - i18n for strings
// - fix up logging
// - mod. Book: all crows are beautiful. Shoutout to the raptors for kyles inspiration. Bottle of blackberries as his favorite gift

namespace OrnithologistsGuild
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static DynamicGameAssets.IDynamicGameAssetsApi DGA;
        internal static ContentPack DGAContentPack;

        public static Mod Instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;

            this.Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        //private void Player_Warped(object sender, WarpedEventArgs e)
        //{
        //    var kyle = e.NewLocation.characters.FirstOrDefault(c => c.Name == "OrinothlogistsGuild_Kyle");

        //    if (kyle != null)
        //    {
        //        // Increase width of character sprite
        //        kyle.Sprite = new AnimatedSprite(kyle.Sprite.loadedTexture, 0, 32, 32);
        //    }
        //}

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SaveDataManager.Load();
            Mail.Initialize();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Internal content
            ContentManager.Initialize();

            // Ornithologist's Guild content packs
            ContentPackManager.Initialize();
            ContentPackManager.LoadBuiltIn(); // TODO option
            ContentPackManager.LoadAll();

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

            // Console commands
            Helper.ConsoleCommands.Add("og_debug", "Adds debug items to inventory", OnDebugCommand);
        }

        private void OnDebugCommand(string cmd, string[] args)
        {
            // Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(270, 32)); // Corn
            // Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(770, 32)); // Mixed Seeds
            Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(431, 32)); // Sunflower Seeds
            // Game1.player.addItemByMenuIfNecessary((Item)new StardewValley.Object(832, 32)); // Pineapple

            Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("WoodenHopper").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("WoodenPlatform").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("PlasticTube").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)DGAContentPack.Find("SeedHuller").ToItem());
            Game1.player.addItemByMenuIfNecessary((Item)new LifeList());
            Game1.player.addItemByMenuIfNecessary((Item)new JojaBinoculars());
            Game1.player.addItemByMenuIfNecessary((Item)new AntiqueBinoculars());
            Game1.player.addItemByMenuIfNecessary((Item)new ProBinoculars());
        }
    }
}