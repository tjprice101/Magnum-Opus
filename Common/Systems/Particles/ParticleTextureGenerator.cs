using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// Generates particle textures procedurally at runtime.
    /// This eliminates the need for external texture files for basic particle shapes.
    /// Uses lazy initialization to ensure textures are created on the main thread.
    /// </summary>
    public class ParticleTextureGenerator : ModSystem
    {
        // Generated textures - access these via properties for lazy initialization
        private static Texture2D _bloomCircle;
        private static Texture2D _bloomRing;
        private static Texture2D _sparkle;
        private static Texture2D _glowSpark;
        private static Texture2D _softGlow;
        private static Texture2D _point;
        private static Texture2D _heavySmoke;
        private static Texture2D _softLine;
        private static Texture2D _hardCircle;
        private static Texture2D _star4Point;
        private static Texture2D _star6Point;
        
        // Musical textures
        private static Texture2D _musicNoteQuarter;
        private static Texture2D _musicNoteEighth;
        private static Texture2D _musicNoteSixteenth;
        private static Texture2D _musicNoteDouble;
        private static Texture2D _trebleClef;
        private static Texture2D _bassClef;
        private static Texture2D _musicStaff;
        private static Texture2D _musicSharp;
        private static Texture2D _musicFlat;
        
        // Lazy-initialized properties - textures are created on first access (which happens on main thread during draw)
        public static Texture2D BloomCircle => _bloomCircle ??= GenerateBloomCircle(64);
        public static Texture2D BloomRing => _bloomRing ??= GenerateBloomRing(64, 0.3f, 0.5f);
        public static Texture2D Sparkle => _sparkle ??= GenerateSparkle(32, 4);
        public static Texture2D GlowSpark => _glowSpark ??= GenerateGlowSpark(48, 64);
        public static Texture2D SoftGlow => _softGlow ??= GenerateBloomCircle(32);
        public static Texture2D Point => _point ??= GenerateBloomCircle(8);
        public static Texture2D HeavySmoke => _heavySmoke ??= GenerateHeavySmoke(64, 7);
        public static Texture2D SoftLine => _softLine ??= GenerateSoftLine(4, 64);
        public static Texture2D HardCircle => _hardCircle ??= GenerateHardCircle(32);
        public static Texture2D Star4Point => _star4Point ??= GenerateSparkle(32, 4);
        public static Texture2D Star6Point => _star6Point ??= GenerateSparkle(32, 6);
        
        // Musical texture properties
        public static Texture2D MusicNoteQuarter => _musicNoteQuarter ??= GenerateMusicNoteQuarter(32);
        public static Texture2D MusicNoteEighth => _musicNoteEighth ??= GenerateMusicNoteEighth(32);
        public static Texture2D MusicNoteSixteenth => _musicNoteSixteenth ??= GenerateMusicNoteSixteenth(32);
        public static Texture2D MusicNoteDouble => _musicNoteDouble ??= GenerateMusicNoteDouble(40);
        public static Texture2D TrebleClef => _trebleClef ??= GenerateTrebleClef(32, 48);
        public static Texture2D BassClef => _bassClef ??= GenerateBassClef(32, 32);
        public static Texture2D MusicStaff => _musicStaff ??= GenerateMusicStaff(64, 32);
        public static Texture2D MusicSharp => _musicSharp ??= GenerateMusicSharp(24);
        public static Texture2D MusicFlat => _musicFlat ??= GenerateMusicFlat(20, 28);

        public override void Load()
        {
            // Textures are now lazy-initialized on first use (during draw, which is on main thread)
            // No initialization needed here
        }

        public override void Unload()
        {
            // Dispose of generated textures
            _bloomCircle?.Dispose();
            _bloomRing?.Dispose();
            _sparkle?.Dispose();
            _glowSpark?.Dispose();
            _softGlow?.Dispose();
            _point?.Dispose();
            _heavySmoke?.Dispose();
            _softLine?.Dispose();
            _hardCircle?.Dispose();
            _star4Point?.Dispose();
            _star6Point?.Dispose();
            _musicNoteQuarter?.Dispose();
            _musicNoteEighth?.Dispose();
            _musicNoteSixteenth?.Dispose();
            _musicNoteDouble?.Dispose();
            _trebleClef?.Dispose();
            _bassClef?.Dispose();
            _musicStaff?.Dispose();
            _musicSharp?.Dispose();
            _musicFlat?.Dispose();
            
            _bloomCircle = null;
            _bloomRing = null;
            _sparkle = null;
            _glowSpark = null;
            _softGlow = null;
            _point = null;
            _heavySmoke = null;
            _softLine = null;
            _hardCircle = null;
            _star4Point = null;
            _star6Point = null;
            _musicNoteQuarter = null;
            _musicNoteEighth = null;
            _musicNoteSixteenth = null;
            _musicNoteDouble = null;
            _trebleClef = null;
            _bassClef = null;
            _musicStaff = null;
            _musicSharp = null;
            _musicFlat = null;
        }

        /// <summary>
        /// Force generation of all textures. Call this from the main thread if you need
        /// all textures pre-loaded (e.g., in PostSetupContent or first frame).
        /// </summary>
        public static void EnsureTexturesGenerated()
        {
            if (Main.dedServ) return;
            
            // Access each property to trigger lazy initialization
            _ = BloomCircle;
            _ = BloomRing;
            _ = Sparkle;
            _ = GlowSpark;
            _ = SoftGlow;
            _ = Point;
            _ = HeavySmoke;
            _ = SoftLine;
            _ = HardCircle;
            _ = Star4Point;
            _ = Star6Point;
            _ = MusicNoteQuarter;
            _ = MusicNoteEighth;
            _ = MusicNoteSixteenth;
            _ = MusicNoteDouble;
            _ = TrebleClef;
            _ = BassClef;
            _ = MusicStaff;
            _ = MusicSharp;
            _ = MusicFlat;
        }

        /// <summary>
        /// Generates a soft circular bloom/glow texture.
        /// White in center, fading to transparent at edges.
        /// </summary>
        public static Texture2D GenerateBloomCircle(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float maxDist = center;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    // Smooth falloff using squared cosine for softer edges
                    float normalizedDist = Math.Min(dist / maxDist, 1f);
                    float alpha = 1f - normalizedDist;
                    alpha = alpha * alpha; // Quadratic falloff for softer glow
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    
                    // Premultiplied alpha for proper blending
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Generates a ring/donut shaped texture for shockwaves.
        /// </summary>
        public static Texture2D GenerateBloomRing(int size, float innerRadius, float outerRadius)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float innerDist = center * innerRadius;
            float outerDist = center * outerRadius;
            float ringWidth = outerDist - innerDist;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    float alpha = 0f;
                    
                    if (dist >= innerDist && dist <= center)
                    {
                        // Calculate distance from ring center
                        float ringCenter = (innerDist + outerDist) / 2f;
                        float distFromRingCenter = Math.Abs(dist - ringCenter);
                        float halfRingWidth = ringWidth / 2f;
                        
                        if (distFromRingCenter < halfRingWidth)
                        {
                            alpha = 1f - (distFromRingCenter / halfRingWidth);
                            alpha = alpha * alpha; // Smooth falloff
                        }
                        
                        // Fade out at outer edge
                        if (dist > outerDist)
                        {
                            float outerFade = 1f - ((dist - outerDist) / (center - outerDist));
                            alpha *= Math.Max(0f, outerFade * outerFade);
                        }
                    }
                    
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Generates a star/sparkle texture with the specified number of points.
        /// </summary>
        public static Texture2D GenerateSparkle(int size, int points)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float maxRadius = center * 0.9f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    float angle = (float)Math.Atan2(dy, dx);
                    
                    // Create star shape using sine wave
                    float starShape = (float)Math.Cos(angle * points) * 0.5f + 0.5f;
                    float starRadius = MathHelper.Lerp(maxRadius * 0.2f, maxRadius, starShape);
                    
                    float alpha = 0f;
                    if (dist < starRadius)
                    {
                        alpha = 1f - (dist / starRadius);
                        alpha = alpha * alpha; // Soft falloff
                        
                        // Boost center brightness
                        if (dist < maxRadius * 0.15f)
                        {
                            alpha = MathHelper.Lerp(alpha, 1f, 1f - (dist / (maxRadius * 0.15f)));
                        }
                    }
                    
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Generates an elongated spark/flame texture.
        /// </summary>
        public static Texture2D GenerateGlowSpark(int width, int height)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            float centerX = width / 2f;
            float centerY = height / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - centerX + 0.5f) / centerX;
                    float dy = (y - centerY + 0.5f) / centerY;
                    
                    // Elongated ellipse shape - wider at center, tapered at ends
                    float verticalPos = Math.Abs(dy);
                    float horizontalWidth = 1f - (verticalPos * verticalPos); // Tapered width
                    horizontalWidth = Math.Max(horizontalWidth, 0.1f);
                    
                    float normalizedX = Math.Abs(dx) / horizontalWidth;
                    float normalizedY = Math.Abs(dy);
                    
                    float alpha = 0f;
                    if (normalizedX <= 1f && normalizedY <= 1f)
                    {
                        // Combine horizontal and vertical falloff
                        float xAlpha = 1f - normalizedX;
                        float yAlpha = 1f - normalizedY;
                        alpha = xAlpha * yAlpha;
                        alpha = alpha * alpha; // Smooth falloff
                        
                        // Boost the core
                        if (normalizedX < 0.3f && normalizedY < 0.5f)
                        {
                            float coreBoost = (1f - normalizedX / 0.3f) * (1f - normalizedY / 0.5f);
                            alpha = MathHelper.Lerp(alpha, 1f, coreBoost * 0.5f);
                        }
                    }
                    
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    byte a = (byte)(alpha * 255);
                    data[y * width + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Generates a soft line texture for trails.
        /// </summary>
        public static Texture2D GenerateSoftLine(int width, int length)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, length, width);
            Color[] data = new Color[length * width];
            
            float centerY = width / 2f;
            
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    float dy = Math.Abs(y - centerY + 0.5f) / centerY;
                    float dx = (float)x / length;
                    
                    // Fade along length and across width
                    float lengthFade = 1f - dx;
                    float widthFade = 1f - dy;
                    
                    float alpha = lengthFade * widthFade * widthFade;
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    
                    byte a = (byte)(alpha * 255);
                    data[y * length + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Generates a hard-edged circle texture.
        /// </summary>
        public static Texture2D GenerateHardCircle(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float radius = center - 1f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    float alpha = 0f;
                    if (dist < radius)
                    {
                        alpha = 1f;
                        // Slight anti-aliasing at edge
                        if (dist > radius - 1.5f)
                        {
                            alpha = (radius - dist) / 1.5f;
                        }
                    }
                    
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Generates a heavy smoke texture with multiple frame variants (vertical strip).
        /// </summary>
        public static Texture2D GenerateHeavySmoke(int frameSize, int frameCount)
        {
            int totalHeight = frameSize * frameCount;
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, frameSize, totalHeight);
            Color[] data = new Color[frameSize * totalHeight];
            
            Random rand = new Random(12345); // Seeded for consistency
            
            for (int frame = 0; frame < frameCount; frame++)
            {
                int frameOffset = frame * frameSize * frameSize;
                float center = frameSize / 2f;
                
                // Generate noise for this frame
                float[,] noise = GeneratePerlinNoise(frameSize, frameSize, 4, rand);
                
                for (int y = 0; y < frameSize; y++)
                {
                    for (int x = 0; x < frameSize; x++)
                    {
                        float dx = x - center + 0.5f;
                        float dy = y - center + 0.5f;
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                        
                        // Base circular shape
                        float normalizedDist = dist / center;
                        float baseAlpha = 1f - normalizedDist;
                        baseAlpha = MathHelper.Clamp(baseAlpha, 0f, 1f);
                        
                        // Apply noise for cloud-like appearance
                        float noiseValue = noise[x, y];
                        float alpha = baseAlpha * (0.5f + noiseValue * 0.5f);
                        
                        // Soften edges
                        if (normalizedDist > 0.6f)
                        {
                            float edgeFade = 1f - ((normalizedDist - 0.6f) / 0.4f);
                            alpha *= edgeFade * edgeFade;
                        }
                        
                        alpha = MathHelper.Clamp(alpha, 0f, 1f);
                        byte a = (byte)(alpha * 255);
                        data[frameOffset + y * frameSize + x] = new Color(a, a, a, a);
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// Simple Perlin-like noise generation for smoke textures.
        /// </summary>
        private static float[,] GeneratePerlinNoise(int width, int height, int octaves, Random rand)
        {
            float[,] result = new float[width, height];
            
            // Generate base noise
            float[,] baseNoise = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    baseNoise[x, y] = (float)rand.NextDouble();
                }
            }
            
            // Smooth and combine octaves
            float amplitude = 1f;
            float totalAmplitude = 0f;
            
            for (int octave = 0; octave < octaves; octave++)
            {
                int samplePeriod = 1 << (octaves - octave - 1);
                float sampleFrequency = 1f / samplePeriod;
                
                for (int y = 0; y < height; y++)
                {
                    int y0 = (y / samplePeriod) * samplePeriod;
                    int y1 = (y0 + samplePeriod) % height;
                    float yBlend = (y - y0) * sampleFrequency;
                    
                    for (int x = 0; x < width; x++)
                    {
                        int x0 = (x / samplePeriod) * samplePeriod;
                        int x1 = (x0 + samplePeriod) % width;
                        float xBlend = (x - x0) * sampleFrequency;
                        
                        // Bilinear interpolation
                        float top = MathHelper.Lerp(baseNoise[x0, y0], baseNoise[x1, y0], xBlend);
                        float bottom = MathHelper.Lerp(baseNoise[x0, y1], baseNoise[x1, y1], xBlend);
                        
                        result[x, y] += MathHelper.Lerp(top, bottom, yBlend) * amplitude;
                    }
                }
                
                totalAmplitude += amplitude;
                amplitude *= 0.5f;
            }
            
            // Normalize
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x, y] /= totalAmplitude;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets a generated texture, ensuring it exists.
        /// Falls back to creating a simple white pixel if needed.
        /// </summary>
        public static Texture2D GetTexture(string name)
        {
            return name switch
            {
                "BloomCircle" => BloomCircle,
                "BloomRing" => BloomRing,
                "Sparkle" => Sparkle,
                "GlowSpark" => GlowSpark,
                "SoftGlow" => SoftGlow,
                "Point" => Point,
                "HeavySmoke" => HeavySmoke,
                "SoftLine" => SoftLine,
                "HardCircle" => HardCircle,
                "Star4Point" => Star4Point,
                "Star6Point" => Star6Point,
                "MusicNoteQuarter" => MusicNoteQuarter,
                "MusicNoteEighth" => MusicNoteEighth,
                "MusicNoteSixteenth" => MusicNoteSixteenth,
                "MusicNoteDouble" => MusicNoteDouble,
                "TrebleClef" => TrebleClef,
                "BassClef" => BassClef,
                "MusicStaff" => MusicStaff,
                "MusicSharp" => MusicSharp,
                "MusicFlat" => MusicFlat,
                _ => null
            };
        }
        
        #region Musical Texture Generation
        
        /// <summary>
        /// Generates a quarter note (filled oval head with stem).
        /// </summary>
        public static Texture2D GenerateMusicNoteQuarter(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float noteHeadCenterX = size * 0.4f;
            float noteHeadCenterY = size * 0.7f;
            float noteHeadRadiusX = size * 0.22f;
            float noteHeadRadiusY = size * 0.16f;
            float noteHeadRotation = -0.3f; // Slight tilt
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 0f;
                    
                    // Note head (rotated ellipse)
                    float dx = x - noteHeadCenterX;
                    float dy = y - noteHeadCenterY;
                    float rotX = dx * (float)Math.Cos(noteHeadRotation) - dy * (float)Math.Sin(noteHeadRotation);
                    float rotY = dx * (float)Math.Sin(noteHeadRotation) + dy * (float)Math.Cos(noteHeadRotation);
                    float ellipseDist = (rotX * rotX) / (noteHeadRadiusX * noteHeadRadiusX) + 
                                       (rotY * rotY) / (noteHeadRadiusY * noteHeadRadiusY);
                    
                    if (ellipseDist < 1f)
                    {
                        alpha = 1f - ellipseDist * 0.3f;
                    }
                    
                    // Stem (vertical line from note head going up)
                    float stemX = noteHeadCenterX + noteHeadRadiusX * 0.8f;
                    float stemWidth = size * 0.08f;
                    float stemTop = size * 0.1f;
                    float stemBottom = noteHeadCenterY - noteHeadRadiusY * 0.5f;
                    
                    if (x >= stemX - stemWidth / 2 && x <= stemX + stemWidth / 2 &&
                        y >= stemTop && y <= stemBottom)
                    {
                        float distFromCenter = Math.Abs(x - stemX) / (stemWidth / 2);
                        alpha = Math.Max(alpha, 1f - distFromCenter * 0.5f);
                    }
                    
                    // Add soft glow around the note
                    if (alpha > 0)
                    {
                        alpha = MathHelper.Clamp(alpha * 1.1f, 0f, 1f);
                    }
                    
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates an eighth note (quarter note with flag).
        /// </summary>
        public static Texture2D GenerateMusicNoteEighth(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float noteHeadCenterX = size * 0.35f;
            float noteHeadCenterY = size * 0.72f;
            float noteHeadRadiusX = size * 0.2f;
            float noteHeadRadiusY = size * 0.14f;
            float noteHeadRotation = -0.3f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 0f;
                    
                    // Note head
                    float dx = x - noteHeadCenterX;
                    float dy = y - noteHeadCenterY;
                    float rotX = dx * (float)Math.Cos(noteHeadRotation) - dy * (float)Math.Sin(noteHeadRotation);
                    float rotY = dx * (float)Math.Sin(noteHeadRotation) + dy * (float)Math.Cos(noteHeadRotation);
                    float ellipseDist = (rotX * rotX) / (noteHeadRadiusX * noteHeadRadiusX) + 
                                       (rotY * rotY) / (noteHeadRadiusY * noteHeadRadiusY);
                    
                    if (ellipseDist < 1f)
                        alpha = 1f - ellipseDist * 0.3f;
                    
                    // Stem
                    float stemX = noteHeadCenterX + noteHeadRadiusX * 0.85f;
                    float stemWidth = size * 0.07f;
                    float stemTop = size * 0.12f;
                    float stemBottom = noteHeadCenterY - noteHeadRadiusY * 0.3f;
                    
                    if (x >= stemX - stemWidth / 2 && x <= stemX + stemWidth / 2 &&
                        y >= stemTop && y <= stemBottom)
                    {
                        alpha = Math.Max(alpha, 1f);
                    }
                    
                    // Flag (curved line from top of stem)
                    float flagStartY = stemTop;
                    float flagEndY = size * 0.45f;
                    if (y >= flagStartY && y <= flagEndY)
                    {
                        float progress = (y - flagStartY) / (flagEndY - flagStartY);
                        float curveX = stemX + (float)Math.Sin(progress * MathHelper.Pi * 0.8f) * size * 0.25f;
                        float flagWidth = size * 0.06f * (1f - progress * 0.5f);
                        
                        if (x >= curveX - flagWidth && x <= curveX + flagWidth)
                        {
                            float distFromCurve = Math.Abs(x - curveX) / flagWidth;
                            alpha = Math.Max(alpha, 1f - distFromCurve * 0.5f);
                        }
                    }
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a sixteenth note (eighth note with two flags).
        /// </summary>
        public static Texture2D GenerateMusicNoteSixteenth(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float noteHeadCenterX = size * 0.32f;
            float noteHeadCenterY = size * 0.75f;
            float noteHeadRadiusX = size * 0.18f;
            float noteHeadRadiusY = size * 0.13f;
            float noteHeadRotation = -0.3f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 0f;
                    
                    // Note head
                    float dx = x - noteHeadCenterX;
                    float dy = y - noteHeadCenterY;
                    float rotX = dx * (float)Math.Cos(noteHeadRotation) - dy * (float)Math.Sin(noteHeadRotation);
                    float rotY = dx * (float)Math.Sin(noteHeadRotation) + dy * (float)Math.Cos(noteHeadRotation);
                    float ellipseDist = (rotX * rotX) / (noteHeadRadiusX * noteHeadRadiusX) + 
                                       (rotY * rotY) / (noteHeadRadiusY * noteHeadRadiusY);
                    
                    if (ellipseDist < 1f)
                        alpha = 1f - ellipseDist * 0.3f;
                    
                    // Stem
                    float stemX = noteHeadCenterX + noteHeadRadiusX * 0.85f;
                    float stemWidth = size * 0.065f;
                    float stemTop = size * 0.1f;
                    float stemBottom = noteHeadCenterY - noteHeadRadiusY * 0.3f;
                    
                    if (x >= stemX - stemWidth / 2 && x <= stemX + stemWidth / 2 &&
                        y >= stemTop && y <= stemBottom)
                    {
                        alpha = Math.Max(alpha, 1f);
                    }
                    
                    // Two flags
                    for (int flag = 0; flag < 2; flag++)
                    {
                        float flagStartY = stemTop + flag * size * 0.12f;
                        float flagEndY = flagStartY + size * 0.28f;
                        if (y >= flagStartY && y <= flagEndY)
                        {
                            float progress = (y - flagStartY) / (flagEndY - flagStartY);
                            float curveX = stemX + (float)Math.Sin(progress * MathHelper.Pi * 0.7f) * size * 0.22f;
                            float flagWidth = size * 0.055f * (1f - progress * 0.6f);
                            
                            if (x >= curveX - flagWidth && x <= curveX + flagWidth)
                            {
                                float distFromCurve = Math.Abs(x - curveX) / flagWidth;
                                alpha = Math.Max(alpha, 1f - distFromCurve * 0.5f);
                            }
                        }
                    }
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates beamed double eighth notes.
        /// </summary>
        public static Texture2D GenerateMusicNoteDouble(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float note1X = size * 0.25f;
            float note2X = size * 0.7f;
            float noteY = size * 0.72f;
            float noteRadiusX = size * 0.12f;
            float noteRadiusY = size * 0.09f;
            float noteRotation = -0.25f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 0f;
                    
                    // Two note heads
                    for (int note = 0; note < 2; note++)
                    {
                        float centerX = note == 0 ? note1X : note2X;
                        float dx = x - centerX;
                        float dy = y - noteY;
                        float rotX = dx * (float)Math.Cos(noteRotation) - dy * (float)Math.Sin(noteRotation);
                        float rotY = dx * (float)Math.Sin(noteRotation) + dy * (float)Math.Cos(noteRotation);
                        float ellipseDist = (rotX * rotX) / (noteRadiusX * noteRadiusX) + 
                                           (rotY * rotY) / (noteRadiusY * noteRadiusY);
                        
                        if (ellipseDist < 1f)
                            alpha = Math.Max(alpha, 1f - ellipseDist * 0.3f);
                        
                        // Stems
                        float stemX = centerX + noteRadiusX * 0.8f;
                        float stemWidth = size * 0.055f;
                        float stemTop = size * 0.15f;
                        float stemBottom = noteY - noteRadiusY * 0.3f;
                        
                        if (x >= stemX - stemWidth / 2 && x <= stemX + stemWidth / 2 &&
                            y >= stemTop && y <= stemBottom)
                        {
                            alpha = Math.Max(alpha, 1f);
                        }
                    }
                    
                    // Beam connecting the two stems
                    float beamY = size * 0.15f;
                    float beamHeight = size * 0.08f;
                    float beamLeft = note1X + noteRadiusX * 0.5f;
                    float beamRight = note2X + noteRadiusX * 1.1f;
                    
                    if (y >= beamY && y <= beamY + beamHeight &&
                        x >= beamLeft && x <= beamRight)
                    {
                        alpha = Math.Max(alpha, 1f);
                    }
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a treble clef (G clef) symbol.
        /// </summary>
        public static Texture2D GenerateTrebleClef(int width, int height)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            float centerX = width * 0.5f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = 0f;
                    float normalizedY = (float)y / height;
                    
                    // Main spiral curve
                    float spiralRadius = width * 0.35f * (0.3f + normalizedY * 0.7f);
                    float spiralAngle = normalizedY * MathHelper.TwoPi * 2.5f - MathHelper.PiOver2;
                    float spiralX = centerX + (float)Math.Cos(spiralAngle) * spiralRadius * 0.4f;
                    float spiralY = height * (0.15f + normalizedY * 0.7f);
                    
                    float distToSpiral = Vector2.Distance(new Vector2(x, y), new Vector2(spiralX, spiralY));
                    float lineWidth = width * 0.12f;
                    
                    if (distToSpiral < lineWidth)
                    {
                        alpha = 1f - (distToSpiral / lineWidth);
                        alpha *= alpha;
                    }
                    
                    // Central vertical line
                    float lineX = centerX;
                    float lineDistX = Math.Abs(x - lineX);
                    if (lineDistX < width * 0.08f && y > height * 0.2f && y < height * 0.9f)
                    {
                        float lineAlpha = 1f - (lineDistX / (width * 0.08f));
                        alpha = Math.Max(alpha, lineAlpha * lineAlpha);
                    }
                    
                    // Bottom curl
                    if (y > height * 0.75f)
                    {
                        float curlProgress = (y - height * 0.75f) / (height * 0.25f);
                        float curlX = centerX - (float)Math.Sin(curlProgress * MathHelper.Pi) * width * 0.25f;
                        float distToCurl = Math.Abs(x - curlX);
                        if (distToCurl < width * 0.1f)
                        {
                            float curlAlpha = 1f - (distToCurl / (width * 0.1f));
                            alpha = Math.Max(alpha, curlAlpha * (1f - curlProgress * 0.5f));
                        }
                    }
                    
                    // Top dot
                    float dotX = centerX;
                    float dotY = height * 0.08f;
                    float dotRadius = width * 0.12f;
                    float distToDot = Vector2.Distance(new Vector2(x, y), new Vector2(dotX, dotY));
                    if (distToDot < dotRadius)
                    {
                        float dotAlpha = 1f - (distToDot / dotRadius);
                        alpha = Math.Max(alpha, dotAlpha * dotAlpha);
                    }
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * width + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a bass clef (F clef) symbol.
        /// </summary>
        public static Texture2D GenerateBassClef(int width, int height)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = 0f;
                    
                    // Main curved body
                    float curveProgress = (float)y / height;
                    float curveX = width * 0.35f + (float)Math.Sin(curveProgress * MathHelper.Pi * 0.8f) * width * 0.3f;
                    float distToCurve = Math.Abs(x - curveX);
                    float curveWidth = width * 0.15f * (1f - curveProgress * 0.3f);
                    
                    if (distToCurve < curveWidth && y > height * 0.1f && y < height * 0.85f)
                    {
                        alpha = 1f - (distToCurve / curveWidth);
                        alpha *= alpha;
                    }
                    
                    // Top dot (head)
                    float headX = width * 0.3f;
                    float headY = height * 0.2f;
                    float headRadius = width * 0.18f;
                    float distToHead = Vector2.Distance(new Vector2(x, y), new Vector2(headX, headY));
                    if (distToHead < headRadius)
                    {
                        float headAlpha = 1f - (distToHead / headRadius);
                        alpha = Math.Max(alpha, headAlpha);
                    }
                    
                    // Two dots on the right
                    float dot1Y = height * 0.3f;
                    float dot2Y = height * 0.5f;
                    float dotX = width * 0.8f;
                    float dotRadius = width * 0.1f;
                    
                    float distToDot1 = Vector2.Distance(new Vector2(x, y), new Vector2(dotX, dot1Y));
                    float distToDot2 = Vector2.Distance(new Vector2(x, y), new Vector2(dotX, dot2Y));
                    
                    if (distToDot1 < dotRadius)
                        alpha = Math.Max(alpha, 1f - (distToDot1 / dotRadius));
                    if (distToDot2 < dotRadius)
                        alpha = Math.Max(alpha, 1f - (distToDot2 / dotRadius));
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * width + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a musical staff (5 horizontal lines with soft glow).
        /// </summary>
        public static Texture2D GenerateMusicStaff(int width, int height)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            float lineSpacing = height / 6f;
            float lineThickness = 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = 0f;
                    
                    // 5 staff lines
                    for (int line = 1; line <= 5; line++)
                    {
                        float lineY = lineSpacing * line;
                        float distToLine = Math.Abs(y - lineY);
                        
                        if (distToLine < lineThickness * 2)
                        {
                            float lineAlpha = 1f - (distToLine / (lineThickness * 2));
                            lineAlpha *= lineAlpha;
                            alpha = Math.Max(alpha, lineAlpha);
                        }
                    }
                    
                    // Fade at edges for softer look
                    float edgeFade = 1f;
                    float edgeWidth = width * 0.1f;
                    if (x < edgeWidth)
                        edgeFade = x / edgeWidth;
                    else if (x > width - edgeWidth)
                        edgeFade = (width - x) / edgeWidth;
                    
                    alpha *= edgeFade;
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * width + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a sharp (#) symbol.
        /// </summary>
        public static Texture2D GenerateMusicSharp(int size)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float lineWidth = size * 0.1f;
            float vLineOffset = size * 0.2f;
            float hLineOffset = size * 0.15f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 0f;
                    float centerX = size / 2f;
                    float centerY = size / 2f;
                    
                    // Two vertical lines (slightly tilted)
                    for (int i = -1; i <= 1; i += 2)
                    {
                        float lineX = centerX + i * vLineOffset;
                        float tiltedX = lineX + (y - centerY) * 0.1f;
                        float distToLine = Math.Abs(x - tiltedX);
                        if (distToLine < lineWidth && y > size * 0.1f && y < size * 0.9f)
                        {
                            alpha = Math.Max(alpha, 1f - (distToLine / lineWidth));
                        }
                    }
                    
                    // Two horizontal lines (thicker)
                    for (int i = -1; i <= 1; i += 2)
                    {
                        float lineY = centerY + i * hLineOffset;
                        float distToLine = Math.Abs(y - lineY);
                        if (distToLine < lineWidth * 1.3f && x > size * 0.15f && x < size * 0.85f)
                        {
                            alpha = Math.Max(alpha, 1f - (distToLine / (lineWidth * 1.3f)));
                        }
                    }
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a flat (b) symbol.
        /// </summary>
        public static Texture2D GenerateMusicFlat(int width, int height)
        {
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            float stemX = width * 0.3f;
            float stemWidth = width * 0.12f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = 0f;
                    
                    // Vertical stem
                    float distToStem = Math.Abs(x - stemX);
                    if (distToStem < stemWidth && y > height * 0.05f && y < height * 0.95f)
                    {
                        alpha = 1f - (distToStem / stemWidth) * 0.5f;
                    }
                    
                    // Curved belly (in bottom half)
                    if (y > height * 0.45f)
                    {
                        float bellyProgress = (y - height * 0.45f) / (height * 0.5f);
                        float bellyX = stemX + (float)Math.Sin(bellyProgress * MathHelper.Pi) * width * 0.5f;
                        float bellyWidth = width * 0.12f * (1f - Math.Abs(bellyProgress - 0.5f) * 1.5f);
                        bellyWidth = Math.Max(bellyWidth, width * 0.05f);
                        
                        float distToBelly = Math.Abs(x - bellyX);
                        if (distToBelly < bellyWidth)
                        {
                            float bellyAlpha = 1f - (distToBelly / bellyWidth);
                            alpha = Math.Max(alpha, bellyAlpha * bellyAlpha);
                        }
                    }
                    
                    byte a = (byte)(MathHelper.Clamp(alpha, 0f, 1f) * 255);
                    data[y * width + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
    }
}
