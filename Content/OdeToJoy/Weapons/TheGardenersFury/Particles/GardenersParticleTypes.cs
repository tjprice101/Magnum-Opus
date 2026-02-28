using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Particles
{
    // ═══════════════════════════════════════════════════════════
    // RapierSparkParticle — gold velocity-stretched spark on thrust tip, additive
    // ═══════════════════════════════════════════════════════════
    public class RapierSparkParticle : GardenersParticle
    {
        private static Asset<Texture2D> _texture;

        public RapierSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.94f;
            Rotation = Velocity.ToRotation();
            Scale *= 0.96f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float stretchX = Scale * (1f + Velocity.Length() * 0.2f);
            float stretchY = Scale * 0.35f;

            Color col = GardenersUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ComboGlowParticle — ambient glow around player, intensity scales with combo stacks, additive
    // ═══════════════════════════════════════════════════════════
    public class ComboGlowParticle : GardenersParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly int _comboStacks;

        public ComboGlowParticle(Vector2 position, Vector2 velocity, int comboStacks, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            _comboStacks = Math.Clamp(comboStacks, 0, 10);
            DrawColor = GardenersUtils.PaletteLerp(_comboStacks / 10f);
            Scale = scale * (0.5f + _comboStacks * 0.08f);
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.9f;
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
            float intensity = fade * (0.3f + _comboStacks * 0.07f);
            Color col = GardenersUtils.Additive(DrawColor, intensity);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalBurstParticle — drifting rose petal with gentle gravity, alpha blend
    // ═══════════════════════════════════════════════════════════
    public class PetalBurstParticle : GardenersParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _rotSpeed;
        private readonly float _sineOffset;

        public PetalBurstParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(GardenersUtils.RoseBlush, GardenersUtils.GoldenPetal, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.07f, 0.07f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.035f; // gentle gravity
            Velocity.X += (float)Math.Sin(Lifetime * 0.09f + _sineOffset) * 0.14f; // gentle sway
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
            float fadeAlpha = fade * fade; // quadratic fade out
            Color col = DrawColor * fadeAlpha;

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FloralNoteParticle — music note, additive, golden, drifts upward with sine wave
    // ═══════════════════════════════════════════════════════════
    public class FloralNoteParticle : GardenersParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public FloralNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = GardenersUtils.JubilantGold;
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
            Velocity.Y -= 0.045f; // drift upward
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.85f;
            Velocity *= 0.975f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = GardenersUtils.Additive(
                Color.Lerp(GardenersUtils.JubilantGold, GardenersUtils.SunlightWhite, LifeRatio * 0.4f),
                fadeAlpha);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GardenerBloomParticle — large expanding celebration bloom, additive, gold→white
    // ═══════════════════════════════════════════════════════════
    public class GardenerBloomParticle : GardenersParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public GardenerBloomParticle(Vector2 position, Vector2 velocity, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = GardenersUtils.JubilantGold;
            Scale = 0.05f;
            _maxScale = maxScale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.9f;

            // Rapid expand, then hold, then fade
            float progress = LifeRatio;
            if (progress < 0.25f)
                Scale = MathHelper.Lerp(0.05f, _maxScale, progress / 0.25f);
            else
                Scale = _maxScale;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = GardenersUtils.Additive(
                Color.Lerp(GardenersUtils.JubilantGold, GardenersUtils.SunlightWhite, LifeRatio * 0.6f),
                fade);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Hot inner core
            Color coreCol = GardenersUtils.Additive(GardenersUtils.SunlightWhite, fade * 0.55f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }
}
