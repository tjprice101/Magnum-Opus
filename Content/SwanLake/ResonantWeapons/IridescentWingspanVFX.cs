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
    /// VFX helper for the Iridescent Wingspan magic staff (3-flare spread).
    /// Handles hold-item ambient (ethereal wing silhouettes), item bloom,
    /// cast VFX (wing unfurl), projectile trail, impact, and death VFX.
    /// The Iridescent Wingspan: ethereal wings manifest as cascading prismatic energy.
    /// </summary>
    public static class IridescentWingspanVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX — ETHEREAL WINGS
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: UNIQUE ethereal wing silhouettes manifesting
        /// as ghostly feathers behind the player, falling feather drift, music notes.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Ethereal wing feathers — ghostly particles behind player (left and right)
            if (Main.rand.NextBool(4))
            {
                float side = Main.rand.NextBool() ? -1f : 1f;
                Vector2 wingOffset = new Vector2(side * Main.rand.NextFloat(12f, 28f), Main.rand.NextFloat(-20f, 5f));
                Vector2 wingPos = center + wingOffset;
                Color wingCol = Color.Lerp(SwanLakePalette.PureWhite, SwanLakePalette.GetRainbow(Main.rand.NextFloat()), 0.3f) * 0.5f;
                var glow = new GenericGlowParticle(wingPos,
                    new Vector2(0f, Main.rand.NextFloat(-0.5f, -0.2f)),
                    wingCol, 0.15f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Falling feather drift (left and right wings)
            if (Main.rand.NextBool(12))
            {
                float side = Main.rand.NextBool() ? -1f : 1f;
                Vector2 driftPos = center + new Vector2(side * Main.rand.NextFloat(10f, 25f), -15f);
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.Pearlescent;
                try { CustomParticles.SwanFeatherDrift(driftPos, featherCol, 0.18f); } catch { }
            }

            // Music notes near staff
            if (Main.rand.NextBool(15))
                SwanLakeVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.7f, 0.85f, 25);

            // Rainbow shimmer light
            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.3f);
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
        //  CAST VFX — WING UNFURL
        // =====================================================================

        /// <summary>
        /// Cast VFX: wing unfurl effect with feather particles bursting outward
        /// in gradient formation, central prismatic burst, gradient halo rings,
        /// feather spiral, music notes, and elegant light.
        /// </summary>
        public static void CastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Wing unfurl: 12 feather particles bursting outward with gradient
            for (int i = 0; i < 12; i++)
            {
                float progress = (float)i / 12f;
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color featherCol = Color.Lerp(SwanLakePalette.PureWhite, SwanLakePalette.GetVividRainbow(progress), progress * 0.5f);
                var glow = new GenericGlowParticle(pos, vel, featherCol * 0.7f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Central prismatic burst
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.7f);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 8, 20f);

            // Gradient halo rings
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.25f);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 3, 0.2f);

            // Feather spiral
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 4, 0.3f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, 3, 0.25f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 28);

            // Gradient spark accent
            for (int i = 0; i < 4; i++)
            {
                Color sparkCol = SwanLakePalette.GetPaletteColor((float)i / 4f);
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(12f, 12f), sparkCol, 0.35f, 14); } catch { }
            }

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame projectile trail: dual-polarity particles, prismatic sparkles,
        /// rainbow flares, pearlescent shimmer trail, feather trail, and music notes.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Dual-polarity trailing particles
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);

            // Rainbow shimmer (1-in-2)
            SwanLakeVFXLibrary.SpawnRainbowShimmer(pos, away);

            // Prismatic flares (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(6f, 6f), rainbow, 0.35f, 14); } catch { }
            }

            // Pearlescent shimmer trail (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color pearls = SwanLakePalette.GetPearlescentShimmer((float)Main.timeForVisualEffects);
                var shimmer = new GenericGlowParticle(pos, away * 0.3f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    pearls * 0.5f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Feather trail (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos, featherCol, 0.2f); } catch { }
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        // =====================================================================
        //  IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Projectile impact: pearlescent rainbow explosion, music notes,
        /// sparkles, feather burst.
        /// </summary>
        public static void ImpactVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, intensity);
            try { UnifiedVFX.SwanLake.Impact(pos, intensity); } catch { }
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.8f * intensity);
            try { ThemedParticles.SwanLakeMusicalImpact(pos, 0.7f * intensity); } catch { }
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        /// <summary>
        /// Projectile death: rainbow explosion, music note burst, fractal gem burst.
        /// </summary>
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.8f);
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 15f, 0.7f, 0.9f, 22);
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 12f);
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.PureWhite, 0.5f); } catch { }
        }
    }
}
