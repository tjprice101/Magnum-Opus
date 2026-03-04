using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles
{
    /// <summary>
    /// Lazy texture cache for Death Tolling Bell VFX.
    /// Loads both generic and Dies Irae theme-specific textures.
    /// </summary>
    public static class BellTextures
    {
        private const string VFX = "MagnumOpus/Assets/VFX Asset Library";
        private const string DI = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Dies Irae";

        // Generic textures (from MagnumTextureRegistry or VFX library)
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _perlinNoise;
        private static Asset<Texture2D> _fbmNoise;
        private static Asset<Texture2D> _cosmicVortex;
        private static Asset<Texture2D> _starFieldScatter;

        // Dies Irae theme-specific
        private static Asset<Texture2D> _diPowerEffectRing;
        private static Asset<Texture2D> _diHarmonicWaveImpact;
        private static Asset<Texture2D> _diAshFlake;
        private static Asset<Texture2D> _diStarFlare;
        private static Asset<Texture2D> _diStarFlare2;
        private static Asset<Texture2D> _diGradientLUT;
        private static Asset<Texture2D> _diCrackedEarth;
        private static Asset<Texture2D> _diRockyHellTrail;
        private static Asset<Texture2D> _diEnergySurgeBeam;

        private static Asset<Texture2D> SafeLoad(string path)
        {
            try { return ModContent.Request<Texture2D>(path, AssetRequestMode.AsyncLoad); }
            catch { return null; }
        }

        private static Texture2D Get(ref Asset<Texture2D> asset, string path)
        {
            asset ??= SafeLoad(path);
            return asset?.IsLoaded == true ? asset.Value : null;
        }

        // ── Generic Glow/Bloom ──
        public static Texture2D SoftGlow => MagnumTextureRegistry.GetSoftGlow();
        public static Texture2D SoftRadialBloom => MagnumTextureRegistry.GetRadialBloom();
        public static Texture2D GlowOrb => Get(ref _glowOrb, $"{VFX}/GlowAndBloom/GlowOrb");
        public static Texture2D SoftCircle => MagnumTextureRegistry.GetHaloRing();
        public static Texture2D PointBloom => MagnumTextureRegistry.GetPointBloom();
        public static Texture2D StarFlare => Get(ref _starFlare, $"{VFX}/GlowAndBloom/StarFlare");

        // ── Generic Noise ──
        public static Texture2D PerlinNoise => MagnumTextureRegistry.GetPerlinNoise();
        public static Texture2D FBMNoise => MagnumTextureRegistry.GetFBMNoise();
        public static Texture2D CosmicVortex => MagnumTextureRegistry.GetCosmicVortex();
        public static Texture2D StarFieldScatter => Get(ref _starFieldScatter, $"{VFX}/NoiseTextures/StarFieldScatter");

        // ── Dies Irae Theme-Specific ──
        public static Texture2D DIPowerEffectRing => Get(ref _diPowerEffectRing, $"{DI}/Impact Effects/DI Power Effect Ring");
        public static Texture2D DIHarmonicWaveImpact => Get(ref _diHarmonicWaveImpact, $"{DI}/Impact Effects/DI Harmonic Resonance Wave Impact");
        public static Texture2D DIAshFlake => Get(ref _diAshFlake, $"{DI}/Particles/DI Ash Flake");
        public static Texture2D DIStarFlare => Get(ref _diStarFlare, $"{DI}/Projectiles/DI Star Flare");
        public static Texture2D DIStarFlare2 => Get(ref _diStarFlare2, $"{DI}/Projectiles/DI Star Flare 2");
        public static Texture2D DIGradientLUT => Get(ref _diGradientLUT, $"{VFX}/ColorGradients/DiesIraeGradientLUTandRAMP");
        public static Texture2D DICrackedEarth => Get(ref _diCrackedEarth, $"{DI}/Noise/DI Unique Theme Noise %E2%80%94 Cracked Earth Pattern");
        public static Texture2D DIRockyHellTrail => Get(ref _diRockyHellTrail, $"{DI}/Trails and Ribbons/DI Rocky Hell Trail");
        public static Texture2D DIEnergySurgeBeam => Get(ref _diEnergySurgeBeam, $"{DI}/Beam Textures/DI Energy Surge Beam");
    }

    // =================================================================
    //  CONCRETE PARTICLE TYPES
    // =================================================================

    /// <summary>
    /// Expanding toll ring glow — large additive ring that pulses outward.
    /// Used as a soft bloom layer behind the shader-driven toll rings.
    /// </summary>
    public class TollRingBloomParticle : BellParticle
    {
        private readonly float _startScale;
        private readonly float _endScale;

        public TollRingBloomParticle(Vector2 pos, Color color, float startScale, float endScale, int lifetime)
        {
            Position = pos;
            DrawColor = color;
            Scale = startScale;
            _startScale = startScale;
            _endScale = endScale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = BellTextures.DIPowerEffectRing ?? BellTextures.SoftCircle;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float t = Progress;
            float scale = MathHelper.Lerp(_startScale, _endScale, EaseOutCubic(t));
            float alpha = t < 0.2f ? t / 0.2f : (1f - (t - 0.2f) / 0.8f);
            alpha = MathHelper.Clamp(alpha, 0f, 1f);

            sb.Draw(tex, pos, null, DrawColor * alpha * 0.5f, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        private static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
    }

    /// <summary>
    /// Bell body glow pulse — pulsing bloom emanating from bell center.
    /// Scales and color shift based on bell state.
    /// </summary>
    public class BellGlowPulseParticle : BellParticle
    {
        public BellGlowPulseParticle(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = BellTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float alpha = MathF.Sin(Progress * MathF.PI) * 0.6f;
            sb.Draw(tex, pos, null, DrawColor * alpha, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Funeral march smoke — dark billowing smoke with crimson tinge.
    /// Uses alpha blend for solid smoke appearance.
    /// </summary>
    public class FuneralSmokeParticle : BellParticle
    {
        private readonly float _rotSpeed;

        public override bool UseAdditiveBlend => false;

        public FuneralSmokeParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _rotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.02f; // Gentle upward drift
            Scale += 0.003f;
            Rotation += _rotSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = MagnumTextureRegistry.GetCloudSmoke();
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float alpha = Progress < 0.3f ? Progress / 0.3f : (1f - (Progress - 0.3f) / 0.7f);
            alpha = MathHelper.Clamp(alpha, 0f, 1f) * 0.7f;

            sb.Draw(tex, pos, null, DrawColor * alpha, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Ash flake particle — Dies Irae theme-specific drifting ash.
    /// </summary>
    public class AshFlakeParticle : BellParticle
    {
        private readonly float _rotSpeed;

        public AshFlakeParticle(Vector2 pos, Vector2 vel, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = DiesIraePalette.AshGray;
            Scale = scale;
            Lifetime = lifetime;
            _rotSpeed = Main.rand.NextFloat(-0.05f, 0.05f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.97f;
            Velocity.Y -= 0.01f;
            Rotation += _rotSpeed;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = BellTextures.DIAshFlake ?? BellTextures.PointBloom;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float alpha = 1f - Progress;
            sb.Draw(tex, pos, null, DrawColor * alpha * 0.7f, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Toll wave ember spark — fast outward-moving fire spark with short lifetime.
    /// </summary>
    public class TollEmberParticle : BellParticle
    {
        public TollEmberParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.94f;
            Scale *= 0.97f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = BellTextures.PointBloom ?? BellTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float alpha = (1f - Progress) * 0.8f;
            sb.Draw(tex, pos, null, DrawColor * alpha, 0f, origin, Scale, SpriteEffects.None, 0f);
            // Hot core
            sb.Draw(tex, pos, null, DiesIraePalette.WrathWhite * alpha * 0.4f, 0f, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Death-Mark flash — dramatic gold-white burst when an enemy reaches 5 Tolled stacks.
    /// </summary>
    public class DeathMarkFlashParticle : BellParticle
    {
        public DeathMarkFlashParticle(Vector2 pos, float scale, int lifetime)
        {
            Position = pos;
            DrawColor = DiesIraePalette.JudgmentGold;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = BellTextures.DIStarFlare ?? BellTextures.StarFlare ?? BellTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float t = Progress;
            float alpha = t < 0.15f ? t / 0.15f : MathF.Pow(1f - (t - 0.15f) / 0.85f, 2f);
            float scale = Scale * (0.8f + 0.4f * MathF.Sin(t * MathF.PI));

            // Gold core
            sb.Draw(tex, pos, null, DiesIraePalette.JudgmentGold * alpha * 0.7f, 0f, origin, scale, SpriteEffects.None, 0f);
            // White hot center
            sb.Draw(tex, pos, null, DiesIraePalette.WrathWhite * alpha * 0.5f, 0f, origin, scale * 0.5f, SpriteEffects.None, 0f);
            // Crimson outer
            sb.Draw(tex, pos, null, DiesIraePalette.BloodRed * alpha * 0.3f, 0f, origin, scale * 1.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Standing wave tether node — pulsing node along the harmonic wave tether.
    /// </summary>
    public class TetherNodeParticle : BellParticle
    {
        public TetherNodeParticle(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = BellTextures.PointBloom ?? BellTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float alpha = MathF.Sin(Progress * MathF.PI) * 0.5f;
            sb.Draw(tex, pos, null, DrawColor * alpha, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
