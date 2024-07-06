using System;
using System.Collections.Generic;
using System.Linq;
using OrnithologistsGuild.Content;
using StardewValley;
using StardewValley.TerrainFeatures;

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

        public static IEnumerable<Tree> GetTrees(this GameLocation gameLocation) =>
            gameLocation.terrainFeatures?.Values
                    .Where(tf => tf is Tree)
                    .Select(tree => (Tree)tree);

        public static IEnumerable<Tree> GetTreesWithNests(this GameLocation gameLocation) =>
            gameLocation.GetTrees()
                .Where(tree => tree.HasNest());
    }
}

