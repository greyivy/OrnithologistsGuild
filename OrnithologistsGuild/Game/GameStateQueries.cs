using OrnithologistsGuild.Content;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace OrnithologistsGuild
{
    /// <summary>
    /// Add game state queries for use in conditions.
    /// </summary>
    public static class GameStateQueries
    {
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;

            GameStateQuery.Register(Constants.CONDITION_IDENTIFIED_AT_LEAST, IDENTIFIED_AT_LEAST);
            GameStateQuery.Register(Constants.CONDITION_IDENTIFIED_ALL, IDENTIFIED_ALL);
        }

        /// <summary>
        /// Check if the player has identified at least (>=) N birdies
        /// Usage: Ivy.OrnithologistsGuild_IDENTIFIED_AT_LEAST \<N\>
        /// /// </summary>
        /// <param name="query">GSQ query, expected to have 2 items</param>
        /// <param name="context">GSQ context, unused</param>
        /// <returns></returns>
        public static bool IDENTIFIED_AT_LEAST(string[] query, GameStateQueryContext context)
        {
            if (!ArgUtility.TryGetInt(query, 1, out int identify, out string error, "int identify"))
            {
                Monitor.Log(error, LogLevel.Error);
                return false;
            }
            return (SaveDataManager.SaveData?.ForPlayer(Game1.player.UniqueMultiplayerID).LifeList?.IdentifiedCount ?? 0) >= identify;
        }

        /// <summary>
        /// Check if the player has identified at least (>=) N birdies
        /// Usage: Ivy.OrnithologistsGuild_IDENTIFIED_ALL
        /// /// </summary>
        /// <param name="query">GSQ query, expected to have 2 items</param>
        /// <param name="context">GSQ context, unused</param>
        /// <returns></returns>
        public static bool IDENTIFIED_ALL(string[] query, GameStateQueryContext context)
        {
            return (SaveDataManager.SaveData?.ForPlayer(Game1.player.UniqueMultiplayerID).LifeList?.IdentifiedCount ?? 0) >= ContentPackManager.BirdieDefs.Count;
        }
    }
}