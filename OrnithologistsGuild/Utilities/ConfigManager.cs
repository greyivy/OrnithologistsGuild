﻿using System;
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

            configMenu.AddSectionTitle(mod: manifest, text: () => I18n.Config_GeneralOptions());
            configMenu.AddParagraph(mod: manifest, text: () => I18n.Config_GeneralOptions_Note());

            //configMenu.AddBoolOption(
            //    mod: manifest,
            //    name: () => I18n.Config_UseVanilla_Name(),
            //    tooltip: () => I18n.Config_UseVanilla_Tooltip(),
            //    getValue: () => Config.LoadVanillaPack,
            //    setValue: value => Config.LoadVanillaPack = value
            //);
            configMenu.AddNumberOption(
                mod: manifest,
                name: () => I18n.Config_Volume_Name(),
                tooltip: () => I18n.Config_Volume_Tooltip(),
                getValue: () => Config.CallVolume,
                setValue: value => Config.CallVolume = value,
                min: 0,
                max: 100,
                interval: 10,
                formatValue: value => $"{value}%"
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.Config_UseBuiltIn_Name(),
                tooltip: () => I18n.Config_UseBuiltIn_Tooltip(),
                getValue: () => Config.LoadBuiltInPack,
                setValue: value => Config.LoadBuiltInPack = value
            );
            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.Config_LogMissingBiomes_Name(),
                tooltip: () => I18n.Config_LogMissingBiomes_Tooltip(),
                getValue: () => Config.LogMissingBiomes,
                setValue: value => Config.LogMissingBiomes = value
            );
            configMenu.AddBoolOption(
                mod: manifest,
                name: () => I18n.Config_NoBreakOrJam_Title(),
                getValue: () => Config.NoBreakOrJam,
                setValue: value => Config.NoBreakOrJam = value
            );

            configMenu.AddSectionTitle(mod: manifest, text: () => I18n.Config_SaveOurBirds_Title());
            configMenu.AddParagraph(mod: manifest, text: () => I18n.Config_SaveOurBirds_Content());
            configMenu.AddParagraph(mod: manifest, text: () => "https://www.birds.cornell.edu/home/seven-simple-actions-to-help-birds/");

            configMenu.AddSectionTitle(mod: manifest, text: () => I18n.Config_AboutKyle_Title());
            configMenu.AddParagraph(mod: manifest, text: () => I18n.Config_AboutKyle_Content());
        }
    }
}
