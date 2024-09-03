using System.Collections.Generic;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public record BinocularsProperties
    {
        public const string TAG = "Ivy_OrnithologistsGuild_Binoculars";

        private const string PROPERTY_RANGE = "Ivy_OrnithologistsGuild_Range";

        public int Range { get; private set; }

        public BinocularsProperties(Tool binoculars)
        {
            if (binoculars.modData.TryGetValue(PROPERTY_RANGE, out string range)) {
                Range = int.Parse(range);
            } else {
                throw new System.ArgumentException($"Must contain {PROPERTY_RANGE} custom field", nameof(binoculars));
            }
        }
    }

    public static class ToolBinocularsPropertiesExtensions
    {
        private static Dictionary<string, BinocularsProperties> cachedBinocularsProperties = new Dictionary<string, BinocularsProperties>();

        public static BinocularsProperties GetBinocularsProperties(this Tool binoculars)
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

        public static bool IsBinoculars(this Tool maybeBinoculars) => maybeBinoculars.modData.TryGetValue(BinocularsProperties.TAG, out _);
    }
}
