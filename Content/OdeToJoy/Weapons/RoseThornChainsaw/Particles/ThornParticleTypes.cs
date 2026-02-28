using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Particles
{
    // ── THORN SPARK ──
    // Velocity-stretched green spark that fades from bright green to deep thorn
    public class ThornSpark : ThornParticle
    {
        private readonly float initScale;

        public ThornSpark(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color;
            Scale = scale; initScale = scale; MaxLifetime = lifetime;
            IsAdditive = true;
            Rotation = vel != Vector2.Zero ? vel.ToRotation() : 0f;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
            Scale = initScale * (1f - LifeRatio);
            if (Velocity != Vector2.Zero) Rotation = Velocity.ToRotation();
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float stretch = Math.Max(Velocity.Length() * 0.6f, 2f);
            Color c = Color.Lerp(DrawColor, RoseThornChainsawUtils.DeepThorn, LifeRatio) * (1f - LifeRatio);
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), c,
                Rotation, new Vector2(0.5f), new Vector2(stretch * Scale, Scale * 0.35f), SpriteEffects.None, 0f);
        }
    }

    // ── ROSE PETAL ──
    // Drifting petal that gently falls with rotation
    public class RosePetal : ThornParticle
    {
        private readonly float initScale;
        private readonly float rotSpeed;
        private readonly float driftAmplitude;
        private readonly float driftFreq;

        public RosePetal(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = color;
            Scale = scale; initScale = scale; MaxLifetime = lifetime;
            IsAdditive = true;
            rotSpeed = Main.rand.NextFloat(0.03f, 0.08f) * (Main.rand.NextBool() ? 1 : -1);
            driftAmplitude = Main.rand.NextFloat(0.3f, 0.8f);
            driftFreq = Main.rand.NextFloat(0.06f, 0.12f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.02f; // gentle gravity
            Velocity.X += (float)Math.Sin(Lifetime * driftFreq) * driftAmplitude * 0.1f; // side drift
            Velocity *= 0.99f;
            Rotation += rotSpeed;
            Scale = initScale * (1f - LifeRatio * 0.5f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float opacity = (1f - LifeRatio);
            Color petalColor = Color.Lerp(DrawColor, RoseThornChainsawUtils.WhiteBloom, LifeRatio * 0.3f) * opacity;

            // Petal shape (stretched ellipse)
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), petalColor,
                Rotation, new Vector2(0.5f), new Vector2(Scale * 6f, Scale * 3f), SpriteEffects.None, 0f);
        }
    }

    // ── VENOM MIST ──
    // Expanding violet poison cloud
    public class VenomMist : ThornParticle
    {
        private readonly float initScale;

        public VenomMist(Vector2 pos, Vector2 vel, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = RoseThornChainsawUtils.VenomViolet;
            Scale = scale; initScale = scale; MaxLifetime = lifetime;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.015f; // rises slightly
            Scale = initScale * (1f + LifeRatio * 0.6f);
            Rotation += 0.015f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float opacity = (1f - LifeRatio) * 0.55f;
            Color c = Color.Lerp(DrawColor, RoseThornChainsawUtils.DeepThorn, LifeRatio * 0.4f) * opacity;
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), c,
                Rotation, new Vector2(0.5f), Scale * 10f, SpriteEffects.None, 0f);
        }
    }

    // ── GOLDEN POLLEN SPARK ──
    // Tiny gold sparkle that fades quickly
    public class PollenSparkle : ThornParticle
    {
        private readonly float initScale;

        public PollenSparkle(Vector2 pos, Vector2 vel, float scale, int lifetime)
        {
            Position = pos; Velocity = vel; DrawColor = RoseThornChainsawUtils.GoldenPollen;
            Scale = scale; initScale = scale; MaxLifetime = lifetime;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.94f;
            Velocity.Y -= 0.03f;
            Scale = initScale * (1f - LifeRatio * 0.8f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Lifetime * 0.3f) * 0.3f;
            Color c = Color.Lerp(DrawColor, RoseThornChainsawUtils.SunlightYellow, (float)Math.Sin(Lifetime * 0.2f) * 0.5f + 0.5f) * (1f - LifeRatio);
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), c,
                0f, new Vector2(0.5f), Scale * pulse * 4f, SpriteEffects.None, 0f);
        }
    }

    // ── THORN BLOOM ──
    // Large soft bloom overlay for impacts
    public class ThornBloom : ThornParticle
    {
        private readonly float initScale;

        public ThornBloom(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos; Velocity = Vector2.Zero; DrawColor = color;
            Scale = scale; initScale = scale; MaxLifetime = lifetime;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Scale = initScale * (1f + LifeRatio * 0.4f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Position - Main.screenPosition;
            float opacity = (1f - (float)Math.Pow(LifeRatio, 2)) * 0.5f;
            sb.Draw(tex, drawPos, new Rectangle(0, 0, 1, 1), DrawColor * opacity,
                0f, new Vector2(0.5f), Scale * 20f, SpriteEffects.None, 0f);
        }
    }
}
