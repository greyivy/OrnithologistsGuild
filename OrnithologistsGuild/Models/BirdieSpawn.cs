using OrnithologistsGuild.Content;

namespace OrnithologistsGuild.Models
{
	public record BirdieSpawn(BirdieDef BirdieDef, BirdiePosition BirdiePosition, bool IsFledgling);
}
