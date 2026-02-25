using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Enemies
{
    /// <summary>
    /// VFX helper for Nachtmusik-themed enemies and the
    /// Queen of Radiance boss. Shared enemy ambient, attack, death,
    /// projectile VFX, and boss-specific spectacle VFX.
    /// </summary>
    public static class NachtmusikEnemyVFX
    {
        // =====================================================================
        //  SHARED ENEMY VFX
        // =====================================================================

        /// <summary>
        /// Per-frame enemy ambient aura: starlit dust, cosmic shimmer,
        /// subtle twinkling. Scales with size.
        /// </summary>
        public static void AmbientAura(Vector2 center, float scale = 1f)
        {
            if (Main.dedServ) return;

            // Starlit dust aura
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * scale, 20f * scale);
                Color col = NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center + offset, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 0, col, 1.1f * scale);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Cosmic shimmer (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color shimmer = NachtmusikPalette.GetShimmer();
                var glow = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(15f * scale, 15f * scale),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    shimmer * 0.5f, 0.2f * scale, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Twinkling accent (1-in-12)
            if (Main.rand.NextBool(12))
            {
                try { CustomParticles.GenericFlare(
                    center + Main.rand.NextVector2Circular(18f * scale, 18f * scale),
                    NachtmusikPalette.StarWhite * 0.4f, 0.15f * scale, 10); } catch { }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(center, 0.2f * scale);
        }

        /// <summary>
        /// Enemy attack flash: starlit burst, directional sparks, music note.
        /// </summary>
        public static void AttackFlash(Vector2 pos, Vector2 direction, float intensity = 1f)
        {
            if (Main.dedServ) return;

            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.StarlitBlue, 0.5f * intensity, 12); } catch { }
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.DeepBlue, 0.3f * intensity, 10); } catch { }

            for (int i = 0; i < 4; i++)
            {
                float spread = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(2f, 5f) * intensity;
                Color col = NachtmusikPalette.GetCelestialGradient((float)i / 4f);
                var spark = new GlowSparkParticle(pos, vel, col, 0.2f * intensity, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.7f, 0.85f, 20);

            Lighting.AddLight(pos, NachtmusikPalette.StarlitBlue.ToVector3() * 0.5f * intensity);
        }

        /// <summary>
        /// Enemy death burst: radial dust, starburst, gradient halos,
        /// shattered starlight, music notes.
        /// </summary>
        public static void DeathBurst(Vector2 pos, float scale = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.ProjectileImpact(pos, scale);
            NachtmusikVFXLibrary.DrawBloom(pos, 0.4f * scale);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, (int)(10 * scale), 4f * scale);
            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f * scale);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, (int)(4 * scale), 4f * scale, 0.5f * scale, true);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, (int)(3 * scale), 15f * scale);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 3, 18f * scale, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 0.7f * scale);
        }

        /// <summary>
        /// Enemy projectile trail: starlit dust and shimmer.
        /// </summary>
        public static void EnemyProjectileTrail(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch,
                Main.rand.NextVector2Circular(0.5f, 0.5f), 0,
                NachtmusikPalette.StarlitBlue, 1.2f);
            d.noGravity = true;

            if (Main.rand.NextBool(3))
            {
                Color shimmer = NachtmusikPalette.GetShimmer();
                var glow = new GenericGlowParticle(pos,
                    -velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    shimmer * 0.5f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.25f);
        }

        /// <summary>
        /// Enemy projectile impact: radial dust and sparkles.
        /// </summary>
        public static void EnemyProjectileImpact(Vector2 pos, float intensity = 0.6f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 6, 3f * intensity);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 2, 10f);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.StarlitBlue * 0.5f, 0.2f * intensity, 10); } catch { }

            Lighting.AddLight(pos, NachtmusikPalette.StarlitBlue.ToVector3() * 0.4f * intensity);
        }

        // =====================================================================
        //  QUEEN OF RADIANCE — BOSS AMBIENT
        // =====================================================================

        /// <summary>
        /// Per-frame Queen of Radiance ambient: grand celestial aura,
        /// orbiting constellation points, golden radiance corona,
        /// cosmic authority particles.
        /// </summary>
        public static void QueenAmbientAura(Vector2 center)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Dense celestial aura (3 per frame)
            for (int d = 0; d < 3; d++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color col = NachtmusikPalette.GetNocturnalGradient(Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(center + offset, DustID.BlueTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, col, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            // Golden radiance corona (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Color radiance = Color.Lerp(NachtmusikPalette.RadianceGold,
                    NachtmusikPalette.StarWhite, Main.rand.NextFloat() * 0.4f);
                var glow = new GenericGlowParticle(center + offset,
                    new Vector2(0, -0.8f),
                    radiance * 0.5f, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Orbiting constellation ring (periodic)
            if (Main.GameUpdateCount % 10 == 0)
            {
                float baseAngle = time * 0.04f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    Vector2 starPos = center + angle.ToRotationVector2() * 45f;
                    try { CustomParticles.GenericFlare(starPos,
                        NachtmusikPalette.GetStarfieldGradient((float)i / 5f),
                        0.35f, 14); } catch { }
                }
            }

            // Authority sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(
                    center + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    NachtmusikPalette.RadianceGold * 0.6f, 0.45f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Boss music notes (1-in-4)
            if (Main.rand.NextBool(4))
                NachtmusikVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.8f, 0.9f, 28);

            // Grand boss light
            float pulse = 0.5f + MathF.Sin(time * 0.08f) * 0.1f;
            Lighting.AddLight(center, NachtmusikPalette.RadianceGold.ToVector3() * pulse);
        }

        // =====================================================================
        //  QUEEN OF RADIANCE — PHASE TRANSITION
        // =====================================================================

        /// <summary>
        /// Phase transition spectacle: grand celestial flash, constellation
        /// explosion, halo cascade, starburst wave, music note shower.
        /// </summary>
        public static void QueenPhaseTransition(Vector2 pos, bool toRadiantPhase)
        {
            if (Main.dedServ) return;

            // Screen shake
            MagnumScreenEffects.AddScreenShake(6f);

            // Grand flash
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.StarWhite, 1.2f, 22); } catch { }
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.RadianceGold, 0.9f, 20); } catch { }

            // Constellation explosion
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 60f, 10,
                Main.rand.NextFloat(MathHelper.TwoPi));

            // Phase-specific color burst
            Color phaseColor = toRadiantPhase ? NachtmusikPalette.RadianceGold : NachtmusikPalette.CosmicPurple;
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color col = Color.Lerp(phaseColor, NachtmusikPalette.StarWhite, (float)i / 20f);
                var spark = new GlowSparkParticle(pos, vel, col, 0.35f, 22);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Halo cascade
            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.4f);
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 4, 0.35f);

            // Starburst wave
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 14, 8f, 0.5f);

            // Shattered starlight
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 8, 6f, 0.8f, true);

            // Grand music note shower
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.0f, 35);

            // Grand bloom
            NachtmusikVFXLibrary.DrawBloom(pos, 0.8f);

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 1.2f);
        }

        // =====================================================================
        //  QUEEN OF RADIANCE — ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Queen's standard attack VFX: enhanced boss-scale attack flash.
        /// </summary>
        public static void QueenAttackVFX(Vector2 pos, Vector2 direction, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Boss-scale attack — 1.5x base enemy intensity
            float bossIntensity = intensity * 1.5f;

            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.RadianceGold, 0.6f * bossIntensity, 14); } catch { }
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.DeepBlue, 0.4f * bossIntensity, 12); } catch { }

            for (int i = 0; i < 6; i++)
            {
                float spread = Main.rand.NextFloat(-0.25f, 0.25f);
                Vector2 vel = direction.RotatedBy(spread) * Main.rand.NextFloat(3f, 7f) * bossIntensity;
                Color col = NachtmusikPalette.GetStarfieldGradient((float)i / 6f);
                var spark = new GlowSparkParticle(pos, vel, col, 0.28f * bossIntensity, 16);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.8f, 0.95f, 25);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 2, 12f);

            Lighting.AddLight(pos, NachtmusikPalette.RadianceGold.ToVector3() * 0.7f * bossIntensity);
        }

        /// <summary>
        /// Queen's boss projectile impact: larger-scale than enemy version.
        /// </summary>
        public static void QueenProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, (int)(10 * intensity), 5f * intensity);
            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f * intensity);
            NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3, 12f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(pos, 4, 3f * intensity);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 0.6f * intensity);
        }

        // =====================================================================
        //  QUEEN OF RADIANCE — SPECIAL ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Queen's Radiance Nova: screen-filling golden supernova attack.
        /// Massive starburst, radiance explosion, constellation web,
        /// golden particle storm.
        /// </summary>
        public static void QueenRadianceNova(Vector2 pos)
        {
            if (Main.dedServ) return;

            MagnumScreenEffects.AddScreenShake(8f);

            // Massive multi-layer flash
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.RadianceGold, 1.5f, 24); } catch { }
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.StarWhite, 1.2f, 22); } catch { }
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.TwinklingWhite, 1.0f, 20); } catch { }

            // Radiance golden particle storm
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color col = Color.Lerp(NachtmusikPalette.RadianceGold,
                    NachtmusikPalette.StarWhite, Main.rand.NextFloat() * 0.5f);
                var spark = new GlowSparkParticle(pos, vel, col, 0.4f, 25);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Grand constellation web
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 80f, 12,
                Main.rand.NextFloat(MathHelper.TwoPi));

            // Massive halo cascade
            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 8, 0.5f);
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 6, 0.4f);

            // Starburst rain
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 16, 10f, 0.6f);

            // Shattered starlight shower
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 10, 8f, 1f, true);

            // Boss bloom
            NachtmusikVFXLibrary.DrawBloom(pos, 1.0f);

            // Grand music note explosion
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.0f, 35);

            Lighting.AddLight(pos, NachtmusikPalette.RadianceGold.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  QUEEN OF RADIANCE — DEATH SEQUENCE
        // =====================================================================

        /// <summary>
        /// Queen of Radiance death spectacle: massive celestial implosion
        /// then supernova expansion, constellation dissolution,
        /// golden stardust rain, culminating music note cascade.
        /// </summary>
        public static void QueenDeathSequence(Vector2 pos)
        {
            if (Main.dedServ) return;

            MagnumScreenEffects.AddScreenShake(10f);

            // Massive death flash
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.StarWhite, 2.0f, 28); } catch { }
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.RadianceGold, 1.5f, 26); } catch { }
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.CosmicPurple, 1.0f, 24); } catch { }

            // Supernova particle whirlwind
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float speed = Main.rand.NextFloat(4f, 14f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = NachtmusikPalette.GetNocturnalGradient((float)i / 40f);
                int dustType = i % 3 == 0 ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Golden radiance crystallization
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color col = Color.Lerp(NachtmusikPalette.RadianceGold,
                    NachtmusikPalette.StarWhite, (float)i / 16f);
                var spark = new GlowSparkParticle(pos, vel, col, 0.5f, 30);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Constellation dissolution
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 100f, 16,
                Main.rand.NextFloat(MathHelper.TwoPi));

            // Massive halo cascade
            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 10, 0.6f);
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 8, 0.5f);

            // Starburst dissolution
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 20, 12f, 0.7f);

            // Shattered starlight storm
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 14, 10f, 1.2f, true);

            // Finisher slam
            NachtmusikVFXLibrary.FinisherSlam(pos, 1.5f);

            // Culminating music note cascade
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 10, 50f, 0.8f, 1.0f, 40);

            Lighting.AddLight(pos, NachtmusikPalette.StarWhite.ToVector3() * 2f);
        }
    }
}
