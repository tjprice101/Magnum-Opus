using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// NEBULA FOG VFX SYSTEM
    /// 
    /// Creates shimmering, shifting fog/gas cloud effects using Perlin noise-based shaders.
    /// Inspired by Calamity's Exoblade constellation trails and Ark of the Cosmos fog effects.
    /// 
    /// Core Features:
    /// - Dual Perlin noise sampling for interference patterns
    /// - Radial vertex masking for soft cloud shapes
    /// - Constellation sparkle effects via thresholding
    /// - HDR simulation through over-brightening and multi-pass bloom
    /// - Theme-integrated color palettes
    /// - Automatic integration with weapons, projectiles, and bosses
    /// 
    /// USAGE:
    /// 1. Call NebulaFogSystem.SpawnFogCloud() for standalone fog effects
    /// 2. Call NebulaFogSystem.AttachTrailFog() to add fog overlay to projectile trails
    /// 3. Use NebulaFogSystem.SpawnConstellationFog() for star-studded cosmic fog
    /// </summary>
    public class NebulaFogSystem : ModSystem
    {
        #region Singleton & Initialization
        
        private static NebulaFogSystem _instance;
        public static NebulaFogSystem Instance => _instance;
        
        // Shader and texture references
        private static Effect _nebulaFogShader;
        private static Texture2D _perlinNoiseTexture;
        private static Texture2D _voronoiNoiseTexture;
        private static Texture2D _softGlowTexture;
        
        // Active fog instances
        private static List<FogCloud> _activeFogs = new List<FogCloud>();
        private static List<TrailFogOverlay> _trailFogOverlays = new List<TrailFogOverlay>();
        
        // Object pools for performance
        private static Queue<FogCloud> _fogPool = new Queue<FogCloud>();
        private static Queue<TrailFogOverlay> _trailFogPool = new Queue<TrailFogOverlay>();
        
        private const int MAX_ACTIVE_FOGS = 50;
        private const int MAX_TRAIL_OVERLAYS = 30;
        
        // Deferred texture generation flags (must happen on main thread)
        private static bool _texturesInitialized = false;
        private static bool _shaderLoadAttempted = false;
        
        public override void Load()
        {
            _instance = this;
            
            // DO NOT load textures or shaders here - Load() runs on a worker thread!
            // Texture2D creation and shader loading must happen on the main thread.
            // We'll use lazy initialization in EnsureTexturesLoaded() instead.
            
            // Pre-populate object pools (this is safe - no graphics calls)
            for (int i = 0; i < 20; i++)
            {
                _fogPool.Enqueue(new FogCloud());
            }
            for (int i = 0; i < 15; i++)
            {
                _trailFogPool.Enqueue(new TrailFogOverlay());
            }
        }
        
        /// <summary>
        /// Ensures textures and shaders are loaded. Must be called from main thread (e.g., during Draw).
        /// Uses lazy initialization pattern to avoid threading issues.
        /// </summary>
        private static void EnsureTexturesLoaded()
        {
            if (_texturesInitialized || Main.dedServ)
                return;
                
            // Try to load shader (only once)
            if (!_shaderLoadAttempted)
            {
                _shaderLoadAttempted = true;
                try
                {
                    if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/NebulaFogShader"))
                    {
                        _nebulaFogShader = ModContent.Request<Effect>(
                            "MagnumOpus/Assets/Shaders/NebulaFogShader",
                            AssetRequestMode.ImmediateLoad
                        ).Value;
                    }
                }
                catch
                {
                    // Shader not available - will use fallback rendering
                    _nebulaFogShader = null;
                }
            }
            
            // Load noise textures on main thread
            LoadNoiseTextures();
            _texturesInitialized = true;
        }
        
        public override void Unload()
        {
            _instance = null;
            _nebulaFogShader = null;
            _perlinNoiseTexture = null;
            _voronoiNoiseTexture = null;
            _softGlowTexture = null;
            _activeFogs.Clear();
            _trailFogOverlays.Clear();
            _fogPool.Clear();
            _trailFogPool.Clear();
            _texturesInitialized = false;
            _shaderLoadAttempted = false;
        }
        
        private static void LoadNoiseTextures()
        {
            try
            {
                // Try to load custom noise textures
                if (ModContent.HasAsset("MagnumOpus/Assets/VFX/PerlinNoise"))
                {
                    _perlinNoiseTexture = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX/PerlinNoise", AssetRequestMode.ImmediateLoad).Value;
                }
                
                if (ModContent.HasAsset("MagnumOpus/Assets/VFX/VoronoiNoise"))
                {
                    _voronoiNoiseTexture = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/VFX/VoronoiNoise", AssetRequestMode.ImmediateLoad).Value;
                }
                
                // Load soft glow for fog base
                if (ModContent.HasAsset("MagnumOpus/Assets/Particles/SoftGlow2"))
                {
                    _softGlowTexture = ModContent.Request<Texture2D>(
                        "MagnumOpus/Assets/Particles/SoftGlow2", AssetRequestMode.ImmediateLoad).Value;
                }
            }
            catch
            {
                // Will generate procedural noise if textures don't exist
            }
            
            // Generate procedural noise if not loaded
            if (_perlinNoiseTexture == null)
            {
                _perlinNoiseTexture = GeneratePerlinNoiseTexture(256, 256);
            }
            
            if (_voronoiNoiseTexture == null)
            {
                _voronoiNoiseTexture = GenerateVoronoiNoiseTexture(256, 256);
            }
            
            if (_softGlowTexture == null)
            {
                _softGlowTexture = GenerateSoftGlowTexture(64, 64);
            }
        }
        
        #endregion
        
        #region Procedural Noise Generation
        
        /// <summary>
        /// Generates a Perlin-like noise texture for fog effects.
        /// </summary>
        private static Texture2D GeneratePerlinNoiseTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var colors = new Color[width * height];
            
            // Multi-octave noise for Perlin-like appearance
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = 0f;
                    float amplitude = 1f;
                    float frequency = 0.01f;
                    float maxValue = 0f;
                    
                    // 4 octaves of noise
                    for (int octave = 0; octave < 4; octave++)
                    {
                        float sampleX = x * frequency;
                        float sampleY = y * frequency;
                        
                        // Simple value noise with smoothing
                        float n = ValueNoise2D(sampleX, sampleY);
                        noise += n * amplitude;
                        maxValue += amplitude;
                        
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }
                    
                    noise /= maxValue;
                    noise = noise * 0.5f + 0.5f; // Normalize to 0-1
                    
                    byte value = (byte)(noise * 255);
                    colors[y * width + x] = new Color(value, value, value, 255);
                }
            }
            
            texture.SetData(colors);
            return texture;
        }
        
        /// <summary>
        /// Generates a Voronoi-like noise texture for constellation effects.
        /// </summary>
        private static Texture2D GenerateVoronoiNoiseTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var colors = new Color[width * height];
            
            // Generate random cell centers
            int cellSize = 32;
            int cellsX = width / cellSize + 2;
            int cellsY = height / cellSize + 2;
            var cellCenters = new Vector2[cellsX * cellsY];
            var random = new Random(12345);
            
            for (int cy = 0; cy < cellsY; cy++)
            {
                for (int cx = 0; cx < cellsX; cx++)
                {
                    cellCenters[cy * cellsX + cx] = new Vector2(
                        (cx + (float)random.NextDouble()) * cellSize,
                        (cy + (float)random.NextDouble()) * cellSize
                    );
                }
            }
            
            // Calculate Voronoi distance for each pixel
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float minDist = float.MaxValue;
                    float secondDist = float.MaxValue;
                    
                    // Find nearest cell centers
                    int baseCellX = x / cellSize;
                    int baseCellY = y / cellSize;
                    
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int cellX = baseCellX + dx;
                            int cellY = baseCellY + dy;
                            
                            if (cellX >= 0 && cellX < cellsX && cellY >= 0 && cellY < cellsY)
                            {
                                Vector2 center = cellCenters[cellY * cellsX + cellX];
                                float dist = Vector2.Distance(pos, center);
                                
                                if (dist < minDist)
                                {
                                    secondDist = minDist;
                                    minDist = dist;
                                }
                                else if (dist < secondDist)
                                {
                                    secondDist = dist;
                                }
                            }
                        }
                    }
                    
                    // Edge detection for Voronoi cells
                    float edge = secondDist - minDist;
                    float value = MathHelper.Clamp(edge / (cellSize * 0.5f), 0f, 1f);
                    
                    byte byteValue = (byte)(value * 255);
                    colors[y * width + x] = new Color(byteValue, byteValue, byteValue, 255);
                }
            }
            
            texture.SetData(colors);
            return texture;
        }
        
        /// <summary>
        /// Generates a soft radial glow texture.
        /// </summary>
        private static Texture2D GenerateSoftGlowTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var colors = new Color[width * height];
            
            Vector2 center = new Vector2(width / 2f, height / 2f);
            float maxDist = width / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float falloff = 1f - MathHelper.Clamp(dist / maxDist, 0f, 1f);
                    falloff = (float)Math.Pow(falloff, 1.5f); // Smoother falloff
                    
                    byte value = (byte)(falloff * 255);
                    colors[y * width + x] = new Color(value, value, value, value);
                }
            }
            
            texture.SetData(colors);
            return texture;
        }
        
        /// <summary>
        /// Gets the fog texture for external use (e.g., themed fog effects).
        /// Ensures the texture is generated if not already loaded.
        /// </summary>
        public static Texture2D GetFogTexture()
        {
            EnsureTexturesLoaded();
            return _softGlowTexture;
        }
        
        /// <summary>
        /// Gets the Perlin noise texture for external use.
        /// </summary>
        public static Texture2D GetPerlinNoiseTexture()
        {
            EnsureTexturesLoaded();
            return _perlinNoiseTexture;
        }
        
        /// <summary>
        /// Gets the Voronoi noise texture for external use.
        /// </summary>
        public static Texture2D GetVoronoiNoiseTexture()
        {
            EnsureTexturesLoaded();
            return _voronoiNoiseTexture;
        }
        
        /// <summary>
        /// Simple 2D value noise function.
        /// </summary>
        private static float ValueNoise2D(float x, float y)
        {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            
            // Smooth interpolation
            xf = xf * xf * (3f - 2f * xf);
            yf = yf * yf * (3f - 2f * yf);
            
            // Hash corners
            float n00 = HashFloat(xi, yi);
            float n10 = HashFloat(xi + 1, yi);
            float n01 = HashFloat(xi, yi + 1);
            float n11 = HashFloat(xi + 1, yi + 1);
            
            // Bilinear interpolation
            float nx0 = MathHelper.Lerp(n00, n10, xf);
            float nx1 = MathHelper.Lerp(n01, n11, xf);
            return MathHelper.Lerp(nx0, nx1, yf);
        }
        
        private static float HashFloat(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            return (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f) * 0.5f + 0.5f;
        }
        
        #endregion
        
        #region Fog Cloud Class
        
        /// <summary>
        /// Represents a single fog cloud instance.
        /// </summary>
        public class FogCloud
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Opacity;
            public float MaxOpacity;
            public int Lifetime;
            public int MaxLifetime;
            public float DistortionStrength;
            public float SparkleThreshold;
            public float SparkleIntensity;
            public float Intensity;
            public FogStyle Style;
            public string Theme;
            public bool Active;
            
            // Scroll velocities for noise
            public Vector2 ScrollVelocity1;
            public Vector2 ScrollVelocity2;
            public float NoiseScale;
            public float PulseSpeed;
            public float PulseAmount;
            
            public void Reset()
            {
                Active = false;
                Lifetime = 0;
            }
            
            public void Initialize(Vector2 position, FogCloudConfig config)
            {
                Position = position;
                Velocity = config.Velocity;
                Scale = config.Scale;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                RotationSpeed = config.RotationSpeed;
                PrimaryColor = config.PrimaryColor;
                SecondaryColor = config.SecondaryColor;
                Opacity = 0f;
                MaxOpacity = config.Opacity;
                Lifetime = 0;
                MaxLifetime = config.Lifetime;
                DistortionStrength = config.DistortionStrength;
                SparkleThreshold = config.SparkleThreshold;
                SparkleIntensity = config.SparkleIntensity;
                Intensity = config.Intensity;
                Style = config.Style;
                Theme = config.Theme;
                ScrollVelocity1 = config.ScrollVelocity1;
                ScrollVelocity2 = config.ScrollVelocity2;
                NoiseScale = config.NoiseScale;
                PulseSpeed = config.PulseSpeed;
                PulseAmount = config.PulseAmount;
                Active = true;
            }
            
            public void Update()
            {
                if (!Active) return;
                
                Lifetime++;
                Position += Velocity;
                Rotation += RotationSpeed;
                
                // Fade in/out
                float lifeProgress = (float)Lifetime / MaxLifetime;
                if (lifeProgress < 0.2f)
                {
                    Opacity = MathHelper.Lerp(0f, MaxOpacity, lifeProgress / 0.2f);
                }
                else if (lifeProgress > 0.7f)
                {
                    Opacity = MathHelper.Lerp(MaxOpacity, 0f, (lifeProgress - 0.7f) / 0.3f);
                }
                else
                {
                    Opacity = MaxOpacity;
                }
                
                // Die when lifetime expires
                if (Lifetime >= MaxLifetime)
                {
                    Active = false;
                }
            }
        }
        
        /// <summary>
        /// Configuration for creating fog clouds.
        /// </summary>
        public struct FogCloudConfig
        {
            public Vector2 Velocity;
            public float Scale;
            public float RotationSpeed;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Opacity;
            public int Lifetime;
            public float DistortionStrength;
            public float SparkleThreshold;
            public float SparkleIntensity;
            public float Intensity;
            public FogStyle Style;
            public string Theme;
            public Vector2 ScrollVelocity1;
            public Vector2 ScrollVelocity2;
            public float NoiseScale;
            public float PulseSpeed;
            public float PulseAmount;
            
            public static FogCloudConfig Default => new FogCloudConfig
            {
                Velocity = Vector2.Zero,
                Scale = 1f,
                RotationSpeed = 0.01f,
                PrimaryColor = Color.White,
                SecondaryColor = Color.LightBlue,
                Opacity = 0.6f,
                Lifetime = 120,
                DistortionStrength = 0.05f,
                SparkleThreshold = 0.8f,
                SparkleIntensity = 1.5f,
                Intensity = 1.2f,
                Style = FogStyle.Standard,
                Theme = "generic",
                ScrollVelocity1 = new Vector2(0.1f, 0.05f),
                ScrollVelocity2 = new Vector2(-0.05f, 0.1f),
                NoiseScale = 1.5f,
                PulseSpeed = 2f,
                PulseAmount = 0.15f
            };
        }
        
        public enum FogStyle
        {
            Standard,       // Basic fog with dual noise
            Constellation,  // Fog with star sparkles
            Trail,          // Elongated trail fog
            Ambient         // Subtle background fog
        }
        
        #endregion
        
        #region Trail Fog Overlay
        
        /// <summary>
        /// Fog overlay that attaches to projectile/weapon trails.
        /// </summary>
        public class TrailFogOverlay
        {
            public int TargetProjectileId = -1;
            public List<Vector2> TrailPoints = new List<Vector2>();
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Width;
            public float Opacity;
            public float Intensity;
            public string Theme;
            public bool Active;
            
            public void Reset()
            {
                Active = false;
                TrailPoints.Clear();
                TargetProjectileId = -1;
            }
        }
        
        #endregion
        
        #region Public Spawning Methods
        
        /// <summary>
        /// Spawns a standalone fog cloud effect.
        /// </summary>
        public static FogCloud SpawnFogCloud(Vector2 position, FogCloudConfig config)
        {
            if (_activeFogs.Count >= MAX_ACTIVE_FOGS)
            {
                // Remove oldest fog
                if (_activeFogs.Count > 0)
                {
                    var oldest = _activeFogs[0];
                    oldest.Reset();
                    _fogPool.Enqueue(oldest);
                    _activeFogs.RemoveAt(0);
                }
            }
            
            FogCloud fog = _fogPool.Count > 0 ? _fogPool.Dequeue() : new FogCloud();
            fog.Initialize(position, config);
            _activeFogs.Add(fog);
            return fog;
        }
        
        /// <summary>
        /// Spawns a themed fog cloud using predefined theme colors.
        /// </summary>
        public static FogCloud SpawnThemedFog(Vector2 position, string theme, float scale = 1f, int lifetime = 90)
        {
            var config = FogCloudConfig.Default;
            config.Scale = scale;
            config.Lifetime = lifetime;
            config.Theme = theme;
            
            // Apply theme colors
            ApplyThemeColors(ref config, theme);
            
            return SpawnFogCloud(position, config);
        }
        
        /// <summary>
        /// Spawns constellation-style fog with star sparkles.
        /// </summary>
        public static FogCloud SpawnConstellationFog(Vector2 position, string theme, float scale = 1.5f)
        {
            var config = FogCloudConfig.Default;
            config.Scale = scale;
            config.Lifetime = 150;
            config.Style = FogStyle.Constellation;
            config.SparkleThreshold = 0.75f;
            config.SparkleIntensity = 2f;
            config.DistortionStrength = 0.08f;
            config.Theme = theme;
            
            ApplyThemeColors(ref config, theme);
            
            return SpawnFogCloud(position, config);
        }
        
        /// <summary>
        /// Spawns ambient background fog (subtle, long-lasting).
        /// </summary>
        public static FogCloud SpawnAmbientFog(Vector2 position, string theme, float scale = 2f)
        {
            var config = FogCloudConfig.Default;
            config.Scale = scale;
            config.Lifetime = 300;
            config.Style = FogStyle.Ambient;
            config.Opacity = 0.25f;
            config.SparkleThreshold = 0.95f;
            config.Intensity = 0.8f;
            config.PulseSpeed = 1f;
            config.Theme = theme;
            
            ApplyThemeColors(ref config, theme);
            
            return SpawnFogCloud(position, config);
        }
        
        /// <summary>
        /// Spawns a burst of multiple fog clouds for impact effects.
        /// </summary>
        public static void SpawnFogBurst(Vector2 position, string theme, int count = 5, float baseScale = 0.8f)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                
                var config = FogCloudConfig.Default;
                config.Scale = baseScale * Main.rand.NextFloat(0.8f, 1.2f);
                config.Velocity = velocity;
                config.Lifetime = 60 + Main.rand.Next(40);
                config.Theme = theme;
                ApplyThemeColors(ref config, theme);
                
                SpawnFogCloud(position + Main.rand.NextVector2Circular(10f, 10f), config);
            }
        }
        
        /// <summary>
        /// Creates an impact fog explosion with sparkles.
        /// </summary>
        public static void SpawnImpactFog(Vector2 position, string theme, float intensity = 1f)
        {
            // Central constellation fog
            SpawnConstellationFog(position, theme, 1.5f * intensity);
            
            // Surrounding burst
            SpawnFogBurst(position, theme, 4, 0.6f * intensity);
            
            // Extra sparkle fogs
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                var config = FogCloudConfig.Default;
                config.Scale = 0.4f * intensity;
                config.Lifetime = 40;
                config.SparkleThreshold = 0.6f;
                config.SparkleIntensity = 3f;
                config.Theme = theme;
                ApplyThemeColors(ref config, theme);
                SpawnFogCloud(position + offset, config);
            }
        }
        
        /// <summary>
        /// Attaches fog overlay to a projectile trail.
        /// </summary>
        public static void AttachTrailFog(int projectileId, string theme, float width = 30f, float opacity = 0.5f)
        {
            if (_trailFogOverlays.Count >= MAX_TRAIL_OVERLAYS) return;
            
            // Check if already attached
            foreach (var existing in _trailFogOverlays)
            {
                if (existing.Active && existing.TargetProjectileId == projectileId)
                    return;
            }
            
            TrailFogOverlay overlay = _trailFogPool.Count > 0 ? _trailFogPool.Dequeue() : new TrailFogOverlay();
            overlay.Reset();
            overlay.TargetProjectileId = projectileId;
            overlay.Width = width;
            overlay.Opacity = opacity;
            overlay.Theme = theme;
            overlay.Active = true;
            
            // Get theme colors
            var colors = GetThemeColors(theme);
            overlay.PrimaryColor = colors.primary;
            overlay.SecondaryColor = colors.secondary;
            overlay.Intensity = 1.2f;
            
            _trailFogOverlays.Add(overlay);
        }
        
        #endregion
        
        #region Theme Color System
        
        private static void ApplyThemeColors(ref FogCloudConfig config, string theme)
        {
            var colors = GetThemeColors(theme);
            config.PrimaryColor = colors.primary;
            config.SecondaryColor = colors.secondary;
        }
        
        public static (Color primary, Color secondary) GetThemeColors(string theme)
        {
            return theme.ToLower() switch
            {
                "lacampanella" => (new Color(255, 140, 40), new Color(80, 40, 20)),
                "eroica" => (new Color(255, 200, 80), new Color(200, 50, 50)),
                "swanlake" => (Color.White, new Color(100, 100, 140)),
                "moonlightsonata" => (new Color(135, 206, 250), new Color(75, 0, 130)),
                "enigma" or "enigmavariations" => (new Color(140, 60, 200), new Color(50, 220, 100)),
                "fate" => (new Color(200, 80, 120), new Color(140, 60, 100)),  // Brightened from near-black to dark pink
                "clairdelune" => (new Color(240, 240, 250), new Color(100, 120, 160)),
                "diesirae" => (new Color(200, 50, 30), new Color(100, 30, 25)),
                _ => (Color.LightBlue, Color.Purple)
            };
        }
        
        #endregion
        
        #region Update & Render
        
        public override void PostUpdateEverything()
        {
            // Update active fogs
            for (int i = _activeFogs.Count - 1; i >= 0; i--)
            {
                var fog = _activeFogs[i];
                if (!fog.Active)
                {
                    _fogPool.Enqueue(fog);
                    _activeFogs.RemoveAt(i);
                    continue;
                }
                
                fog.Update();
            }
            
            // Update trail fog overlays
            for (int i = _trailFogOverlays.Count - 1; i >= 0; i--)
            {
                var overlay = _trailFogOverlays[i];
                if (!overlay.Active)
                {
                    _trailFogPool.Enqueue(overlay);
                    _trailFogOverlays.RemoveAt(i);
                    continue;
                }
                
                // Track projectile position
                if (overlay.TargetProjectileId >= 0 && overlay.TargetProjectileId < Main.maxProjectiles)
                {
                    var proj = Main.projectile[overlay.TargetProjectileId];
                    if (proj.active && proj.ModProjectile?.Mod == Mod)
                    {
                        overlay.TrailPoints.Add(proj.Center);
                        if (overlay.TrailPoints.Count > 30)
                            overlay.TrailPoints.RemoveAt(0);
                    }
                    else
                    {
                        overlay.Active = false;
                    }
                }
            }
        }
        
        /// <summary>
        /// Draws all fog effects. Call this from a ModSystem.PostDrawEverything or similar.
        /// </summary>
        public static void DrawAllFogs(SpriteBatch spriteBatch)
        {
            // Ensure textures are loaded on main thread (lazy initialization)
            EnsureTexturesLoaded();
            
            if (_activeFogs.Count == 0 && _trailFogOverlays.Count == 0)
                return;
            
            // Try to end current SpriteBatch state - it may not be active
            bool endedSuccessfully = false;
            try
            {
                spriteBatch.End();
                endedSuccessfully = true;
            }
            catch (System.InvalidOperationException)
            {
                // SpriteBatch wasn't active - that's okay
            }
            
            try
            {
                // Begin with additive blending for HDR-style glow
                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Additive,
                    SamplerState.LinearWrap,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    _nebulaFogShader,
                    Main.GameViewMatrix.TransformationMatrix
                );
                
                // Draw standalone fog clouds
                foreach (var fog in _activeFogs)
                {
                    if (!fog.Active) continue;
                    DrawFogCloud(spriteBatch, fog);
                }
                
                // Draw trail fog overlays
                foreach (var overlay in _trailFogOverlays)
                {
                    if (!overlay.Active || overlay.TrailPoints.Count < 2) continue;
                    DrawTrailFog(spriteBatch, overlay);
                }
                
                spriteBatch.End();
                
                // === BLOOM PASS ===
                // Draw again at larger scale with lower opacity for bloom
                spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Additive,
                    SamplerState.LinearClamp,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    Main.GameViewMatrix.TransformationMatrix
                );
                
                foreach (var fog in _activeFogs)
                {
                    if (!fog.Active) continue;
                    DrawFogBloom(spriteBatch, fog);
                }
                
                spriteBatch.End();
                
                // Restore normal SpriteBatch only if we ended one before
                if (endedSuccessfully)
                {
                    spriteBatch.Begin(
                        SpriteSortMode.Deferred,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        DepthStencilState.None,
                        RasterizerState.CullNone,
                        null,
                        Main.GameViewMatrix.TransformationMatrix
                    );
                }
            }
            catch (System.Exception)
            {
                // VFX failed - try to restore a valid state
                try
                {
                    if (endedSuccessfully)
                    {
                        spriteBatch.Begin(
                            SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            SamplerState.PointClamp,
                            DepthStencilState.None,
                            RasterizerState.CullNone,
                            null,
                            Main.GameViewMatrix.TransformationMatrix
                        );
                    }
                }
                catch { }
            }
        }
        
        private static void DrawFogCloud(SpriteBatch spriteBatch, FogCloud fog)
        {
            if (_softGlowTexture == null) return;
            
            Vector2 drawPos = fog.Position - Main.screenPosition;
            
            // Set shader parameters if shader is loaded
            if (_nebulaFogShader != null)
            {
                SetShaderParameters(fog);
            }
            
            // Draw the fog cloud
            float scale = fog.Scale * 2f; // Scale up the soft glow texture
            
            spriteBatch.Draw(
                _softGlowTexture,
                drawPos,
                null,
                fog.PrimaryColor * fog.Opacity,
                fog.Rotation,
                new Vector2(_softGlowTexture.Width / 2f, _softGlowTexture.Height / 2f),
                scale,
                SpriteEffects.None,
                0f
            );
            
            // Secondary color layer
            spriteBatch.Draw(
                _softGlowTexture,
                drawPos,
                null,
                fog.SecondaryColor * fog.Opacity * 0.5f,
                fog.Rotation + 0.5f,
                new Vector2(_softGlowTexture.Width / 2f, _softGlowTexture.Height / 2f),
                scale * 1.2f,
                SpriteEffects.None,
                0f
            );
        }
        
        private static void DrawFogBloom(SpriteBatch spriteBatch, FogCloud fog)
        {
            if (_softGlowTexture == null) return;
            
            Vector2 drawPos = fog.Position - Main.screenPosition;
            
            // Multiple bloom layers at increasing scales
            float[] bloomScales = { 1.4f, 1.8f, 2.4f, 3.2f };
            float[] bloomOpacities = { 0.25f, 0.15f, 0.08f, 0.04f };
            
            for (int i = 0; i < bloomScales.Length; i++)
            {
                float scale = fog.Scale * 2f * bloomScales[i];
                float opacity = fog.Opacity * bloomOpacities[i];
                
                Color bloomColor = Color.Lerp(fog.PrimaryColor, fog.SecondaryColor, i / (float)bloomScales.Length);
                bloomColor = new Color(bloomColor.R, bloomColor.G, bloomColor.B, 0); // Remove alpha for additive
                
                spriteBatch.Draw(
                    _softGlowTexture,
                    drawPos,
                    null,
                    bloomColor * opacity,
                    fog.Rotation + i * 0.2f,
                    new Vector2(_softGlowTexture.Width / 2f, _softGlowTexture.Height / 2f),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static void DrawTrailFog(SpriteBatch spriteBatch, TrailFogOverlay overlay)
        {
            if (overlay.TrailPoints.Count < 2 || _softGlowTexture == null)
                return;
            
            // Draw fog puffs along the trail
            for (int i = 0; i < overlay.TrailPoints.Count; i++)
            {
                float progress = (float)i / (overlay.TrailPoints.Count - 1);
                Vector2 pos = overlay.TrailPoints[i] - Main.screenPosition;
                
                float size = overlay.Width * (1f - progress * 0.5f) / _softGlowTexture.Width;
                float alpha = overlay.Opacity * (1f - progress);
                Color color = Color.Lerp(overlay.SecondaryColor, overlay.PrimaryColor, progress);
                color = new Color(color.R, color.G, color.B, 0);
                
                spriteBatch.Draw(
                    _softGlowTexture,
                    pos,
                    null,
                    color * alpha,
                    Main.GlobalTimeWrappedHourly * 2f + i * 0.3f,
                    new Vector2(_softGlowTexture.Width / 2f, _softGlowTexture.Height / 2f),
                    size,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static void SetShaderParameters(FogCloud fog)
        {
            if (_nebulaFogShader == null) return;
            
            try
            {
                _nebulaFogShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                _nebulaFogShader.Parameters["uOpacity"]?.SetValue(fog.Opacity);
                _nebulaFogShader.Parameters["uIntensity"]?.SetValue(fog.Intensity);
                _nebulaFogShader.Parameters["uColor"]?.SetValue(fog.PrimaryColor.ToVector3());
                _nebulaFogShader.Parameters["uSecondaryColor"]?.SetValue(fog.SecondaryColor.ToVector3());
                _nebulaFogShader.Parameters["uDistortionStrength"]?.SetValue(fog.DistortionStrength);
                _nebulaFogShader.Parameters["uSparkleThreshold"]?.SetValue(fog.SparkleThreshold);
                _nebulaFogShader.Parameters["uSparkleIntensity"]?.SetValue(fog.SparkleIntensity);
                _nebulaFogShader.Parameters["uScrollVelocity1"]?.SetValue(fog.ScrollVelocity1);
                _nebulaFogShader.Parameters["uScrollVelocity2"]?.SetValue(fog.ScrollVelocity2);
                _nebulaFogShader.Parameters["uNoiseScale"]?.SetValue(fog.NoiseScale);
                _nebulaFogShader.Parameters["uRadialFalloff"]?.SetValue(1.5f);
                _nebulaFogShader.Parameters["uPulseSpeed"]?.SetValue(fog.PulseSpeed);
                _nebulaFogShader.Parameters["uPulseAmount"]?.SetValue(fog.PulseAmount);
                
                // Set noise texture
                if (_perlinNoiseTexture != null)
                {
                    Main.graphics.GraphicsDevice.Textures[1] = _perlinNoiseTexture;
                }
                if (_voronoiNoiseTexture != null)
                {
                    Main.graphics.GraphicsDevice.Textures[3] = _voronoiNoiseTexture;
                }
            }
            catch
            {
                // Shader parameter errors - continue without shader
            }
        }
        
        #endregion
        
        #region Static Accessors
        
        public static int ActiveFogCount => _activeFogs.Count;
        public static int ActiveTrailOverlayCount => _trailFogOverlays.Count;
        
        public static void ClearAllFogs()
        {
            foreach (var fog in _activeFogs)
            {
                fog.Reset();
                _fogPool.Enqueue(fog);
            }
            _activeFogs.Clear();
            
            foreach (var overlay in _trailFogOverlays)
            {
                overlay.Reset();
                _trailFogPool.Enqueue(overlay);
            }
            _trailFogOverlays.Clear();
        }
        
        #endregion
    }
}
