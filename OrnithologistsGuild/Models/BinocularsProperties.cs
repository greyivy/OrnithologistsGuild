using System.Collections.Generic;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public record BinocularsProperties
    {
        public const string CONTEXT_TAG = "Ivy_OrnithologistsGuild_Binoculars";

        private const string PROPERTY_RANGE = "Ivy_OrnithologistsGuild_Range";

        public int Range { get; private set; }

        public BinocularsProperties(Object binoculars)
        {
            if (DataLoader.Objects(Game1.content).TryGetValue(binoculars.ItemId, out var objectData))
            {
                if (objectData.CustomFields.TryGetValue(PROPERTY_RANGE, out string range)) {
                    Range = int.Parse(range);
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_RANGE} custom field", nameof(binoculars));
                }
            } else {
                throw new System.ArgumentException("Must contain custom fields", nameof(binoculars));
            }
        }
    }

    public static class ObjectBinocularsPropertiesExtensions
    {
        private static Dictionary<string, BinocularsProperties> cachedBinocularsProperties = new Dictionary<string, BinocularsProperties>();

        public static BinocularsProperties GetBinocularsProperties(this Object binoculars)
        {
            if (!IsBinoculars(binoculars)) return null;

            BinocularsProperties binocularsProperties;
            if (!cachedBinocularsProperties.TryGetValue(binoculars.QualifiedItemId, out binocularsProperties))
            {
                binocularsProperties = new BinocularsProperties(binoculars);
                cachedBinocularsProperties[binoculars.QualifiedItemId] = binocularsProperties;
            }

            return binocularsProperties;
        }

        public static bool IsBinoculars(this Object maybeBinoculars) => maybeBinoculars.HasContextTag(BinocularsProperties.CONTEXT_TAG);
    }
}
