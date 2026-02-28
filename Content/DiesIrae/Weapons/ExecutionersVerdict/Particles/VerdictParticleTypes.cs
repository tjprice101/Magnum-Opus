using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Particles
{
    // ═══════════════════════════════════════════════════════════════
    //  BLOOD DRIP — drips from blade on swing, gravity-affected
    // ═══════════════════════════════════════════════════════════════
    public class BloodDripParticle : VerdictParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float initialScale;

        public BloodDripParticle(Vector2 pos, Vector2 vel, float scale = 0.4f, int life = 40)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            initialScale = scale;
            MaxLifetime = life;
            DrawColor = ExecutionersVerdictUtils.BloodRed;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.15f; // Gravity
            Velocity.X *= 0.97f;
            Scale = initialScale * (1f - LifeRatio * 0.6f);
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SmallHardCircleMask");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  JUDGMENT SMOKE — heavy charcoal smoke from heavy swings
    // ═══════════════════════════════════════════════════════════════
    public class JudgmentSmokeParticle : VerdictParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;

        public JudgmentSmokeParticle(Vector2 pos, Vector2 vel, float scale = 1f, int life = 45)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = ExecutionersVerdictUtils.CharcoalSmoke;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.05f; // Rise slowly
            Rotation += rotSpeed;
            Scale += 0.015f; // Expand
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.5f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.15f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  EXECUTION BLOOM — massive blood-red bloom on execution kill
    // ═══════════════════════════════════════════════════════════════
    public class ExecutionBloomParticle : VerdictParticle
    {
        private static Asset<Texture2D> texture;

        public ExecutionBloomParticle(Vector2 pos, float scale = 2f, int life = 25)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = ExecutionersVerdictUtils.BloodRed;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Scale += 0.08f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - (float)Math.Pow(LifeRatio, 0.5);
            Color color1 = ExecutionersVerdictUtils.Additive(DrawColor, alpha * 0.8f);
            Color color2 = ExecutionersVerdictUtils.Additive(ExecutionersVerdictUtils.AshWhite, alpha * 0.3f);
            sb.Draw(tex, Position - Main.screenPosition, null, color1, 0f,
                tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, color2, 0f,
                tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  EMBER SHARD — hot shards flung from blade on executioner strikes
    // ═══════════════════════════════════════════════════════════════
    public class EmberShardParticle : VerdictParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;

        public EmberShardParticle(Vector2 pos, Vector2 vel, float scale = 0.5f, int life = 30)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = ExecutionersVerdictUtils.MulticolorLerp(Main.rand.NextFloat(),
                ExecutionersVerdictUtils.EmberGlow, ExecutionersVerdictUtils.BurningCrimson, ExecutionersVerdictUtils.BloodRed);
            IsAdditive = true;
            Rotation = vel.ToRotation();
            rotSpeed = Main.rand.NextFloat(-0.1f, 0.1f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y += 0.08f;
            Rotation += rotSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            // Stretched along velocity for motion-blur feel
            Vector2 scale = new Vector2(Scale * (1f + Velocity.Length() * 0.1f), Scale * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, ExecutionersVerdictUtils.Additive(DrawColor, alpha),
                Rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  JUDGMENT MUSIC NOTE — dark blood-red notes on heavy strikes
    // ═══════════════════════════════════════════════════════════════
    public class JudgmentNoteParticle : VerdictParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;

        private static readonly string[] NotePaths = new[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
        };

        public JudgmentNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.7f, int life = 50)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            noteIndex = Main.rand.Next(NotePaths.Length);

            noteTextures ??= new Asset<Texture2D>[NotePaths.Length];
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.97f;
            Velocity.Y -= 0.04f; // Rise gently
            Rotation += Velocity.X * 0.01f;
            // Slight oscillation
            Position.X += (float)Math.Sin(Lifetime * 0.1f) * 0.3f;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return;

            var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, ExecutionersVerdictUtils.Additive(DrawColor, alpha * 0.8f),
                Rotation, tex.Size() / 2f, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }
}
