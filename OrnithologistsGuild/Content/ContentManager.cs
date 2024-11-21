using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData;

namespace OrnithologistsGuild.Content
{
    public class ContentManager
    {
        public static Models.FoodDef[] Foods { get; private set; }

        public static Dictionary<string, string[]> DefaultBiomes { get; private set; }

        public static void Initialize()
        {
            ModEntry.Instance.Helper.Events.Content.AssetRequested += Content_AssetRequested;

            Foods = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.FoodDef[]>("foods.json");

            DefaultBiomes = ModEntry.Instance.Helper.Data.ReadJsonFile<Dictionary<string, string[]>>("default-biomes.json");
        }

        private static void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Mods/Ivy.OrnithologistsGuild/Nest"))
            {
                e.LoadFromModFile<Texture2D>("assets/nest.png", AssetLoadPriority.Low);
            }
            else if (e.Name.IsEquivalentTo("Mods/Ivy.OrnithologistsGuild/Binoculars"))
            {
                e.LoadFromModFile<Texture2D>("assets/binoculars.png", AssetLoadPriority.Low);
            }
            // mail & triggers
            // this can be done in content patcher ofc, kept here for i18n key.
            if (e.Name.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(Edit_DataMail);
            }
            if (e.Name.IsEquivalentTo("Data/TriggerActions"))
            {
                e.Edit(Edit_DataTriggerActions);
            }
        }

        private static void Edit_DataMail(IAssetData data)
        {
            var mail = data.AsDictionary<string, string>().Data;
            // give lifelist
            mail["Mods_Ivy_OrnithologistsGuild_Introduction"] = $"{I18n.Mail_Introduction()}%item id (T)Ivy_OrnithologistsGuild_LifeList %% %item conversationTopic Ivy_OrnithologistGuild_Introduction 14 %% [#]{I18n.Mail_Introduction_Title()}";
            // 1 bird identified, mixed seeds
            mail["Mods_Ivy_OrnithologistsGuild_LifeList1"] = $"{I18n.Mail_LifeList1()}%item id (O)770 5 %% [#]{I18n.Mail_LifeList1_Title()}";
            // 3 birds identified, corn
            mail["Mods_Ivy_OrnithologistsGuild_LifeList3"] = $"{I18n.Mail_LifeList3()}%item id (O)270 5 %% [#]{I18n.Mail_LifeList3_Title()}";
            // 5 birds identified, sunflower seeds
            mail["Mods_Ivy_OrnithologistsGuild_LifeList5"] = $"{I18n.Mail_LifeList5()}%item id (O)431 5 %% [#]{I18n.Mail_LifeList5_Title()}";
            // 7 birds identified, salmonberries
            mail["Mods_Ivy_OrnithologistsGuild_LifeList7"] = $"{I18n.Mail_LifeList7()}%item id (O)296 5 %% [#]{I18n.Mail_LifeList7_Title()}";
            // all bird identified, golden egg
            mail["Mods_Ivy_OrnithologistsGuild_LifeListAll"] = $"{I18n.Mail_LifeListAll()}%item id (O)928 %% %item conversationTopic Ivy_OrnithologistGuild_LifeListAll 14 %% [#]{I18n.Mail_LifeListAll_Title()}";
        }

        private static void Edit_DataTriggerActions(IAssetData data)
        {
            string modId = ModEntry.Instance.ModManifest.UniqueID;
            var tActs = data.GetData<List<TriggerActionData>>();
            // send intro mail once, immediately on day start/mod install
            tActs.Add(new()
            {
                Id = $"{modId}_Mail_Introduction",
                Trigger = "DayStarted",
                Condition = "DATE_RANGE spring 5 1",
                Action = "AddMail Current Mods_Ivy_OrnithologistsGuild_Introduction Now",
                // set MarkActionApplied=true to do this only once, this is the default value
                //MarkActionApplied = true
            });
            // send identified count mails at end of day (for tomorrow), when goal is reached
            tActs.Add(new()
            {
                Id = $"{modId}_Mail_LifeList1",
                Trigger = "DayEnding",
                Condition = "Ivy.OrnithologistsGuild_IDENTIFIED_AT_LEAST 1",
                Action = "AddMail Current Mods_Ivy_OrnithologistsGuild_LifeList1",
            });
            tActs.Add(new()
            {
                Id = $"{modId}_Mail_LifeList3",
                Trigger = "DayEnding",
                Condition = "Ivy.OrnithologistsGuild_IDENTIFIED_AT_LEAST 3",
                Action = "AddMail Current Mods_Ivy_OrnithologistsGuild_LifeList3",
            });
            tActs.Add(new()
            {
                Id = $"{modId}_Mail_LifeList5",
                Trigger = "DayEnding",
                Condition = "Ivy.OrnithologistsGuild_IDENTIFIED_AT_LEAST 5",
                Action = "AddMail Current Mods_Ivy_OrnithologistsGuild_LifeList5",
            });
            tActs.Add(new()
            {
                Id = $"{modId}_Mail_LifeList7",
                Trigger = "DayEnding",
                Condition = "Ivy.OrnithologistsGuild_IDENTIFIED_AT_LEAST 7",
                Action = "AddMail Current Mods_Ivy_OrnithologistsGuild_LifeList7",
            });
            tActs.Add(new()
            {
                Id = $"{modId}_Mail_LifeListAll",
                Trigger = "DayEnding",
                Condition = "Ivy.OrnithologistsGuild_IDENTIFIED_ALL",
                Action = "AddMail Current Mods_Ivy_OrnithologistsGuild_LifeListAll",
            });
        }

    }
}

