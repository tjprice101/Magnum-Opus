using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    // ============================================================================
    // VFX COMBOS - Pre-built effect combinations for common use cases
    // These combine multiple particle types, screen effects, and dust for
    // professional-quality visual effects.
    // ============================================================================

    /// <summary>
    /// Pre-built VFX combinations inspired by Infernum patterns.
    /// Call these methods for instant high-quality effects.
    /// </summary>
    public static class VFXCombos
    {
        // ============================================================================
        // IMPACT EFFECTS
        // ============================================================================

        /// <summary>
        /// Standard impact effect for projectile hits and basic attacks.
        /// </summary>
        public static void StandardImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
        {
            // Pulse ring
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, primary, 0f, 2.5f * scale, 30);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }

            // Bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primary * 0.6f, 1.5f * scale, 20);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }

            // Sparks
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 12f) * scale;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, false, 30, 1.5f * scale, primary);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }

            // Dust fallback
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.RainbowMk2, velocity, 0, primary, 1.2f * scale);
                dust.noGravity = true;
            }

            MagnumScreenEffects.AddScreenShake(3f * scale);
        }

        /// <summary>
        /// Heavy impact effect for powerful attacks.
        /// </summary>
        public static void HeavyImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
        {
            // Multiple pulse rings
            for (int i = 0; i < 2; i++)
            {
                float ringScale = (1f + i * 0.3f) * scale;
                Color ringColor = Color.Lerp(primary, secondary, i / 2f);
                try
                {
                    var ring = new PulseRingParticle(position, Vector2.Zero, ringColor, 0f, 3f * ringScale, 35 + i * 8);
                    MagnumParticleHandler.SpawnParticle(ring);
                }
                catch { }
            }

            // Strong bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primary, 2f * scale, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }

            // Sparks
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 18f) * scale;
                Color sparkColor = Main.rand.NextBool() ? primary : secondary;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, Main.rand.NextBool(4), 40, 1.8f * scale, sparkColor);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }

            // Smoke burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 6f) * scale;
                try
                {
                    var smoke = new CloudSmokeParticle(position, velocity, primary, Color.DarkGray, 35, 1.2f * scale);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                catch { }
            }

            MagnumScreenEffects.AddScreenShake(6f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 0.8f * scale, 15);
        }

        /// <summary>
        /// Critical hit impact with flare effect.
        /// </summary>
        public static void CriticalImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
        {
            // Flare shine
            try
            {
                var flare = new FlareShineParticle(position, Vector2.Zero, primary, secondary, 0f, new Vector2(5f * scale), 35);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            catch { }

            // Pulse ring
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, primary, 0f, 2f * scale, 25);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }

            // Crit sparks
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f) * scale;
                try
                {
                    var spark = new CritSparkParticle(position, velocity, primary, secondary, 1.5f * scale, 30);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }

            MagnumScreenEffects.AddScreenShake(4f * scale);
        }

        // ============================================================================
        // EXPLOSION EFFECTS
        // ============================================================================

        /// <summary>
        /// Fire explosion with smoke and sparks.
        /// </summary>
        public static void FireExplosion(Vector2 position, Color flameColor, Color smokeColor, float scale = 1f)
        {
            ExplosionUtility.CreateFireExplosion(position, flameColor, smokeColor, scale);
        }

        /// <summary>
        /// Energy/magical explosion with electric effects.
        /// </summary>
        public static void EnergyExplosion(Vector2 position, Color energyColor, float scale = 1f)
        {
            ExplosionUtility.CreateEnergyExplosion(position, energyColor, scale);
        }

        /// <summary>
        /// Major explosion for boss deaths and climactic moments.
        /// </summary>
        public static void MajorExplosion(Vector2 position, Color primary, Color secondary, float scale = 1f)
        {
            // Multiple pulse rings
            for (int i = 0; i < 3; i++)
            {
                float ringScale = (1f + i * 0.3f) * scale;
                Color ringColor = Color.Lerp(primary, secondary, i / 3f);
                try
                {
                    var ring = new PulseRingParticle(position, Vector2.Zero, ringColor, 0f, 3f * ringScale, 35 + i * 10);
                    MagnumParticleHandler.SpawnParticle(ring);
                }
                catch { }
            }

            // Strong bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primary, 3f * scale, 30);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }

            // Electric arcs
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 25f) * scale;
                try
                {
                    var arc = new ElectricArcParticle(position, velocity, secondary, 1f, 45);
                    MagnumParticleHandler.SpawnParticle(arc);
                }
                catch { }
            }

            // Sparks burst
            for (int i = 0; i < 30; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 20f) * scale;
                Color sparkColor = Main.rand.NextBool() ? primary : secondary;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, Main.rand.NextBool(4), 50, 2f * scale, sparkColor);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }

            // Smoke ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f) * scale;
                Color smokeColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                try
                {
                    var smoke = new DenseSmokeParticle(position, velocity, smokeColor, 50, 2f * scale, 0.8f);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                catch { }
            }

            // Flare shine
            try
            {
                var flare = new FlareShineParticle(position, Vector2.Zero, primary, secondary, 0f, new Vector2(8f * scale), 50);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            catch { }

            MagnumScreenEffects.AddScreenShake(5f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 1.5f * scale, 25);
        }

        /// <summary>
        /// Death explosion for boss kills - maximum visual impact.
        /// </summary>
        public static void BossDeathExplosion(Vector2 position, Color primary, Color secondary, float scale = 1f)
        {
            ExplosionUtility.CreateDeathExplosion(position, primary, secondary, scale);
        }

        // ============================================================================
        // TELEPORT EFFECTS
        // ============================================================================

        /// <summary>
        /// Teleport departure/arrival effect.
        /// </summary>
        public static void TeleportBurst(Vector2 position, Color color, float scale = 1f)
        {
            // Pulse ring
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, color, 0f, 2f * scale, 25);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }

            // Bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, color, 2f * scale, 20);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }

            // Vertical sparks (rising)
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-10f, -5f)) * scale;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, false, 35, 1.5f * scale, color);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }

            // Dust burst
            for (int i = 0; i < 20; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f) * scale;
                Dust dust = Dust.NewDustPerfect(position, DustID.RainbowMk2, velocity, 0, color, 1.5f * scale);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }

            MagnumScreenEffects.AddScreenShake(3f * scale);
        }

        /// <summary>
        /// Dramatic teleport with both departure and arrival positions.
        /// Call once - handles both effects.
        /// </summary>
        public static void DramaticTeleport(Vector2 departure, Vector2 arrival, Color color, float scale = 1f)
        {
            // Departure effects
            TeleportBurst(departure, color * 0.8f, scale);

            // Arrival effects (slightly larger)
            TeleportBurst(arrival, color, scale * 1.2f);

            // Lightning arc connecting the two points
            try
            {
                Vector2 velocity = (arrival - departure).SafeNormalize(Vector2.UnitX) * 30f;
                var arc = new ElectricArcParticle(departure, velocity, color, 1.5f, 30);
                MagnumParticleHandler.SpawnParticle(arc);
            }
            catch { }
        }

        // ============================================================================
        // CHARGE/WINDUP EFFECTS
        // ============================================================================

        /// <summary>
        /// Pulsing effect for attack windups.
        /// Call every frame during windup phase.
        /// </summary>
        public static void ChargePulse(Vector2 position, Color color, float chargeProgress, float scale = 1f)
        {
            // Only spawn particles periodically
            if (Main.rand.NextBool(3))
            {
                // Converging particles
                float radius = 100f * (1f - chargeProgress) * scale;
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawnPos = position + angle.ToRotationVector2() * radius;
                Vector2 velocity = (position - spawnPos) * 0.05f;

                try
                {
                    var glow = new SquishyLightParticle(spawnPos, velocity, 0.5f * scale, color, 30);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                catch { }
            }

            // Central pulsing glow
            if (Main.rand.NextBool(5))
            {
                float pulse = 0.3f + chargeProgress * 0.5f;
                try
                {
                    var bloom = new StrongBloomParticle(position, Vector2.Zero, color * 0.5f, pulse * scale, 15);
                    MagnumParticleHandler.SpawnParticle(bloom);
                }
                catch { }
            }

            // Building screen shake
            if (chargeProgress > 0.5f)
                MagnumScreenEffects.AddScreenShake(chargeProgress * 2f * scale);
        }

        /// <summary>
        /// Attack release burst - call when windup ends and attack fires.
        /// </summary>
        public static void AttackRelease(Vector2 position, Color primary, Color secondary, float scale = 1f)
        {
            // Pulse ring
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, primary, 0f, 3f * scale, 30);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }

            // Bloom
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, primary, 2.5f * scale, 25);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }

            // Radial spark burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f) * scale;
                Color sparkColor = Main.rand.NextBool() ? primary : secondary;
                try
                {
                    var spark = new DirectionalSparkParticle(position, velocity, false, 35, 1.5f * scale, sparkColor);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }

            MagnumScreenEffects.AddScreenShake(5f * scale);
            MagnumScreenEffects.SetFlashEffect(position, 1f * scale, 15);
        }

        // ============================================================================
        // TRAIL EFFECTS
        // ============================================================================

        /// <summary>
        /// Basic projectile trail - call every few frames.
        /// </summary>
        public static void ProjectileTrail(Vector2 position, Vector2 velocity, Color color, float scale = 1f)
        {
            // Trailing glow
            try
            {
                var glow = new GenericGlowParticle(position, -velocity * 0.1f, color, 0.3f * scale, 15);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            catch { }

            // Optional sparks
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(2f, 2f);
                try
                {
                    var spark = new DirectionalSparkParticle(position, sparkVel, true, 20, 0.8f * scale, color);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                catch { }
            }
        }

        /// <summary>
        /// Fire trail - call every few frames.
        /// </summary>
        public static void FireTrail(Vector2 position, Vector2 velocity, Color flameColor, float scale = 1f)
        {
            // Smoke
            try
            {
                var smoke = new DenseSmokeParticle(position, -velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 
                    Color.DarkGray, 30, 0.8f * scale, 0.6f, glowing: false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            catch { }

            // Flame glow
            try
            {
                var glow = new GenericGlowParticle(position, -velocity * 0.1f, flameColor, 0.4f * scale, 12);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            catch { }

            // Embers
            if (Main.rand.NextBool(2))
            {
                Vector2 emberVel = -velocity * 0.1f + new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                Dust ember = Dust.NewDustPerfect(position, DustID.Torch, emberVel, 0, flameColor, 1.2f * scale);
                ember.noGravity = true;
            }
        }

        /// <summary>
        /// Electric trail - call every few frames.
        /// </summary>
        public static void ElectricTrail(Vector2 position, Vector2 velocity, Color electricColor, float scale = 1f)
        {
            // Electric arc
            if (Main.rand.NextBool(2))
            {
                Vector2 arcVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
                try
                {
                    var arc = new ElectricArcParticle(position, arcVel, electricColor, 0.5f * scale, 20);
                    MagnumParticleHandler.SpawnParticle(arc);
                }
                catch { }
            }

            // Glow
            try
            {
                var glow = new GenericGlowParticle(position, -velocity * 0.1f, electricColor, 0.4f * scale, 10);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            catch { }
        }

        // ============================================================================
        // AMBIENT EFFECTS
        // ============================================================================

        /// <summary>
        /// Ambient sparkles around a position - call every frame.
        /// </summary>
        public static void AmbientSparkles(Vector2 position, float radius, Color color, float scale = 1f)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 sparklePos = position + Main.rand.NextVector2Circular(radius, radius);
                Vector2 velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                try
                {
                    var sparkle = new SquishyLightParticle(sparklePos, velocity, 0.3f * scale, color, 30);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                catch { }
            }
        }

        /// <summary>
        /// Orbiting particles around a position - call every frame.
        /// </summary>
        public static void OrbitingParticles(Vector2 center, float radius, Color primary, Color secondary, float scale = 1f)
        {
            if (Main.rand.NextBool(3))
            {
                float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                float particleRadius = radius + Main.rand.NextFloat(-10f, 10f);
                Vector2 pos = center + angle.ToRotationVector2() * particleRadius;
                Vector2 velocity = (angle + MathHelper.PiOver2).ToRotationVector2() * 2f;
                Color color = Main.rand.NextBool() ? primary : secondary;

                try
                {
                    var glow = new GenericGlowParticle(pos, velocity, color, 0.3f * scale, 20);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                catch { }
            }
        }

        // ============================================================================
        // SHOCKWAVE EFFECTS
        // ============================================================================

        /// <summary>
        /// Ground shockwave - call on landing or ground-based attacks.
        /// </summary>
        public static void GroundShockwave(Vector2 position, Color color, float scale = 1f)
        {
            ShockwaveUtility.CreateShockwave(position, 2, 8, 75f * scale, color);

            // Ground debris
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(-MathHelper.PiOver2 - 0.5f, -MathHelper.PiOver2 + 0.5f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f) * scale;
                Dust debris = Dust.NewDustPerfect(position, DustID.Stone, velocity, 0, Color.Gray, 1.5f);
                debris.noGravity = false;
            }
        }

        /// <summary>
        /// Aerial shockwave - call for mid-air explosions.
        /// </summary>
        public static void AerialShockwave(Vector2 position, Color color, float scale = 1f)
        {
            ShockwaveUtility.CreateThemedShockwave(position, color, color * 0.5f, scale);
        }
    }
}
