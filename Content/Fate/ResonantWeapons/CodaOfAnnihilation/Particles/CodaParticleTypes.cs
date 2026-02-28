using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Particles
{
    // =========================================================================
    // 6 Particle Types for Coda of Annihilation
    // =========================================================================

    /// <summary>
    /// Cosmic mote — drifting nebula dots that shimmer with annihilation palette.
    /// Used for ambient trail scatter and sword wake effects.
    /// </summary>
    public class CosmicMoteParticle : CodaParticle
    {
        private static Texture2D _bloomTex;
        private float _opacity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public CosmicMoteParticle(Vector2 position, Vector2 velocity, Color color, float scale = 0.3f, int lifetime = 20)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = 1f;
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Scale *= 0.97f;
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_bloomTex == null || _bloomTex.IsDisposed)
                _bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (_bloomTex == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_bloomTex.Width / 2f, _bloomTex.Height / 2f);
            Color c = CodaUtils.Additive(DrawColor, _opacity);

            sb.Draw(_bloomTex, screenPos, null, c, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Arc spark — velocity-stretched fire spark that flies off sword edges.
    /// Squishes along movement direction for a slashing feel.
    /// </summary>
    public class ArcSparkParticle : CodaParticle
    {
        private static Texture2D _bloomTex;
        private float _opacity;
        private readonly float _squish;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public ArcSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale = 0.35f, int lifetime = 18)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = 1f;
            _squish = 2.5f;
        }

        public override void Update()
        {
            Velocity *= 0.91f;
            Scale *= 0.95f;
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_bloomTex == null || _bloomTex.IsDisposed)
                _bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (_bloomTex == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_bloomTex.Width / 2f, _bloomTex.Height / 2f);

            float speed = Velocity.Length();
            float squishFactor = MathHelper.Clamp(speed / 6f * _squish, 1f, 5f);
            float rot = (float)Math.Atan2(Velocity.Y, Velocity.X);
            Vector2 squishScale = new Vector2(Scale * squishFactor, Scale / squishFactor);

            Color outer = CodaUtils.Additive(DrawColor, _opacity * 0.5f);
            sb.Draw(_bloomTex, screenPos, null, outer, rot, origin, squishScale * 2f, SpriteEffects.None, 0f);

            Color core = CodaUtils.Additive(Color.White, _opacity * 0.7f);
            sb.Draw(_bloomTex, screenPos, null, core, rot, origin, squishScale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Zenith note — floating music note particle that drifts upward.
    /// The coda's musical signature — every blade sings when it strikes.
    /// </summary>
    public class ZenithNoteParticle : CodaParticle
    {
        private static Texture2D _noteTex;
        private float _opacity;
        private readonly float _wobble;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public ZenithNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale = 0.4f, int lifetime = 35)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = 1f;
            _wobble = Main.rand.NextFloat(0.02f, 0.06f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Velocity.Y -= 0.03f; // Drift upward
            Velocity.X += (float)Math.Sin(Time * _wobble) * 0.1f; // Wobble
            Scale *= 0.985f;
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.8f);
            Rotation += 0.02f;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_noteTex == null || _noteTex.IsDisposed)
            {
                try
                {
                    _noteTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    _noteTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
            }
            if (_noteTex == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_noteTex.Width / 2f, _noteTex.Height / 2f);
            Color c = CodaUtils.Additive(DrawColor, _opacity);

            sb.Draw(_noteTex, screenPos, null, c, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Glyph burst — arcane symbol that expands and fades on impact.
    /// Fate's rune inscribed into the fabric of reality upon each strike.
    /// </summary>
    public class GlyphBurstParticle : CodaParticle
    {
        private static Texture2D _glyphTex;
        private float _opacity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public GlyphBurstParticle(Vector2 position, Color color, float scale = 0.4f, int lifetime = 20)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = 1f;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Scale += 0.03f; // Expand
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.3f);
            Rotation += 0.015f;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_glyphTex == null || _glyphTex.IsDisposed)
            {
                try
                {
                    _glyphTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
                catch
                {
                    _glyphTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                        ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                }
            }
            if (_glyphTex == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_glyphTex.Width / 2f, _glyphTex.Height / 2f);
            Color c = CodaUtils.Additive(DrawColor, _opacity);

            sb.Draw(_glyphTex, screenPos, null, c, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Annihilation flare — large bright flash for spawn / impact / finisher moments.
    /// Multi-layer glow expanding outward. The signature annihilation flash.
    /// </summary>
    public class AnnihilationFlareParticle : CodaParticle
    {
        private static Texture2D _flareTex;
        private float _opacity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public AnnihilationFlareParticle(Vector2 position, Color color, float scale = 0.5f, int lifetime = 15)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = 1f;
        }

        public override void Update()
        {
            Scale *= 1.06f; // Expand rapidly
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.6f);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_flareTex == null || _flareTex.IsDisposed)
                _flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (_flareTex == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_flareTex.Width / 2f, _flareTex.Height / 2f);

            // Outer flare
            Color outer = CodaUtils.Additive(DrawColor, _opacity * 0.4f);
            sb.Draw(_flareTex, screenPos, null, outer, 0f, origin, Scale * 2f, SpriteEffects.None, 0f);

            // Core flare
            Color core = CodaUtils.Additive(Color.White, _opacity * 0.8f);
            sb.Draw(_flareTex, screenPos, null, core, 0f, origin, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Swing trail — lingering glow left behind a swing arc.
    /// Pairs with the CodaHeldSwing projectile to create the iconic sweep effect.
    /// </summary>
    public class SwingTrailParticle : CodaParticle
    {
        private static Texture2D _bloomTex;
        private float _opacity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        public SwingTrailParticle(Vector2 position, Vector2 velocity, Color color, float scale = 0.25f, int lifetime = 16)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = 1f;
        }

        public override void Update()
        {
            Velocity *= 0.88f;
            Scale *= 0.94f;
            _opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_bloomTex == null || _bloomTex.IsDisposed)
                _bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (_bloomTex == null) return;

            Vector2 screenPos = Position - Main.screenPosition;
            Vector2 origin = new Vector2(_bloomTex.Width / 2f, _bloomTex.Height / 2f);
            Color c = CodaUtils.Additive(DrawColor, _opacity);

            sb.Draw(_bloomTex, screenPos, null, c, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
