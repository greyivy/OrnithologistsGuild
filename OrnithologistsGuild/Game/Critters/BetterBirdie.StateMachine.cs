using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OrnithologistsGuild.Content;
using OrnithologistsGuild.Models;
using StardewValley;
using StateMachine;

namespace OrnithologistsGuild.Game.Critters
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
        Relocating,
        Bathing,
        Swimming
    }

    public enum BetterBirdieTrigger
    {
        Peck,
        Walk,
        Stop,
        Hop,
        FlyAway,
        Sleep,
        Relocate,
        Bathe,
        Swim
    }

    public partial class BetterBirdie : StardewValley.BellsAndWhistles.Critter
    {
        // Relocate
        private Vector3? RelocateFrom;
        private BirdiePosition RelocateTo;
        private float? RelocateDistance;
        private int? RelocateDuration;
        private int? RelocateElapsed;

        // Swim
        private Vector2? SwimFrom;
        private Vector2? SwimTo;
        private float? SwimDistance;
        private int? SwimDuration;
        private int? SwimElapsed;

        private void InitializeStateMachine()
        {
            StateMachine = Fsm<BetterBirdieState, BetterBirdieTrigger>.Builder(BetterBirdieState.Stopping)
                .State(BetterBirdieState.Stopping)
                    .TransitionTo(BetterBirdieState.Stopped).On(BetterBirdieTrigger.Stop)
                    .Update(a =>
                    {
                        // Wait for current animation or emote to stop
                        if (sprite.CurrentAnimation == null && !isEmoting) StateMachine.Trigger(BetterBirdieTrigger.Stop);
                    })
                .State(BetterBirdieState.Stopped)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .TransitionTo(BetterBirdieState.Sleeping).On(BetterBirdieTrigger.Sleep)
                    .TransitionTo(BetterBirdieState.Pecking).On(BetterBirdieTrigger.Peck)
                    .TransitionTo(BetterBirdieState.Walking).On(BetterBirdieTrigger.Walk)
                    .TransitionTo(BetterBirdieState.Hopping).On(BetterBirdieTrigger.Hop)
                    .TransitionTo(BetterBirdieState.Bathing).On(BetterBirdieTrigger.Bathe)
                    .TransitionTo(BetterBirdieState.Swimming).On(BetterBirdieTrigger.Swim)
                    .OnEnter(e =>
                    {
                        // Reset animation to base frame
                        sprite.currentFrame = IsInWater ? baseFrame + 9 : baseFrame;

                        var weight = BirdieDef.GetContextualWeight();
                        if (BirdieDef.GetContextualWeight() == 0f)
                        {
                            // Despawn birds e.g. at night
                            AfterBetween(TimeSpan.Zero, TimeSpan.FromSeconds(60),
                                () => StateMachine.Trigger(BetterBirdieTrigger.FlyAway), null);
                        }

                        var contextualBehavior = GetContextualBehavior();
                        var nextBehavior = Utilities.WeightedRandom(contextualBehavior, b => b.Weight);

                        if (nextBehavior.Immediate)
                        {
                            // Execute next action immediately
                            StateMachine.Trigger(nextBehavior.Action());
                        } else
                        {
                            // Wait a little while before executing next action
                            AfterBetween(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(5),
                                () => StateMachine.Trigger(nextBehavior.Action()), null);
                        }
                    })
                .State(BetterBirdieState.Hopping)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        // Hop!
                        gravityAffectedDY = -Utility.RandomFloat(1, 4);
                    })
                    .Update(a =>
                    {
                        if (gravityAffectedDY >= 0f && yJumpOffset >= 0f) {
                            // Done hopping
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                            return;
                        }

                        // Hop left or right
                        if (!flip && CanMoveLeft()) position.X -= 2f;
                        else if (flip && CanMoveRight()) position.X += 2f;
                    })
                .State(BetterBirdieState.Walking)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        // Start walk animation
                        sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame> {
                            new FarmerSprite.AnimationFrame ((short)baseFrame, 100),
                            new FarmerSprite.AnimationFrame ((short)(baseFrame + 1), 100)
                        });
                        sprite.loop = true;

                        if (position.X >= startingPosition.X) flip = false;
                        else flip = true;

                        AfterBetween(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(5),
                            () => StateMachine.Trigger(BetterBirdieTrigger.Stop), BetterBirdieState.Walking);
                    })
                    .OnExit(e =>
                    {
                        // Stop walk animation
                        sprite.loop = false;
                        sprite.CurrentAnimation = null;
                    })
                    .Update(a =>
                    {
                        var canWalkLeft = CanMoveLeft();
                        var canWalkRight = CanMoveRight();

                        if (!canWalkLeft && !canWalkRight)
                        {
                            StateMachine.Trigger(BetterBirdieTrigger.Relocate);
                            return;
                        }

                        // Move left and right
                        // TODO sometimes bird flip rapidly
                        if (!flip)
                        {
                            if (canWalkLeft) position.X -= 1f;
                            else Flip();
                        }
                        else
                        {
                            if (canWalkRight) position.X += 1f;
                            else Flip();
                        }

                        // Move up and down randomly
                        switch(Game1.random.Next(3))
                        {
                            case 0:
                                break;
                            case 1:
                                position.Y += 0.5f;
                                break;
                            case 2:
                                position.Y -= 0.5f;
                                break;
                        }
                    })
                .State(BetterBirdieState.Swimming)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        var maxSwimDistance = 3f * Game1.tileSize;

                        // Get a random position somewhere within a circle around the bird
                        var r = maxSwimDistance * MathF.Sqrt(Utility.RandomFloat(0f, 1f));
                        var theta = Utility.RandomFloat(0f, 1f) * 2 * MathF.PI;

                        var swimTo = new Vector2(position.X + r * MathF.Cos(theta), position.Y + r * MathF.Sin(theta));

                        for (int trial = 0; trial < 5; trial++)
                        {
                            if (Environment.isOpenWater((int)swimTo.X / Game1.tileSize, (int)swimTo.Y / Game1.tileSize))
                            {
                                SwimTo = swimTo;
                                break;
                            }
                        }

                        if (SwimTo.HasValue)
                        {
                            SwimFrom = position;

                            SwimDistance = Vector2.Distance(position, SwimTo.Value);

                            SwimDuration = (int)(SwimDistance.Value / Utility.RandomFloat(0.02f, 0.06f));
                            SwimElapsed = 0;

                            if (position.X > SwimTo.Value.X) flip = false;
                            else flip = true;

                            Splash(
                                0.5f + ((int)SwimDistance / maxSwimDistance),
                                flip ? 0.75f : -0.75f
                            );

                            sprite.currentFrame = baseFrame + 10;
                        }
                        else
                        {
                            StateMachine.Trigger(BetterBirdieTrigger.Stop);
                        }
                    })
                    .OnExit(e =>
                    {
                        if (SwimTo.HasValue)
                        {
                            // Clean up
                            SwimFrom = null;
                            SwimTo = null;

                            SwimDistance = null;

                            SwimDuration = null;
                            SwimElapsed = null;
                        }
                    })
                    .Update(a =>
                    {
                        if (SwimTo.HasValue)
                        {
                            // Swim to tile
                            SwimElapsed += a.ElapsedTimeSpan.Milliseconds;
                            if (SwimElapsed <= SwimDuration)
                            {
                                var factor = (float)SwimElapsed.Value / (float)SwimDuration.Value;

                                position = Vector2.Lerp(SwimFrom.Value, SwimTo.Value, Utilities.EaseOutSine(factor));
                            }
                            else
                            {
                                // Relocation complete
                                Position3 = new Vector3(SwimTo.Value, yOffset);
                                startingPosition = position;

                                StateMachine.Trigger(BetterBirdieTrigger.Stop);
                            }
                        }

                    })
                .State(BetterBirdieState.Pecking)
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
                                list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 4), 100, secondaryArm: false, flip,
                                    _ => Environment.localSound("shiny4", TileLocation))); // Pecking noise
                            }

                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 3), 100));
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 2), 70, secondaryArm: false, flip));
                            list.Add(new FarmerSprite.AnimationFrame((short)(baseFrame + 1), 70, secondaryArm: false, flip));
                            list.Add(new FarmerSprite.AnimationFrame((short)baseFrame, 500, secondaryArm: false, flip,
                                _ => StateMachine.Trigger(BetterBirdieTrigger.Stop)));

                            sprite.setCurrentAnimation(list);
                            sprite.loop = false;
                        }
                    })
                .State(BetterBirdieState.Sleeping)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        sprite.currentFrame = baseFrame + 5;

                        AfterBetween(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15),
                            () => StateMachine.Trigger(BetterBirdieTrigger.Stop), BetterBirdieState.Sleeping);
                    })
                    .Update(a =>
                    {
                        if (!isEmoting && Game1.random.NextDouble() < 0.0025) doEmote(Character.sleepEmote);
                    })
                .State(BetterBirdieState.FlyingAway)
                    .OnEnter(e =>
                    {
                        // No longer perched
                        Perch = null;
                        stopEmote();

                        Character character = Utility.isThereAFarmerOrCharacterWithinDistance(position / Game1.tileSize, BirdieDef.GetContextualCautiousness(), Environment);

                        // Fly away from nearest character
                        if (character != null)
                        {
                            if (character.Position.X > position.X) flip = false;
                            else flip = true;
                        }

                        if (Game1.random.NextDouble() < 0.85) PlayCall();

                        sprite.setCurrentAnimation(GetFlyingAnimation());
                        sprite.loop = true;
                    })
                    .Update(a =>
                    {
                        if (!flip) position.X -= FlySpeed; // Left
                        else position.X += FlySpeed; // Right

                        yOffset -= 2f;
                    })
                .State(BetterBirdieState.Relocating)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        BirdiePosition relocateTo;
                        if (!ModEntry.debug_BirdWhisperer.HasValue)
                        {
                            relocateTo = GetRandomPositionOrPerch();
                        } else
                        {
                            relocateTo = new (Position: new Vector3(ModEntry.debug_BirdWhisperer.Value.X, ModEntry.debug_BirdWhisperer.Value.Y, 0), Perch: null);
                            ModEntry.debug_BirdWhisperer = null;
                        }

                        if (relocateTo != null)
                        {
                            stopEmote();

                            // Immediately update perch to prevent collisions
                            Perch = relocateTo.Perch;

                            RelocateFrom = Position3;
                            RelocateTo = relocateTo;

                            RelocateDistance = Vector2.Distance(position, Utilities.XY(relocateTo.Position));

                            RelocateDuration = (int)(RelocateDistance.Value / (FlySpeed / 15f));
                            RelocateElapsed = 0;

                            if (position.X > RelocateTo.Position.X) flip = false;
                            else flip = true;

                            if (Game1.random.NextDouble() < 0.8) PlayCall();

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
                        if (RelocateTo != null) {
                            // Stop fly animation
                            sprite.loop = false;
                            sprite.CurrentAnimation = null;

                            // Clean up
                            RelocateFrom = null;
                            RelocateTo = null;

                            RelocateDistance = null;

                            RelocateDuration = null;
                            RelocateElapsed = null;

                            MaybeBuildNest();
                        }
                    })
                    .Update(a =>
                    {
                        if (RelocateTo != null)
                        {
                            // Fly to tile
                            RelocateElapsed += a.ElapsedTimeSpan.Milliseconds;
                            if (RelocateElapsed <= RelocateDuration)
                            {
                                var factor = (float)RelocateElapsed.Value / (float)RelocateDuration.Value;

                                var midPointZ = ((RelocateFrom.Value.Z + RelocateTo.Position.Z) / 2) - (RelocateDistance.Value / 6f); // Midpoint of Z values + (distance / 6)

                                // Fly in an arc
                                // Note: yOffset is Z
                                if (factor < 0.5f)
                                {
                                    // Fly up to mid point
                                    var arcFactor = factor * 2f;
                                    yOffset = Utility.Lerp(RelocateFrom.Value.Z, midPointZ, Utilities.EaseOutSine(arcFactor));
                                }
                                else
                                {
                                    // Fly down from mid point
                                    var arcFactor = (factor - 0.5f) * 2f;
                                    yOffset = Utility.Lerp(midPointZ, RelocateTo.Position.Z, Utilities.EaseOutSine(arcFactor));
                                }

                                position = Vector2.Lerp(Utilities.XY(RelocateFrom.Value), Utilities.XY(RelocateTo.Position), Utilities.EaseOutSine(factor));
                            }
                            else
                            {
                                // Relocation complete
                                Position3 = RelocateTo.Position;
                                startingPosition = position;
   
                                if (IsPerched && Perch.Type == PerchType.Tree)
                                {
                                    // Shake tree on landing
                                    ModEntry.Instance.Helper.Reflection.GetMethod(Perch.Tree, "shake").Invoke(Perch.Tree.Tile, false);
                                }

                                if (IsInWater) Splash(1.75f);

                                StateMachine.Trigger(BetterBirdieTrigger.Stop);
                            }
                        }
                    })
                .State(BetterBirdieState.Bathing)
                    .TransitionTo(BetterBirdieState.Stopping).On(BetterBirdieTrigger.Stop)
                    .OnEnter(e =>
                    {
                        sprite.setCurrentAnimation(GetBathingAnimation());
                        sprite.loop = true;

                        AfterBetween(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(2.5),
                            () => StateMachine.Trigger(BetterBirdieTrigger.Stop), BetterBirdieState.Bathing);
                    })
                    .OnExit(e =>
                    {
                        sprite.loop = false;
                        sprite.CurrentAnimation = null;
                    })
                .GlobalTransitionTo(BetterBirdieState.FlyingAway).OnGlobal(BetterBirdieTrigger.FlyAway)
                .GlobalTransitionTo(BetterBirdieState.Relocating).OnGlobal(BetterBirdieTrigger.Relocate)
                .Build();

            StateMachine.AddStateChangeHandler((state, e) =>
            {
                ClearDelayedActionsInitiatedBy(e.From.Identifier);
                ModEntry.Instance.Monitor.Log($"{BirdieDef.ID}: {e.From} -> {e.To}");
            });
        }
    }
}
