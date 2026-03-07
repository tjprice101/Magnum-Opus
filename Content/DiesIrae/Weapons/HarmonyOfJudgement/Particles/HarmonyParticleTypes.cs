using MagnumOpus.Common.Systems.VFX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles
{
    /// <summary>
    /// Lazy texture cache for Harmony of Judgement VFX.
    /// </summary>
    public static class SigilTextures
    {
        private const string VFX = "MagnumOpus/Assets/VFX Asset Library";
        private const string DI = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Dies Irae";

        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _diPowerEffectRing;
        private static Asset<Texture2D> _diHarmonicWave;
        private static Asset<Texture2D> _diStarFlare;
        private static Asset<Texture2D> _diRadialSlash;

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

        public static Texture2D SoftGlow => MagnumTextureRegistry.GetSoftGlow();
        public static Texture2D SoftRadialBloom => MagnumTextureRegistry.GetRadialBloom();
        public static Texture2D PointBloom => MagnumTextureRegistry.GetPointBloom();
        public static Texture2D SoftCircle => MagnumTextureRegistry.GetHaloRing();
        public static Texture2D GlowOrb => Get(ref _glowOrb, $"{VFX}/GlowAndBloom/GlowOrb");
        public static Texture2D StarFlare => Get(ref _starFlare, $"{VFX}/GlowAndBloom/StarFlare");
        public static Texture2D BeamStreak => MagnumTextureRegistry.GetBeamStreak();
        public static Texture2D ThinGlow => MagnumTextureRegistry.GetThinGlow();

        // Dies Irae theme-specific
        public static Texture2D DIPowerEffectRing => Get(ref _diPowerEffectRing, $"{DI}/Impact Effects/DI Power Effect Ring");
        public static Texture2D DIHarmonicWave => Get(ref _diHarmonicWave, $"{DI}/Impact Effects/DI Harmonic Resonance Wave Impact");
        public static Texture2D DIStarFlare => Get(ref _diStarFlare, $"{DI}/Projectiles/DI Star Flare");
        public static Texture2D DIRadialSlash => Get(ref _diRadialSlash, $"{DI}/Impact Effects/DI Radial Slash Star Impact");

        // Noise
        public static Texture2D VoronoiNoise => MagnumTextureRegistry.GetVoronoiNoise();
        public static Texture2D PerlinNoise => MagnumTextureRegistry.GetPerlinNoise();
    }

    // ═══════════════════════════════════════════════════════
    //  CONCRETE PARTICLE TYPES
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Judgment sigil body glow — rotating glow around the sigil entity.
    /// </summary>
    public class SigilGlowParticle : SigilParticle
    {
        private readonly float _rotSpeed;

        public SigilGlowParticle(Vector2 pos, Color color, float scale, int lifetime)
        {
            Position = pos;
            DrawColor = color;
            Scale = scale;
            Lifetime = lifetime;
            _rotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = SigilTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;
            float alpha = MathF.Sin(Progress * MathF.PI) * 0.5f;
            sb.Draw(tex, pos, null, DrawColor * alpha, Rotation + Time * _rotSpeed, origin, MathHelper.Min(Scale, 0.586f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Execute burst ring — expanding judgment ring at execute impact.
    /// </summary>
    public class ExecuteBurstParticle : SigilParticle
    {
        private readonly float _startScale;
        private readonly float _endScale;

        public ExecuteBurstParticle(Vector2 pos, Color color, float startScale, float endScale, int lifetime)
        {
            Position = pos;
            DrawColor = color;
            _startScale = startScale;
            _endScale = endScale;
            Scale = startScale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = SigilTextures.DIPowerEffectRing ?? SigilTextures.SoftCircle;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float t = Progress;
            float scale = MathHelper.Min(MathHelper.Lerp(_startScale, _endScale, 1f - MathF.Pow(1f - t, 3f)), 0.293f);
            float alpha = t < 0.15f ? t / 0.15f : MathF.Pow(1f - (t - 0.15f) / 0.85f, 2f);

            sb.Draw(tex, pos, null, DrawColor * alpha * 0.6f, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Beam spark — small spark along the judgment beam path.
    /// </summary>
    public class JudgmentBeamSparkParticle : SigilParticle
    {
        public JudgmentBeamSparkParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            Velocity *= 0.93f;
            Scale *= 0.96f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = SigilTextures.PointBloom ?? SigilTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;
            float alpha = (1f - Progress) * 0.7f;
            sb.Draw(tex, pos, null, DrawColor * alpha, 0f, origin, MathHelper.Min(Scale, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, DiesIraePalette.WrathWhite * alpha * 0.3f, 0f, origin, MathHelper.Min(Scale * 0.4f, 0.139f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Harmonized Verdict X-slash particle — dramatic gold X on enhanced executions.
    /// </summary>
    public class HarmonizedXParticle : SigilParticle
    {
        public HarmonizedXParticle(Vector2 pos, float scale, int lifetime)
        {
            Position = pos;
            DrawColor = DiesIraePalette.JudgmentGold;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = SigilTextures.DIRadialSlash ?? SigilTextures.StarFlare;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float t = Progress;
            float alpha = t < 0.1f ? t / 0.1f : MathF.Pow(1f - (t - 0.1f) / 0.9f, 1.5f);
            float scale = Scale * (0.5f + 0.5f * MathF.Sin(t * MathF.PI * 0.5f));

            // Gold X
            sb.Draw(tex, pos, null, DiesIraePalette.JudgmentGold * alpha * 0.7f, MathHelper.PiOver4, origin, MathHelper.Min(scale, 0.293f), SpriteEffects.None, 0f);
            // White core X
            sb.Draw(tex, pos, null, DiesIraePalette.WrathWhite * alpha * 0.4f, MathHelper.PiOver4, origin, MathHelper.Min(scale * 0.6f, 0.293f), SpriteEffects.None, 0f);
            // Crimson outer X
            sb.Draw(tex, pos, null, DiesIraePalette.BloodRed * alpha * 0.3f, MathHelper.PiOver4, origin, MathHelper.Min(scale * 1.3f, 0.293f), SpriteEffects.None, 0f);
        }
    }
}
