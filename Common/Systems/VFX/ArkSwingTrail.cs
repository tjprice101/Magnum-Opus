using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using ReLogic.Content;

// VFX Texture Registry for proper noise/LUT/mask texture lookups
// This replaces the old CinematicVFX texture references with the centralized system

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ARK OF THE COSMOS-STYLE MELEE SWING TRAIL
    /// 
    /// Renders melee swing trails as continuous triangle strip meshes with:
    /// - UV-mapped noise texture scrolling (not discrete particles!)
    /// - Multi-pass rendering (background fog, main trail, bright core)
    /// - Proper width tapering along the arc
    /// - Custom Trail shader with proper gradient and bloom support
    /// - Additive blending for proper glow accumulation
    /// 
    /// This is the proper way to do swing trails - NOT spawning discrete fog particles.
    /// Now uses the compiled Trail.xnb shader for proper visual effects.
    /// </summary>
    public static class ArkSwingTrail
    {
        #region Trail Data
        
        private static List<SwingTrailData> _activeTrails = new List<SwingTrailData>();
        private const int MaxTrails = 8;
        private const int MaxPointsPerTrail = 32;
        
        // Primary: Custom Trail shader (compiled Trail.xnb)
        private static Effect _trailShader;
        // Fallback: BasicEffect if shader fails to load
        private static BasicEffect _basicEffect;
        private static bool _shaderLoadAttempted = false;
        private static bool _useCustomShader = false;
        
        private static VertexPositionColorTexture[] _vertices;
        private static short[] _indices;
        
        private class SwingTrailData
        {
            public Player Owner;
            public int OwnerIndex;
            public List<SwingPoint> Points;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public Color GlowColor;
            public float BaseWidth;
            public float NoiseScrollOffset;
            public int Timer;
            public int FadeTimer;
            public bool IsFading;
            public string Theme;
            
            public SwingTrailData()
            {
                Points = new List<SwingPoint>(MaxPointsPerTrail);
            }
        }
        
        private struct SwingPoint
        {
            public Vector2 Position;
            public Vector2 Direction; // Perpendicular direction for width
            public float Rotation;
            public uint FrameCreated;
        }
        
        #endregion
        
        #region Initialization
        
        public static void Initialize()
        {
            if (Main.dedServ) return;
            
            // Try to load custom Trail shader first
            TryLoadCustomShader();
            
            // Always create BasicEffect as fallback
            _basicEffect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = true,
                World = Matrix.Identity,
                View = Matrix.Identity
            };
            
            // Allocate for max trails * max points * 2 vertices per point
            _vertices = new VertexPositionColorTexture[MaxTrails * MaxPointsPerTrail * 2];
            _indices = new short[MaxTrails * MaxPointsPerTrail * 6];
        }
        
        /// <summary>
        /// Attempts to load the custom Trail shader from Assets/Shaders/Trail.xnb
        /// Falls back to BasicEffect if shader is unavailable.
        /// </summary>
        private static void TryLoadCustomShader()
        {
            if (_shaderLoadAttempted) return;
            _shaderLoadAttempted = true;
            
            try
            {
                // Try to load the compiled Trail.xnb shader
                // Path: MagnumOpus/Assets/Shaders/Trail (without extension)
                if (ModContent.HasAsset("MagnumOpus/Assets/Shaders/Trail"))
                {
                    _trailShader = ModContent.Request<Effect>(
                        "MagnumOpus/Assets/Shaders/Trail",
                        AssetRequestMode.ImmediateLoad
                    ).Value;
                    
                    if (_trailShader != null)
                    {
                        _useCustomShader = true;
                        Main.NewText("[ArkSwingTrail] Custom Trail shader loaded successfully!", 
                            Microsoft.Xna.Framework.Color.LightGreen);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't crash - will use BasicEffect fallback
                _useCustomShader = false;
                Main.NewText($"[ArkSwingTrail] Shader load failed, using fallback: {ex.Message}", 
                    Microsoft.Xna.Framework.Color.Yellow);
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Updates the swing trail for a player's melee weapon.
        /// Call this every frame during the swing in the weapon's UseItemFrame or similar.
        /// </summary>
        /// <param name="player">The player swinging</param>
        /// <param name="bladeLength">How far from player center the blade tip is</param>
        /// <param name="primaryColor">Main trail color</param>
        /// <param name="secondaryColor">Secondary/glow color</param>
        /// <param name="width">Base width of the trail</param>
        /// <param name="theme">Theme name for palette (optional)</param>
        public static void UpdateSwingTrail(Player player, float bladeLength, 
            Color primaryColor, Color secondaryColor, float width = 30f, string theme = null)
        {
            if (Main.dedServ) return;
            
            // Find or create trail for this player
            SwingTrailData trail = null;
            foreach (var t in _activeTrails)
            {
                if (t.OwnerIndex == player.whoAmI && !t.IsFading)
                {
                    trail = t;
                    break;
                }
            }
            
            if (trail == null)
            {
                // Create new trail
                if (_activeTrails.Count >= MaxTrails)
                {
                    // Remove oldest fading trail
                    for (int i = 0; i < _activeTrails.Count; i++)
                    {
                        if (_activeTrails[i].IsFading)
                        {
                            _activeTrails.RemoveAt(i);
                            break;
                        }
                    }
                    if (_activeTrails.Count >= MaxTrails)
                        _activeTrails.RemoveAt(0);
                }
                
                trail = new SwingTrailData
                {
                    Owner = player,
                    OwnerIndex = player.whoAmI,
                    PrimaryColor = primaryColor,
                    SecondaryColor = secondaryColor,
                    GlowColor = Color.Lerp(primaryColor, Color.White, 0.5f),
                    BaseWidth = width,
                    NoiseScrollOffset = Main.rand.NextFloat(100f),
                    Timer = 0,
                    FadeTimer = 0,
                    IsFading = false,
                    Theme = theme
                };
                _activeTrails.Add(trail);
            }
            
            // Update colors (in case they change during swing)
            trail.PrimaryColor = primaryColor;
            trail.SecondaryColor = secondaryColor;
            trail.GlowColor = Color.Lerp(primaryColor, Color.White, 0.5f);
            trail.BaseWidth = width;
            trail.Timer++;
            
            // Calculate blade tip position
            float swingAngle = player.itemRotation;
            Vector2 bladeTip = player.Center + swingAngle.ToRotationVector2() * bladeLength;
            Vector2 perpDir = swingAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
            
            // Add new point
            if (trail.Points.Count == 0 || 
                Vector2.DistanceSquared(bladeTip, trail.Points[trail.Points.Count - 1].Position) > 4f)
            {
                // Remove old points if at capacity
                while (trail.Points.Count >= MaxPointsPerTrail)
                    trail.Points.RemoveAt(0);
                
                trail.Points.Add(new SwingPoint
                {
                    Position = bladeTip,
                    Direction = perpDir,
                    Rotation = swingAngle,
                    FrameCreated = Main.GameUpdateCount
                });
            }
        }
        
        /// <summary>
        /// Marks a player's swing trail as fading (call when swing ends).
        /// </summary>
        public static void EndSwingTrail(Player player)
        {
            foreach (var trail in _activeTrails)
            {
                if (trail.OwnerIndex == player.whoAmI && !trail.IsFading)
                {
                    trail.IsFading = true;
                    trail.FadeTimer = 0;
                }
            }
        }
        
        /// <summary>
        /// Spawns an instant swing trail arc for a completed swing.
        /// Use for weapons that don't call UpdateSwingTrail every frame.
        /// </summary>
        public static void SpawnSwingArc(Player player, float startAngle, float endAngle,
            float bladeLength, Color primaryColor, Color secondaryColor, float width = 30f,
            int pointCount = 16, string theme = null)
        {
            if (Main.dedServ) return;
            
            // Create trail
            if (_activeTrails.Count >= MaxTrails)
                _activeTrails.RemoveAt(0);
            
            var trail = new SwingTrailData
            {
                Owner = player,
                OwnerIndex = player.whoAmI,
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                GlowColor = Color.Lerp(primaryColor, Color.White, 0.5f),
                BaseWidth = width,
                NoiseScrollOffset = Main.rand.NextFloat(100f),
                Timer = 0,
                FadeTimer = 0,
                IsFading = true, // Immediately starts fading
                Theme = theme
            };
            
            // Generate arc points
            for (int i = 0; i < pointCount; i++)
            {
                float t = (float)i / (pointCount - 1);
                float angle = MathHelper.Lerp(startAngle, endAngle, t);
                Vector2 pos = player.Center + angle.ToRotationVector2() * bladeLength;
                Vector2 perpDir = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                
                trail.Points.Add(new SwingPoint
                {
                    Position = pos,
                    Direction = perpDir,
                    Rotation = angle,
                    FrameCreated = Main.GameUpdateCount
                });
            }
            
            _activeTrails.Add(trail);
        }
        
        #endregion
        
        #region Update
        
        public static void Update()
        {
            for (int i = _activeTrails.Count - 1; i >= 0; i--)
            {
                var trail = _activeTrails[i];
                
                trail.NoiseScrollOffset += 0.03f; // Scroll noise texture
                
                if (trail.IsFading)
                {
                    trail.FadeTimer++;
                    
                    // Fade out over ~15 frames
                    if (trail.FadeTimer > 20 || trail.Points.Count == 0)
                    {
                        _activeTrails.RemoveAt(i);
                        continue;
                    }
                    
                    // Remove oldest points gradually
                    if (trail.FadeTimer % 2 == 0 && trail.Points.Count > 0)
                        trail.Points.RemoveAt(0);
                }
            }
        }
        
        #endregion
        
        #region Render
        
        public static void Render(SpriteBatch spriteBatch)
        {
            // Triangle strip rendering with proper additive blending
            // All passes now use BlendState.Additive with bright colors
            if (_activeTrails.Count == 0) return;
            
            // Ensure initialization
            if (_basicEffect == null) Initialize();
            if (_basicEffect == null) return;
            
            // ============================================
            // TEXTURE LOOKUPS VIA VFXTextureRegistry
            // ============================================
            // This uses the centralized texture registry for proper
            // noise/LUT/mask texture management with fallbacks.
            
            // Primary noise for fog/nebula passes (prefer smoke noise for organic flow)
            Texture2D noiseTexture = VFXTextureRegistry.Noise.Smoke 
                ?? VFXTextureRegistry.Noise.TileableFBM
                ?? MagnumTextureRegistry.GetBloom();
            
            // Marble noise for mid-layer nebula effect (swirling patterns)
            Texture2D marbleNoise = VFXTextureRegistry.Noise.Marble 
                ?? VFXTextureRegistry.Noise.Smoke;
            
            // Energy gradient for main trail pass
            Texture2D trailTexture = VFXTextureRegistry.LUT.HorizontalEnergy 
                ?? VFXTextureRegistry.LUT.EnergyGradient
                ?? VFXTextureRegistry.Beam.Streak1
                ?? MagnumTextureRegistry.GetBloom();
            
            // Soft glow for core pass (white-hot center)
            Texture2D softGlow = VFXTextureRegistry.Mask.RadialGradient 
                ?? MagnumTextureRegistry.GetBloom();
            
            GraphicsDevice device = Main.instance.GraphicsDevice;
            
            // Setup projection matrix for BasicEffect fallback
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
            foreach (var trail in _activeTrails)
            {
                if (trail.Points.Count < 2) continue;
                
                float fadeAlpha = trail.IsFading ? 1f - (trail.FadeTimer / 20f) : 1f;
                fadeAlpha = Math.Max(0f, fadeAlpha);
                
                // Get theme-specific noise texture for this trail
                // This allows different themes to have different noise characteristics
                Texture2D themeNoise = VFXTextureRegistry.GetNoiseForTheme(trail.Theme) ?? noiseTexture;
                
                // === PASS 1: BACKGROUND FOG (Large, soft, uses theme-specific noise) ===
                // ALL PASSES USE ADDITIVE to prevent black blob artifacts
                RenderTrailPass(device, trail, themeNoise, 
                    widthMult: 2.5f, 
                    alphaMult: 0.15f * fadeAlpha,
                    useSecondaryColor: true,
                    scrollSpeed: 0.3f,
                    BlendState.Additive);
                
                // === PASS 2: MIDGROUND NEBULA (Medium, flowing, marble noise) ===
                RenderTrailPass(device, trail, marbleNoise ?? themeNoise, 
                    widthMult: 1.6f, 
                    alphaMult: 0.25f * fadeAlpha,
                    useSecondaryColor: false,
                    scrollSpeed: 0.5f,
                    BlendState.Additive);
                
                // === PASS 3: MAIN TRAIL (Energy gradient texture) ===
                RenderTrailPass(device, trail, trailTexture, 
                    widthMult: 1.0f, 
                    alphaMult: 0.6f * fadeAlpha,
                    useSecondaryColor: false,
                    scrollSpeed: 0.8f,
                    BlendState.Additive);
                
                // === PASS 4: BRIGHT CORE (Thin, white, additive) ===
                RenderTrailPass(device, trail, softGlow ?? trailTexture, 
                    widthMult: 0.35f, 
                    alphaMult: 0.7f * fadeAlpha,
                    useSecondaryColor: false,
                    scrollSpeed: 1.0f,
                    BlendState.Additive,
                    useWhiteCore: true);
            }
        }
        
        private static void RenderTrailPass(GraphicsDevice device, SwingTrailData trail,
            Texture2D texture, float widthMult, float alphaMult, bool useSecondaryColor,
            float scrollSpeed, BlendState blendState, bool useWhiteCore = false)
        {
            if (texture == null || trail.Points.Count < 2) return;
            
            int pointCount = trail.Points.Count;
            int vertexCount = pointCount * 2;
            int triangleCount = (pointCount - 1) * 2;
            
            if (vertexCount > _vertices.Length) return;
            
            float gameTime = Main.GameUpdateCount * 0.02f;
            float scrollOffset = trail.NoiseScrollOffset * scrollSpeed;
            
            // Build vertices
            for (int i = 0; i < pointCount; i++)
            {
                float completionRatio = (float)i / (pointCount - 1);
                
                // Width tapering: thick in middle, thin at edges (QuadraticBump style)
                float widthFactor = MathF.Sin(completionRatio * MathHelper.Pi);
                float width = trail.BaseWidth * widthMult * widthFactor;
                
                // Color gradient along trail
                Color baseColor;
                if (useWhiteCore)
                {
                    baseColor = Color.White;
                }
                else if (useSecondaryColor)
                {
                    baseColor = trail.SecondaryColor;
                }
                else
                {
                    baseColor = Color.Lerp(trail.PrimaryColor, trail.SecondaryColor, completionRatio);
                }
                
                // Alpha fade at edges
                float alphaFactor = widthFactor * alphaMult;
                Color finalColor = baseColor * alphaFactor;
                
                // For additive blending, remove alpha channel
                if (blendState == BlendState.Additive)
                    finalColor = finalColor with { A = 0 };
                
                var point = trail.Points[i];
                Vector2 screenPos = point.Position - Main.screenPosition;
                
                // UV coordinates: X = position along trail, Y = across width
                // Scroll X based on time for flowing effect
                float u = completionRatio + scrollOffset;
                
                // Top vertex
                _vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(screenPos + point.Direction * width * 0.5f, 0),
                    finalColor,
                    new Vector2(u, 0));
                
                // Bottom vertex
                _vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(screenPos - point.Direction * width * 0.5f, 0),
                    finalColor,
                    new Vector2(u, 1));
            }
            
            // Build indices (triangle strip as indexed triangles)
            int idx = 0;
            for (int i = 0; i < pointCount - 1; i++)
            {
                int baseVertex = i * 2;
                _indices[idx++] = (short)baseVertex;
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 2);
                _indices[idx++] = (short)(baseVertex + 1);
                _indices[idx++] = (short)(baseVertex + 3);
                _indices[idx++] = (short)(baseVertex + 2);
            }
            
            // Setup graphics state
            device.BlendState = blendState;
            device.SamplerStates[0] = SamplerState.LinearWrap; // Wrap for scrolling UVs
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            
            // Choose between custom shader and BasicEffect fallback
            if (_useCustomShader && _trailShader != null)
            {
                // Use custom Trail shader with proper uniforms
                try
                {
                    // Set primary shader parameters
                    _trailShader.Parameters["SpriteTexture"]?.SetValue(texture);
                    _trailShader.Parameters["uTime"]?.SetValue(gameTime);
                    _trailShader.Parameters["uOpacity"]?.SetValue(alphaMult);
                    _trailShader.Parameters["uIntensity"]?.SetValue(1.5f); // HDR-like intensity
                    
                    // ============================================
                    // VFXTextureRegistry Integration
                    // ============================================
                    // Set additional textures from registry for advanced effects
                    VFXTextureRegistry.SetShaderTextures(device, _trailShader);
                    
                    // Use trail colors for proper gradient
                    Vector3 primaryVec = useWhiteCore ? Vector3.One : 
                        new Vector3(trail.PrimaryColor.R / 255f, trail.PrimaryColor.G / 255f, trail.PrimaryColor.B / 255f);
                    Vector3 secondaryVec = useWhiteCore ? Vector3.One :
                        new Vector3(trail.SecondaryColor.R / 255f, trail.SecondaryColor.G / 255f, trail.SecondaryColor.B / 255f);
                    
                    _trailShader.Parameters["uColor"]?.SetValue(primaryVec);
                    _trailShader.Parameters["uSecondaryColor"]?.SetValue(secondaryVec);
                    
                    // Choose technique (DefaultTechnique for main, BloomTechnique for glow passes)
                    string techniqueName = (widthMult > 2.0f) ? "BloomTechnique" : "DefaultTechnique";
                    if (_trailShader.Techniques[techniqueName] != null)
                        _trailShader.CurrentTechnique = _trailShader.Techniques[techniqueName];
                    
                    // Draw with custom shader
                    foreach (var pass in _trailShader.CurrentTechnique.Passes)
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
                catch
                {
                    // Fall back to BasicEffect if shader fails
                    RenderWithBasicEffect(device, texture, vertexCount, triangleCount);
                }
            }
            else
            {
                // Fallback: Use BasicEffect
                RenderWithBasicEffect(device, texture, vertexCount, triangleCount);
            }
        }
        
        /// <summary>
        /// Fallback rendering using BasicEffect when custom shader is unavailable.
        /// </summary>
        private static void RenderWithBasicEffect(GraphicsDevice device, Texture2D texture, 
            int vertexCount, int triangleCount)
        {
            if (_basicEffect == null) return;
            
            _basicEffect.Texture = texture;
            _basicEffect.TextureEnabled = true;
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
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
        
        #endregion
        
        #region Utility
        
        public static void Clear()
        {
            _activeTrails.Clear();
        }
        
        public static void Unload()
        {
            Clear();
            _trailShader = null; // Don't dispose - managed by ModContent
            _basicEffect?.Dispose();
            _basicEffect = null;
            _shaderLoadAttempted = false;
            _useCustomShader = false;
            _vertices = null;
            _indices = null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// ModSystem to hook ArkSwingTrail into update and render loops.
    /// </summary>
    public class ArkSwingTrailSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawProjectiles += DrawSwingTrails;
        }
        
        public override void Unload()
        {
            On_Main.DrawProjectiles -= DrawSwingTrails;
            ArkSwingTrail.Unload();
        }
        
        public override void PostUpdatePlayers()
        {
            ArkSwingTrail.Update();
        }
        
        private void DrawSwingTrails(On_Main.orig_DrawProjectiles orig, Main self)
        {
            // Call original first
            orig(self);
            
            // Now render swing trails on top
            // The Render method uses direct GraphicsDevice calls with BasicEffect,
            // so we don't need to manage SpriteBatch state at all.
            // This avoids the End/Begin mismatch that caused crashes.
            try
            {
                ArkSwingTrail.Render(Main.spriteBatch);
            }
            catch (System.Exception ex)
            {
                // Log but don't crash - VFX failure shouldn't break the game
                Mod?.Logger?.Warn($"ArkSwingTrail render failed: {ex.Message}");
            }
        }
        
        public override void OnWorldUnload()
        {
            ArkSwingTrail.Clear();
        }
    }
}
