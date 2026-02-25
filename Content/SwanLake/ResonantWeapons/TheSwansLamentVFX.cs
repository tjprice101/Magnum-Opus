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
    /// VFX helper for The Swan's Lament ranger rifle.
    /// Handles hold-item ambient, item bloom, muzzle flash,
    /// bullet trail, destruction halo, and death VFX.
    /// The Swan's Lament: sorrowful monochrome shots that erupt in color on impact.
    /// </summary>
    public static class TheSwansLamentVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient aura, rainbow flares at muzzle,
        /// feather drifts, and warm light.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 28f); } catch { }

            // Rainbow flares at muzzle tip
            if (Main.rand.NextBool(8))
            {
                Vector2 muzzleOffset = new Vector2(player.direction * 38f, -4f);
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(center + muzzleOffset, rainbow * 0.4f, 0.2f, 10); } catch { }
            }

            // Feather drift
            if (Main.rand.NextBool(22))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(20f, 20f), featherCol, 0.15f); } catch { }
            }

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer Swan Lake item bloom for the rifle sprite.
        /// </summary>
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
        /// Heavy muzzle flash: seeking crystals, dual-polarity sparks,
        /// rainbow explosion, gradient halo rings, and heavy recoil spark bursts.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Core impact (increased scale for rifle)
            try { UnifiedVFX.SwanLake.Impact(muzzlePos, 1.0f); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(muzzlePos, 0.7f);

            // Heavy spark burst (16 particles, directed)
            for (int i = 0; i < 16; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 8f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Rainbow dust burst
            SwanLakeVFXLibrary.SpawnRainbowBurst(muzzlePos, 10, 5f);

            // Gradient halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(muzzlePos, 4, 0.25f);

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(muzzlePos, 5, 12f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(muzzlePos, 3, 15f, 0.75f, 1.0f, 25);

            // Feather burst
            SwanLakeVFXLibrary.SpawnFeatherBurst(muzzlePos, 3, 0.25f);

            Lighting.AddLight(muzzlePos, SwanLakePalette.PureWhite.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  BULLET TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame bullet trail: dual-polarity flaming trail, prismatic sparkles,
        /// rainbow shimmer, and music notes.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Dual-polarity blazing trail
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);

            // Rainbow shimmer (1-in-2)
            SwanLakeVFXLibrary.SpawnRainbowShimmer(pos, away);

            // Prismatic flare (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(5f, 5f), rainbow, 0.3f, 12); } catch { }
            }

            // Fractal trail accents (1-in-5)
            if (Main.rand.NextBool(5))
            {
                try { ThemedParticles.SwanLakeFractalTrail(pos, 0.4f); } catch { }
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        // =====================================================================
        //  DESTRUCTION HALO VFX
        // =====================================================================

        /// <summary>
        /// Destruction halo on major impact: massive monochrome + rainbow explosion,
        /// stacked halos, black/white lightning rays, sparkles, and feather burst.
        /// </summary>
        public static void DestructionHaloVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Core impacts
            try { UnifiedVFX.SwanLake.Impact(pos, 1.5f * intensity); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 1.2f * intensity);
            try { ThemedParticles.SwanLakeMusicalImpact(pos, intensity); } catch { }

            // Massive bloom
            SwanLakeVFXLibrary.DrawBloom(pos, 0.7f * intensity);

            // Stacked halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.3f * intensity);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 4, 0.25f * intensity);

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 10, 25f * intensity);

            // Rainbow flares
            for (int i = 0; i < 8; i++)
            {
                Color rainbow = SwanLakePalette.GetVividRainbow((float)i / 8f);
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(20f, 20f), rainbow, 0.5f, 18); } catch { }
            }

            // Dual-polarity radial burst
            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 18, 7f * intensity);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 14, 6f * intensity);

            // Feather explosion
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(6 * intensity), 0.35f);

            // Music note scatter
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.8f, 1.1f, 30);

            // Fractal gem burst
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.PureWhite, intensity); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.3f * intensity);
        }

        // =====================================================================
        //  BULLET DEATH VFX
        // =====================================================================

        /// <summary>
        /// Bullet death VFX: small dual-polarity burst with rainbow sparks.
        /// </summary>
        public static void BulletDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 6, 3f);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 4, 3f);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 2, 8f);
            try { CustomParticles.HaloRing(pos, SwanLakePalette.SwanSilver, 0.15f, 8); } catch { }
        }
    }
}
