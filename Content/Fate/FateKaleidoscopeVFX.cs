using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Unique kaleidoscopic visual effects for each Fate weapon.
    /// Each weapon gets its own distinct cosmic distortion pattern with:
    /// - Shifting chromatic visuals
    /// - Heatwave/reality distortion effects
    /// - Kaleidoscopic geometric patterns
    /// </summary>
    public static class FateKaleidoscopeVFX
    {
        // DARK PRISMATIC COLOR PALETTE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color FateCyan = new Color(60, 200, 220);
        private static readonly Color FateGold = new Color(255, 180, 50);

        /// <summary>
        /// Gets a chromatic shift color based on time and offset
        /// </summary>
        private static Color GetChromaticShift(float timeOffset, float saturation = 1f)
        {
            float hue = (Main.GameUpdateCount * 0.008f + timeOffset) % 1f;
            return Main.hslToRgb(hue, saturation, 0.6f);
        }

        /// <summary>
        /// Gets the Fate gradient with optional chromatic shifting
        /// </summary>
        private static Color GetFateGradient(float progress, float chromaticOffset = 0f)
        {
            // Add chromatic shift to base gradient
            Color baseColor;
            if (progress < 0.4f)
                baseColor = Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                baseColor = Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                baseColor = Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);

            // Blend with chromatic shift
            if (chromaticOffset > 0f)
            {
                Color chromatic = GetChromaticShift(progress + chromaticOffset);
                return Color.Lerp(baseColor, chromatic, 0.25f);
            }
            return baseColor;
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE1: DESTINY CLEAVER - TEMPORAL FRACTURE KALEIDOSCOPE
        // Unique Pattern: Time-splitting fractals that create afterimage echoes
        #endregion

        /// <summary>
        /// Temporal Fracture - Time seems to split around the blade
        /// Creates kaleidoscopic time-echo patterns
        /// </summary>
        public static void DestinyCleaverKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.05f;
            
            // === TEMPORAL FRACTURE CORE ===
            // Reality splits into multiple time-streams
            int fractures = 8;
            for (int i = 0; i < fractures; i++)
            {
                float angle = MathHelper.TwoPi * i / fractures + time;
                float timeDelay = i * 0.12f;
                float fractureRadius = 45f * scale * (1f + (float)Math.Sin(time * 2f + i) * 0.3f);
                
                Vector2 fracturePos = position + angle.ToRotationVector2() * fractureRadius;
                
                // Time-split chromatic aberration
                Vector2 redOffset = new Vector2((float)Math.Cos(time + i), (float)Math.Sin(time + i)) * 4f;
                Vector2 cyanOffset = -redOffset;
                
                CustomParticles.GenericFlare(fracturePos + redOffset, Color.Red * 0.4f, 0.25f * scale, 12);
                CustomParticles.GenericFlare(fracturePos, GetFateGradient(i / (float)fractures, time) * 0.8f, 0.35f * scale, 15);
                CustomParticles.GenericFlare(fracturePos + cyanOffset, FateCyan * 0.4f, 0.25f * scale, 12);
            }
            
            // === HEATWAVE DISTORTION RINGS ===
            // Concentric distortion waves emanating outward
            for (int ring = 0; ring < 4; ring++)
            {
                float ringPhase = (time + ring * 0.5f) % 1f;
                float ringRadius = 20f + ringPhase * 60f * scale;
                float ringAlpha = 1f - ringPhase;
                
                // Draw distortion points around ring
                int ringPoints = 12;
                for (int p = 0; p < ringPoints; p++)
                {
                    float pointAngle = MathHelper.TwoPi * p / ringPoints + time * 0.5f;
                    Vector2 ringPos = position + pointAngle.ToRotationVector2() * ringRadius;
                    
                    // Shimmer effect - offset based on angle for heatwave
                    float shimmer = (float)Math.Sin(pointAngle * 3f + time * 4f) * 3f;
                    ringPos += new Vector2(shimmer, shimmer * 0.5f);
                    
                    Color ringColor = GetFateGradient(p / (float)ringPoints, time * 0.5f) * ringAlpha * 0.6f;
                    CustomParticles.GenericFlare(ringPos, ringColor, 0.18f * scale, 8);
                }
            }
            
            // === CENTRAL VOID CORE ===
            CustomParticles.GenericFlare(position, FateBlack, 0.5f * scale, 10);
            CustomParticles.GenericFlare(position, FateBrightRed * 0.7f, 0.35f * scale, 8);
        }

        /// <summary>
        /// Temporal Echo Trail - afterimages that show past positions
        /// </summary>
        public static void DestinyCleaverEchoTrail(Vector2 position, Vector2 velocity, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            
            // Create multiple time-echoes trailing behind
            for (int echo = 0; echo < 5; echo++)
            {
                float echoDelay = echo * 0.15f;
                float echoAlpha = 1f - echo * 0.18f;
                Vector2 echoOffset = -velocity.SafeNormalize(Vector2.Zero) * echo * 12f;
                Vector2 echoPos = position + echoOffset;
                
                // Chromatic split on echoes
                CustomParticles.GenericFlare(echoPos + new Vector2(-2, 0), Color.Red * 0.3f * echoAlpha, 0.2f * scale, 10);
                CustomParticles.GenericFlare(echoPos, GetFateGradient(echoDelay, time) * echoAlpha, 0.28f * scale, 12);
                CustomParticles.GenericFlare(echoPos + new Vector2(2, 0), FateCyan * 0.3f * echoAlpha, 0.2f * scale, 10);
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE2: CANNON OF INEVITABILITY - TIMELINE CONVERGENCE KALEIDOSCOPE
        // Unique Pattern: Multiple timelines visibly converging to single point
        #endregion

        /// <summary>
        /// Timeline Convergence - Multiple realities collapse to one point
        /// </summary>
        public static void CannonOfInevitabilityKaleidoscope(Vector2 position, Vector2 targetDirection, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            
            // === CONVERGING TIMELINE STREAMS ===
            int timelines = 6;
            for (int t = 0; t < timelines; t++)
            {
                float timelineAngle = MathHelper.TwoPi * t / timelines + time * 0.3f;
                float convergenceProgress = (time + t * 0.16f) % 1f;
                
                // Timeline starts far and converges to center
                float startRadius = 80f * scale;
                float currentRadius = startRadius * (1f - convergenceProgress);
                Vector2 timelinePos = position + timelineAngle.ToRotationVector2() * currentRadius;
                
                // Each timeline has unique chromatic signature
                float hueOffset = t / (float)timelines;
                Color timelineColor = GetChromaticShift(hueOffset + time * 0.5f);
                float alpha = convergenceProgress; // Brighter as they converge
                
                CustomParticles.GenericFlare(timelinePos, timelineColor * alpha * 0.7f, 0.3f * scale * (1f + convergenceProgress), 14);
                
                // Trailing particles along convergence path
                for (int trail = 0; trail < 3; trail++)
                {
                    float trailProgress = convergenceProgress - trail * 0.1f;
                    if (trailProgress > 0f)
                    {
                        float trailRadius = startRadius * (1f - trailProgress);
                        Vector2 trailPos = position + timelineAngle.ToRotationVector2() * trailRadius;
                        CustomParticles.GenericFlare(trailPos, timelineColor * (alpha - trail * 0.25f) * 0.4f, 0.15f * scale, 8);
                    }
                }
            }
            
            // === INEVITABILITY PULSE ===
            // Pulsing core where all timelines meet
            float pulse = (float)Math.Sin(time * 3f) * 0.2f + 0.8f;
            CustomParticles.GenericFlare(position, FateWhite * pulse * 0.5f, 0.4f * scale * pulse, 10);
            CustomParticles.HaloRing(position, FateDarkPink * 0.6f, 0.3f * scale, 12);
        }

        /// <summary>
        /// Probability Wave - Visual representation of collapsing probability
        /// </summary>
        public static void CannonProbabilityWave(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.06f;
            
            // Wave-like distortion pattern
            int waves = 3;
            for (int w = 0; w < waves; w++)
            {
                float wavePhase = (time + w * 0.33f) % 1f;
                float waveRadius = 30f + wavePhase * 50f * scale;
                float waveAlpha = 1f - wavePhase;
                
                // Probability uncertainty - particles flicker in and out
                int points = 16;
                for (int p = 0; p < points; p++)
                {
                    if (Main.rand.NextBool(3)) continue; // Quantum uncertainty
                    
                    float angle = MathHelper.TwoPi * p / points;
                    // Heatwave shimmer
                    float shimmer = (float)Math.Sin(angle * 5f + time * 6f) * 5f * waveAlpha;
                    Vector2 wavePos = position + angle.ToRotationVector2() * (waveRadius + shimmer);
                    
                    Color waveColor = Color.Lerp(FatePurple, FateBrightRed, p / (float)points) * waveAlpha * 0.5f;
                    CustomParticles.GenericFlare(wavePos, waveColor, 0.12f * scale, 6);
                }
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE3: COSMIC DECREE - CELESTIAL JUDGMENT KALEIDOSCOPE
        // Unique Pattern: Concentric decree circles with celestial symbols
        #endregion

        /// <summary>
        /// Celestial Judgment - Cosmic law manifests as geometric patterns
        /// </summary>
        public static void CosmicDecreeKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.035f;
            
            // === DECREE RINGS ===
            // Multiple concentric rings rotating in opposite directions
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = (25f + ring * 30f) * scale;
                float ringRotation = time * (ring % 2 == 0 ? 1f : -0.7f);
                int segments = 6 + ring * 2;
                
                for (int s = 0; s < segments; s++)
                {
                    float segAngle = MathHelper.TwoPi * s / segments + ringRotation;
                    Vector2 segPos = position + segAngle.ToRotationVector2() * ringRadius;
                    
                    // Celestial chromatic shifting
                    float chromatic = (s + ring * segments) / (float)(segments * 3);
                    Color segColor = GetFateGradient(chromatic, time * 0.3f);
                    
                    // Decree symbol points
                    CustomParticles.GenericFlare(segPos, segColor * 0.75f, 0.28f * scale, 14);
                    
                    // Connect to next segment with faint line particles
                    float nextAngle = MathHelper.TwoPi * (s + 1) / segments + ringRotation;
                    Vector2 nextPos = position + nextAngle.ToRotationVector2() * ringRadius;
                    Vector2 midPoint = (segPos + nextPos) / 2f;
                    CustomParticles.GenericFlare(midPoint, segColor * 0.35f, 0.12f * scale, 8);
                }
            }
            
            // === JUDGMENT CORE ===
            // Central all-seeing cosmic eye effect
            float eyePulse = (float)Math.Sin(time * 2f) * 0.15f + 0.85f;
            CustomParticles.GenericFlare(position, FateBlack, 0.45f * scale * eyePulse, 12);
            CustomParticles.GenericFlare(position, FateBrightRed * 0.8f, 0.3f * scale, 10);
            
            // Radiating judgment lines
            for (int ray = 0; ray < 8; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 8f + time * 0.5f;
                for (int r = 1; r <= 4; r++)
                {
                    Vector2 rayPos = position + rayAngle.ToRotationVector2() * (r * 18f * scale);
                    float rayAlpha = 1f - r * 0.2f;
                    CustomParticles.GenericFlare(rayPos, GetChromaticShift(ray / 8f + time) * rayAlpha * 0.4f, 0.15f * scale, 8);
                }
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE4: SUPERNOVA BOW - STELLAR COLLAPSE KALEIDOSCOPE
        // Unique Pattern: Star death spiral with gravitational lensing
        #endregion

        /// <summary>
        /// Stellar Collapse - A dying star's light bends around the bow
        /// </summary>
        public static void SupernovaBowKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.045f;
            
            // === GRAVITATIONAL LENSING EFFECT ===
            // Light appears to bend around a central mass
            int lensingRays = 12;
            for (int ray = 0; ray < lensingRays; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / lensingRays;
                
                // Curved path - light bends toward center then away
                for (int point = 0; point < 8; point++)
                {
                    float t = point / 7f;
                    float bendFactor = (float)Math.Sin(t * MathHelper.Pi) * 15f;
                    float radius = 15f + t * 55f * scale;
                    
                    // Apply gravitational bend
                    float bentAngle = rayAngle + bendFactor * 0.02f * (float)Math.Sin(time + ray);
                    Vector2 bentPos = position + bentAngle.ToRotationVector2() * radius;
                    
                    // Chromatic dispersion from lensing
                    float dispersion = t * 0.3f;
                    CustomParticles.GenericFlare(bentPos + new Vector2(-dispersion * 3, 0), Color.Red * 0.3f * (1f - t), 0.12f * scale, 6);
                    CustomParticles.GenericFlare(bentPos, GetFateGradient(t, time) * (1f - t * 0.5f) * 0.6f, 0.18f * scale, 8);
                    CustomParticles.GenericFlare(bentPos + new Vector2(dispersion * 3, 0), FateCyan * 0.3f * (1f - t), 0.12f * scale, 6);
                }
            }
            
            // === STELLAR CORE ===
            float corePulse = (float)Math.Abs(Math.Sin(time * 4f));
            CustomParticles.GenericFlare(position, FateWhite * corePulse * 0.6f, 0.35f * scale, 8);
            CustomParticles.GenericFlare(position, FateDarkPink * 0.8f, 0.25f * scale, 10);
        }

        /// <summary>
        /// Supernova Burst - Explosive star death visuals on arrow release
        /// </summary>
        public static void SupernovaBurst(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.1f;
            
            // Expanding chromatic shell
            int shells = 3;
            for (int s = 0; s < shells; s++)
            {
                float shellPhase = (time + s * 0.33f) % 1f;
                float shellRadius = shellPhase * 60f * scale;
                float shellAlpha = 1f - shellPhase;
                
                int points = 16;
                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.TwoPi * p / points;
                    Vector2 shellPos = position + angle.ToRotationVector2() * shellRadius;
                    
                    // Heavy chromatic split on explosion
                    CustomParticles.GenericFlare(shellPos + new Vector2(-4, 0), Color.Red * shellAlpha * 0.5f, 0.2f * scale, 8);
                    CustomParticles.GenericFlare(shellPos, GetChromaticShift(p / (float)points) * shellAlpha * 0.7f, 0.28f * scale, 10);
                    CustomParticles.GenericFlare(shellPos + new Vector2(4, 0), FateCyan * shellAlpha * 0.5f, 0.2f * scale, 8);
                }
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE5: COSMIC PULSE REPEATER - QUANTUM FREQUENCY KALEIDOSCOPE
        // Unique Pattern: Rapid pulse waves like quantum oscillations
        #endregion

        /// <summary>
        /// Quantum Frequency - Rapid oscillating wave patterns
        /// </summary>
        public static void CosmicPulseKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.08f; // Faster for repeater
            
            // === RAPID PULSE WAVES ===
            int pulseCount = 5;
            for (int pulse = 0; pulse < pulseCount; pulse++)
            {
                float pulsePhase = (time * 2f + pulse * 0.2f) % 1f;
                float pulseRadius = pulsePhase * 40f * scale;
                float pulseAlpha = (1f - pulsePhase) * 0.6f;
                
                // Sine wave distortion on the ring
                int wavePoints = 20;
                for (int p = 0; p < wavePoints; p++)
                {
                    float angle = MathHelper.TwoPi * p / wavePoints;
                    // Quantum oscillation
                    float oscillation = (float)Math.Sin(angle * 6f + time * 8f) * 4f * pulseAlpha;
                    float actualRadius = pulseRadius + oscillation;
                    
                    Vector2 wavePos = position + angle.ToRotationVector2() * actualRadius;
                    Color waveColor = GetFateGradient(p / (float)wavePoints, time) * pulseAlpha;
                    CustomParticles.GenericFlare(wavePos, waveColor, 0.1f * scale, 5);
                }
            }
            
            // === FREQUENCY CORE ===
            // Vibrating center
            float vibrate = (float)Math.Sin(time * 15f) * 2f;
            CustomParticles.GenericFlare(position + new Vector2(vibrate, 0), FateBrightRed * 0.7f, 0.22f * scale, 6);
            CustomParticles.GenericFlare(position - new Vector2(vibrate, 0), FatePurple * 0.7f, 0.22f * scale, 6);
        }

        /// <summary>
        /// Pulse Muzzle Flash - rapid fire chromatic flash
        /// </summary>
        public static void PulseMuzzleFlash(Vector2 position, Vector2 direction, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.1f;
            
            // Quick chromatic burst
            CustomParticles.GenericFlare(position + new Vector2(-3, 0), Color.Red * 0.5f, 0.25f * scale, 6);
            CustomParticles.GenericFlare(position, GetChromaticShift(time) * 0.8f, 0.35f * scale, 8);
            CustomParticles.GenericFlare(position + new Vector2(3, 0), FateCyan * 0.5f, 0.25f * scale, 6);
            
            // Directional pulse line
            for (int i = 0; i < 4; i++)
            {
                Vector2 linePos = position + direction * (i * 8f);
                float lineAlpha = 1f - i * 0.2f;
                CustomParticles.GenericFlare(linePos, FateDarkPink * lineAlpha * 0.6f, 0.15f * scale, 6);
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE6: ROCKET CATACLYSM - REALITY SHATTER KALEIDOSCOPE
        // Unique Pattern: Fragmented reality shards flying outward
        #endregion

        /// <summary>
        /// Reality Shatter - Space itself appears to crack and fragment
        /// </summary>
        public static void RocketCataclysmKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            
            // === REALITY FRAGMENTS ===
            // Jagged shard-like patterns
            int shardLayers = 4;
            for (int layer = 0; layer < shardLayers; layer++)
            {
                float layerRadius = (20f + layer * 25f) * scale;
                float layerRotation = time * (layer % 2 == 0 ? 0.8f : -0.6f);
                int shards = 5 + layer;
                
                for (int shard = 0; shard < shards; shard++)
                {
                    float shardAngle = MathHelper.TwoPi * shard / shards + layerRotation;
                    
                    // Jagged shard shape - multiple points per shard
                    float shardOffset = (float)Math.Sin(shard * 2f + time * 3f) * 8f;
                    Vector2 shardPos = position + shardAngle.ToRotationVector2() * (layerRadius + shardOffset);
                    
                    // Reality crack colors - harsh chromatic separation
                    CustomParticles.GenericFlare(shardPos + new Vector2(-5, -2), Color.Red * 0.4f, 0.18f * scale, 10);
                    CustomParticles.GenericFlare(shardPos, FateWhite * 0.6f, 0.25f * scale, 12);
                    CustomParticles.GenericFlare(shardPos + new Vector2(5, 2), Color.Blue * 0.4f, 0.18f * scale, 10);
                    
                    // Shard edge glow
                    Vector2 edgeDir = shardAngle.ToRotationVector2();
                    CustomParticles.GenericFlare(shardPos + edgeDir * 10f, GetFateGradient(shard / (float)shards, time) * 0.5f, 0.12f * scale, 8);
                }
            }
            
            // === CATACLYSM CORE ===
            // Unstable energy at center
            float instability = (float)Math.Sin(time * 5f) * 0.3f + 0.7f;
            CustomParticles.GenericFlare(position, FateBlack * instability, 0.55f * scale, 12);
            CustomParticles.GenericFlare(position, FateBrightRed * (1f - instability * 0.3f), 0.4f * scale, 10);
        }

        /// <summary>
        /// Cataclysm Explosion - Ultimate destruction visuals
        /// </summary>
        public static void CataclysmExplosion(Vector2 position, float scale = 1f)
        {
            // Reality-shattering explosion with extreme chromatic aberration
            int explosionWaves = 5;
            for (int wave = 0; wave < explosionWaves; wave++)
            {
                float waveDelay = wave * 0.15f;
                float waveRadius = 30f + wave * 25f * scale;
                
                int fragments = 12 + wave * 2;
                for (int f = 0; f < fragments; f++)
                {
                    float angle = MathHelper.TwoPi * f / fragments + wave * 0.2f;
                    Vector2 fragPos = position + angle.ToRotationVector2() * waveRadius;
                    
                    // Extreme RGB split
                    CustomParticles.GenericFlare(fragPos + new Vector2(-8, 0), Color.Red * 0.6f, 0.3f * scale, 15);
                    CustomParticles.GenericFlare(fragPos, FateWhite * 0.8f, 0.4f * scale, 18);
                    CustomParticles.GenericFlare(fragPos + new Vector2(8, 0), Color.Cyan * 0.6f, 0.3f * scale, 15);
                }
            }
            
            // Central flash
            CustomParticles.GenericFlare(position, FateWhite, 1.5f * scale, 20);
            CustomParticles.HaloRing(position, FateBrightRed, 0.8f * scale, 18);
            CustomParticles.HaloRing(position, FateDarkPink, 0.6f * scale, 15);
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE7: DESTINY SPIRAL GRIMOIRE - COSMIC VORTEX KALEIDOSCOPE
        // Unique Pattern: Spiraling text-like symbols being drawn into vortex
        #endregion

        /// <summary>
        /// Cosmic Vortex - Knowledge spirals into oblivion
        /// </summary>
        public static void DestinySpiralKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            
            // === SPIRALING COSMIC TEXT ===
            int spiralArms = 3;
            for (int arm = 0; arm < spiralArms; arm++)
            {
                float armOffset = MathHelper.TwoPi * arm / spiralArms;
                
                // Each arm spirals inward
                for (int point = 0; point < 12; point++)
                {
                    float spiralProgress = point / 12f;
                    float spiralAngle = armOffset + spiralProgress * MathHelper.TwoPi * 2f + time;
                    float spiralRadius = (1f - spiralProgress * 0.8f) * 60f * scale;
                    
                    Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
                    
                    // Text-like glyph particles
                    Color glyphColor = GetFateGradient(spiralProgress, time + arm * 0.33f);
                    CustomParticles.GenericFlare(spiralPos, glyphColor * 0.7f, 0.2f * scale * (1f - spiralProgress * 0.5f), 12);
                    
                    // Chromatic trailing
                    if (point % 3 == 0)
                    {
                        CustomParticles.GenericFlare(spiralPos + new Vector2(-3, 0), Color.Red * 0.25f, 0.12f * scale, 8);
                        CustomParticles.GenericFlare(spiralPos + new Vector2(3, 0), FateCyan * 0.25f, 0.12f * scale, 8);
                    }
                }
            }
            
            // === VORTEX CENTER ===
            // The knowledge singularity
            float vortexPull = (float)Math.Sin(time * 2f) * 0.2f + 0.8f;
            CustomParticles.GenericFlare(position, FateBlack * vortexPull, 0.5f * scale, 14);
            CustomParticles.GenericFlare(position, FatePurple * (1f - vortexPull * 0.3f), 0.35f * scale, 12);
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE8: COLLAPSE STAFF - ENTROPY CASCADE KALEIDOSCOPE
        // Unique Pattern: Order dissolving into chaos, structured becoming random
        #endregion

        /// <summary>
        /// Entropy Cascade - Order collapses into beautiful chaos
        /// </summary>
        public static void CollapseStaffKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            
            // === ORDERED STRUCTURE DISSOLVING ===
            // Starts geometric, becomes chaotic
            int structurePoints = 8;
            for (int i = 0; i < structurePoints; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / structurePoints;
                
                // Structure dissolving - adds randomness over time
                float chaos = (float)Math.Sin(time + i) * 10f;
                float radius = 40f * scale + chaos;
                float actualAngle = baseAngle + (float)Math.Sin(time * 2f + i * 0.5f) * 0.2f;
                
                Vector2 structPos = position + actualAngle.ToRotationVector2() * radius;
                
                // Color shifts from ordered (gradient) to chaotic (random chromatic)
                float chaosBlend = (float)(Math.Sin(time * 3f + i) * 0.5f + 0.5f);
                Color orderedColor = GetFateGradient(i / (float)structurePoints, 0f);
                Color chaoticColor = GetChromaticShift(time + i * 0.12f);
                Color finalColor = Color.Lerp(orderedColor, chaoticColor, chaosBlend);
                
                CustomParticles.GenericFlare(structPos, finalColor * 0.75f, 0.28f * scale, 14);
                
                // Entropy trails
                Vector2 entropyDir = (structPos - position).SafeNormalize(Vector2.Zero);
                for (int e = 0; e < 3; e++)
                {
                    Vector2 entropyPos = structPos + entropyDir * (e * 8f) + Main.rand.NextVector2Circular(chaos * 0.3f, chaos * 0.3f);
                    CustomParticles.GenericFlare(entropyPos, finalColor * (0.4f - e * 0.1f), 0.12f * scale, 8);
                }
            }
            
            // === COLLAPSE CORE ===
            // Where entropy originates
            float collapseIntensity = (float)Math.Abs(Math.Sin(time * 4f));
            CustomParticles.GenericFlare(position, FateBrightRed * collapseIntensity * 0.8f, 0.4f * scale, 10);
            CustomParticles.GenericFlare(position, FateBlack * (1f - collapseIntensity * 0.3f), 0.3f * scale, 8);
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE9: SINGULARITY SCEPTER - BLACK HOLE KALEIDOSCOPE
        // Unique Pattern: Event horizon with Hawking radiation
        #endregion

        /// <summary>
        /// Event Horizon - Light bends impossibly around darkness
        /// </summary>
        public static void SingularityScepterKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.025f;
            
            // === EVENT HORIZON RING ===
            // The point of no return - light orbits eternally
            float horizonRadius = 35f * scale;
            int orbitingPhotons = 20;
            for (int photon = 0; photon < orbitingPhotons; photon++)
            {
                float photonAngle = MathHelper.TwoPi * photon / orbitingPhotons + time * 2f;
                
                // Light spiraling toward singularity
                float spiralFactor = (float)Math.Sin(time * 3f + photon * 0.3f) * 0.15f;
                float actualRadius = horizonRadius * (1f + spiralFactor);
                
                Vector2 photonPos = position + photonAngle.ToRotationVector2() * actualRadius;
                
                // Gravitational redshift - color shifts based on proximity
                float redshift = 1f - spiralFactor * 2f;
                Color photonColor = Color.Lerp(FateWhite, FateBrightRed, redshift);
                
                CustomParticles.GenericFlare(photonPos, photonColor * 0.6f, 0.15f * scale, 10);
                
                // Chromatic gravitational lensing
                Vector2 lensOffset = (position - photonPos).SafeNormalize(Vector2.Zero) * 4f;
                CustomParticles.GenericFlare(photonPos + lensOffset.RotatedBy(MathHelper.PiOver2), Color.Red * 0.25f, 0.1f * scale, 6);
                CustomParticles.GenericFlare(photonPos + lensOffset.RotatedBy(-MathHelper.PiOver2), Color.Cyan * 0.25f, 0.1f * scale, 6);
            }
            
            // === HAWKING RADIATION ===
            // Particles escaping the singularity
            if (Main.rand.NextBool(3))
            {
                float escapeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 escapePos = position + escapeAngle.ToRotationVector2() * (horizonRadius + 5f);
                Vector2 escapeVel = escapeAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                
                Color hawkingColor = GetChromaticShift(Main.rand.NextFloat());
                var hawking = new GenericGlowParticle(escapePos, escapeVel, hawkingColor * 0.6f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(hawking);
            }
            
            // === SINGULARITY CORE ===
            // Absolute darkness with bright accretion edge
            CustomParticles.GenericFlare(position, FateBlack, 0.6f * scale, 15);
            CustomParticles.GenericFlare(position, FateDarkPink * 0.3f, 0.2f * scale, 8);
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE10: WISP CONDUCTOR - ORCHESTRA KALEIDOSCOPE
        // Unique Pattern: Musical conductor patterns, sweeping baton trails
        #endregion

        /// <summary>
        /// Orchestra Conductor - Musical energy flows with baton movements
        /// </summary>
        public static void WispConductorKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            
            // === CONDUCTING PATTERNS ===
            // Figure-8 and sweeping arc patterns
            float figure8X = (float)Math.Sin(time * 2f) * 35f * scale;
            float figure8Y = (float)Math.Sin(time * 4f) * 20f * scale;
            Vector2 batonPos = position + new Vector2(figure8X, figure8Y);
            
            // Baton trail with chromatic sweep
            for (int trail = 0; trail < 8; trail++)
            {
                float trailTime = time - trail * 0.05f;
                float trailX = (float)Math.Sin(trailTime * 2f) * 35f * scale;
                float trailY = (float)Math.Sin(trailTime * 4f) * 20f * scale;
                Vector2 trailPos = position + new Vector2(trailX, trailY);
                
                float trailAlpha = 1f - trail * 0.1f;
                Color trailColor = GetFateGradient(trail / 8f, time);
                
                CustomParticles.GenericFlare(trailPos, trailColor * trailAlpha * 0.6f, 0.18f * scale * trailAlpha, 10);
            }
            
            // === MUSICAL STAFF LINES ===
            // Horizontal lines like sheet music
            for (int line = 0; line < 5; line++)
            {
                float lineY = position.Y - 20f + line * 10f;
                for (int note = 0; note < 6; note++)
                {
                    float noteX = position.X - 40f + note * 16f + (float)Math.Sin(time + note + line) * 3f;
                    Vector2 notePos = new Vector2(noteX, lineY + (float)Math.Sin(time * 2f + note) * 2f);
                    
                    // Staff lines shimmer
                    Color lineColor = GetFateGradient((line + note) / 11f, time * 0.5f) * 0.4f;
                    CustomParticles.GenericFlare(notePos, lineColor, 0.08f * scale, 6);
                }
            }
            
            // === CONDUCTOR'S GLOW ===
            float conductorPulse = (float)Math.Sin(time * 3f) * 0.2f + 0.8f;
            CustomParticles.GenericFlare(batonPos, FateBrightRed * conductorPulse, 0.3f * scale, 10);
            CustomParticles.GenericFlare(batonPos, FateWhite * 0.4f, 0.15f * scale, 6);
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE11: DESTINY CHOIR - HARMONIC RESONANCE KALEIDOSCOPE
        // Unique Pattern: Sound wave interference patterns, vocal harmonics
        #endregion

        /// <summary>
        /// Harmonic Resonance - Sound waves create interference patterns
        /// </summary>
        public static void DestinyChoirKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.035f;
            
            // === VOCAL HARMONICS ===
            // Three singers create overlapping wave patterns
            Vector2[] singerPositions = new Vector2[3];
            for (int singer = 0; singer < 3; singer++)
            {
                float singerAngle = MathHelper.TwoPi * singer / 3f + MathHelper.PiOver2;
                singerPositions[singer] = position + singerAngle.ToRotationVector2() * 30f * scale;
                
                // Each singer emits waves
                for (int wave = 0; wave < 3; wave++)
                {
                    float wavePhase = (time + wave * 0.3f + singer * 0.2f) % 1f;
                    float waveRadius = wavePhase * 35f * scale;
                    float waveAlpha = (1f - wavePhase) * 0.5f;
                    
                    // Wave ring
                    int wavePoints = 12;
                    for (int p = 0; p < wavePoints; p++)
                    {
                        float pointAngle = MathHelper.TwoPi * p / wavePoints;
                        Vector2 wavePos = singerPositions[singer] + pointAngle.ToRotationVector2() * waveRadius;
                        
                        // Harmonic color based on singer
                        Color harmonic = singer == 0 ? FateBrightRed : (singer == 1 ? FateDarkPink : FatePurple);
                        CustomParticles.GenericFlare(wavePos, harmonic * waveAlpha, 0.08f * scale, 5);
                    }
                }
                
                // Singer core
                CustomParticles.GenericFlare(singerPositions[singer], GetFateGradient(singer / 3f, time) * 0.7f, 0.22f * scale, 12);
            }
            
            // === INTERFERENCE PATTERN ===
            // Where waves meet, create bright interference nodes
            Vector2 center = position;
            for (int node = 0; node < 6; node++)
            {
                float nodeAngle = MathHelper.TwoPi * node / 6f + time * 0.5f;
                float nodeRadius = 25f * scale;
                Vector2 nodePos = center + nodeAngle.ToRotationVector2() * nodeRadius;
                
                // Constructive interference - bright chromatic flash
                float interference = (float)Math.Sin(time * 4f + node) * 0.3f + 0.7f;
                CustomParticles.GenericFlare(nodePos, GetChromaticShift(node / 6f + time) * interference * 0.5f, 0.15f * scale, 8);
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // FATE12: SINGULARITY FAMILIAR - COSMIC ZONE KALEIDOSCOPE
        // Unique Pattern: Space-time distortion zones, gravitational anomalies
        #endregion

        /// <summary>
        /// Cosmic Zone - Space-time itself warps and distorts
        /// </summary>
        public static void SingularityFamiliarKaleidoscope(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            
            // === SPACE-TIME GRID DISTORTION ===
            // A grid that visibly warps around the singularity
            int gridSize = 5;
            float gridSpacing = 20f * scale;
            
            for (int gx = -gridSize; gx <= gridSize; gx++)
            {
                for (int gy = -gridSize; gy <= gridSize; gy++)
                {
                    // Base grid position
                    Vector2 gridBase = new Vector2(gx * gridSpacing, gy * gridSpacing);
                    float distFromCenter = gridBase.Length();
                    
                    if (distFromCenter < 15f || distFromCenter > gridSize * gridSpacing * 0.9f) continue;
                    
                    // Gravitational warping - grid bends toward center
                    float warpStrength = 30f / (distFromCenter + 10f);
                    Vector2 warpDir = -gridBase.SafeNormalize(Vector2.Zero);
                    Vector2 warpedPos = position + gridBase + warpDir * warpStrength * 15f;
                    
                    // Time dilation effect - color shifts based on proximity
                    float timeDilation = 1f - warpStrength * 0.5f;
                    Color gridColor = GetFateGradient(distFromCenter / (gridSize * gridSpacing), time * timeDilation);
                    
                    CustomParticles.GenericFlare(warpedPos, gridColor * 0.35f, 0.1f * scale, 8);
                    
                    // Chromatic time distortion
                    if ((gx + gy) % 3 == 0)
                    {
                        CustomParticles.GenericFlare(warpedPos + new Vector2(-3, 0), Color.Red * 0.2f, 0.06f * scale, 5);
                        CustomParticles.GenericFlare(warpedPos + new Vector2(3, 0), Color.Cyan * 0.2f, 0.06f * scale, 5);
                    }
                }
            }
            
            // === ANOMALY CORE ===
            // The familiar itself - a concentrated gravitational anomaly
            float anomalyPulse = (float)Math.Sin(time * 2.5f) * 0.25f + 0.75f;
            CustomParticles.GenericFlare(position, FateBlack * anomalyPulse, 0.7f * scale, 15);
            CustomParticles.GenericFlare(position, FateBrightRed * (1f - anomalyPulse * 0.3f), 0.45f * scale, 12);
            CustomParticles.HaloRing(position, FateDarkPink * 0.5f, 0.35f * scale, 14);
            
            // === GRAVITATIONAL WAVES ===
            // Ripples in space-time
            for (int wave = 0; wave < 3; wave++)
            {
                float wavePhase = (time + wave * 0.33f) % 1f;
                float waveRadius = 20f + wavePhase * 80f * scale;
                float waveAlpha = (1f - wavePhase) * 0.4f;
                
                CustomParticles.HaloRing(position, GetChromaticShift(wave / 3f + time) * waveAlpha, 
                    0.15f * scale * (1f - wavePhase * 0.5f), 10);
            }
        }

        #region ═══════════════════════════════════════════════════════════════
        // HEATWAVE DISTORTION UTILITY - Used by all weapons
        #endregion

        /// <summary>
        /// Universal heatwave distortion effect - makes air shimmer around weapons
        /// </summary>
        public static void HeatwaveDistortion(Vector2 position, float radius, float intensity = 1f)
        {
            float time = Main.GameUpdateCount * 0.05f;
            
            int shimmerPoints = (int)(radius / 8f);
            for (int i = 0; i < shimmerPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / shimmerPoints;
                float pointRadius = radius * (0.7f + Main.rand.NextFloat(0.3f));
                
                // Heatwave shimmer offset
                float shimmerX = (float)Math.Sin(angle * 3f + time * 5f) * 4f * intensity;
                float shimmerY = (float)Math.Cos(angle * 2f + time * 4f) * 3f * intensity;
                
                Vector2 shimmerPos = position + angle.ToRotationVector2() * pointRadius + new Vector2(shimmerX, shimmerY);
                
                // Very subtle, nearly invisible distortion particles
                Color shimmerColor = Color.White * 0.08f * intensity;
                CustomParticles.GenericFlare(shimmerPos, shimmerColor, 0.06f, 4);
            }
        }

        /// <summary>
        /// Cosmic distortion lines - streaks that show reality bending
        /// </summary>
        public static void CosmicDistortionLines(Vector2 position, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            
            // Create streaking distortion lines
            int lines = 6;
            for (int line = 0; line < lines; line++)
            {
                float lineAngle = MathHelper.TwoPi * line / lines + time * 0.5f;
                float lineLength = 40f * scale;
                
                // Streaking particles along line
                for (int point = 0; point < 5; point++)
                {
                    float pointProgress = point / 5f;
                    Vector2 linePos = position + lineAngle.ToRotationVector2() * (15f + pointProgress * lineLength);
                    
                    // Line fades outward with chromatic split
                    float lineAlpha = 1f - pointProgress * 0.7f;
                    CustomParticles.GenericFlare(linePos + new Vector2(-2 * pointProgress, 0), Color.Red * lineAlpha * 0.3f, 0.08f * scale, 6);
                    CustomParticles.GenericFlare(linePos, GetFateGradient(pointProgress, time) * lineAlpha * 0.5f, 0.12f * scale, 8);
                    CustomParticles.GenericFlare(linePos + new Vector2(2 * pointProgress, 0), FateCyan * lineAlpha * 0.3f, 0.08f * scale, 6);
                }
            }
        }
    }
}
