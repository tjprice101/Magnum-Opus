using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Particles
{
    /// <summary>Additive, bright electric spark that stutters position — lightning bolt accent.</summary>
    public class LightningSparkParticle : PiercingParticle
    {
        private Color _initialColor;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public LightningSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            // Stutter / jitter position for electric feel
            Position += new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f));
            Velocity *= 0.88f;
            Scale *= 0.93f;
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

    /// <summary>Additive, massive expanding golden flash for the 10th crescendo shot.</summary>
    public class CrescendoFlashParticle : PiercingParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public CrescendoFlashParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            // Rapid expand then linger and fade
            float expandRate = LifetimeCompletion < 0.3f ? 0.25f : 0.04f;
            Scale += expandRate;
            Velocity *= 0.8f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom").Value;
            Vector2 origin = tex.Size() / 2f;
            Color drawCol = DrawColor;
            drawCol.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, drawCol, 0f, origin, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha blend, lingering energy wisps along the bullet path — sniper trail accent.</summary>
    public class SniperTrailParticle : PiercingParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public SniperTrailParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            // Slow fade with gentle drift
            Velocity *= 0.96f;
            Velocity.Y -= 0.01f; // subtle upward drift
            Rotation += 0.008f;
            Scale *= 0.998f;

            float fade = (float)Math.Pow(LifetimeCompletion, 1.5);
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, fade) * 0.45f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Additive, radial star burst on enemy hit — golden-white impact flare.</summary>
    public class PiercingImpactParticle : PiercingParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public PiercingImpactParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            // Expand rapidly then fade
            Scale += 0.08f;
            Velocity *= 0.88f;
            Rotation += 0.02f;
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, (float)Math.Pow(LifetimeCompletion, 2));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard").Value;
            Vector2 origin = tex.Size() / 2f;
            Color drawCol = DrawColor;
            drawCol.A = 0;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, drawCol, Rotation, origin, Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Alpha blend, music note that fans out from barrel on the 10th crescendo shot.</summary>
    public class CrescendoNoteParticle : PiercingParticle
    {
        private float _rotationSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public CrescendoNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Update()
        {
            // Float upward with gentle rotation, slow fade
            Velocity.Y -= 0.025f;
            Velocity.X *= 0.97f;
            Velocity.Y *= 0.99f;
            Rotation += _rotationSpeed;
            Scale *= 0.996f;

            float fade = (float)Math.Pow(LifetimeCompletion, 2.5);
            DrawColor = Color.Lerp(DrawColor, Color.Transparent, fade);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote").Value;
            Vector2 origin = tex.Size() / 2f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, DrawColor, Rotation, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
