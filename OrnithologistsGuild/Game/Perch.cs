using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Game.Critters;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Linq;
using OrnithologistsGuild.Models;
using OrnithologistsGuild.Content;

namespace OrnithologistsGuild.Game
{
    public enum PerchType
    {
        MapTile,
        Bath,
        Feeder,
        Tree
    }

    public class Perch
    {
        public Vector3 Position { get; private set; } // NOT tile

        public Vector2? MapTile { get; private set; }
        public Object Bath { get; private set; }
        public Object Feeder { get; private set; }
        public Tree Tree { get; private set; }

        public PerchType? Type
        {
            get
            {
                if (MapTile.HasValue) return PerchType.MapTile;
                else if (Bath != null) return PerchType.Bath;
                else if (Feeder != null) return PerchType.Feeder;
                else if (Tree != null) return PerchType.Tree;
                return null;
            }
        }

        public Perch(Vector2 mapTile, int zOffset)
        {
            MapTile = mapTile;
            Position = new Vector3(
                MapTile.Value.X * Game1.tileSize,
                MapTile.Value.Y * Game1.tileSize,
                zOffset);

            // Center on tile
            Position = Position with
            {
                X = Position.X + Game1.tileSize / 2,
                Y = Position.Y + Game1.tileSize / 2
            };
        }
        public Perch(Tree tree)
        {
            Tree = tree;

            Position = tree.GetPerchPosition();

            // Center on tile
            Position = Position with
            {
                X = Position.X + Game1.tileSize / 2,
                Y = Position.Y + Game1.tileSize / 2
            };
        }
        public Perch(Object bigCraftable)
        {
            if (bigCraftable.IsBath())
            {
                Bath = bigCraftable;

                var bathProperties = bigCraftable.GetBathProperties();
                Position = new Vector3(
                    Bath.TileLocation.X * Game1.tileSize,
                    Bath.TileLocation.Y * Game1.tileSize,
                    bathProperties.ZOffset);

                // Center on tile
                Position = Position with
                {
                    X = Position.X + Game1.tileSize / 2,
                    Y = Position.Y + Game1.tileSize / 2
                };
            } else if (bigCraftable.IsFeeder())
            {
                Feeder = bigCraftable;

                var feederProperties = bigCraftable.GetFeederProperties();
                Position = new Vector3(
                    Feeder.TileLocation.X * Game1.tileSize,
                    Feeder.TileLocation.Y * Game1.tileSize,
                    feederProperties.ZOffset);

                // Center on tile
                Position = Position with
                {
                    X = Position.X + Game1.tileSize / 2,
                    Y = Position.Y + Game1.tileSize / 2
                };
            }
        }

        public override bool Equals(object obj)
        {
            var perch = obj as Perch;

            if (perch == null) return false;
            else if (perch.Type == PerchType.MapTile && Type == PerchType.MapTile)
                return perch.MapTile.Equals(MapTile);
            else if (perch.Type == PerchType.Bath && Type == PerchType.Bath)
                return perch.Bath.TileLocation.Equals(Bath.TileLocation);
            else if (perch.Type == PerchType.Feeder && Type == PerchType.Feeder)
                return perch.Feeder.TileLocation.Equals(Feeder.TileLocation);
            else if (perch.Type == PerchType.Tree && Type == PerchType.Tree)
                return perch.Tree.Tile.Equals(Tree.Tile);
            return false;
        }

        public override int GetHashCode()
        {
            if (Type == PerchType.MapTile) return MapTile.GetHashCode();
            else if (Type == PerchType.Bath) return Bath.GetHashCode();
            else if (Type == PerchType.Feeder) return Feeder.GetHashCode();
            else if (Type == PerchType.Tree) return Tree.GetHashCode();
            return 0;
        }

        public BetterBirdie GetOccupant(GameLocation location)
        {
            if (location.critters == null) return null;

            IEnumerable<BetterBirdie> birdies = location.critters.Where(c => c is BetterBirdie && ((BetterBirdie)c).IsPerched).Select(c => (BetterBirdie)c);
            return birdies.FirstOrDefault(birdie => birdie.Perch.Equals(this));
        }

        public static IEnumerable<Perch> GetAllMapPerches(GameLocation location)
        {
            var mapPropertyPerches = location.getMapProperty("Perches");
            if (string.IsNullOrWhiteSpace(mapPropertyPerches)) return Enumerable.Empty<Perch>();

            try
            {
                // Get all map perches
                return mapPropertyPerches.Split("/")
                    .Select(mapPerch =>
                    {
                        var values = mapPerch.Split(" ");
                        var x = int.Parse(values[0]);
                        var y = int.Parse(values[1]);
                        var zOffset = int.Parse(values[2]);
                        // var perchType = int.Parse(values[3]);

                        var tileLocation = new Vector2(x, y);

                        return new Perch(tileLocation, zOffset);
                    });
            } catch (System.Exception e) {
                ModEntry.Instance.Monitor.Log($"Invalid map property Perches: {e}", StardewModdingAPI.LogLevel.Error);
            }

            return Enumerable.Empty<Perch>();
        }

        public static IEnumerable<Perch> GetAllTreePerches(GameLocation location)
        {
            return location.GetTrees()
                .Where(tree => tree.GetAllowPerchingOrNesting() && !tree.HasNest())
                .Select(tree => new Perch(tree));
        }

        public static IEnumerable<Perch> GetAllOwnedNestPerches(GameLocation location, BirdieDef birdieDef)
        {
            var treesWithNests = location.GetTreesWithNests();

            return treesWithNests
                .Where(tree =>
                    tree.HasNestOwnedBy(birdieDef) &&
                    tree.GetNest().Stage != NestStage.Fledged &&
                    tree.GetNest().Stage != NestStage.Removed)
                .Select(tree => new Perch(tree));
        }

        public static IEnumerable<Perch> GetAllBirdBathPerches(GameLocation location)
        {
            return location.Objects
                .SelectMany(overlaidDict => overlaidDict.Values)
                .Where(obj => obj.IsBath())
                .Where(bath => !Game1.currentSeason.Equals("winter") || bath.GetBathProperties().Heated)
                .Select(bath => new Perch(bath));
        }

        public static IEnumerable<Perch> GetAllFeederPerches(GameLocation location)
        {
            // Get all bird feeders
            return location.Objects
                .SelectMany(overlaidDict => overlaidDict.Values)
                .Where(obj => obj.IsFeeder())
                .Where(feeder => feeder.MinutesUntilReady > 0) // No empty feeders
                .Select(feeder => new Perch(feeder));
        }

        public static IEnumerable<Perch> GetAllAvailablePerches(GameLocation location, BirdieDef birdieDef, bool mapTile, bool tree, bool feeder, bool bath, bool nest)
        {
            // Get all perched birdies
            var occupiedPerches = location.critters == null ?
                Enumerable.Empty<Perch>() :
                location.critters.Where(c => c is BetterBirdie && ((BetterBirdie)c).IsPerched).Select(c => ((BetterBirdie)c).Perch);

            return Enumerable.Empty<Perch>()
                .Concat(mapTile ? GetAllMapPerches(location)                            : Enumerable.Empty<Perch>())
                .Concat(tree    ? GetAllTreePerches(location)                           : Enumerable.Empty<Perch>())
                .Concat(feeder  ? GetAllFeederPerches(location)                         : Enumerable.Empty<Perch>())
                .Concat(bath    ? GetAllBirdBathPerches(location)                       : Enumerable.Empty<Perch>())
                .Concat(nest    ? GetAllOwnedNestPerches(location, birdieDef): Enumerable.Empty<Perch>())
                .Except(occupiedPerches);
        }
    }
}
