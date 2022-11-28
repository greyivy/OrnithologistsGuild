using System;
using GenericModConfigMenu;
using OrnithologistsGuild.Models;

namespace OrnithologistsGuild
{
    public class ConfigManager
    {
        public static ConfigData Config;

        public static void Initialize()
        {
            Config = ModEntry.Instance.Helper.ReadConfig<ConfigData>();

            var manifest = ModEntry.Instance.ModManifest;

            // Config menu
            var configMenu = ModEntry.Instance.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: manifest,
                reset: () => Config = new ConfigData(),
                save: () => ModEntry.Instance.Helper.WriteConfig(Config)
            );

            configMenu.AddSectionTitle(mod: manifest, text: () => "General options");
            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Use vanilla birds *",
                tooltip: () => "* Requires restart",
                getValue: () => Config.LoadVanillaPack,
                setValue: value => Config.LoadVanillaPack = value
            );
            configMenu.AddBoolOption(
                mod: manifest,
                name: () => "Use built-in bird pack *",
                tooltip: () => "* Requires restart",
                getValue: () => Config.LoadBuiltInPack,
                setValue: value => Config.LoadBuiltInPack = value
            );

            configMenu.AddSectionTitle(mod: manifest, text: () => "Save our birds!");
            configMenu.AddParagraph(mod: manifest, text: () => "The world has lost nearly 3 BILLION birds since 1970. For information on the many ways you can help, please visit:");
            configMenu.AddParagraph(mod: manifest, text: () => "https://www.birds.cornell.edu/home/seven-simple-actions-to-help-birds/");
        }
    }
}
