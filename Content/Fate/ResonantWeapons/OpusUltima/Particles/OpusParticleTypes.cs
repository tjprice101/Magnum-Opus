using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Particles
{
    // ========================================================================================
    // OPUS ULTIMA PARTICLE TYPES — 6 unique particle types for The Magnum Opus
    // Each type has its own behavior, rendering, and visual identity.
    // ========================================================================================

    /// <summary>
    /// Cosmic mote particle: small drifting glow that fades in then out.
    /// Ambient hold effects and trail accents. Crimson-gold palette.
    /// </summary>
    public class OpusMote : OpusParticle
    {
        private static Asset<Texture2D> _tex;

        public OpusMote(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            sb.Draw(_tex.Value, drawPos, null, OpusUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, _tex.Value.Size() / 2f, MathHelper.Min(Scale, 0.586f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Cosmic spark particle: fast-moving directional spark that stretches
    /// based on velocity. Hit impacts and swing accents.
    /// </summary>
    public class OpusSpark : OpusParticle
    {
        private static Asset<Texture2D> _tex;

        public OpusSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            sb.Draw(_tex.Value, drawPos, null, OpusUtils.Additive(DrawColor, alpha),
                rot, _tex.Value.Size() / 2f, new Vector2(MathHelper.Min(Scale * stretch, 0.293f), MathHelper.Min(Scale * 0.5f, 0.293f)), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Musical note particle: floats with gentle sinusoidal drift, rendered
    /// in alpha blend mode. The musical heartbeat of the Magnum Opus.
    /// </summary>
    public class OpusNoteParticle : OpusParticle
    {
        private static Asset<Texture2D>[] _noteTex;
        private readonly int _noteVariant;

        public OpusNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = false;
            _noteVariant = Main.rand.Next(4);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            RotationSpeed = Main.rand.NextFloat(-0.04f, 0.04f);
        }

        public override bool Update()
        {
            Velocity.X += MathF.Sin(TimeAlive * 0.12f) * 0.15f;
            Velocity *= 0.97f;
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_noteTex == null)
            {
                _noteTex = new Asset<Texture2D>[4];
                _noteTex[0] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote");
                _noteTex[1] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote");
                _noteTex[2] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNoteWithSlashes");
                _noteTex[3] = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/QuarterNote");
            }
            var tex = _noteTex[_noteVariant].Value;
            float t = LifetimeCompletion;
            float alpha = t < 0.15f ? t / 0.15f : t > 0.7f ? (1f - t) / 0.3f : 0.9f;
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(tex, drawPos, null, DrawColor * alpha, Rotation, tex.Size() / 2f,
                Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Cosmic glyph particle: slowly rotating arcane symbol that fades.
    /// Used for energy ball explosions and combo finishers.
    /// </summary>
    public class OpusGlyph : OpusParticle
    {
        private static Asset<Texture2D>[] _glyphTex;
        private readonly int _glyphVariant;

        public OpusGlyph(Vector2 pos, Color color, float scale, int lifetime)
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
            float growScale = Scale * (0.6f + OpusUtils.SineOut(MathHelper.Clamp(t * 3f, 0f, 1f)) * 0.4f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(tex, drawPos, null, OpusUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, growScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bloom flare particle: large soft glow that rapidly expands then fades.
    /// Used for impacts, energy ball explosions, and supernova detonations.
    /// </summary>
    public class OpusBloomFlare : OpusParticle
    {
        private static Asset<Texture2D> _tex;

        public OpusBloomFlare(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = Vector2.Zero; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float t = LifetimeCompletion;
            float expand = OpusUtils.ExpOut(t);
            float alpha = 1f - t * t;
            float drawScale = MathHelper.Min(Scale * (0.5f + expand * 1.5f), 0.586f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, OpusUtils.Additive(DrawColor, alpha * 0.8f),
                0f, _tex.Value.Size() / 2f, drawScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Nebula wisp particle: slow-moving smoky tendril that drifts upward.
    /// Ambient atmospheric effects around the weapon and energy balls.
    /// </summary>
    public class OpusNebulaWisp : OpusParticle
    {
        private static Asset<Texture2D> _tex;

        public OpusNebulaWisp(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.01f, 0.01f);
        }

        public override bool Update()
        {
            Velocity.Y -= 0.03f;
            Velocity *= 0.98f;
            Scale *= 1.005f;
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            float t = LifetimeCompletion;
            float alpha = t < 0.2f ? t / 0.2f : (1f - t);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, OpusUtils.Additive(DrawColor, alpha * 0.35f),
                Rotation, _tex.Value.Size() / 2f, MathHelper.Min(Scale, 0.293f), SpriteEffects.None, 0f);
        }
    }
}
