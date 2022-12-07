using System.Linq;
using DynamicGameAssets.Game;
using OrnithologistsGuild.Content;

namespace OrnithologistsGuild.Models
{
    public class FoodDef
    {
        // TODO capitalize like BirdieDef
        public string type;
        public int feederAssetIndex;

        public static FoodDef FromFeeder(CustomBigCraftable feeder)
        {
            return ContentManager.Foods.FirstOrDefault(food => feeder.TextureOverride.EndsWith($":{food.feederAssetIndex}"));
        }
    }
}
