using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles
{
    // ═══════════════════════════════════════════════════════════════
    //  TOLL RING — expanding concentric ring from bell toll, fading with bell decay
    // ═══════════════════════════════════════════════════════════════
    public class TollRingParticle : BellParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float maxRadius;
        private readonly float ringWidth;

        public TollRingParticle(Vector2 center, float maxRadius, Color color, float ringWidth = 4f, int life = 30)
        {
            Position = center;
            Velocity = Vector2.Zero;
            this.maxRadius = maxRadius;
            this.ringWidth = ringWidth;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
            Scale = 0f;
        }

        public override void Update()
        {
            base.Update();
            Scale = LifeRatio * maxRadius; // Expand outward
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;

            float decay = BellUtils.BellDecay(LifeRatio, 2f, 4f);
            float alpha = decay * 0.8f;
            if (alpha < 0.01f) return;

            // Draw expanding ring as two circles (outer - inner)
            float outerScale = Scale / (tex.Width * 0.5f);
            float innerScale = Math.Max(0f, (Scale - ringWidth) / (tex.Width * 0.5f));

            // Outer glow ring
            sb.Draw(tex, Position - Main.screenPosition, null, BellUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, outerScale, SpriteEffects.None, 0f);

            // Draw negative inner (approximated by darker smaller circle)
            if (innerScale > 0f)
            {
                sb.Draw(tex, Position - Main.screenPosition, null, BellUtils.Additive(DrawColor, alpha * 0.3f),
                    0f, tex.Size() / 2f, innerScale, SpriteEffects.None, 0f);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  BELL BLOOM — soft radial bloom during charge and toll
    // ═══════════════════════════════════════════════════════════════
    public class BellBloomParticle : BellParticle
    {
        private static Asset<Texture2D> texture;

        public BellBloomParticle(Vector2 pos, Color color, float scale = 1.5f, int life = 20)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Scale += 0.04f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - (float)Math.Pow(LifeRatio, 0.5);
            sb.Draw(tex, Position - Main.screenPosition, null, BellUtils.Additive(DrawColor, alpha * 0.6f),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, BellUtils.Additive(BellUtils.BellWhite, alpha * 0.2f),
                0f, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  BELL SMOKE — dark smoke rising from the bell
    // ═══════════════════════════════════════════════════════════════
    public class BellSmokeParticle : BellParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;

        public BellSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.7f, int life = 40)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = BellUtils.DarkSmoke;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.05f;
            Rotation += rotSpeed;
            Scale += 0.012f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.4f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.12f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  BELL EMBER — small rising ember sparks
    // ═══════════════════════════════════════════════════════════════
    public class BellEmberParticle : BellParticle
    {
        private static Asset<Texture2D> texture;

        public BellEmberParticle(Vector2 pos, Vector2 vel, float scale = 0.3f, int life = 25)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = BellUtils.MulticolorLerp(Main.rand.NextFloat(),
                BellUtils.EmberOrange, BellUtils.HellfireGold, BellUtils.EchoGold);
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.08f; // Rise
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, BellUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  BELL NOTE — music notes rising during toll
    // ═══════════════════════════════════════════════════════════════
    public class BellNoteParticle : BellParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = new[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };

        public BellNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.6f, int life = 45)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
            noteIndex = Main.rand.Next(NotePaths.Length);
            noteTextures ??= new Asset<Texture2D>[NotePaths.Length];
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.97f;
            Velocity.Y -= 0.04f;
            Position.X += (float)Math.Sin(Lifetime * 0.1f) * 0.3f;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return;
            var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, BellUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
}
