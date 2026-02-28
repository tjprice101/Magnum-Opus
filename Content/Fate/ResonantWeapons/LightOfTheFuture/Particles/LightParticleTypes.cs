using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Particles
{
    // ========================================================================================
    // LIGHT PARTICLE TYPES — 6 unique particle types for Light of the Future
    // ========================================================================================

    /// <summary>
    /// Cosmic mote particle: small drifting glow that fades in then out.
    /// Used for ambient hold effects and bullet trail accents.
    /// </summary>
    public class LightMote : LightParticle
    {
        private static Asset<Texture2D> _tex;

        public LightMote(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }

        public override bool Update()
        {
            Velocity *= 0.96f;
            Scale *= 0.99f;
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float t = LifetimeCompletion;
            float alpha = t < 0.2f ? t / 0.2f : t > 0.7f ? (1f - t) / 0.3f : 1f;
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, LightUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, _tex.Value.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Directional spark particle: fast-moving spark that stretches based on velocity.
    /// Used for muzzle flash accents and bullet impact sparks.
    /// </summary>
    public class LightSpark : LightParticle
    {
        private static Asset<Texture2D> _tex;

        public LightSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
        }

        public override bool Update()
        {
            Velocity *= 0.92f;
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");
            float t = LifetimeCompletion;
            float alpha = 1f - t * t;
            float stretch = Velocity.Length() * 0.15f + 1f;
            float rot = Velocity.ToRotation();
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, LightUtils.Additive(DrawColor, alpha),
                rot, _tex.Value.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Tracer particle: a very fast, short-lived line that simulates speed lines
    /// behind accelerating bullets. Renders as a stretched flare along velocity.
    /// </summary>
    public class LightTracer : LightParticle
    {
        private static Asset<Texture2D> _tex;
        private readonly float _initialSpeed;

        public LightTracer(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            _initialSpeed = vel.Length();
        }

        public override bool Update()
        {
            Velocity *= 0.88f;
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");
            float t = LifetimeCompletion;
            float alpha = (1f - t) * 0.8f;
            float speedFactor = _initialSpeed > 0 ? Velocity.Length() / _initialSpeed : 0f;
            float stretch = 2f + speedFactor * 4f;
            float rot = Velocity.ToRotation();
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, LightUtils.Additive(DrawColor, alpha),
                rot, _tex.Value.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.3f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Cosmic glyph particle: slowly rotating arcane symbol.
    /// Used for rocket spawns and special shot markers.
    /// </summary>
    public class LightGlyph : LightParticle
    {
        private static Asset<Texture2D>[] _glyphTex;
        private readonly int _glyphVariant;

        public LightGlyph(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = Vector2.Zero; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            _glyphVariant = Main.rand.Next(4);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_glyphTex == null)
            {
                _glyphTex = new Asset<Texture2D>[4];
                _glyphTex[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote");
                _glyphTex[1] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote");
                _glyphTex[2] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/WholeNote");
                _glyphTex[3] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/QuarterNote");
            }
            var tex = _glyphTex[_glyphVariant].Value;
            float t = LifetimeCompletion;
            float alpha = t < 0.1f ? t / 0.1f : (1f - t);
            float growScale = Scale * (0.6f + LightUtils.SineOut(MathHelper.Clamp(t * 3f, 0f, 1f)) * 0.4f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(tex, drawPos, null, LightUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, growScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bloom flare particle: large soft glow that rapidly expands then fades.
    /// Used for muzzle flash core and impact explosions.
    /// </summary>
    public class LightBloomFlare : LightParticle
    {
        private static Asset<Texture2D> _tex;

        public LightBloomFlare(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = Vector2.Zero; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float t = LifetimeCompletion;
            float expand = LightUtils.ExpOut(t);
            float alpha = 1f - t * t;
            float drawScale = Scale * (0.5f + expand * 1.5f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, LightUtils.Additive(DrawColor, alpha * 0.8f),
                0f, _tex.Value.Size() / 2f, drawScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Smoke particle: slow-moving wisp that drifts and expands.
    /// Used for rocket exhaust trails and muzzle smoke.
    /// </summary>
    public class LightSmoke : LightParticle
    {
        private static Asset<Texture2D> _tex;

        public LightSmoke(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.01f, 0.01f);
        }

        public override bool Update()
        {
            Velocity.Y -= 0.02f; // Gentle upward drift
            Velocity *= 0.98f;
            Scale *= 1.006f; // Slowly expand
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            float t = LifetimeCompletion;
            float alpha = t < 0.2f ? t / 0.2f : (1f - t);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, LightUtils.Additive(DrawColor, alpha * 0.35f),
                Rotation, _tex.Value.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
}
