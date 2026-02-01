using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// Helper class for safely loading particle textures.
    /// Uses procedurally generated textures from ParticleTextureGenerator.
    /// </summary>
    public static class ParticleTextureHelper
    {
        /// <summary>
        /// Gets a particle texture by path. Uses generated textures when available.
        /// </summary>
        public static Texture2D GetTexture(string texturePath)
        {
            // Extract texture name from path
            string textureName = texturePath;
            int lastSlash = texturePath.LastIndexOf('/');
            if (lastSlash >= 0)
                textureName = texturePath.Substring(lastSlash + 1);
            
            // Try to get generated texture
            Texture2D generated = ParticleTextureGenerator.GetTexture(textureName);
            if (generated != null)
                return generated;
            
            // Try to load from mod assets
            try
            {
                if (ModContent.HasAsset(texturePath))
                    return ModContent.Request<Texture2D>(texturePath).Value;
            }
            catch { }
            
            // Fallback to magic pixel
            return TextureAssets.MagicPixel.Value;
        }
        
        /// <summary>
        /// Gets a specific generated texture by name.
        /// </summary>
        public static Texture2D GetGeneratedTexture(string name)
        {
            return ParticleTextureGenerator.GetTexture(name) ?? TextureAssets.MagicPixel.Value;
        }
        
        /// <summary>
        /// Checks if a texture exists (either generated or as an asset).
        /// </summary>
        public static bool TextureExists(string texturePath)
        {
            string textureName = texturePath;
            int lastSlash = texturePath.LastIndexOf('/');
            if (lastSlash >= 0)
                textureName = texturePath.Substring(lastSlash + 1);
            
            if (ParticleTextureGenerator.GetTexture(textureName) != null)
                return true;
                
            try
            {
                return ModContent.HasAsset(texturePath);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A circular bloom particle that expands and fades.
    /// Perfect for explosion cores, magic impact effects, and energy bursts.
    /// </summary>
    public class BloomParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private float OriginalScale;
        private float FinalScale;
        private float opacity;
        private Color BaseColor;
        private bool Fade;

        /// <summary>
        /// Creates a bloom particle with expanding scale animation.
        /// </summary>
        public BloomParticle(Vector2 position, Vector2 velocity, Color color, float originalScale, float finalScale, int lifeTime, bool fade = true)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            OriginalScale = originalScale;
            FinalScale = finalScale;
            Scale = originalScale;
            Lifetime = lifeTime;
            Fade = fade;
        }
        
        /// <summary>
        /// Simple constructor for bloom particle with default expansion.
        /// </summary>
        public BloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime)
            : this(position, velocity, color, scale, scale * 1.5f, lifeTime, true)
        {
        }

        public override void Update()
        {
            float pulseProgress = PiecewiseAnimation(LifetimeCompletion, new CurveSegment[] {
                new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4)
            });
            Scale = MathHelper.Lerp(OriginalScale, FinalScale, pulseProgress);
            
            if (Fade)
                opacity = (float)Math.Sin(MathHelper.PiOver2 + LifetimeCompletion * MathHelper.PiOver2);
            else
                opacity = 1f;

            Color = BaseColor * (Fade ? opacity : 1f);
            Lighting.AddLight(Position, Color.R / 255f * 0.5f, Color.G / 255f * 0.5f, Color.B / 255f * 0.5f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            // FARGOS PATTERN: Multi-layer bloom stacking with { A = 0 }
            // Draw 4 layers from largest (faint) to smallest (bright)
            float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };
            float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };
            float baseAlpha = Fade ? opacity : 1f;
            
            // CRITICAL: Remove alpha channel for proper additive blending
            Color bloomColor = Color with { A = 0 };
            
            for (int i = 0; i < 4; i++)
            {
                float layerScale = Scale * scales[i];
                float layerAlpha = baseAlpha * opacities[i];
                spriteBatch.Draw(tex, drawPos, null, bloomColor * layerAlpha, 
                    0, origin, layerScale, SpriteEffects.None, 0);
            }
        }
    }

    /// <summary>
    /// An expanding ring effect for shockwaves and pulse effects.
    /// </summary>
    public class BloomRingParticle : Particle
    {
        public override string Texture => "BloomRing";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private float opacity;
        private Color BaseColor;
        private float ExpansionRate;

        public BloomRingParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime, float expansionRate = 0.05f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Scale = scale;
            Lifetime = lifeTime;
            ExpansionRate = expansionRate;
        }

        public override void Update()
        {
            opacity = 1f - LifetimeCompletion;
            Scale += ExpansionRate;
            Color = BaseColor * opacity;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            // FARGOS PATTERN: Multi-layer bloom for ring with { A = 0 }
            // Ring uses fewer layers for performance but still gets the bloom look
            Color bloomColor = Color with { A = 0 };
            
            // Outer glow layer
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.4f), 
                Rotation, origin, Scale * 1.3f, SpriteEffects.None, 0);
            // Main ring layer
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.7f), 
                Rotation, origin, Scale, SpriteEffects.None, 0);
            // Inner bright layer
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.9f), 
                Rotation, origin, Scale * 0.7f, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A sparkle particle with a bloom circle behind it.
    /// Great for magical effects, hit sparks, and energy particles.
    /// </summary>
    public class SparkleParticle : Particle
    {
        public override string Texture => "Sparkle";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        private float Spin;
        private float opacity;
        private Color Bloom;
        private float BloomScale;

        /// <summary>
        /// Full constructor with all options.
        /// </summary>
        public SparkleParticle(Vector2 position, Vector2 velocity, Color color, Color bloom, float scale, int lifeTime, float rotationSpeed = 1f, float bloomScale = 1f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Bloom = bloom;
            Scale = scale;
            Lifetime = lifeTime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = rotationSpeed;
            BloomScale = bloomScale;
        }
        
        /// <summary>
        /// Simple constructor - bloom color matches main color.
        /// </summary>
        public SparkleParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime)
            : this(position, velocity, color, color * 0.7f, scale, lifeTime, 0.08f, 1.5f)
        {
        }

        public override void Update()
        {
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            Lighting.AddLight(Position, Bloom.R / 255f * opacity, Bloom.G / 255f * opacity, Bloom.B / 255f * opacity);
            Velocity *= 0.95f;
            Rotation += Spin * ((Velocity.X > 0) ? 1f : -1f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D starTexture = ParticleTextureHelper.GetTexture(Texture);
            Texture2D bloomTexture = ParticleTextureHelper.GetTexture("BloomCircle");
            
            float properBloomSize = (float)starTexture.Height / (float)bloomTexture.Height;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // FARGOS PATTERN: Remove alpha channel for proper additive blending
            Color bloomDrawColor = Bloom with { A = 0 };
            Color sparkleDrawColor = Color with { A = 0 };

            // Draw multi-layer bloom behind (Fargos style)
            // Layer 1: Outer soft glow
            spriteBatch.Draw(bloomTexture, drawPos, null, bloomDrawColor * opacity * 0.25f, 
                0, bloomTexture.Size() / 2f, Scale * BloomScale * properBloomSize * 1.5f, SpriteEffects.None, 0);
            // Layer 2: Main bloom
            spriteBatch.Draw(bloomTexture, drawPos, null, bloomDrawColor * opacity * 0.5f, 
                0, bloomTexture.Size() / 2f, Scale * BloomScale * properBloomSize, SpriteEffects.None, 0);
            // Layer 3: Inner bright bloom
            spriteBatch.Draw(bloomTexture, drawPos, null, bloomDrawColor * opacity * 0.7f, 
                0, bloomTexture.Size() / 2f, Scale * BloomScale * properBloomSize * 0.6f, SpriteEffects.None, 0);
            
            // Draw sparkle
            spriteBatch.Draw(starTexture, drawPos, null, sparkleDrawColor * opacity, 
                Rotation, starTexture.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// An elongated spark particle that looks like it's stretching in motion.
    /// Perfect for electricity, speed lines, and energy trails.
    /// </summary>
    public class GlowSparkParticle : Particle
    {
        public override string Texture => "GlowSpark";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        public Color InitialColor;
        public bool AffectedByGravity;
        public bool QuickShrink;
        public bool Glowing;
        public Vector2 Squash = new Vector2(0.5f, 1.6f);

        /// <summary>
        /// Full constructor with all options.
        /// </summary>
        public GlowSparkParticle(Vector2 position, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color, Vector2 squash, bool quickShrink = false, bool glow = true)
        {
            Position = position;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Lifetime = lifetime;
            Scale = scale;
            InitialColor = color;
            Color = color;
            Squash = squash;
            QuickShrink = quickShrink;
            Glowing = glow;
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }
        
        /// <summary>
        /// Simple constructor with sensible defaults.
        /// </summary>
        public GlowSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime)
            : this(position, velocity, false, lifeTime, scale, color, new Vector2(0.5f, 1.6f), false, true)
        {
        }

        public override void Update()
        {
            if (AffectedByGravity)
                Velocity.Y += 0.1f;

            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            Color = Color.Lerp(InitialColor, Color.Transparent, LifetimeCompletion);

            if (QuickShrink)
                Scale *= 0.95f;

            Lighting.AddLight(Position, Color.R / 255f * 0.3f, Color.G / 255f * 0.3f, Color.B / 255f * 0.3f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = Squash * Scale;
            Texture2D texture = ParticleTextureHelper.GetTexture(Texture);

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color, 
                Rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            
            if (Glowing)
            {
                spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color.White * (1f - LifetimeCompletion), 
                    Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), SpriteEffects.None, 0f);
            }
        }
    }

    /// <summary>
    /// A generic soft glow particle for ambient effects.
    /// </summary>
    public class GenericGlowParticle : Particle
    {
        public override string Texture => "SoftGlow";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        private float opacity;
        private Color BaseColor;
        private bool ProduceLight;

        public GenericGlowParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifeTime, bool produceLight = true)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Scale = scale;
            Lifetime = lifeTime;
            ProduceLight = produceLight;
        }

        public override void Update()
        {
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            Color = BaseColor * opacity;
            Velocity *= 0.97f;

            if (ProduceLight)
                Lighting.AddLight(Position, Color.R / 255f * opacity * 0.5f, Color.G / 255f * opacity * 0.5f, Color.B / 255f * opacity * 0.5f);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;
            
            // FARGOS PATTERN: Multi-layer bloom with { A = 0 }
            Color bloomColor = Color with { A = 0 };
            
            // Layer 1: Outer soft glow
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.3f), 
                Rotation, origin, Scale * 1.6f, SpriteEffects.None, 0);
            // Layer 2: Mid glow
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.5f), 
                Rotation, origin, Scale * 1.2f, SpriteEffects.None, 0);
            // Layer 3: Main glow
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.75f), 
                Rotation, origin, Scale, SpriteEffects.None, 0);
            // Layer 4: Inner bright core
            spriteBatch.Draw(tex, drawPos, null, bloomColor * (opacity * 0.9f), 
                Rotation, origin, Scale * 0.5f, SpriteEffects.None, 0);
        }
    }

    /// <summary>
    /// A point/line particle that stretches in the direction of movement.
    /// Great for speed lines and directional sparks.
    /// </summary>
    public class PointParticle : Particle
    {
        public override string Texture => "Point";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;

        private float opacity;
        public bool AffectedByGravity;

        public PointParticle(Vector2 position, Vector2 velocity, bool affectedByGravity, int lifetime, float scale, Color color)
        {
            Position = position;
            Velocity = velocity;
            AffectedByGravity = affectedByGravity;
            Lifetime = lifetime;
            Scale = scale;
            Color = color;
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            if (AffectedByGravity)
                Velocity.Y += 0.15f;

            opacity = 1f - LifetimeCompletion;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Vector2 scale = new Vector2(0.5f, 1.6f) * Scale;
            Texture2D texture = ParticleTextureHelper.GetTexture(Texture);

            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color * opacity, 
                Rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, Position - Main.screenPosition, null, Color * opacity, 
                Rotation, texture.Size() * 0.5f, scale * new Vector2(0.45f, 1f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Heavy smoke particle with frame animation.
    /// </summary>
    public class HeavySmokeParticle : Particle
    {
        public override bool SetLifetime => true;
        public override int FrameVariants => 7;
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => Glowing;
        public override bool UseHalfTransparency => !Glowing;
        public override string Texture => "HeavySmoke";

        private float Opacity;
        private float Spin;
        private bool Glowing;

        public HeavySmokeParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f, bool glowing = false)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Lifetime = lifetime;
            Scale = scale;
            Opacity = opacity;
            Spin = rotationSpeed;
            Glowing = glowing;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Variant = Main.rand.Next(6);
        }

        public override void Update()
        {
            Opacity *= 0.97f;
            Scale *= 1.01f;
            Velocity *= 0.95f;
            Rotation += Spin;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ParticleTextureHelper.GetTexture(Texture);
            Rectangle frame = tex.Frame(1, 7, 0, Variant);
            spriteBatch.Draw(tex, Position - Main.screenPosition, frame, Color * Opacity, 
                Rotation, frame.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// A directional line particle for laser-like effects, speed lines, etc.
    /// </summary>
    public class LineParticle : Particle
    {
        public override string Texture => "Point";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private float opacity;
        private float length;
        private float thickness;
        private Color BaseColor;
        
        public LineParticle(Vector2 position, Vector2 velocity, Color color, float length, float thickness, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Color = color;
            this.length = length;
            this.thickness = thickness;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation();
        }
        
        public override void Update()
        {
            opacity = 1f - LifetimeCompletion;
            Color = BaseColor * opacity;
            Rotation = Velocity.ToRotation();
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Draw line using scaled pixel
            spriteBatch.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), Color * opacity,
                Rotation, new Vector2(0, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
            
            // Inner glow
            spriteBatch.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), Color.White * opacity * 0.5f,
                Rotation, new Vector2(0, 0.5f), new Vector2(length, thickness * 0.4f), SpriteEffects.None, 0f);
        }
    }
    
    #region Musical Particles
    
    /// <summary>
    /// A floating music note particle that drifts upward and fades.
    /// Perfect for ambient musical effects, weapon trails, and magical audio themes.
    /// </summary>
    public class MusicNoteParticle : Particle
    {
        public override string Texture => _noteType;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private string _noteType;
        private float opacity;
        private Color BaseColor;
        private Color BloomColor;
        private float Spin;
        private float Wobble;
        private float WobbleSpeed;
        private float OriginalX;
        
        /// <summary>
        /// Creates a music note particle.
        /// </summary>
        /// <param name="position">Start position</param>
        /// <param name="velocity">Initial velocity (typically upward)</param>
        /// <param name="color">Main note color</param>
        /// <param name="bloomColor">Glow color behind note</param>
        /// <param name="scale">Size multiplier</param>
        /// <param name="lifetime">Duration in frames</param>
        /// <param name="noteType">Type: "MusicNoteQuarter", "MusicNoteEighth", "MusicNoteSixteenth", "MusicNoteDouble"</param>
        public MusicNoteParticle(Vector2 position, Vector2 velocity, Color color, Color bloomColor, float scale, int lifetime, string noteType = "MusicNoteQuarter")
        {
            Position = position;
            OriginalX = position.X;
            Velocity = velocity;
            BaseColor = color;
            BloomColor = bloomColor;
            Scale = scale;
            Lifetime = lifetime;
            _noteType = noteType;
            Rotation = Main.rand.NextFloat(-0.2f, 0.2f);
            Spin = Main.rand.NextFloat(-0.03f, 0.03f);
            Wobble = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            WobbleSpeed = Main.rand.NextFloat(0.05f, 0.12f);
        }
        
        /// <summary>
        /// Simple constructor with random note type.
        /// </summary>
        public MusicNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
            : this(position, velocity, color, color * 0.5f, scale, lifetime, GetRandomNoteType())
        {
        }
        
        private static string GetRandomNoteType()
        {
            return Main.rand.Next(4) switch
            {
                0 => "MusicNoteQuarter",
                1 => "MusicNoteEighth",
                2 => "MusicNoteSixteenth",
                _ => "MusicNoteDouble"
            };
        }
        
        public override void Update()
        {
            // Fade in then out
            if (LifetimeCompletion < 0.2f)
                opacity = LifetimeCompletion / 0.2f;
            else
                opacity = 1f - ((LifetimeCompletion - 0.2f) / 0.8f);
            
            // Gentle wobble side to side
            Wobble += WobbleSpeed;
            Position = new Vector2(OriginalX + (float)Math.Sin(Wobble) * 8f, Position.Y);
            OriginalX += Velocity.X;
            
            // Slow rotation
            Rotation += Spin;
            
            // Slow down over time
            Velocity *= 0.98f;
            
            // Light emission
            if (opacity > 0.3f)
                Lighting.AddLight(Position, BloomColor.R / 255f * opacity * 0.4f, BloomColor.G / 255f * opacity * 0.4f, BloomColor.B / 255f * opacity * 0.4f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D noteTexture = ParticleTextureHelper.GetTexture(_noteType);
            Texture2D bloomTexture = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Draw bloom glow behind (subtle, not overwhelming)
            float bloomScale = Scale * 1.4f;
            spriteBatch.Draw(bloomTexture, drawPos, null, BloomColor * opacity * 0.45f,
                0f, bloomTexture.Size() / 2f, bloomScale, SpriteEffects.None, 0f);
            
            // Draw note
            spriteBatch.Draw(noteTexture, drawPos, null, BaseColor * opacity,
                Rotation, noteTexture.Size() / 2f, Scale, SpriteEffects.None, 0f);
            
            // Bright core
            spriteBatch.Draw(noteTexture, drawPos, null, Color.White * opacity * 0.4f,
                Rotation, noteTexture.Size() / 2f, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// A clef particle (treble or bass) for major musical events.
    /// </summary>
    public class ClefParticle : Particle
    {
        public override string Texture => _clefType;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private string _clefType;
        private float opacity;
        private Color BaseColor;
        private Color BloomColor;
        private float Spin;
        private float ExpandScale;
        private float OriginalScale;
        
        /// <summary>
        /// Creates a clef particle for dramatic musical effects.
        /// </summary>
        public ClefParticle(Vector2 position, Vector2 velocity, Color color, Color bloomColor, float scale, int lifetime, bool isTreble = true)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            BloomColor = bloomColor;
            Scale = scale;
            OriginalScale = scale;
            ExpandScale = scale * 1.5f;
            Lifetime = lifetime;
            _clefType = isTreble ? "TrebleClef" : "BassClef";
            Rotation = 0f;
            Spin = Main.rand.NextFloat(-0.01f, 0.01f);
        }
        
        public override void Update()
        {
            // Quick fade in, slow fade out
            if (LifetimeCompletion < 0.15f)
                opacity = LifetimeCompletion / 0.15f;
            else
                opacity = 1f - ((LifetimeCompletion - 0.15f) / 0.85f);
            
            // Expand slightly then shrink
            float expandProgress = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            Scale = MathHelper.Lerp(OriginalScale, ExpandScale, expandProgress * 0.3f);
            
            Rotation += Spin;
            Velocity *= 0.95f;
            
            if (opacity > 0.3f)
                Lighting.AddLight(Position, BloomColor.R / 255f * opacity * 0.6f, BloomColor.G / 255f * opacity * 0.6f, BloomColor.B / 255f * opacity * 0.6f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D clefTexture = ParticleTextureHelper.GetTexture(_clefType);
            Texture2D bloomTexture = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Large bloom behind
            spriteBatch.Draw(bloomTexture, drawPos, null, BloomColor * opacity * 0.5f,
                0f, bloomTexture.Size() / 2f, Scale * 2.5f, SpriteEffects.None, 0f);
            
            // Clef symbol
            spriteBatch.Draw(clefTexture, drawPos, null, BaseColor * opacity,
                Rotation, clefTexture.Size() / 2f, Scale, SpriteEffects.None, 0f);
            
            // Inner glow
            spriteBatch.Draw(clefTexture, drawPos, null, Color.White * opacity * 0.3f,
                Rotation, clefTexture.Size() / 2f, Scale * 0.7f, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Glowing musical staff lines that appear and fade.
    /// </summary>
    public class MusicStaffParticle : Particle
    {
        public override string Texture => "MusicStaff";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private float opacity;
        private Color BaseColor;
        private float ExpandWidth;
        private float OriginalWidth;
        
        public MusicStaffParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Scale = scale;
            OriginalWidth = 0.2f;
            ExpandWidth = 1f;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(-0.05f, 0.05f);
        }
        
        public override void Update()
        {
            // Fade in, hold, fade out
            if (LifetimeCompletion < 0.2f)
                opacity = LifetimeCompletion / 0.2f;
            else if (LifetimeCompletion > 0.7f)
                opacity = 1f - ((LifetimeCompletion - 0.7f) / 0.3f);
            else
                opacity = 1f;
            
            Velocity *= 0.96f;
            
            Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.3f, BaseColor.G / 255f * opacity * 0.3f, BaseColor.B / 255f * opacity * 0.3f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D staffTexture = ParticleTextureHelper.GetTexture("MusicStaff");
            Texture2D bloomTexture = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Soft glow
            spriteBatch.Draw(bloomTexture, drawPos, null, BaseColor * opacity * 0.3f,
                Rotation, bloomTexture.Size() / 2f, new Vector2(Scale * 2f, Scale * 0.8f), SpriteEffects.None, 0f);
            
            // Staff lines
            spriteBatch.Draw(staffTexture, drawPos, null, BaseColor * opacity,
                Rotation, staffTexture.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// A sharp or flat accidental particle.
    /// </summary>
    public class AccidentalParticle : Particle
    {
        public override string Texture => _isSharp ? "MusicSharp" : "MusicFlat";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private bool _isSharp;
        private float opacity;
        private Color BaseColor;
        private float Spin;
        
        public AccidentalParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, bool isSharp = true)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _isSharp = isSharp;
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            Spin = Main.rand.NextFloat(-0.04f, 0.04f);
        }
        
        public override void Update()
        {
            opacity = (float)Math.Sin(LifetimeCompletion * MathHelper.Pi);
            Rotation += Spin;
            Velocity *= 0.97f;
            
            if (opacity > 0.2f)
                Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.3f, BaseColor.G / 255f * opacity * 0.3f, BaseColor.B / 255f * opacity * 0.3f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ParticleTextureHelper.GetTexture(_isSharp ? "MusicSharp" : "MusicFlat");
            Texture2D bloomTexture = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Glow
            spriteBatch.Draw(bloomTexture, drawPos, null, BaseColor * opacity * 0.5f,
                0f, bloomTexture.Size() / 2f, Scale * 1.5f, SpriteEffects.None, 0f);
            
            // Symbol
            spriteBatch.Draw(texture, drawPos, null, BaseColor * opacity,
                Rotation, texture.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
    
    #endregion
    
    #region Nachtmusik Celestial Particles
    
    /// <summary>
    /// An 8-pointed star burst particle for Nachtmusik's celestial impacts.
    /// Uses StarBurst1.png or StarBurst2.png textures.
    /// </summary>
    public class StarBurstParticle : Particle
    {
        public override string Texture => _variant == 0 ? "StarBurst1" : "StarBurst2";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private int _variant;
        private float opacity;
        private Color BaseColor;
        private float Spin;
        private float OriginalScale;
        private float FinalScale;
        
        public StarBurstParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, int variant = -1)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            OriginalScale = scale;
            FinalScale = scale * 0.3f;
            Scale = scale;
            Lifetime = lifetime;
            _variant = variant < 0 ? Main.rand.Next(2) : variant;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = Main.rand.NextFloat(-0.08f, 0.08f);
        }
        
        public override void Update()
        {
            // Rapid fade-out curve for impact feel
            float fadeProgress = 1f - (LifetimeCompletion * LifetimeCompletion);
            opacity = fadeProgress;
            
            // Scale shrinks over time
            Scale = MathHelper.Lerp(OriginalScale, FinalScale, LifetimeCompletion);
            
            Rotation += Spin;
            Velocity *= 0.92f;
            
            if (opacity > 0.15f)
                Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.8f, BaseColor.G / 255f * opacity * 0.8f, BaseColor.B / 255f * opacity * 0.8f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ParticleTextureHelper.GetTexture(Texture);
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // FARGOS PATTERN: Multi-layer bloom with { A = 0 }
            Color bloomColor = BaseColor with { A = 0 };
            
            // Outer bloom glow
            spriteBatch.Draw(bloomTex, drawPos, null, bloomColor * opacity * 0.4f,
                0f, bloomTex.Size() / 2f, Scale * 2.2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(bloomTex, drawPos, null, bloomColor * opacity * 0.6f,
                0f, bloomTex.Size() / 2f, Scale * 1.4f, SpriteEffects.None, 0f);
            
            // Star burst sprite
            spriteBatch.Draw(texture, drawPos, null, Color.White * opacity * 0.9f,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
            
            // White-hot core
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * opacity * 0.7f,
                0f, bloomTex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Crystalline shattered starlight fragment particle for Nachtmusik weapon hits.
    /// Creates sharp, gleaming shards of light that scatter on impact.
    /// </summary>
    public class ShatteredStarlightParticle : Particle
    {
        public override string Texture => "ShatteredStarlight";
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;
        public override bool SetLifetime => true;
        
        private float opacity;
        private Color BaseColor;
        private float Spin;
        private bool HasGravity;
        private float GravityStrength;
        private Rectangle? SourceRect;
        
        /// <summary>
        /// Creates a shattered starlight fragment.
        /// </summary>
        /// <param name="position">Start position</param>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="color">Tint color (texture is grayscale)</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Frames until despawn</param>
        /// <param name="hasGravity">Whether fragment falls</param>
        /// <param name="gravityStrength">Gravity acceleration if enabled</param>
        public ShatteredStarlightParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, bool hasGravity = true, float gravityStrength = 0.15f)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            Scale = scale;
            Lifetime = lifetime;
            HasGravity = hasGravity;
            GravityStrength = gravityStrength;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Spin = Main.rand.NextFloat(-0.12f, 0.12f);
            
            // Random source rectangle from the sprite sheet (assumed 16 variations in a 4x4 grid of 32x32)
            int frameX = Main.rand.Next(4);
            int frameY = Main.rand.Next(4);
            SourceRect = new Rectangle(frameX * 32, frameY * 32, 32, 32);
        }
        
        public override void Update()
        {
            // Smooth fade out
            opacity = 1f - (LifetimeCompletion * LifetimeCompletion);
            
            Rotation += Spin;
            Spin *= 0.98f; // Slow down spin
            
            if (HasGravity)
                Velocity.Y += GravityStrength;
            
            Velocity *= 0.96f;
            
            if (opacity > 0.15f)
                Lighting.AddLight(Position, BaseColor.R / 255f * opacity * 0.5f, BaseColor.G / 255f * opacity * 0.5f, BaseColor.B / 255f * opacity * 0.5f);
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D texture = ParticleTextureHelper.GetTexture(Texture);
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            
            Vector2 drawPos = Position - Main.screenPosition;
            
            Rectangle sourceRect = SourceRect ?? new Rectangle(0, 0, 32, 32);
            Vector2 origin = new Vector2(16, 16); // Center of 32x32 frame
            
            // FARGOS PATTERN: Remove alpha for additive bloom
            Color bloomColor = BaseColor with { A = 0 };
            
            // Soft outer glow
            spriteBatch.Draw(bloomTex, drawPos, null, bloomColor * opacity * 0.35f,
                0f, bloomTex.Size() / 2f, Scale * 1.8f, SpriteEffects.None, 0f);
            
            // Fragment sprite (tinted)
            spriteBatch.Draw(texture, drawPos, sourceRect, BaseColor * opacity,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
            
            // Gleaming edge highlight
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * opacity * 0.5f,
                0f, bloomTex.Size() / 2f, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }
    
    #endregion
}
