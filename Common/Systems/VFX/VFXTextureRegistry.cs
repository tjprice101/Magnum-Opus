using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CENTRALIZED VFX TEXTURE REGISTRY
    /// ================================
    /// 
    /// Technical Artifact Management System based on Calamity/FargosSoulsDLC patterns.
    /// 
    /// This registry provides:
    /// 1. NOISE TEXTURES - For UV advection, vertex displacement, dissolve effects
    /// 2. LUT TEXTURES - Color lookup tables for spectral/rainbow effects
    /// 3. BEAM TEXTURES - Core textures for primitive trail rendering
    /// 4. MASK TEXTURES - Edge glow, halo rings, ripple distortion
    /// 5. FALLBACK SYSTEM - Graceful degradation if textures are missing
    /// 
    /// All textures are loaded on mod initialization and cached for performance.
    /// </summary>
    public class VFXTextureRegistry : ModSystem
    {
        #region Singleton Access
        
        public static VFXTextureRegistry Instance { get; private set; }
        
        #endregion

        #region Texture Categories
        
        /// <summary>
        /// Noise textures for procedural effects.
        /// Use these for: UV advection, vertex displacement, dissolve, domain warping.
        /// </summary>
        public static class Noise
        {
            /// <summary>Primary smoke/cloud noise - organic flowing patterns (from NoiseSmoke.png)</summary>
            public static Texture2D Smoke { get; internal set; }
            
            /// <summary>Tileable FBM noise for general procedural effects</summary>
            public static Texture2D TileableFBM { get; internal set; }
            
            /// <summary>Marble/swirl noise for liquid effects</summary>
            public static Texture2D Marble { get; internal set; }
            
            /// <summary>Sparkly noise for glitter/shimmer</summary>
            public static Texture2D Sparkly { get; internal set; }
            
            /// <summary>Nebula wisp noise for cosmic effects</summary>
            public static Texture2D NebulaWisp { get; internal set; }
            
            /// <summary>Worley cellular noise for heat distortion (voronoi pattern)</summary>
            public static Texture2D Worley128 { get; internal set; }
            
            /// <summary>Fire noise optimized for dual-scroll advection</summary>
            public static Texture2D Fire { get; internal set; }
            
            /// <summary>Directional flow noise (horizontal bias)</summary>
            public static Texture2D Flow { get; internal set; }
            
            /// <summary>Gets noise texture by index (0-7) for shader sampling</summary>
            public static Texture2D GetByIndex(int index) => index switch
            {
                0 => Smoke,
                1 => TileableFBM,
                2 => Marble,
                3 => Sparkly,
                4 => NebulaWisp,
                5 => Worley128,
                6 => Fire,
                7 => Flow,
                _ => Smoke // fallback
            };
        }
        
        /// <summary>
        /// Lookup Table (LUT) textures for color gradients.
        /// Use these for: Spectral shimmer, palette lerp, rainbow effects.
        /// </summary>
        public static class LUT
        {
            /// <summary>Full rainbow spectrum gradient (from RainbowLUT.png)</summary>
            public static Texture2D Rainbow { get; internal set; }
            
            /// <summary>Energy gradient with black core and bright edges</summary>
            public static Texture2D EnergyGradient { get; internal set; }
            
            /// <summary>Horizontal energy gradient for beam effects</summary>
            public static Texture2D HorizontalEnergy { get; internal set; }
            
            /// <summary>
            /// Samples the rainbow LUT at a given position (0-1).
            /// Use for shader-like color lookups in C#.
            /// </summary>
            public static Color SampleRainbow(float t)
            {
                if (Rainbow == null) return Color.White;
                
                t = MathHelper.Clamp(t, 0f, 1f);
                int x = (int)(t * (Rainbow.Width - 1));
                
                Color[] data = new Color[1];
                Rainbow.GetData(0, new Rectangle(x, Rainbow.Height / 2, 1, 1), data, 0, 1);
                return data[0];
            }
        }
        
        /// <summary>
        /// Beam/Trail core textures for primitive rendering.
        /// Use these for: Trail shader sampling, beam cores, energy streaks.
        /// </summary>
        public static class Beam
        {
            /// <summary>Primary beam streak with lens flare (from BeamStreak1.png)</summary>
            public static Texture2D Streak1 { get; internal set; }
            
            /// <summary>Simple white bloom line for basic trails</summary>
            public static Texture2D BloomLine { get; internal set; }
            
            /// <summary>Tapered line for fade-out trails</summary>
            public static Texture2D TaperedLine { get; internal set; }
            
            /// <summary>Single white pixel for procedural generation</summary>
            public static Texture2D Pixel { get; internal set; }
        }
        
        /// <summary>
        /// Ribbon textures for triangle strip mesh rendering.
        /// Use these for: Weapon swing trails, projectile ribbons.
        /// </summary>
        public static class Ribbon
        {
            /// <summary>Soft energy ribbon with gaussian falloff</summary>
            public static Texture2D Soft { get; internal set; }
            
            /// <summary>Flame ribbon with erosion edges</summary>
            public static Texture2D Flame { get; internal set; }
            
            /// <summary>Cosmic nebula ribbon for Fate theme</summary>
            public static Texture2D Cosmic { get; internal set; }
            
            /// <summary>Electric arc ribbon with jagged edges</summary>
            public static Texture2D Electric { get; internal set; }
        }
        
        /// <summary>
        /// Mask textures for effects like halos, rings, distortion.
        /// Use these for: Edge glow smoothstep, shockwaves, screen distortion.
        /// </summary>
        public static class Mask
        {
            /// <summary>Eclipse ring for edge glow effects (from EclipseRing.png)</summary>
            public static Texture2D EclipseRing { get; internal set; }
            
            /// <summary>Concentric ripple rings for distortion (from RippleRing.png)</summary>
            public static Texture2D RippleRing { get; internal set; }
            
            /// <summary>Radial gradient (white center to black edge)</summary>
            public static Texture2D RadialGradient { get; internal set; }
            
            /// <summary>Linear gradient (white to black horizontal)</summary>
            public static Texture2D LinearGradient { get; internal set; }
        }
        
        #endregion

        #region Lifecycle
        
        private bool _texturesLoaded = false;
        private bool _fallbacksNeedGeneration = false;
        
        public override void Load()
        {
            Instance = this;
            
            if (Main.dedServ) return;
            
            // Don't load textures here - they need to be loaded on main thread
            // Use PostSetupContent instead
        }
        
        public override void PostSetupContent()
        {
            if (Main.dedServ) return;
            
            // PostSetupContent may run on background thread, queue to main thread for texture creation
            Main.QueueMainThreadAction(LoadAllTextures);
        }
        
        public override void Unload()
        {
            Instance = null;
            
            // Clear all texture references (textures are managed by tModLoader)
            Noise.Smoke = null;
            Noise.TileableFBM = null;
            Noise.Marble = null;
            Noise.Sparkly = null;
            Noise.NebulaWisp = null;
            Noise.Worley128 = null;
            Noise.Fire = null;
            Noise.Flow = null;
            
            LUT.Rainbow = null;
            LUT.EnergyGradient = null;
            LUT.HorizontalEnergy = null;
            
            Beam.Streak1 = null;
            Beam.BloomLine = null;
            Beam.TaperedLine = null;
            Beam.Pixel = null;
            
            Ribbon.Soft = null;
            Ribbon.Flame = null;
            Ribbon.Cosmic = null;
            Ribbon.Electric = null;
            
            Mask.EclipseRing = null;
            Mask.RippleRing = null;
            Mask.RadialGradient = null;
            Mask.LinearGradient = null;
        }
        
        #endregion

        #region Texture Loading
        
        private void LoadAllTextures()
        {
            // ==========================================
            // NOISE TEXTURES
            // ==========================================
            Noise.Smoke = LoadTexture("Assets/VFX/Noise/NoiseSmoke", CreateFallbackNoise);
            Noise.TileableFBM = LoadTexture("Assets/VFX/Noise/TileableFBMNoise", CreateFallbackNoise);
            Noise.Marble = LoadTexture("Assets/VFX/Noise/TileableMarbleNoise", CreateFallbackNoise);
            Noise.Sparkly = LoadTexture("Assets/VFX/Noise/SparklyNoiseTexture", CreateFallbackNoise);
            Noise.NebulaWisp = LoadTexture("Assets/VFX/Noise/NebulaWispNoise", CreateFallbackNoise);
            Noise.Worley128 = LoadTexture("Assets/VFX/Noise/WorleyNoise512", CreateFallbackWorleyNoise);
            Noise.Fire = LoadTexture("Assets/VFX/Noise/FireNoise512", CreateFallbackNoise);
            Noise.Flow = LoadTexture("Assets/VFX/Noise/FlowNoise512", CreateFallbackNoise);
            
            // ==========================================
            // LUT TEXTURES (Color Lookup Tables)
            // ==========================================
            LUT.Rainbow = LoadTexture("Assets/VFX/RainbowLUT", CreateFallbackRainbowLUT);
            LUT.EnergyGradient = LoadTexture("Assets/VFX/Noise/HorizontalBlackCoreCenterEnergyGradient", CreateFallbackGradient);
            LUT.HorizontalEnergy = LoadTexture("Assets/VFX/Noise/HorizontalEnergyGradient", CreateFallbackGradient);
            
            // ==========================================
            // BEAM/TRAIL TEXTURES
            // ==========================================
            Beam.Streak1 = LoadTexture("Assets/VFX/BeamStreak1", CreateFallbackBeam);
            Beam.BloomLine = LoadTexture("Assets/Particles/SoftGlow2", CreateFallbackBeam);
            Beam.TaperedLine = LoadTexture("Assets/Particles/ParticleTrail1", CreateFallbackBeam);
            try
            {
                Beam.Pixel = CreatePixelTexture();
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"[VFXTextureRegistry] Failed to create pixel texture: {ex.Message}");
            }
            
            // ==========================================
            // RIBBON TEXTURES
            // ==========================================
            Ribbon.Soft = LoadTexture("Assets/VFX/Ribbons/RibbonSoft512", CreateFallbackBeam);
            Ribbon.Flame = LoadTexture("Assets/VFX/Ribbons/RibbonFlame512", CreateFallbackBeam);
            Ribbon.Cosmic = LoadTexture("Assets/VFX/Ribbons/RibbonCosmic512", CreateFallbackBeam);
            Ribbon.Electric = LoadTexture("Assets/VFX/Ribbons/RibbonElectric512", CreateFallbackBeam);
            
            // ==========================================
            // MASK TEXTURES
            // ==========================================
            Mask.EclipseRing = LoadTexture("Assets/VFX/EclipseRing", CreateFallbackHalo);
            Mask.RippleRing = LoadTexture("Assets/VFX/RippleRing", CreateFallbackHalo);
            Mask.RadialGradient = LoadTexture("Assets/Particles/SoftGlow3", CreateFallbackGradient);
            Mask.LinearGradient = LoadTexture("Assets/Particles/SoftGlow4", CreateFallbackGradient);
            
            LogLoadStatus();
        }
        
        /// <summary>
        /// Loads a texture with fallback generation if missing.
        /// Must be called from main thread for fallback generation to work.
        /// </summary>
        private Texture2D LoadTexture(string path, Func<Texture2D> fallbackGenerator)
        {
            try
            {
                string fullPath = $"MagnumOpus/{path}";
                if (ModContent.HasAsset(fullPath))
                {
                    // Use ImmediateLoad since we're in PostSetupContent (main thread)
                    return ModContent.Request<Texture2D>(fullPath, AssetRequestMode.ImmediateLoad).Value;
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"[VFXTextureRegistry] Failed to load {path}: {ex.Message}");
            }
            
            // Generate fallback - only safe on main thread
            try
            {
                return fallbackGenerator?.Invoke();
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"[VFXTextureRegistry] Failed to create fallback for {path}: {ex.Message}");
                return null;
            }
        }
        
        private void LogLoadStatus()
        {
            int loaded = 0;
            int fallback = 0;
            
            // Count loaded vs fallback textures
            void Check(Texture2D tex, string name)
            {
                if (tex != null && tex.Width > 4) 
                {
                    loaded++;
                    Mod.Logger.Info($"[VFXTextureRegistry] ✓ {name}: {tex.Width}x{tex.Height}");
                }
                else 
                {
                    fallback++;
                    Mod.Logger.Warn($"[VFXTextureRegistry] ✗ {name}: FALLBACK ({(tex == null ? "null" : $"{tex.Width}x{tex.Height}")})");
                }
            }
            
            // Check all noise textures specifically
            Check(Noise.Smoke, "Noise.Smoke");
            Check(Noise.TileableFBM, "Noise.TileableFBM");
            Check(Noise.Marble, "Noise.Marble");
            Check(Noise.NebulaWisp, "Noise.NebulaWisp");
            Check(Noise.Sparkly, "Noise.Sparkly");
            Check(LUT.Rainbow, "LUT.Rainbow");
            Check(LUT.EnergyGradient, "LUT.EnergyGradient");
            Check(LUT.HorizontalEnergy, "LUT.HorizontalEnergy");
            Check(Beam.Streak1, "Beam.Streak1");
            Check(Mask.EclipseRing, "Mask.EclipseRing");
            Check(Mask.RippleRing, "Mask.RippleRing");
            
            Mod.Logger.Info($"[VFXTextureRegistry] SUMMARY: {loaded} textures loaded, {fallback} using fallbacks");
        }
        
        #endregion

        #region Fallback Texture Generation
        
        /// <summary>
        /// Creates a simple procedural noise texture as fallback.
        /// </summary>
        private Texture2D CreateFallbackNoise()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Random rand = new Random(12345);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Simple value noise
                    float noise = (float)rand.NextDouble();
                    byte value = (byte)(noise * 255);
                    data[y * size + x] = new Color(value, value, value, 255);
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Creates a Worley (cellular) noise texture as fallback.
        /// Used for flame erosion and cell-like patterns.
        /// </summary>
        private Texture2D CreateFallbackWorleyNoise()
        {
            const int size = 128;
            const int cellCount = 8; // 8x8 grid of feature points
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Random rand = new Random(54321);
            
            // Generate random feature points in each cell
            Vector2[,] featurePoints = new Vector2[cellCount, cellCount];
            for (int cy = 0; cy < cellCount; cy++)
            {
                for (int cx = 0; cx < cellCount; cx++)
                {
                    float cellSize = size / (float)cellCount;
                    float px = cx * cellSize + (float)rand.NextDouble() * cellSize;
                    float py = cy * cellSize + (float)rand.NextDouble() * cellSize;
                    featurePoints[cx, cy] = new Vector2(px, py);
                }
            }
            
            // For each pixel, find distance to nearest feature point
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float minDist = float.MaxValue;
                    float secondDist = float.MaxValue;
                    
                    // Check surrounding cells (3x3 neighborhood)
                    int cellX = (int)(x / (size / (float)cellCount));
                    int cellY = (int)(y / (size / (float)cellCount));
                    
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int cx = (cellX + dx + cellCount) % cellCount;
                            int cy = (cellY + dy + cellCount) % cellCount;
                            
                            // Handle wrapping for seamless tiling
                            Vector2 featurePos = featurePoints[cx, cy];
                            if (dx == -1 && cellX == 0) featurePos.X -= size;
                            if (dx == 1 && cellX == cellCount - 1) featurePos.X += size;
                            if (dy == -1 && cellY == 0) featurePos.Y -= size;
                            if (dy == 1 && cellY == cellCount - 1) featurePos.Y += size;
                            
                            float dist = Vector2.Distance(pos, featurePos);
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
                    
                    // F1 (distance to nearest) normalized
                    float maxExpectedDist = size / (float)cellCount;
                    float normalizedDist = Math.Min(1f, minDist / maxExpectedDist);
                    
                    // Store F1 in R, edge (F2-F1) in G for shader flexibility
                    byte f1 = (byte)(normalizedDist * 255);
                    byte edge = (byte)(Math.Min(1f, (secondDist - minDist) / (maxExpectedDist * 0.5f)) * 255);
                    
                    data[y * size + x] = new Color(f1, edge, f1, 255);
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Creates a rainbow gradient LUT as fallback.
        /// </summary>
        private Texture2D CreateFallbackRainbowLUT()
        {
            const int width = 256;
            const int height = 4;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int x = 0; x < width; x++)
            {
                float hue = (float)x / width;
                Color color = Main.hslToRgb(hue, 1f, 0.5f);
                
                for (int y = 0; y < height; y++)
                {
                    data[y * width + x] = color;
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Creates a simple horizontal gradient as fallback.
        /// </summary>
        private Texture2D CreateFallbackGradient()
        {
            const int width = 256;
            const int height = 4;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int x = 0; x < width; x++)
            {
                float t = (float)x / width;
                byte value = (byte)(t * 255);
                Color color = new Color(value, value, value, 255);
                
                for (int y = 0; y < height; y++)
                {
                    data[y * width + x] = color;
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Creates a simple beam texture as fallback.
        /// </summary>
        private Texture2D CreateFallbackBeam()
        {
            const int width = 128;
            const int height = 16;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                // Vertical falloff (bright center, dark edges)
                float yNorm = (float)y / height;
                float verticalFade = 1f - Math.Abs(yNorm - 0.5f) * 2f;
                verticalFade = (float)Math.Pow(verticalFade, 0.5);
                
                for (int x = 0; x < width; x++)
                {
                    // Horizontal falloff (bright left/center, dark right)
                    float xNorm = (float)x / width;
                    float horizontalFade = 1f - xNorm;
                    
                    float intensity = verticalFade * horizontalFade;
                    byte value = (byte)(intensity * 255);
                    data[y * width + x] = new Color(value, value, value, value);
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Creates a radial halo texture as fallback.
        /// </summary>
        private Texture2D CreateFallbackHalo()
        {
            const int size = 128;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float normalizedDist = dist / maxDist;
                    
                    // Ring pattern (bright at specific radius)
                    float ringValue = 1f - Math.Abs(normalizedDist - 0.8f) * 5f;
                    ringValue = Math.Max(0f, Math.Min(1f, ringValue));
                    
                    byte value = (byte)(ringValue * 255);
                    data[y * size + x] = new Color(value, value, value, value);
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Creates a single white pixel texture.
        /// </summary>
        private Texture2D CreatePixelTexture()
        {
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
            tex.SetData(new[] { Color.White });
            return tex;
        }
        
        #endregion

        #region Public Convenience Methods
        
        /// <summary>
        /// Gets a simple 1x1 white pixel texture for drawing solid shapes.
        /// Use as base texture when shader does all the work.
        /// </summary>
        public static Texture2D GetWhitePixel()
        {
            return Beam.Pixel ?? Instance?.CreatePixelTexture();
        }
        
        /// <summary>
        /// Gets a generic noise texture for procedural effects.
        /// Falls back through available noise textures.
        /// </summary>
        public static Texture2D GetGenericNoise()
        {
            return Noise.Smoke ?? Noise.TileableFBM ?? Noise.Marble ?? GetWhitePixel();
        }
        
        #endregion
        
        #region Shader Integration Helpers
        
        /// <summary>
        /// Prepares texture samplers for shader use.
        /// Call this before drawing with a custom shader that needs VFX textures.
        /// </summary>
        public static void SetShaderTextures(GraphicsDevice device, Effect shader)
        {
            if (shader == null) return;
            
            try
            {
                // Set noise texture to sampler slot 1
                if (shader.Parameters["uNoiseTexture"] != null && Noise.Smoke != null)
                {
                    shader.Parameters["uNoiseTexture"].SetValue(Noise.Smoke);
                }
                
                // Set LUT texture to sampler slot 2
                if (shader.Parameters["uPaletteLUT"] != null && LUT.Rainbow != null)
                {
                    shader.Parameters["uPaletteLUT"].SetValue(LUT.Rainbow);
                }
                
                // Set mask texture to sampler slot 3
                if (shader.Parameters["uMaskTexture"] != null && Mask.EclipseRing != null)
                {
                    shader.Parameters["uMaskTexture"].SetValue(Mask.EclipseRing);
                }
            }
            catch (Exception ex)
            {
                // Silent fail - shader may not have these parameters
            }
        }
        
        /// <summary>
        /// Gets the appropriate noise texture for a theme.
        /// Different themes use different noise characteristics.
        /// </summary>
        public static Texture2D GetNoiseForTheme(string theme)
        {
            return theme?.ToLowerInvariant() switch
            {
                "fate" or "enigma" or "enigmavariations" => Noise.NebulaWisp ?? Noise.Smoke,
                "lacampanella" => Noise.Smoke,
                "swanlake" => Noise.Marble ?? Noise.Smoke,
                "moonlightsonata" or "clairderlune" => Noise.Marble ?? Noise.Smoke,
                "eroica" or "diesirae" => Noise.Sparkly ?? Noise.Smoke,
                _ => Noise.Smoke ?? Noise.TileableFBM
            };
        }
        
        #endregion
    }
}
