using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Content.Spring.Bosses.Systems.PrimaveraSkySystem;

namespace MagnumOpus.Content.Spring.Bosses.Systems
{
    /// <summary>
    /// Primavera attack VFX choreography — Vivaldi's Spring.
    /// Every attack is layered: telegraph → execution → impact,
    /// each with phase-aware colors, screen effects, and musical accents.
    ///
    /// Phase 1 (Dawn):    Serene, gentle — soft pink petal scatters, warm light flares
    /// Phase 2 (Storm):   Energetic — green wind trails, cherry blossom rain bursts
    /// Phase 3 (Bloom):   Overwhelming — vine telegraphs, petal tornados, bloom cascades
    /// Enrage (Wrath):    Hostile — magenta flashes, thorn trails, sharp and aggressive
    /// </summary>
    public static class PrimaveraAttackVFX
    {
        // ===== NEW PALETTE =====
        private static readonly Color CherryPink = new Color(255, 183, 197);
        private static readonly Color FreshGreen = new Color(124, 252, 0);
        private static readonly Color Lavender = new Color(181, 126, 220);
        private static readonly Color HotPink = new Color(255, 105, 180);
        private static readonly Color ViolentMagenta = new Color(255, 0, 255);
        private static readonly Color DeepCrimson = new Color(139, 0, 0);
        private static readonly Color WarmWhite = new Color(255, 250, 240);
        private static readonly Color WarmAmber = new Color(255, 215, 120);

        // ===== HELPERS =====
        private static int GetPhase(NPC boss) =>
            (float)boss.life / boss.lifeMax > 0.6f ? 0 :
            (float)boss.life / boss.lifeMax > 0.3f ? 1 : 2;

        private static bool IsEnraged() => PrimaveraSky.BossIsEnraged;

        private static Color GetPhaseAccent(int phase) => phase switch
        {
            0 => WarmAmber,
            1 => FreshGreen,
            2 => HotPink,
            _ => ViolentMagenta
        };

        private static Color GetPhasePrimary(int phase) => phase switch
        {
            0 => CherryPink,
            1 => Color.Lerp(CherryPink, FreshGreen, 0.4f),
            2 => HotPink,
            _ => ViolentMagenta
        };

        private static float GetPhaseShakeMult() =>
            IsEnraged() ? 1.5f : 1f;

        #region Phase 1 — Dawn Attacks (PetalStorm, BlossomBreeze, SpringShower)

        // ────────────────────────────────────────────────────────────────────
        // PETAL STORM — Radial petal burst from boss center
        // ────────────────────────────────────────────────────────────────────

        public static void PetalStormTelegraph(Vector2 position, Vector2 direction)
        {
            Color telColor = IsEnraged() ? ViolentMagenta : CherryPink;

            // Converging ring of dots spiraling inward
            TelegraphSystem.ConvergingRing(position, 120f, 60, telColor * 0.6f);

            // Threat line in attack direction
            TelegraphSystem.ThreatLine(position, direction, 380f, 50, telColor * 0.5f);

            // Musical buildup spiral
            Phase10BossVFX.AccelerandoSpiral(position, telColor, 0.6f);

            // Soft center glow building
            BossVFXOptimizer.WarningFlare(position, IsEnraged() ? 0.8f : 0.5f);

            // Dawn: warm sparkle at center
            CustomParticles.GenericFlare(position, WarmAmber, 0.3f, 20);
        }

        public static void PetalStormImpact(Vector2 position)
        {
            float shakeMult = GetPhaseShakeMult();
            bool enraged = IsEnraged();

            // Screen effects
            MagnumScreenEffects.AddScreenShake(5f * shakeMult);
            TriggerBlossomFlash(enraged ? 7f : 4f);

            // Petal scatter — pink bloom ring expanding outward
            Color burstColor = enraged ? ViolentMagenta : CherryPink;
            CustomParticles.ExplosionBurst(position, burstColor, enraged ? 14 : 8);

            // Soft expanding halo
            CustomParticles.HaloRing(position, Lavender, 0.5f, 18);

            // Vibrant bloom particles radiating outward
            int bloomCount = enraged ? 10 : 6;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 2f);
                Color bc = Color.Lerp(CherryPink, Color.White, Main.rand.NextFloat() * 0.4f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, bc, 0.35f, 16));
            }

            // Music note accent — petal impact spawns a note
            CustomParticles.GenericMusicNotes(position, CherryPink, 2, 35f);

            // Dawn warmth flare
            CustomParticles.GenericFlare(position, WarmAmber, 0.4f, 12);
        }

        // ────────────────────────────────────────────────────────────────────
        // BLOSSOM BREEZE — Line of wind-carried petals
        // ────────────────────────────────────────────────────────────────────

        public static void BlossomBreezeTelegraph(Vector2 position, Vector2 direction)
        {
            Color telColor = IsEnraged() ? ViolentMagenta : FreshGreen;

            // Horizontal sweep line
            TelegraphSystem.ThreatLine(position, direction, 450f, 40, telColor * 0.5f);

            // Wind gust preview — green-tinted slide
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 450f, telColor, 0.5f);

            // Scattered sparks along the line showing wind direction
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkPos = position + direction * (100f * i);
                CustomParticles.GenericFlare(sparkPos, FreshGreen * 0.5f, 0.15f, 10);
            }
        }

        public static void BlossomBreezeTrail(Vector2 position, Vector2 velocity)
        {
            bool enraged = IsEnraged();

            // Core petal glow
            Color trailColor = enraged ? ViolentMagenta : CherryPink;
            CustomParticles.GenericFlare(position, trailColor, 0.25f, 8);

            // Green wind streak behind each petal
            Color windColor = enraged ? DeepCrimson : FreshGreen;
            CustomParticles.GlowTrail(position, windColor, enraged ? 0.3f : 0.2f);

            // Occasional music note from the breeze
            if (Main.rand.NextBool(4))
            {
                Color noteColor = enraged ? ViolentMagenta : Lavender;
                CustomParticles.GenericMusicNotes(position, noteColor, 1, 20f);
            }

            // Wind sparkle accents
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkOffset = Main.rand.NextVector2Circular(15f, 15f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    position + sparkOffset, velocity * 0.3f, FreshGreen, 0.15f, 12));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // SPRING SHOWER — Rain of projectiles from above
        // ────────────────────────────────────────────────────────────────────

        public static void SpringShowerTelegraph(Vector2 targetArea)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? ViolentMagenta : WarmAmber;

            // Danger zone on ground
            TelegraphSystem.DangerZone(targetArea, 220f, 40, telColor * 0.3f);

            // Staff line convergence overhead (musical rain preview)
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -220), telColor, 0.8f);

            // Lavender accent dots marking drop zone
            for (int i = 0; i < 5; i++)
            {
                Vector2 dotPos = targetArea + new Vector2(Main.rand.NextFloat(-180f, 180f), -10f);
                CustomParticles.GenericFlare(dotPos, Lavender * 0.4f, 0.12f, 15);
            }
        }

        public static void SpringShowerParticle(Vector2 position)
        {
            bool enraged = IsEnraged();

            // Soft glow on each raindrop petal
            Color color = enraged
                ? Color.Lerp(ViolentMagenta, DeepCrimson, Main.rand.NextFloat(0.3f))
                : Color.Lerp(CherryPink, WarmAmber, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.22f, 6);

            // Ground splash on impact - tiny bloom
            if (Main.rand.NextBool(3))
            {
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    position, new Vector2(0, -0.5f), CherryPink * 0.6f, 0.12f, 10));
            }

            // Music note from the rain
            if (Main.rand.NextBool(5))
                CustomParticles.GenericMusicNotes(position, Lavender, 1, 15f);
        }

        #endregion

        #region Phase 2 — Storm Attacks (VernalVortex, GrowthSurge, FloralBarrage)

        // ────────────────────────────────────────────────────────────────────
        // VERNAL VORTEX — Spiraling petal tornado (boss charges through)
        // ────────────────────────────────────────────────────────────────────

        public static void VernalVortexTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? ViolentMagenta : FreshGreen;

            // Converging spiral onto target
            TelegraphSystem.ConvergingRing(center, 150f, 50, telColor * 0.5f);

            // Spiral chord buildup
            Color[] chordColors = enraged
                ? new[] { ViolentMagenta, DeepCrimson }
                : new[] { FreshGreen, CherryPink };
            Phase10BossVFX.ChordBuildupSpiral(center, chordColors, 0.7f);

            // Ground marker pulse
            TelegraphSystem.ImpactPoint(center, 80f, 50);

            // Wind energy sparkles converging
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6 + (float)Main.timeForVisualEffects * 0.02f;
                Vector2 sparkPos = center + angle.ToRotationVector2() * 120f;
                CustomParticles.GenericFlare(sparkPos, FreshGreen * 0.6f, 0.2f, 12);
            }
        }

        public static void VernalVortexBurst(Vector2 center, int waveIndex)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetPhaseShakeMult();

            // Progressive flash intensity
            float flashPower = 3f + waveIndex * 0.8f;
            if (enraged) TriggerWrathFlash(flashPower);
            else TriggerGrowthFlash(flashPower);

            MagnumScreenEffects.AddScreenShake((3f + waveIndex) * shakeMult);

            // Multi-colored petal explosion
            Color burstColor = Color.Lerp(FreshGreen, CherryPink, waveIndex / 5f);
            if (enraged) burstColor = Color.Lerp(ViolentMagenta, DeepCrimson, waveIndex / 5f);
            CustomParticles.ExplosionBurst(center, burstColor, 10 + waveIndex * 3);

            // Bloom ring cascade — each wave slightly larger
            float ringScale = 0.4f + waveIndex * 0.15f;
            CustomParticles.HaloRing(center, FreshGreen, ringScale, 18);

            // Radial bloom particles
            int bloomCount = 6 + waveIndex * 2;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = angle.ToRotationVector2() * (3f + waveIndex * 0.5f);
                Color bc = Color.Lerp(FreshGreen, CherryPink, i / (float)bloomCount);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bc, 0.3f, 15));
            }

            // Musical accent — note constellation
            Phase10BossVFX.NoteConstellationWarning(center, burstColor, 0.4f + waveIndex * 0.15f);

            // Vortex center flare
            CustomParticles.GenericFlare(center, WarmWhite, 0.6f + waveIndex * 0.1f, 14);
        }

        // ────────────────────────────────────────────────────────────────────
        // GROWTH SURGE — Healing attack with projectile burst
        // ────────────────────────────────────────────────────────────────────

        public static void GrowthSurgeTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? DeepCrimson : FreshGreen;

            // Converging ring — energy being absorbed
            TelegraphSystem.ConvergingRing(center, 100f, 40, telColor * 0.6f);

            // Fermata hold indicator (musical: sustained note)
            Phase10BossVFX.FermataHoldIndicator(center, telColor, 0.5f);

            // Green sparkles converging inward
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4 + (float)Main.timeForVisualEffects * 0.03f;
                Vector2 from = center + angle.ToRotationVector2() * 80f;
                Vector2 vel = (center - from) * 0.04f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(from, vel, FreshGreen, 0.2f, 18));
            }
        }

        public static void GrowthSurgeHealPulse(Vector2 center, float healProgress)
        {
            bool enraged = IsEnraged();
            float radius = 30f + healProgress * 80f;

            // Expanding ring of growth energy
            Color healColor = enraged
                ? Color.Lerp(DeepCrimson, ViolentMagenta, healProgress)
                : Color.Lerp(FreshGreen, CherryPink, healProgress * 0.5f);
            CustomParticles.HaloRing(center, healColor, 0.3f + healProgress * 0.5f, 20);

            // Scattered growth particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                CustomParticles.GenericFlare(center + offset, FreshGreen, 0.2f + healProgress * 0.15f, 15);
            }

            // Core glow intensifying as heal progresses
            CustomParticles.GenericGlow(center, healColor, 0.4f + healProgress * 0.35f, 10);

            // Musical accent — pizzicato pops
            Phase10BossVFX.PizzicatoPop(center, healColor);

            // Ascending green sparkle (growth visualization)
            if (healProgress > 0.3f)
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 2.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    center + Main.rand.NextVector2Circular(20f, 20f), sparkVel, FreshGreen, 0.18f, 16));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // FLORAL BARRAGE — Rapid-fire targeted bursts
        // ────────────────────────────────────────────────────────────────────

        public static void FloralBarrageTelegraph(Vector2 position, Vector2 target)
        {
            bool enraged = IsEnraged();
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            Color telColor = enraged ? ViolentMagenta : CherryPink;

            // Quick crosshair flash at target
            TelegraphSystem.ThreatLine(position, dir, 550f, 25, telColor * 0.7f);
            TelegraphSystem.ImpactPoint(target, 40f, 25);

            // Fortissimo flash (loud! incoming!)
            Phase10BossVFX.FortissimoFlashWarning(position, telColor, enraged ? 1.0f : 0.8f);
        }

        public static void FloralBarrageAfterimage(Vector2 position, int burstNumber)
        {
            bool enraged = IsEnraged();
            float intensity = 0.3f + burstNumber * 0.1f;

            // Color shifts across the barrage (accelerando pattern)
            Color trailColor = enraged
                ? Color.Lerp(ViolentMagenta, DeepCrimson, burstNumber / 8f)
                : Color.Lerp(CherryPink, HotPink, burstNumber / 8f);

            // Core flare trail
            CustomParticles.GenericFlare(position, trailColor, intensity, 10);

            // Halo per burst
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 12, 2);

            // Spiral petal afterimage
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spiralVel = angle.ToRotationVector2() * 1.5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    position, spiralVel, trailColor, 0.18f, 10));
            }

            // Screen micro-shake on each burst (rhythmic feeling)
            MagnumScreenEffects.AddScreenShake(0.5f * GetPhaseShakeMult());
        }

        #endregion

        #region Phase 3 — Full Bloom Attacks (BloomingJudgment, RebornSpring, AprilShowers)

        // ────────────────────────────────────────────────────────────────────
        // BLOOMING JUDGMENT — Signature spectacle (vine telegraphs → multi-wave burst)
        // ────────────────────────────────────────────────────────────────────

        public static void BloomingJudgmentTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? ViolentMagenta : HotPink;

            // Massive converging ring — the big one is coming
            TelegraphSystem.ConvergingRing(center, 220f, 90, telColor * 0.6f);

            // Chord buildup (musical: full harmonic convergence)
            Color[] chordColors = enraged
                ? new[] { ViolentMagenta, DeepCrimson, Color.White }
                : new[] { CherryPink, FreshGreen, Lavender };
            Phase10BossVFX.ChordBuildupSpiral(center, chordColors, 1.2f);

            // Fortissimo warning — THIS IS THE BIG HIT
            Phase10BossVFX.FortissimoFlashWarning(center, telColor, 1.4f);

            // Vine-like green lines radiating outward (attack telegraph)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6 + (float)Main.timeForVisualEffects * 0.01f;
                Vector2 dir = angle.ToRotationVector2();
                TelegraphSystem.ThreatLine(center, dir, 300f, 80, FreshGreen * 0.4f);

                // Growth tip flare at each vine end
                Vector2 tipPos = center + dir * 280f;
                CustomParticles.GenericFlare(tipPos, FreshGreen, 0.25f, 15);
            }

            // Bloom particles growing at telegraph positions
            for (int i = 0; i < 8; i++)
            {
                Vector2 bloomPos = center + Main.rand.NextVector2Circular(180f, 180f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    bloomPos, Vector2.Zero, CherryPink * 0.5f, 0.15f, 20));
            }
        }

        public static void BloomingJudgmentRelease(Vector2 center)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetPhaseShakeMult();

            // MASSIVE screen effects
            MagnumScreenEffects.AddScreenShake(16f * shakeMult);
            if (enraged) TriggerWrathFlash(14f);
            else TriggerVernalFlash(12f);

            // White-hot center flare
            CustomParticles.GenericFlare(center, WarmWhite, 1.5f, 22);

            // Radial bloom halo ring — alternating pink/green
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Color haloColor = i % 2 == 0
                    ? (enraged ? ViolentMagenta : HotPink)
                    : (enraged ? DeepCrimson : FreshGreen);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 60f, haloColor, 0.45f, 18);
            }

            // Tutti full ensemble (musical: ALL instruments at once)
            Color[] tuttiColors = enraged
                ? new[] { ViolentMagenta, DeepCrimson, Color.White }
                : new[] { CherryPink, FreshGreen, Lavender };
            Phase10BossVFX.TuttiFullEnsemble(center, tuttiColors, 1.5f);

            // Bloom burst signature vfx
            BossSignatureVFX.SpringBloomBurst(center, enraged ? 2.0f : 1.5f);

            // Dense bloom cascade ring — 12 blooms radiating outward
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * 4.5f;
                Color bc = Color.Lerp(CherryPink, Lavender, i / 12f);
                if (enraged) bc = Color.Lerp(ViolentMagenta, DeepCrimson, i / 12f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bc, 0.55f, 20));
            }

            // Music note crescendo — 5 notes ascending
            for (int i = 0; i < 5; i++)
            {
                Vector2 notePos = center + new Vector2(Main.rand.NextFloat(-30f, 30f), -20f * i);
                Color noteColor = Color.Lerp(CherryPink, Color.White, i / 5f);
                if (enraged) noteColor = Color.Lerp(ViolentMagenta, Color.White, i / 5f);
                CustomParticles.GenericMusicNotes(notePos, noteColor, 1, 15f + i * 8f);
            }

            // Ascending sparkle cascade
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(60f, 60f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 5f));
                Color sc = enraged ? HotPink : WarmWhite;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, sc, 0.3f, 26));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // REBORN SPRING — Desperation healing + massive expanding ring
        // ────────────────────────────────────────────────────────────────────

        public static void RebornSpringTelegraph(Vector2 position, Vector2 target)
        {
            bool enraged = IsEnraged();
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            Color telColor = enraged ? DeepCrimson : FreshGreen;

            // Threat line
            TelegraphSystem.ThreatLine(position, dir, 750f, 40, telColor * 0.7f);

            // Impact point warning
            TelegraphSystem.ImpactPoint(target, 70f, 40);

            // Crescendo danger rings (building intensity)
            Phase10BossVFX.CrescendoDangerRings(target, telColor, enraged ? 1.0f : 0.8f);

            // Particles reversing direction — everything converges on boss
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6;
                Vector2 from = position + angle.ToRotationVector2() * 120f;
                Vector2 vel = (position - from) * 0.05f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    from, vel, FreshGreen, 0.22f, 20));
            }
        }

        public static void RebornSpringImpact(Vector2 position)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetPhaseShakeMult();

            // Heavy screen impact
            MagnumScreenEffects.AddScreenShake(20f * shakeMult);
            if (enraged) TriggerWrathFlash(16f);
            else TriggerGrowthFlash(14f);

            // Green energy explosion
            Color burstColor = enraged ? ViolentMagenta : FreshGreen;
            CustomParticles.ExplosionBurst(position, burstColor, 16);

            // White-hot center
            CustomParticles.GenericFlare(position, WarmWhite, 1.8f, 25);

            // Radial halo ring — bloom cascade
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Color haloC = Color.Lerp(FreshGreen, CherryPink, i / 10f);
                if (enraged) haloC = Color.Lerp(ViolentMagenta, DeepCrimson, i / 10f);
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 45f, haloC, 0.45f, 16);
            }

            // Bloom burst
            BossSignatureVFX.SpringBloomBurst(position, enraged ? 2.0f : 1.8f);

            // Timpani drumroll impact (musical: powerful low percussion)
            Phase10BossVFX.TimpaniDrumrollImpact(position, burstColor, 1.5f);

            // Bloom ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 5.5f;
                Color bc = Color.Lerp(FreshGreen, CherryPink, i / 10f);
                if (enraged) bc = Color.Lerp(DeepCrimson, ViolentMagenta, i / 10f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, bc, 0.5f, 18));
            }

            // Ascending sparkle shower
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkPos = position + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -Main.rand.NextFloat(2f, 4.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    sparkPos, sparkVel, enraged ? HotPink : WarmWhite, 0.32f, 24));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // APRIL SHOWERS — Massive area denial rain
        // ────────────────────────────────────────────────────────────────────

        public static void AprilShowersTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? ViolentMagenta : Lavender;

            // Large converging ring — wide area warning
            TelegraphSystem.ConvergingRing(center, 280f, 60, telColor * 0.5f);

            // Staff line convergence overhead
            Phase10BossVFX.StaffLineConvergence(center + new Vector2(0, -250), telColor, 1.2f);

            // Warning flare
            BossVFXOptimizer.WarningFlare(center, enraged ? 1.2f : 0.8f);

            // Rain preview — tiny sparkles falling
            for (int i = 0; i < 6; i++)
            {
                Vector2 dropPos = center + new Vector2(Main.rand.NextFloat(-250f, 250f), -200f);
                Vector2 dropVel = new Vector2(0, 3f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    dropPos, dropVel, Lavender * 0.5f, 0.1f, 15));
            }
        }

        public static void AprilShowersWave(Vector2 center, int waveIndex)
        {
            bool enraged = IsEnraged();

            // Progressive flash per wave
            float flashPower = 2f + waveIndex * 0.25f;
            if (enraged) TriggerWrathFlash(flashPower);
            else TriggerBlossomFlash(flashPower);

            // 5-arm spiral pattern of petal flares
            float angle = waveIndex * 0.15f;
            for (int arm = 0; arm < 5; arm++)
            {
                float armAngle = angle + MathHelper.TwoPi * arm / 5f;
                Vector2 pos = center + armAngle.ToRotationVector2() * 40f;
                Color color = enraged
                    ? Color.Lerp(ViolentMagenta, DeepCrimson, arm / 5f)
                    : Color.Lerp(CherryPink, Lavender, arm / 5f);
                CustomParticles.GenericFlare(pos, color, 0.3f, 10);
            }

            // Music notes scattered in the rain
            Color noteColor = enraged ? ViolentMagenta : Lavender;
            CustomParticles.GenericMusicNotes(center, noteColor, 2, 50f);

            // Bloom particles with each wave
            for (int i = 0; i < 3; i++)
            {
                Vector2 bloomPos = center + Main.rand.NextVector2Circular(60f, 60f);
                Vector2 bloomVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f));
                Color bc = Color.Lerp(CherryPink, Lavender, Main.rand.NextFloat());
                if (enraged) bc = Color.Lerp(ViolentMagenta, DeepCrimson, Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new BloomParticle(bloomPos, bloomVel, bc, 0.2f, 14));
            }
        }

        public static void AprilShowersFinale(Vector2 center)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetPhaseShakeMult();

            // MASSIVE finale impact
            MagnumScreenEffects.AddScreenShake(25f * shakeMult);
            if (enraged) TriggerWrathFlash(20f);
            else TriggerRebirthFlash(18f);

            // Supernova center flare
            CustomParticles.GenericFlare(center, WarmWhite, 2.2f, 30);

            // 20-point radial bloom starburst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 pos = center + angle.ToRotationVector2() * 100f;
                Color color = enraged
                    ? Color.Lerp(ViolentMagenta, DeepCrimson, i / 20f)
                    : Color.Lerp(CherryPink, Lavender, i / 20f);
                CustomParticles.GenericFlare(pos, color, 0.8f, 25);
            }

            // Coda finale (musical: final resolution chord)
            Color codaA = enraged ? ViolentMagenta : CherryPink;
            Color codaB = enraged ? DeepCrimson : FreshGreen;
            Phase10BossVFX.CodaFinale(center, codaA, codaB, 2.2f);

            // Massive bloom ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 6f;
                Color bc = Color.Lerp(CherryPink, Lavender, i / 16f);
                if (enraged) bc = Color.Lerp(ViolentMagenta, Color.White, i / 16f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bc, 0.7f, 24));
            }

            // Grand ascending sparkle cascade
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2.5f, 5f));
                Color sc = enraged ? HotPink : WarmWhite;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, sc, 0.38f, 30));
            }

            // Music note fountain — notes rising in a pentatonic arc
            for (int i = 0; i < 6; i++)
            {
                Vector2 notePos = center + new Vector2(-40f + i * 16f, -10f * i);
                Color nc = Color.Lerp(CherryPink, Color.White, i / 6f);
                if (enraged) nc = Color.Lerp(ViolentMagenta, Color.White, i / 6f);
                CustomParticles.GenericMusicNotes(notePos, nc, 1, 12f + i * 10f);
            }
        }

        #endregion
    }
}
