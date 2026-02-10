using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE NEBULA TRAIL SYSTEM
    /// 
    /// Implements the 5-layer rendering technique from the Ark of the Cosmos:
    /// 
    /// Layer 1 - Base Shape (Nebula Wisp Noise): Defines primary visual form
    /// Layer 2 - Detail Variation (FBM Noise): Adds volumetric depth and variation
    /// Layer 3 - Core Definition (Horizontal Energy Gradient): Creates intensity hotspot
    /// Layer 4 - Sparkle Overlay (Sparkly Noise): Final detail polish
    /// Layer 5 - Distortion Pass (Marble Noise): UV distortion for organic movement
    /// 
    /// All layers use additive blending and scroll at different speeds/directions
    /// for a dynamic, flowing nebula effect.
    /// </summary>
    public static class CalamityNebulaTrail
    {
        #region Constants
        
        private const int MAX_TRAILS = 10;
        private const int MAX_SEGMENTS = 35;
        private const float MAX_AGE = 0.6f; // Seconds
        
        #endregion
        
        #region Trail Data
        
        private static List<NebulaTrailData> _trails = new List<NebulaTrailData>();
        private static BasicEffect _basicEffect;
        private static VertexPositionColorTexture[] _vertices;
        private static short[] _indices;
        private static bool _debugLogOnce = true; // Reset on world load
        
        private class NebulaTrailData
        {
            public List<TrailSegment> Segments = new List<TrailSegment>(MAX_SEGMENTS);
            public int OwnerId;
            public Color ColorHot;
            public Color ColorCool;
            public Color AccentColor;
            public float BaseWidth;
            public bool IsActive;
            public bool IsFading;
            public float FadeProgress;
            public string Theme;
            
            // Scroll offsets for each layer (accumulate over time)
            public float ScrollOffset1;
            public float ScrollOffset2;
            public float SparkleOffset;
            public float DistortionOffset;
        }
        
        private struct TrailSegment
        {
            public Vector2 Position;
            public Vector2 Perpendicular;
            public float Age;
            public float Rotation;
        }
        
        #endregion
        
        #region Initialization
        
        public static void Initialize()
        {
            if (Main.dedServ) return;
            
            _basicEffect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity
            };
            
            // Allocate buffers for max capacity
            int maxVertices = MAX_TRAILS * MAX_SEGMENTS * 2;
            int maxIndices = MAX_TRAILS * MAX_SEGMENTS * 6;
            _vertices = new VertexPositionColorTexture[maxVertices];
            _indices = new short[maxIndices];
        }
        
        public static void Unload()
        {
            _trails.Clear();
            // Don't dispose BasicEffect here - it causes threading issues during mod unload
            // The graphics device will clean it up when the game exits
            _basicEffect = null;
            _vertices = null;
            _indices = null;
        }
        
        #endregion
        
        #region Texture Access (VFXTextureRegistry)
        
        // ============================================
        // TEXTURE LOOKUPS - NOW USE VFXTextureRegistry
        // ============================================
        // Centralized texture management with proper fallbacks.
        
        private static Texture2D NebulaWispNoise => VFXTextureRegistry.Noise.NebulaWisp ?? VFXTextureRegistry.Noise.Smoke;
        private static Texture2D FBMNoise => VFXTextureRegistry.Noise.TileableFBM ?? VFXTextureRegistry.Noise.Smoke;
        private static Texture2D MarbleNoise => VFXTextureRegistry.Noise.Marble ?? VFXTextureRegistry.Noise.Smoke;
        private static Texture2D EnergyGradient => VFXTextureRegistry.LUT.HorizontalEnergy ?? VFXTextureRegistry.LUT.EnergyGradient;
        private static Texture2D BlackCoreGradient => VFXTextureRegistry.LUT.EnergyGradient;
        private static Texture2D SparklyNoise => VFXTextureRegistry.Noise.Sparkly ?? VFXTextureRegistry.Noise.Smoke;
        
        #endregion
        
        #region Color Palettes
        
        /// <summary>
        /// Gets the cosmic color gradient for a theme.
        /// Returns (hotCore, midRange, coolEdges, accentSparkle) colors.
        /// </summary>
        public static (Color hot, Color mid, Color cool, Color accent) GetCosmicPalette(string theme)
        {
            return theme?.ToLower() switch
            {
                "fate" => (
                    new Color(255, 255, 255),      // Hot: White core
                    new Color(200, 80, 120),       // Mid: Dark pink
                    new Color(80, 20, 60),         // Cool: Deep magenta
                    new Color(255, 180, 200)       // Accent: Bright pink sparkle
                ),
                "lacampanella" => (
                    new Color(255, 255, 200),      // Hot: Warm white
                    new Color(255, 140, 40),       // Mid: Orange flames
                    new Color(80, 30, 10),         // Cool: Dark ember
                    new Color(255, 200, 80)        // Accent: Gold sparkle
                ),
                "eroica" => (
                    new Color(255, 255, 255),      // Hot: White
                    new Color(255, 200, 80),       // Mid: Gold
                    new Color(120, 30, 30),        // Cool: Deep scarlet
                    new Color(255, 180, 200)       // Accent: Sakura pink
                ),
                "swanlake" => (
                    new Color(255, 255, 255),      // Hot: Pure white
                    new Color(180, 220, 255),      // Mid: Icy blue
                    new Color(40, 40, 60),         // Cool: Deep black-blue
                    new Color(255, 200, 255)       // Accent: Iridescent
                ),
                "moonlightsonata" => (
                    new Color(200, 220, 255),      // Hot: Moon white
                    new Color(120, 80, 255),       // Mid: Purple-blue
                    new Color(40, 20, 80),         // Cool: Deep purple
                    new Color(180, 200, 255)       // Accent: Moonlight
                ),
                "enigma" or "enigmavariations" => (
                    new Color(180, 255, 200),      // Hot: Eerie green-white
                    new Color(140, 60, 200),       // Mid: Purple
                    new Color(20, 40, 30),         // Cool: Void green
                    new Color(100, 255, 150)       // Accent: Ghostly green
                ),
                "diesirae" => (
                    new Color(255, 255, 200),      // Hot: Hellfire white
                    new Color(200, 50, 30),        // Mid: Crimson
                    new Color(60, 10, 10),         // Cool: Blood red
                    new Color(255, 150, 100)       // Accent: Fire orange
                ),
                _ => ( // Default cosmic theme
                    new Color(255, 255, 255),      // Hot: White
                    new Color(120, 80, 255),       // Mid: Purple-blue
                    new Color(40, 20, 80),         // Cool: Deep purple
                    new Color(200, 200, 255)       // Accent: Light blue
                )
            };
        }
        
        /// <summary>
        /// Interpolates through the hot-mid-cool gradient.
        /// t: 0.0 = cool edges, 1.0 = hot core
        /// </summary>
        public static Color GetGradientColor(Color hot, Color mid, Color cool, float t)
        {
            if (t < 0.5f)
            {
                return Color.Lerp(cool, mid, t * 2f);
            }
            else
            {
                return Color.Lerp(mid, hot, (t - 0.5f) * 2f);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Updates the nebula trail for a swing.
        /// Call every frame during a weapon swing.
        /// </summary>
        public static void UpdateTrail(Player player, Vector2 tipPosition, float rotation, 
            float width = 50f, string theme = null)
        {
            if (Main.dedServ) return;
            
            var (hot, mid, cool, accent) = GetCosmicPalette(theme);
            
            // Find or create trail
            NebulaTrailData trail = FindOrCreateTrail(player.whoAmI, hot, mid, cool, accent, width, theme);
            if (trail == null) return;
            
            // Update scroll offsets (different speeds per layer)
            float dt = 1f / 60f; // Assuming 60 FPS
            trail.ScrollOffset1 += 0.15f * dt * 60f;     // Layer 1: 0.15 scroll
            trail.ScrollOffset2 += -0.10f * dt * 60f;    // Layer 2: -0.10 (counter-scroll)
            trail.SparkleOffset += 0.25f * dt * 60f;     // Sparkles: faster
            trail.DistortionOffset += 0.05f * dt * 60f;  // Distortion: slow
            
            Vector2 perpendicular = new Vector2(-MathF.Sin(rotation), MathF.Cos(rotation));
            
            // Add new segment if moved enough
            if (trail.Segments.Count == 0 || 
                Vector2.DistanceSquared(tipPosition, trail.Segments[^1].Position) > 9f)
            {
                // Remove oldest if at capacity
                while (trail.Segments.Count >= MAX_SEGMENTS)
                    trail.Segments.RemoveAt(0);
                
                trail.Segments.Add(new TrailSegment
                {
                    Position = tipPosition,
                    Perpendicular = perpendicular,
                    Age = 0f,
                    Rotation = rotation
                });
            }
            
            // Age all segments
            for (int i = 0; i < trail.Segments.Count; i++)
            {
                var seg = trail.Segments[i];
                seg.Age += dt;
                trail.Segments[i] = seg;
            }
            
            // Remove expired segments
            trail.Segments.RemoveAll(s => s.Age > MAX_AGE);
        }
        
        /// <summary>
        /// Ends the trail (starts fading).
        /// </summary>
        public static void EndTrail(Player player)
        {
            foreach (var trail in _trails)
            {
                if (trail.OwnerId == player.whoAmI && trail.IsActive && !trail.IsFading)
                {
                    trail.IsFading = true;
                    trail.FadeProgress = 0f;
                }
            }
        }
        
        /// <summary>
        /// Spawns an instant swing arc.
        /// </summary>
        public static void SpawnSwingArc(Player player, float startAngle, float endAngle,
            float bladeLength, float width = 50f, string theme = null, int pointCount = 20)
        {
            if (Main.dedServ) return;
            
            var (hot, mid, cool, accent) = GetCosmicPalette(theme);
            
            // Create new trail
            if (_trails.Count >= MAX_TRAILS)
                _trails.RemoveAt(0);
            
            var trail = new NebulaTrailData
            {
                OwnerId = player.whoAmI,
                ColorHot = hot,
                ColorCool = mid,
                AccentColor = accent,
                BaseWidth = width,
                IsActive = true,
                IsFading = true, // Immediately fading
                FadeProgress = 0f,
                Theme = theme,
                ScrollOffset1 = Main.rand.NextFloat(10f),
                ScrollOffset2 = Main.rand.NextFloat(10f),
                SparkleOffset = Main.rand.NextFloat(10f),
                DistortionOffset = Main.rand.NextFloat(10f)
            };
            
            // Generate arc
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                float angle = MathHelper.Lerp(startAngle, endAngle, t);
                Vector2 pos = player.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * bladeLength;
                Vector2 perp = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
                
                trail.Segments.Add(new TrailSegment
                {
                    Position = pos,
                    Perpendicular = perp,
                    Age = t * MAX_AGE * 0.3f, // Pre-age for taper effect
                    Rotation = angle
                });
            }
            
            _trails.Add(trail);
        }
        
        #endregion
        
        #region Internal
        
        private static NebulaTrailData FindOrCreateTrail(int ownerId, Color hot, Color mid, Color cool, 
            Color accent, float width, string theme)
        {
            // Find existing active trail
            foreach (var t in _trails)
            {
                if (t.OwnerId == ownerId && t.IsActive && !t.IsFading)
                    return t;
            }
            
            // Create new
            if (_trails.Count >= MAX_TRAILS)
            {
                // Remove oldest fading trail
                for (int i = 0; i < _trails.Count; i++)
                {
                    if (_trails[i].IsFading)
                    {
                        _trails.RemoveAt(i);
                        break;
                    }
                }
                if (_trails.Count >= MAX_TRAILS)
                    _trails.RemoveAt(0);
            }
            
            var trail = new NebulaTrailData
            {
                OwnerId = ownerId,
                ColorHot = hot,
                ColorCool = Color.Lerp(cool, mid, 0.5f),
                AccentColor = accent,
                BaseWidth = width,
                IsActive = true,
                IsFading = false,
                FadeProgress = 0f,
                Theme = theme,
                ScrollOffset1 = Main.rand.NextFloat(10f),
                ScrollOffset2 = Main.rand.NextFloat(10f),
                SparkleOffset = Main.rand.NextFloat(10f),
                DistortionOffset = Main.rand.NextFloat(10f)
            };
            _trails.Add(trail);
            return trail;
        }
        
        #endregion
        
        #region Update
        
        public static void Update()
        {
            float dt = 1f / 60f;
            
            for (int i = _trails.Count - 1; i >= 0; i--)
            {
                var trail = _trails[i];
                
                // Update scroll offsets continuously
                trail.ScrollOffset1 += 0.08f;
                trail.ScrollOffset2 += -0.06f;
                trail.SparkleOffset += 0.12f;
                trail.DistortionOffset += 0.03f;
                
                if (trail.IsFading)
                {
                    trail.FadeProgress += dt * 2.5f; // ~0.4 second fade
                    
                    if (trail.FadeProgress >= 1f || trail.Segments.Count == 0)
                    {
                        trail.IsActive = false;
                        _trails.RemoveAt(i);
                        continue;
                    }
                    
                    // Shrink trail from head
                    if (trail.Segments.Count > 0 && Main.GameUpdateCount % 2 == 0)
                    {
                        trail.Segments.RemoveAt(0);
                    }
                }
            }
        }
        
        #endregion
        
        #region Render
        
        /// <summary>
        /// Renders all active nebula trails with the 5-layer technique.
        /// </summary>
        public static void Render()
        {
            if (_trails.Count == 0) return;
            if (_basicEffect == null) Initialize();
            if (_basicEffect == null) return;
            
            // Get all textures
            Texture2D nebulaWisp = NebulaWispNoise;
            Texture2D fbm = FBMNoise;
            Texture2D marble = MarbleNoise;
            Texture2D energyGrad = EnergyGradient ?? BlackCoreGradient;
            Texture2D sparkles = SparklyNoise;
            
            // DEBUG: Log texture status once
            if (_debugLogOnce)
            {
                _debugLogOnce = false;
                ModContent.GetInstance<MagnumOpus>()?.Logger?.Info(
                    $"[CalamityNebulaTrail] Textures: " +
                    $"NebulaWisp={(nebulaWisp != null ? $"{nebulaWisp.Width}x{nebulaWisp.Height}" : "NULL")}, " +
                    $"FBM={(fbm != null ? $"{fbm.Width}x{fbm.Height}" : "NULL")}, " +
                    $"Marble={(marble != null ? $"{marble.Width}x{marble.Height}" : "NULL")}, " +
                    $"EnergyGrad={(energyGrad != null ? $"{energyGrad.Width}x{energyGrad.Height}" : "NULL")}, " +
                    $"Sparkles={(sparkles != null ? $"{sparkles.Width}x{sparkles.Height}" : "NULL")}");
            }
            
            // Fallback if missing hero texture
            Texture2D baseTexture = nebulaWisp ?? fbm ?? marble ?? energyGrad;
            if (baseTexture == null) return;
            
            GraphicsDevice device = Main.instance.GraphicsDevice;
            
            // Setup projection
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
            foreach (var trail in _trails)
            {
                if (trail.Segments.Count < 2) continue;
                
                float fadeAlpha = trail.IsFading ? 1f - trail.FadeProgress : 1f;
                fadeAlpha = Math.Clamp(fadeAlpha, 0f, 1f);
                
                // ============================================================
                // LAYER 1: BASE SHAPE (Nebula Wisp Noise)
                // Primary visual form, scrolling at moderate speed
                // ============================================================
                if (nebulaWisp != null)
                {
                    RenderLayer(device, trail, nebulaWisp,
                        widthMult: 2.0f,
                        alphaMult: 0.6f * fadeAlpha,
                        scrollOffset: trail.ScrollOffset1,
                        colorMode: ColorMode.Gradient,
                        blendState: BlendState.Additive,
                        distortionOffset: trail.DistortionOffset,
                        distortionStrength: 0.02f,
                        distortionTexture: marble);
                }
                
                // ============================================================
                // LAYER 2: DETAIL VARIATION (FBM Noise)
                // Volumetric depth, counter-scroll at 0.7x speed
                // ============================================================
                if (fbm != null)
                {
                    RenderLayer(device, trail, fbm,
                        widthMult: 1.5f,
                        alphaMult: 0.4f * fadeAlpha,
                        scrollOffset: trail.ScrollOffset2,
                        colorMode: ColorMode.Secondary,
                        blendState: BlendState.Additive,
                        distortionOffset: trail.DistortionOffset * 0.7f,
                        distortionStrength: 0.015f,
                        distortionTexture: marble);
                }
                
                // ============================================================
                // LAYER 3: CORE DEFINITION (Energy Gradient)
                // Intensity hotspot along centerline
                // ============================================================
                if (energyGrad != null)
                {
                    RenderLayer(device, trail, energyGrad,
                        widthMult: 0.8f,
                        alphaMult: 0.7f * fadeAlpha,
                        scrollOffset: 0f, // No scroll - locked to geometry
                        colorMode: ColorMode.Hot,
                        blendState: BlendState.Additive,
                        distortionOffset: 0f,
                        distortionStrength: 0f,
                        distortionTexture: null);
                }
                
                // ============================================================
                // LAYER 4: SPARKLE OVERLAY (Sparkly Noise)
                // Fast scroll with threshold, 30% spawn chance per segment
                // ============================================================
                if (sparkles != null && Main.rand.NextFloat() < 0.4f)
                {
                    RenderLayer(device, trail, sparkles,
                        widthMult: 1.8f,
                        alphaMult: 0.25f * fadeAlpha,
                        scrollOffset: trail.SparkleOffset,
                        colorMode: ColorMode.Accent,
                        blendState: BlendState.Additive,
                        distortionOffset: 0f,
                        distortionStrength: 0f,
                        distortionTexture: null,
                        applyThreshold: true,
                        threshold: 0.7f);
                }
                
                // ============================================================
                // LAYER 5: BRIGHT CORE (Energy gradient, thin, white)
                // Final hotspot at center
                // ============================================================
                {
                    Texture2D coreTex = BlackCoreGradient ?? energyGrad ?? baseTexture;
                    RenderLayer(device, trail, coreTex,
                        widthMult: 0.3f,
                        alphaMult: 0.8f * fadeAlpha,
                        scrollOffset: 0f,
                        colorMode: ColorMode.White,
                        blendState: BlendState.Additive,
                        distortionOffset: 0f,
                        distortionStrength: 0f,
                        distortionTexture: null);
                }
            }
        }
        
        private enum ColorMode
        {
            Gradient,   // Hot-to-cool gradient based on position
            Hot,        // Hot color only
            Secondary,  // Cool/secondary color
            Accent,     // Accent sparkle color
            White       // Pure white for core
        }
        
        private static void RenderLayer(GraphicsDevice device, NebulaTrailData trail,
            Texture2D texture, float widthMult, float alphaMult, float scrollOffset,
            ColorMode colorMode, BlendState blendState,
            float distortionOffset, float distortionStrength, Texture2D distortionTexture,
            bool applyThreshold = false, float threshold = 0.8f)
        {
            if (texture == null || trail.Segments.Count < 2) return;
            
            int pointCount = trail.Segments.Count;
            int vertexCount = pointCount * 2;
            int triangleCount = (pointCount - 1) * 2;
            
            if (vertexCount > _vertices.Length) return;
            
            // Build vertices
            for (int i = 0; i < pointCount; i++)
            {
                var seg = trail.Segments[i];
                float completionRatio = (float)i / (pointCount - 1);
                float ageRatio = seg.Age / MAX_AGE;
                
                // QuadraticBump width: thick in middle, thin at edges
                // Combined with age-based fade
                float bumpFactor = completionRatio * (4f - completionRatio * 4f); // 0→1→0
                float ageFade = 1f - ageRatio;
                float width = trail.BaseWidth * widthMult * bumpFactor * ageFade;
                
                // Get color based on mode
                Color baseColor = colorMode switch
                {
                    ColorMode.Gradient => GetGradientColor(trail.ColorHot, 
                        Color.Lerp(trail.ColorHot, trail.ColorCool, 0.5f), 
                        trail.ColorCool, 
                        bumpFactor),
                    ColorMode.Hot => trail.ColorHot,
                    ColorMode.Secondary => trail.ColorCool,
                    ColorMode.Accent => trail.AccentColor,
                    ColorMode.White => Color.White,
                    _ => trail.ColorHot
                };
                
                // Apply alpha
                float alpha = alphaMult * ageFade * bumpFactor;
                Color finalColor = baseColor * alpha;
                
                // Remove alpha channel for additive blending
                if (blendState == BlendState.Additive)
                    finalColor = finalColor with { A = 0 };
                
                Vector2 screenPos = seg.Position - Main.screenPosition;
                
                // Calculate UV with scrolling and optional distortion
                float u = completionRatio + scrollOffset;
                
                // Apply marble noise distortion if enabled
                if (distortionStrength > 0 && distortionTexture != null)
                {
                    // Pseudo-distortion based on position (proper would need shader)
                    float noiseX = (seg.Position.X * 0.01f + distortionOffset) % 1f;
                    float noiseY = (seg.Position.Y * 0.01f) % 1f;
                    u += MathF.Sin(noiseX * MathHelper.TwoPi) * distortionStrength;
                }
                
                // Top and bottom vertices
                Vector2 offset = seg.Perpendicular * width * 0.5f;
                Vector2 topPos = screenPos + offset;
                Vector2 bottomPos = screenPos - offset;
                
                _vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(topPos.X, topPos.Y, 0),
                    finalColor,
                    new Vector3(u, 0, 0));
                
                _vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(bottomPos.X, bottomPos.Y, 0),
                    finalColor,
                    new Vector3(u, 1, 0));
            }
            
            // Build triangle indices
            int idx = 0;
            for (int i = 0; i < pointCount - 1; i++)
            {
                int baseV = i * 2;
                _indices[idx++] = (short)baseV;
                _indices[idx++] = (short)(baseV + 1);
                _indices[idx++] = (short)(baseV + 2);
                _indices[idx++] = (short)(baseV + 1);
                _indices[idx++] = (short)(baseV + 3);
                _indices[idx++] = (short)(baseV + 2);
            }
            
            // Set graphics state
            device.BlendState = blendState;
            device.SamplerStates[0] = SamplerState.LinearWrap;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            
            // Set texture
            _basicEffect.Texture = texture;
            _basicEffect.TextureEnabled = true;
            _basicEffect.VertexColorEnabled = true;
            
            // Draw
            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices,
                    0,
                    vertexCount,
                    _indices,
                    0,
                    triangleCount);
            }
        }
        
        public static void Clear()
        {
            _trails.Clear();
        }
        
        #endregion
    }
    
    /// <summary>
    /// ModSystem to integrate CalamityNebulaTrail into the game.
    /// </summary>
    public class CalamityNebulaTrailSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawNebulaTrails;
        }
        
        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawNebulaTrails;
            CalamityNebulaTrail.Unload();
        }
        
        public override void PostUpdatePlayers()
        {
            CalamityNebulaTrail.Update();
        }
        
        private void DrawNebulaTrails(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            
            try
            {
                CalamityNebulaTrail.Render();
            }
            catch (Exception ex)
            {
                Mod?.Logger?.Warn($"CalamityNebulaTrail render error: {ex.Message}");
            }
        }
        
        public override void OnWorldUnload()
        {
            CalamityNebulaTrail.Clear();
        }
    }
}
