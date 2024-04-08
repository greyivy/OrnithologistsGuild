using System.Linq;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using StardewValley;

namespace OrnithologistsGuild.Models
{
    public class BathDef
    {
        public string ID;
        public bool Heated;

        public int ZOffset;

        public static BathDef FromBath(Object bath)
        {
            if (!bath.bigCraftable.Value) return null;

            return ContentManager.Baths.FirstOrDefault(bathDef => bathDef.ID == bath.GetBigCraftableDefinitionId());
        }

        public static bool IsBath(Object maybeBath)
        {
            return FromBath(maybeBath) != null;
        }
    }
}
