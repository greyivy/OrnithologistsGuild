using System.Linq;
using OrnithologistsGuild.Content;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public class FoodDef
    {
        public string Type;

        public string QualifiedItemId;
        public int? CategoryId;

        public static FoodDef FromFeeder(Object feeder)
        {
            return ContentManager.Foods.FirstOrDefault(food =>
                feeder.lastInputItem.Value?.category.Value == food.CategoryId ||
                feeder.lastInputItem.Value?.QualifiedItemId == food.QualifiedItemId);
        }
    }
}
