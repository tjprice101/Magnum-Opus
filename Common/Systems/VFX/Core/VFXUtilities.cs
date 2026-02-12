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
    /// - QuadraticBump: 0ↁEↁE curve for edge fadeouts
    /// - InverseLerp: Maps a value from one range to 0-1
    /// - Convert01To010: Converts 0ↁE to 0ↁEↁE
    /// - Proper color manipulation for additive blending
    /// </summary>
    public static class VFXUtilities
    {
        #region Mathematical Utilities
        
        /// <summary>
        /// The QuadraticBump function - creates a smooth 0ↁEↁE curve.
        /// Used everywhere in FargosSoulsDLC shaders for edge-to-center intensity.
        /// 
        /// Input 0.0 ↁEOutput 0.0
        /// Input 0.5 ↁEOutput 1.0 (peak)
        /// Input 1.0 ↁEOutput 0.0
        /// </summary>
        /// <param name="x">Input value from 0 to 1</param>
        /// <returns>Smooth bump curve value</returns>
        public static float QuadraticBump(float x)
        {
            return x * (4f - x * 4f);
        }
        
        /// <summary>
        /// Converts a 0ↁE interpolant to a 0ↁEↁE interpolant (triangle wave).
        /// Perfect for effects that appear, peak, then disappear.
        /// </summary>
        public static float Convert01To010(float interpolant)
        {
            return interpolant < 0.5f 
                ? interpolant * 2f 
                : 2f - interpolant * 2f;
        }
        
        /// <summary>
        /// Converts a 0ↁE interpolant to a 0ↁEↁE interpolant using sine.
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
        /// Creates a bump interpolant that goes 0ↁEↁE across specified ranges.
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
        
        /// <summary>
        /// Hermite interpolation - spline with explicit tangents.
        /// Allows control over the curve's shape at start and end.
        /// 
        /// p0 = start position, p1 = end position
        /// m0 = start tangent (velocity), m1 = end tangent
        /// t = interpolation parameter (0-1)
        /// 
        /// The Hermite basis functions are:
        ///   h00(t) = 2t³ - 3t² + 1     (position at start)
        ///   h01(t) = -2t³ + 3t²        (position at end)
        ///   h10(t) = t³ - 2t² + t      (tangent at start)
        ///   h11(t) = t³ - t²           (tangent at end)
        /// </summary>
        public static float Hermite(float p0, float p1, float m0, float m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h01 = -2f * t3 + 3f * t2;
            float h10 = t3 - 2f * t2 + t;
            float h11 = t3 - t2;
            
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }
        
        /// <summary>
        /// Vector Hermite interpolation with explicit tangents.
        /// </summary>
        public static Vector2 HermiteVector(Vector2 p0, Vector2 p1, Vector2 m0, Vector2 m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h01 = -2f * t3 + 3f * t2;
            float h10 = t3 - 2f * t2 + t;
            float h11 = t3 - t2;
            
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }
        
        /// <summary>
        /// Elastic easing - overshoots and oscillates before settling.
        /// Great for bouncy, springy animations.
        /// </summary>
        public static float EaseOutElastic(float t, float amplitude = 1f, float period = 0.3f)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            
            float s = period / MathHelper.TwoPi * MathF.Asin(1f / amplitude);
            return amplitude * MathF.Pow(2f, -10f * t) * MathF.Sin((t - s) * MathHelper.TwoPi / period) + 1f;
        }
        
        /// <summary>
        /// Bounce easing - simulates a bouncing ball.
        /// </summary>
        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            
            if (t < 1f / d1)
                return n1 * t * t;
            else if (t < 2f / d1)
                return n1 * (t -= 1.5f / d1) * t + 0.75f;
            else if (t < 2.5f / d1)
                return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            else
                return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
        
        /// <summary>
        /// Back easing - overshoots, then returns.
        /// </summary>
        public static float EaseOutBack(float t, float overshoot = 1.70158f)
        {
            t = t - 1f;
            return t * t * ((overshoot + 1f) * t + overshoot) + 1f;
        }
        
        /// <summary>
        /// Exponential easing - starts very slow, then accelerates rapidly.
        /// </summary>
        public static float EaseOutExpo(float t)
        {
            return t >= 1f ? 1f : 1f - MathF.Pow(2f, -10f * t);
        }
        
        /// <summary>
        /// Circular easing - follows a circular arc.
        /// </summary>
        public static float EaseOutCirc(float t)
        {
            return MathF.Sqrt(1f - MathF.Pow(t - 1f, 2f));
        }
        
        /// <summary>
        /// Applies an easing function in both directions (ease in, then ease out).
        /// </summary>
        public static float EaseInOutElastic(float t, float amplitude = 1f, float period = 0.45f)
        {
            if (t < 0.5f)
            {
                // Ease in
                t *= 2f;
                float s = period / MathHelper.TwoPi * MathF.Asin(1f / amplitude);
                return -0.5f * amplitude * MathF.Pow(2f, 10f * (t - 1f)) * 
                       MathF.Sin((t - 1f - s) * MathHelper.TwoPi / period);
            }
            else
            {
                // Ease out
                t = t * 2f - 1f;
                float s = period / MathHelper.TwoPi * MathF.Asin(1f / amplitude);
                return amplitude * MathF.Pow(2f, -10f * t) * 
                       MathF.Sin((t - s) * MathHelper.TwoPi / period) * 0.5f + 1f;
            }
        }
        
        /// <summary>
        /// Spring physics interpolation - simulates damped harmonic oscillation.
        /// Returns position based on spring physics parameters.
        /// </summary>
        /// <param name="t">Time parameter (0 to ~1, can go higher for continued oscillation)</param>
        /// <param name="frequency">Oscillation frequency (higher = faster bounce)</param>
        /// <param name="damping">Damping ratio (0 = no damping, 1 = critical damping)</param>
        public static float SpringInterpolation(float t, float frequency = 8f, float damping = 0.5f)
        {
            if (t <= 0f) return 0f;
            
            float dampingFactor = MathF.Exp(-damping * t * frequency);
            float oscillation = MathF.Cos(frequency * MathF.Sqrt(1f - damping * damping) * t);
            
            return 1f - dampingFactor * oscillation;
        }
        
        /// <summary>
        /// Spring physics for Vector2 - interpolates from start to end with spring motion.
        /// </summary>
        public static Vector2 SpringLerp(Vector2 start, Vector2 end, float t, 
            float frequency = 8f, float damping = 0.5f)
        {
            float springT = SpringInterpolation(t, frequency, damping);
            return Vector2.Lerp(start, end, springT);
        }
        
        /// <summary>
        /// Remap a value from one range to another.
        /// Useful for converting between different parameter spaces.
        /// </summary>
        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            float t = InverseLerp(inMin, inMax, value);
            return MathHelper.Lerp(outMin, outMax, t);
        }
        
        /// <summary>
        /// Wrap a value to a range (like modulo but handles negatives correctly).
        /// </summary>
        public static float Wrap(float value, float min, float max)
        {
            float range = max - min;
            return min + ((((value - min) % range) + range) % range);
        }
        
        /// <summary>
        /// Ping-pong a value between 0 and max (triangle wave).
        /// </summary>
        public static float PingPong(float t, float max)
        {
            t = Wrap(t, 0f, max * 2f);
            return max - MathF.Abs(t - max);
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
        /// Creates a 3-color gradient (start ↁEmiddle ↁEend).
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
            try { spriteBatch.End(); } catch { }
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
            try { spriteBatch.End(); } catch { }
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
        
        #region Velocity Stretch & Motion Utilities
        
        /// <summary>
        /// Calculates squash and stretch scale based on velocity magnitude.
        /// Used for making sprites appear to "stretch" when moving fast.
        /// </summary>
        /// <param name="velocity">Current velocity of the entity</param>
        /// <param name="stretchFactor">How much velocity affects stretch (0.01-0.05 typical)</param>
        /// <param name="minScale">Minimum Y scale (squash limit)</param>
        /// <param name="maxStretch">Maximum Y scale (stretch limit)</param>
        /// <returns>Scale vector (X is inverse of Y for volume preservation)</returns>
        public static Vector2 GetSquashStretch(Vector2 velocity, float stretchFactor = 0.02f, 
            float minScale = 0.8f, float maxStretch = 1.5f)
        {
            float speed = velocity.Length();
            float yScale = MathHelper.Clamp(1f + speed * stretchFactor, minScale, maxStretch);
            // Preserve approximate volume by inverse-scaling X
            float xScale = 1f / MathF.Sqrt(yScale);
            return new Vector2(xScale, yScale);
        }
        
        /// <summary>
        /// Calculates squash and stretch with direction alignment.
        /// The stretch is applied along the velocity direction.
        /// </summary>
        /// <param name="velocity">Current velocity</param>
        /// <param name="stretchFactor">Stretch intensity</param>
        /// <param name="maxStretch">Maximum stretch ratio</param>
        /// <returns>Tuple of (scale, rotation) for drawing</returns>
        public static (Vector2 scale, float rotation) GetDirectionalStretch(Vector2 velocity, 
            float stretchFactor = 0.03f, float maxStretch = 1.8f)
        {
            float speed = velocity.Length();
            if (speed < 0.01f)
                return (Vector2.One, 0f);
                
            float rotation = velocity.ToRotation();
            float yScale = MathHelper.Clamp(1f + speed * stretchFactor, 0.7f, maxStretch);
            float xScale = 1f / MathF.Sqrt(yScale);
            
            return (new Vector2(xScale, yScale), rotation);
        }
        
        /// <summary>
        /// Gets motion blur sample positions along the velocity direction.
        /// </summary>
        /// <param name="position">Current position</param>
        /// <param name="velocity">Current velocity</param>
        /// <param name="samples">Number of blur samples</param>
        /// <param name="spread">How far back to spread samples (0.5 = half velocity)</param>
        /// <returns>Array of positions for motion blur drawing</returns>
        public static Vector2[] GetMotionBlurPositions(Vector2 position, Vector2 velocity, 
            int samples = 4, float spread = 0.5f)
        {
            Vector2[] positions = new Vector2[samples];
            Vector2 offset = velocity * spread / samples;
            
            for (int i = 0; i < samples; i++)
            {
                positions[i] = position - offset * (i + 1);
            }
            
            return positions;
        }
        
        /// <summary>
        /// Calculates alpha values for motion blur samples (fading trail).
        /// </summary>
        /// <param name="samples">Number of samples</param>
        /// <param name="falloff">How quickly alpha falls off (1 = linear, 2 = quadratic)</param>
        /// <returns>Array of alpha values from 1 to 0</returns>
        public static float[] GetMotionBlurAlphas(int samples, float falloff = 1.5f)
        {
            float[] alphas = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float progress = (float)i / samples;
                alphas[i] = MathF.Pow(1f - progress, falloff);
            }
            
            return alphas;
        }
        
        /// <summary>
        /// Interpolates position for sub-pixel smoothing.
        /// Essential for 144Hz+ smooth rendering.
        /// </summary>
        /// <param name="oldPosition">Position from previous frame</param>
        /// <param name="currentPosition">Current position</param>
        /// <returns>Interpolated position for smooth rendering</returns>
        public static Vector2 InterpolatePosition(Vector2 oldPosition, Vector2 currentPosition)
        {
            return Vector2.Lerp(oldPosition, currentPosition, InterpolatedRenderer.PartialTicks);
        }
        
        /// <summary>
        /// Interpolates rotation with proper angle wrapping.
        /// </summary>
        public static float InterpolateRotation(float oldRotation, float currentRotation, float amount = 0.5f)
        {
            float diff = MathHelper.WrapAngle(currentRotation - oldRotation);
            return oldRotation + diff * amount;
        }
        
        /// <summary>
        /// Catmull-Rom spline interpolation for smooth curves through control points.
        /// Used for flowing trails and organic motion.
        /// </summary>
        public static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
        
        /// <summary>
        /// Smooths a trail of positions using Catmull-Rom interpolation.
        /// Makes jagged trails appear smooth and flowing.
        /// </summary>
        public static Vector2[] SmoothTrail(Vector2[] positions, int subdivisions = 2)
        {
            if (positions == null || positions.Length < 4)
                return positions;
                
            int newLength = (positions.Length - 3) * subdivisions + positions.Length;
            Vector2[] smoothed = new Vector2[newLength];
            int index = 0;
            
            for (int i = 0; i < positions.Length - 3; i++)
            {
                smoothed[index++] = positions[i + 1];
                
                for (int j = 1; j < subdivisions; j++)
                {
                    float t = (float)j / subdivisions;
                    smoothed[index++] = CatmullRom(
                        positions[i], positions[i + 1], 
                        positions[i + 2], positions[i + 3], t);
                }
            }
            
            // Add remaining points
            for (int i = positions.Length - 2; i < positions.Length; i++)
            {
                if (index < smoothed.Length)
                    smoothed[index++] = positions[i];
            }
            
            return smoothed;
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
