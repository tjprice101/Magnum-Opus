using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles
{
    public class SigilGlowParticle : HarmonyParticle
    {
        private static Asset<Texture2D> texture;
        public SigilGlowParticle(Vector2 pos, Color color, float scale = 0.5f, int life = 12)
        { Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - (float)Math.Pow(LifeRatio, 0.5)) * 0.4f;
            sb.Draw(tex, Position - Main.screenPosition, null, HarmonyUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class RayTrailParticle : HarmonyParticle
    {
        private static Asset<Texture2D> texture;
        public RayTrailParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.12f, int life = 10)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.9f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.5f);
            sb.Draw(tex, Position - Main.screenPosition, null, HarmonyUtils.Additive(DrawColor, alpha * 0.7f),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.4f), SpriteEffects.None, 0f);
        }
    }

    public class HarmonyBloomParticle : HarmonyParticle
    {
        private static Asset<Texture2D> texture;
        public HarmonyBloomParticle(Vector2 pos, Color color, float scale = 1.2f, int life = 12)
        { Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Scale += 0.03f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio * LifeRatio) * 0.4f;
            sb.Draw(tex, Position - Main.screenPosition, null, HarmonyUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class HarmonyEmberParticle : HarmonyParticle
    {
        private static Asset<Texture2D> texture;
        public HarmonyEmberParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.1f, int life = 18)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.93f; Velocity.Y -= 0.04f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            sb.Draw(tex, Position - Main.screenPosition, null, HarmonyUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class HarmonyNoteParticle : HarmonyParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
        };
        public HarmonyNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.4f, int life = 35)
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
            sb.Draw(tex, Position - Main.screenPosition, null, HarmonyUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
