using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Particles
{
    // ═══════════════════════════════════════════════════════════
    // FountainSprayParticle — upward water droplets from fountain, additive
    // Simulates a real fountain spray with gravity arc
    // ═══════════════════════════════════════════════════════════
    public class FountainSprayParticle : FountainParticle
    {
        private static Asset<Texture2D> _texture;

        public FountainSprayParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(FountainUtils.AquaGlow, FountainUtils.GoldenSpray, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.06f; // gravity pulls droplet back down
            Velocity *= 0.98f;
            Scale *= 0.97f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = FountainUtils.Additive(DrawColor, fade * 0.8f);

            // Outer soft water mist
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.3f, 0f, origin,
                Scale * 1.4f, SpriteEffects.None, 0f);
            // Core droplet
            sb.Draw(tex, Position - Main.screenPosition, null, col, 0f, origin,
                Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HealingAuraParticle — green glow ring expanding outward on heal, additive
    // ═══════════════════════════════════════════════════════════
    public class HealingAuraParticle : FountainParticle
    {
        private static Asset<Texture2D> _texture;

        public HealingAuraParticle(Vector2 position, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = FountainUtils.HealingGreen;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Scale += 0.8f; // expand outward
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade; // quadratic fade for smooth disappearance
            Color col = FountainUtils.Additive(DrawColor, fadeAlpha * 0.4f);

            // Expanding ring — draw outer ring and subtract inner to simulate a ring
            sb.Draw(tex, Position - Main.screenPosition, null, col, 0f, origin,
                Scale, SpriteEffects.None, 0f);

            // Brighter inner edge
            Color inner = FountainUtils.Additive(FountainUtils.FountainWhite, fadeAlpha * 0.2f);
            sb.Draw(tex, Position - Main.screenPosition, null, inner, 0f, origin,
                Scale * 0.85f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // WaterBoltTrailParticle — blue-gold trail behind water bolts, additive
    // ═══════════════════════════════════════════════════════════
    public class WaterBoltTrailParticle : FountainParticle
    {
        private static Asset<Texture2D> _texture;

        public WaterBoltTrailParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(FountainUtils.AquaGlow, FountainUtils.GoldenSpray, Main.rand.NextFloat());
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
            Rotation = Velocity.ToRotation();
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
            float stretchY = Scale * 0.35f;

            Color col = FountainUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

            // Bright core
            Color core = FountainUtils.Additive(FountainUtils.FountainWhite, fade * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, Rotation, origin,
                new Vector2(stretchX * 0.5f, stretchY * 0.5f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalSplashParticle — rose petals splashing outward from impact, alpha blend
    // ═══════════════════════════════════════════════════════════
    public class PetalSplashParticle : FountainParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _rotSpeed;

        public PetalSplashParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(FountainUtils.RoseSplash, FountainUtils.GoldenSpray, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.04f; // gentle gravity
            Velocity *= 0.96f;
            Rotation += _rotSpeed;
            Scale *= 0.98f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            // Squish to oval for petal shape
            float stretchX = Scale * 0.6f;
            float stretchY = Scale * 0.3f;

            Color col = DrawColor * fadeAlpha * 0.8f;
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FountainNoteParticle — music note rising from fountain, additive, drifts upward
    // ═══════════════════════════════════════════════════════════
    public class FountainNoteParticle : FountainParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public FountainNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = FountainUtils.GoldenSpray;
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
            Velocity.Y -= 0.04f; // drift upward like notes rising from water
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.8f;
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
            Color col = FountainUtils.Additive(
                Color.Lerp(FountainUtils.GoldenSpray, FountainUtils.FountainWhite, LifeRatio * 0.4f),
                fadeAlpha);

            // Bloom behind the note
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.35f, Rotation, origin,
                Scale * 1.5f, SpriteEffects.None, 0f);

            // Main note
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }
}
