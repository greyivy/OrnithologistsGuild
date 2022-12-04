using System;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Game;
using StardewModdingAPI;
using StardewValley;

namespace OrnithologistsGuild
{
    public class TreePatches
    {
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool performUseAction_Prefix(StardewValley.TerrainFeatures.Tree __instance, Vector2 tileLocation, GameLocation location)
        {
            try
            {
                Monitor.Log($"performUseAction_Prefix");

                var birdie = new Perch(__instance).GetOccupant(location);
                if (birdie != null)
                {
                    birdie.Frighten();
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performUseAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}

