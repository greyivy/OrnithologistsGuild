using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using OrnithologistsGuild.Game.Critters;
using OrnithologistsGuild.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace OrnithologistsGuild
{
	public partial class ObjectPatches
	{
        private const string ID_JOJA_BINOCULARS = "(O)Ivy_OrnithologistsGuild_JojaBinoculars";
        private const string ID_ANTIQUE_BINOCULARS = "(O)Ivy_OrnithologistsGuild_AntiqueBinoculars";
        private const string ID_PRO_BINOCULARS = "(O)Ivy_OrnithologistsGuild_ProBinoculars";

        private static readonly int AnimateDuration = 750;
        private static int? AnimateElapsed;

        public static void actionWhenBeingHeld_Postfix(StardewValley.Object __instance, Farmer who)
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
        public static void actionWhenStopBeingHeld_Postfix(StardewValley.Object __instance, Farmer who)
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

        public static void drawWhenHeld_Postfix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            try
            {
                if (__instance.IsBinoculars())
                {
                    if (AnimateElapsed.HasValue)
                    {
                        var binocularsProperties = __instance.GetBinocularsProperties();

                        AnimateElapsed += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                        if (AnimateElapsed <= AnimateDuration)
                        {
                            var factor = Utilities.EaseOutSine(((float)AnimateElapsed.Value / (float)AnimateDuration));

                            var animatedRange = Utility.Lerp(0, binocularsProperties.Range * Game1.tileSize, factor);
                            var opacity = Utility.Lerp(0.7f, 0.1f, factor);

                            MonoGame.Primitives2D.DrawCircle(
                                spriteBatch,
                                objectPosition + new Vector2(0.5f * Game1.tileSize, 1.5f * Game1.tileSize),
                                animatedRange,
                                (int)animatedRange / 4,
                                Color.AliceBlue * opacity,
                                (Game1.tileSize / 16) * 2);
                        }
                        else
                        {
                            // Animation complete
                            AnimateElapsed = null;

                            IdentifyBirdies(f.currentLocation, f, binocularsProperties);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(actionWhenBeingHeld_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        private static void UseBinoculars(StardewValley.Object binoculars, GameLocation location, out bool removeFromInventory)
        {
            removeFromInventory = false;

            if (
                location != null &&
                location.IsOutdoors &&
                !AnimateElapsed.HasValue)
            {
                if (binoculars.QualifiedItemId == ID_JOJA_BINOCULARS) UseJojaBinoculars();
                else if (binoculars.QualifiedItemId == ID_ANTIQUE_BINOCULARS) UseAntiqueBinoculars(binoculars, out removeFromInventory);
                else if (binoculars.QualifiedItemId == ID_PRO_BINOCULARS) UseProBinoculars();
            }
        }

        private static void UseJojaBinoculars()
        {
            if (!ConfigManager.Config.NoBreakOrJam && Game1.random.NextDouble() < 0.1)
            {
                Game1.drawObjectDialogue(I18n.Items_JojaBinoculars_Message());
            }
            else
            {
                // Start binoculars animation
                AnimateElapsed = 0;
            }
        }

        private static void UseAntiqueBinoculars(StardewValley.Object binoculars, out bool removeFromInventory)
        {
            if (!ConfigManager.Config.NoBreakOrJam && Game1.random.NextDouble() < 0.025)
            {
                Game1.drawObjectDialogue(I18n.Items_AntiqueBinoculars_Message());

                removeFromInventory = true;
            }
            else
            {
                // Start binoculars animation
                AnimateElapsed = 0;

                removeFromInventory = false;
            }
        }

        private static void UseProBinoculars()
        {
            // Start binoculars animation
            AnimateElapsed = 0;
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
                    var birdieDef = nest.Owner;
                    var id = birdieDef.ID;

                    var lines = new List<string>();

                    if (SaveDataManager.SaveData.ForPlayer(Game1.player.UniqueMultiplayerID).LifeList.TryGetValue(birdieDef.UniqueID, out var lifeListEntry) && lifeListEntry.Identified) {
                        // Birdie identified
                        var contentPack = birdieDef.ContentPackDef.ContentPack;
                        var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
                        var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");

                        lines.Add(I18n.Items_Binoculars_NestId());
                        lines.Add(string.Empty);

                        lines.Add(Utilities.LocaleToUpper(commonNameString.ToString()));
                        if (scientificNameString.HasValue()) lines.Add(scientificNameString.ToString());
                    }
                    else
                    {
                        // Birdie not identified
                        lines.Add(I18n.Items_Binoculars_NestNoId());
                    }

                    lines.Add(string.Empty);
                    if (nest.Stage == NestStage.Built) lines.Add(I18n.Items_Binoculars_NestStateBuilt());
                    else if (nest.Stage == NestStage.EggsLaid) lines.Add(I18n.Items_Binoculars_NestStateEggsLaid());
                    else if (nest.Stage == NestStage.EggsHatched) lines.Add(I18n.Items_Binoculars_NestStateEggsHatched());
                    else if (nest.Stage == NestStage.Fledged) lines.Add(I18n.Items_Binoculars_NestStateFledged());

                    Game1.drawObjectDialogue(string.Join("^", lines));
                    break;
                }
                else
                {
                    var birdie = identification.Birdie;
                    var id = birdie.BirdieDef.ID;

                    if (birdie.IsFlying || birdie.IsSpotted) continue;
                    if (spottedBirdieUniqueIds.Contains(birdie.BirdieDef.UniqueID))
                    {
                        birdie.IsSpotted = true;
                        continue;
                    }

                    if (Game1.random.NextDouble() < 0.25)
                    {
                        birdie.Frighten();
                        Game1.drawObjectDialogue(I18n.Items_Binoculars_Frighten());
                    }

                    int? newAttribute;
                    var sighting = SaveDataManager.SaveData.ForPlayer(Game1.player.UniqueMultiplayerID).LifeList.GetOrAddEntry(birdie.BirdieDef, out newAttribute);

                    var contentPack = birdie.BirdieDef.ContentPackDef.ContentPack;

                    // Translations
                    var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
                    var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");
                    var funFactString = contentPack.Translation.Get($"birdie.{id}.funFact");
                    var attributeStrings = Enumerable.Range(1, birdie.BirdieDef.Attributes).ToDictionary(i => i, i => contentPack.Translation.Get($"birdie.{id}.attribute.{i}"));

                    var lines = new List<string>();

                    if (sighting.Identified)
                    {
                        lines.Add(newAttribute.HasValue ? I18n.Items_Binoculars_NewlyIdentified() : I18n.Items_Binoculars_AlreadyIdentified());

                        lines.Add(Utilities.LocaleToUpper(commonNameString.ToString()));
                        if (scientificNameString.HasValue()) lines.Add(scientificNameString.ToString());

                        if (newAttribute.HasValue)
                        {
                            lines.Add(string.Empty);
                            lines.Add(string.Join(Utilities.GetLocaleSeparator(), attributeStrings.Values));

                            if (funFactString.HasValue())
                            {
                                lines.Add(string.Empty);
                                lines.Add(funFactString);
                            }
                        }
                    }
                    else
                    {
                        lines.Add(I18n.Items_Binoculars_NotYetIdentified());
                        lines.Add(I18n.Items_Binoculars_Placeholder());
                        lines.Add(I18n.Items_Binoculars_Placeholder());
                        lines.Add(string.Empty);
                        lines.Add(string.Join(Utilities.GetLocaleSeparator(), attributeStrings.Select(a => sighting.Sightings.Select(s => s.Attribute).Contains(a.Key) ? a.Value : I18n.Items_Binoculars_Placeholder())));
                    }

                    Game1.drawObjectDialogue(string.Join("^", lines));

                    // Ignore the birds on consecutive uses of the binoculars
                    birdie.IsSpotted = true;
                    spottedBirdieUniqueIds.Add(birdie.BirdieDef.UniqueID);

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
