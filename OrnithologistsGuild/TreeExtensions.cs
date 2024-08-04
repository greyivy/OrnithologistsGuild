using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Game;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace OrnithologistsGuild
{
	public static class TreeExtensions
	{
        private static readonly Dictionary<string, float> PerchableTreeHeight = new Dictionary<string, float> {
            { Tree.bushyTree, 270 },
            { Tree.leafyTree, 270 },
            { Tree.pineTree, 270 },
            { Tree.mahoganyTree, 270 },
            { Tree.palmTree, 200 },
            { Tree.palmTree2, 250 },
            { Tree.greenRainTreeBushy, 280 },
            { Tree.greenRainTreeLeafy, 280 }
        };

        public static bool GetAllowPerchingOrNesting(this Tree tree)
		{
            var treeType = tree.treeType.Value;

            var tileHeight = tree.getRenderBounds().Height / Game1.tileSize;
            if (tileHeight < 4) return false; // Small tree
            if (tree.health.Value < Tree.startingHealth) return false; // Damaged tree
            if (tree.tapped.Value) return false; // Tapped tree
            if (!PerchableTreeHeight.ContainsKey(treeType)) return false; // Non-perchable tree

            return true;
        }

        public static bool HasNest(this Tree tree) => NestManager.GetNest(tree) != null;
        public static bool HasNestOwnedBy(this Tree tree, BirdieDef birdieDef) => NestManager.GetNest(tree)?.Owner == birdieDef;
        public static Nest GetNest(this Tree tree) => NestManager.GetNest(tree);
        public static void SetNest(this Tree tree, Nest nest) => NestManager.SetNest(tree, nest);
        public static void ClearNest(this Tree tree) => NestManager.ClearNest(tree);

        public static Vector3 GetPerchPosition(this Tree tree)
        {
            if (PerchableTreeHeight.TryGetValue(tree.treeType.Value, out var height))
            {
                return new Vector3(
                    tree.Tile.X * Game1.tileSize,
                    tree.Tile.Y * Game1.tileSize,
                    -height);
            }

            return Vector3.Zero;
        }

        public static Vector2 GetNestPosition(this Tree tree)
        {
            var perchPosition = tree.GetPerchPosition();
            return Utilities.XY(perchPosition) + new Vector2(0, perchPosition.Z - 15);
        }
    }
}

