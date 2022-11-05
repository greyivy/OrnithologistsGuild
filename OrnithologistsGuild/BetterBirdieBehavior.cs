using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.BellsAndWhistles;

namespace OrnithologistsGuild
{
    public enum BetterBirdieState
    {
        Pecking,
        FlyingAway,
        Sleeping,
        Stopped,
        Walking
    }

    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        private BetterBirdieState State;

        private float flightOffset;

        private int characterCheckTimer = 200;

        private int walkTimer;
        private int gravTimer;

        private void Log(string message)
        {
            ModEntry.instance.Monitor.Log($"{Birdie.id}: {message}");
        }

        private void Hop(Farmer who)
        {
            gravityAffectedDY = -2f;
        }

        private void CheckCharacterProximity(GameTime time, GameLocation environment)
        {
            characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
            if (characterCheckTimer < 0)
            {
                Character character = Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 4, environment);
                characterCheckTimer = 200;

                if (character != null && State != BetterBirdieState.FlyingAway)
                {
                    Log("CheckCharacterProximity got too close, fly away!");

                    if (Game1.random.NextDouble() < 0.85)
                    {
                        Game1.playSound("SpringBirds");
                    }

                    State = BetterBirdieState.FlyingAway;
                    gravTimer = Game1.random.Next(2, 7) * 100;

                    if (character.Position.X > position.X)
                    {
                        flip = false;
                    }
                    else
                    {
                        flip = true;
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
                }
            }
        }

        // Good!
        private void DoStatePecking(GameTime time, GameLocation environment)
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
                    // Done pecking -- stop or peck again
                    State = ((!(Game1.random.NextDouble() < 0.5)) ? BetterBirdieState.Stopped : BetterBirdieState.Pecking);
                }));

                sprite.loop = false;
                sprite.setCurrentAnimation(list);
            }
        }

        private void DoStateFlyingAway(GameTime time)
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

            gravTimer -= time.ElapsedGameTime.Milliseconds;
            if (gravTimer < 0) {
                // TODO testing out the way chickadees fly where they kinda go up and down a bit.. it looks great!
                //gravityAffectedDY = -4f;

                //gravTimer = Game1.random.Next(2, 7) * 100;
            }

            // TODO see if we can fly to an open space sometimes!?
        }

        // Good!
        private void DoStateSleeping()
        {
            if (sprite.CurrentAnimation == null)
            {
                sprite.currentFrame = baseFrame + 5;
            }
            if (Game1.random.NextDouble() < 0.003 && sprite.CurrentAnimation == null)
            {
                State = BetterBirdieState.Stopped;
            }
        }

        private void DoStateWalking(GameTime time, GameLocation environment)
        {
            // TODO perhaps feeder perch should specify left and right (e.g. to help tube feeder that is off center)
            // Also: i don't think birds should try to walk/jump off the edge to be stopped by the perch bounds. It looks unnatural. just don't walk/jump if at the edge
            if (!flip && !(Perch != null && position.X < startingPosition.X - 1f) && !environment.isCollidingPosition(getBoundingBox(-1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
            {
                position.X -= 1f;
            }
            else if (flip && !(Perch != null && position.X > startingPosition.X + 3f) && !environment.isCollidingPosition(getBoundingBox(1, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
            {
                position.X += 1f;
            }
            walkTimer -= time.ElapsedGameTime.Milliseconds;
            if (walkTimer < 0)
            {
                State = BetterBirdieState.Stopped;
                sprite.loop = false;
                sprite.CurrentAnimation = null;
                sprite.currentFrame = baseFrame;
            }
        }

        private void DoStateStopped(GameTime time, GameLocation environment)
        {
            if (Game1.random.NextDouble() < 0.008 && sprite.CurrentAnimation == null && yJumpOffset >= 0f)
            {
                switch (Game1.random.Next(6))
                {
                    case 0:
                        State = BetterBirdieState.Sleeping;
                        break;
                    case 1:
                        State = BetterBirdieState.Pecking;
                        break;
                    case 2:
                        Hop(null);
                        break;
                    case 3:
                        flip = !flip;
                        Hop(null);
                        break;
                    case 4:
                    case 5:
                        State = BetterBirdieState.Walking;

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

                        walkTimer = Game1.random.Next(5, 15) * 100;
                        break;
                }
            }
            else if (sprite.CurrentAnimation == null)
            {
                sprite.currentFrame = baseFrame;
            }
        }

        public override bool update(GameTime time, GameLocation environment)
        {
            if (yJumpOffset < 0f && State != BetterBirdieState.FlyingAway)
            {
                // This moves left or right when jumping!
                if (!flip && !(Perch != null && position.X <= startingPosition.X) && !environment.isCollidingPosition(getBoundingBox(-2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                {
                    position.X -= 2f;
                }
                else if (!(Perch != null && position.X > startingPosition.X + 1) && !environment.isCollidingPosition(getBoundingBox(2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
                {
                    position.X += 2f;
                }
            }

            CheckCharacterProximity(time, environment);

            switch (State)
            {
                case BetterBirdieState.Pecking:
                    DoStatePecking(time, environment);
                    break;
                case BetterBirdieState.FlyingAway:
                    DoStateFlyingAway(time);
                    break;

                case BetterBirdieState.Sleeping:
                    DoStateSleeping();
                    break;

                case BetterBirdieState.Walking:
                    DoStateWalking(time, environment);
                    break;

                case BetterBirdieState.Stopped:
                    DoStateStopped(time, environment);
                    break;
            }

            //// TODO
            //if (Perch != null && this.yOffset == 0)
            //{
            //    // Bird likely not flying away -- keep stationary
            //    this.position = this.startingPosition;
            //}

            return base.update(time, environment);
        }
    }
}

