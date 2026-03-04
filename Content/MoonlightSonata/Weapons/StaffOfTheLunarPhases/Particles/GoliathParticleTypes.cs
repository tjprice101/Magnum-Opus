using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Particles
{
    // =================================================================
    // TEXTURE CACHE
    // =================================================================

    /// <summary>Lazy texture loader for Goliath particle assets — includes Moonlight Sonata theme-specific textures.</summary>
    internal static class GoliathTextures
    {
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starSoft;
        private static Asset<Texture2D> _musicNote1;
        private static Asset<Texture2D> _musicNote2;
        private static Asset<Texture2D> _circularMask;
        private static Asset<Texture2D> _energyFlare;
        private static Asset<Texture2D> _glyph;

        // === Moonlight Sonata Theme-Specific Textures ===
        private static Asset<Texture2D> _msStarFlare;
        private static Asset<Texture2D> _msLensFlare;
        private static Asset<Texture2D> _msGlowOrb;
        private static Asset<Texture2D> _msCrescentMoon;
        private static Asset<Texture2D> _msMusicNote;
        private static Asset<Texture2D> _msHarmonicImpact;
        private static Asset<Texture2D> _msPowerEffectRing;
        private static Asset<Texture2D> _msGradientLUT;

        // Generic shared textures
        public static Texture2D PointBloom => (_pointBloom ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom")).Value;
        public static Texture2D SoftRadialBloom => (_softRadialBloom ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom")).Value;
        public static Texture2D StarSoft => (_starSoft ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft")).Value;
        public static Texture2D MusicNote1 => (_musicNote1 ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/Particles Asset Library/MusicNote")).Value;
        public static Texture2D MusicNote2 => (_musicNote2 ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote")).Value;
        public static Texture2D CircularMask => (_circularMask ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle")).Value;
        public static Texture2D EnergyFlare => (_energyFlare ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/SandboxLastPrism/Pixel/Flare")).Value;
        public static Texture2D Glyph => (_glyph ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/Particles Asset Library/MusicNote")).Value;

        // Moonlight Sonata theme-specific — glow and bloom
        public static Texture2D MSStarFlare => (_msStarFlare ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Star Flare")).Value;
        public static Texture2D MSLensFlare => (_msLensFlare ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Lens Flare")).Value;
        public static Texture2D MSGlowOrb => (_msGlowOrb ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Glow Orb")).Value;

        // Moonlight Sonata theme-specific — particles
        public static Texture2D MSCrescentMoon => (_msCrescentMoon ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Crescent Moon")).Value;
        public static Texture2D MSMusicNote => (_msMusicNote ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Music Note")).Value;

        // Moonlight Sonata theme-specific — impacts
        public static Texture2D MSHarmonicImpact => (_msHarmonicImpact ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Impacts/MS Harmonic Resonance Wave Impact")).Value;
        public static Texture2D MSPowerEffectRing => (_msPowerEffectRing ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Impacts/MS Power Effect Ring")).Value;

        // Moonlight Sonata color gradient LUT
        public static Texture2D MSGradientLUT => (_msGradientLUT ??= ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP")).Value;
    }

    // =================================================================
    // BEAM SPARK PARTICLE — sparkles along moonlight beam paths
    // =================================================================

    /// <summary>
    /// Bright spark particle that scatters along Goliath beam projectile paths.
    /// Starts ice-blue-white, fades through nebula purple.
    /// </summary>
    public class BeamSparkParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _initialScale;

        public BeamSparkParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            _initialScale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            DrawColor = GoliathUtils.IceBlueBrilliance;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            DrawColor = GoliathUtils.GetCosmicGradient(0.6f + t * 0.4f);
            Scale = _initialScale * (1f - GoliathUtils.PolyIn(t));
            Velocity *= 0.94f;
            Rotation += 0.15f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.StarSoft;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = 1f - LifetimeCompletion;
            sb.Draw(tex, drawPos, null, DrawColor * fade,
                Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // RIFT MOTE PARTICLE — swirling cosmic dust around the Goliath
    // =================================================================

    /// <summary>
    /// Slow drifting cosmic mote that orbits near the Goliath.
    /// Creates an ambient gravitational rift feel.
    /// </summary>
    public class RiftMoteParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly Vector2 _anchor;
        private readonly float _orbitRadius;
        private float _orbitAngle;
        private readonly float _orbitSpeed;

        public RiftMoteParticle(Vector2 anchor, float orbitRadius, float startAngle, float scale, int lifetime)
        {
            _anchor = anchor;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = 0.02f + Main.rand.NextFloat(0.02f);
            Position = anchor + _orbitAngle.ToRotationVector2() * orbitRadius;
            Velocity = Vector2.Zero;
            Scale = scale;
            Lifetime = lifetime;
            DrawColor = GoliathUtils.GravityWell;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            _orbitAngle += _orbitSpeed;
            float currentRadius = _orbitRadius * (1f - t * 0.3f);
            Position = _anchor + _orbitAngle.ToRotationVector2() * currentRadius;
            DrawColor = GoliathUtils.GetCosmicGradient(0.2f + t * 0.5f);
            Scale *= 0.998f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.PointBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = GoliathUtils.SineBump(LifetimeCompletion) * 0.6f;
            sb.Draw(tex, drawPos, null, DrawColor * fade,
                0f, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // SUMMON GLOW PARTICLE — radiating glow during summoning ritual
    // =================================================================

    /// <summary>
    /// Expanding radial glow that appears during the summoning ritual.
    /// Fast expansion, slow fade, matches the summon circle shader effect.
    /// </summary>
    public class SummonGlowParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public SummonGlowParticle(Vector2 position, Color color, float maxScale, int lifetime)
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
            Scale = _maxScale * GoliathUtils.SineOut(Math.Min(t * 2f, 1f));
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.SoftRadialBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = 1f - GoliathUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.7f,
                0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // MUSIC NOTE PARTICLE — musical motifs scattered from beam impacts
    // =================================================================

    /// <summary>
    /// Floating music note that drifts upward from beam impact points.
    /// Musical identity of the Moonlight Sonata theme.
    /// </summary>
    public class MusicNoteParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly bool _useAltNote;
        private readonly float _wobblePhase;

        public MusicNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            _useAltNote = Main.rand.NextBool();
            _wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Velocity.Y -= 0.03f; // float upward
            Velocity.X += MathF.Sin(_wobblePhase + t * 8f) * 0.05f; // gentle wobble
            Velocity *= 0.98f;
            Scale *= 0.997f;
            Rotation += 0.02f * MathF.Sin(t * 4f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = _useAltNote ? GoliathTextures.MusicNote2 : GoliathTextures.MusicNote1;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = 1f - GoliathUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.8f,
                Rotation, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // GRAVITY WELL PARTICLE — pulled-in particles for rift effect
    // =================================================================

    /// <summary>
    /// Particle that spirals inward toward the Goliath's body,
    /// creating a visual gravity well / event horizon effect.
    /// </summary>
    public class GravityWellParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly Vector2 _target;
        private readonly float _pullStrength;

        public GravityWellParticle(Vector2 position, Vector2 target, float scale, int lifetime)
        {
            Position = position;
            _target = target;
            Velocity = (position - target).SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 3f;
            Scale = scale;
            Lifetime = lifetime;
            _pullStrength = 0.15f + Main.rand.NextFloat(0.1f);
            DrawColor = GoliathUtils.EnergyTendril;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Vector2 toTarget = _target - Position;
            float dist = toTarget.Length();
            if (dist > 1f)
            {
                toTarget /= dist;
                Velocity += toTarget * _pullStrength;
            }
            Velocity *= 0.96f;
            DrawColor = GoliathUtils.GetCosmicGradient(0.3f + t * 0.6f);
            Scale *= 0.99f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.PointBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = 1f - GoliathUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.5f,
                0f, origin, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // CONDUCTOR GLYPH PARTICLE — arcane glyphs during Conductor Mode
    // =================================================================

    /// <summary>
    /// Slowly rotating arcane glyph that orbits during Conductor Mode.
    /// Represents the conductor's will directing the Goliath's beams.
    /// </summary>
    public class ConductorGlyphParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _rotSpeed;

        public ConductorGlyphParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _rotSpeed = 0.03f + Main.rand.NextFloat(0.02f);
            DrawColor = GoliathUtils.ConductorHighlight;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Velocity *= 0.95f;
            Rotation += _rotSpeed;
            Scale *= 0.995f;
            DrawColor = Color.Lerp(GoliathUtils.ConductorHighlight, GoliathUtils.NebulaPurple, t);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.Glyph;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = GoliathUtils.SineBump(LifetimeCompletion) * 0.7f;
            sb.Draw(tex, drawPos, null, DrawColor * fade,
                Rotation, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // IMPACT BLOOM PARTICLE — expanding bloom at beam hit points
    // =================================================================

    /// <summary>
    /// Expanding soft bloom at beam impact points.
    /// Rapid expansion with smooth fade-out.
    /// </summary>
    public class ImpactBloomParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public ImpactBloomParticle(Vector2 position, Color color, float maxScale, int lifetime)
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
            Scale = _maxScale * GoliathUtils.SineOut(Math.Min(t * 2f, 1f));
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.SoftRadialBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;
            float fade = 1f - GoliathUtils.PolyIn(LifetimeCompletion);
            sb.Draw(tex, drawPos, null, DrawColor * fade * 0.8f,
                0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // LUNAR PHASE RING — expanding ring showing current lunar phase
    // =================================================================

    /// <summary>
    /// Expanding phase ring that pulses around the Goliath
    /// to indicate the current lunar phase. Color shifts with phase.
    /// </summary>
    public class LunarPhaseRingParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public LunarPhaseRingParticle(Vector2 position, Color color, float maxScale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            DrawColor = color;
            _maxScale = maxScale;
            Lifetime = lifetime;
            Scale = 0.1f;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * GoliathUtils.ExpoOut(t);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float t = LifetimeCompletion;
            float alpha = (1f - t * t) * 0.4f;
            Color color = DrawColor * alpha;

            Texture2D tex = GoliathTextures.CircularMask;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            // Outer ring
            sb.Draw(tex, drawPos, null, color,
                0f, origin, Scale, SpriteEffects.None, 0f);
            // Inner ring (brighter, smaller)
            sb.Draw(tex, drawPos, null, color * 0.7f,
                0f, origin, Scale * 0.65f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // LUNAR HEALING PARTICLE — soft orb arcing from Goliath to player
    // =================================================================

    /// <summary>
    /// Soft healing orb that arcs from the Goliath toward the player during Waning phase.
    /// Fades from lavender to white as it approaches.
    /// </summary>
    public class LunarHealingParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly Vector2 _target;
        private readonly Vector2 _controlPoint;

        public LunarHealingParticle(Vector2 start, Vector2 target, float scale, int lifetime)
        {
            Position = start;
            _target = target;
            // Arc control point — offset perpendicular to the line connecting start and target
            Vector2 mid = (start + target) * 0.5f;
            Vector2 perpendicular = new(-(target.Y - start.Y), target.X - start.X);
            perpendicular.Normalize();
            _controlPoint = mid + perpendicular * (50f + Main.rand.NextFloat(30f)) * (Main.rand.NextBool() ? 1 : -1);
            Velocity = Vector2.Zero;
            Scale = scale;
            Lifetime = lifetime;
            DrawColor = new Color(160, 130, 220); // Waning lavender
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            // Quadratic Bezier interpolation
            float inv = 1f - t;
            Vector2 startPos = Position;
            // Recalculate from original start... actually we need stored start
            // Use simpler approach: lerp toward target with arc
            Vector2 straight = Vector2.Lerp(Position, _target, 0.08f);
            Vector2 toControl = _controlPoint - Position;
            float controlInfluence = GoliathUtils.SineBump(t) * 0.05f;
            Position = straight + toControl * controlInfluence;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float t = LifetimeCompletion;
            float alpha = GoliathUtils.SineBump(t) * 0.6f;
            Color color = Color.Lerp(DrawColor, GoliathUtils.SupermoonWhite, t) * alpha;

            Texture2D tex = GoliathTextures.PointBloom;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            // Soft bloom
            sb.Draw(tex, drawPos, null, color * 0.5f,
                0f, origin, Scale * 0.6f, SpriteEffects.None, 0f);
            // Core
            sb.Draw(tex, drawPos, null, color,
                0f, origin, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }

    // =================================================================
    // THEME-SPECIFIC GOLIATH PARTICLES — Moonlight Sonata VFX Library
    // =================================================================

    /// <summary>
    /// Goliath star flare using the MS Star Flare texture.
    /// Bright 4-pointed flare that appears at beam impact points and phase transitions.
    /// </summary>
    public class GoliathStarFlareParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _pulseSpeed;

        public GoliathStarFlareParticle(Vector2 position, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _pulseSpeed = 0.1f + Main.rand.NextFloat(0.08f);
        }

        public override void Update()
        {
            Rotation += 0.02f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.MSStarFlare;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float pulse = 0.75f + 0.25f * (float)Math.Sin(Time * _pulseSpeed);
            float opacity = (1f - (float)Math.Pow(LifetimeCompletion, 1.5f)) * pulse;
            Color color = Color.Lerp(DrawColor, GoliathUtils.SupermoonWhite, pulse * 0.3f);
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                Rotation, origin, Scale * 0.35f * pulse, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Goliath crescent moon particle using the MS Crescent Moon texture.
    /// Drifts from the Goliath during phase transitions, representing lunar cycles.
    /// </summary>
    public class GoliathCrescentParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _wobblePhase;

        public GoliathCrescentParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
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
            Velocity.Y -= 0.02f;
            Rotation += (float)Math.Sin(_wobblePhase + Time * 0.05f) * 0.02f;
            Scale *= 0.998f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.MSCrescentMoon;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
            Color color = Color.Lerp(DrawColor, GoliathUtils.NebulaPurple, LifetimeCompletion * 0.4f);
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                Rotation, origin, Scale * 0.2f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Goliath glow orb using the MS Glow Orb texture.
    /// Ethereal lunar bloom layer for Goliath body and beam impacts.
    /// </summary>
    public class GoliathGlowOrbParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _pulseRate;

        public GoliathGlowOrbParticle(Vector2 position, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _pulseRate = 0.08f + Main.rand.NextFloat(0.04f);
        }

        public override void Update() { }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.MSGlowOrb;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Time * _pulseRate);
            float opacity = (1f - LifetimeCompletion * LifetimeCompletion) * pulse * 0.5f;
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity,
                0f, origin, Scale * 0.3f * pulse, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Goliath harmonic wave impact using the MS Harmonic Resonance Wave Impact texture.
    /// Expanding concentric wave on Goliath phase transition moments.
    /// </summary>
    public class GoliathHarmonicWaveParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public GoliathHarmonicWaveParticle(Vector2 position, Color color, float maxScale, int lifetime)
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
            Scale = _maxScale * GoliathUtils.ExpoOut(t);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.MSHarmonicImpact;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float fade = (1f - LifetimeCompletion * LifetimeCompletion) * 0.6f;
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * fade,
                0f, origin, Scale * 0.45f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Goliath lens flare using the MS Lens Flare texture.
    /// Cinematic flare at supermoon beam focus points.
    /// </summary>
    public class GoliathLensFlareParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public GoliathLensFlareParticle(Vector2 position, float scale, Color color, int lifetime)
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
            Scale *= 1.003f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.MSLensFlare;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2f);
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity * 0.7f,
                Rotation, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Goliath themed music note using the MS Music Note texture.
    /// Floats upward from beam impacts and phase transitions.
    /// </summary>
    public class GoliathMoonlightNoteParticle : GoliathParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _wavePhase;

        public GoliathMoonlightNoteParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
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
            Velocity *= 0.95f;
            Velocity.Y -= 0.04f;
            Position.X += (float)Math.Sin(_wavePhase + Time * 0.05f) * 0.4f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            Texture2D tex = GoliathTextures.MSMusicNote;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Position - Main.screenPosition;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2f);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Time * 0.08f);
            Color color = DrawColor;
            color.A = 0;

            sb.Draw(tex, drawPos, null, color * opacity * pulse,
                Rotation, origin, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }
}
