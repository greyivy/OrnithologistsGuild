using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OrnithologistsGuild.Content;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using StateMachine;

namespace OrnithologistsGuild.Game.Critters
{
    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        public BirdieDef BirdieDef;
        private GameLocation Environment;

        public Fsm<BetterBirdieState, BetterBirdieTrigger> StateMachine;

        public Perch Perch;

        public bool IsFledgling;

        public Vector3 Position3 {
            get => new Vector3(position.X, position.Y, yOffset);
            set
            {
                position = Utilities.XY(value);
                yOffset = value.Z;
            }
        }
        public Vector2 TileLocation => Utilities.XY(Position3) / Game1.tileSize;

        public float ZIndex => (position.Y / (10000f - 1f)) + (position.X / 1000000f);

        public bool Is(BetterBirdieState state) => StateMachine.Current.Identifier == state;

        public bool IsFlying => Is(BetterBirdieState.Relocating) || Is(BetterBirdieState.FlyingAway);
        public bool IsPerched => Perch != null;
        public bool IsRoosting => IsPerched && (Perch.Type == PerchType.MapTile || Perch.Type == PerchType.Tree);
        public bool IsInNest => IsPerched && Perch.Tree?.HasNest() == true;
        public bool IsInBath => IsPerched && Perch.Type == PerchType.Bath;
        public bool IsInWater => Environment.isWaterTile((int)TileLocation.X, (int)TileLocation.Y);

        // Timers
        private int CharacterCheckTimer = 200;

        public bool IsSpotted; // Whether player has spotted bird with binoculars

        private float FlySpeedOffset; // Individual birds fly at slightly different speeds

        private bool HasBuiltNest = false;

        private TimeSpan AfterElapsed = TimeSpan.Zero;
        private List<(TimeSpan Trigger, Action Action, BetterBirdieState? InitiatedBy)> DelayedActions = new();

        private void After(TimeSpan after, Action action, BetterBirdieState? initiatedBy)
        {
            DelayedActions.Add((AfterElapsed + after, action, initiatedBy));
        }
        private void AfterBetween(TimeSpan a, TimeSpan b, Action action, BetterBirdieState? initiatedBy)
        {
            var difference = b - a;
            var offset = Game1.random.NextInt64(difference.Ticks);

            After(a + TimeSpan.FromTicks(offset), action, initiatedBy);
        }
        private void ClearDelayedActionsInitiatedBy(BetterBirdieState state)
        {
            DelayedActions.RemoveAll(a => a.InitiatedBy == state);
        }

        public BetterBirdie(BirdieDef birdieDef, Vector2 tileLocation, Perch perch = null, bool isFledgling = false) : base(0, Vector2.Zero)
        {
            BirdieDef = birdieDef;
            Perch = perch;
            IsFledgling = isFledgling;

            if (birdieDef.AssetPath != null)
            {
                var internalAssetName = birdieDef.ContentPackDef.ContentPack.ModContent.GetInternalAssetName(birdieDef.AssetPath).BaseName;

                baseFrame = birdieDef.BaseFrame;
                sprite = new AnimatedSprite(internalAssetName, baseFrame, 32, 32);
            } else
            {
                baseFrame = birdieDef.BaseFrame;
                sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
            }

            flip = Game1.random.NextDouble() < 0.5;

            // Position
            if (Perch == null)
            {
                position = tileLocation * Game1.tileSize;

                // Center on tile
                position.X += Game1.tileSize / 2;
                position.Y += Game1.tileSize / 2;

                // Scatter by up to half a tile in any direction
                position = Utility.getTranslatedVector2(position, Game1.random.Next(4), Game1.random.Next(Game1.tileSize / 2));
            } else
            {
                Position3 = Perch.Position;
            }
            startingPosition = position;

            FlySpeedOffset = (float)Game1.random.NextDouble() - 0.5f;

            InitializeStateMachine();
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            Environment = environment;

            // Process any delayed actions
            AfterElapsed += time.ElapsedGameTime;
            foreach (var delayedAction in DelayedActions.ToArray())
            {
                if (delayedAction.Trigger <= AfterElapsed)
                {
                    delayedAction.Action();
                    DelayedActions.Remove(delayedAction);
                }
            }

            if (!IsFlying)
            {
                if (IsRoosting &&
                    Perch.Type == PerchType.Tree &&
                    // Birds that only perch are still frightened on character proximity
                    (BirdieDef.LandPreference > 0 || BirdieDef.WaterPreference > 0))
                {
                    // Fly away when tree is chopped
                    if (Perch.Tree.health.Value < Tree.startingHealth)
                    {
                        Frighten();
                    }

                    // Fly away when tree is shaken (see TreePatches)
                }
                else if (!IsRoosting)
                {
                    CheckCharacterProximity(time, environment);
                }
            }

            StateMachine.Update(time.ElapsedGameTime);

            updateEmote(time);

            return base.update(time, environment);
        }

        public void PlayCall()
        {
            Environment.localSound(BirdieDef.SoundID == null ? "SpringBirds" : BirdieDef.SoundID, TileLocation);
        }

        public void Flip()
        {
            flip = !flip;
        }

        private bool CanMoveLeft() => !(
            Environment.isCollidingPosition(getBoundingBox(-1, 0), Game1.viewport, false, 0, false, null, false, false, true) ||
            Environment.isCollidingPosition(getBoundingBox(-2, 0), Game1.viewport, false, 0, false, null, false, false, true));
        private bool CanMoveRight() => !(
            Environment.isCollidingPosition(getBoundingBox(1, 0), Game1.viewport, false, 0, false, null, false, false, true) ||
            Environment.isCollidingPosition(getBoundingBox(2, 0), Game1.viewport, false, 0, false, null, false, false, true));

        private void CheckCharacterProximity(GameTime time, GameLocation environment)
        {
            CharacterCheckTimer -= time.ElapsedGameTime.Milliseconds;
            if (CharacterCheckTimer < 0)
            {
                CharacterCheckTimer = 200;

                if (!IsFlying && Utility.isThereAFarmerOrCharacterWithinDistance(position / Game1.tileSize, BirdieDef.GetContextualCautiousness(), environment) != null)
                {
                    Frighten();
                }
            }
        }

        public void Frighten() {
            if (ModEntry.debug_PerchType.HasValue || ModEntry.debug_BirdWhisperer.HasValue)
            {
                // Force relocate
                StateMachine.Trigger(BetterBirdieTrigger.Relocate);
                return;
            }

            if (Game1.random.NextDouble() < 0.8) StateMachine.Trigger(BetterBirdieTrigger.FlyAway);
            else StateMachine.Trigger(BetterBirdieTrigger.Relocate);
        }

        private void MaybeBuildNest()
        {
            // 20% chance to consider building a nest if the conditions are right
            var shouldBuildNest =
                !HasBuiltNest &&
                Perch?.Type == PerchType.Tree &&
                !Perch.Tree.HasNest() &&
                NestManager.CanBuildNestAt(Environment) &&
                Game1.random.NextDouble() < 0.2;

            if (shouldBuildNest)
            {
                Perch.Tree.SetNest(new Nest(BirdieDef, NestType.TreeTop, SDate.Now()));
                HasBuiltNest = true;

                ModEntry.Instance.Monitor.Log($"MaybeBuildNest {BirdieDef.ID} nest -> {Perch.Tree.Tile}");
            }
        }

        #region Rendering
        public void drawBirdie(SpriteBatch b)
        {
            if (sprite != null)
            {
                // Override `base.draw()` for extra control of clipping when bathing
                if (sprite.Texture != null)
                {
                    var clipBottom = Is(BetterBirdieState.Bathing) ? BirdieDef.BathingClipBottom : 0;
                    var bathYOffset = clipBottom > 0 ? (IsInBath ? (clipBottom * 3) : (clipBottom * 6)) : 0;

                    var sourceRectangle = new Rectangle(sprite.sourceRect.X, sprite.sourceRect.Y + clipBottom, sprite.sourceRect.Width, sprite.sourceRect.Height - (clipBottom * 2));

                    var spriteEffects = (flip || (sprite.CurrentAnimation != null && sprite.CurrentAnimation[sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                    if (IsFledgling)
                    {
                        var screenPosition = Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-48f, -96f + yJumpOffset + yOffset + bathYOffset));
                        b.Draw(sprite.Texture, screenPosition, sourceRectangle, Color.White * (Perch?.Tree?.alpha ?? 1f), 0, Vector2.Zero, 3f, spriteEffects, ZIndex);
                    }
                    else
                    {
                        var screenPosition = Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset + bathYOffset));
                        b.Draw(sprite.Texture, screenPosition, sourceRectangle, Color.White * (Perch?.Tree?.alpha ?? 1f), 0, Vector2.Zero, 4f, spriteEffects, ZIndex);
                    }
                }

                if (!IsInBath)
                {
                    // Draw shadow
                    b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, -4f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 64f), SpriteEffects.None, ZIndex - (1f / 10000f));
                }
            }
        }
        private void drawEmote(SpriteBatch b)
        {
            if (isEmoting)
            {
                Vector2 localPosition = getLocalPosition(Game1.viewport);
                localPosition.Y -= 118f - yOffset;
                b.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle(currentEmoteFrame * 16 % Game1.emoteSpriteSheet.Width, currentEmoteFrame * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White /** alpha*/, 0f, Vector2.Zero, 4f, SpriteEffects.None, ZIndex);
            }
        }

        public override void drawAboveFrontLayer(SpriteBatch b)
        {
            if (IsFlying || IsRoosting)
            {
                drawBirdie(b);
                drawEmote(b);
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!(IsFlying || IsRoosting))
            {
                drawBirdie(b);
                drawEmote(b);
            }
        }

        public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
        {
            Vector2 vector = position;
            return new Vector2(vector.X - (float)viewport.X - (0.5f * Game1.tileSize), vector.Y - (float)viewport.Y + (float)yJumpOffset);
        }

        public void Splash(float scale = 1f, float offsetX = 0, float offsetY = 0)
        {
            Environment.localSound("waterSlosh", TileLocation);

            // Center splash
            var splashPosition = new Vector2(
                position.X - (((scale / 2) + offsetX) * Game1.tileSize),
                position.Y - (((scale / 2) + offsetY) * Game1.tileSize));

            if (Utility.isOnScreen(position, Game1.tileSize))
            {
                var splash = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 832, 64, 64), (BirdieDef.FlapDuration / 2) / 10, 10, 1, splashPosition, false, Game1.random.Next(0, 2) == 0);
                splash.layerDepth = ZIndex + 2f;
                splash.scale = scale;
                Environment.temporarySprites.Add(splash);
            }
        }

        private List<FarmerSprite.AnimationFrame> GetFlyingAnimation()
        {
            return new List<FarmerSprite.AnimationFrame> {
                    new FarmerSprite.AnimationFrame (baseFrame + 6, (int)MathF.Round(0.27f * BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * BirdieDef.FlapDuration), secondaryArm: false, flip, frameBehavior: (Farmer who) =>
                    {
                        // Make bird shoot up a bit while flapping for more realistic flight
                        // e.g. flapDuration = 500, gravityAffectedDY = 4
                        // e.g. flapDuration = 250, gravityAffectedDY = 2
                        gravityAffectedDY = -(BirdieDef.FlapDuration * (4f/500f));

                        // Play flapping noise
                        if (Utility.isOnScreen(position, Game1.tileSize)) Environment.localSound("batFlap", position);
                    }),
                    new FarmerSprite.AnimationFrame (baseFrame + 8, (int)MathF.Round(0.27f * BirdieDef.FlapDuration)),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * BirdieDef.FlapDuration))
                };
        }

        private List<FarmerSprite.AnimationFrame> GetBathingAnimation()
        {
            return new List<FarmerSprite.AnimationFrame> {
                    new FarmerSprite.AnimationFrame (baseFrame + 6, (int)MathF.Round(0.27f * (BirdieDef.FlapDuration / 2))),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * (BirdieDef.FlapDuration / 2)), secondaryArm: false, flip, frameBehavior: (Farmer who) =>
                    {
                        Splash(
                            1 + Utility.RandomFloat(0, 0.75f),
                            (flip ? 1f : -1f) * Utility.RandomFloat(-0.25f, 0.5f),
                            Utility.RandomFloat(0, 1f)
                        );
                    }),
                    new FarmerSprite.AnimationFrame (baseFrame + 8, (int)MathF.Round(0.27f * (BirdieDef.FlapDuration / 2))),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * (BirdieDef.FlapDuration / 2)))
                };
        }
        #endregion
    }
}
