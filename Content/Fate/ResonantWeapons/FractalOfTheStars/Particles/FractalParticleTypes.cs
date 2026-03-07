using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Particles
{
    // ========================================================================================
    // FRACTAL PARTICLE TYPES — 6 unique particle types for Fractal of the Stars
    // Star-themed: fractal stars, constellation sparks, nebula wisps, golden glyphs.
    // ========================================================================================

    /// <summary>
    /// Stellar mote particle: small drifting glow that twinkles like a distant star.
    /// Used for ambient hold effects and trail accents.
    /// </summary>
    public class FractalMote : FractalParticle
    {
        private static Asset<Texture2D> _tex;

        public FractalMote(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }

        public override bool Update()
        {
            Velocity *= 0.96f;
            // Twinkle: oscillate scale
            Scale *= 0.99f;
            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float t = LifetimeCompletion;
            // Twinkle effect
            float twinkle = MathF.Sin(TimeAlive * 0.3f) * 0.3f + 0.7f;
            float alpha = t < 0.2f ? t / 0.2f : t > 0.7f ? (1f - t) / 0.3f : 1f;
            alpha *= twinkle;
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, FractalUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, _tex.Value.Size() / 2f, MathHelper.Min(Scale, 0.586f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Stellar spark particle: fast-moving directional spark that stretches
    /// based on velocity. Gold-white star sparks for impacts and swing accents.
    /// </summary>
    public class FractalSpark : FractalParticle
    {
        private static Asset<Texture2D> _tex;

        public FractalSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            sb.Draw(_tex.Value, drawPos, null, FractalUtils.Additive(DrawColor, alpha),
                rot, _tex.Value.Size() / 2f, new Vector2(MathHelper.Min(Scale * stretch, 0.293f), MathHelper.Min(Scale * 0.5f, 0.293f)), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Star-shaped particle that rotates and can spawn smaller star children.
    /// The signature particle of Fractal of the Stars — rendered as a
    /// 5-pointed star using crossed FlareSpike textures.
    /// </summary>
    public class FractalStarParticle : FractalParticle
    {
        private static Asset<Texture2D> _tex;
        private readonly int _points;
        private bool _hasSpawned;

        public FractalStarParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime, int points = 5)
        {
            Position = pos; Velocity = vel; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
            _points = Math.Max(3, Math.Min(points, 8));
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            RotationSpeed = Main.rand.NextFloat(0.04f, 0.1f) * (Main.rand.NextBool() ? 1 : -1);
            _hasSpawned = false;
        }

        public override bool Update()
        {
            Velocity *= 0.95f;

            // At 50% lifetime, spawn 2-3 smaller star children (fractal splitting!)
            if (!_hasSpawned && LifetimeCompletion > 0.5f && Scale > 0.15f)
            {
                _hasSpawned = true;
                int children = Main.rand.Next(2, 4);
                for (int i = 0; i < children; i++)
                {
                    float childAngle = Rotation + MathHelper.TwoPi * i / children;
                    Vector2 childVel = childAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                    Color childCol = Color.Lerp(DrawColor, FractalUtils.ConstellationWhite, 0.3f);
                    FractalParticleHandler.SpawnParticle(new FractalStarParticle(
                        Position, childVel, childCol, Scale * 0.45f, Lifetime / 2, _points));
                }
            }

            return base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16");
            float t = LifetimeCompletion;
            float alpha = t < 0.15f ? t / 0.15f : t > 0.6f ? (1f - t) / 0.4f : 1f;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = _tex.Value.Size() / 2f;

            // Draw multiple rotated flare spikes to form a star shape
            for (int i = 0; i < _points; i++)
            {
                float pointAngle = Rotation + MathHelper.TwoPi * i / _points;
                Color pointCol = FractalUtils.Additive(DrawColor, alpha * 0.6f);
                sb.Draw(_tex.Value, drawPos, null, pointCol,
                    pointAngle, origin, MathHelper.Min(Scale, 0.293f), SpriteEffects.None, 0f);
            }

            // Hot white center
            sb.Draw(_tex.Value, drawPos, null, FractalUtils.Additive(FractalUtils.SupernovaFlash, alpha * 0.4f),
                Rotation, origin, MathHelper.Min(Scale * 0.4f, 0.293f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Cosmic glyph particle: slowly rotating arcane symbol that fades.
    /// Used for Star Fracture explosions and orbit blade spawns.
    /// </summary>
    public class FractalGlyph : FractalParticle
    {
        private static Asset<Texture2D>[] _glyphTex;
        private readonly int _glyphVariant;

        public FractalGlyph(Vector2 pos, Color color, float scale, int lifetime)
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
            float growScale = Scale * (0.6f + FractalUtils.SineOut(MathHelper.Clamp(t * 3f, 0f, 1f)) * 0.4f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(tex, drawPos, null, FractalUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, growScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bloom flare particle: large soft glow that rapidly expands then fades.
    /// Used for impacts and Star Fracture explosions. Gold/purple themed.
    /// </summary>
    public class FractalBloomFlare : FractalParticle
    {
        private static Asset<Texture2D> _tex;

        public FractalBloomFlare(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = Vector2.Zero; DrawColor = color; Scale = scale;
            Lifetime = lifetime; UseAdditiveBlend = true;
        }

        public override void Draw(SpriteBatch sb)
        {
            _tex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float t = LifetimeCompletion;
            float expand = FractalUtils.ExpOut(t);
            float alpha = 1f - t * t;
            float drawScale = MathHelper.Min(Scale * (0.5f + expand * 1.5f), 0.586f);
            Vector2 drawPos = Position - Main.screenPosition;
            sb.Draw(_tex.Value, drawPos, null, FractalUtils.Additive(DrawColor, alpha * 0.8f),
                0f, _tex.Value.Size() / 2f, drawScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Nebula wisp particle: slow-moving smoky tendril that drifts upward
    /// with a gentle purple-pink glow. Used for atmospheric stellar effects.
    /// </summary>
    public class FractalNebulaWisp : FractalParticle
    {
        private static Asset<Texture2D> _tex;

        public FractalNebulaWisp(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            sb.Draw(_tex.Value, drawPos, null, FractalUtils.Additive(DrawColor, alpha * 0.35f),
                Rotation, _tex.Value.Size() / 2f, MathHelper.Min(Scale, 0.293f), SpriteEffects.None, 0f);
        }
    }
}
