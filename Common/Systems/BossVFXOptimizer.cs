using Microsoft.Xna.Framework;
using System;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Optimized VFX system for boss fights - reduces particle counts while maintaining visual impact.
    /// Also provides standardized warning indicators for attacks.
    /// 
    /// DESIGN PHILOSOPHY:
    /// - Use fewer, larger particles instead of many small ones
    /// - Skip particles based on frame counts to reduce load
    /// - Warning indicators are bright and distinct (cyan/yellow/red)
    /// - Scale effects based on game quality settings
    /// </summary>
    public static class BossVFXOptimizer
    {
        // Frame skip multiplier - higher = fewer particles (1 = all particles, 2 = half, 3 = third)
        private static int FrameSkipMult => Main.gameMenu ? 1 : (Main.FrameSkipMode == 0 ? 1 : 2);
        
        // Quality reduction based on particle count in world
        private static float QualityMult => Math.Max(0.4f, 1f - MagnumParticleHandler.ActiveParticleCount * 0.001f);
        
        #region Core Particle Methods (Optimized)
        
        /// <summary>
        /// Optimized flare - only spawns every N frames based on load
        /// </summary>
        public static void OptimizedFlare(Vector2 position, Color color, float scale, int lifetime, int frameInterval = 2)
        {
            if (Main.GameUpdateCount % (frameInterval * FrameSkipMult) != 0) return;
            CustomParticles.GenericFlare(position, color, scale * QualityMult, lifetime);
        }
        
        /// <summary>
        /// Optimized halo ring - only spawns every N frames
        /// </summary>
        public static void OptimizedHalo(Vector2 position, Color color, float scale, int lifetime, int frameInterval = 3)
        {
            if (Main.GameUpdateCount % (frameInterval * FrameSkipMult) != 0) return;
            CustomParticles.HaloRing(position, color, scale * QualityMult, lifetime);
        }
        
        /// <summary>
        /// Optimized explosion burst - reduces particle count under load
        /// </summary>
        public static void OptimizedBurst(Vector2 position, Color color, int baseCount, float speed)
        {
            int actualCount = Math.Max(4, (int)(baseCount * QualityMult));
            CustomParticles.ExplosionBurst(position, color, actualCount, speed);
        }
        
        /// <summary>
        /// Optimized radial flares - spawns a ring of flares with reduced count under load
        /// </summary>
        public static void OptimizedRadialFlares(Vector2 center, Color color, int baseCount, float radius, float scale, int lifetime)
        {
            int actualCount = Math.Max(4, (int)(baseCount * QualityMult));
            for (int i = 0; i < actualCount; i++)
            {
                float angle = MathHelper.TwoPi * i / actualCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, color, scale, lifetime);
            }
        }
        
        /// <summary>
        /// Optimized cascading halos - reduces ring count under load
        /// </summary>
        public static void OptimizedCascadingHalos(Vector2 center, Color startColor, Color endColor, int baseCount, float baseScale, int baseLifetime)
        {
            int actualCount = Math.Max(3, (int)(baseCount * QualityMult));
            for (int i = 0; i < actualCount; i++)
            {
                float progress = i / (float)actualCount;
                Color ringColor = Color.Lerp(startColor, endColor, progress);
                CustomParticles.HaloRing(center, ringColor, baseScale + i * 0.1f, baseLifetime + i * 2);
            }
        }
        
        /// <summary>
        /// Optimized themed particles (sakura, feathers, etc) - reduced counts
        /// </summary>
        public static void OptimizedThemedParticles(Vector2 center, string theme, int baseCount, float radius)
        {
            int actualCount = Math.Max(2, (int)(baseCount * QualityMult * 0.6f)); // More aggressive reduction for themed
            
            switch (theme.ToLower())
            {
                case "sakura":
                case "eroica":
                    ThemedParticles.SakuraPetals(center, actualCount, radius);
                    break;
                case "feather":
                case "swan":
                    CustomParticles.SwanFeatherExplosion(center, actualCount, radius * 0.01f);
                    break;
                default:
                    // Generic burst
                    OptimizedBurst(center, Color.White, actualCount, radius * 0.1f);
                    break;
            }
        }
        
        #endregion
        
        #region Attack Warning System
        
        /// <summary>
        /// Draws a bright warning indicator at a position - ALWAYS rendered (no optimization)
        /// Used to telegraph where danger is coming
        /// </summary>
        public static void WarningFlare(Vector2 position, float intensity = 1f, WarningType type = WarningType.Danger)
        {
            Color warningColor = type switch
            {
                WarningType.Safe => Color.Cyan,
                WarningType.Caution => Color.Yellow,
                WarningType.Danger => Color.Red,
                WarningType.Imminent => Color.White,
                _ => Color.Yellow
            };
            
            // Pulsing effect
            float pulse = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.3f;
            CustomParticles.GenericFlare(position, warningColor * intensity * pulse, 0.35f * intensity, 6);
        }
        
        /// <summary>
        /// Draws a line of warning indicators showing projectile trajectory
        /// </summary>
        public static void WarningLine(Vector2 start, Vector2 direction, float length, int markerCount = 10, WarningType type = WarningType.Danger)
        {
            Color warningColor = type switch
            {
                WarningType.Safe => Color.Cyan,
                WarningType.Caution => Color.Yellow,
                WarningType.Danger => Color.Red,
                WarningType.Imminent => Color.White,
                _ => Color.Yellow
            };
            
            direction = direction.SafeNormalize(Vector2.UnitX);
            float pulse = 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.4f;
            
            for (int i = 0; i < markerCount; i++)
            {
                float progress = i / (float)markerCount;
                Vector2 pos = start + direction * length * progress;
                float alpha = 1f - progress * 0.5f; // Fade toward end
                CustomParticles.GenericFlare(pos, warningColor * alpha * pulse, 0.18f + progress * 0.1f, 4);
            }
        }
        
        /// <summary>
        /// Draws a circular safe zone indicator around a position
        /// </summary>
        public static void SafeZoneRing(Vector2 center, float radius, int markerCount = 12)
        {
            float pulse = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.3f;
            
            for (int i = 0; i < markerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / markerCount + Main.GameUpdateCount * 0.02f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, Color.Cyan * pulse, 0.28f, 5);
            }
        }
        
        /// <summary>
        /// Draws a danger zone indicator (area to avoid)
        /// </summary>
        public static void DangerZoneRing(Vector2 center, float radius, int markerCount = 16)
        {
            float pulse = 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.4f;
            
            for (int i = 0; i < markerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / markerCount + Main.GameUpdateCount * 0.03f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, Color.Red * pulse * 0.8f, 0.22f, 4);
            }
        }
        
        /// <summary>
        /// Draws converging warning particles toward a point (attack charging up)
        /// </summary>
        public static void ConvergingWarning(Vector2 center, float maxRadius, float progress, Color primaryColor, int particleCount = 8)
        {
            if (Main.GameUpdateCount % 4 != 0) return; // Only every 4 frames
            
            float currentRadius = maxRadius * (1f - progress * 0.6f);
            int actualCount = Math.Max(4, (int)(particleCount * QualityMult));
            
            for (int i = 0; i < actualCount; i++)
            {
                float angle = MathHelper.TwoPi * i / actualCount + Main.GameUpdateCount * 0.04f;
                Vector2 pos = center + angle.ToRotationVector2() * currentRadius;
                Color color = Color.Lerp(primaryColor, Color.White, progress * 0.5f);
                CustomParticles.GenericFlare(pos, color, 0.25f + progress * 0.25f, 10);
            }
        }
        
        /// <summary>
        /// Shows an arc of safe area indicators (where projectiles WON'T go)
        /// </summary>
        public static void SafeArcIndicator(Vector2 center, float safeAngle, float arcWidth, float radius, int markerCount = 8)
        {
            float halfArc = arcWidth / 2f;
            float pulse = 0.75f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.25f;
            
            for (int i = 0; i < markerCount; i++)
            {
                float progress = (i / (float)(markerCount - 1)) - 0.5f; // -0.5 to 0.5
                float angle = safeAngle + progress * arcWidth;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, Color.Cyan * pulse, 0.3f, 6);
            }
        }
        
        /// <summary>
        /// Draws impact warning at ground level (for attacks from above)
        /// </summary>
        public static void GroundImpactWarning(Vector2 impactPoint, float radius, float progress)
        {
            float pulse = 0.5f + progress * 0.5f;
            int markerCount = Math.Max(6, (int)(12 * QualityMult));
            
            // Expanding danger ring
            for (int i = 0; i < markerCount; i++)
            {
                float angle = MathHelper.TwoPi * i / markerCount;
                Vector2 pos = impactPoint + angle.ToRotationVector2() * radius * progress;
                CustomParticles.GenericFlare(pos, Color.Red * pulse, 0.25f, 4);
            }
            
            // Central X marker
            if (Main.GameUpdateCount % 3 == 0)
            {
                CustomParticles.GenericFlare(impactPoint, Color.Yellow * pulse, 0.4f * progress, 5);
            }
        }
        
        /// <summary>
        /// Draws laser beam warning (line where laser will fire)
        /// </summary>
        public static void LaserBeamWarning(Vector2 start, float angle, float length, float intensity = 1f)
        {
            float pulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f;
            Vector2 dir = angle.ToRotationVector2();
            
            int markerCount = Math.Max(8, (int)(20 * QualityMult));
            for (int i = 0; i < markerCount; i++)
            {
                float progress = i / (float)markerCount;
                Vector2 pos = start + dir * length * progress;
                Color warningColor = Color.Lerp(Color.Red, Color.Yellow, progress) * pulse * intensity;
                CustomParticles.GenericFlare(pos, warningColor, 0.15f, 3);
            }
        }
        
        /// <summary>
        /// Draws electrical buildup warning (for shock attacks)
        /// </summary>
        /// <param name="center">Center of the buildup effect</param>
        /// <param name="primaryColor">Main color of the electrical sparks</param>
        /// <param name="radius">Radius of the effect area</param>
        /// <param name="progress">Charge progress from 0-1</param>
        public static void ElectricalBuildupWarning(Vector2 center, Color primaryColor, float radius, float progress)
        {
            if (Main.GameUpdateCount % 5 != 0) return;
            
            int arcCount = 3 + (int)(progress * 5);
            for (int i = 0; i < arcCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = radius * 0.5f + Main.rand.NextFloat(radius * 0.5f) * (1f - progress);
                Vector2 arcStart = center + angle.ToRotationVector2() * dist;
                Vector2 arcEnd = center + Main.rand.NextVector2Circular(30f * progress, 30f * progress);
                
                // Small electrical spark
                Color sparkColor = Color.Lerp(primaryColor, Color.White, 0.5f);
                for (int j = 0; j < 4; j++)
                {
                    Vector2 sparkPos = Vector2.Lerp(arcStart, arcEnd, j / 4f) + Main.rand.NextVector2Circular(10f, 10f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor * (0.5f + progress * 0.5f), 0.2f, 6);
                }
            }
        }
        
        #endregion
        
        #region Boss-Specific Optimized Effects
        
        /// <summary>
        /// Optimized attack release burst - used when boss fires an attack
        /// </summary>
        public static void AttackReleaseBurst(Vector2 center, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Central white flash (always show)
            CustomParticles.GenericFlare(center, Color.White, 1.2f * scale, 20);
            CustomParticles.GenericFlare(center, primaryColor, 0.9f * scale, 18);
            
            // Optimized radial flares
            OptimizedRadialFlares(center, secondaryColor, 6, 40f * scale, 0.4f, 12);
            
            // Optimized halo cascade
            OptimizedCascadingHalos(center, primaryColor, secondaryColor, 5, 0.3f * scale, 14);
        }
        
        /// <summary>
        /// Optimized projectile trail - called every frame, handles own throttling
        /// </summary>
        public static void ProjectileTrail(Vector2 position, Vector2 velocity, Color color)
        {
            if (Main.GameUpdateCount % 3 != 0) return;
            
            Vector2 trailPos = position - velocity * 0.2f + Main.rand.NextVector2Circular(5f, 5f);
            CustomParticles.GenericFlare(trailPos, color * 0.7f, 0.25f * QualityMult, 8);
        }
        
        /// <summary>
        /// Optimized death explosion - full spectacle but optimized particle counts
        /// </summary>
        public static void DeathExplosion(Vector2 center, Color primaryColor, Color secondaryColor, float scale = 1f)
        {
            // Core flash (always full)
            CustomParticles.GenericFlare(center, Color.White, 2f * scale, 30);
            CustomParticles.GenericFlare(center, primaryColor, 1.5f * scale, 28);
            
            // Optimized burst
            OptimizedBurst(center, primaryColor, 20, 12f);
            OptimizedBurst(center, secondaryColor, 15, 8f);
            
            // Optimized cascading halos
            OptimizedCascadingHalos(center, primaryColor, secondaryColor, 8, 0.4f * scale, 18);
            
            // Optimized radial pattern
            OptimizedRadialFlares(center, secondaryColor, 8, 60f * scale, 0.5f, 15);
        }
        
        #endregion
        
        #region Attack Ending Visual Cues - Signal Recovery to Players
        
        /// <summary>
        /// Spawns visual cue indicating attack has ended and boss is recovering.
        /// Players can use this window to attack safely.
        /// </summary>
        /// <param name="center">Boss center position</param>
        /// <param name="primaryColor">Theme primary color</param>
        /// <param name="secondaryColor">Theme secondary color</param>
        /// <param name="intensity">Effect intensity (0.5-2.0)</param>
        public static void AttackEndCue(Vector2 center, Color primaryColor, Color secondaryColor, float intensity = 1f)
        {
            // "Exhale" burst - signals boss is vulnerable
            int particleCount = ScaleCount((int)(10 * intensity));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 offset = angle.ToRotationVector2() * (25f + Main.rand.NextFloat(15f));
                float progress = i / (float)particleCount;
                Color burstColor = Color.Lerp(primaryColor, secondaryColor, progress) * 0.6f;
                CustomParticles.GenericFlare(center + offset, burstColor, 0.25f * intensity, 18);
            }
            
            // Cooldown shimmer ring - cyan tint indicates safety
            Color safetyTint = Color.Lerp(primaryColor, Color.Cyan, 0.3f);
            CustomParticles.HaloRing(center, safetyTint * 0.4f, 0.5f * intensity, 25);
        }
        
        /// <summary>
        /// Spawns deceleration trail effect showing boss slowing down.
        /// Call EVERY FRAME during deceleration phase.
        /// </summary>
        /// <param name="center">Boss center position</param>
        /// <param name="velocity">Current velocity (for trail direction)</param>
        /// <param name="color">Trail color</param>
        /// <param name="decelerationProgress">Progress 0-1 (more particles at start)</param>
        public static void DecelerationTrail(Vector2 center, Vector2 velocity, Color color, float decelerationProgress)
        {
            if (velocity.LengthSquared() < 4f) return; // Don't spawn if nearly stopped
            
            // Spawn chance decreases as we slow down
            float spawnChance = (1f - decelerationProgress) * 0.6f;
            if (Main.GameUpdateCount % 2 != 0) return;
            if (Main.rand.NextFloat() > spawnChance) return;
            
            Vector2 trailDir = velocity.SafeNormalize(Vector2.UnitX);
            Vector2 trailPos = center - trailDir * 20f + Main.rand.NextVector2Circular(8f, 8f);
            
            // Fading trail particles
            float alpha = (1f - decelerationProgress) * 0.5f;
            CustomParticles.GenericFlare(trailPos, color * alpha, 0.2f, 12);
        }
        
        /// <summary>
        /// Spawns "ready to attack" cue when boss finishes recovery.
        /// Sharp flash signals danger resuming.
        /// </summary>
        /// <param name="center">Boss center position</param>
        /// <param name="primaryColor">Theme color</param>
        /// <param name="intensity">Effect intensity</param>
        public static void ReadyToAttackCue(Vector2 center, Color primaryColor, float intensity = 1f)
        {
            // Sharp central flash
            CustomParticles.GenericFlare(center, Color.White, 0.8f * intensity, 12);
            CustomParticles.GenericFlare(center, primaryColor, 0.6f * intensity, 15);
            
            // Danger-indicating ring (red tint)
            Color dangerTint = Color.Lerp(primaryColor, Color.Red, 0.2f);
            CustomParticles.HaloRing(center, dangerTint * 0.6f, 0.4f * intensity, 18);
        }
        
        /// <summary>
        /// Creates wind-down effect for charge/dash attacks.
        /// Trailing wisps that fade as boss slows.
        /// </summary>
        /// <param name="center">Boss center position</param>
        /// <param name="velocity">Current velocity</param>
        /// <param name="color">Effect color</param>
        /// <param name="windDownProgress">Progress 0-1</param>
        public static void WindDownEffect(Vector2 center, Vector2 velocity, Color color, float windDownProgress)
        {
            if (Main.GameUpdateCount % 4 != 0) return;
            
            float alpha = 1f - windDownProgress;
            if (alpha < 0.15f) return;
            
            Vector2 trailDir = velocity.SafeNormalize(Vector2.UnitX);
            
            // Trailing wisps behind movement
            for (int i = 0; i < 2; i++)
            {
                float dist = 30f + i * 20f;
                Vector2 wispPos = center - trailDir * dist + Main.rand.NextVector2Circular(10f, 10f);
                CustomParticles.GenericFlare(wispPos, color * alpha * 0.4f, 0.18f, 15);
            }
        }
        
        /// <summary>
        /// Creates smooth recovery shimmer around boss during cooldown.
        /// Signals the boss is in a vulnerable state.
        /// </summary>
        /// <param name="center">Boss center position</param>
        /// <param name="color">Theme color</param>
        /// <param name="radius">Shimmer radius</param>
        /// <param name="recoveryProgress">Progress 0-1 through recovery</param>
        public static void RecoveryShimmer(Vector2 center, Color color, float radius, float recoveryProgress)
        {
            if (Main.GameUpdateCount % 6 != 0) return;
            
            // Gentle orbiting particles during recovery - cyan safety tint
            float alpha = 1f - recoveryProgress * 0.5f;
            Color shimmerColor = Color.Lerp(color, Color.Cyan, 0.2f) * alpha * 0.4f;
            
            float angle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 3; i++)
            {
                float particleAngle = angle + MathHelper.TwoPi * i / 3f;
                Vector2 pos = center + particleAngle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, shimmerColor, 0.2f, 10);
            }
        }
        
        #endregion
        
        #region Performance Utilities
        
        /// <summary>
        /// Returns a scaled count based on current particle load.
        /// Use this when you need to spawn N particles but want to scale based on performance.
        /// Example: int count = BossVFXOptimizer.ScaleCount(20); // Returns 8-20 based on load
        /// </summary>
        public static int ScaleCount(int baseCount, float minRatio = 0.4f)
        {
            return Math.Max((int)(baseCount * minRatio), (int)(baseCount * QualityMult));
        }
        
        /// <summary>
        /// Check if we should spawn a particle this frame (for throttling in loops).
        /// Example: if (BossVFXOptimizer.ShouldSpawn(i, 2)) spawn particle...
        /// </summary>
        public static bool ShouldSpawn(int index, int interval = 2)
        {
            return (index + Main.GameUpdateCount) % (interval * FrameSkipMult) == 0;
        }
        
        /// <summary>
        /// Returns true if the particle system is under heavy load (>500 particles).
        /// Use this to skip non-essential particles entirely.
        /// </summary>
        public static bool IsHighLoad => MagnumParticleHandler.ActiveParticleCount > 500;
        
        /// <summary>
        /// Returns true if the particle system is at critical load (>800 particles).
        /// Use this to skip ALL optional particles.
        /// </summary>
        public static bool IsCriticalLoad => MagnumParticleHandler.ActiveParticleCount > 800;
        
        #endregion
    }
    
    /// <summary>
    /// Warning type enum for visual distinction
    /// </summary>
    public enum WarningType
    {
        Safe,       // Cyan - this area is safe
        Caution,    // Yellow - be careful
        Danger,     // Red - avoid this area
        Imminent    // White - attack incoming NOW
    }
}
