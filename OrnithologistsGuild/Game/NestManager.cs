using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace OrnithologistsGuild.Game
{
    public class NestManager
    {
        // TODO disable nesting for some birds
        public const string KeyNest = "Ivy_OrnithologistsGuild__Nest";

        private static ConditionalWeakTable<Tree, Nest> nestCache = new ConditionalWeakTable<Tree, Nest>();

        public static void Initialize()
        {
            ModEntry.Instance.Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
        }

        private static IEnumerable<GameLocation> GetValidNestingLocations() =>
            Game1.locations.Where(location => location?.NameOrUniqueName != null && location.IsOutdoors);

        private static void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            ClearRemovedNests();

            // Flush cache every day
            nestCache.Clear();
        }

        public static bool CanBuildNestAt(GameLocation gameLocation)
        {
            var trees = gameLocation.GetTrees();
            var percentTreesWithNests = trees.Where(tree => tree.HasNest()).Count() / trees.Count();

            return percentTreesWithNests < 0.2 && GetValidNestingLocations().Contains(gameLocation) &&
                (Game1.season == Season.Spring && Game1.dayOfMonth >= 0 && Game1.dayOfMonth <= 6); // First week of Spring
        }

        private static void ClearRemovedNests()
        {
            if (Game1.IsMasterGame)
            {
                foreach (var location in GetValidNestingLocations())
                {
                    foreach (var tree in location.GetTreesWithNests().Where(tree => tree.GetNest()?.Stage == NestStage.Removed))
                    {
                        tree.ClearNest();
                    }
                }
            }
        }

        public static void SetNest(Tree tree, Nest nest)
        {
            if (Game1.IsMasterGame)
            {
                tree.modData[KeyNest] = nest.ToString();
                nestCache.AddOrUpdate(tree, nest);
            }
        }
        public static Nest GetNest(Tree tree)
        {
            Nest nest;

            if (!nestCache.TryGetValue(tree, out nest))
            {
                if (tree.modData.TryGetValue(KeyNest, out var nestString) && Nest.TryParse(nestString, out nest))
                {
                    nestCache.AddOrUpdate(tree, nest);
                }
            }

            return nest;
        }
        public static void ClearNest(Tree tree)
        {
            if (Game1.IsMasterGame)
            {
                tree.modData.Remove(KeyNest);
                nestCache.Remove(tree);
            }
        }
    }
}

