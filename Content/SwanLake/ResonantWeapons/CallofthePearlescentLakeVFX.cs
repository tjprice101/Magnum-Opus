using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// VFX helper for Call of the Pearlescent Lake ranged assault rifle.
    /// Handles hold-item ambient, item bloom, muzzle flash,
    /// rocket trail, hit explosion, and death/explosion VFX.
    /// The Pearlescent Lake: lake's frozen beauty, pearlescent shimmer rockets.
    /// </summary>
    public static class CallofthePearlescentLakeVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            try { UnifiedVFX.SwanLake.Aura(center, 28f); } catch { }

            // Rainbow at muzzle
            if (Main.rand.NextBool(8))
            {
                Vector2 muzzleOffset = new Vector2(player.direction * 36f, -4f);
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(center + muzzleOffset, rainbow * 0.4f, 0.2f, 10); } catch { }
            }

            // Directional feather drifts
            if (Main.rand.NextBool(20))
            {
                Vector2 featherOffset = new Vector2(player.direction * Main.rand.NextFloat(10f, 30f), Main.rand.NextFloat(-10f, 10f));
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + featherOffset, featherCol, 0.15f); } catch { }
            }

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            SwanLakePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  MUZZLE VFX
        // =====================================================================

        /// <summary>
        /// Muzzle flash with dual-polarity fractal burst, prismatic sparkles,
        /// gradient halo rings, feather burst, heavy spark dust, and rainbow dust.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            try { UnifiedVFX.SwanLake.Impact(muzzlePos, 0.9f); } catch { }

            // Heavy spark dust (16 particles, directed)
            for (int i = 0; i < 16; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.35f, 0.35f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 7f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.5f);
                d.noGravity = true;
            }

            // Rainbow dust burst
            SwanLakeVFXLibrary.SpawnRainbowBurst(muzzlePos, 10, 5f);

            // Gradient halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(muzzlePos, 3, 0.2f);

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(muzzlePos, 5, 12f);

            // Feather burst
            SwanLakeVFXLibrary.SpawnFeatherBurst(muzzlePos, 2, 0.2f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(muzzlePos, SwanLakePalette.PureWhite.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  ROCKET TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame rocket trail: dual-polarity blazing, prismatic sparkles,
        /// frequent flares, pearlescent shimmer particles, rainbow flares,
        /// feather trail, and pulsing rainbow light.
        /// </summary>
        public static void RocketTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Dual-polarity blazing trail
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);

            // Rainbow shimmer (1-in-2)
            SwanLakeVFXLibrary.SpawnRainbowShimmer(pos, away);

            // Frequent flares (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(5f, 5f), rainbow, 0.3f, 12); } catch { }
            }

            // Pearlescent shimmer particles (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color pearl = SwanLakePalette.GetPearlescentShimmer((float)Main.timeForVisualEffects);
                var shimmer = new GenericGlowParticle(pos, away * 0.3f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    pearl * 0.5f, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Feather trail (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos, featherCol, 0.18f); } catch { }
            }

            // Fractal trail (1-in-5)
            if (Main.rand.NextBool(5))
            {
                try { ThemedParticles.SwanLakeFractalTrail(pos, 0.4f); } catch { }
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            // Pulsing rainbow light
            SwanLakeVFXLibrary.AddPulsingLight(pos, (float)Main.timeForVisualEffects, 0.4f);
        }

        // =====================================================================
        //  HIT EXPLOSION VFX
        // =====================================================================

        /// <summary>
        /// Hit explosion: monochrome + rainbow impact, sparkles, halo rings,
        /// feather burst, music notes, and fractal gem burst.
        /// </summary>
        public static void HitExplosionVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            try { UnifiedVFX.SwanLake.Impact(pos, 1.2f * intensity); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.9f * intensity);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, (int)(8 * intensity), 20f);

            // Rainbow flares
            for (int i = 0; i < 6; i++)
            {
                Color rainbow = SwanLakePalette.GetVividRainbow((float)i / 6f);
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(15f, 15f), rainbow, 0.4f, 16); } catch { }
            }

            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 12, 5f * intensity);
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.25f * intensity);
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(4 * intensity), 0.3f);
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 25);
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.PureWhite, 0.7f * intensity); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 1.0f * intensity);
        }

        // =====================================================================
        //  DEATH / MAJOR EXPLOSION VFX
        // =====================================================================

        /// <summary>
        /// Rocket death explosion: pearlescent rainbow explosion, dual-polarity flame bursts,
        /// massive rainbow spark bursts, pearlescent shimmer, halo rings,
        /// music notes, feathers, sparkles, and light burst.
        /// </summary>
        public static void DeathExplosionVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Pearlescent rainbow explosion
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 1.2f * intensity);
            try { ThemedParticles.SwanLakeMusicalImpact(pos, intensity); } catch { }

            // Massive bloom
            SwanLakeVFXLibrary.DrawBloom(pos, 0.7f * intensity);

            // Halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.3f * intensity);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 4, 0.25f * intensity);

            // Separate dual-polarity flame bursts
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                bool isWhite = i % 2 == 0;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Rainbow spark bursts
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 14, 6f * intensity);

            // Pearlescent shimmer
            for (int i = 0; i < 6; i++)
            {
                Color pearl = SwanLakePalette.GetPearlescentShimmer((float)Main.timeForVisualEffects + (float)i / 6f);
                var shimmer = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(3f, 3f),
                    pearl * 0.6f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 10, 25f * intensity);

            // Feathers
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(6 * intensity), 0.35f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.8f, 1.1f, 30);

            // Light burst
            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.4f * intensity);
        }
    }
}
