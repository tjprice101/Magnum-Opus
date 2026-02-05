using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Rainbow Gradient System for "Exo" style visual effects.
    /// 
    /// Provides time-based color cycling with smooth sine wave transitions
    /// for that signature prismatic, iridescent look.
    /// </summary>
    public static class RainbowGradientSystem
    {
        #region Core Gradient Functions
        
        /// <summary>
        /// Gets a rainbow color based on time.
        /// This is the foundation for all rainbow effects.
        /// </summary>
        /// <param name="timeOffset">Offset to create variation between particles</param>
        /// <param name="speed">How fast the rainbow cycles (1.0 = standard)</param>
        /// <param name="saturation">Color saturation 0-1</param>
        /// <param name="luminosity">Color brightness 0-1</param>
        public static Color GetRainbowColor(float timeOffset = 0f, float speed = 1f, float saturation = 1f, float luminosity = 0.65f)
        {
            float time = Main.GameUpdateCount * 0.01f * speed + timeOffset;
            float hue = time % 1f;
            return Main.hslToRgb(hue, saturation, luminosity);
        }
        
        /// <summary>
        /// Gets a rainbow color using sine wave for extra smoothness.
        /// Creates more organic transitions than linear hue shifting.
        /// </summary>
        public static Color GetSineRainbow(float timeOffset = 0f, float speed = 1f)
        {
            float time = Main.GameUpdateCount * 0.02f * speed + timeOffset;
            
            // Use sine waves for each channel with phase offsets
            float r = (float)(Math.Sin(time) * 0.5f + 0.5f);
            float g = (float)(Math.Sin(time + MathHelper.TwoPi / 3f) * 0.5f + 0.5f);
            float b = (float)(Math.Sin(time + MathHelper.TwoPi * 2f / 3f) * 0.5f + 0.5f);
            
            return new Color(r, g, b);
        }
        
        /// <summary>
        /// Gets a constrained rainbow within a specific hue range.
        /// Perfect for theme-colored weapons that still want rainbow variation.
        /// </summary>
        /// <param name="minHue">Minimum hue (0-1)</param>
        /// <param name="maxHue">Maximum hue (0-1)</param>
        /// <param name="timeOffset">Time offset for variation</param>
        /// <param name="speed">Cycle speed</param>
        public static Color GetConstrainedRainbow(float minHue, float maxHue, float timeOffset = 0f, float speed = 1f)
        {
            float time = Main.GameUpdateCount * 0.01f * speed + timeOffset;
            
            // Oscillate between min and max hue
            float oscillation = (float)(Math.Sin(time * MathHelper.TwoPi) * 0.5f + 0.5f);
            float hue = MathHelper.Lerp(minHue, maxHue, oscillation);
            
            return Main.hslToRgb(hue, 1f, 0.65f);
        }
        
        #endregion

        #region Theme-Specific Gradients
        
        /// <summary>
        /// Gets a color cycling through the Swan Lake palette (white/black with rainbow shimmer).
        /// </summary>
        public static Color GetSwanLakeShimmer(float timeOffset = 0f)
        {
            float time = Main.GameUpdateCount * 0.015f + timeOffset;
            
            // Primarily white with occasional rainbow
            float rainbowIntensity = (float)Math.Pow(Math.Sin(time * 2f) * 0.5f + 0.5f, 3f) * 0.4f;
            Color rainbow = GetRainbowColor(timeOffset, 0.5f, 0.8f, 0.7f);
            
            return Color.Lerp(Color.White, rainbow, rainbowIntensity);
        }
        
        /// <summary>
        /// Gets a color cycling through the Fate cosmic palette.
        /// Dark prismatic: black → pink → red with occasional white star sparkle.
        /// </summary>
        public static Color GetFateCosmic(float timeOffset = 0f)
        {
            float time = Main.GameUpdateCount * 0.008f + timeOffset;
            
            // Base oscillation through fate palette
            float t = (float)(Math.Sin(time) * 0.5f + 0.5f);
            
            Color black = new Color(15, 5, 20);
            Color darkPink = new Color(180, 50, 100);
            Color brightRed = new Color(255, 60, 80);
            
            Color baseColor;
            if (t < 0.5f)
            {
                baseColor = Color.Lerp(black, darkPink, t * 2f);
            }
            else
            {
                baseColor = Color.Lerp(darkPink, brightRed, (t - 0.5f) * 2f);
            }
            
            // Occasional white star sparkle
            float sparkle = (float)Math.Pow(Math.Sin(time * 5f) * 0.5f + 0.5f, 8f);
            return Color.Lerp(baseColor, Color.White, sparkle * 0.3f);
        }
        
        /// <summary>
        /// Gets a color for Eroica theme (scarlet → gold oscillation).
        /// </summary>
        public static Color GetEroicaFlame(float timeOffset = 0f)
        {
            float time = Main.GameUpdateCount * 0.012f + timeOffset;
            float t = (float)(Math.Sin(time) * 0.5f + 0.5f);
            
            Color scarlet = new Color(200, 50, 50);
            Color gold = new Color(255, 200, 80);
            
            return Color.Lerp(scarlet, gold, t);
        }
        
        /// <summary>
        /// Gets a color for La Campanella theme (black → orange → gold).
        /// </summary>
        public static Color GetCampanellaInferno(float timeOffset = 0f)
        {
            float time = Main.GameUpdateCount * 0.01f + timeOffset;
            float t = (float)(Math.Sin(time) * 0.5f + 0.5f);
            
            Color black = new Color(30, 20, 25);
            Color orange = new Color(255, 140, 40);
            Color gold = new Color(255, 200, 80);
            
            if (t < 0.5f)
            {
                return Color.Lerp(black, orange, t * 2f);
            }
            else
            {
                return Color.Lerp(orange, gold, (t - 0.5f) * 2f);
            }
        }
        
        /// <summary>
        /// Gets a color for Moonlight Sonata theme (purple → blue).
        /// </summary>
        public static Color GetMoonlightGlow(float timeOffset = 0f)
        {
            float time = Main.GameUpdateCount * 0.008f + timeOffset;
            float t = (float)(Math.Sin(time) * 0.5f + 0.5f);
            
            Color darkPurple = new Color(75, 0, 130);
            Color lightBlue = new Color(135, 206, 250);
            Color silver = new Color(220, 220, 235);
            
            // Tri-color oscillation
            if (t < 0.33f)
            {
                return Color.Lerp(darkPurple, lightBlue, t * 3f);
            }
            else if (t < 0.66f)
            {
                return Color.Lerp(lightBlue, silver, (t - 0.33f) * 3f);
            }
            else
            {
                return Color.Lerp(silver, darkPurple, (t - 0.66f) * 3f);
            }
        }
        
        /// <summary>
        /// Gets a color for Enigma theme (black → purple → green).
        /// </summary>
        public static Color GetEnigmaVoid(float timeOffset = 0f)
        {
            float time = Main.GameUpdateCount * 0.006f + timeOffset;
            float t = (float)(Math.Sin(time) * 0.5f + 0.5f);
            
            Color voidBlack = new Color(15, 10, 20);
            Color deepPurple = new Color(140, 60, 200);
            Color eerieGreen = new Color(50, 220, 100);
            
            if (t < 0.5f)
            {
                return Color.Lerp(voidBlack, deepPurple, t * 2f);
            }
            else
            {
                return Color.Lerp(deepPurple, eerieGreen, (t - 0.5f) * 2f);
            }
        }
        
        #endregion

        #region Gradient Helpers
        
        /// <summary>
        /// Creates a gradient between multiple colors based on a 0-1 parameter.
        /// </summary>
        /// <param name="colors">Array of colors to interpolate between</param>
        /// <param name="t">Parameter 0-1</param>
        public static Color MultiColorGradient(Color[] colors, float t)
        {
            if (colors == null || colors.Length == 0)
                return Color.White;
            if (colors.Length == 1)
                return colors[0];
                
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaledT = t * (colors.Length - 1);
            int index = (int)scaledT;
            float localT = scaledT - index;
            
            if (index >= colors.Length - 1)
                return colors[colors.Length - 1];
                
            return Color.Lerp(colors[index], colors[index + 1], localT);
        }
        
        /// <summary>
        /// Creates a pulsing brightness effect.
        /// </summary>
        /// <param name="baseColor">Base color to pulse</param>
        /// <param name="pulseSpeed">Pulse speed</param>
        /// <param name="pulseIntensity">How much to pulse (0-1)</param>
        public static Color PulseColor(Color baseColor, float pulseSpeed = 1f, float pulseIntensity = 0.3f)
        {
            float pulse = (float)(Math.Sin(Main.GameUpdateCount * 0.1f * pulseSpeed) * 0.5f + 0.5f);
            float brightness = 1f + pulse * pulseIntensity;
            
            return new Color(
                (int)Math.Min(255, baseColor.R * brightness),
                (int)Math.Min(255, baseColor.G * brightness),
                (int)Math.Min(255, baseColor.B * brightness),
                baseColor.A
            );
        }
        
        /// <summary>
        /// Creates a flickering effect for fire-like colors.
        /// </summary>
        public static Color FlickerColor(Color baseColor, float intensity = 0.2f)
        {
            float flicker = Main.rand.NextFloat(-intensity, intensity);
            float brightness = 1f + flicker;
            
            return new Color(
                (int)MathHelper.Clamp(baseColor.R * brightness, 0, 255),
                (int)MathHelper.Clamp(baseColor.G * brightness, 0, 255),
                (int)MathHelper.Clamp(baseColor.B * brightness, 0, 255),
                baseColor.A
            );
        }
        
        #endregion

        #region Position-Based Gradients
        
        /// <summary>
        /// Gets a rainbow color based on position for spatial rainbow effects.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="scale">How large the rainbow bands are</param>
        /// <param name="angle">Angle of the gradient bands</param>
        public static Color GetPositionalRainbow(Vector2 position, float scale = 0.01f, float angle = 0f)
        {
            float time = Main.GameUpdateCount * 0.02f;
            Vector2 rotated = position.RotatedBy(-angle);
            float hue = (rotated.X * scale + rotated.Y * scale * 0.5f + time) % 1f;
            if (hue < 0) hue += 1f;
            
            return Main.hslToRgb(hue, 1f, 0.65f);
        }
        
        /// <summary>
        /// Gets a radial rainbow emanating from a center point.
        /// </summary>
        public static Color GetRadialRainbow(Vector2 position, Vector2 center, float scale = 0.01f)
        {
            float time = Main.GameUpdateCount * 0.02f;
            float distance = Vector2.Distance(position, center);
            float hue = (distance * scale + time) % 1f;
            
            return Main.hslToRgb(hue, 1f, 0.65f);
        }
        
        /// <summary>
        /// Gets an angular rainbow (color based on angle from center).
        /// </summary>
        public static Color GetAngularRainbow(Vector2 position, Vector2 center, float rotationSpeed = 0.02f)
        {
            float time = Main.GameUpdateCount * rotationSpeed;
            float angle = (position - center).ToRotation();
            float hue = ((angle / MathHelper.TwoPi) + 0.5f + time) % 1f;
            
            return Main.hslToRgb(hue, 1f, 0.65f);
        }
        
        #endregion

        #region Trail Gradients
        
        /// <summary>
        /// Gets a color for trail rendering that fades and shifts.
        /// </summary>
        /// <param name="trailProgress">0 = newest (at projectile), 1 = oldest (end of trail)</param>
        /// <param name="timeOffset">For rainbow shifting</param>
        /// <param name="fadeToColor">Color to fade toward at trail end</param>
        public static Color GetTrailGradient(float trailProgress, float timeOffset = 0f, Color? fadeToColor = null)
        {
            Color rainbow = GetRainbowColor(timeOffset + trailProgress * 0.2f, 0.5f);
            Color fade = fadeToColor ?? Color.Transparent;
            
            // Fade alpha and color toward end of trail
            float alpha = 1f - trailProgress;
            Color result = Color.Lerp(rainbow, fade, trailProgress * 0.7f);
            result.A = (byte)(alpha * 255);
            
            return result;
        }
        
        /// <summary>
        /// Gets a prismatic trail color with white core fading to rainbow edge.
        /// </summary>
        public static Color GetPrismaticTrail(float trailProgress, float timeOffset = 0f)
        {
            // Core starts white, shifts to rainbow
            float coreToRainbow = MathHelper.Clamp(trailProgress * 3f, 0f, 1f);
            Color rainbow = GetRainbowColor(timeOffset + trailProgress * 0.3f, 0.7f);
            Color core = Color.Lerp(Color.White, rainbow, coreToRainbow);
            
            // Fade out toward end
            float alpha = 1f - (float)Math.Pow(trailProgress, 0.7f);
            core.A = (byte)(alpha * 255);
            
            return core;
        }
        
        #endregion
    }
}
