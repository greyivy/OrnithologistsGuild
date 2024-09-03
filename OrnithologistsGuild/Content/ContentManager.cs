using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

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
        }
    }
}

