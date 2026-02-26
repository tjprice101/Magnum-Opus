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

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight
{
    /// <summary>
    /// Overhauled VFX for Incisor of Moonlight — "The Stellar Scalpel".
    /// 5-phase Surgical Precision visual pipeline.
    ///
    /// Visual Identity: Surgical precision meets celestial resonance.
    /// - Constellation trail: Blade tip positions form connected star patterns
    /// - ConstellationField.fx shader-driven starfield overlay (phases 2+)
    /// - IncisorResonance.fx shader-driven trail glow (phases 1+)
    /// - Resonant frequency waves: Perpendicular oscillation particles that intensify with combo
    /// - Standing wave nodes: Bright points at harmonic intervals along the blade
    /// - Afterimage cascade: Ghosted blade positions on Phase 2+ for motion blur feel
    /// - Custom dust types: LunarMote, StarPointDust, ResonantPulseDust
    /// - Screen effects: Chromatic aberration flash + screen distortion on Stellar Crescendo
    ///
    /// Inspired by:
    /// - VFX+ SandboxLastPrism: Multi-layer dust rendering, screen flash system
    /// - Calamity Exoblade: Dual trail systems, lens flares, dynamic dust RiskOfDust
    /// - Coralite Thyphion: Multi-phase escalation, angular momentum VFX
    /// </summary>
    public static class IncisorOfMoonlightVFX
    {
        // === UNIQUE ACCENT COLORS ===
        public static readonly Color ResonantSilver = new Color(230, 235, 255);
        public static readonly Color FrequencyPulse = new Color(170, 140, 255);
        public static readonly Color HarmonicWhite = new Color(235, 240, 255);
        public static readonly Color DeepResonance = new Color(90, 50, 160);
        public static readonly Color ConstellationBlue = new Color(160, 200, 255);
        public static readonly Color CrescendoBright = new Color(150, 210, 255);

        // === CONSTELLATION TRAIL STORAGE ===
        // Tracks blade tip positions to draw connecting constellation lines
        private static readonly Vector2[] _constellationNodes = new Vector2[12];
        private static int _nodeWriteIndex;
        private static int _nodeCount;
        private static int _constellationTimer;

        // === AFTERIMAGE STORAGE ===
        private static readonly Vector2[] _afterimageTips = new Vector2[6];
        private static readonly Vector2[] _afterimagePommels = new Vector2[6];
        private static readonly float[] _afterimageRotations = new float[6];
        private static int _afterimageWriteIndex;
        private static int _afterimageTimer;

        // === 5-PHASE RESONANCE MAPPING ===
        private static float GetResonanceLevel(int comboStep)
        {
            return comboStep switch
            {
                0 => 0.20f,  // Precise Incision — subtle
                1 => 0.40f,  // Crescent Cut
                2 => 0.60f,  // Constellation Mapping
                3 => 0.80f,  // Harmonic Surge
                4 => 1.00f,  // Stellar Crescendo
                _ => 0.5f
            };
        }

        /// <summary>
        /// Resonant frequency color with combo escalation.
        /// Phase 0: Cool silver. Phase 1: Warming violet. Phase 2: Ice-blue.
        /// Phase 3: Harmonic white. Phase 4: Blazing constellation bright.
        /// </summary>
        public static Color GetResonanceColor(float progress, int comboStep)
        {
            float resonance = MathHelper.Clamp(comboStep / 4f, 0f, 1f);
            Color cold = Color.Lerp(ResonantSilver, MoonlightVFXLibrary.IceBlue, progress);
            Color hot = Color.Lerp(FrequencyPulse, HarmonicWhite, progress);
            return Color.Lerp(cold, hot, resonance);
        }

        /// <summary>
        /// Resets constellation and afterimage tracking. Call when a new swing begins.
        /// </summary>
        public static void ResetSwingTracking()
        {
            _nodeCount = 0;
            _nodeWriteIndex = 0;
            _constellationTimer = 0;
            _afterimageWriteIndex = 0;
            _afterimageTimer = 0;
            Array.Clear(_constellationNodes, 0, _constellationNodes.Length);
            Array.Clear(_afterimageTips, 0, _afterimageTips.Length);
            Array.Clear(_afterimagePommels, 0, _afterimagePommels.Length);
        }

        // =====================================================================
        //  CONSTELLATION TRAIL SYSTEM
        // =====================================================================

        /// <summary>
        /// Records a constellation node at the blade tip position.
        /// Called every few frames during swing to build the constellation pattern.
        /// </summary>
        public static void RecordConstellationNode(Vector2 tipPos)
        {
            _constellationTimer++;
            if (_constellationTimer % 4 != 0) return;

            _constellationNodes[_nodeWriteIndex % _constellationNodes.Length] = tipPos;
            _nodeWriteIndex++;
            _nodeCount = Math.Min(_nodeCount + 1, _constellationNodes.Length);
        }

        /// <summary>
        /// Draws the constellation trail — faint connecting lines between blade tip positions
        /// with bright star nodes at each point. Creates a "star map traced by sword" effect.
        /// </summary>
        public static void DrawConstellationTrail(SpriteBatch sb, int comboStep, float progression)
        {
            if (sb == null || _nodeCount < 2) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;

            float resonance = GetResonanceLevel(comboStep);
            float fadeMultiplier = MathHelper.Clamp(progression * 3f, 0f, 1f)
                                 * MathHelper.Clamp((1f - progression) * 4f, 0f, 1f);

            int start = Math.Max(0, _nodeWriteIndex - _nodeCount);

            for (int i = start; i < _nodeWriteIndex; i++)
            {
                int idx = i % _constellationNodes.Length;
                Vector2 nodeScreen = _constellationNodes[idx] - Main.screenPosition;
                if (nodeScreen == -Main.screenPosition) continue;

                float age = (float)(_nodeWriteIndex - i) / _nodeCount;
                float nodeAlpha = (1f - age) * fadeMultiplier * resonance;

                // Star node glow
                float twinkle = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + i * 1.7f) * 0.2f;
                float nodeScale = (0.08f + (1f - age) * 0.06f) * twinkle;

                Color nodeColor = Color.Lerp(ConstellationBlue, HarmonicWhite, 1f - age);

                sb.Draw(bloomTex, nodeScreen, null,
                    (nodeColor with { A = 0 }) * nodeAlpha * 0.6f,
                    0f, bloomOrigin, nodeScale * 1.5f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, nodeScreen, null,
                    (Color.White with { A = 0 }) * nodeAlpha * 0.4f,
                    0f, bloomOrigin, nodeScale * 0.5f, SpriteEffects.None, 0f);

                // 4-pointed star at each constellation node
                var starTex = MoonlightSonataTextures.Star4Point?.Value;
                if (starTex != null)
                {
                    Vector2 starOrig = starTex.Size() * 0.5f;
                    float starRot = Main.GlobalTimeWrappedHourly * 2f + i * 1.2f;
                    sb.Draw(starTex, nodeScreen, null,
                        (ConstellationBlue with { A = 0 }) * nodeAlpha * 0.7f,
                        starRot, starOrig, nodeScale * 0.4f, SpriteEffects.None, 0f);
                }

                // Constellation burst at the newest node
                if (i == _nodeWriteIndex - 1)
                {
                    var burstTex = MoonlightSonataTextures.ConstellationBurstFlare?.Value;
                    if (burstTex != null)
                    {
                        Vector2 burstOrig = burstTex.Size() * 0.5f;
                        float burstRot = Main.GlobalTimeWrappedHourly * 0.5f;
                        sb.Draw(burstTex, nodeScreen, null,
                            (HarmonicWhite with { A = 0 }) * nodeAlpha * 0.15f,
                            burstRot, burstOrig, nodeScale * 0.18f, SpriteEffects.None, 0f);
                    }
                }

                // Connecting line to next node
                if (i > start)
                {
                    int prevIdx = (i - 1) % _constellationNodes.Length;
                    Vector2 prevScreen = _constellationNodes[prevIdx] - Main.screenPosition;
                    if (prevScreen == -Main.screenPosition) continue;

                    float lineDist = Vector2.Distance(nodeScreen, prevScreen);
                    if (lineDist < 2f || lineDist > 200f) continue;

                    float lineAngle = (nodeScreen - prevScreen).ToRotation();
                    Vector2 lineCenter = (nodeScreen + prevScreen) / 2f;
                    float lineAlpha = nodeAlpha * 0.25f;

                    sb.Draw(bloomTex, lineCenter, null,
                        (ConstellationBlue with { A = 0 }) * lineAlpha,
                        lineAngle, bloomOrigin,
                        new Vector2(lineDist / bloomTex.Width, 0.012f),
                        SpriteEffects.None, 0f);
                }
            }
        }

        // =====================================================================
        //  CONSTELLATION FIELD OVERLAY (Shader-driven, Phases 2+)
        // =====================================================================

        /// <summary>
        /// Draws the ConstellationField.fx shader overlay between player center and blade tip.
        /// Creates a parallax starfield that the blade appears to slice through.
        /// Falls back to particle-based constellation dots if shader is unavailable.
        /// </summary>
        public static void DrawConstellationFieldOverlay(SpriteBatch sb, Vector2 playerCenter,
            Vector2 tipPos, int comboStep, float progression)
        {
            if (sb == null || comboStep < 2) return;

            float resonance = GetResonanceLevel(comboStep);
            float fieldFade = MathHelper.Clamp((progression - 0.10f) / 0.15f, 0f, 1f)
                            * MathHelper.Clamp((0.90f - progression) / 0.12f, 0f, 1f);

            if (fieldFade < 0.05f) return;

            if (MoonlightSonataShaderManager.HasConstellationField)
            {
                DrawShaderConstellationField(sb, playerCenter, tipPos, comboStep, resonance, fieldFade);
            }
            else
            {
                DrawFallbackConstellationField(sb, playerCenter, tipPos, comboStep, resonance, fieldFade);
            }
        }

        /// <summary>
        /// Shader-driven constellation field using ConstellationField.fx.
        /// Renders a starfield overlay on a soft glow quad centered between
        /// player and blade tip.
        /// </summary>
        private static void DrawShaderConstellationField(SpriteBatch sb, Vector2 playerCenter,
            Vector2 tipPos, int comboStep, float resonance, float fieldFade)
        {
            var fieldTex = MoonlightSonataTextures.IncisorTrail?.Value
                        ?? MagnumTextureRegistry.GetSoftGlow();
            if (fieldTex == null) return;

            Vector2 center = (playerCenter + tipPos) * 0.5f;
            Vector2 drawPos = center - Main.screenPosition;
            Vector2 origin = fieldTex.Size() * 0.5f;

            float bladeLen = Vector2.Distance(playerCenter, tipPos);
            float fieldScale = bladeLen / fieldTex.Width * 1.2f;
            float rotation = (tipPos - playerCenter).ToRotation();

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                MoonlightSonataShaderManager.ApplyIncisorConstellationField(
                    Main.GlobalTimeWrappedHourly, resonance, comboStep > 1);

                sb.Draw(fieldTex, drawPos, null,
                    Color.White * fieldFade * 0.7f, rotation, origin,
                    new Vector2(fieldScale, fieldScale * 0.6f),
                    SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                DrawFallbackConstellationField(sb, playerCenter, tipPos, comboStep, resonance, fieldFade);
            }
        }

        /// <summary>
        /// Particle-based fallback for ConstellationField when shader is unavailable.
        /// Draws scattered star bloom dots along the swing arc.
        /// </summary>
        private static void DrawFallbackConstellationField(SpriteBatch sb, Vector2 playerCenter,
            Vector2 tipPos, int comboStep, float resonance, float fieldFade)
        {
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Scatter star dots along the blade arc area
            int starCount = 4 + (comboStep - 2) * 3;
            float time = Main.GlobalTimeWrappedHourly;

            for (int i = 0; i < starCount; i++)
            {
                float t = (float)i / starCount;

                // Pseudo-random deterministic offset using hash
                float hash1 = MathF.Abs(MathF.Sin(i * 127.1f + 311.7f)) % 1f;
                float hash2 = MathF.Abs(MathF.Sin(i * 269.5f + 183.3f)) % 1f;

                Vector2 bladePos = Vector2.Lerp(playerCenter, tipPos, 0.3f + t * 0.7f);
                Vector2 perp = new Vector2(
                    -(tipPos.Y - playerCenter.Y),
                    tipPos.X - playerCenter.X);
                perp = Vector2.Normalize(perp) * (hash2 - 0.5f) * 40f;
                Vector2 starPos = bladePos + perp - Main.screenPosition;

                float twinkle = 0.5f + 0.5f * MathF.Sin(time * (3f + hash1 * 4f) + hash2 * 6.28f);
                float starAlpha = fieldFade * resonance * twinkle * 0.5f;
                float starScale = 0.04f + hash1 * 0.04f;

                Color starColor = Color.Lerp(ConstellationBlue, HarmonicWhite, hash1);

                sb.Draw(bloomTex, starPos, null,
                    (starColor with { A = 0 }) * starAlpha,
                    0f, origin, starScale, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, starPos, null,
                    (Color.White with { A = 0 }) * starAlpha * 0.6f,
                    0f, origin, starScale * 0.4f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  AFTERIMAGE SYSTEM (Phase 2+)
        // =====================================================================

        /// <summary>
        /// Records an afterimage frame. Call every few frames during Phase 2+ swings.
        /// </summary>
        public static void RecordAfterimage(Vector2 pommelPos, Vector2 tipPos, float rotation)
        {
            _afterimageTimer++;
            if (_afterimageTimer % 3 != 0) return;

            int idx = _afterimageWriteIndex % _afterimageTips.Length;
            _afterimageTips[idx] = tipPos;
            _afterimagePommels[idx] = pommelPos;
            _afterimageRotations[idx] = rotation;
            _afterimageWriteIndex++;
        }

        /// <summary>
        /// Draws ghosted blade afterimages from previous positions.
        /// Creates motion-blur-like effect showing the swing path.
        /// </summary>
        public static void DrawAfterimages(SpriteBatch sb, Texture2D bladeTex, float baseScale)
        {
            if (sb == null || bladeTex == null || _afterimageWriteIndex < 2) return;

            Vector2 bladeOrigin = new Vector2(0, bladeTex.Height / 2f);
            int count = Math.Min(_afterimageWriteIndex, _afterimageTips.Length);

            for (int i = 0; i < count; i++)
            {
                int idx = (_afterimageWriteIndex - 1 - i) % _afterimageTips.Length;
                Vector2 tip = _afterimageTips[idx];
                Vector2 pommel = _afterimagePommels[idx];

                if (tip == Vector2.Zero || pommel == Vector2.Zero) continue;

                float age = (float)(i + 1) / count;
                float alpha = (1f - age) * 0.3f;
                float rot = _afterimageRotations[idx];

                Color ghostColor = Color.Lerp(FrequencyPulse, DeepResonance, age);
                Vector2 drawPos = pommel - Main.screenPosition;

                sb.Draw(bladeTex, drawPos, null,
                    (ghostColor with { A = 0 }) * alpha,
                    rot, bladeOrigin, baseScale * (1f - age * 0.1f),
                    SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  RESONANT EDGE BLOOM (Enhanced)
        // =====================================================================

        /// <summary>
        /// Sharp constellation-node bloom along the blade edge with standing wave pattern.
        /// Nodes pulse at harmonic frequencies — brighter at "antinode" positions.
        /// Enhanced with custom StarPointDust spawning at bloom positions.
        /// </summary>
        public static void DrawResonantEdgeBloom(SpriteBatch sb, Vector2 pommelPos,
            Vector2 tipPos, int comboStep, float progression)
        {
            if (sb == null) return;

            float intensityRamp = MathHelper.Clamp(progression * 4f, 0f, 1f)
                                * MathHelper.Clamp((1f - progression) * 3f, 0f, 1f);
            if (intensityRamp < 0.05f) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            float resonance = GetResonanceLevel(comboStep);
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 12f) * 0.08f;

            // Standing wave frequency increases with combo step
            float standingWaveFreq = 3f + comboStep * 1.5f;

            int pointCount = 5 + comboStep * 2;
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)(i + 1) / (pointCount + 1);
                Vector2 worldPos = Vector2.Lerp(pommelPos, tipPos, t);
                Vector2 drawPos = worldPos - Main.screenPosition;

                // Standing wave modulation — creates visible "frequency" nodes
                float standingWave = MathF.Abs(MathF.Sin(t * MathF.PI * standingWaveFreq + time * 4f));
                float pointIntensity = 0.4f + standingWave * 0.6f;

                float pointScale = (0.10f + t * 0.16f) * resonance * pulse * intensityRamp * pointIntensity;

                // 4-layer bloom with standing wave brightness
                sb.Draw(bloomTex, drawPos, null,
                    (DeepResonance with { A = 0 }) * 0.25f * intensityRamp * pointIntensity,
                    0f, origin, pointScale * 2.2f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (FrequencyPulse with { A = 0 }) * 0.45f * intensityRamp * pointIntensity,
                    0f, origin, pointScale * 1.4f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.6f * intensityRamp * pointIntensity,
                    0f, origin, pointScale * 0.85f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, drawPos, null,
                    (HarmonicWhite with { A = 0 }) * 0.75f * intensityRamp * pointIntensity,
                    0f, origin, pointScale * 0.35f, SpriteEffects.None, 0f);

                // Spawn StarPointDust at antinode positions (standing wave peaks)
                if (standingWave > 0.85f && Main.rand.NextBool(4))
                {
                    Color starColor = GetResonanceColor(t, comboStep);
                    Dust star = Dust.NewDustPerfect(worldPos,
                        ModContent.DustType<StarPointDust>(),
                        Main.rand.NextVector2Circular(1f, 1f),
                        0, starColor, 0.4f * resonance);
                    star.customData = new StarPointBehavior(0.15f, 20);
                }
            }

            // Brightest bloom at blade tip with enhanced scale
            Vector2 tipScreen = tipPos - Main.screenPosition;
            float tipScale = 0.35f * resonance * pulse * intensityRamp;
            sb.Draw(bloomTex, tipScreen, null,
                (DeepResonance with { A = 0 }) * 0.35f * intensityRamp,
                0f, origin, tipScale * 2.5f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null,
                (FrequencyPulse with { A = 0 }) * 0.55f * intensityRamp,
                0f, origin, tipScale * 1.6f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null,
                (ResonantSilver with { A = 0 }) * 0.7f * intensityRamp,
                0f, origin, tipScale * 1.0f, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, tipScreen, null,
                (HarmonicWhite with { A = 0 }) * 0.85f * intensityRamp,
                0f, origin, tipScale * 0.4f, SpriteEffects.None, 0f);

            // Tuning fork resonance flare at blade tip — Incisor identity
            var forkTex = MoonlightSonataTextures.TuningForkFlare?.Value;
            if (forkTex != null)
            {
                Vector2 forkOrigin = forkTex.Size() * 0.5f;
                float forkPulse = 0.8f + MathF.Sin(time * 6f) * 0.2f;
                sb.Draw(forkTex, tipScreen, null,
                    (FrequencyPulse with { A = 0 }) * 0.25f * intensityRamp * forkPulse,
                    0f, forkOrigin, tipScale * 0.35f, SpriteEffects.None, 0f);
            }

            // Bloom orb ring around blade tip — resonance halo
            var orbTex = MoonlightSonataTextures.BloomOrb?.Value;
            if (orbTex != null)
            {
                Vector2 orbOrigin = orbTex.Size() * 0.5f;
                sb.Draw(orbTex, tipScreen, null,
                    (ConstellationBlue with { A = 0 }) * 0.2f * intensityRamp,
                    0f, orbOrigin, tipScale * 0.65f, SpriteEffects.None, 0f);
            }

            // 4-pointed star accent at tip
            var starTipTex = MoonlightSonataTextures.Star4Point?.Value;
            if (starTipTex != null)
            {
                Vector2 starTipOrigin = starTipTex.Size() * 0.5f;
                float starTipRot = time * 3f;
                sb.Draw(starTipTex, tipScreen, null,
                    (HarmonicWhite with { A = 0 }) * 0.6f * intensityRamp,
                    starTipRot, starTipOrigin, tipScale * 0.3f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  SWING FRAME VFX (Enhanced for 5-Phase)
        // =====================================================================

        /// <summary>
        /// Per-frame swing effects with custom dusts, resonant frequency oscillation,
        /// constellation node recording, and afterimage tracking.
        /// Scales across all 5 phases of Surgical Precision.
        /// </summary>
        public static void SwingFrameEffects(Vector2 ownerCenter, Vector2 tipPos,
            Vector2 swordDirection, int comboStep, int timer)
        {
            float resonance = GetResonanceLevel(comboStep);

            // Record constellation trail node
            RecordConstellationNode(tipPos);

            // RESONANT FREQUENCY OSCILLATION — perpendicular vibrating particles
            // Amplitude increases with combo step (tuning fork building energy)
            float oscillationAmp = 3f + comboStep * 4f;
            int sparkCount = 1 + comboStep;
            Vector2 perp = new Vector2(-swordDirection.Y, swordDirection.X);

            for (int i = 0; i < sparkCount; i++)
            {
                float bladeT = Main.rand.NextFloat(0.4f, 1f);
                Vector2 bladePos = Vector2.Lerp(ownerCenter, tipPos, bladeT);

                // Oscillating perpendicular displacement
                float oscOffset = MathF.Sin(timer * 0.6f + bladeT * 8f + i * 2f) * oscillationAmp;
                Vector2 sparkPos = bladePos + perp * oscOffset;

                Color sparkColor = GetResonanceColor(bladeT, comboStep);
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.MagicMirror,
                    -swordDirection * Main.rand.NextFloat(2f, 4f) + perp * oscOffset * 0.1f,
                    0, sparkColor, 1.1f + resonance * 0.4f);
                d.noGravity = true;
            }

            // LUNAR MOTE dust at blade tip (custom dust)
            if (timer % 6 == 0)
            {
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<LunarMote>(),
                    -swordDirection * 0.5f,
                    0, moteColor, 0.35f + resonance * 0.15f);
                mote.customData = new LunarMoteBehavior(tipPos, Main.rand.NextFloat(MathHelper.TwoPi))
                {
                    OrbitRadius = 8f,
                    OrbitSpeed = 0.08f,
                    Lifetime = 30,
                    FadePower = 0.92f
                };
            }

            // STAR POINT DUST along blade (custom dust) — precision sparkle
            if (timer % 4 == 0)
            {
                float starT = Main.rand.NextFloat(0.5f, 1f);
                Vector2 starPos = Vector2.Lerp(ownerCenter, tipPos, starT);
                Color starColor = GetResonanceColor(starT, comboStep);
                Dust star = Dust.NewDustPerfect(starPos,
                    ModContent.DustType<StarPointDust>(),
                    perp * Main.rand.NextFloat(-2f, 2f),
                    0, starColor, 0.3f + resonance * 0.2f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.18f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 25,
                    FadeStartTime = 6
                };
            }

            // Contrasting silver sparkle
            if (timer % 3 == 0)
            {
                Vector2 sparklePos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.6f, 1f));
                MoonlightVFXLibrary.SpawnContrastSparkle(sparklePos, -swordDirection);
            }

            // Music notes — frequency increases with combo
            int noteInterval = Math.Max(6 - comboStep, 2);
            if (timer % noteInterval == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(tipPos, 1, 4f, 0.65f + resonance * 0.2f, 0.85f, 25);
            }

            // RESONANT PULSE RING at tip every 10 frames on Phase 2+
            if (comboStep >= 2 && timer % 10 == 0)
            {
                Color pulseColor = Color.Lerp(FrequencyPulse, HarmonicWhite, Main.rand.NextFloat(0.3f));
                Dust pulse = Dust.NewDustPerfect(tipPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor, 0.2f + resonance * 0.1f);
                pulse.customData = new ResonantPulseBehavior(0.03f, 20);
            }

            // Prismatic sparkles on Phase 3+
            if (comboStep >= 3 && Main.rand.NextBool(3))
            {
                Vector2 sparklePos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.3f, 1f));
                CustomParticles.PrismaticSparkle(sparklePos,
                    GetResonanceColor(Main.rand.NextFloat(), comboStep), 0.25f);
            }

            // Phase 3+: ResonantPulseDust expanding rings from blade path
            if (comboStep >= 3 && timer % (8 - Math.Min(comboStep - 3, 2)) == 0)
            {
                Vector2 pulsePos = Vector2.Lerp(ownerCenter, tipPos, Main.rand.NextFloat(0.5f, 1f));
                Color ringColor = Color.Lerp(FrequencyPulse, HarmonicWhite, Main.rand.NextFloat(0.4f));
                Dust ring = Dust.NewDustPerfect(pulsePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor, 0.12f + comboStep * 0.02f);
                ring.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.03f + comboStep * 0.005f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 14 + comboStep * 2,
                    PulseFrequency = 0.25f
                };
            }

            // Phase 4: Intense star cascade from blade edge
            if (comboStep >= 4 && Main.rand.NextBool(2))
            {
                float sprayT = Main.rand.NextFloat(0.5f, 1f);
                Vector2 sprayPos = Vector2.Lerp(ownerCenter, tipPos, sprayT);
                Vector2 sprayVel = perp * Main.rand.NextFloat(-3f, 3f) - swordDirection * 2f;
                Color sprayColor = Color.Lerp(ConstellationBlue, HarmonicWhite, Main.rand.NextFloat());
                Dust spray = Dust.NewDustPerfect(sprayPos,
                    ModContent.DustType<StarPointDust>(),
                    sprayVel, 0, sprayColor, 0.4f);
                spray.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.2f,
                    Lifetime = 25,
                    FadeStartTime = 6,
                    VelocityDecay = 0.94f
                };
            }
        }

        // =====================================================================
        //  IMPACT VFX (Enhanced for 5-Phase)
        // =====================================================================

        /// <summary>
        /// Enhanced on-hit impact with custom dusts, resonant shockwaves,
        /// and frequency-dependent burst patterns. Scales across 5 phases.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int comboStep, bool isCrit)
        {
            float resonance = GetResonanceLevel(comboStep);

            // Base moonlight impact
            MoonlightVFXLibrary.MeleeImpact(hitPos, comboStep);

            // RESONANT PULSE RINGS — expanding tuning-fork vibration rings
            int ringCount = 2 + comboStep;
            for (int i = 0; i < ringCount; i++)
            {
                Color ringColor = Color.Lerp(DeepResonance, HarmonicWhite, (float)i / ringCount);
                Dust pulse = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.3f + i * 0.08f + resonance * 0.1f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f + i * 0.015f,
                    ExpansionDecay = 0.95f,
                    Lifetime = 20 + i * 5,
                    PulseFrequency = 0.3f + i * 0.1f
                };
            }

            // FREQUENCY BURST — 4-directional perpendicular spark lines
            float hitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            int burstDirections = 2 + comboStep;
            for (int dir = 0; dir < burstDirections; dir++)
            {
                float dirAngle = hitAngle + MathHelper.TwoPi * dir / burstDirections;
                Vector2 lineDir = dirAngle.ToRotationVector2();

                for (int j = 0; j < 3 + comboStep; j++)
                {
                    Vector2 sparkPos = hitPos + lineDir * (8f + j * 7f);
                    Color sparkCol = GetResonanceColor((float)j / (3 + comboStep), comboStep);
                    Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                        lineDir * (2f + j * 0.8f), 0, sparkCol, 1.0f + resonance * 0.3f);
                    d.noGravity = true;
                }
            }

            // STAR POINT DUST burst at impact
            for (int i = 0; i < 3 + comboStep; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color starColor = GetResonanceColor(Main.rand.NextFloat(), comboStep);
                Dust star = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.4f + resonance * 0.2f);
                star.customData = new StarPointBehavior(0.15f, 25);
            }

            // Gradient halo rings
            for (int ring = 0; ring < 2 + comboStep; ring++)
            {
                float progress = (float)ring / (2 + comboStep);
                Color haloColor = Color.Lerp(DeepResonance, ResonantSilver, progress);
                CustomParticles.HaloRing(hitPos, haloColor, 0.3f + ring * 0.1f, 12 + ring * 2);
            }

            // CRIT: Harmonic resonance burst with god rays + screen flash
            if (isCrit)
            {
                // 5-layer bloom flare cascade
                CustomParticles.GenericFlare(hitPos, DeepResonance, 0.9f, 24);
                CustomParticles.GenericFlare(hitPos, FrequencyPulse, 0.7f, 22);
                CustomParticles.GenericFlare(hitPos, ResonantSilver, 0.5f, 20);
                CustomParticles.GenericFlare(hitPos, ConstellationBlue, 0.35f, 18);
                CustomParticles.GenericFlare(hitPos, HarmonicWhite, 0.25f, 16);

                // God ray burst — 6 rays with secondary FrequencyPulse
                GodRaySystem.CreateBurst(hitPos, MoonlightVFXLibrary.IceBlue,
                    rayCount: 6, radius: 40f, duration: 20, GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: FrequencyPulse);

                // Screen shake on crit
                MagnumScreenEffects.AddScreenShake(2f + resonance * 1.5f);
            }
        }

        // =====================================================================
        //  CRESCENDO FINISHER (Phase 4 — Stellar Crescendo)
        // =====================================================================

        /// <summary>
        /// Phase 4 Stellar Crescendo finisher — the night sky trembles.
        /// Full lunar detonation with screen distortion, chromatic aberration flash,
        /// constellation starburst, resonance cascade, and god ray explosion.
        /// </summary>
        public static void CrescendoFinisherVFX(Vector2 pos)
        {
            // Use shared finisher as base (screen shake + massive bloom + notes)
            MoonlightVFXLibrary.FinisherSlam(pos, 1.6f);

            // RESONANT FREQUENCY CASCADE — 8 expanding concentric pulse rings
            for (int i = 0; i < 8; i++)
            {
                Color ringColor = Color.Lerp(DeepResonance, HarmonicWhite, (float)i / 8f);
                Dust pulse = Dust.NewDustPerfect(pos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + i * 0.12f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.06f + i * 0.02f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 15 + i * 4,
                    PulseFrequency = 0.2f + i * 0.08f
                };
            }

            // CONSTELLATION STARBURST — 16 star fragments spiraling outward
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                Color starColor = Color.Lerp(ConstellationBlue, HarmonicWhite, (float)i / 16f);
                Dust star = Dust.NewDustPerfect(pos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.65f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.2f,
                    Lifetime = 40,
                    FadeStartTime = 10,
                    VelocityDecay = 0.95f
                };
            }

            // LUNAR MOTE ORBIT — 6 crescent motes spiraling outward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                    MoonlightVFXLibrary.MoonWhite, (float)i / 6f);
                Dust mote = Dust.NewDustPerfect(pos + angle.ToRotationVector2() * 10f,
                    ModContent.DustType<LunarMote>(),
                    angle.ToRotationVector2() * 2f,
                    0, moteColor, 0.5f);
                mote.customData = new LunarMoteBehavior(pos, angle)
                {
                    OrbitRadius = 30f + i * 5f,
                    OrbitSpeed = 0.05f,
                    Lifetime = 45,
                    FadePower = 0.93f
                };
            }

            // MUSIC NOTE STARBURST — 10 notes spiraling outward in a ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteOffset = angle.ToRotationVector2() * 8f;
                MoonlightVFXLibrary.SpawnMusicNotes(pos + noteOffset, 1, 7f, 0.9f, 1.1f, 40);
            }

            // BLOOM FLARE MEGA-CASCADE
            CustomParticles.GenericFlare(pos, DeepResonance, 1.3f, 30);
            CustomParticles.GenericFlare(pos, FrequencyPulse, 1.0f, 26);
            CustomParticles.GenericFlare(pos, ResonantSilver, 0.75f, 23);
            CustomParticles.GenericFlare(pos, ConstellationBlue, 0.55f, 20);
            CustomParticles.GenericFlare(pos, CrescendoBright, 0.4f, 18);
            CustomParticles.GenericFlare(pos, HarmonicWhite, 0.3f, 16);

            // CONSTELLATION HALO RINGS
            for (int ring = 0; ring < 4; ring++)
            {
                Color ringColor = Color.Lerp(ConstellationBlue, HarmonicWhite, ring / 4f);
                CustomParticles.HaloRing(pos, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }

            // GOD RAY BURST — 10-ray starburst with pulsing style
            GodRaySystem.CreateBurst(pos, MoonlightVFXLibrary.IceBlue,
                rayCount: 10, radius: 70f, duration: 35,
                GodRaySystem.GodRayStyle.Explosion,
                secondaryColor: FrequencyPulse);

            // SCREEN EFFECTS
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(pos, FrequencyPulse, 0.9f, 28);
                MagnumScreenEffects.AddScreenShake(8f);
            }

            // CHROMATIC ABERRATION FLASH (via SLP flash system)
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.40f,
                    lifetime: 22,
                    whiteIntensity: 0.85f,
                    distanceMult: 0.6f,
                    moveIn: true);
            }
            catch { }

            // Sword arc crescent burst
            CustomParticles.SwordArcCrescent(pos, Vector2.UnitY * -1f, FrequencyPulse, 0.9f);
            CustomParticles.SwordArcBurst(pos, ConstellationBlue, 10, 0.55f);
        }

        // =====================================================================
        //  PHASE TRANSITION VFX
        // =====================================================================

        /// <summary>
        /// Called when the combo advances to the next phase.
        /// Visual feedback for the resonance building across 5 phases.
        /// </summary>
        public static void PhaseTransitionBurst(Vector2 playerCenter, int newComboStep)
        {
            float resonance = GetResonanceLevel(newComboStep);

            // Phase transition ring
            Color transColor = GetResonanceColor(0.5f, newComboStep);
            Dust pulse = Dust.NewDustPerfect(playerCenter,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, transColor, 0.3f + newComboStep * 0.02f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 18 + newComboStep);

            // Star point burst pattern — more stars at higher phases
            int starCount = 3 + newComboStep * 2;
            for (int i = 0; i < starCount; i++)
            {
                float angle = MathHelper.TwoPi * i / starCount;
                Vector2 vel = angle.ToRotationVector2() * (2f + newComboStep);
                Color starColor = GetResonanceColor((float)i / starCount, newComboStep);
                Dust star = Dust.NewDustPerfect(playerCenter,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.35f + newComboStep * 0.02f);
                star.customData = new StarPointBehavior(0.12f, 20 + newComboStep * 2);
            }

            // Flare at player center
            CustomParticles.GenericFlare(playerCenter, transColor, 0.4f + resonance * 0.2f, 15 + newComboStep * 2);

            // Music note to mark the beat
            MoonlightVFXLibrary.SpawnMusicNotes(playerCenter, 2, 8f, 0.7f + resonance * 0.2f, 1f, 30);

            // Phase 3+: Additional halo ring with constellation accent
            if (newComboStep >= 3)
            {
                CustomParticles.HaloRing(playerCenter,
                    Color.Lerp(ConstellationBlue, HarmonicWhite, resonance),
                    0.3f + resonance * 0.15f, 14 + newComboStep * 2);

                // Deeper resonance pulse
                Dust deepPulse = Dust.NewDustPerfect(playerCenter,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, DeepResonance, 0.35f);
                deepPulse.customData = new ResonantPulseBehavior(0.05f, 14);
            }

            // Phase 4: Constellation detonation flash — massive
            if (newComboStep >= 4)
            {
                CustomParticles.GenericFlare(playerCenter, ConstellationBlue, 0.6f, 20);
                CustomParticles.GenericFlare(playerCenter, Color.White, 0.25f, 14);

                GodRaySystem.CreateBurst(playerCenter, ConstellationBlue,
                    rayCount: 4, radius: 30f, duration: 14,
                    GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: HarmonicWhite);

                // Extra star burst for Stellar Crescendo phase entry
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                    Color burstColor = Color.Lerp(ConstellationBlue, CrescendoBright, (float)i / 6f);
                    Dust burst = Dust.NewDustPerfect(playerCenter,
                        ModContent.DustType<StarPointDust>(),
                        vel, 0, burstColor, 0.45f);
                    burst.customData = new StarPointBehavior(0.18f, 25);
                }
            }
        }

        // =====================================================================
        //  TRAIL FUNCTIONS
        // =====================================================================

        /// <summary>
        /// Wave projectile trail color — silver-dominant with resonant frequency shimmer.
        /// Standing wave pattern creates brightness variation along trail.
        /// </summary>
        public static Color WaveTrailColor(float progress)
        {
            float wave = MathF.Abs(MathF.Sin(progress * MathF.PI * 4f + Main.GlobalTimeWrappedHourly * 6f));
            Color c = Color.Lerp(ResonantSilver, FrequencyPulse,
                progress * 0.5f + wave * 0.3f);
            float brightness = 0.7f + wave * 0.3f;
            return (c * (brightness - progress * 0.5f)) with { A = 0 };
        }

        /// <summary>
        /// Wave trail width with precision taper and subtle oscillation.
        /// </summary>
        public static float WaveTrailWidth(float progress)
        {
            float sharp = 1f - progress;
            float oscillation = 1f + MathF.Sin(progress * MathF.PI * 6f) * 0.08f;
            return sharp * sharp * 20f * oscillation;
        }

        /// <summary>
        /// Resonant lighting with faster frequency pulsing.
        /// </summary>
        public static void AddResonantLight(Vector2 worldPos, float intensity = 0.7f)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.15f;
            Lighting.AddLight(worldPos, MoonlightVFXLibrary.IceBlue.ToVector3() * intensity * pulse);
        }
    }
}
