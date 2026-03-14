using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.LaCampanella.Bosses.Systems
{
    /// <summary>
    /// La Campanella, Chime of Life — full boss VFX choreography.
    /// The boss IS the bell. Liszt's impossible virtuosity made manifest.
    ///
    /// Phase 1 — First Toll: singular shockwave rings, golden harmonic particles vibrating in place,
    ///           billowing black smoke with orange fire flickering through gaps.
    /// Phase 2 — Accelerando: rapid overlapping toll rings, frantic fire trails on movement,
    ///           thickened churning smoke, burning glyphs that re-ring after delay.
    /// Phase 3 — Virtuoso Cascade: continuous wall of expanding rings, rhythmic fire bursts
    ///           synced to attacks, flame tongues in piano arpeggio patterns.
    /// Enrage — Bell Cracking: jagged fractured rings, fire bleeding through sky cracks,
    ///          smoke consuming everything, infernal orange light beams stabbing through.
    ///
    /// Palette: deep black smoke, infernal orange, molten gold highlights.
    /// </summary>
    public static class LaCampanellaAttackVFX
    {
        // Canonical palette — musical dynamics scale
        private static readonly Color SootBlack = new Color(20, 15, 20);
        private static readonly Color DeepEmber = new Color(180, 60, 0);
        private static readonly Color InfernalOrange = new Color(255, 100, 0);
        private static readonly Color FlameYellow = new Color(255, 200, 50);
        private static readonly Color BellGold = new Color(218, 165, 32);
        private static readonly Color MoltenGold = new Color(255, 180, 40);
        private static readonly Color FlameWhite = new Color(255, 230, 200);

        #region Phase 1 — The First Toll

        /// <summary>
        /// Bell Slam telegraph — gathering fire converges on impact zone.
        /// The bell draws back, smoke billows upward, golden motes gather.
        /// </summary>
        public static void BellSlamTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitY);
            TelegraphSystem.ThreatLine(position, dir, 600f, 30, InfernalOrange * 0.7f);
            TelegraphSystem.ImpactPoint(target, 80f, 30);

            // Smoke billows upward from the bell as it winds up
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -2f - Main.rand.NextFloat(1.5f));
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(40f, 20f),
                    smokeVel, SootBlack, Main.rand.Next(35, 55), 0.4f, 0.9f, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Golden harmonic motes converge on the impact point
            for (int i = 0; i < 3; i++)
            {
                Vector2 spawnPos = target + Main.rand.NextVector2CircularEdge(120f, 120f);
                Vector2 vel = (target - spawnPos).SafeNormalize(Vector2.Zero) * 1.5f;
                MagnumParticleHandler.SpawnParticle(new GlowSparkParticle(spawnPos, vel, BellGold, 0.2f, 25));
            }
        }

        /// <summary>
        /// Bell Slam impact — THE TOLL. A single visible shockwave ring expands outward
        /// like a bell being struck. Golden harmonic particles vibrate in place at the
        /// impact site. Heavy black smoke plumes erupt.
        /// </summary>
        public static void BellSlamImpact(Vector2 position)
        {
            // === THE TOLL: Expanding shockwave ring ===
            MagnumScreenEffects.AddScreenShake(15f);
            LaCampanellaSkySystem.TriggerBellTollFlash();
            BossSignatureVFX.LaCampanellaBellToll(position, 1);

            // Primary shockwave ring — the visual bell toll
            CustomParticles.HaloRing(position, MoltenGold, 1.0f, 25);
            ThemedParticles.LaCampanellaShockwave(position, 1.2f);

            // Secondary inner ring for depth
            CustomParticles.HaloRing(position, InfernalOrange, 0.6f, 18);

            // === GOLDEN HARMONIC PARTICLES — vibrate in place ===
            // These linger and oscillate, not flying away — they resonate
            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(60f, 40f);
                // Near-zero velocity — they vibrate, not fly
                Vector2 vel = Main.rand.NextVector2Circular(0.15f, 0.15f);
                Color harmonicColor = Color.Lerp(BellGold, FlameYellow, Main.rand.NextFloat() * 0.5f);
                MagnumParticleHandler.SpawnParticle(new PulsingBloomParticle(
                    position + offset, vel, harmonicColor, 0.18f, 45));
            }

            // === BLACK SMOKE ERUPTION — billowing plumes ===
            for (int i = 0; i < 8; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-3f, 3f), -2.5f - Main.rand.NextFloat(2f));
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(50f, 25f),
                    smokeVel, SootBlack, Main.rand.Next(40, 65), 0.5f, 1.2f, 0.014f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Orange fire flickering through the smoke gaps
            for (int i = 0; i < 5; i++)
            {
                Vector2 fireVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -1.5f - Main.rand.NextFloat(1f));
                Color fireCol = Color.Lerp(InfernalOrange, FlameWhite, Main.rand.NextFloat() * 0.3f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    position + Main.rand.NextVector2Circular(35f, 20f), fireVel, fireCol, 0.25f, 20));
            }
        }

        /// <summary>
        /// Toll Wave — a pure expanding sound wave ring.
        /// Phase 1: clean, singular, majestic. One ring per toll.
        /// </summary>
        public static void TollWaveTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 120f, 8, InfernalOrange * 0.5f);

            // Gathering golden glow at center — the breath before the toll
            MagnumParticleHandler.SpawnParticle(new BloomParticle(center, Vector2.Zero, BellGold, 0.4f, 15));
        }

        public static void TollWaveRelease(Vector2 center, int waveIndex)
        {
            // Single clean expanding ring — the visible shockwave
            float ringScale = 0.6f + waveIndex * 0.25f;
            Color ringColor = Color.Lerp(MoltenGold, InfernalOrange, (float)Math.Sin(waveIndex * 0.5f) * 0.5f + 0.5f);
            CustomParticles.HaloRing(center, ringColor, ringScale, 22);
            ThemedParticles.LaCampanellaShockwave(center, 0.9f + waveIndex * 0.2f);

            // Flash the sky with each toll
            LaCampanellaSkySystem.TriggerBellTollFlash();

            // Harmonic vibration particles at the ring origin
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                MagnumParticleHandler.SpawnParticle(new PulsingBloomParticle(
                    center + offset, Main.rand.NextVector2Circular(0.1f, 0.1f),
                    BellGold, 0.15f, 35));
            }

            // Smoke wafts outward from the ring's passage
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 smokePos = center + angle.ToRotationVector2() * (50f + waveIndex * 20f);
                Vector2 smokeVel = angle.ToRotationVector2() * 0.8f + new Vector2(0, -0.5f);
                var smoke = new HeavySmokeParticle(smokePos, smokeVel, SootBlack,
                    Main.rand.Next(30, 45), 0.3f, 0.7f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        /// <summary>
        /// Ember Shower — fire rain from above, each ember trailing small smoke wisps.
        /// </summary>
        public static void EmberShowerTelegraph(Vector2 targetArea)
        {
            Phase10BossVFX.StaffLineConvergence(targetArea + new Vector2(0, -250), InfernalOrange, 0.7f);
            TelegraphSystem.DangerZone(targetArea, 250f, 30, InfernalOrange * 0.3f);
        }

        public static void EmberShowerParticle(Vector2 position)
        {
            Color color = Color.Lerp(InfernalOrange, FlameWhite, Main.rand.NextFloat(0.3f));
            CustomParticles.GenericFlare(position, color, 0.3f, 8);
            ThemedParticles.LaCampanellaSparks(position, Vector2.UnitY, 2, 3f);

            // Each ember leaves a small smoke wisp
            var smoke = new HeavySmokeParticle(position, new Vector2(0, -0.5f),
                SootBlack, Main.rand.Next(15, 25), 0.15f, 0.3f, 0.008f, false);
            MagnumParticleHandler.SpawnParticle(smoke);

            // Occasional music note sparking off embers
            if (Main.rand.NextBool(5))
                CustomParticles.LaCampanellaMusicNotes(position, 1, 15f);
        }

        /// <summary>
        /// Fire Wall Sweep — horizontal infernal wave with dense smoke front.
        /// </summary>
        public static void FireWallSweepTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 40f, 45, InfernalOrange * 0.6f);
            Phase10BossVFX.AccelerandoSpiral(start, InfernalOrange, 0.6f);
        }

        public static void FireWallSweepTrail(Vector2 position, Vector2 velocity)
        {
            ThemedParticles.LaCampanellaTrail(position, velocity);

            // Leading smoke front
            var smoke = new HeavySmokeParticle(position + Main.rand.NextVector2Circular(15f, 30f),
                velocity * 0.3f + new Vector2(0, -1f), SootBlack,
                Main.rand.Next(25, 40), 0.3f, 0.6f, 0.01f, false);
            MagnumParticleHandler.SpawnParticle(smoke);

            // Fire licking through the smoke
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                position + Main.rand.NextVector2Circular(10f, 20f),
                velocity * 0.2f, InfernalOrange, 0.2f, 12));
        }

        #endregion

        #region Phase 2 — The Accelerando

        /// <summary>
        /// Rapid Toll Rings — multiple shockwave rings overlapping at different expansion stages.
        /// In Phase 2, toll rings come fast and stack visually.
        /// </summary>
        public static void AccelerandoTollRing(Vector2 center, int rapidIndex, int totalRapid)
        {
            float t = (float)rapidIndex / Math.Max(1, totalRapid);
            float ringScale = 0.4f + t * 0.6f;
            Color ringColor = Color.Lerp(MoltenGold, InfernalOrange, t);

            // Each ring is smaller than the last — they stack concentrically
            CustomParticles.HaloRing(center, ringColor, ringScale, 20);
            ThemedParticles.LaCampanellaShockwave(center, 0.7f + t * 0.5f);

            // Flash intensifies with rapid tolls
            if (rapidIndex % 2 == 0)
                LaCampanellaSkySystem.TriggerBellTollFlash();

            // Quick-burst harmonic particles — more frantic than Phase 1
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(30f + rapidIndex * 8f, 30f + rapidIndex * 8f);
                MagnumParticleHandler.SpawnParticle(new GlowSparkParticle(
                    center + offset, offset.SafeNormalize(Vector2.Zero) * 1.2f,
                    BellGold, 0.15f, 18));
            }
        }

        /// <summary>
        /// Frantic Fire Trail — fire particles stream from every movement.
        /// Smoke thickens and churns, trailing the bell like a comet.
        /// </summary>
        public static void FranticFireTrail(Vector2 position, Vector2 velocity)
        {
            // Dense fire stream — multiple particles per call
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 fireVel = -velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color fireCol = Color.Lerp(InfernalOrange, FlameYellow, Main.rand.NextFloat() * 0.4f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    position + offset, fireVel, fireCol, 0.2f + Main.rand.NextFloat(0.1f), 15));
            }

            // Thick churning smoke trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 smokeVel = -velocity.SafeNormalize(Vector2.Zero) * 0.8f
                    + Main.rand.NextVector2Circular(1f, 1f);
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(20f, 20f),
                    smokeVel, SootBlack, Main.rand.Next(35, 55), 0.4f, 1.0f, 0.013f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        /// <summary>
        /// Chime Rings — concentric rings of bell projectiles, now faster and more layered.
        /// </summary>
        public static void ChimeRingsTelegraph(Vector2 center)
        {
            for (int i = 0; i < 3; i++)
            {
                float radius = 80f + i * 60f;
                TelegraphSystem.ConvergingRing(center, radius, 6, BellGold * 0.5f);
            }
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { InfernalOrange, BellGold, FlameWhite }, 0.6f);
        }

        public static void ChimeRingsRelease(Vector2 center, int ringIndex)
        {
            CustomParticles.LaCampanellaBellChime(center, 8 + ringIndex * 3);
            CustomParticles.HaloRing(center, BellGold, 0.5f + ringIndex * 0.15f, 16);
            LaCampanellaSkySystem.TriggerInfernalFlash(0.15f + ringIndex * 0.1f);

            // Smoke erupts with each ring
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 smokeVel = angle.ToRotationVector2() * 1.5f + new Vector2(0, -1f);
                var smoke = new HeavySmokeParticle(
                    center + angle.ToRotationVector2() * (40f + ringIndex * 15f),
                    smokeVel, SootBlack, Main.rand.Next(30, 45), 0.3f, 0.7f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Radial fire sparks
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + ringIndex * 0.5f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    center + vel * 5f, vel, InfernalOrange, 0.25f, 22));
            }
        }

        /// <summary>
        /// Inferno Circle — ring of fire closing in, smoke front leads.
        /// </summary>
        public static void InfernoCircleTelegraph(Vector2 center, float radius)
        {
            TelegraphSystem.DangerZone(center, radius, 60, InfernalOrange * 0.4f);
            Phase10BossVFX.CrescendoDangerRings(center, InfernalOrange, 0.9f, 4);
        }

        public static void InfernoCircleRelease(Vector2 center, float radius)
        {
            int count = 16;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, InfernalOrange, 0.4f, 12);
                ThemedParticles.LaCampanellaSparks(pos, -angle.ToRotationVector2(), 3, 5f);

                // Smoke at each fire point
                var smoke = new HeavySmokeParticle(pos,
                    -angle.ToRotationVector2() * 0.5f + new Vector2(0, -0.8f),
                    SootBlack, Main.rand.Next(25, 40), 0.25f, 0.5f, 0.009f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            ThemedParticles.LaCampanellaShockwave(center, 1.2f);
            LaCampanellaSkySystem.TriggerCrimsonFlash(0.4f);
            MagnumScreenEffects.AddScreenShake(8f);
        }

        /// <summary>
        /// Rhythmic Toll — escalating multi-hit bell strikes.
        /// Each toll produces a shockwave ring AND leaves burning glyph residue.
        /// </summary>
        public static void RhythmicTollTelegraph(Vector2 position)
        {
            Phase10BossVFX.MetronomeTickWarning(position, BellGold, 3, 6);
        }

        public static void RhythmicTollStrike(Vector2 position, int tollNumber)
        {
            float intensity = 0.4f + tollNumber * 0.15f;

            // Shockwave ring per toll — overlapping at higher counts
            CustomParticles.HaloRing(position,
                Color.Lerp(BellGold, InfernalOrange, tollNumber / 6f), intensity, 18);
            ThemedParticles.LaCampanellaShockwave(position, 0.6f + tollNumber * 0.15f);
            BossSignatureVFX.LaCampanellaBellToll(position, tollNumber, intensity);
            MagnumScreenEffects.AddScreenShake(5f + tollNumber * 3f);

            // Sky flash escalates with each toll
            if (tollNumber >= 1)
                LaCampanellaSkySystem.TriggerBellTollFlash();

            // Heavy smoke eruption grows with each strike
            for (int i = 0; i < 2 + tollNumber; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 1f) + new Vector2(0, -2f);
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(30f, 15f),
                    smokeVel, SootBlack, Main.rand.Next(30, 50), 0.3f + tollNumber * 0.05f,
                    0.8f + tollNumber * 0.1f, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Ascending fire sparks
            for (int i = 0; i < 2 + tollNumber; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), -3f - tollNumber * 0.5f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    position + Main.rand.NextVector2Circular(15f, 10f),
                    vel, InfernalOrange, 0.2f, 22));
            }
        }

        #endregion

        #region Phase 3 — The Virtuoso Cascade

        /// <summary>
        /// Continuous Toll Wall — impossibly fast toll sequences creating a continuous
        /// wall of expanding rings. Multiple rings exist simultaneously at different
        /// stages of expansion.
        /// </summary>
        public static void VirtuosoTollWall(Vector2 center, int sequenceIndex)
        {
            // Rapid overlapping rings — each slightly offset
            float phase = sequenceIndex * 0.4f;
            for (int i = 0; i < 2; i++)
            {
                float scale = 0.3f + (sequenceIndex % 5) * 0.2f + i * 0.15f;
                Color ringColor = Color.Lerp(MoltenGold, FlameWhite, (float)Math.Sin(phase + i) * 0.3f + 0.3f);
                CustomParticles.HaloRing(center, ringColor, scale, 20);
            }
            ThemedParticles.LaCampanellaShockwave(center, 0.5f + (sequenceIndex % 4) * 0.3f);

            // Constant sky pulsing
            if (sequenceIndex % 2 == 0)
                LaCampanellaSkySystem.TriggerBellTollFlash();

            // Machine-gun harmonic particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(50f, 50f);
                MagnumParticleHandler.SpawnParticle(new PulsingBloomParticle(
                    center + offset, Main.rand.NextVector2Circular(0.2f, 0.2f),
                    BellGold, 0.12f, 20));
            }
        }

        /// <summary>
        /// Flame Tongue Arpeggio — flame tongues lash outward from the boss in the
        /// pattern of piano arpeggios. Sequential directional fire bursts that sweep
        /// around the boss like fingers running up the keyboard.
        /// </summary>
        public static void FlameTongueArpeggio(Vector2 center, int noteIndex, int totalNotes)
        {
            // Each "note" is a flame tongue at a specific angle — sweeping like an arpeggio
            float startAngle = -MathHelper.PiOver2; // Start upward
            float sweep = MathHelper.Pi * 1.5f; // Sweep 270 degrees
            float angle = startAngle + sweep * noteIndex / Math.Max(1, totalNotes);

            Vector2 tongueDir = angle.ToRotationVector2();
            float tongueLength = 150f + noteIndex * 20f;

            // The flame tongue itself — a burst of fire along the direction
            for (int i = 0; i < 5; i++)
            {
                float dist = tongueLength * (i + 1) / 5f;
                Vector2 pos = center + tongueDir * dist;
                Vector2 vel = tongueDir * (3f + i * 0.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color fireCol = Color.Lerp(FlameWhite, InfernalOrange, (float)i / 5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(pos, vel, fireCol, 0.3f - i * 0.03f, 18));
            }

            // Smoke follows each tongue
            for (int i = 0; i < 2; i++)
            {
                Vector2 smokePos = center + tongueDir * (tongueLength * 0.5f) + Main.rand.NextVector2Circular(15f, 15f);
                var smoke = new HeavySmokeParticle(smokePos, tongueDir * 0.5f, SootBlack,
                    Main.rand.Next(20, 35), 0.2f, 0.5f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Music note at tongue tip
            if (noteIndex % 2 == 0)
                CustomParticles.LaCampanellaMusicNotes(center + tongueDir * tongueLength, 1, 10f);
        }

        /// <summary>
        /// Infernal Judgment — massive radial fire pattern with overlapping rings
        /// and dense smoke walls. The ultimate Phase 3 attack.
        /// </summary>
        public static void InfernalJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, InfernalOrange * 0.7f);
            Phase10BossVFX.ChordBuildupSpiral(center, new[] { InfernalOrange, BellGold, FlameWhite }, 0.9f);
            Phase10BossVFX.FortissimoFlashWarning(center, InfernalOrange, 1.3f);
        }

        public static void InfernalJudgmentRelease(Vector2 center, int wave, int totalWaves)
        {
            float progress = (float)wave / Math.Max(1, totalWaves);
            MagnumScreenEffects.AddScreenShake(18f);
            LaCampanellaSkySystem.TriggerWhiteFlash(0.5f + progress * 0.3f);
            BossSignatureVFX.LaCampanellaInfernalJudgment(center, wave, totalWaves, 1.2f);

            // Multiple overlapping shockwave rings
            for (int r = 0; r < 3; r++)
            {
                float delay = r * 0.3f;
                Color ringCol = Color.Lerp(MoltenGold, FlameWhite, delay);
                CustomParticles.HaloRing(center, ringCol, 0.5f + r * 0.25f + progress * 0.3f, 22);
            }

            // Massive smoke walls radiating outward
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + wave * 0.3f;
                Vector2 smokeVel = angle.ToRotationVector2() * 3f + new Vector2(0, -1f);
                var smoke = new HeavySmokeParticle(
                    center + angle.ToRotationVector2() * 40f,
                    smokeVel, SootBlack, Main.rand.Next(40, 60), 0.5f, 1.3f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Fire cascade
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -1.5f);
                Color col = Color.Lerp(InfernalOrange, FlameWhite, Main.rand.NextFloat() * 0.5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    center + Main.rand.NextVector2Circular(40f, 40f), vel, col, 0.4f, 30));
            }
        }

        /// <summary>Triple Slam — three consecutive bell slams with escalating shockwave rings.</summary>
        public static void TripleSlamImpact(Vector2 position, int slamIndex)
        {
            float intensity = 0.6f + slamIndex * 0.3f;
            MagnumScreenEffects.AddScreenShake(10f + slamIndex * 5f);

            // Each slam produces an expanding ring — they overlap visually
            for (int r = 0; r <= slamIndex; r++)
            {
                CustomParticles.HaloRing(position,
                    Color.Lerp(MoltenGold, InfernalOrange, (float)r / Math.Max(1, slamIndex)),
                    intensity - r * 0.15f, 20);
            }
            ThemedParticles.LaCampanellaShockwave(position, intensity);
            ThemedParticles.LaCampanellaImpact(position, intensity);
            LaCampanellaSkySystem.TriggerBellTollFlash();

            // Dense smoke eruption — more with each slam
            for (int i = 0; i < 5 + slamIndex * 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(3f, 2f) + new Vector2(0, -2.5f);
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(40f, 20f),
                    smokeVel, SootBlack, Main.rand.Next(35, 55), 0.4f + slamIndex * 0.08f,
                    1.0f + slamIndex * 0.2f, 0.013f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Fire bursts
            for (int i = 0; i < 4 + slamIndex * 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 3f) + new Vector2(0, -1.5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    position + Main.rand.NextVector2Circular(25f, 15f),
                    vel, InfernalOrange, 0.25f + slamIndex * 0.05f, 20));
            }

            if (slamIndex >= 2)
                LaCampanellaSkySystem.TriggerCrimsonFlash(0.5f);
        }

        /// <summary>Bell Laser Grid — crossing laser beams with fire and smoke.</summary>
        public static void BellLaserGridTelegraph(Vector2 start, Vector2 end)
        {
            TelegraphSystem.LaserPath(start, end, 30f, 40, BellGold * 0.5f);
            BossVFXOptimizer.WarningLine(start, (end - start).SafeNormalize(Vector2.UnitX),
                (end - start).Length(), 12, WarningType.Danger);
        }

        public static void BellLaserGridBeam(Vector2 start, Vector2 end)
        {
            Phase10BossVFX.StaffLineLaser(start, end, InfernalOrange, 35f);
            ThemedParticles.LaCampanellaSparkles(start, 4, 20f);

            // Smoke curls along the beam path
            Vector2 mid = (start + end) * 0.5f;
            var smoke = new HeavySmokeParticle(mid + Main.rand.NextVector2Circular(30f, 30f),
                new Vector2(0, -0.8f), SootBlack, Main.rand.Next(20, 35), 0.2f, 0.4f, 0.008f, false);
            MagnumParticleHandler.SpawnParticle(smoke);
        }

        /// <summary>Infernal Torrent — spiral fire streams.</summary>
        public static void InfernalTorrentTelegraph(Vector2 position)
        {
            Phase10BossVFX.AccelerandoSpiral(position, InfernalOrange, 0.8f, 16);
            TelegraphSystem.ConvergingRing(position, 150f, 8, InfernalOrange * 0.6f);
        }

        public static void InfernalTorrentRelease(Vector2 position, int burstIndex)
        {
            float angle = burstIndex * 0.25f;
            Color color = Color.Lerp(InfernalOrange, FlameWhite, (float)Math.Sin(angle) * 0.5f + 0.5f);
            CustomParticles.GenericFlare(position, color, 0.5f, 14);
            ThemedParticles.LaCampanellaSparks(position, angle.ToRotationVector2(), 4, 6f);

            // Trailing smoke
            var smoke = new HeavySmokeParticle(position, -angle.ToRotationVector2() * 0.5f,
                SootBlack, Main.rand.Next(20, 35), 0.2f, 0.4f, 0.008f, false);
            MagnumParticleHandler.SpawnParticle(smoke);
        }

        /// <summary>Inferno Cage — enclosing fire pillars with choking smoke.</summary>
        public static void InfernoCageTelegraph(Vector2 center, float radius)
        {
            int pillarCount = 8;
            for (int i = 0; i < pillarCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pillarCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                TelegraphSystem.ThreatLine(pos, Vector2.UnitY, 300f, 30, InfernalOrange * 0.5f);
            }
            Phase10BossVFX.CrescendoDangerRings(center, SootBlack, 0.6f);
        }

        public static void InfernoCageRelease(Vector2 center, float radius)
        {
            int pillarCount = 8;
            for (int i = 0; i < pillarCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pillarCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(pos, InfernalOrange, 0.6f, 15);
                ThemedParticles.LaCampanellaBloomBurst(pos, 0.8f);

                // Smoke pillar at each cage bar
                for (int s = 0; s < 3; s++)
                {
                    Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f - s);
                    var smoke = new HeavySmokeParticle(
                        pos + new Vector2(0, -s * 30f), smokeVel, SootBlack,
                        Main.rand.Next(30, 50), 0.35f, 0.8f, 0.012f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            MagnumScreenEffects.AddScreenShake(10f);
            LaCampanellaSkySystem.TriggerInfernalFlash(0.4f);
        }

        /// <summary>Resonant Shock — expanding sonic force waves.</summary>
        public static void ResonantShockTelegraph(Vector2 center)
        {
            Phase10BossVFX.FortissimoFlashWarning(center, BellGold, 0.8f);
        }

        public static void ResonantShockRelease(Vector2 center, float radius)
        {
            // Double ring — sonic and fire
            CustomParticles.HaloRing(center, BellGold, radius / 150f, 22);
            CustomParticles.HaloRing(center, InfernalOrange, radius / 200f, 24);
            ThemedParticles.LaCampanellaShockwave(center, radius / 100f);
            LaCampanellaSkySystem.TriggerBellTollFlash();

            // Smoke ring accompanying the shockwave
            int smokeCount = 10;
            for (int i = 0; i < smokeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / smokeCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius * 0.5f;
                Vector2 vel = angle.ToRotationVector2() * 1f + new Vector2(0, -0.5f);
                var smoke = new HeavySmokeParticle(pos, vel, SootBlack,
                    Main.rand.Next(20, 35), 0.25f, 0.5f, 0.009f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Ascending golden sparks
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 pos = center + angle.ToRotationVector2() * radius * 0.5f;
                Vector2 vel = new Vector2(0, -2.5f) + Main.rand.NextVector2Circular(1f, 0.5f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(pos, vel, BellGold, 0.2f, 25));
            }
        }

        #endregion

        #region Enrage — The Bell Cracking

        /// <summary>
        /// Fractured Toll Ring — the perfect rings become jagged and cracked.
        /// Instead of clean circles, the rings break apart with gaps, producing
        /// arc segments with jagged edges and fire bleeding through.
        /// </summary>
        public static void FracturedTollRing(Vector2 center, int crackIndex)
        {
            // Multiple fragmented ring arcs instead of clean circles
            int fragments = 5 + crackIndex;
            for (int i = 0; i < fragments; i++)
            {
                float angle = MathHelper.TwoPi * i / fragments + Main.rand.NextFloat(-0.2f, 0.2f);
                float dist = 30f + crackIndex * 15f + Main.rand.NextFloat(-10f, 10f);
                Vector2 pos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat(1.5f));

                // Jagged fire fragments where ring segments should be
                Color fragColor = Color.Lerp(InfernalOrange, FlameWhite, Main.rand.NextFloat() * 0.5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(pos, vel, fragColor, 0.25f, 16));

                // Fire bleeds through the cracks between fragments
                if (i % 2 == 0)
                {
                    Vector2 crackVel = angle.ToRotationVector2() * 4f + Main.rand.NextVector2Circular(1f, 1f);
                    MagnumParticleHandler.SpawnParticle(new GlowSparkParticle(
                        pos, crackVel, FlameYellow, 0.18f, 14));
                }
            }

            // The broken ring still produces a shockwave — but distorted
            ThemedParticles.LaCampanellaShockwave(center, 0.6f + crackIndex * 0.15f);
            LaCampanellaSkySystem.TriggerCrimsonFlash(0.3f);
            MagnumScreenEffects.AddScreenShake(8f + crackIndex * 2f);

            // Violent smoke eruption — smoke consuming everything
            for (int i = 0; i < 6 + crackIndex * 2; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(4f, 3f) + new Vector2(0, -1.5f);
                var smoke = new HeavySmokeParticle(
                    center + Main.rand.NextVector2Circular(50f, 30f),
                    smokeVel, SootBlack, Main.rand.Next(40, 65), 0.5f, 1.4f, 0.016f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        /// <summary>
        /// Infernal Light Beam — violent beams of infernal orange light stabbing through
        /// the black smoke. Used during enrage to create dramatic light/dark contrast.
        /// </summary>
        public static void InfernalLightBeam(Vector2 origin, float angle)
        {
            Vector2 dir = angle.ToRotationVector2();
            float beamLength = 400f;

            // Line of intense fire particles along the beam path
            for (int i = 0; i < 8; i++)
            {
                float dist = beamLength * (i + 1) / 8f;
                Vector2 pos = origin + dir * dist + Main.rand.NextVector2Circular(5f, 5f);
                Color beamCol = Color.Lerp(FlameWhite, InfernalOrange, (float)i / 8f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    pos, dir * 0.5f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    beamCol, 0.35f - i * 0.03f, 12));
            }

            // Bright core at beam origin
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                origin, Vector2.Zero, FlameWhite, 0.5f, 10));

            // Smoke pushes away from the beam — the light cuts through darkness
            for (int i = 0; i < 3; i++)
            {
                Vector2 perpDir = new Vector2(-dir.Y, dir.X);
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 smokePos = origin + dir * Main.rand.NextFloat(beamLength) + perpDir * side * 20f;
                Vector2 smokeVel = perpDir * side * 1.5f;
                var smoke = new HeavySmokeParticle(smokePos, smokeVel, SootBlack,
                    Main.rand.Next(25, 40), 0.3f, 0.6f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
        }

        /// <summary>
        /// Enrage Ambient Smoke — the all-consuming darkness.
        /// Continuously spawns heavy black smoke filling the arena.
        /// </summary>
        public static void EnrageSmokeConsumption(Vector2 center, float arenaRadius)
        {
            // Dense smoke from all directions — the bell cracks, darkness pours in
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(arenaRadius);
                Vector2 pos = center + angle.ToRotationVector2() * dist;
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f);
                var smoke = new HeavySmokeParticle(pos, vel, SootBlack,
                    Main.rand.Next(45, 75), 0.5f, 1.5f, 0.018f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Occasional violent infernal light stabbing through
            if (Main.rand.NextBool(8))
            {
                float beamAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                InfernalLightBeam(center, beamAngle);
            }
        }

        /// <summary>
        /// Grand Finale — the bell's final desperate toll. Full infernal eruption.
        /// Fractured rings, light beams, flame tongues, all at once.
        /// </summary>
        public static void GrandFinaleTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 16, InfernalOrange);
            Phase10BossVFX.StaffLineConvergence(center, BellGold, 1.0f);
            Phase10BossVFX.FortissimoFlashWarning(center, FlameWhite, 1.5f);
        }

        public static void GrandFinaleRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(25f);
            LaCampanellaSkySystem.TriggerWhiteFlash(0.9f);

            // Massive bloom supernova core
            CustomParticles.GenericFlare(center, FlameWhite, 2.0f, 30);

            // Ring of fractured toll blasts
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                FracturedTollRing(center + angle.ToRotationVector2() * 60f, 3);
            }

            // Radial light beams stabbing outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                InfernalLightBeam(center, angle);
            }

            // Flame tongue arpeggio sweep
            for (int i = 0; i < 12; i++)
            {
                FlameTongueArpeggio(center, i, 12);
            }

            // Total smoke eruption
            for (int i = 0; i < 20; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(5f, 4f) + new Vector2(0, -2f);
                var smoke = new HeavySmokeParticle(
                    center + Main.rand.NextVector2Circular(60f, 40f),
                    smokeVel, SootBlack, Main.rand.Next(50, 80), 0.6f, 1.8f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            // Golden music note cascade
            CustomParticles.LaCampanellaMusicNotes(center, 8, 80f);
            CustomParticles.LaCampanellaBellChime(center, 16);

            Phase10BossVFX.CodaFinale(center, InfernalOrange, BellGold, 2.0f);
            Phase10BossVFX.TuttiFullEnsemble(center, new[] { InfernalOrange, BellGold, FlameWhite }, 1.8f);
            BossSignatureVFX.LaCampanellaInfernalJudgment(center, 5, 5, 2.0f);
        }

        #endregion
    }
}
