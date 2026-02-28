using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Utilities;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Particles
{
    // =================================================================
    // TEXTURE CACHE
    // =================================================================

    /// <summary>Lazy texture loader for comet particle assets.</summary>
    internal static class CometTextures
    {
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starSoft;
        private static Asset<Texture2D> _musicNote1;
        private static Asset<Texture2D> _circularMask;

        public static Texture2D PointBloom => (_pointBloom ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom")).Value;
        public static Texture2D SoftRadialBloom => (_softRadialBloom ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom")).Value;
        public static Texture2D StarSoft => (_starSoft ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft")).Value;
        public static Texture2D MusicNote => (_musicNote1 ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/Particles Asset Library/MusicNote")).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle")).Value;
    }

    // =================================================================
    // EMBER TRAIL PARTICLE — burning embers jetting off comet trails
    // =================================================================

    /// <summary>
    /// Burning ember particle that jets off comet projectile trails.
    /// Starts bright gold-white, cools to deep violet, then fades.
    /// </summary>
    public class EmberTrailParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _initialScale;

        public EmberTrailParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            _initialScale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            DrawColor = CometUtils.CometCoreWhite;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;

            // Cooling gradient: white-hot → gold → violet → fade
            DrawColor = CometUtils.GetCometGradient(t);

            // Shrink and slow down
            Scale = _initialScale * (1f - CometUtils.PolyIn(t));
            Velocity *= 0.96f;
            Rotation += 0.1f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.PointBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            // Stretch ember along velocity for speed feel
            float stretch = Math.Max(1f, Velocity.Length() * 0.3f);
            float velAngle = Velocity.Length() > 0.5f ? Velocity.ToRotation() : Rotation;

            sb.Draw(tex, drawPos, null, DrawColor * (1f - LifetimeCompletion),
                velAngle, origin, new Vector2(stretch, 1f) * Scale * 0.5f,
                SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // CRATER BLOOM PARTICLE — expanding bloom at ricochet/impact points
    // =================================================================

    /// <summary>
    /// Expanding soft bloom at ricochet points and impact craters.
    /// Rapid expansion with smooth fade-out.
    /// </summary>
    public class CraterBloomParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public CraterBloomParticle(Vector2 position, Color color, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = color;
            _maxScale = maxScale;
            Lifetime = lifetime;
            Scale = 0f;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            // SineOut for fast expansion, then hold
            Scale = _maxScale * CometUtils.SineOut(Math.Min(t * 2f, 1f));
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.SoftRadialBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = 1f - CometUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.8f,
                0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // SHOCKWAVE RING PARTICLE — expanding ring for supernova detonation
    // =================================================================

    /// <summary>
    /// Expanding ring shockwave for Supernova detonation.
    /// Thin expanding circle that fades rapidly.
    /// </summary>
    public class ShockwaveRingParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public ShockwaveRingParticle(Vector2 position, Color color, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = color;
            _maxScale = maxScale;
            Lifetime = lifetime;
            Scale = 0f;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * CometUtils.ExpoOut(t);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.CircularMask;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = 1f - LifetimeCompletion * LifetimeCompletion;
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.6f,
                0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // LUNAR SHARD PARTICLE — angular shards flying from impacts
    // =================================================================

    /// <summary>
    /// Angular lunar shard particle flung from ricochet impacts.
    /// Spins and fades with gravity-like drift.
    /// </summary>
    public class LunarShardParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        private readonly float _spinRate;

        public LunarShardParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _spinRate = Main.rand.NextFloat(-0.15f, 0.15f);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.Y += 0.05f; // slight gravity drift
            Rotation += _spinRate;
            Scale *= 0.99f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.StarSoft;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = 1f - CometUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade,
                Rotation, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // COMET MIST PARTICLE — ambient mist around projectile heads
    // =================================================================

    /// <summary>
    /// Soft expanding mist around comet projectile heads and crater impacts.
    /// Gentle drift with sinusoidal alpha breathing.
    /// </summary>
    public class CometMistParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public CometMistParticle(Vector2 position, Vector2 velocity, Color color, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            _maxScale = maxScale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Scale = 0.1f;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * CometUtils.SineBump(t);
            Velocity *= 0.98f;
            Rotation += 0.01f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.SoftRadialBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float alpha = CometUtils.SineBump(LifetimeCompletion) * 0.35f;
            sb.Draw(tex, drawPos, null, DrawColor * alpha,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
