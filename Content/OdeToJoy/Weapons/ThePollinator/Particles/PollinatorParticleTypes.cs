using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Particles
{
    // ═══════════════════════════════════════════════════════════
    // PollenDustParticle — small yellow-gold puff, additive, drifts and fades
    // ═══════════════════════════════════════════════════════════
    public class PollenDustParticle : PollinatorParticle
    {
        private static Asset<Texture2D> _texture;

        public PollenDustParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(PollinatorUtils.PollenGold, PollinatorUtils.SunGold, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.94f;
            Velocity.Y -= 0.015f; // gentle upward drift
            Scale *= 0.985f;
            Rotation += 0.02f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = PollinatorUtils.Additive(DrawColor, fadeAlpha * 0.8f);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalFlutterParticle — rose petal that flutters with sine wave, alpha blend
    // ═══════════════════════════════════════════════════════════
    public class PetalFlutterParticle : PollinatorParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _rotSpeed;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public PetalFlutterParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(PollinatorUtils.RoseBlush, PollinatorUtils.PollenGold, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.07f, 0.07f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineFreq = Main.rand.NextFloat(0.06f, 0.12f);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.025f; // gentle gravity — petals fall
            Velocity.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.15f; // flutter sway
            Velocity *= 0.98f;
            Rotation += _rotSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = DrawColor * fadeAlpha;

            // Stretch slightly for petal shape
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale * 0.45f, Scale * 0.25f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // MuzzleBloomParticle — golden flash at gun barrel, additive, expands + fades fast
    // ═══════════════════════════════════════════════════════════
    public class MuzzleBloomParticle : PollinatorParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public MuzzleBloomParticle(Vector2 position, Vector2 velocity, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = PollinatorUtils.SunGold;
            Scale = 0.1f;
            _maxScale = maxScale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.85f;

            // Expand quickly then shrink
            float progress = LifeRatio;
            if (progress < 0.25f)
                Scale = MathHelper.Lerp(0.1f, _maxScale, progress / 0.25f);
            else
                Scale = MathHelper.Lerp(_maxScale, 0f, (progress - 0.25f) / 0.75f);
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = PollinatorUtils.Additive(
                Color.Lerp(PollinatorUtils.SunGold, PollinatorUtils.PureLight, LifeRatio * 0.5f),
                fade);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner white core
            Color coreCol = PollinatorUtils.Additive(PollinatorUtils.PureLight, fade * 0.5f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SeedTrailParticle — small green-gold trail dot, additive, velocity-stretched
    // ═══════════════════════════════════════════════════════════
    public class SeedTrailParticle : PollinatorParticle
    {
        private static Asset<Texture2D> _texture;

        public SeedTrailParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.93f;
            Rotation = Velocity.Length() > 0.1f ? Velocity.ToRotation() : Rotation;
            Scale *= 0.96f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float stretchX = Scale * (1f + Velocity.Length() * 0.12f);
            float stretchY = Scale * 0.4f;

            Color col = PollinatorUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HarvestNoteParticle — music note on kill, additive, golden, drifts upward with sine wave
    // ═══════════════════════════════════════════════════════════
    public class HarvestNoteParticle : PollinatorParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public HarvestNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = PollinatorUtils.SunGold;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineFreq = Main.rand.NextFloat(0.06f, 0.12f);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y -= 0.05f; // drift upward
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.9f; // sine wave sway
            Velocity *= 0.98f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = PollinatorUtils.Additive(
                Color.Lerp(PollinatorUtils.SunGold, PollinatorUtils.PureLight, LifeRatio * 0.4f),
                fadeAlpha);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }
}
