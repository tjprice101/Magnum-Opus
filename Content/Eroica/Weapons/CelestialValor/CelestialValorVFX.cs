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
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor VFX System — A complete visual identity overhaul.
    /// 
    /// Visual concept: "The Hero's Burning Oath" — a blade that burns with the
    /// courage of fallen heroes. Each swing crescendos from smoldering embers to
    /// a roaring inferno. The combo system represents a hero's battle cry building
    /// from a whisper to a thunderous declaration.
    /// 
    /// Architecture follows IncisorOfMoonlightVFX / SandboxLastPrism pattern:
    ///   - Per-phase accent colors and particle choreography
    ///   - Flame trail ring buffer for afterimage system
    ///   - Standing-wave flame resonance along blade edge
    ///   - Multi-layered bloom cascade on finisher
    ///   - Custom ValorEmberDust / HeroicSparkDust / FlameRibbonDust / ValorCrestDust
    /// </summary>
    public static class CelestialValorVFX
    {
        // ╔══════════════════════════════════════════════════════════╗
        //  UNIQUE ACCENT COLORS (per-weapon, beyond EroicaPalette)
        // ╚══════════════════════════════════════════════════════════╝

        public static readonly Color EmberCore = new Color(255, 140, 50);
        public static readonly Color HeroicBlaze = new Color(255, 90, 30);
        public static readonly Color ValorGlint = new Color(255, 230, 160);
        public static readonly Color CrimsonOath = new Color(200, 30, 50);
        public static readonly Color SakuraDawn = new Color(255, 170, 195);
        public static readonly Color FinaleWhite = new Color(255, 250, 240);

        // ╔══════════════════════════════════════════════════════════╗
        //  FLAME TRAIL RING BUFFER — Afterimage tracking system
        // ╚══════════════════════════════════════════════════════════╝

        private const int FlameTrailLength = 8;
        private static readonly Vector2[] _flameTrailPos = new Vector2[FlameTrailLength];
        private static readonly float[] _flameTrailRot = new float[FlameTrailLength];
        private static int _flameTrailIndex;
        private static int _flameTrailRecordTimer;

        /// <summary>Record blade tip position for afterimage system. Call every frame during swing.</summary>
        public static void RecordFlameTrail(Vector2 tipPos, float rotation)
        {
            _flameTrailRecordTimer++;
            if (_flameTrailRecordTimer % 3 == 0) // Record every 3 frames
            {
                _flameTrailPos[_flameTrailIndex % FlameTrailLength] = tipPos;
                _flameTrailRot[_flameTrailIndex % FlameTrailLength] = rotation;
                _flameTrailIndex++;
            }
        }

        /// <summary>Reset trail buffer on new swing.</summary>
        public static void ResetFlameTrail()
        {
            _flameTrailIndex = 0;
            _flameTrailRecordTimer = 0;
            Array.Fill(_flameTrailPos, Vector2.Zero);
            Array.Fill(_flameTrailRot, 0f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  SWING PHASE VFX — Dramatic escalation per combo step
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Phase 0 — "Valor's Whisper": A controlled opening strike.
        /// Quick bloom flare + directional heroic sparks + ember dust burst.
        /// Musical motif: piano opening notes.
        /// </summary>
        public static void SwingPhase0VFX(Vector2 tipPos, Vector2 swordDir)
        {
            // Bloom flare cascade: gold → scarlet → white-hot
            EroicaVFXLibrary.BloomFlare(tipPos, EroicaPalette.Gold, 0.7f, 18);
            EroicaVFXLibrary.SpawnGradientHaloRings(tipPos, 3, 0.35f);

            // Directional heroic sparks — velocity-aligned
            for (int i = 0; i < 8; i++)
            {
                float angle = swordDir.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                var spark = new GlowSparkParticle(tipPos + Main.rand.NextVector2Circular(6f, 6f),
                    vel, EmberCore, Main.rand.NextFloat(0.4f, 0.65f), Main.rand.Next(14, 22));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Custom ValorEmber dust burst — 4 rising embers
            SpawnValorEmbers(tipPos, 4, 3f);

            // Subtle screen lighting pulse
            Lighting.AddLight(tipPos, EmberCore.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Phase 1 — "Crimson Declaration": The hero raises their voice.
        /// Heroic impact + cross-pattern spark burst + sakura courage petals.
        /// Musical motif: forte declaration with brass.
        /// </summary>
        public static void SwingPhase1VFX(Vector2 tipPos, Vector2 swordDir)
        {
            // Full heroic impact with expanded scale
            EroicaVFXLibrary.HeroicImpact(tipPos, 1.0f);

            // Cross-pattern directional sparks — 4 cardinal + 4 diagonal
            for (int i = 0; i < 12; i++)
            {
                float angle = swordDir.ToRotation() + MathHelper.TwoPi * (i / 12f) + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color sparkColor = (i % 3 == 0) ? EroicaPalette.Gold : CrimsonOath;
                var spark = new GlowSparkParticle(tipPos, vel, sparkColor,
                    Main.rand.NextFloat(0.5f, 0.8f), Main.rand.Next(16, 26));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Sakura courage petals — drifting from sakura pink
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 5, 35f);

            // Flame ribbon dust burst
            SpawnFlameRibbons(tipPos, 6, 5f);

            // Expanding scarlet ring
            var ring = new BloomRingParticle(tipPos, Vector2.Zero,
                CrimsonOath * 0.9f, 0.55f, 22, 0.03f);
            MagnumParticleHandler.SpawnParticle(ring);

            // Music notes — heroic
            EroicaVFXLibrary.SpawnMusicNotes(tipPos, 3, 25f);

            Lighting.AddLight(tipPos, CrimsonOath.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Phase 2 — "Heroic Finale": The full declaration of valor.
        /// Massive finisher slam + screen shake + multi-layered bloom cascade
        /// + god rays + lightning + sakura storm + music note burst.
        /// Musical motif: sforzando orchestral climax.
        /// </summary>
        public static void SwingPhase2VFX(Vector2 tipPos)
        {
            // === SCREEN EFFECTS ===
            EroicaVFXLibrary.FinisherSlam(tipPos, 1.6f);
            ScreenDistortionManager.TriggerRipple(tipPos, CrimsonOath, 0.7f, 25);

            // === MULTI-LAYERED BLOOM CASCADE ===
            // 6 concentric bloom layers expanding outward (SLP flash inspiration)
            for (int layer = 0; layer < 6; layer++)
            {
                float delay = layer * 0.08f;
                float ringScale = 0.3f + layer * 0.15f;
                Color ringColor = EroicaPalette.PaletteLerp(EroicaPalette.CelestialValorBlade, layer / 5f);
                var ring = new BloomRingParticle(tipPos, Vector2.Zero,
                    ringColor * (0.9f - layer * 0.1f), ringScale, 28 + layer * 3, 0.025f + layer * 0.005f);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // === SAKURA STORM — explosive petal cascade ===
            EroicaVFXLibrary.SpawnSakuraPetals(tipPos, 10, 55f);

            // === HEROIC SPARK SUPERNOVA — 20 velocity-aligned sparks radiating outward ===
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                Color sparkColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, Main.rand.NextFloat());
                var spark = new GlowSparkParticle(tipPos, vel, sparkColor,
                    Main.rand.NextFloat(0.6f, 1.1f), Main.rand.Next(18, 30));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === FLAME RIBBONS — spiraling fire streamers ===
            SpawnFlameRibbons(tipPos, 12, 8f);

            // === VALOR CREST — 4-pointed star flash at epicenter ===
            SpawnValorCrests(tipPos, 3, 0.6f);

            // === MUSIC NOTE BURST — full ring of musical power ===
            EroicaVFXLibrary.MusicNoteBurst(tipPos, EroicaPalette.Gold, 10, 5f);
            EroicaVFXLibrary.SpawnSakuraMusicNotes(tipPos, 4, 40f);

            // === LIGHTNING TENDRILS — 6 branching bolts ===
            SpawnHeroicLightning(tipPos, 6);

            // === FIRE SPARK GRAVITY RAIN — debris shower ===
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f) + Vector2.UnitY * -3f;
                Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Torch, vel, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
            }

            Lighting.AddLight(tipPos, EroicaPalette.HotCore.ToVector3() * 2.0f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  PHASE TRANSITION BURST — called when combo step changes
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Burst VFX when transitioning between combo phases.</summary>
        public static void PhaseTransitionBurst(Vector2 center, int newComboStep)
        {
            // Expanding pulse ring
            Color pulseColor = EroicaPalette.PaletteLerp(EroicaPalette.CelestialValorBlade,
                newComboStep / 2f);
            var pulse = new BloomRingParticle(center, Vector2.Zero,
                pulseColor * 0.7f, 0.3f + newComboStep * 0.1f, 18, 0.02f);
            MagnumParticleHandler.SpawnParticle(pulse);

            // Star pattern — escalating count
            int starCount = 3 + newComboStep * 2;
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                Vector2 vel = angle.ToRotationVector2() * (3f + newComboStep * 1.5f);
                var spark = new GlowSparkParticle(center, vel,
                    Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, (float)i / starCount),
                    Main.rand.NextFloat(0.35f, 0.55f), Main.rand.Next(12, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Phase 2: Extra flame bloom + crests
            if (newComboStep >= 2)
            {
                EroicaVFXLibrary.BloomFlare(center, EroicaPalette.Flame, 0.8f, 15);
                SpawnValorCrests(center, 2, 0.4f);
                EroicaVFXLibrary.SpawnMusicNotes(center, 2 + newComboStep, 20f);
            }

            Lighting.AddLight(center, pulseColor.ToVector3() * (0.6f + newComboStep * 0.2f));
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  SWING HIT IMPACT — Complex per-combo-step hit effects
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// On-hit VFX for melee swing — dramatically escalating with combo step.
        /// Phase 0: Ember burst + directed sparks
        /// Phase 1: + expanding ring + sakura + flame ribbons
        /// Phase 2: + valor crests + god rays + screen distortion
        /// </summary>
        public static void SwingHitImpact(Vector2 center, int comboStep)
        {
            // Base impact — always present, scales with combo
            float impactScale = 0.6f + comboStep * 0.25f;
            EroicaVFXLibrary.MeleeImpact(center, comboStep);

            // Directed spark fan — opposite to swing direction
            int sparkCount = 6 + comboStep * 4;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f + comboStep * 2f, 6f + comboStep * 2f);
                Color sparkColor = EroicaPalette.GetFireGradient(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(center + Main.rand.NextVector2Circular(8f, 8f),
                    vel, sparkColor, Main.rand.NextFloat(0.3f, 0.6f + comboStep * 0.1f),
                    Main.rand.Next(12, 20 + comboStep * 3));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Custom ember dust burst
            SpawnValorEmbers(center, 3 + comboStep * 2, 4f + comboStep);

            // Phase 1+: Sakura petals + expanding ring
            if (comboStep >= 1)
            {
                EroicaVFXLibrary.SpawnSakuraPetals(center, 2 + comboStep, 25f);
                var ring = new BloomRingParticle(center, Vector2.Zero,
                    CrimsonOath * 0.7f, 0.3f + comboStep * 0.1f, 18, 0.02f);
                MagnumParticleHandler.SpawnParticle(ring);
                SpawnFlameRibbons(center, comboStep * 3, 4f);
            }

            // Phase 2: Valor crests + screen flash + music notes
            if (comboStep >= 2)
            {
                SpawnValorCrests(center, 2, 0.35f);
                EroicaVFXLibrary.SpawnMusicNotes(center, 4, 30f);
                Lighting.AddLight(center, FinaleWhite.ToVector3() * 1.5f);
            }

            // Music notes on all hits
            EroicaVFXLibrary.SpawnMusicNotes(center, 2 + comboStep, 25f);
            Lighting.AddLight(center, EroicaPalette.Scarlet.ToVector3() * (0.8f + comboStep * 0.2f));
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  SWING TRAIL VFX — Per-frame blade-edge choreography
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Per-frame swing trail VFX — a multi-layered heroic fire blade.
        /// 
        /// Layers (inside → outside):
        ///   1. Flame resonance points along blade (standing-wave pattern)
        ///   2. Valor ember trail at tip + along blade
        ///   3. Sakura shimmer accents (Phase 1+)
        ///   4. Music notes (periodic)
        ///   5. Flame ribbon afterimages (Phase 2)
        ///   6. Bloom at blade tip (always)
        ///   7. Afterimage trail drawing
        /// </summary>
        public static void DrawSwingTrailVFX(Vector2 tipPos, Vector2 ownerCenter,
            Vector2 swordDir, float progression, int comboStep)
        {
            if (progression <= 0.06f || progression >= 0.94f)
                return;

            float swingIntensity = MathHelper.Clamp((progression - 0.06f) / 0.12f, 0f, 1f)
                                 * MathHelper.Clamp((0.94f - progression) / 0.12f, 0f, 1f);

            // Record trail for afterimage system
            RecordFlameTrail(tipPos, swordDir.ToRotation());

            // ── 1. FLAME RESONANCE — Standing wave fire nodes along blade ──
            float bladeLength = Vector2.Distance(ownerCenter, tipPos);
            int resonanceNodes = 4 + comboStep * 2;
            float waveFrequency = 3f + comboStep; // Increases with combo step
            for (int n = 0; n < resonanceNodes; n++)
            {
                float t = (n + 1f) / (resonanceNodes + 1f);
                float waveAmp = (float)Math.Sin(t * waveFrequency * MathHelper.Pi +
                    Main.GameUpdateCount * 0.15f) * (4f + comboStep * 2f);

                Vector2 bladePoint = Vector2.Lerp(ownerCenter, tipPos, t);
                Vector2 perpendicular = new Vector2(-swordDir.Y, swordDir.X);
                bladePoint += perpendicular * waveAmp;

                // Spawn flame resonance dust at antinode peaks
                if (Math.Abs(waveAmp) > 2f && Main.rand.NextBool(2))
                {
                    Color nodeColor = EroicaPalette.GetFireGradient(t);
                    Dust d = Dust.NewDustPerfect(bladePoint, DustID.GoldFlame,
                        perpendicular * waveAmp * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0, nodeColor, 0.8f + comboStep * 0.15f);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }
            }

            // ── 2. VALOR EMBER TRAIL — Custom dust at blade tip ──
            if (Main.rand.NextBool(Math.Max(1, 2 - comboStep)))
            {
                SpawnValorEmbers(tipPos, 1, 2f + comboStep);
            }

            // Dense swing dust at blade tip
            EroicaVFXLibrary.SpawnSwingDust(tipPos, -swordDir, DustID.GoldFlame);

            // Additional ember along blade (1/3 of blade length from tip)
            if (Main.rand.NextBool(2))
            {
                Vector2 midPos = Vector2.Lerp(ownerCenter, tipPos, 0.7f);
                Dust mid = Dust.NewDustPerfect(midPos, DustID.CrimsonTorch,
                    -swordDir * Main.rand.NextFloat(0.5f, 1.5f), 0,
                    EroicaPalette.Scarlet, 1.0f + comboStep * 0.2f);
                mid.noGravity = true;
            }

            // ── 3. SAKURA SHIMMER ACCENTS (Phase 1+) ──
            if (comboStep >= 1 && Main.rand.NextBool(3))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Color sakuraColor = Color.Lerp(EroicaPalette.Sakura, SakuraDawn, Main.rand.NextFloat());
                Dust s = Dust.NewDustPerfect(sparklePos, DustID.PinkFairy,
                    -swordDir * Main.rand.NextFloat(0.5f, 2.5f), 0, sakuraColor, 1.2f);
                s.noGravity = true;
            }

            // Valor shimmer / hue oscillation — always active now
            if (Main.rand.NextBool(3))
            {
                Color valorColor = EroicaPalette.GetShimmer(Main.GameUpdateCount);
                Dust v = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.GoldFlame, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, valorColor,
                    1.3f + comboStep * 0.15f);
                v.noGravity = true;
            }

            // ── 4. MUSIC NOTES — Frequency increases with combo ──
            int noteChance = 6 - comboStep; // 6, 5, 4
            if (Main.rand.NextBool(Math.Max(2, noteChance)))
            {
                EroicaVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.95f, 30);
            }

            // ── 5. FLAME RIBBONS (Phase 2) — flowing fire streamers ──
            if (comboStep >= 2 && Main.rand.NextBool(4))
            {
                SpawnFlameRibbons(tipPos, 1, 3f);
            }

            // ── 6. BLADE TIP BLOOM — Always present, scales with combo ──
            if (swingIntensity > 0f)
            {
                float bloomScale = (0.35f + comboStep * 0.12f) * swingIntensity;
                EroicaVFXLibrary.DrawComboBloom(tipPos, comboStep, bloomScale, swingIntensity);
            }

            // ── 7. AFTERIMAGE TRAIL — Draw ghosted blades from ring buffer (Phase 1+) ──
            if (comboStep >= 1)
            {
                DrawFlameAfterimages(tipPos, comboStep);
            }

            // Contrast sparkle for dual-color trail edge
            EroicaVFXLibrary.SpawnContrastSparkle(tipPos, -swordDir);

            // Heroic aura light
            float lightIntensity = 0.5f + comboStep * 0.15f;
            Color lightColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, progression);
            Lighting.AddLight(tipPos, lightColor.ToVector3() * lightIntensity * swingIntensity);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  AFTERIMAGE DRAWING — Ghosted blade echoes
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Draw ghosted flame afterimages from the ring buffer.</summary>
        private static void DrawFlameAfterimages(Vector2 currentTip, int comboStep)
        {
            int count = Math.Min(_flameTrailIndex, FlameTrailLength);
            if (count < 2) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 origin = new Vector2(glowTex.Width / 2f, glowTex.Height / 2f);

            for (int i = 0; i < count; i++)
            {
                int idx = (_flameTrailIndex - 1 - i + FlameTrailLength) % FlameTrailLength;
                Vector2 pos = _flameTrailPos[idx];
                if (pos == Vector2.Zero) continue;

                float fadeProgress = (float)i / count;
                Color ghostColor = Color.Lerp(HeroicBlaze, CrimsonOath, fadeProgress);
                ghostColor = ghostColor with { A = 0 } * (0.35f - fadeProgress * 0.3f);
                float ghostScale = (0.5f + comboStep * 0.1f) * (1f - fadeProgress * 0.5f);

                sb.Draw(glowTex, pos - Main.screenPosition, null, ghostColor,
                    _flameTrailRot[idx], origin, ghostScale, SpriteEffects.None, 0f);
            }
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  PROJECTILE TRAIL VFX — Enhanced trail particles
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Per-frame projectile trail: flame dust, valor sparks, ember shimmer, music notes.</summary>
        public static void ProjectileTrailVFX(Projectile proj)
        {
            // Flame trail dust — every frame
            if (Main.rand.NextBool(2))
                EroicaVFXLibrary.SpawnFlameTrailDust(proj.Center, proj.velocity);

            // Custom valor embers — rising from projectile path
            if (Main.rand.NextBool(3))
                SpawnValorEmbers(proj.Center, 1, 2f);

            // Valor sparkles
            if (Main.rand.NextBool(3))
                EroicaVFXLibrary.SpawnValorSparkles(proj.Center, 1, 8f);

            // Perpendicular fire shimmer
            if (Main.rand.NextBool(4))
            {
                Vector2 perp = new Vector2(-proj.velocity.Y, proj.velocity.X).SafeNormalize(Vector2.UnitX);
                Dust shimmer = Dust.NewDustPerfect(proj.Center, DustID.GoldFlame,
                    perp * Main.rand.NextFloat(-2f, 2f), 0, EmberCore, 0.8f);
                shimmer.noGravity = true;
            }

            // Music notes
            if (Main.rand.NextBool(5))
                EroicaVFXLibrary.SpawnMusicNotes(proj.Center, 1, 10f, 0.6f, 0.85f, 25);

            EroicaVFXLibrary.AddPaletteLighting(proj.Center, 0.4f, 0.9f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  PROJECTILE HIT VFX — Impact burst at target
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Enhanced projectile impact: bloom cascade + spark fan + music.</summary>
        public static void ProjectileHitVFX(Vector2 center)
        {
            EroicaVFXLibrary.HeroicImpact(center, 0.9f);

            // Spark fan — 8 directional sparks
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                var spark = new GlowSparkParticle(center, vel,
                    Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(12, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            SpawnValorEmbers(center, 3, 4f);
            EroicaVFXLibrary.SpawnMusicNotes(center, 4, 30f, 0.7f, 1.0f, 28);
            Lighting.AddLight(center, ValorGlint.ToVector3() * 1.2f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  AOE EXPLOSION — Full heroic detonation
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Complete AOE explosion VFX: layered boom with bloom cascade, lightning,
        /// fire rain, music burst, and screen effects.
        /// </summary>
        public static void AOEExplosion(Vector2 position)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.25f, Volume = 0.6f }, position);
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.3f, Volume = 0.35f }, position);

            // === SCREEN EFFECTS ===
            EroicaVFXLibrary.HeroicImpact(position, 1.6f);
            ScreenDistortionManager.TriggerRipple(position, EroicaPalette.Scarlet, 0.65f, 22);

            // === MULTI-RING BLOOM CASCADE ===
            for (int ring = 0; ring < 4; ring++)
            {
                float ringScale = 0.5f + ring * 0.2f;
                Color ringColor = EroicaPalette.PaletteLerp(EroicaPalette.CelestialValorBlade, ring / 3f);
                var halo = new BloomRingParticle(position, Vector2.Zero,
                    ringColor * (0.85f - ring * 0.1f), ringScale, 25 + ring * 3, 0.025f);
                MagnumParticleHandler.SpawnParticle(halo);
            }

            // === MUSICAL IMPACT ===
            EroicaVFXLibrary.MusicalImpact(position, 1.3f, true);

            // === LIGHTNING TENDRILS ===
            SpawnHeroicLightning(position, 5);

            // === SPARK SUPERNOVA ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                var spark = new GlowSparkParticle(position, vel,
                    EroicaPalette.GetFireGradient(Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.5f, 0.9f), Main.rand.Next(15, 28));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === VALOR EMBER SHOWER ===
            SpawnValorEmbers(position, 10, 8f);
            SpawnFlameRibbons(position, 8, 7f);

            // === DUST RINGS ===
            EroicaVFXLibrary.SpawnRadialDustBurst(position, 30, 10f, DustID.GoldFlame);
            EroicaVFXLibrary.SpawnRadialDustBurst(position, 20, 7f, DustID.CrimsonTorch);

            // === FIRE DEBRIS RAIN ===
            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(9f, 9f);
                Dust.NewDustPerfect(position, DustID.Torch, vel, 0, default,
                    Main.rand.NextFloat(1.5f, 2.5f));
            }

            Lighting.AddLight(position, EroicaPalette.HotCore.ToVector3() * 1.8f);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  HEROIC LIGHTNING — Improved branching bolts
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Spawn branching heroic lightning bolts with ember particles.</summary>
        public static void SpawnHeroicLightning(Vector2 position, int boltCount)
        {
            for (int b = 0; b < boltCount; b++)
            {
                float baseAngle = MathHelper.TwoPi * b / boltCount + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 boltDir = baseAngle.ToRotationVector2();
                Vector2 boltStart = position;

                int segments = Main.rand.Next(5, 9);
                for (int seg = 0; seg < segments; seg++)
                {
                    float segLen = Main.rand.NextFloat(14f, 28f);
                    float offset = Main.rand.NextFloat(-0.5f, 0.5f);
                    boltDir = boltDir.RotatedBy(offset);
                    Vector2 boltEnd = boltStart + boltDir * segLen;

                    // Draw bolt segment with alternating smoke + fire dust
                    for (int p = 0; p < 4; p++)
                    {
                        float lerp = p / 4f;
                        Vector2 particlePos = Vector2.Lerp(boltStart, boltEnd, lerp);

                        if (p % 2 == 0)
                        {
                            Dust smoke = Dust.NewDustPerfect(particlePos, DustID.Smoke,
                                Main.rand.NextVector2Circular(0.8f, 0.8f), 180, Color.Black, 1.3f);
                            smoke.noGravity = true;
                        }
                        else
                        {
                            Color boltColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold,
                                (float)seg / segments);
                            Dust fire = Dust.NewDustPerfect(particlePos, DustID.CrimsonTorch,
                                Main.rand.NextVector2Circular(0.8f, 0.8f), 80, boltColor, 1.6f);
                            fire.noGravity = true;
                        }
                    }

                    // Bright ember at each joint
                    var jointSpark = new GlowSparkParticle(boltEnd,
                        Main.rand.NextVector2Circular(1.5f, 1.5f),
                        EmberCore, 0.25f, 10);
                    MagnumParticleHandler.SpawnParticle(jointSpark);

                    boltStart = boltEnd;
                }
            }

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.35f }, position);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  DEATH FLASH — Projectile expiry effect
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Projectile death: ember scatter + flash + sound.</summary>
        public static void DeathFlash(Vector2 center)
        {
            EroicaVFXLibrary.DeathHeroicFlash(center, 0.8f);

            // Scatter embers outward
            SpawnValorEmbers(center, 5, 4f);

            // Small spark burst
            for (int i = 0; i < 6; i++)
            {
                var spark = new GlowSparkParticle(center,
                    Main.rand.NextVector2Circular(4f, 4f),
                    EmberCore, 0.3f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.45f, Pitch = 0.2f }, center);
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  PROJECTILE PREDRAW — Complete custom rendering
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Full projectile rendering pipeline:
        ///   1. {A=0} bloom trail from oldPos
        ///   2. Flame afterimage trail with gradient fade
        ///   3. Perpendicular fire shimmer lines
        ///   4. 4-layer bloom stack
        ///   5. Main sprite with warm tint
        /// </summary>
        public static bool DrawProjectile(SpriteBatch sb, Projectile proj, ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 projScreen = proj.Center - Main.screenPosition;

            // 1. {A=0} bloom trail via library
            EroicaVFXLibrary.DrawProjectileTrail(sb, proj, EroicaPalette.Scarlet);

            // 1b. Shader-enhanced flame trail pass
            {
                Texture2D shaderGlow = MagnumTextureRegistry.GetSoftGlow();
                EroicaShaderManager.BeginShaderAdditive(sb);
                EroicaShaderManager.ApplyCelestialValorSwingTrail(Main.GlobalTimeWrappedHourly, 0.5f);
                Vector2 glowOrigin = shaderGlow.Size() * 0.5f;
                for (int k = 0; k < proj.oldPos.Length; k++)
                {
                    if (proj.oldPos[k] == Vector2.Zero) continue;
                    Vector2 shaderPos = proj.oldPos[k] - Main.screenPosition + new Vector2(proj.width / 2f, proj.height / 2f);
                    float shaderProgress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                    sb.Draw(shaderGlow, shaderPos, null, Color.White * shaderProgress * 0.6f, proj.oldRot[k],
                        glowOrigin, proj.scale * (0.4f + shaderProgress * 0.7f), SpriteEffects.None, 0f);
                }
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            // 2. Flame afterimage trail — gradient from gold (newest) to deep scarlet (oldest)
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;

                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                    new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                // Fire gradient: newest = gold, oldest = deep scarlet
                Color trailColor = Color.Lerp(EroicaPalette.DeepScarlet, EmberCore, progress);
                trailColor = (trailColor * progress * 0.7f) with { A = 0 };
                float scale = proj.scale * (0.4f + progress * 0.6f);

                sb.Draw(texture, drawPos, null, trailColor, proj.oldRot[k],
                    drawOrigin, scale, SpriteEffects.None, 0f);
            }

            // 3. Perpendicular fire shimmer lines (every 3rd old position)
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

                    Color shimmerColor = EroicaPalette.Gold with { A = 0 } * (progress * 0.25f);
                    float perpRot = proj.oldRot[k] + MathHelper.PiOver2;
                    sb.Draw(glowTex, drawPos, null, shimmerColor, perpRot, glowOrigin,
                        new Vector2(0.15f, progress * 0.8f), SpriteEffects.None, 0f);
                }
            }

            // 4. 4-layer bloom stack — pulsing
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.08f + 1f;
            Color layer1 = (EroicaPalette.DeepScarlet with { A = 0 }) * 0.3f;
            Color layer2 = (EroicaPalette.Scarlet with { A = 0 }) * 0.35f;
            Color layer3 = (EroicaPalette.Gold with { A = 0 }) * 0.3f;
            Color layer4 = (EroicaPalette.HotCore with { A = 0 }) * 0.2f;

            sb.Draw(texture, projScreen, null, layer1, proj.rotation, drawOrigin,
                proj.scale * 1.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, layer2, proj.rotation, drawOrigin,
                proj.scale * 1.22f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, layer3, proj.rotation, drawOrigin,
                proj.scale * 1.10f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, null, layer4, proj.rotation, drawOrigin,
                proj.scale * 1.03f * pulse, SpriteEffects.None, 0f);

            // 5. Main sprite with warm heroic tint
            Color mainColor = new Color(255, 245, 225, 215);
            sb.Draw(texture, projScreen, null, mainColor, proj.rotation,
                drawOrigin, proj.scale, SpriteEffects.None, 0f);

            return false;
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  CUSTOM DUST SPAWNERS — Celestial Valor's unique particles
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>Spawn custom ValorEmber dust with behavior.</summary>
        public static void SpawnValorEmbers(Vector2 position, int count, float speed)
        {
            int dustType = ModContent.DustType<ValorEmberDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + Vector2.UnitY * -1.5f;
                Color color = EroicaPalette.GetFireGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(8f, 8f),
                    dustType, vel, 0, color, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
                d.customData = CVDustBehaviorUtil.CreateHeroicEmber(Main.rand.Next(30, 50));
            }
        }

        /// <summary>Spawn custom FlameRibbon dust with behavior.</summary>
        public static void SpawnFlameRibbons(Vector2 position, int count, float speed)
        {
            int dustType = ModContent.DustType<FlameRibbonDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                Dust d = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, vel, 0, EroicaPalette.Flame, Main.rand.NextFloat(0.6f, 1.2f));
                d.noGravity = true;
                d.customData = CVDustBehaviorUtil.CreateFlameRibbon(Main.rand.Next(25, 40));
            }
        }

        /// <summary>Spawn custom ValorCrest dust (4-pointed star flash).</summary>
        public static void SpawnValorCrests(Vector2 position, int count, float scale)
        {
            int dustType = ModContent.DustType<ValorCrestDust>();
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustPerfect(position + offset, dustType,
                    Main.rand.NextVector2Circular(1f, 1f), 0, EroicaPalette.Gold,
                    scale + Main.rand.NextFloat(-0.1f, 0.1f));
                d.noGravity = true;
            }
        }

        // ╔══════════════════════════════════════════════════════════╗
        //  TRAIL FUNCTIONS — For CalamityStyleTrailRenderer
        // ╚══════════════════════════════════════════════════════════╝

        /// <summary>
        /// Heroic flame trail width function — wider at center, tapers at edges,
        /// with a subtle oscillation from the musical "vibrato" of the blade.
        /// </summary>
        public static float HeroicFlameTrailWidth(float completionRatio)
        {
            float baseWidth = 16f;
            float taper = (float)Math.Sin(completionRatio * MathHelper.Pi); // Wide in middle
            float vibrato = (float)Math.Sin(completionRatio * 8f * MathHelper.Pi) * 0.08f; // Musical oscillation
            return baseWidth * taper * (1f + vibrato);
        }

        /// <summary>
        /// Heroic flame trail color function — scarlet edge to gold-white core
        /// with standing-wave brightness variation.
        /// </summary>
        public static Color HeroicFlameTrailColor(float completionRatio)
        {
            float wave = (float)Math.Sin(completionRatio * 6f * MathHelper.Pi) * 0.5f + 0.5f;
            Color baseColor = Color.Lerp(EroicaPalette.DeepScarlet, EmberCore, completionRatio);
            return Color.Lerp(baseColor, EroicaPalette.Gold, wave * 0.3f) with { A = 0 };
        }
    }
}
