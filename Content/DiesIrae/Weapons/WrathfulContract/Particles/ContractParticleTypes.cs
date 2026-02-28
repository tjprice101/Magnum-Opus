using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Particles
{
    public class DemonAuraParticle : ContractParticle
    {
        private static Asset<Texture2D> texture;
        public DemonAuraParticle(Vector2 pos, Color color, float scale = 0.5f, int life = 12)
        { Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Scale += 0.02f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - (float)Math.Pow(LifeRatio, 0.5)) * 0.4f;
            sb.Draw(tex, Position - Main.screenPosition, null, ContractUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class DashTrailParticle : ContractParticle
    {
        private static Asset<Texture2D> texture;
        public DashTrailParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.25f, int life = 12)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.88f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, ContractUtils.Additive(DrawColor, alpha * 0.7f),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.4f), SpriteEffects.None, 0f);
        }
    }

    public class FireballGlowParticle : ContractParticle
    {
        private static Asset<Texture2D> texture;
        public FireballGlowParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.3f, int life = 15)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.93f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.5f;
            sb.Draw(tex, Position - Main.screenPosition, null, ContractUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class ContractSmokeParticle : ContractParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public ContractSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.4f, int life = 28)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life;
            DrawColor = ContractUtils.AbyssBlack; IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }
        public override void Update() { base.Update(); Velocity *= 0.93f; Velocity.Y -= 0.03f; Rotation += rotSpeed; Scale += 0.01f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.3f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    public class ContractNoteParticle : ContractParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/MusicNoteWithSlashes",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
        };
        public ContractNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.4f, int life = 38)
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
            sb.Draw(tex, Position - Main.screenPosition, null, ContractUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
