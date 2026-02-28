using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Particles
{
    public class MuzzleFlashParticle : SinParticle
    {
        private static Asset<Texture2D> texture;
        public MuzzleFlashParticle(Vector2 pos, float rot, Color color, float scale = 1.5f, int life = 8)
        {
            Position = pos; Rotation = rot; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true;
        }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, SinUtils.Additive(DrawColor, alpha * 0.8f),
                Rotation, tex.Size() / 2f, new Vector2(Scale * 1.5f, Scale * 0.6f), SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, SinUtils.Additive(SinUtils.WhiteFlash, alpha * 0.4f),
                Rotation, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    public class SinBulletTrailParticle : SinParticle
    {
        private static Asset<Texture2D> texture;
        public SinBulletTrailParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.2f, int life = 12)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true;
        }
        public override void Update() { base.Update(); Velocity *= 0.93f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.5f);
            sb.Draw(tex, Position - Main.screenPosition, null, SinUtils.Additive(DrawColor, alpha),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    public class SinImpactBloomParticle : SinParticle
    {
        private static Asset<Texture2D> texture;
        public SinImpactBloomParticle(Vector2 pos, Color color, float scale = 1.5f, int life = 15)
        {
            Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true;
        }
        public override void Update() { base.Update(); Scale += 0.05f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - (float)Math.Pow(LifeRatio, 0.4);
            sb.Draw(tex, Position - Main.screenPosition, null, SinUtils.Additive(DrawColor, alpha * 0.6f),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class SinSmokeParticle : SinParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public SinSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.4f, int life = 30)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = SinUtils.DarkSmoke;
            IsAdditive = false; Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }
        public override void Update() { base.Update(); Velocity *= 0.95f; Rotation += rotSpeed; Scale += 0.008f; }
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

    public class SinNoteParticle : SinParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };
        public SinNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.5f, int life = 40)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color;
            IsAdditive = true; noteIndex = Main.rand.Next(NotePaths.Length);
            noteTextures ??= new Asset<Texture2D>[NotePaths.Length];
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }
        public override void Update() { base.Update(); Velocity *= 0.97f; Velocity.Y -= 0.03f; }
        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return;
            var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, SinUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, tex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
