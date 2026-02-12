using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Common.Systems.VFX.Core
{
    /// <summary>
    /// Advanced noise generation utilities for VFX.
    /// Provides GPU-style procedural noise functions optimized for real-time graphics.
    /// 
    /// Includes:
    /// - Perlin noise (smooth gradient noise)
    /// - Simplex noise (faster, fewer artifacts)
    /// - Cellular/Worley noise (organic cell patterns)
    /// - Value noise (simple interpolated random)
    /// - fBM (fractal Brownian motion) for layered detail
    /// - Domain warping for organic distortion
    /// - Turbulence for fire/smoke effects
    /// </summary>
    public static class NoiseUtilities
    {
        #region Hash Functions
        
        // Permutation table for Perlin noise (shuffled 0-255)
        private static readonly int[] Permutation = new int[512];
        
        // Gradient vectors for 2D Perlin noise
        private static readonly Vector2[] Gradients2D = new Vector2[8]
        {
            new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1),
            new Vector2(0.707f, 0.707f), new Vector2(-0.707f, 0.707f),
            new Vector2(0.707f, -0.707f), new Vector2(-0.707f, -0.707f)
        };
        
        static NoiseUtilities()
        {
            // Initialize permutation table
            var random = new Random(42); // Fixed seed for reproducibility
            int[] p = new int[256];
            for (int i = 0; i < 256; i++) p[i] = i;
            
            // Fisher-Yates shuffle
            for (int i = 255; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }
            
            // Duplicate for overflow protection
            for (int i = 0; i < 256; i++)
            {
                Permutation[i] = p[i];
                Permutation[i + 256] = p[i];
            }
        }
        
        /// <summary>
        /// Fast hash function for coordinates (GPU-style).
        /// </summary>
        private static float Hash(float x, float y)
        {
            // Sine-based hash (fast, good distribution)
            float h = (float)Math.Sin(x * 12.9898f + y * 78.233f) * 43758.5453f;
            return h - (float)Math.Floor(h);
        }
        
        /// <summary>
        /// Hash function returning a Vector2.
        /// </summary>
        private static Vector2 Hash2D(float x, float y)
        {
            float a = Hash(x, y);
            float b = Hash(x + 1.7f, y + 3.2f);
            return new Vector2(a, b);
        }
        
        #endregion
        
        #region Interpolation
        
        /// <summary>
        /// Quintic interpolation (smoother than cubic for noise).
        /// </summary>
        private static float Fade(float t)
        {
            // 6t^5 - 15t^4 + 10t^3
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }
        
        /// <summary>
        /// Linear interpolation.
        /// </summary>
        private static float Lerp(float a, float b, float t) => a + t * (b - a);
        
        #endregion
        
        #region Perlin Noise
        
        /// <summary>
        /// Classic 2D Perlin noise.
        /// Returns value in range [-1, 1].
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public static float Perlin(float x, float y)
        {
            // Grid cell coordinates
            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;
            
            // Relative position within cell
            float xf = x - (float)Math.Floor(x);
            float yf = y - (float)Math.Floor(y);
            
            // Fade curves for interpolation
            float u = Fade(xf);
            float v = Fade(yf);
            
            // Hash corner coordinates
            int aa = Permutation[Permutation[xi] + yi];
            int ab = Permutation[Permutation[xi] + yi + 1];
            int ba = Permutation[Permutation[xi + 1] + yi];
            int bb = Permutation[Permutation[xi + 1] + yi + 1];
            
            // Gradient vectors
            Vector2 g00 = Gradients2D[aa & 7];
            Vector2 g10 = Gradients2D[ba & 7];
            Vector2 g01 = Gradients2D[ab & 7];
            Vector2 g11 = Gradients2D[bb & 7];
            
            // Distance vectors
            Vector2 d00 = new Vector2(xf, yf);
            Vector2 d10 = new Vector2(xf - 1f, yf);
            Vector2 d01 = new Vector2(xf, yf - 1f);
            Vector2 d11 = new Vector2(xf - 1f, yf - 1f);
            
            // Dot products
            float n00 = Vector2.Dot(g00, d00);
            float n10 = Vector2.Dot(g10, d10);
            float n01 = Vector2.Dot(g01, d01);
            float n11 = Vector2.Dot(g11, d11);
            
            // Bilinear interpolation
            float nx0 = Lerp(n00, n10, u);
            float nx1 = Lerp(n01, n11, u);
            return Lerp(nx0, nx1, v);
        }
        
        /// <summary>
        /// Perlin noise returning value in [0, 1] range.
        /// </summary>
        public static float Perlin01(float x, float y)
        {
            return Perlin(x, y) * 0.5f + 0.5f;
        }
        
        /// <summary>
        /// Perlin noise with Vector2 input.
        /// </summary>
        public static float Perlin(Vector2 pos) => Perlin(pos.X, pos.Y);
        public static float Perlin01(Vector2 pos) => Perlin01(pos.X, pos.Y);
        
        #endregion
        
        #region Simplex Noise
        
        // Simplex noise constants
        private const float F2 = 0.3660254f;  // (sqrt(3) - 1) / 2
        private const float G2 = 0.2113249f;  // (3 - sqrt(3)) / 6
        
        /// <summary>
        /// 2D Simplex noise (faster than Perlin, better gradients).
        /// Returns value in range [-1, 1].
        /// </summary>
        public static float Simplex(float x, float y)
        {
            // Skew to simplex space
            float s = (x + y) * F2;
            int i = (int)Math.Floor(x + s);
            int j = (int)Math.Floor(y + s);
            
            // Unskew back
            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;
            float x0 = x - X0;
            float y0 = y - Y0;
            
            // Determine simplex (triangle) we're in
            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }  // Lower triangle
            else { i1 = 0; j1 = 1; }           // Upper triangle
            
            // Offsets for corners 1 and 2
            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1f + 2f * G2;
            float y2 = y0 - 1f + 2f * G2;
            
            // Hash corners
            int ii = i & 255;
            int jj = j & 255;
            int gi0 = Permutation[ii + Permutation[jj]] & 7;
            int gi1 = Permutation[ii + i1 + Permutation[jj + j1]] & 7;
            int gi2 = Permutation[ii + 1 + Permutation[jj + 1]] & 7;
            
            // Calculate contributions
            float n0 = 0, n1 = 0, n2 = 0;
            
            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 >= 0)
            {
                t0 *= t0;
                n0 = t0 * t0 * Vector2.Dot(Gradients2D[gi0], new Vector2(x0, y0));
            }
            
            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 >= 0)
            {
                t1 *= t1;
                n1 = t1 * t1 * Vector2.Dot(Gradients2D[gi1], new Vector2(x1, y1));
            }
            
            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 >= 0)
            {
                t2 *= t2;
                n2 = t2 * t2 * Vector2.Dot(Gradients2D[gi2], new Vector2(x2, y2));
            }
            
            // Sum and scale to [-1, 1]
            return 70f * (n0 + n1 + n2);
        }
        
        /// <summary>
        /// Simplex noise returning value in [0, 1] range.
        /// </summary>
        public static float Simplex01(float x, float y)
        {
            return Simplex(x, y) * 0.5f + 0.5f;
        }
        
        public static float Simplex(Vector2 pos) => Simplex(pos.X, pos.Y);
        public static float Simplex01(Vector2 pos) => Simplex01(pos.X, pos.Y);
        
        #endregion
        
        #region Value Noise
        
        /// <summary>
        /// Simple value noise (interpolated random values).
        /// Faster but less smooth than gradient noise.
        /// Returns value in range [0, 1].
        /// </summary>
        public static float ValueNoise(float x, float y)
        {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            
            // Smooth interpolation
            float u = Fade(xf);
            float v = Fade(yf);
            
            // Hash corners
            float n00 = Hash(xi, yi);
            float n10 = Hash(xi + 1, yi);
            float n01 = Hash(xi, yi + 1);
            float n11 = Hash(xi + 1, yi + 1);
            
            // Bilinear interpolation
            float nx0 = Lerp(n00, n10, u);
            float nx1 = Lerp(n01, n11, u);
            return Lerp(nx0, nx1, v);
        }
        
        public static float ValueNoise(Vector2 pos) => ValueNoise(pos.X, pos.Y);
        
        #endregion
        
        #region Cellular (Worley) Noise
        
        /// <summary>
        /// Cellular/Worley noise - returns distance to nearest feature point.
        /// Creates organic cell patterns (voronoi-like).
        /// Returns value in range [0, 1].
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="jitter">How much to randomize cell positions (0-1)</param>
        public static float Cellular(float x, float y, float jitter = 1f)
        {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            
            float minDist = float.MaxValue;
            
            // Check 3x3 neighborhood
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    // Feature point position (hashed)
                    Vector2 h = Hash2D(xi + i, yi + j);
                    float fx = i + h.X * jitter - xf;
                    float fy = j + h.Y * jitter - yf;
                    
                    // Distance to this feature point
                    float dist = fx * fx + fy * fy;
                    minDist = Math.Min(minDist, dist);
                }
            }
            
            return (float)Math.Sqrt(minDist);
        }
        
        /// <summary>
        /// Cellular noise returning F2 - F1 (distance between two closest points).
        /// Creates cell edge patterns.
        /// </summary>
        public static float CellularEdge(float x, float y, float jitter = 1f)
        {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            
            float minDist1 = float.MaxValue;
            float minDist2 = float.MaxValue;
            
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 h = Hash2D(xi + i, yi + j);
                    float fx = i + h.X * jitter - xf;
                    float fy = j + h.Y * jitter - yf;
                    float dist = fx * fx + fy * fy;
                    
                    if (dist < minDist1)
                    {
                        minDist2 = minDist1;
                        minDist1 = dist;
                    }
                    else if (dist < minDist2)
                    {
                        minDist2 = dist;
                    }
                }
            }
            
            return (float)Math.Sqrt(minDist2) - (float)Math.Sqrt(minDist1);
        }
        
        public static float Cellular(Vector2 pos, float jitter = 1f) => Cellular(pos.X, pos.Y, jitter);
        public static float CellularEdge(Vector2 pos, float jitter = 1f) => CellularEdge(pos.X, pos.Y, jitter);
        
        #endregion
        
        #region Fractal Brownian Motion (fBM)
        
        /// <summary>
        /// Fractal Brownian Motion - layered noise for natural detail.
        /// Each octave adds higher frequency, lower amplitude detail.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="octaves">Number of layers (1-8 recommended)</param>
        /// <param name="lacunarity">Frequency multiplier per octave (typically 2.0)</param>
        /// <param name="gain">Amplitude multiplier per octave (typically 0.5)</param>
        /// <param name="noiseFunc">Base noise function to use</param>
        public static float fBM(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f, 
            Func<float, float, float> noiseFunc = null)
        {
            noiseFunc ??= Perlin;
            
            float amplitude = 1f;
            float frequency = 1f;
            float sum = 0f;
            float maxValue = 0f;
            
            for (int i = 0; i < octaves; i++)
            {
                sum += amplitude * noiseFunc(x * frequency, y * frequency);
                maxValue += amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }
            
            return sum / maxValue;
        }
        
        /// <summary>
        /// fBM with Perlin noise base.
        /// </summary>
        public static float fBMPerlin(float x, float y, int octaves = 4)
            => fBM(x, y, octaves, noiseFunc: Perlin);
        
        /// <summary>
        /// fBM with Simplex noise base.
        /// </summary>
        public static float fBMSimplex(float x, float y, int octaves = 4)
            => fBM(x, y, octaves, noiseFunc: Simplex);
        
        /// <summary>
        /// fBM with Vector2 input.
        /// </summary>
        public static float fBM(Vector2 pos, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
            => fBM(pos.X, pos.Y, octaves, lacunarity, gain);
        
        #endregion
        
        #region Turbulence
        
        /// <summary>
        /// Turbulence - absolute value of fBM for fire/smoke effects.
        /// Creates billowing, turbulent patterns.
        /// </summary>
        public static float Turbulence(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float sum = 0f;
            float maxValue = 0f;
            
            for (int i = 0; i < octaves; i++)
            {
                sum += amplitude * Math.Abs(Perlin(x * frequency, y * frequency));
                maxValue += amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }
            
            return sum / maxValue;
        }
        
        public static float Turbulence(Vector2 pos, int octaves = 4) 
            => Turbulence(pos.X, pos.Y, octaves);
        
        #endregion
        
        #region Domain Warping
        
        /// <summary>
        /// Domain warping - uses noise to distort the input coordinates.
        /// Creates organic, flowing distortion patterns.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="strength">Warp strength multiplier</param>
        /// <param name="scale">Coordinate scale</param>
        public static float DomainWarp(float x, float y, float strength = 4f, float scale = 1f)
        {
            // First layer of noise for distortion
            float qx = fBM(x * scale, y * scale, 4);
            float qy = fBM(x * scale + 5.2f, y * scale + 1.3f, 4);
            
            // Apply distortion and sample again
            return fBM(x + qx * strength, y + qy * strength, 4);
        }
        
        /// <summary>
        /// Multi-layer domain warping for complex organic patterns.
        /// </summary>
        public static float DomainWarpAdvanced(float x, float y, float strength = 4f, float scale = 1f)
        {
            // First warp layer
            float qx = fBM(x * scale, y * scale, 4);
            float qy = fBM(x * scale + 5.2f, y * scale + 1.3f, 4);
            
            // Second warp layer using first result
            float rx = fBM(x + qx * strength, y + qy * strength + 1.7f, 4);
            float ry = fBM(x + qx * strength + 8.3f, y + qy * strength + 2.8f, 4);
            
            // Final sample
            return fBM(x + rx * strength, y + ry * strength, 4);
        }
        
        public static float DomainWarp(Vector2 pos, float strength = 4f, float scale = 1f)
            => DomainWarp(pos.X, pos.Y, strength, scale);
        
        #endregion
        
        #region Ridged Noise
        
        /// <summary>
        /// Ridged multifractal noise - inverts and sharpens peaks.
        /// Great for mountain ridges, veins, lightning.
        /// </summary>
        public static float Ridged(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
        {
            float amplitude = 1f;
            float frequency = 1f;
            float sum = 0f;
            float prev = 1f;
            
            for (int i = 0; i < octaves; i++)
            {
                float n = 1f - Math.Abs(Perlin(x * frequency, y * frequency));
                n = n * n; // Square for sharper ridges
                
                // Weight by previous octave
                sum += n * amplitude * prev;
                prev = n;
                
                amplitude *= gain;
                frequency *= lacunarity;
            }
            
            return sum;
        }
        
        public static float Ridged(Vector2 pos, int octaves = 4)
            => Ridged(pos.X, pos.Y, octaves);
        
        #endregion
        
        #region Utility Functions
        
        /// <summary>
        /// Generates animated noise value based on time.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="time">Time value (Main.GlobalTimeWrappedHourly)</param>
        /// <param name="speed">Animation speed multiplier</param>
        public static float AnimatedNoise(float x, float y, float time, float speed = 1f)
        {
            // Offset coordinates based on time
            float offsetX = Perlin(time * speed * 0.1f, 0f) * 10f;
            float offsetY = Perlin(0f, time * speed * 0.1f + 5f) * 10f;
            return Perlin(x + offsetX, y + offsetY);
        }
        
        /// <summary>
        /// Seamless tiling noise for textures.
        /// </summary>
        /// <param name="x">X coordinate (0-1 for one tile)</param>
        /// <param name="y">Y coordinate (0-1 for one tile)</param>
        /// <param name="scale">Noise scale within tile</param>
        public static float TilingNoise(float x, float y, float scale = 4f)
        {
            // Use 4D noise sampling a torus for seamless tiling
            float nx = (float)Math.Cos(x * MathHelper.TwoPi) * scale;
            float ny = (float)Math.Sin(x * MathHelper.TwoPi) * scale;
            float nz = (float)Math.Cos(y * MathHelper.TwoPi) * scale;
            float nw = (float)Math.Sin(y * MathHelper.TwoPi) * scale;
            
            // Use 2D noise with combined coordinates
            return Perlin(nx + nz, ny + nw);
        }
        
        /// <summary>
        /// Radial noise - useful for explosions, ripples.
        /// </summary>
        public static float RadialNoise(float x, float y, float centerX, float centerY, float angularScale = 8f, float radialScale = 2f)
        {
            float dx = x - centerX;
            float dy = y - centerY;
            float angle = (float)Math.Atan2(dy, dx);
            float radius = (float)Math.Sqrt(dx * dx + dy * dy);
            
            return Perlin(angle * angularScale / MathHelper.TwoPi, radius * radialScale);
        }
        
        public static float RadialNoise(Vector2 pos, Vector2 center, float angularScale = 8f, float radialScale = 2f)
            => RadialNoise(pos.X, pos.Y, center.X, center.Y, angularScale, radialScale);
        
        #endregion
    }
}
