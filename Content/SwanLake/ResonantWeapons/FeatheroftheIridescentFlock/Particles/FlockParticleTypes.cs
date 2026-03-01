using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Particles
{
    /// <summary>
    /// Iridescent feather — graceful floating feather with oil-sheen shimmer.
    /// </summary>
    public class IridescentFeatherParticle : FlockParticle
    {
        private float _swayPhase;
        public override bool UseAdditiveBlend => false;
        protected override int SetLifetime() => 55 + Main.rand.Next(30);

        public override void Update()
        {
            base.Update();
            _swayPhase += 0.06f;
            Velocity.Y += 0.02f; // Gentle fall
            Velocity.X += (float)Math.Sin(_swayPhase) * 0.04f;
            Velocity *= 0.98f;
            Rotation += Velocity.X * 0.04f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = MathHelper.SmoothStep(1f, 0f, Progress);
            Texture2D tex = MagnumTextureRegistry.GetWideEllipse();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            Color sheen = FlockUtils.GetOilSheen(Rotation, Time);
            spriteBatch.Draw(tex, drawPos, null, sheen * alpha, Rotation,
                origin, new Vector2(Scale * 0.5f, Scale * 0.12f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                spriteBatch.Draw(glow, drawPos, null, sheen * alpha * 0.35f, Rotation,
                    glow.Size() * 0.5f, new Vector2(Scale * 0.75f, Scale * 0.18f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Crystal shard — sharp bright fragment from crystal attacks.
    /// </summary>
    public class CrystalShardParticle : FlockParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 18 + Main.rand.Next(12);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.93f;
            Velocity.Y += 0.06f;
            Rotation += 0.2f;
            Scale *= 0.97f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = (1f - Progress) * 0.9f;
            Texture2D tex = MagnumTextureRegistry.GetEllipse();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;

            spriteBatch.Draw(tex, drawPos, null, DrawColor * alpha, Rotation,
                tex.Size() * 0.5f, new Vector2(Scale * 0.04f, Scale * 0.15f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                spriteBatch.Draw(glow, drawPos, null, DrawColor * alpha * 0.4f, Rotation,
                    glow.Size() * 0.5f, new Vector2(Scale * 0.06f, Scale * 0.22f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Flock formation glow — soft ring that appears when formation is active.
    /// </summary>
    public class FormationGlowParticle : FlockParticle
    {
        private float _maxRadius;
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 30;

        public void Setup(float maxRadius) { _maxRadius = maxRadius; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float t = Progress;
            float radius = _maxRadius * (float)Math.Sqrt(t);
            float alpha = (1f - t) * (1f - t) * 0.5f;

            Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
            if (pixel == null) return;
            int segments = 48;
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Color col = FlockUtils.GetIridescent((float)i / segments);

                spriteBatch.Draw(pixel, Position - Main.screenPosition + offset,
                    new Rectangle(0, 0, 1, 1), col * alpha * Scale,
                    0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            }

            // Bloom overlay at ring center
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Color bloomCol = FlockUtils.GetIridescent(t);
                spriteBatch.Draw(glow, Position - Main.screenPosition, null, bloomCol * alpha * 0.3f * Scale,
                    0f, glow.Size() * 0.5f, radius / 40f, SpriteEffects.None, 0f);
            }
        }
    }

    /// <summary>
    /// Oil shimmer mote — small additive glow that drifts with iridescent color.
    /// </summary>
    public class OilShimmerParticle : FlockParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 25 + Main.rand.Next(15);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Scale *= 0.98f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = 1f - Progress;
            Texture2D tex = MagnumTextureRegistry.GetStar4Soft();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Color col = FlockUtils.GetIridescent(Progress + (float)Main.GameUpdateCount * 0.01f);

            spriteBatch.Draw(tex, drawPos, null, col * alpha * 0.5f, 0f,
                tex.Size() * 0.5f, Scale * 0.1f, SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                spriteBatch.Draw(glow, drawPos, null, col * alpha * 0.3f, 0f,
                    glow.Size() * 0.5f, Scale * 0.16f, SpriteEffects.None, 0f);
        }
    }
}
