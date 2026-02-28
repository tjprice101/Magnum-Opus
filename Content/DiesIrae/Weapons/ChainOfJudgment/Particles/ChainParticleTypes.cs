using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Particles
{
    // ═══════════════════════════════════════════════════════════════
    //  CHAIN SPARK — small hot sparks flying on bounce impacts
    // ═══════════════════════════════════════════════════════════════
    public class ChainSparkParticle : ChainParticle
    {
        private static Asset<Texture2D> texture;

        public ChainSparkParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.25f, int life = 18)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
            Velocity.Y += 0.06f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.3f);
            sb.Draw(tex, Position - Main.screenPosition, null, ChainUtils.Additive(DrawColor, alpha),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.6f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CHAIN LINK GHOST — ghostly chain link afterimage
    // ═══════════════════════════════════════════════════════════════
    public class ChainLinkGhostParticle : ChainParticle
    {
        private static Asset<Texture2D> texture;

        public ChainLinkGhostParticle(Vector2 pos, Vector2 vel, float rot, Color color, float scale = 0.5f, int life = 20)
        {
            Position = pos;
            Velocity = vel;
            Rotation = rot;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.92f;
            Scale += 0.005f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SmallHardCircleMask");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.5f;
            sb.Draw(tex, Position - Main.screenPosition, null, ChainUtils.Additive(DrawColor, alpha),
                Rotation, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CHAIN BLOOM — bounce impact bloom
    // ═══════════════════════════════════════════════════════════════
    public class ChainBloomParticle : ChainParticle
    {
        private static Asset<Texture2D> texture;

        public ChainBloomParticle(Vector2 pos, Color color, float scale = 1.5f, int life = 15)
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
            Scale += 0.06f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - (float)Math.Pow(LifeRatio, 0.4);
            sb.Draw(tex, Position - Main.screenPosition, null, ChainUtils.Additive(DrawColor, alpha * 0.6f),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, ChainUtils.Additive(ChainUtils.AshWhite, alpha * 0.2f),
                0f, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CHAIN SMOKE — dark smoke from impacts
    // ═══════════════════════════════════════════════════════════════
    public class ChainSmokeParticle : ChainParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;

        public ChainSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.5f, int life = 35)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = ChainUtils.DarkSmoke;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.04f;
            Rotation += rotSpeed;
            Scale += 0.01f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.35f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  CHAIN NOTE — music notes on big bounce impacts
    // ═══════════════════════════════════════════════════════════════
    public class ChainNoteParticle : ChainParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = new[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };

        public ChainNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.5f, int life = 40)
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
            Velocity.Y -= 0.03f;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return;
            var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, ChainUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
}
