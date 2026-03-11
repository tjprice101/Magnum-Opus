using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.Enemies
{
    /// <summary>
    /// Shared VFX helper for Swan Lake enemies.
    /// Provides ambient aura, attack flash, death burst, and themed particles
    /// for all Swan Lake-themed enemies (ShatteredPrima, etc.).
    /// </summary>
    public static class SwanLakeEnemyVFX
    {
        // =====================================================================
        //  AMBIENT AURA — Per-frame passive VFX
        // =====================================================================

        /// <summary>
        /// Per-frame ambient aura for Swan Lake enemies: dual-polarity motes,
        /// prismatic shimmer, and gentle feather drift.
        /// </summary>
        public static void AmbientAura(Vector2 center, float scale = 1f)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Dual-polarity motes (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(30f * scale, 30f * scale);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                Dust d = Dust.NewDustPerfect(motePos, dustType, Vector2.Zero, isWhite ? 0 : 100, col, 0.5f * scale);
                d.noGravity = true;
            }

            // Prismatic shimmer (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                Vector2 shimmerPos = center + Main.rand.NextVector2Circular(25f * scale, 25f * scale);
                var glow = new GenericGlowParticle(shimmerPos,
                    new Vector2(0f, -0.3f),
                    rainbow * 0.3f, 0.12f * scale, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Feather drift (1-in-20)
            if (Main.rand.NextBool(20))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(20f * scale, 20f * scale), featherCol, 0.15f * scale); } catch { }
            }

            // Ambient light
            SwanLakeVFXLibrary.AddDualPolarityLight(center, time, 0.3f * scale);
        }

        // =====================================================================
        //  ATTACK FLASH — On-attack VFX burst
        // =====================================================================

        /// <summary>
        /// Attack flash VFX: monochromatic burst with prismatic accents.
        /// Call when the enemy initiates an attack.
        /// </summary>
        public static void AttackFlash(Vector2 pos, Vector2 direction, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Dual-polarity spark burst (directed)
            for (int i = 0; i < (int)(8 * intensity); i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 6f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.4f);
                d.noGravity = true;
            }

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, (int)(4 * intensity), 12f);

            // Halo ring
            try { CustomParticles.HaloRing(pos, SwanLakePalette.SwanSilver, 0.25f * intensity, 12); } catch { }

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.7f * intensity);
        }

        // =====================================================================
        //  DEATH BURST — On-death explosion VFX
        // =====================================================================

        /// <summary>
        /// Death burst VFX: dramatic dual-polarity explosion with prismatic cascade,
        /// feather explosion, music note scatter, and fading bloom.
        /// </summary>
        public static void DeathBurst(Vector2 pos, float scale = 1f)
        {
            if (Main.dedServ) return;

            // Core impact
            try { UnifiedVFX.SwanLake.Impact(pos, 1.0f * scale); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.8f * scale);

            // Dual-polarity radial burst
            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, (int)(14 * scale), 6f * scale);

            // Rainbow burst
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, (int)(10 * scale), 5f * scale);

            // Gradient halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, (int)(4 * scale), 0.25f * scale);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 3, 0.2f * scale);

            // Feather explosion
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(6 * scale), 0.3f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, (int)(3 * scale), 0.25f);

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, (int)(8 * scale), 20f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.1f, 30);

            // Fractal gem burst
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.PureWhite, 0.6f * scale); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.2f * scale);
        }

        // =====================================================================
        //  PROJECTILE VFX — For enemy projectiles
        // =====================================================================

        /// <summary>
        /// Per-frame trail for enemy projectiles: monochrome trail with rainbow edge.
        /// </summary>
        public static void EnemyProjectileTrail(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);

            if (Main.rand.NextBool(3))
                SwanLakeVFXLibrary.SpawnRainbowShimmer(pos, away);

            if (Main.rand.NextBool(8))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.3f);
        }

        /// <summary>
        /// Enemy projectile impact.
        /// </summary>
        public static void EnemyProjectileImpact(Vector2 pos, float intensity = 0.6f)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 8, 4f * intensity);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 3, 10f);
            try { CustomParticles.HaloRing(pos, SwanLakePalette.SwanSilver, 0.2f * intensity, 10); } catch { }
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.7f, 0.9f, 20);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.5f * intensity);
        }

        // =====================================================================
        //  SHATTERED PRIMA SPECIFIC
        // =====================================================================

        /// <summary>
        /// Shattered Prima specific ambient: cracked porcelain aesthetic,
        /// shards of light leaking through broken shell.
        /// </summary>
        public static void ShatteredPrimaAmbient(Vector2 center)
        {
            if (Main.dedServ) return;

            // Base ambient
            AmbientAura(center, 1.2f);

            // "Cracking light" — sharp white sparkles leaking outward
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 crackPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(10f, 25f);
                Vector2 vel = (crackPos - center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f);
                Dust d = Dust.NewDustPerfect(crackPos, DustID.WhiteTorch, vel, 0, SwanLakePalette.PureWhite * 0.7f, 0.8f);
                d.noGravity = true;
            }
        }
    }
}
