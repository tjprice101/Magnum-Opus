using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Particles
{
    // ═══════════════════════════════════════════════════════════════
    //  CORONA FLARE — spinning additive flare for the eclipse orb
    // ═══════════════════════════════════════════════════════════════
    public class CoronaFlareParticle : EclipseParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;
        private readonly float startScale;

        public CoronaFlareParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.5f, int life = 30)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            startScale = scale;
            MaxLifetime = life;
            DrawColor = color;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.15f, 0.15f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
            Rotation += rotSpeed;
            Scale = startScale * (1f - LifeRatio * 0.5f);
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, EclipseUtils.Additive(DrawColor, alpha),
                Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ECLIPSE SMOKE — dark smoke rising from the eclipse orb
    // ═══════════════════════════════════════════════════════════════
    public class EclipseSmokeParticle : EclipseParticle
    {
        private static Asset<Texture2D> texture;
        private readonly float rotSpeed;

        public EclipseSmokeParticle(Vector2 pos, Vector2 vel, float scale = 0.8f, int life = 40)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = EclipseUtils.EclipseSmoke;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.06f;
            Rotation += rotSpeed;
            Scale += 0.01f;
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
    //  SOLAR BLOOM — massive additive bloom for explosion / core
    // ═══════════════════════════════════════════════════════════════
    public class SolarBloomParticle : EclipseParticle
    {
        private static Asset<Texture2D> texture;

        public SolarBloomParticle(Vector2 pos, Color color, float scale = 2f, int life = 25)
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
            float alpha = 1f - (float)Math.Pow(LifeRatio, 0.5);
            sb.Draw(tex, Position - Main.screenPosition, null, EclipseUtils.Additive(DrawColor, alpha * 0.7f),
                0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            // Hot white core
            sb.Draw(tex, Position - Main.screenPosition, null, EclipseUtils.Additive(EclipseUtils.SolarWhite, alpha * 0.3f),
                0f, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  WRATH EMBER — small burning ember sparks for shard trails
    // ═══════════════════════════════════════════════════════════════
    public class WrathEmberParticle : EclipseParticle
    {
        private static Asset<Texture2D> texture;

        public WrathEmberParticle(Vector2 pos, Vector2 vel, float scale = 0.3f, int life = 22)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            MaxLifetime = life;
            DrawColor = EclipseUtils.MulticolorLerp(Main.rand.NextFloat(),
                EclipseUtils.OuterCorona, EclipseUtils.MidCorona, EclipseUtils.InnerCorona);
            IsAdditive = true;
            Rotation = vel.ToRotation();
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
            Velocity.Y += 0.05f;
        }

        public override void Draw(SpriteBatch sb)
        {
            texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!texture.IsLoaded) return;
            var tex = texture.Value;
            float alpha = 1f - LifeRatio * LifeRatio;
            Vector2 scale = new Vector2(Scale * (1f + Velocity.Length() * 0.08f), Scale * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, EclipseUtils.Additive(DrawColor, alpha),
                Rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ECLIPSE NOTE — music notes with solar corona coloring
    // ═══════════════════════════════════════════════════════════════
    public class EclipseNoteParticle : EclipseParticle
    {
        private static Asset<Texture2D>[] noteTextures;
        private readonly int noteIndex;
        private static readonly string[] NotePaths = new[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
        };

        public EclipseNoteParticle(Vector2 pos, Vector2 vel, Color color, float scale = 0.6f, int life = 45)
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
            Position.X += (float)Math.Sin(Lifetime * 0.12f) * 0.25f;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (noteTextures[noteIndex] == null)
                noteTextures[noteIndex] = ModContent.Request<Texture2D>(NotePaths[noteIndex]);
            if (!noteTextures[noteIndex].IsLoaded) return;
            var tex = noteTextures[noteIndex].Value;
            float alpha = (float)Math.Sin(LifeRatio * MathHelper.Pi);
            sb.Draw(tex, Position - Main.screenPosition, null, EclipseUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, tex.Size() / 2f, Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }
}
