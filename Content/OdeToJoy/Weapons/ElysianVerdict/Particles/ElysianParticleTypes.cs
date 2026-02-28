using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Particles
{
    // ═══════════════════════════════════════════════════════════
    // ElysianGlowParticle — ambient golden-green glow orbiting the hovering orb
    // Additive, slowly orbits a center, pulses in brightness
    // ═══════════════════════════════════════════════════════════
    public class ElysianGlowParticle : ElysianParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _orbitSpeed;
        private readonly float _orbitRadius;
        private float _orbitAngle;
        private Vector2 _anchorPosition;

        public ElysianGlowParticle(Vector2 anchor, float orbitRadius, float scale, int lifetime)
        {
            _anchorPosition = anchor;
            _orbitRadius = orbitRadius;
            _orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            _orbitSpeed = Main.rand.NextFloat(0.03f, 0.06f);
            Position = anchor + new Vector2((float)Math.Cos(_orbitAngle), (float)Math.Sin(_orbitAngle)) * _orbitRadius;
            Velocity = Vector2.Zero;
            DrawColor = Color.Lerp(ElysianUtils.ElysianGold, ElysianUtils.VineGreen, Main.rand.NextFloat(0.4f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = 0f;
        }

        /// <summary>
        /// Allows the orb projectile to update the anchor each frame.
        /// </summary>
        public void UpdateAnchor(Vector2 newAnchor)
        {
            _anchorPosition = newAnchor;
        }

        public override void Update()
        {
            Lifetime++;
            if (Lifetime >= MaxLifetime)
            {
                Active = false;
                return;
            }

            _orbitAngle += _orbitSpeed;
            Position = _anchorPosition + new Vector2(
                (float)Math.Cos(_orbitAngle) * _orbitRadius,
                (float)Math.Sin(_orbitAngle) * _orbitRadius * 0.6f); // slightly elliptical
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float pulse = 0.7f + (float)Math.Sin(Lifetime * 0.12f) * 0.3f;
            Color col = ElysianUtils.Additive(DrawColor, fade * pulse);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VineTrailParticle — green trail behind vine missiles
    // Additive, velocity-stretched, fades quickly
    // ═══════════════════════════════════════════════════════════
    public class VineTrailParticle : ElysianParticle
    {
        private static Asset<Texture2D> _texture;

        public VineTrailParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ElysianUtils.VineGreen, ElysianUtils.VerdantDeep, Main.rand.NextFloat(0.5f));
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
            Rotation = Velocity != Vector2.Zero ? Velocity.ToRotation() : Rotation;
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

            Color col = ElysianUtils.Additive(DrawColor, fade * 0.8f);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VerdictBloomParticle — large expanding golden bloom on detonation
    // Additive, expands rapidly, fades from gold to white
    // ═══════════════════════════════════════════════════════════
    public class VerdictBloomParticle : ElysianParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public VerdictBloomParticle(Vector2 position, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = ElysianUtils.GoldenVerdict;
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

            // Rapid expansion in first 25%, then holds
            float progress = LifeRatio;
            if (progress < 0.25f)
                Scale = MathHelper.Lerp(0.05f, _maxScale, progress / 0.25f);
            else
                Scale = _maxScale * (1f - (progress - 0.25f) * 0.3f); // slight shrink over life
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color outer = ElysianUtils.Additive(
                Color.Lerp(ElysianUtils.GoldenVerdict, ElysianUtils.PureRadiance, LifeRatio * 0.5f),
                fade * 0.9f);
            sb.Draw(tex, Position - Main.screenPosition, null, outer, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner bright core
            Color core = ElysianUtils.Additive(ElysianUtils.PureRadiance, fade * 0.6f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, Rotation, origin,
                Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FloatingLeafParticle — leaf drifting near the orb, gentle gravity + sway
    // Alpha blend, green tinted, gentle float
    // ═══════════════════════════════════════════════════════════
    public class FloatingLeafParticle : ElysianParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _rotSpeed;
        private readonly float _sineOffset;

        public FloatingLeafParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ElysianUtils.VineGreen, ElysianUtils.ElysianGold, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.05f, 0.05f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y += 0.025f; // gentle gravity
            Velocity.X += (float)Math.Sin(Lifetime * 0.07f + _sineOffset) * 0.1f; // sway
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
            float fadeAlpha = fade * fade; // quadratic fade
            Color col = DrawColor * fadeAlpha;

            // Slightly elongated to suggest leaf shape
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale * 0.5f, Scale * 0.3f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // JudgmentNoteParticle — music note on detonation, golden, drifts upward with sine
    // Additive, uses MusicNote texture, jubilant burst
    // ═══════════════════════════════════════════════════════════
    public class JudgmentNoteParticle : ElysianParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public JudgmentNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ElysianUtils.GoldenVerdict, ElysianUtils.RoseJudgment, Main.rand.NextFloat(0.3f));
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
            Color col = ElysianUtils.Additive(
                Color.Lerp(DrawColor, ElysianUtils.PureRadiance, LifeRatio * 0.4f),
                fadeAlpha);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }
}
