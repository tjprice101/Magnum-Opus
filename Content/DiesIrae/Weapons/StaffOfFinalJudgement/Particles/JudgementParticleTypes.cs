using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Particles
{
    public class IgnitionOrbParticle : JudgementParticle
    {
        private static Asset<Texture2D> texture;
        public IgnitionOrbParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.4f, int life = 15)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.92f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, JudgementUtils.Additive(DrawColor, alpha * 0.5f),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class JudgmentDetonationParticle : JudgementParticle
    {
        private static Asset<Texture2D> texture;
        public JudgmentDetonationParticle(Vector2 pos, Color color, float scale = 2.5f, int life = 18)
        { Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Scale += 0.1f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - (float)Math.Pow(LifeRatio, 0.4)) * 0.6f;
            sb.Draw(tex, Position - Main.screenPosition, null, JudgementUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, JudgementUtils.Additive(JudgementUtils.DivineWhite, alpha * 0.3f),
                0f, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    public class JudgmentEmberParticle : JudgementParticle
    {
        private static Asset<Texture2D> texture;
        public JudgmentEmberParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.15f, int life = 22)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.95f; Velocity.Y -= 0.05f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.3f);
            sb.Draw(tex, Position - Main.screenPosition, null, JudgementUtils.Additive(DrawColor, alpha),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    public class JudgmentSmokeParticle : JudgementParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public JudgmentSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.5f, int life = 30)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life;
            DrawColor = JudgementUtils.InfernalBlack; IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }
        public override void Update() { base.Update(); Velocity *= 0.93f; Velocity.Y -= 0.03f; Rotation += rotSpeed; Scale += 0.01f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.35f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    public class JudgmentNoteParticle : JudgementParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };
        public JudgmentNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.45f, int life = 40)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color;
            IsAdditive = true; noteIndex = Main.rand.Next(NotePaths.Length);
            noteTextures ??= new Asset<Texture2D>[NotePaths.Length];
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }
        public override void Update() { base.Update(); Velocity *= 0.97f; Velocity.Y -= 0.04f; }
        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return; var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, JudgementUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
