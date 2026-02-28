using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Particles
{
    /// <summary>Floating petal particle — drifts down with rotation, alpha-blended.</summary>
    public class SakuraPetalParticle : SakuraParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public SakuraPetalParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity.X *= 0.97f;
            Velocity.Y += 0.03f;
            Velocity.Y *= 0.98f;
            Rotation += Velocity.X * 0.06f;
            Scale *= 0.995f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive ember spark — directional with velocity squeeze.</summary>
    public class BlossomSparkParticle : SakuraParticle
    {
        private Color _initialColor;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public BlossomSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = _initialColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Scale *= 0.96f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            DrawColor = Color.Lerp(_initialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar").Value;
            Vector2 origin = tex.Size() / 2f;
            float squish = MathHelper.Clamp(Velocity.Length() / 8f, 0.3f, 2.5f);
            Vector2 scale = new Vector2(Scale * 0.3f, Scale * 0.3f * squish);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive bloom burst — expanding soft glow.</summary>
    public class SakuraBloomParticle : SakuraParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public SakuraBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.04f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            DrawColor.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, 0f, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha-blended music note — floats upward with gentle sway.</summary>
    public class SakuraNoteParticle : SakuraParticle
    {
        private int _variant;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        private static readonly string[] NoteTextures = new[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote",
            "MagnumOpus/Assets/Particles Asset Library/MusicNoteWithSlashes",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
        };

        public SakuraNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _variant = Main.rand.Next(NoteTextures.Length);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }

        public override void Update()
        {
            Velocity *= 0.98f;
            Velocity.X += (float)Math.Sin(Time * 0.08f) * 0.12f;
            Rotation += Velocity.X * 0.02f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(NoteTextures[_variant]).Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive pollen mote — gentle rising glow.</summary>
    public class PollenMoteParticle : SakuraParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public PollenMoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.X += (float)Math.Sin(Time * 0.1f) * 0.05f;
            Velocity.Y -= 0.02f;
            Scale *= 0.99f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            DrawColor.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, 0f, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }
}
