using System;
using System.Linq;

namespace OrnithologistsGuild.Content
{
    public class ContentManager
    {
        public static Models.BathDef[] Baths;
        public static Models.FeederDef[] Feeders;

        public static Models.FoodDef[] Foods;

        public static string[] BathIds;
        public static string[] FeederIds;

        public static void Initialize()
        {
            Baths = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.BathDef[]>("baths.json");
            Feeders = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.FeederDef[]>("feeders.json");

            Foods = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.FoodDef[]>("foods.json");

            BathIds = Baths.Select(b => b.ID).ToArray();
            FeederIds = Feeders.Select(f => f.ID).ToArray();
        }
    }
}

