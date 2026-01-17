using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Fate Lens Flare System - Creates cinematic lens flares and cosmic distortions
    /// for the endgame Fate weapons. These effects create that epic "reality bending" feel.
    /// </summary>
    public static class FateLensFlare
    {
        // Dark Prismatic color palette
        public static readonly Color FateBlack = new Color(15, 5, 20);
        public static readonly Color FateDarkPink = new Color(180, 50, 100);
        public static readonly Color FateBrightRed = new Color(255, 60, 80);
        public static readonly Color FatePurple = new Color(120, 30, 140);
        public static readonly Color FateWhite = new Color(255, 255, 255);
        public static readonly Color FateCyan = new Color(80, 200, 255);
        
        /// <summary>
        /// Gets a color along the Fate gradient (Black → Dark Pink → Bright Red → White)
        /// </summary>
        public static Color GetFateGradient(float progress)
        {
            if (progress < 0.4f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
        }
        
        /// <summary>
        /// Draws a cinematic lens flare at the specified position.
        /// Creates multiple layered elements: core, rays, hexagonal ghosts, and chromatic aberration.
        /// </summary>
        public static void DrawLensFlare(SpriteBatch spriteBatch, Vector2 worldPosition, float intensity = 1f, float size = 1f)
        {
            if (!Main.gameMenu && Main.LocalPlayer != null)
            {
                Vector2 screenPos = worldPosition - Main.screenPosition;
                Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
                Vector2 flareDirection = (screenPos - screenCenter).SafeNormalize(Vector2.UnitY);
                float distanceFromCenter = Vector2.Distance(screenPos, screenCenter);
                
                Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
                Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
                
                // Store original blend state
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                float time = Main.GameUpdateCount * 0.02f;
                float pulse = (float)Math.Sin(time * 2f) * 0.15f + 0.85f;
                
                // === CORE FLARE - The brightest center point ===
                Color coreColor = FateWhite * intensity * pulse;
                spriteBatch.Draw(flareTex, screenPos, null, coreColor, 0f, 
                    flareTex.Size() / 2f, 0.8f * size * pulse, SpriteEffects.None, 0f);
                
                // === INNER GLOW - Dark pink bloom ===
                Color innerColor = FateDarkPink * intensity * 0.7f;
                spriteBatch.Draw(glowTex, screenPos, null, innerColor, 0f,
                    glowTex.Size() / 2f, 1.5f * size, SpriteEffects.None, 0f);
                
                // === CHROMATIC ABERRATION RAYS - RGB separated streaks ===
                float rayLength = 80f * size * intensity;
                for (int i = 0; i < 6; i++)
                {
                    float rayAngle = MathHelper.TwoPi * i / 6f + time * 0.3f;
                    Vector2 rayDir = rayAngle.ToRotationVector2();
                    
                    // Red ray (offset left)
                    Vector2 redPos = screenPos + rayDir * rayLength * 0.9f - new Vector2(3, 0);
                    spriteBatch.Draw(glowTex, redPos, null, Color.Red * intensity * 0.4f, rayAngle,
                        glowTex.Size() / 2f, new Vector2(0.15f, 0.6f) * size, SpriteEffects.None, 0f);
                    
                    // Core ray
                    Vector2 coreRayPos = screenPos + rayDir * rayLength;
                    spriteBatch.Draw(glowTex, coreRayPos, null, FateBrightRed * intensity * 0.5f, rayAngle,
                        glowTex.Size() / 2f, new Vector2(0.2f, 0.8f) * size, SpriteEffects.None, 0f);
                    
                    // Cyan ray (offset right)
                    Vector2 cyanPos = screenPos + rayDir * rayLength * 1.1f + new Vector2(3, 0);
                    spriteBatch.Draw(glowTex, cyanPos, null, FateCyan * intensity * 0.4f, rayAngle,
                        glowTex.Size() / 2f, new Vector2(0.15f, 0.6f) * size, SpriteEffects.None, 0f);
                }
                
                // === ANAMORPHIC STREAK - Horizontal lens streak ===
                float streakWidth = 200f * size * intensity;
                spriteBatch.Draw(glowTex, screenPos, null, FateDarkPink * intensity * 0.35f, 0f,
                    glowTex.Size() / 2f, new Vector2(streakWidth / glowTex.Width, 0.15f), SpriteEffects.None, 0f);
                
                // Chromatic aberration on streak
                spriteBatch.Draw(glowTex, screenPos + new Vector2(0, -4), null, Color.Red * intensity * 0.2f, 0f,
                    glowTex.Size() / 2f, new Vector2(streakWidth / glowTex.Width * 1.1f, 0.08f), SpriteEffects.None, 0f);
                spriteBatch.Draw(glowTex, screenPos + new Vector2(0, 4), null, FateCyan * intensity * 0.2f, 0f,
                    glowTex.Size() / 2f, new Vector2(streakWidth / glowTex.Width * 1.1f, 0.08f), SpriteEffects.None, 0f);
                
                // === HEXAGONAL GHOSTS - Lens artifacts going toward screen center ===
                for (int ghost = 1; ghost <= 5; ghost++)
                {
                    float ghostProgress = ghost / 5f;
                    Vector2 ghostPos = Vector2.Lerp(screenPos, screenCenter, ghostProgress * 0.8f);
                    float ghostScale = (0.3f - ghostProgress * 0.15f) * size;
                    float ghostAlpha = (0.4f - ghostProgress * 0.25f) * intensity;
                    
                    Color ghostColor = GetFateGradient(ghostProgress);
                    
                    // Hexagonal shape approximated with 6-point star
                    for (int hex = 0; hex < 6; hex++)
                    {
                        float hexAngle = MathHelper.TwoPi * hex / 6f;
                        Vector2 hexOffset = hexAngle.ToRotationVector2() * 8f * ghostScale;
                        spriteBatch.Draw(glowTex, ghostPos + hexOffset, null, ghostColor * ghostAlpha, 0f,
                            glowTex.Size() / 2f, ghostScale * 0.5f, SpriteEffects.None, 0f);
                    }
                    spriteBatch.Draw(glowTex, ghostPos, null, ghostColor * ghostAlpha * 0.7f, 0f,
                        glowTex.Size() / 2f, ghostScale, SpriteEffects.None, 0f);
                }
                
                // === OUTER HALO - Subtle ring around the flare ===
                float haloScale = 2.5f * size * pulse;
                spriteBatch.Draw(glowTex, screenPos, null, FatePurple * intensity * 0.15f, 0f,
                    glowTex.Size() / 2f, haloScale, SpriteEffects.None, 0f);
                
                // Restore blend state
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        /// <summary>
        /// Draws a heat wave / cosmic distortion effect using particle-based simulation.
        /// Creates the appearance of light bending around the weapon.
        /// </summary>
        public static void DrawHeatWaveDistortion(Vector2 worldPosition, float radius, float intensity = 1f)
        {
            float time = Main.GameUpdateCount * 0.05f;
            int waveCount = (int)(8 * intensity);
            
            for (int i = 0; i < waveCount; i++)
            {
                float angle = MathHelper.TwoPi * i / waveCount + time;
                float waveRadius = radius * (0.6f + (float)Math.Sin(time * 2f + i) * 0.4f);
                Vector2 wavePos = worldPosition + angle.ToRotationVector2() * waveRadius;
                
                // Distortion shimmer particles
                float shimmer = (float)Math.Sin(time * 3f + i * 0.5f) * 0.3f + 0.7f;
                Color waveColor = GetFateGradient((float)i / waveCount) * shimmer * 0.4f;
                
                var shimmerParticle = new GenericGlowParticle(
                    wavePos + Main.rand.NextVector2Circular(3f, 3f),
                    angle.ToRotationVector2() * 0.5f,
                    waveColor,
                    0.1f + shimmer * 0.1f,
                    8,
                    true
                );
                MagnumParticleHandler.SpawnParticle(shimmerParticle);
            }
        }
        
        /// <summary>
        /// Creates a simple flare burst effect - NO CONCENTRIC RINGS.
        /// Just central flares with spark spray.
        /// </summary>
        public static void KaleidoscopeBurst(Vector2 position, float scale = 1f, int segments = 8)
        {
            // Central flares only - NO RINGS
            CustomParticles.GenericFlare(position, FateWhite, 0.7f * scale, 18);
            CustomParticles.GenericFlare(position, FateBrightRed, 0.5f * scale, 15);
            
            // Simple spark spray outward - NOT in ring patterns
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = GetFateGradient((float)i / segments);
                
                var spark = new GlowSparkParticle(position, vel, sparkColor, 0.25f * scale, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }
        
        /// <summary>
        /// Creates shifting chromatic visual effect around a position - the "reality bending" look.
        /// </summary>
        public static void ChromaticShift(Vector2 position, float radius, float intensity = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            
            // RGB channel separation effect
            Vector2 redOffset = new Vector2((float)Math.Cos(time) * 4f, (float)Math.Sin(time * 0.7f) * 3f);
            Vector2 greenOffset = Vector2.Zero;
            Vector2 blueOffset = new Vector2((float)Math.Cos(time + MathHelper.Pi) * 4f, (float)Math.Sin(time * 0.7f + MathHelper.Pi) * 3f);
            
            // Spawn chromatic particles at the edge of the radius
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = position + angle.ToRotationVector2() * radius;
                
                // Red channel
                var redParticle = new GenericGlowParticle(edgePos + redOffset, Vector2.Zero, Color.Red * 0.4f * intensity, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(redParticle);
                
                // Green/Pink channel
                var greenParticle = new GenericGlowParticle(edgePos + greenOffset, Vector2.Zero, FateDarkPink * 0.5f * intensity, 0.18f, 10, true);
                MagnumParticleHandler.SpawnParticle(greenParticle);
                
                // Blue/Cyan channel
                var blueParticle = new GenericGlowParticle(edgePos + blueOffset, Vector2.Zero, FateCyan * 0.4f * intensity, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(blueParticle);
            }
        }
        
        /// <summary>
        /// Creates a simple void effect - NO CONCENTRIC RINGS.
        /// Just a dark center with sparse particles.
        /// </summary>
        public static void EventHorizon(Vector2 position, float radius, float intensity = 1f)
        {
            // Simple dark void flare
            CustomParticles.GenericFlare(position, FateBlack, 0.4f * intensity, 10);
            CustomParticles.GenericFlare(position, FateDarkPink * 0.5f, 0.3f * intensity, 8);
            
            // Just a few random particles around - NOT in a ring pattern
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f);
                Color sparkColor = GetFateGradient(Main.rand.NextFloat());
                
                var particle = new GenericGlowParticle(position + offset, Main.rand.NextVector2Circular(1f, 1f), sparkColor * 0.5f, 0.1f, 12, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
        }
        
        /// <summary>
        /// Creates reality fracture lines - sharp visual cracks in space.
        /// </summary>
        public static void RealityFracture(Vector2 position, float length, int fractures = 5)
        {
            for (int i = 0; i < fractures; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float fractureLength = length * Main.rand.NextFloat(0.5f, 1f);
                
                int segments = (int)(fractureLength / 8f);
                Vector2 startPos = position;
                
                for (int seg = 0; seg < segments; seg++)
                {
                    float segProgress = (float)seg / segments;
                    Vector2 segOffset = angle.ToRotationVector2() * (seg * 8f);
                    // Add jagged variation
                    segOffset += (angle + MathHelper.PiOver2).ToRotationVector2() * (float)Math.Sin(seg * 2f) * 5f;
                    
                    Vector2 segPos = startPos + segOffset;
                    Color segColor = Color.Lerp(FateWhite, FateBrightRed, segProgress);
                    
                    CustomParticles.GenericFlare(segPos, segColor * (1f - segProgress * 0.5f), 0.15f, 8);
                    
                    // Chromatic edges
                    CustomParticles.GenericFlare(segPos + new Vector2(-2, 0), Color.Red * 0.3f, 0.1f, 6);
                    CustomParticles.GenericFlare(segPos + new Vector2(2, 0), FateCyan * 0.3f, 0.1f, 6);
                }
            }
        }
        
        #region Zodiac Constellation Auras
        
        /// <summary>
        /// ARIES - The Ram: Aggressive angular ram horns pattern with fiery charging particles.
        /// Used by Fate1 (Destiny Cleaver sword)
        /// </summary>
        public static void ZodiacAriesAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            float radius = 55f * scale;
            
            // Draw ram horn constellation - two curved horns
            for (int horn = 0; horn < 2; horn++)
            {
                float hornDir = horn == 0 ? -1f : 1f;
                int hornPoints = 6;
                
                for (int i = 0; i < hornPoints; i++)
                {
                    float progress = (float)i / hornPoints;
                    // Horn curves outward then up
                    float hornAngle = MathHelper.PiOver2 + hornDir * (0.3f + progress * 0.8f);
                    float hornRadius = radius * (0.3f + progress * 0.7f);
                    Vector2 starPos = center + hornAngle.ToRotationVector2() * hornRadius;
                    
                    if (Main.rand.NextBool(12))
                    {
                        Color starColor = GetFateGradient(progress) * 0.7f;
                        CustomParticles.GenericFlare(starPos, starColor, 0.2f + progress * 0.15f, 12);
                    }
                    
                    // Constellation lines
                    if (i > 0 && Main.rand.NextBool(20))
                    {
                        float prevProgress = (float)(i - 1) / hornPoints;
                        float prevAngle = MathHelper.PiOver2 + hornDir * (0.3f + prevProgress * 0.8f);
                        float prevRadius = radius * (0.3f + prevProgress * 0.7f);
                        Vector2 prevPos = center + prevAngle.ToRotationVector2() * prevRadius;
                        
                        Vector2 linePos = Vector2.Lerp(prevPos, starPos, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(linePos, FateDarkPink * 0.3f, 0.08f, 8);
                    }
                }
            }
            
            // Central aggressive charging particles
            if (Main.rand.NextBool(5))
            {
                float chargeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 chargeStart = center + chargeAngle.ToRotationVector2() * (radius + 20f);
                Vector2 chargeVel = (center - chargeStart).SafeNormalize(Vector2.Zero) * 3f;
                var charge = new GenericGlowParticle(chargeStart, chargeVel, FateBrightRed * 0.5f, 0.15f, 18, true);
                MagnumParticleHandler.SpawnParticle(charge);
            }
        }
        
        /// <summary>
        /// TAURUS - The Bull: Solid grounding V-shaped bull head with sturdy orbital ring.
        /// Used by Fate2 (Cosmic Executioner greatsword)
        /// </summary>
        public static void ZodiacTaurusAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.025f;
            float radius = 60f * scale;
            
            // Grounding orbital ring - slow, steady, powerful
            int ringStars = 8;
            for (int i = 0; i < ringStars; i++)
            {
                float angle = MathHelper.TwoPi * i / ringStars + time;
                Vector2 ringPos = center + angle.ToRotationVector2() * radius;
                
                if (Main.rand.NextBool(10))
                {
                    Color ringColor = GetFateGradient((float)i / ringStars) * 0.5f;
                    CustomParticles.GenericFlare(ringPos, ringColor, 0.22f, 14);
                }
            }
            
            // Bull head V-shape constellation
            Vector2[] bullHead = new Vector2[]
            {
                center + new Vector2(-30f, -25f) * scale, // Left horn tip
                center + new Vector2(-15f, 5f) * scale,   // Left inner
                center + new Vector2(0f, 15f) * scale,    // Snout
                center + new Vector2(15f, 5f) * scale,    // Right inner
                center + new Vector2(30f, -25f) * scale,  // Right horn tip
            };
            
            for (int i = 0; i < bullHead.Length; i++)
            {
                if (Main.rand.NextBool(15))
                {
                    CustomParticles.GenericFlare(bullHead[i], FateDarkPink * 0.6f, 0.18f, 12);
                }
                
                // Lines between stars
                if (i > 0 && Main.rand.NextBool(25))
                {
                    Vector2 linePos = Vector2.Lerp(bullHead[i - 1], bullHead[i], Main.rand.NextFloat());
                    CustomParticles.GenericFlare(linePos, FatePurple * 0.25f, 0.06f, 8);
                }
            }
        }
        
        /// <summary>
        /// GEMINI - The Twins: Dual mirrored orbital patterns with twin stars.
        /// Used by Fate3 (Singularity Spear)
        /// </summary>
        public static void ZodiacGeminiAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.035f;
            float radius = 50f * scale;
            
            // Twin orbits - mirror of each other
            for (int twin = 0; twin < 2; twin++)
            {
                float twinOffset = twin * MathHelper.Pi;
                int twinStars = 5;
                
                for (int i = 0; i < twinStars; i++)
                {
                    float starAngle = time + twinOffset + MathHelper.TwoPi * i / twinStars;
                    float starRadius = radius * (0.7f + (float)Math.Sin(time * 2f + i) * 0.2f);
                    Vector2 starPos = center + starAngle.ToRotationVector2() * starRadius;
                    
                    if (Main.rand.NextBool(10))
                    {
                        Color starColor = twin == 0 ? FateDarkPink * 0.6f : FateCyan * 0.5f;
                        CustomParticles.GenericFlare(starPos, starColor, 0.18f, 12);
                    }
                }
                
                // Main twin stars - larger, brighter
                float mainAngle = time * 0.5f + twinOffset;
                Vector2 mainStar = center + mainAngle.ToRotationVector2() * (radius * 0.6f);
                
                if (Main.rand.NextBool(8))
                {
                    Color mainColor = twin == 0 ? FateBrightRed * 0.7f : FateWhite * 0.6f;
                    CustomParticles.GenericFlare(mainStar, mainColor, 0.28f, 15);
                }
            }
            
            // Connection line between twins
            float twinAngle1 = time * 0.5f;
            float twinAngle2 = time * 0.5f + MathHelper.Pi;
            Vector2 twin1 = center + twinAngle1.ToRotationVector2() * (radius * 0.6f);
            Vector2 twin2 = center + twinAngle2.ToRotationVector2() * (radius * 0.6f);
            
            if (Main.rand.NextBool(12))
            {
                Vector2 linkPos = Vector2.Lerp(twin1, twin2, Main.rand.NextFloat());
                CustomParticles.GenericFlare(linkPos, FatePurple * 0.3f, 0.08f, 10);
            }
        }
        
        /// <summary>
        /// CANCER - The Crab: Protective shell dome pattern with lunar shimmer.
        /// Used by Fate4 (Destiny's Hammer)
        /// </summary>
        public static void ZodiacCancerAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            float radius = 55f * scale;
            
            // Protective shell - arc of stars above player
            int shellStars = 7;
            for (int i = 0; i < shellStars; i++)
            {
                float shellProgress = (float)i / (shellStars - 1);
                float shellAngle = MathHelper.Pi * 0.2f + MathHelper.Pi * 0.6f * shellProgress; // Arc from left to right above
                Vector2 shellPos = center + new Vector2((float)Math.Cos(shellAngle), -(float)Math.Sin(shellAngle)) * radius;
                
                if (Main.rand.NextBool(12))
                {
                    Color shellColor = GetFateGradient(shellProgress) * 0.55f;
                    CustomParticles.GenericFlare(shellPos, shellColor, 0.2f, 12);
                }
            }
            
            // Crab claw pincers - two curved shapes
            for (int claw = 0; claw < 2; claw++)
            {
                float clawDir = claw == 0 ? -1f : 1f;
                int clawPoints = 4;
                
                for (int i = 0; i < clawPoints; i++)
                {
                    float progress = (float)i / clawPoints;
                    Vector2 clawPos = center + new Vector2(clawDir * (20f + progress * 25f), 10f + progress * 15f - (float)Math.Sin(progress * MathHelper.Pi) * 20f) * scale;
                    
                    if (Main.rand.NextBool(15))
                        CustomParticles.GenericFlare(clawPos, FateDarkPink * 0.5f, 0.15f, 10);
                }
            }
            
            // Lunar shimmer particles - gentle side-to-side wave
            if (Main.rand.NextBool(6))
            {
                float lunarX = (float)Math.Sin(time * 1.5f) * radius * 0.8f;
                Vector2 lunarPos = center + new Vector2(lunarX, -radius * 0.5f);
                CustomParticles.GenericFlare(lunarPos, FateWhite * 0.35f, 0.12f, 15);
            }
        }
        
        /// <summary>
        /// LEO - The Lion: Radiant mane of light rays with regal crown formation.
        /// Used by Fate5 (Cosmic Scythe)
        /// </summary>
        public static void ZodiacLeoAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.045f;
            float radius = 50f * scale;
            
            // Radiant mane - rays emanating outward
            int maneRays = 12;
            for (int i = 0; i < maneRays; i++)
            {
                float rayAngle = MathHelper.TwoPi * i / maneRays + time * 0.3f;
                float rayPulse = (float)Math.Sin(time * 3f + i * 0.5f) * 0.3f + 0.7f;
                
                // Ray extends outward
                float rayLength = radius * (0.6f + rayPulse * 0.4f);
                Vector2 rayEnd = center + rayAngle.ToRotationVector2() * rayLength;
                
                if (Main.rand.NextBool(10))
                {
                    Color rayColor = Color.Lerp(FateBrightRed, FateWhite, rayPulse * 0.5f) * 0.5f;
                    CustomParticles.GenericFlare(rayEnd, rayColor, 0.18f + rayPulse * 0.1f, 10);
                }
                
                // Ray particles along the ray
                if (Main.rand.NextBool(20))
                {
                    float rayProgress = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 rayParticle = center + rayAngle.ToRotationVector2() * rayLength * rayProgress;
                    CustomParticles.GenericFlare(rayParticle, FateDarkPink * 0.3f, 0.08f, 8);
                }
            }
            
            // Crown formation - arc of brighter stars
            int crownStars = 5;
            for (int i = 0; i < crownStars; i++)
            {
                float crownProgress = (float)i / (crownStars - 1);
                float crownAngle = MathHelper.Pi * 0.3f + MathHelper.Pi * 0.4f * crownProgress;
                Vector2 crownPos = center + new Vector2((float)Math.Cos(crownAngle), -(float)Math.Sin(crownAngle)) * (radius * 0.7f);
                
                if (Main.rand.NextBool(12))
                    CustomParticles.GenericFlare(crownPos, FateWhite * 0.6f, 0.25f, 14);
            }
        }
        
        /// <summary>
        /// VIRGO - Precise geometric wings with orderly patterns.
        /// Used by Fate6 (Cannon of Inevitability)
        /// </summary>
        public static void ZodiacVirgoAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            float radius = 55f * scale;
            
            // Geometric wing patterns - precise and orderly
            for (int wing = 0; wing < 2; wing++)
            {
                float wingDir = wing == 0 ? -1f : 1f;
                int feathers = 6;
                
                for (int i = 0; i < feathers; i++)
                {
                    float featherProgress = (float)i / feathers;
                    float featherAngle = MathHelper.PiOver4 * wingDir + featherProgress * MathHelper.PiOver4 * wingDir * 0.8f;
                    float featherLength = radius * (0.5f + featherProgress * 0.5f);
                    
                    // Multiple points along each feather
                    for (int p = 1; p <= 3; p++)
                    {
                        float pointProgress = (float)p / 3f;
                        Vector2 featherPos = center + featherAngle.ToRotationVector2() * featherLength * pointProgress;
                        
                        if (Main.rand.NextBool(18))
                        {
                            Color featherColor = GetFateGradient(featherProgress) * 0.5f;
                            CustomParticles.GenericFlare(featherPos, featherColor, 0.12f + pointProgress * 0.08f, 10);
                        }
                    }
                }
            }
            
            // Central vertical line - the maiden's form
            int spineStars = 5;
            for (int i = 0; i < spineStars; i++)
            {
                float spineProgress = (float)i / spineStars;
                Vector2 spinePos = center + new Vector2(0, -radius * 0.3f + radius * 0.6f * spineProgress);
                
                if (Main.rand.NextBool(15))
                    CustomParticles.GenericFlare(spinePos, FateDarkPink * 0.5f, 0.15f, 12);
            }
        }
        
        /// <summary>
        /// LIBRA - The Scales: Balanced dual-pan scales with perfect symmetry.
        /// Used by Fate7 (Gun of Inevitable Doom)
        /// </summary>
        public static void ZodiacLibraAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.025f;
            float radius = 50f * scale;
            
            // Central balance beam
            float beamSway = (float)Math.Sin(time) * 5f;
            Vector2 beamLeft = center + new Vector2(-radius * 0.7f, beamSway);
            Vector2 beamRight = center + new Vector2(radius * 0.7f, -beamSway);
            
            // Balance beam line
            if (Main.rand.NextBool(8))
            {
                Vector2 beamPos = Vector2.Lerp(beamLeft, beamRight, Main.rand.NextFloat());
                CustomParticles.GenericFlare(beamPos, FatePurple * 0.4f, 0.1f, 10);
            }
            
            // Two scale pans
            for (int pan = 0; pan < 2; pan++)
            {
                float panDir = pan == 0 ? -1f : 1f;
                Vector2 panCenter = pan == 0 ? beamLeft : beamRight;
                
                // Pan rim - circle of stars
                int rimStars = 6;
                for (int i = 0; i < rimStars; i++)
                {
                    float rimAngle = MathHelper.TwoPi * i / rimStars + time * panDir * 0.5f;
                    Vector2 rimPos = panCenter + rimAngle.ToRotationVector2() * (radius * 0.3f);
                    
                    if (Main.rand.NextBool(12))
                    {
                        Color rimColor = pan == 0 ? FateDarkPink * 0.6f : FateCyan * 0.5f;
                        CustomParticles.GenericFlare(rimPos, rimColor, 0.15f, 10);
                    }
                }
                
                // Pan center
                if (Main.rand.NextBool(15))
                    CustomParticles.GenericFlare(panCenter, FateWhite * 0.5f, 0.2f, 12);
            }
            
            // Central pillar - fulcrum
            if (Main.rand.NextBool(10))
            {
                Vector2 fulcrum = center + new Vector2(0, -radius * 0.2f);
                CustomParticles.GenericFlare(fulcrum, FateBrightRed * 0.6f, 0.22f, 14);
            }
        }
        
        /// <summary>
        /// SCORPIO - Curving scorpion tail with stinger, intense coiling presence.
        /// Used by Fate8 (Tome of Infinite Fates)
        /// </summary>
        public static void ZodiacScorpioAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.04f;
            float radius = 55f * scale;
            
            // Scorpion tail - curves up and over
            int tailSegments = 8;
            for (int i = 0; i < tailSegments; i++)
            {
                float segProgress = (float)i / tailSegments;
                // Tail curves from bottom-right, arcs up and over to point down
                float tailAngle = -MathHelper.PiOver4 + segProgress * MathHelper.Pi * 1.2f;
                float tailRadius = radius * (0.3f + segProgress * 0.6f);
                
                Vector2 segPos = center + new Vector2((float)Math.Cos(tailAngle) * tailRadius * 0.6f, 
                                                       (float)Math.Sin(tailAngle) * tailRadius);
                
                if (Main.rand.NextBool(10))
                {
                    Color segColor = GetFateGradient(segProgress) * 0.6f;
                    CustomParticles.GenericFlare(segPos, segColor, 0.14f + segProgress * 0.08f, 12);
                }
                
                // Segment lines
                if (i > 0 && Main.rand.NextBool(25))
                {
                    float prevProgress = (float)(i - 1) / tailSegments;
                    float prevAngle = -MathHelper.PiOver4 + prevProgress * MathHelper.Pi * 1.2f;
                    float prevRadius = radius * (0.3f + prevProgress * 0.6f);
                    Vector2 prevPos = center + new Vector2((float)Math.Cos(prevAngle) * prevRadius * 0.6f,
                                                           (float)Math.Sin(prevAngle) * prevRadius);
                    Vector2 linePos = Vector2.Lerp(prevPos, segPos, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(linePos, FatePurple * 0.25f, 0.06f, 8);
                }
            }
            
            // Stinger - bright point at the end
            float stingerAngle = -MathHelper.PiOver4 + MathHelper.Pi * 1.2f;
            Vector2 stingerPos = center + new Vector2((float)Math.Cos(stingerAngle) * radius * 0.55f,
                                                       (float)Math.Sin(stingerAngle) * radius);
            
            if (Main.rand.NextBool(6))
            {
                float stingerPulse = (float)Math.Sin(time * 4f) * 0.3f + 0.7f;
                CustomParticles.GenericFlare(stingerPos, FateBrightRed * stingerPulse, 0.25f, 12);
            }
            
            // Pincers
            for (int pincer = 0; pincer < 2; pincer++)
            {
                float pincerDir = pincer == 0 ? -1f : 1f;
                Vector2 pincerPos = center + new Vector2(pincerDir * radius * 0.4f, radius * 0.3f);
                
                if (Main.rand.NextBool(15))
                    CustomParticles.GenericFlare(pincerPos, FateDarkPink * 0.5f, 0.18f, 10);
            }
        }
        
        /// <summary>
        /// SAGITTARIUS - The Archer: Bow and arrow formation aiming forward.
        /// Used by Fate9 (Cosmic Conduit Staff)
        /// </summary>
        public static void ZodiacSagittariusAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.035f;
            float radius = 55f * scale;
            
            // Bow arc
            int bowStars = 7;
            for (int i = 0; i < bowStars; i++)
            {
                float bowProgress = (float)i / (bowStars - 1);
                float bowAngle = MathHelper.PiOver2 - MathHelper.PiOver4 + MathHelper.PiOver2 * bowProgress;
                Vector2 bowPos = center + new Vector2((float)Math.Cos(bowAngle) * radius * 0.6f,
                                                       (float)Math.Sin(bowAngle) * radius * 0.8f - radius * 0.2f);
                
                if (Main.rand.NextBool(12))
                {
                    Color bowColor = GetFateGradient(bowProgress) * 0.55f;
                    CustomParticles.GenericFlare(bowPos, bowColor, 0.16f, 12);
                }
            }
            
            // Arrow - straight line pointing right/forward
            int arrowStars = 5;
            for (int i = 0; i < arrowStars; i++)
            {
                float arrowProgress = (float)i / arrowStars;
                Vector2 arrowPos = center + new Vector2(radius * 0.3f * arrowProgress, 0);
                
                if (Main.rand.NextBool(10))
                {
                    CustomParticles.GenericFlare(arrowPos, FateDarkPink * 0.6f, 0.12f + arrowProgress * 0.1f, 10);
                }
            }
            
            // Arrowhead
            Vector2 arrowhead = center + new Vector2(radius * 0.4f, 0);
            if (Main.rand.NextBool(8))
                CustomParticles.GenericFlare(arrowhead, FateBrightRed * 0.7f, 0.22f, 12);
            
            // Arrow fletching
            for (int fletch = 0; fletch < 2; fletch++)
            {
                float fletchDir = fletch == 0 ? -1f : 1f;
                Vector2 fletchPos = center + new Vector2(-radius * 0.15f, fletchDir * radius * 0.12f);
                
                if (Main.rand.NextBool(18))
                    CustomParticles.GenericFlare(fletchPos, FatePurple * 0.4f, 0.1f, 10);
            }
            
            // Occasional arrow flight particle
            if (Main.rand.NextBool(8))
            {
                Vector2 flightStart = center;
                Vector2 flightVel = new Vector2(4f, 0);
                var flight = new GenericGlowParticle(flightStart, flightVel, FateWhite * 0.4f, 0.1f, 20, true);
                MagnumParticleHandler.SpawnParticle(flight);
            }
        }
        
        /// <summary>
        /// CAPRICORN - Sea-Goat: Goat horns above and fish tail below, duality pattern.
        /// Used by Fate10 (Staff of Destined Champions)
        /// </summary>
        public static void ZodiacCapricornAura(Vector2 center, float scale = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            float radius = 55f * scale;
            
            // Goat horns - curved upward
            for (int horn = 0; horn < 2; horn++)
            {
                float hornDir = horn == 0 ? -1f : 1f;
                int hornPoints = 5;
                
                for (int i = 0; i < hornPoints; i++)
                {
                    float progress = (float)i / hornPoints;
                    float hornCurve = hornDir * (0.4f + progress * 0.6f);
                    Vector2 hornPos = center + new Vector2(hornCurve * radius * 0.5f, -radius * 0.3f - progress * radius * 0.4f);
                    
                    if (Main.rand.NextBool(14))
                    {
                        Color hornColor = GetFateGradient(progress) * 0.6f;
                        CustomParticles.GenericFlare(hornPos, hornColor, 0.16f, 12);
                    }
                }
            }
            
            // Fish tail - wavy pattern below
            int tailWaves = 6;
            for (int i = 0; i < tailWaves; i++)
            {
                float waveProgress = (float)i / tailWaves;
                float waveX = waveProgress * radius * 0.8f - radius * 0.4f;
                float waveY = radius * 0.2f + (float)Math.Sin(waveProgress * MathHelper.TwoPi + time) * radius * 0.15f;
                Vector2 wavePos = center + new Vector2(waveX, waveY);
                
                if (Main.rand.NextBool(12))
                {
                    Color waveColor = Color.Lerp(FateCyan, FateDarkPink, waveProgress) * 0.5f;
                    CustomParticles.GenericFlare(wavePos, waveColor, 0.14f, 10);
                }
            }
            
            // Tail fin at end
            Vector2 finPos = center + new Vector2(radius * 0.5f, radius * 0.25f);
            if (Main.rand.NextBool(15))
                CustomParticles.GenericFlare(finPos, FatePurple * 0.55f, 0.18f, 12);
            
            // Center body - connection point
            if (Main.rand.NextBool(10))
                CustomParticles.GenericFlare(center, FateWhite * 0.4f, 0.2f, 14);
        }
        
        #endregion

        /// <summary>
        /// Spawns lens flare particles without the draw layer - use this for immediate effects.
        /// Perfect for weapon fire effects and impacts.
        /// </summary>
        public static void SpawnLensFlareParticles(Vector2 worldPosition, float intensity = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;

            // === CENTRAL CORE ===
            CustomParticles.GenericFlare(worldPosition, FateWhite * intensity, 0.8f, 20);
            CustomParticles.GenericFlare(worldPosition, FateDarkPink * intensity * 0.8f, 0.6f, 18);
            CustomParticles.GenericFlare(worldPosition, FateBrightRed * intensity * 0.6f, 0.5f, 15);

            // === CHROMATIC RAYS ===
            for (int i = 0; i < 6; i++)
            {
                float rayAngle = MathHelper.TwoPi * i / 6f + time;
                Vector2 rayDir = rayAngle.ToRotationVector2();
                float rayLength = 40f * intensity;

                // RGB split rays
                Vector2 redPos = worldPosition + rayDir * rayLength * 0.9f - new Vector2(2, 0);
                Vector2 cyanPos = worldPosition + rayDir * rayLength * 1.1f + new Vector2(2, 0);

                CustomParticles.GenericFlare(redPos, Color.Red * 0.4f * intensity, 0.2f, 12);
                CustomParticles.GenericFlare(worldPosition + rayDir * rayLength, FateBrightRed * 0.5f * intensity, 0.25f, 15);
                CustomParticles.GenericFlare(cyanPos, FateCyan * 0.4f * intensity, 0.2f, 12);
            }

            // === HORIZONTAL ANAMORPHIC STREAK ===
            CustomParticles.GenericFlare(worldPosition + new Vector2(-30, 0) * intensity, FateDarkPink * 0.3f * intensity, 0.18f, 12);
            CustomParticles.GenericFlare(worldPosition + new Vector2(30, 0) * intensity, FateDarkPink * 0.3f * intensity, 0.18f, 12);

            // === HEXAGONAL GHOST ARTIFACTS ===
            Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
            Vector2 flareDir = (worldPosition - Main.screenPosition - screenCenter).SafeNormalize(Vector2.UnitY);

            for (int ghost = 1; ghost <= 3; ghost++)
            {
                float ghostProgress = ghost / 3f;
                Vector2 ghostPos = worldPosition - flareDir * 50f * ghostProgress * intensity;
                float ghostAlpha = (0.4f - ghostProgress * 0.1f) * intensity;

                Color ghostColor = GetFateGradient(ghostProgress);
                CustomParticles.GenericFlare(ghostPos, ghostColor * ghostAlpha, 0.2f * (1f - ghostProgress * 0.3f), 10);
            }

            // === HALO RING ===
            CustomParticles.HaloRing(worldPosition, FatePurple * intensity * 0.4f, 0.5f * intensity, 15);

            // === LIGHTING ===
            Lighting.AddLight(worldPosition, FateBrightRed.ToVector3() * intensity * 1.5f);
        }
    }
    
    /// <summary>
    /// Draw layer for lens flares - renders after everything else for proper layering.
    /// </summary>
    public class FateLensFlareDrawLayer : ModSystem
    {
        private static List<(Vector2 position, float intensity, float size, int lifetime, int maxLifetime)> activeFlares = new();
        
        public static void AddFlare(Vector2 worldPosition, float intensity = 1f, float size = 1f, int lifetime = 30)
        {
            activeFlares.Add((worldPosition, intensity, size, lifetime, lifetime));
        }
        
        public override void PostDrawTiles()
        {
            if (activeFlares.Count == 0) return;
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = activeFlares.Count - 1; i >= 0; i--)
            {
                var flare = activeFlares[i];
                float progress = (float)flare.lifetime / flare.maxLifetime;
                float fadeIntensity = flare.intensity * progress;
                
                FateLensFlare.DrawLensFlare(spriteBatch, flare.position, fadeIntensity, flare.size);
                
                // Update lifetime
                activeFlares[i] = (flare.position, flare.intensity, flare.size, flare.lifetime - 1, flare.maxLifetime);
                
                if (flare.lifetime <= 0)
                    activeFlares.RemoveAt(i);
            }
            
            spriteBatch.End();
        }
    }
}
