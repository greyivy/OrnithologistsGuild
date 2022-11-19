using System;
namespace OrnithologistsGuild.Content
{
    public class ContentManager
    {
        public static Models.FeederDef[] Feeders;
        public static Models.FoodDef[] Foods;

        public static void Initialize()
        {
            Feeders = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.FeederDef[]>("feeders.json");
            Foods = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.FoodDef[]>("foods.json");
        }
    }
}

