using System;
using StardewModdingAPI;

namespace OrnithologistsGuild
{
    public class GameLocationPatches
    {
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool addBirdies_Prefix(StardewValley.GameLocation __instance, double chance, bool onlyIfOnScreen = false)
        {
            try
            {
                Monitor.Log($"{nameof(addBirdies_Prefix)}: chance={chance.ToString()} onlyIfOnScreen={onlyIfOnScreen.ToString()}");

                BetterBirdieSpawner.AddBirdies(__instance, chance, !onlyIfOnScreen /* for some reason this is inverted in the original game code */);

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addBirdies_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}

