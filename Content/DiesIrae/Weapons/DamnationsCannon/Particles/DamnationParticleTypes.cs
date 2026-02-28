using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Particles
{
    public class CannonFlashParticle : DamnationParticle
    {
        private static Asset<Texture2D> texture;
        public CannonFlashParticle(Vector2 pos, float rot, Color color, float scale = 2f, int life = 10)
        { Position = pos; Rotation = rot; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, DamnationUtils.Additive(DrawColor, alpha * 0.9f),
                Rotation, tex.Size() / 2f, new Vector2(Scale * 2f, Scale * 0.8f), SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, DamnationUtils.Additive(DamnationUtils.DetonationWhite, alpha * 0.5f),
                Rotation, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    public class ExplosionBloomParticle : DamnationParticle
    {
        private static Asset<Texture2D> texture;
        public ExplosionBloomParticle(Vector2 pos, Color color, float scale = 2.5f, int life = 20)
        { Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Scale += 0.08f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - (float)Math.Pow(LifeRatio, 0.35);
            sb.Draw(tex, Position - Main.screenPosition, null, DamnationUtils.Additive(DrawColor, alpha * 0.7f),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, DamnationUtils.Additive(DamnationUtils.DetonationWhite, alpha * 0.3f),
                0f, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    public class ShrapnelSparkParticle : DamnationParticle
    {
        private static Asset<Texture2D> texture;
        public ShrapnelSparkParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.25f, int life = 18)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.94f; Velocity.Y += 0.08f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, DamnationUtils.Additive(DrawColor, alpha),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    public class CannonSmokeParticle : DamnationParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public CannonSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.6f, int life = 40)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = DamnationUtils.DarkSmoke;
            IsAdditive = false; Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.015f, 0.015f);
        }
        public override void Update() { base.Update(); Velocity *= 0.95f; Velocity.Y -= 0.03f; Rotation += rotSpeed; Scale += 0.012f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.4f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.12f, SpriteEffects.None, 0f);
        }
    }

    public class DamnationNoteParticle : DamnationParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };
        public DamnationNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.5f, int life = 45)
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
            sb.Draw(tex, Position - Main.screenPosition, null, DamnationUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
}
