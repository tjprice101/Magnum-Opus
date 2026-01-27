using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Essential utility functions for VFX rendering in MagnumOpus.
    /// Implements patterns from FargosSoulsDLC/Luminance library.
    /// 
    /// Key concepts:
    /// - QuadraticBump: 0→1→0 curve for edge fadeouts
    /// - InverseLerp: Maps a value from one range to 0-1
    /// - Convert01To010: Converts 0→1 to 0→1→0
    /// - Proper color manipulation for additive blending
    /// </summary>
    public static class VFXUtilities
    {
        #region Mathematical Utilities
        
        /// <summary>
        /// The QuadraticBump function - creates a smooth 0→1→0 curve.
        /// Used everywhere in FargosSoulsDLC shaders for edge-to-center intensity.
        /// 
        /// Input 0.0 → Output 0.0
        /// Input 0.5 → Output 1.0 (peak)
        /// Input 1.0 → Output 0.0
        /// </summary>
        /// <param name="x">Input value from 0 to 1</param>
        /// <returns>Smooth bump curve value</returns>
        public static float QuadraticBump(float x)
        {
            return x * (4f - x * 4f);
        }
        
        /// <summary>
        /// Converts a 0→1 interpolant to a 0→1→0 interpolant (triangle wave).
        /// Perfect for effects that appear, peak, then disappear.
        /// </summary>
        public static float Convert01To010(float interpolant)
        {
            return interpolant < 0.5f 
                ? interpolant * 2f 
                : 2f - interpolant * 2f;
        }
        
        /// <summary>
        /// Converts a 0→1 interpolant to a 0→1→0 interpolant using sine.
        /// Smoother than Convert01To010.
        /// </summary>
        public static float SineBump(float interpolant)
        {
            return MathF.Sin(interpolant * MathHelper.Pi);
        }
        
        /// <summary>
        /// Inverse linear interpolation - maps a value from [min, max] to [0, 1].
        /// Clamped version.
        /// </summary>
        public static float InverseLerp(float min, float max, float value)
        {
            if (Math.Abs(max - min) < 0.0001f)
                return 0f;
            return MathHelper.Clamp((value - min) / (max - min), 0f, 1f);
        }
        
        /// <summary>
        /// Inverse linear interpolation - unclamped version.
        /// </summary>
        public static float InverseLerpUnclamped(float min, float max, float value)
        {
            if (Math.Abs(max - min) < 0.0001f)
                return 0f;
            return (value - min) / (max - min);
        }
        
        /// <summary>
        /// Creates a bump interpolant that goes 0→1→0 across specified ranges.
        /// Useful for "active zone" effects.
        /// 
        /// Example: InverseLerpBump(0.1f, 0.3f, 0.7f, 0.9f, x)
        /// - x &lt; 0.1: returns 0
        /// - x = 0.1 to 0.3: ramps from 0 to 1
        /// - x = 0.3 to 0.7: stays at 1
        /// - x = 0.7 to 0.9: ramps from 1 to 0
        /// - x > 0.9: returns 0
        /// </summary>
        public static float InverseLerpBump(float rampUpStart, float rampUpEnd, 
            float rampDownStart, float rampDownEnd, float value)
        {
            float rampUp = InverseLerp(rampUpStart, rampUpEnd, value);
            float rampDown = InverseLerp(rampDownEnd, rampDownStart, value);
            return rampUp * rampDown;
        }
        
        /// <summary>
        /// Applies easing to a 0-1 interpolant. Power controls the curve steepness.
        /// </summary>
        public static float EaseIn(float t, float power = 2f)
        {
            return MathF.Pow(t, power);
        }
        
        /// <summary>
        /// Ease out curve - fast start, slow end.
        /// </summary>
        public static float EaseOut(float t, float power = 2f)
        {
            return 1f - MathF.Pow(1f - t, power);
        }
        
        /// <summary>
        /// Ease in-out curve - slow start and end, fast middle.
        /// </summary>
        public static float EaseInOut(float t, float power = 2f)
        {
            return t < 0.5f 
                ? MathF.Pow(2f, power - 1f) * MathF.Pow(t, power)
                : 1f - MathF.Pow(-2f * t + 2f, power) / 2f;
        }
        
        /// <summary>
        /// Sine wave with output range [0, 1] instead of [-1, 1].
        /// </summary>
        public static float Cos01(float radians)
        {
            return (MathF.Cos(radians) + 1f) * 0.5f;
        }
        
        /// <summary>
        /// Sine wave with output range [0, 1].
        /// </summary>
        public static float Sin01(float radians)
        {
            return (MathF.Sin(radians) + 1f) * 0.5f;
        }
        
        /// <summary>
        /// Smoothstep function - smooth Hermite interpolation.
        /// </summary>
        public static float Smoothstep(float edge0, float edge1, float x)
        {
            float t = MathHelper.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return t * t * (3f - 2f * t);
        }
        
        /// <summary>
        /// Smootherstep - Ken Perlin's improved smoothstep.
        /// </summary>
        public static float Smootherstep(float edge0, float edge1, float x)
        {
            float t = MathHelper.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }
        
        #endregion
        
        #region Color Utilities
        
        /// <summary>
        /// CRITICAL: Removes alpha channel for proper additive blending.
        /// This is THE most important pattern for bloom effects.
        /// 
        /// Usage: color.WithoutAlpha() * opacity
        /// </summary>
        public static Color WithoutAlpha(this Color color)
        {
            return new Color(color.R, color.G, color.B, 0);
        }
        
        /// <summary>
        /// Creates a color with the same RGB but specified alpha.
        /// </summary>
        public static Color WithAlpha(this Color color, byte alpha)
        {
            return new Color(color.R, color.G, color.B, alpha);
        }
        
        /// <summary>
        /// Creates a color with the same RGB but specified alpha (0-1 range).
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.R, color.G, color.B, (byte)(alpha * 255));
        }
        
        /// <summary>
        /// Multiplies only the RGB components, leaving alpha unchanged.
        /// Useful for brightness adjustment without affecting transparency.
        /// </summary>
        public static Color MultiplyRGB(this Color color, float factor)
        {
            return new Color(
                (byte)MathHelper.Clamp(color.R * factor, 0, 255),
                (byte)MathHelper.Clamp(color.G * factor, 0, 255),
                (byte)MathHelper.Clamp(color.B * factor, 0, 255),
                color.A);
        }
        
        /// <summary>
        /// Linearly interpolates through a color palette array.
        /// Essential for multi-color gradients.
        /// </summary>
        /// <param name="palette">Array of colors to interpolate through</param>
        /// <param name="progress">Progress through the palette (0 to 1)</param>
        public static Color PaletteLerp(Color[] palette, float progress)
        {
            if (palette == null || palette.Length == 0)
                return Color.White;
            if (palette.Length == 1)
                return palette[0];
                
            progress = MathHelper.Clamp(progress, 0f, 1f);
            float scaledProgress = progress * (palette.Length - 1);
            int startIndex = (int)scaledProgress;
            int endIndex = Math.Min(startIndex + 1, palette.Length - 1);
            float localProgress = scaledProgress - startIndex;
            
            return Color.Lerp(palette[startIndex], palette[endIndex], localProgress);
        }
        
        /// <summary>
        /// Creates a 3-color gradient (start → middle → end).
        /// </summary>
        public static Color ThreeColorGradient(Color start, Color middle, Color end, float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(start, middle, progress * 2f);
            else
                return Color.Lerp(middle, end, (progress - 0.5f) * 2f);
        }
        
        /// <summary>
        /// Gets a rainbow color based on hue (0-1).
        /// </summary>
        public static Color GetRainbow(float hueOffset = 0f, float saturation = 1f, float lightness = 0.5f)
        {
            float hue = (Main.GlobalTimeWrappedHourly * 0.5f + hueOffset) % 1f;
            return Main.hslToRgb(hue, saturation, lightness);
        }
        
        /// <summary>
        /// Shifts the hue of a color by the specified amount (0-1 = full rotation).
        /// </summary>
        public static Color HueShift(this Color color, float shift)
        {
            // Convert to HSL
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float l = (max + min) / 2f;
            float h = 0f, s = 0f;
            
            if (max != min)
            {
                float d = max - min;
                s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
                
                if (max == r)
                    h = (g - b) / d + (g < b ? 6f : 0f);
                else if (max == g)
                    h = (b - r) / d + 2f;
                else
                    h = (r - g) / d + 4f;
                h /= 6f;
            }
            
            // Shift hue
            h = (h + shift) % 1f;
            if (h < 0f) h += 1f;
            
            return Main.hslToRgb(h, s, l);
        }
        
        #endregion
        
        #region SpriteBatch Extensions
        
        /// <summary>
        /// Prepares the SpriteBatch for shader/additive rendering.
        /// Call this before drawing bloom effects.
        /// </summary>
        public static void PrepareForShaders(this SpriteBatch spriteBatch, BlendState blendState = null)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Immediate, 
                blendState ?? BlendState.Additive, 
                SamplerState.LinearClamp, 
                DepthStencilState.None, 
                RasterizerState.CullNone, 
                null, 
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Resets the SpriteBatch to default state after shader operations.
        /// </summary>
        public static void ResetToDefault(this SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                Main.DefaultSamplerState, 
                DepthStencilState.None, 
                Main.Rasterizer, 
                null, 
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Begins a SpriteBatch optimized for additive bloom drawing.
        /// </summary>
        public static void BeginAdditive(this SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.Additive, 
                SamplerState.LinearClamp, 
                DepthStencilState.None, 
                RasterizerState.CullNone, 
                null, 
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Begins a SpriteBatch for standard alpha-blended drawing.
        /// </summary>
        public static void BeginAlphaBlend(this SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                Main.DefaultSamplerState, 
                DepthStencilState.None, 
                Main.Rasterizer, 
                null, 
                Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Drawing Utilities
        
        /// <summary>
        /// Draws a multi-layer bloom stack at a position.
        /// The standard FargosSoulsDLC bloom pattern.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw with</param>
        /// <param name="position">World position</param>
        /// <param name="primaryColor">Main bloom color</param>
        /// <param name="scale">Base scale</param>
        /// <param name="layers">Number of bloom layers (default 3)</param>
        public static void DrawBloomStack(SpriteBatch spriteBatch, Vector2 position, 
            Color primaryColor, float scale = 1f, int layers = 3)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = position - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            // Layer multipliers for standard bloom stack
            float[] scaleMultipliers = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacityMultipliers = { 0.3f, 0.5f, 0.7f, 0.85f };
            
            for (int i = 0; i < Math.Min(layers, 4); i++)
            {
                Color layerColor = i == layers - 1 
                    ? Color.White.WithoutAlpha() * opacityMultipliers[i]
                    : primaryColor.WithoutAlpha() * opacityMultipliers[i];
                    
                spriteBatch.Draw(bloom, drawPos, null, layerColor, 0f, origin, 
                    scale * scaleMultipliers[i], SpriteEffects.None, 0f);
            }
        }
        
        /// <summary>
        /// Draws a pulsing bloom effect with animated scale.
        /// </summary>
        public static void DrawPulsingBloom(SpriteBatch spriteBatch, Vector2 position,
            Color color, float baseScale, float pulseIntensity = 0.1f, float pulseSpeed = 48f)
        {
            float pulse = MathF.Cos(Main.GlobalTimeWrappedHourly * pulseSpeed) * pulseIntensity + 1f;
            DrawBloomStack(spriteBatch, position, color, baseScale * pulse);
        }
        
        /// <summary>
        /// Draws a shine flare (4-point star) with rotation animation.
        /// </summary>
        public static void DrawShineFlare(SpriteBatch spriteBatch, Vector2 position,
            Color color, float scale, float rotationSpeed = 2f)
        {
            Texture2D flare = MagnumTextureRegistry.GetShineFlare4Point();
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            
            if (bloom == null) return;
            
            Vector2 drawPos = position - Main.screenPosition;
            float rotation = Main.GlobalTimeWrappedHourly * rotationSpeed;
            
            // Background bloom
            Vector2 bloomOrigin = bloom.Size() * 0.5f;
            spriteBatch.Draw(bloom, drawPos, null, color.WithoutAlpha() * 0.4f, 0f, 
                bloomOrigin, scale * 1.5f, SpriteEffects.None, 0f);
            
            // Flare on top (use bloom as fallback if flare not available)
            if (flare != null)
            {
                Vector2 flareOrigin = flare.Size() * 0.5f;
                spriteBatch.Draw(flare, drawPos, null, color.WithoutAlpha(), rotation, 
                    flareOrigin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                // Fallback: draw stretched blooms as cross
                for (int i = 0; i < 4; i++)
                {
                    float angle = rotation + i * MathHelper.PiOver2;
                    Vector2 offset = angle.ToRotationVector2() * scale * 10f;
                    spriteBatch.Draw(bloom, drawPos + offset, null, color.WithoutAlpha() * 0.6f, 
                        0f, bloomOrigin, scale * 0.3f, SpriteEffects.None, 0f);
                }
            }
        }
        
        #endregion
        
        // NOTE: Vector utilities like SafeNormalize, ToRotationVector2, and ToRotation
        // are provided by Terraria.Utils - use those instead to avoid ambiguity
    }
}
