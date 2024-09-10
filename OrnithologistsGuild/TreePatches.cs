using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Game;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

namespace OrnithologistsGuild
{
    public class TreePatches
    {
        private static IMonitor Monitor;

        private static ConditionalWeakTable<Tree, AnimatedSprite> nestSpriteCache = new ConditionalWeakTable<Tree, AnimatedSprite>();

        private static List<FarmerSprite.AnimationFrame> EggsHatchedAnimation = new List<FarmerSprite.AnimationFrame> {
            new FarmerSprite.AnimationFrame (2, 300),
            new FarmerSprite.AnimationFrame (3, 300),
            new FarmerSprite.AnimationFrame (4, 300),
            new FarmerSprite.AnimationFrame (5, 300),
        };

        public static void Initialize(IMonitor monitor, Harmony harmony)
        {
            Monitor = monitor;

            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.performUseAction)),
                postfix: new HarmonyMethod(typeof(TreePatches), nameof(performUseAction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.draw)),
                postfix: new HarmonyMethod(typeof(TreePatches), nameof(draw_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.performToolAction)),
                postfix: new HarmonyMethod(typeof(TreePatches), nameof(performToolAction_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.tickUpdate)),
                postfix: new HarmonyMethod(typeof(TreePatches), nameof(tickUpdate_Postfix))
            );
        }

        public static void performUseAction_Postfix(Tree __instance, Vector2 tileLocation)
        {
            try
            {
                var birdie = new Perch(__instance).GetOccupant(__instance.Location);
                if (birdie != null)
                {
                    birdie.Frighten();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performUseAction_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void tickUpdate_Postfix(Tree __instance, GameTime time)
        {
            try
            {
                var nest = __instance.GetNest();
                var hasSprite = nestSpriteCache.TryGetValue(__instance, out var sprite);

                if (nest == null || nest.Stage == NestStage.Removed && hasSprite)
                {
                    // Nest removed -- remove sprite
                    sprite = null;
                    nestSpriteCache.Remove(__instance);
                }
                else if (nest != null && !hasSprite)
                {
                    // Nest added -- attach sprite
                    sprite = new AnimatedSprite("Mods/Ivy.OrnithologistsGuild/Nest", 0, 16, 16);
                    nestSpriteCache.AddOrUpdate(__instance, sprite);
                }

                if (sprite != null)
                {
                    if (nest.Stage == NestStage.EggsLaid)
                    {
                        sprite.ClearAnimation();
                        sprite.CurrentFrame = 1;
                    }
                    else if (nest.Stage == NestStage.EggsHatched)
                    {
                        if (sprite.CurrentAnimation == null)
                        {
                            sprite.setCurrentAnimation(EggsHatchedAnimation);
                            sprite.loop = true;
                        }
                    }
                    else
                    {
                        sprite.ClearAnimation();
                        sprite.currentFrame = 0;
                    }

                    sprite.animateOnce(time);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(tickUpdate_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void draw_Postfix(Tree __instance, SpriteBatch spriteBatch)
        {
            try
            {
                if (nestSpriteCache.TryGetValue(__instance, out var sprite))
                {  
                    var baseSortPosition = __instance.getBoundingBox().Bottom;

                    var screenPosition = Game1.GlobalToLocal(Game1.viewport, __instance.GetNestPosition()) + new Vector2(__instance.shakeRotation * 25, 0);
                    var sourceRectangle = new Rectangle(sprite.sourceRect.X, sprite.sourceRect.Y, sprite.sourceRect.Width, sprite.sourceRect.Height);

                    var spriteEffects = (sprite.CurrentAnimation != null && sprite.CurrentAnimation[sprite.currentAnimationIndex].flip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    spriteBatch.Draw(
                        sprite.Texture,
                        screenPosition,
                        sourceRectangle,
                        Color.White * __instance.alpha,
                        -__instance.shakeRotation,
                        Vector2.Zero,
                        4f,
                        spriteEffects,
                        baseSortPosition / 10000f + 0.01f);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(draw_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void performToolAction_Postfix(Tree __instance, Tool t, int explosion, Vector2 tileLocation)
        {
            try
            {
                if (t is Axe && __instance.HasNest())
                {
                    __instance.ClearNest();

                    Game1.createItemDebris(ItemRegistry.Create(Constants.NEST_FQID, 1), __instance.GetNestPosition(), Game1.recentMultiplayerRandom.Next(1) == 0 ? 1 : 3, __instance.Location);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(performToolAction_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
