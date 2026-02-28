using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Weapons.TriumphantFractal.Utilities;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal.Particles
{
    /// <summary>Additive directional golden spark — stretches along velocity with drag 0.9, fast fade.</summary>
    public class FractalSparkParticle : FractalParticle
    {
        private Color _initialColor;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public FractalSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.9f;
            Scale *= 0.96f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            DrawColor = Color.Lerp(_initialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar").Value;
            Vector2 origin = tex.Size() / 2f;
            float squish = MathHelper.Clamp(Velocity.Length() / 8f, 0.3f, 2.5f);
            Vector2 scale = new Vector2(Scale * 0.25f, Scale * 0.25f * squish);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive expanding geometric flash — radiates outward on fractal triggers.</summary>
    public class GeometryFlashParticle : FractalParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public GeometryFlashParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Scale += 0.08f; // expand outward
            Rotation += 0.02f; // slow geometric rotation
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            Color glowColor = DrawColor;
            glowColor.A = 0; // pure additive glow
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, glowColor, Rotation, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha-blended jagged lightning arc segment — jitters position each frame for electric feel.</summary>
    public class LightningArcParticle : FractalParticle
    {
        private Vector2 _basePosition;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public LightningArcParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            _basePosition = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.95f;
            _basePosition += Velocity;

            // Jittery position offset for electric jaggedness
            Vector2 jitter = new Vector2(
                Main.rand.NextFloat(-3f, 3f),
                Main.rand.NextFloat(-3f, 3f)
            );
            Position = _basePosition + jitter;

            Rotation = Velocity.ToRotation();
            Scale *= 0.97f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive soft bloom glow on impacts — expands and fades for fractal resonance.</summary>
    public class FractalBloomParticle : FractalParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public FractalBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.05f; // gentle expand
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            Color glowColor = DrawColor;
            glowColor.A = 0; // pure additive glow
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, glowColor, 0f, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha-blended music note drifting after hit — golden-violet coloring with gentle sway.</summary>
    public class FractalNoteParticle : FractalParticle
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

        public FractalNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            // Gold-to-violet coloring for fractal theme
            DrawColor = Color.Lerp(FractalUtils.FractalGold, FractalUtils.FractalViolet, Main.rand.NextFloat());
            Scale = scale;
            Lifetime = lifetime;
            _variant = Main.rand.Next(NoteTextures.Length);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.Y -= 0.015f; // float upward
            Velocity.X += (float)Math.Sin(Time * 0.07f) * 0.1f; // gentle sway
            Rotation += Velocity.X * 0.025f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>(NoteTextures[_variant]).Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
