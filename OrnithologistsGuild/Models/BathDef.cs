using System.Linq;
using DynamicGameAssets.Game;
using OrnithologistsGuild.Content;

namespace OrnithologistsGuild.Models
{
    public class BathDef
    {
        public string ID;
        public bool Heated;

        public int ZOffset;

        public static BathDef FromBath(CustomBigCraftable bath)
        {
            return ContentManager.Baths.FirstOrDefault(bathDef => bathDef.ID == bath.Id);
        }
    }
}
