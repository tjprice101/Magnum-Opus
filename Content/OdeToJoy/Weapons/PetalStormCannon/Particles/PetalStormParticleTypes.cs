using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Particles
{
    // ═══════════════════════════════════════════════════════════
    // StormPetalParticle — swirling petal caught in a vortex updraft
    // Alpha blend, rose-gold color, flutters with sine wave, orbits center
    // ═══════════════════════════════════════════════════════════
    public class StormPetalParticle : PetalStormParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _rotSpeed;
        private readonly float _sineOffset;
        private readonly float _sineFreq;
        private readonly Vector2 _orbitCenter;
        private float _orbitAngle;
        private readonly float _orbitRadius;
        private readonly float _orbitSpeed;

        public StormPetalParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.04f, 0.09f);
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Velocity = new Vector2(0f, Main.rand.NextFloat(-1.8f, -0.6f)); // upward drift
            DrawColor = Color.Lerp(PetalStormUtils.RoseBurst, PetalStormUtils.AmberFlame, Main.rand.NextFloat(0.4f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
            _sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _sineFreq = Main.rand.NextFloat(0.05f, 0.1f);
        }

        public override void Update()
        {
            base.Update();
            _orbitAngle += _orbitSpeed;
            float shrinkFactor = 1f - LifeRatio * 0.3f;
            float currentRadius = _orbitRadius * shrinkFactor;
            Position = _orbitCenter + _orbitAngle.ToRotationVector2() * currentRadius;
            Position.Y += Velocity.Y * Lifetime * 0.5f; // cumulative upward drift
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 1.2f;
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

            // Stretched ellipse for petal shape
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(Scale * 0.5f, Scale * 0.25f), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ExplosionBloomParticle — golden flash on bomb detonation, additive, expands + fades fast
    // ═══════════════════════════════════════════════════════════
    public class ExplosionBloomParticle : PetalStormParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public ExplosionBloomParticle(Vector2 position, Vector2 velocity, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = PetalStormUtils.GoldenExplosion;
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
            Velocity *= 0.82f;

            // Expand quickly then shrink
            float progress = LifeRatio;
            if (progress < 0.2f)
                Scale = MathHelper.Lerp(0.1f, _maxScale, progress / 0.2f);
            else
                Scale = MathHelper.Lerp(_maxScale, 0f, (progress - 0.2f) / 0.8f);
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            Color col = PetalStormUtils.Additive(
                Color.Lerp(PetalStormUtils.GoldenExplosion, PetalStormUtils.WhiteFlash, LifeRatio * 0.5f),
                fade);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);

            // Inner white core
            Color coreCol = PetalStormUtils.Additive(PetalStormUtils.WhiteFlash, fade * 0.6f);
            sb.Draw(tex, Position - Main.screenPosition, null, coreCol, Rotation, origin,
                Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // CannonSmokeParticle — thick gold-tinted smoke, alpha blend, expands and drifts
    // ═══════════════════════════════════════════════════════════
    public class CannonSmokeParticle : PetalStormParticle
    {
        private static Asset<Texture2D> _texture;

        public CannonSmokeParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = Color.Lerp(PetalStormUtils.CannonBronze, PetalStormUtils.AmberFlame, Main.rand.NextFloat(0.5f));
            Scale = scale;
            MaxLifetime = lifetime;
            Lifetime = 0;
            IsAdditive = false;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.02f; // drifts upward slightly
            Scale += 0.005f; // expands over time
            Rotation += 0.01f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade * 0.6f; // smoke is semi-transparent
            Color col = DrawColor * fadeAlpha;

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ShrapnelTrailParticle — small green-gold trail dot behind shrapnel, additive, velocity-stretched
    // ═══════════════════════════════════════════════════════════
    public class ShrapnelTrailParticle : PetalStormParticle
    {
        private static Asset<Texture2D> _texture;

        public ShrapnelTrailParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.92f;
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
            float stretchX = Scale * (1f + Velocity.Length() * 0.15f);
            float stretchY = Scale * 0.35f;

            Color col = PetalStormUtils.Additive(DrawColor, fade);
            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VortexNoteParticle — music note caught in vortex updraft, additive, golden, drifts upward with sine
    // ═══════════════════════════════════════════════════════════
    public class VortexNoteParticle : PetalStormParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _sineOffset;
        private readonly float _sineFreq;
        private readonly Vector2 _orbitCenter;
        private float _orbitAngle;
        private readonly float _orbitRadius;
        private readonly float _orbitSpeed;

        public VortexNoteParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.05f, 0.1f);
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Velocity = new Vector2(0f, Main.rand.NextFloat(-2.0f, -0.8f));
            DrawColor = PetalStormUtils.GoldenExplosion;
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
            _orbitAngle += _orbitSpeed;
            float shrinkFactor = 1f - LifeRatio * 0.4f;
            float currentRadius = _orbitRadius * shrinkFactor;
            Position = _orbitCenter + _orbitAngle.ToRotationVector2() * currentRadius;
            Position.Y += Velocity.Y * Lifetime * 0.4f;
            Position.X += (float)Math.Sin(Lifetime * _sineFreq + _sineOffset) * 0.9f;
        }

        public override void Draw(SpriteBatch sb)
        {
            _texture ??= ModContent.GetInstance<MagnumOpus>().Assets.Request<Texture2D>(
                "Assets/Particles Asset Library/MusicNote", AssetRequestMode.ImmediateLoad);

            Texture2D tex = _texture.Value;
            Vector2 origin = tex.Size() / 2f;

            float fade = 1f - LifeRatio;
            float fadeAlpha = fade * fade;
            Color col = PetalStormUtils.Additive(
                Color.Lerp(PetalStormUtils.GoldenExplosion, PetalStormUtils.WhiteFlash, LifeRatio * 0.4f),
                fadeAlpha);

            sb.Draw(tex, Position - Main.screenPosition, null, col, Rotation, origin,
                Scale, SpriteEffects.None, 0f);
        }
    }
}
