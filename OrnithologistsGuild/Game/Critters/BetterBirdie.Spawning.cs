using System;
using OrnithologistsGuild.Content;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using OrnithologistsGuild.Models;
using xTile.Tiles;

namespace OrnithologistsGuild.Game.Critters
{
    public enum SpawnType
    {
        Land,
        Water,
        Perch
    }

    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
	{
        private const int Trials = 50;

        private bool CheckRelocationDistance(Vector2 relocateToTile)
        {
            var currentTile = position / Game1.tileSize;

            var distance = Vector2.Distance(currentTile, relocateToTile);
            if (distance < 10) return false; // Too close

            var distanceX = MathF.Abs(currentTile.X - relocateToTile.X);
            var distanceY = MathF.Abs(currentTile.Y - relocateToTile.Y);
            if (distanceX < 6 || distanceY < 4) return false; // Too straight (lol)

            return true;
        }

        public static SpawnType GetRandomSpawnTypeUsingMinimumWeightsFrom(IEnumerable<BirdieDef> birdieDefs)
        {
            return Utilities.WeightedRandom(
                new List<SpawnType>() { SpawnType.Land, SpawnType.Water, SpawnType.Perch },
                spawnType => spawnType switch
                {
                    SpawnType.Land => birdieDefs.Min(b => b.LandPreference),
                    SpawnType.Water => birdieDefs.Min(b => b.WaterPreference),
                    SpawnType.Perch => birdieDefs.Min(b => b.PerchPreference),
                    _ => 0
                }
            );
        }

        private static bool CheckArea(Rectangle area, Func<Vector2, bool> predicate)
        {
            for (int left = area.Left; left < area.Right; ++left)
            {
                for (int top = area.Top; top < area.Bottom; ++top)
                {
                    var tile = new Vector2(left, top);

                    if (!predicate(tile)) return false;
                }
            }

            return true;
        }

        private static bool CheckCorners(Rectangle area, Func<Vector2, bool> predicate)
        {
            Vector2[] corners = new Vector2[]
            {
                new Vector2(area.Left, area.Top),
                new Vector2(area.Right, area.Top),
                new Vector2(area.Left, area.Bottom),
                new Vector2(area.Right, area.Bottom)
            };

            foreach (var corner in corners)
            {
                if (!predicate(corner)) return false;
            }

            return true;
        }

        public BirdiePosition GetRandomPositionOrPerch(SpawnType? spawnType = null)
        {
            var result = GetRandomPositionsOrPerchesFor(Environment, new Dictionary<BirdieDef, float>() { { BirdieDef, 1f } }, mustBeOffscreen: false, birdie: this, flockSize: 1, spawnType);
            return result.Any() ? result.First().BirdiePosition : null;
        }

        public static IEnumerable<BirdieSpawn> GetRandomPositionsOrPerchesFor(GameLocation location, Dictionary<BirdieDef, float> weightedBirdieDefs, bool mustBeOffscreen, BetterBirdie birdie = null, int? flockSize = null, SpawnType? spawnType = null, Rectangle? tileAreaBound = null)
        {
            var birdieDefs = weightedBirdieDefs.Keys;

            if (!spawnType.HasValue) spawnType = ModEntry.debug_PerchType == null ? GetRandomSpawnTypeUsingMinimumWeightsFrom(birdieDefs) : SpawnType.Perch;
            if (!flockSize.HasValue) flockSize = Game1.random.Next(1, birdieDefs.Max(b => b.MaxFlockSize) + 1);

            ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor flock of {flockSize} {string.Join(", ", birdieDefs.Select(b => b.ID))} on {spawnType}");

            IEnumerable<Perch> availablePerches = Enumerable.Empty<Perch>();
            if (spawnType == SpawnType.Perch)
            {
                if (ModEntry.debug_PerchType == null)
                {
                    availablePerches = Utilities.Randomize(
                        Perch.GetAllAvailablePerches(location, birdieDefs,
                            mapTile: true,
                            tree: true,
                            feeder: birdieDefs.Any(b => b.FeederBaseWts.Any()),
                            bath: birdieDefs.Any(b => b.CanUseBaths),
                            nest: birdieDefs.Any(b => b.CanNestInTrees)));
                }
                else
                {
                    availablePerches = Utilities.Randomize(
                        Perch.GetAllAvailablePerches(location, birdieDefs,
                            mapTile: ModEntry.debug_PerchType == PerchType.MapTile,
                            tree: ModEntry.debug_PerchType == PerchType.Tree,
                            feeder: ModEntry.debug_PerchType == PerchType.Feeder,
                            bath: ModEntry.debug_PerchType == PerchType.Bath,
                            nest: ModEntry.debug_PerchType == PerchType.Tree));
                }

                if (!availablePerches.Any())
                {
                    ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor -> no perches available");
                    return Enumerable.Empty<BirdieSpawn>();
                }
            }

            for (var trial = 1; trial <= Trials; trial++)
            {
                if (spawnType == SpawnType.Land || spawnType == SpawnType.Water)
                {
                    // Try to find a clear area
                    var randomTile = tileAreaBound.HasValue ?
                        Utility.getRandomPositionInThisRectangle(tileAreaBound.Value, Game1.random) :
                        location.getRandomTile();

                    var tileArea = Utility.getRectangleCenteredAt(randomTile, flockSize == 1 ? 1 : 3);

                    if (CheckArea(tileArea, tile =>
                    {
                        // Tile onscreen
                        if (mustBeOffscreen && Utility.isOnScreen(tile * Game1.tileSize, Game1.tileSize)) return false;

                        // Tile not on map
                        if (!location.isTileOnMap(tile)) return false;

                        // Tile not a valid land tile
                        if (spawnType == SpawnType.Land &&
                            !(location.isTilePassable(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport) &&
                            !location.isWaterTile((int)tile.X, (int)tile.Y) &&
                            !location.IsTileOccupiedBy(tile))) return false;

                        // Tile not a valid water tile
                        if (spawnType == SpawnType.Water &&
                            !location.isOpenWater((int)tile.X, (int)tile.Y)) return false;

                        // Too close/straight to existing birdie
                        if (birdie != null && !birdie.CheckRelocationDistance(tile)) return false;

                        // Too close to character
                        if (Utility.isThereAFarmerOrCharacterWithinDistance(tile, birdieDefs.Max(b => b.GetContextualCautiousness()), location) != null) return false;

                        return true;
                    }))
                    {
                        // Suitable area found, distribute birds throughout it
                        var positionArea = new Rectangle(
                            tileArea.X * Game1.tileSize,
                            tileArea.Y * Game1.tileSize,
                            tileArea.Width * Game1.tileSize,
                            tileArea.Height * Game1.tileSize);

                        var locationHasFledgedNest = location.GetTreesWithNests()
                            .Any(tree => tree.HasNestOwnedBy(birdieDefs) && tree.GetNest().Stage == NestStage.Fledged);
                        var fledglingCount = locationHasFledgedNest ? (int)Math.Ceiling(flockSize.Value * Utility.RandomFloat(1, 2)) : 0;

                        var birdsToSpawn = Enumerable
                            .Repeat(0, flockSize.Value + fledglingCount)
                            .Select<int, (BirdieDef BirdieDef, BirdiePosition BirdiePosition)>(_ => (
                                BirdieDef: Utilities.WeightedRandom(birdieDefs, b => weightedBirdieDefs[b]),
                                BirdiePosition: new(
                                    Position: new Vector3(Utility.getRandomPositionInThisRectangle(positionArea, Game1.random), 0),
                                    Perch: null))
                            );

                        var adults = birdsToSpawn
                            .Take(flockSize.Value)
                            .Select(tuple => new BirdieSpawn(tuple.BirdieDef, tuple.BirdiePosition, IsFledgling: false));
                        var fledglings = birdsToSpawn
                            .Skip(flockSize.Value)
                            .Select(tuple => new BirdieSpawn(tuple.BirdieDef, tuple.BirdiePosition, IsFledgling: true));

                        ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor -> found suitable area in {trial} trial(s)");
                        return Enumerable.Concat(adults, fledglings);
                    }
                }
                else if (spawnType == SpawnType.Perch)
                {
                    if (flockSize > 1)
                    {
                        // Try to find a clear area
                        var randomTile = tileAreaBound.HasValue ?
                            Utility.getRandomPositionInThisRectangle(tileAreaBound.Value, Game1.random) :
                            location.getRandomTile();

                        var tileArea = Utility.getRectangleCenteredAt(randomTile, 17);

                        if (CheckCorners(tileArea, tile =>
                        {
                            // Tile onscreen
                            if (mustBeOffscreen && Utility.isOnScreen(tile * Game1.tileSize, Game1.tileSize)) return false;

                            // Tile not on map
                            if (!location.isTileOnMap(tile)) return false;

                            // Too close/straight to existing birdie
                            if (birdie != null && !birdie.CheckRelocationDistance(tile)) return false;

                            // Too close to character
                            if (Utility.isThereAFarmerOrCharacterWithinDistance(tile, birdieDefs.Max(b => b.GetContextualCautiousness()), location) != null) return false;

                            return true;
                        }))
                        {
                            var locationHasFledgedNest = location.GetTreesWithNests()
                                .Any(tree => tree.HasNestOwnedBy(birdieDefs) && tree.GetNest().Stage == NestStage.Fledged);
                            var fledglingCount = locationHasFledgedNest ? (int)Math.Ceiling(flockSize.Value * Utility.RandomFloat(1, 2)) : 0;

                            var birdsToSpawn = availablePerches
                                .Where(perch => tileArea.Contains(Utilities.XY(perch.Position) / Game1.tileSize))
                                .Take(flockSize.Value + fledglingCount)
                                .Select(perch => (
                                    BirdieDef: Utilities.WeightedRandom(
                                        birdieDefs.Where(b => b.CanPerchAt(perch)),
                                        b => weightedBirdieDefs[b]),
                                    BirdiePosition: new BirdiePosition(perch.Position, perch)
                                ));

                            var adults = birdsToSpawn
                                .Take(flockSize.Value)
                                .Select(tuple => new BirdieSpawn(tuple.BirdieDef, tuple.BirdiePosition, IsFledgling: false));
                            var fledglings = birdsToSpawn
                                .Skip(flockSize.Value)
                                .Select(tuple => new BirdieSpawn(tuple.BirdieDef, tuple.BirdiePosition, IsFledgling: true));

                            ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor -> found suitable perches in {trial} trial(s)");
                            return Enumerable.Concat(adults, fledglings);
                        }
                    } else
                    {
                        var firstBirdieDef = birdieDefs.First();

                        // Take at most 3 of each type of perch (this will favor
                        // feeders, baths, trees with owned nests, and map tiles for relocation)
                        ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor -> found suitable perch in {trial} trial(s)");
                        return Utilities.Randomize(
                            availablePerches
                                .Where(perch =>
                                {
                                    // Perch onscreen
                                    if (mustBeOffscreen && Utility.isOnScreen(Utilities.XY(perch.Position), Game1.tileSize)) return false;

                                    // Too close/straight to existing birdie
                                    if (birdie != null && !birdie.CheckRelocationDistance(Utilities.XY(perch.Position) / Game1.tileSize)) return false;

                                    // Too close to character
                                    if (Utility.isThereAFarmerOrCharacterWithinDistance(Utilities.XY(perch.Position) / Game1.tileSize, firstBirdieDef.GetContextualCautiousness(), location) != null) return false;

                                    // Can't perch in nest
                                    if (!firstBirdieDef.CanPerchAt(perch)) return false;

                                    return true;
                                })
                                .GroupBy(perch => (PerchType: perch.Type, OwnedNest: perch.Tree?.HasNestOwnedBy(birdieDefs) == true))
                                .SelectMany(group => group.Take(3)))
                            .Take(1)
                            .Select(perch => new BirdieSpawn(firstBirdieDef, new BirdiePosition(perch.Position, perch), false));
                    }
                }
            }

            return Enumerable.Empty<BirdieSpawn>();
        }
    }
}
