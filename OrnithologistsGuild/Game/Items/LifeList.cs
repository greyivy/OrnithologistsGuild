using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using OrnithologistsGuild.Content;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace OrnithologistsGuild.Game.Items
{
    [XmlType("Mods_Ivy_OrnithologistsGuild_LifeList")]
    public class LifeList : CustomObject
    {
        private const int PAGE_SIZE = 5;
        private const string ACTION_NEXT = "ACTION_NEXT";

        public LifeList() : base((ObjectPackData)ModEntry.DGAContentPack.Find("LifeList"))
        {
            var dataPack = ModEntry.DGAContentPack.Find("LifeList");

            this.name = $"{dataPack.ID}_Subclass";
        }

        private void drawBirdieList(Models.LifeList lifeList, List<Response> choices, int page = 1)
        {
            var totalPages = (int)Math.Floor((double)lifeList.Count / (double)PAGE_SIZE);

            var title = $"{Game1.player.Name}'s Life List ({lifeList.IdentifiedCount}/{ContentPackManager.BirdieDefs.Count} birds)^Page {page} of {totalPages}";
            var action = new GameLocation.afterQuestionBehavior((_, choice) => {
                if (choice.Equals(ACTION_NEXT)) drawBirdieList(lifeList, choices, page + 1);
                else drawBirdieDialogue(ContentPackManager.BirdieDefs[choice], lifeList[choice]);
            });

            var pageChoices = choices.Skip(page * PAGE_SIZE).Take(PAGE_SIZE).ToList();
            if (page < totalPages) pageChoices.Add(new Response(ACTION_NEXT, "Next page"));

            Game1.currentLocation.createQuestionDialogue(title, pageChoices.ToArray(), action);
        }

        private void drawBirdieDialogue(BirdieDef birdieDef, Models.LifeListEntry lifeListEntry)
        {
            var id = birdieDef.ID;

            var contentPack = birdieDef.ContentPackDef.ContentPack;

            // Translations
            var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
            var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");
            var funFactString = contentPack.Translation.Get($"birdie.{id}.funFact");
            var attributeStrings = Enumerable.Range(1, birdieDef.Attributes).ToDictionary(i => i, i => contentPack.Translation.Get($"birdie.{id}.attribute.{i}"));

            var lines = new List<string>();

            lines.Add(commonNameString.ToString().ToUpper());
            if (scientificNameString.HasValue()) lines.Add(scientificNameString.ToString());

            lines.Add(string.Empty);
            foreach (var sighting in lifeListEntry.Sightings)
            {
                var dateSpotted = SDate.FromDaysSinceStart(lifeListEntry.Sightings.Last().DaysSinceStart);
                var location = Game1.getLocationFromName(lifeListEntry.Sightings.Last().LocationName).Name;

                var adj = lifeListEntry.Sightings.IndexOf(sighting) == lifeListEntry.Sightings.Count - 1 ? "Identified" : "Sighted";
                lines.Add($"{adj} {dateSpotted} ({location}): {attributeStrings[sighting.Attribute]}");
            }

            if (funFactString.HasValue())
            {
                lines.Add(string.Empty);
                lines.Add(funFactString);
            }

            Game1.drawObjectDialogue(string.Join("^", lines));
        }

        public override bool performUseAction(GameLocation location)
        {
            var lifeList = SaveDataManager.SaveData.LifeList;

            if (lifeList.IdentifiedCount > 0)
            {
                var identified = ContentPackManager.BirdieDefs.Values.Where(birdieDef => lifeList.ContainsKey(birdieDef.UniqueID) && lifeList[birdieDef.UniqueID].Identified).ToList();

                List<Response> choices = identified
                    .OrderBy(birdieDef =>
                    {
                        var id = birdieDef.ID;
                        var contentPack = birdieDef.ContentPackDef.ContentPack;

                        return contentPack.Translation.Get($"birdie.{id}.commonName").ToString();
                    }).Select(birdieDef =>
                    {
                        var id = birdieDef.ID;
                        var contentPack = birdieDef.ContentPackDef.ContentPack;

                        var commonNameString = contentPack.Translation.Get($"birdie.{id}.commonName");
                        var scientificNameString = contentPack.Translation.Get($"birdie.{id}.scientificName");

                        if (scientificNameString.HasValue()) return new Response(birdieDef.UniqueID, $"{commonNameString.ToString().ToUpper()} ({scientificNameString})");
                        else return new Response(birdieDef.UniqueID, commonNameString.ToString().ToUpper());
                    }).ToList();

                drawBirdieList(lifeList, choices, 1);
            }
            else
            {
                Game1.drawObjectDialogue($"- {Game1.player.Name}'s Life List (0/{ContentPackManager.BirdieDefs.Count}) -^empty^^Tip: binoculars can help you identify the birds around you.");
            }

            return false;
        }

        public override Item getOne()
        {
            var ret = new LifeList();
            ret.Quality = this.Quality;
            ret.Stack = 1;
            ret.Price = this.Price;
            ret.ObjectColor = this.ObjectColor;
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool canStackWith(ISalable other)
        {
            return false;
        }
    }
}
