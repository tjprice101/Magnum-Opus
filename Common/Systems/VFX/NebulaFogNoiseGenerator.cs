using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// NEBULA FOG NOISE GENERATOR
    /// 
    /// Generates procedural noise textures for fog/nebula effects:
    /// - Perlin noise for smooth fog density
    /// - Voronoi noise for constellation star patterns
    /// - Fractal brownian motion for detailed cloud textures
    /// 
    /// These textures are sampled by NebulaFogShader.fx
    /// </summary>
    public class NebulaFogNoiseGenerator : ModSystem
    {
        // Generated noise textures
        public static Texture2D PerlinNoiseTexture { get; private set; }
        public static Texture2D VoronoiNoiseTexture { get; private set; }
        public static Texture2D FBMNoiseTexture { get; private set; }
        public static Texture2D WarpNoiseTexture { get; private set; }
        
        private const int NOISE_SIZE = 256;
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            // Generate all noise textures
            Main.QueueMainThreadAction(() =>
            {
                GeneratePerlinNoiseTexture();
                GenerateVoronoiNoiseTexture();
                GenerateFBMNoiseTexture();
                GenerateWarpNoiseTexture();
            });
        }
        
        public override void Unload()
        {
            // Cache references before nulling - texture disposal must happen on main thread
            var perlin = PerlinNoiseTexture;
            var voronoi = VoronoiNoiseTexture;
            var fbm = FBMNoiseTexture;
            var warp = WarpNoiseTexture;
            
            // Null out references immediately (safe on any thread)
            PerlinNoiseTexture = null;
            VoronoiNoiseTexture = null;
            FBMNoiseTexture = null;
            WarpNoiseTexture = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            // Note: If the graphics device is already disposed, disposal will be skipped
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    perlin?.Dispose();
                    voronoi?.Dispose();
                    fbm?.Dispose();
                    warp?.Dispose();
                }
                catch
                {
                    // Silently ignore disposal errors during shutdown
                }
            });
        }
        
        #region Perlin Noise
        
        private static void GeneratePerlinNoiseTexture()
        {
            Color[] pixels = new Color[NOISE_SIZE * NOISE_SIZE];
            
            // Generate gradient vectors for Perlin noise
            int gradientGridSize = 16;
            Vector2[,] gradients = new Vector2[gradientGridSize + 1, gradientGridSize + 1];
            Random rand = new Random(42);
            
            for (int gx = 0; gx <= gradientGridSize; gx++)
            {
                for (int gy = 0; gy <= gradientGridSize; gy++)
                {
                    float angle = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    gradients[gx, gy] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                }
            }
            
            for (int y = 0; y < NOISE_SIZE; y++)
            {
                for (int x = 0; x < NOISE_SIZE; x++)
                {
                    // Multi-octave Perlin noise
                    float noiseValue = 0f;
                    float amplitude = 1f;
                    float frequency = 1f;
                    float maxValue = 0f;
                    
                    for (int octave = 0; octave < 4; octave++)
                    {
                        float sampleX = x / (float)NOISE_SIZE * gradientGridSize * frequency;
                        float sampleY = y / (float)NOISE_SIZE * gradientGridSize * frequency;
                        
                        float octaveValue = SamplePerlinNoise(sampleX, sampleY, gradients, gradientGridSize);
                        noiseValue += octaveValue * amplitude;
                        maxValue += amplitude;
                        
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }
                    
                    noiseValue /= maxValue;
                    noiseValue = (noiseValue + 1f) * 0.5f; // Normalize to 0-1
                    
                    byte value = (byte)(noiseValue * 255);
                    pixels[y * NOISE_SIZE + x] = new Color(value, value, value, 255);
                }
            }
            
            PerlinNoiseTexture = new Texture2D(Main.graphics.GraphicsDevice, NOISE_SIZE, NOISE_SIZE);
            PerlinNoiseTexture.SetData(pixels);
        }
        
        private static float SamplePerlinNoise(float x, float y, Vector2[,] gradients, int gridSize)
        {
            // Ensure positive indices (C# modulo can return negative for negative inputs)
            int x0 = ((int)Math.Floor(x) % gridSize + gridSize) % gridSize;
            int y0 = ((int)Math.Floor(y) % gridSize + gridSize) % gridSize;
            int x1 = (x0 + 1) % gridSize;
            int y1 = (y0 + 1) % gridSize;
            
            float fx = x - (float)Math.Floor(x);
            float fy = y - (float)Math.Floor(y);
            
            // Smoothstep
            float u = fx * fx * (3f - 2f * fx);
            float v = fy * fy * (3f - 2f * fy);
            
            // Dot products
            float n00 = Vector2.Dot(gradients[x0, y0], new Vector2(fx, fy));
            float n10 = Vector2.Dot(gradients[x1, y0], new Vector2(fx - 1f, fy));
            float n01 = Vector2.Dot(gradients[x0, y1], new Vector2(fx, fy - 1f));
            float n11 = Vector2.Dot(gradients[x1, y1], new Vector2(fx - 1f, fy - 1f));
            
            // Bilinear interpolation
            float nx0 = MathHelper.Lerp(n00, n10, u);
            float nx1 = MathHelper.Lerp(n01, n11, u);
            return MathHelper.Lerp(nx0, nx1, v);
        }
        
        #endregion
        
        #region Voronoi Noise
        
        private static void GenerateVoronoiNoiseTexture()
        {
            Color[] pixels = new Color[NOISE_SIZE * NOISE_SIZE];
            
            // Generate cell points
            int cellCount = 32;
            Vector2[] cellPoints = new Vector2[cellCount * cellCount];
            Random rand = new Random(123);
            
            for (int cy = 0; cy < cellCount; cy++)
            {
                for (int cx = 0; cx < cellCount; cx++)
                {
                    float baseX = cx / (float)cellCount;
                    float baseY = cy / (float)cellCount;
                    float jitterX = (float)rand.NextDouble() * 0.8f / cellCount;
                    float jitterY = (float)rand.NextDouble() * 0.8f / cellCount;
                    cellPoints[cy * cellCount + cx] = new Vector2(baseX + jitterX, baseY + jitterY);
                }
            }
            
            for (int y = 0; y < NOISE_SIZE; y++)
            {
                for (int x = 0; x < NOISE_SIZE; x++)
                {
                    float px = x / (float)NOISE_SIZE;
                    float py = y / (float)NOISE_SIZE;
                    
                    // Find distances to two nearest cells
                    float minDist1 = float.MaxValue;
                    float minDist2 = float.MaxValue;
                    
                    // Check nearby cells (3x3 neighborhood for wrapping)
                    int cellX = (int)(px * cellCount);
                    int cellY = (int)(py * cellCount);
                    
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int checkCellX = (cellX + dx + cellCount) % cellCount;
                            int checkCellY = (cellY + dy + cellCount) % cellCount;
                            
                            Vector2 cellPoint = cellPoints[checkCellY * cellCount + checkCellX];
                            
                            // Handle wrapping
                            cellPoint.X += dx < 0 && cellX == 0 ? -1f : (dx > 0 && cellX == cellCount - 1 ? 1f : 0f);
                            cellPoint.Y += dy < 0 && cellY == 0 ? -1f : (dy > 0 && cellY == cellCount - 1 ? 1f : 0f);
                            
                            float dist = Vector2.Distance(new Vector2(px, py), cellPoint);
                            
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
                    
                    // F1 (distance to nearest) in R channel
                    // F2-F1 (edge detection) in G channel
                    // F2 in B channel
                    float f1 = minDist1 * cellCount;
                    float f2f1 = (minDist2 - minDist1) * cellCount * 2f;
                    float f2 = minDist2 * cellCount;
                    
                    byte r = (byte)(MathHelper.Clamp(f1, 0f, 1f) * 255);
                    byte g = (byte)(MathHelper.Clamp(f2f1, 0f, 1f) * 255);
                    byte b = (byte)(MathHelper.Clamp(f2, 0f, 1f) * 255);
                    
                    pixels[y * NOISE_SIZE + x] = new Color(r, g, b, 255);
                }
            }
            
            VoronoiNoiseTexture = new Texture2D(Main.graphics.GraphicsDevice, NOISE_SIZE, NOISE_SIZE);
            VoronoiNoiseTexture.SetData(pixels);
        }
        
        #endregion
        
        #region FBM (Fractal Brownian Motion) Noise
        
        private static void GenerateFBMNoiseTexture()
        {
            Color[] pixels = new Color[NOISE_SIZE * NOISE_SIZE];
            
            // Generate gradient vectors
            int gradientGridSize = 8;
            Vector2[,] gradients = new Vector2[gradientGridSize + 1, gradientGridSize + 1];
            Random rand = new Random(456);
            
            for (int gx = 0; gx <= gradientGridSize; gx++)
            {
                for (int gy = 0; gy <= gradientGridSize; gy++)
                {
                    float angle = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    gradients[gx, gy] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                }
            }
            
            for (int y = 0; y < NOISE_SIZE; y++)
            {
                for (int x = 0; x < NOISE_SIZE; x++)
                {
                    // 6 octaves of FBM
                    float fbm = 0f;
                    float amplitude = 0.5f;
                    float frequency = 1f;
                    float maxValue = 0f;
                    
                    for (int octave = 0; octave < 6; octave++)
                    {
                        float sampleX = x / (float)NOISE_SIZE * gradientGridSize * frequency;
                        float sampleY = y / (float)NOISE_SIZE * gradientGridSize * frequency;
                        
                        float noise = SamplePerlinNoise(sampleX, sampleY, gradients, gradientGridSize);
                        fbm += noise * amplitude;
                        maxValue += amplitude;
                        
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }
                    
                    fbm /= maxValue;
                    fbm = (fbm + 1f) * 0.5f;
                    
                    // Create more varied texture by storing different values in RGB
                    float fbm2 = SamplePerlinNoise(
                        (x + 100) / (float)NOISE_SIZE * gradientGridSize,
                        (y + 100) / (float)NOISE_SIZE * gradientGridSize,
                        gradients, gradientGridSize
                    );
                    fbm2 = (fbm2 + 1f) * 0.5f;
                    
                    float fbm3 = SamplePerlinNoise(
                        (x + 200) / (float)NOISE_SIZE * gradientGridSize * 2f,
                        (y + 200) / (float)NOISE_SIZE * gradientGridSize * 2f,
                        gradients, gradientGridSize
                    );
                    fbm3 = (fbm3 + 1f) * 0.5f;
                    
                    byte r = (byte)(fbm * 255);
                    byte g = (byte)(fbm2 * 255);
                    byte b = (byte)(fbm3 * 255);
                    
                    pixels[y * NOISE_SIZE + x] = new Color(r, g, b, 255);
                }
            }
            
            FBMNoiseTexture = new Texture2D(Main.graphics.GraphicsDevice, NOISE_SIZE, NOISE_SIZE);
            FBMNoiseTexture.SetData(pixels);
        }
        
        #endregion
        
        #region Warp Noise
        
        private static void GenerateWarpNoiseTexture()
        {
            Color[] pixels = new Color[NOISE_SIZE * NOISE_SIZE];
            
            // Generate gradients
            int gradientGridSize = 8;
            Vector2[,] gradients = new Vector2[gradientGridSize + 1, gradientGridSize + 1];
            Random rand = new Random(789);
            
            for (int gx = 0; gx <= gradientGridSize; gx++)
            {
                for (int gy = 0; gy <= gradientGridSize; gy++)
                {
                    float angle = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    gradients[gx, gy] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                }
            }
            
            for (int y = 0; y < NOISE_SIZE; y++)
            {
                for (int x = 0; x < NOISE_SIZE; x++)
                {
                    float px = x / (float)NOISE_SIZE;
                    float py = y / (float)NOISE_SIZE;
                    
                    // Domain warping: sample noise to offset the coordinates
                    float warpX = SamplePerlinNoise(
                        px * gradientGridSize * 2f,
                        py * gradientGridSize * 2f,
                        gradients, gradientGridSize
                    );
                    float warpY = SamplePerlinNoise(
                        (px + 0.5f) * gradientGridSize * 2f,
                        (py + 0.5f) * gradientGridSize * 2f,
                        gradients, gradientGridSize
                    );
                    
                    // Apply warp
                    float warpedX = px + warpX * 0.2f;
                    float warpedY = py + warpY * 0.2f;
                    
                    // Sample with warped coordinates
                    float finalNoise = SamplePerlinNoise(
                        warpedX * gradientGridSize * 3f,
                        warpedY * gradientGridSize * 3f,
                        gradients, gradientGridSize
                    );
                    finalNoise = (finalNoise + 1f) * 0.5f;
                    
                    // Store warp vectors in RG, final noise in B
                    warpX = (warpX + 1f) * 0.5f;
                    warpY = (warpY + 1f) * 0.5f;
                    
                    byte r = (byte)(warpX * 255);
                    byte g = (byte)(warpY * 255);
                    byte b = (byte)(finalNoise * 255);
                    
                    pixels[y * NOISE_SIZE + x] = new Color(r, g, b, 255);
                }
            }
            
            WarpNoiseTexture = new Texture2D(Main.graphics.GraphicsDevice, NOISE_SIZE, NOISE_SIZE);
            WarpNoiseTexture.SetData(pixels);
        }
        
        #endregion
        
        #region Public Utility Methods
        
        /// <summary>
        /// Samples Perlin noise value at given UV coordinates.
        /// </summary>
        public static float SamplePerlin(float u, float v)
        {
            if (PerlinNoiseTexture == null)
                return 0.5f;
            
            int x = (int)(u * NOISE_SIZE) % NOISE_SIZE;
            int y = (int)(v * NOISE_SIZE) % NOISE_SIZE;
            if (x < 0) x += NOISE_SIZE;
            if (y < 0) y += NOISE_SIZE;
            
            Color[] data = new Color[1];
            PerlinNoiseTexture.GetData(0, new Rectangle(x, y, 1, 1), data, 0, 1);
            
            return data[0].R / 255f;
        }
        
        /// <summary>
        /// Samples Voronoi noise at given UV coordinates.
        /// Returns (F1, F2-F1, F2) in RGB.
        /// </summary>
        public static Vector3 SampleVoronoi(float u, float v)
        {
            if (VoronoiNoiseTexture == null)
                return Vector3.Zero;
            
            int x = (int)(u * NOISE_SIZE) % NOISE_SIZE;
            int y = (int)(v * NOISE_SIZE) % NOISE_SIZE;
            if (x < 0) x += NOISE_SIZE;
            if (y < 0) y += NOISE_SIZE;
            
            Color[] data = new Color[1];
            VoronoiNoiseTexture.GetData(0, new Rectangle(x, y, 1, 1), data, 0, 1);
            
            return new Vector3(data[0].R / 255f, data[0].G / 255f, data[0].B / 255f);
        }
        
        /// <summary>
        /// Generates a small noise texture for a specific effect (e.g., per-entity unique texture).
        /// </summary>
        public static Texture2D GenerateSmallNoise(int size, int seed)
        {
            Color[] pixels = new Color[size * size];
            Random rand = new Random(seed);
            
            // Simple value noise
            float[,] values = new float[size + 1, size + 1];
            for (int y = 0; y <= size; y++)
            {
                for (int x = 0; x <= size; x++)
                {
                    values[x % size, y % size] = (float)rand.NextDouble();
                }
            }
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Bilinear interpolation for smoothness
                    int x0 = x;
                    int y0 = y;
                    int x1 = (x + 1) % size;
                    int y1 = (y + 1) % size;
                    
                    float fx = 0.5f;
                    float fy = 0.5f;
                    
                    float v00 = values[x0, y0];
                    float v10 = values[x1, y0];
                    float v01 = values[x0, y1];
                    float v11 = values[x1, y1];
                    
                    float v0 = MathHelper.Lerp(v00, v10, fx);
                    float v1 = MathHelper.Lerp(v01, v11, fx);
                    float noise = MathHelper.Lerp(v0, v1, fy);
                    
                    byte value = (byte)(noise * 255);
                    pixels[y * size + x] = new Color(value, value, value, 255);
                }
            }
            
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            tex.SetData(pixels);
            return tex;
        }
        
        #endregion
    }
}
