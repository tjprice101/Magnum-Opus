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
        /// Creates a kaleidoscopic burst effect - perfect for weapon impacts and special attacks.
        /// </summary>
        public static void KaleidoscopeBurst(Vector2 position, float scale = 1f, int segments = 8)
        {
            float time = Main.GameUpdateCount * 0.03f;
            
            // Central flare
            CustomParticles.GenericFlare(position, FateWhite, 0.8f * scale, 20);
            CustomParticles.GenericFlare(position, FateBrightRed, 0.6f * scale, 18);
            
            // Kaleidoscope segments - mirrored patterns
            for (int mirror = 0; mirror < 2; mirror++)
            {
                float mirrorMult = mirror == 0 ? 1f : -1f;
                
                for (int seg = 0; seg < segments; seg++)
                {
                    float segAngle = MathHelper.TwoPi * seg / segments + time;
                    float segProgress = (float)seg / segments;
                    
                    // Inner ring
                    Vector2 innerPos = position + (segAngle * mirrorMult).ToRotationVector2() * 25f * scale;
                    Color innerColor = GetFateGradient(segProgress);
                    CustomParticles.GenericFlare(innerPos, innerColor, 0.35f * scale, 15);
                    
                    // Middle ring (rotates opposite)
                    Vector2 middlePos = position + ((segAngle + MathHelper.Pi / segments) * -mirrorMult).ToRotationVector2() * 50f * scale;
                    Color middleColor = GetFateGradient((segProgress + 0.33f) % 1f);
                    CustomParticles.GenericFlare(middlePos, middleColor, 0.3f * scale, 15);
                    
                    // Outer ring
                    Vector2 outerPos = position + (segAngle * mirrorMult).ToRotationVector2() * 75f * scale;
                    Color outerColor = GetFateGradient((segProgress + 0.66f) % 1f);
                    CustomParticles.GenericFlare(outerPos, outerColor * 0.7f, 0.25f * scale, 15);
                    
                    // Chromatic trails between segments
                    if (seg % 2 == 0)
                    {
                        Vector2 trailVel = (segAngle * mirrorMult).ToRotationVector2() * 4f;
                        var redTrail = new GenericGlowParticle(innerPos - new Vector2(2, 0), trailVel, Color.Red * 0.5f, 0.2f * scale, 12, true);
                        var cyanTrail = new GenericGlowParticle(innerPos + new Vector2(2, 0), trailVel, FateCyan * 0.5f, 0.2f * scale, 12, true);
                        MagnumParticleHandler.SpawnParticle(redTrail);
                        MagnumParticleHandler.SpawnParticle(cyanTrail);
                    }
                }
            }
            
            // Expanding halos
            for (int halo = 0; halo < 3; halo++)
            {
                Color haloColor = GetFateGradient(halo / 3f);
                CustomParticles.HaloRing(position, haloColor * 0.6f, (0.3f + halo * 0.2f) * scale, 15 + halo * 5);
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
        /// Creates a cosmic event horizon effect - swirling darkness with bright edge.
        /// Perfect for black hole / singularity themed weapons.
        /// </summary>
        public static void EventHorizon(Vector2 position, float radius, float intensity = 1f)
        {
            float time = Main.GameUpdateCount * 0.03f;
            
            // Dark void center
            CustomParticles.GenericFlare(position, FateBlack, 0.5f * intensity, 8);
            
            // Accretion disk - swirling bright particles
            int diskParticles = (int)(12 * intensity);
            for (int i = 0; i < diskParticles; i++)
            {
                float angle = time * 2f + MathHelper.TwoPi * i / diskParticles;
                float diskRadius = radius * (0.7f + (float)Math.Sin(time + i) * 0.3f);
                Vector2 diskPos = position + angle.ToRotationVector2() * diskRadius;
                
                float progress = (float)i / diskParticles;
                Color diskColor = GetFateGradient(progress);
                
                // Velocity spirals inward
                Vector2 spiralVel = (angle + MathHelper.PiOver2).ToRotationVector2() * 2f - (diskPos - position).SafeNormalize(Vector2.Zero) * 0.5f;
                
                var diskParticle = new GenericGlowParticle(diskPos, spiralVel, diskColor * 0.6f, 0.12f + progress * 0.08f, 15, true);
                MagnumParticleHandler.SpawnParticle(diskParticle);
            }
            
            // Bright event horizon edge
            if (Main.rand.NextBool(2))
            {
                float edgeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edgePos = position + edgeAngle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(edgePos, FateBrightRed, 0.25f * intensity, 10);
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
