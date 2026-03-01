using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Particles
{
    /// <summary>
    /// Ethereal feather — graceful spectral feather that dissolves into light.
    /// </summary>
    public class EtherealFeatherParticle : WingspanParticle
    {
        private float _swayPhase;
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 40 + Main.rand.Next(25);

        public override void Update()
        {
            base.Update();
            _swayPhase += 0.07f;
            Velocity.Y -= 0.02f;
            Velocity.X += (float)Math.Sin(_swayPhase) * 0.05f;
            Velocity *= 0.97f;
            Rotation += Velocity.X * 0.03f;
            Scale *= 1.005f;
        }

        public override void Draw(SpriteBatch sb)
        {
            float alpha = MathHelper.SmoothStep(0.7f, 0f, Progress);
            Texture2D tex = MagnumTextureRegistry.GetWideEllipse();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Color col = Color.Lerp(WingspanUtils.EtherealWhite,
                WingspanUtils.GetPrismaticEdge(Rotation), 0.3f);

            sb.Draw(tex, drawPos, null, col * alpha, Rotation,
                tex.Size() * 0.5f, new Vector2(Scale * 0.4f, Scale * 0.1f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                sb.Draw(glow, drawPos, null, col * alpha * 0.4f, Rotation,
                    glow.Size() * 0.5f, new Vector2(Scale * 0.6f, Scale * 0.15f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Wing spark — bright point that trails behind wing projectiles.
    /// </summary>
    public class WingSparkParticle : WingspanParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 12 + Main.rand.Next(8);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.92f;
            Scale *= 0.95f;
        }

        public override void Draw(SpriteBatch sb)
        {
            float alpha = 1f - Progress;
            Texture2D tex = MagnumTextureRegistry.GetPointBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;

            sb.Draw(tex, drawPos, null, DrawColor * alpha, 0f,
                tex.Size() * 0.5f, Scale * 0.1f, SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                sb.Draw(glow, drawPos, null, DrawColor * alpha * 0.35f, 0f,
                    glow.Size() * 0.5f, Scale * 0.18f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Charged wing burst — expanding wing-shaped flash for empowered shots.
    /// </summary>
    public class WingBurstParticle : WingspanParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 20;

        public override void Update()
        {
            base.Update();
            Scale *= 1.06f;
        }

        public override void Draw(SpriteBatch sb)
        {
            float alpha = (1f - Progress) * (1f - Progress);
            Texture2D tex = MagnumTextureRegistry.GetRadialBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Color col = Color.Lerp(Color.White, WingspanUtils.WingGold, Progress);

            // Wing shape: stretched horizontally
            sb.Draw(tex, drawPos, null, col * alpha * 0.6f, Rotation,
                tex.Size() * 0.5f, new Vector2(Scale * 1.2f, Scale * 0.3f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                sb.Draw(glow, drawPos, null, col * alpha * 0.3f, Rotation,
                    glow.Size() * 0.5f, new Vector2(Scale * 1.8f, Scale * 0.45f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Prismatic mote — tiny drifting glow with edge-rainbow coloring.
    /// </summary>
    public class PrismaticMoteParticle : WingspanParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 20 + Main.rand.Next(10);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
        }

        public override void Draw(SpriteBatch sb)
        {
            float alpha = (1f - Progress) * 0.7f;
            Texture2D tex = MagnumTextureRegistry.GetStar4Soft();
            if (tex == null) return;
            Color col = WingspanUtils.GetPrismaticEdge(Rotation + Progress * MathHelper.TwoPi);
            Vector2 drawPos = Position - Main.screenPosition;

            sb.Draw(tex, drawPos, null, col * alpha, 0f,
                tex.Size() * 0.5f, Scale * 0.06f, SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                sb.Draw(glow, drawPos, null, col * alpha * 0.4f, 0f,
                    glow.Size() * 0.5f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }
}
