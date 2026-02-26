using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts;
using static MagnumOpus.Common.Systems.VFX.GodRaySystem;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom VFX System — "Spring's Unfurling Blade"
    /// 
    /// Visual concept: A blade that blooms like a cherry tree through its combo.
    /// Phase 0 = single bud opening; Phase 1 = petals scattering in wind;
    /// Phase 2 = full bloom with golden pollen; Phase 3 = storm of petals —
    /// a sakura tornado of devastating beauty.
    /// 
    /// Unique visual language vs Celestial Valor:
    ///   - CV = fire resonance, ember trails, heroic lightning
    ///   - SB = petal flutter physics, pollen drift, spring-wind streaks,
    ///          bloom rings unfurling, and sakura storm vortex
    /// 
    /// Uses custom dusts: SakuraPetalDust, SakuraEmberDust, PollenMoteDust,
    /// SpringSparkDust, PetalWindLine, BlossomRingDust, BlossomGlowOrb
    /// </summary>
    public static class SakurasBlossomVFX
    {
        // ╔══════════════════════════════════════════════════════════╗
        //  SAKURA ACCENT PALETTE (weapon-specific identity colors)
        // ╚══════════════════════════════════════════════════════════╝

        private static readonly Color BudCrimson = new Color(120, 25, 45);
        private static readonly Color BloomPink = new Color(255, 130, 165);
        private static readonly Color PollenGold = EroicaPalette.PollenGold;
        private static readonly Color PetalWhite = new Color(255, 240, 235);
        private static readonly Color SpringGreen = new Color(140, 200, 120);
        private static readonly Color BlossomCore = new Color(220, 80, 110);
        private static readonly Color PetalDrifter = new Color(255, 180, 200);
        private static readonly Color SunlitPetal = new Color(255, 220, 180);

        // ╔══════════════════════════════════════════════════════════╗
        //  PETAL TRAIL RING BUFFER — Afterimage petal scatter system
        // ╚══════════════════════════════════════════════════════════╝

        private const int PetalTrailLength = 10;
        private static readonly Vector2[] _petalTrailPos = new Vector2[PetalTrailLength];
        private static readonly float[] _petalTrailRot = new float[PetalTrailLength];
        private static int _petalTrailIndex;
        private static int _petalTrailTimer;

        /// <summary>Record blade tip for petal afterimage system.</summary>
        public static void RecordPetalTrail(Vector2 tipPos, float rotation)
        {
            _petalTrailTimer++;
            if (_petalTrailTimer % 2 == 0)
            {
                _petalTrailPos[_petalTrailIndex % PetalTrailLength] = tipPos;
                _petalTrailRot[_petalTrailIndex % PetalTrailLength] = rotation;
                _petalTrailIndex++;
            }
        }

        /// <summary>Reset trail buffer on new swing.</summary>
        public static void ResetPetalTrail()
        {
            _petalTrailIndex = 0;
            _petalTrailTimer = 0;
            Array.Fill(_petalTrailPos, Vector2.Zero);
            Array.Fill(_petalTrailRot, 0f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  1. PER-PHASE SWING VFX — Blooming escalation
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Phase 0 — Petal Slash: A single bud opens. Quick scatter with
        /// SakuraPetalDust, bloom flare, and delicate directional sparks.
        /// </summary>
        public static void PetalSlashVFX(Vector2 tipPos, Vector2 swordDir)
        {
            EroicaVFXLibrary.BloomFlare(tipPos, BloomPink, 0.55f, 14);
            EroicaVFXLibrary.SpawnGradientHaloRings(tipPos, 2, 0.3f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 3, 25f);

            // Custom petal dust — 4 fluttering sakura petals
            SpawnSakuraPetals(tipPos, 4, 4f);

            // Directional spring sparks
            for (int i = 0; i < 5; i++)
            {
                float angle = swordDir.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                var spark = new GlowSparkParticle(tipPos + Main.rand.NextVector2Circular(5f, 5f),
                    vel, BloomPink, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(12, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 1, 10f);
            Lighting.AddLight(tipPos, BloomPink.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Phase 1 — Crimson Scatter: Petals scatter in the wind. Dual-color
        /// burst (crimson + pink), petal wind streaks, and ember fragments.
        /// </summary>
        public static void CrimsonScatterVFX(Vector2 tipPos, Vector2 swordDir)
        {
            EroicaVFXLibrary.HeroicImpact(tipPos, 0.8f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 5, 30f);

            // Custom dust: 6 petals + 4 ember fragments
            SpawnSakuraPetals(tipPos, 6, 5f);
            SpawnSakuraEmbers(tipPos, 4, 5f);

            // Petal wind streaks — directional speed lines
            SpawnPetalWindLines(tipPos, swordDir, 4, 8f);

            // Dual-color spark ring
            for (int i = 0; i < 10; i++)
            {
                float angle = swordDir.ToRotation() + Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color col = (i % 2 == 0) ? BudCrimson : BloomPink;
                var spark = new GlowSparkParticle(tipPos, vel, col,
                    Main.rand.NextFloat(0.4f, 0.65f), Main.rand.Next(14, 22));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 2, 20f);

            var ring = new BloomRingParticle(tipPos, Vector2.Zero,
                BlossomCore * 0.7f, 0.45f, 20, 0.025f);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(tipPos, BlossomCore.ToVector3() * 0.85f);
        }

        /// <summary>
        /// Phase 2 — Blossom Bloom: Full bloom. Golden pollen explosion
        /// with rising motes, bloom rings unfurling, and blossom glow orbs.
        /// </summary>
        public static void BlossomBloomVFX(Vector2 tipPos)
        {
            EroicaVFXLibrary.BloomFlare(tipPos, PollenGold, 0.75f, 18);
            EroicaVFXLibrary.SpawnGradientHaloRings(tipPos, 4, 0.35f);
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 8, 40f);

            // Custom dust: petals + pollen + glow orbs
            SpawnSakuraPetals(tipPos, 8, 6f);
            SpawnPollenMotes(tipPos, 8, 3f);
            SpawnBlossomGlowOrbs(tipPos, 3, 0.5f);

            // 2-layer bloom ring cascade — golden pollen halo + inner blossom
            var pollenRing = new BloomRingParticle(tipPos, Vector2.Zero,
                PollenGold * 0.7f, 0.6f, 25, 0.06f);
            MagnumParticleHandler.SpawnParticle(pollenRing);

            var innerRing = new BloomRingParticle(tipPos, Vector2.Zero,
                BlossomCore * 0.6f, 0.35f, 20, 0.04f);
            MagnumParticleHandler.SpawnParticle(innerRing);

            // Music: spring melody
            EroicaVFXLibrary.SpawnMusicNotes(tipPos, 3, 25f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 2, 20f);

            // Radial spark burst — 12 golden pollen sparks
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var spark = new GlowSparkParticle(tipPos, vel, PollenGold,
                    Main.rand.NextFloat(0.35f, 0.6f), Main.rand.Next(15, 24));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(tipPos, PollenGold.ToVector3() * 1.1f);
        }

        /// <summary>
        /// Phase 3 — Storm of Petals: FINALE. The sakura vortex — a devastating
        /// petal tornado with screen shake, god rays, screen distortion, dense
        /// petal/pollen/ember eruption, blossom ring cascade, and music burst.
        /// </summary>
        public static void StormOfPetalsVFX(Vector2 tipPos)
        {
            // === SCREEN EFFECTS ===
            MagnumScreenEffects.AddScreenShake(12f);
            ScreenDistortionManager.TriggerRipple(tipPos, BlossomCore, 0.9f, 28);

            // === FINISHER SLAM + GOD RAYS ===
            EroicaVFXLibrary.FinisherSlam(tipPos, 1.5f);
            GodRaySystem.CreateBurst(tipPos, BlossomCore, 8, 120f, 45, GodRayStyle.Explosion, PollenGold);

            // === PETAL STORM — massive custom dust eruption ===
            SpawnSakuraPetals(tipPos, 16, 10f);
            SpawnSakuraEmbers(tipPos, 10, 8f);
            SpawnPollenMotes(tipPos, 12, 5f);
            SpawnPetalWindLines(tipPos, Vector2.UnitX, 8, 12f); // Multi-direction wind lines
            SpawnBlossomGlowOrbs(tipPos, 5, 0.7f);
            SpawnBlossomRings(tipPos, 4, 0.7f);

            // === MULTI-RING BLOOM CASCADE — 5 expanding halos ===
            for (int ring = 0; ring < 5; ring++)
            {
                float ringScale = 0.3f + ring * 0.18f;
                Color ringColor = Color.Lerp(BlossomCore, PollenGold, ring / 4f);
                var halo = new BloomRingParticle(tipPos, Vector2.Zero,
                    ringColor * (0.85f - ring * 0.1f), ringScale, 28 + ring * 3, 0.02f + ring * 0.004f);
                MagnumParticleHandler.SpawnParticle(halo);
            }

            // === SPARK SUPERNOVA — 20 radial sparks ===
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color sparkColor = Color.Lerp(BloomPink, PollenGold, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(tipPos, vel, sparkColor,
                    Main.rand.NextFloat(0.5f, 1.0f), Main.rand.Next(18, 30));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === VANILLA PETALS + DUST ===
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 14, 60f);
            EroicaVFXLibrary.SpawnRadialDustBurst(tipPos, 25, 8f, DustID.PinkTorch);

            // === MUSIC CASCADE — the song of spring's climax ===
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 8, 45f);
            EroicaVFXLibrary.SpawnMusicNotes(tipPos, 5, 35f, 0.9f, 1.2f, 40);

            // === SOUND — blooming impact ===
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.15f, Volume = 0.45f }, tipPos);

            Lighting.AddLight(tipPos, PetalWhite.ToVector3() * 1.7f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  PHASE TRANSITION BURST — bloom unfurl between phases
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Petal burst when transitioning between combo phases.</summary>
        public static void PhaseTransitionBurst(Vector2 center, int newComboStep)
        {
            Color pulseColor = Color.Lerp(BloomPink, PollenGold, newComboStep / 3f);
            var pulse = new BloomRingParticle(center, Vector2.Zero,
                pulseColor * 0.65f, 0.25f + newComboStep * 0.1f, 16, 0.02f);
            MagnumParticleHandler.SpawnParticle(pulse);

            // Petal scatter — escalating count
            SpawnSakuraPetals(center, 2 + newComboStep * 2, 3f + newComboStep);

            // Pollen motes on later phases
            if (newComboStep >= 2)
            {
                SpawnPollenMotes(center, 3 + newComboStep, 2f);
                SpawnBlossomGlowOrbs(center, 1, 0.3f);
            }

            // Phase 3: Extra blossom ring
            if (newComboStep >= 3)
            {
                SpawnBlossomRings(center, 2, 0.5f);
                EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 3, 20f);
            }

            Lighting.AddLight(center, pulseColor.ToVector3() * (0.5f + newComboStep * 0.15f));
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  2. SWING HIT IMPACT — Sakura burst on contact
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// On-hit VFX — combo-scaled sakura impact with escalating petal bursts,
        /// custom dust types, and Phase 3 screen trauma.
        /// </summary>
        public static void SwingHitImpact(Vector2 center, int comboStep)
        {
            float impactScale = 0.6f + comboStep * 0.2f;
            EroicaVFXLibrary.MeleeImpact(center, comboStep);

            // Custom petal + pollen dust burst
            SpawnSakuraPetals(center, 3 + comboStep * 2, 4f + comboStep);
            if (comboStep >= 1) SpawnPollenMotes(center, 2 + comboStep, 3f);
            if (comboStep >= 2) SpawnSakuraEmbers(center, comboStep * 2, 4f);

            // Directional spark fan
            int sparkCount = 5 + comboStep * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f + comboStep * 1.5f, 5f + comboStep * 1.5f);
                Color sparkColor = Color.Lerp(BloomPink, PollenGold, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(center + Main.rand.NextVector2Circular(6f, 6f),
                    vel, sparkColor, Main.rand.NextFloat(0.3f, 0.55f + comboStep * 0.08f),
                    Main.rand.Next(12, 18 + comboStep * 2));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Sakura music notes
            EroicaVFXLibrary.SpawnSakuraPetals(center, 2 + comboStep, 30f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 2 + comboStep, 25f);

            // Phase 2+: Expanding halo ring
            if (comboStep >= 2)
            {
                var ring = new BloomRingParticle(center, Vector2.Zero,
                    BlossomCore * 0.6f, 0.3f + comboStep * 0.08f, 18, 0.02f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Phase 3: Screen trauma
            if (comboStep >= 3)
            {
                MagnumScreenEffects.AddScreenShake(5f);
                ScreenDistortionManager.TriggerRipple(center, BlossomCore, 0.4f, 15);
                SpawnBlossomGlowOrbs(center, 2, 0.35f);
            }

            Lighting.AddLight(center, BloomPink.ToVector3() * (0.8f + comboStep * 0.15f));
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  3. PER-FRAME SWING TRAIL — Spring wind blade choreography
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Per-frame swing trail VFX — a multi-layered sakura blade.
        /// 
        /// Layers (inside → outside):
        ///   1. Petal flutter resonance along blade (standing-wave petal nodes)
        ///   2. Custom sakura petal dust at tip + along blade
        ///   3. Golden pollen motes drifting upward
        ///   4. Sakura shimmer hue-oscillation (Phase 1+)
        ///   5. Spring-green leaf accents (rare)
        ///   6. Petal wind streaks (Phase 2+)
        ///   7. Music notes (periodic)
        ///   8. Bloom glow at blade tip
        ///   9. Petal afterimage trail (Phase 2+)
        /// </summary>
        public static void DrawSwingTrailVFX(Vector2 tipPos, Vector2 ownerCenter,
            Vector2 swordDir, float progression, int comboStep)
        {
            if (progression <= 0.06f || progression >= 0.94f)
                return;

            float swingIntensity = MathHelper.Clamp((progression - 0.06f) / 0.12f, 0f, 1f)
                                 * MathHelper.Clamp((0.94f - progression) / 0.12f, 0f, 1f);

            // Record for petal afterimage
            RecordPetalTrail(tipPos, swordDir.ToRotation());

            // ── 1. PETAL FLUTTER RESONANCE — Standing wave petal nodes along blade ──
            float bladeLength = Vector2.Distance(ownerCenter, tipPos);
            int resonanceNodes = 3 + comboStep * 2;
            float waveFreq = 2.5f + comboStep * 0.8f;
            for (int n = 0; n < resonanceNodes; n++)
            {
                float t = (n + 1f) / (resonanceNodes + 1f);
                float waveAmp = (float)Math.Sin(t * waveFreq * MathHelper.Pi +
                    Main.GameUpdateCount * 0.12f) * (3.5f + comboStep * 1.5f);

                Vector2 bladePoint = Vector2.Lerp(ownerCenter, tipPos, t);
                Vector2 perp = new Vector2(-swordDir.Y, swordDir.X);
                bladePoint += perp * waveAmp;

                // Spawn petal resonance dust at antinode peaks
                if (Math.Abs(waveAmp) > 2f && Main.rand.NextBool(2))
                {
                    Color nodeColor = Color.Lerp(BloomPink, PollenGold, t);
                    Dust d = Dust.NewDustPerfect(bladePoint, DustID.PinkFairy,
                        perp * waveAmp * 0.08f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                        0, nodeColor, 0.9f + comboStep * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }
            }

            // ── 2. CUSTOM SAKURA PETAL DUST at tip ──
            if (Main.rand.NextBool(Math.Max(1, 3 - comboStep)))
            {
                SpawnSakuraPetals(tipPos, 1, 2f + comboStep * 0.5f);
            }

            // Dense vanilla swing dust
            EroicaVFXLibrary.SpawnSwingDust(tipPos, -swordDir, DustID.PinkTorch);

            // ── 3. GOLDEN POLLEN MOTES drifting upward ──
            if (Main.rand.NextBool(3))
            {
                Vector2 pollenPos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Dust pollen = Dust.NewDustPerfect(pollenPos, DustID.GoldFlame,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2.5f)),
                    0, PollenGold, 1.1f + comboStep * 0.1f);
                pollen.noGravity = true;
            }

            if (comboStep >= 2 && Main.rand.NextBool(3))
            {
                SpawnPollenMotes(tipPos, 1, 1.5f);
            }

            // ── 4. SAKURA SHIMMER — hue oscillation (Phase 1+) ──
            if (comboStep >= 1 && Main.rand.NextBool(3))
            {
                Color shimmer = EroicaPalette.GetSakuraShimmer(Main.GameUpdateCount);
                Dust s = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.PinkFairy, Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, shimmer, 1.2f + comboStep * 0.1f);
                s.noGravity = true;
            }

            // ── 5. SPRING-GREEN LEAF ACCENTS (rare) ──
            if (Main.rand.NextBool(7))
            {
                Dust leaf = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GrassBlades, -swordDir * Main.rand.NextFloat(0.5f, 1.5f),
                    0, SpringGreen, 1.0f);
                leaf.noGravity = true;
            }

            // ── 6. PETAL WIND STREAKS (Phase 2+) — speed lines ──
            if (comboStep >= 2 && Main.rand.NextBool(4))
            {
                SpawnPetalWindLines(tipPos, swordDir, 1, 5f);
            }

            // ── 7. MUSIC NOTES — spring melody ──
            int noteChance = 6 - comboStep;
            if (Main.rand.NextBool(Math.Max(2, noteChance)))
            {
                EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 1, 8f);
            }

            // ── 8. CONTRAST SPARKLE + BLADE TIP BLOOM ──
            EroicaVFXLibrary.SpawnContrastSparkle(tipPos, -swordDir);

            if (swingIntensity > 0f)
            {
                float bloomScale = (0.35f + comboStep * 0.1f) * swingIntensity;
                EroicaVFXLibrary.DrawComboBloom(tipPos, comboStep, bloomScale, swingIntensity);
            }

            // ── 9. PETAL AFTERIMAGE TRAIL (Phase 2+) ──
            if (comboStep >= 2)
            {
                DrawPetalAfterimages(tipPos, comboStep);
            }

            // Dynamic lighting
            float lightIntensity = 0.45f + comboStep * 0.12f;
            Color lightColor = Color.Lerp(BloomPink, PollenGold, progression);
            Lighting.AddLight(tipPos, lightColor.ToVector3() * lightIntensity * swingIntensity);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  PETAL AFTERIMAGE DRAWING — Ghosted sakura echoes
        // ╚══════════════════════════════════════════════════════════╝

        private static void DrawPetalAfterimages(Vector2 currentTip, int comboStep)
        {
            int count = Math.Min(_petalTrailIndex, PetalTrailLength);
            if (count < 2) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 origin = new Vector2(glowTex.Width / 2f, glowTex.Height / 2f);

            for (int i = 0; i < count; i++)
            {
                int idx = (_petalTrailIndex - 1 - i + PetalTrailLength) % PetalTrailLength;
                Vector2 pos = _petalTrailPos[idx];
                if (pos == Vector2.Zero) continue;

                float fadeProgress = (float)i / count;
                Color ghostColor = Color.Lerp(BloomPink, BudCrimson, fadeProgress);
                ghostColor = ghostColor with { A = 0 } * (0.3f - fadeProgress * 0.25f);
                float ghostScale = (0.4f + comboStep * 0.08f) * (1f - fadeProgress * 0.5f);

                sb.Draw(glowTex, pos - Main.screenPosition, null, ghostColor,
                    _petalTrailRot[idx], origin, ghostScale, SpriteEffects.None, 0f);
            }
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  4. SPECTRAL COPY VFX — Phantom blade lifecycle
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Bloom flash + petal scatter when spectral copies materialize.</summary>
        public static void SpectralCopySpawnVFX(Vector2 pos, int copyCount)
        {
            float intensity = 0.5f + copyCount * 0.12f;
            EroicaVFXLibrary.BloomFlare(pos, BloomPink, intensity, 16);
            EroicaVFXLibrary.SpawnGradientHaloRings(pos, 2 + copyCount / 2, 0.3f);
            EroicaVFXLibrary.SpawnSakuraPetals(pos, 2 + copyCount, 25f + copyCount * 5f);

            // Custom dust: petals + glow orbs
            SpawnSakuraPetals(pos, 2 + copyCount, 4f);
            SpawnBlossomGlowOrbs(pos, 1 + copyCount / 3, 0.3f);

            EroicaVFXLibrary.SpawnSakuraMusicNotes(pos, 1 + copyCount / 2, 15f);
            Lighting.AddLight(pos, BloomPink.ToVector3() * (0.6f + copyCount * 0.1f));
        }

        /// <summary>Per-frame trail for spectral copies: sakura flame, petal sparkles, dynamic light.</summary>
        public static void SpectralCopyTrailVFX(Projectile proj)
        {
            // Sakura flame dust trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -proj.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(BloomPink, PollenGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(proj.Center, DustID.PinkTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Custom petal dust (every 3 frames)
            if (Main.rand.NextBool(3))
                SpawnSakuraPetals(proj.Center, 1, 2f);

            // Petal sparkles
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = proj.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust s = Dust.NewDustPerfect(sparklePos, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(1f, 1f), 0, PetalDrifter, 1.1f);
                s.noGravity = true;
            }

            // Music notes
            if (Main.rand.NextBool(6))
                EroicaVFXLibrary.SpawnSakuraMusicNotes(proj.Center, 1, 8f);

            EroicaVFXLibrary.AddPaletteLighting(proj.Center, 0.5f, 0.6f);
        }

        /// <summary>Smaller sakura burst on spectral copy hit.</summary>
        public static void SpectralCopyHitVFX(Vector2 center)
        {
            EroicaVFXLibrary.HeroicImpact(center, 0.6f);

            SpawnSakuraPetals(center, 4, 4f);
            SpawnPollenMotes(center, 2, 2f);

            // Hit sparks
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                var spark = new GlowSparkParticle(center, vel,
                    Color.Lerp(BloomPink, PollenGold, Main.rand.NextFloat()),
                    0.3f, Main.rand.Next(10, 16));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            EroicaVFXLibrary.SpawnSakuraPetals(center, 3, 20f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 2, 18f);
            Lighting.AddLight(center, BloomPink.ToVector3() * 0.8f);
        }

        /// <summary>Petal scatter + bloom flash when spectral copy dies.</summary>
        public static void SpectralCopyDeathVFX(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 0.5f);

            SpawnSakuraPetals(center, 5, 4f);
            SpawnSakuraEmbers(center, 3, 3f);

            EroicaVFXLibrary.SpawnSakuraPetals(center, 5, 30f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(center, 2, 20f);
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.3f, Volume = 0.4f }, center);

            Lighting.AddLight(center, PetalDrifter.ToVector3() * 0.7f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  5. SPECTRAL COPY PREDRAW — Full custom rendering
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Full spectral copy rendering:
        ///   1. {A=0} sakura bloom trail from oldPos
        ///   2. Afterimage trail: BudCrimson → PollenGold gradient
        ///   3. Perpendicular petal shimmer lines
        ///   4. 4-layer bloom stack
        ///   5. Main sprite with warm sakura tint
        /// </summary>
        public static bool DrawSpectralCopy(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // 1. {A=0} bloom trail
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, BloomPink);

            // 1b. Shader-enhanced sakura swing trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplySakurasBlossomSwingTrail(Main.GlobalTimeWrappedHourly, 1f);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * 0.5f, proj.oldRot[k],
                        glowOrigin, proj.scale * (0.35f + shaderProgress * 0.6f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // 2. Afterimage trail with sakura gradient
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                    new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                Color trailColor = Color.Lerp(BudCrimson, PollenGold, progress);
                trailColor = (trailColor * progress * 0.75f) with { A = 0 };
                float scale = proj.scale * (0.4f + progress * 0.6f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k],
                    drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // 3. Perpendicular petal shimmer lines
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex != null)
            {
                Vector2 glowOrigin = new Vector2(glowTex.Width / 2f, glowTex.Height / 2f);
                for (int k = 0; k < proj.oldPos.Length; k += 3)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                        new Vector2(proj.width / 2f, proj.height / 2f);
                    float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                    Color shimmerColor = BloomPink with { A = 0 } * (progress * 0.2f);
                    float perpRot = proj.oldRot[k] + MathHelper.PiOver2;
                    sb.Draw(glowTex, drawPos, null, shimmerColor, perpRot, glowOrigin,
                        new Vector2(0.12f, progress * 0.6f), SpriteEffects.None, 0f);
                }
            }

            // 4. 4-layer bloom stack — BlossomCore outer → PetalWhite core
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.08f + 1f;
            Color layer1 = (BudCrimson with { A = 0 }) * 0.3f;
            Color layer2 = (BlossomCore with { A = 0 }) * 0.35f;
            Color layer3 = (BloomPink with { A = 0 }) * 0.3f;
            Color layer4 = (PetalWhite with { A = 0 }) * 0.2f;

            sb.Draw(texture, projScreen, null, layer1, proj.rotation, drawOrigin,
                proj.scale * 1.3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, layer2, proj.rotation, drawOrigin,
                proj.scale * 1.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, layer3, proj.rotation, drawOrigin,
                proj.scale * 1.1f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, layer4, proj.rotation, drawOrigin,
                proj.scale * 1.03f * pulse, SpriteEffects.None, 0f);

            // 5. Main sprite with warm sakura tint
            Color mainColor = new Color(255, 235, 230, 215);
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation,
                drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  6. AMBIENT HOLD VFX — Spring blossom aura
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Gentle sakura aura while holding: orbiting petal motes, drifting pollen,
        /// spring-green leaf accents, GlowSpark particles, and pulsing light.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.gameMenu) return;

            // Heroic ambient aura
            EroicaVFXLibrary.SpawnHeroicAura(player.Center, 35f);

            // Orbiting sakura petal motes
            if (Main.rand.NextBool(12))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 10f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                EroicaVFXLibrary.SpawnSakuraMusicNotes(flarePos, 1, 5f);
            }

            // Drifting sakura petals
            if (Main.rand.NextBool(10))
            {
                EroicaVFXLibrary.SpawnSakuraPetals(
                    player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 22f);
            }

            // Custom petal dust — gentle ambient
            if (Main.rand.NextBool(18))
            {
                SpawnSakuraPetals(player.Center, 1, 1f);
            }

            // Spring-green leaf accents
            if (Main.rand.NextBool(25))
            {
                Dust leaf = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Circular(28f, 28f),
                    DustID.GrassBlades,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.3f, 0.8f)),
                    0, SpringGreen, 0.9f);
                leaf.noGravity = true;
            }

            // Pollen sparkles
            if (Main.rand.NextBool(14))
            {
                EroicaVFXLibrary.SpawnValorSparkles(
                    player.Center + Main.rand.NextVector2Circular(25f, 25f), 1, 10f);
            }

            // GlowSpark ambient motes
            if (Main.rand.NextBool(16))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Vector2 vel = Vector2.UnitY * -Main.rand.NextFloat(0.4f, 1.2f);
                Color moteColor = Color.Lerp(BloomPink, PollenGold, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(player.Center + offset, vel,
                    moteColor, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(22, 38));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Pulsing sakura light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Vector3 lightCol = Color.Lerp(BloomPink, PollenGold, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightCol * pulse * 0.55f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  CUSTOM DUST SPAWNERS — Sakura's Blossom's unique particles
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Spawn custom SakuraPetalDust with flutter behavior.</summary>
        public static void SpawnSakuraPetals(Vector2 position, int count, float speed)
        {
            int dustType = ModContent.DustType<SakuraPetalDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + Vector2.UnitY * -0.5f;
                Color color = Color.Lerp(BloomPink, PetalWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(8f, 8f),
                    dustType, vel, 0, color, Main.rand.NextFloat(0.7f, 1.3f));
                d.noGravity = true;
                d.customData = SBDustBehaviorUtil.Petal();
            }
        }

        /// <summary>Spawn custom SakuraEmberDust — burning petal fragments.</summary>
        public static void SpawnSakuraEmbers(Vector2 position, int count, float speed)
        {
            int dustType = ModContent.DustType<SakuraEmberDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                Color color = Color.Lerp(BlossomCore, PollenGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, vel, 0, color, Main.rand.NextFloat(0.6f, 1.1f));
                d.noGravity = true;
                d.customData = SBDustBehaviorUtil.Ember();
            }
        }

        /// <summary>Spawn custom PollenMoteDust — golden rising pollen.</summary>
        public static void SpawnPollenMotes(Vector2 position, int count, float speed)
        {
            int dustType = ModContent.DustType<PollenMoteDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-speed, speed),
                    -Main.rand.NextFloat(speed * 0.5f, speed * 1.5f));
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(10f, 10f),
                    dustType, vel, 0, PollenGold, Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
                d.customData = SBDustBehaviorUtil.Pollen();
            }
        }

        /// <summary>Spawn custom PetalWindLine — directional speed streaks.</summary>
        public static void SpawnPetalWindLines(Vector2 position, Vector2 direction, int count, float speed)
        {
            int dustType = ModContent.DustType<PetalWindLine>();
            for (int i = 0; i < count; i++)
            {
                float angle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(speed * 0.5f, speed);
                Color color = Color.Lerp(BloomPink, PetalDrifter, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(8f, 8f),
                    dustType, vel, 0, color, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
                d.customData = SBDustBehaviorUtil.WindLine();
            }
        }

        /// <summary>Spawn custom BlossomGlowOrb — soft feathered energy orbs.</summary>
        public static void SpawnBlossomGlowOrbs(Vector2 position, int count, float scale)
        {
            int dustType = ModContent.DustType<BlossomGlowOrb>();
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustPerfect(position + offset, dustType,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, BloomPink,
                    scale + Main.rand.NextFloat(-0.1f, 0.1f));
                d.noGravity = true;
                d.customData = SBDustBehaviorUtil.GlowOrb();
            }
        }

        /// <summary>Spawn custom BlossomRingDust — expanding blossom rings.</summary>
        public static void SpawnBlossomRings(Vector2 position, int count, float scale)
        {
            int dustType = ModContent.DustType<BlossomRingDust>();
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, Vector2.Zero, 0, BlossomCore,
                    scale + Main.rand.NextFloat(-0.1f, 0.15f));
                d.noGravity = true;
                d.customData = SBDustBehaviorUtil.Ring();
            }
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  TRAIL FUNCTIONS — For CalamityStyleTrailRenderer
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Sakura trail width — softer taper with petal flutter modulation.</summary>
        public static float SakuraTrailWidth(float completionRatio)
        {
            float baseWidth = 14f;
            float taper = (float)Math.Sin(completionRatio * MathHelper.Pi);
            float flutter = (float)Math.Sin(completionRatio * 6f * MathHelper.Pi) * 0.06f;
            return baseWidth * taper * (1f + flutter);
        }

        /// <summary>Sakura trail color — pink-to-gold gradient with petal shimmer.</summary>
        public static Color SakuraTrailColor(float completionRatio)
        {
            float shimmer = (float)Math.Sin(completionRatio * 5f * MathHelper.Pi) * 0.5f + 0.5f;
            Color baseColor = Color.Lerp(BudCrimson, BloomPink, completionRatio);
            return Color.Lerp(baseColor, PollenGold, shimmer * 0.25f) with { A = 0 };
        }
    }
}
