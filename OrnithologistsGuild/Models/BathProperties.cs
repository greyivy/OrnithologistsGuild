using System.Collections.Generic;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public record BathProperties
    {
        public const string CONTEXT_TAG = "Ivy_OrnithologistsGuild_Bath";

        private const string PROPERTY_HEATED = "Ivy_OrnithologistsGuild_Heated";
        private const string PROPERTY_Z_OFFSET = "Ivy_OrnithologistsGuild_ZOffset";

        public bool Heated { get; private set; }

        public int ZOffset { get; private set; }

        public BathProperties(Object bath)
        {
            if (DataLoader.BigCraftables(Game1.content).TryGetValue(bath.ItemId, out var bigCraftableData))
            {
                if (bigCraftableData.CustomFields.TryGetValue(PROPERTY_HEATED, out string heated)) {
                    Heated = bool.Parse(heated);
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_HEATED} custom field", nameof(bath));
                }

                if (bigCraftableData.CustomFields.TryGetValue(PROPERTY_Z_OFFSET, out string zOffset)) {
                    ZOffset = int.Parse(zOffset);
                } else {
                    throw new System.ArgumentException($"Must contain {PROPERTY_Z_OFFSET} custom field", nameof(bath));
                }
            } else {
                throw new System.ArgumentException("Must contain custom fields", nameof(bath));
            }
        }
    }

    public static class ObjectBathPropertiesExtensions
    {
        private static Dictionary<string, BathProperties> cachedBathProperties = new Dictionary<string, BathProperties>();

        public static BathProperties GetBathProperties(this Object bath)
        {
            if (!IsBath(bath)) return null;

            if (!cachedBathProperties.ContainsKey(bath.QualifiedItemId))
            {
                cachedBathProperties[bath.QualifiedItemId] = new BathProperties(bath);
            }

            return cachedBathProperties[bath.QualifiedItemId];
        }

        public static bool IsBath(this Object maybeBath) => maybeBath.bigCraftable.Value && maybeBath.HasContextTag(BathProperties.CONTEXT_TAG);
    }
}
