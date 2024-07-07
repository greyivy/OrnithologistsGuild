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

        public static SpawnType GetRandomSpawnType(BirdieDef birdieDef)
        {
            return Utilities.WeightedRandom(
                new List<SpawnType>() { SpawnType.Land, SpawnType.Water, SpawnType.Perch },
                spawnType => spawnType switch
                {
                    SpawnType.Land => birdieDef.LandPreference,
                    SpawnType.Water => birdieDef.WaterPreference,
                    SpawnType.Perch => birdieDef.PerchPreference,
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
            var result = GetRandomPositionsOrPerchesFor(Environment, BirdieDef, mustBeOffscreen: false, birdie: this, flockSize: 1, spawnType);
            return result.Any() ? result.First().BirdiePosition : null;
        }

        public static IEnumerable<(BirdiePosition BirdiePosition, bool IsFledgling)> GetRandomPositionsOrPerchesFor(GameLocation location, BirdieDef birdieDef, bool mustBeOffscreen, BetterBirdie birdie = null, int? flockSize = null, SpawnType? spawnType = null, Rectangle? tileAreaBound = null)
        {
            if (!spawnType.HasValue) spawnType = ModEntry.debug_PerchType == null ? GetRandomSpawnType(birdieDef) : SpawnType.Perch;
            if (!flockSize.HasValue) flockSize = Game1.random.Next(1, birdieDef.MaxFlockSize + 1);

            ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor flock of {flockSize} {birdieDef.ID} on {spawnType}");

            IEnumerable<Perch> availablePerches = Enumerable.Empty<Perch>();
            if (spawnType == SpawnType.Perch)
            {
                if (ModEntry.debug_PerchType == null)
                {
                    availablePerches = Utilities.Randomize(
                        Perch.GetAllAvailablePerches(location, birdieDef,
                            mapTile: true,
                            tree: true,
                            feeder: birdieDef.FeederBaseWts.Any(),
                            bath: birdieDef.CanUseBaths,
                            nest: birdieDef.CanNestInTrees));
                }
                else
                {
                    availablePerches = Utilities.Randomize(
                        Perch.GetAllAvailablePerches(location, birdieDef,
                            mapTile: ModEntry.debug_PerchType == PerchType.MapTile,
                            tree: ModEntry.debug_PerchType == PerchType.Tree,
                            feeder: ModEntry.debug_PerchType == PerchType.Feeder,
                            bath: ModEntry.debug_PerchType == PerchType.Bath,
                            nest: ModEntry.debug_PerchType == PerchType.Tree));
                }

                if (!availablePerches.Any())
                {
                    ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor -> no perches available");
                    return Enumerable.Empty<(BirdiePosition BirdiePosition, bool IsFledgling)>();
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
                        if (Utility.isThereAFarmerOrCharacterWithinDistance(tile, birdieDef.GetContextualCautiousness(), location) != null) return false;

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
                                .Any(tree => tree.HasNestOwnedBy(birdieDef) && tree.GetNest().Stage == NestStage.Fledged);

                        var adults = Enumerable
                            .Repeat(0, flockSize.Value)
                            .Select<int, (BirdiePosition BirdiePosition, bool IsFledgling)>(_ => (new(
                                Position: new Vector3(Utility.getRandomPositionInThisRectangle(positionArea, Game1.random), 0),
                                Perch: null), IsFledgling: false)
                            );
                        var fledglings = Enumerable
                            .Repeat(0, locationHasFledgedNest ? flockSize.Value * 2 : 0)
                            .Select<int, (BirdiePosition BirdiePosition, bool IsFledgling)>(_ => (new(
                                Position: new Vector3(Utility.getRandomPositionInThisRectangle(positionArea, Game1.random), 0),
                                Perch: null), IsFledgling: true)
                            );

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
                            if (Utility.isThereAFarmerOrCharacterWithinDistance(tile, birdieDef.GetContextualCautiousness(), location) != null) return false;

                            return true;
                        }))
                        {
                            var locationHasFledgedNest = location.GetTreesWithNests()
                                .Any(tree => tree.HasNestOwnedBy(birdieDef) && tree.GetNest().Stage == NestStage.Fledged);

                            var totalPerches = locationHasFledgedNest ? flockSize.Value * 3 : flockSize.Value;
                            var perches = availablePerches
                                .Where(perch => tileArea.Contains(Utilities.XY(perch.Position) / Game1.tileSize))
                                .Take(totalPerches);

                            var adults = perches
                                .Take(flockSize.Value)
                                .Select(perch => new BirdiePosition(perch.Position, perch));
                            var fledglings = availablePerches
                                .Take(totalPerches - flockSize.Value)
                                .Select(perch => new BirdiePosition(perch.Position, perch));

                            ModEntry.Instance.Monitor.Log($"GetRandomPositionsOrPerchesFor -> found suitable perches in {trial} trial(s)");
                            return Enumerable.Concat(
                                adults.Select(birdiePosition => (birdiePosition, false)),
                                fledglings.Select(birdiePosition => (birdiePosition, true)));
                        }
                    } else
                    {
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
                                    if (Utility.isThereAFarmerOrCharacterWithinDistance(Utilities.XY(perch.Position) / Game1.tileSize, birdieDef.GetContextualCautiousness(), location) != null) return false;

                                    return true;
                                })
                                .GroupBy(perch => (PerchType: perch.Type, OwnedNest: perch.Tree?.HasNestOwnedBy(birdieDef) == true))
                                .SelectMany(group => group.Take(3)))
                            .Take(1)
                            .Select(perch => (new BirdiePosition(perch.Position, perch), false));
                    }
                }
            }

            return Enumerable.Empty<(BirdiePosition BirdiePosition, bool IsFledgling)>();
        }
    }
}
