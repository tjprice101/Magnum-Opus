using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Common.Systems
{
    // ============================================================================
    // BOSS AI UTILITIES - Ported from InfernumMode patterns
    // Provides common AI behaviors, state management, and movement helpers.
    // ============================================================================

    /// <summary>
    /// Common attack state enum for boss state machines.
    /// Extend this or create your own for specific bosses.
    /// </summary>
    public enum BossAttackState
    {
        Idle = 0,
        PhaseTransition = 1,
        Despawning = 2,
        DeathAnimation = 3,
        // Attack states start at 100+
        Attack1 = 100,
        Attack2 = 101,
        Attack3 = 102,
        Attack4 = 103,
        Attack5 = 104,
        Attack6 = 105,
        Attack7 = 106,
        Attack8 = 107,
        // Special attacks at 200+
        SpecialAttack1 = 200,
        SpecialAttack2 = 201,
        SpecialAttack3 = 202,
        // Enraged attacks at 300+
        EnragedAttack1 = 300,
        EnragedAttack2 = 301,
        EnragedAttack3 = 302
    }

    /// <summary>
    /// Utility class for common boss AI behaviors.
    /// Inspired by Infernum's boss AI patterns.
    /// </summary>
    public static class BossAIUtilities
    {
        // ============================================================================
        // ATTACK SELECTION - Weighted random selection avoiding repeats
        // ============================================================================
        
        /// <summary>
        /// Selects a random attack from the available attacks, avoiding the last used attack.
        /// </summary>
        /// <typeparam name="T">Attack enum type</typeparam>
        /// <param name="availableAttacks">Array of available attacks</param>
        /// <param name="lastAttack">The last used attack to avoid</param>
        /// <returns>A new attack different from the last one</returns>
        public static T SelectNextAttack<T>(T[] availableAttacks, T lastAttack) where T : Enum
        {
            if (availableAttacks.Length <= 1)
                return availableAttacks[0];
            
            T selectedAttack;
            int attempts = 0;
            
            do
            {
                selectedAttack = availableAttacks[Main.rand.Next(availableAttacks.Length)];
                attempts++;
            }
            while (selectedAttack.Equals(lastAttack) && attempts < 10);
            
            return selectedAttack;
        }
        
        /// <summary>
        /// Selects a random attack with weights for each attack type.
        /// </summary>
        /// <typeparam name="T">Attack enum type</typeparam>
        /// <param name="attackWeights">Dictionary mapping attacks to their weights</param>
        /// <param name="lastAttack">Last attack to avoid (or default to not avoid)</param>
        /// <returns>Weighted random attack</returns>
        public static T SelectWeightedAttack<T>(Dictionary<T, float> attackWeights, T lastAttack = default) where T : Enum
        {
            float totalWeight = 0f;
            foreach (var weight in attackWeights.Values)
                totalWeight += weight;
            
            float randomValue = Main.rand.NextFloat() * totalWeight;
            float cumulativeWeight = 0f;
            
            T selectedAttack = default;
            int attempts = 0;
            
            do
            {
                randomValue = Main.rand.NextFloat() * totalWeight;
                cumulativeWeight = 0f;
                
                foreach (var kvp in attackWeights)
                {
                    cumulativeWeight += kvp.Value;
                    if (randomValue <= cumulativeWeight)
                    {
                        selectedAttack = kvp.Key;
                        break;
                    }
                }
                
                attempts++;
            }
            while (selectedAttack.Equals(lastAttack) && attempts < 10);
            
            return selectedAttack;
        }
        
        // ============================================================================
        // MOVEMENT BEHAVIORS
        // ============================================================================
        
        /// <summary>
        /// Smoothly moves the NPC toward a target position.
        /// </summary>
        /// <param name="npc">The NPC to move</param>
        /// <param name="targetPosition">Target position to move toward</param>
        /// <param name="speed">Movement speed</param>
        /// <param name="turnResistance">How resistant the NPC is to turning (higher = slower turns)</param>
        public static void SmoothFlyToward(NPC npc, Vector2 targetPosition, float speed, float turnResistance = 10f)
        {
            Vector2 idealVelocity = npc.SafeDirectionTo(targetPosition) * speed;
            npc.velocity = Vector2.Lerp(npc.velocity, idealVelocity, 1f / turnResistance);
        }
        
        /// <summary>
        /// Performs a charge/dash toward a target position.
        /// </summary>
        /// <param name="npc">The NPC to charge</param>
        /// <param name="chargeDestination">Target position to charge toward</param>
        /// <param name="chargeSpeed">Speed of the charge</param>
        public static void DoCharge(NPC npc, Vector2 chargeDestination, float chargeSpeed)
        {
            npc.velocity = npc.SafeDirectionTo(chargeDestination) * chargeSpeed;
        }
        
        /// <summary>
        /// Slows down the NPC's movement over time.
        /// </summary>
        /// <param name="npc">The NPC to slow</param>
        /// <param name="slowdownFactor">How quickly to slow (0-1, lower = faster slowdown)</param>
        public static void SlowDown(NPC npc, float slowdownFactor = 0.95f)
        {
            npc.velocity *= slowdownFactor;
        }
        
        /// <summary>
        /// Performs a hover behavior, oscillating around a position.
        /// </summary>
        /// <param name="npc">The NPC to hover</param>
        /// <param name="hoverPosition">Position to hover around</param>
        /// <param name="speed">Movement speed</param>
        /// <param name="amplitude">Oscillation amplitude</param>
        /// <param name="timer">Timer for oscillation (pass npc.ai[0] or similar)</param>
        public static void HoverAround(NPC npc, Vector2 hoverPosition, float speed, float amplitude, float timer)
        {
            float xOffset = (float)Math.Sin(timer * 0.05f) * amplitude;
            float yOffset = (float)Math.Cos(timer * 0.035f) * amplitude * 0.5f;
            
            Vector2 targetPos = hoverPosition + new Vector2(xOffset, yOffset);
            SmoothFlyToward(npc, targetPos, speed, 15f);
        }
        
        /// <summary>
        /// Performs circular movement around a center point.
        /// </summary>
        /// <param name="npc">The NPC to move</param>
        /// <param name="center">Center of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="angularSpeed">Speed of rotation in radians per frame</param>
        /// <param name="currentAngle">Current angle (pass by reference to update)</param>
        public static void CircleAround(NPC npc, Vector2 center, float radius, float angularSpeed, ref float currentAngle)
        {
            currentAngle += angularSpeed;
            Vector2 targetPos = center + new Vector2((float)Math.Cos(currentAngle), (float)Math.Sin(currentAngle)) * radius;
            SmoothFlyToward(npc, targetPos, Vector2.Distance(npc.Center, targetPos) * 0.1f + 5f, 8f);
        }
        
        // ============================================================================
        // TELEPORT BEHAVIORS
        // ============================================================================
        
        /// <summary>
        /// Teleports the NPC to a position with optional visual effects.
        /// </summary>
        /// <param name="npc">The NPC to teleport</param>
        /// <param name="targetPosition">Position to teleport to</param>
        /// <param name="primaryColor">Color for teleport effects</param>
        /// <param name="useParticles">Whether to spawn particles at departure and arrival</param>
        public static void TeleportTo(NPC npc, Vector2 targetPosition, Color primaryColor, bool useParticles = true)
        {
            if (useParticles)
            {
                // Departure effects
                CreateTeleportBurst(npc.Center, primaryColor);
            }
            
            npc.Center = targetPosition;
            npc.velocity = Vector2.Zero;
            npc.netUpdate = true;
            
            if (useParticles)
            {
                // Arrival effects
                CreateTeleportBurst(npc.Center, primaryColor);
            }
        }
        
        /// <summary>
        /// Teleports the NPC above the target player.
        /// </summary>
        public static void TeleportAboveTarget(NPC npc, Player target, float heightOffset, Color primaryColor, bool useParticles = true)
        {
            Vector2 targetPos = target.Center + new Vector2(0f, -heightOffset);
            TeleportTo(npc, targetPos, primaryColor, useParticles);
        }
        
        /// <summary>
        /// Teleports the NPC to a side of the target player.
        /// </summary>
        public static void TeleportToSideOfTarget(NPC npc, Player target, float horizontalOffset, Color primaryColor, bool useParticles = true)
        {
            float side = Main.rand.NextBool() ? -1f : 1f;
            Vector2 targetPos = target.Center + new Vector2(horizontalOffset * side, 0f);
            TeleportTo(npc, targetPos, primaryColor, useParticles);
        }
        
        /// <summary>
        /// Teleports the NPC to a random position around the target.
        /// </summary>
        public static void TeleportRandomlyAroundTarget(NPC npc, Player target, float minDistance, float maxDistance, Color primaryColor, bool useParticles = true)
        {
            Vector2 offset = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(minDistance, maxDistance);
            TeleportTo(npc, target.Center + offset, primaryColor, useParticles);
        }
        
        /// <summary>
        /// Creates visual effects for teleportation.
        /// </summary>
        public static void CreateTeleportBurst(Vector2 position, Color color)
        {
            // Dust burst
            for (int i = 0; i < 30; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.RainbowMk2, velocity, 0, color, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
            
            // Particle effects
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, color, 0f, 2f, 25);
                Particles.MagnumParticleHandler.SpawnParticle(ring);
                
                var bloom = new StrongBloomParticle(position, Vector2.Zero, color, 2f, 20);
                Particles.MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }
            
            MagnumScreenEffects.AddScreenShake(3f);
        }
        
        // ============================================================================
        // PHASE TRANSITION HELPERS
        // ============================================================================
        
        /// <summary>
        /// Performs a dramatic phase transition with effects.
        /// Call this over multiple frames during a transition state.
        /// </summary>
        /// <param name="npc">The boss NPC</param>
        /// <param name="transitionTimer">Current frame of the transition</param>
        /// <param name="totalTransitionTime">Total frames for the transition</param>
        /// <param name="primaryColor">Primary effect color</param>
        /// <param name="secondaryColor">Secondary effect color</param>
        /// <returns>True when transition is complete</returns>
        public static bool DoPhaseTransition(NPC npc, int transitionTimer, int totalTransitionTime, Color primaryColor, Color secondaryColor)
        {
            float progress = (float)transitionTimer / totalTransitionTime;
            
            // Slow down movement during transition
            npc.velocity *= 0.95f;
            
            // Periodic pulse effects
            if (transitionTimer % 10 == 0)
            {
                float pulse = (float)Math.Sin(transitionTimer * 0.15f) * 0.5f + 0.5f;
                Color pulseColor = Color.Lerp(primaryColor, secondaryColor, pulse);
                
                try
                {
                    var ring = new PulseRingParticle(npc.Center, Vector2.Zero, pulseColor, 0f, 2f + progress * 2f, 30);
                    Particles.MagnumParticleHandler.SpawnParticle(ring);
                }
                catch { }
            }
            
            // Building screen shake
            MagnumScreenEffects.AddScreenShake(2f + progress * 8f);
            
            // Climax at end
            if (transitionTimer >= totalTransitionTime - 1)
            {
                ExplosionUtility.CreateDeathExplosion(npc.Center, primaryColor, secondaryColor, 0.8f);
                return true;
            }
            
            return false;
        }
        
        // ============================================================================
        // DEATH ANIMATION HELPERS
        // ============================================================================
        
        /// <summary>
        /// Performs a dramatic death animation.
        /// Call this every frame during death state.
        /// </summary>
        /// <param name="npc">The dying boss NPC</param>
        /// <param name="deathTimer">Current frame of death animation</param>
        /// <param name="totalDeathTime">Total frames for death animation</param>
        /// <param name="primaryColor">Primary effect color</param>
        /// <param name="secondaryColor">Secondary effect color</param>
        /// <returns>True when animation is complete and NPC should die</returns>
        public static bool DoDeathAnimation(NPC npc, int deathTimer, int totalDeathTime, Color primaryColor, Color secondaryColor)
        {
            float progress = (float)deathTimer / totalDeathTime;
            
            // Stop all movement
            npc.velocity = Vector2.Zero;
            npc.dontTakeDamage = true;
            
            // Progressive electric arcs bursting out
            if (deathTimer % 5 == 0)
            {
                int arcCount = 1 + (int)(progress * 4f);
                for (int i = 0; i < arcCount; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f + progress * 10f);
                    Color arcColor = Main.rand.NextBool() ? primaryColor : secondaryColor;
                    
                    try
                    {
                        var arc = new ElectricArcParticle(npc.Center, velocity, arcColor, 1f, 40);
                        Particles.MagnumParticleHandler.SpawnParticle(arc);
                    }
                    catch { }
                }
            }
            
            // Intensifying screen shake
            MagnumScreenEffects.AddScreenShake(5f + progress * 15f);
            
            // Periodic explosion bursts
            if (deathTimer % 12 == 0)
            {
                Vector2 explosionPos = npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height) * 0.5f;
                ExplosionUtility.CreateFireExplosion(explosionPos, primaryColor, secondaryColor, 0.5f + progress * 0.5f);
            }
            
            // Final mega explosion
            if (deathTimer >= totalDeathTime - 1)
            {
                ExplosionUtility.CreateDeathExplosion(npc.Center, primaryColor, secondaryColor, 1.5f);
                MagnumScreenEffects.SetFlashEffect(npc.Center, 2f, 60);
                return true;
            }
            
            return false;
        }
        
        // ============================================================================
        // UTILITY METHODS
        // ============================================================================
        
        /// <summary>
        /// Gets the current target player for this boss.
        /// </summary>
        public static Player GetTarget(NPC npc)
        {
            if (npc.target < 0 || npc.target >= Main.maxPlayers)
                npc.TargetClosest();
            
            return Main.player[npc.target];
        }
        
        /// <summary>
        /// Checks if the boss should despawn (no valid target).
        /// </summary>
        public static bool ShouldDespawn(NPC npc)
        {
            Player target = GetTarget(npc);
            return !target.active || target.dead || Vector2.Distance(npc.Center, target.Center) > 5000f;
        }
        
        /// <summary>
        /// Smoothly rotates the NPC toward a target direction.
        /// </summary>
        public static void RotateTowardTarget(NPC npc, Vector2 targetPosition, float rotationSpeed = 0.1f)
        {
            float targetRotation = npc.AngleTo(targetPosition);
            npc.rotation = npc.rotation.AngleLerp(targetRotation, rotationSpeed);
        }
        
        /// <summary>
        /// Sets the NPC's frame based on animation requirements.
        /// </summary>
        public static void AnimateFrames(NPC npc, int frameCount, int framesPerAnimation, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter >= framesPerAnimation)
            {
                frameCounter = 0;
                npc.frame.Y += npc.frame.Height;
                if (npc.frame.Y >= npc.frame.Height * frameCount)
                    npc.frame.Y = 0;
            }
        }
        
        /// <summary>
        /// Creates a warning telegraph line from boss to target.
        /// </summary>
        public static void CreateTelegraphLine(Vector2 start, Vector2 end, Color color, float width = 2f)
        {
            // This creates a dust line telegraph
            float distance = Vector2.Distance(start, end);
            Vector2 direction = (end - start).SafeNormalize(Vector2.Zero);
            
            for (float i = 0; i < distance; i += 20f)
            {
                Vector2 pos = start + direction * i;
                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, Vector2.Zero, 0, color, width * 0.8f);
                dust.noGravity = true;
                dust.noLight = false;
            }
        }
        
        /// <summary>
        /// Creates a warning telegraph circle around a position.
        /// </summary>
        public static void CreateTelegraphCircle(Vector2 position, float radius, Color color, int dustCount = 36)
        {
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 pos = position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, Vector2.Zero, 0, color, 1.5f);
                dust.noGravity = true;
            }
        }
    }
    
    // ============================================================================
    // EXTENSION METHODS
    // ============================================================================
    
    /// <summary>
    /// Extension methods for NPC class, inspired by Infernum utilities.
    /// </summary>
    public static class NPCExtensions
    {
        /// <summary>
        /// Gets a safe direction from this NPC to a target position.
        /// Returns Vector2.Zero if positions are the same.
        /// </summary>
        public static Vector2 SafeDirectionTo(this NPC npc, Vector2 destination)
        {
            return (destination - npc.Center).SafeNormalize(Vector2.Zero);
        }
        
        /// <summary>
        /// Gets the angle from this NPC to a target position.
        /// </summary>
        public static float AngleTo(this NPC npc, Vector2 destination)
        {
            return (destination - npc.Center).ToRotation();
        }
        
        /// <summary>
        /// Checks if the NPC is within a certain distance of a position.
        /// </summary>
        public static bool WithinRange(this NPC npc, Vector2 position, float range)
        {
            return Vector2.DistanceSquared(npc.Center, position) < range * range;
        }
    }
    
    // ============================================================================
    // TARGETING UTILITIES - For homing projectiles and minions
    // ============================================================================
    
    /// <summary>
    /// Utility class for proper hitbox-based targeting instead of center-only targeting.
    /// This ensures projectiles can hit any pixel on an enemy's hitbox, not just the center.
    /// </summary>
    public static class TargetingUtilities
    {
        /// <summary>
        /// Gets a random point within an NPC's hitbox for targeting.
        /// This allows homing projectiles to hit anywhere on the enemy, not just the center.
        /// </summary>
        /// <param name="npc">The target NPC</param>
        /// <returns>A random point within the NPC's hitbox</returns>
        public static Vector2 GetRandomHitboxPoint(NPC npc)
        {
            return new Vector2(
                npc.position.X + Main.rand.NextFloat(npc.width),
                npc.position.Y + Main.rand.NextFloat(npc.height)
            );
        }
        
        /// <summary>
        /// Gets a weighted random point within an NPC's hitbox, biased toward the center.
        /// Use this for a balance between random targeting and center targeting.
        /// </summary>
        /// <param name="npc">The target NPC</param>
        /// <param name="centerBias">0 = fully random, 1 = center only. Default 0.3f</param>
        /// <returns>A point within the NPC's hitbox</returns>
        public static Vector2 GetBiasedHitboxPoint(NPC npc, float centerBias = 0.3f)
        {
            Vector2 randomPoint = GetRandomHitboxPoint(npc);
            return Vector2.Lerp(randomPoint, npc.Center, centerBias);
        }
        
        /// <summary>
        /// Gets the closest point on an NPC's hitbox to a given position.
        /// Useful for projectiles that should target the nearest edge of an enemy.
        /// </summary>
        /// <param name="npc">The target NPC</param>
        /// <param name="fromPosition">The position to measure from (e.g., projectile center)</param>
        /// <returns>The closest point on the NPC's hitbox edge</returns>
        public static Vector2 GetClosestHitboxPoint(NPC npc, Vector2 fromPosition)
        {
            Rectangle hitbox = npc.Hitbox;
            float closestX = MathHelper.Clamp(fromPosition.X, hitbox.Left, hitbox.Right);
            float closestY = MathHelper.Clamp(fromPosition.Y, hitbox.Top, hitbox.Bottom);
            return new Vector2(closestX, closestY);
        }
        
        /// <summary>
        /// Gets a target point for homing that varies based on projectile lifetime.
        /// This prevents all projectiles from converging on the same point.
        /// </summary>
        /// <param name="npc">The target NPC</param>
        /// <param name="projectileIndex">Unique index for this projectile (use Projectile.whoAmI)</param>
        /// <returns>A consistent but varied target point</returns>
        public static Vector2 GetVariedTargetPoint(NPC npc, int projectileIndex)
        {
            // Use projectile index to create consistent but different offsets for each projectile
            float offsetX = (float)Math.Sin(projectileIndex * 0.7f) * npc.width * 0.4f;
            float offsetY = (float)Math.Cos(projectileIndex * 0.5f) * npc.height * 0.4f;
            return npc.Center + new Vector2(offsetX, offsetY);
        }
        
        /// <summary>
        /// Checks if a projectile can hit an NPC's hitbox (not just center).
        /// Use this for line-of-sight checks that respect the full hitbox.
        /// </summary>
        /// <param name="fromPosition">Starting position</param>
        /// <param name="npc">Target NPC</param>
        /// <returns>True if any part of the hitbox is visible</returns>
        public static bool CanHitHitbox(Vector2 fromPosition, NPC npc)
        {
            // Check multiple points on the hitbox for line of sight
            Vector2[] checkPoints = new Vector2[]
            {
                npc.Center,
                new Vector2(npc.position.X, npc.position.Y),                          // Top-left
                new Vector2(npc.position.X + npc.width, npc.position.Y),              // Top-right
                new Vector2(npc.position.X, npc.position.Y + npc.height),             // Bottom-left
                new Vector2(npc.position.X + npc.width, npc.position.Y + npc.height), // Bottom-right
            };
            
            foreach (var point in checkPoints)
            {
                if (Collision.CanHitLine(fromPosition, 1, 1, point, 1, 1))
                    return true;
            }
            
            return false;
        }
    }
    
    /// <summary>
    /// Extension methods for float (angles).
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Lerps between two angles, handling wrap-around properly.
        /// </summary>
        public static float AngleLerp(this float current, float target, float amount)
        {
            float difference = MathHelper.WrapAngle(target - current);
            return current + difference * amount;
        }
    }
}
