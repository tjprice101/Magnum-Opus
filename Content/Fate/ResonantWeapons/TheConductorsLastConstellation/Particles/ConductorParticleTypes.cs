using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Particles
{
    // ========================================================================================
    // CONDUCTOR PARTICLE TYPES — 6 unique particle types for The Conductor's Last Constellation
    // Conductor/lightning themed: electric motes, directional sparks, zigzag lightning,
    // arcane glyphs, bloom flares, and nebula wisps.
    // ========================================================================================

    /// <summary>
    /// Conductor mote: small drifting electric glow that pulses like a held note.
    /// Used for ambient hold effects and trail accents.
    /// </summary>
    public class ConductorMote : ConductorParticle
    {
        private static Asset<Texture2D> _tex;

        public ConductorMote(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            float pulse = MathF.Sin(TimeAlive * 0.35f) * 0.3f + 0.7f;
            float alpha = t < 0.2f ? t / 0.2f : t > 0.7f ? (1f - t) / 0.3f : 1f;
            alpha *= pulse;
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, ConductorUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, _tex.Value.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Conductor spark: fast-moving directional spark that stretches
    /// based on velocity. Cyan/gold sparks for impacts and swing accents.
    /// </summary>
    public class ConductorSpark : ConductorParticle
    {
        private static Asset<Texture2D> _tex;

        public ConductorSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
        }

        public override bool Update()
        {
            Velocity *= 0.91f;
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
            sb.Draw(_tex.Value, drawPos, null, ConductorUtils.Additive(DrawColor, alpha),
                rot, _tex.Value.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Lightning spark: ZIGZAG motion particle — the signature Conductor effect.
    /// Moves in jagged lateral steps (like a lightning bolt segment) while
    /// traveling along its primary velocity vector.
    /// </summary>
    public class LightningSpark : ConductorParticle
    {
        private static Asset<Texture2D> _tex;
        private readonly float _zigzagAmplitude;
        private readonly float _zigzagFrequency;
        private Vector2 _baseVelocity;

        public LightningSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime,
            float zigzagAmplitude = 3f, float zigzagFrequency = 0.4f)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            _baseVelocity = vel;
            _zigzagAmplitude = zigzagAmplitude;
            _zigzagFrequency = zigzagFrequency;
        }

        public override bool Update()
        {
            // Zigzag: offset perpendicular to travel direction
            float zigzag = MathF.Sin(TimeAlive * _zigzagFrequency * MathHelper.Pi) * _zigzagAmplitude;
            Vector2 perp = new Vector2(-_baseVelocity.Y, _baseVelocity.X);
            if (perp != Vector2.Zero) perp.Normalize();
            Velocity = _baseVelocity + perp * zigzag;
            _baseVelocity *= 0.95f;

            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");
            float t = LifetimeCompletion;
            float alpha = t < 0.1f ? t / 0.1f : 1f - MathF.Pow(t, 1.5f);
            float stretch = Velocity.Length() * 0.2f + 1f;
            float rot = Velocity.ToRotation();
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, ConductorUtils.Additive(DrawColor, alpha),
                rot, _tex.Value.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.4f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Cosmic glyph particle: slowly rotating arcane symbol that fades.
    /// Used for Convergence explosions and conductor gestures.
    /// </summary>
    public class ConductorGlyph : ConductorParticle
    {
        private static Asset<Texture2D>[] _glyphTex;
        private readonly int _glyphVariant;

        public ConductorGlyph(Vector2 pos, Color color, float scale, int lifetime)
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
            float growScale = Scale * (0.6f + ConductorUtils.SineOut(MathHelper.Clamp(t * 3f, 0f, 1f)) * 0.4f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(tex, drawPos, null, ConductorUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, growScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bloom flare particle: large soft glow that rapidly expands then fades.
    /// Used for impacts and Convergence explosions. Cyan/gold themed.
    /// </summary>
    public class ConductorBloomFlare : ConductorParticle
    {
        private static Asset<Texture2D> _tex;

        public ConductorBloomFlare(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = Vector2.Zero; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float t = LifetimeCompletion;
            float expand = ConductorUtils.ExpOut(t);
            float alpha = 1f - t * t;
            float drawScale = Scale * (0.5f + expand * 1.5f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, ConductorUtils.Additive(DrawColor, alpha * 0.8f),
                0f, _tex.Value.Size() / 2f, drawScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Nebula wisp particle: slow-moving smoky tendril that drifts upward
    /// with a gentle purple-cyan glow. Used for atmospheric conductor effects.
    /// </summary>
    public class ConductorNebulaWisp : ConductorParticle
    {
        private static Asset<Texture2D> _tex;

        public ConductorNebulaWisp(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.01f, 0.01f);
        }

        public override bool Update()
        {
            Velocity.Y -= 0.03f; // Gentle upward drift
            Velocity *= 0.98f;
            Scale *= 1.005f; // Slowly expand
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            float t = LifetimeCompletion;
            float alpha = t < 0.2f ? t / 0.2f : (1f - t);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, ConductorUtils.Additive(DrawColor, alpha * 0.35f),
                Rotation, _tex.Value.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
}
