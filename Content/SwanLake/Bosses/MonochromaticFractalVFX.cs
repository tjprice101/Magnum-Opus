using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.Bosses
{
    /// <summary>
    /// VFX helper for The Monochromatic Fractal boss (950K HP).
    /// Handles ambient aura, phase transitions, attack VFX,
    /// SwanSerenade barrage, MonochromaticApocalypse, and death sequence.
    /// The Monochromatic Fractal: existence fractured into black and white,
    /// with iridescent energy bleeding through the cracks.
    /// </summary>
    public static class MonochromaticFractalVFX
    {
        // =====================================================================
        //  AMBIENT AURA — Boss idle/passive VFX
        // =====================================================================

        /// <summary>
        /// Per-frame boss ambient aura: intense dual-polarity particles,
        /// fractal crack particles, prismatic bleed-through, and heavy feather drift.
        /// Call every frame during the boss fight.
        /// </summary>
        public static void AmbientAura(Vector2 center)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Heavy dual-polarity motes (every frame, 1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 motePos = center + Main.rand.NextVector2Circular(60f, 60f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                Dust d = Dust.NewDustPerfect(motePos, dustType, Main.rand.NextVector2Circular(1f, 1f),
                    isWhite ? 0 : 100, col, 0.7f);
                d.noGravity = true;
            }

            // Fractal crack particles — light leaking through broken reality (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 crackPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 50f);
                Vector2 vel = (crackPos - center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f);
                Color crackCol = Color.Lerp(SwanLakePalette.PureWhite, SwanLakePalette.GetRainbow(angle / MathHelper.TwoPi), 0.3f);
                var glow = new GenericGlowParticle(crackPos, vel, crackCol * 0.5f, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Prismatic bleed-through (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color rainbow = SwanLakePalette.GetVividRainbow(Main.rand.NextFloat());
                Vector2 bleedPos = center + Main.rand.NextVector2Circular(40f, 40f);
                try { CustomParticles.GenericFlare(bleedPos, rainbow * 0.5f, 0.3f, 14); } catch { }
            }

            // Heavy feather drift (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(50f, 50f), featherCol, 0.25f); } catch { }
            }

            // Music notes (1-in-10)
            if (Main.rand.NextBool(10))
                SwanLakeVFXLibrary.SpawnMusicNotes(center, 1, 40f, 0.8f, 1.0f, 30);

            // Intense dual-polarity light
            SwanLakeVFXLibrary.AddDualPolarityLight(center, time, 0.8f);
        }

        // =====================================================================
        //  PHASE TRANSITION — Dramatic black-white polarity shift
        // =====================================================================

        /// <summary>
        /// Phase transition VFX: dramatic screen shake, massive monochromatic flash,
        /// dual-polarity explosion, rainbow shockwave, feather cascade, and music burst.
        /// </summary>
        public static void PhaseTransition(Vector2 pos, bool toBlackPhase)
        {
            if (Main.dedServ) return;

            // Screen shake
            MagnumScreenEffects.AddScreenShake(12f);

            // Massive monochromatic flash
            Color flashCol = toBlackPhase ? SwanLakePalette.ObsidianBlack : SwanLakePalette.MonochromaticFlash;

            // Dual-polarity explosion — massive radial burst
            SwanLakeVFXLibrary.SpawnRadialDustBurst(pos, 30, 10f);

            // Pure polarity burst (all one color in direction of phase)
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                int dustType = toBlackPhase ? DustID.Shadowflame : DustID.WhiteTorch;
                Color col = toBlackPhase ? SwanLakePalette.ObsidianBlack : SwanLakePalette.PureWhite;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, toBlackPhase ? 100 : 0, col, 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Rainbow shockwave at boundary
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 2.0f);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 20, 9f);

            // Massive halo ring cascade
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 8, 0.4f);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 6, 0.35f);

            // Feather cascade
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 12, 0.4f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, 8, 0.35f);

            // Prismatic swirl
            SwanLakeVFXLibrary.SpawnPrismaticSwirl(pos, 14, 100f);

            // Music burst
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 8, 50f, 0.9f, 1.3f, 45);

            // Fractal gem burst
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, flashCol, 1.5f); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 2.0f);
        }

        // =====================================================================
        //  SWAN SERENADE ATTACK — Graceful feather barrage
        // =====================================================================

        /// <summary>
        /// SwanSerenade attack VFX: each feather projectile launch gets
        /// a graceful prismatic trail and musical accent.
        /// Call per feather launched.
        /// </summary>
        public static void SwanSerenadeFeatherLaunch(Vector2 launchPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Launch flash
            try { CustomParticles.GenericFlare(launchPos, SwanLakePalette.PureWhite, 0.4f, 12); } catch { }

            // Directional feather burst
            for (int i = 0; i < 3; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(2f, 5f);
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(launchPos, DustID.RainbowTorch, vel, 0, rainbow, 1.2f);
                d.noGravity = true;
            }

            // Music note
            SwanLakeVFXLibrary.SpawnMusicNotes(launchPos, 1, 8f, 0.7f, 0.85f, 20);
        }

        /// <summary>
        /// Per-frame feather projectile trail for SwanSerenade barrage.
        /// </summary>
        public static void SwanSerenadeFeatherTrail(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);
            SwanLakeVFXLibrary.SpawnDualPolarityDust(pos, away);
            SwanLakeVFXLibrary.SpawnRainbowShimmer(pos, away);

            if (Main.rand.NextBool(4))
            {
                Color rainbow = SwanLakePalette.GetRainbow(Main.rand.NextFloat());
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(5f, 5f), rainbow, 0.25f, 10); } catch { }
            }

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.3f);
        }

        // =====================================================================
        //  MONOCHROMATIC APOCALYPSE — Screen-filling extreme VFX
        // =====================================================================

        /// <summary>
        /// MonochromaticApocalypse attack VFX: screen-filling black/white contrast explosion.
        /// Massive dual-polarity shockwave, prismatic shattering, feather storm.
        /// Call once at detonation.
        /// </summary>
        public static void MonochromaticApocalypse(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Heavy screen shake
            MagnumScreenEffects.AddScreenShake(16f);

            // Massive radial bursts (40 particles each)
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float speed = Main.rand.NextFloat(6f, 14f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                // White burst
                Dust w = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, SwanLakePalette.PureWhite, 2.2f);
                w.noGravity = true;
                w.fadeIn = 1.5f;

                // Black burst (offset angle)
                float bAngle = angle + MathHelper.TwoPi / 80f;
                Vector2 bVel = bAngle.ToRotationVector2() * speed * 0.9f;
                Dust b = Dust.NewDustPerfect(pos, DustID.Shadowflame, bVel, 150, SwanLakePalette.ObsidianBlack, 2.0f);
                b.noGravity = true;
                b.fadeIn = 1.5f;
            }

            // Massive rainbow detonation
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 2.5f);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 30, 12f);

            // Massive halo cascade
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 10, 0.5f);
            SwanLakeVFXLibrary.SpawnRainbowHaloRings(pos, 8, 0.4f);

            // Massive prismatic sparkle ring
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 20, 40f);

            // Feather storm
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 16, 0.45f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, 10, 0.4f);

            // Prismatic implosion/explosion swirl
            SwanLakeVFXLibrary.SpawnPrismaticSwirl(pos, 16, 120f);

            // Massive music note scatter
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 10, 60f, 0.9f, 1.4f, 50);

            // Fractal gem burst
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.MonochromaticFlash, 2.0f); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 2.5f);
        }

        // =====================================================================
        //  BOSS ATTACK VFX — Standard attacks
        // =====================================================================

        /// <summary>
        /// Standard boss attack VFX: directed dual-polarity burst with halo.
        /// </summary>
        public static void StandardAttackVFX(Vector2 pos, Vector2 direction, float intensity = 1f)
        {
            if (Main.dedServ) return;

            Enemies.SwanLakeEnemyVFX.AttackFlash(pos, direction, intensity * 1.5f);

            // Additional boss-scale particles
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, (int)(8 * intensity), 5f);
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(3 * intensity), 0.25f);
        }

        /// <summary>
        /// Boss projectile impact (larger scale than enemy).
        /// </summary>
        public static void BossProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, intensity);
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 0.7f * intensity);
        }

        // =====================================================================
        //  DEATH SEQUENCE — Boss death spectacle
        // =====================================================================

        /// <summary>
        /// Boss death sequence VFX: the fractal shatters back into unity.
        /// Screen shake, massive dual-polarity implosion/explosion,
        /// prismatic crystallization, feather rain, and fading iridescence.
        /// </summary>
        public static void DeathSequence(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Heavy screen shake
            MagnumScreenEffects.AddScreenShake(20f);

            // Full finisher slam effects
            SwanLakeVFXLibrary.FinisherSlam(pos, 2.0f);

            // Additional death-specific effects
            // Massive dual-polarity whirlwind
            for (int i = 0; i < 50; i++)
            {
                float angle = MathHelper.TwoPi * i / 50f;
                float radius = Main.rand.NextFloat(30f, 80f);
                Vector2 particlePos = pos + angle.ToRotationVector2() * radius;
                Vector2 vel = (particlePos - pos).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 4f
                    + (pos - particlePos).SafeNormalize(Vector2.Zero) * 2f;

                bool isWhite = i % 2 == 0;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(particlePos, dustType, vel, isWhite ? 0 : 150, col, 2.0f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Prismatic crystallization — massive inward-spiraling rainbow
            SwanLakeVFXLibrary.SpawnPrismaticSwirl(pos, 20, 150f, 0.8f);

            // Final rainbow explosion
            SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 3.0f);
            SwanLakeVFXLibrary.SpawnRainbowBurst(pos, 40, 15f);

            // Feather rain
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 20, 0.5f);
            SwanLakeVFXLibrary.SpawnFeatherDuality(pos, 12, 0.4f);

            // Massive music note celebration
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 12, 70f, 1.0f, 1.5f, 60);

            // Fractal dissolution
            try { ThemedParticles.SwanLakeFractalGemBurst(pos, SwanLakePalette.MonochromaticFlash, 3.0f); } catch { }

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 3.0f);
        }
    }
}
