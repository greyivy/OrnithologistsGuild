using System.Xml.Serialization;
using StardewValley;

namespace OrnithologistsGuild.Game.Items
{
    [XmlType("Mods_Ivy_OrnithologistsGuild_ProBinoculars")]
    public class ProBinoculars : Binoculars
    {
        public ProBinoculars(): base(ModEntry.dgaPack.Find("ProBinoculars"), 10)
        {
        }

        public ProBinoculars(string arg) : this()
        {
            // Required for DGA
        }

        public override Item getOne()
        {
            var ret = new AntiqueBinoculars();
            ret.Quality = this.Quality;
            ret.Stack = 1;
            ret.Price = this.Price;
            ret.ObjectColor = this.ObjectColor;
            ret._GetOneFrom(this);
            return ret;
        }
    }
}

