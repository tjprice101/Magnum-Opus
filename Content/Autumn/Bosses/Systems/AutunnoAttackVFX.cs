using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Content.Autumn.Bosses.Systems.AutunnoSkySystem;

namespace MagnumOpus.Content.Autumn.Bosses.Systems
{
    /// <summary>
    /// Autunno boss attack VFX choreography — Vivaldi's Autumn across 4 phases.
    /// Every attack: telegraph → execution → impact, phase-aware colors and intensity.
    ///
    /// Phase 1 (Twilight Hunt):   Warm amber spirals, gentle leaf vortex, golden flares
    /// Phase 2 (Harvest Reaping): Scythe-arc telegraphs, decay bursts, foggy ground slams
    /// Phase 3 (Death of Year):   Skeletal branching, ashen blasts, desaturated everything
    /// Phase 4 (Funeral Pyre):    Ember silhouette attacks, dying flame bursts, constricting fog
    /// </summary>
    public static class AutunnoAttackVFX
    {
        // Full autumn palette — warm to cold to ember
        private static readonly Color TwilightAmber = new Color(200, 120, 40);
        private static readonly Color HarvestGold = new Color(218, 165, 32);
        private static readonly Color DecayBrown = new Color(100, 60, 30);
        private static readonly Color WitheredRed = new Color(150, 50, 30);
        private static readonly Color AshenGray = new Color(120, 110, 105);
        private static readonly Color TwilightPurple = new Color(100, 60, 120);
        private static readonly Color EmberOrange = new Color(255, 100, 30);
        private static readonly Color AutumnWhite = new Color(255, 240, 220);

        // ===== HELPERS =====
        private static bool IsEnraged() => AutunnoSky.BossIsEnraged;
        private static float GetShakeMult() => IsEnraged() ? 1.5f : 1f;

        private static Color GetPhasePrimary(int phase) => phase switch
        {
            1 => TwilightAmber,
            2 => DecayBrown,
            3 => TwilightPurple,
            _ => EmberOrange
        };

        private static Color GetPhaseAccent(int phase) => phase switch
        {
            1 => HarvestGold,
            2 => WitheredRed,
            3 => AshenGray,
            _ => WitheredRed
        };

        #region Phase 1 — Twilight Hunt (LeafVortexStrike, AutumnalGust, TwilightDash)

        // ────────────────────────────────────────────────────────────────────
        // LEAF VORTEX STRIKE — Spiral leaf convergence → collapse → burst
        // ────────────────────────────────────────────────────────────────────

        public static void LeafVortexTelegraph(Vector2 position, Vector2 direction)
        {
            Color telColor = IsEnraged() ? EmberOrange : TwilightAmber;

            // Spiral convergence ring
            TelegraphSystem.ConvergingRing(position, 130f, 60, telColor * 0.6f);

            // Threat line in attack direction
            TelegraphSystem.ThreatLine(position, direction, 380f, 50, telColor * 0.5f);

            // Musical spiral buildup — leaves spiraling inward
            Phase10BossVFX.AccelerandoSpiral(position, HarvestGold, 0.6f);

            // Soft center glow
            BossVFXOptimizer.WarningFlare(position, IsEnraged() ? 0.7f : 0.4f);

            // Warm amber sparkle at core
            CustomParticles.GenericFlare(position, HarvestGold, 0.3f, 20);
        }

        public static void LeafVortexImpact(Vector2 position)
        {
            float shakeMult = GetShakeMult();
            bool enraged = IsEnraged();

            MagnumScreenEffects.AddScreenShake(6f * shakeMult);
            TriggerTwilightFlash(enraged ? 6f : 4f);

            // Leaf burst — amber particles scattering outward
            Color burstColor = enraged ? EmberOrange : TwilightAmber;
            CustomParticles.ExplosionBurst(position, burstColor, enraged ? 14 : 10);

            // Expanding golden halo
            CustomParticles.HaloRing(position, HarvestGold, 0.5f, 16);

            // Cymbal crash on impact
            Phase10BossVFX.CymbalCrashBurst(position, 0.7f);

            // Music notes scattered from the burst
            CustomParticles.GenericMusicNotes(position, TwilightAmber, 3, 40f);

            // Bloom particles radiating outward
            int bloomCount = enraged ? 8 : 5;
            for (int i = 0; i < bloomCount; i++)
            {
                float angle = MathHelper.TwoPi * i / bloomCount;
                Vector2 vel = angle.ToRotationVector2() * (2.5f + Main.rand.NextFloat() * 1.5f);
                Color bc = Color.Lerp(TwilightAmber, HarvestGold, Main.rand.NextFloat() * 0.5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, bc, 0.35f, 14));
            }

            // Warm center flare
            CustomParticles.GenericFlare(position, AutumnWhite, 0.5f, 12);
        }

        // ────────────────────────────────────────────────────────────────────
        // AUTUMNAL GUST — Wind wall sweeping across arena
        // ────────────────────────────────────────────────────────────────────

        public static void AutumnalGustTelegraph(Vector2 position, Vector2 direction)
        {
            Color telColor = IsEnraged() ? EmberOrange : HarvestGold;

            // Horizontal sweep line
            TelegraphSystem.ThreatLine(position, direction, 500f, 40, telColor * 0.5f);

            // Wind slide preview
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 500f, telColor, 0.5f);

            // Sparks along the gust line
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkPos = position + direction * (120f * i);
                CustomParticles.GenericFlare(sparkPos, TwilightAmber * 0.5f, 0.15f, 10);
            }
        }

        public static void AutumnalGustTrail(Vector2 position, Vector2 velocity)
        {
            bool enraged = IsEnraged();

            // Core leaf glow
            Color trailColor = enraged ? EmberOrange : TwilightAmber;
            CustomParticles.GenericFlare(position, trailColor, 0.25f, 8);

            // Wind streak behind
            Color windColor = enraged ? WitheredRed : HarvestGold;
            CustomParticles.GlowTrail(position, windColor, enraged ? 0.3f : 0.2f);

            // Occasional music note from the breeze
            if (Main.rand.NextBool(4))
                CustomParticles.GenericMusicNotes(position, HarvestGold, 1, 20f);

            // Leaf sparkle accents
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkOffset = Main.rand.NextVector2Circular(12f, 12f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    position + sparkOffset, velocity * 0.3f, HarvestGold, 0.12f, 10));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // TWILIGHT DASH — Boss charges through leaving a foliage cascade
        // ────────────────────────────────────────────────────────────────────

        public static void TwilightDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            Color telColor = IsEnraged() ? EmberOrange : TwilightAmber;

            TelegraphSystem.ThreatLine(position, dir, 550f, 30, telColor * 0.7f);
            Phase10BossVFX.FortissimoFlashWarning(position, telColor, IsEnraged() ? 1.0f : 0.7f);
        }

        public static void TwilightDashAfterimage(Vector2 position, int dashNumber)
        {
            bool enraged = IsEnraged();
            float intensity = 0.3f + dashNumber * 0.1f;

            Color trailColor = enraged
                ? Color.Lerp(EmberOrange, WitheredRed, dashNumber / 6f)
                : Color.Lerp(TwilightAmber, HarvestGold, dashNumber / 6f);

            CustomParticles.GenericFlare(position, trailColor, intensity, 8);
            BossVFXOptimizer.OptimizedHalo(position, trailColor, intensity, 12, 2);

            // Leaf afterimage — scattered bloom
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spiralVel = angle.ToRotationVector2() * 1.2f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    position, spiralVel, trailColor, 0.15f, 10));
            }

            MagnumScreenEffects.AddScreenShake(0.4f * GetShakeMult());
        }

        #endregion

        #region Phase 2 — Harvest Reaping (HarvestSweep, ReapingStrike, WitherBurst)

        // ────────────────────────────────────────────────────────────────────
        // HARVEST SWEEP — Scythe-arc attack with golden edge
        // ────────────────────────────────────────────────────────────────────

        public static void HarvestSweepTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? EmberOrange : DecayBrown;

            // Converging arc
            TelegraphSystem.ConvergingRing(center, 140f, 50, telColor * 0.5f);

            // Chord buildup — harvest reaping energy
            Color[] chordColors = enraged
                ? new[] { EmberOrange, WitheredRed }
                : new[] { HarvestGold, DecayBrown };
            Phase10BossVFX.ChordBuildupSpiral(center, chordColors, 0.7f);

            // Ground marker
            TelegraphSystem.ImpactPoint(center, 70f, 50);

            // Decay sparkles converging
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5 + (float)Main.timeForVisualEffects * 0.02f;
                Vector2 sparkPos = center + angle.ToRotationVector2() * 110f;
                CustomParticles.GenericFlare(sparkPos, DecayBrown * 0.6f, 0.18f, 12);
            }
        }

        public static void HarvestSweepImpact(Vector2 position)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetShakeMult();

            MagnumScreenEffects.AddScreenShake(10f * shakeMult);
            TriggerHarvestFlash(enraged ? 8f : 5f);

            // Scythe-arc burst
            Color burstColor = enraged ? EmberOrange : HarvestGold;
            CustomParticles.ExplosionBurst(position, burstColor, enraged ? 14 : 10);

            // Golden halo
            CustomParticles.HaloRing(position, HarvestGold, 0.6f, 18);

            // Sforzando spike — sharp musical accent
            Phase10BossVFX.SforzandoSpike(position, burstColor, 0.9f);

            // Bloom particles in scythe pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3.5f;
                Color bc = Color.Lerp(HarvestGold, DecayBrown, i / 8f);
                if (enraged) bc = Color.Lerp(EmberOrange, WitheredRed, i / 8f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, bc, 0.4f, 14));
            }

            // Leaf storm signature on big hits
            BossSignatureVFX.AutumnLeafStorm(position, enraged ? 1.3f : 1.0f);

            // Music notes
            CustomParticles.GenericMusicNotes(position, burstColor, 3, 35f);
        }

        // ────────────────────────────────────────────────────────────────────
        // REAPING STRIKE — Ground slam with decay wave
        // ────────────────────────────────────────────────────────────────────

        public static void ReapingStrikeTelegraph(Vector2 position, Vector2 target)
        {
            bool enraged = IsEnraged();
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            Color telColor = enraged ? WitheredRed : DecayBrown;

            TelegraphSystem.ThreatLine(position, dir, 650f, 35, telColor * 0.7f);
            TelegraphSystem.ImpactPoint(target, 60f, 35);
            Phase10BossVFX.CrescendoDangerRings(target, telColor, enraged ? 0.9f : 0.7f);

            // Converging decay particles
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5;
                Vector2 from = target + angle.ToRotationVector2() * 100f;
                Vector2 vel = (target - from) * 0.04f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(from, vel, DecayBrown, 0.18f, 18));
            }
        }

        public static void ReapingStrikeImpact(Vector2 position)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetShakeMult();

            MagnumScreenEffects.AddScreenShake(14f * shakeMult);
            TriggerWitheringFlash(enraged ? 10f : 6f);

            // Heavy ground burst
            Color burstColor = enraged ? WitheredRed : DecayBrown;
            CustomParticles.ExplosionBurst(position, burstColor, 14);
            CustomParticles.GenericFlare(position, HarvestGold, 1.2f, 20);

            // Multi-halo cascade at different radii
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Color haloC = Color.Lerp(DecayBrown, HarvestGold, i / 8f);
                if (enraged) haloC = Color.Lerp(WitheredRed, EmberOrange, i / 8f);
                CustomParticles.HaloRing(position + angle.ToRotationVector2() * 40f, haloC, 0.4f, 15);
            }

            // Timpani drumroll impact — deep percussive hit
            Phase10BossVFX.TimpaniDrumrollImpact(position, burstColor, enraged ? 1.3f : 1.0f);

            // Bloom ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 4.5f;
                Color bc = Color.Lerp(DecayBrown, HarvestGold, i / 8f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(position, vel, bc, 0.4f, 14));
            }

            // Ascending decay sparkles
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkPos = position + Main.rand.NextVector2Circular(40f, 40f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1.5f, 3.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    sparkPos, sparkVel, HarvestGold, 0.25f, 22));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // WITHER BURST — Radial explosion of decay energy
        // ────────────────────────────────────────────────────────────────────

        public static void WitherBurstTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? WitheredRed : DecayBrown;

            TelegraphSystem.ConvergingRing(center, 160f, 50, telColor * 0.5f);
            Phase10BossVFX.AccelerandoSpiral(center, telColor, 0.8f);

            // Fermata hold — building tension
            Phase10BossVFX.FermataHoldIndicator(center, telColor, 0.5f);
        }

        public static void WitherBurstWave(Vector2 center, int waveIndex)
        {
            bool enraged = IsEnraged();

            float flashPower = 2f + waveIndex * 0.6f;
            TriggerWitheringFlash(enraged ? flashPower * 1.3f : flashPower);

            MagnumScreenEffects.AddScreenShake((2f + waveIndex * 1.5f) * GetShakeMult());

            // Progressive decay burst
            Color burstColor = Color.Lerp(DecayBrown, WitheredRed, waveIndex / 5f);
            if (enraged) burstColor = Color.Lerp(WitheredRed, EmberOrange, waveIndex / 5f);
            CustomParticles.ExplosionBurst(center, burstColor, 8 + waveIndex * 3);

            // Expanding halo ring
            CustomParticles.HaloRing(center, burstColor, 0.4f + waveIndex * 0.15f, 16);

            // Note constellation — musical debris
            Phase10BossVFX.NoteConstellationWarning(center, burstColor, 0.4f + waveIndex * 0.12f);

            // Centre flare
            CustomParticles.GenericFlare(center, HarvestGold, 0.5f + waveIndex * 0.08f, 12);

            // Bloom particles per wave
            for (int i = 0; i < 4 + waveIndex; i++)
            {
                float angle = MathHelper.TwoPi * i / (4 + waveIndex);
                Vector2 vel = angle.ToRotationVector2() * (2.5f + waveIndex * 0.5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, burstColor, 0.25f, 12));
            }
        }

        #endregion

        #region Phase 3 — Death of the Year (BranchStrike, AshenBlast, FinalHarvest)

        // ────────────────────────────────────────────────────────────────────
        // BRANCH STRIKE — Skeletal branching telegraph → piercing attack
        // ────────────────────────────────────────────────────────────────────

        public static void BranchStrikeTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? EmberOrange : TwilightPurple;

            // Branching threat lines radiating outward — skeletal branches
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5 + (float)Main.timeForVisualEffects * 0.008f;
                Vector2 dir = angle.ToRotationVector2();
                TelegraphSystem.ThreatLine(center, dir, 280f, 60, telColor * 0.4f);

                // Branch tip glow
                Vector2 tipPos = center + dir * 260f;
                CustomParticles.GenericFlare(tipPos, AshenGray, 0.2f, 12);
            }

            // Chord buildup — ashen harmony
            Color[] chordColors = enraged
                ? new[] { EmberOrange, WitheredRed, AutumnWhite }
                : new[] { TwilightPurple, AshenGray };
            Phase10BossVFX.ChordBuildupSpiral(center, chordColors, 0.9f);

            // Fortissimo warning
            Phase10BossVFX.FortissimoFlashWarning(center, telColor, enraged ? 1.3f : 1.0f);

            // Gray bloom building at telegraph positions
            for (int i = 0; i < 6; i++)
            {
                Vector2 bloomPos = center + Main.rand.NextVector2Circular(160f, 160f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    bloomPos, Vector2.Zero, AshenGray * 0.5f, 0.12f, 18));
            }
        }

        public static void BranchStrikeRelease(Vector2 center)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetShakeMult();

            MagnumScreenEffects.AddScreenShake(14f * shakeMult);
            TriggerWitheringFlash(enraged ? 12f : 8f);

            // White-gray center burst
            CustomParticles.GenericFlare(center, AutumnWhite, 1.2f, 20);

            // Radial halo ring — alternating purple/gray
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Color haloColor = i % 2 == 0
                    ? (enraged ? EmberOrange : TwilightPurple)
                    : (enraged ? WitheredRed : AshenGray);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 55f, haloColor, 0.4f, 16);
            }

            // Tutti full ensemble —  all instruments
            Color[] tuttiColors = enraged
                ? new[] { EmberOrange, WitheredRed, AutumnWhite }
                : new[] { TwilightPurple, AshenGray, DecayBrown };
            Phase10BossVFX.TuttiFullEnsemble(center, tuttiColors, 1.3f);

            // Bloom cascade ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 4f;
                Color bc = Color.Lerp(TwilightPurple, AshenGray, i / 10f);
                if (enraged) bc = Color.Lerp(EmberOrange, WitheredRed, i / 10f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bc, 0.45f, 18));
            }

            // Ascending ashen sparkles
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), -Main.rand.NextFloat(2f, 4f));
                Color sc = enraged ? EmberOrange : AshenGray;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, sc, 0.28f, 24));
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // ASHEN BLAST — Cone of ash and decay
        // ────────────────────────────────────────────────────────────────────

        public static void AshenBlastTelegraph(Vector2 position, Vector2 direction)
        {
            Color telColor = IsEnraged() ? EmberOrange : AshenGray;

            TelegraphSystem.ThreatLine(position, direction, 450f, 35, telColor * 0.6f);
            Phase10BossVFX.GlissandoSlideWarning(position, position + direction * 450f, telColor, 0.6f);

            // Cone preview dots
            for (int i = -2; i <= 2; i++)
            {
                float spread = i * 0.15f;
                Vector2 spreadDir = (direction.ToRotation() + spread).ToRotationVector2();
                Vector2 dotPos = position + spreadDir * 200f;
                CustomParticles.GenericFlare(dotPos, AshenGray * 0.4f, 0.12f, 10);
            }
        }

        public static void AshenBlastImpact(Vector2 position)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetShakeMult();

            MagnumScreenEffects.AddScreenShake(8f * shakeMult);
            TriggerWitheringFlash(enraged ? 6f : 4f);

            // Ash explosion
            Color burstColor = enraged ? Color.Lerp(EmberOrange, AshenGray, 0.3f) : AshenGray;
            CustomParticles.ExplosionBurst(position, burstColor, 12);

            // Glyph burst — arcane death symbols
            CustomParticles.GlyphBurst(position, TwilightPurple, 5);

            // Halo
            CustomParticles.HaloRing(position, AshenGray, 0.5f, 15);

            // Staccato burst — sharp rapid hits
            Phase10BossVFX.StaccatoMultiBurst(position, burstColor, 3);

            // Bloom
            MagnumParticleHandler.SpawnParticle(new BloomParticle(position, Vector2.Zero,
                burstColor, 0.4f, 12));
        }

        // ────────────────────────────────────────────────────────────────────
        // FINAL HARVEST — Ultimate P3 attack: 3 converging arcs + massive impact
        // ────────────────────────────────────────────────────────────────────

        public static void FinalHarvestTelegraph(Vector2 center)
        {
            bool enraged = IsEnraged();
            Color telColor = enraged ? EmberOrange : TwilightPurple;

            // Massive converging ring
            TelegraphSystem.ConvergingRing(center, 250f, 90, telColor * 0.6f);

            // 3 converging threat lines — the scythe arcs
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3 + (float)Main.timeForVisualEffects * 0.01f;
                Vector2 dir = angle.ToRotationVector2();
                TelegraphSystem.ThreatLine(center, dir, 350f, 80, telColor * 0.5f);
            }

            // Full chord buildup — this is the crescendo
            Color[] chordColors = enraged
                ? new[] { EmberOrange, WitheredRed, AutumnWhite }
                : new[] { TwilightPurple, AshenGray, DecayBrown };
            Phase10BossVFX.ChordBuildupSpiral(center, chordColors, 1.3f);

            // Maximum warning
            Phase10BossVFX.FortissimoFlashWarning(center, telColor, 1.5f);
            BossVFXOptimizer.WarningFlare(center, enraged ? 1.2f : 0.9f);
        }

        public static void FinalHarvestRelease(Vector2 center)
        {
            bool enraged = IsEnraged();
            float shakeMult = GetShakeMult();

            // MASSIVE screen effects
            MagnumScreenEffects.AddScreenShake(22f * shakeMult);
            if (enraged) TriggerFuneralFlash(18f);
            else TriggerWitheringFlash(15f);

            // White-hot center flare
            CustomParticles.GenericFlare(center, AutumnWhite, 1.8f, 25);

            // 16-point radial halo starburst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color haloColor = enraged
                    ? Color.Lerp(EmberOrange, WitheredRed, i / 16f)
                    : Color.Lerp(TwilightPurple, AshenGray, i / 16f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 70f, haloColor, 0.5f, 18);
            }

            // Tutti full ensemble
            Color[] tuttiColors = enraged
                ? new[] { EmberOrange, WitheredRed, AutumnWhite }
                : new[] { TwilightPurple, AshenGray, DecayBrown };
            Phase10BossVFX.TuttiFullEnsemble(center, tuttiColors, 1.6f);

            // Autumn leaf storm signature — massive
            BossSignatureVFX.AutumnLeafStorm(center, enraged ? 2.0f : 1.5f);

            // Dense bloom cascade — 14 bloom particles
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color bc = Color.Lerp(TwilightPurple, AshenGray, i / 14f);
                if (enraged) bc = Color.Lerp(EmberOrange, AutumnWhite, i / 14f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, bc, 0.6f, 22));
            }

            // Music note crescendo — notes ascending
            for (int i = 0; i < 5; i++)
            {
                Vector2 notePos = center + new Vector2(Main.rand.NextFloat(-30f, 30f), -18f * i);
                Color noteColor = Color.Lerp(TwilightPurple, AutumnWhite, i / 5f);
                if (enraged) noteColor = Color.Lerp(EmberOrange, AutumnWhite, i / 5f);
                CustomParticles.GenericMusicNotes(notePos, noteColor, 1, 12f + i * 10f);
            }

            // Grand ascending sparkle cascade
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(55f, 55f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2f, 5f));
                Color sc = enraged ? EmberOrange : AshenGray;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel, sc, 0.35f, 28));
            }
        }

        #endregion

        #region Phase Transitions

        public static void PhaseTransitionVFX(Vector2 center, int fromPhase, int toPhase)
        {
            float intensity = toPhase * 0.4f;
            Color primary = GetPhasePrimary(toPhase);
            Color accent = GetPhaseAccent(toPhase);

            // Escalating halo rings
            for (int i = 0; i < 6 + toPhase * 2; i++)
            {
                float progress = i / (float)(6 + toPhase * 2);
                Color haloColor = Color.Lerp(primary, accent, progress);
                CustomParticles.HaloRing(center, haloColor, 0.35f + i * 0.12f, 14 + i * 2);
            }

            // Center flares
            CustomParticles.GenericFlare(center, AutumnWhite, 1.2f + intensity, 25 + toPhase * 5);
            CustomParticles.GenericFlare(center, primary, 1.0f + intensity * 0.5f, 20 + toPhase * 3);
            MagnumScreenEffects.AddScreenShake(6f + toPhase * 4f);

            // Phase-specific flash and musical accent
            switch (toPhase)
            {
                case 2:
                    TriggerHarvestFlash(6f);
                    Phase10BossVFX.CrescendoDangerRings(center, DecayBrown, 0.8f);
                    BossSignatureVFX.AutumnLeafStorm(center, 1.0f);
                    break;
                case 3:
                    TriggerWitheringFlash(10f);
                    Phase10BossVFX.TuttiFullEnsemble(center, new[] { TwilightPurple, AshenGray, DecayBrown }, 1.0f);
                    break;
            }

            // Bloom cascade
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3.5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    Color.Lerp(primary, accent, i / 8f), 0.4f, 16));
            }
        }

        public static void EnrageTransitionVFX(Vector2 center)
        {
            // Funeral Pyre onset — world goes dark, boss erupts in embers
            MagnumScreenEffects.AddScreenShake(25f);
            TriggerFuneralFlash(15f);

            // Massive ember burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Color color = Color.Lerp(EmberOrange, WitheredRed, i / 16f);
                CustomParticles.HaloRing(center + angle.ToRotationVector2() * 80f, color, 0.6f, 20);
                Vector2 vel = angle.ToRotationVector2() * 5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel, color, 0.5f, 20));
            }

            // Center flares — dying light
            CustomParticles.GenericFlare(center, EmberOrange, 2.5f, 35);
            CustomParticles.GenericFlare(center, AutumnWhite, 1.8f, 30);

            // Coda — the beginning of the end
            Phase10BossVFX.CodaFinale(center, EmberOrange, WitheredRed, 1.5f);

            // Autumn leaf storm — last great burst of leaves
            BossSignatureVFX.AutumnLeafStorm(center, 2.0f);

            // Ascending ember sparkles
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2.5f, 5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel,
                    EmberOrange, 0.35f, 26));
            }
        }

        #endregion

        #region Death Sequence

        public static void DeathEscalation(Vector2 center, int deathTimer)
        {
            float intensity = MathHelper.Clamp(deathTimer / 100f, 0f, 1f);

            // Escalating flashes — dying embers flaring
            if (deathTimer % 18 == 0 && deathTimer > 0)
                TriggerFuneralFlash(3f + intensity * 10f);

            // Spiraling ember flares
            if (deathTimer % 4 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f + deathTimer * 0.06f;
                    Vector2 offset = angle.ToRotationVector2() * (30f + intensity * 55f);
                    Color flareColor = Color.Lerp(EmberOrange, WitheredRed, (float)i / 5f);
                    CustomParticles.GenericFlare(center + offset, flareColor, 0.4f + intensity * 0.3f, 10);
                }
            }

            // Ascending dying sparkles
            if (deathTimer % 6 == 0)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(30f + intensity * 40f, 30f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), -Main.rand.NextFloat(2f, 4f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel,
                    Color.Lerp(EmberOrange, AutumnWhite, intensity), 0.25f + intensity * 0.15f, 20));
            }

            MagnumScreenEffects.AddScreenShake(intensity * 5f);
        }

        public static void DeathFinale(Vector2 center)
        {
            TriggerFinalFlash(20f);
            MagnumScreenEffects.AddScreenShake(25f);

            // Triple-layer center flares
            CustomParticles.GenericFlare(center, AutumnWhite, 2.5f, 45);
            CustomParticles.GenericFlare(center, EmberOrange, 2f, 40);
            CustomParticles.GenericFlare(center, WitheredRed, 1.5f, 35);

            // 16-ring halo cascade
            for (int i = 0; i < 16; i++)
            {
                Color ringColor = Color.Lerp(EmberOrange, AshenGray, i / 16f);
                CustomParticles.HaloRing(center, ringColor, 0.4f + i * 0.15f, 18 + i * 2);
            }

            // Massive bloom ring — 14 particles radiating
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * 5.5f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(center, vel,
                    Color.Lerp(EmberOrange, AutumnWhite, i / 14f), 0.6f, 24));
            }

            // Ascending sparkle shower — the last embers rising
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(45f, 45f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -Main.rand.NextFloat(2.5f, 5.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(sparkPos, sparkVel,
                    AutumnWhite, 0.38f, 30));
            }

            // Music note fountain — final melody ascending
            for (int i = 0; i < 6; i++)
            {
                Vector2 notePos = center + new Vector2(-40f + i * 16f, -12f * i);
                Color nc = Color.Lerp(EmberOrange, AutumnWhite, i / 6f);
                CustomParticles.GenericMusicNotes(notePos, nc, 1, 12f + i * 10f);
            }

            // Coda finale — the final chord
            Phase10BossVFX.CodaFinale(center, EmberOrange, AshenGray, 2.0f);
        }

        #endregion
    }
}