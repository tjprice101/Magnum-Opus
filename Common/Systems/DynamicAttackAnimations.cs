using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// DYNAMIC ATTACK ANIMATIONS
    /// 
    /// Provides uniquely special attack animations with phased timing,
    /// dynamic particle choreography, and spectacular visual sequences.
    /// 
    /// === PHILOSOPHY ===
    /// Each attack animation should tell a story:
    /// - WINDUP: Build anticipation with converging effects
    /// - STRIKE: Explosive release of visual energy
    /// - FOLLOW-THROUGH: Lingering trails and aftermath
    /// - RECOVERY: Graceful fade with sparkle residue
    /// 
    /// === USAGE ===
    /// Call AnimateAttack() each frame during weapon use with current frame/totalFrames.
    /// The system automatically handles all phases and spawns appropriate particles.
    /// </summary>
    public static class DynamicAttackAnimations
    {
        #region Animation Phase Helpers
        
        /// <summary>
        /// Determines the current phase of an attack animation.
        /// </summary>
        public enum AttackPhase
        {
            Windup,        // 0-20% of animation
            Strike,        // 20-50% of animation
            FollowThrough, // 50-80% of animation
            Recovery       // 80-100% of animation
        }
        
        /// <summary>
        /// Gets the current attack phase based on animation progress.
        /// </summary>
        public static AttackPhase GetPhase(float progress)
        {
            if (progress < 0.2f) return AttackPhase.Windup;
            if (progress < 0.5f) return AttackPhase.Strike;
            if (progress < 0.8f) return AttackPhase.FollowThrough;
            return AttackPhase.Recovery;
        }
        
        /// <summary>
        /// Gets the progress within the current phase (0-1).
        /// </summary>
        public static float GetPhaseProgress(float totalProgress)
        {
            if (totalProgress < 0.2f) return totalProgress / 0.2f;
            if (totalProgress < 0.5f) return (totalProgress - 0.2f) / 0.3f;
            if (totalProgress < 0.8f) return (totalProgress - 0.5f) / 0.3f;
            return (totalProgress - 0.8f) / 0.2f;
        }
        
        #endregion
        
        #region Universal Phased Attack Animation
        
        /// <summary>
        /// Universal phased attack animation. Call each frame during attack.
        /// </summary>
        /// <param name="center">Center of attack (hitbox center)</param>
        /// <param name="direction">Direction of swing</param>
        /// <param name="currentFrame">Current animation frame</param>
        /// <param name="totalFrames">Total frames in animation</param>
        /// <param name="primary">Primary theme color</param>
        /// <param name="secondary">Secondary theme color</param>
        /// <param name="style">Attack animation style</param>
        /// <param name="intensity">Effect intensity multiplier (0.5-2.0)</param>
        public static void AnimateAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames,
            Color primary, Color secondary, AttackStyle style = AttackStyle.Standard, float intensity = 1f)
        {
            float progress = (float)currentFrame / totalFrames;
            AttackPhase phase = GetPhase(progress);
            float phaseProgress = GetPhaseProgress(progress);
            
            switch (style)
            {
                case AttackStyle.Standard:
                    AnimateStandardAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
                case AttackStyle.Crescendo:
                    AnimateCrescendoAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
                case AttackStyle.Celestial:
                    AnimateCelestialAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
                case AttackStyle.Infernal:
                    AnimateInfernalAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
                case AttackStyle.Ethereal:
                    AnimateEtherealAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
                case AttackStyle.Prismatic:
                    AnimatePrismaticAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
                case AttackStyle.Vortex:
                    AnimateVortexAttack(center, direction, phase, phaseProgress, primary, secondary, intensity);
                    break;
            }
        }
        
        #endregion
        
        #region Attack Styles
        
        public enum AttackStyle
        {
            Standard,    // Balanced visual with clear phases
            Crescendo,   // Musical - builds to dramatic finale
            Celestial,   // Stars, comets, cosmic effects
            Infernal,    // Fire, smoke, hellish effects
            Ethereal,    // Ghostly, misty, spectral effects
            Prismatic,   // Rainbow, color cycling, prismatic
            Vortex       // Spiraling, orbiting, gravitational
        }
        
        #endregion
        
        #region Standard Animation
        
        private static void AnimateStandardAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Converging particles toward strike point
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 50f * (1f - phaseProgress) + 10f;
                        Vector2 startPos = center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (center - startPos).SafeNormalize(Vector2.Zero) * 3f * intensity;
                        
                        DynamicParticleEffects.PulsingGlow(startPos, vel, primary, secondary,
                            0.25f * intensity, 20, 0.2f, 0.3f);
                    }
                    
                    // Building glow at center
                    if (phaseProgress > 0.5f && Main.rand.NextBool(3))
                    {
                        float glowScale = 0.2f + phaseProgress * 0.3f;
                        CustomParticles.GenericFlare(center, primary * 0.5f, glowScale * intensity, 8);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // Explosive burst of arcs
                    if (phaseProgress < 0.3f && Main.rand.NextBool(2))
                    {
                        int arcCount = (int)(3 * intensity);
                        for (int i = 0; i < arcCount; i++)
                        {
                            float angleOffset = (i - arcCount / 2f) * 0.2f;
                            Vector2 arcDir = direction.RotatedBy(angleOffset);
                            Color arcColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                            CustomParticles.SwordArcSlash(center, arcDir * 4f, arcColor * 0.9f, 
                                0.45f * intensity, arcDir.ToRotation());
                        }
                        
                        // Central flash
                        DynamicParticleEffects.DramaticImpact(center, primary, secondary, 0.5f * intensity, 25);
                    }
                    
                    // Trailing particles during strike
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 trailPos = center + Main.rand.NextVector2Circular(15f, 15f);
                        Vector2 trailVel = direction * Main.rand.NextFloat(2f, 5f) * intensity;
                        var trail = new GenericGlowParticle(trailPos, trailVel, 
                            Color.Lerp(primary, secondary, Main.rand.NextFloat()) * 0.8f, 
                            0.3f * intensity, 20, true);
                        MagnumParticleHandler.SpawnParticle(trail);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Lingering arcs with diminishing opacity
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        float opacity = 0.8f - phaseProgress * 0.5f;
                        float angleOffset = Main.rand.NextFloat(-0.4f, 0.4f);
                        Vector2 arcDir = direction.RotatedBy(angleOffset);
                        CustomParticles.SwordArcSlash(center + direction * phaseProgress * 20f, 
                            arcDir * 2f, secondary * opacity, 0.35f * intensity, arcDir.ToRotation());
                    }
                    
                    // Music note accents
                    if (Main.rand.NextBool(5))
                    {
                        Vector2 notePos = center + Main.rand.NextVector2Circular(25f, 25f);
                        ThemedParticles.MusicNote(notePos, direction * 0.5f, primary * 0.7f, 0.7f * intensity, 30);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Sparkle residue
                    if (Main.rand.NextBool((int)(5 / intensity)))
                    {
                        float fadeAlpha = 1f - phaseProgress;
                        Vector2 sparklePos = center + Main.rand.NextVector2Circular(30f, 30f);
                        DynamicParticleEffects.TwinklingSparks(sparklePos, secondary * fadeAlpha, 
                            2, 10f, 0.2f * intensity, 25);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Crescendo Animation (Musical)
        
        private static void AnimateCrescendoAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            // Musical crescendo builds from quiet to thunderous finale
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Gentle music notes gathering
                    if (Main.rand.NextBool((int)(6 / intensity)))
                    {
                        float hueBase = Main.GameUpdateCount * 0.01f % 1f;
                        DynamicParticleEffects.HueShiftingMusicNotes(
                            center + Main.rand.NextVector2Circular(40f * (1f - phaseProgress), 40f * (1f - phaseProgress)),
                            (center - center - Main.rand.NextVector2Circular(30f, 30f)).SafeNormalize(Vector2.Zero) * 2f,
                            hueBase, hueBase + 0.2f, (int)(2 * intensity), 0.5f * intensity, 25);
                    }
                    
                    // Pulsing glow builds
                    if (phaseProgress > 0.6f)
                    {
                        float buildIntensity = (phaseProgress - 0.6f) / 0.4f;
                        DynamicParticleEffects.PulsingGlow(center, Vector2.Zero, primary, secondary,
                            0.3f * buildIntensity * intensity, 15, 0.25f, 0.4f);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // FORTE! Explosive musical burst
                    if (phaseProgress < 0.4f)
                    {
                        // Dramatic flare
                        DynamicParticleEffects.DramaticBurst(center, primary, secondary, 
                            (int)(6 * intensity), 0.7f * intensity, 35);
                        
                        // Music notes exploding outward
                        DynamicParticleEffects.HueShiftingMusicNotes(center, direction * 3f,
                            0f, 1f, (int)(8 * intensity), 0.9f * intensity, 40);
                        
                        // Orbiting note ring expands
                        DynamicParticleEffects.SpiralBurst(center, primary, secondary,
                            (int)(8 * intensity), 0.1f, 3f * intensity, 0.3f * intensity, 30);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Reverberating echoes
                    if (Main.rand.NextBool(2))
                    {
                        float echoScale = 0.6f - phaseProgress * 0.3f;
                        DynamicParticleEffects.PulsingBurst(center + direction * phaseProgress * 30f,
                            primary * (1f - phaseProgress * 0.5f), secondary * (1f - phaseProgress * 0.5f),
                            (int)(4 * intensity), 2f * intensity, echoScale * intensity, 25);
                    }
                    
                    // Trailing notes
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 notePos = center + direction * phaseProgress * 40f + Main.rand.NextVector2Circular(15f, 15f);
                        ThemedParticles.MusicNote(notePos, direction * 0.3f, secondary * (1f - phaseProgress * 0.6f), 
                            0.7f * intensity, 35);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Gentle diminuendo
                    if (Main.rand.NextBool((int)(8 / intensity)))
                    {
                        float fade = 1f - phaseProgress;
                        Vector2 fadePos = center + Main.rand.NextVector2Circular(35f, 35f);
                        ThemedParticles.MusicNote(fadePos, Vector2.UnitY * -0.5f, primary * fade * 0.4f, 
                            0.5f * intensity, 30);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Celestial Animation (Cosmic)
        
        private static void AnimateCelestialAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Stars converging from constellation
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 60f * (1f - phaseProgress * 0.7f);
                        Vector2 starPos = center + angle.ToRotationVector2() * radius;
                        Vector2 starVel = (center - starPos).SafeNormalize(Vector2.Zero) * 2.5f * intensity;
                        
                        // Twinkling star converging
                        DynamicParticleEffects.Comet(starPos, starVel, Color.White, primary * 0.4f,
                            0.2f * intensity, 4, 25);
                    }
                    
                    // Growing celestial glow
                    if (phaseProgress > 0.5f)
                    {
                        float glowIntensity = (phaseProgress - 0.5f) * 2f;
                        DynamicParticleEffects.ConcentricOrbits(center, primary, secondary,
                            2, 3, 15f * glowIntensity, 8f, 0.1f, 0.15f * intensity, 15);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // Supernova burst!
                    if (phaseProgress < 0.3f)
                    {
                        DynamicParticleEffects.CelestialBurst(center, intensity);
                        
                        // Sword arcs as light rays
                        for (int i = 0; i < (int)(8 * intensity); i++)
                        {
                            float rayAngle = MathHelper.TwoPi * i / (8 * intensity);
                            Vector2 rayDir = rayAngle.ToRotationVector2();
                            Color rayColor = Color.Lerp(Color.White, primary, Main.rand.NextFloat(0.3f, 0.7f));
                            CustomParticles.SwordArcSlash(center, rayDir * 6f, rayColor * 0.8f, 
                                0.5f * intensity, rayAngle);
                        }
                    }
                    
                    // Comet shower
                    if (Main.rand.NextBool(2))
                    {
                        DynamicParticleEffects.CometShower(center, direction, Color.White, primary * 0.5f,
                            (int)(4 * intensity), 5f * intensity, 0.6f, 0.25f * intensity, 30);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Nebula trail
                    if (Main.rand.NextBool(2))
                    {
                        float nebulaAlpha = 1f - phaseProgress * 0.6f;
                        Vector2 nebulaPos = center + direction * phaseProgress * 35f;
                        
                        for (int i = 0; i < (int)(3 * intensity); i++)
                        {
                            Vector2 cloudOffset = Main.rand.NextVector2Circular(15f, 15f);
                            Color cloudColor = Color.Lerp(primary, secondary, Main.rand.NextFloat()) * nebulaAlpha * 0.5f;
                            var cloud = new GenericGlowParticle(nebulaPos + cloudOffset, 
                                direction * 1f + Main.rand.NextVector2Circular(1f, 1f),
                                cloudColor, 0.3f * intensity, 25, true);
                            MagnumParticleHandler.SpawnParticle(cloud);
                        }
                    }
                    
                    // Trailing stars
                    if (Main.rand.NextBool(4))
                    {
                        DynamicParticleEffects.TwinklingSparks(center + direction * phaseProgress * 30f,
                            Color.White * (1f - phaseProgress * 0.5f), 3, 15f, 0.25f * intensity, 30);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Fading starlight
                    if (Main.rand.NextBool((int)(6 / intensity)))
                    {
                        Vector2 starPos = center + Main.rand.NextVector2Circular(40f, 40f);
                        float fade = 1f - phaseProgress;
                        var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                            Color.White * fade * 0.6f, 0.15f * intensity, 20, true);
                        MagnumParticleHandler.SpawnParticle(star);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Infernal Animation (Fire/Hell)
        
        private static void AnimateInfernalAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            // Fire colors
            Color fireOrange = new Color(255, 140, 40);
            Color fireRed = new Color(200, 50, 30);
            Color ember = new Color(255, 200, 80);
            
            // Blend with provided colors
            Color flame1 = Color.Lerp(primary, fireOrange, 0.5f);
            Color flame2 = Color.Lerp(secondary, fireRed, 0.5f);
            
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Gathering flames
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 45f * (1f - phaseProgress * 0.6f);
                        Vector2 flamePos = center + angle.ToRotationVector2() * radius;
                        
                        // Streaking flame toward center
                        Vector2 flameVel = (center - flamePos).SafeNormalize(Vector2.Zero) * 4f * intensity;
                        DynamicParticleEffects.SpeedStreaks(flamePos, flameVel, flame1, 2, 0.12f * intensity, 18);
                    }
                    
                    // Building ember glow
                    if (phaseProgress > 0.4f && Main.rand.NextBool(3))
                    {
                        DynamicParticleEffects.PulsingGlow(center, Vector2.Zero, flame1, ember,
                            0.35f * phaseProgress * intensity, 12, 0.3f, 0.5f);
                    }
                    
                    // Smoke wisps
                    if (Main.rand.NextBool(4))
                    {
                        var smoke = new HeavySmokeParticle(center + Main.rand.NextVector2Circular(15f, 15f),
                            Vector2.UnitY * -1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                            Color.Black * 0.4f, Main.rand.Next(25, 35), 0.2f * intensity, 0.4f, 0.02f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // INFERNO ERUPTION!
                    if (phaseProgress < 0.35f)
                    {
                        // Dramatic explosion
                        DynamicParticleEffects.DramaticBurst(center, flame1, flame2, (int)(8 * intensity), 
                            0.8f * intensity, 35);
                        
                        // Fire arcs
                        for (int i = 0; i < (int)(6 * intensity); i++)
                        {
                            float arcAngle = direction.ToRotation() + (i - 3) * 0.25f;
                            Vector2 arcDir = arcAngle.ToRotationVector2();
                            CustomParticles.SwordArcSlash(center, arcDir * 5f, 
                                Color.Lerp(flame1, flame2, Main.rand.NextFloat()) * 0.9f,
                                0.55f * intensity, arcAngle);
                        }
                        
                        // Ember shower
                        DynamicParticleEffects.SpeedStreaks(center, direction * 6f, ember, 
                            (int)(12 * intensity), 0.1f * intensity, 25, true);
                    }
                    
                    // Continuous flame trail
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 trailPos = center + Main.rand.NextVector2Circular(20f, 20f);
                        var flame = new GenericGlowParticle(trailPos, direction * Main.rand.NextFloat(2f, 5f),
                            Color.Lerp(flame1, ember, Main.rand.NextFloat()) * 0.8f, 0.35f * intensity, 22, true);
                        MagnumParticleHandler.SpawnParticle(flame);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Trailing flames
                    if (Main.rand.NextBool(2))
                    {
                        float trailAlpha = 1f - phaseProgress * 0.5f;
                        Vector2 trailPos = center + direction * phaseProgress * 40f + Main.rand.NextVector2Circular(15f, 15f);
                        var flame = new GenericGlowParticle(trailPos, direction * 2f + Vector2.UnitY * -1f,
                            flame2 * trailAlpha, 0.3f * intensity, 20, true);
                        MagnumParticleHandler.SpawnParticle(flame);
                    }
                    
                    // Smoke billow
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 smokePos = center + direction * phaseProgress * 30f;
                        var smoke = new HeavySmokeParticle(smokePos + Main.rand.NextVector2Circular(10f, 10f),
                            Vector2.UnitY * -2f + Main.rand.NextVector2Circular(1f, 1f),
                            Color.Black * (0.5f - phaseProgress * 0.3f), Main.rand.Next(30, 45), 
                            0.3f * intensity, 0.6f, 0.015f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Dying embers
                    if (Main.rand.NextBool((int)(5 / intensity)))
                    {
                        float fade = 1f - phaseProgress;
                        Vector2 emberPos = center + Main.rand.NextVector2Circular(35f, 35f);
                        Vector2 emberVel = Main.rand.NextVector2Circular(1f, 1f) + Vector2.UnitY * 0.5f;
                        var emberP = new GenericGlowParticle(emberPos, emberVel, ember * fade * 0.6f,
                            0.15f * intensity, 30, true);
                        MagnumParticleHandler.SpawnParticle(emberP);
                    }
                    
                    // Residual smoke
                    if (Main.rand.NextBool(6))
                    {
                        var smoke = new HeavySmokeParticle(center + Main.rand.NextVector2Circular(25f, 25f),
                            Vector2.UnitY * -1.5f, Color.Black * 0.25f * (1f - phaseProgress),
                            Main.rand.Next(20, 35), 0.15f * intensity, 0.3f, 0.02f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Ethereal Animation (Ghostly)
        
        private static void AnimateEtherealAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            // Ethereal colors - blend toward ghostly tones
            Color ghost1 = Color.Lerp(primary, new Color(200, 220, 255), 0.3f) * 0.8f;
            Color ghost2 = Color.Lerp(secondary, new Color(180, 200, 240), 0.3f) * 0.8f;
            
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Ghostly wisps gathering
                    if (Main.rand.NextBool((int)(4 / intensity)))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 55f * (1f - phaseProgress * 0.5f);
                        Vector2 wispPos = center + angle.ToRotationVector2() * radius;
                        Vector2 wispVel = (center - wispPos).SafeNormalize(Vector2.Zero) * 2f * intensity;
                        
                        // Ethereal wisp with pulsing
                        DynamicParticleEffects.PulsingGlow(wispPos, wispVel, ghost1 * 0.6f, ghost2 * 0.4f,
                            0.2f * intensity, 25, 0.15f, 0.2f);
                    }
                    
                    // Phased appearance bloom
                    if (phaseProgress > 0.5f && Main.rand.NextBool(3))
                    {
                        var phased = new PhasedBloomParticle(center, Vector2.Zero, ghost1 * 0.5f, ghost2,
                            0.2f * intensity, 0.4f * intensity, 20, 0.3f, 0.5f);
                        MagnumParticleHandler.SpawnParticle(phased);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // Spectral release
                    if (phaseProgress < 0.35f)
                    {
                        // Phased dramatic appearance
                        DynamicParticleEffects.PhasedAppearance(center, Vector2.Zero, ghost1, Color.White,
                            0.7f * intensity, 30, 0.15f, 0.6f);
                        
                        // Ghost arcs (semi-transparent)
                        for (int i = 0; i < (int)(5 * intensity); i++)
                        {
                            float arcAngle = direction.ToRotation() + (i - 2.5f) * 0.3f;
                            Vector2 arcDir = arcAngle.ToRotationVector2();
                            CustomParticles.SwordArcSlash(center, arcDir * 4f, ghost1 * 0.7f,
                                0.5f * intensity, arcAngle);
                        }
                        
                        // Spectral burst
                        DynamicParticleEffects.SpiralBurst(center, ghost1 * 0.6f, ghost2 * 0.4f,
                            (int)(8 * intensity), 0.08f, 2f * intensity, 0.25f * intensity, 35);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Lingering specters
                    if (Main.rand.NextBool(2))
                    {
                        float fadeAlpha = 0.7f - phaseProgress * 0.4f;
                        Vector2 specterPos = center + direction * phaseProgress * 35f + Main.rand.NextVector2Circular(12f, 12f);
                        
                        DynamicParticleEffects.PulsingGlow(specterPos, direction * 1f, ghost1 * fadeAlpha, ghost2 * fadeAlpha,
                            0.25f * intensity, 28, 0.12f, 0.25f);
                    }
                    
                    // Ethereal mist
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 mistPos = center + direction * phaseProgress * 25f;
                        var mist = new HeavySmokeParticle(mistPos + Main.rand.NextVector2Circular(15f, 15f),
                            direction * 0.5f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                            ghost2 * 0.3f, Main.rand.Next(35, 50), 0.25f * intensity, 0.5f, 0.01f, false);
                        MagnumParticleHandler.SpawnParticle(mist);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Fading spirits
                    if (Main.rand.NextBool((int)(6 / intensity)))
                    {
                        float fade = 1f - phaseProgress;
                        Vector2 fadePos = center + Main.rand.NextVector2Circular(40f, 40f);
                        
                        var spirit = new PhasedBloomParticle(fadePos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                            ghost1 * fade * 0.4f, ghost2 * fade * 0.3f, 0.1f * intensity, 0.2f * intensity,
                            30, 0.1f, 0.3f);
                        MagnumParticleHandler.SpawnParticle(spirit);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Prismatic Animation (Rainbow)
        
        private static void AnimatePrismaticAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            float baseHue = Main.GameUpdateCount * 0.02f % 1f;
            
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Rainbow particles converging
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float radius = 50f * (1f - phaseProgress * 0.6f);
                        Vector2 prismPos = center + angle.ToRotationVector2() * radius;
                        Vector2 prismVel = (center - prismPos).SafeNormalize(Vector2.Zero) * 3f * intensity;
                        
                        float hue = (baseHue + angle / MathHelper.TwoPi) % 1f;
                        Color prismColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                        
                        var prism = new RainbowCyclingParticle(prismPos, prismVel, hue, hue + 0.3f,
                            0.9f, 0.75f, 0.25f * intensity, 25, 0.02f);
                        MagnumParticleHandler.SpawnParticle(prism);
                    }
                    
                    // Building prismatic glow
                    if (phaseProgress > 0.4f)
                    {
                        Color buildColor = Main.hslToRgb(baseHue, 0.95f, 0.8f);
                        DynamicParticleEffects.PulsingGlow(center, Vector2.Zero, buildColor, 
                            Main.hslToRgb((baseHue + 0.5f) % 1f, 0.9f, 0.7f),
                            0.3f * phaseProgress * intensity, 12, 0.2f, 0.35f);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // RAINBOW EXPLOSION!
                    if (phaseProgress < 0.3f)
                    {
                        DynamicParticleEffects.RainbowBurst(center, (int)(12 * intensity), 4f * intensity, 
                            0.35f * intensity, 35);
                        
                        // Prismatic arcs
                        for (int i = 0; i < (int)(8 * intensity); i++)
                        {
                            float hue = (baseHue + i / (8f * intensity)) % 1f;
                            Color arcColor = Main.hslToRgb(hue, 0.95f, 0.75f);
                            float arcAngle = direction.ToRotation() + (i - 4) * 0.2f;
                            Vector2 arcDir = arcAngle.ToRotationVector2();
                            CustomParticles.SwordArcSlash(center, arcDir * 5f, arcColor * 0.9f,
                                0.5f * intensity, arcAngle);
                        }
                    }
                    
                    // Continuous rainbow trail
                    if (Main.rand.NextBool(2))
                    {
                        float trailHue = Main.rand.NextFloat();
                        Color trailColor = Main.hslToRgb(trailHue, 0.9f, 0.75f);
                        Vector2 trailPos = center + Main.rand.NextVector2Circular(15f, 15f);
                        var trail = new RainbowCyclingParticle(trailPos, direction * Main.rand.NextFloat(2f, 5f),
                            trailHue, trailHue + 0.5f, 0.9f, 0.7f, 0.3f * intensity, 22, 0.03f);
                        MagnumParticleHandler.SpawnParticle(trail);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Prismatic trail
                    if (Main.rand.NextBool(2))
                    {
                        float hue = Main.rand.NextFloat();
                        DynamicParticleEffects.HueRangeBurst(center + direction * phaseProgress * 30f,
                            hue, hue + 0.3f, 0.85f, 0.7f, 3, 2f * intensity, 0.2f * intensity, 25);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Fading rainbow sparkles
                    if (Main.rand.NextBool((int)(5 / intensity)))
                    {
                        float fade = 1f - phaseProgress;
                        float hue = Main.rand.NextFloat();
                        Color fadeColor = Main.hslToRgb(hue, 0.8f, 0.75f) * fade;
                        DynamicParticleEffects.TwinklingSparks(center + Main.rand.NextVector2Circular(35f, 35f),
                            fadeColor, 2, 8f, 0.2f * intensity, 25);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Vortex Animation (Spiral/Gravitational)
        
        private static void AnimateVortexAttack(Vector2 center, Vector2 direction, AttackPhase phase, 
            float phaseProgress, Color primary, Color secondary, float intensity)
        {
            switch (phase)
            {
                case AttackPhase.Windup:
                    // Spiraling inward vortex
                    if (Main.rand.NextBool((int)(3 / intensity)))
                    {
                        DynamicParticleEffects.SpiralVortex(center, primary, secondary * 0.6f,
                            (int)(4 * intensity), 50f * (1f - phaseProgress * 0.4f), 0.12f, 1.5f * intensity,
                            0.2f * intensity, 30);
                    }
                    
                    // Building orbital energy
                    if (phaseProgress > 0.3f)
                    {
                        float orbitalIntensity = (phaseProgress - 0.3f) / 0.7f;
                        DynamicParticleEffects.OrbitingRing(center, primary * orbitalIntensity,
                            (int)(4 * intensity), 20f * (1f - orbitalIntensity * 0.3f), 0.15f,
                            0.2f * intensity, 15);
                    }
                    break;
                    
                case AttackPhase.Strike:
                    // VORTEX RELEASE!
                    if (phaseProgress < 0.35f)
                    {
                        // Expanding spiral explosion
                        DynamicParticleEffects.SpiralBurst(center, primary, secondary,
                            (int)(12 * intensity), 0.15f, 4f * intensity, 0.35f * intensity, 35);
                        
                        // Vortex arcs spiraling outward
                        for (int i = 0; i < (int)(6 * intensity); i++)
                        {
                            float spiralAngle = direction.ToRotation() + i * 0.5f + Main.GameUpdateCount * 0.1f;
                            Vector2 spiralDir = spiralAngle.ToRotationVector2();
                            Color spiralColor = Color.Lerp(primary, secondary, (float)i / (6 * intensity));
                            CustomParticles.SwordArcSlash(center, spiralDir * 5f, spiralColor * 0.85f,
                                0.5f * intensity, spiralAngle);
                        }
                        
                        // Central dramatic impact
                        DynamicParticleEffects.DramaticImpact(center, primary, secondary, 0.6f * intensity, 30);
                    }
                    
                    // Continuing spiral particles
                    if (Main.rand.NextBool(2))
                    {
                        float spawnAngle = Main.GameUpdateCount * 0.2f + Main.rand.NextFloat(MathHelper.TwoPi);
                        var spiral = new SpiralParticle(center, 10f, spawnAngle, 0.1f, 3f * intensity,
                            primary * 0.8f, secondary * 0.5f, 0.25f * intensity, 25, true);
                        MagnumParticleHandler.SpawnParticle(spiral);
                    }
                    break;
                    
                case AttackPhase.FollowThrough:
                    // Dissipating vortex
                    if (Main.rand.NextBool(2))
                    {
                        float vortexFade = 1f - phaseProgress * 0.6f;
                        Vector2 vortexCenter = center + direction * phaseProgress * 25f;
                        
                        var dissipate = new SpiralParticle(vortexCenter, 15f * vortexFade, 
                            Main.rand.NextFloat(MathHelper.TwoPi), 0.08f, 1.5f * intensity,
                            primary * vortexFade, secondary * vortexFade * 0.6f, 0.2f * intensity, 25, true);
                        MagnumParticleHandler.SpawnParticle(dissipate);
                    }
                    break;
                    
                case AttackPhase.Recovery:
                    // Residual swirls
                    if (Main.rand.NextBool((int)(6 / intensity)))
                    {
                        float fade = 1f - phaseProgress;
                        Vector2 swirlPos = center + Main.rand.NextVector2Circular(30f, 30f);
                        
                        var swirl = new SpiralParticle(swirlPos, 8f * fade, Main.rand.NextFloat(MathHelper.TwoPi),
                            0.05f, 0.5f * intensity, primary * fade * 0.4f, secondary * fade * 0.3f,
                            0.12f * intensity, 30, true);
                        MagnumParticleHandler.SpawnParticle(swirl);
                    }
                    break;
            }
        }
        
        #endregion
        
        #region Theme-Specific Preset Animations
        
        /// <summary>
        /// Spring themed attack animation.
        /// </summary>
        public static void AnimateSpringAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color blossom = new Color(255, 180, 200);
            Color leaf = new Color(120, 200, 120);
            AnimateAttack(center, direction, currentFrame, totalFrames, blossom, leaf, AttackStyle.Ethereal, intensity);
        }
        
        /// <summary>
        /// Summer themed attack animation.
        /// </summary>
        public static void AnimateSummerAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color sunGold = new Color(255, 200, 50);
            Color sunsetOrange = new Color(255, 140, 50);
            AnimateAttack(center, direction, currentFrame, totalFrames, sunGold, sunsetOrange, AttackStyle.Infernal, intensity);
        }
        
        /// <summary>
        /// Autumn themed attack animation.
        /// </summary>
        public static void AnimateAutumnAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color maple = new Color(200, 80, 40);
            Color amber = new Color(255, 180, 80);
            AnimateAttack(center, direction, currentFrame, totalFrames, maple, amber, AttackStyle.Vortex, intensity);
        }
        
        /// <summary>
        /// Winter themed attack animation.
        /// </summary>
        public static void AnimateWinterAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color ice = new Color(180, 220, 255);
            Color frost = new Color(220, 240, 255);
            AnimateAttack(center, direction, currentFrame, totalFrames, ice, frost, AttackStyle.Ethereal, intensity);
        }
        
        /// <summary>
        /// Nachtmusik themed attack animation.
        /// </summary>
        public static void AnimateNachtmusikAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color nightBlue = new Color(40, 60, 140);
            Color starGold = new Color(255, 220, 100);
            AnimateAttack(center, direction, currentFrame, totalFrames, nightBlue, starGold, AttackStyle.Celestial, intensity);
        }
        
        /// <summary>
        /// Dies Irae themed attack animation.
        /// </summary>
        public static void AnimateDiesIraeAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color blood = new Color(139, 0, 0);
            Color fire = new Color(255, 80, 0);
            AnimateAttack(center, direction, currentFrame, totalFrames, blood, fire, AttackStyle.Infernal, intensity);
        }
        
        /// <summary>
        /// Four Seasons themed attack animation (prismatic rainbow).
        /// </summary>
        public static void AnimateFourSeasonsAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color spring = new Color(255, 180, 200);
            Color winter = new Color(180, 220, 255);
            AnimateAttack(center, direction, currentFrame, totalFrames, spring, winter, AttackStyle.Prismatic, intensity);
        }
        
        /// <summary>
        /// Fate themed attack animation (cosmic).
        /// </summary>
        public static void AnimateFateAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color cosmic = new Color(180, 50, 100);
            Color void_ = new Color(15, 5, 20);
            AnimateAttack(center, direction, currentFrame, totalFrames, cosmic, void_, AttackStyle.Celestial, intensity);
        }
        
        /// <summary>
        /// Moonlight Sonata themed attack animation.
        /// </summary>
        public static void AnimateMoonlightAttack(Vector2 center, Vector2 direction, int currentFrame, int totalFrames, float intensity = 1f)
        {
            Color deepPurple = new Color(75, 0, 130);
            Color lightBlue = new Color(135, 206, 250);
            AnimateAttack(center, direction, currentFrame, totalFrames, deepPurple, lightBlue, AttackStyle.Crescendo, intensity);
        }
        
        #endregion
    }
}
