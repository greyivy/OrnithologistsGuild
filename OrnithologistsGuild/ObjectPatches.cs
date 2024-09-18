using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Models;
using StardewModdingAPI;
using StardewValley;

namespace OrnithologistsGuild
{
	public partial class ObjectPatches
	{
        private static IMonitor Monitor;

        public static void Initialize(IMonitor monitor, Harmony harmony)
        {
            Monitor = monitor;

            // Shared
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Tool), nameof(StardewValley.Tool.DoFunction)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(DoFunction_Postfix))
            );

            // Binoculars
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.FarmerRenderer), nameof(StardewValley.FarmerRenderer.drawHairAndAccesories)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(drawHairAndAccesories_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.pressUseToolButton)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(pressUseToolButton_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Tool), nameof(StardewValley.Tool.actionWhenBeingHeld)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(actionWhenBeingHeld_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Tool), nameof(StardewValley.Tool.actionWhenStopBeingHeld)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(actionWhenStopBeingHeld_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Farmer), nameof(StardewValley.Farmer.draw), new Type[] { typeof(SpriteBatch) }),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(Farmer_draw_Postfix))
            );

            // Life List
            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Menus.LetterViewerMenu), nameof(StardewValley.Menus.LetterViewerMenu.draw), new Type[] { typeof(SpriteBatch) }),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(LetterViewerMenu_draw_Postfix))
            );
            harmony.Patch(
              original: AccessTools.Method(typeof(StardewValley.BellsAndWhistles.SpriteText), nameof(StardewValley.BellsAndWhistles.SpriteText.getStringBrokenIntoSectionsOfHeight)),
              prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(getStringBrokenIntoSectionsOfHeight_Prefix))
            );
            
        }

        /// <summary>
        /// Allows <see cref="DoFunction_Postfix"/> to function when <see cref="Farmer.canOnlyWalk"/> is false.
        /// </summary>
        public static void pressUseToolButton_Postfix(ref bool __result)
        {
            try
            {
                if (__result && Game1.player.canOnlyWalk &&
                    Game1.player.CurrentTool != null &&
                    Game1.player.CurrentTool.IsBinoculars())
                {
                    Game1.player.CurrentTool.DoFunction(Game1.player.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, 1, Game1.player);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(pressUseToolButton_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void DoFunction_Postfix(Tool __instance, GameLocation location, int x, int y, int power, Farmer who)
        {
            try
            {
                if (__instance.IsBinoculars()) UseBinoculars(__instance, location, who);
                else if (__instance.QualifiedItemId == Constants.LIFE_LIST_FQID) UseLifeList(who);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DoFunction_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
