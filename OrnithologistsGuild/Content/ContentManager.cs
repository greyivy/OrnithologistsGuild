using System.Collections.Generic;

namespace OrnithologistsGuild.Content
{
    public class ContentManager
    {
        public static Models.FoodDef[] Foods { get; private set; }

        public static Dictionary<string, string[]> DefaultBiomes { get; private set; }

        public static void Initialize()
        {
            Foods = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.FoodDef[]>("foods.json");

            DefaultBiomes = ModEntry.Instance.Helper.Data.ReadJsonFile<Dictionary<string, string[]>>("default-biomes.json");
        }
    }
}

