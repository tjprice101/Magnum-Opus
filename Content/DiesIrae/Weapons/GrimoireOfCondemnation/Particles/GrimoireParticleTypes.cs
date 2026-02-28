using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Particles
{
    public class CursedShardTrailParticle : GrimoireParticle
    {
        private static Asset<Texture2D> texture;
        public CursedShardTrailParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.2f, int life = 12)
        { Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Velocity *= 0.9f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            float stretch = Math.Max(1f, Velocity.Length() * 0.3f);
            sb.Draw(tex, Position - Main.screenPosition, null, GrimoireUtils.Additive(DrawColor, alpha * 0.7f),
                Velocity.ToRotation(), tex.Size() / 2f, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    public class ChainLightningSegment : GrimoireParticle
    {
        private static Asset<Texture2D> texture;
        private readonly Vector2 endPos;
        public ChainLightningSegment(Vector2 start, Vector2 end, Color color, int life = 8)
        {
            Position = start; endPos = end; MaxLifetime = life; DrawColor = color;
            IsAdditive = true; Scale = 1f;
        }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = 1f - LifeRatio;
            const int segments = 6;
            Vector2 prev = Position;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 ideal = Vector2.Lerp(Position, endPos, t);
                if (i < segments) ideal += Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dir = ideal - prev;
                float len = dir.Length();
                float rot = dir.ToRotation();
                sb.Draw(tex, prev - Main.screenPosition, null, GrimoireUtils.Additive(DrawColor, alpha * 0.8f),
                    rot, tex.Size() / 2f, new Vector2(len / tex.Width, 0.04f), SpriteEffects.None, 0f);
                prev = ideal;
            }
        }
    }

    public class GrimoireImpactBloom : GrimoireParticle
    {
        private static Asset<Texture2D> texture;
        public GrimoireImpactBloom(Vector2 pos, Color color, float scale = 1.2f, int life = 12)
        { Position = pos; Scale = scale; MaxLifetime = life; DrawColor = color; IsAdditive = true; }
        public override void Update() { base.Update(); Scale += 0.04f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - (float)Math.Pow(LifeRatio, 0.5)) * 0.5f;
            sb.Draw(tex, Position - Main.screenPosition, null, GrimoireUtils.Additive(DrawColor, alpha),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    public class GrimoireSmokeParticle : GrimoireParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        public GrimoireSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.4f, int life = 25)
        {
            Position = pos; Velocity = vel; Scale = scale; MaxLifetime = life;
            DrawColor = GrimoireUtils.VoidInk; IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }
        public override void Update() { base.Update(); Velocity *= 0.93f; Velocity.Y -= 0.03f; Rotation += rotSpeed; Scale += 0.008f; }
        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            if (!texture.IsLoaded) return; var tex = texture.Value;
            float alpha = (1f - LifeRatio) * 0.3f;
            sb.Draw(tex, Position - Main.screenPosition, null, DrawColor * alpha, Rotation,
                tex.Size() / 2f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    public class GrimoireNoteParticle : GrimoireParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote",
        };
        public GrimoireNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.4f, int life = 35)
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
            sb.Draw(tex, Position - Main.screenPosition, null, GrimoireUtils.Additive(DrawColor, alpha * 0.6f),
                Rotation, tex.Size() / 2f, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
