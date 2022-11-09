using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StateMachine;

namespace OrnithologistsGuild
{
    public enum BetterBirdieState
    {
        Stopping,
        Stopped,
        Hopping,
        Walking,
        Pecking,
        Sleeping,
        FlyingAway,
        Relocating
    }

    public enum BetterBirdieTrigger
    {
        Peck,
        Walk,
        Stop,
        Hop,
        FlyAway,
        Sleep,
        Relocate
    }

    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        private int CharacterCheckTimer = 200;

        private int WalkTimer;
        private int RelocateFlyAwayTimer;

        private Vector2? RelocateFrom;
        private Vector2? RelocateTo;
        private float? RelocateDistance;
        private int? RelocateDuration;
        private int? RelocateElapsed;

        public Fsm<BetterBirdieState, BetterBirdieTrigger> StateMachine;

        private GameLocation Environment;

        private List<FarmerSprite.AnimationFrame> GetFlyingAnimation()
        {
            return new List<FarmerSprite.AnimationFrame> {
                    new FarmerSprite.AnimationFrame (baseFrame + 6, (int)MathF.Round(0.27f * Birdie.flapDuration)),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * Birdie.flapDuration), secondaryArm: false, flip, frameBehavior: (Farmer who) =>
                    {
                        // Make bird shoot up a bit while flapping for more realistic flight
                        // e.g. flapDuration = 500, gravityAffectedDY = 4
                        // e.g. flapDuration = 250, gravityAffectedDY = 2
                        gravityAffectedDY = -(Birdie.flapDuration * (4f/500f));

                        // Play flapping noise
                        if (Utility.isOnScreen(position, Game1.tileSize)) Game1.playSound("batFlap");
                    }),
                    new FarmerSprite.AnimationFrame (baseFrame + 8, (int)MathF.Round(0.27f * Birdie.flapDuration)),
                    new FarmerSprite.AnimationFrame (baseFrame + 7, (int)MathF.Round(0.23f * Birdie.flapDuration))
                };
        }

        public bool IsFlying { get {
            return StateMachine.Current.Identifier == BetterBirdieState.Relocating || StateMachine.Current.Identifier == BetterBirdieState.FlyingAway;
        } }

        private void CheckCharacterProximity(GameTime time, GameLocation environment)
        {
            CharacterCheckTimer -= time.ElapsedGameTime.Milliseconds;
            if (CharacterCheckTimer < 0)
            {
                CharacterCheckTimer = 200;

                if (!IsFlying && Utility.isThereAFarmerOrCharacterWithinDistance(position / Game1.tileSize, Birdie.cautiousness, environment) != null)
                {
                    StateMachine.Trigger(Game1.random.NextDouble() < 0.75 ? BetterBirdieTrigger.FlyAway : BetterBirdieTrigger.Relocate);
                }
            }
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            Environment = environment;

            CheckCharacterProximity(time, environment);

            StateMachine.Update(time.ElapsedGameTime);

            updateEmote(time);

            return base.update(time, environment);
        }

        private void InitializeStateMachine()
        {
            StateMachine = Fsm<BetterBirdieState, BetterBirdieTrigger>.Builder(BetterBirdieState.Pecking)
                .State(BetterBirdieState.Stopping) // Done!
                    .TransitionTo(BetterBirdieState.Stopped).On(BetterBirdieTrigger.Stop)
                    .Update(a =>
                    {
                        // Wait for current animation to stop
                        if (sprite.CurrentAnimation == null)
                        {
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                        }
                    })
                .State(BetterBirdieState.Stopped) // Done! // TODO probability per bird, whether perched
                    .TransitionTo(BetterBirdieState.Sleeping).On(BetterBirdieTrigger.Sleep)
                    .TransitionTo(BetterBirdieState.Pecking).On(BetterBirdieTrigger.Peck)
                    .TransitionTo(BetterBirdieState.Walking).On(BetterBirdieTrigger.Walk)
                    .TransitionTo(BetterBirdieState.Hopping).On(BetterBirdieTrigger.Hop)
                    .OnEnter(e =>
                    {
                        // Reset animation to base frame
                        sprite.currentFrame = baseFrame;
                    })
                    .Update(a =>
                    {
                        if (Game1.random.NextDouble() < 0.008)
                        {
                            switch (Game1.random.Next(7))
                            {
                                case 0:
                                    StateMachine.Trigger(Perch == null ? BetterBirdieTrigger.Sleep : BetterBirdieTrigger.Peck);
                                    break;
                                case 1:
                                    StateMachine.Trigger(BetterBirdieTrigger.Peck);
                                    break;
                                case 2:
                                    StateMachine.Trigger(BetterBirdieTrigger.Hop);
                                    break;
                                case 3:
                                    flip = !flip;
                                    StateMachine.Trigger(BetterBirdieTrigger.Hop);
                                    break;
                                case 4:
                                case 5:
                                    StateMachine.Trigger(BetterBirdieTrigger.Walk);
                                    break;
                                case 6:
                                    var random = Game1.random.NextDouble();
                                    if (random < 0.15)
                                    {
                                        StateMachine.Trigger(BetterBirdieTrigger.FlyAway);
                                    } else if (random < 0.5)
                                    {
                                        StateMachine.Trigger(BetterBirdieTrigger.Relocate);
                                    }
                                    break;
                            }
                        }
                    })
                .State(BetterBirdieState.Hopping) // Done!
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        gravityAffectedDY = -2f;
                    })
                    .Update(a =>
                    {
                        if (Perch == null && yJumpOffset < 0f)
                        {
                            // Hop left or right
                            if (!flip)
                            {
                                if (!Environment.isCollidingPosition(getBoundingBox(-2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                                {
                                    position.X -= 2f;
                                }
                                else
                                {
                                    // Can't hop left -- flip instead
                                    flip = !flip;
                                }
                            }
                            else
                            {
                                if (!Environment.isCollidingPosition(getBoundingBox(2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                                {
                                    position.X += 2f;
                                }
                                else
                                {
                                    flip = !flip;
                                }
                            }
                        }
                        else if (yJumpOffset >= 0)
                        {
                            // Done hopping
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                        }
                    })
                .State(BetterBirdieState.Walking) // Done! // TODO feeder bounds, maybe fly down from perch?
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        // Start walk animation
                        sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame> {
                                    new FarmerSprite.AnimationFrame ((short)baseFrame, 100),
                                    new FarmerSprite.AnimationFrame ((short)(baseFrame + 1), 100)
                                });
                        sprite.loop = true;

                        if (position.X >= startingPosition.X)
                        {
                            flip = false;
                        }
                        else
                        {
                            flip = true;
                        }

                        WalkTimer = Game1.random.Next(5, 50) * 100;
                    })
                    .OnExit(e =>
                    {
                        // Stop walk animation
                        sprite.loop = false;
                        sprite.CurrentAnimation = null;
                    })
                    .Update(a =>
                    {
                        WalkTimer -= a.ElapsedTimeSpan.Milliseconds;

                        if (!flip)
                        {
                            if (!(Perch != null && position.X < startingPosition.X - 1f) && !Environment.isCollidingPosition(getBoundingBox(-1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                            {
                                position.X -= 1f;
                            }
                            else
                            {
                                // Can't walk left
                                if (Perch == null)
                                {
                                    flip = !flip;
                                }
                                else
                                {
                                    StateMachine.Trigger(BetterBirdieTrigger.Stop);
                                }
                            }
                        }
                        else
                        {
                            if (!(Perch != null && position.X > startingPosition.X + 3f) && !Environment.isCollidingPosition(getBoundingBox(1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                            {
                                position.X += 1f;
                            }
                            else
                            {
                                // Can't walk right
                                if (Perch == null)
                                {
                                    flip = !flip;
                                }
                                else
                                {
                                    StateMachine.Trigger(BetterBirdieTrigger.Stop);
                                }
                            }
                        }

                        if (WalkTimer <= 0)
                        {
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                        }
                    })
                .State(BetterBirdieState.Pecking) // Done!
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .Update(a =>
                    {
                        if (sprite.CurrentAnimation == null)
                        {
                            List<FarmerSprite.AnimationFrame> list = new List<FarmerSprite.AnimationFrame>();
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 480));
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 170, secondaryArm: false, flip));
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 170, secondaryArm: false, flip));

                            int num = Game1.random.Next(1, 5);
                            for (int i = 0; i < num; i++)
                            {
                                list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 70));
                                list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 100, secondaryArm: false, flip, (Farmer who) =>
                                {
                                    // Play pecking noise
                                    if (Utility.isOnScreen(position, Game1.tileSize))
                                    {
                                        Game1.playSound("shiny4");
                                    }
                                }));
                            }

                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 100));
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 70, secondaryArm: false, flip));
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 1), 70, secondaryArm: false, flip));
                            list.Add(new FarmerSprite.AnimationFrame((short)baseFrame, 500, secondaryArm: false, flip, (Farmer who) =>
                            {
                                // 50% chance to peck again
                                if (Game1.random.NextDouble() < 0.5)
                                {
                                    StateMachine.Trigger(BetterBirdieTrigger.Stop);
                                }
                            }));

                            sprite.loop = false;
                            sprite.setCurrentAnimation(list);
                        }
                    })
                .State(BetterBirdieState.Sleeping) // Done!
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        sprite.currentFrame = baseFrame + 5;
                    })
                    .Update(a =>
                    {
                        if (isEmoting) return;

                        if (Game1.random.NextDouble() < 0.003)
                        {
                            doEmote(Character.sleepEmote);
                        }
                        else if (Game1.random.NextDouble() < 0.005)
                        {
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                        }
                    })
                .State(BetterBirdieState.FlyingAway) // Done! // TODO sounds, fly speed
                    .OnEnter(e =>
                    {
                        Character character = Utility.isThereAFarmerOrCharacterWithinDistance(position / Game1.tileSize, Birdie.cautiousness, Environment);

                        // Fly away from nearest character
                        if (character != null)
                        {
                            if (character.Position.X > position.X)
                            {
                                flip = false;
                            }
                            else
                            {
                                flip = true;
                            }
                        }

                        if (Game1.random.NextDouble() < 0.85)
                        {
                            Game1.playSound("SpringBirds");
                        }

                        sprite.setCurrentAnimation(GetFlyingAnimation());
                        sprite.loop = true;
                    })
                    .Update(a =>
                    {
                        if (!flip)
                        {
                            position.X -= Birdie.flySpeed;
                        }
                        else
                        {
                            position.X += Birdie.flySpeed;
                        }
                        yOffset -= 2f + flightOffset;
                    })
                .State(BetterBirdieState.Relocating)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        // Try to find clear tile to relocate to
                        for (int trial = 0; trial < 50; trial++)
                        {
                            var randomTile = Environment.getRandomTile();

                            // Get a 3x3 patch around the random tile
                            var randomRect = new Microsoft.Xna.Framework.Rectangle((int)randomTile.X - 1, (int)randomTile.Y - 1, 3, 3);

                            if (Environment.isAreaClear(randomRect) && Utility.isThereAFarmerOrCharacterWithinDistance(randomTile, Birdie.cautiousness, Environment) == null)
                            {
                                var randomPosition = new Vector2(randomTile.X * Game1.tileSize, randomTile.Y * Game1.tileSize);

                                var distance = Vector2.Distance(position, randomPosition);
                                if (distance < 500 || distance > 2500) continue; // Too close/far

                                RelocateFrom = position;
                                RelocateTo = randomPosition;

                                RelocateDistance = distance;

                                RelocateDuration = (int)(RelocateDistance.Value / (Birdie.flySpeed / 15f));
                                RelocateElapsed = 0;

                                if (position.X > RelocateTo.Value.X)
                                {
                                    flip = false;
                                }
                                else
                                {
                                    flip = true;
                                }

                                break;
                            }
                        }

                        if (RelocateTo.HasValue)
                        {
                            if (Game1.random.NextDouble() < 0.85)
                            {
                                Game1.playSound("SpringBirds");
                            }

                            sprite.setCurrentAnimation(GetFlyingAnimation());
                            sprite.loop = true;
                        }
                        else
                        {
                            // No clear location -- fly away instead
                            StateMachine.Trigger(BetterBirdieTrigger.FlyAway);
                        }
                    })
                    .OnExit(e =>
                    {
                        if (RelocateTo.HasValue) {
                            // Stop fly animation
                            sprite.loop = false;
                            sprite.CurrentAnimation = null;

                            // No longer perched
                            Perch = null;

                            // Clean up
                            RelocateFrom = null;
                            RelocateTo = null;

                            RelocateDistance = null;

                            RelocateDuration = null;
                            RelocateElapsed = null;
                        }
                    })
                    .Update(a =>
                    {
                        if (RelocateTo.HasValue)
                        {
                            // Fly to tile
                            RelocateElapsed += a.ElapsedTimeSpan.Milliseconds;
                            if (RelocateElapsed <= RelocateDuration)
                            {
                                var factor = ((float)RelocateElapsed.Value / (float)RelocateDuration.Value);

                                // Fly in an arc
                                if (factor < 0.5f)
                                {
                                    // Fly up to mid point
                                    var arcFactor = factor * 2f;
                                    yOffset = -(Utility.Lerp(0, (RelocateDistance.Value / 6f), Utilities.EaseOutSine(arcFactor)));

                                    //yOffset = -(Vector2.Lerp(new Vector2(0, 0), new Vector2(0, (RelocateDistance.Value / 6f)), EaseOutSine(arcFactor)).Y);
                                }
                                else
                                {
                                    // Fly down from mid point
                                    var arcFactor = (factor - 0.5f) * 2f;
                                    yOffset = -(Utility.Lerp(RelocateDistance.Value / 6f, 0, Utilities.EaseOutSine(arcFactor)));

                                    //yOffset = -(Vector2.Lerp(new Vector2(0, RelocateDistance.Value / 6f), new Vector2(0, 0), EaseOutSine(arcFactor)).Y);
                                }

                                position = Vector2.Lerp(RelocateFrom.Value, RelocateTo.Value, Utilities.EaseOutSine(factor));
                            }
                            else
                            {
                                // Relocation complete
                                position = RelocateTo.Value;
                                yOffset = 0;
                                StateMachine.Trigger(BetterBirdieTrigger.Stop);
                            }
                        }
                    })
                .GlobalTransitionTo(BetterBirdieState.FlyingAway).OnGlobal(BetterBirdieTrigger.FlyAway)
                .GlobalTransitionTo(BetterBirdieState.Relocating).OnGlobal(BetterBirdieTrigger.Relocate)
                .Build();

            StateMachine.AddStateChangeHandler((state, e) =>
            {
                ModEntry.instance.Monitor.Log($"{Birdie.id}: {e.From.ToString()} -> {e.To.ToString()}");
            });
        }
    }
}

