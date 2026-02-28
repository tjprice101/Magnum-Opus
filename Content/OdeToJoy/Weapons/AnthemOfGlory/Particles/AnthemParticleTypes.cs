using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Particles
{
    // ═══════════════════════════════════════════════════════════
    // ShardSparkParticle — golden spark on shard travel/impact
    // Additive, velocity-stretched, fades quickly
    // ═══════════════════════════════════════════════════════════
    public class ShardSparkParticle : AnthemParticle
    {
        private static Asset<Texture2D> _texture;

        public ShardSparkParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(AnthemUtils.BrilliantAmber, AnthemUtils.RichGold, Main.rand.NextFloat(0.5f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.92f;
            Rotation = Velocity != Vector2.Zero ? Velocity.ToRotation() : Rotation;
            Scale *= 0.95f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float stretchX = Scale * (1f + Velocity.Length() * 0.15f);
            float stretchY = Scale * 0.3f;

            Color col = AnthemUtils.Additive(DrawColor, fade * 0.85f);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // LightningFlashParticle — white-gold flash at chain lightning endpoints
    // Additive, expands rapidly then fades, no velocity
    // ═══════════════════════════════════════════════════════════
    public class LightningFlashParticle : AnthemParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public LightningFlashParticle(Vector2 position, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = Color.Lerp(AnthemUtils.GloryWhite, AnthemUtils.LightningBlue, Main.rand.NextFloat(0.3f));
            Scale = 0.1f;
            _maxScale = maxScale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Lifetime++;
            if (Lifetime >= MaxLifetime)
            {
                Active = false;
                return;
            }

            float progress = LifeRatio;
            if (progress < 0.2f)
                Scale = MathHelper.Lerp(0.1f, _maxScale, progress / 0.2f);
            else
                Scale = _maxScale * (1f - (progress - 0.2f) * 1.1f);

            if (Scale < 0.01f)
                Scale = 0.01f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = AnthemUtils.Additive(DrawColor, fade * 0.9f);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner bright core
            Color core = AnthemUtils.Additive(AnthemUtils.GloryWhite, fade * 0.6f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, Rotation, origin,
                Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GloryBeamParticle — golden particle spraying along glory beam length
    // Additive, drifts sideways from beam body, short-lived
    // ═══════════════════════════════════════════════════════════
    public class GloryBeamParticle : AnthemParticle
    {
        private static Asset<Texture2D> _texture;

        public GloryBeamParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(AnthemUtils.BrilliantAmber, AnthemUtils.GloryWhite, Main.rand.NextFloat(0.6f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.9f;
            Scale *= 0.94f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float pulse = 0.8f + (float)Math.Sin(Lifetime * 0.2f) * 0.2f;
            Color col = AnthemUtils.Additive(DrawColor, fade * pulse * 0.75f);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // AnthemNoteParticle — music note scattered on glory beam impact
    // Alpha blend, drifts upward with gentle sway, gold tinted
    // ═══════════════════════════════════════════════════════════
    public class AnthemNoteParticle : AnthemParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _rotSpeed;

        public AnthemNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(AnthemUtils.BrilliantAmber, AnthemUtils.RoseTint, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.04f, 0.04f);
        }

        public override void Update()
        {
            Lifetime++;
            if (Lifetime >= MaxLifetime)
            {
                Active = false;
                return;
            }

            Position += Velocity;
            Velocity.Y -= 0.02f; // slight upward drift
            Velocity.X += (float)Math.Sin(Lifetime * 0.08f + _sineOffset) * 0.15f;
            Velocity *= 0.98f;
            Rotation += _rotSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeIn = MathHelper.Clamp(Lifetime / 5f, 0f, 1f);
            Color col = DrawColor * (fade * fadeIn * 0.9f);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // BeamBloomParticle — massive golden bloom at beam origin
    // Additive, expands rapidly, bright gold to white, short-lived
    // ═══════════════════════════════════════════════════════════
    public class BeamBloomParticle : AnthemParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public BeamBloomParticle(Vector2 position, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = AnthemUtils.BrilliantAmber;
            Scale = 0.05f;
            _maxScale = maxScale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Lifetime++;
            if (Lifetime >= MaxLifetime)
            {
                Active = false;
                return;
            }

            float progress = LifeRatio;
            if (progress < 0.15f)
                Scale = MathHelper.Lerp(0.05f, _maxScale, progress / 0.15f);
            else
                Scale = _maxScale * (1f - (progress - 0.15f) * 0.6f);

            if (Scale < 0.01f)
                Scale = 0.01f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            // Outer warm glow
            Color outer = AnthemUtils.Additive(
                Color.Lerp(AnthemUtils.BrilliantAmber, AnthemUtils.GloryWhite, LifeRatio * 0.4f),
                fade * 0.85f);
            sb.Draw(tex, Position - Main.screenPosition, null, outer, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner radiant core
            Color core = AnthemUtils.Additive(AnthemUtils.GloryWhite, fade * 0.65f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, Rotation, origin,
                Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
}
