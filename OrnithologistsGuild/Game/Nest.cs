using System;
using OrnithologistsGuild.Content;
using StardewModdingAPI.Utilities;

namespace OrnithologistsGuild.Game
{
	public enum NestType
	{
		TreeTop
	}

	public enum NestStage
	{
		Built,
		EggsLaid,
		EggsHatched,
		Fledged,
		Removed
	}

	public record Nest
	{
        private const int AgeEggs = 2;
        private const int AgeHatch = AgeEggs + 6;
        private const int AgeFledge = AgeHatch + 8;
        private const int AgeRemoved = AgeFledge + 4;

        public Nest(BirdieDef owner, NestType nestType, SDate dateBuilt)
		{
			Owner = owner;
			NestType = nestType;
			DateBuilt = dateBuilt;
		}

		public static bool TryParse(string value, out Nest nest)
		{
			try
			{
				var array = value.Split(',');

				var birdieDefUniqueId = array[0];
				var nestTypeString = array[1];
				var daysSinceStartString = array[2];

				if (ContentPackManager.BirdieDefs.TryGetValue(birdieDefUniqueId, out var birdieDef) &&
					Enum.TryParse<NestType>(nestTypeString, out var nestType) &&
					int.TryParse(daysSinceStartString, out var daysSinceStart))
				{
					nest = new Nest(birdieDef, nestType, SDate.FromDaysSinceStart(daysSinceStart));
					return true;
				}
			}
			catch (Exception e) {
				ModEntry.Instance.Monitor.Log($"Could not parse Nest data for '{value}': {e.Message}", StardewModdingAPI.LogLevel.Error);
            }

			nest = null;
			return false;
        }

		public override string ToString()
		{
			return string.Join(',', Owner.UniqueID, NestType.ToString(), DateBuilt.DaysSinceStart);
		}

		public BirdieDef Owner { get; private set; }

		public NestType NestType { get; private set; }

		public SDate DateBuilt { get; private set; }

		public int Age => SDate.Now().DaysSinceStart - DateBuilt.DaysSinceStart;

		public NestStage Stage
		{
			get
			{
				if (Age < AgeEggs) return NestStage.Built;
				if (Age < AgeHatch) return NestStage.EggsLaid;
				if (Age < AgeFledge) return NestStage.EggsHatched;
				if (Age < AgeRemoved) return NestStage.Fledged;
				return NestStage.Removed;
			}
		}
	}
}

