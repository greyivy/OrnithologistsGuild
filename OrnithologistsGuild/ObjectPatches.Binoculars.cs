using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Game;
using OrnithologistsGuild.Game.Critters;
using OrnithologistsGuild.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using static StardewValley.FarmerRenderer;

namespace OrnithologistsGuild
{
    public partial class ObjectPatches {
        private const int ANIMATE_DURATION = 750;
        private static int? animateElapsed;
        private static string animateToolId;

        private static Lazy<Texture2D> binocularsTexture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>("Mods/Ivy.OrnithologistsGuild/Binoculars"));

        /// <summary>
        /// Draws the binoculars.
        /// </summary>
        public static void drawHairAndAccesories_Postfix(FarmerRenderer __instance, SpriteBatch b, int facingDirection, Farmer who, Vector2 position, Vector2 origin, float scale, int currentFrame, float rotation, Color overrideColor, float layerDepth)
        {
            try
            {
                if (who.CurrentItem is GenericTool binoculars && binoculars.IsBinoculars())
                {
                    FarmerSpriteLayers accessoryLayer = FarmerSpriteLayers.Accessory;
                    if (who.accessory.Value >= 0 && __instance.drawAccessoryBelowHair(who.accessory.Value))
                    {
                        accessoryLayer = FarmerSpriteLayers.AccessoryUnderHair;
                    }

                    var sourceRect = new Rectangle(binoculars.CurrentParentTileIndex * 16, 0, 16, 16);

                    if (Utilities.TryGetNonPublicFieldValue(__instance, "positionOffset", out Vector2 positionOffset) &&
                        Utilities.TryGetNonPublicFieldValue(__instance, "rotationAdjustment", out Vector2 rotationAdjustment))
                    {
                        switch (facingDirection)
                        {
                            case 0:
                                break;
                            case 1:
                                sourceRect.Offset(0, 16);
                                b.Draw(
                                    binocularsTexture.Value,
                                    position + origin + positionOffset + rotationAdjustment +
                                        new Vector2(
                                            featureXOffsetPerFrame[currentFrame] * 4,
                                            4 + featureYOffsetPerFrame[currentFrame] * 4 + __instance.heightOffset.Value + 20),
                                    sourceRect, overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, accessoryLayer));
                                break;
                            case 2:
                                b.Draw(
                                    binocularsTexture.Value,
                                    position + origin + positionOffset + rotationAdjustment +
                                        new Vector2(
                                            featureXOffsetPerFrame[currentFrame] * 4,
                                            8 + featureYOffsetPerFrame[currentFrame] * 4 + __instance.heightOffset.Value + 24),
                                    sourceRect, overrideColor, rotation, origin, 4f * scale, SpriteEffects.None, GetLayerDepth(layerDepth, accessoryLayer));
                                break;
                            case 3:
                                sourceRect.Offset(0, 16);
                                b.Draw(
                                    binocularsTexture.Value,
                                    position + origin + positionOffset + rotationAdjustment +
                                        new Vector2(
                                            -featureXOffsetPerFrame[currentFrame] * 4,
                                            4 + featureYOffsetPerFrame[currentFrame] * 4 + __instance.heightOffset.Value + 20),
                                    sourceRect, overrideColor, rotation, origin, 4f * scale, SpriteEffects.FlipHorizontally, GetLayerDepth(layerDepth, accessoryLayer));
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(drawHairAndAccesories_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Disallow run when binoculars are being held.
        /// </summary>
        public static void actionWhenBeingHeld_Postfix(Tool __instance, Farmer who)
        {
            try
            {
                if (__instance.IsBinoculars())
                {
                    who.setRunning(false);
                    who.canOnlyWalk = true;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(actionWhenBeingHeld_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        /// <summary>
        /// Allow run when binoculars are not being held.
        /// </summary>
        public static void actionWhenStopBeingHeld_Postfix(Tool __instance, Farmer who)
        {
            try
            {
                if (__instance.IsBinoculars())
                {
                    who.canOnlyWalk = false;
                    if (Game1.options.autoRun)
                    {
                        who.setRunning(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(actionWhenStopBeingHeld_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Draws the binocular range circle.
        /// </summary>
        public static void Farmer_draw_Postfix(Farmer __instance, SpriteBatch b)
        {
            try
            {
                if (__instance.CurrentTool?.IsBinoculars() == true)
                {
                    if (animateElapsed.HasValue)
                    {
                        var binocularsProperties = __instance.CurrentTool.GetBinocularsProperties();

                        animateElapsed += Game1.currentGameTime.ElapsedGameTime.Milliseconds;

                        if (animateToolId != __instance.CurrentTool?.QualifiedItemId)
                        {
                            // Animation stopped due to switching tools
                            animateElapsed = null;
                            animateToolId = null;
                        }
                        else if (animateElapsed <= ANIMATE_DURATION)
                        {
                            var factor = Utilities.EaseOutSine((float)animateElapsed / (float)ANIMATE_DURATION);

                            var animatedRange = Utility.Lerp(0, binocularsProperties.Range * Game1.tileSize, factor);
                            var opacity = Utility.Lerp(0.7f, 0.01f, factor);

                            MonoGame.Primitives2D.DrawCircle(
                                b,
                                __instance.getLocalPosition(Game1.viewport) + new Vector2(0.5f * Game1.tileSize, 0),
                                animatedRange,
                                (int)animatedRange / 4,
                                (Game1.season == Season.Winter ? Color.MidnightBlue : Color.AliceBlue) * opacity,
                                (Game1.tileSize / 16) * 2);
                        }
                        else
                        {
                            // Animation complete
                            animateElapsed = null;
                            animateToolId = null;

                            IdentifyBirdies(__instance.currentLocation, __instance, binocularsProperties);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(actionWhenBeingHeld_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static void UseBinoculars(Tool binoculars, GameLocation location, Farmer who)
        {
            if (
                location != null &&
                location.IsOutdoors &&
                !animateElapsed.HasValue)
            {
                if (binoculars.QualifiedItemId == Constants.BINOCULARS_JOJA_FQID) UseJojaBinoculars(who);
                else if (binoculars.QualifiedItemId == Constants.BINOCULARS_ANTIQUE_FQID) UseAntiqueBinoculars(binoculars, who);
                else UseBinoculars(who); // No special behavior
            }
        }
        private static void UseJojaBinoculars(Farmer who)
        {
            if (!ConfigManager.Config.NoBreakOrJam && Game1.random.NextDouble() < 0.1)
            {
                Game1.drawObjectDialogue(I18n.Items_JojaBinoculars_Message());
            }
            else UseBinoculars(who);
        }
        private static void UseAntiqueBinoculars(Tool binoculars, Farmer who)
        {
            if (!ConfigManager.Config.NoBreakOrJam && Game1.random.NextDouble() < 0.025)
            {
                Game1.drawObjectDialogue(I18n.Items_AntiqueBinoculars_Message());

                Game1.player.removeItemFromInventory(binoculars);
            }
            else UseBinoculars(who);
        }
        private static void UseBinoculars(Farmer who)
        {
            // Start binoculars animation
            animateElapsed = 0;
            animateToolId = who.CurrentTool?.QualifiedItemId;
        }

        private record Identification(Vector2 Position, BetterBirdie Birdie = null, Nest Nest = null);

        private static void IdentifyBirdies(GameLocation location, Farmer who, BinocularsProperties binocularsProperties)
        {
            if (location.critters == null) return;

            List<string> alreadyIdentified = new List<string>();
            List<string> newlyIdentified = new List<string>();

            var actualRange = (binocularsProperties.Range + 0.5) * Game1.tileSize;
            var midPoint = who.Position + new Vector2(0.5f * Game1.tileSize, -0.25f * Game1.tileSize);

            List<string> spottedBirdieUniqueIds = new List<string>();

            var treesWithNests = location.GetTreesWithNests()
                .Where(tree => Vector2.Distance(midPoint, tree.GetNestPosition()) <= actualRange)
                .Select(tree => new Identification(tree.GetNestPosition(), Nest: tree.GetNest()));
            var betterBirdies = location.critters
                .Where(critter => critter is BetterBirdie && Vector2.Distance(midPoint, critter.position) <= actualRange)
                .Select(betterBirdie => new Identification(betterBirdie.position, Birdie: (BetterBirdie)betterBirdie));

            var closestIdentifications = Enumerable
                .Concat(treesWithNests, betterBirdies)
                .OrderBy(identification => Vector2.Distance(midPoint, identification.Position));

            foreach (var identification in closestIdentifications)
            {
                var nest = identification.Nest ?? identification.Birdie.Perch?.Tree?.GetNest();

                if (nest != null && nest.Stage != NestStage.Removed)
                {
                    if (NestManager.IsNestAtPositionSpotted(identification.Position)) continue;

                    var birdieDef = nest.Owner;
                    var id = birdieDef.ID;

                    var lines = new List<string>();

                    var isIdentified = SaveDataManager.SaveData.ForPlayer(Game1.player.UniqueMultiplayerID).LifeList.TryGetValue(birdieDef.UniqueID, out var lifeListEntry) && lifeListEntry.Identified;

                    var idPart = isIdentified ? I18n.Items_Binoculars_NestId() : I18n.Items_Binoculars_NestNoId();
                    if (nest.Stage == NestStage.Built) lines.Add($"{I18n.Items_Binoculars_NestStateBuilt()} {idPart}");
                    else if (nest.Stage == NestStage.EggsLaid) lines.Add($"{I18n.Items_Binoculars_NestStateEggsLaid()} {idPart}");
                    else if (nest.Stage == NestStage.EggsHatched) lines.Add($"{I18n.Items_Binoculars_NestStateEggsHatched()} {idPart}");
                    else if (nest.Stage == NestStage.Fledged) lines.Add($"{I18n.Items_Binoculars_NestStateFledged()} {idPart}");

                    if (isIdentified) {
                        // Birdie identified
                        var contentPack = birdieDef.ContentPackDef.ContentPack;
                        var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
                        var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");
                        var funFactString = contentPack.Translation.Get($"birdie.{id}.funFact");

                        lines.Add(string.Empty);

                        lines.Add(Utilities.LocaleToUpper(commonNameString.ToString()));
                        if (scientificNameString.HasValue()) lines.Add(scientificNameString.ToString());

                        if (funFactString.HasValue())
                        {
                            lines.Add(string.Empty);
                            lines.Add(I18n.Items_Binoculars_LifeList());
                        }
                    }

                    Game1.drawObjectDialogue(string.Join("^", lines));

                    // Ignore the nest on consecutive uses of the binoculars
                    NestManager.SpottedNestAtPosition(identification.Position);

                    break;
                }
                else if (identification.Birdie != null)
                {
                    var birdie = identification.Birdie;
                    var id = birdie.BirdieDef.ID;

                    if (birdie.IsFlying || birdie.IsSpotted) continue;

                    // Still = 30% scare chance, walking = 60% scare chance
                    var scareChance = 0.3f + ((who.getMostRecentMovementVector().Length() / 2f) * 0.3f);
                    if (Game1.random.NextDouble() < Utility.Clamp(scareChance, 0.3f, 0.6f))
                    {
                        birdie.Frighten();
                        Game1.drawObjectDialogue(I18n.Items_Binoculars_Frighten());

                        break;
                    }

                    var sighting = SaveDataManager.SaveData.ForPlayer(Game1.player.UniqueMultiplayerID)
                        .LifeList.GetOrAddEntry(birdie.BirdieDef, out var newAttribute, out var existingAttribute);

                    var contentPack = birdie.BirdieDef.ContentPackDef.ContentPack;

                    // Translations
                    var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
                    var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");
                    var funFactString = contentPack.Translation.Get($"birdie.{id}.funFact");
                    var attributeStrings = Enumerable.Range(1, birdie.BirdieDef.Attributes).ToDictionary(i => i,
                        i => contentPack.Translation.Get($"birdie.{id}.attribute.{i}"));

                    var lines = new List<string>();

                    if (sighting.Identified)
                    {
                        lines.Add(newAttribute.HasValue ? I18n.Items_Binoculars_NewlyIdentified() : I18n.Items_Binoculars_AlreadyIdentified());
                        lines.Add(string.Empty);

                        lines.Add(Utilities.LocaleToUpper(commonNameString.ToString()));
                        if (scientificNameString.HasValue()) lines.Add(scientificNameString.ToString());

                        if (newAttribute.HasValue)
                        {
                            lines.Add(string.Empty);
                            lines.Add(string.Join(Utilities.GetLocaleSeparator(), attributeStrings.Values));
                        }

                        if (funFactString.HasValue())
                        {
                            lines.Add(string.Empty);
                            lines.Add(I18n.Items_Binoculars_LifeList());
                        }
                    }
                    else
                    {
                        lines.Add(I18n.Items_Binoculars_NotYetIdentified());
                        lines.Add(string.Empty);
                        lines.Add(string.Join(Utilities.GetLocaleSeparator(), attributeStrings.Select(a =>
                            sighting.Sightings.Select(s => s.Attribute).Contains(a.Key) ? a.Value : I18n.Placeholder())));

                        if (existingAttribute.HasValue)
                        {
                            lines.Add(string.Empty);
                            lines.Add(I18n.Items_Binoculars_AlreadySightedAtDateAndLocation());
                        }
                    }

                    Game1.drawObjectDialogue(string.Join("^", lines));

                    // Ignore the birds on consecutive uses of the binoculars
                    birdie.IsSpotted = true;

                    break;
                }

                //else if (critter is Woodpecker && Vector2.Distance(midPoint, critter.position) <= actualRange)
                //{

                //}
                //else if (critter is Seagull && Vector2.Distance(midPoint, critter.position) <= actualRange)
                //{

                //}
                //else if (critter is Crow && Vector2.Distance(midPoint, critter.position) <= actualRange)
                //{

                //}
                //else if (critter is Owl)
                //{

                //}
                // ... other critter types? Bird? PerchingBird?
            }
        }
    }
}
