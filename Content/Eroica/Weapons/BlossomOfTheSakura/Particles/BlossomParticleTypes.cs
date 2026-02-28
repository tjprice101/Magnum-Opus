using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura.Particles
{
    /// <summary>Additive, directional velocity-squeezed spark for bullet tracers.</summary>
    public class TracerSparkParticle : BlossomParticle
    {
        private Color _initialColor;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TracerSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.92f;
            Scale *= 0.95f;
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

    /// <summary>Additive, expanding flash at barrel on fire.</summary>
    public class MuzzleFlashParticle : BlossomParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public MuzzleFlashParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.08f;
            Velocity *= 0.85f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            DrawColor.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, 0f, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha blend, rising wavy distortion particle for heat shimmer above barrel.</summary>
    public class HeatShimmerParticle : BlossomParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public HeatShimmerParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            // Slow rise with horizontal wave motion
            Velocity.Y -= 0.015f;
            Velocity.X = (float)Math.Sin(Time * 0.12f) * 0.3f;
            Velocity *= 0.98f;
            Rotation += 0.01f;
            Scale *= 0.997f;

            // Very low opacity — shimmer is subtle
            float fade = (float)Math.Pow(LifetimeCompletion, 1.5);
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, fade) * 0.25f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive, expanding bloom at bullet impact point.</summary>
    public class ImpactBloomParticle : BlossomParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public ImpactBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.06f;
            Velocity *= 0.9f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            DrawColor.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, 0f, origin, Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha blend petal that drifts from bullet path — gentle sakura accent.</summary>
    public class BulletPetalParticle : BlossomParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public BulletPetalParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity.X *= 0.96f;
            Velocity.Y += 0.04f; // gentle gravity — petals drift down
            Velocity.Y *= 0.98f;
            Rotation += Velocity.X * 0.07f;
            Scale *= 0.994f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2.5));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
