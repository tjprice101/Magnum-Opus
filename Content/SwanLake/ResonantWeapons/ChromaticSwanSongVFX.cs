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
    /// VFX helper for the Chromatic Swan Song magic pistol.
    /// Handles hold-item ambient, item bloom, muzzle flash,
    /// projectile trail, small hit, and 3-hit combo explosion.
    /// The Chromatic Swan Song: prismatic magic cascade, the dying aria.
    /// </summary>
    public static class ChromaticSwanSongVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient prismatic aura, fractal flares,
        /// rainbow shimmer, and feather drift.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 30f); } catch { }

            // Ambient prismatic flare near muzzle
            if (Main.rand.NextBool(8))
            {
                Vector2 muzzleOffset = new Vector2(player.direction * 32f, -6f);
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(center + muzzleOffset + Main.rand.NextVector2Circular(4f, 4f), rainbow * 0.5f, 0.25f, 12); } catch { }
            }

            // Feather drift
            if (Main.rand.NextBool(20))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(18f, 18f), featherCol, 0.15f); } catch { }
            }

            // Rainbow light
            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.3f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Enhanced 4-layer prismatic item bloom for the Chromatic Swan Song sprite.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            SwanLakePalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time);
        }

        // =====================================================================
        //  MUZZLE FLASH VFX
        // =====================================================================

        /// <summary>
        /// Muzzle flash when firing: prismatic spark burst, dual-polarity shockwave,
        /// gradient halo rings, and feather burst.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Core impact
            try { UnifiedVFX.SwanLake.Impact(muzzlePos, 0.8f); } catch { }

            // Heavy spark burst (directed)
            for (int i = 0; i < 8; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 7f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.5f);
                d.noGravity = true;
            }

            // Prismatic sparkles at muzzle
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(muzzlePos, 4, 10f);

            // Gradient halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(muzzlePos, 3, 0.2f);

            // Feather burst
            SwanLakeVFXLibrary.SpawnFeatherBurst(muzzlePos, 2, 0.2f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(muzzlePos, SwanLakePalette.PureWhite.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame projectile trail: dual-polarity blazing trail, contrasting sparkles,
        /// rainbow shimmer, prismatic flares, and feather trail.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Dual-polarity blazing trail
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);

            // Contrasting rainbow sparkles (1-in-2)
            SwanLakeVFXLibrary.SpawnRainbowShimmer(pos, away);

            // Prismatic flares (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(6f, 6f), rainbow, 0.35f, 14); } catch { }
            }

            // Feather trail (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos + Main.rand.NextVector2Circular(6f, 6f), featherCol, 0.2f); } catch { }
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        // =====================================================================
        //  SMALL HIT VFX
        // =====================================================================

        /// <summary>
        /// On-hit VFX for each shot: rainbow sparkles, halo rings,
        /// dual-polarity dust, and music notes.
        /// </summary>
        public static void SmallHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 4, 12f);
            SwanLakeVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.2f);
            SwanLakeVFXLibrary.SpawnRadialDustBurst(hitPos, 8, 4f);
            SwanLakeVFXLibrary.SpawnRainbowBurst(hitPos, 6, 4f);
            SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.7f, 0.9f, 22);
            SwanLakeVFXLibrary.SpawnFeatherDrift(hitPos, 2, 12f);

            Lighting.AddLight(hitPos, SwanLakePalette.PureWhite.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  3-HIT COMBO EXPLOSION VFX
        // =====================================================================

        /// <summary>
        /// 3-hit combo explosion: massive prismatic detonation with seeking crystals,
        /// triple-stacked halos, feather explosions, and brilliant light.
        /// </summary>
        public static void ComboExplosionVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Core impacts - triple stacked
            try { UnifiedVFX.SwanLake.Impact(pos, 1.5f * intensity); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 1.2f * intensity);

            // Massive bloom
            SwanLakeVFXLibrary.DrawBloom(pos, 0.8f * intensity);

            // Triple-stacked halo rings (monochrome + rainbow)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.3f * intensity);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 5, 0.25f * intensity);

            // Massive prismatic sparkle ring
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 12, 25f * intensity);

            // Feather explosion
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(8 * intensity), 0.35f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, (int)(4 * intensity), 0.3f);

            // Prismatic swirl
            SwanLakeVFXLibrary.SpawnPrismaticSwirl(pos, 8, 50f * intensity);

            // Massive music note scatter
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.2f, 35);

            // Radial dust bursts
            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 16, 7f * intensity);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 12, 6f * intensity);

            // Fractal gem burst
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.PureWhite, intensity); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.5f * intensity);
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        /// <summary>
        /// Projectile death VFX: rainbow sparkles, dual-polarity burst, and feather scatter.
        /// </summary>
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 8, 4f);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 6, 3f);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 3, 10f);
            try { CustomParticles.HaloRing(pos, SwanLakePalette.SwanSilver, 0.2f, 10); } catch { }
        }
    }
}
