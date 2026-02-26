using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon
{
    /// <summary>
    /// Overhauled VFX for Eternal Moon — "The Eternal Tide".
    /// 5-phase Tidal Lunar Cycle visual pipeline.
    ///
    /// Visual Identity: Moonlight on water — flowing, cyclical, tidal.
    /// - Tidal wake: Blade leaves flowing afterwash that ripples outward
    /// - Lunar phase cycling: New → Waxing → Half → Waning → Full moon visual escalation
    /// - Crescent bloom: Asymmetric glow that grows with each phase (shader-driven on 2+)
    /// - Tidal dust: Flowing water-like particles with sinusoidal drift
    /// - Eclipse flash: Brief dark-to-light burst on phase transitions
    /// - Shader trails: TidalTrail.fx for flowing water caustics (phases 1+)
    /// - Shader aura: LunarPhaseAura.fx concentric rings around player (phases 3+)
    ///
    /// Inspired by:
    /// - Calamity Ark of Cosmos: Curved energy slashes, layered trails
    /// - Coralite NoctiflairStrike: Expanding phase mechanics, curved flight
    /// - VFX+ SandboxLastPrism: Multi-layer dust, screen effects, shader pipeline
    /// </summary>
    public static class EternalMoonVFX
    {
        // === UNIQUE ACCENT COLORS ===
        public static readonly Color CrescentGlow = new Color(170, 225, 255);
        public static readonly Color LunarEclipse = new Color(60, 20, 80);
        public static readonly Color TidalFoam = new Color(200, 220, 255);
        public static readonly Color DeepTide = new Color(40, 30, 100);
        public static readonly Color TidalLavender = new Color(175, 130, 255);

        // === TIDAL WAKE STORAGE ===
        private static readonly Vector2[] _wakePositions = new Vector2[16];
        private static readonly float[] _wakeRotations = new float[16];
        private static int _wakeWriteIndex;
        private static int _wakeCount;
        private static int _wakeTimer;

        // === 5-PHASE LUNAR MAPPING ===
        private static float GetMoonPhase(int comboStep)
        {
            return comboStep switch
            {
                0 => 0.15f,  // New Moon — barely visible
                1 => 0.35f,  // Waxing Crescent
                2 => 0.55f,  // Half Moon
                3 => 0.80f,  // Waning Gibbous
                4 => 1.00f,  // Full Moon
                _ => 0.5f
            };
        }

        /// <summary>
        /// Lunar phase color cycling unique to EternalMoon.
        /// Flows through the palette like moonlight on water.
        /// </summary>
        public static Color GetLunarPhaseColor(float progress, int comboStep)
        {
            float phaseOffset = comboStep * 0.05f;
            float t = MathHelper.Clamp(progress + phaseOffset, 0f, 1f);

            if (t < 0.2f)
                return Color.Lerp(DeepTide, MoonlightVFXLibrary.DarkPurple, t / 0.2f);
            else if (t < 0.4f)
                return Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.Violet, (t - 0.2f) / 0.2f);
            else if (t < 0.6f)
                return Color.Lerp(MoonlightVFXLibrary.Violet, TidalLavender, (t - 0.4f) / 0.2f);
            else if (t < 0.8f)
                return Color.Lerp(TidalLavender, MoonlightVFXLibrary.IceBlue, (t - 0.6f) / 0.2f);
            else
                return Color.Lerp(MoonlightVFXLibrary.IceBlue, MoonlightVFXLibrary.MoonWhite, (t - 0.8f) / 0.2f);
        }

        /// <summary>
        /// Resets tidal wake tracking. Call at the start of each new swing.
        /// </summary>
        public static void ResetWakeTracking()
        {
            _wakeCount = 0;
            _wakeWriteIndex = 0;
            _wakeTimer = 0;
            Array.Clear(_wakePositions, 0, _wakePositions.Length);
        }

        // =====================================================================
        //  TIDAL WAKE SYSTEM
        // =====================================================================

        /// <summary>
        /// Records a tidal wake position at the blade midpoint.
        /// </summary>
        public static void RecordWakePosition(Vector2 bladePos, float rotation)
        {
            _wakeTimer++;
            if (_wakeTimer % 3 != 0) return;

            int idx = _wakeWriteIndex % _wakePositions.Length;
            _wakePositions[idx] = bladePos;
            _wakeRotations[idx] = rotation;
            _wakeWriteIndex++;
            _wakeCount = Math.Min(_wakeCount + 1, _wakePositions.Length);
        }

        /// <summary>
        /// Draws the tidal wake — flowing water-like trail that ripples outward
        /// from past blade positions. Creates "moonlight on water" afterwash.
        /// Phases 1+: Shader-driven tidal trail overlay on wake segments.
        /// </summary>
        public static void DrawTidalWake(SpriteBatch sb, int comboStep, float progression)
        {
            if (sb == null || _wakeCount < 2) return;

            var glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            float phase = GetMoonPhase(comboStep);
            float fadeMultiplier = MathHelper.Clamp(progression * 3f, 0f, 1f)
                                 * MathHelper.Clamp((1f - progression) * 4f, 0f, 1f);

            int start = Math.Max(0, _wakeWriteIndex - _wakeCount);

            // === SHADER-DRIVEN TIDAL WAKE GLOW (phases 1+) ===
            if (comboStep >= 1 && MoonlightSonataShaderManager.HasTidalTrail)
            {
                DrawShaderTidalWake(sb, comboStep, phase, fadeMultiplier, start, glowTex);
            }

            // === PARTICLE-BASED WAKE (always drawn as baseline) ===
            for (int i = start; i < _wakeWriteIndex; i++)
            {
                int idx = i % _wakePositions.Length;
                Vector2 wakeScreen = _wakePositions[idx] - Main.screenPosition;
                if (wakeScreen == -Main.screenPosition) continue;

                float age = (float)(_wakeWriteIndex - i) / _wakeCount;
                float wakeAlpha = (1f - age) * fadeMultiplier * phase;

                // Ripple offset — perpendicular displacement that increases with age
                float ripple = MathF.Sin(age * MathF.PI * 3f + Main.GlobalTimeWrappedHourly * 4f);
                float rippleOffset = ripple * age * 8f * phase;
                Vector2 perp = new Vector2(
                    MathF.Cos(_wakeRotations[idx] + MathHelper.PiOver2),
                    MathF.Sin(_wakeRotations[idx] + MathHelper.PiOver2));
                Vector2 ripplePos = wakeScreen + perp * rippleOffset;

                float wakeScale = (0.06f + age * 0.12f) * phase;
                Color wakeColor = Color.Lerp(TidalFoam, MoonlightVFXLibrary.IceBlue, age);

                // 3-layer wake glow
                sb.Draw(glowTex, ripplePos, null,
                    (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * wakeAlpha * 0.2f,
                    0f, glowOrigin, wakeScale * 2.0f, SpriteEffects.None, 0f);
                sb.Draw(glowTex, ripplePos, null,
                    (wakeColor with { A = 0 }) * wakeAlpha * 0.4f,
                    0f, glowOrigin, wakeScale * 1.2f, SpriteEffects.None, 0f);
                sb.Draw(glowTex, ripplePos, null,
                    (Color.White with { A = 0 }) * wakeAlpha * 0.25f,
                    0f, glowOrigin, wakeScale * 0.5f, SpriteEffects.None, 0f);

                // Connecting flow lines between wake points
                if (i > start)
                {
                    int prevIdx = (i - 1) % _wakePositions.Length;
                    Vector2 prevScreen = _wakePositions[prevIdx] - Main.screenPosition;
                    if (prevScreen == -Main.screenPosition) continue;

                    float lineDist = Vector2.Distance(wakeScreen, prevScreen);
                    if (lineDist < 2f || lineDist > 200f) continue;

                    float lineAngle = (wakeScreen - prevScreen).ToRotation();
                    Vector2 lineCenter = (wakeScreen + prevScreen) / 2f;

                    sb.Draw(glowTex, lineCenter, null,
                        (MoonlightVFXLibrary.Violet with { A = 0 }) * wakeAlpha * 0.15f,
                        lineAngle, glowOrigin,
                        new Vector2(lineDist / glowTex.Width, 0.015f),
                        SpriteEffects.None, 0f);
                }

                // Star sparkle at the newest wake positions
                if (i >= _wakeWriteIndex - 3 && age < 0.3f)
                {
                    var starTex = MoonlightSonataTextures.Star4Point?.Value;
                    if (starTex != null)
                    {
                        Vector2 starOrig = starTex.Size() * 0.5f;
                        float starRot = Main.GlobalTimeWrappedHourly * 3f + i * 0.5f;
                        sb.Draw(starTex, ripplePos, null,
                            (CrescentGlow with { A = 0 }) * wakeAlpha * 0.6f,
                            starRot, starOrig, 0.06f * phase, SpriteEffects.None, 0f);
                    }
                }
            }
        }

        /// <summary>
        /// Draws shader-driven tidal glow overlays on the wake trail segments.
        /// Uses TidalTrail.fx CrescentGlowPass for a soft, flowing water feel.
        /// </summary>
        private static void DrawShaderTidalWake(SpriteBatch sb, int comboStep, float phase,
            float fadeMultiplier, int start, Texture2D glowTex)
        {
            if (glowTex == null) return;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);
                MoonlightSonataShaderManager.ApplyEternalMoonTidalTrail(
                    Main.GlobalTimeWrappedHourly, phase, glowPass: true);

                for (int i = start; i < _wakeWriteIndex; i++)
                {
                    int idx = i % _wakePositions.Length;
                    Vector2 wakeScreen = _wakePositions[idx] - Main.screenPosition;
                    if (wakeScreen == -Main.screenPosition) continue;

                    float age = (float)(_wakeWriteIndex - i) / _wakeCount;
                    float wakeAlpha = (1f - age) * fadeMultiplier * phase * 0.5f;
                    float wakeScale = (0.10f + age * 0.15f) * phase;

                    sb.Draw(glowTex, wakeScreen, null,
                        Color.White * wakeAlpha,
                        _wakeRotations[idx], glowOrigin,
                        wakeScale * 1.8f, SpriteEffects.None, 0f);
                }

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
            }
        }

        // =====================================================================
        //  CRESCENT TIP BLOOM (Enhanced + Shader Integration)
        // =====================================================================

        /// <summary>
        /// Enhanced crescent-shaped multi-layer bloom at the blade tip.
        /// Phase 0: Thin new moon sliver.
        /// Phase 1: Waxing crescent.
        /// Phase 2: Half moon.
        /// Phase 3: Gibbous blaze.
        /// Phase 4: Full blazing moon.
        /// Phases 2+: Shader-driven crescent overlay via CrescentBloom.fx.
        /// </summary>
        public static void DrawCrescentTipBloom(SpriteBatch sb, Vector2 tipWorldPos,
            float swordRotation, int comboStep, float progression)
        {
            if (sb == null) return;

            float phase = GetMoonPhase(comboStep);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + comboStep) * 0.12f;
            float intensityRamp = MathHelper.Clamp(progression * 3f, 0f, 1f)
                                * MathHelper.Clamp((1f - progression) * 4f, 0f, 1f);
            float baseScale = (0.20f + phase * 0.30f) * pulse * intensityRamp;

            if (baseScale < 0.05f) return;

            Vector2 crescentOffset = new Vector2(
                MathF.Cos(swordRotation + MathHelper.PiOver2),
                MathF.Sin(swordRotation + MathHelper.PiOver2)) * (5f * phase);

            Vector2 drawPos = tipWorldPos - Main.screenPosition;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer deep tide halo
            sb.Draw(bloomTex, drawPos + crescentOffset, null,
                (DeepTide with { A = 0 }) * 0.2f * intensityRamp,
                0f, origin, baseScale * 2.5f, SpriteEffects.None, 0f);

            // Layer 2: DarkPurple crescent halo
            sb.Draw(bloomTex, drawPos + crescentOffset * 0.7f, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.3f * intensityRamp,
                0f, origin, baseScale * 1.8f, SpriteEffects.None, 0f);

            // Layer 3: Violet mid crescent
            sb.Draw(bloomTex, drawPos + crescentOffset * 0.3f, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.45f * intensityRamp,
                0f, origin, baseScale * 1.3f, SpriteEffects.None, 0f);

            // Layer 4: Ice blue inner
            sb.Draw(bloomTex, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.6f * intensityRamp,
                0f, origin, baseScale * 0.85f, SpriteEffects.None, 0f);

            // Layer 5: Crescent gold core
            sb.Draw(bloomTex, drawPos, null,
                (CrescentGlow with { A = 0 }) * 0.7f * intensityRamp,
                0f, origin, baseScale * 0.35f, SpriteEffects.None, 0f);

            // Phase 2+: Tidal lavender accent on the shadow side
            if (comboStep >= 2)
            {
                sb.Draw(bloomTex, drawPos - crescentOffset * 0.4f, null,
                    (TidalLavender with { A = 0 }) * 0.2f * intensityRamp,
                    0f, origin, baseScale * 1.0f, SpriteEffects.None, 0f);
            }

            // Phase 3+: Additional deep tide outer ring
            if (comboStep >= 3)
            {
                sb.Draw(bloomTex, drawPos + crescentOffset * 1.2f, null,
                    (DeepTide with { A = 0 }) * 0.12f * intensityRamp,
                    0f, origin, baseScale * 3.2f, SpriteEffects.None, 0f);
            }

            // Bloom orb ring — lunar halo accent around blade tip
            var orbTex = MoonlightSonataTextures.BloomOrb?.Value;
            if (orbTex != null)
            {
                Vector2 orbOrigin = orbTex.Size() * 0.5f;
                sb.Draw(orbTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.18f * intensityRamp,
                    0f, orbOrigin, baseScale * 0.55f, SpriteEffects.None, 0f);
            }

            // 4-pointed star sparkle at crescent peak
            var starTex = MoonlightSonataTextures.Star4Point?.Value;
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() * 0.5f;
                float starRot = Main.GlobalTimeWrappedHourly * 2f;
                float starScale = baseScale * (0.20f + comboStep * 0.03f);
                sb.Draw(starTex, drawPos, null,
                    (CrescentGlow with { A = 0 }) * 0.5f * intensityRamp,
                    starRot, starOrigin, starScale, SpriteEffects.None, 0f);

                // Phase 4: Larger, slower counter-rotating star
                if (comboStep >= 4)
                {
                    sb.Draw(starTex, drawPos, null,
                        (Color.White with { A = 0 }) * 0.25f * intensityRamp,
                        -starRot * 0.6f, starOrigin, starScale * 1.5f, SpriteEffects.None, 0f);
                }
            }
        }

        // =====================================================================
        //  LUNAR PHASE AURA (Shader-driven, Phases 3+)
        // =====================================================================

        /// <summary>
        /// Draws the LunarPhaseAura shader around the player center.
        /// Expanding concentric tidal rings visible during active swing on phases 3+.
        /// Falls back to particle-only mode if shader is unavailable.
        /// </summary>
        public static void DrawLunarPhaseAura(SpriteBatch sb, Vector2 playerCenter,
            int comboStep, float progression)
        {
            if (sb == null || comboStep < 3) return;

            float phase = GetMoonPhase(comboStep);
            float auraFade = MathHelper.Clamp(progression * 4f, 0f, 1f)
                           * MathHelper.Clamp((1f - progression) * 4f, 0f, 1f);

            if (auraFade < 0.05f) return;

            Vector2 drawPos = playerCenter - Main.screenPosition;
            float auraScale = 0.5f + phase * 0.5f;

            if (MoonlightSonataShaderManager.HasLunarPhaseAura)
            {
                var auraTex = MoonlightSonataTextures.TidalBloom?.Value
                           ?? MagnumTextureRegistry.GetSoftGlow();
                if (auraTex == null) return;

                Vector2 origin = auraTex.Size() * 0.5f;

                try
                {
                    MoonlightSonataShaderManager.BeginShaderBatch(sb);
                    MoonlightSonataShaderManager.ApplyEternalMoonAura(
                        Main.GlobalTimeWrappedHourly, phase);

                    sb.Draw(auraTex, drawPos, null,
                        Color.White * auraFade * 0.6f, 0f, origin,
                        auraScale, SpriteEffects.None, 0f);

                    MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
                }
                catch
                {
                    try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                    DrawFallbackAura(sb, drawPos, phase, auraFade);
                }
            }
            else
            {
                DrawFallbackAura(sb, drawPos, phase, auraFade);
            }
        }

        /// <summary>
        /// Particle-based fallback for LunarPhaseAura when shader is unavailable.
        /// </summary>
        private static void DrawFallbackAura(SpriteBatch sb, Vector2 drawPos,
            float phase, float auraFade)
        {
            var glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;
            Vector2 origin = glowTex.Size() * 0.5f;

            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.1f;
            float scale = (0.4f + phase * 0.6f) * pulse;

            sb.Draw(glowTex, drawPos, null,
                (DeepTide with { A = 0 }) * auraFade * 0.1f,
                0f, origin, scale * 2.0f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * auraFade * 0.15f,
                0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * auraFade * 0.1f,
                0f, origin, scale * 0.8f, SpriteEffects.None, 0f);
        }

        // =====================================================================
        //  SWING FRAME VFX (Enhanced for 5-Phase)
        // =====================================================================

        /// <summary>
        /// Per-frame swing VFX with tidal wake recording, custom dusts,
        /// flowing moon particles, and 5-phase cycling effects.
        /// </summary>
        public static void SwingFrameEffects(Vector2 ownerCenter, Vector2 tipPos,
            Vector2 swordDirection, int comboStep, int timer)
        {
            float phase = GetMoonPhase(comboStep);

            // Record tidal wake position at blade midpoint
            Vector2 midBlade = Vector2.Lerp(ownerCenter, tipPos, 0.6f);
            RecordWakePosition(midBlade, swordDirection.ToRotation());

            // TIDAL DUST TRAIL — flowing water-like particles along blade
            int tidalCount = 1 + comboStep;
            for (int i = 0; i < tidalCount; i++)
            {
                float bladeT = Main.rand.NextFloat(0.3f, 1f);
                Vector2 dustPos = Vector2.Lerp(ownerCenter, tipPos, bladeT);
                Vector2 perp = new Vector2(-swordDirection.Y, swordDirection.X);
                dustPos += perp * Main.rand.NextFloat(-4f, 4f);

                Color dustColor = GetLunarPhaseColor(bladeT, comboStep);
                Dust tidal = Dust.NewDustPerfect(dustPos,
                    ModContent.DustType<TidalMoonDust>(),
                    -swordDirection * Main.rand.NextFloat(1.5f, 3.5f) +
                    perp * Main.rand.NextFloat(-1f, 1f),
                    0, dustColor, 0.25f + phase * 0.1f);
                tidal.customData = new TidalMoonBehavior
                {
                    DriftAmplitude = 2f + comboStep,
                    DriftFrequency = 0.18f,
                    VelocityDecay = 0.94f,
                    BaseScale = 0.25f + phase * 0.1f,
                    Lifetime = 25 + comboStep * 5
                };
            }

            // CRESCENT MOTE at blade tip (LunarMote)
            if (timer % (6 - Math.Min(comboStep, 3)) == 0)
            {
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<LunarMote>(),
                    -swordDirection * 0.5f,
                    0, moteColor, 0.3f + phase * 0.1f);
                mote.customData = new LunarMoteBehavior(tipPos, Main.rand.NextFloat(MathHelper.TwoPi))
                {
                    OrbitRadius = 10f + comboStep * 2f,
                    OrbitSpeed = 0.07f + comboStep * 0.005f,
                    Lifetime = 25 + comboStep * 3,
                    FadePower = 0.91f
                };
            }

            // CONTRAST SPARKLE
            if (timer % 2 == 0)
            {
                Vector2 sparkPos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.5f, 1f));
                MoonlightVFXLibrary.SpawnContrastSparkle(sparkPos, -swordDirection);
            }

            // MUSIC NOTES — frequency increases with combo
            int noteInterval = Math.Max(5 - comboStep, 1);
            if (timer % noteInterval == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(midBlade, 1, 5f, 0.65f + phase * 0.2f, 0.85f, 25);
            }

            // PHASE 2+: Tidal foam sparkles along entire blade
            if (comboStep >= 2 && Main.rand.NextBool(3 - Math.Min(comboStep / 2, 1)))
            {
                float foamT = Main.rand.NextFloat(0.3f, 1f);
                Vector2 foamPos = Vector2.Lerp(ownerCenter, tipPos, foamT);
                Color foamColor = Color.Lerp(TidalFoam, MoonlightVFXLibrary.MoonWhite,
                    Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(foamPos,
                    ModContent.DustType<StarPointDust>(),
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, foamColor, 0.2f + comboStep * 0.02f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    Lifetime = 18 + comboStep * 2,
                    FadeStartTime = 5
                };
            }

            // PHASE 3+: ResonantPulseDust expanding rings from blade path
            if (comboStep >= 3 && timer % (6 - Math.Min(comboStep - 3, 2)) == 0)
            {
                Vector2 pulsePos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.5f, 1f));
                Color pulseColor = Color.Lerp(CrescentGlow, TidalFoam, Main.rand.NextFloat(0.4f));
                Dust pulse = Dust.NewDustPerfect(pulsePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor, 0.15f + comboStep * 0.02f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.03f + comboStep * 0.005f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 14 + comboStep * 2,
                    PulseFrequency = 0.25f
                };
            }

            // PHASE 4: Intense tidal spray from blade edge
            if (comboStep >= 4 && Main.rand.NextBool(2))
            {
                float sprayT = Main.rand.NextFloat(0.6f, 1f);
                Vector2 sprayPos = Vector2.Lerp(ownerCenter, tipPos, sprayT);
                Vector2 perp2 = new Vector2(-swordDirection.Y, swordDirection.X);
                Vector2 sprayVel = perp2 * Main.rand.NextFloat(-3f, 3f) - swordDirection * 2f;
                Color sprayColor = Color.Lerp(TidalFoam, CrescentGlow, Main.rand.NextFloat());
                Dust spray = Dust.NewDustPerfect(sprayPos,
                    ModContent.DustType<TidalMoonDust>(),
                    sprayVel, 0, sprayColor, 0.35f);
                spray.customData = new TidalMoonBehavior
                {
                    DriftAmplitude = 4f,
                    DriftFrequency = 0.2f,
                    VelocityDecay = 0.93f,
                    BaseScale = 0.35f,
                    Lifetime = 30
                };
            }
        }

        // =====================================================================
        //  IMPACT VFX (Enhanced for 5-Phase)
        // =====================================================================

        /// <summary>
        /// Enhanced on-hit impact with tidal dust splash, crescent pulse rings,
        /// and eclipse burst on crits. Scales with 5-phase combo system.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int comboStep, bool isCrit)
        {
            float phase = GetMoonPhase(comboStep);

            // Base moonlight impact
            MoonlightVFXLibrary.MeleeImpact(hitPos, comboStep);

            // TIDAL SPLASH — fan of tidal dust outward
            int splashCount = 4 + comboStep * 2;
            for (int i = 0; i < splashCount; i++)
            {
                float angle = MathHelper.TwoPi * i / splashCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color splashColor = GetLunarPhaseColor((float)i / splashCount, comboStep);
                Dust tidal = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, splashColor, 0.3f + phase * 0.15f);
                tidal.customData = new TidalMoonBehavior(3f + comboStep, 20 + comboStep * 3);
            }

            // CRESCENT PULSE RINGS — count scales with phase
            int ringCount = 2 + comboStep;
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = Color.Lerp(DeepTide, TidalFoam, (float)ring / ringCount);
                Dust pulse = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + ring * 0.06f + phase * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.035f + ring * 0.01f,
                    ExpansionDecay = 0.95f,
                    Lifetime = 18 + ring * 4,
                    PulseFrequency = 0.25f
                };
            }

            // Gradient halo rings
            for (int ring = 0; ring < ringCount; ring++)
            {
                float progress = (float)ring / ringCount;
                Color haloColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.IceBlue, progress);
                CustomParticles.HaloRing(hitPos, haloColor, 0.3f + ring * 0.1f, 12 + ring * 2);
            }

            // CRIT: Lunar eclipse burst — dark flash into bright explosion
            if (isCrit)
            {
                CustomParticles.GenericFlare(hitPos, LunarEclipse, 0.8f + phase * 0.2f, 22);
                CustomParticles.GenericFlare(hitPos, MoonlightVFXLibrary.DarkPurple, 0.6f, 20);
                CustomParticles.GenericFlare(hitPos, MoonlightVFXLibrary.Violet, 0.5f, 18);
                CustomParticles.GenericFlare(hitPos, CrescentGlow, 0.35f, 16);
                CustomParticles.GenericFlare(hitPos, Color.White, 0.2f, 14);

                // Constellation burst flare overlay on crit
                CustomParticles.GenericFlare(hitPos, MoonlightVFXLibrary.IceBlue, 0.6f, 20);
                CustomParticles.HaloRing(hitPos, CrescentGlow, 0.5f, 18);

                int rayCount = 5 + comboStep;
                float rayRadius = 40f + comboStep * 5f;
                GodRaySystem.CreateBurst(hitPos, MoonlightVFXLibrary.Violet,
                    rayCount: rayCount, radius: rayRadius, duration: 20 + comboStep * 2,
                    GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: CrescentGlow);

                MagnumScreenEffects.AddScreenShake(2f + phase * 1.5f);
            }
        }

        // =====================================================================
        //  CRESCENDO FINALE (Phase 4 — Full Moon Climax)
        // =====================================================================

        /// <summary>
        /// Phase 4 Crescendo Finale — the full moon rises.
        /// Massive tidal detonation with expanding lunar rings, eclipse flash,
        /// tidal dust cascade, crescent arc burst, god rays, and chromatic aberration.
        /// </summary>
        public static void CrescendoFinaleVFX(Vector2 pos)
        {
            // Base finisher
            MoonlightVFXLibrary.FinisherSlam(pos, 1.5f);

            // TIDAL WAVE CASCADE — 8 expanding pulse rings (increased from 6)
            for (int i = 0; i < 8; i++)
            {
                Color ringColor = Color.Lerp(DeepTide, TidalFoam, (float)i / 8f);
                Dust pulse = Dust.NewDustPerfect(pos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.2f + i * 0.1f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.05f + i * 0.018f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 14 + i * 4,
                    PulseFrequency = 0.2f + i * 0.06f
                };
            }

            // TIDAL DUST STARBURST — 16 flowing tidal motes (increased from 12)
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                Color tidalColor = GetLunarPhaseColor((float)i / 16f, 4);
                Dust tidal = Dust.NewDustPerfect(pos,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, tidalColor, 0.5f);
                tidal.customData = new TidalMoonBehavior
                {
                    DriftAmplitude = 5f,
                    DriftFrequency = 0.15f,
                    VelocityDecay = 0.95f,
                    BaseScale = 0.5f,
                    Lifetime = 40
                };
            }

            // LUNAR MOTE ORBIT — 7 crescent motes spiraling outward
            for (int i = 0; i < 7; i++)
            {
                float angle = MathHelper.TwoPi * i / 7f;
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.MoonWhite, (float)i / 7f);
                Dust mote = Dust.NewDustPerfect(pos + angle.ToRotationVector2() * 10f,
                    ModContent.DustType<LunarMote>(),
                    angle.ToRotationVector2() * 2f,
                    0, moteColor, 0.55f);
                mote.customData = new LunarMoteBehavior(pos, angle)
                {
                    OrbitRadius = 25f + i * 6f,
                    OrbitSpeed = 0.05f,
                    Lifetime = 45,
                    FadePower = 0.93f
                };
            }

            // ECLIPSE FLARE CASCADE — layered from dark to light
            CustomParticles.GenericFlare(pos, LunarEclipse, 1.2f, 28);
            CustomParticles.GenericFlare(pos, DeepTide, 0.9f, 26);
            CustomParticles.GenericFlare(pos, MoonlightVFXLibrary.DarkPurple, 0.75f, 24);
            CustomParticles.GenericFlare(pos, MoonlightVFXLibrary.Violet, 0.65f, 22);
            CustomParticles.GenericFlare(pos, TidalLavender, 0.55f, 20);
            CustomParticles.GenericFlare(pos, MoonlightVFXLibrary.IceBlue, 0.45f, 18);
            CustomParticles.GenericFlare(pos, CrescentGlow, 0.35f, 16);
            CustomParticles.GenericFlare(pos, Color.White, 0.25f, 14);

            // CRESCENT TRIPLE RINGS
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.IceBlue, CrescentGlow, ring / 4f);
                CustomParticles.HaloRing(pos, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }

            // MUSIC NOTE CASCADE — grand finale
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteOffset = angle.ToRotationVector2() * 8f;
                MoonlightVFXLibrary.SpawnMusicNotes(pos + noteOffset, 1, 8f, 0.85f, 1.1f, 40);
            }

            // GOD RAY BURST — massive
            GodRaySystem.CreateBurst(pos, MoonlightVFXLibrary.IceBlue,
                rayCount: 8, radius: 70f, duration: 35,
                GodRaySystem.GodRayStyle.Explosion,
                secondaryColor: CrescentGlow);

            // SCREEN EFFECTS
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(pos, MoonlightVFXLibrary.Violet, 0.8f, 28);
                MagnumScreenEffects.AddScreenShake(7f);
            }

            // CHROMATIC ABERRATION FLASH
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.3f,
                    lifetime: 22,
                    whiteIntensity: 0.7f,
                    distanceMult: 0.5f,
                    moveIn: true);
            }
            catch { }

            // Crescent arc burst
            CustomParticles.SwordArcCrescent(pos, Vector2.UnitY * -1f, MoonlightVFXLibrary.Violet, 0.8f);
            CustomParticles.SwordArcBurst(pos, CrescentGlow, 8, 0.5f);
        }

        // =====================================================================
        //  PHASE TRANSITION VFX
        // =====================================================================

        /// <summary>
        /// Eclipse flash — brief dark-to-light burst when combo advances.
        /// Enhanced for 5-phase transitions with scaling intensity.
        /// </summary>
        public static void PhaseTransitionEclipse(Vector2 playerCenter, int newComboStep)
        {
            float phase = GetMoonPhase(newComboStep);

            // Eclipse shadow ring (dark pulse)
            Dust shadow = Dust.NewDustPerfect(playerCenter,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, LunarEclipse, 0.3f + newComboStep * 0.03f);
            shadow.customData = new ResonantPulseBehavior(0.05f, 12 + newComboStep);

            // Bright burst — intensity scales with phase
            Color transColor = GetLunarPhaseColor(0.5f, newComboStep);
            CustomParticles.GenericFlare(playerCenter, transColor, 0.35f + phase * 0.2f, 15 + newComboStep * 2);

            // Tidal dust burst — count increases with combo
            int splashCount = 3 + newComboStep * 2;
            for (int i = 0; i < splashCount; i++)
            {
                float angle = MathHelper.TwoPi * i / splashCount;
                Vector2 vel = angle.ToRotationVector2() * (2f + newComboStep);
                Color tidalColor = GetLunarPhaseColor((float)i / splashCount, newComboStep);
                Dust tidal = Dust.NewDustPerfect(playerCenter,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, tidalColor, 0.3f + newComboStep * 0.02f);
                tidal.customData = new TidalMoonBehavior(2.5f + newComboStep * 0.3f, 18 + newComboStep * 2);
            }

            // Phase 3+: Additional halo ring on transition
            if (newComboStep >= 3)
            {
                CustomParticles.HaloRing(playerCenter,
                    Color.Lerp(MoonlightVFXLibrary.Violet, CrescentGlow, phase),
                    0.3f + phase * 0.15f, 14 + newComboStep * 2);
            }

            // Phase 4: Eclipse detonation flash — massive
            if (newComboStep >= 4)
            {
                CustomParticles.GenericFlare(playerCenter, LunarEclipse, 0.6f, 20);
                CustomParticles.GenericFlare(playerCenter, Color.White, 0.25f, 14);

                GodRaySystem.CreateBurst(playerCenter, MoonlightVFXLibrary.Violet,
                    rayCount: 4, radius: 30f, duration: 14,
                    GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: CrescentGlow);
            }

            // Music notes for the beat
            MoonlightVFXLibrary.SpawnMusicNotes(playerCenter, 2, 8f, 0.7f + phase * 0.2f, 1f, 30);
        }

        // =====================================================================
        //  TRAIL FUNCTIONS
        // =====================================================================

        /// <summary>
        /// Trail color for EternalMoon projectiles — flowing tidal gradient.
        /// </summary>
        public static Color WaveTrailColor(float progress)
        {
            float wave = MathF.Abs(MathF.Sin(progress * MathF.PI * 3f + Main.GlobalTimeWrappedHourly * 4f));
            Color c = GetLunarPhaseColor(progress, 0);
            float brightness = 0.7f + wave * 0.3f;
            return (c * (brightness - progress * 0.5f)) with { A = 0 };
        }

        /// <summary>
        /// Trail width — crescent-shaped taper for tidal wave feel.
        /// </summary>
        public static float WaveTrailWidth(float progress)
        {
            float ramp = MathHelper.Clamp(progress * 5f, 0f, 1f);
            float fade = MathHelper.Clamp((1f - progress) * 3f, 0f, 1f);
            float ripple = 1f + MathF.Sin(progress * MathF.PI * 4f) * 0.06f;
            return ramp * fade * 22f * ripple;
        }

        /// <summary>
        /// Dynamic lighting — pulsing violet with tidal rhythm.
        /// </summary>
        public static void AddCrescentLight(Vector2 worldPos, float intensity = 0.8f)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.15f;
            Vector3 lightColor = MoonlightVFXLibrary.Violet.ToVector3() * intensity * pulse;
            Lighting.AddLight(worldPos, lightColor);
        }
    }
}
