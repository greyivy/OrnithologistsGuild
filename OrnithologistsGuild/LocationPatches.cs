using System;
using StardewModdingAPI;

namespace OrnithologistsGuild
{
    public class LocationPatches
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
                Monitor.Log($"addBirdies_Prefix {chance.ToString()} {onlyIfOnScreen.ToString()}");

                BetterBirdieSpawner.AddBirdies(__instance, chance, onlyIfOnScreen);

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

