using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE BEAM VERTEX STRIP MANAGER
    /// 
    /// Manages the state of a beam's vertex strip with:
    /// - Frame-independent tick-based timing
    /// - Sub-pixel interpolation for 144Hz+ smoothness
    /// - Position history caching for smooth trails
    /// - Automatic vertex buffer management
    /// 
    /// This class is instanced per-beam for projectiles that need persistent beam state.
    /// 
    /// Usage:
    ///   var beamStrip = new BeamVertexStrip(maxPoints: 30, tickLifetime: 15);
    ///   
    ///   // In AI():
    ///   beamStrip.AddPoint(position, velocity);
    ///   beamStrip.Update();
    ///   
    ///   // In PreDraw():
    ///   beamStrip.Render(settings);
    /// </summary>
    public class BeamVertexStrip
    {
        #region Point Data
        
        /// <summary>
        /// Represents a single point on the beam with timing data.
        /// </summary>
        public struct BeamPoint
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Width;
            public Color Color;
            public uint SpawnTick;
            public int TickAge;
            
            public bool IsValid => Position != Vector2.Zero;
            public float NormalizedAge(int maxAge) => Math.Clamp((float)TickAge / maxAge, 0f, 1f);
        }
        
        private List<BeamPoint> _points = new List<BeamPoint>();
        private int _maxPoints;
        private int _tickLifetime;
        private uint _lastUpdateTick;
        
        // Cached smoothed positions for rendering
        private Vector2[] _smoothedPositions;
        private float[] _smoothedWidths;
        private Color[] _smoothedColors;
        private bool _needsRebuild = true;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Number of active points in the strip.
        /// </summary>
        public int PointCount => _points.Count;
        
        /// <summary>
        /// Whether the strip has enough points to render.
        /// </summary>
        public bool CanRender => _points.Count >= 2;
        
        /// <summary>
        /// The most recent point added to the strip.
        /// </summary>
        public BeamPoint HeadPoint => _points.Count > 0 ? _points[_points.Count - 1] : default;
        
        /// <summary>
        /// The oldest point in the strip.
        /// </summary>
        public BeamPoint TailPoint => _points.Count > 0 ? _points[0] : default;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates a new beam vertex strip manager.
        /// </summary>
        /// <param name="maxPoints">Maximum number of points to store</param>
        /// <param name="tickLifetime">How many game ticks a point lives before expiring</param>
        public BeamVertexStrip(int maxPoints = 25, int tickLifetime = 12)
        {
            _maxPoints = maxPoints;
            _tickLifetime = tickLifetime;
            _lastUpdateTick = Main.GameUpdateCount;
        }
        
        #endregion
        
        #region Point Management
        
        /// <summary>
        /// Adds a new point to the head of the beam strip.
        /// </summary>
        public void AddPoint(Vector2 position, Vector2 velocity, float width = 1f, Color? color = null)
        {
            var point = new BeamPoint
            {
                Position = position,
                Velocity = velocity,
                Width = width,
                Color = color ?? Color.White,
                SpawnTick = Main.GameUpdateCount,
                TickAge = 0
            };
            
            _points.Add(point);
            _needsRebuild = true;
            
            // Cap at max points
            while (_points.Count > _maxPoints)
            {
                _points.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Adds a point using a projectile's current interpolated position.
        /// </summary>
        public void AddPointFromProjectile(Projectile projectile, float width = 1f, Color? color = null)
        {
            Vector2 interpolatedPos = InterpolatedRenderer.GetInterpolatedCenter(projectile);
            AddPoint(interpolatedPos, projectile.velocity, width, color);
        }
        
        /// <summary>
        /// Updates the strip - ages points and removes expired ones.
        /// Call this once per game tick (in AI).
        /// </summary>
        public void Update()
        {
            // Calculate ticks since last update
            uint currentTick = Main.GameUpdateCount;
            int ticksPassed = (int)(currentTick - _lastUpdateTick);
            _lastUpdateTick = currentTick;
            
            if (ticksPassed <= 0) return;
            
            // Age all points
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var point = _points[i];
                point.TickAge += ticksPassed;
                _points[i] = point;
                
                // Remove expired points
                if (point.TickAge > _tickLifetime)
                {
                    _points.RemoveAt(i);
                    _needsRebuild = true;
                }
            }
        }
        
        /// <summary>
        /// Clears all points from the strip.
        /// </summary>
        public void Clear()
        {
            _points.Clear();
            _needsRebuild = true;
        }
        
        #endregion
        
        #region Smoothing & Interpolation
        
        /// <summary>
        /// Rebuilds the smoothed position cache using Catmull-Rom interpolation.
        /// </summary>
        private void RebuildSmoothedPositions(int outputCount)
        {
            if (_points.Count < 2)
            {
                _smoothedPositions = null;
                _smoothedWidths = null;
                _smoothedColors = null;
                return;
            }
            
            _smoothedPositions = new Vector2[outputCount];
            _smoothedWidths = new float[outputCount];
            _smoothedColors = new Color[outputCount];
            
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1) * (_points.Count - 1);
                int segment = (int)t;
                float segmentT = t - segment;
                
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(_points.Count - 1, segment + 1);
                int p3 = Math.Min(_points.Count - 1, segment + 2);
                
                // Interpolate position with Catmull-Rom
                _smoothedPositions[i] = CatmullRom(
                    _points[p0].Position, _points[p1].Position,
                    _points[p2].Position, _points[p3].Position, segmentT);
                
                // Linearly interpolate width and color
                _smoothedWidths[i] = MathHelper.Lerp(_points[p1].Width, _points[p2].Width, segmentT);
                _smoothedColors[i] = Color.Lerp(_points[p1].Color, _points[p2].Color, segmentT);
            }
            
            _needsRebuild = false;
        }
        
        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders the beam strip using MassiveBeamSystem.
        /// </summary>
        public void Render(MassiveBeamSystem.BeamSettings settings)
        {
            if (!CanRender) return;
            
            // Rebuild smoothed positions if needed
            if (_needsRebuild || _smoothedPositions == null)
            {
                RebuildSmoothedPositions(settings.SegmentCount);
            }
            
            if (_smoothedPositions == null || _smoothedPositions.Length < 2)
                return;
            
            // Get UV scroll offset
            float uvScroll = Main.GlobalTimeWrappedHourly * settings.TextureScrollSpeed;
            
            // Render using the positions with age-based fading
            RenderWithAgeFading(settings, uvScroll);
        }
        
        /// <summary>
        /// Renders with age-based opacity fading.
        /// </summary>
        private void RenderWithAgeFading(MassiveBeamSystem.BeamSettings settings, float uvScroll)
        {
            if (_smoothedPositions == null) return;
            
            // Create modified color function that incorporates age
            MassiveBeamSystem.BeamColorFunction ageAwareColor = (ratio, scroll) =>
            {
                Color baseColor = settings.ColorFunc(ratio, scroll);
                
                // Calculate age at this ratio
                int pointIndex = (int)(ratio * (_points.Count - 1));
                pointIndex = Math.Clamp(pointIndex, 0, _points.Count - 1);
                
                float ageFade = 1f - _points[pointIndex].NormalizedAge(_tickLifetime);
                
                return baseColor * ageFade;
            };
            
            // Render using MassiveBeamSystem's multi-pass rendering
            var modifiedSettings = settings;
            modifiedSettings.ColorFunc = ageAwareColor;
            
            // Multi-pass render
            RenderPass(_smoothedPositions, modifiedSettings, uvScroll, settings.BloomMultiplier, 0.2f, false);
            RenderPass(_smoothedPositions, modifiedSettings, uvScroll, 1.5f, 0.45f, false);
            RenderPass(_smoothedPositions, modifiedSettings, uvScroll, 1f, 0.85f, false);
            RenderPass(_smoothedPositions, modifiedSettings, uvScroll, settings.CoreMultiplier, 1f, true);
        }
        
        private void RenderPass(Vector2[] positions, MassiveBeamSystem.BeamSettings settings, 
            float uvScroll, float widthMult, float opacityMult, bool forceWhite)
        {
            // End SpriteBatch for primitive rendering
            try { Main.spriteBatch.End(); } catch { }
            
            try
            {
                int vertexCount = positions.Length * 2;
                int triangleCount = (positions.Length - 1) * 2;
                
                var vertices = new VertexPositionColorTexture[vertexCount];
                var indices = new short[triangleCount * 3];
                
                for (int i = 0; i < positions.Length; i++)
                {
                    float ratio = (float)i / (positions.Length - 1);
                    
                    float width = settings.WidthFunc(ratio) * widthMult;
                    if (_smoothedWidths != null && i < _smoothedWidths.Length)
                        width *= _smoothedWidths[i];
                    
                    Color color;
                    if (forceWhite)
                    {
                        float fade = MathF.Sin(ratio * MathHelper.Pi);
                        color = Color.White * fade * opacityMult;
                    }
                    else
                    {
                        color = settings.ColorFunc(ratio, uvScroll) * opacityMult;
                        if (_smoothedColors != null && i < _smoothedColors.Length)
                            color = Color.Lerp(color, _smoothedColors[i], 0.5f);
                    }
                    
                    color = color.WithoutAlpha();
                    
                    // Calculate perpendicular
                    Vector2 direction;
                    if (i == 0)
                        direction = (positions[1] - positions[0]).SafeNormalize(Vector2.UnitY);
                    else if (i == positions.Length - 1)
                        direction = (positions[i] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
                    else
                        direction = (positions[i + 1] - positions[i - 1]).SafeNormalize(Vector2.UnitY);
                    
                    Vector2 perp = new Vector2(-direction.Y, direction.X);
                    Vector2 screenPos = positions[i] - Main.screenPosition;
                    
                    float u = ratio + uvScroll;
                    
                    vertices[i * 2] = new VertexPositionColorTexture(
                        new Vector3(screenPos + perp * width * 0.5f, 0),
                        color,
                        new Vector2(u, 0));
                    
                    vertices[i * 2 + 1] = new VertexPositionColorTexture(
                        new Vector3(screenPos - perp * width * 0.5f, 0),
                        color,
                        new Vector2(u, 1));
                }
                
                int idx = 0;
                for (int i = 0; i < positions.Length - 1; i++)
                {
                    int baseV = i * 2;
                    indices[idx++] = (short)baseV;
                    indices[idx++] = (short)(baseV + 1);
                    indices[idx++] = (short)(baseV + 2);
                    indices[idx++] = (short)(baseV + 1);
                    indices[idx++] = (short)(baseV + 3);
                    indices[idx++] = (short)(baseV + 2);
                }
                
                // Draw
                var device = Main.instance.GraphicsDevice;
                var prevBlend = device.BlendState;
                
                device.BlendState = BlendState.Additive;
                device.RasterizerState = RasterizerState.CullNone;
                device.DepthStencilState = DepthStencilState.None;
                
                var effect = new BasicEffect(device)
                {
                    VertexColorEnabled = true,
                    View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up),
                    Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1),
                    World = Matrix.Identity
                };
                
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    vertices, 0, vertexCount, indices, 0, triangleCount);
                
                device.BlendState = prevBlend;
                effect.Dispose();
            }
            finally
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        #endregion
        
        #region Helpers
        
        /// <summary>
        /// Gets all current positions as an array for external use.
        /// </summary>
        public Vector2[] GetPositionsArray()
        {
            Vector2[] result = new Vector2[_points.Count];
            for (int i = 0; i < _points.Count; i++)
            {
                result[i] = _points[i].Position;
            }
            return result;
        }
        
        /// <summary>
        /// Gets the interpolated head position using partial ticks.
        /// </summary>
        public Vector2 GetInterpolatedHeadPosition()
        {
            if (_points.Count < 2)
                return HeadPoint.Position;
            
            var head = _points[_points.Count - 1];
            var prev = _points[_points.Count - 2];
            
            return Vector2.Lerp(prev.Position, head.Position, InterpolatedRenderer.PartialTicks);
        }
        
        #endregion
    }
}
