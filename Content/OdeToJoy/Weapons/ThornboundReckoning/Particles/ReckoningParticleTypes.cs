using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Particles
{
    // ═══════════════════════════════════════════════════════════
    // VineSparkParticle — velocity-stretched green spark, additive
    // ═══════════════════════════════════════════════════════════
    public class VineSparkParticle : ReckoningParticle
    {
        private static Asset<Texture2D> _texture;

        public VineSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.96f;
            Rotation = Velocity.ToRotation();
            Scale *= 0.97f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float stretchX = Scale * (1f + Velocity.Length() * 0.15f);
            float stretchY = Scale * 0.4f;

            Color col = ReckoningUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalSwirlParticle — drifting rose-gold petal with gentle gravity
    // ═══════════════════════════════════════════════════════════
    public class PetalSwirlParticle : ReckoningParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _rotSpeed;
        private readonly float _sineOffset;

        public PetalSwirlParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ReckoningUtils.RoseGold, ReckoningUtils.JubilantGold, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.06f, 0.06f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.03f; // gentle gravity
            Velocity.X += (float)Math.Sin(Lifetime * 0.08f + _sineOffset) * 0.12f; // gentle sway
            Velocity *= 0.985f;
            Rotation += _rotSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade; // quadratic fade for smooth disappearance
            Color col = DrawColor * fadeAlpha;

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GoldenBloomBurstParticle — large expanding bloom, additive, golden→white
    // ═══════════════════════════════════════════════════════════
    public class GoldenBloomBurstParticle : ReckoningParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public GoldenBloomBurstParticle(Vector2 position, Vector2 velocity, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = ReckoningUtils.JubilantGold;
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
            Velocity *= 0.92f;

            // Expand quickly then hold
            float progress = LifeRatio;
            if (progress < 0.3f)
                Scale = MathHelper.Lerp(0.05f, _maxScale, progress / 0.3f);
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
            Color col = ReckoningUtils.Additive(
                Color.Lerp(ReckoningUtils.JubilantGold, ReckoningUtils.WhiteBloom, LifeRatio * 0.6f),
                fade);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner bright core
            Color coreCol = ReckoningUtils.Additive(ReckoningUtils.WhiteBloom, fade * 0.6f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VerdantMistParticle — expanding green cloud, alpha blend, floats up
    // ═══════════════════════════════════════════════════════════
    public class VerdantMistParticle : ReckoningParticle
    {
        private static Asset<Texture2D> _texture;

        public VerdantMistParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ReckoningUtils.ForestGreen, ReckoningUtils.VerdantGold, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y -= 0.02f; // float upward
            Velocity *= 0.97f;
            Scale += 0.008f; // slowly expand
            Rotation += 0.005f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float alpha = fade * 0.35f; // soft, semi-transparent mist
            Color col = DrawColor * alpha;

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ReckoningNoteParticle — music note, additive, golden, drifts upward with sine wave
    // ═══════════════════════════════════════════════════════════
    public class ReckoningNoteParticle : ReckoningParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public ReckoningNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = ReckoningUtils.JubilantGold;
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
            Velocity.Y -= 0.04f; // drift upward
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.8f; // sine wave sway
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
            Color col = ReckoningUtils.Additive(
                Color.Lerp(ReckoningUtils.JubilantGold, ReckoningUtils.WhiteBloom, LifeRatio * 0.4f),
                fadeAlpha);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }
}
