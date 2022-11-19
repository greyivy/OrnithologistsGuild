using MailFrameworkMod;
using StardewValley;
using System.Collections.Generic;
using StardewModdingAPI.Utilities;
using OrnithologistsGuild.Game.Items;
using OrnithologistsGuild.Content;

namespace OrnithologistsGuild
{
    public class Mail
    {
        public static void Initialize()
        {
            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_OrnithologistsGuild_ll_none",
                @"Dear @, ^^Your grandmother, an avid birder of many years, left you this. We hope you find a way to continue her tradition this spring! ^^Good luck and happy birding, ^The Pelican Town Ornithologist's Guild",
                new List<Item> { new LifeList() },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && (SDate.From(Game1.Date) >= new SDate(5, "spring", 1) || SaveDataManager.SaveData.LifeList.Count > 0),
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_OrnithologistsGuild_ll_1",
                @"Dear @, ^^Congrats, you've identified your first bird! We think these may be of some help. ^^Good luck and happy birding, ^The Pelican Town Ornithologist's Guild",
                new List<Item> { new StardewValley.Object(770 /* Mixed Seeds */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && SaveDataManager.SaveData.LifeList.IdentifiedCount > 0,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_OrnithologistsGuild_ll_5",
                @"Dear @, ^^Five birds identified! You're on a roll. Leave some of this delicious corn for the birds! ^^Good luck and happy birding, ^The Pelican Town Ornithologist's Guild",
                new List<Item> { new StardewValley.Object(270 /* Corn */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && SaveDataManager.SaveData.LifeList.IdentifiedCount >= 5,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_OrnithologistsGuild_ll_10",
                @"Dear @, ^^Ten birds identified is quite the milestone. If only these Sunflower Seeds were hulled... ^^Good luck and happy birding, ^The Pelican Town Ornithologist's Guild",
                new List<Item> { new StardewValley.Object(431 /* Sunflower Seeds */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && SaveDataManager.SaveData.LifeList.IdentifiedCount >= 10,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_OrnithologistsGuild_ll_15",
                @"Dear @, ^^Fifteen birds identified!? You're becoming an expert! Perhaps the frugivores in your life will enjoy this. ^^Good luck and happy birding, ^The Pelican Town Ornithologist's Guild",
                new List<Item> { new StardewValley.Object(296 /* Salmonberries */, 5) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && SaveDataManager.SaveData.LifeList.IdentifiedCount >= 15,
                (l) => Game1.player.mailReceived.Add(l.Id)));

            MailDao.SaveLetter(new Letter(
                "Mods_Ivy_OrnithologistsGuild_ll_all",
                @"Dear @, ^^You've identified every bird Pelican Town has to offer! Your grandmother left you this as a reward for a job well done. She would be very proud of the birder you've become. ^^Until next time, ^The Pelican Town Ornithologist's Guild <",
                new List<Item> { new StardewValley.Object(928 /* Golden Egg */, 1) },
                (l) => !Game1.player.mailReceived.Contains(l.Id) && SaveDataManager.SaveData.LifeList.IdentifiedCount >= ContentPackManager.BirdieDefs.Count,
                (l) => Game1.player.mailReceived.Add(l.Id)));
        }
    }
}

