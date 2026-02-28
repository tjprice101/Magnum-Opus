using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles
{
    /// <summary>
    /// Blazing ember spark — velocity-squished fire mote that flies off the blade during swings.
    /// Leaves a hot orange-gold streak as it arcs away.
    /// </summary>
    public class InfernalEmberParticle : DualFatedChimeParticle
    {
        private static Texture2D _bloomTexture;
        private float _opacity;
        private readonly float _squishStrength;
        private readonly float _heat; // 0→1 maps to palette position

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public InfernalEmberParticle(Vector2 position, Vector2 velocity, float heat = 0.5f, int lifetime = 25, float scale = 0.4f)
        {
            Position = position;
            Velocity = velocity;
            _heat = heat;
            Lifetime = lifetime;
            Scale = scale;
            _opacity = 1f;
            _squishStrength = 2.5f;
            DrawColor = DualFatedChimeUtils.GetInfernalGradient(heat);
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            Velocity.Y += 0.04f; // Slight gravity — embers drift down
            Scale *= 0.96f;
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bloomTexture == null || _bloomTexture.IsDisposed)
                _bloomTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_bloomTexture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_bloomTexture.Width / 2f, _bloomTexture.Height / 2f);

            float speed = Velocity.Length();
            float squish = MathHelper.Clamp(speed / 8f * _squishStrength, 1f, 4f);
            float rot = (float)Math.Atan2(Velocity.Y, Velocity.X);
            Vector2 squishScale = new Vector2(Scale * squish, Scale / squish);

            // Outer fiery glow
            Color glowColor = DualFatedChimeUtils.Additive(DrawColor, _opacity * 0.5f);
            spriteBatch.Draw(_bloomTexture, screenPos, null, glowColor, rot, origin, squishScale * 2f, SpriteEffects.None, 0f);

            // White-hot core
            Color coreColor = DualFatedChimeUtils.Additive(Color.White, _opacity * 0.8f);
            spriteBatch.Draw(_bloomTexture, screenPos, null, coreColor, rot, origin, squishScale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Dense black smoke particle — billows from blade impacts and the Inferno Waltz.
    /// La Campanella's signature dark smoke effect.
    /// </summary>
    public class BellSmokeParticle : DualFatedChimeParticle
    {
        private static Texture2D _texture;
        private float _opacity;
        private readonly float _rotationSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;

        public BellSmokeParticle(Vector2 position, Vector2 velocity, int lifetime = 50, float scale = 1f, float opacity = 0.6f)
        {
            Position = position;
            Velocity = velocity;
            Lifetime = lifetime;
            Scale = scale;
            _opacity = opacity;
            _rotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            DrawColor = new Color(20, 15, 20); // Soot black
        }

        public override void Update()
        {
            Velocity *= 0.90f;
            Velocity.Y -= 0.05f; // Smoke rises
            Rotation += _rotationSpeed;

            if (LifetimeCompletion < 0.2f)
                Scale *= 1.04f;
            else
                Scale *= 0.98f;

            _opacity *= 0.97f;

            if (LifetimeCompletion > 0.75f)
            {
                float fadeProgress = (LifetimeCompletion - 0.75f) / 0.25f;
                _opacity *= 1f - fadeProgress;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_texture == null || _texture.IsDisposed)
                _texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_texture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            Color drawCol = DrawColor * _opacity;

            spriteBatch.Draw(_texture, screenPos, null, drawCol, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bell chime flash — a brief radial burst of golden light on impact.
    /// Represents the resonant toll of the bell.
    /// </summary>
    public class BellChimeFlashParticle : DualFatedChimeParticle
    {
        private static Texture2D _bloomTexture;
        private float _opacity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public BellChimeFlashParticle(Vector2 position, int lifetime = 15, float scale = 1.5f)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Lifetime = lifetime;
            Scale = scale;
            _opacity = 1f;
            DrawColor = new Color(255, 200, 80); // Bell gold
        }

        public override void Update()
        {
            // Fast expansion, then fade
            if (LifetimeCompletion < 0.3f)
                Scale *= 1.12f;

            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bloomTexture == null || _bloomTexture.IsDisposed)
                _bloomTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            if (_bloomTexture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_bloomTexture.Width / 2f, _bloomTexture.Height / 2f);

            // Large gold bell glow
            Color outerGlow = DualFatedChimeUtils.Additive(DrawColor, _opacity * 0.4f);
            spriteBatch.Draw(_bloomTexture, screenPos, null, outerGlow, 0f, origin, Scale * 2f, SpriteEffects.None, 0f);

            // White-hot inner flash
            Color innerFlash = DualFatedChimeUtils.Additive(new Color(255, 240, 200), _opacity * 0.7f);
            spriteBatch.Draw(_bloomTexture, screenPos, null, innerFlash, 0f, origin, Scale * 0.6f, SpriteEffects.None, 0f);

            // Orange ring
            Color ring = DualFatedChimeUtils.Additive(new Color(255, 100, 0), _opacity * 0.3f);
            spriteBatch.Draw(_bloomTexture, screenPos, null, ring, 0f, origin, Scale * 1.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Music note ember — a burning music note shape that floats upward.
    /// Represents the musical identity of La Campanella's bell theme.
    /// </summary>
    public class MusicalFlameParticle : DualFatedChimeParticle
    {
        private static Texture2D _noteTexture;
        private float _opacity;
        private readonly float _driftFrequency;
        private readonly float _driftAmplitude;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public MusicalFlameParticle(Vector2 position, Vector2 velocity, int lifetime = 40, float scale = 0.5f)
        {
            Position = position;
            Velocity = velocity;
            Lifetime = lifetime;
            Scale = scale;
            _opacity = 1f;
            _driftFrequency = Main.rand.NextFloat(0.06f, 0.12f);
            _driftAmplitude = Main.rand.NextFloat(0.3f, 1f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            DrawColor = DualFatedChimeUtils.GetInfernalGradient(Main.rand.NextFloat(0.3f, 0.8f));
        }

        public override void Update()
        {
            Velocity.X += (float)Math.Sin(Time * _driftFrequency) * _driftAmplitude * 0.02f;
            Velocity.Y -= 0.03f; // Rise up like heat
            Velocity *= 0.97f;
            Rotation += 0.02f;
            Scale *= 0.995f;

            if (LifetimeCompletion > 0.6f)
            {
                float fadeProgress = (LifetimeCompletion - 0.6f) / 0.4f;
                _opacity = 1f - fadeProgress * fadeProgress;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Try to load a music note texture, fallback to bloom
            if (_noteTexture == null || _noteTexture.IsDisposed)
            {
                try
                {
                    _noteTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    _noteTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
            }

            if (_noteTexture == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_noteTexture.Width / 2f, _noteTexture.Height / 2f);
            Color drawCol = DualFatedChimeUtils.Additive(DrawColor, _opacity);

            // Note sprite
            spriteBatch.Draw(_noteTexture, screenPos, null, drawCol, Rotation, origin, Scale, SpriteEffects.None, 0f);

            // Warm glow around note
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }

            if (bloomTex != null)
            {
                Vector2 bloomOrigin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);
                Color glowCol = DualFatedChimeUtils.Additive(DrawColor, _opacity * 0.3f);
                spriteBatch.Draw(bloomTex, screenPos, null, glowCol, 0f, bloomOrigin, Scale * 1.5f, SpriteEffects.None, 0f);
            }
        }
    }
}
