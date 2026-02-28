using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Particles
{
    // ═══════════════════════════════════════════════════════════
    // OrbitalNoteGlowParticle — golden glow that orbits alongside
    // the note projectiles, additive, pulsing and fading
    // ═══════════════════════════════════════════════════════════
    public class OrbitalNoteGlowParticle : HymnParticle
    {
        private static Asset<Texture2D> _texture;

        public OrbitalNoteGlowParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(HymnUtils.BrilliantGold, HymnUtils.WarmAmber, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.92f;
            Scale *= 0.98f;
            Rotation += 0.03f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            float pulse = 1f + (float)Math.Sin(Lifetime * 0.3f) * 0.15f;
            Color col = HymnUtils.Additive(DrawColor, fadeAlpha * 0.7f);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.4f * pulse, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SymphonicWaveParticle — expanding ring on symphonic explosion,
    // additive, grows outward and fades
    // ═══════════════════════════════════════════════════════════
    public class SymphonicWaveParticle : HymnParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public SymphonicWaveParticle(Vector2 position, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = HymnUtils.BrilliantGold;
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
                Active = false;

            // Expand outward smoothly
            float progress = LifeRatio;
            Scale = MathHelper.Lerp(0.1f, _maxScale, (float)Math.Sqrt(progress));
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float ringAlpha = fade * fade * 0.6f;

            // Draw as a stretched ring shape — two perpendicular stretched draws
            Color col = HymnUtils.Additive(DrawColor, ringAlpha);
            Color innerCol = HymnUtils.Additive(HymnUtils.DivineLight, ringAlpha * 0.4f);

            // Outer ring glow
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale, Scale * 0.15f), SpriteEffects.None, 0f);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation + MathHelper.PiOver2, origin,
                new Vector2(Scale, Scale * 0.15f), SpriteEffects.None, 0f);

            // Inner bright core
            sb.Draw(tex, Position - Main.screenPosition, null, innerCol, Rotation, origin,
                Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HealingMoteParticle — green upward-drifting sparkle on heal,
    // additive, floats up and fades
    // ═══════════════════════════════════════════════════════════
    public class HealingMoteParticle : HymnParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;

        public HealingMoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(HymnUtils.HealGreen, HymnUtils.DivineLight, Main.rand.NextFloat(0.3f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity.Y -= 0.04f; // drift upward
            Velocity.X += (float)Math.Sin(Lifetime * 0.08f + _sineOffset) * 0.1f; // gentle sway
            Velocity *= 0.97f;
            Scale *= 0.99f;
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
            float twinkle = 0.7f + (float)Math.Sin(Lifetime * 0.4f) * 0.3f;
            Color col = HymnUtils.Additive(DrawColor, fadeAlpha * twinkle * 0.8f);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale * 0.25f, SpriteEffects.None, 0f);

            // Tiny white core
            Color coreCol = HymnUtils.Additive(Color.White, fadeAlpha * twinkle * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // MelodyTrailParticle — gold trail behind launched notes,
    // additive, velocity-stretched, fades quickly
    // ═══════════════════════════════════════════════════════════
    public class MelodyTrailParticle : HymnParticle
    {
        private static Asset<Texture2D> _texture;

        public MelodyTrailParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(HymnUtils.BrilliantGold, HymnUtils.WarmAmber, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = velocity.Length() > 0.1f ? velocity.ToRotation() : Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.91f;
            Rotation = Velocity.Length() > 0.1f ? Velocity.ToRotation() : Rotation;
            Scale *= 0.95f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = HymnUtils.Additive(DrawColor, fadeAlpha * 0.9f);

            // Stretch in velocity direction for trail effect
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale * 0.5f, Scale * 0.2f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // HymnBloomParticle — massive golden bloom on symphonic
    // explosion, additive, expands rapidly then fades
    // ═══════════════════════════════════════════════════════════
    public class HymnBloomParticle : HymnParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public HymnBloomParticle(Vector2 position, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = HymnUtils.BrilliantGold;
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
                Active = false;

            // Fast expand then ease out
            float progress = LifeRatio;
            if (progress < 0.2f)
                Scale = MathHelper.Lerp(0.05f, _maxScale, progress / 0.2f);
            else
                Scale = MathHelper.Lerp(_maxScale, _maxScale * 0.3f, (progress - 0.2f) / 0.8f);
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float bloomAlpha = fade * fade;

            // Bright golden core
            Color coreCol = HymnUtils.Additive(HymnUtils.DivineLight, bloomAlpha * 0.9f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.5f, SpriteEffects.None, 0f);

            // Golden bloom outer
            Color outerCol = HymnUtils.Additive(HymnUtils.BrilliantGold, bloomAlpha * 0.6f);
            sb.Draw(tex, Position - Main.screenPosition, null, outerCol, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Warm amber fringe
            Color fringeCol = HymnUtils.Additive(HymnUtils.WarmAmber, bloomAlpha * 0.35f);
            sb.Draw(tex, Position - Main.screenPosition, null, fringeCol, Rotation + 0.3f, origin,
                Scale * 1.2f, SpriteEffects.None, 0f);

            // Rose harmony accent
            Color roseCol = HymnUtils.Additive(HymnUtils.RoseHarmony, bloomAlpha * 0.2f);
            sb.Draw(tex, Position - Main.screenPosition, null, roseCol, Rotation - 0.2f, origin,
                Scale * 0.7f, SpriteEffects.None, 0f);
        }
    }
}
