using System;
using OrnithologistsGuild.Content;
using StardewValley;

namespace OrnithologistsGuild
{
    public static class GameLocationExtensions
    {
        public static string[] GetBiomes(this GameLocation gameLocation)
        {
            if (!gameLocation.IsOutdoors) return null;

            if (!string.IsNullOrWhiteSpace(gameLocation.getMapProperty("Biomes")))
            {
                return gameLocation.getMapProperty("Biomes").Split("/");
            }

            if (ContentManager.DefaultBiomes.ContainsKey(gameLocation.Name))
            {
                return ContentManager.DefaultBiomes[gameLocation.Name];
            }

            return new string[] { "default" };
        }
    }
}

