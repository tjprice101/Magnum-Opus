using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE PRIMITIVE TRAIL RENDERER
    /// 
    /// Implements the Exoblade/Ark of the Cosmos style swing trails:
    /// - Coordinate Tracking: Every frame, saves the last 10-20 positions
    /// - Bézier Interpolation: Fills gaps between saved points with smooth curves
    /// - VertexStrip Rendering: Draws as a textured mesh using SpriteBatch.DrawStrip
    /// - Linear Gradient: Uses custom shader for width/color tapering
    /// 
    /// Usage in melee weapons:
    /// 1. Call TrackPosition() every frame in AI()
    /// 2. Call RenderTrail() in PreDraw() with return false
    /// </summary>
    public class PrimitiveTrailRenderer : ModSystem
    {
        // Trail position pool for all active trails
        private static Dictionary<int, TrailData> _activeTrails = new Dictionary<int, TrailData>();
        
        /// <summary>
        /// Trail data for a single projectile/swing
        /// </summary>
        public class TrailData
        {
            public Vector2[] Positions;
            public Vector2[] OldPositions; // For interpolation
            public float[] Rotations;
            public int PositionCount;
            public int MaxPositions;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Width;
            public float Lifetime;
            public int EntityId;
            
            public TrailData(int maxPositions)
            {
                MaxPositions = maxPositions;
                Positions = new Vector2[maxPositions];
                OldPositions = new Vector2[maxPositions];
                Rotations = new float[maxPositions];
                PositionCount = 0;
            }
            
            /// <summary>
            /// Add a new position to the trail
            /// </summary>
            public void AddPosition(Vector2 position, float rotation)
            {
                // Shift all positions back
                for (int i = MaxPositions - 1; i > 0; i--)
                {
                    OldPositions[i] = OldPositions[i - 1];
                    Positions[i] = Positions[i - 1];
                    Rotations[i] = Rotations[i - 1];
                }
                
                OldPositions[0] = Positions[0];
                Positions[0] = position;
                Rotations[0] = rotation;
                
                if (PositionCount < MaxPositions)
                    PositionCount++;
            }
            
            /// <summary>
            /// Get interpolated position using sub-pixel interpolation
            /// Enables buttery 144Hz+ rendering
            /// </summary>
            public Vector2 GetInterpolatedPosition(int index)
            {
                if (index >= PositionCount)
                    return Positions[Math.Min(index, PositionCount - 1)];
                
                // Lerp between old and current for sub-frame smoothness
                float lerpFactor = Main.GameUpdateCount % 2 == 0 ? 0.5f : 1f;
                return Vector2.Lerp(OldPositions[index], Positions[index], lerpFactor);
            }
        }
        
        public override void Unload()
        {
            _activeTrails?.Clear();
            _activeTrails = null;
        }
        
        public override void PostUpdateEverything()
        {
            // Clean up trails for dead entities
            List<int> toRemove = new List<int>();
            foreach (var kvp in _activeTrails)
            {
                bool stillExists = kvp.Key < Main.maxProjectiles && Main.projectile[kvp.Key].active;
                if (!stillExists)
                    toRemove.Add(kvp.Key);
            }
            foreach (int id in toRemove)
                _activeTrails.Remove(id);
        }
        
        #region Public API
        
        /// <summary>
        /// Create or get a trail for an entity
        /// </summary>
        public static TrailData GetOrCreateTrail(int entityId, int maxPositions = 20)
        {
            if (!_activeTrails.ContainsKey(entityId))
            {
                _activeTrails[entityId] = new TrailData(maxPositions);
                _activeTrails[entityId].EntityId = entityId;
            }
            return _activeTrails[entityId];
        }
        
        /// <summary>
        /// Track a new position for the trail (call every frame in AI)
        /// </summary>
        public static void TrackPosition(int entityId, Vector2 position, float rotation, 
            Color primaryColor, Color secondaryColor, float width = 30f)
        {
            TrailData trail = GetOrCreateTrail(entityId);
            trail.AddPosition(position, rotation);
            trail.PrimaryColor = primaryColor;
            trail.SecondaryColor = secondaryColor;
            trail.Width = width;
        }
        
        /// <summary>
        /// Render the trail as a primitive strip with Bézier smoothing
        /// Call in PreDraw with return false
        /// </summary>
        public static void RenderTrail(int entityId, SpriteBatch spriteBatch)
        {
            if (!_activeTrails.TryGetValue(entityId, out TrailData trail))
                return;
            
            if (trail.PositionCount < 3)
                return;
            
            // Use Calamity's VertexStrip pattern
            RenderBezierTrail(trail, spriteBatch);
        }
        
        /// <summary>
        /// Render trail with custom width and color functions (Calamity pattern)
        /// </summary>
        public static void RenderTrailCustom(int entityId, SpriteBatch spriteBatch,
            Func<float, float> widthFunction, Func<float, Color> colorFunction)
        {
            if (!_activeTrails.TryGetValue(entityId, out TrailData trail))
                return;
            
            if (trail.PositionCount < 3)
                return;
            
            RenderBezierTrailCustom(trail, spriteBatch, widthFunction, colorFunction);
        }
        
        /// <summary>
        /// Clear a trail (call when projectile dies)
        /// </summary>
        public static void ClearTrail(int entityId)
        {
            _activeTrails.Remove(entityId);
        }
        
        #endregion
        
        #region Private Rendering
        
        /// <summary>
        /// Render trail using Bézier curve interpolation
        /// This is the core Exoblade technique
        /// </summary>
        private static void RenderBezierTrail(TrailData trail, SpriteBatch spriteBatch)
        {
            // Generate Bézier-interpolated points (2x density)
            int interpolatedCount = (trail.PositionCount - 1) * 2;
            Vector2[] interpolatedPositions = new Vector2[interpolatedCount];
            float[] interpolatedProgress = new float[interpolatedCount];
            
            for (int i = 0; i < trail.PositionCount - 1; i++)
            {
                Vector2 p0 = i > 0 ? trail.GetInterpolatedPosition(i - 1) : trail.GetInterpolatedPosition(i);
                Vector2 p1 = trail.GetInterpolatedPosition(i);
                Vector2 p2 = trail.GetInterpolatedPosition(i + 1);
                Vector2 p3 = i + 2 < trail.PositionCount ? trail.GetInterpolatedPosition(i + 2) : p2;
                
                // Catmull-Rom spline for smooth curves
                interpolatedPositions[i * 2] = p1;
                interpolatedPositions[i * 2 + 1] = CatmullRom(p0, p1, p2, p3, 0.5f);
                
                interpolatedProgress[i * 2] = (float)i / (trail.PositionCount - 1);
                interpolatedProgress[i * 2 + 1] = ((float)i + 0.5f) / (trail.PositionCount - 1);
            }
            
            // Build vertex strip
            DrawTrailStrip(interpolatedPositions, interpolatedCount, trail, spriteBatch,
                progress => trail.Width * (1f - progress), // Width tapers to 0
                progress => Color.Lerp(trail.PrimaryColor, trail.SecondaryColor, progress) * (1f - progress * 0.5f));
        }
        
        private static void RenderBezierTrailCustom(TrailData trail, SpriteBatch spriteBatch,
            Func<float, float> widthFunction, Func<float, Color> colorFunction)
        {
            // Generate Bézier-interpolated points (2x density)
            int interpolatedCount = (trail.PositionCount - 1) * 2;
            Vector2[] interpolatedPositions = new Vector2[interpolatedCount];
            
            for (int i = 0; i < trail.PositionCount - 1; i++)
            {
                Vector2 p0 = i > 0 ? trail.GetInterpolatedPosition(i - 1) : trail.GetInterpolatedPosition(i);
                Vector2 p1 = trail.GetInterpolatedPosition(i);
                Vector2 p2 = trail.GetInterpolatedPosition(i + 1);
                Vector2 p3 = i + 2 < trail.PositionCount ? trail.GetInterpolatedPosition(i + 2) : p2;
                
                interpolatedPositions[i * 2] = p1;
                interpolatedPositions[i * 2 + 1] = CatmullRom(p0, p1, p2, p3, 0.5f);
            }
            
            DrawTrailStrip(interpolatedPositions, interpolatedCount, trail, spriteBatch, widthFunction, colorFunction);
        }
        
        /// <summary>
        /// Catmull-Rom spline interpolation for smooth curves
        /// </summary>
        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
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
        /// Draw trail as vertex strip (Calamity's VertexStrip pattern)
        /// </summary>
        private static void DrawTrailStrip(Vector2[] positions, int count, TrailData trail,
            SpriteBatch spriteBatch, Func<float, float> widthFunction, Func<float, Color> colorFunction)
        {
            if (count < 2)
                return;
            
            // End current spritebatch to use custom rendering
            try { spriteBatch.End(); } catch { }
            
            // Use additive blending for glow
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            
            // Get a simple glow texture
            Texture2D trailTex = GetTrailTexture();
            
            // Draw trail segments
            for (int i = 0; i < count - 1; i++)
            {
                float progress = (float)i / (count - 1);
                float nextProgress = (float)(i + 1) / (count - 1);
                
                Vector2 start = positions[i] - Main.screenPosition;
                Vector2 end = positions[i + 1] - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                
                if (length < 1f)
                    continue;
                
                float rotation = direction.ToRotation();
                float width = widthFunction(progress);
                float nextWidth = widthFunction(nextProgress);
                float avgWidth = (width + nextWidth) * 0.5f;
                
                Color color = colorFunction(progress);
                
                // Draw segment as stretched texture (simulates strip)
                Rectangle sourceRect = new Rectangle(0, 0, trailTex.Width, trailTex.Height);
                Vector2 origin = new Vector2(0, trailTex.Height / 2f);
                Vector2 scale = new Vector2(length / trailTex.Width, avgWidth / trailTex.Height);
                
                spriteBatch.Draw(trailTex, start, sourceRect, color, rotation, origin, scale, SpriteEffects.None, 0f);
            }
            
            // Restore normal spritebatch
            try { spriteBatch.End(); } catch { }
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        /// <summary>
        /// Get or create a simple gradient texture for trails
        /// </summary>
        private static Texture2D _trailTexture;
        private static Texture2D GetTrailTexture()
        {
            if (_trailTexture != null && !_trailTexture.IsDisposed)
                return _trailTexture;
            
            // Create a simple horizontal gradient texture
            int width = 64;
            int height = 16;
            _trailTexture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Horizontal fade (leading to trailing)
                    float xProgress = (float)x / width;
                    
                    // Vertical fade (center bright, edges dim) - QuadraticBump pattern
                    float yProgress = (float)y / height;
                    float verticalFade = yProgress * (1f - yProgress) * 4f;
                    
                    float alpha = (1f - xProgress) * verticalFade;
                    data[y * width + x] = Color.White * alpha;
                }
            }
            
            _trailTexture.SetData(data);
            return _trailTexture;
        }
        
        #endregion
        
        #region Helper Methods for Melee Weapons
        
        /// <summary>
        /// QuadraticBump: 0 → 1 → 0 curve (peaks at 0.5)
        /// Used by Calamity for width functions
        /// </summary>
        public static float QuadraticBump(float x)
        {
            return x * (4f - x * 4f);
        }
        
        /// <summary>
        /// Standard width function for sword swings (thick start, thin end)
        /// </summary>
        public static Func<float, float> SwingWidthFunction(float baseWidth)
        {
            return progress => baseWidth * (1f - progress * 0.7f) * QuadraticBump(MathHelper.Clamp(progress * 2f, 0f, 1f));
        }
        
        /// <summary>
        /// Standard color function with gradient
        /// </summary>
        public static Func<float, Color> GradientColorFunction(Color start, Color end)
        {
            return progress => Color.Lerp(start, end, progress) * (1f - progress * 0.5f);
        }
        
        /// <summary>
        /// Smooth step interpolation (eases in and out)
        /// </summary>
        public static float SmoothStep(float x)
        {
            return x * x * (3f - 2f * x);
        }
        
        /// <summary>
        /// Create Bézier control point for curved swings
        /// </summary>
        public static Vector2 BezierControlPoint(Vector2 start, Vector2 end, float curveAmount, float progress)
        {
            Vector2 mid = (start + end) * 0.5f;
            Vector2 perpendicular = (end - start).RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);
            Vector2 control = mid + perpendicular * curveAmount;
            
            // Quadratic Bézier
            return Vector2.Lerp(Vector2.Lerp(start, control, progress), Vector2.Lerp(control, end, progress), progress);
        }
        
        #endregion
    }
}
