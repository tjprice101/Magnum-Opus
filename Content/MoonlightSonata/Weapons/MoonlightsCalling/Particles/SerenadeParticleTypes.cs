using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Particles
{
    /// <summary>
    /// Concrete particle types for Moonlight's Calling — "The Serenade".
    /// Each type provides a unique visual element for the prismatic beam weapon.
    /// </summary>

    // =========================================================================
    // 1. PRISMATIC SPARK — tiny directional spark that shifts hue over lifetime
    // =========================================================================
    public class PrismaticSparkParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly Color _startColor;
        private readonly Color _endColor;

        public PrismaticSparkParticle(Vector2 pos, Vector2 vel, Color startColor, Color endColor, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            _startColor = startColor;
            _endColor = endColor;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = vel.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Scale *= 0.97f;
            Rotation = Velocity.ToRotation();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float t = LifetimeCompletion;
            float alpha = 1f - t * t;
            Color color = Color.Lerp(_startColor, _endColor, t) * alpha;

            var tex = SerenadeTextures.PointBloom;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;
            float stretch = 1f + Velocity.Length() * 0.04f;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                Rotation, origin, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // 2. REFRACTION BLOOM — expanding soft glow at beam bounce points
    // =========================================================================
    public class RefractionBloomParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public RefractionBloomParticle(Vector2 pos, Color color, float maxScale, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            DrawColor = color;
            Scale = 0.1f;
            _maxScale = maxScale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * SerenadeUtils.SineOut(Math.Min(t * 3f, 1f));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float t = LifetimeCompletion;
            float alpha = (1f - t) * 0.7f;
            Color color = DrawColor * alpha;

            var tex = SerenadeTextures.SoftRadialBloom;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // 3. SPECTRAL NOTE — floating music note with spectral color cycling
    // =========================================================================
    public class SpectralNoteParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly int _noteVariant;
        private static readonly string[] NoteTextures = new[]
        {
            "CursiveMusicNote", "MusicNote", "QuarterNote", "TallMusicNote", "WholeNote"
        };

        public SpectralNoteParticle(Vector2 pos, Vector2 vel, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            Scale = scale;
            Lifetime = lifetime;
            _noteVariant = Main.rand.Next(NoteTextures.Length);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.95f;
            Velocity.Y -= 0.03f; // Float upward
            Rotation += 0.02f;
            Position.X += MathF.Sin(Time * 0.08f) * 0.5f; // Gentle sway
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float t = LifetimeCompletion;
            float fadeIn = Math.Min(t * 5f, 1f);
            float fadeOut = 1f - MathF.Pow(Math.Max(t - 0.6f, 0f) / 0.4f, 2f);
            float alpha = fadeIn * fadeOut;

            // Cycle through spectral colors over lifetime
            float hueT = (t * 2f + Time * 0.01f) % 1f;
            Color color = SerenadeUtils.MulticolorLerp(hueT, SerenadeUtils.SpectralColors) * alpha;

            var tex = SerenadeTextures.GetNoteTexture(_noteVariant);
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // 4. PRISM SHARD — angular shard flying outward from prismatic events
    // =========================================================================
    public class PrismShardParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public PrismShardParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = vel.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.93f;
            Rotation += 0.15f;
            Scale *= 0.98f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float alpha = 1f - LifetimeCompletion;
            Color color = DrawColor * alpha;

            var tex = SerenadeTextures.StarSoft;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                Rotation, origin, Scale * new Vector2(1.5f, 0.6f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // 5. SERENADE MIST — soft ambient mist for Serenade Mode channeling
    // =========================================================================
    public class SerenadeMistParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public SerenadeMistParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Scale += 0.01f;
            Rotation += 0.005f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float t = LifetimeCompletion;
            float alpha = SerenadeUtils.SineBump(t) * 0.35f;
            Color color = DrawColor * alpha;

            var tex = SerenadeTextures.SoftRadialBloom;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // 6. HARMONIC NODE — stationary pulsing star at standing wave positions
    // =========================================================================
    public class HarmonicNodeParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly Color _baseColor;
        private readonly Color _peakColor;

        public HarmonicNodeParticle(Vector2 pos, Color baseColor, Color peakColor, float scale, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            _baseColor = baseColor;
            _peakColor = peakColor;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Stationary — just pulse rotation
            Rotation += 0.04f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float t = LifetimeCompletion;
            // Pulse between base and peak with standing wave rhythm
            float pulse = (1f + MathF.Sin(Time * 0.25f)) * 0.5f;
            float alpha = SerenadeUtils.SineBump(t) * (0.5f + pulse * 0.5f);
            Color color = Color.Lerp(_baseColor, _peakColor, pulse) * alpha;

            var tex = SerenadeTextures.StarSoft;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;
            float scaleAnim = Scale * (0.8f + pulse * 0.4f);

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                Rotation, origin, scaleAnim, SpriteEffects.None, 0f);

            // Inner bright core
            Color coreColor = SerenadeUtils.MoonWhite * (alpha * pulse * 0.6f);
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, coreColor,
                Rotation + 0.5f, origin, scaleAnim * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // 7. RESONANCE PULSE — expanding ring on resonance level transition
    // =========================================================================
    public class ResonancePulseParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public ResonancePulseParticle(Vector2 pos, Color color, float maxScale, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            DrawColor = color;
            Scale = 0.2f;
            _maxScale = maxScale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * SerenadeUtils.ExpoOut(t);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            float t = LifetimeCompletion;
            float alpha = (1f - t * t) * 0.5f;
            Color color = DrawColor * alpha;

            var tex = SerenadeTextures.SoftCircle;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            // Outer ring
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color,
                0f, origin, Scale, SpriteEffects.None, 0f);

            // Inner ring (slightly smaller, brighter)
            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color * 0.6f,
                0f, origin, Scale * 0.7f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    // TEXTURE HELPER — lazy-loaded texture references for particles
    // (includes Moonlight Sonata theme-specific textures from VFX Library)
    // =========================================================================
    internal static class SerenadeTextures
    {
        private static Texture2D _pointBloom;
        private static Texture2D _softRadialBloom;
        private static Texture2D _starSoft;
        private static Texture2D _softCircle;
        private static Texture2D[] _noteTextures;

        // === Moonlight Sonata Theme-Specific Textures ===
        private static Texture2D _msStarFlare;
        private static Texture2D _msLensFlare;
        private static Texture2D _msGlowOrb;
        private static Texture2D _msCrescentMoon;
        private static Texture2D _msMusicNote;
        private static Texture2D _msTidalMistWisp;
        private static Texture2D _msHarmonicImpact;
        private static Texture2D _msPowerEffectRing;
        private static Texture2D _msEnergyMotionBeam;
        private static Texture2D _msEnergySurgeBeam;
        private static Texture2D _msHarmonicStandingWave;
        private static Texture2D _msGradientLUT;
        private static Texture2D _msBasicTrail;

        // Generic shared textures
        public static Texture2D PointBloom => _pointBloom ??= LoadTex("Assets/VFX Asset Library/GlowAndBloom/PointBloom");
        public static Texture2D SoftRadialBloom => _softRadialBloom ??= LoadTex("Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
        public static Texture2D StarSoft => _starSoft ??= LoadTex("Assets/Particles Asset Library/Stars/4PointedStarSoft");
        public static Texture2D SoftCircle => _softCircle ??= LoadTex("Assets/VFX Asset Library/MasksAndShapes/SoftCircle");

        // Moonlight Sonata theme-specific — glow and bloom
        public static Texture2D MSStarFlare => _msStarFlare ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Star Flare");
        public static Texture2D MSLensFlare => _msLensFlare ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Lens Flare");
        public static Texture2D MSGlowOrb => _msGlowOrb ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Glow and Bloom/MS Glow Orb");

        // Moonlight Sonata theme-specific — particles
        public static Texture2D MSCrescentMoon => _msCrescentMoon ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Crescent Moon");
        public static Texture2D MSMusicNote => _msMusicNote ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Music Note");
        public static Texture2D MSTidalMistWisp => _msTidalMistWisp ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Particles/MS Tidal Mist Wisp");

        // Moonlight Sonata theme-specific — impacts
        public static Texture2D MSHarmonicImpact => _msHarmonicImpact ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Impacts/MS Harmonic Resonance Wave Impact");
        public static Texture2D MSPowerEffectRing => _msPowerEffectRing ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Impacts/MS Power Effect Ring");

        // Moonlight Sonata theme-specific — beam textures
        public static Texture2D MSEnergyMotionBeam => _msEnergyMotionBeam ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Beam Textures/MS Energy Motion Beam");
        public static Texture2D MSEnergySurgeBeam => _msEnergySurgeBeam ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Beam Textures/MS Energy Surge Beam");

        // Moonlight Sonata theme-specific — trails and ribbons
        public static Texture2D MSHarmonicStandingWave => _msHarmonicStandingWave ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Trails and Ribbons/MS Harmonic Standing Wave Ribbon");
        public static Texture2D MSBasicTrail => _msBasicTrail ??= LoadTex("Assets/VFX Asset Library/Theme Specific/Moonlight Sonata/Trails and Ribbons/MS Basic Trail");

        // Moonlight Sonata color gradient LUT
        public static Texture2D MSGradientLUT => _msGradientLUT ??= LoadTex("Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

        private static readonly string[] NoteNames = new[]
        {
            "Assets/Particles Asset Library/CursiveMusicNote",
            "Assets/Particles Asset Library/MusicNote",
            "Assets/Particles Asset Library/QuarterNote",
            "Assets/Particles Asset Library/TallMusicNote",
            "Assets/Particles Asset Library/WholeNote"
        };

        public static Texture2D GetNoteTexture(int variant)
        {
            _noteTextures ??= new Texture2D[NoteNames.Length];
            variant = Math.Clamp(variant, 0, NoteNames.Length - 1);
            return _noteTextures[variant] ??= LoadTex(NoteNames[variant]);
        }

        private static Texture2D LoadTex(string path)
        {
            if (ModContent.HasAsset(path))
                return ModContent.Request<Texture2D>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            return null;
        }
    }

    // =================================================================
    // THEME-SPECIFIC SERENADE PARTICLES — Moonlight Sonata VFX Library
    // =================================================================

    /// <summary>
    /// Harmonic standing wave ribbon particle using the MS Harmonic Standing Wave Ribbon texture.
    /// Undulates along beam segments to convey harmonic resonance visually.
    /// </summary>
    public class HarmonicWaveRibbonParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _waveFreq;

        public HarmonicWaveRibbonParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = vel.ToRotation();
            _waveFreq = 0.08f + Main.rand.NextFloat(0.04f);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            float wave = (float)Math.Sin(Time * _waveFreq) * 1.5f;
            Vector2 perp = Velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            Position += perp * wave * 0.3f;
            Rotation = Velocity.ToRotation();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var tex = SerenadeTextures.MSHarmonicStandingWave;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            float opacity = SerenadeUtils.SineBump(LifetimeCompletion) * 0.6f;
            Color color = DrawColor;
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color * opacity,
                Rotation, origin, Scale * 0.2f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Serenade star flare particle using the MS Star Flare texture.
    /// A sharp 4-pointed flare that flashes at beam head, bounce points, and resonance events.
    /// </summary>
    public class SerenadeStarFlareParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public SerenadeStarFlareParticle(Vector2 pos, float scale, Color color, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Rotation += 0.015f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var tex = SerenadeTextures.MSStarFlare;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.8f);
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Time * 0.15f);
            Color color = Color.Lerp(DrawColor, SerenadeUtils.MoonWhite, pulse * 0.3f);
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color * opacity * pulse,
                Rotation, origin, Scale * 0.3f * pulse, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Serenade glow orb particle using the MS Glow Orb texture.
    /// Soft ethereal bloom layer for beam head and resonance level transitions.
    /// </summary>
    public class SerenadeGlowOrbParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public SerenadeGlowOrbParticle(Vector2 pos, float scale, Color color, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
        }

        public override void Update() { }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var tex = SerenadeTextures.MSGlowOrb;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Time * 0.12f);
            float opacity = (1f - LifetimeCompletion * LifetimeCompletion) * pulse * 0.5f;
            Color color = DrawColor;
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color * opacity,
                0f, origin, Scale * 0.35f * pulse, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Serenade resonance impact wave using the MS Harmonic Resonance Wave Impact texture.
    /// Expanding concentric wave at beam bounce points and resonance transitions.
    /// </summary>
    public class SerenadeResonanceWaveParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _maxScale;

        public SerenadeResonanceWaveParticle(Vector2 pos, Color color, float maxScale, int lifetime)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            DrawColor = color;
            _maxScale = maxScale;
            Lifetime = lifetime;
            Scale = 0f;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = _maxScale * SerenadeUtils.ExpoOut(t);
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var tex = SerenadeTextures.MSHarmonicImpact;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            float fade = (1f - LifetimeCompletion * LifetimeCompletion) * 0.6f;
            Color color = DrawColor;
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color * fade,
                0f, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Themed music note particle using the MS Music Note texture for Serenade.
    /// Floats with prismatic shimmer along beam path segments.
    /// </summary>
    public class SerenadeMoonlightNoteParticle : SerenadeParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private readonly float _wavePhase;
        private readonly Color _startColor;
        private readonly Color _endColor;

        public SerenadeMoonlightNoteParticle(Vector2 pos, Vector2 vel, Color startColor, Color endColor, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            _startColor = startColor;
            _endColor = endColor;
            Scale = scale;
            Lifetime = lifetime;
            _wavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
        }

        public override void Update()
        {
            Velocity *= 0.95f;
            Velocity.Y -= 0.04f;
            Position.X += (float)Math.Sin(_wavePhase + Time * 0.05f) * 0.5f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var tex = SerenadeTextures.MSMusicNote;
            if (tex == null) return;
            var origin = tex.Size() * 0.5f;

            float t = LifetimeCompletion;
            float opacity = 1f - t * t;
            Color color = Color.Lerp(_startColor, _endColor, t);
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null, color * opacity,
                Rotation, origin, Scale * 0.25f, SpriteEffects.None, 0f);
        }
    }
}
