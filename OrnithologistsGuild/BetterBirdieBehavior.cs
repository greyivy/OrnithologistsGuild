using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.BellsAndWhistles;
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
        FlyingAway
    }

    public enum BetterBirdieTrigger
    {
        Peck,
        Walk,
        Stop,
        Hop,
        FlyAway,
        Sleep
    }

    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        private int characterCheckTimer = 200;

        private int walkTimer;

        public Fsm<BetterBirdieState, BetterBirdieTrigger> StateMachine;

        private GameLocation Environment;

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
                            switch (Game1.random.Next(6))
                            {
                                case 0:
                                    StateMachine.Trigger(BetterBirdieTrigger.Sleep);
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
                                } else
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
                                } else
                                {
                                    flip = !flip;
                                }
                            }
                        } else if (yJumpOffset >= 0)
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

                        walkTimer = Game1.random.Next(5, 25) * 100;
                    })
                    .OnExit(e =>
                    {
                        // Stop walk animation
                        sprite.loop = false;
                        sprite.CurrentAnimation = null;
                    })
                    .Update(a =>
                    {
                        walkTimer -= a.ElapsedTimeSpan.Milliseconds;

                        if (!flip)
                        {
                            if (!(Perch != null && position.X < startingPosition.X - 1f) && !Environment.isCollidingPosition(getBoundingBox(-1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                            {
                                position.X -= 1f;
                            } else
                            {
                                // Can't walk left
                                if (Perch == null)
                                {
                                    flip = !flip;
                                } else
                                {
                                    StateMachine.Trigger(BetterBirdieTrigger.Stop);
                                }
                            }
                        } else
                        {
                            if (!(Perch != null && position.X > startingPosition.X + 3f) && !Environment.isCollidingPosition(getBoundingBox(1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                            {
                                position.X += 1f;
                            } else
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

                        if (walkTimer <= 0)
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
                                    if (Utility.isOnScreen(position, 64))
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
                        if (Game1.random.NextDouble() < 0.003)
                        {
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                        }
                    })
                .State(BetterBirdieState.FlyingAway) // Done! // TODO sounds, fly speed
                    .OnEnter(e =>
                    {
                        if (Game1.random.NextDouble() < 0.85)
                        {
                            Game1.playSound("SpringBirds");
                        }

                        sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame> {
                            new FarmerSprite.AnimationFrame (baseFrame + 6, (int)Math.Round(0.27 * (float)Birdie.flapDuration)),
                            new FarmerSprite.AnimationFrame (baseFrame + 7, (int)Math.Round(0.23 * (float)Birdie.flapDuration), secondaryArm: false, flip, frameBehavior: (Farmer who) =>
                            {
                                // Make bird shoot up a bit while flapping for more realistic flight
                                // e.g. flapDuration = 500, gravityAffectedDY = 4
                                // e.g. flapDuration = 250, gravityAffectedDY = 2
                                gravityAffectedDY = (float)Math.Round((float)Birdie.flapDuration * (4f/500f));

                                // Play flapping noise
                                if (Utility.isOnScreen(position, 64)) Game1.playSound("batFlap");
                            }),
                            new FarmerSprite.AnimationFrame (baseFrame + 8, (int)Math.Round(0.27 * (float)Birdie.flapDuration)),
                            new FarmerSprite.AnimationFrame (baseFrame + 7, (int)Math.Round(0.23 * (float)Birdie.flapDuration))
                        });
                        sprite.loop = true;
                    })
                    .Update(a =>
                    {
                        if (!flip)
                        {
                            position.X -= 6f;
                        }
                        else
                        {
                            position.X += 6f;
                        }
                        yOffset -= 2f + flightOffset;
                    })
                .GlobalTransitionTo(BetterBirdieState.FlyingAway).OnGlobal(BetterBirdieTrigger.FlyAway)
                .Build();

            StateMachine.AddStateChangeHandler((state, e) =>
            {
                ModEntry.instance.Monitor.Log($"{Birdie.id}: {e.From.ToString()} -> {e.To.ToString()}");
            });
        }

        private void CheckCharacterProximity(GameTime time, GameLocation environment)
        {
            characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
            if (characterCheckTimer < 0)
            {
                Character character = Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 4, environment);
                characterCheckTimer = 200;

                if (character != null && StateMachine.Current.Identifier != BetterBirdieState.FlyingAway)
                {
                    if (character.Position.X > position.X)
                    {
                        flip = false;
                    }
                    else
                    {
                        flip = true;
                    }

                    StateMachine.Trigger(BetterBirdieTrigger.FlyAway);
                }
            }
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            Environment = environment;

            CheckCharacterProximity(time, environment);

            StateMachine.Update(time.ElapsedGameTime);

            return base.update(time, environment);
        }
    }
}

