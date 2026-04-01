using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Particles
{
    /// <summary>
    /// Graceful feather particle that drifts and tumbles like a ballet dancer's plume.
    /// Fades between black and white polarity as it falls.
    /// Uses the existing FeatherWhite or FeatherBlack VFX textures.
    /// </summary>
    public class FeatherDriftParticle : BlackSwanParticle
    {
        private static Texture2D _texture;
        private float _rotationSpeed;
        private readonly bool _isBlack;
        private readonly float _driftAmplitude;
        private readonly float _driftFrequency;
        private float _opacity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // Trail5Loop has black bg

        public FeatherDriftParticle(Vector2 position, Vector2 velocity, bool isBlack, int lifetime = 60, float scale = 0.7f)
        {
            Position = position;
            Velocity = velocity;
            _isBlack = isBlack;
            Lifetime = lifetime;
            Scale = scale;
            _rotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
            _driftAmplitude = Main.rand.NextFloat(0.5f, 1.5f);
            _driftFrequency = Main.rand.NextFloat(0.05f, 0.12f);
            _opacity = 1f;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            DrawColor = isBlack ? new Color(15, 15, 25) : new Color(248, 245, 255);
        }

        public override void Update()
        {
            // Gentle drift oscillation (like a real feather falling)
            Velocity.X += (float)Math.Sin(Time * _driftFrequency) * _driftAmplitude * 0.02f;
            Velocity *= 0.97f;
            Velocity.Y += 0.02f; // Gentle gravity

            Rotation += _rotationSpeed;
            _rotationSpeed *= 0.99f; // Slowly stop spinning

            // Fade out in final 30% of life
            if (LifetimeCompletion > 0.7f)
            {
                float fadeProgress = (LifetimeCompletion - 0.7f) / 0.3f;
                _opacity = 1f - fadeProgress * fadeProgress;
            }

            Scale *= 0.998f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Use a simple feather-shaped draw (elongated ellipse via scale)
            if (_texture == null || _texture.IsDisposed)
                _texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Trails/Trail5Loop", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_texture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            Color drawCol = DrawColor * _opacity;

            // Draw feather: stretched in one direction for feather shape
            Vector2 featherScale = new Vector2(Scale * 0.3f, Scale);
            spriteBatch.Draw(_texture, screenPos, null, drawCol, Rotation, origin, featherScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bright, velocity-squished spark that represents the duality clash.
    /// Black sparks and white sparks collide at the swing boundary.
    /// </summary>
    public class DualitySparkParticle : BlackSwanParticle
    {
        private static Texture2D _bloomTexture;
        private readonly bool _isBlack;
        private float _opacity;
        private readonly float _squishStrength;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public DualitySparkParticle(Vector2 position, Vector2 velocity, bool isBlack, int lifetime = 30, float scale = 0.5f)
        {
            Position = position;
            Velocity = velocity;
            _isBlack = isBlack;
            Lifetime = lifetime;
            Scale = scale;
            _opacity = 1f;
            _squishStrength = 2.5f;
            DrawColor = isBlack ? new Color(60, 60, 80) : new Color(255, 255, 255);
        }

        public override void Update()
        {
            Velocity *= 0.93f;
            Scale *= 0.96f;
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bloomTexture == null || _bloomTexture.IsDisposed)
                _bloomTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_bloomTexture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_bloomTexture.Width / 2f, _bloomTexture.Height / 2f);

            // Velocity-based squish for motion blur effect
            float speed = Velocity.Length();
            float squish = MathHelper.Clamp(speed / 8f * _squishStrength, 1f, 4f);
            float rot = (float)Math.Atan2(Velocity.Y, Velocity.X);

            Vector2 squishScale = new Vector2(Scale * squish, Scale / squish);

            // Core spark (SoftGlow64 = 64px, no outer bloom)
            Color coreColor = Color.Lerp(DrawColor, Color.White, 0.5f) * _opacity * 0.8f;
            spriteBatch.Draw(_bloomTexture, screenPos, null, coreColor, rot, origin, squishScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Heavy smoke particle for empowerment activation and impact moments.
    /// Creates dramatic monochrome clouds.
    /// </summary>
    public class MonochromaticSmokeParticle : BlackSwanParticle
    {
        private static Texture2D _texture;
        private float _opacity;
        private readonly float _rotationSpeed;
        private readonly bool _isBlack;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftGlow has black bg

        public MonochromaticSmokeParticle(Vector2 position, Vector2 velocity, bool isBlack, int lifetime = 45, float scale = 1f, float opacity = 0.6f)
        {
            Position = position;
            Velocity = velocity;
            _isBlack = isBlack;
            Lifetime = lifetime;
            Scale = scale;
            _opacity = opacity;
            _rotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            DrawColor = isBlack ? new Color(15, 15, 25) : new Color(230, 230, 240);
        }

        public override void Update()
        {
            Velocity *= 0.88f;
            Rotation += _rotationSpeed;

            // Grow for first 20%, then shrink
            if (LifetimeCompletion < 0.2f)
                Scale *= 1.03f;
            else
                Scale *= 0.98f;

            _opacity *= 0.97f;

            // Final fade
            if (LifetimeCompletion > 0.8f)
            {
                float fadeProgress = (LifetimeCompletion - 0.8f) / 0.2f;
                _opacity *= 1f - fadeProgress;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null || _texture.IsDisposed)
                _texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_texture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            Color drawCol = DrawColor * _opacity;

            spriteBatch.Draw(_texture, screenPos, null, drawCol, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
