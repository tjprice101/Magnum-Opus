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

    /// <summary>Lazy texture loader for comet particle assets — includes Moonlight Sonata theme-specific textures.</summary>
    internal static class CometTextures
    {
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starSoft;
        private static Asset<Texture2D> _musicNote1;
        private static Asset<Texture2D> _circularMask;

        // === Moonlight Sonata Theme-Specific Textures ===
        private static Asset<Texture2D> _msStarFlare;
        private static Asset<Texture2D> _msLensFlare;
        private static Asset<Texture2D> _msGlowOrb;
        private static Asset<Texture2D> _msCrescentMoon;
        private static Asset<Texture2D> _msMusicNote;
        private static Asset<Texture2D> _msTidalMistWisp;
        private static Asset<Texture2D> _msHarmonicImpact;
        private static Asset<Texture2D> _msPowerEffectRing;
        private static Asset<Texture2D> _msEnergyMotionBeam;
        private static Asset<Texture2D> _msEnergySurgeBeam;
        private static Asset<Texture2D> _msGradientLUT;

        // Generic shared textures
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

        // Moonlight Sonata theme-specific textures — glow, bloom, and flares
        public static Texture2D MSStarFlare => (_msStarFlare ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Star Flare")).Value;
        public static Texture2D MSLensFlare => (_msLensFlare ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Lens Flare")).Value;
        public static Texture2D MSGlowOrb => (_msGlowOrb ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Glow Orb")).Value;

        // Moonlight Sonata theme-specific particles
        public static Texture2D MSCrescentMoon => (_msCrescentMoon ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Crescent Moon")).Value;
        public static Texture2D MSMusicNote => (_msMusicNote ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Music Note")).Value;
        public static Texture2D MSTidalMistWisp => (_msTidalMistWisp ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Tidal Mist Wisp")).Value;

        // Moonlight Sonata theme-specific impacts
        public static Texture2D MSHarmonicImpact => (_msHarmonicImpact ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Impacts/MS Harmonic Resonance Wave Impact")).Value;
        public static Texture2D MSPowerEffectRing => (_msPowerEffectRing ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Impacts/MS Power Effect Ring")).Value;

        // Moonlight Sonata theme-specific beam textures
        public static Texture2D MSEnergyMotionBeam => (_msEnergyMotionBeam ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Beam Textures/MS Energy Motion Beam")).Value;
        public static Texture2D MSEnergySurgeBeam => (_msEnergySurgeBeam ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Beam Textures/MS Energy Surge Beam")).Value;

        // Moonlight Sonata color gradient LUT
        public static Texture2D MSGradientLUT => (_msGradientLUT ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP")).Value;
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

    // =================================================================
    // MOONRISE CHARGE PARTICLE — spirals inward during charge hold
    // =================================================================

    /// <summary>
    /// Moonrise charge particle that spirals inward toward the gun barrel
    /// during a charge hold. Logarithmic spiral path, consumed at center.
    /// Uses existing SoftGlow texture.
    /// </summary>
    public class MoonriseChargeParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly Vector2 _target;
        private readonly float _initialRadius;
        private readonly float _spiralAngle;
        private readonly float _initialScale;

        public MoonriseChargeParticle(Vector2 startPos, Vector2 target, float scale, Color color, int lifetime)
        {
            Position = startPos;
            _target = target;
            _initialRadius = Vector2.Distance(startPos, target);
            _spiralAngle = (startPos - target).ToRotation();
            _initialScale = scale;
            Scale = scale;
            Velocity = Vector2.Zero;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;

            // Logarithmic spiral inward
            float currentRadius = _initialRadius * (1f - CometUtils.PolyIn(t, 1.5f));
            float currentAngle = _spiralAngle + t * MathHelper.TwoPi * 2.5f;
            Position = _target + currentAngle.ToRotationVector2() * currentRadius;

            // Shrink as it approaches target
            Scale = _initialScale * (1f - t * 0.8f);
            Rotation += 0.15f;

            // Color shift: starts cool, heats up
            DrawColor = Color.Lerp(CometUtils.DeepSpaceViolet, CometUtils.CometCoreWhite, t);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.PointBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = 1f - CometUtils.PolyIn(LifetimeCompletion) * 0.5f;
            sb.Draw(tex, drawPos, null, DrawColor * fade,
                Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // LUNAR CYCLE INDICATOR PARTICLE — visual phase indicator ring
    // =================================================================

    /// <summary>
    /// Expanding ring that indicates the current lunar cycle phase on shot.
    /// Uses the SoftCircle mask texture. Phase color differentiates the visuals.
    /// </summary>
    public class LunarCycleRingParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public LunarCycleRingParticle(Vector2 position, Color color, float maxScale, int lifetime)
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
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.5f,
                0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // SUPERNOVA DEBRIS PARTICLE — lunar rock tumbling from explosions
    // =================================================================

    /// <summary>
    /// Gravity-affected lunar debris particle flung from supernova detonations.
    /// Tumbles with rotation, fades as it falls.
    /// </summary>
    public class SupernovaDebrisParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        private readonly float _spinRate;

        public SupernovaDebrisParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _spinRate = Main.rand.NextFloat(-0.2f, 0.2f);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.Y += 0.1f; // Gravity
            Rotation += _spinRate;
            Scale *= 0.995f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.StarSoft;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = 1f - CometUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade,
                Rotation, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // THEME-SPECIFIC PARTICLES — Moonlight Sonata VFX Library
    // =================================================================

    /// <summary>
    /// Supernova corona particle using the themed MS Star Flare texture.
    /// A 4-pointed star burst that radiates from the projectile head,
    /// conveying the violent brilliance of a supernova shell in flight.
    /// </summary>
    public class SupernovaStarFlareParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _pulseRate;

        public SupernovaStarFlareParticle(Vector2 position, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _pulseRate = 0.12f + Main.rand.NextFloat(0.06f);
        }

        public override void Update()
        {
            Rotation += 0.02f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSStarFlare;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float pulse = 0.7f + 0.3f * (float)Math.Sin(Time * _pulseRate);
            float opacity = (1f - (float)Math.Pow(LifetimeCompletion, 1.5f)) * pulse;
            Color color = Color.Lerp(DrawColor, CometUtils.CometCoreWhite, pulse * 0.3f);
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                Rotation, origin, Scale * 0.3f * pulse, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Lunar lens flare particle using the themed MS Lens Flare texture.
    /// Appears at muzzle flash and supernova impact points for a cinematic lens effect.
    /// </summary>
    public class LunarLensFlareParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public LunarLensFlareParticle(Vector2 position, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Rotation += 0.01f;
            Scale *= 1.005f; // Gentle expansion
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSLensFlare;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2f);
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity * 0.8f,
                Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Crescent moon particle using the themed MS Crescent Moon texture.
    /// Floats away from explosions with gentle drift, representing the "Resurrection" of the moon.
    /// </summary>
    public class ResurrectionCrescentParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _wobblePhase;

        public ResurrectionCrescentParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation();
            _wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Velocity.Y -= 0.02f; // Float upward gently
            Rotation += (float)Math.Sin(_wobblePhase + Time * 0.06f) * 0.03f;
            Scale *= 0.997f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSCrescentMoon;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
            Color color = Color.Lerp(DrawColor, CometUtils.DeepSpaceViolet, LifetimeCompletion * 0.4f);
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                Rotation, origin, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Harmonic resonance wave impact particle using the themed MS Harmonic Resonance Wave Impact texture.
    /// Expanding concentric wave that appears at supernova detonation points.
    /// </summary>
    public class HarmonicResonanceWaveParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public HarmonicResonanceWaveParticle(Vector2 position, Color color, float maxScale, int lifetime)
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
            Texture2D tex = CometTextures.MSHarmonicImpact;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = (1f - LifetimeCompletion * LifetimeCompletion) * 0.7f;
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * fade,
                0f, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Power effect ring using the themed MS Power Effect Ring texture.
    /// A concentrated ring burst that expands on supernova shell impacts and crescent wave events.
    /// </summary>
    public class LunarPowerRingParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public LunarPowerRingParticle(Vector2 position, Color color, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = color;
            _maxScale = maxScale;
            Lifetime = lifetime;
            Scale = 0.1f;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * CometUtils.ExpoOut(t);
            Rotation += 0.01f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSPowerEffectRing;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = (1f - (float)Math.Pow(LifetimeCompletion, 1.5f)) * 0.6f;
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * fade,
                Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Tidal mist wisp using the themed MS Tidal Mist Wisp texture.
    /// Slow-drifting atmospheric mist that enhances the tidal/lunar ambience of the weapon.
    /// </summary>
    public class TidalMistWispParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TidalMistWispParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Scale += 0.008f;
            Rotation += 0.003f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSTidalMistWisp;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = CometUtils.SineBump(LifetimeCompletion) * 0.3f;
            Color color = Color.Lerp(DrawColor, CometUtils.DeepSpaceViolet, LifetimeCompletion * 0.5f);
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                Rotation, origin, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Themed music note particle using the MS Music Note texture.
    /// Adds the Moonlight Sonata-specific note shape to projectile impacts and muzzle effects.
    /// </summary>
    public class MoonlightMusicNoteParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _wavePhase;

        public MoonlightMusicNoteParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _wavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Velocity.Y -= 0.03f; // Float upward
            Position.X += (float)Math.Sin(_wavePhase + Time * 0.06f) * 0.4f;
            Rotation += Velocity.X * 0.01f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSMusicNote;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2f);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Time * 0.1f);
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity * pulse,
                Rotation, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Themed glow orb using the MS Glow Orb texture. 
    /// A soft, ethereal lunar glow that acts as an enhanced bloom layer for projectile heads and impacts.
    /// </summary>
    public class MoonlightGlowOrbParticle : CometParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _pulseSpeed;

        public MoonlightGlowOrbParticle(Vector2 position, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _pulseSpeed = 0.1f + Main.rand.NextFloat(0.05f);
        }

        public override void Update() { }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = CometTextures.MSGlowOrb;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Time * _pulseSpeed);
            float opacity = (1f - (float)Math.Pow(LifetimeCompletion, 2f)) * pulse * 0.6f;
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                0f, origin, Scale * 0.35f * pulse, SpriteEffects.None, 0f);
        }
    }
}
