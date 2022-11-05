using System;
using MailFrameworkMod;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;

namespace MoreBirdsPlease
{
    public class Mail
    {
        public static void Initialize()
        {
            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_none",
                @"Dear @, ^^Your grandmother, an avid birder of many years, left you this. We hope you find a way to continue his tradition this spring! ^^Good luck and happy birding, ^The Pelican Town Ornithological Guild",
                new List<Item> { ModEntry.dgaPack.Find("LifeList").ToItem() },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && (SDate.From(Game1.Date) >= new SDate(5, "spring", 1) || DataManager.SaveData.lifeList.Length >= 1),
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_1",
                @"Dear @, ^^Congrats, you've identified your first bird! We think these may be of some help. ^^Good luck and happy birding, ^The Pelican Town Ornithological Guild",
                new List<Item> { new StardewValley.Object(770 /* Mixed Seeds */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 1,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_5",
                @"Dear @, ^^Five birds identified! You're on a roll. Did you know that some birds like to eat corn? ^^Good luck and happy birding, ^The Pelican Town Ornithological Guild",
                new List<Item> { new StardewValley.Object(270 /* Corn */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 5,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_10",
                @"Dear @, ^^Ten birds identified is quite the milestone. If only these Sunflower Seeds were hulled... ^^Good luck and happy birding, ^The Pelican Town Ornithological Guild",
                new List<Item> { new StardewValley.Object(431 /* Sunflower Seeds */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 10,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_15",
                @"Dear @, ^^Fifteen birds identified!? You're becoming an expert. How about an apple? ^^Good luck and happy birding, ^The Pelican Town Ornithological Guild",
                new List<Item> { new StardewValley.Object(613 /* Apple */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= 15,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_MoreBirdsPlease_ll_all",
                @"Dear @, ^^You've identified every bird Pelican Town has to offer! Your grandmother left you this as a reward for a job well done. He would be very proud of the birder you've become. ^^Until next time, ^The Pelican Town Ornithological Guild <",
                new List<Item> { new StardewValley.Object(928 /* Golden Egg */, 1) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && DataManager.SaveData.lifeList.Length >= DataManager.Birdies.Length,
                (l) => Game1.player.mailReceived.Add(l.Id)));
        }
    }
}

