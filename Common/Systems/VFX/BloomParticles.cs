using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Glowing pixel particle that scales and fades between colors.
    /// Direct implementation of FargosSoulsDLC's BloomPixelParticle pattern.
    /// 
    /// Usage:
    /// MagnumParticleHandler.SpawnParticle(new BloomPixelParticle(
    ///     position, velocity, Color.Orange, Color.Red, 20, 
    ///     new Vector2(1.5f), new Vector2(0.3f)));
    /// </summary>
    public class BloomPixelParticle : Particle
    {
        public Color StartColor;
        public Color EndColor;
        public Vector2 StartScale;
        public Vector2 EndScale;
        public float Drag;
        public float Gravity;
        
        private Vector2 _scaleVector;
        
        public override string Texture => ""; // Uses bloom texture directly
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        
        /// <summary>
        /// Creates a bloom pixel particle.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="startColor">Color at spawn</param>
        /// <param name="endColor">Color at death</param>
        /// <param name="lifetime">Lifetime in frames</param>
        /// <param name="startScale">Scale at spawn (Vector2 for non-uniform)</param>
        /// <param name="endScale">Scale at death</param>
        /// <param name="drag">Velocity multiplier per frame (0.96 = standard)</param>
        /// <param name="gravity">Y velocity added per frame</param>
        public BloomPixelParticle(Vector2 position, Vector2 velocity, Color startColor, Color endColor,
            int lifetime, Vector2 startScale, Vector2 endScale, float drag = 0.96f, float gravity = 0f)
        {
            Position = position;
            Velocity = velocity;
            StartColor = startColor;
            EndColor = endColor;
            Lifetime = lifetime;
            StartScale = startScale;
            EndScale = endScale;
            Drag = drag;
            Gravity = gravity;
            _scaleVector = startScale;
        }
        
        /// <summary>
        /// Convenience constructor with uniform scale.
        /// </summary>
        public BloomPixelParticle(Vector2 position, Vector2 velocity, Color startColor, Color endColor,
            int lifetime, float startScale, float endScale, float drag = 0.96f, float gravity = 0f)
            : this(position, velocity, startColor, endColor, lifetime, 
                  new Vector2(startScale), new Vector2(endScale), drag, gravity)
        {
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Interpolate color and scale
            Color = Color.Lerp(StartColor, EndColor, progress);
            _scaleVector = Vector2.Lerp(StartScale, EndScale, progress);
            Scale = _scaleVector.X; // For compatibility
            
            // Apply physics
            Velocity *= Drag;
            Velocity.Y += Gravity;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            // CRITICAL: Remove alpha for proper additive blending
            Color drawColor = Color.WithoutAlpha();
            
            spriteBatch.Draw(bloom, drawPos, null, drawColor, 0f, origin, _scaleVector, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Glowing square/diamond particle that rotates and fades.
    /// Based on FargosSoulsDLC's GlowySquareParticle pattern.
    /// Great for sparks, debris, and crystalline effects.
    /// </summary>
    public class GlowySquareParticle : Particle
    {
        public Color ParticleColor;
        public float StartScale;
        public float RotationSpeed;
        public bool UseGravity;
        public float Drag;
        
        public override string Texture => ""; // Uses generated square or bloom
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        
        public GlowySquareParticle(Vector2 position, Vector2 velocity, Color color, 
            float scale, int lifetime, bool useGravity = false, float drag = 0.95f)
        {
            Position = position;
            Velocity = velocity;
            ParticleColor = color;
            StartScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            UseGravity = useGravity;
            Drag = drag;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Fade and shrink
            Scale = StartScale * (1f - progress);
            Color = ParticleColor * (1f - progress);
            
            // Physics
            if (UseGravity)
                Velocity.Y += 0.15f;
            else
                Velocity *= Drag;
            
            // Rotation based on velocity
            Rotation += RotationSpeed + Velocity.Length() * 0.02f;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GlowySpark?.Value ?? MagnumTextureRegistry.GetBloom();
            if (tex == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            
            // Draw with alpha removed for additive
            Color drawColor = Color.WithoutAlpha();
            spriteBatch.Draw(tex, drawPos, null, drawColor, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Large intense bloom flash for impact effects.
    /// Based on FargosSoulsDLC's StrongBloom particle.
    /// Quick expand then fade pattern.
    /// </summary>
    public class StrongBloomParticle : Particle
    {
        public Color BloomColor;
        public float StartScale;
        public float PeakTime; // Fraction of lifetime at peak (0.2 = 20%)
        
        public override string Texture => "";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        public override bool Important => true; // Always show impact flashes
        
        public StrongBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            BloomColor = color;
            StartScale = scale;
            Lifetime = lifetime;
            PeakTime = 0.2f; // Quick expand to peak
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Quick expand then fade
            float scaleProgress = progress < PeakTime 
                ? progress / PeakTime 
                : 1f - (progress - PeakTime) / (1f - PeakTime);
            
            Scale = StartScale * (0.7f + scaleProgress * 0.3f);
            Color = BloomColor * (1f - progress);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            // Multi-layer bloom stack for intensity
            Color colorNoAlpha = Color.WithoutAlpha();
            
            // Outer large glow
            spriteBatch.Draw(bloom, drawPos, null, colorNoAlpha * 0.5f, 0f, origin, Scale * 1.5f, SpriteEffects.None, 0f);
            // Main bloom
            spriteBatch.Draw(bloom, drawPos, null, colorNoAlpha * 0.8f, 0f, origin, Scale, SpriteEffects.None, 0f);
            // White hot core
            spriteBatch.Draw(bloom, drawPos, null, Color.White.WithoutAlpha() * 0.5f, 0f, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Directional bloom flash that stretches in motion direction.
    /// Great for motion blur effects and directional impacts.
    /// </summary>
    public class DirectionalBloomParticle : Particle
    {
        public Color BloomColor;
        public float StartScale;
        public float StretchFactor;
        
        public override string Texture => "";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        
        public DirectionalBloomParticle(Vector2 position, Vector2 velocity, Color color, 
            float scale, int lifetime, float stretch = 3f)
        {
            Position = position;
            Velocity = velocity;
            BloomColor = color;
            StartScale = scale;
            Lifetime = lifetime;
            StretchFactor = stretch;
            Rotation = velocity.ToRotation();
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            Scale = StartScale * (1f - progress);
            Color = BloomColor * (1f - progress);
            Velocity *= 0.95f;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = bloom.Size() * 0.5f;
            
            // Stretched scale for directional effect
            Vector2 stretchedScale = new Vector2(Scale * StretchFactor, Scale);
            
            Color colorNoAlpha = Color.WithoutAlpha();
            spriteBatch.Draw(bloom, drawPos, null, colorNoAlpha, Rotation, origin, stretchedScale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Line particle that stretches and fades from endpoints.
    /// Based on FargosSoulsDLC's LineParticle pattern.
    /// Perfect for electric arcs and energy connections.
    /// </summary>
    public class LineBloomParticle : Particle
    {
        public Color StartColor;
        public Color? EndColorOverride;
        public float StartLength;
        public bool FadeIn;
        
        public override string Texture => "";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        
        public LineBloomParticle(Vector2 position, Vector2 velocity, Color color, float length, 
            int lifetime, bool fadeIn = false, Color? endColor = null)
        {
            Position = position;
            Velocity = velocity;
            StartColor = color;
            EndColorOverride = endColor;
            StartLength = length;
            Lifetime = lifetime;
            FadeIn = fadeIn;
            Rotation = velocity.ToRotation();
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Opacity: fade in then fade out, or just fade out
            float opacity = FadeIn 
                ? (progress < 0.3f ? progress / 0.3f : 1f - (progress - 0.3f) / 0.7f)
                : (1f - progress);
            
            // Color gradient over lifetime
            Color currentColor = EndColorOverride.HasValue 
                ? Color.Lerp(StartColor, EndColorOverride.Value, progress) 
                : StartColor;
            Color = currentColor * opacity;
            
            // Scale shrinks
            Scale = StartLength * (1f - progress * 0.5f);
            
            // Physics
            Velocity *= 0.92f;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D lineTex = MagnumTextureRegistry.BloomLine?.Value ?? MagnumTextureRegistry.GetBloom();
            if (lineTex == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Origin at left-center for line texture
            Vector2 origin = new Vector2(0, lineTex.Height * 0.5f);
            Vector2 lineScale = new Vector2(Scale * 20f, 0.5f); // Stretch horizontally
            
            Color colorNoAlpha = Color.WithoutAlpha();
            spriteBatch.Draw(lineTex, drawPos, null, colorNoAlpha, Rotation, origin, lineScale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Enhanced smoke particle with drift, rotation, and proper expansion.
    /// Based on FargosSoulsDLC's HeavySmokeParticle pattern.
    /// </summary>
    public class HeavySmokeBloomParticle : Particle
    {
        public Color SmokeColor;
        public float StartScale;
        public float EndScale;
        public float RotationSpeed;
        public bool Rises;
        
        public override string Texture => "";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => false; // Smoke uses alpha blend
        public override bool UseHalfTransparency => true;
        
        public HeavySmokeBloomParticle(Vector2 position, Vector2 velocity, Color color, int lifetime,
            float startScale, float endScale, float rotationSpeed = 0.02f, bool rises = true)
        {
            Position = position;
            Velocity = velocity;
            SmokeColor = color;
            Lifetime = lifetime;
            StartScale = startScale;
            EndScale = endScale;
            RotationSpeed = rotationSpeed;
            Rises = rises;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Expand as it dissipates
            Scale = MathHelper.Lerp(StartScale, EndScale, progress);
            
            // Fade out
            Color = SmokeColor * (1f - progress);
            
            // Physics
            Velocity *= 0.97f;
            
            if (Rises)
                Velocity.Y -= 0.02f; // Rise upward
            
            // Random drift
            Velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
            
            // Rotate
            Rotation += RotationSpeed;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D smokeTex = MagnumTextureRegistry.SmokePuff?.Value ?? MagnumTextureRegistry.GetBloom();
            if (smokeTex == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = smokeTex.Size() * 0.5f;
            
            // Smoke doesn't need alpha removed - uses alpha blend
            spriteBatch.Draw(smokeTex, drawPos, null, Color, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Animated shine flare with rotation for gleaming effects.
    /// Perfect for charge-ups, telegraph indicators, and highlights.
    /// </summary>
    public class ShineFlareParticle : Particle
    {
        public Color FlareColor;
        public float StartScale;
        public float RotationRate;
        
        public override string Texture => "";
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;
        public override bool Important => true;
        
        public ShineFlareParticle(Vector2 position, Color color, float scale, int lifetime, float rotationRate = 2f)
        {
            Position = position;
            Velocity = Vector2.Zero;
            FlareColor = color;
            StartScale = scale;
            Lifetime = lifetime;
            RotationRate = rotationRate;
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            // Use Convert01To010 for appear-peak-disappear effect
            float scaleInterpolant = VFXUtilities.Convert01To010(progress);
            Scale = MathF.Pow(scaleInterpolant, 1.4f) * StartScale + 0.1f;
            
            Color = FlareColor * (1f - progress * 0.3f); // Slight fade only
            Rotation += Main.GlobalTimeWrappedHourly * RotationRate;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D flare = MagnumTextureRegistry.ShineFlare4Point?.Value;
            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            
            if (bloom == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 bloomOrigin = bloom.Size() * 0.5f;
            Color colorNoAlpha = Color.WithoutAlpha();
            
            // Background bloom layers
            spriteBatch.Draw(bloom, drawPos, null, colorNoAlpha * 0.3f, 0f, bloomOrigin, Scale * 1.9f, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloom, drawPos, null, colorNoAlpha * 0.54f, 0f, bloomOrigin, Scale, SpriteEffects.None, 0f);
            
            // Cross flare on top
            if (flare != null)
            {
                Vector2 flareOrigin = flare.Size() * 0.5f;
                spriteBatch.Draw(flare, drawPos, null, colorNoAlpha, Rotation, flareOrigin, Scale, SpriteEffects.None, 0f);
            }
        }
    }
}
