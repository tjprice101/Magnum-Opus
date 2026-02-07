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
    /// SPRITEBATCH-BASED NEBULA TRAIL SYSTEM
    /// 
    /// This approach uses SpriteBatch to draw noise textures along the trail arc,
    /// which properly samples textures unlike BasicEffect triangle strips.
    /// 
    /// Each trail segment draws multiple additive texture layers:
    /// - Layer 1: Large foggy base (NebulaWispNoise)
    /// - Layer 2: Medium detail (FBMNoise)
    /// - Layer 3: Core energy (EnergyGradient)
    /// - Layer 4: Sparkles (SparklyNoise) - occasional
    /// 
    /// All textures are drawn as rotated quads centered on each trail point,
    /// with size, opacity, and color interpolated along the trail.
    /// </summary>
    public static class SpriteBatchNebulaTrail
    {
        #region Constants
        
        private const int MAX_TRAILS = 8;
        private const int MAX_POINTS = 40;
        private const float POINT_SPACING = 8f;
        
        #endregion
        
        #region Trail Data
        
        private class TrailData
        {
            public int OwnerId;
            public List<TrailPoint> Points = new List<TrailPoint>(MAX_POINTS);
            public Color ColorHot;
            public Color ColorCool;
            public Color ColorAccent;
            public float BaseWidth;
            public bool IsActive;
            public bool IsFading;
            public float FadeProgress;
            public string Theme;
            public float ScrollOffset;
        }
        
        private struct TrailPoint
        {
            public Vector2 Position;
            public float Rotation;
            public float Age; // 0-1
        }
        
        private static List<TrailData> _trails = new List<TrailData>();
        
        #endregion
        
        #region Textures
        
        private static Texture2D _nebulaWisp;
        private static Texture2D _fbmNoise;
        private static Texture2D _energyGradient;
        private static Texture2D _sparklyNoise;
        private static Texture2D _fallbackGlow;
        
        private static void EnsureTextures()
        {
            if (_fallbackGlow == null || _fallbackGlow.IsDisposed)
            {
                // Create a simple gradient circle as fallback
                _fallbackGlow = new Texture2D(Main.instance.GraphicsDevice, 64, 64);
                Color[] data = new Color[64 * 64];
                Vector2 center = new Vector2(32, 32);
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center) / 32f;
                        float alpha = Math.Max(0, 1f - dist);
                        alpha = alpha * alpha; // Quadratic falloff
                        data[y * 64 + x] = new Color(255, 255, 255, (byte)(alpha * 255));
                    }
                }
                _fallbackGlow.SetData(data);
            }
            
            // Try to load from CinematicVFX
            _nebulaWisp = CinematicVFX.NebulaWispNoise ?? _fallbackGlow;
            _fbmNoise = CinematicVFX.FBMNoise ?? _fallbackGlow;
            _energyGradient = CinematicVFX.HorizontalEnergyGradient ?? CinematicVFX.HorizontalBlackCore ?? _fallbackGlow;
            _sparklyNoise = CinematicVFX.SparklyNoise ?? _fallbackGlow;
        }
        
        #endregion
        
        #region Color Palettes
        
        /// <summary>
        /// Gets the color palette for a theme.
        /// IMPORTANT: For additive blending, we use bright luminous colors only!
        /// Dark colors create "black blob" effects with additive blending.
        /// </summary>
        public static (Color hot, Color mid, Color cool, Color accent) GetThemePalette(string theme)
        {
            // All colors must be bright/luminous for additive blending to work properly!
            // "Cool" colors are now soft pastels, not darks
            return theme?.ToLower() switch
            {
                "fate" => (
                    new Color(255, 255, 255),      // hot: white
                    new Color(255, 120, 180),       // mid: bright pink
                    new Color(180, 100, 200),       // cool: soft purple (NOT dark!)
                    new Color(255, 200, 220)        // accent: soft pink
                ),
                "lacampanella" => (
                    new Color(255, 255, 220),       // hot: bright warm white
                    new Color(255, 160, 60),        // mid: bright orange
                    new Color(255, 120, 80),        // cool: warm orange (NOT dark!)
                    new Color(255, 220, 120)        // accent: gold
                ),
                "eroica" => (
                    new Color(255, 255, 255),       // hot: white
                    new Color(255, 200, 100),       // mid: gold
                    new Color(255, 120, 120),       // cool: soft red (NOT dark!)
                    new Color(255, 180, 200)        // accent: sakura pink
                ),
                "swanlake" => (
                    new Color(255, 255, 255),       // hot: pure white
                    new Color(200, 230, 255),       // mid: ice blue
                    new Color(180, 200, 255),       // cool: soft blue (NOT dark!)
                    new Color(255, 220, 255)        // accent: soft magenta
                ),
                "moonlightsonata" => (
                    new Color(240, 220, 255),       // hot: lavender white
                    new Color(180, 140, 255),       // mid: bright purple
                    new Color(150, 180, 255),       // cool: soft blue-purple (NOT dark!)
                    new Color(200, 200, 255)        // accent: periwinkle
                ),
                "enigma" or "enigmavariations" => (
                    new Color(200, 255, 220),       // hot: bright green-white
                    new Color(120, 255, 180),       // mid: bright green
                    new Color(160, 200, 255),       // cool: soft cyan (NOT dark!)
                    new Color(180, 255, 200)        // accent: mint
                ),
                _ => (
                    new Color(255, 255, 255),       // hot: white
                    new Color(220, 200, 255),       // mid: soft purple
                    new Color(200, 180, 255),       // cool: lavender (NOT dark!)
                    new Color(240, 220, 255)        // accent: pale violet
                )
            };
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Updates a melee swing trail.
        /// </summary>
        public static void UpdateTrail(Player player, float bladeLength, 
            Color primaryColor, Color secondaryColor, float width = 45f, string theme = null)
        {
            if (Main.dedServ) return;
            
            // Find or create trail
            TrailData trail = null;
            foreach (var t in _trails)
            {
                if (t.OwnerId == player.whoAmI && t.IsActive && !t.IsFading)
                {
                    trail = t;
                    break;
                }
            }
            
            if (trail == null)
            {
                if (_trails.Count >= MAX_TRAILS)
                {
                    // Remove oldest
                    for (int i = 0; i < _trails.Count; i++)
                    {
                        if (_trails[i].IsFading || !_trails[i].IsActive)
                        {
                            _trails.RemoveAt(i);
                            break;
                        }
                    }
                    if (_trails.Count >= MAX_TRAILS)
                        _trails.RemoveAt(0);
                }
                
                var palette = GetThemePalette(theme);
                trail = new TrailData
                {
                    OwnerId = player.whoAmI,
                    ColorHot = palette.hot,
                    ColorCool = palette.cool,
                    ColorAccent = palette.accent,
                    BaseWidth = width,
                    IsActive = true,
                    IsFading = false,
                    FadeProgress = 0f,
                    Theme = theme,
                    ScrollOffset = Main.rand.NextFloat(10f)
                };
                _trails.Add(trail);
            }
            
            // Calculate blade tip position
            float swingRotation = player.itemRotation + (player.direction < 0 ? MathHelper.Pi : 0);
            Vector2 tipPosition = player.Center + new Vector2(MathF.Cos(swingRotation), MathF.Sin(swingRotation)) * bladeLength;
            
            // Add point if far enough from last
            if (trail.Points.Count == 0 || 
                Vector2.DistanceSquared(trail.Points[^1].Position, tipPosition) > POINT_SPACING * POINT_SPACING)
            {
                // Age existing points
                for (int i = 0; i < trail.Points.Count; i++)
                {
                    var p = trail.Points[i];
                    p.Age = Math.Min(1f, p.Age + 0.04f);
                    trail.Points[i] = p;
                }
                
                // Add new point
                trail.Points.Add(new TrailPoint
                {
                    Position = tipPosition,
                    Rotation = swingRotation,
                    Age = 0f
                });
                
                // Limit points
                while (trail.Points.Count > MAX_POINTS)
                    trail.Points.RemoveAt(0);
            }
            
            // Update scroll
            trail.ScrollOffset += 0.08f;
        }
        
        /// <summary>
        /// Ends the trail for a player (begins fade-out).
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
        /// Spawns an instant arc effect.
        /// </summary>
        public static void SpawnArc(Player player, float startAngle, float endAngle, 
            float bladeLength, Color primaryColor, Color secondaryColor, float width, int pointCount, string theme)
        {
            if (Main.dedServ) return;
            
            if (_trails.Count >= MAX_TRAILS)
            {
                _trails.RemoveAt(0);
            }
            
            var palette = GetThemePalette(theme);
            var trail = new TrailData
            {
                OwnerId = player.whoAmI,
                ColorHot = palette.hot,
                ColorCool = palette.cool,
                ColorAccent = palette.accent,
                BaseWidth = width,
                IsActive = true,
                IsFading = true, // Immediately start fading
                FadeProgress = 0f,
                Theme = theme,
                ScrollOffset = Main.rand.NextFloat(10f)
            };
            
            // Generate arc points
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                float angle = MathHelper.Lerp(startAngle, endAngle, t);
                Vector2 pos = player.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * bladeLength;
                
                trail.Points.Add(new TrailPoint
                {
                    Position = pos,
                    Rotation = angle,
                    Age = t * 0.3f // Pre-age for taper
                });
            }
            
            _trails.Add(trail);
        }
        
        #endregion
        
        #region Update
        
        public static void Update()
        {
            float dt = 1f / 60f;
            
            for (int i = _trails.Count - 1; i >= 0; i--)
            {
                var trail = _trails[i];
                
                // Update scroll
                trail.ScrollOffset += 0.05f;
                
                if (trail.IsFading)
                {
                    trail.FadeProgress += dt * 3f; // ~0.33 second fade
                    
                    // Age all points faster during fade
                    for (int j = 0; j < trail.Points.Count; j++)
                    {
                        var p = trail.Points[j];
                        p.Age = Math.Min(1f, p.Age + 0.06f);
                        trail.Points[j] = p;
                    }
                    
                    // Remove old points
                    while (trail.Points.Count > 0 && trail.Points[0].Age >= 1f)
                        trail.Points.RemoveAt(0);
                    
                    if (trail.FadeProgress >= 1f || trail.Points.Count == 0)
                    {
                        _trails.RemoveAt(i);
                        continue;
                    }
                }
                else
                {
                    // Age points normally
                    for (int j = 0; j < trail.Points.Count; j++)
                    {
                        var p = trail.Points[j];
                        p.Age = Math.Min(1f, p.Age + 0.02f);
                        trail.Points[j] = p;
                    }
                    
                    // Remove fully aged points
                    while (trail.Points.Count > 0 && trail.Points[0].Age >= 1f)
                        trail.Points.RemoveAt(0);
                }
            }
        }
        
        #endregion
        
        #region Render
        
        private static bool _spriteBatchStarted = false;
        
        public static void Render()
        {
            if (_trails.Count == 0) return;
            
            EnsureTextures();
            
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Start the SpriteBatch in additive mode first (we're drawing after DrawProjectiles)
            _spriteBatchStarted = true;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var trail in _trails)
            {
                if (trail.Points.Count < 2) continue;
                
                float fadeAlpha = trail.IsFading ? 1f - trail.FadeProgress : 1f;
                fadeAlpha = Math.Clamp(fadeAlpha, 0f, 1f);
                
                // Draw each layer
                DrawLayer(spriteBatch, trail, _nebulaWisp, 
                    widthMult: 2.2f, alphaMult: 0.5f * fadeAlpha, 
                    colorStart: trail.ColorCool, colorEnd: trail.ColorHot,
                    scrollSpeed: 0.3f);
                
                DrawLayer(spriteBatch, trail, _fbmNoise, 
                    widthMult: 1.6f, alphaMult: 0.4f * fadeAlpha, 
                    colorStart: trail.ColorCool, colorEnd: trail.ColorHot,
                    scrollSpeed: -0.2f);
                
                DrawLayer(spriteBatch, trail, _energyGradient, 
                    widthMult: 0.9f, alphaMult: 0.7f * fadeAlpha, 
                    colorStart: trail.ColorHot, colorEnd: Color.White,
                    scrollSpeed: 0f);
                
                // Sparkle layer (sparse)
                if (Main.rand.NextFloat() < 0.5f)
                {
                    DrawLayer(spriteBatch, trail, _sparklyNoise, 
                        widthMult: 1.5f, alphaMult: 0.35f * fadeAlpha, 
                        colorStart: trail.ColorAccent, colorEnd: Color.White,
                        scrollSpeed: 0.5f);
                }
                
                // Bright core
                DrawLayer(spriteBatch, trail, _fallbackGlow, 
                    widthMult: 0.4f, alphaMult: 0.9f * fadeAlpha, 
                    colorStart: Color.White, colorEnd: Color.White,
                    scrollSpeed: 0f);
            }
            
            // End the SpriteBatch we started in Render()
            if (_spriteBatchStarted)
            {
                spriteBatch.End();
                _spriteBatchStarted = false;
            }
        }
        
        private static void DrawLayer(SpriteBatch spriteBatch, TrailData trail, Texture2D texture,
            float widthMult, float alphaMult, Color colorStart, Color colorEnd, float scrollSpeed)
        {
            if (texture == null || texture.IsDisposed) return;
            
            // SpriteBatch is already started in Render(), just draw
            // No End/Begin cycling - we stay in additive mode throughout
            
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            float time = Main.GameUpdateCount * 0.016f;
            
            for (int i = 0; i < trail.Points.Count; i++)
            {
                var point = trail.Points[i];
                float progress = (float)i / Math.Max(1, trail.Points.Count - 1);
                
                // QuadraticBump: 0 at edges, 1 in middle
                float bumpFactor = progress * (4f - progress * 4f);
                bumpFactor = Math.Clamp(bumpFactor, 0f, 1f);
                
                // Age-based fade
                float ageFade = 1f - point.Age;
                
                // Final alpha
                float alpha = alphaMult * bumpFactor * ageFade;
                if (alpha <= 0.01f) continue;
                
                // Size
                float size = trail.BaseWidth * widthMult * bumpFactor * ageFade / texture.Width;
                if (size <= 0.01f) continue;
                
                // Color interpolation
                Color color = Color.Lerp(colorStart, colorEnd, bumpFactor);
                color *= alpha;
                // Remove alpha channel for additive blending
                color = color with { A = 0 };
                
                // Rotation with scroll
                float rotation = point.Rotation + time * scrollSpeed + trail.ScrollOffset * scrollSpeed;
                
                // Screen position
                Vector2 screenPos = point.Position - Main.screenPosition;
                
                // Draw texture
                spriteBatch.Draw(texture, screenPos, null, color, rotation, origin, size, SpriteEffects.None, 0f);
            }
            // No End/Begin cycling here - Render() handles the final End()
        }
        
        public static void Clear()
        {
            _trails.Clear();
        }
        
        public static void Unload()
        {
            _trails.Clear();
            _fallbackGlow?.Dispose();
            _fallbackGlow = null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// ModSystem integration for SpriteBatchNebulaTrail.
    /// </summary>
    public class SpriteBatchNebulaTrailSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawNebulaTrails;
        }
        
        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawNebulaTrails;
            SpriteBatchNebulaTrail.Unload();
        }
        
        public override void PostUpdatePlayers()
        {
            SpriteBatchNebulaTrail.Update();
        }
        
        private void DrawNebulaTrails(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            
            try
            {
                SpriteBatchNebulaTrail.Render();
            }
            catch (Exception ex)
            {
                Mod?.Logger?.Warn($"SpriteBatchNebulaTrail render error: {ex.Message}");
            }
        }
        
        public override void OnWorldUnload()
        {
            SpriteBatchNebulaTrail.Clear();
        }
    }
}
