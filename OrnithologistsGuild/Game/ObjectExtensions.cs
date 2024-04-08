using StardewValley;

namespace OrnithologistsGuild.Game
{
	public static class ObjectExtensions
	{
		public static string GetBigCraftableDefinitionId(this Object obj)
		{
            if (DataLoader.BigCraftables(Game1.content).TryGetValue(obj.QualifiedItemId, out var bigCraftableData) &&
                bigCraftableData.CustomFields.TryGetValue("Ivy.OrnithologistsGuild_DefinitionId", out string definitionId))
            {
                return definitionId;
            }

            return null;
        }
	}
}

