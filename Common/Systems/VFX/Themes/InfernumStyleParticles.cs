using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    // ============================================================================
    // INFERNUM-STYLE PARTICLES - Ported from InfernumMode patterns
    // These particles provide professional-quality visual effects for boss fights,
    // impacts, explosions, and dramatic moments.
    // ============================================================================

    /// <summary>
    /// A pulse ring that expands outward with fading opacity.
    /// Perfect for shockwaves, explosions, phase transitions, and energy bursts.
    /// Inspired by Infernum's PulseRing particle.
    /// </summary>
    public class PulseRingParticle : Particle
    {
        public override string Texture => "BloomRing";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool Important => true;

        private float StartScale;
        private float EndScale;
        private Color BaseColor;
        private float Spin;

        /// <summary>
        /// Creates a pulse ring that expands from startScale to endScale.
        /// </summary>
        /// <param name="position">Center position</param>
        /// <param name="velocity">Movement velocity (usually Vector2.Zero)</param>
        /// <param name="color">Ring color</param>
        /// <param name="startScale">Initial scale</param>
        /// <param name="endScale">Final scale when lifetime ends</param>
        /// <param name="lifetime">Duration in frames</param>
        /// <param name="rotationSpeed">Optional rotation speed</param>
        public PulseRingParticle(Vector2 position, Vector2 velocity, Color color, float startScale, float endScale, int lifetime, float rotationSpeed = 0.05f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            StartScale = startScale;
            EndScale = endScale;
            Scale = startScale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed;
        }

        public override void Update()
        {
            // Smooth scale interpolation
            Scale = MathHelper.Lerp(StartScale, EndScale, LifetimeCompletion);
            
            // Fade out over lifetime
            float opacity = 1f - LifetimeCompletion;
            Color = BaseColor * opacity;
            
            Rotation += Spin;
            
            // Produce light
            Lighting.AddLight(Position, Color.R / 255f * opacity, Color.G / 255f * opacity, Color.B / 255f * opacity);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            float opacity = 1f - LifetimeCompletion;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity,
                Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A strong bloom effect for bright flashes and impacts.
    /// Creates a powerful glow at a point that fades over time.
    /// Inspired by Infernum's StrongBloom particle.
    /// </summary>
    public class StrongBloomParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool Important => true;

        private Color BaseColor;
        private float MaxScale;

        public StrongBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            MaxScale = scale;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            // Pulse effect - brightest in the middle of lifetime
            float pulseProgress = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            Scale = MaxScale * (0.5f + pulseProgress * 0.5f);
            Color = BaseColor * pulseProgress;
            
            Lighting.AddLight(Position, Color.R / 255f * pulseProgress, Color.G / 255f * pulseProgress, Color.B / 255f * pulseProgress);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            float opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            
            // Draw multiple layers for extra bloom
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity * 0.5f,
                0, tex.Size() / 2f, Scale * 1.5f, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity,
                0, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color.White * opacity * 0.5f,
                0, tex.Size() / 2f, Scale * 0.5f, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A cloud/smoke particle that drifts and fades with color transition.
    /// Use for fire explosions, teleport effects, impact clouds.
    /// Inspired by Infernum's CloudParticle.
    /// </summary>
    public class CloudSmokeParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => false;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private Color StartColor;
        private Color EndColor;
        private float Spin;
        private float OriginalScale;

        public CloudSmokeParticle(Vector2 position, Vector2 velocity, Color startColor, Color endColor, int lifetime, float scale, bool randomRotation = true)
        {
            Position = position;
            Velocity = velocity;
            StartColor = startColor;
            EndColor = endColor;
            Lifetime = lifetime;
            Scale = scale;
            OriginalScale = scale;
            Rotation = randomRotation ? Main.rand.NextFloat(MathHelper.TwoPi) : 0f;
            Spin = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Update()
        {
            // Color transition over lifetime
            Color = Color.Lerp(StartColor, EndColor, LifetimeCompletion);
            
            // Slow down and expand slightly
            Velocity *= 0.96f;
            Scale = OriginalScale * (1f + LifetimeCompletion * 0.3f);
            Rotation += Spin;
            
            // Fade out at the end
            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2);
            Color *= opacity;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color,
                Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// Dense smoke particle for explosions and dramatic effects.
    /// Has drift, rotation, and optional glow.
    /// Inspired by Infernum's HeavySmokeParticle.
    /// </summary>
    public class DenseSmokeParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => false;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private Color BaseColor;
        private float Spin;
        private float FadeRate;
        private bool Glowing;
        private float OriginalScale;

        public DenseSmokeParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0.02f, bool glowing = false)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color * opacity;
            Lifetime = lifetime;
            Scale = scale;
            OriginalScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed * (Main.rand.NextBool() ? 1f : -1f);
            Glowing = glowing;
            FadeRate = opacity;
        }

        public override void Update()
        {
            // Drift and slow down
            Velocity *= 0.94f;
            Velocity.Y -= 0.02f; // Slight upward drift
            
            Rotation += Spin;
            
            // Expand over time
            Scale = OriginalScale * (1f + LifetimeCompletion * 0.5f);
            
            // Fade out
            float opacity = (1f - LifetimeCompletion) * FadeRate;
            Color = BaseColor * opacity;
            
            if (Glowing)
                Lighting.AddLight(Position, Color.R / 255f * opacity * 0.3f, Color.G / 255f * opacity * 0.3f, Color.B / 255f * opacity * 0.3f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            float opacity = 1f - LifetimeCompletion;
            
            if (Glowing)
            {
                // Draw glow layer first
                spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity * 0.5f,
                    Rotation, tex.Size() / 2f, Scale * 1.3f, SpriteEffects.None, 0);
            }
            
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity,
                Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A directional spark particle for impacts, explosions, and electrical effects.
    /// Stretches in the direction of movement.
    /// Inspired by Infernum's SparkParticle.
    /// </summary>
    public class DirectionalSparkParticle : Particle
    {
        public override string Texture => "GlowSpark";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private Color BaseColor;
        private bool AffectedByGravity;
        private float OriginalScale;

        public DirectionalSparkParticle(Vector2 position, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            AffectedByGravity = affectedByGravity;
            Lifetime = lifetime;
            Scale = scale;
            OriginalScale = scale;
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            if (AffectedByGravity)
                Velocity.Y += 0.15f;
            
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            
            // Fade and shrink over lifetime
            float progress = LifetimeCompletion;
            Scale = OriginalScale * (1f - progress * 0.5f);
            Color = BaseColor * (1f - progress);
            
            Lighting.AddLight(Position, Color.R / 255f * 0.3f, Color.G / 255f * 0.3f, Color.B / 255f * 0.3f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            float opacity = 1f - LifetimeCompletion;
            
            // Stretch based on velocity
            float stretchFactor = Math.Min(Velocity.Length() * 0.1f + 1f, 3f);
            Vector2 scale = new Vector2(0.5f, stretchFactor) * Scale;
            
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity,
                Rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0);
            
            // Inner bright core
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color.White * opacity * 0.5f,
                Rotation, tex.Size() / 2f, scale * 0.4f, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// Electric arc particle for lightning effects.
    /// Creates a branching lightning appearance.
    /// Inspired by Infernum's ElectricArc particle.
    /// </summary>
    public class ElectricArcParticle : Particle
    {
        public override string Texture => "GlowSpark";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool Important => true;

        private Color BaseColor;
        private float ArcIntensity;
        private Vector2[] ArcPoints;

        public ElectricArcParticle(Vector2 position, Vector2 velocity, Color color, float intensity, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            ArcIntensity = intensity;
            Lifetime = lifetime;
            
            // Generate arc points
            GenerateArcPoints();
        }

        private void GenerateArcPoints()
        {
            int segments = 8;
            ArcPoints = new Vector2[segments];
            Vector2 direction = Velocity.SafeNormalize(Vector2.UnitX);
            float length = Velocity.Length() * 5f;
            
            for (int i = 0; i < segments; i++)
            {
                float progress = i / (float)(segments - 1);
                Vector2 basePos = Position + direction * length * progress;
                
                // Add randomness perpendicular to direction
                Vector2 perp = direction.RotatedBy(MathHelper.PiOver2);
                float offset = Main.rand.NextFloat(-15f, 15f) * ArcIntensity * (1f - Math.Abs(progress - 0.5f) * 2f);
                
                ArcPoints[i] = basePos + perp * offset;
            }
        }

        public override void Update()
        {
            // Regenerate arc points occasionally for flickering effect
            if (Time % 3 == 0)
                GenerateArcPoints();
            
            Color = BaseColor * (1f - LifetimeCompletion);
            
            // Light along the arc
            foreach (var point in ArcPoints)
                Lighting.AddLight(point, Color.R / 255f * 0.5f, Color.G / 255f * 0.5f, Color.B / 255f * 0.5f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            float opacity = 1f - LifetimeCompletion;
            
            // Draw lines between arc points
            for (int i = 0; i < ArcPoints.Length - 1; i++)
            {
                Vector2 start = ArcPoints[i];
                Vector2 end = ArcPoints[i + 1];
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                // Draw stretched spark as arc segment
                Vector2 scale = new Vector2(0.3f * Scale, length / tex.Height);
                Vector2 drawPos = start + direction * 0.5f;
                
                spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, Color * opacity,
                    rotation + MathHelper.PiOver2, tex.Size() / 2f, scale, SpriteEffects.None, 0);
                
                // Bright core
                spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, Color.White * opacity * 0.7f,
                    rotation + MathHelper.PiOver2, tex.Size() / 2f, scale * 0.5f, SpriteEffects.None, 0);
            }
        }
    }

    /// <summary>
    /// A squished light particle for soft magical effects.
    /// Creates a soft, stretched glow that can be used for energy gathering effects.
    /// Inspired by Infernum's SquishyLightParticle.
    /// </summary>
    public class SquishyLightParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private Color BaseColor;
        private float Squish;
        private float SquishSpeed;

        public SquishyLightParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime, float squish = 1f, float squishSpeed = 0.1f)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            BaseColor = color;
            Lifetime = lifetime;
            Squish = squish;
            SquishSpeed = squishSpeed;
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            
            // Squish animation
            Squish += SquishSpeed;
            
            float opacity = 1f - LifetimeCompletion;
            Color = BaseColor * opacity;
            
            Lighting.AddLight(Position, Color.R / 255f * opacity * 0.4f, Color.G / 255f * opacity * 0.4f, Color.B / 255f * opacity * 0.4f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            float opacity = 1f - LifetimeCompletion;
            
            // Calculate squished scale
            float squishX = 1f + (float)Math.Sin(Squish) * 0.3f;
            float squishY = 1f - (float)Math.Sin(Squish) * 0.3f;
            Vector2 scale = new Vector2(squishX, squishY) * Scale;
            
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * opacity,
                0, tex.Size() / 2f, scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A flare shine particle for dramatic impacts and death effects.
    /// Creates a bright cross/star flare effect.
    /// Inspired by Infernum's FlareShine particle.
    /// </summary>
    public class FlareShineParticle : Particle
    {
        public override string Texture => "Sparkle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        public override bool Important => true;

        private Color MainColor;
        private Color BloomColor;
        private Vector2 StartScale;
        private Vector2 EndScale;
        private float Spin;

        public FlareShineParticle(Vector2 position, Vector2 velocity, Color mainColor, Color bloomColor, float startScale, Vector2 endScale, int lifetime, float rotationSpeed = 0f)
        {
            Position = position;
            Velocity = velocity;
            MainColor = mainColor;
            BloomColor = bloomColor;
            StartScale = new Vector2(startScale);
            EndScale = endScale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed;
        }

        public override void Update()
        {
            Rotation += Spin;
            
            // Scale up over lifetime
            float progress = LifetimeCompletion;
            Scale = MathHelper.Lerp(StartScale.X, EndScale.X, progress);
            
            // Peak brightness in the middle, then fade
            float brightness;
            if (progress < 0.3f)
                brightness = progress / 0.3f;
            else
                brightness = 1f - ((progress - 0.3f) / 0.7f);
            
            Color = MainColor * brightness;
            
            Lighting.AddLight(Position, BloomColor.R / 255f * brightness, BloomColor.G / 255f * brightness, BloomColor.B / 255f * brightness);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D sparkTex = ParticleTextureHelper.GetTexture(Texture);
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            
            float progress = LifetimeCompletion;
            float brightness;
            if (progress < 0.3f)
                brightness = progress / 0.3f;
            else
                brightness = 1f - ((progress - 0.3f) / 0.7f);
            
            // Draw bloom behind
            float bloomScale = Scale * 2f * (bloomTex.Width > 0 ? (float)sparkTex.Width / bloomTex.Width : 1f);
            spriteBatch.Draw(bloomTex, Position - Main.screenPosition, null, BloomColor * brightness * 0.5f,
                0, bloomTex.Size() / 2f, bloomScale, SpriteEffects.None, 0);
            
            // Draw cross flare
            spriteBatch.Draw(sparkTex, Position - Main.screenPosition, null, Color * brightness,
                Rotation, sparkTex.Size() / 2f, Scale, SpriteEffects.None, 0);
            spriteBatch.Draw(sparkTex, Position - Main.screenPosition, null, Color * brightness,
                Rotation + MathHelper.PiOver2, sparkTex.Size() / 2f, Scale * 0.7f, SpriteEffects.None, 0);
            
            // White core
            spriteBatch.Draw(sparkTex, Position - Main.screenPosition, null, Color.White * brightness * 0.8f,
                Rotation, sparkTex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// Critical hit spark for impacts - creates a bright burst effect.
    /// Inspired by Infernum's CritSpark particle.
    /// </summary>
    public class CritSparkParticle : Particle
    {
        public override string Texture => "Sparkle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private Color BaseColor;
        private Color SecondaryColor;
        private float MaxScale;
        private float DecayRate;
        private float BloomScale;

        public CritSparkParticle(Vector2 position, Vector2 velocity, Color baseColor, Color secondaryColor, float scale, int lifetime, float decayRate = 0.01f, float bloomScale = 2f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = baseColor;
            SecondaryColor = secondaryColor;
            Scale = scale;
            MaxScale = scale;
            Lifetime = lifetime;
            DecayRate = decayRate;
            BloomScale = bloomScale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            Scale = MaxScale * (1f - LifetimeCompletion);
            
            // Color transition
            Color = Color.Lerp(BaseColor, SecondaryColor, LifetimeCompletion);
            
            Rotation += DecayRate * 5f;
            
            float brightness = 1f - LifetimeCompletion;
            Lighting.AddLight(Position, Color.R / 255f * brightness, Color.G / 255f * brightness, Color.B / 255f * brightness);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D sparkTex = ParticleTextureHelper.GetTexture(Texture);
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            
            float opacity = 1f - LifetimeCompletion;
            
            // Bloom
            float bScale = Scale * BloomScale * (bloomTex.Width > 0 ? (float)sparkTex.Width / bloomTex.Width : 1f);
            spriteBatch.Draw(bloomTex, Position - Main.screenPosition, null, SecondaryColor * opacity * 0.4f,
                0, bloomTex.Size() / 2f, bScale, SpriteEffects.None, 0);
            
            // Spark
            spriteBatch.Draw(sparkTex, Position - Main.screenPosition, null, Color * opacity,
                Rotation, sparkTex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// Mist/fire particle for atmospheric effects.
    /// Creates soft, drifting fire or mist clouds.
    /// Inspired by Infernum's MediumMistParticle.
    /// </summary>
    public class MistFireParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => false;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private Color StartColor;
        private Color EndColor;
        private float Spin;
        private float OriginalScale;

        public MistFireParticle(Vector2 position, Vector2 velocity, Color startColor, Color endColor, float scale, float lifetime, float rotationSpeed = 0.02f)
        {
            Position = position;
            Velocity = velocity;
            StartColor = startColor;
            EndColor = endColor;
            Scale = scale;
            OriginalScale = scale;
            Lifetime = (int)lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed * (Main.rand.NextBool() ? 1f : -1f);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Rotation += Spin;
            
            // Expand slightly
            Scale = OriginalScale * (1f + LifetimeCompletion * 0.4f);
            
            // Color and opacity transition
            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
            Color = Color.Lerp(StartColor, EndColor, LifetimeCompletion) * opacity;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color,
                Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }
}
