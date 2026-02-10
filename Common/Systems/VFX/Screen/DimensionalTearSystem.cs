using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// DIMENSIONAL TEAR / STENCIL MASKING SYSTEM
    /// 
    /// Implements the "Dimensional Tear" melee concept:
    /// - Sword swing leaves a "rip" in reality that reveals an alternate dimension
    /// - Uses render target masking (simulated stencil buffer)
    /// - Tear shows cosmic/void texture bleeding through
    /// - Edges have energy crackling effects
    /// 
    /// Technical Approach (No Actual Stencil Buffer):
    /// - Draw tear shape to a render target
    /// - Use that as mask for "other dimension" texture
    /// - Overlay edge glow and particle effects
    /// 
    /// Usage:
    ///   DimensionalTearSystem.CreateTear(startPos, endPos, width, style, colors);
    /// </summary>
    public class DimensionalTearSystem : ModSystem
    {
        private static List<DimensionalTear> _activeTears = new();
        private static RenderTarget2D _tearMaskTarget;
        private static RenderTarget2D _dimensionTarget;
        private static Texture2D _cosmicTexture;
        private static Texture2D _voidTexture;
        private static Texture2D _edgeGlowTexture;
        
        private const int MaxActiveTears = 3;
        private const int MaskTargetSize = 512;
        
        #region Tear Styles
        
        public enum TearStyle
        {
            Cosmic,         // Star field visible through tear
            Void,           // Dark void with purple edges
            Infernal,       // Fire and brimstone
            Prismatic,      // Rainbow chromatic energy
            Fate            // Dark pink/crimson cosmic
        }
        
        #endregion
        
        #region Tear Data
        
        private class DimensionalTear
        {
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public float Width;
            public float CurrentWidth;
            public TearStyle Style;
            public Color EdgeColor;
            public Color CoreColor;
            public int Timer;
            public int Lifetime;
            public float TextureOffset;
            public List<EdgeCrackle> EdgeEffects;
            public bool IsComplete => Timer >= Lifetime;
            
            public float Progress => (float)Timer / Lifetime;
        }
        
        private struct EdgeCrackle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public int Timer;
            public int Lifetime;
        }
        
        #endregion
        
        #region Initialization
        
        public override void Load()
        {
            if (Main.dedServ) return;
            
            Main.OnResolutionChanged += OnResolutionChanged;
        }
        
        public override void Unload()
        {
            Main.OnResolutionChanged -= OnResolutionChanged;
            _activeTears?.Clear();
            
            // Cache references and null immediately (safe on any thread)
            var tearMask = _tearMaskTarget;
            var dimension = _dimensionTarget;
            var cosmic = _cosmicTexture;
            var voidTex = _voidTexture;
            var edgeGlow = _edgeGlowTexture;
            
            _tearMaskTarget = null;
            _dimensionTarget = null;
            _cosmicTexture = null;
            _voidTexture = null;
            _edgeGlowTexture = null;
            
            // Queue texture disposal on main thread to avoid ThreadStateException
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    tearMask?.Dispose();
                    dimension?.Dispose();
                    cosmic?.Dispose();
                    voidTex?.Dispose();
                    edgeGlow?.Dispose();
                }
                catch { }
            });
        }
        
        private void OnResolutionChanged(Vector2 newSize)
        {
            _tearMaskTarget?.Dispose();
            _dimensionTarget?.Dispose();
            _tearMaskTarget = null;
            _dimensionTarget = null;
        }
        
        public override void PostUpdateEverything()
        {
            for (int i = _activeTears.Count - 1; i >= 0; i--)
            {
                UpdateTear(_activeTears[i]);
                
                if (_activeTears[i].IsComplete)
                    _activeTears.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a dimensional tear along a line (like a sword slash).
        /// </summary>
        public static void CreateTear(
            Vector2 startPosition,
            Vector2 endPosition,
            float width = 40f,
            TearStyle style = TearStyle.Cosmic,
            int lifetime = 60)
        {
            if (_activeTears.Count >= MaxActiveTears) return;
            
            var (edgeColor, coreColor) = GetStyleColors(style);
            
            var tear = new DimensionalTear
            {
                StartPosition = startPosition,
                EndPosition = endPosition,
                Width = width,
                CurrentWidth = 0f,
                Style = style,
                EdgeColor = edgeColor,
                CoreColor = coreColor,
                Timer = 0,
                Lifetime = lifetime,
                TextureOffset = 0f,
                EdgeEffects = new List<EdgeCrackle>()
            };
            
            _activeTears.Add(tear);
        }
        
        /// <summary>
        /// Creates a tear from a melee swing arc.
        /// </summary>
        public static void CreateSwingTear(
            Vector2 center,
            float radius,
            float startAngle,
            float endAngle,
            TearStyle style = TearStyle.Cosmic,
            int lifetime = 45)
        {
            // Convert arc to line approximation
            float midAngle = (startAngle + endAngle) / 2f;
            Vector2 start = center + startAngle.ToRotationVector2() * radius;
            Vector2 end = center + endAngle.ToRotationVector2() * radius;
            
            CreateTear(start, end, radius * 0.3f, style, lifetime);
        }
        
        /// <summary>
        /// Creates a Fate-themed reality tear with cosmic effects.
        /// </summary>
        public static void CreateFateTear(Vector2 start, Vector2 end, int lifetime = 50)
        {
            CreateTear(start, end, 50f, TearStyle.Fate, lifetime);
        }
        
        #endregion
        
        #region Update Logic
        
        private static void UpdateTear(DimensionalTear tear)
        {
            tear.Timer++;
            tear.TextureOffset += 0.02f;
            
            // Width animation: Quickly open, slowly close
            float progress = tear.Progress;
            if (progress < 0.15f)
            {
                // Opening phase
                tear.CurrentWidth = MathHelper.SmoothStep(0f, tear.Width, progress / 0.15f);
            }
            else if (progress < 0.7f)
            {
                // Stable phase
                tear.CurrentWidth = tear.Width;
            }
            else
            {
                // Closing phase
                float closeProgress = (progress - 0.7f) / 0.3f;
                tear.CurrentWidth = MathHelper.SmoothStep(tear.Width, 0f, closeProgress);
            }
            
            // Spawn edge crackle effects
            if (tear.Timer % 3 == 0 && tear.CurrentWidth > 0)
            {
                SpawnEdgeCrackle(tear);
            }
            
            // Update existing crackles
            for (int i = tear.EdgeEffects.Count - 1; i >= 0; i--)
            {
                var crackle = tear.EdgeEffects[i];
                crackle.Timer++;
                crackle.Position += crackle.Velocity;
                crackle.Velocity *= 0.95f;
                crackle.Scale *= 0.96f;
                tear.EdgeEffects[i] = crackle;
                
                if (crackle.Timer >= crackle.Lifetime || crackle.Scale < 0.05f)
                    tear.EdgeEffects.RemoveAt(i);
            }
        }
        
        private static void SpawnEdgeCrackle(DimensionalTear tear)
        {
            Vector2 tearDir = tear.EndPosition - tear.StartPosition;
            float tearLength = tearDir.Length();
            if (tearLength < 1f) return;
            
            tearDir /= tearLength;
            Vector2 perpendicular = new Vector2(-tearDir.Y, tearDir.X);
            
            // Random position along tear
            float t = Main.rand.NextFloat();
            Vector2 basePos = Vector2.Lerp(tear.StartPosition, tear.EndPosition, t);
            
            // On either edge
            float side = Main.rand.NextBool() ? 1f : -1f;
            Vector2 edgePos = basePos + perpendicular * (tear.CurrentWidth * 0.5f * side);
            
            tear.EdgeEffects.Add(new EdgeCrackle
            {
                Position = edgePos,
                Velocity = perpendicular * side * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                Scale = Main.rand.NextFloat(0.2f, 0.5f),
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                Timer = 0,
                Lifetime = Main.rand.Next(15, 30)
            });
        }
        
        private static (Color edge, Color core) GetStyleColors(TearStyle style)
        {
            return style switch
            {
                TearStyle.Cosmic => (new Color(150, 200, 255), new Color(10, 20, 40)),
                TearStyle.Void => (new Color(140, 60, 200), new Color(20, 10, 30)),
                TearStyle.Infernal => (new Color(255, 140, 40), new Color(40, 10, 10)),
                TearStyle.Prismatic => (Color.White, Color.Black),
                TearStyle.Fate => (new Color(255, 100, 150), new Color(40, 10, 30)),
                _ => (Color.White, Color.Black)
            };
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active dimensional tears.
        /// </summary>
        public static void RenderAll(SpriteBatch spriteBatch)
        {
            if (_activeTears.Count == 0) return;
            
            EnsureTextures();
            EnsureRenderTargets();
            
            foreach (var tear in _activeTears)
            {
                RenderTear(spriteBatch, tear);
            }
        }
        
        private static void RenderTear(SpriteBatch spriteBatch, DimensionalTear tear)
        {
            if (tear.CurrentWidth < 0.5f) return;
            
            Vector2 tearCenter = (tear.StartPosition + tear.EndPosition) / 2f - Main.screenPosition;
            Vector2 tearDir = tear.EndPosition - tear.StartPosition;
            float tearLength = tearDir.Length();
            float tearRotation = tearDir.ToRotation();
            
            // Get the dimension texture based on style
            Texture2D dimensionTex = tear.Style == TearStyle.Void ? _voidTexture : _cosmicTexture;
            
            try { spriteBatch.End(); } catch { }
            
            // Draw the "other dimension" visible through the tear
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Calculate UV offset for scrolling dimension texture (ensure positive modulo)
            int srcX = (int)(tear.TextureOffset * 100f) % dimensionTex.Width;
            int srcY = (int)(tear.TextureOffset * 50f) % dimensionTex.Height;
            if (srcX < 0) srcX += dimensionTex.Width;
            if (srcY < 0) srcY += dimensionTex.Height;
            Rectangle sourceRect = new Rectangle(
                srcX,
                srcY,
                dimensionTex.Width,
                dimensionTex.Height
            );
            
            // Draw dimension texture stretched across tear shape
            Vector2 tearSize = new Vector2(tearLength, tear.CurrentWidth);
            Rectangle destRect = new Rectangle(
                (int)(tearCenter.X - tearLength / 2f),
                (int)(tearCenter.Y - tear.CurrentWidth / 2f),
                (int)tearLength,
                (int)tear.CurrentWidth
            );
            
            // Draw core dimension
            spriteBatch.Draw(
                dimensionTex,
                tearCenter,
                sourceRect,
                tear.CoreColor * 0.8f,
                tearRotation,
                new Vector2(dimensionTex.Width / 2f, dimensionTex.Height / 2f),
                new Vector2(tearLength / dimensionTex.Width, tear.CurrentWidth / dimensionTex.Height),
                SpriteEffects.None,
                0f
            );
            
            try { spriteBatch.End(); } catch { }
            
            // Draw edge glow
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Edge bloom layers
            for (int layer = 0; layer < 3; layer++)
            {
                float layerScale = 1f + layer * 0.5f;
                float layerAlpha = 0.6f / (layer + 1);
                
                // Draw glow stretched along tear
                spriteBatch.Draw(
                    _edgeGlowTexture,
                    tearCenter,
                    null,
                    tear.EdgeColor * layerAlpha,
                    tearRotation,
                    new Vector2(_edgeGlowTexture.Width / 2f, _edgeGlowTexture.Height / 2f),
                    new Vector2(tearLength / _edgeGlowTexture.Width * layerScale, 
                                (tear.CurrentWidth + 10f * layer) / _edgeGlowTexture.Height),
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Draw edge crackle particles
            foreach (var crackle in tear.EdgeEffects)
            {
                spriteBatch.Draw(
                    _edgeGlowTexture,
                    crackle.Position - Main.screenPosition,
                    null,
                    tear.EdgeColor * 0.7f,
                    crackle.Rotation,
                    new Vector2(_edgeGlowTexture.Width / 2f, _edgeGlowTexture.Height / 2f),
                    crackle.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // For prismatic style, add color shifting
            if (tear.Style == TearStyle.Prismatic)
            {
                float hue = (Main.GameUpdateCount * 0.02f + tear.TextureOffset) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.7f);
                
                spriteBatch.Draw(
                    _edgeGlowTexture,
                    tearCenter,
                    null,
                    rainbowColor * 0.4f,
                    tearRotation,
                    new Vector2(_edgeGlowTexture.Width / 2f, _edgeGlowTexture.Height / 2f),
                    new Vector2(tearLength / _edgeGlowTexture.Width * 1.2f, 
                                (tear.CurrentWidth + 20f) / _edgeGlowTexture.Height),
                    SpriteEffects.None,
                    0f
                );
            }
            
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        #endregion
        
        #region Texture/Target Generation
        
        private static void EnsureTextures()
        {
            if (_cosmicTexture == null || _cosmicTexture.IsDisposed)
                _cosmicTexture = GenerateCosmicTexture(64, 64);
            
            if (_voidTexture == null || _voidTexture.IsDisposed)
                _voidTexture = GenerateVoidTexture(64, 64);
            
            if (_edgeGlowTexture == null || _edgeGlowTexture.IsDisposed)
                _edgeGlowTexture = GenerateEdgeGlowTexture(64, 16);
        }
        
        private static void EnsureRenderTargets()
        {
            if (_tearMaskTarget == null || _tearMaskTarget.IsDisposed)
            {
                _tearMaskTarget = new RenderTarget2D(
                    Main.graphics.GraphicsDevice,
                    MaskTargetSize, MaskTargetSize,
                    false, SurfaceFormat.Color, DepthFormat.None);
            }
            
            if (_dimensionTarget == null || _dimensionTarget.IsDisposed)
            {
                _dimensionTarget = new RenderTarget2D(
                    Main.graphics.GraphicsDevice,
                    MaskTargetSize, MaskTargetSize,
                    false, SurfaceFormat.Color, DepthFormat.None);
            }
        }
        
        /// <summary>
        /// Generates a starfield/cosmic texture.
        /// </summary>
        private static Texture2D GenerateCosmicTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            // Dark base with scattered stars
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = new Color(5, 10, 20);
            }
            
            // Add stars
            Random rand = new Random(12345);
            int starCount = width * height / 20;
            for (int i = 0; i < starCount; i++)
            {
                int x = rand.Next(width);
                int y = rand.Next(height);
                float brightness = (float)rand.NextDouble();
                
                Color starColor = Color.Lerp(new Color(100, 150, 255), Color.White, brightness);
                data[y * width + x] = starColor * (0.5f + brightness * 0.5f);
                
                // Add small glow around bright stars
                if (brightness > 0.7f)
                {
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        for (int oy = -1; oy <= 1; oy++)
                        {
                            int nx = (x + ox + width) % width;
                            int ny = (y + oy + height) % height;
                            if (data[ny * width + nx].R < 50)
                                data[ny * width + nx] = new Color(30, 40, 80);
                        }
                    }
                }
            }
            
            // Add nebula clouds
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = PerlinNoise2D(x * 0.1f, y * 0.1f);
                    if (noise > 0.3f)
                    {
                        float intensity = (noise - 0.3f) / 0.7f;
                        Color nebula = Color.Lerp(new Color(80, 20, 120), new Color(150, 50, 200), intensity);
                        data[y * width + x] = Color.Lerp(data[y * width + x], nebula, intensity * 0.3f);
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a dark void texture.
        /// </summary>
        private static Texture2D GenerateVoidTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noise = PerlinNoise2D(x * 0.15f, y * 0.15f);
                    
                    // Dark purple void
                    int r = (int)(15 + noise * 25);
                    int g = (int)(5 + noise * 15);
                    int b = (int)(30 + noise * 40);
                    
                    data[y * width + x] = new Color(r, g, b);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates an edge glow texture.
        /// </summary>
        private static Texture2D GenerateEdgeGlowTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            float centerY = height / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Horizontal gradient (for stretching along tear)
                    float hGrad = 1f - MathF.Pow(MathF.Abs((x / (float)width) - 0.5f) * 2f, 2f);
                    
                    // Vertical gradient (edge glow shape)
                    float vDist = MathF.Abs(y - centerY) / centerY;
                    float vGrad = 1f - vDist * vDist;
                    
                    float alpha = hGrad * vGrad;
                    data[y * width + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Simple 2D Perlin noise approximation.
        /// </summary>
        private static float PerlinNoise2D(float x, float y)
        {
            int xi = (int)MathF.Floor(x);
            int yi = (int)MathF.Floor(y);
            float xf = x - xi;
            float yf = y - yi;
            
            float u = xf * xf * (3f - 2f * xf);
            float v = yf * yf * (3f - 2f * yf);
            
            float n00 = DotGridGradient(xi, yi, x, y);
            float n01 = DotGridGradient(xi, yi + 1, x, y);
            float n10 = DotGridGradient(xi + 1, yi, x, y);
            float n11 = DotGridGradient(xi + 1, yi + 1, x, y);
            
            float nx0 = MathHelper.Lerp(n00, n10, u);
            float nx1 = MathHelper.Lerp(n01, n11, u);
            
            return (MathHelper.Lerp(nx0, nx1, v) + 1f) / 2f;
        }
        
        private static float DotGridGradient(int ix, int iy, float x, float y)
        {
            int hash = (ix * 374761393 + iy * 668265263) ^ (ix * 1274126177);
            hash = (hash >> 13) ^ hash;
            
            float angle = (hash % 360) * MathHelper.Pi / 180f;
            float gx = MathF.Cos(angle);
            float gy = MathF.Sin(angle);
            
            float dx = x - ix;
            float dy = y - iy;
            
            return dx * gx + dy * gy;
        }
        
        #endregion
    }
}
