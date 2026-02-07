using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// GOD RAYS / VOLUMETRIC LIGHT SIMULATION SYSTEM
    /// 
    /// Implements Universe Splitter-style god rays using screen-space primitives:
    /// - Origin Point: Center of explosion/light source
    /// - Vertex Fan: Triangles fanning out from center
    /// - Jitter: Ray length randomized per frame
    /// - Gradient Shader: Brightest at source, 0% alpha at tip
    /// 
    /// Technical Implementation:
    /// - TriangleFan primitive rendering
    /// - Time-seeded random jitter for organic feel
    /// - Multi-pass for bloom effect
    /// - Additive blending for "light bleeding"
    /// 
    /// Usage:
    ///   GodRaySystem.CreateBurst(position, color, rayCount: 16, radius: 200f);
    /// </summary>
    public class GodRaySystem : ModSystem
    {
        private static List<GodRayBurst> _activeBursts = new();
        private static Texture2D _gradientTexture;
        private static BasicEffect _basicEffect;
        
        private const int MaxActiveBursts = 20;
        
        #region God Ray Data
        
        private class GodRayBurst
        {
            public Vector2 Position;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public int RayCount;
            public float BaseRadius;
            public float[] RayLengths;
            public float[] RayAngles;
            public float[] RayWidths;
            public int Timer;
            public int MaxLifetime;
            public float RotationSpeed;
            public uint RandomSeed;
            public GodRayStyle Style;
            
            public bool IsExpired => Timer >= MaxLifetime;
            public float Progress => (float)Timer / MaxLifetime;
        }
        
        public enum GodRayStyle
        {
            Explosion,      // Rays shoot outward then fade
            Sustained,      // Rays persist with jitter
            Pulsing,        // Rays pulse in and out
            Spiral          // Rays rotate while extending
        }
        
        #endregion
        
        #region Initialization
        
        public override void Load()
        {
            if (Main.dedServ) return;
            
            Main.QueueMainThreadAction(() =>
            {
                _basicEffect = new BasicEffect(Main.graphics.GraphicsDevice)
                {
                    VertexColorEnabled = true,
                    TextureEnabled = false
                };
            });
        }
        
        public override void Unload()
        {
            _activeBursts?.Clear();
            
            // Cache references and null immediately (safe on any thread)
            var gradient = _gradientTexture;
            var effect = _basicEffect;
            _gradientTexture = null;
            _basicEffect = null;
            
            // Queue disposal on main thread to avoid ThreadStateException
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    gradient?.Dispose();
                    effect?.Dispose();
                }
                catch { }
            });
        }
        
        public override void PostUpdateEverything()
        {
            for (int i = _activeBursts.Count - 1; i >= 0; i--)
            {
                UpdateBurst(_activeBursts[i]);
                
                if (_activeBursts[i].IsExpired)
                    _activeBursts.RemoveAt(i);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a god ray burst effect.
        /// </summary>
        public static void CreateBurst(
            Vector2 position,
            Color primaryColor,
            int rayCount = 16,
            float radius = 150f,
            int duration = 45,
            GodRayStyle style = GodRayStyle.Explosion,
            Color? secondaryColor = null)
        {
            if (_activeBursts.Count >= MaxActiveBursts)
                _activeBursts.RemoveAt(0);
            
            var burst = new GodRayBurst
            {
                Position = position,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor ?? Color.Lerp(primaryColor, Color.White, 0.5f),
                RayCount = rayCount,
                BaseRadius = radius,
                Timer = 0,
                MaxLifetime = duration,
                Style = style,
                RotationSpeed = style == GodRayStyle.Spiral ? 0.03f : 0f,
                RandomSeed = (uint)Main.rand.Next()
            };
            
            // Initialize ray properties
            burst.RayLengths = new float[rayCount];
            burst.RayAngles = new float[rayCount];
            burst.RayWidths = new float[rayCount];
            
            for (int i = 0; i < rayCount; i++)
            {
                burst.RayAngles[i] = MathHelper.TwoPi * i / rayCount + Main.rand.NextFloat(-0.1f, 0.1f);
                burst.RayLengths[i] = radius * Main.rand.NextFloat(0.7f, 1.2f);
                burst.RayWidths[i] = Main.rand.NextFloat(3f, 8f);
            }
            
            _activeBursts.Add(burst);
        }
        
        /// <summary>
        /// Creates a sustained light source with god rays.
        /// </summary>
        public static void CreateLightSource(
            Vector2 position,
            Color color,
            int rayCount = 24,
            float radius = 100f,
            int duration = 120)
        {
            CreateBurst(position, color, rayCount, radius, duration, GodRayStyle.Sustained);
        }
        
        /// <summary>
        /// Creates a Universe Splitter-style explosive god ray effect.
        /// </summary>
        public static void CreateUniverseSplitterBurst(
            Vector2 position,
            Color primaryColor,
            Color secondaryColor,
            int duration = 60)
        {
            CreateBurst(position, primaryColor, 32, 300f, duration, GodRayStyle.Explosion, secondaryColor);
            
            // Also add a secondary spiral layer
            CreateBurst(position, secondaryColor, 16, 200f, duration, GodRayStyle.Spiral, primaryColor);
        }
        
        #endregion
        
        #region Update
        
        private static void UpdateBurst(GodRayBurst burst)
        {
            burst.Timer++;
            
            // Jitter ray lengths based on time and random seed
            float time = Main.GlobalTimeWrappedHourly + burst.RandomSeed * 0.001f;
            
            for (int i = 0; i < burst.RayCount; i++)
            {
                // Time-based jitter for organic feel
                float jitter = MathF.Sin(time * 5f + i * 1.7f) * 0.1f + 
                               MathF.Sin(time * 8f + i * 2.3f) * 0.05f;
                
                burst.RayLengths[i] = burst.BaseRadius * (0.7f + jitter + Main.rand.NextFloat() * 0.05f);
                
                // Rotate in spiral mode
                if (burst.Style == GodRayStyle.Spiral)
                {
                    burst.RayAngles[i] += burst.RotationSpeed;
                }
            }
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active god ray effects.
        /// Call after tiles are drawn for proper depth.
        /// </summary>
        public static void RenderAll(SpriteBatch spriteBatch)
        {
            if (_activeBursts.Count == 0) return;
            
            foreach (var burst in _activeBursts)
            {
                RenderBurst(spriteBatch, burst);
            }
        }
        
        private static void RenderBurst(SpriteBatch spriteBatch, GodRayBurst burst)
        {
            float progress = burst.Progress;
            float alpha = GetAlphaForStyle(burst.Style, progress);
            
            if (alpha <= 0.01f) return;
            
            // End current batch for custom rendering
            spriteBatch.End();
            
            // === PASS 1: BLOOM LAYER (additive, wide, dim) ===
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            DrawRays(spriteBatch, burst, 2.0f, alpha * 0.3f, burst.PrimaryColor);
            
            spriteBatch.End();
            
            // === PASS 2: MAIN RAYS ===
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            DrawRays(spriteBatch, burst, 1.0f, alpha * 0.7f, burst.SecondaryColor);
            
            spriteBatch.End();
            
            // === PASS 3: CORE (thin, bright, white) ===
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            DrawRays(spriteBatch, burst, 0.4f, alpha * 0.9f, Color.White);
            
            // Draw central glow
            DrawCentralGlow(spriteBatch, burst, alpha);
            
            spriteBatch.End();
            
            // Restore alpha blend
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private static void DrawRays(SpriteBatch spriteBatch, GodRayBurst burst, float widthMult, float alpha, Color color)
        {
            EnsureGradientTexture();
            
            Vector2 screenPos = burst.Position - Main.screenPosition;
            
            for (int i = 0; i < burst.RayCount; i++)
            {
                float angle = burst.RayAngles[i];
                float length = burst.RayLengths[i] * GetLengthMultForStyle(burst.Style, burst.Progress);
                float width = burst.RayWidths[i] * widthMult;
                
                Vector2 direction = angle.ToRotationVector2();
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                
                // Draw gradient ray as textured quad
                Vector2 tip = screenPos + direction * length;
                Vector2 baseLeft = screenPos + perpendicular * (width * 0.5f);
                Vector2 baseRight = screenPos - perpendicular * (width * 0.5f);
                
                // Simple triangle (fan-like)
                DrawGradientTriangle(spriteBatch, screenPos, tip, baseLeft, baseRight, 
                    color * alpha, color * 0f, _gradientTexture);
            }
        }
        
        private static void DrawGradientTriangle(SpriteBatch spriteBatch, Vector2 center, Vector2 tip, 
            Vector2 left, Vector2 right, Color centerColor, Color tipColor, Texture2D texture)
        {
            // Approximate with a stretched sprite
            Vector2 direction = tip - center;
            float length = direction.Length();
            float rotation = direction.ToRotation();
            
            spriteBatch.Draw(
                texture,
                center,
                null,
                centerColor,
                rotation,
                new Vector2(0, texture.Height / 2f),
                new Vector2(length / texture.Width, (left - right).Length() / texture.Height),
                SpriteEffects.None,
                0f
            );
        }
        
        private static void DrawCentralGlow(SpriteBatch spriteBatch, GodRayBurst burst, float alpha)
        {
            Vector2 screenPos = burst.Position - Main.screenPosition;
            float glowSize = 50f * (1f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 10f));
            
            // Multiple concentric glows
            for (int i = 0; i < 3; i++)
            {
                float layerScale = 1f + i * 0.5f;
                float layerAlpha = alpha * (1f - i * 0.3f);
                Color layerColor = Color.Lerp(Color.White, burst.PrimaryColor, i * 0.4f);
                
                spriteBatch.Draw(
                    _gradientTexture,
                    screenPos,
                    null,
                    layerColor * layerAlpha,
                    0f,
                    new Vector2(_gradientTexture.Width / 2f, _gradientTexture.Height / 2f),
                    glowSize * layerScale / _gradientTexture.Width,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        private static float GetAlphaForStyle(GodRayStyle style, float progress)
        {
            switch (style)
            {
                case GodRayStyle.Explosion:
                    // Fast attack, slow decay
                    if (progress < 0.2f)
                        return progress / 0.2f;
                    else
                        return 1f - ((progress - 0.2f) / 0.8f);
                    
                case GodRayStyle.Sustained:
                    // Fade in, sustain, fade out
                    if (progress < 0.1f)
                        return progress / 0.1f;
                    else if (progress > 0.9f)
                        return (1f - progress) / 0.1f;
                    else
                        return 1f;
                    
                case GodRayStyle.Pulsing:
                    return 0.5f + 0.5f * MathF.Sin(progress * MathHelper.TwoPi * 3f);
                    
                case GodRayStyle.Spiral:
                    return 1f - progress;
                    
                default:
                    return 1f - progress;
            }
        }
        
        private static float GetLengthMultForStyle(GodRayStyle style, float progress)
        {
            switch (style)
            {
                case GodRayStyle.Explosion:
                    // Snap out quickly, then stay
                    return MathF.Min(1f, progress * 4f);
                    
                case GodRayStyle.Pulsing:
                    return 0.7f + 0.3f * MathF.Sin(progress * MathHelper.TwoPi * 2f);
                    
                case GodRayStyle.Spiral:
                    return 0.5f + 0.5f * progress;
                    
                default:
                    return 1f;
            }
        }
        
        #endregion
        
        #region Texture Generation
        
        private static void EnsureGradientTexture()
        {
            if (_gradientTexture == null || _gradientTexture.IsDisposed)
            {
                _gradientTexture = CreateGradientTexture(64, 16);
            }
        }
        
        /// <summary>
        /// Creates a horizontal gradient texture (bright left, transparent right).
        /// </summary>
        private static Texture2D CreateGradientTexture(int width, int height)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            for (int y = 0; y < height; y++)
            {
                // Vertical falloff for ray width
                float yNorm = (float)y / (height - 1);
                float verticalFade = 1f - MathF.Abs(yNorm - 0.5f) * 2f;
                verticalFade = MathF.Pow(verticalFade, 0.7f);
                
                for (int x = 0; x < width; x++)
                {
                    // Horizontal fade from bright to transparent
                    float xNorm = (float)x / (width - 1);
                    float horizontalFade = 1f - xNorm;
                    horizontalFade = MathF.Pow(horizontalFade, 1.5f);
                    
                    float alpha = verticalFade * horizontalFade;
                    data[y * width + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
    }
}
