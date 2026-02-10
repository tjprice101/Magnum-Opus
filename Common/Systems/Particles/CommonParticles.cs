using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
    /// Heavy smoke particle with frame animation - GALAXIA STYLE.
    /// 
    /// KEY DIFFERENCES FROM BEFORE:
    /// 1. BOTH layers use ADDITIVE blending (no muddy normal blend)
    /// 2. Frame ANIMATES over lifetime (cycles through spritesheet)
    /// 3. Uses { A = 0 } pattern for proper additive color
    /// 4. Vibrant saturated colors with chaos variation
    /// 5. Smooth interpolated position tracking
    /// </summary>
    public class HeavySmokeParticle : Particle
    {
        public override bool SetLifetime => true;
        public override int FrameVariants => 30;  // 5 columns x 6 rows
        public override bool UseCustomDraw => true;
        public override bool UseAdditiveBlend => true;  // ALWAYS additive for vibrant look
        public override string Texture => "MagnumOpus/Assets/Particles/HeavySmoke";

        private float Opacity;
        private float OriginalOpacity;
        private float Spin;
        private bool IsGlowLayer;
        private float HueShift;
        private float HueChaos;  // Random per-frame hue variation
        private int StartFrame;  // Starting frame for animation
        private float AnimationSpeed;  // How fast to cycle through frames
        private Vector2 PreviousPosition;  // For interpolation
        private Color OriginalColor;
        private float Saturation;

        /// <summary>
        /// Creates a Galaxia-style heavy smoke particle with frame animation.
        /// </summary>
        public HeavySmokeParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, 
            float opacity, float rotationSpeed = 0f, bool glowing = false, float hueShift = 0f, bool randomDelay = false)
        {
            Position = position;
            PreviousPosition = position;
            Velocity = velocity;
            OriginalColor = color;
            Color = color;
            Lifetime = lifetime;
            Scale = scale;
            Opacity = opacity;
            OriginalOpacity = opacity;
            Spin = rotationSpeed;
            IsGlowLayer = glowing;
            HueShift = hueShift;
            HueChaos = Main.rand.NextFloat(0.02f, 0.06f);  // Random chaos per particle
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            StartFrame = Main.rand.Next(30);  // Random start frame
            AnimationSpeed = Main.rand.NextFloat(0.3f, 0.6f);  // Frames per game tick
            Saturation = 1f + Main.rand.NextFloat(0.1f, 0.3f);  // Boost saturation
            
            // Random delay for staggered particle groups
            if (randomDelay)
                Time = Main.rand.Next(lifetime / 4);
        }

        public override void Update()
        {
            // Store previous position for interpolation
            PreviousPosition = Position;
            
            // Fade out over lifetime (exponential for smooth fade)
            float lifeProgress = (float)Time / Lifetime;
            Opacity = OriginalOpacity * (1f - lifeProgress * lifeProgress);
            
            // Grow slightly over time
            Scale *= 1.008f;
            
            // Slow down
            Velocity *= 0.94f;
            
            // Spin
            Rotation += Spin;
            
            // Galaxia-style hue shifting with chaos
            float totalHueShift = HueShift + (float)Math.Sin(Time * 0.2f) * HueChaos;
            if (totalHueShift != 0)
            {
                Vector3 hsl = Main.rgbToHsl(OriginalColor);
                float newHue = (hsl.X + totalHueShift * Time) % 1f;
                if (newHue < 0) newHue += 1f;
                // Boost saturation for vibrant colors
                float boostedSat = Math.Min(1f, hsl.Y * Saturation);
                // Boost lightness slightly for glow layer
                float lightness = IsGlowLayer ? Math.Min(1f, hsl.Z * 1.2f) : hsl.Z;
                Color = Main.hslToRgb(newHue, boostedSat, lightness);
            }
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            // Get texture - try mod asset first, then helper
            Texture2D tex = null;
            try
            {
                if (ModContent.HasAsset(Texture))
                {
                    tex = ModContent.Request<Texture2D>(Texture, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
            }
            catch { }
            
            // Fallback to helper
            if (tex == null)
                tex = ParticleTextureHelper.GetTexture(Texture);
            
            if (tex == null || tex.Width < 5 || tex.Height < 6) 
                return;  // Invalid texture
            
            // ANIMATE through frames based on time
            int currentFrame = (StartFrame + (int)(Time * AnimationSpeed)) % 30;
            int frameX = currentFrame % 5;  // Column (0-4)
            int frameY = currentFrame / 5;  // Row (0-5)
            
            // Calculate frame dimensions properly
            int frameWidth = tex.Width / 5;
            int frameHeight = tex.Height / 6;
            Rectangle frame = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            
            // Interpolate position for smooth 144Hz+ rendering
            float partialTicks = Main.GameUpdateCount % 1f;  // Approximate
            Vector2 drawPos = Vector2.Lerp(PreviousPosition, Position, 0.5f + partialTicks * 0.5f);
            drawPos -= Main.screenPosition;
            
            // BOOSTED OPACITY for visibility - additive needs high values to show up
            // Against bright backgrounds, additive effects are less visible
            float boostedOpacity = MathHelper.Clamp(Opacity * 1.8f, 0f, 1f);
            
            // ALWAYS use { A = 0 } for proper additive blending - NO muddy colors
            Color drawColor = Color with { A = 0 } * boostedOpacity;
            
            // Glow layer is brighter and slightly larger
            float drawScale = IsGlowLayer ? Scale * 1.2f : Scale;
            
            Vector2 origin = new Vector2(frameWidth, frameHeight) * 0.5f;
            
            // Draw main smoke cloud
            spriteBatch.Draw(tex, drawPos, frame, drawColor, 
                Rotation, origin, drawScale, SpriteEffects.None, 0f);
            
            // Second layer for more density
            spriteBatch.Draw(tex, drawPos, frame, drawColor * 0.6f, 
                Rotation + 0.5f, origin, drawScale * 0.85f, SpriteEffects.None, 0f);
            
            // Glow layer gets an extra bright core bloom
            if (IsGlowLayer && Opacity > 0.2f)
            {
                Color bloomColor = Color.White with { A = 0 } * (boostedOpacity * 0.5f);
                spriteBatch.Draw(tex, drawPos, frame, bloomColor, 
                    Rotation, origin, drawScale * 0.5f, SpriteEffects.None, 0f);
            }
        }
    }
    
    /// <summary>
    /// Helper class for spawning Galaxia-style dual-layer smoke effects.
    /// UPDATED: Both layers are additive for vibrant, non-muddy colors.
    /// </summary>
    public static class DualLayerSmoke
    {
        /// <summary>
        /// Spawns the Galaxia-style dual-layer smoke effect with vibrant additive blending.
        /// </summary>
        public static void Spawn(Vector2 position, Vector2 velocity, Color baseColor, Color glowColor, 
            int lifetime = 40, float scale = 0.5f, float baseOpacity = 0.75f, float glowOpacity = 0.4f, float hueShift = 0.01f)
        {
            // Boost color saturation for vibrant look
            Vector3 baseHsl = Main.rgbToHsl(baseColor);
            Vector3 glowHsl = Main.rgbToHsl(glowColor);
            baseColor = Main.hslToRgb(baseHsl.X, Math.Min(1f, baseHsl.Y * 1.3f), baseHsl.Z);
            glowColor = Main.hslToRgb(glowHsl.X, Math.Min(1f, glowHsl.Y * 1.3f), Math.Min(1f, glowHsl.Z * 1.1f));
            
            // Layer 1: Base smoke (additive, vibrant)
            var baseSmoke = new HeavySmokeParticle(
                position, velocity, baseColor, lifetime, scale,
                baseOpacity, Main.rand.NextFloat(-0.03f, 0.03f), 
                glowing: false, hueShift: hueShift * 0.5f, randomDelay: true);
            MagnumParticleHandler.SpawnParticle(baseSmoke);
            
            // Layer 2: Glow overlay (additive, brighter, faster hue shift)
            var glowSmoke = new HeavySmokeParticle(
                position + Main.rand.NextVector2Circular(3f, 3f), 
                velocity * 0.85f, glowColor, lifetime, scale * 1.1f,
                glowOpacity, Main.rand.NextFloat(-0.03f, 0.03f), 
                glowing: true, hueShift: hueShift * 1.5f, randomDelay: true);
            MagnumParticleHandler.SpawnParticle(glowSmoke);
        }
        
        /// <summary>
        /// Spawns dual-layer smoke with theme-based VIBRANT colors.
        /// </summary>
        public static void SpawnThemed(Vector2 position, Vector2 velocity, string theme, 
            int lifetime = 40, float scale = 0.5f, float intensity = 1f)
        {
            // Use more saturated, vibrant color pairs
            var (baseColor, glowColor) = theme.ToLower() switch
            {
                "phoenix" or "lacampanella" => (new Color(255, 120, 30), new Color(255, 200, 100)),
                "polaris" or "winter" => (new Color(80, 180, 255), new Color(180, 240, 255)),
                "aries" or "eroica" => (new Color(255, 80, 120), new Color(255, 180, 200)),
                "andromeda" or "fate" => (new Color(180, 80, 255), new Color(220, 160, 255)),
                "swanlake" => (new Color(220, 220, 255), new Color(255, 255, 255)),
                "enigma" => (new Color(120, 60, 200), new Color(180, 120, 255)),
                "moonlight" => (new Color(140, 100, 220), new Color(200, 180, 255)),
                _ => (new Color(200, 120, 255), new Color(240, 200, 255))
            };
            
            Spawn(position, velocity, baseColor, glowColor, lifetime, scale, 
                0.6f * intensity, 0.4f * intensity, 0.015f);
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
    
    #region TexturedParticle - Generic texture-based particle
    
    /// <summary>
    /// A generic particle that uses an externally provided texture (like CrossParticle PNGs or MusicNote PNGs).
    /// Supports rotation, spin, and multi-layer bloom rendering.
    /// </summary>
    public class TexturedParticle : Particle
    {
        public override string Texture => "BloomCircle"; // Fallback only
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        private Texture2D _customTexture;
        private Color BaseColor;
        private float OriginalScale;
        private float opacity;
        private float Spin;
        
        /// <summary>
        /// Creates a textured particle with custom texture.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="texture">The custom texture to use</param>
        /// <param name="color">Tint color</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Lifetime in frames</param>
        /// <param name="rotation">Initial rotation</param>
        /// <param name="spin">Rotation speed per frame</param>
        public TexturedParticle(Vector2 position, Vector2 velocity, Texture2D texture, Color color, float scale, int lifetime, float rotation = 0f, float spin = 0f)
        {
            Position = position;
            Velocity = velocity;
            _customTexture = texture;
            BaseColor = color;
            OriginalScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = rotation;
            Spin = spin;
            opacity = 1f;
        }
        
        public override void Update()
        {
            // Smooth fade out with slight expansion
            float progress = LifetimeCompletion;
            opacity = 1f - (progress * progress);
            Scale = OriginalScale * (1f + progress * 0.2f); // Slight expansion
            
            Rotation += Spin;
            Velocity *= 0.97f;
            
            // Subtle lighting
            if (opacity > 0.2f)
            {
                float lightIntensity = opacity * 0.4f;
                Lighting.AddLight(Position, BaseColor.R / 255f * lightIntensity, BaseColor.G / 255f * lightIntensity, BaseColor.B / 255f * lightIntensity);
            }
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            if (_customTexture == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = _customTexture.Size() / 2f;
            
            // FARGOS PATTERN: Remove alpha for proper additive blending
            Color bloomColor = BaseColor with { A = 0 };
            
            // Outer bloom layer (large, soft)
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            spriteBatch.Draw(bloomTex, drawPos, null, bloomColor * opacity * 0.25f,
                0f, bloomTex.Size() / 2f, Scale * 2.5f, SpriteEffects.None, 0f);
            
            // Multi-layer bloom stack for the texture
            float[] scales = { 1.4f, 1.15f, 1.0f };
            float[] opacities = { 0.3f, 0.5f, 0.85f };
            
            for (int i = 0; i < 3; i++)
            {
                float layerScale = Scale * scales[i];
                float layerAlpha = opacity * opacities[i];
                Color layerColor = i < 2 ? bloomColor : BaseColor;
                
                spriteBatch.Draw(_customTexture, drawPos, null, layerColor * layerAlpha,
                    Rotation, origin, layerScale, SpriteEffects.None, 0f);
            }
            
            // White center highlight for visibility
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * opacity * 0.4f,
                0f, bloomTex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Specialized vine rose particle for Ode to Joy theme.
    /// Uses the three custom vine textures: VineWithNoRoses, VineWithRoseOnTop, VineWithTwoRoses
    /// </summary>
    public class VineRoseParticle : Particle
    {
        public override string Texture => "BloomCircle"; // Fallback only
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        public enum VineType
        {
            NoRoses,
            RoseOnTop,
            TwoRoses
        }
        
        private VineType _vineType;
        private Color BaseColor;
        private Color AccentColor;
        private float OriginalScale;
        private float opacity;
        private float Spin;
        private static Texture2D[] _vineTextures;
        
        /// <summary>
        /// Creates a vine rose particle with the specified type.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="vineType">Which vine texture to use (NoRoses, RoseOnTop, or TwoRoses)</param>
        /// <param name="color">Primary tint color (vine color)</param>
        /// <param name="accentColor">Accent color for the rose highlight</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Lifetime in frames</param>
        /// <param name="rotation">Initial rotation</param>
        /// <param name="spin">Rotation speed per frame</param>
        public VineRoseParticle(Vector2 position, Vector2 velocity, VineType vineType, Color color, Color accentColor, float scale, int lifetime, float rotation = 0f, float spin = 0f)
        {
            Position = position;
            Velocity = velocity;
            _vineType = vineType;
            BaseColor = color;
            AccentColor = accentColor;
            OriginalScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = rotation;
            Spin = spin;
            opacity = 1f;
            
            // Lazy load textures
            LoadTexturesIfNeeded();
        }
        
        private static void LoadTexturesIfNeeded()
        {
            if (_vineTextures == null)
            {
                _vineTextures = new Texture2D[3];
            }
            
            if (_vineTextures[0] == null)
            {
                try
                {
                    _vineTextures[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/VineWithNoRoses", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    _vineTextures[1] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/VineWithRoseOnTop", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    _vineTextures[2] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/VineWithTwoRoses", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    // Fallback if textures don't exist yet
                }
            }
        }
        
        /// <summary>
        /// Creates a vine particle with a random type.
        /// </summary>
        public static VineRoseParticle CreateRandom(Vector2 position, Vector2 velocity, Color vineColor, Color roseColor, float scale, int lifetime, float rotation = 0f, float spin = 0f)
        {
            VineType type = Main.rand.Next(3) switch
            {
                0 => VineType.NoRoses,
                1 => VineType.RoseOnTop,
                _ => VineType.TwoRoses
            };
            return new VineRoseParticle(position, velocity, type, vineColor, roseColor, scale, lifetime, rotation, spin);
        }
        
        public override void Update()
        {
            // Smooth fade out with growth effect
            float progress = LifetimeCompletion;
            
            // Fade in quickly, then fade out
            if (progress < 0.2f)
                opacity = progress / 0.2f;
            else
                opacity = 1f - ((progress - 0.2f) / 0.8f);
            
            // Vine growth effect - starts smaller, grows, then shrinks
            float growthCurve = (float)Math.Sin(progress * Math.PI);
            Scale = OriginalScale * (0.7f + growthCurve * 0.5f);
            
            Rotation += Spin;
            Velocity *= 0.96f;
            
            // Lighting with vine green tint
            if (opacity > 0.2f)
            {
                float lightIntensity = opacity * 0.5f;
                Color lightColor = Color.Lerp(BaseColor, AccentColor, 0.3f);
                Lighting.AddLight(Position, lightColor.R / 255f * lightIntensity, lightColor.G / 255f * lightIntensity, lightColor.B / 255f * lightIntensity);
            }
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            LoadTexturesIfNeeded();
            
            int textureIndex = _vineType switch
            {
                VineType.NoRoses => 0,
                VineType.RoseOnTop => 1,
                VineType.TwoRoses => 2,
                _ => 0
            };
            
            Texture2D texture = _vineTextures?[textureIndex];
            if (texture == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // FARGOS PATTERN: Remove alpha for proper additive blending
            Color vineBloom = BaseColor with { A = 0 };
            Color roseBloom = AccentColor with { A = 0 };
            
            // Outer soft bloom (vine colored)
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            spriteBatch.Draw(bloomTex, drawPos, null, vineBloom * opacity * 0.2f,
                0f, bloomTex.Size() / 2f, Scale * 3f, SpriteEffects.None, 0f);
            
            // Rose accent bloom (for vine types with roses)
            if (_vineType != VineType.NoRoses)
            {
                spriteBatch.Draw(bloomTex, drawPos, null, roseBloom * opacity * 0.3f,
                    0f, bloomTex.Size() / 2f, Scale * 2f, SpriteEffects.None, 0f);
            }
            
            // Multi-layer bloom stack for the vine texture
            // Layer 1: Large outer glow (vine color)
            spriteBatch.Draw(texture, drawPos, null, vineBloom * opacity * 0.3f,
                Rotation, origin, Scale * 1.5f, SpriteEffects.None, 0f);
            
            // Layer 2: Medium glow (vine color)
            spriteBatch.Draw(texture, drawPos, null, vineBloom * opacity * 0.5f,
                Rotation, origin, Scale * 1.2f, SpriteEffects.None, 0f);
            
            // Layer 3: Core texture (white tinted)
            Color coreColor = Color.Lerp(BaseColor, Color.White, 0.5f);
            spriteBatch.Draw(texture, drawPos, null, coreColor * opacity * 0.9f,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
            
            // White highlight sparkle at center
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * opacity * 0.35f,
                0f, bloomTex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
    
    /// <summary>
    /// Rose bud particle for Ode to Joy theme explosions and impacts.
    /// Uses the RosesBud.png texture for beautiful rose bloom effects.
    /// Perfect for projectile impacts, explosion centers, and boss death effects.
    /// </summary>
    public class RoseBudParticle : Particle
    {
        public override string Texture => "BloomCircle"; // Fallback only
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        private Color BaseColor;
        private Color AccentColor;
        private float OriginalScale;
        private float opacity;
        private float Spin;
        private bool _isBloomPhase; // For opening/blooming animation
        private static Texture2D _roseBudTexture;
        
        /// <summary>
        /// Creates a rose bud particle.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="color">Primary tint color (petal color)</param>
        /// <param name="accentColor">Accent color for the glow</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Lifetime in frames</param>
        /// <param name="rotation">Initial rotation</param>
        /// <param name="spin">Rotation speed per frame</param>
        /// <param name="bloomPhase">If true, starts small and blooms open; if false, starts full and fades</param>
        public RoseBudParticle(Vector2 position, Vector2 velocity, Color color, Color accentColor, float scale, int lifetime, float rotation = 0f, float spin = 0f, bool bloomPhase = false)
        {
            Position = position;
            Velocity = velocity;
            BaseColor = color;
            AccentColor = accentColor;
            OriginalScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = rotation;
            Spin = spin;
            _isBloomPhase = bloomPhase;
            opacity = 1f;
            
            LoadTextureIfNeeded();
        }
        
        private static void LoadTextureIfNeeded()
        {
            if (_roseBudTexture == null)
            {
                try
                {
                    _roseBudTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/RosesBud", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    // Fallback if texture doesn't exist yet
                }
            }
        }
        
        /// <summary>
        /// Creates a rose bud with random initial rotation and spin.
        /// </summary>
        public static RoseBudParticle CreateRandom(Vector2 position, Vector2 velocity, Color petalColor, Color glowColor, float scale, int lifetime, bool bloomPhase = false)
        {
            float rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            float spin = Main.rand.NextFloat(-0.08f, 0.08f);
            return new RoseBudParticle(position, velocity, petalColor, glowColor, scale, lifetime, rotation, spin, bloomPhase);
        }
        
        /// <summary>
        /// Creates a rose bud burst of multiple particles.
        /// </summary>
        public static void SpawnBurst(Vector2 position, int count, float speed, Color petalColor, Color glowColor, float scale, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                float burstSpeed = speed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * burstSpeed;
                
                var roseBud = CreateRandom(position, vel, petalColor, glowColor, scale * Main.rand.NextFloat(0.8f, 1.2f), lifetime);
                MagnumParticleHandler.SpawnParticle(roseBud);
            }
        }
        
        public override void Update()
        {
            float progress = LifetimeCompletion;
            
            if (_isBloomPhase)
            {
                // Bloom animation: starts small, grows, holds, then fades
                if (progress < 0.3f)
                {
                    Scale = OriginalScale * (progress / 0.3f);
                    opacity = progress / 0.3f;
                }
                else if (progress < 0.7f)
                {
                    Scale = OriginalScale;
                    opacity = 1f;
                }
                else
                {
                    Scale = OriginalScale * (1f - (progress - 0.7f) / 0.3f);
                    opacity = 1f - (progress - 0.7f) / 0.3f;
                }
            }
            else
            {
                // Standard fade: fade in quickly, then fade out
                if (progress < 0.15f)
                    opacity = progress / 0.15f;
                else
                    opacity = 1f - ((progress - 0.15f) / 0.85f);
                
                // Gentle scale pulse
                Scale = OriginalScale * (1f + (float)Math.Sin(progress * Math.PI * 2) * 0.1f);
            }
            
            Rotation += Spin;
            Velocity *= 0.97f;
            
            // Petal lighting
            if (opacity > 0.2f)
            {
                float lightIntensity = opacity * 0.6f;
                Color lightColor = Color.Lerp(BaseColor, AccentColor, 0.4f);
                Lighting.AddLight(Position, lightColor.R / 255f * lightIntensity, lightColor.G / 255f * lightIntensity, lightColor.B / 255f * lightIntensity);
            }
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            LoadTextureIfNeeded();
            
            Texture2D texture = _roseBudTexture;
            if (texture == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // FARGOS PATTERN: Remove alpha for proper additive blending
            Color petalBloom = BaseColor with { A = 0 };
            Color accentBloom = AccentColor with { A = 0 };
            
            // Outer soft glow (accent color)
            Texture2D bloomTex = ParticleTextureHelper.GetTexture("BloomCircle");
            spriteBatch.Draw(bloomTex, drawPos, null, accentBloom * opacity * 0.25f,
                0f, bloomTex.Size() / 2f, Scale * 3.5f, SpriteEffects.None, 0f);
            
            // Secondary glow (petal color)
            spriteBatch.Draw(bloomTex, drawPos, null, petalBloom * opacity * 0.35f,
                0f, bloomTex.Size() / 2f, Scale * 2.5f, SpriteEffects.None, 0f);
            
            // Multi-layer bloom stack for the rose texture
            // Layer 1: Large outer glow
            spriteBatch.Draw(texture, drawPos, null, accentBloom * opacity * 0.3f,
                Rotation, origin, Scale * 1.6f, SpriteEffects.None, 0f);
            
            // Layer 2: Medium glow (petal color)
            spriteBatch.Draw(texture, drawPos, null, petalBloom * opacity * 0.5f,
                Rotation, origin, Scale * 1.3f, SpriteEffects.None, 0f);
            
            // Layer 3: Core texture (bright pink tinted)
            Color coreColor = Color.Lerp(BaseColor, Color.White, 0.6f);
            spriteBatch.Draw(texture, drawPos, null, coreColor * opacity * 0.95f,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
            
            // White highlight sparkle at center
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * opacity * 0.45f,
                0f, bloomTex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
    
    #endregion
}
