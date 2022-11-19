using System.Xml.Serialization;
using StardewValley;

namespace OrnithologistsGuild.Game.Items
{
    [XmlType("Mods_Ivy_OrnithologistsGuild_AntiqueBinoculars")]
    public class AntiqueBinoculars : Binoculars
    {
        public AntiqueBinoculars(): base(ModEntry.DGAContentPack.Find("AntiqueBinoculars"), 6)
        {
        }

        public AntiqueBinoculars(string arg) : this()
        {
            // Required for DGA
        }

        public override bool performUseAction(GameLocation location)
        {
            if (Game1.random.NextDouble() < 0.025)
            {
                Game1.addHUDMessage(new HUDMessage("Binoculars fell apart in your hands", HUDMessage.error_type));

                return true;
            }

            return base.performUseAction(location);
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

