using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace OrnithologistsGuild
{
    public class DataManager
    {
        public static Models.FeederModel[] Feeders;
        public static Models.FoodModel[] Foods;
        public static Models.BirdieModel[] Birdies;

        public static Dictionary<string, string> BirdieAssetIds = new Dictionary<string, string>();

        public static Models.SaveJSONModel SaveData;

        private static Mod Mod;

        private static string GetSaveDataFilename()
        {
            return $"data/{Constants.SaveFolderName}.json";
        }

        public static void Initialize(Mod mod)
        {
            Feeders = mod.Helper.Data.ReadJsonFile<Models.FeederModel[]>("feeders.json");
            Foods = mod.Helper.Data.ReadJsonFile<Models.FoodModel[]>("foods.json");
            Birdies = mod.Helper.Data.ReadJsonFile<Models.BirdieModel[]>("birdies.json");

            // Load birdie assets
            foreach (var birdie in Birdies)
            {
                var asset = $"assets/birdies/{birdie.id.ToString()}.png";

                try
                {
                    mod.Helper.ModContent.Load<Texture2D>(asset);
                    BirdieAssetIds.Add(birdie.id, mod.Helper.ModContent.GetInternalAssetName(asset).BaseName);

                    mod.Monitor.Log($"Loaded birdie asset {asset}", LogLevel.Debug);
                }
                catch
                {
                    mod.Monitor.Log($"Error loading birdie asset {asset}", LogLevel.Warn);
                }
            }

            Mod = mod;
        }

        public static void InitializeSaveData()
        {
            SaveData = Mod.Helper.Data.ReadJsonFile<Models.SaveJSONModel>(GetSaveDataFilename()) ?? new Models.SaveJSONModel();

            Mod.Monitor.Log($"Loaded {SaveData.lifeList.Length} birds on life list");
        }

        public static bool LifeListContains(Models.BirdieModel birdie)
        {
            return SaveData.lifeList.Contains(birdie.id);
        }

        public static void AddToLifeList(Models.BirdieModel birdie)
        {
            var list = SaveData.lifeList.ToList();
            list.Add(birdie.id);

            SaveData.lifeList = list.ToArray();
            Mod.Helper.Data.WriteJsonFile(GetSaveDataFilename(), SaveData);

            Mod.Monitor.Log($"{birdie.name} added to life list");
        }
    }
}
