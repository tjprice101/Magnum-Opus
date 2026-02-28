using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Particles
{
    // ═══════════════════════════════════════════════════════════
    // OvationGlowParticle — golden ambient glow orbiting minion, additive
    // ═══════════════════════════════════════════════════════════
    public class OvationGlowParticle : OvationParticle
    {
        private static Asset<Texture2D> _texture;

        public OvationGlowParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.94f;
            Scale *= 0.98f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = OvationUtils.Additive(DrawColor, fade * 0.7f);

            // Outer soft glow
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.4f, 0f, origin,
                Scale * 1.6f, SpriteEffects.None, 0f);
            // Main glow
            sb.Draw(tex, Position - Main.screenPosition, null, col, 0f, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // JoyWaveTrailParticle — golden crescent trail behind joy waves, additive
    // ═══════════════════════════════════════════════════════════
    public class JoyWaveTrailParticle : OvationParticle
    {
        private static Asset<Texture2D> _texture;

        public JoyWaveTrailParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            float stretchX = Scale * (1f + Velocity.Length() * 0.12f);
            float stretchY = Scale * 0.35f;

            Color col = OvationUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ApplauseSparkParticle — burst of gold sparks when minion attacks, additive
    // ═══════════════════════════════════════════════════════════
    public class ApplauseSparkParticle : OvationParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly bool _affectedByGravity;

        public ApplauseSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime, bool gravity = true)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = velocity.ToRotation();
            _affectedByGravity = gravity;
        }

        public override void Update()
        {
            base.Update();
            if (_affectedByGravity)
                Velocity.Y += 0.08f;
            Velocity *= 0.97f;
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
            float stretchX = Scale * (1f + Velocity.Length() * 0.18f);
            float stretchY = Scale * 0.4f;

            Color col = OvationUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

            // Bright inner core
            Color core = OvationUtils.Additive(OvationUtils.JoyfulWhite, fade * 0.5f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, Rotation, origin,
                new Vector2(stretchX * 0.5f, stretchY * 0.5f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // CelebrationMoteParticle — small floating happy particles near minions, alpha blend
    // ═══════════════════════════════════════════════════════════
    public class CelebrationMoteParticle : OvationParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;

        public CelebrationMoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y -= 0.02f; // gentle float upward
            Velocity.X += (float)Math.Sin(Lifetime * 0.07f + _sineOffset) * 0.1f; // gentle sway
            Velocity *= 0.985f;
            Rotation += 0.02f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade; // quadratic fade for smooth disappearance
            Color col = DrawColor * fadeAlpha * 0.7f;

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // OvationNoteParticle — music note on kill, additive, drifts upward with sine wave
    // ═══════════════════════════════════════════════════════════
    public class OvationNoteParticle : OvationParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public OvationNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = OvationUtils.SpotlightGold;
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
            Color col = OvationUtils.Additive(
                Color.Lerp(OvationUtils.SpotlightGold, OvationUtils.JoyfulWhite, LifeRatio * 0.4f),
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
