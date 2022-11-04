using System;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using DynamicGameAssets.Game;
using DynamicGameAssets.PackData;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using System.Collections.Generic;
using MailFrameworkMod;
using SpaceShared;

namespace MoreBirdsPlease.Game.Items
{
    [XmlType("Mods_Ivy_MoreBirdsPlease_Binoculars")]
    public class Binoculars : CustomObject
    {
        public Binoculars() : base((ObjectPackData)ModEntry.dgaPack.Find("Binoculars"))
        { 
        }

        public override bool performUseAction(GameLocation location)
        {
            if (location == null) return false;

            if (Game1.random.NextDouble() < 0.05)
            {
                Game1.addHUDMessage(new HUDMessage("Binoculars fell apart in your hands", HUDMessage.error_type));

                return true;
            }

            foreach (var critter in location.critters)
            {
                if (critter is BetterBirdie && Utility.isOnScreen(critter.position, 0))
                {
                    var birdie = (BetterBirdie)critter;

                    if (!DataManager.LifeListContains(birdie.Birdie))
                    {
                        DataManager.AddToLifeList(birdie.Birdie);

                        Game1.addHUDMessage(new HUDMessage($"{birdie.Birdie.name} added to life list", HUDMessage.achievement_type));
                    }
                }
            }

            return false;
        }
    }
}


