using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles
{
    public class HeroicEmberParticle : ValorParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float _driftPhase;

        public HeroicEmberParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _driftPhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Velocity.Y -= 0.06f;
            float drift = (float)Math.Sin(Time * 0.12f + _driftPhase) * 0.4f;
            Vector2 perp = new Vector2(-Velocity.Y, Velocity.X).SafeNormalize(Vector2.UnitX);
            Position += perp * drift;
            Scale *= 0.985f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            Vector2 origin = tex.Size() * 0.5f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, 0f, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    public class ValorSparkParticle : ValorParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private Color _initialColor;
        private Vector2 _squash;

        public ValorSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = _initialColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _squash = new Vector2(0.4f, 1.8f);
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Scale *= 0.96f;
            _squash.X *= 0.85f;
            _squash.Y *= 1.1f;
            DrawColor = Color.Lerp(_initialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3));
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar").Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 scale = _squash * Scale;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                Color.Lerp(Color.White, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3)),
                Rotation, origin, scale * new Vector2(0.4f, 1f), SpriteEffects.None, 0f);
        }
    }

    public class HeroicBloomParticle : ValorParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public HeroicBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            Scale *= 1.015f;
            DrawColor = DrawColor * (1f - (float)Math.Pow(LifetimeCompletion, 1.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() * 0.5f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, 0f, origin, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    public class SakuraNoteParticle : ValorParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        private int _variant;
        private float _wobblePhase;

        private static readonly string[] NoteTextures = new string[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
            "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote"
        };

        public SakuraNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _variant = Main.rand.Next(NoteTextures.Length);
            _wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.Y -= 0.03f;
            float wobble = (float)Math.Sin(Time * 0.08f + _wobblePhase) * 0.6f;
            Position += new Vector2(wobble, 0f);
            Rotation += Velocity.X * 0.02f;
            DrawColor = DrawColor * (1f - (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(NoteTextures[_variant]).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Color c = DrawColor;
            c.A = (byte)(255 * (1f - LifetimeCompletion));
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, c, Rotation, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    public class HeroicSmokeParticle : ValorParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float _opacity;
        private float _spin;

        public HeroicSmokeParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, float opacity = 1f, float spin = 0f)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _opacity = opacity;
            _spin = spin;
        }

        public override void Update()
        {
            Velocity *= 0.88f;
            Scale *= 1.01f;
            _opacity *= 0.97f;
            Rotation += _spin * (Velocity.X > 0 ? 1f : -1f);
            float fade = Utils.GetLerpValue(1f, 0.85f, LifetimeCompletion, true);
            DrawColor *= fade;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke").Value;
            Vector2 origin = tex.Size() * 0.5f;
            Color col = DrawColor * _opacity;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }
}
