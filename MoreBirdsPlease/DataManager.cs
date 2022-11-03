using System;
using StardewModdingAPI;

namespace MoreBirdsPlease
{
    public class DataManager
    {
        public static Models.FeederModel[] Feeders;
        public static Models.FoodModel[] Foods;
        public static Models.BirdieModel[] Birdies;

        public static void Initialize(Mod mod)
        {
            Feeders = mod.Helper.Data.ReadJsonFile<Models.FeederModel[]>("feeders.json");
            Foods = mod.Helper.Data.ReadJsonFile<Models.FoodModel[]>("foods.json");
            Birdies = mod.Helper.Data.ReadJsonFile<Models.BirdieModel[]>("birdies.json");
        }
    }
}
