using System;
using System.Collections.Generic;
using StardewValley;

namespace OrnithologistsGuild.Models
{
	public class SaveData
	{
		public Dictionary<long, PlayerSaveData> Players;

		public SaveData()
		{
			Players = new Dictionary<long, PlayerSaveData>();
		}

		public PlayerSaveData ForPlayer(long uniquePlayerId)
		{
			if (!Game1.IsMasterGame && Game1.player.UniqueMultiplayerID != uniquePlayerId)
			{
				throw new ArgumentOutOfRangeException(nameof(uniquePlayerId), "Farmhands only have access to their own PlayerSaveData");
			}

			if (!Players.ContainsKey(uniquePlayerId))
			{
				Players[uniquePlayerId] = new PlayerSaveData();
			}

			return Players[uniquePlayerId];
		}
	}

	public class PlayerSaveData
	{
        public LifeList LifeList;

        public PlayerSaveData()
        {
            LifeList = new LifeList();
        }
    }

	[Obsolete]
    public class LegacySaveData
    {
        public LifeList LifeList;

        public LegacySaveData()
        {
            LifeList = new LifeList();
        }
    }
}

