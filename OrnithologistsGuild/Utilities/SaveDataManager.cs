using StardewModdingAPI;

namespace OrnithologistsGuild
{
    public class SaveDataManager
    {
        public static Models.SaveData SaveData;

        private static string GetSaveDataFilename()
        {
            return $"data/{Constants.SaveFolderName}.json";
        }

        public static void Load()
        {
            SaveData = ModEntry.Instance.Helper.Data.ReadJsonFile<Models.SaveData>(GetSaveDataFilename()) ?? new Models.SaveData();

            ModEntry.Instance.Monitor.Log($"Loaded {SaveData.LifeList.Count} life list entries");
        }

        public static void Save()
        {
            ModEntry.Instance.Helper.Data.WriteJsonFile<Models.SaveData>(GetSaveDataFilename(), SaveData);

            ModEntry.Instance.Monitor.Log($"Saved {SaveData.LifeList.Count} life list entries");
        }
    }
}
