using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Particles
{
    /// <summary>
    /// Calamity-style smear particles for melee swing effects.
    /// These create the signature "nebula flame" effect used by Ark of the Cosmos.
    /// 
    /// KEY PATTERN: Set smear.Time = 0 each frame to keep the particle alive during a swing.
    /// When you stop resetting Time, the particle will die naturally after its Lifetime.
    /// </summary>
    
    /// <summary>
    /// Full 360° circular smear effect. 
    /// Used for continuous spinning attacks or large area sweeps.
    /// </summary>
    public class CircularSmearVFX : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Textures/CircularSmear";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        /// <summary>
        /// Optional custom texture. If null, uses default texture.
        /// </summary>
        public Asset<Texture2D> LoadedAsset;
        
        public CircularSmearVFX(Vector2 position, Color color, float rotation, float scale, Asset<Texture2D> texture = null)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Rotation = rotation;
            Lifetime = 2; // Short lifetime - meant to be kept alive manually
            LoadedAsset = texture;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = LoadedAsset?.Value ?? GetDefaultTexture();
            if (tex == null) return;
            
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Draw with alpha removed for proper additive blending
            Color drawColor = Color with { A = 0 };
            spriteBatch.Draw(tex, drawPos, null, drawColor, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
        
        private Texture2D GetDefaultTexture()
        {
            // Try to load from mod content, fallback to procedural
            if (ModContent.HasAsset(Texture))
                return ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            return SmearTextureGenerator.CircularSmear;
        }
    }
    
    /// <summary>
    /// Smokey variant of the circular smear with softer edges.
    /// Creates a more nebulous, flame-like effect.
    /// </summary>
    public class CircularSmearSmokeyVFX : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Textures/CircularSmearSmokey";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        public Asset<Texture2D> LoadedAsset;
        
        public CircularSmearSmokeyVFX(Vector2 position, Color color, float rotation, float scale, Asset<Texture2D> texture = null)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Rotation = rotation;
            Lifetime = 2;
            LoadedAsset = texture;
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = LoadedAsset?.Value ?? GetDefaultTexture();
            if (tex == null) return;
            
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Multi-layer bloom for smokey effect
            Color baseColor = Color with { A = 0 };
            
            // Outer soft layer
            spriteBatch.Draw(tex, drawPos, null, baseColor * 0.3f, Rotation, origin, Scale * 1.2f, SpriteEffects.None, 0f);
            // Main layer
            spriteBatch.Draw(tex, drawPos, null, baseColor * 0.7f, Rotation, origin, Scale, SpriteEffects.None, 0f);
            // Inner bright layer
            spriteBatch.Draw(tex, drawPos, null, baseColor * 0.5f, Rotation, origin, Scale * 0.8f, SpriteEffects.None, 0f);
        }
        
        private Texture2D GetDefaultTexture()
        {
            if (ModContent.HasAsset(Texture))
                return ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            return SmearTextureGenerator.CircularSmearSmokey;
        }
    }
    
    /// <summary>
    /// 180° semi-circular smear with squish/distortion support.
    /// Perfect for sword swings and half-arc attacks.
    /// </summary>
    public class SemiCircularSmearVFX : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Textures/SemiCircularSmear";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        /// <summary>
        /// Squish factor for distorting the smear (X = horizontal, Y = vertical).
        /// Use Vector2.One for no distortion.
        /// </summary>
        public Vector2 Squish = Vector2.One;
        
        /// <summary>
        /// If true, the smear stays centered on the local player.
        /// </summary>
        public bool PlayerCentered = false;
        
        public SemiCircularSmearVFX(Vector2 position, Color color, float rotation, float scale, Vector2? squish = null)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Rotation = rotation;
            Lifetime = 2;
            Squish = squish ?? Vector2.One;
        }
        
        public override void Update()
        {
            if (PlayerCentered && Main.LocalPlayer != null)
            {
                Position = Main.LocalPlayer.Center;
            }
            base.Update();
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetDefaultTexture();
            if (tex == null) return;
            
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            
            Color drawColor = Color with { A = 0 };
            Vector2 scaledSize = Squish * Scale;
            
            spriteBatch.Draw(tex, drawPos, null, drawColor, Rotation, origin, scaledSize, SpriteEffects.None, 0f);
        }
        
        private Texture2D GetDefaultTexture()
        {
            if (ModContent.HasAsset(Texture))
                return ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            return SmearTextureGenerator.SemiCircularSmear;
        }
    }
    
    /// <summary>
    /// Fading variant of semi-circular smear.
    /// Produces light and rotates with velocity.
    /// </summary>
    public class SemiCircularSmearFade : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Textures/SemiCircularSmear";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        public Vector2 Squish = Vector2.One;
        
        public SemiCircularSmearFade(Vector2 position, Vector2 velocity, Color color, float rotation, float scale, int lifetime, Vector2? squish = null)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Rotation = rotation;
            Lifetime = lifetime;
            Squish = squish ?? Vector2.One;
        }
        
        public override void Update()
        {
            // Rotate based on velocity
            if (Velocity != Vector2.Zero)
            {
                Rotation = Velocity.ToRotation();
            }
            
            // Add light at position
            float lightIntensity = (1f - LifetimeCompletion) * 0.5f;
            Lighting.AddLight(Position, Color.ToVector3() * lightIntensity);
            
            base.Update();
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetDefaultTexture();
            if (tex == null) return;
            
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Fade out over lifetime
            float fadeAlpha = 1f - LifetimeCompletion;
            Color drawColor = Color with { A = 0 } * fadeAlpha;
            Vector2 scaledSize = Squish * Scale;
            
            spriteBatch.Draw(tex, drawPos, null, drawColor, Rotation, origin, scaledSize, SpriteEffects.None, 0f);
        }
        
        private Texture2D GetDefaultTexture()
        {
            if (ModContent.HasAsset(Texture))
                return ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            return SmearTextureGenerator.SemiCircularSmear;
        }
    }
    
    /// <summary>
    /// 120° third-arc smear - the SIGNATURE effect for Ark of the Cosmos style attacks.
    /// Perfect for charged melee swings with cosmic/nebula effects.
    /// </summary>
    public class TrientCircularSmear : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Textures/TrientCircularSmear";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        public TrientCircularSmear(Vector2 position, Color color, float rotation, float scale)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Rotation = rotation;
            Lifetime = 3; // Slightly longer lifetime for more persistence
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetDefaultTexture();
            if (tex == null) return;
            
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            
            // Multi-layer bloom for the cosmic effect
            Color baseColor = Color with { A = 0 };
            
            // Outer nebula glow
            spriteBatch.Draw(tex, drawPos, null, baseColor * 0.25f, Rotation, origin, Scale * 1.3f, SpriteEffects.None, 0f);
            // Mid layer
            spriteBatch.Draw(tex, drawPos, null, baseColor * 0.5f, Rotation, origin, Scale * 1.1f, SpriteEffects.None, 0f);
            // Core layer
            spriteBatch.Draw(tex, drawPos, null, baseColor * 0.8f, Rotation, origin, Scale, SpriteEffects.None, 0f);
            // Inner bright core
            spriteBatch.Draw(tex, drawPos, null, (Color.White with { A = 0 }) * 0.3f, Rotation, origin, Scale * 0.6f, SpriteEffects.None, 0f);
        }
        
        private Texture2D GetDefaultTexture()
        {
            if (ModContent.HasAsset(Texture))
                return ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            return SmearTextureGenerator.TrientCircularSmear;
        }
    }
    
    /// <summary>
    /// CritSpark - glittering sparkle particle with bloom overlay.
    /// Used for magical impacts, crit effects, and swing glitter.
    /// </summary>
    public class CritSpark : Particle
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ThinSparkle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;
        
        private Color _bloomColor;
        private float _bloomScale;
        private float _angularVelocity;
        
        /// <summary>
        /// Creates a CritSpark particle.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="velocity">Movement velocity</param>
        /// <param name="sparkColor">Main spark color (usually white)</param>
        /// <param name="bloomColor">Bloom/glow color</param>
        /// <param name="scale">Base scale</param>
        /// <param name="lifetime">Lifetime in frames</param>
        /// <param name="angularVelocity">Rotation speed per frame</param>
        /// <param name="bloomScale">Scale multiplier for bloom layer</param>
        public CritSpark(Vector2 position, Vector2 velocity, Color sparkColor, Color bloomColor, 
                        float scale, int lifetime, float angularVelocity = 0.05f, float bloomScale = 3f)
        {
            Position = position;
            Velocity = velocity;
            Color = sparkColor;
            _bloomColor = bloomColor;
            Scale = scale;
            Lifetime = lifetime;
            _angularVelocity = angularVelocity;
            _bloomScale = bloomScale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        
        public override void Update()
        {
            Rotation += _angularVelocity;
            Velocity *= 0.96f; // Slight drag
            base.Update();
        }
        
        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D sparkTex = GetSparkTexture();
            Texture2D bloomTex = ParticleTextureGenerator.BloomCircle;
            
            if (sparkTex == null) return;
            
            Vector2 drawPos = Position - Main.screenPosition;
            float fadeAlpha = 1f - LifetimeCompletion;
            
            // Draw bloom circle behind spark
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                Color bloomDraw = _bloomColor with { A = 0 } * fadeAlpha * 0.6f;
                spriteBatch.Draw(bloomTex, drawPos, null, bloomDraw, 0f, bloomOrigin, Scale * _bloomScale * 0.1f, SpriteEffects.None, 0f);
            }
            
            // Draw main spark
            Vector2 sparkOrigin = sparkTex.Size() * 0.5f;
            Color sparkDraw = Color with { A = 0 } * fadeAlpha;
            spriteBatch.Draw(sparkTex, drawPos, null, sparkDraw, Rotation, sparkOrigin, Scale, SpriteEffects.None, 0f);
        }
        
        private Texture2D GetSparkTexture()
        {
            if (ModContent.HasAsset(Texture))
                return ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            return ParticleTextureGenerator.Sparkle;
        }
    }
    
    /// <summary>
    /// Procedural generator for smear textures.
    /// Creates arc-shaped gradients for sword swing effects.
    /// </summary>
    public static class SmearTextureGenerator
    {
        private static Texture2D _circularSmear;
        private static Texture2D _circularSmearSmokey;
        private static Texture2D _semiCircularSmear;
        private static Texture2D _trientCircularSmear;
        
        public static Texture2D CircularSmear => _circularSmear ??= GenerateCircularSmear(256);
        public static Texture2D CircularSmearSmokey => _circularSmearSmokey ??= GenerateCircularSmearSmokey(256);
        public static Texture2D SemiCircularSmear => _semiCircularSmear ??= GenerateSemiCircularSmear(256);
        public static Texture2D TrientCircularSmear => _trientCircularSmear ??= GenerateTrientCircularSmear(256);
        
        /// <summary>
        /// Generates a full 360° circular smear/gradient texture.
        /// </summary>
        public static Texture2D GenerateCircularSmear(int size)
        {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) return null;
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float maxRadius = center * 0.95f;
            float innerRadius = center * 0.3f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    float alpha = 0f;
                    
                    if (dist >= innerRadius && dist <= maxRadius)
                    {
                        // Radial falloff - brightest in middle of the ring
                        float ringMid = (innerRadius + maxRadius) / 2f;
                        float ringWidth = (maxRadius - innerRadius) / 2f;
                        float distFromMid = Math.Abs(dist - ringMid);
                        
                        alpha = 1f - (distFromMid / ringWidth);
                        alpha = MathHelper.Clamp(alpha, 0f, 1f);
                        alpha = alpha * alpha; // Soften falloff
                    }
                    
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a softer, smokier circular smear with noise-like falloff.
        /// </summary>
        public static Texture2D GenerateCircularSmearSmokey(int size)
        {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) return null;
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float maxRadius = center * 0.95f;
            float innerRadius = center * 0.25f;
            
            // Use seeded random for consistent look
            Random rand = new Random(42);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    float angle = (float)Math.Atan2(dy, dx);
                    
                    float alpha = 0f;
                    
                    if (dist >= innerRadius && dist <= maxRadius)
                    {
                        // Base radial falloff
                        float ringMid = (innerRadius + maxRadius) / 2f;
                        float ringWidth = (maxRadius - innerRadius) / 2f;
                        float distFromMid = Math.Abs(dist - ringMid);
                        
                        alpha = 1f - (distFromMid / ringWidth);
                        
                        // Add angular variation for "smokey" effect
                        float angularNoise = (float)Math.Sin(angle * 8) * 0.15f + 
                                           (float)Math.Sin(angle * 13) * 0.1f +
                                           (float)Math.Sin(angle * 21) * 0.05f;
                        alpha += angularNoise;
                        
                        // Add radial noise
                        float radialNoise = (float)Math.Sin(dist * 0.3f) * 0.1f;
                        alpha += radialNoise;
                        
                        alpha = MathHelper.Clamp(alpha, 0f, 1f);
                        alpha = (float)Math.Pow(alpha, 1.5f); // Softer falloff
                    }
                    
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a 180° semi-circular smear for half-arc swings.
        /// </summary>
        public static Texture2D GenerateSemiCircularSmear(int size)
        {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) return null;
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float maxRadius = center * 0.95f;
            float innerRadius = center * 0.3f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    float angle = (float)Math.Atan2(dy, dx);
                    
                    float alpha = 0f;
                    
                    // Only draw in the upper half (or adjust for desired arc)
                    bool inArc = angle >= -MathHelper.Pi && angle <= 0; // Top half
                    
                    if (inArc && dist >= innerRadius && dist <= maxRadius)
                    {
                        // Radial falloff
                        float ringMid = (innerRadius + maxRadius) / 2f;
                        float ringWidth = (maxRadius - innerRadius) / 2f;
                        float distFromMid = Math.Abs(dist - ringMid);
                        
                        alpha = 1f - (distFromMid / ringWidth);
                        
                        // Angular falloff toward edges of the arc
                        float normalizedAngle = (angle + MathHelper.Pi) / MathHelper.Pi; // 0 to 1
                        float edgeFalloff = (float)Math.Sin(normalizedAngle * MathHelper.Pi);
                        alpha *= edgeFalloff;
                        
                        alpha = MathHelper.Clamp(alpha, 0f, 1f);
                        alpha = alpha * alpha;
                    }
                    
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Generates a 120° third-arc smear - the Ark of the Cosmos signature effect.
        /// </summary>
        public static Texture2D GenerateTrientCircularSmear(int size)
        {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) return null;
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            float center = size / 2f;
            float maxRadius = center * 0.95f;
            float innerRadius = center * 0.2f;
            
            // 120 degrees = 2π/3 radians
            float arcAngle = MathHelper.TwoPi / 3f;
            float startAngle = -arcAngle / 2f; // Center the arc at angle 0
            float endAngle = arcAngle / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center + 0.5f;
                    float dy = y - center + 0.5f;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    float angle = (float)Math.Atan2(dy, dx);
                    
                    float alpha = 0f;
                    
                    // Check if within the 120° arc
                    bool inArc = angle >= startAngle && angle <= endAngle;
                    
                    if (inArc && dist >= innerRadius && dist <= maxRadius)
                    {
                        // Radial falloff - weighted toward outer edge for "flame trail" look
                        float normalizedDist = (dist - innerRadius) / (maxRadius - innerRadius);
                        float radialAlpha = (float)Math.Sin(normalizedDist * MathHelper.Pi);
                        
                        // Weight toward outer edge
                        radialAlpha *= (0.3f + normalizedDist * 0.7f);
                        
                        // Angular falloff - smooth at edges
                        float normalizedAngle = (angle - startAngle) / arcAngle; // 0 to 1
                        float angularFalloff = (float)Math.Sin(normalizedAngle * MathHelper.Pi);
                        angularFalloff = (float)Math.Pow(angularFalloff, 0.7f); // Less aggressive falloff
                        
                        alpha = radialAlpha * angularFalloff;
                        
                        // Add subtle variation
                        float noise = (float)Math.Sin(angle * 12 + dist * 0.1f) * 0.1f;
                        alpha += noise;
                        
                        alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    }
                    
                    byte a = (byte)(alpha * 255);
                    data[y * size + x] = new Color(a, a, a, a);
                }
            }
            
            texture.SetData(data);
            return texture;
        }
        
        /// <summary>
        /// Cleanup textures on mod unload.
        /// </summary>
        public static void Unload()
        {
            _circularSmear = null;
            _circularSmearSmokey = null;
            _semiCircularSmear = null;
            _trientCircularSmear = null;
        }
    }
}
