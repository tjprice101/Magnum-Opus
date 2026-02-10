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
    /// RIBBON TRAIL SYSTEM - Calamity-Style Dynamic Mesh Generation
    /// 
    /// Creates smooth, flowing trail ribbons by generating triangle strip meshes
    /// from position history. Each vertex stores "progress" (0.0 at tip to 1.0 at tail)
    /// for proper UV mapping to noise/LUT textures.
    /// 
    /// Features:
    /// - Position history tracking (configurable 15-30+ positions)
    /// - Triangle strip generation with proper winding
    /// - Vertex progress for shader UV alignment
    /// - Catmull-Rom smoothing for fluid curves
    /// - Multi-pass rendering support (bloom, main, core)
    /// - Automatic cleanup of completed trails
    /// 
    /// Based on Calamity's primitive mesh system for Ark of the Cosmos.
    /// </summary>
    public class RibbonTrailSystem : ModSystem
    {
        #region Constants
        
        private const int MaxActiveRibbons = 32;
        private const int DefaultPositionHistory = 25;
        private const int MaxPositionHistory = 40;
        
        #endregion
        
        #region Static Instance
        
        private static RibbonTrailSystem _instance;
        public static RibbonTrailSystem Instance => _instance;
        
        #endregion
        
        #region Ribbon Data Structures
        
        /// <summary>
        /// A single point in the ribbon trail with associated metadata.
        /// </summary>
        public struct RibbonPoint
        {
            public Vector2 Position;
            public Vector2 Velocity;        // Used for width calculation
            public float Rotation;
            public float Width;             // Width at this point
            public float TimeCreated;       // For age-based effects
            public bool Valid;
        }
        
        /// <summary>
        /// Complete ribbon trail data with position history and rendering parameters.
        /// </summary>
        public class RibbonData
        {
            // Identification
            public int Id;
            public int OwnerId;             // Entity (player/projectile) this belongs to
            public string Theme;            // For VFXTextureRegistry theme lookup
            
            // Position History (the core of the ribbon)
            public RibbonPoint[] Points;
            public int PointCount;
            public int MaxPoints;
            
            // Appearance
            public Color PrimaryColor;
            public Color SecondaryColor;
            public Color CoreColor;         // White-hot center
            public float BaseWidth;
            public float WidthMultiplier;
            
            // Animation
            public float NoiseOffset;       // For UV advection sync
            public float Age;               // Time since creation
            public float FadeProgress;      // 0 = fully visible, 1 = faded out
            public bool IsFading;
            public bool IsComplete;
            
            // Rendering
            public BlendState BlendMode;
            public Effect CustomShader;
            
            public RibbonData(int maxPoints = DefaultPositionHistory)
            {
                MaxPoints = Math.Min(maxPoints, MaxPositionHistory);
                Points = new RibbonPoint[MaxPoints];
                PointCount = 0;
                BlendMode = BlendState.Additive;
            }
            
            /// <summary>
            /// Adds a new point to the ribbon head, shifting older points back.
            /// </summary>
            public void AddPoint(Vector2 position, Vector2 velocity, float rotation, float width)
            {
                // Shift all points back by one
                for (int i = MaxPoints - 1; i > 0; i--)
                {
                    Points[i] = Points[i - 1];
                }
                
                // Add new point at head
                Points[0] = new RibbonPoint
                {
                    Position = position,
                    Velocity = velocity,
                    Rotation = rotation,
                    Width = width,
                    TimeCreated = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds,
                    Valid = true
                };
                
                PointCount = Math.Min(PointCount + 1, MaxPoints);
            }
            
            /// <summary>
            /// Gets the progress value (0-1) for a point index.
            /// 0 = trail head (newest), 1 = trail tail (oldest)
            /// </summary>
            public float GetProgress(int index)
            {
                if (PointCount <= 1) return 0f;
                return (float)index / (PointCount - 1);
            }
        }
        
        #endregion
        
        #region Internal State
        
        private List<RibbonData> _activeRibbons;
        private int _nextRibbonId;
        
        // Rendering resources
        private BasicEffect _basicEffect;
        private VertexPositionColorTexture[] _vertexBuffer;
        private short[] _indexBuffer;
        private const int MaxVerticesPerRibbon = MaxPositionHistory * 2;
        private const int MaxIndicesPerRibbon = MaxPositionHistory * 6;
        
        // Shader (optional)
        private Effect _ribbonShader;
        private bool _shaderLoaded;
        
        #endregion
        
        #region Lifecycle
        
        public override void Load()
        {
            _instance = this;
            _activeRibbons = new List<RibbonData>(MaxActiveRibbons);
            _nextRibbonId = 0;
            
            Main.QueueMainThreadAction(() =>
            {
                InitializeRenderingResources();
            });
        }
        
        public override void Unload()
        {
            _instance = null;
            _activeRibbons?.Clear();
            
            // Capture references before nulling
            var basicEffect = _basicEffect;
            var ribbonShader = _ribbonShader;
            
            // Null fields immediately
            _basicEffect = null;
            _ribbonShader = null;
            
            // Queue disposal to main thread (graphics resources must be disposed on main thread)
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    basicEffect?.Dispose();
                    ribbonShader?.Dispose();
                }
                catch { /* Ignore disposal errors during unload */ }
            });
        }
        
        private void InitializeRenderingResources()
        {
            if (Main.dedServ) return;
            
            var device = Main.instance.GraphicsDevice;
            
            _basicEffect = new BasicEffect(device)
            {
                VertexColorEnabled = true,
                TextureEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity
            };
            
            // Allocate buffers for all possible ribbons
            _vertexBuffer = new VertexPositionColorTexture[MaxActiveRibbons * MaxVerticesPerRibbon];
            _indexBuffer = new short[MaxActiveRibbons * MaxIndicesPerRibbon];
            
            // Try to load custom shader
            TryLoadShader();
        }
        
        private void TryLoadShader()
        {
            try
            {
                if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/CalamityFireShader"))
                {
                    _ribbonShader = ModContent.Request<Effect>(
                        "MagnumOpus/Assets/Shaders/CalamityFireShader",
                        AssetRequestMode.ImmediateLoad
                    ).Value;
                    _shaderLoaded = _ribbonShader != null;
                }
            }
            catch
            {
                _shaderLoaded = false;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Creates a new ribbon trail and returns its ID for tracking.
        /// </summary>
        public static int CreateRibbon(
            int ownerId,
            Color primaryColor,
            Color secondaryColor,
            float baseWidth = 30f,
            int maxPoints = DefaultPositionHistory,
            string theme = null)
        {
            if (_instance == null) return -1;
            return _instance.CreateRibbonInternal(ownerId, primaryColor, secondaryColor, baseWidth, maxPoints, theme);
        }
        
        /// <summary>
        /// Updates a ribbon with a new position.
        /// Call this every frame for active trails.
        /// </summary>
        public static void UpdateRibbon(int ribbonId, Vector2 position, Vector2 velocity, float rotation, float width = -1f)
        {
            if (_instance == null) return;
            _instance.UpdateRibbonInternal(ribbonId, position, velocity, rotation, width);
        }
        
        /// <summary>
        /// Begins fading out a ribbon. It will be removed when fade completes.
        /// </summary>
        public static void FadeRibbon(int ribbonId)
        {
            if (_instance == null) return;
            _instance.FadeRibbonInternal(ribbonId);
        }
        
        /// <summary>
        /// Immediately removes a ribbon.
        /// </summary>
        public static void DestroyRibbon(int ribbonId)
        {
            if (_instance == null) return;
            _instance.DestroyRibbonInternal(ribbonId);
        }
        
        /// <summary>
        /// Gets a ribbon by ID (for advanced manipulation).
        /// </summary>
        public static RibbonData GetRibbon(int ribbonId)
        {
            if (_instance == null) return null;
            return _instance.GetRibbonInternal(ribbonId);
        }
        
        #endregion
        
        #region Internal Implementation
        
        private int CreateRibbonInternal(int ownerId, Color primaryColor, Color secondaryColor, 
            float baseWidth, int maxPoints, string theme)
        {
            // Check capacity
            if (_activeRibbons.Count >= MaxActiveRibbons)
            {
                // Remove oldest fading ribbon
                for (int i = 0; i < _activeRibbons.Count; i++)
                {
                    if (_activeRibbons[i].IsFading || _activeRibbons[i].IsComplete)
                    {
                        _activeRibbons.RemoveAt(i);
                        break;
                    }
                }
                
                if (_activeRibbons.Count >= MaxActiveRibbons)
                {
                    _activeRibbons.RemoveAt(0);
                }
            }
            
            var ribbon = new RibbonData(maxPoints)
            {
                Id = _nextRibbonId++,
                OwnerId = ownerId,
                Theme = theme ?? "Eroica",
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                CoreColor = Color.Lerp(primaryColor, Color.White, 0.7f),
                BaseWidth = baseWidth,
                WidthMultiplier = 1f,
                NoiseOffset = Main.rand.NextFloat(100f),
                Age = 0f,
                FadeProgress = 0f,
                IsFading = false,
                IsComplete = false
            };
            
            _activeRibbons.Add(ribbon);
            return ribbon.Id;
        }
        
        private void UpdateRibbonInternal(int ribbonId, Vector2 position, Vector2 velocity, float rotation, float width)
        {
            var ribbon = GetRibbonInternal(ribbonId);
            if (ribbon == null || ribbon.IsFading) return;
            
            float pointWidth = width > 0 ? width : ribbon.BaseWidth;
            ribbon.AddPoint(position, velocity, rotation, pointWidth);
            ribbon.Age += 1f / 60f; // Approximate frame time
        }
        
        private void FadeRibbonInternal(int ribbonId)
        {
            var ribbon = GetRibbonInternal(ribbonId);
            if (ribbon == null) return;
            ribbon.IsFading = true;
        }
        
        private void DestroyRibbonInternal(int ribbonId)
        {
            for (int i = 0; i < _activeRibbons.Count; i++)
            {
                if (_activeRibbons[i].Id == ribbonId)
                {
                    _activeRibbons.RemoveAt(i);
                    return;
                }
            }
        }
        
        private RibbonData GetRibbonInternal(int ribbonId)
        {
            foreach (var ribbon in _activeRibbons)
            {
                if (ribbon.Id == ribbonId)
                    return ribbon;
            }
            return null;
        }
        
        #endregion
        
        #region Update Loop
        
        public override void PostUpdateEverything()
        {
            UpdateAllRibbons();
        }
        
        private void UpdateAllRibbons()
        {
            for (int i = _activeRibbons.Count - 1; i >= 0; i--)
            {
                var ribbon = _activeRibbons[i];
                
                if (ribbon.IsFading)
                {
                    ribbon.FadeProgress += 0.05f; // Fade over ~20 frames
                    if (ribbon.FadeProgress >= 1f)
                    {
                        ribbon.IsComplete = true;
                    }
                }
                
                if (ribbon.IsComplete)
                {
                    _activeRibbons.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Rendering
        
        /// <summary>
        /// Renders all active ribbons. Call from a draw hook.
        /// </summary>
        public static void RenderAllRibbons(SpriteBatch spriteBatch)
        {
            if (_instance == null || Main.dedServ) return;
            
            try
            {
                _instance.RenderAllRibbonsInternal(spriteBatch);
            }
            catch (Exception ex)
            {
                // Silently handle render errors to prevent crashes
                // Ribbons failing to render is not critical
            }
        }
        
        private void RenderAllRibbonsInternal(SpriteBatch spriteBatch)
        {
            if (_activeRibbons.Count == 0) return;
            
            var device = Main.instance.GraphicsDevice;
            
            // Check if SpriteBatch is active before ending
            // Use reflection to check internal _begun field, or just skip the End/Begin cycle
            bool endedSpriteBatch = false;
            
            // Save render state
            var oldBlendState = device.BlendState;
            var oldRasterizerState = device.RasterizerState;
            var oldSamplerState = device.SamplerStates[0];
            
            // Set up for additive blending
            device.BlendState = BlendState.Additive;
            device.RasterizerState = RasterizerState.CullNone;
            device.SamplerStates[0] = SamplerState.LinearWrap;
            
            // Render each ribbon
            foreach (var ribbon in _activeRibbons)
            {
                if (ribbon.PointCount < 2) continue;
                RenderRibbon(device, ribbon);
            }
            
            // Restore render state
            device.BlendState = oldBlendState;
            device.RasterizerState = oldRasterizerState;
            device.SamplerStates[0] = oldSamplerState;
        }
        
        /// <summary>
        /// Renders a single ribbon as a triangle strip mesh.
        /// </summary>
        private void RenderRibbon(GraphicsDevice device, RibbonData ribbon)
        {
            if (ribbon.PointCount < 2) return;
            
            // Generate vertices
            int vertexCount = 0;
            int indexCount = 0;
            
            GenerateRibbonMesh(ribbon, ref vertexCount, ref indexCount);
            
            if (vertexCount < 4 || indexCount < 6) return;
            
            // Set up effect
            SetupEffect(ribbon);
            
            // Multi-pass rendering: Bloom → Main → Core
            
            // Pass 1: Bloom (wide, soft glow)
            RenderPass(device, ribbon, vertexCount, indexCount, 0, 2.5f, 0.3f);
            
            // Pass 2: Main trail
            RenderPass(device, ribbon, vertexCount, indexCount, 1, 1.0f, 0.8f);
            
            // Pass 3: Core (bright center)
            RenderPass(device, ribbon, vertexCount, indexCount, 2, 0.4f, 1.0f);
        }
        
        private void GenerateRibbonMesh(RibbonData ribbon, ref int vertexCount, ref int indexCount)
        {
            // Generate triangle strip from position history
            for (int i = 0; i < ribbon.PointCount; i++)
            {
                if (!ribbon.Points[i].Valid) continue;
                
                var point = ribbon.Points[i];
                float progress = ribbon.GetProgress(i);
                
                // Calculate perpendicular direction for width
                Vector2 perpendicular;
                if (i == 0 && ribbon.PointCount > 1)
                {
                    // Use direction to next point
                    Vector2 toNext = ribbon.Points[1].Position - point.Position;
                    if (toNext.LengthSquared() > 0.01f)
                    {
                        toNext.Normalize();
                        perpendicular = new Vector2(-toNext.Y, toNext.X);
                    }
                    else
                    {
                        perpendicular = Vector2.UnitY;
                    }
                }
                else if (i < ribbon.PointCount - 1)
                {
                    // Use average direction
                    Vector2 toPrev = ribbon.Points[i - 1].Position - point.Position;
                    Vector2 toNext = ribbon.Points[i + 1].Position - point.Position;
                    Vector2 avgDir = (toNext - toPrev);
                    if (avgDir.LengthSquared() > 0.01f)
                    {
                        avgDir.Normalize();
                        perpendicular = new Vector2(-avgDir.Y, avgDir.X);
                    }
                    else
                    {
                        perpendicular = Vector2.UnitY;
                    }
                }
                else
                {
                    // Use direction from previous point
                    Vector2 toPrev = point.Position - ribbon.Points[i - 1].Position;
                    if (toPrev.LengthSquared() > 0.01f)
                    {
                        toPrev.Normalize();
                        perpendicular = new Vector2(-toPrev.Y, toPrev.X);
                    }
                    else
                    {
                        perpendicular = Vector2.UnitY;
                    }
                }
                
                // Calculate width with taper
                float widthMultiplier = CalculateWidthMultiplier(progress);
                float width = point.Width * widthMultiplier * ribbon.WidthMultiplier;
                
                // Apply fade
                float fadeMultiplier = 1f - ribbon.FadeProgress;
                
                // Calculate color with gradient
                Color vertexColor = CalculateVertexColor(ribbon, progress, fadeMultiplier);
                
                // Screen-space position
                Vector2 screenPos = point.Position - Main.screenPosition;
                
                // Two vertices per point (top and bottom of ribbon)
                Vector2 offset = perpendicular * width * 0.5f;
                Vector2 topVert = screenPos + offset;
                Vector2 bottomVert = screenPos - offset;
                
                // Top vertex (UV.y = 0)
                _vertexBuffer[vertexCount] = new VertexPositionColorTexture(
                    new Vector3(topVert.X, topVert.Y, 0),
                    vertexColor,
                    new Vector3(progress, 0f, 0f)
                );
                vertexCount++;
                
                // Bottom vertex (UV.y = 1)
                _vertexBuffer[vertexCount] = new VertexPositionColorTexture(
                    new Vector3(bottomVert.X, bottomVert.Y, 0),
                    vertexColor,
                    new Vector3(progress, 1f, 0f)
                );
                vertexCount++;
            }
            
            // Generate indices for triangle strip
            int quadCount = (vertexCount / 2) - 1;
            for (int i = 0; i < quadCount; i++)
            {
                int baseVertex = i * 2;
                
                // Two triangles per quad
                _indexBuffer[indexCount++] = (short)baseVertex;
                _indexBuffer[indexCount++] = (short)(baseVertex + 1);
                _indexBuffer[indexCount++] = (short)(baseVertex + 2);
                
                _indexBuffer[indexCount++] = (short)(baseVertex + 2);
                _indexBuffer[indexCount++] = (short)(baseVertex + 1);
                _indexBuffer[indexCount++] = (short)(baseVertex + 3);
            }
        }
        
        private float CalculateWidthMultiplier(float progress)
        {
            // FargosSoulsDLC-style width profile:
            // Thin at start, full in middle, tapers at end
            float rampUp = VFXUtilities.InverseLerp(0.0f, 0.15f, progress);
            float rampDown = VFXUtilities.InverseLerp(1.0f, 0.7f, progress);
            return rampUp * rampDown;
        }
        
        private Color CalculateVertexColor(RibbonData ribbon, float progress, float fadeMultiplier)
        {
            // Gradient from primary (head) to secondary (tail)
            Color baseColor = Color.Lerp(ribbon.PrimaryColor, ribbon.SecondaryColor, progress);
            
            // Apply fade
            return baseColor * fadeMultiplier;
        }
        
        private void SetupEffect(RibbonData ribbon)
        {
            // Use BasicEffect for now (shader integration later)
            Matrix projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
            _basicEffect.Projection = projection;
            _basicEffect.View = Matrix.Identity;
            _basicEffect.World = Matrix.Identity;
            
            // Try to get theme noise texture
            var noiseTexture = VFXTextureRegistry.GetNoiseForTheme(ribbon.Theme);
            if (noiseTexture != null)
            {
                _basicEffect.TextureEnabled = true;
                _basicEffect.Texture = noiseTexture;
            }
        }
        
        private void RenderPass(GraphicsDevice device, RibbonData ribbon, 
            int vertexCount, int indexCount, int passIndex, float widthScale, float opacity)
        {
            // Modify vertex colors for this pass
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = _vertexBuffer[i];
                Color passColor;
                
                switch (passIndex)
                {
                    case 0: // Bloom pass - wider, softer
                        passColor = ribbon.SecondaryColor * (opacity * 0.4f);
                        vertex.Position.X *= widthScale;
                        vertex.Position.Y *= widthScale;
                        break;
                    case 1: // Main pass
                        passColor = Color.Lerp(ribbon.PrimaryColor, ribbon.SecondaryColor, 
                            vertex.TextureCoordinate.X) * opacity;
                        break;
                    case 2: // Core pass - brighter, narrower
                        passColor = ribbon.CoreColor * (opacity * 1.2f);
                        break;
                    default:
                        passColor = vertex.Color;
                        break;
                }
                
                // Apply fade
                passColor *= (1f - ribbon.FadeProgress);
                _vertexBuffer[i].Color = passColor;
            }
            
            // Draw
            foreach (var pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexBuffer,
                    0,
                    vertexCount,
                    _indexBuffer,
                    0,
                    indexCount / 3
                );
            }
        }
        
        #endregion
        
        #region Catmull-Rom Smoothing
        
        /// <summary>
        /// Applies Catmull-Rom spline interpolation to smooth the ribbon path.
        /// Call this to get intermediate positions between recorded points.
        /// </summary>
        public static Vector2 CatmullRomInterpolate(RibbonData ribbon, float t)
        {
            if (ribbon.PointCount < 4) return ribbon.Points[0].Position;
            
            // Find the four control points around t
            float scaledT = t * (ribbon.PointCount - 1);
            int i1 = Math.Max(0, (int)scaledT);
            int i0 = Math.Max(0, i1 - 1);
            int i2 = Math.Min(ribbon.PointCount - 1, i1 + 1);
            int i3 = Math.Min(ribbon.PointCount - 1, i1 + 2);
            
            float localT = scaledT - i1;
            
            Vector2 p0 = ribbon.Points[i0].Position;
            Vector2 p1 = ribbon.Points[i1].Position;
            Vector2 p2 = ribbon.Points[i2].Position;
            Vector2 p3 = ribbon.Points[i3].Position;
            
            return CatmullRom(p0, p1, p2, p3, localT);
        }
        
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
        
        #endregion
    }
}
