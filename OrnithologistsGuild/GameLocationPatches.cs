using System;
using System.Threading.Channels;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using xTile;

namespace OrnithologistsGuild
{
    public class GameLocationPatches
    {
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor, Harmony harmony)
        {
            Monitor = monitor;

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.addBirdies)),
               prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(addBirdies_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.tryToAddCritters)),
               prefix: new HarmonyMethod(typeof(GameLocationPatches), nameof(tryToAddCritters_Prefix))
            );
        }

        public static bool addBirdies_Prefix(GameLocation __instance, double chance, bool onlyIfOnScreen = false)
        {
            try
            {
                return false; // don't run original logic -- we want to handle adding birdies ourself
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addBirdies_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool tryToAddCritters_Prefix(GameLocation __instance, bool onlyIfOnScreen = false)
        {
            try
            {
                // Copy logic from original game ...
                if (Game1.CurrentEvent != null)
                {
                    return true;
                }
                double mapArea = __instance.map.Layers[0].LayerWidth * __instance.map.Layers[0].LayerHeight;
                double baseChance = Math.Max(0.15, Math.Min(0.5, mapArea / 15000.0));

                // ... but allow spawning Birdies when it's raining ...
                if (__instance.IsRainingHere()) baseChance /= 2;

                double birdieChance = baseChance;

                // ... or on the Beach
                if (__instance.critters != null && __instance.critters.Count <= (__instance.IsSummerHere() ? 20 : 10))
                {
                    Monitor.Log($"{nameof(addBirdies_Prefix)}: chance={birdieChance} onlyIfOnScreen={onlyIfOnScreen}");

                    BetterBirdieSpawner.AddBirdies(__instance, birdieChance, !onlyIfOnScreen /* for some reason this is inverted in the original game code */);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(tryToAddCritters_Prefix)}:\n{ex}", LogLevel.Error);
            }

            return true; // run original logic
        }
    }
}

