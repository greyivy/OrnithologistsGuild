using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace OrnithologistsGuild
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        internal static ContentPatcher.IContentPatcherAPI CP;

        public static Mod Instance;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Instance = this;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            // Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            Helper.Events.Content.AssetRequested += Content_OnAssetRequested;

            SaveDataManager.Initialize();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            DebugHandleInput(e);
        }

        // private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        // {
        //     if (e.IsOneSecond)
        //     {
        //         Monitor.Log(string.Join(",", Game1.player.mailReceived.Select(m => m.ToString())));
        //     }
        // }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            SaveDataManager.Load();
            Mail.Initialize();
            NestManager.Initialize();

            if (ConfigManager.Config.LogMissingBiomes) {
                foreach (var location in StardewValley.Game1.locations)
                {
                    var biomes = location.GetBiomes();
                    if (location.IsOutdoors && (
                        biomes == null ||
                        biomes.Length == 0 ||
                        (biomes.Length == 1 && biomes[0].Equals("default")
                    )))
                    {
                        Monitor.Log($"No biomes specified for outdoor location \"{location.Name}\"", LogLevel.Warn);
                    }
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Custom tokens
            CP = Helper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
            CP.RegisterToken(ModManifest, "LocationBiome", () =>
            {
                if (!Context.IsWorldReady) return null;

                return Game1.player.currentLocation.GetBiomes();
            });
            // Player name for PowerUp e.g. `Ivy` -> `I-V-Y`
            CP.RegisterToken(ModManifest, "PowerUpPlayerName", () => {
                string[] GetValue(string playerName)
                {
                    return new[] { string.Join('-', playerName.ToUpper().ToCharArray()) };
                }

                // Save is loaded
                if (Context.IsWorldReady)
                    return GetValue(Game1.player.Name);

                // Or save is currently loading
                if (SaveGame.loaded?.player != null)
                    return GetValue(SaveGame.loaded.player.Name);

                // No save loaded (e.g. on the title screen)
                return null;
            });

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

            // Harmony patches
            var harmony = new Harmony(ModManifest.UniqueID);
            GameLocationPatches.Initialize(Monitor, harmony);
            TreePatches.Initialize(Monitor, harmony);
            ObjectPatches.Initialize(Monitor, harmony);

            RegisterDebugCommands();
        }

        private void Content_OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Mods/Ivy.OrnithologistsGuild/Nest"))
            {
                e.LoadFromModFile<Texture2D>("assets/nest.png", AssetLoadPriority.Low);
            }
        }
    }
}