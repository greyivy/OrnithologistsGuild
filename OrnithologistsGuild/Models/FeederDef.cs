using System.Linq;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public class FeederDef
    {
        public string ID;
        public string Type;

        public int ZOffset;

        public int Range;
        public int MaxFlocks;

        public static FeederDef FromFeeder(Object feeder)
        {
            if (!feeder.bigCraftable.Value) return null;

            return ContentManager.Feeders.FirstOrDefault(feederDef => feederDef.ID == feeder.GetBigCraftableDefinitionId());
        }

        public static bool IsFeeder(Object maybeFeeder)
        {
            return FromFeeder(maybeFeeder) != null;
        }
    }
}
