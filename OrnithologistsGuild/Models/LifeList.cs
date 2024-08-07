﻿using System.Collections.Generic;
using System.Linq;
using OrnithologistsGuild.Content;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;

namespace OrnithologistsGuild.Models
{
    public class LifeList: Dictionary<string, LifeListEntry>
    {
        public int IdentifiedCount { get
            {
                return Values.Where(v => v.Identified).Count();
            } }

        public LifeListEntry GetOrAddEntry(BirdieDef birdieDef, out int? newAttribute, out int? latestAttribute)
        {
            newAttribute = null;
            latestAttribute = null;

            LifeListEntry lifeListEntry;
            if (!TryGetValue(birdieDef.UniqueID, out lifeListEntry))
            {
                lifeListEntry = new LifeListEntry();
                Add(birdieDef.UniqueID, lifeListEntry);
            }

            if (lifeListEntry.Identified) return lifeListEntry; // Already added

            var existingSightingAtDateAndLocation = lifeListEntry.Sightings.FirstOrDefault(sighting =>
                sighting.DaysSinceStart == SDate.From(Game1.Date).DaysSinceStart &&
                sighting.LocationName.Equals(Game1.player.currentLocation.Name));
            if (existingSightingAtDateAndLocation != null) { // Already sighted at this day / location
                latestAttribute = existingSightingAtDateAndLocation.Attribute;
                return lifeListEntry;
            }

            var attributes = Enumerable.Range(1, birdieDef.Attributes);
            var undiscoveredAttributes = attributes.Except(lifeListEntry.Sightings.Select(logEntry => logEntry.Attribute)).ToList();
            if (undiscoveredAttributes.Count == 1)
            {
                lifeListEntry.Identified = true;
            }

            newAttribute = Game1.random.ChooseFrom(undiscoveredAttributes);
            lifeListEntry.AddSighting(newAttribute.Value);

            SaveDataManager.Save();

            return lifeListEntry;
        }
    }

    public class LifeListEntry
    {
        public string ID;

        public bool Identified;
        public List<LifeListSighting> Sightings;

        public LifeListEntry()
        {
            Sightings = new List<LifeListSighting>();
        }

        public void AddSighting(int attribute)
        {
            Sightings.Add(new LifeListSighting()
            {
                DaysSinceStart = SDate.From(Game1.Date).DaysSinceStart,
                TimeOfDay = Game1.timeOfDay,

                LocationName = Game1.player.currentLocation.Name,

                Attribute = attribute
            });
        }
    }

    public class LifeListSighting
    {
        public int DaysSinceStart;
        public int TimeOfDay;

        public string LocationName;

        public int Attribute;
    }
}
