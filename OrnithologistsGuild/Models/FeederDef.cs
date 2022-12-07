using System.Linq;
using DynamicGameAssets.Game;
using OrnithologistsGuild.Content;

namespace OrnithologistsGuild.Models
{
    public class FeederDef
    {
        // TODO capitalize like BirdieDef
        public string id;
        public string type;

        public int zOffset;

        public int range;
        public int maxFlocks;

        public static FeederDef FromFeeder(CustomBigCraftable feeder)
        {
            return ContentManager.Feeders.FirstOrDefault(feederDef => feederDef.id == feeder.Id);
        }
    }
}

