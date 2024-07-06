using System.Collections.Generic;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public record FeederProperties
    {
        public const string CONTEXT_TAG = "Ivy_OrnithologistsGuild_Feeder";

        private const string PROPERTY_TYPE = "Ivy_OrnithologistsGuild_Type";
        private const string PROPERTY_RANGE = "Ivy_OrnithologistsGuild_Range";
        private const string PROPERTY_MAX_FLOCKS = "Ivy_OrnithologistsGuild_MaxFlocks";
        private const string PROPERTY_Z_OFFSET = "Ivy_OrnithologistsGuild_ZOffset";

        public string Type { get; private set; }
        public int Range { get; private set; }
        public int MaxFlocks { get; private set; }

        public int ZOffset { get; private set; }

        public FeederProperties(Object feeder)
        {
            if (DataLoader.BigCraftables(Game1.content).TryGetValue(feeder.ItemId, out var bigCraftableData))
            {
                if (bigCraftableData.CustomFields.TryGetValue(PROPERTY_TYPE, out string type)) {
                    Type = type;
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_TYPE} custom field", nameof(feeder));
                }

                if (bigCraftableData.CustomFields.TryGetValue(PROPERTY_RANGE, out string range)) {
                    Range = int.Parse(range);
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_RANGE} custom field", nameof(feeder));
                }

                if (bigCraftableData.CustomFields.TryGetValue(PROPERTY_MAX_FLOCKS, out string maxFlocks)) {
                    MaxFlocks = int.Parse(maxFlocks);
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_MAX_FLOCKS} custom field", nameof(feeder));
                }

                if (bigCraftableData.CustomFields.TryGetValue(PROPERTY_Z_OFFSET, out string zOffset)) {
                    ZOffset = int.Parse(zOffset);
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_Z_OFFSET} custom field", nameof(feeder));
                }
            } else {
                throw new System.ArgumentException($"Must contain custom fields", nameof(feeder));
            }
        }
    }

    public static class ObjectFeederPropertiesExtensions
    {
        private static Dictionary<string, FeederProperties> cachedFeederProperties = new Dictionary<string, FeederProperties>();

        public static FeederProperties GetFeederProperties(this Object feeder)
        {
            if (!IsFeeder(feeder)) return null;

            if (!cachedFeederProperties.ContainsKey(feeder.QualifiedItemId))
            {
                cachedFeederProperties[feeder.QualifiedItemId] = new FeederProperties(feeder);
            }

            return cachedFeederProperties[feeder.QualifiedItemId];
        }

        public static bool IsFeeder(this Object maybeFeeder) => maybeFeeder.bigCraftable.Value && maybeFeeder.HasContextTag(FeederProperties.CONTEXT_TAG);
    }
}
