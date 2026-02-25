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
    /// VFX helper for Feather of the Iridescent Flock summon weapon.
    /// Handles hold-item ambient, summon formation, crystal orbit trail,
    /// flare attack, explosive beam, and death VFX.
    /// The Iridescent Flock: prismatic crystal sentinels orbiting in graceful formation.
    /// </summary>
    public static class FeatheroftheIridescentFlockVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle Swan Lake aura (reduced)
            try { UnifiedVFX.SwanLake.Aura(center, 22f); } catch { }

            // Subtle feathers
            if (Main.rand.NextBool(30))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(16f, 16f), featherCol, 0.12f); } catch { }
            }

            // Subtle rainbow shimmer
            if (Main.rand.NextBool(12))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.RainbowTorch, Vector2.Zero, 0, rainbow * 0.4f, 0.5f);
                d.noGravity = true;
            }

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.2f);
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
            SwanLakePalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time);
        }

        // =====================================================================
        //  SUMMON FORMATION VFX
        // =====================================================================

        /// <summary>
        /// VFX when crystals are first summoned: rainbow explosion, halo rings,
        /// sparkles, feather explosion, music notes.
        /// </summary>
        public static void SummonFormationVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.8f);
            try { ThemedParticles.SwanLakeMusicalImpact(pos, 0.7f); } catch { }
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.25f);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 6, 15f);
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 4, 0.25f);
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 25);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  CRYSTAL ORBIT TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame crystal orbit trail: heavy rainbow sparkle trail,
        /// dual-polarity core, prismatic flares, and feather trail.
        /// </summary>
        public static void CrystalOrbitTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Constant rainbow sparkle trail (heavy)
            Color rainbow = SwanLakePalette.GetVividRainbow(Main.rand.NextFloat());
            Dust r = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                DustID.RainbowTorch, away * Main.rand.NextFloat(0.5f, 1.5f), 0, rainbow, 1.2f);
            r.noGravity = true;

            // Dual-polarity core trail
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);

            // Frequent rainbow flares (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color flareRainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(6f, 6f), flareRainbow, 0.3f, 12); } catch { }
            }

            // Pearlescent flares (1-in-4)
            if (Main.rand.NextBool(4))
            {
                try { CustomParticles.GenericFlare(pos, SwanLakePalette.Pearlescent * 0.6f, 0.25f, 10); } catch { }
            }

            // Feather trail (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos, featherCol, 0.18f); } catch { }
            }

            // Music notes (1-in-8)
            if (Main.rand.NextBool(8))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.35f);
        }

        // =====================================================================
        //  FLARE ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Explosive flare burst when crystals attack: massive rainbow flare ring,
        /// dual-polarity contrast burst, bloom, halo rings, and feather burst.
        /// </summary>
        public static void FlareAttackVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Explosive rainbow flare burst (14 flares)
            for (int i = 0; i < 14; i++)
            {
                float hue = (float)i / 14f;
                Color flareCol = Main.hslToRgb(hue, 1f, 0.8f);
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                try { CustomParticles.GenericFlare(pos + offset, flareCol, 0.5f * intensity, 16); } catch { }
            }

            // Massive rainbow spark burst (24 dust)
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 24, 6f * intensity);

            // Dual-polarity contrast burst
            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 12, 5f * intensity);

            // Bloom
            SwanLakeVFXLibrary.DrawBloom(pos, 0.6f * intensity);

            // Halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.25f * intensity);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 3, 0.2f * intensity);

            // Feather burst
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(5 * intensity), 0.3f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 4, 20f, 0.75f, 1.0f, 25);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 1.0f * intensity);
        }

        // =====================================================================
        //  EXPLOSIVE BEAM VFX
        // =====================================================================

        /// <summary>
        /// Explosive beam VFX: rainbow explosion, halos, sparkles, feather spiral,
        /// rainbow meteor shower (36 dust toward target), and massive light.
        /// </summary>
        public static void ExplosiveBeamVFX(Vector2 pos, Vector2 targetDir, float intensity = 1f)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 1.0f * intensity);
            try { ThemedParticles.SwanLakeMusicalImpact(pos, intensity); } catch { }

            // Rainbow halo rings
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 5, 0.3f * intensity);
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.25f * intensity);

            // Prismatic sparkles
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, (int)(10 * intensity), 25f);

            // Rainbow meteor shower (36 dust toward target direction)
            for (int i = 0; i < 36; i++)
            {
                float hue = (float)i / 36f;
                Color meteorCol = Main.hslToRgb(hue, 1f, 0.75f);
                float spreadAngle = Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 vel = targetDir.RotatedBy(spreadAngle) * Main.rand.NextFloat(4f, 10f);
                Dust d = Dust.NewDustPerfect(pos, DustID.RainbowTorch, vel, 0, meteorCol, 1.5f);
                d.noGravity = true;
            }

            // Feather spiral
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(8 * intensity), 0.35f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, (int)(4 * intensity), 0.3f);

            // Prismatic swirl
            SwanLakeVFXLibrary.SpawnPrismaticSwirl(pos, 8, 60f * intensity);

            // Massive music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.2f, 35);

            // Bloom
            SwanLakeVFXLibrary.DrawBloom(pos, 0.8f * intensity);

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.5f * intensity);
        }

        // =====================================================================
        //  CRYSTAL IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Crystal hit impact: rainbow explosion, music notes, feathers, sparkles.
        /// </summary>
        public static void CrystalImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, 0.8f);
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.6f);
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.PureWhite, 0.6f); } catch { }
        }
    }
}
