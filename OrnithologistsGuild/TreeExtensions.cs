using System;
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
        public static bool GetAllowPerchingOrNesting(this Tree tree)
		{
            var tileHeight = tree.getRenderBounds().Height / Game1.tileSize;
            if (tileHeight < 4) return false; // Small tree
            if (tree.health.Value < Tree.startingHealth) return false; // Damaged tree
            if (tree.tapped.Value) return false; // Tapped tree

            return true;
        }

        public static bool HasNest(this Tree tree) => NestManager.GetNest(tree) != null;
        public static bool HasNestOwnedBy(this Tree tree, BirdieDef birdieDef) => NestManager.GetNest(tree)?.Owner == birdieDef;
        public static Nest GetNest(this Tree tree) => NestManager.GetNest(tree);
        public static void SetNest(this Tree tree, Nest nest) => NestManager.SetNest(tree, nest);
        public static void ClearNest(this Tree tree) => NestManager.ClearNest(tree);

        public static Vector3 GetPerchPosition(this Tree tree)
        {
            var height = tree.getRenderBounds().Height;
            return new Vector3(
                tree.Tile.X * Game1.tileSize,
                tree.Tile.Y * Game1.tileSize,
                -MathF.Ceiling(height / 1.65f));
        }

        public static Vector2 GetNestPosition(this Tree tree)
        {
            var perchPosition = tree.GetPerchPosition();
            return Utilities.XY(perchPosition) + new Vector2(0, perchPosition.Z - 15);
        }
    }
}

