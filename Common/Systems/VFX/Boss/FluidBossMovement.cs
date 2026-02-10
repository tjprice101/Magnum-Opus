using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Boss AI Fluidity System providing Calamity-style smooth movement.
    /// 
    /// Instead of instant velocity changes, bosses use acceleration and drag
    /// for organic, weighty movement that feels powerful.
    /// 
    /// Core Formula: Velocity += (Target - Velocity) * EaseInFactor
    /// This creates smooth asymptotic approach to target velocity.
    /// </summary>
    public static class FluidBossMovement
    {
        #region Core Movement Patterns
        
        /// <summary>
        /// Applies smooth acceleration toward a target velocity.
        /// The boss gradually speeds up/slows down instead of instant changes.
        /// </summary>
        /// <param name="currentVelocity">Current velocity (modified)</param>
        /// <param name="targetVelocity">Desired velocity</param>
        /// <param name="acceleration">How fast to approach target (0.01-0.2 typical)</param>
        /// <returns>New velocity after acceleration</returns>
        public static Vector2 ApplyAcceleration(Vector2 currentVelocity, Vector2 targetVelocity, float acceleration)
        {
            return currentVelocity + (targetVelocity - currentVelocity) * MathHelper.Clamp(acceleration, 0.001f, 1f);
        }
        
        /// <summary>
        /// Applies friction/drag to slow down movement.
        /// </summary>
        /// <param name="velocity">Current velocity</param>
        /// <param name="drag">Drag coefficient (0.95-0.99 for smooth, 0.8-0.9 for heavy)</param>
        /// <returns>Velocity after drag applied</returns>
        public static Vector2 ApplyDrag(Vector2 velocity, float drag)
        {
            return velocity * MathHelper.Clamp(drag, 0f, 1f);
        }
        
        /// <summary>
        /// Complete movement update combining acceleration and drag.
        /// Use this as your standard boss movement update.
        /// </summary>
        public static Vector2 UpdateMovement(Vector2 currentVelocity, Vector2 targetVelocity, float acceleration, float drag)
        {
            // First apply acceleration toward target
            Vector2 newVelocity = ApplyAcceleration(currentVelocity, targetVelocity, acceleration);
            
            // Then apply drag for natural slowdown when not actively moving
            if (targetVelocity.LengthSquared() < 1f)
            {
                newVelocity = ApplyDrag(newVelocity, drag);
            }
            
            return newVelocity;
        }
        
        /// <summary>
        /// Moves toward a target position with smooth acceleration.
        /// </summary>
        /// <param name="npc">The NPC to move</param>
        /// <param name="targetPosition">Where to move</param>
        /// <param name="maxSpeed">Maximum movement speed</param>
        /// <param name="acceleration">Acceleration factor</param>
        /// <param name="drag">Drag when near target</param>
        public static void MoveToward(NPC npc, Vector2 targetPosition, float maxSpeed, float acceleration, float drag = 0.95f)
        {
            Vector2 toTarget = targetPosition - npc.Center;
            float distance = toTarget.Length();
            
            if (distance < 5f)
            {
                // Very close - just apply drag
                npc.velocity = ApplyDrag(npc.velocity, drag);
                return;
            }
            
            // Calculate target velocity (direction * speed)
            Vector2 direction = toTarget / distance;
            
            // Scale speed based on distance for smooth arrival
            float speedMult = Math.Min(1f, distance / 200f);
            Vector2 targetVelocity = direction * maxSpeed * speedMult;
            
            // Apply acceleration
            npc.velocity = UpdateMovement(npc.velocity, targetVelocity, acceleration, drag);
        }
        
        /// <summary>
        /// Orbits around a target position with smooth movement.
        /// </summary>
        /// <param name="npc">The NPC to move</param>
        /// <param name="targetPosition">Center to orbit around</param>
        /// <param name="orbitRadius">Desired orbit distance</param>
        /// <param name="orbitSpeed">Angular speed in radians per frame</param>
        /// <param name="acceleration">Acceleration factor</param>
        public static void OrbitAround(NPC npc, Vector2 targetPosition, float orbitRadius, float orbitSpeed, float acceleration)
        {
            Vector2 toCenter = npc.Center - targetPosition;
            float currentRadius = toCenter.Length();
            float currentAngle = toCenter.ToRotation();
            
            // Calculate next orbit position
            float nextAngle = currentAngle + orbitSpeed;
            float nextRadius = MathHelper.Lerp(currentRadius, orbitRadius, 0.05f);
            
            Vector2 targetPos = targetPosition + nextAngle.ToRotationVector2() * nextRadius;
            Vector2 toTarget = targetPos - npc.Center;
            
            float speed = orbitRadius * Math.Abs(orbitSpeed) * 1.2f; // Slightly faster than needed for smooth orbiting
            Vector2 targetVelocity = toTarget.SafeNormalize(Vector2.Zero) * speed;
            
            npc.velocity = ApplyAcceleration(npc.velocity, targetVelocity, acceleration);
        }
        
        /// <summary>
        /// Performs a dash with acceleration buildup and drag slowdown.
        /// </summary>
        /// <param name="npc">The NPC dashing</param>
        /// <param name="dashDirection">Direction to dash</param>
        /// <param name="dashSpeed">Maximum dash speed</param>
        /// <param name="accelerationPhase">If true, speeding up. If false, slowing down.</param>
        /// <param name="acceleration">Acceleration/deceleration rate</param>
        public static void Dash(NPC npc, Vector2 dashDirection, float dashSpeed, bool accelerationPhase, float acceleration = 0.15f)
        {
            dashDirection = dashDirection.SafeNormalize(Vector2.UnitX);
            
            if (accelerationPhase)
            {
                // Accelerate to full dash speed
                Vector2 targetVelocity = dashDirection * dashSpeed;
                npc.velocity = ApplyAcceleration(npc.velocity, targetVelocity, acceleration);
            }
            else
            {
                // Decelerate with drag
                npc.velocity = ApplyDrag(npc.velocity, 0.92f);
            }
        }
        
        #endregion

        #region Predictive Targeting
        
        /// <summary>
        /// Predicts where a player will be in T frames.
        /// Essential for fair but challenging boss attacks.
        /// </summary>
        /// <param name="player">The target player</param>
        /// <param name="frames">How many frames ahead to predict</param>
        /// <returns>Predicted position</returns>
        public static Vector2 PredictPlayerPosition(Player player, int frames)
        {
            return player.Center + player.velocity * frames;
        }
        
        /// <summary>
        /// Calculates interception point for a projectile to hit a moving player.
        /// </summary>
        /// <param name="projectileStart">Where the projectile spawns</param>
        /// <param name="projectileSpeed">Projectile speed</param>
        /// <param name="player">Target player</param>
        /// <returns>Interception point, or player center if no solution</returns>
        public static Vector2 CalculateInterceptionPoint(Vector2 projectileStart, float projectileSpeed, Player player)
        {
            // Solve quadratic for interception time
            Vector2 toPlayer = player.Center - projectileStart;
            Vector2 playerVel = player.velocity;
            
            float a = playerVel.LengthSquared() - projectileSpeed * projectileSpeed;
            float b = 2f * Vector2.Dot(toPlayer, playerVel);
            float c = toPlayer.LengthSquared();
            
            // Handle edge cases
            if (Math.Abs(a) < 0.001f)
            {
                // Player moving at same speed as projectile
                if (Math.Abs(b) > 0.001f)
                {
                    float tEdge = -c / b;
                    if (tEdge > 0)
                        return player.Center + playerVel * tEdge;
                }
                return player.Center;
            }
            
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0)
            {
                // No solution - projectile can't catch player
                // Fall back to prediction
                return PredictPlayerPosition(player, (int)(toPlayer.Length() / projectileSpeed));
            }
            
            float sqrtDisc = (float)Math.Sqrt(discriminant);
            float t1 = (-b + sqrtDisc) / (2f * a);
            float t2 = (-b - sqrtDisc) / (2f * a);
            
            // Use smallest positive time
            float t = (t1 > 0 && t2 > 0) ? Math.Min(t1, t2) : Math.Max(t1, t2);
            
            if (t <= 0)
                return player.Center;
                
            return player.Center + playerVel * t;
        }
        
        /// <summary>
        /// Leads the target by a fixed amount for telegraphed attacks.
        /// Less aggressive than full interception.
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="leadFactor">How much to lead (0-1, 0.5 is moderate)</param>
        /// <param name="maxLeadDistance">Maximum lead distance in pixels</param>
        public static Vector2 LeadTarget(Player player, float leadFactor = 0.5f, float maxLeadDistance = 200f)
        {
            Vector2 lead = player.velocity * 30f * leadFactor; // ~0.5 second lead at max
            
            if (lead.Length() > maxLeadDistance)
                lead = lead.SafeNormalize(Vector2.Zero) * maxLeadDistance;
                
            return player.Center + lead;
        }
        
        #endregion

        #region Movement Presets
        
        /// <summary>
        /// Preset for heavy, tanky bosses (slow acceleration, high momentum).
        /// </summary>
        public static class HeavyPreset
        {
            public const float Acceleration = 0.03f;
            public const float Drag = 0.98f;
            public const float MaxSpeed = 12f;
            
            public static void Move(NPC npc, Vector2 targetPosition)
            {
                MoveToward(npc, targetPosition, MaxSpeed, Acceleration, Drag);
            }
        }
        
        /// <summary>
        /// Preset for agile, quick bosses (fast acceleration, quick stops).
        /// </summary>
        public static class AgilePreset
        {
            public const float Acceleration = 0.12f;
            public const float Drag = 0.85f;
            public const float MaxSpeed = 18f;
            
            public static void Move(NPC npc, Vector2 targetPosition)
            {
                MoveToward(npc, targetPosition, MaxSpeed, Acceleration, Drag);
            }
        }
        
        /// <summary>
        /// Preset for floaty, ethereal bosses (medium acceleration, low drag).
        /// </summary>
        public static class FloatyPreset
        {
            public const float Acceleration = 0.06f;
            public const float Drag = 0.99f;
            public const float MaxSpeed = 10f;
            
            public static void Move(NPC npc, Vector2 targetPosition)
            {
                MoveToward(npc, targetPosition, MaxSpeed, Acceleration, Drag);
            }
        }
        
        /// <summary>
        /// Preset for erratic, unpredictable bosses.
        /// </summary>
        public static class ErraticPreset
        {
            public static void Move(NPC npc, Vector2 targetPosition)
            {
                // Randomize acceleration for jittery feel
                float accel = 0.08f + Main.rand.NextFloat(0.04f);
                float drag = 0.9f + Main.rand.NextFloat(0.05f);
                MoveToward(npc, targetPosition, 15f, accel, drag);
            }
        }
        
        #endregion

        #region State Machine Helpers
        
        /// <summary>
        /// Represents a boss AI phase with fluid transitions.
        /// </summary>
        public struct PhaseState
        {
            public int Phase;
            public int AttackTimer;
            public int SubPhase;
            public float TransitionProgress;
            public bool IsTransitioning;
            
            /// <summary>
            /// Begins a smooth transition to a new phase.
            /// </summary>
            public void BeginTransition(int newPhase)
            {
                Phase = newPhase;
                AttackTimer = 0;
                SubPhase = 0;
                TransitionProgress = 0f;
                IsTransitioning = true;
            }
            
            /// <summary>
            /// Updates transition progress.
            /// </summary>
            /// <param name="transitionSpeed">How fast to transition (0.01-0.1)</param>
            /// <returns>True when transition complete</returns>
            public bool UpdateTransition(float transitionSpeed = 0.05f)
            {
                if (!IsTransitioning)
                    return true;
                    
                TransitionProgress += transitionSpeed;
                
                if (TransitionProgress >= 1f)
                {
                    TransitionProgress = 1f;
                    IsTransitioning = false;
                    return true;
                }
                
                return false;
            }
            
            /// <summary>
            /// Packs state into NPC.ai array.
            /// </summary>
            public void Pack(NPC npc)
            {
                npc.ai[0] = Phase;
                npc.ai[1] = AttackTimer;
                npc.ai[2] = SubPhase;
                npc.ai[3] = TransitionProgress;
            }
            
            /// <summary>
            /// Unpacks state from NPC.ai array.
            /// </summary>
            public static PhaseState Unpack(NPC npc)
            {
                return new PhaseState
                {
                    Phase = (int)npc.ai[0],
                    AttackTimer = (int)npc.ai[1],
                    SubPhase = (int)npc.ai[2],
                    TransitionProgress = npc.ai[3],
                    IsTransitioning = npc.ai[3] < 1f && npc.ai[3] > 0f
                };
            }
        }
        
        /// <summary>
        /// Interpolates between two movement states during a phase transition.
        /// </summary>
        public static Vector2 InterpolateMovement(Vector2 fromVelocity, Vector2 toVelocity, float progress)
        {
            // Use smooth step for natural feel
            float smoothProgress = progress * progress * (3f - 2f * progress);
            return Vector2.Lerp(fromVelocity, toVelocity, smoothProgress);
        }
        
        #endregion
    }
}
