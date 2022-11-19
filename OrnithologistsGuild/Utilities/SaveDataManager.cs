using StardewModdingAPI;

namespace OrnithologistsGuild
{
    public class SaveDataManager
    {
        public static Models.SaveJSON SaveData;

        private static string GetSaveDataFilename()
        {
            return $"data/{Constants.SaveFolderName}.json";
        }

        public static void Load()
        {
            SaveData = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.SaveJSON>(GetSaveDataFilename()) ?? new Models.SaveJSON();

            ModEntry.Instance.Monitor.Log($"Loaded {SaveData.LifeList.Count} life list entries");
        }

        public static void Save()
        {
            ModEntry.Instance.Helper.Data.WriteJsonFile<Models.SaveJSON>(GetSaveDataFilename(), SaveData);

            ModEntry.Instance.Monitor.Log($"Saved {SaveData.LifeList.Count} life list entries");
        }
    }
}
