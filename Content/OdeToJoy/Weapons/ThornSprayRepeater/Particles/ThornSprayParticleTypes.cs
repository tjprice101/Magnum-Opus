using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Particles
{
    // ═══════════════════════════════════════════════════════════
    // ThornSparkParticle — quick sharp green spark on bolt impact / spawn
    // Additive, small, fast fade, velocity-stretched
    // ═══════════════════════════════════════════════════════════
    public class ThornSparkParticle : ThornSprayParticle
    {
        private static Asset<Texture2D> _texture;

        public ThornSparkParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ThornSprayUtils.ThornGreen, ThornSprayUtils.VerdantBolt, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.88f;
            Rotation = Velocity.Length() > 0.1f ? Velocity.ToRotation() : Rotation;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float stretch = 1f + Velocity.Length() * 0.15f;
            Color col = ThornSprayUtils.Additive(DrawColor, fade * fade);

            // Velocity-stretched spark
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale * stretch, Scale * 0.4f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // StickyGlowParticle — pulsing glow on embedded thorn, additive
    // Grows and shifts from green to gold over its life
    // ═══════════════════════════════════════════════════════════
    public class StickyGlowParticle : ThornSprayParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly Vector2 _anchor; // offset from NPC center
        private readonly int _npcIndex;

        public StickyGlowParticle(Vector2 position, int npcIndex, Vector2 anchorOffset, float scale, int lifetime)
        {
            Position = position;
            _npcIndex = npcIndex;
            _anchor = anchorOffset;
            Velocity = Vector2.Zero;
            DrawColor = ThornSprayUtils.ThornGreen;
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = 0f;
        }

        public override void Update()
        {
            Lifetime++;
            if (Lifetime >= MaxLifetime)
            {
                Active = false;
                return;
            }

            // Follow the NPC
            if (_npcIndex >= 0 && _npcIndex < Main.maxNPCs && Main.npc[_npcIndex].active)
                Position = Main.npc[_npcIndex].Center + _anchor;
            else
                Active = false;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float progress = LifeRatio;
            // Pulse: grows as timer approaches detonation
            float pulse = 1f + (float)Math.Sin(Lifetime * 0.2f) * 0.3f * progress;
            float growScale = Scale * (0.6f + progress * 0.8f) * pulse;

            // Color shifts green -> gold as timer counts down
            Color baseCol = Color.Lerp(ThornSprayUtils.ThornGreen, ThornSprayUtils.ExplosionGold, progress);
            Color col = ThornSprayUtils.Additive(baseCol, 0.5f + progress * 0.5f);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                growScale, SpriteEffects.None, 0f);

            // Inner white warning core at high progress
            if (progress > 0.6f)
            {
                float warnAlpha = (progress - 0.6f) / 0.4f;
                Color warnCol = ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, warnAlpha * 0.5f * pulse);
                sb.Draw(tex, Position - Main.screenPosition, null, warnCol, Rotation, origin,
                    growScale * 0.4f, SpriteEffects.None, 0f);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ChainExplosionParticle — golden ring expanding outward on detonation
    // Additive, ring rendered as scaled bloom with hollow center faked by alpha
    // ═══════════════════════════════════════════════════════════
    public class ChainExplosionParticle : ThornSprayParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public ChainExplosionParticle(Vector2 position, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = ThornSprayUtils.ExplosionGold;
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

            // Ring expands quickly then fades
            float progress = LifeRatio;
            if (progress < 0.25f)
                Scale = MathHelper.Lerp(0.1f, _maxScale, progress / 0.25f);
            else
                Scale = MathHelper.Lerp(_maxScale, _maxScale * 1.3f, (progress - 0.25f) / 0.75f);
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float outerAlpha = fade * fade * 0.7f;
            float innerAlpha = fade * fade * 0.35f;

            // Outer golden ring
            Color outerCol = ThornSprayUtils.Additive(
                Color.Lerp(ThornSprayUtils.ExplosionGold, ThornSprayUtils.AmberWarn, LifeRatio * 0.5f),
                outerAlpha);
            sb.Draw(tex, Position - Main.screenPosition, null, outerCol, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner hot white core (fades faster to create ring illusion)
            Color innerCol = ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, innerAlpha);
            sb.Draw(tex, Position - Main.screenPosition, null, innerCol, Rotation, origin,
                Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SplinterTrailParticle — small green trail dot behind splinters, additive
    // ═══════════════════════════════════════════════════════════
    public class SplinterTrailParticle : ThornSprayParticle
    {
        private static Asset<Texture2D> _texture;

        public SplinterTrailParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ThornSprayUtils.ThornGreen, ThornSprayUtils.VerdantBolt, Main.rand.NextFloat(0.6f));
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
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = ThornSprayUtils.Additive(DrawColor, fade * fade);

            float stretch = 1f + Velocity.Length() * 0.12f;
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ThornNoteParticle — music note burst on chain explosion
    // Additive, scatters outward from detonation, drifts upward with gentle sine wave
    // Uses PointBloom stretched into a note-like shape with rotation
    // ═══════════════════════════════════════════════════════════
    public class ThornNoteParticle : ThornSprayParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;
        private readonly float _rotSpeed;

        public ThornNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(ThornSprayUtils.ExplosionGold, ThornSprayUtils.AmberWarn, Main.rand.NextFloat());
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = true;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineFreq = Main.rand.NextFloat(0.06f, 0.12f);
            _rotSpeed = Main.rand.NextFloat(-0.1f, 0.1f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
            Velocity.Y -= 0.04f; // drifts upward like rising music
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.6f;
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

            // Main note shape — asymmetric stretch to suggest a note head + stem
            Color noteCol = ThornSprayUtils.Additive(DrawColor, fadeAlpha * 0.8f);
            sb.Draw(tex, Position - Main.screenPosition, null, noteCol, Rotation, origin,
                new Vector2(Scale * 0.6f, Scale * 0.35f), SpriteEffects.None, 0f);

            // Bright core
            Color coreCol = ThornSprayUtils.Additive(ThornSprayUtils.FlashWhite, fadeAlpha * 0.4f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.2f, SpriteEffects.None, 0f);
        }
    }
}
