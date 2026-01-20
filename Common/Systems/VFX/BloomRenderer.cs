using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// High-level bloom and glow rendering utilities for MagnumOpus.
    /// Implements the multi-layer bloom stack patterns from FargosSoulsDLC.
    /// 
    /// Key patterns:
    /// - Multi-layer bloom stacking for vibrant glows
    /// - Proper { A = 0 } pattern for additive blending
    /// - Pulsing and animated bloom effects
    /// - Theme-specific bloom presets
    /// </summary>
    public static class BloomRenderer
    {
        #region Core Bloom Drawing
        
        /// <summary>
        /// Draws a standard 4-layer bloom stack. The FargosSoulsDLC standard pattern.
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch (should be in additive blend mode)</param>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <param name="primaryColor">Main glow color</param>
        /// <param name="scale">Base scale multiplier</param>
        /// <param name="opacity">Overall opacity (0-1)</param>
        public static void DrawBloomStack(SpriteBatch spriteBatch, Vector2 worldPosition, 
            Color primaryColor, float scale = 1f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            Color colorNoAlpha = primaryColor.WithoutAlpha();
            
            // Layer 1: Outer soft glow (largest, most transparent)
            spriteBatch.Draw(bloom, drawPos, null, 
                colorNoAlpha * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);
            
            // Layer 2: Middle glow
            spriteBatch.Draw(bloom, drawPos, null, 
                colorNoAlpha * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
            
            // Layer 3: Inner bloom
            spriteBatch.Draw(bloom, drawPos, null, 
                colorNoAlpha * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            
            // Layer 4: Bright white core
            spriteBatch.Draw(bloom, drawPos, null, 
                Color.White.WithoutAlpha() * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Draws a two-color bloom stack (outer and inner colors).
        /// </summary>
        public static void DrawBloomStack(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color outerColor, Color innerColor, float scale = 1f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            // Outer layers
            spriteBatch.Draw(bloom, drawPos, null, 
                outerColor.WithoutAlpha() * 0.3f * opacity, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloom, drawPos, null, 
                outerColor.WithoutAlpha() * 0.5f * opacity, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f);
            
            // Inner layers
            spriteBatch.Draw(bloom, drawPos, null, 
                innerColor.WithoutAlpha() * 0.7f * opacity, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloom, drawPos, null, 
                Color.White.WithoutAlpha() * 0.85f * opacity, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }
        
        /// <summary>
        /// Draws a simple single-layer bloom (for performance or subtle effects).
        /// </summary>
        public static void DrawSimpleBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float scale = 1f, float opacity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            spriteBatch.Draw(bloom, drawPos, null, 
                color.WithoutAlpha() * opacity, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        
        #endregion
        
        #region Animated Bloom Effects
        
        /// <summary>
        /// Draws a pulsing bloom with animated scale.
        /// </summary>
        /// <param name="pulseSpeed">Pulse frequency (48 = fast, 12 = slow)</param>
        /// <param name="pulseIntensity">Scale variation (0.1 = 10% variation)</param>
        public static void DrawPulsingBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float baseScale, float pulseSpeed = 48f, float pulseIntensity = 0.1f)
        {
            float pulse = MathF.Cos(Main.GlobalTimeWrappedHourly * pulseSpeed) * pulseIntensity + 1f;
            DrawBloomStack(spriteBatch, worldPosition, color, baseScale * pulse);
        }
        
        /// <summary>
        /// Draws a bloom with "breathe" animation (slower, more dramatic pulse).
        /// </summary>
        public static void DrawBreathingBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float baseScale, float breathPeriod = 2f)
        {
            float progress = (Main.GlobalTimeWrappedHourly % breathPeriod) / breathPeriod;
            float breath = VFXUtilities.SineBump(progress);
            float scale = baseScale * (0.8f + breath * 0.4f);
            float opacity = 0.7f + breath * 0.3f;
            
            DrawBloomStack(spriteBatch, worldPosition, color, scale, opacity);
        }
        
        /// <summary>
        /// Draws a charge-up bloom that grows over time.
        /// </summary>
        /// <param name="chargeProgress">Charge progress from 0 (start) to 1 (full)</param>
        public static void DrawChargeBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float chargeProgress, float maxScale = 2f)
        {
            float scale = 0.2f + chargeProgress * maxScale;
            float opacity = 0.3f + chargeProgress * 0.6f;
            
            // Add pulsing as charge increases
            float pulseIntensity = chargeProgress * 0.15f;
            float pulse = MathF.Cos(Main.GlobalTimeWrappedHourly * 30f) * pulseIntensity + 1f;
            
            DrawBloomStack(spriteBatch, worldPosition, color, scale * pulse, opacity);
        }
        
        /// <summary>
        /// Draws an impact bloom that quickly expands and fades.
        /// Call this every frame with increasing progress for the animation.
        /// </summary>
        /// <param name="progress">Animation progress from 0 (impact) to 1 (fully faded)</param>
        public static void DrawImpactBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float progress, float baseScale = 1f)
        {
            // Quick expand, slow fade
            float scaleProgress = VFXUtilities.EaseOut(progress, 3f);
            float opacityProgress = 1f - VFXUtilities.EaseIn(progress, 2f);
            
            float scale = baseScale * (0.5f + scaleProgress * 1.5f);
            
            DrawBloomStack(spriteBatch, worldPosition, color, scale, opacityProgress);
        }
        
        #endregion
        
        #region Flare Effects
        
        /// <summary>
        /// Draws a shine flare (4-point star) with optional rotation.
        /// </summary>
        public static void DrawShineFlare(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float scale, float rotation = 0f, float opacity = 1f)
        {
            Texture2D flare = MagnumTextureRegistry.ShineFlare4Point?.Value;
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            
            Vector2 drawPos = worldPosition - Main.screenPosition;
            Color colorNoAlpha = color.WithoutAlpha();
            
            if (bloom != null)
            {
                Vector2 bloomOrigin = bloom.Size() * 0.5f;
                
                // Background bloom
                spriteBatch.Draw(bloom, drawPos, null, 
                    colorNoAlpha * 0.4f * opacity, 0f, bloomOrigin, scale * 1.5f, SpriteEffects.None, 0f);
            }
            
            if (flare != null)
            {
                Vector2 flareOrigin = flare.Size() * 0.5f;
                spriteBatch.Draw(flare, drawPos, null, 
                    colorNoAlpha * opacity, rotation, flareOrigin, scale, SpriteEffects.None, 0f);
            }
            else if (bloom != null)
            {
                // Fallback: draw stretched blooms as makeshift cross
                Vector2 bloomOrigin = bloom.Size() * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = rotation + i * MathHelper.PiOver2;
                    Vector2 offset = angle.ToRotationVector2() * scale * 8f;
                    spriteBatch.Draw(bloom, drawPos + offset, null, 
                        colorNoAlpha * 0.5f * opacity, 0f, bloomOrigin, scale * 0.4f, SpriteEffects.None, 0f);
                }
            }
        }
        
        /// <summary>
        /// Draws an animated glimmer flare (appears, peaks, fades).
        /// </summary>
        /// <param name="glimmerProgress">Progress from 0 (appear) to 1 (fade)</param>
        public static void DrawGlimmerFlare(SpriteBatch spriteBatch, Vector2 worldPosition,
            Color color, float glimmerProgress, float baseScale = 1f)
        {
            float scaleInterpolant = VFXUtilities.Convert01To010(glimmerProgress);
            float scale = MathF.Pow(scaleInterpolant, 1.4f) * baseScale * 1.9f + 0.1f;
            float opacity = VFXUtilities.InverseLerp(1f, 0.75f, glimmerProgress);
            float rotation = Main.GlobalTimeWrappedHourly * 2f;
            
            DrawShineFlare(spriteBatch, worldPosition, color, scale, rotation, opacity);
        }
        
        #endregion
        
        #region Theme-Specific Bloom Presets
        
        /// <summary>
        /// Draws a La Campanella themed bloom (orange fire with black smoke undertones).
        /// </summary>
        public static void DrawLaCampanellaBloom(SpriteBatch spriteBatch, Vector2 worldPosition, 
            float scale = 1f, float intensity = 1f)
        {
            DrawBloomStack(spriteBatch, worldPosition, 
                MagnumThemePalettes.LaCampanellaOrange, 
                MagnumThemePalettes.LaCampanellaGold, 
                scale, intensity);
        }
        
        /// <summary>
        /// Draws an Eroica themed bloom (scarlet to gold heroic).
        /// </summary>
        public static void DrawEroicaBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            float scale = 1f, float intensity = 1f)
        {
            DrawBloomStack(spriteBatch, worldPosition,
                MagnumThemePalettes.EroicaScarlet,
                MagnumThemePalettes.EroicaGold,
                scale, intensity);
        }
        
        /// <summary>
        /// Draws a Moonlight Sonata themed bloom (purple to silver ethereal).
        /// </summary>
        public static void DrawMoonlightBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            float scale = 1f, float intensity = 1f)
        {
            DrawBloomStack(spriteBatch, worldPosition,
                MagnumThemePalettes.MoonlightDarkPurple,
                MagnumThemePalettes.MoonlightIceBlue,
                scale, intensity);
        }
        
        /// <summary>
        /// Draws a Swan Lake themed bloom (white with prismatic shimmer).
        /// </summary>
        public static void DrawSwanLakeBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            float scale = 1f, float intensity = 1f)
        {
            // Get subtle rainbow shimmer
            Color shimmer = MagnumThemePalettes.GetSwanRainbow(worldPosition.X * 0.01f);
            
            DrawBloomStack(spriteBatch, worldPosition,
                Color.White,
                shimmer,
                scale, intensity);
        }
        
        /// <summary>
        /// Draws an Enigma Variations themed bloom (purple to eerie green).
        /// </summary>
        public static void DrawEnigmaBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            float scale = 1f, float intensity = 1f)
        {
            DrawBloomStack(spriteBatch, worldPosition,
                MagnumThemePalettes.EnigmaPurple,
                MagnumThemePalettes.EnigmaGreenFlame,
                scale, intensity);
        }
        
        /// <summary>
        /// Draws a Fate themed bloom (cosmic dark pink to bright red with white stars).
        /// </summary>
        public static void DrawFateBloom(SpriteBatch spriteBatch, Vector2 worldPosition,
            float scale = 1f, float intensity = 1f)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = worldPosition - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            // Outer cosmic void
            spriteBatch.Draw(bloom, drawPos, null,
                MagnumThemePalettes.FateBlack.WithoutAlpha() * 0.4f * intensity, 
                0f, origin, scale * 2.2f, SpriteEffects.None, 0f);
            
            // Dark pink layer
            spriteBatch.Draw(bloom, drawPos, null,
                MagnumThemePalettes.FateDarkPink.WithoutAlpha() * 0.5f * intensity,
                0f, origin, scale * 1.5f, SpriteEffects.None, 0f);
            
            // Bright red layer
            spriteBatch.Draw(bloom, drawPos, null,
                MagnumThemePalettes.FateBrightRed.WithoutAlpha() * 0.7f * intensity,
                0f, origin, scale * 1.0f, SpriteEffects.None, 0f);
            
            // White star core
            spriteBatch.Draw(bloom, drawPos, null,
                Color.White.WithoutAlpha() * 0.9f * intensity,
                0f, origin, scale * 0.4f, SpriteEffects.None, 0f);
        }
        
        #endregion
        
        #region Particle Spawning Helpers
        
        /// <summary>
        /// Spawns a standard bloom impact burst at a position.
        /// </summary>
        public static void SpawnImpactBloom(Vector2 position, Color color, float scale = 1f)
        {
            MagnumParticleHandler.SpawnParticle(new StrongBloomParticle(
                position, Vector2.Zero, color, scale, 15));
        }
        
        /// <summary>
        /// Spawns a radial burst of bloom pixels.
        /// </summary>
        public static void SpawnBloomBurst(Vector2 position, Color startColor, Color endColor,
            int count = 12, float speed = 6f, float scale = 0.8f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 velocity = angle.ToRotationVector2() * (speed + Main.rand.NextFloat(speed * 0.5f));
                
                MagnumParticleHandler.SpawnParticle(new BloomPixelParticle(
                    position, velocity, startColor, endColor, 
                    20 + Main.rand.Next(10), 
                    scale + Main.rand.NextFloat(0.2f), 
                    0.1f));
            }
        }
        
        /// <summary>
        /// Spawns a glimmer/gleam flare particle.
        /// </summary>
        public static void SpawnGlimmer(Vector2 position, Color color, float scale = 1f, int lifetime = 20)
        {
            MagnumParticleHandler.SpawnParticle(new ShineFlareParticle(
                position, color, scale, lifetime));
        }
        
        /// <summary>
        /// Spawns themed bloom effects based on theme name.
        /// </summary>
        public static void SpawnThemedImpact(Vector2 position, string themeName, float scale = 1f)
        {
            Color primary = MagnumThemePalettes.GetThemePrimary(themeName);
            Color secondary = MagnumThemePalettes.GetThemeSecondary(themeName);
            
            SpawnImpactBloom(position, primary, scale);
            SpawnBloomBurst(position, primary, secondary, 8, 5f, scale * 0.6f);
        }
        
        #endregion
    }
}
