using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// PHASE 10: BOSS ATTACK VFX SYSTEM
    /// 
    /// Contains 70 unique boss attack VFX patterns categorized by type:
    /// - Telegraph Effects (warnings before attacks)
    /// - Impact/Explosion Effects 
    /// - Beam/Laser Effects
    /// - Area Denial Effects
    /// - Musical Special Effects
    /// </summary>
    public static class Phase10BossVFX
    {
        #region Constants and Helpers
        
        // Musical timing constants
        private const float BPM_SLOW = 60f;
        private const float BPM_MODERATE = 120f;
        private const float BPM_FAST = 180f;
        
        private static float GetFramesPerBeat(float bpm) => 3600f / bpm;
        private static bool IsOnBeat(float bpm) => (int)(Main.GameUpdateCount % GetFramesPerBeat(bpm)) < 4;
        
        #endregion
        
        #region Telegraph Effects (1-15)
        
        /// <summary>
        /// #1 - Staff Line Convergence: 5 parallel lines converging on target point
        /// </summary>
        public static void StaffLineConvergence(Vector2 target, Color color, float progress, float maxRadius = 200f)
        {
            float radius = maxRadius * (1f - progress);
            
            // 5 horizontal staff lines converging
            for (int line = -2; line <= 2; line++)
            {
                Vector2 leftPos = target + new Vector2(-radius, line * 12f);
                Vector2 rightPos = target + new Vector2(radius, line * 12f);
                
                // Draw particles along line
                int particles = (int)(8 * progress) + 3;
                for (int i = 0; i < particles; i++)
                {
                    float t = i / (float)(particles - 1);
                    Vector2 pos = Vector2.Lerp(leftPos, rightPos, t);
                    
                    Dust lineDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero, 0, color, 0.6f + progress * 0.4f);
                    lineDust.noGravity = true;
                    lineDust.fadeIn = 1.2f;
                }
            }
            
            // Notes appearing on staff as it converges
            if (Main.rand.NextBool(3) && progress > 0.3f)
            {
                int line = Main.rand.Next(-2, 3);
                Vector2 notePos = target + new Vector2(Main.rand.NextFloat(-radius, radius), line * 12f);
                ThemedParticles.MusicNote(notePos, (target - notePos) * 0.02f, color, 0.7f + progress * 0.3f, 20);
            }
        }
        
        /// <summary>
        /// #2 - Crescendo Danger Rings: Pulsing rings that grow with intensity
        /// </summary>
        public static void CrescendoDangerRings(Vector2 center, Color color, float intensity, int ringCount = 3)
        {
            for (int i = 0; i < ringCount; i++)
            {
                float ringProgress = (Main.GameUpdateCount * 0.03f + i * 0.33f) % 1f;
                float ringRadius = 30f + ringProgress * 150f * intensity;
                float ringAlpha = (1f - ringProgress) * intensity;
                
                Color ringColor = Color.Lerp(color, Color.White, ringProgress * 0.3f);
                CustomParticles.HaloRing(center, ringColor * ringAlpha, ringRadius / 150f, 12);
            }
            
            // Central warning pulse
            float pulse = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f * intensity;
            EnhancedParticles.BloomFlare(center, Color.White * intensity, pulse, 8, 2, 0.5f);
        }
        
        /// <summary>
        /// #3 - Note Constellation Warning: Notes form constellation pattern before attack
        /// </summary>
        public static void NoteConstellationWarning(Vector2 center, Color color, float progress, int noteCount = 8)
        {
            float radius = 60f + progress * 40f;
            
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / noteCount;
                Vector2 notePos = center + angle.ToRotationVector2() * radius;
                
                // Constellation lines connecting notes
                if (progress > 0.5f)
                {
                    int nextI = (i + 1) % noteCount;
                    float nextAngle = MathHelper.TwoPi * nextI / noteCount;
                    Vector2 nextPos = center + nextAngle.ToRotationVector2() * radius;
                    
                    // Draw connecting line
                    int lineParticles = 5;
                    for (int p = 0; p < lineParticles; p++)
                    {
                        Vector2 linePos = Vector2.Lerp(notePos, nextPos, p / (float)(lineParticles - 1));
                        Dust lineDust = Dust.NewDustPerfect(linePos, DustID.GoldCoin, Vector2.Zero, 0, color * 0.5f, 0.4f);
                        lineDust.noGravity = true;
                    }
                }
                
                // Notes pulsing brighter as attack approaches
                if (Main.rand.NextBool(4))
                {
                    float noteScale = 0.6f + progress * 0.4f;
                    ThemedParticles.MusicNote(notePos, Vector2.Zero, color, noteScale, 15);
                }
            }
        }
        
        /// <summary>
        /// #4 - Metronome Tick Warning: Ticking countdown with visual beats
        /// </summary>
        public static void MetronomeTickWarning(Vector2 center, Color color, int ticksRemaining, int maxTicks)
        {
            float progress = 1f - (float)ticksRemaining / maxTicks;
            
            // Tick marks around center
            for (int i = 0; i < maxTicks; i++)
            {
                float angle = MathHelper.TwoPi * i / maxTicks - MathHelper.PiOver2;
                Vector2 tickPos = center + angle.ToRotationVector2() * 50f;
                
                bool isPast = i < (maxTicks - ticksRemaining);
                Color tickColor = isPast ? Color.Red : color;
                float tickScale = isPast ? 0.4f : 0.25f;
                
                CustomParticles.GenericFlare(tickPos, tickColor, tickScale, 10);
            }
            
            // Central metronome visualization
            float pendulumAngle = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * MathHelper.PiOver4;
            Vector2 pendulumTip = center + new Vector2(0, -30f).RotatedBy(pendulumAngle);
            
            EnhancedParticles.BloomFlare(center, color, 0.2f, 8, 2, 0.4f);
            CustomParticles.GenericFlare(pendulumTip, Color.Gold, 0.3f + progress * 0.2f, 6);
        }
        
        /// <summary>
        /// #5 - Chord Buildup Spiral: Notes spiral inward as chord charges
        /// </summary>
        public static void ChordBuildupSpiral(Vector2 center, Color[] chordColors, float progress)
        {
            if (chordColors == null || chordColors.Length == 0) return;
            
            float maxRadius = 120f;
            float currentRadius = maxRadius * (1f - progress);
            
            for (int note = 0; note < chordColors.Length; note++)
            {
                float spiralAngle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * note / chordColors.Length;
                spiralAngle += progress * MathHelper.TwoPi * 2f; // Spiral faster as it converges
                
                Vector2 notePos = center + spiralAngle.ToRotationVector2() * currentRadius;
                
                // Notes grow brighter as they converge
                float noteScale = 0.5f + progress * 0.5f;
                ThemedParticles.MusicNote(notePos, (center - notePos) * 0.05f, chordColors[note], noteScale, 15);
                
                // Sparkle trail
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new SparkleParticle(notePos, (center - notePos) * 0.02f, chordColors[note], 0.3f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Central chord glow growing
            if (progress > 0.5f)
            {
                Color avgColor = chordColors[0];
                for (int i = 1; i < chordColors.Length; i++)
                    avgColor = Color.Lerp(avgColor, chordColors[i], 0.5f);
                
                EnhancedParticles.BloomFlare(center, avgColor, 0.3f * (progress - 0.5f) * 2f, 12, 3, 0.6f);
            }
        }
        
        /// <summary>
        /// #6 - Fortissimo Flash Warning: Bright "ff" flash before powerful attack
        /// </summary>
        public static void FortissimoFlashWarning(Vector2 center, Color color, float intensity)
        {
            // Multiple pulsing layers representing volume
            int layers = 3 + (int)(intensity * 3);
            
            for (int i = 0; i < layers; i++)
            {
                float layerDelay = i * 0.15f;
                float pulse = (float)Math.Sin((Main.GameUpdateCount * 0.2f + layerDelay) * MathHelper.Pi);
                pulse = Math.Max(0, pulse); // Only positive pulses
                
                float scale = 0.3f + i * 0.1f + pulse * 0.2f * intensity;
                float alpha = (1f - i * 0.15f) * intensity;
                
                Color layerColor = Color.Lerp(color, Color.White, pulse * 0.5f);
                EnhancedParticles.BloomFlare(center, layerColor * alpha, scale, 8, 2, 0.5f);
            }
            
            // Screen shake simulation through particle jitter
            if (intensity > 0.7f && Main.rand.NextBool(3))
            {
                Vector2 jitter = Main.rand.NextVector2Circular(intensity * 8f, intensity * 8f);
                CustomParticles.GenericFlare(center + jitter, Color.White, 0.4f, 6);
            }
        }
        
        /// <summary>
        /// #7 - Fermata Hold Indicator: Time-freeze visual showing attack paused
        /// </summary>
        public static void FermataHoldIndicator(Vector2 center, Color color, float holdProgress)
        {
            // Fermata symbol (curved arc over dot)
            // Draw arc
            int arcPoints = 12;
            float arcRadius = 30f;
            
            for (int i = 0; i < arcPoints; i++)
            {
                float t = i / (float)(arcPoints - 1);
                float angle = MathHelper.Lerp(-MathHelper.PiOver2 - 0.8f, -MathHelper.PiOver2 + 0.8f, t);
                Vector2 arcPos = center + new Vector2(0, -20f) + angle.ToRotationVector2() * arcRadius;
                
                Dust arcDust = Dust.NewDustPerfect(arcPos, DustID.GoldCoin, Vector2.Zero, 0, color, 0.5f);
                arcDust.noGravity = true;
            }
            
            // Central dot
            EnhancedParticles.BloomFlare(center + new Vector2(0, -20f), color, 0.2f + holdProgress * 0.1f, 10, 2, 0.5f);
            
            // Time ripples showing held state
            float ripplePhase = Main.GameUpdateCount * 0.05f;
            float rippleAlpha = 0.3f + holdProgress * 0.3f;
            CustomParticles.HaloRing(center, color * rippleAlpha, 0.3f + (float)Math.Sin(ripplePhase) * 0.1f, 15);
        }
        
        /// <summary>
        /// #8 - Accelerando Spiral: Particles speed up as attack accelerates
        /// </summary>
        public static void AccelerandoSpiral(Vector2 center, Color color, float speedMultiplier, int particleCount = 12)
        {
            float baseSpeed = 0.02f;
            float currentSpeed = baseSpeed * speedMultiplier;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.GameUpdateCount * currentSpeed + MathHelper.TwoPi * i / particleCount;
                float radius = 40f + (float)Math.Sin(Main.GameUpdateCount * currentSpeed * 2f + i) * 15f;
                
                Vector2 particlePos = center + angle.ToRotationVector2() * radius;
                
                // Particles stretch based on speed
                Vector2 stretchDir = angle.ToRotationVector2();
                float stretchAmount = speedMultiplier * 5f;
                
                Dust speedDust = Dust.NewDustPerfect(particlePos, DustID.GoldCoin, stretchDir * stretchAmount, 0, color, 0.6f + speedMultiplier * 0.3f);
                speedDust.noGravity = true;
            }
            
            // Speed lines radiating when fast enough
            if (speedMultiplier > 1.5f)
            {
                int lineCount = (int)((speedMultiplier - 1.5f) * 4);
                for (int i = 0; i < lineCount; i++)
                {
                    float lineAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 lineStart = center + lineAngle.ToRotationVector2() * 30f;
                    Vector2 lineEnd = center + lineAngle.ToRotationVector2() * (30f + speedMultiplier * 20f);
                    
                    var spark = new GlowSparkParticle(lineStart, (lineEnd - lineStart) * 0.1f, true, 8, 0.2f, color, new Vector2(0.02f, speedMultiplier));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
        }
        
        /// <summary>
        /// #9 - Glissando Slide Warning: Smooth slide across target area
        /// </summary>
        public static void GlissandoSlideWarning(Vector2 start, Vector2 end, Color color, float progress)
        {
            // Current position on glissando
            Vector2 currentPos = Vector2.Lerp(start, end, progress);
            
            // Draw slide line
            int lineParticles = 20;
            for (int i = 0; i < lineParticles; i++)
            {
                float t = i / (float)(lineParticles - 1);
                Vector2 linePos = Vector2.Lerp(start, end, t);
                
                float alpha = t <= progress ? 0.8f : 0.3f;
                Dust lineDust = Dust.NewDustPerfect(linePos, DustID.GoldCoin, Vector2.Zero, 0, color * alpha, 0.4f);
                lineDust.noGravity = true;
            }
            
            // Current position indicator
            EnhancedParticles.BloomFlare(currentPos, color, 0.4f, 10, 2, 0.6f);
            
            // Notes at chromatic intervals
            int noteCount = 7; // One octave
            for (int i = 0; i < noteCount; i++)
            {
                float noteT = i / (float)(noteCount - 1);
                if (noteT <= progress)
                {
                    Vector2 notePos = Vector2.Lerp(start, end, noteT);
                    if (Main.rand.NextBool(8))
                    {
                        ThemedParticles.MusicNote(notePos, Vector2.Zero, color, 0.6f, 12);
                    }
                }
            }
        }
        
        /// <summary>
        /// #10 - Trill Vibration Warning: Rapid alternating notes showing impending attack
        /// </summary>
        public static void TrillVibrationWarning(Vector2 center, Color colorA, Color colorB, float intensity)
        {
            // Alternating note positions
            float trillSpeed = 0.3f * intensity;
            bool isA = ((int)(Main.GameUpdateCount * trillSpeed) % 2) == 0;
            
            float offsetX = (float)Math.Sin(Main.GameUpdateCount * trillSpeed * MathHelper.TwoPi) * 15f;
            Vector2 notePos = center + new Vector2(offsetX, 0);
            
            Color currentColor = isA ? colorA : colorB;
            ThemedParticles.MusicNote(notePos, Vector2.Zero, currentColor, 0.7f + intensity * 0.3f, 10);
            
            // Trill lines showing the oscillation
            for (int i = -3; i <= 3; i++)
            {
                Vector2 trillPos = center + new Vector2(i * 5f, 0);
                float trillAlpha = 0.3f * (1f - Math.Abs(i) * 0.15f) * intensity;
                
                Dust trillDust = Dust.NewDustPerfect(trillPos, DustID.GoldCoin, Vector2.Zero, 0, Color.Lerp(colorA, colorB, 0.5f) * trillAlpha, 0.3f);
                trillDust.noGravity = true;
            }
        }
        
        #endregion
        
        #region Impact/Explosion Effects (16-35)
        
        /// <summary>
        /// #16 - Cymbal Crash Burst: Radial brass-colored shockwave with harmonic overtones
        /// </summary>
        public static void CymbalCrashBurst(Vector2 center, float intensity = 1f)
        {
            Color brassGold = new Color(218, 165, 32);
            Color brassLight = new Color(255, 223, 150);
            
            // Main crash ring
            CustomParticles.HaloRing(center, brassGold, 0.5f * intensity, 20);
            
            // Harmonic overtone rings (at musical intervals)
            float[] intervals = { 0.5f, 0.667f, 0.75f, 0.8f }; // Octave, fifth, fourth, major third
            for (int i = 0; i < intervals.Length; i++)
            {
                float delay = i * 0.15f;
                float scale = 0.3f * intervals[i] * intensity;
                float alpha = 0.6f - i * 0.1f;
                
                CustomParticles.HaloRing(center, brassLight * alpha, scale, 15 + i * 2);
            }
            
            // Radial sparks like cymbal shimmer
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * intensity;
                
                var spark = new SparkleParticle(center, sparkVel, brassGold, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Central crash flash
            EnhancedParticles.BloomFlare(center, brassLight, 0.6f * intensity, 15, 4, 1f);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f * intensity }, center);
        }
        
        /// <summary>
        /// #17 - Timpani Drumroll Impact: Deep bass explosion with rolling aftershocks
        /// </summary>
        public static void TimpaniDrumrollImpact(Vector2 center, Color color, float intensity = 1f)
        {
            // Main impact
            EnhancedParticles.BloomFlare(center, Color.White, 0.7f * intensity, 20, 4, 1f);
            EnhancedParticles.BloomFlare(center, color, 0.55f * intensity, 25, 3, 0.8f);
            
            // Deep bass ripples
            int rippleCount = 5;
            for (int i = 0; i < rippleCount; i++)
            {
                float rippleDelay = i * 0.2f;
                float rippleScale = (0.3f + i * 0.15f) * intensity;
                float rippleAlpha = 0.8f - i * 0.12f;
                
                // Stagger the halos
                CustomParticles.HaloRing(center, color * rippleAlpha, rippleScale, 12 + i * 4);
            }
            
            // Ground-hugging dust wave
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                dustVel.Y = Math.Abs(dustVel.Y) * 0.3f; // Flatten to ground
                
                Dust groundDust = Dust.NewDustPerfect(center, DustID.Smoke, dustVel, 100, color * 0.5f, 1.2f);
                groundDust.noGravity = false;
            }
            
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.7f * intensity }, center);
        }
        
        /// <summary>
        /// #18 - Chord Resolution Bloom: Satisfying harmonic explosion with color gradient
        /// </summary>
        public static void ChordResolutionBloom(Vector2 center, Color[] chordColors, float scale = 1f)
        {
            if (chordColors == null || chordColors.Length == 0) return;
            
            // Each chord note blooms outward in sequence
            for (int i = 0; i < chordColors.Length; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / chordColors.Length;
                float delay = i * 0.12f;
                
                // Note bloom at angle
                Vector2 bloomDir = noteAngle.ToRotationVector2();
                Vector2 bloomPos = center + bloomDir * 25f;
                
                EnhancedParticles.BloomFlare(bloomPos, chordColors[i], 0.4f * scale, 18, 3, 0.7f);
                
                // Trailing particles
                for (int p = 0; p < 4; p++)
                {
                    Vector2 trailVel = bloomDir * Main.rand.NextFloat(3f, 7f) * scale;
                    var glow = new GenericGlowParticle(center, trailVel, chordColors[i] * 0.7f, 0.25f, 20, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Central harmonic convergence
            Color avgColor = chordColors[0];
            for (int i = 1; i < chordColors.Length; i++)
                avgColor = Color.Lerp(avgColor, chordColors[i], 1f / (i + 1));
            
            EnhancedParticles.BloomFlare(center, Color.White, 0.5f * scale, 15, 4, 1f);
            EnhancedParticles.BloomFlare(center, avgColor, 0.4f * scale, 20, 3, 0.8f);
            
            // Music note scatter
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                ThemedParticles.MusicNote(center, noteVel, chordColors[i % chordColors.Length], 0.75f, 25);
            }
        }
        
        /// <summary>
        /// #19 - Staccato Multi-Burst: Rapid series of short sharp explosions
        /// </summary>
        public static void StaccatoMultiBurst(Vector2 center, Color color, int burstCount, float spread = 30f)
        {
            for (int burst = 0; burst < burstCount; burst++)
            {
                // Random offset for each staccato hit
                Vector2 offset = Main.rand.NextVector2Circular(spread, spread);
                Vector2 burstPos = center + offset;
                
                // Sharp, short burst
                EnhancedParticles.BloomFlare(burstPos, color, 0.35f, 8, 2, 0.5f);
                CustomParticles.GenericFlare(burstPos, Color.White * 0.8f, 0.25f, 6);
                
                // Quick radial sparks
                for (int s = 0; s < 4; s++)
                {
                    float angle = MathHelper.TwoPi * s / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                    
                    Dust sparkDust = Dust.NewDustPerfect(burstPos, DustID.GoldCoin, sparkVel, 0, color, 0.6f);
                    sparkDust.noGravity = true;
                }
            }
        }
        
        /// <summary>
        /// #20 - Sforzando Spike: Sudden dramatic impact spike
        /// </summary>
        public static void SforzandoSpike(Vector2 center, Color color, float intensity = 1f)
        {
            // Central spike burst
            EnhancedParticles.BloomFlare(center, Color.White, 0.9f * intensity, 12, 4, 1f);
            EnhancedParticles.BloomFlare(center, color, 0.7f * intensity, 18, 3, 0.9f);
            
            // Radial spike lines (8 pointed)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.PiOver4 * i;
                Vector2 spikeDir = angle.ToRotationVector2();
                
                // Spike is longer
                float spikeLength = 60f * intensity;
                
                for (int p = 0; p < 8; p++)
                {
                    float t = p / 7f;
                    Vector2 spikePos = center + spikeDir * (t * spikeLength);
                    float spikeScale = 0.4f * (1f - t * 0.5f) * intensity;
                    
                    CustomParticles.GenericFlare(spikePos, color, spikeScale, 10);
                }
            }
            
            // Shockwave
            CustomParticles.HaloRing(center, color, 0.6f * intensity, 18);
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1f * intensity }, center);
        }
        
        /// <summary>
        /// #21 - Pizzicato Pop: Light, bouncy impact effect
        /// </summary>
        public static void PizzicatoPop(Vector2 center, Color color)
        {
            // Light pop
            EnhancedParticles.BloomFlare(center, color, 0.3f, 10, 2, 0.5f);
            CustomParticles.GenericFlare(center, Color.White * 0.6f, 0.2f, 8);
            
            // Bouncy dust
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 popVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                popVel.Y -= 2f; // Slight upward bias for bounce feel
                
                Dust popDust = Dust.NewDustPerfect(center, DustID.GoldCoin, popVel, 0, color, 0.8f);
                popDust.noGravity = false; // Let gravity affect for bounce
            }
            
            // Small note
            ThemedParticles.MusicNote(center, new Vector2(0, -2f), color, 0.5f, 15);
            
            SoundEngine.PlaySound(SoundID.Item9 with { Pitch = 0.6f, Volume = 0.4f }, center);
        }
        
        /// <summary>
        /// #22 - Legato Wave Wash: Smooth, connected impact wave
        /// </summary>
        public static void LegatoWaveWash(Vector2 center, Vector2 direction, Color color, float length = 100f)
        {
            direction = direction.SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Smooth wave across direction
            int wavePoints = 15;
            for (int i = 0; i < wavePoints; i++)
            {
                float t = i / (float)(wavePoints - 1);
                float waveOffset = (float)Math.Sin(t * MathHelper.Pi) * 20f;
                Vector2 wavePos = center + direction * (t - 0.5f) * length + perpendicular * waveOffset;
                
                float alpha = (float)Math.Sin(t * MathHelper.Pi); // Fade at edges
                CustomParticles.GenericFlare(wavePos, color * alpha, 0.25f, 15);
                
                // Smooth glow particles
                if (Main.rand.NextBool(3))
                {
                    var glow = new GenericGlowParticle(wavePos, perpendicular * waveOffset * 0.05f, color * 0.5f, 0.2f, 18, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Connecting ribbon
            EnhancedParticles.BloomFlare(center, color, 0.35f, 15, 2, 0.5f);
        }
        
        /// <summary>
        /// #23 - Arpeggio Cascade: Notes ripple outward in sequence
        /// </summary>
        public static void ArpeggioCascade(Vector2 center, Color baseColor, int noteCount = 8, float spread = 80f)
        {
            for (int i = 0; i < noteCount; i++)
            {
                // Staggered radial positions
                float angle = MathHelper.TwoPi * i / noteCount;
                float radius = 20f + (i * spread / noteCount);
                Vector2 notePos = center + angle.ToRotationVector2() * radius;
                
                // Color gradient through arpeggio
                float hueShift = (float)i / noteCount * 0.15f;
                Color noteColor = RotateHue(baseColor, hueShift);
                
                // Note with trail back to center
                ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 2f, noteColor, 0.7f, 20 + i * 2);
                
                // Connecting line to previous note
                if (i > 0)
                {
                    float prevAngle = MathHelper.TwoPi * (i - 1) / noteCount;
                    float prevRadius = 20f + ((i - 1) * spread / noteCount);
                    Vector2 prevPos = center + prevAngle.ToRotationVector2() * prevRadius;
                    
                    Vector2 midPoint = Vector2.Lerp(prevPos, notePos, 0.5f);
                    CustomParticles.GenericFlare(midPoint, noteColor * 0.5f, 0.15f, 10);
                }
            }
            
            // Central arpeggio root
            EnhancedParticles.BloomFlare(center, baseColor, 0.4f, 15, 3, 0.6f);
        }
        
        // Helper function to rotate hue
        private static Color RotateHue(Color color, float amount)
        {
            Vector3 hsv = Main.rgbToHsl(color);
            hsv.X = (hsv.X + amount) % 1f;
            return Main.hslToRgb(hsv.X, hsv.Y, hsv.Z);
        }
        
        /// <summary>
        /// #24 - Fermata Release Burst: Held tension exploding outward
        /// </summary>
        public static void FermataReleaseBurst(Vector2 center, Color color, float holdDuration)
        {
            // Scale explosion based on hold duration
            float intensity = Math.Min(2f, 0.5f + holdDuration * 0.5f);
            
            // Main release explosion
            EnhancedParticles.BloomFlare(center, Color.White, 0.8f * intensity, 20, 4, 1f);
            EnhancedParticles.BloomFlare(center, color, 0.65f * intensity, 25, 3, 0.9f);
            
            // Radial release waves
            for (int wave = 0; wave < 3; wave++)
            {
                float waveScale = (0.4f + wave * 0.2f) * intensity;
                CustomParticles.HaloRing(center, color, waveScale, 15 + wave * 5);
            }
            
            // Held energy particles bursting free
            int particleCount = (int)(12 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f) * intensity;
                
                var glow = new GenericGlowParticle(center, burstVel, color * 0.8f, 0.3f, 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Music notes representing released phrase
            int noteCount = (int)(6 * intensity);
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / noteCount;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                ThemedParticles.MusicNote(center, noteVel, color, 0.8f, 30);
            }
        }
        
        /// <summary>
        /// #25 - Tutti Full Ensemble: Massive all-instruments explosion
        /// </summary>
        public static void TuttiFullEnsemble(Vector2 center, Color[] instrumentColors, float intensity = 1f)
        {
            // Default colors if none provided
            if (instrumentColors == null || instrumentColors.Length == 0)
            {
                instrumentColors = new Color[]
                {
                    new Color(139, 90, 43),  // Strings - brown
                    new Color(218, 165, 32), // Brass - gold
                    new Color(192, 192, 192), // Woodwinds - silver
                    Color.White               // Percussion - white
                };
            }
            
            // Central tutti burst
            EnhancedParticles.BloomFlare(center, Color.White, 1f * intensity, 25, 4, 1f);
            
            // Each instrument section explodes in its direction
            for (int section = 0; section < instrumentColors.Length; section++)
            {
                float sectionAngle = MathHelper.TwoPi * section / instrumentColors.Length;
                Vector2 sectionDir = sectionAngle.ToRotationVector2();
                Vector2 sectionCenter = center + sectionDir * 30f * intensity;
                
                // Section burst
                EnhancedParticles.BloomFlare(sectionCenter, instrumentColors[section], 0.5f * intensity, 18, 3, 0.7f);
                
                // Section particles
                for (int p = 0; p < 8; p++)
                {
                    float pAngle = sectionAngle + Main.rand.NextFloat(-0.5f, 0.5f);
                    Vector2 pVel = pAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * intensity;
                    
                    var glow = new GenericGlowParticle(sectionCenter, pVel, instrumentColors[section] * 0.7f, 0.25f, 22, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // Massive shockwave
            CustomParticles.HaloRing(center, Color.White, 0.8f * intensity, 25);
            
            // Music note storm
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f) * intensity;
                ThemedParticles.MusicNote(center, noteVel, instrumentColors[i % instrumentColors.Length], 0.8f, 30);
            }
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.2f * intensity }, center);
        }
        
        #endregion
        
        #region Beam/Laser Effects (36-50)
        
        /// <summary>
        /// #36 - Sound Wave Beam: Visible sound wave propagating with frequency visualization
        /// </summary>
        public static void SoundWaveBeam(Vector2 start, Vector2 end, Color color, float frequency = 1f)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            float length = Vector2.Distance(start, end);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Wave visualization along beam
            int wavePoints = (int)(length / 8f);
            for (int i = 0; i < wavePoints; i++)
            {
                float t = i / (float)(wavePoints - 1);
                float wavePhase = Main.GameUpdateCount * 0.1f * frequency;
                float amplitude = (float)Math.Sin(t * MathHelper.TwoPi * 3f * frequency + wavePhase) * 10f;
                
                Vector2 wavePos = Vector2.Lerp(start, end, t) + perpendicular * amplitude;
                
                float alpha = (float)Math.Sin(t * MathHelper.Pi); // Fade at ends
                Dust waveDust = Dust.NewDustPerfect(wavePos, DustID.GoldCoin, perpendicular * amplitude * 0.02f, 0, color * alpha, 0.6f);
                waveDust.noGravity = true;
            }
            
            // Core beam glow
            int corePoints = 10;
            for (int i = 0; i < corePoints; i++)
            {
                float t = i / (float)(corePoints - 1);
                Vector2 corePos = Vector2.Lerp(start, end, t);
                CustomParticles.GenericFlare(corePos, color * 0.5f, 0.2f, 6);
            }
            
            // End points
            EnhancedParticles.BloomFlare(start, color, 0.3f, 10, 2, 0.5f);
            EnhancedParticles.BloomFlare(end, color, 0.4f, 12, 2, 0.6f);
        }
        
        /// <summary>
        /// #37 - Staff Line Laser: 5-line musical staff as deadly laser
        /// </summary>
        public static void StaffLineLaser(Vector2 start, Vector2 end, Color color, float width = 40f)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            float length = Vector2.Distance(start, end);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // 5 parallel lines
            for (int line = -2; line <= 2; line++)
            {
                Vector2 lineOffset = perpendicular * (line * width / 5f);
                Vector2 lineStart = start + lineOffset;
                Vector2 lineEnd = end + lineOffset;
                
                // Draw line particles
                int lineParticles = (int)(length / 15f);
                for (int i = 0; i < lineParticles; i++)
                {
                    float t = i / (float)(lineParticles - 1);
                    Vector2 particlePos = Vector2.Lerp(lineStart, lineEnd, t);
                    
                    float lineAlpha = line == 0 ? 0.9f : 0.6f; // Center line brighter
                    Dust lineDust = Dust.NewDustPerfect(particlePos, DustID.GoldCoin, Vector2.Zero, 0, color * lineAlpha, 0.5f);
                    lineDust.noGravity = true;
                }
            }
            
            // Random notes on the staff
            if (Main.rand.NextBool(4))
            {
                float t = Main.rand.NextFloat();
                int line = Main.rand.Next(-2, 3);
                Vector2 notePos = Vector2.Lerp(start, end, t) + perpendicular * (line * width / 5f);
                ThemedParticles.MusicNote(notePos, direction * 3f, color, 0.65f, 15);
            }
        }
        
        /// <summary>
        /// #38 - Crescendo Laser: Beam that intensifies over duration
        /// </summary>
        public static void CrescendoLaser(Vector2 start, Vector2 end, Color color, float intensity, float maxIntensity = 1f)
        {
            float normalizedIntensity = intensity / maxIntensity;
            
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            float length = Vector2.Distance(start, end);
            
            // Beam width increases with intensity
            float beamWidth = 5f + normalizedIntensity * 15f;
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Core beam
            int corePoints = (int)(length / 10f);
            for (int i = 0; i < corePoints; i++)
            {
                float t = i / (float)(corePoints - 1);
                Vector2 corePos = Vector2.Lerp(start, end, t);
                
                // Random offset within beam width
                Vector2 offset = perpendicular * Main.rand.NextFloat(-beamWidth / 2f, beamWidth / 2f);
                
                float scale = 0.2f + normalizedIntensity * 0.3f;
                CustomParticles.GenericFlare(corePos + offset, color, scale, 8);
            }
            
            // Dynamic markings along beam (pp, p, mp, mf, f, ff)
            string[] dynamics = { "pp", "p", "mp", "mf", "f", "ff" };
            int currentDynamic = (int)(normalizedIntensity * (dynamics.Length - 1));
            
            // Visual representation at intervals
            for (int d = 0; d <= currentDynamic; d++)
            {
                float t = (d + 0.5f) / dynamics.Length;
                Vector2 dynamicPos = Vector2.Lerp(start, end, t);
                float dynamicScale = 0.15f + d * 0.05f;
                EnhancedParticles.BloomFlare(dynamicPos, color, dynamicScale, 10, 2, 0.4f + d * 0.1f);
            }
            
            // End point glow scales with intensity
            EnhancedParticles.BloomFlare(end, color, 0.3f + normalizedIntensity * 0.4f, 15, 3, 0.5f + normalizedIntensity * 0.5f);
        }
        
        /// <summary>
        /// #39 - Harmonic Overtone Beam: Main beam with harmonic frequency beams
        /// </summary>
        public static void HarmonicOvertoneBeam(Vector2 start, Vector2 end, Color fundamentalColor)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            float length = Vector2.Distance(start, end);
            Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
            
            // Fundamental beam (center)
            DrawBeamLine(start, end, fundamentalColor, 0.6f);
            
            // Harmonic overtones at musical ratios
            float[] harmonicRatios = { 0.5f, 0.333f, 0.25f, 0.2f }; // Octave, 5th, etc.
            Color[] harmonicColors = {
                RotateHue(fundamentalColor, 0.1f),
                RotateHue(fundamentalColor, 0.2f),
                RotateHue(fundamentalColor, 0.3f),
                RotateHue(fundamentalColor, 0.4f)
            };
            
            for (int h = 0; h < harmonicRatios.Length; h++)
            {
                float offset = 15f + h * 8f;
                float alpha = 0.5f - h * 0.1f;
                
                // Upper harmonic
                Vector2 upperStart = start + perpendicular * offset;
                Vector2 upperEnd = end + perpendicular * offset;
                DrawBeamLine(upperStart, upperEnd, harmonicColors[h] * alpha, 0.3f);
                
                // Lower harmonic
                Vector2 lowerStart = start - perpendicular * offset;
                Vector2 lowerEnd = end - perpendicular * offset;
                DrawBeamLine(lowerStart, lowerEnd, harmonicColors[h] * alpha, 0.3f);
            }
        }
        
        private static void DrawBeamLine(Vector2 start, Vector2 end, Color color, float scale)
        {
            int points = (int)(Vector2.Distance(start, end) / 12f);
            for (int i = 0; i < points; i++)
            {
                float t = i / (float)(points - 1);
                Vector2 pos = Vector2.Lerp(start, end, t);
                Dust beamDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero, 0, color, scale);
                beamDust.noGravity = true;
            }
        }
        
        /// <summary>
        /// #40 - Glissando Sweep Laser: Smooth frequency sweep across area
        /// </summary>
        public static void GlissandoSweepLaser(Vector2 origin, float startAngle, float endAngle, float length, Color color, float progress)
        {
            float currentAngle = MathHelper.Lerp(startAngle, endAngle, progress);
            Vector2 beamEnd = origin + currentAngle.ToRotationVector2() * length;
            
            // Main sweep beam
            DrawBeamLine(origin, beamEnd, color, 0.7f);
            
            // Trail showing sweep path
            int trailSteps = 8;
            for (int i = 0; i < trailSteps; i++)
            {
                float trailProgress = progress - (i * 0.03f);
                if (trailProgress < 0) continue;
                
                float trailAngle = MathHelper.Lerp(startAngle, endAngle, trailProgress);
                Vector2 trailEnd = origin + trailAngle.ToRotationVector2() * length;
                
                float trailAlpha = 0.3f * (1f - i * 0.1f);
                DrawBeamLine(origin, trailEnd, color * trailAlpha, 0.3f);
            }
            
            // Sweep tip
            EnhancedParticles.BloomFlare(beamEnd, color, 0.4f, 10, 2, 0.6f);
            
            // Chromatic notes at sweep position
            if (Main.rand.NextBool(5))
            {
                ThemedParticles.MusicNote(beamEnd, currentAngle.ToRotationVector2() * 2f, color, 0.6f, 15);
            }
        }
        
        #endregion
        
        #region Area Denial Effects (51-60)
        
        /// <summary>
        /// #51 - Rest Zone: Musical rest symbols marking safe/danger zones
        /// </summary>
        public static void RestZone(Vector2 center, float radius, Color color, bool isDanger)
        {
            // Circular boundary
            int boundaryPoints = 24;
            for (int i = 0; i < boundaryPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / boundaryPoints;
                Vector2 boundaryPos = center + angle.ToRotationVector2() * radius;
                
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.3f) * 0.2f + 0.8f;
                Color boundaryColor = isDanger ? Color.Red : Color.Cyan;
                boundaryColor = Color.Lerp(boundaryColor, color, 0.5f);
                
                Dust boundaryDust = Dust.NewDustPerfect(boundaryPos, DustID.GoldCoin, Vector2.Zero, 0, boundaryColor * pulse, 0.5f);
                boundaryDust.noGravity = true;
            }
            
            // Rest symbol in center (represented by pattern)
            float restPulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            EnhancedParticles.BloomFlare(center, isDanger ? Color.Red * 0.5f : color * 0.5f, 0.3f * restPulse, 12, 2, 0.4f);
            
            // Ambient particles within zone
            if (Main.rand.NextBool(8))
            {
                Vector2 ambientPos = center + Main.rand.NextVector2Circular(radius * 0.8f, radius * 0.8f);
                CustomParticles.GenericFlare(ambientPos, color * 0.3f, 0.15f, 15);
            }
        }
        
        /// <summary>
        /// #52 - Tempo Zone: Area with beat-synchronized damage pulses
        /// </summary>
        public static void TempoZone(Vector2 center, float radius, Color color, float bpm)
        {
            bool onBeat = IsOnBeat(bpm);
            
            // Boundary with beat pulse
            int boundaryPoints = 20;
            for (int i = 0; i < boundaryPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / boundaryPoints;
                float pulseRadius = radius + (onBeat ? 10f : 0f);
                Vector2 boundaryPos = center + angle.ToRotationVector2() * pulseRadius;
                
                float scale = onBeat ? 0.7f : 0.4f;
                Dust boundaryDust = Dust.NewDustPerfect(boundaryPos, DustID.GoldCoin, Vector2.Zero, 0, color, scale);
                boundaryDust.noGravity = true;
            }
            
            // Beat flash
            if (onBeat)
            {
                EnhancedParticles.BloomFlare(center, color, 0.5f, 10, 3, 0.7f);
                CustomParticles.HaloRing(center, color, radius / 100f, 12);
            }
            
            // BPM indicator
            float beatProgress = (Main.GameUpdateCount % GetFramesPerBeat(bpm)) / GetFramesPerBeat(bpm);
            float indicatorAngle = -MathHelper.PiOver2 + beatProgress * MathHelper.TwoPi;
            Vector2 indicatorPos = center + indicatorAngle.ToRotationVector2() * (radius * 0.7f);
            CustomParticles.GenericFlare(indicatorPos, Color.White, 0.25f, 6);
        }
        
        /// <summary>
        /// #53 - Harmony Field: Area that rewards staying in sync with boss rhythm
        /// </summary>
        public static void HarmonyField(Vector2 center, float radius, Color[] chordColors, float harmonyLevel)
        {
            if (chordColors == null || chordColors.Length == 0) return;
            
            // Field boundary with chord colors
            int segments = chordColors.Length * 6;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                Color segmentColor = chordColors[i % chordColors.Length];
                float alpha = 0.3f + harmonyLevel * 0.5f;
                
                Dust segmentDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero, 0, segmentColor * alpha, 0.5f);
                segmentDust.noGravity = true;
            }
            
            // Harmony glow (brighter when in sync)
            Color avgColor = chordColors[0];
            for (int i = 1; i < chordColors.Length; i++)
                avgColor = Color.Lerp(avgColor, chordColors[i], 0.5f);
            
            float glowIntensity = 0.2f + harmonyLevel * 0.5f;
            EnhancedParticles.BloomFlare(center, avgColor, glowIntensity, 15, 2, 0.3f + harmonyLevel * 0.4f);
            
            // Musical notes when harmony is high
            if (harmonyLevel > 0.5f && Main.rand.NextBool(6))
            {
                Vector2 notePos = center + Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f);
                ThemedParticles.MusicNote(notePos, Vector2.Zero, chordColors[Main.rand.Next(chordColors.Length)], 0.6f + harmonyLevel * 0.3f, 20);
            }
        }
        
        /// <summary>
        /// #54 - Dissonance Storm: Chaotic area with clashing visual elements
        /// </summary>
        public static void DissonanceStorm(Vector2 center, float radius, Color colorA, Color colorB)
        {
            // Chaotic boundary with clashing colors
            int boundaryPoints = 30;
            for (int i = 0; i < boundaryPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / boundaryPoints;
                // Irregular radius
                float irregularRadius = radius + Main.rand.NextFloat(-10f, 10f);
                Vector2 pos = center + angle.ToRotationVector2() * irregularRadius;
                
                // Rapidly alternating colors
                Color dustColor = ((i + Main.GameUpdateCount / 3) % 2 == 0) ? colorA : colorB;
                
                Dust stormDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Main.rand.NextVector2Circular(1f, 1f), 0, dustColor, 0.6f);
                stormDust.noGravity = true;
            }
            
            // Inner chaos
            for (int i = 0; i < 3; i++)
            {
                Vector2 chaosPos = center + Main.rand.NextVector2Circular(radius * 0.7f, radius * 0.7f);
                Color chaosColor = Main.rand.NextBool() ? colorA : colorB;
                CustomParticles.GenericFlare(chaosPos, chaosColor, 0.25f, 8);
            }
            
            // Clashing sparks
            if (Main.rand.NextBool(4))
            {
                Vector2 clashPos = center + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f);
                EnhancedParticles.BloomFlare(clashPos, Color.White, 0.3f, 6, 2, 0.5f);
            }
        }
        
        /// <summary>
        /// #55 - Crescendo Ring: Expanding danger zone that grows in intensity
        /// </summary>
        public static void CrescendoRing(Vector2 center, float currentRadius, float maxRadius, Color color)
        {
            float progress = currentRadius / maxRadius;
            float intensity = 0.3f + progress * 0.7f;
            
            // Ring boundary
            int points = 30;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                Vector2 pos = center + angle.ToRotationVector2() * currentRadius;
                
                Dust ringDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, angle.ToRotationVector2() * 0.5f, 0, color, 0.4f + intensity * 0.4f);
                ringDust.noGravity = true;
            }
            
            // Inner danger visualization
            if (Main.rand.NextBool(3))
            {
                float innerRadius = Main.rand.NextFloat() * currentRadius;
                float innerAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 innerPos = center + innerAngle.ToRotationVector2() * innerRadius;
                
                CustomParticles.GenericFlare(innerPos, color * intensity, 0.2f + intensity * 0.15f, 10);
            }
            
            // Intensity indicator at center
            EnhancedParticles.BloomFlare(center, color, 0.2f + intensity * 0.3f, 12, 2, 0.3f + intensity * 0.4f);
        }
        
        #endregion
        
        #region Musical Special Effects (61-70)
        
        /// <summary>
        /// #61 - Key Change Flash: Visual key modulation effect
        /// </summary>
        public static void KeyChangeFlash(Vector2 center, Color oldKeyColor, Color newKeyColor, float transitionProgress)
        {
            // Blend colors based on progress
            Color currentColor = Color.Lerp(oldKeyColor, newKeyColor, transitionProgress);
            
            // Modulation wave
            float waveRadius = 50f + transitionProgress * 100f;
            int wavePoints = 20;
            
            for (int i = 0; i < wavePoints; i++)
            {
                float angle = MathHelper.TwoPi * i / wavePoints;
                Vector2 wavePos = center + angle.ToRotationVector2() * waveRadius;
                
                // Color splits during transition
                Color pointColor = (i % 2 == 0) ? oldKeyColor : newKeyColor;
                pointColor = Color.Lerp(pointColor, currentColor, transitionProgress);
                
                float alpha = 1f - transitionProgress * 0.5f;
                Dust waveDust = Dust.NewDustPerfect(wavePos, DustID.GoldCoin, angle.ToRotationVector2() * 2f, 0, pointColor * alpha, 0.6f);
                waveDust.noGravity = true;
            }
            
            // Central modulation burst
            EnhancedParticles.BloomFlare(center, currentColor, 0.4f + transitionProgress * 0.3f, 15, 3, 0.6f);
            
            // Key signature notes changing
            if (transitionProgress > 0.3f && transitionProgress < 0.7f && Main.rand.NextBool(3))
            {
                Vector2 noteOffset = Main.rand.NextVector2Circular(40f, 40f);
                ThemedParticles.MusicNote(center + noteOffset, noteOffset * 0.05f, newKeyColor, 0.7f, 20);
            }
        }
        
        /// <summary>
        /// #62 - Tempo Shift Distortion: Time-warp visual when tempo changes
        /// </summary>
        public static void TempoShiftDistortion(Vector2 center, float oldBPM, float newBPM, float radius = 80f)
        {
            float tempoRatio = newBPM / oldBPM;
            bool speedingUp = tempoRatio > 1f;
            
            // Distortion waves
            int waveCount = speedingUp ? 5 : 3;
            for (int w = 0; w < waveCount; w++)
            {
                float waveRadius = radius * (w + 1) / waveCount;
                int points = 16;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points;
                    float distortion = speedingUp ? 
                        (float)Math.Sin(Main.GameUpdateCount * 0.2f + i * 0.5f) * 5f :
                        (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.3f) * 3f;
                    
                    Vector2 pos = center + angle.ToRotationVector2() * (waveRadius + distortion);
                    
                    Color distortColor = speedingUp ? Color.Cyan : Color.Orange;
                    Dust distortDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero, 0, distortColor * 0.5f, 0.4f);
                    distortDust.noGravity = true;
                }
            }
            
            // Speed/slow indicators
            if (speedingUp)
            {
                // Forward arrows/streaks
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f;
                    Vector2 streakStart = center + angle.ToRotationVector2() * 20f;
                    Vector2 streakEnd = center + angle.ToRotationVector2() * 50f;
                    
                    var streak = new GlowSparkParticle(streakStart, (streakEnd - streakStart) * 0.1f, true, 10, 0.2f, Color.Cyan, new Vector2(0.02f, 2f));
                    MagnumParticleHandler.SpawnParticle(streak);
                }
            }
            
            // Central tempo indicator
            EnhancedParticles.BloomFlare(center, speedingUp ? Color.Cyan : Color.Orange, 0.4f, 12, 2, 0.5f);
        }
        
        /// <summary>
        /// #63 - Dynamics Wave: Visual representation of loud/soft dynamics
        /// </summary>
        public static void DynamicsWave(Vector2 center, float dynamicLevel, Color color)
        {
            // Dynamic levels: 0 = ppp, 0.5 = mf, 1 = fff
            float radius = 20f + dynamicLevel * 60f;
            float particleScale = 0.3f + dynamicLevel * 0.5f;
            int particleCount = 3 + (int)(dynamicLevel * 12);
            
            // Dynamic wave
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                float waveOffset = (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.5f) * (5f + dynamicLevel * 10f);
                Vector2 pos = center + angle.ToRotationVector2() * (radius + waveOffset);
                
                Color dynamicColor = Color.Lerp(color * 0.5f, color, dynamicLevel);
                
                Dust dynamicDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, angle.ToRotationVector2() * dynamicLevel * 2f, 0, dynamicColor, particleScale);
                dynamicDust.noGravity = true;
            }
            
            // Central glow scaled to dynamics
            EnhancedParticles.BloomFlare(center, color, 0.2f + dynamicLevel * 0.4f, 10, 2, 0.3f + dynamicLevel * 0.5f);
            
            // Accent marks at high dynamics
            if (dynamicLevel > 0.7f && Main.rand.NextBool(5))
            {
                Vector2 accentPos = center + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f);
                CustomParticles.GenericFlare(accentPos, Color.White, 0.25f, 8);
            }
        }
        
        /// <summary>
        /// #64 - Syncopation Stutter: Off-beat visual stutter effect
        /// </summary>
        public static void SyncopationStutter(Vector2 center, Color color, float bpm)
        {
            // Syncopated timing (off the main beat)
            float halfBeatFrames = GetFramesPerBeat(bpm) / 2f;
            bool onOffBeat = ((int)(Main.GameUpdateCount % halfBeatFrames) < 4) && !IsOnBeat(bpm);
            
            if (onOffBeat)
            {
                // Stutter burst
                EnhancedParticles.BloomFlare(center, color, 0.35f, 8, 2, 0.5f);
                
                // Jerky offset particles
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                    CustomParticles.GenericFlare(center + offset, color * 0.7f, 0.2f, 6);
                }
            }
            
            // Syncopation indicator lines
            int lineCount = 4;
            for (int i = 0; i < lineCount; i++)
            {
                float angle = MathHelper.PiOver2 * i;
                Vector2 lineEnd = center + angle.ToRotationVector2() * 25f;
                
                float alpha = onOffBeat ? 0.8f : 0.3f;
                Dust lineDust = Dust.NewDustPerfect(lineEnd, DustID.GoldCoin, Vector2.Zero, 0, color * alpha, 0.4f);
                lineDust.noGravity = true;
            }
        }
        
        /// <summary>
        /// #65 - Counterpoint Duality: Two contrasting visual streams
        /// </summary>
        public static void CounterpointDuality(Vector2 center, Color voiceAColor, Color voiceBColor, float phaseOffset = 0f)
        {
            // Voice A - upper stream
            float angleA = Main.GameUpdateCount * 0.05f + phaseOffset;
            Vector2 posA = center + new Vector2((float)Math.Cos(angleA) * 40f, -25f + (float)Math.Sin(angleA * 2f) * 10f);
            
            EnhancedParticles.BloomFlare(posA, voiceAColor, 0.25f, 10, 2, 0.4f);
            
            // Voice A trail
            if (Main.rand.NextBool(2))
            {
                var trailA = new GenericGlowParticle(posA, new Vector2(-1f, 0), voiceAColor * 0.5f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trailA);
            }
            
            // Voice B - lower stream (contrary motion)
            float angleB = -Main.GameUpdateCount * 0.05f - phaseOffset;
            Vector2 posB = center + new Vector2((float)Math.Cos(angleB) * 40f, 25f + (float)Math.Sin(angleB * 2f) * 10f);
            
            EnhancedParticles.BloomFlare(posB, voiceBColor, 0.25f, 10, 2, 0.4f);
            
            // Voice B trail
            if (Main.rand.NextBool(2))
            {
                var trailB = new GenericGlowParticle(posB, new Vector2(1f, 0), voiceBColor * 0.5f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trailB);
            }
            
            // Intersection points
            if (Math.Abs(posA.X - posB.X) < 15f)
            {
                Vector2 intersect = (posA + posB) / 2f;
                Color blendColor = Color.Lerp(voiceAColor, voiceBColor, 0.5f);
                CustomParticles.GenericFlare(intersect, blendColor, 0.3f, 8);
            }
        }
        
        /// <summary>
        /// #66 - Rubato Breath: Flexible timing visual that stretches and compresses
        /// </summary>
        public static void RubatoBreath(Vector2 center, Color color, float breathPhase)
        {
            // Breath phase: 0-0.5 = inhale (expand), 0.5-1 = exhale (compress)
            bool inhaling = breathPhase < 0.5f;
            float breathProgress = inhaling ? breathPhase * 2f : (1f - breathPhase) * 2f;
            
            float radius = 30f + breathProgress * 30f;
            float alpha = 0.5f + breathProgress * 0.3f;
            
            // Breathing ring
            int points = 20;
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                
                Dust breathDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, 
                    angle.ToRotationVector2() * (inhaling ? 0.5f : -0.5f), 0, color * alpha, 0.4f + breathProgress * 0.2f);
                breathDust.noGravity = true;
            }
            
            // Central pulse
            EnhancedParticles.BloomFlare(center, color, 0.2f + breathProgress * 0.2f, 12, 2, 0.4f + breathProgress * 0.3f);
            
            // Breath particles
            if (Main.rand.NextBool(4))
            {
                Vector2 particleVel = inhaling ? 
                    (center - (center + Main.rand.NextVector2Circular(radius, radius))).SafeNormalize(Vector2.Zero) * 2f :
                    Main.rand.NextVector2Circular(2f, 2f);
                
                var breathParticle = new GenericGlowParticle(
                    center + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f),
                    particleVel, color * 0.4f, 0.15f, 15, true);
                MagnumParticleHandler.SpawnParticle(breathParticle);
            }
        }
        
        /// <summary>
        /// #67 - Canon Echo: Delayed visual repetitions of attack
        /// </summary>
        public static void CanonEcho(Vector2 center, Color color, int echoCount, float echoDelay)
        {
            for (int echo = 0; echo < echoCount; echo++)
            {
                float echoProgress = ((Main.GameUpdateCount - echo * echoDelay) % 60f) / 60f;
                if (echoProgress < 0) continue;
                
                float echoAlpha = (1f - (float)echo / echoCount) * (1f - echoProgress);
                float echoScale = 0.4f - echo * 0.08f;
                float echoRadius = echoProgress * 80f;
                
                // Echo ring
                int points = 12;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points;
                    Vector2 pos = center + angle.ToRotationVector2() * echoRadius;
                    
                    Dust echoDust = Dust.NewDustPerfect(pos, DustID.GoldCoin, Vector2.Zero, 0, color * echoAlpha, echoScale);
                    echoDust.noGravity = true;
                }
                
                // Echo center glow
                if (echoProgress < 0.3f)
                {
                    EnhancedParticles.BloomFlare(center, color * echoAlpha, echoScale, 8, 2, 0.3f);
                }
            }
        }
        
        /// <summary>
        /// #68 - Fugue Interlace: Multiple interweaving visual themes
        /// </summary>
        public static void FugueInterlace(Vector2 center, Color[] voiceColors, float time)
        {
            if (voiceColors == null || voiceColors.Length == 0) return;
            
            int voiceCount = Math.Min(voiceColors.Length, 4);
            float[] voicePhases = new float[voiceCount];
            
            for (int v = 0; v < voiceCount; v++)
            {
                // Each voice has different entry time
                voicePhases[v] = time - v * 0.5f;
                if (voicePhases[v] < 0) continue;
                
                // Voice spiral path
                float spiralAngle = voicePhases[v] * 2f + MathHelper.TwoPi * v / voiceCount;
                float spiralRadius = 30f + (float)Math.Sin(voicePhases[v] * 3f) * 15f;
                Vector2 voicePos = center + spiralAngle.ToRotationVector2() * spiralRadius;
                
                // Voice glow
                EnhancedParticles.BloomFlare(voicePos, voiceColors[v], 0.25f, 10, 2, 0.4f);
                
                // Voice trail
                if (Main.rand.NextBool(2))
                {
                    Vector2 trailVel = (-spiralAngle).ToRotationVector2() * 1.5f;
                    var trail = new GenericGlowParticle(voicePos, trailVel, voiceColors[v] * 0.5f, 0.15f, 15, true);
                    MagnumParticleHandler.SpawnParticle(trail);
                }
                
                // Voice intersections with other voices
                for (int other = v + 1; other < voiceCount; other++)
                {
                    if (voicePhases[other] < 0) continue;
                    
                    float otherAngle = voicePhases[other] * 2f + MathHelper.TwoPi * other / voiceCount;
                    float otherRadius = 30f + (float)Math.Sin(voicePhases[other] * 3f) * 15f;
                    Vector2 otherPos = center + otherAngle.ToRotationVector2() * otherRadius;
                    
                    if (Vector2.Distance(voicePos, otherPos) < 20f)
                    {
                        // Intersection highlight
                        Color blendColor = Color.Lerp(voiceColors[v], voiceColors[other], 0.5f);
                        CustomParticles.GenericFlare((voicePos + otherPos) / 2f, blendColor, 0.2f, 6);
                    }
                }
            }
        }
        
        /// <summary>
        /// #69 - Cadence Finisher: Dramatic ending sequence
        /// </summary>
        public static void CadenceFinisher(Vector2 center, Color[] cadenceColors, float progress)
        {
            if (cadenceColors == null || cadenceColors.Length < 2) return;
            
            // Cadence phases: I-IV-V-I (simplified visual)
            int phase = (int)(progress * 4f);
            float phaseProgress = (progress * 4f) % 1f;
            
            Color currentColor = cadenceColors[Math.Min(phase, cadenceColors.Length - 1)];
            Color nextColor = cadenceColors[Math.Min(phase + 1, cadenceColors.Length - 1)];
            Color blendColor = Color.Lerp(currentColor, nextColor, phaseProgress);
            
            // Expanding resolution rings
            float ringRadius = 20f + progress * 100f;
            int ringPoints = 24;
            
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints;
                Vector2 ringPos = center + angle.ToRotationVector2() * ringRadius;
                
                Dust ringDust = Dust.NewDustPerfect(ringPos, DustID.GoldCoin, Vector2.Zero, 0, blendColor, 0.5f + progress * 0.3f);
                ringDust.noGravity = true;
            }
            
            // Central resolution glow
            float glowIntensity = 0.3f + progress * 0.5f;
            EnhancedParticles.BloomFlare(center, blendColor, glowIntensity, 15, 3, 0.4f + progress * 0.4f);
            
            // Final chord notes
            if (progress > 0.8f)
            {
                int noteCount = (int)((progress - 0.8f) * 40);
                for (int n = 0; n < noteCount; n++)
                {
                    if (Main.rand.NextBool(3))
                    {
                        float noteAngle = MathHelper.TwoPi * n / noteCount;
                        Vector2 notePos = center + noteAngle.ToRotationVector2() * Main.rand.NextFloat(20f, ringRadius * 0.8f);
                        ThemedParticles.MusicNote(notePos, noteAngle.ToRotationVector2() * 2f, blendColor, 0.7f, 20);
                    }
                }
            }
        }
        
        /// <summary>
        /// #70 - Coda Finale: Ultimate ending effect combining all elements
        /// </summary>
        public static void CodaFinale(Vector2 center, Color primaryColor, Color secondaryColor, float intensity = 1f)
        {
            // Central explosion
            EnhancedParticles.BloomFlare(center, Color.White, 1.2f * intensity, 30, 4, 1f);
            EnhancedParticles.BloomFlare(center, primaryColor, 1f * intensity, 35, 3, 0.9f);
            EnhancedParticles.BloomFlare(center, secondaryColor, 0.8f * intensity, 40, 3, 0.8f);
            
            // Cascading halo waves
            for (int wave = 0; wave < 8; wave++)
            {
                float waveScale = (0.3f + wave * 0.15f) * intensity;
                Color waveColor = Color.Lerp(primaryColor, secondaryColor, wave / 8f);
                CustomParticles.HaloRing(center, waveColor, waveScale, 18 + wave * 3);
            }
            
            // Radial music note storm
            for (int n = 0; n < 24; n++)
            {
                float angle = MathHelper.TwoPi * n / 24f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f) * intensity;
                Color noteColor = Color.Lerp(primaryColor, secondaryColor, n / 24f);
                ThemedParticles.MusicNote(center, noteVel, noteColor, 0.8f + Main.rand.NextFloat(0.2f), 35);
            }
            
            // Radial sparks
            for (int s = 0; s < 32; s++)
            {
                float angle = MathHelper.TwoPi * s / 32f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f) * intensity;
                Color sparkColor = Color.Lerp(primaryColor, Color.White, Main.rand.NextFloat(0.3f));
                
                var spark = new SparkleParticle(center, sparkVel, sparkColor, 0.4f, 30);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Staff line burst
            for (int line = -2; line <= 2; line++)
            {
                for (int i = 0; i < 12; i++)
                {
                    float t = i / 11f;
                    Vector2 linePos = center + new Vector2(-150f + t * 300f, line * 15f) * intensity;
                    
                    Dust staffDust = Dust.NewDustPerfect(linePos, DustID.GoldCoin, 
                        new Vector2(t < 0.5f ? -3f : 3f, 0) * intensity, 0, primaryColor, 0.7f);
                    staffDust.noGravity = true;
                }
            }
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.3f * intensity }, center);
        }
        
        #endregion
        
        #region Additional Effects (Missing Methods)
        
        /// <summary>
        /// Diminuendo Fade: Visual effect showing attack power decreasing
        /// Fading particles moving outward with decreasing intensity
        /// </summary>
        public static void DiminuendoFade(Vector2 center, Color color, float fadeProgress)
        {
            // fadeProgress: 0 = full power, 1 = fully faded
            float intensity = 1f - fadeProgress;
            
            // Fading rings moving outward
            float ringRadius = 30f + fadeProgress * 80f;
            float ringAlpha = intensity * 0.6f;
            
            CustomParticles.HaloRing(center, color * ringAlpha, ringRadius / 100f, 12);
            
            // Diminishing particles
            int particleCount = (int)(8 * intensity) + 2;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 particlePos = center + angle.ToRotationVector2() * ringRadius;
                
                // Particles fade and shrink
                float particleScale = 0.3f * intensity;
                CustomParticles.GenericFlare(particlePos, color * ringAlpha, particleScale, 10);
            }
            
            // Fading central glow
            if (intensity > 0.3f)
            {
                EnhancedParticles.BloomFlare(center, color * (intensity - 0.3f), 0.2f * intensity, 8, 2, 0.4f);
            }
        }
        
        /// <summary>
        /// Chord Buildup Convergence: Multiple colored streams converging during charge
        /// Used for signature attacks like Hero's Judgment
        /// </summary>
        public static void ChordBuildupConvergence(Vector2 center, Color[] chordColors, float progress)
        {
            if (chordColors == null || chordColors.Length == 0) return;
            
            float radius = 200f * (1f - progress * 0.7f);
            int voiceCount = chordColors.Length;
            
            // Each chord voice converges from a different angle
            for (int voice = 0; voice < voiceCount; voice++)
            {
                float baseAngle = MathHelper.TwoPi * voice / voiceCount;
                Color voiceColor = chordColors[voice];
                
                // Stream of particles for this voice
                int streamParticles = 5 + (int)(progress * 10);
                for (int p = 0; p < streamParticles; p++)
                {
                    float streamProgress = p / (float)(streamParticles - 1);
                    float particleRadius = radius * (1f - streamProgress * progress);
                    
                    // Slight spiral as they converge
                    float spiralOffset = streamProgress * progress * 0.5f;
                    Vector2 particlePos = center + (baseAngle + spiralOffset).ToRotationVector2() * particleRadius;
                    
                    float alpha = streamProgress * progress;
                    CustomParticles.GenericFlare(particlePos, voiceColor * alpha, 0.25f + progress * 0.2f, 8);
                }
                
                // Music note at stream head
                if (progress > 0.3f && Main.rand.NextBool(6))
                {
                    Vector2 notePos = center + baseAngle.ToRotationVector2() * radius;
                    Vector2 noteVel = (center - notePos).SafeNormalize(Vector2.Zero) * 2f;
                    ThemedParticles.MusicNote(notePos, noteVel, voiceColor, 0.6f + progress * 0.3f, 15);
                }
            }
            
            // Central chord glow grows with progress
            if (progress > 0.5f)
            {
                Color blendColor = chordColors[0];
                for (int i = 1; i < chordColors.Length; i++)
                    blendColor = Color.Lerp(blendColor, chordColors[i], 1f / (i + 1));
                
                float coreIntensity = (progress - 0.5f) * 2f;
                EnhancedParticles.BloomFlare(center, blendColor, 0.3f * coreIntensity, 15, 3, 0.6f);
            }
            
            // Harmonic overtone rings at high progress
            if (progress > 0.7f)
            {
                float overtoneIntensity = (progress - 0.7f) / 0.3f;
                CustomParticles.HaloRing(center, chordColors[0] * overtoneIntensity * 0.4f, 0.2f + overtoneIntensity * 0.2f, 10);
            }
        }
        
        #endregion
    }
}
