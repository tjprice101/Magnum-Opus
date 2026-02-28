using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Particles
{
    // ═══════════════════════════════════════════════════════════
    // ChorusGlowParticle — golden ambient glow ring around minion, additive
    // Orbiting soft glow that gives the chorus entity a warm radiant presence
    // ═══════════════════════════════════════════════════════════
    public class ChorusGlowParticle : ChorusParticle
    {
        private static Asset<Texture2D> _texture;

        public ChorusGlowParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ChorusUtils.HarmonyGold, ChorusUtils.TriumphGold, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
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
            Color col = ChorusUtils.Additive(DrawColor, fadeAlpha * 0.5f);

            // Outer soft golden aura
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.3f, 0f, origin,
                Scale * 1.6f, SpriteEffects.None, 0f);
            // Inner warm core
            sb.Draw(tex, Position - Main.screenPosition, null, col, 0f, origin,
                Scale * 0.7f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HarmonicTrailParticle — gold trail behind harmonic blasts, additive
    // Stretched along velocity for a comet-like golden trail
    // ═══════════════════════════════════════════════════════════
    public class HarmonicTrailParticle : ChorusParticle
    {
        private static Asset<Texture2D> _texture;

        public HarmonicTrailParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ChorusUtils.TriumphGold, ChorusUtils.FinaleWhite, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.91f;
            Rotation = Velocity.ToRotation();
            Scale *= 0.94f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float stretchX = Scale * (1f + Velocity.Length() * 0.18f);
            float stretchY = Scale * 0.3f;

            Color col = ChorusUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

            // Hot white core
            Color core = ChorusUtils.Additive(ChorusUtils.FinaleWhite, fade * 0.45f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, Rotation, origin,
                new Vector2(stretchX * 0.4f, stretchY * 0.4f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // GrandFinaleBloomParticle — massive expanding golden bloom on finale, additive
    // Single large bloom burst that expands and fades — the visual climax
    // ═══════════════════════════════════════════════════════════
    public class GrandFinaleBloomParticle : ChorusParticle
    {
        private static Asset<Texture2D> _texture;

        public GrandFinaleBloomParticle(Vector2 position, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = ChorusUtils.TriumphGold;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
        }

        public override void Update()
        {
            base.Update();
            Scale += 1.2f; // rapid expansion
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade * fade; // cubic fade for dramatic falloff

            // Outer massive golden bloom
            Color outer = ChorusUtils.Additive(ChorusUtils.HarmonyGold, fadeAlpha * 0.35f);
            sb.Draw(tex, Position - Main.screenPosition, null, outer, 0f, origin,
                Scale * 1.4f, SpriteEffects.None, 0f);

            // Mid bloom — triumph gold
            Color mid = ChorusUtils.Additive(ChorusUtils.TriumphGold, fadeAlpha * 0.55f);
            sb.Draw(tex, Position - Main.screenPosition, null, mid, 0f, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner brilliant white core
            Color core = ChorusUtils.Additive(ChorusUtils.FinaleWhite, fadeAlpha * 0.7f);
            sb.Draw(tex, Position - Main.screenPosition, null, core, 0f, origin,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FinaleNoteParticle — music notes cascading outward on grand finale, additive
    // Radially expanding notes with sine wave drift — the celebration shower
    // ═══════════════════════════════════════════════════════════
    public class FinaleNoteParticle : ChorusParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;

        public FinaleNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ChorusUtils.TriumphGold, ChorusUtils.CrescendoRose, Main.rand.NextFloat(0.4f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(-0.4f, 0.4f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineFreq = Main.rand.NextFloat(0.05f, 0.1f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.97f;
            // Gentle sine drift perpendicular to velocity
            Vector2 perp = new Vector2(-Velocity.Y, Velocity.X);
            if (perp != Vector2.Zero)
                perp.Normalize();
            Position += perp * (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.6f;
            Rotation += 0.02f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = ChorusUtils.Additive(
                Color.Lerp(DrawColor, ChorusUtils.FinaleWhite, LifeRatio * 0.3f),
                fadeAlpha);

            // Bloom halo behind note
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.3f, Rotation, origin,
                Scale * 1.6f, SpriteEffects.None, 0f);

            // Main note sprite
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ChorusSparkParticle — sparkles on regular attacks, additive
    // Small bright sparks that flash and fade quickly — the attack accent
    // ═══════════════════════════════════════════════════════════
    public class ChorusSparkParticle : ChorusParticle
    {
        private static Asset<Texture2D> _texture;

        public ChorusSparkParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ChorusUtils.TriumphGold, ChorusUtils.FinaleWhite, Main.rand.NextFloat(0.5f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.93f;
            Scale *= 0.95f;
            Rotation += 0.1f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            // Bright flash at start, quick decay
            float flashMult = Lifetime < 3 ? 1.5f : 1f;
            Color col = ChorusUtils.Additive(DrawColor, fade * flashMult);

            // Cross-star shape via two stretched passes
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.7f, Rotation, origin,
                new Vector2(Scale * 1.2f, Scale * 0.3f), SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, col * 0.7f, Rotation + MathHelper.PiOver2, origin,
                new Vector2(Scale * 1.2f, Scale * 0.3f), SpriteEffects.None, 0f);

            // Core dot
            sb.Draw(tex, Position - Main.screenPosition, null, col, 0f, origin,
                Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}
