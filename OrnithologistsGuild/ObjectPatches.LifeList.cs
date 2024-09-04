using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Content;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace OrnithologistsGuild
{
    public partial class ObjectPatches
    {
        private const string ID_LIFE_LIST = "(T)Ivy_OrnithologistsGuild_LifeList";
        private const string PAGE_BREAK_TOKEN = "%Ivy.OrnithologistsGuild.PageBreak%";
        private const string HEADER_PATTERN = @"^\((?'index'\d+)\/\d+\)";

        private static List<AnimatedSprite> pageBirdieSprites = new List<AnimatedSprite>();

        private static string PluralizeBird(int count) => count == 1 ? I18n.Bird() : I18n.Birds();

        private static void UseLifeList()
        {
            var lifeList = SaveDataManager.SaveData.ForPlayer(Game1.player.UniqueMultiplayerID).LifeList;

            var sighted = ContentPackManager.BirdieDefs.Values
                .Select(birdieDef =>
                {
                    if (lifeList.TryGetValue(birdieDef.UniqueID, out var lifeListEntry))
                    {
                        return (BirdieDef: birdieDef, LifeListEntry: lifeListEntry);
                    }
                    return (BirdieDef: birdieDef, LifeListEntry: null);
                })
                .Where(tuple => tuple.LifeListEntry != null)
                .OrderBy(tuple => tuple.LifeListEntry.Sightings.First().DaysSinceStart)
                .ToList();

            var identified = sighted.Where(tuple => tuple.LifeListEntry.Identified).ToList();

            var remainingCount = ContentPackManager.BirdieDefs.Values.Count - identified.Count;

            var bestLocation = identified
                .GroupBy(tuple => tuple.LifeListEntry.Sightings.Last().LocationName)
                .OrderByDescending(group => group.Count())
                .FirstOrDefault();
            var bestSeason = identified
                .GroupBy(tuple =>
                    Utility.getSeasonNameFromNumber(SDate.FromDaysSinceStart(tuple.LifeListEntry.Sightings.Last().DaysSinceStart).SeasonIndex))
                .OrderByDescending(group => group.Count())
                .FirstOrDefault();
            var bestYear = identified
                .GroupBy(tuple => SDate.FromDaysSinceStart(tuple.LifeListEntry.Sightings.Last().DaysSinceStart).Year)
                .OrderByDescending(group => group.Count())
                .FirstOrDefault();

            var pages = new List<string>();

            // Add cover page
            var coverPageLines = new List<string>();
            coverPageLines.Add(Utilities.LocaleToUpper(I18n.Items_LifeList_Title(Game1.player.Name)));
            coverPageLines.Add(string.Empty);

            if (sighted.Count == 0)
            {
                coverPageLines.Add(I18n.Items_LifeList_Empty());
                coverPageLines.Add(string.Empty);
                coverPageLines.Add(I18n.Items_LifeList_EmptyTip());
            }
            else
            {
                coverPageLines.Add(I18n.Items_LifeList_TotalSighted(sighted.Count, PluralizeBird(sighted.Count)));
                coverPageLines.Add(I18n.Items_LifeList_TotalIdentified(identified.Count, PluralizeBird(identified.Count)));
                coverPageLines.Add(I18n.Items_LifeList_TotalUnidentified(remainingCount, PluralizeBird(remainingCount)));
                coverPageLines.Add(string.Empty);
                coverPageLines.Add(I18n.Items_LifeList_BestLocation(bestLocation.Key, bestLocation.Count(), PluralizeBird(bestLocation.Count())));
                coverPageLines.Add(I18n.Items_LifeList_BestSeason(bestSeason.Key, bestSeason.Count(), PluralizeBird(bestSeason.Count())));
                coverPageLines.Add(I18n.Items_LifeList_BestYear(bestYear.Key, bestYear.Count(), PluralizeBird(bestYear.Count())));
            }

            pages.Add(string.Join("^", coverPageLines));

            // Add individual birdie pages
            pages.AddRange(sighted.Select((tuple, i) => GetBirdiePage(i, sighted.Count, tuple.BirdieDef, tuple.LifeListEntry)));

            // Create birdie sprites
            pageBirdieSprites.AddRange(sighted.Select(tuple =>
            {
                var internalAssetName = tuple.BirdieDef.ContentPackDef.ContentPack.ModContent.GetInternalAssetName(tuple.BirdieDef.AssetPath).BaseName;
                var birdieSprite = new AnimatedSprite(internalAssetName, tuple.BirdieDef.BaseFrame, 32, 32);
                if (!tuple.LifeListEntry.Identified)
                {
                    birdieSprite.spriteTexture = Utilities.CensorTexture(birdieSprite.Texture);
                }

                var flyingAnimation = new List<FarmerSprite.AnimationFrame> {
                    new FarmerSprite.AnimationFrame (tuple.BirdieDef.BaseFrame + 6, (int)MathF.Round(0.27f * tuple.BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (tuple.BirdieDef.BaseFrame + 7, (int)MathF.Round(0.23f * tuple.BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (tuple.BirdieDef.BaseFrame + 8, (int)MathF.Round(0.27f * tuple.BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (tuple.BirdieDef.BaseFrame + 7, (int)MathF.Round(0.23f * tuple.BirdieDef.FlapDuration))
                };
                birdieSprite.setCurrentAnimation(flyingAnimation);
                birdieSprite.loop = true;

                return birdieSprite;
            }));

            var letter = new LetterViewerMenu(string.Join(PAGE_BREAK_TOKEN, pages));
            letter.whichBG = 1;
            letter.exitFunction = () => pageBirdieSprites.Clear();
            Game1.activeClickableMenu = letter;
        }

        private static string GetBirdiePage(int index, int total, BirdieDef birdieDef, Models.LifeListEntry lifeListEntry)
        {
            var id = birdieDef.ID;

            var contentPack = birdieDef.ContentPackDef.ContentPack;

            // Translations
            var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
            var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");
            var funFactString = contentPack.Translation.Get($"birdie.{id}.funFact");
            var attributeStrings = Enumerable
                .Range(1, birdieDef.Attributes)
                .ToDictionary(i => i, i => lifeListEntry.Sightings.Any(sighting => sighting.Attribute == i) ?
                    contentPack.Translation.Get($"birdie.{id}.attribute.{i}") :
                    I18n.Placeholder());

            var lines = new List<string>();

            var header = $"({index + 1}/{total})";
            var name = lifeListEntry.Identified ? commonNameString : I18n.Placeholder();
            lines.Add(Utilities.LocaleToUpper($"{header} {name}"));

            lines.Add(lifeListEntry.Identified && scientificNameString.HasValue() ? scientificNameString.ToString() : string.Empty);
            lines.Add(string.Empty);

            lines.Add(I18n.Items_LifeList_Sightings());
            foreach (var sighting in lifeListEntry.Sightings)
            {
                var date = SDate.FromDaysSinceStart(sighting.DaysSinceStart);
                var location = Game1.getLocationFromName(sighting.LocationName);
                var locationName = location == null ? sighting.LocationName : location.Name;
                var attribute = attributeStrings[sighting.Attribute];

                lines.Add(I18n.Items_LifeList_Sighting(date.ToLocaleString(), locationName, attribute));
            }

            if (lifeListEntry.Identified && funFactString.HasValue())
            {
                lines.Add(string.Empty);
                lines.Add(funFactString);
            }

            return string.Join('^', lines);
        }

        private static void LetterViewerMenu_draw_Postfix(LetterViewerMenu __instance, SpriteBatch b)
        {
            try
            {
                if (__instance.scale == 1f && pageBirdieSprites.Count > 0)
                {
                    var page = __instance.mailMessage[__instance.page];
                    var headerMatch = Regex.Match(page, HEADER_PATTERN);

                    if (headerMatch.Success && int.TryParse(headerMatch.Groups["index"].Value, out var indexPlusOne))
                    {
                        var birdieSprite = pageBirdieSprites[indexPlusOne - 1];

                        var scale = 5;
                        birdieSprite.animateOnce(Game1.currentGameTime);

                        // Bob up and down at most 24px
                        var offsetY = MathF.Sin((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1250f) * 24f;
                        var offsetX = MathF.Sin((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 2000f) * 24f;
                        birdieSprite.draw(b, new Vector2(__instance.xPositionOnScreen + __instance.width - (birdieSprite.SpriteWidth * scale) - 36 + offsetX, __instance.yPositionOnScreen + 36 + offsetY), 1f, 0, 0, Color.White, scale: 5);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(LetterViewerMenu_draw_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool getStringBrokenIntoSectionsOfHeight_Prefix(string s, int width, int height, ref List<string> __result)
        {
            try
            {
                if (s.Contains(PAGE_BREAK_TOKEN))
                {
                    __result = s
                        .Split(PAGE_BREAK_TOKEN)
                        .SelectMany(page => SpriteText.getStringBrokenIntoSectionsOfHeight(page, width, height))
                        .ToList();

                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(getStringBrokenIntoSectionsOfHeight_Prefix)}:\n{ex}", LogLevel.Error);
            }

            return true;
        }
    }
}
