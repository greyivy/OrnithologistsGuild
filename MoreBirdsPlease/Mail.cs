using System;
using MailFrameworkMod;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;

namespace MoreBirdsPlease
{
    public class Mail
    {
        public Mail()
        {

        }

        public static void Initialize()
        {
            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_1",
                @"Dear @, ^^Congrats, you've added your first bird to your life list! We think this may be of some help. ^^Happy birding, ^The Pelican Town Ornithological Guild <",
                new List<Item> { new StardewValley.Object(770 /* Mixed Seeds */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 1,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_5",
                @"Dear @, ^^Five birds! You're on a roll. Did you know that some birds like to eat corn? ^^Happy birding, ^The Pelican Town Ornithological Guild <",
                new List<Item> { new StardewValley.Object(270 /* Corn */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 5,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_10",
                @"Dear @, ^^Ten birds is quite the milestone. If only these Sunflower Seeds were hulled... ^^Happy birding, ^The Pelican Town Ornithological Guild <",
                new List<Item> { new StardewValley.Object(431 /* Sunflower Seeds */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 10,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_15",
                @"Dear @, ^^Fifteen birds!? You're becoming a real expert. Perhaps these apples will be of use. ^^Happy birding, ^The Pelican Town Ornithological Guild <",
                new List<Item> { new StardewValley.Object(613 /* Apple */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 15,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_all",
                @"Dear @, ^^You've seen every bird Pelican Town has to offer! ^^Amazing work, ^The Pelican Town Ornithological Guild <",
                new List<Item> { new StardewValley.Object(928 /* Golden Egg */, 1) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= DataManager.Birdies.Length,
                (l) => Game1.player.mailReceived.Add(l.Id)));
        }
    }
}

