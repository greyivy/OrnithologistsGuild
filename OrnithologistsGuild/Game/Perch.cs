using System;
using DynamicGameAssets.Game;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Game.Critters;
using StardewValley;
using StardewValley.TerrainFeatures;
using OrnithologistsGuild.Content;
using System.Linq;

namespace OrnithologistsGuild.Game
{
    public class Perch
    {
        public Vector2 LocationTile;

        public Tree Tree;
        public Models.FeederDef FeederDef;

        public Perch(Vector2 location, Tree tree)
        {
            LocationTile = location;
            Tree = tree;
        }
        public Perch(Vector2 locationTile, Models.FeederDef feederDef)
        {
            LocationTile = locationTile;
            FeederDef = feederDef;
        }

        public override bool Equals(object obj)
        {
            var perch = obj as Perch;

            return perch != null && LocationTile.Equals(perch.LocationTile);
        }

        public override int GetHashCode()
        {
            return LocationTile.GetHashCode();
        }

        public static List<Perch> GetAvailablePerches(GameLocation location)
        {
            List<Perch> perches = new List<Perch>();

            // Get all perchable trees
            foreach (var terrainFeature in location.terrainFeatures.Values)
            {
                if (terrainFeature is Tree)
                {
                    Tree tree = (Tree)terrainFeature;

                    var tileHeight = terrainFeature.getRenderBounds(terrainFeature.currentTileLocation).Height / Game1.tileSize;
                    if (tileHeight < 4) continue; // Small tree
                    if (tree.health.Value < 10) continue; // Damaged tree

                    perches.Add(new Perch(new Vector2(terrainFeature.currentTileLocation.X, terrainFeature.currentTileLocation.Y - MathF.Ceiling((float)(tileHeight) / 2)), tree));
                }
            }

            // Get all perchable feeders
            foreach (var overlaidDict in location.Objects)
            {
                foreach (var obj in overlaidDict.Values)
                {
                    if (typeof(CustomBigCraftable).IsAssignableFrom(obj.GetType()))
                    {
                        var bigCraftable = (CustomBigCraftable)obj;

                        if (bigCraftable.MinutesUntilReady <= 0) continue; // Empty feeder
                        
                        var feederDef = ContentManager.Feeders.FirstOrDefault(feeder => feeder.id == bigCraftable.Id);
                        if (feederDef != null)
                        {
                            perches.Add(new Perch(bigCraftable.TileLocation, feederDef));
                        }
                    }
                }
            }

            // Determine which perches are occupied
            IEnumerable<BetterBirdie> birdies = location.critters.Where(c => c is BetterBirdie && ((BetterBirdie)c).IsPerched).Select(c => (BetterBirdie)c);
            return perches.Where(perch => !birdies.Any(birdie => perch.Equals(birdie.Perch))).ToList();
        }
    
        public static bool IsPerchAvailable(GameLocation location, Vector2 locationTile) {
            IEnumerable<BetterBirdie> birdies = location.critters.Where(c => c is BetterBirdie && ((BetterBirdie)c).IsPerched).Select(c => (BetterBirdie)c);
            return !birdies.Any(birdie => locationTile.Equals(birdie.Perch.LocationTile));
        }
    }
}
