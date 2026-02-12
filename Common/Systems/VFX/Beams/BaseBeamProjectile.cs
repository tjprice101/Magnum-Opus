using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BASE CLASS FOR CALAMITY-STYLE BEAM PROJECTILES
    /// 
    /// OPTIMIZED: Uses static pooled buffers to prevent per-frame allocations.
    /// 
    /// Inherit from this class to create massive, shiny beam projectiles with:
    /// - Automatic primitive mesh rendering with multi-pass bloom
    /// - Sub-pixel interpolation for 144Hz+ smoothness
    /// - Tick-based lifetime management
    /// - Dynamic width/color functions
    /// - Automatic particle emission
    /// 
    /// Implementations should:
    /// 1. Set ThemeName in SetDefaults or constructor
    /// 2. Override BeamWidth for custom width profile
    /// 3. Override GetBeamEndPoint to define beam target
    /// 4. Optionally override WidthStyle, BloomMultiplier, etc.
    /// </summary>
    public abstract class BaseBeamProjectile : ModProjectile
    {
        #region Static Pooled Buffers (MEMORY OPTIMIZATION)
        
        // Static buffers shared by all BaseBeamProjectile instances
        private const int PooledVertexCapacity = 256; // Enough for 128 beam points
        private const int PooledIndexCapacity = 768;  // 128 quads * 6 indices
        
        private static VertexPositionColorTexture[] _pooledVertices;
        private static short[] _pooledIndices;
        private static bool _poolInitialized = false;
        
        private static void EnsurePoolInitialized()
        {
            if (_poolInitialized || Main.dedServ) return;
            _pooledVertices = new VertexPositionColorTexture[PooledVertexCapacity];
            _pooledIndices = new short[PooledIndexCapacity];
            _poolInitialized = true;
        }
        
        internal static void UnloadPooledResources()
        {
            _pooledVertices = null;
            _pooledIndices = null;
            _poolInitialized = false;
            _effect?.Dispose();
            _effect = null;
        }
        
        #endregion
        #region Abstract/Virtual Properties (Override These)
        
        /// <summary>
        /// Theme name for color palette. Required.
        /// Examples: "Eroica", "Fate", "LaCampanella", "MoonlightSonata", "SwanLake", "EnigmaVariations"
        /// </summary>
        public virtual string ThemeName => "";
        
        /// <summary>
        /// Base width of the beam in pixels.
        /// </summary>
        public virtual float BeamWidth => 40f;
        
        /// <summary>
        /// Width style profile. Default is QuadraticBump (thin→thick→thin).
        /// </summary>
        public virtual CalamityBeamSystem.WidthStyle WidthStyle => CalamityBeamSystem.WidthStyle.QuadraticBump;
        
        /// <summary>
        /// Bloom multiplier for outer glow layer. Higher = larger glow.
        /// </summary>
        public virtual float BloomMultiplier => 2.5f;
        
        /// <summary>
        /// Core multiplier for inner bright layer. Lower = thinner core.
        /// </summary>
        public virtual float CoreMultiplier => 0.3f;
        
        /// <summary>
        /// Speed of texture scrolling effect.
        /// </summary>
        public virtual float TextureScrollSpeed => 2.5f;
        
        /// <summary>
        /// Number of segments in the beam mesh. Higher = smoother curves.
        /// </summary>
        public virtual int SegmentCount => 50;
        
        /// <summary>
        /// Whether to emit particles along the beam.
        /// </summary>
        public virtual bool EmitParticles => true;
        
        /// <summary>
        /// Particle emission density multiplier.
        /// </summary>
        public virtual float ParticleDensity => 1f;
        
        /// <summary>
        /// Maximum beam length in pixels.
        /// </summary>
        public virtual float MaxBeamLength => 2000f;
        
        /// <summary>
        /// Whether the beam should fade out at the end of its lifetime.
        /// </summary>
        public virtual bool FadeOnDeath => true;
        
        /// <summary>
        /// Duration of fade-out in ticks (if FadeOnDeath is true).
        /// </summary>
        public virtual int FadeDuration => 15;
        
        #endregion
        
        #region Abstract Methods (Must Override)
        
        /// <summary>
        /// Returns the world position where the beam ends.
        /// Override this to define beam targeting logic.
        /// </summary>
        protected abstract Vector2 GetBeamEndPoint();
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// Current beam start position (usually Projectile.Center).
        /// </summary>
        protected Vector2 BeamStart => InterpolatedRenderer.GetInterpolatedCenter(Projectile);
        
        /// <summary>
        /// Current beam end position.
        /// </summary>
        protected Vector2 BeamEnd { get; private set; }
        
        /// <summary>
        /// Cached beam profile for rendering.
        /// </summary>
        protected CalamityBeamSystem.BeamProfile BeamProfile { get; private set; }
        
        /// <summary>
        /// Fade multiplier (0-1) for death animation.
        /// </summary>
        protected float FadeMultiplier { get; private set; } = 1f;
        
        /// <summary>
        /// Whether the beam is currently fading out.
        /// </summary>
        protected bool IsFading { get; private set; }
        
        /// <summary>
        /// Tick counter for fade animation.
        /// </summary>
        protected int FadeTimer { get; private set; }
        
        // Position history for smooth trails (if using trail mode)
        private List<Vector2> _positionHistory = new();
        private const int MaxPositionHistory = 30;
        
        #endregion
        
        #region Lifecycle
        
        public override void SetStaticDefaults()
        {
            // Enable old position tracking for trail rendering
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = SegmentCount;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            // Default beam projectile settings - override in derived class
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;  // Override in derived class
            Projectile.friendly = true;  // Override in derived class
            Projectile.timeLeft = 120;   // Override in derived class
            
            // Build beam profile
            RebuildBeamProfile();
        }
        
        /// <summary>
        /// Rebuilds the beam profile from current settings.
        /// Call if you change properties at runtime.
        /// </summary>
        protected void RebuildBeamProfile()
        {
            BeamProfile = new CalamityBeamSystem.BeamProfile
            {
                ThemeName = ThemeName,
                BaseWidth = BeamWidth,
                WidthType = WidthStyle,
                BloomMultiplier = BloomMultiplier,
                CoreMultiplier = CoreMultiplier,
                TextureScrollSpeed = TextureScrollSpeed,
                SegmentCount = SegmentCount,
                EmitParticles = EmitParticles,
                ParticleDensity = ParticleDensity
            };
        }
        
        #endregion
        
        #region AI
        
        public override void AI()
        {
            // Update beam end position
            BeamEnd = GetBeamEndPoint();
            
            // Clamp to max length
            Vector2 direction = (BeamEnd - BeamStart).SafeNormalize(Vector2.UnitX);
            float distance = (BeamEnd - BeamStart).Length();
            if (distance > MaxBeamLength)
            {
                BeamEnd = BeamStart + direction * MaxBeamLength;
            }
            
            // Store position for trail mode
            StorePositionHistory();
            
            // Handle fade-out
            if (FadeOnDeath && Projectile.timeLeft <= FadeDuration)
            {
                IsFading = true;
                FadeTimer++;
                FadeMultiplier = 1f - (float)FadeTimer / FadeDuration;
            }
            
            // Custom AI logic (override OnBeamAI in derived class)
            OnBeamAI();
            
            // Add lighting along beam
            AddBeamLighting();
        }
        
        /// <summary>
        /// Override for custom AI behavior.
        /// </summary>
        protected virtual void OnBeamAI() { }
        
        /// <summary>
        /// Stores current position in history for trail rendering.
        /// </summary>
        private void StorePositionHistory()
        {
            Vector2 interpolatedPos = InterpolatedRenderer.GetInterpolatedCenter(Projectile);
            
            _positionHistory.Add(interpolatedPos);
            
            while (_positionHistory.Count > MaxPositionHistory)
            {
                _positionHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Adds lighting along the beam.
        /// </summary>
        protected virtual void AddBeamLighting()
        {
            Color[] palette = MagnumThemePalettes.GetThemePalette(ThemeName);
            Color lightColor = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            Vector2 direction = (BeamEnd - BeamStart).SafeNormalize(Vector2.UnitX);
            float length = (BeamEnd - BeamStart).Length();
            int lightPoints = Math.Max(3, (int)(length / 100f));
            
            for (int i = 0; i < lightPoints; i++)
            {
                float t = (float)i / (lightPoints - 1);
                Vector2 lightPos = Vector2.Lerp(BeamStart, BeamEnd, t);
                
                float intensity = MathF.Sin(t * MathHelper.Pi) * FadeMultiplier * 0.8f;
                Lighting.AddLight(lightPos, lightColor.ToVector3() * intensity);
            }
        }
        
        #endregion
        
        #region Rendering
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Render the beam using CalamityBeamSystem
            RenderBeam();
            
            // Don't draw default sprite
            return false;
        }
        
        /// <summary>
        /// Renders the beam. Override for custom rendering behavior.
        /// </summary>
        protected virtual void RenderBeam()
        {
            // Calculate fade
            float fade = FadeMultiplier;
            
            // Use the unified beam system
            var profile = BeamProfile;
            profile.ParticleDensity *= fade;
            
            // Render with interpolated positions
            RenderBeamInternal(BeamStart, BeamEnd, profile, fade);
        }
        
        /// <summary>
        /// Internal rendering using CalamityBeamSystem patterns.
        /// </summary>
        private void RenderBeamInternal(Vector2 start, Vector2 end, CalamityBeamSystem.BeamProfile profile, float fade)
        {
            // Get palette
            Color[] palette = MagnumThemePalettes.GetThemePalette(profile.ThemeName) ?? new[] { Color.White };
            
            float uvScroll = Main.GlobalTimeWrappedHourly * profile.TextureScrollSpeed;
            
            // Generate control points
            int segmentCount = profile.SegmentCount;
            Vector2[] controlPoints = new Vector2[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                controlPoints[i] = Vector2.Lerp(start, end, t);
            }
            
            // Multi-pass rendering (mirrors CalamityBeamSystem)
            RenderPass(controlPoints, profile, uvScroll, palette, profile.BloomMultiplier, 0.15f * fade, true);
            RenderPass(controlPoints, profile, uvScroll, palette, profile.BloomMultiplier * 0.6f, 0.35f * fade, true);
            RenderPass(controlPoints, profile, uvScroll, palette, 1f, 0.85f * fade, false);
            RenderPass(controlPoints, profile, uvScroll, palette, profile.CoreMultiplier, 1f * fade, false, true);
            
            // Particles
            if (profile.EmitParticles)
            {
                EmitBeamParticles(start, end, palette, profile.ParticleDensity * fade);
            }
        }
        
        /// <summary>
        /// Renders a single pass.
        /// </summary>
        private void RenderPass(Vector2[] points, CalamityBeamSystem.BeamProfile profile, float uvScroll,
            Color[] palette, float widthMult, float opacityMult, bool additive, bool forceWhite = false)
        {
            if (points.Length < 2) return;
            
            try { Main.spriteBatch.End(); } catch { }
            
            try
            {
                // Ensure pooled buffers are ready
                EnsurePoolInitialized();
                if (_pooledVertices == null) return;
                
                int vertexCount = points.Length * 2;
                int triangleCount = (points.Length - 1) * 2;
                int indexCount = triangleCount * 3;
                
                // Bounds check against pooled buffer capacity
                if (vertexCount > PooledVertexCapacity || indexCount > PooledIndexCapacity)
                    return; // Skip if too many vertices - prevents allocation
                
                for (int i = 0; i < points.Length; i++)
                {
                    float ratio = (float)i / (points.Length - 1);
                    float width = CalculateWidth(ratio, profile) * widthMult;
                    
                    Color color;
                    if (forceWhite)
                    {
                        color = Color.White * MathF.Sin(ratio * MathHelper.Pi) * opacityMult;
                    }
                    else
                    {
                        float scrolledRatio = (ratio * 0.5f + uvScroll * 0.3f) % 1f;
                        color = VFXUtilities.PaletteLerp(palette, scrolledRatio) * 
                            MathF.Sin(ratio * MathHelper.Pi) * opacityMult;
                    }
                    
                    if (additive) color = color.WithoutAlpha();
                    
                    Vector2 dir = GetDirection(points, i);
                    Vector2 perp = new Vector2(-dir.Y, dir.X);
                    Vector2 screenPos = points[i] - Main.screenPosition;
                    
                    float u = ratio + uvScroll;
                    
                    Vector2 topPos = screenPos + perp * width * 0.5f;
                    Vector2 bottomPos = screenPos - perp * width * 0.5f;
                    _pooledVertices[i * 2] = new VertexPositionColorTexture(
                        new Vector3(topPos.X, topPos.Y, 0), color, new Vector2(u, 0));
                    _pooledVertices[i * 2 + 1] = new VertexPositionColorTexture(
                        new Vector3(bottomPos.X, bottomPos.Y, 0), color, new Vector2(u, 1));
                }
                
                int idx = 0;
                for (int i = 0; i < points.Length - 1; i++)
                {
                    int b = i * 2;
                    _pooledIndices[idx++] = (short)b;
                    _pooledIndices[idx++] = (short)(b + 1);
                    _pooledIndices[idx++] = (short)(b + 2);
                    _pooledIndices[idx++] = (short)(b + 1);
                    _pooledIndices[idx++] = (short)(b + 3);
                    _pooledIndices[idx++] = (short)(b + 2);
                }
                
                DrawPrimitives(_pooledVertices, _pooledIndices, vertexCount, triangleCount, additive);
            }
            finally
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        private float CalculateWidth(float ratio, CalamityBeamSystem.BeamProfile profile)
        {
            float width = profile.BaseWidth;
            
            return profile.WidthType switch
            {
                CalamityBeamSystem.WidthStyle.QuadraticBump => width * MathF.Sin(ratio * MathHelper.Pi),
                CalamityBeamSystem.WidthStyle.SourceTaper => MathHelper.Lerp(width, width * 0.08f, ratio * ratio * (3f - 2f * ratio)),
                CalamityBeamSystem.WidthStyle.Constant => width,
                CalamityBeamSystem.WidthStyle.PulsingWidth => width * (1f + MathF.Sin(Main.GlobalTimeWrappedHourly * profile.PulseSpeed) * profile.PulseAmount) * MathF.Sin(ratio * MathHelper.Pi),
                _ => width * MathF.Sin(ratio * MathHelper.Pi)
            };
        }
        
        private Vector2 GetDirection(Vector2[] points, int i)
        {
            if (i == 0) return (points[1] - points[0]).SafeNormalize(Vector2.UnitY);
            if (i == points.Length - 1) return (points[i] - points[i - 1]).SafeNormalize(Vector2.UnitY);
            return (points[i + 1] - points[i - 1]).SafeNormalize(Vector2.UnitY);
        }
        
        private static BasicEffect _effect;
        
        private void DrawPrimitives(VertexPositionColorTexture[] vertices, short[] indices,
            int vertexCount, int triangleCount, bool additive)
        {
            var device = Main.instance.GraphicsDevice;
            
            if (_effect == null)
            {
                _effect = new BasicEffect(device) { VertexColorEnabled = true, TextureEnabled = false };
            }
            
            var prevBlend = device.BlendState;
            var prevRaster = device.RasterizerState;
            var prevDepth = device.DepthStencilState;
            
            device.BlendState = additive ? BlendState.Additive : BlendState.AlphaBlend;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            
            _effect.View = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            _effect.World = Matrix.Identity;
            
            try
            {
                foreach (var pass in _effect.CurrentTechnique.Passes)
                    pass.Apply();
                
                device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                    vertices, 0, vertexCount, indices, 0, triangleCount);
            }
            finally
            {
                device.BlendState = prevBlend;
                device.RasterizerState = prevRaster;
                device.DepthStencilState = prevDepth;
            }
        }
        
        private void EmitBeamParticles(Vector2 start, Vector2 end, Color[] palette, float density)
        {
            if (density <= 0 || Main.rand.NextFloat() > density * 0.25f) return;
            
            Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);
            float len = (end - start).Length();
            Color color = palette.Length > 0 ? palette[Main.rand.Next(palette.Length)] : Color.White;
            
            int count = Math.Max(1, (int)(len / 100f * density));
            
            for (int i = 0; i < count; i++)
            {
                if (!Main.rand.NextBool(5)) continue;
                
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(start, end, t);
                Vector2 offset = new Vector2(-dir.Y, dir.X) * Main.rand.NextFloat(-10f, 10f);
                
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.Enchanted_Gold,
                    dir * Main.rand.NextFloat(0.2f, 1f), 0, color, 1.1f);
                d.noGravity = true;
            }
        }
        
        #endregion
        
        #region Collision
        
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Line collision along beam
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                BeamStart, BeamEnd, BeamWidth * 0.5f, ref _);
        }
        
        #endregion
        
        #region Kill Effects
        
        public override void OnKill(int timeLeft)
        {
            // Impact effect at beam end
            if (!IsFading)
            {
                CalamityBeamSystem.CreateImpactEffect(BeamEnd, ThemeName, 1f);
            }
            
            OnBeamKill();
        }
        
        /// <summary>
        /// Override for custom kill effects.
        /// </summary>
        protected virtual void OnBeamKill() { }
        
        #endregion
    }
}
