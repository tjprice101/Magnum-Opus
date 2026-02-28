using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Utilities;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Particles
{
    /// <summary>Additive rising dark flame ember — color shifts DarkFlameBlack→AbyssalCrimson→EmberGold as it ages.</summary>
    public class DarkFlameParticle : FinalityParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public DarkFlameParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.97f;
            Velocity.Y -= 0.015f; // slow upward rise
            Rotation += 0.01f;
            Scale *= 0.995f;

            // Shift DarkFlameBlack → AbyssalCrimson → EmberGold as it ages
            float t = LifetimeCompletion;
            DrawColor = FinalityUtils.MulticolorLerp(t * 0.7f, FinalityUtils.DarkFlameBlack, FinalityUtils.AbyssalCrimson, FinalityUtils.EmberGold);
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(t, 2.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive bright directional spark — stretches along velocity for fate-themed impacts.</summary>
    public class FateSpark : FinalityParticle
    {
        private Color _initialColor;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public FateSpark(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.87f;
            Scale *= 0.95f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            DrawColor = Color.Lerp(_initialColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 3));
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

    /// <summary>Alpha-blended floating ash mote from dark flames — gentle drift with sine wave sway.</summary>
    public class SummonAshParticle : FinalityParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public SummonAshParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.96f;
            Velocity.Y -= 0.01f; // very slow upward drift
            Velocity.X += (float)Math.Sin(Time * 0.06f) * 0.04f; // gentle sine sway
            Rotation += 0.008f;
            Scale *= 0.997f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke").Value;
            Vector2 origin = tex.Size() / 2f;
            Color ashColor = DrawColor * 0.5f; // semi-transparent dark
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, ashColor, Rotation, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive expanding bloom for summoning and impact moments — fades quadratically.</summary>
    public class DarkBloomParticle : FinalityParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public DarkBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.05f; // expand at 0.05f/tick
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            Color glowColor = DrawColor;
            glowColor.A = 0; // pure additive glow
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, glowColor, 0f, origin, Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha-blended music note from fate theme — crimson-violet coloring with gentle sway and upward drift.</summary>
    public class FinalityNoteParticle : FinalityParticle
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

        public FinalityNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            // Crimson-violet coloring for fate theme
            DrawColor = Color.Lerp(FinalityUtils.AbyssalCrimson, FinalityUtils.FateViolet, Main.rand.NextFloat());
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
