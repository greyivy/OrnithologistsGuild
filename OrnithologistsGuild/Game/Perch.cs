using System;
using DynamicGameAssets.Game;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Game.Critters;
using StardewValley;
using StardewValley.TerrainFeatures;
using OrnithologistsGuild.Content;
using System.Linq;
using HarmonyLib;
using OrnithologistsGuild.Models;
using StardewValley.Locations;

namespace OrnithologistsGuild.Game
{
    public class Perch
    {
        public Vector2 Position; // NOT tile

        public CustomBigCraftable Feeder;
        public Tree Tree;

        public Perch(Tree tree)
        {
            Tree = tree;

            var tileHeight = tree.getRenderBounds(tree.currentTileLocation).Height / Game1.tileSize;
            var tileLocation = new Vector2(tree.currentTileLocation.X, tree.currentTileLocation.Y - MathF.Ceiling((float)(tileHeight) / 2));
            Position = tileLocation * Game1.tileSize;

            // Center on tile
            Position.X += Game1.tileSize / 2;
            Position.Y += Game1.tileSize / 2;
        }
        public Perch(CustomBigCraftable feeder)
        {
            Feeder = feeder;

            Position = feeder.TileLocation * Game1.tileSize;

            // Center on tile
            Position.X += Game1.tileSize / 2;
            Position.Y += Game1.tileSize / 2;

            // Bird feeders have a small Y offset to properly position birds on their perch
            var feederDef = FeederDef.FromFeeder(feeder);
            if (feederDef != null)
            {
                Position.Y += feederDef.perchOffset;
            }
        }

        public override bool Equals(object obj)
        {
            var perch = obj as Perch;

            if (perch == null) return false;
            else if (perch.Feeder != null && this.Feeder != null && perch.Feeder.TileLocation.Equals(Feeder.TileLocation)) return true;
            else if (perch.Tree != null && this.Tree != null && perch.Tree.currentTileLocation.Equals(Tree.currentTileLocation)) return true;
            return false;
        }

        public override int GetHashCode()
        {
            if (Feeder != null) return Feeder.GetHashCode();
            if (Tree != null) return Tree.GetHashCode();
            return 0;
        }

        public BetterBirdie GetOccupant(GameLocation location)
        {
            IEnumerable<BetterBirdie> birdies = location.critters.Where(c => c is BetterBirdie && ((BetterBirdie)c).IsPerched).Select(c => (BetterBirdie)c);
            return birdies.FirstOrDefault(birdie => birdie.Perch.Equals(this));
        }

        public static Perch GetRandomAvailableTreePerch(GameLocation location, List<Perch> occupiedPerches)
        {
            ModEntry.Instance.Monitor.Log("GetRandomAvailableTreePerch");

            // Get all trees
            var allTrees = location.terrainFeatures.Values.Where(tf => tf is Tree).ToList();

            // Check random trees until an available one is found or we reach 25 trials
            for (int trial = 0; trial < 25; trial++)
            {
                Tree tree = (Tree)Utility.GetRandom(allTrees);

                var tileHeight = tree.getRenderBounds(tree.currentTileLocation).Height / Game1.tileSize;
                if (tileHeight < 4) continue; // Small tree
                if (tree.health.Value < Tree.startingHealth) continue; // Damaged tree
                if (tree.tapped.Value) continue; // Tapped tree

                var perch = new Perch(tree);
                if (occupiedPerches.Any(occupiedPerch => occupiedPerch.Equals(perch))) continue; // Occupied tree (more performant than calling Perch.GetOccupant() each time)

                return perch;
            }

            return null;
        }

        public static Perch GetRandomAvailableFeederPerch(BirdieDef birdieDef, GameLocation location, List<Perch> occupiedPerches)
        {
            ModEntry.Instance.Monitor.Log("GetRandomAvailableFeederPerch");

            // Get all bird feeders
            var allFeeders = location.Objects.SelectMany(overlaidDict => overlaidDict.Values).Where(obj => typeof(CustomBigCraftable).IsAssignableFrom(obj.GetType())).ToList();

            // Check random trees until an available one is found or we reach 25 trials
            for (int trial = 0; trial < 25; trial++)
            {
                CustomBigCraftable feeder = (CustomBigCraftable)Utility.GetRandom(allFeeders);

                if (feeder.MinutesUntilReady <= 0) continue; // Empty feeder

                var feederDef = FeederDef.FromFeeder(feeder);
                var foodDef = FoodDef.FromFeeder(feeder);
                if (!(birdieDef.CanPerchAt(feederDef) && birdieDef.CanEat(foodDef))) continue; // Incompatible feeder

                if (Utility.isThereAFarmerOrCharacterWithinDistance(feeder.TileLocation, birdieDef.GetContextualCautiousness(), location) != null) continue; // Character nearby

                var perch = new Perch(feeder);
                if (occupiedPerches.Any(occupiedPerch => occupiedPerch.Equals(perch))) continue; // Occupied feeder (more performant than calling Perch.GetOccupant() each time)

                return perch;
            }

            return null;
        }

        public static Perch GetRandomAvailablePerch(BirdieDef birdieDef, GameLocation location)
        {
            // Get all perched birdies
            var occupiedPerches = location.critters.Where(c => c is BetterBirdie && ((BetterBirdie)c).IsPerched).Select(c => ((BetterBirdie)c).Perch).ToList();

            Perch perch = null;

            if (Game1.random.NextDouble() < 0.5)
            {
                // Try to get available tree perch first
                perch = GetRandomAvailableTreePerch(location, occupiedPerches);
                if (perch == null)
                {
                    perch = GetRandomAvailableFeederPerch(birdieDef, location, occupiedPerches);
                }
            } else
            {
                // Try to get available feeder perch first
                perch = GetRandomAvailableFeederPerch(birdieDef, location, occupiedPerches);
                if (perch == null)
                {
                    perch = GetRandomAvailableTreePerch(location, occupiedPerches);
                }
            }

            return perch;
        }
    }
}
