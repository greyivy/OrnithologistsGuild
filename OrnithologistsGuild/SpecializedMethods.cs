﻿using System.Collections.Generic;
using StardewValley;
using StardewValley.GameData.Machines;

namespace OrnithologistsGuild
{
	public class SpecializedMethods
	{
        private static Dictionary<string, int> weightedFeederOutput = new Dictionary<string, int>()
        {
            { "(O)747" /* Rotten Plant */, 25 },
            { "(O)335" /* Iron Bar */, 3 },
            { "(O)336" /* Gold Bar */, 1 }
        };

        public static Item GetFeederOutput(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, StardewValley.Farmer player, out int? overrideMinutesUntilReady)
		{
            overrideMinutesUntilReady = null;

            var qualifiedItemId = Utilities.WeightedRandom<string>(weightedFeederOutput.Keys, key => weightedFeederOutput[key]);
            return ItemRegistry.Create(qualifiedItemId, 1);
        }
    }
}
