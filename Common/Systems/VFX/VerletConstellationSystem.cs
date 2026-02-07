using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// VERLET INTEGRATION CONSTELLATION SYSTEM
    /// 
    /// Implements Ark of the Cosmos-style constellation lines with physics:
    /// - Verlet Integration: Lines "bow" and "lag" behind moving anchors
    /// - Spark Nodes: Glowing points at vertices that sparkle
    /// - Bloom Strips: Multi-pass rendering for glow effect
    /// - Physics Solver: Position - OldPosition for natural movement
    /// 
    /// Usage:
    ///   var constellation = new ConstellationLine(startAnchor, endAnchor, segments: 12);
    ///   constellation.Update();
    ///   constellation.Render(spriteBatch, color);
    /// </summary>
    public class VerletConstellationSystem : ModSystem
    {
        private static List<ConstellationLine> _activeConstellations = new();
        private static Texture2D _nodeTexture;
        private static Texture2D _lineTexture;
        private static Texture2D _sparkleTexture;
        
        private const int MaxConstellations = 30;
        
        public override void Load()
        {
            if (Main.dedServ) return;
        }
        
        public override void Unload()
        {
            _activeConstellations?.Clear();
            _nodeTexture = null;
            _lineTexture = null;
            _sparkleTexture = null;
        }
        
        public override void PostUpdateEverything()
        {
            // Update all active constellations
            for (int i = _activeConstellations.Count - 1; i >= 0; i--)
            {
                _activeConstellations[i].Update();
                
                if (_activeConstellations[i].IsExpired)
                    _activeConstellations.RemoveAt(i);
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Creates a constellation line between two world positions.
        /// </summary>
        public static ConstellationLine CreateLine(
            Vector2 startAnchor, 
            Vector2 endAnchor, 
            int segments = 12, 
            Color? color = null,
            float stiffness = 0.8f,
            int lifetime = 60)
        {
            if (_activeConstellations.Count >= MaxConstellations)
            {
                // Remove oldest
                _activeConstellations.RemoveAt(0);
            }
            
            var line = new ConstellationLine(startAnchor, endAnchor, segments)
            {
                PrimaryColor = color ?? Color.White,
                Stiffness = stiffness,
                MaxLifetime = lifetime
            };
            
            _activeConstellations.Add(line);
            return line;
        }
        
        /// <summary>
        /// Creates a constellation web connecting multiple points.
        /// </summary>
        public static List<ConstellationLine> CreateWeb(
            Vector2 center,
            Vector2[] points,
            Color? color = null,
            float stiffness = 0.6f,
            int lifetime = 45)
        {
            var lines = new List<ConstellationLine>();
            
            // Connect center to all points
            foreach (var point in points)
            {
                lines.Add(CreateLine(center, point, 8, color, stiffness, lifetime));
            }
            
            // Connect adjacent points
            for (int i = 0; i < points.Length; i++)
            {
                int next = (i + 1) % points.Length;
                lines.Add(CreateLine(points[i], points[next], 6, color, stiffness * 0.8f, lifetime));
            }
            
            return lines;
        }
        
        /// <summary>
        /// Renders all active constellations.
        /// Call this in a ModSystem.PostDrawTiles or similar.
        /// </summary>
        public static void RenderAll(SpriteBatch spriteBatch)
        {
            EnsureTextures();
            
            foreach (var constellation in _activeConstellations)
            {
                constellation.Render(spriteBatch, _nodeTexture, _lineTexture, _sparkleTexture);
            }
        }
        
        #endregion
        
        #region Texture Management
        
        private static void EnsureTextures()
        {
            if (_nodeTexture == null)
            {
                // Create a simple circular node texture procedurally
                _nodeTexture = CreateCircleTexture(16, Color.White);
            }
            
            if (_lineTexture == null)
            {
                // Create a simple 1xN line texture
                _lineTexture = CreateLineTexture(4, 32, Color.White);
            }
            
            if (_sparkleTexture == null)
            {
                // 4-pointed star sparkle
                _sparkleTexture = CreateSparkleTexture(32);
            }
        }
        
        private static Texture2D CreateCircleTexture(int diameter, Color color)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, diameter, diameter);
            var data = new Color[diameter * diameter];
            
            float radius = diameter / 2f;
            Vector2 center = new Vector2(radius, radius);
            
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Math.Max(0, 1f - dist / radius);
                    alpha = MathF.Pow(alpha, 0.5f); // Soft falloff
                    data[y * diameter + x] = color * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private static Texture2D CreateLineTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, width, height);
            var data = new Color[width * height];
            
            float halfWidth = width / 2f;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float xDist = Math.Abs(x - halfWidth) / halfWidth;
                    float alpha = 1f - xDist;
                    alpha = MathF.Pow(alpha, 0.7f);
                    data[y * width + x] = color * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        private static Texture2D CreateSparkleTexture(int size)
        {
            var texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            var data = new Color[size * size];
            
            float center = size / 2f;
            float armLength = size * 0.45f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    
                    // 4-pointed star: horizontal + vertical arms
                    float horizArm = Math.Max(0, 1f - Math.Abs(dy) / 2f) * Math.Max(0, 1f - dist / armLength);
                    float vertArm = Math.Max(0, 1f - Math.Abs(dx) / 2f) * Math.Max(0, 1f - dist / armLength);
                    
                    float alpha = Math.Max(horizArm, vertArm);
                    alpha = MathF.Pow(alpha, 1.5f);
                    
                    data[y * size + x] = Color.White * alpha;
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        #endregion
    }
    
    /// <summary>
    /// A single constellation line with Verlet physics.
    /// </summary>
    public class ConstellationLine
    {
        #region Verlet Point
        
        public struct VerletPoint
        {
            public Vector2 Position;
            public Vector2 OldPosition;
            public bool IsAnchored;
            public float SparklePhase;
            
            /// <summary>
            /// Calculates velocity using Verlet integration: v = pos - oldPos
            /// </summary>
            public Vector2 Velocity => Position - OldPosition;
            
            public void ApplyVerlet(float damping = 0.98f)
            {
                if (IsAnchored) return;
                
                Vector2 velocity = Position - OldPosition;
                OldPosition = Position;
                Position += velocity * damping;
            }
        }
        
        #endregion
        
        public VerletPoint[] Points { get; private set; }
        public int SegmentCount { get; private set; }
        
        public Color PrimaryColor = Color.White;
        public Color SecondaryColor = new Color(100, 150, 255);
        public float Stiffness = 0.8f;
        public float Width = 3f;
        public int MaxLifetime = 60;
        public int Timer = 0;
        
        public Vector2 StartAnchor { get; set; }
        public Vector2 EndAnchor { get; set; }
        
        public bool IsExpired => Timer >= MaxLifetime;
        public float Progress => (float)Timer / MaxLifetime;
        
        private float _restLength;
        
        public ConstellationLine(Vector2 start, Vector2 end, int segments)
        {
            SegmentCount = segments;
            StartAnchor = start;
            EndAnchor = end;
            
            Points = new VerletPoint[segments + 1];
            _restLength = Vector2.Distance(start, end) / segments;
            
            // Initialize points along the line
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, t);
                
                Points[i] = new VerletPoint
                {
                    Position = pos,
                    OldPosition = pos,
                    IsAnchored = (i == 0 || i == segments),
                    SparklePhase = Main.rand.NextFloat() * MathHelper.TwoPi
                };
            }
        }
        
        /// <summary>
        /// Updates the constellation physics.
        /// </summary>
        public void Update()
        {
            Timer++;
            
            // Update anchors
            Points[0].Position = StartAnchor;
            Points[0].OldPosition = StartAnchor;
            Points[SegmentCount].Position = EndAnchor;
            Points[SegmentCount].OldPosition = EndAnchor;
            
            // Apply Verlet integration
            for (int i = 0; i <= SegmentCount; i++)
            {
                Points[i].ApplyVerlet(0.96f);
            }
            
            // Apply gravity (very slight downward)
            for (int i = 1; i < SegmentCount; i++)
            {
                Points[i].Position += new Vector2(0, 0.15f);
            }
            
            // Constraint solving - maintain rest lengths (multiple iterations for stability)
            for (int iter = 0; iter < 3; iter++)
            {
                SolveConstraints();
            }
            
            // Update sparkle phases
            for (int i = 0; i <= SegmentCount; i++)
            {
                Points[i].SparklePhase += 0.15f;
            }
        }
        
        private void SolveConstraints()
        {
            for (int i = 0; i < SegmentCount; i++)
            {
                ref VerletPoint p1 = ref Points[i];
                ref VerletPoint p2 = ref Points[i + 1];
                
                Vector2 diff = p2.Position - p1.Position;
                float currentLength = diff.Length();
                
                if (currentLength < 0.0001f) continue;
                
                float correction = (currentLength - _restLength) / currentLength * Stiffness;
                Vector2 offset = diff * correction * 0.5f;
                
                if (!p1.IsAnchored)
                    p1.Position += offset;
                if (!p2.IsAnchored)
                    p2.Position -= offset;
            }
        }
        
        /// <summary>
        /// Renders the constellation with bloom and sparkle effects.
        /// </summary>
        public void Render(SpriteBatch spriteBatch, Texture2D nodeTex, Texture2D lineTex, Texture2D sparkleTex)
        {
            float fadeAlpha = 1f - EaseInQuad(Progress);
            
            // End current batch and start additive for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === PASS 1: BLOOM LAYER (wider, dimmer) ===
            for (int i = 0; i < SegmentCount; i++)
            {
                DrawSegmentLine(spriteBatch, lineTex, i, Width * 2.5f, PrimaryColor * 0.2f * fadeAlpha);
            }
            
            // === PASS 2: MAIN LINE ===
            for (int i = 0; i < SegmentCount; i++)
            {
                DrawSegmentLine(spriteBatch, lineTex, i, Width, PrimaryColor * 0.7f * fadeAlpha);
            }
            
            // === PASS 3: CORE LINE (bright, thin) ===
            for (int i = 0; i < SegmentCount; i++)
            {
                DrawSegmentLine(spriteBatch, lineTex, i, Width * 0.5f, Color.White * 0.9f * fadeAlpha);
            }
            
            // === PASS 4: NODE SPARKLES ===
            for (int i = 0; i <= SegmentCount; i++)
            {
                float sparkle = 0.5f + 0.5f * MathF.Sin(Points[i].SparklePhase);
                float nodeScale = (0.15f + sparkle * 0.1f) * fadeAlpha;
                
                // Bloom glow
                spriteBatch.Draw(
                    nodeTex,
                    Points[i].Position - Main.screenPosition,
                    null,
                    SecondaryColor * 0.4f * fadeAlpha,
                    0f,
                    new Vector2(nodeTex.Width / 2f, nodeTex.Height / 2f),
                    nodeScale * 2f,
                    SpriteEffects.None,
                    0f
                );
                
                // Core sparkle
                if (sparkleTex != null)
                {
                    float sparkleRot = Main.GlobalTimeWrappedHourly * 2f + Points[i].SparklePhase;
                    spriteBatch.Draw(
                        sparkleTex,
                        Points[i].Position - Main.screenPosition,
                        null,
                        Color.White * sparkle * fadeAlpha,
                        sparkleRot,
                        new Vector2(sparkleTex.Width / 2f, sparkleTex.Height / 2f),
                        nodeScale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
            
            // Restore alpha blend
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private void DrawSegmentLine(SpriteBatch spriteBatch, Texture2D tex, int segmentIndex, float width, Color color)
        {
            Vector2 start = Points[segmentIndex].Position;
            Vector2 end = Points[segmentIndex + 1].Position;
            
            Vector2 diff = end - start;
            float length = diff.Length();
            float rotation = diff.ToRotation();
            
            spriteBatch.Draw(
                tex,
                start - Main.screenPosition,
                null,
                color,
                rotation,
                new Vector2(0, tex.Height / 2f),
                new Vector2(length / tex.Width, width / tex.Height),
                SpriteEffects.None,
                0f
            );
        }
        
        private static float EaseInQuad(float t) => t * t;
    }
}
