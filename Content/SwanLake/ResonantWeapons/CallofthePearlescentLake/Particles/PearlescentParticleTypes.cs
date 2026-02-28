using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Particles
{
    /// <summary>
    /// Ripple ring — expands outward like a water surface disturbance.
    /// Draws as a thin ring with pearlescent shimmer, fading as it expands.
    /// </summary>
    public class RippleRingParticle : PearlescentParticle
    {
        private float _maxRadius;
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 35 + Main.rand.Next(10);

        public void Setup(float maxRadius)
        {
            _maxRadius = maxRadius;
        }

        public override void Update()
        {
            base.Update();
            Rotation += 0.01f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float t = Progress;
            float radius = MathHelper.Lerp(0f, _maxRadius, (float)Math.Sqrt(t));
            float alpha = 1f - t * t; // Quadratic fade
            float thickness = MathHelper.Lerp(3f, 1f, t);

            // Draw ring as a series of points approximating a circle
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            int segments = 48;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 drawPos = Position - Main.screenPosition + offset;

                // Pearlescent shimmer — hue shifts around the ring
                float hueShift = (float)i / segments + (float)Main.GameUpdateCount * 0.01f;
                Color shimmer = Color.Lerp(DrawColor, Utilities.PearlescentUtils.PearlWhite, 
                    (float)(Math.Sin(hueShift * MathHelper.TwoPi) + 1f) * 0.3f);

                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), shimmer * alpha * Scale,
                    0f, Vector2.Zero, thickness, SpriteEffects.None, 0f);
            }
        }
    }

    /// <summary>
    /// Water mist — soft floating cloud that drifts upward, like lake fog.
    /// </summary>
    public class LakeMistParticle : PearlescentParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 40 + Main.rand.Next(20);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.97f;
            Velocity.Y -= 0.02f; // Gentle rise
            Scale *= 1.008f; // Slowly expand
            Rotation += Velocity.X * 0.02f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = 1f - Progress;
            alpha *= alpha; // Quadratic fade

            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[174].Value; // Soft glow
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            spriteBatch.Draw(tex, drawPos, null, DrawColor * alpha * 0.4f, Rotation,
                origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Pearl droplet — tiny bright spark that falls with gravity like a water drop.
    /// </summary>
    public class PearlDropletParticle : PearlescentParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 20 + Main.rand.Next(15);

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.12f; // Gravity
            Velocity.X *= 0.98f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = (1f - Progress) * 0.9f;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[174].Value;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            // Bright white core
            spriteBatch.Draw(tex, drawPos, null, Color.White * alpha, 0f,
                origin, Scale * 0.15f, SpriteEffects.None, 0f);
            // Pearlescent halo
            spriteBatch.Draw(tex, drawPos, null, DrawColor * alpha * 0.5f, 0f,
                origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Prismatic feather shard — gracefully tumbling feather fragment with rainbow edge.
    /// </summary>
    public class PrismaticFeatherParticle : PearlescentParticle
    {
        private float _rotationSpeed;
        public override bool UseAdditiveBlend => false;
        protected override int SetLifetime() => 50 + Main.rand.Next(30);

        public void Setup()
        {
            _rotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y += 0.03f;
            Velocity.X += (float)Math.Sin(Time * 0.15f) * 0.05f; // Flutter
            Rotation += _rotationSpeed;
            _rotationSpeed *= 0.99f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = 1f - Progress * Progress;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[174].Value;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            // Stretched to look feather-like
            Vector2 scale = new Vector2(Scale * 0.6f, Scale * 0.15f);

            // Rainbow edge shimmer
            float hue = (Progress + (float)Main.GameUpdateCount * 0.005f) % 1f;
            Color rainbow = Main.hslToRgb(hue, 0.6f, 0.8f);
            Color final_color = Color.Lerp(DrawColor, rainbow, 0.3f);

            spriteBatch.Draw(tex, drawPos, null, final_color * alpha, Rotation,
                origin, scale, SpriteEffects.None, 0f);
        }
    }
}
