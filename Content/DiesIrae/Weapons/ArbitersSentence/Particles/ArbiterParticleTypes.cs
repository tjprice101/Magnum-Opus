using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Particles
{
    public class JudgmentFlameParticle : ArbiterParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public JudgmentFlameParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.6f, int life = 25)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color;
            IsAdditive = true; Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.04f, 0.04f);
        }
        public override void Update() { base.Update(); Velocity *= 0.96f; Rotation += rotSpeed; Scale += 0.01f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.6f;
            Color c = Color.Lerp(DrawColor, ArbiterUtils.AshBlack, LifeRatio * 0.5f);
            sb.Draw(tex, Position - Main.screenPosition, null, ArbiterUtils.Additive(c, alpha),
                Rotation, tex.Size() / 2f, Scale * 0.15f, SpriteEffects.None, 0f);
        }
    }

    public class FlameEmberParticle : ArbiterParticle
    {
        private static Asset<Texture2D> texture;
        public FlameEmberParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.12f, int life = 20)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.94f; Velocity.Y -= 0.06f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, ArbiterUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class JudgmentSmokeParticle : ArbiterParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public JudgmentSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.5f, int life = 30)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life;
            DrawColor = ArbiterUtils.AshBlack; IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }
        public override void Update() { base.Update(); Velocity *= 0.94f; Velocity.Y -= 0.04f; Rotation += rotSpeed; Scale += 0.015f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.35f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    public class NozzleFlashParticle : ArbiterParticle
    {
        private static Asset<Texture2D> texture;
        public NozzleFlashParticle(Vector2 pos, float rot, Color color, float scale = 1.5f, int life = 5)
        { Position = pos; Rotation = rot; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, ArbiterUtils.Additive(DrawColor, alpha * 0.5f),
                Rotation, tex.Size() / 2f, new Vector2(Scale, Scale * 0.4f), SpriteEffects.None, 0f);
        }
    }

    public class ArbiterNoteParticle : ArbiterParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };
        public ArbiterNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.4f, int life = 35)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color;
            IsAdditive = true; noteIndex = Main.rand.Next(NotePaths.Length);
            noteTextures ??= new Asset<Texture2D>[NotePaths.Length];
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }
        public override void Update() { base.Update(); Velocity *= 0.97f; Velocity.Y -= 0.05f; }
        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return; var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, ArbiterUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
