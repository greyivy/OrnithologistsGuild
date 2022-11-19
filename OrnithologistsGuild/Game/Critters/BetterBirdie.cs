using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Content;
using StardewValley;

namespace OrnithologistsGuild.Game.Critters
{
    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        public BirdieDef BirdieDef;

        public Models.FeederDef Perch;
        public bool Spotted;

        private float FlightOffset;

        public BetterBirdie(BirdieDef birdieDef, int tileX, int tileY, Models.FeederDef perch = null) : base(0, new Vector2(tileX * Game1.tileSize, tileY * Game1.tileSize))
        {
            BirdieDef = birdieDef;

            InitializeStateMachine();

            if (birdieDef.InternalAssetName != null)
            {
                baseFrame = birdieDef.InternalAssetBaseFrame;
                sprite = new AnimatedSprite(birdieDef.InternalAssetName, baseFrame, 32, 32);
            } else
            {
                baseFrame = birdieDef.InternalAssetBaseFrame;
                sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
            }

            flip = Game1.random.NextDouble() < 0.5;

            position.X += 32f;
            position.Y += 32f;

            startingPosition = position;

            FlightOffset = (float)Game1.random.NextDouble() - 0.5f;
            RelocateFlyAwayTimer = Game1.random.Next(10, 120) * 1000;

            this.Perch = perch;
            if (perch != null)
            {
                this.position.Y += perch.perchOffset;
                this.startingPosition.Y = this.position.Y;
            }
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            if (IsFlying)
            {
                base.draw(b);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!IsFlying)
            {
                base.draw(b);

                if (isEmoting)
                {
                    Vector2 localPosition = getLocalPosition(Game1.viewport);
                    localPosition.Y -= 118f;
                    b.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle(currentEmoteFrame * 16 % Game1.emoteSpriteSheet.Width, currentEmoteFrame * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White /** alpha*/, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)position.Y / 10000f);
                }
            }
        }

        public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
        {
            Vector2 vector = position;
            return new Vector2(vector.X - (float)viewport.X - (0.5f * Game1.tileSize), vector.Y - (float)viewport.Y + (float)yJumpOffset);
        }
    }
}
