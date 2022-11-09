using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace OrnithologistsGuild
{
    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        public readonly int[] FallbackBirdTypes = new int[] { StardewValley.BellsAndWhistles.Birdie.blueBird, StardewValley.BellsAndWhistles.Birdie.brownBird };

        public Models.BirdieModel Birdie;
        public Models.FeederModel Perch;

        private float flightOffset;

        public BetterBirdie(Models.BirdieModel birdie, int tileX, int tileY, Models.FeederModel perch = null) : base(0, new Vector2(tileX * Game1.tileSize, tileY * Game1.tileSize))
        {
            Birdie = birdie;

            InitializeStateMachine();

            if (DataManager.BirdieAssetIds.ContainsKey(birdie.id))
            {
                baseFrame = 0;
                sprite = new AnimatedSprite(DataManager.BirdieAssetIds[birdie.id], baseFrame, 32, 32);
            } else
            {
                // Fallback to random vanilla bird
                baseFrame = FallbackBirdTypes[Game1.random.Next(0, FallbackBirdTypes.Length)];
                sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
            }

            flip = Game1.random.NextDouble() < 0.5;

            position.X += 32f;
            position.Y += 32f;

            startingPosition = position;

            flightOffset = (float)Game1.random.NextDouble() - 0.5f;
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
            return new Vector2(vector.X - (float)viewport.X - (0.5f * Game1.tileSize), vector.Y - (float)viewport.Y + (float)yJumpOffset) /*+ drawOffset*/;
        }

        // Emotes
        public bool isEmoting;

        public int currentEmote;

        public int currentEmoteFrame;

        public float emoteInterval;

        private bool emoteFading;

        public void updateEmote(GameTime time)
        {
            if (!isEmoting)
            {
                return;
            }
            emoteInterval += time.ElapsedGameTime.Milliseconds;
            if (emoteFading && emoteInterval > 20f)
            {
                emoteInterval = 0f;
                currentEmoteFrame--;
                if (currentEmoteFrame < 0)
                {
                    emoteFading = false;
                    isEmoting = false;
                }
            }
            else if (!emoteFading && emoteInterval > 20f && currentEmoteFrame <= 3)
            {
                emoteInterval = 0f;
                currentEmoteFrame++;
                if (currentEmoteFrame == 4)
                {
                    currentEmoteFrame = currentEmote;
                }
            }
            else if (!emoteFading && emoteInterval > 250f)
            {
                emoteInterval = 0f;
                currentEmoteFrame++;
                if (currentEmoteFrame >= currentEmote + 4)
                {
                    emoteFading = true;
                    currentEmoteFrame = 3;
                }
            }
        }

        public virtual void doEmote(int whichEmote)
        {
            if (!isEmoting)
            {
                isEmoting = true;
                currentEmote = whichEmote;
                currentEmoteFrame = 0;
                emoteInterval = 0f;
                //nextEventcommandAfterEmote = nextEventCommand;
            }
        }
    }
}
