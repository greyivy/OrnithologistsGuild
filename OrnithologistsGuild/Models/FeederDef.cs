using System.Linq;
using DynamicGameAssets.Game;
using OrnithologistsGuild.Content;

namespace OrnithologistsGuild.Models
{
    public class FeederDef
    {
        public string ID;
        public string Type;

        public int ZOffset;

        public int Range;
        public int MaxFlocks;

        public static FeederDef FromFeeder(CustomBigCraftable feeder)
        {
            return ContentManager.Feeders.FirstOrDefault(feederDef => feederDef.ID == feeder.Id);
        }
    }
}
