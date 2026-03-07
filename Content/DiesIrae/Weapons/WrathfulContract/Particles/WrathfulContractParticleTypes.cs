using MagnumOpus.Common.Systems.VFX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Particles
{
    /// <summary>
    /// Lazy texture cache for Wrathful Contract VFX.
    /// </summary>
    public static class DemonTextures
    {
        private const string VFX = "MagnumOpus/Assets/VFX Asset Library";
        private const string DI = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Dies Irae";

        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _diStarFlare;
        private static Asset<Texture2D> _diStarFlare2;
        private static Asset<Texture2D> _diPowerEffectRing;
        private static Asset<Texture2D> _diAshFlake;

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
        public static Texture2D SmokePuff => MagnumTextureRegistry.GetCloudSmoke();
        public static Texture2D BeamStreak => MagnumTextureRegistry.GetBeamStreak();
        public static Texture2D ThinGlow => MagnumTextureRegistry.GetThinGlow();

        // Dies Irae theme-specific
        public static Texture2D DIStarFlare => Get(ref _diStarFlare, $"{DI}/Projectiles/DI Star Flare");
        public static Texture2D DIStarFlare2 => Get(ref _diStarFlare2, $"{DI}/Projectiles/DI Star Flare 2");
        public static Texture2D DIPowerEffectRing => Get(ref _diPowerEffectRing, $"{DI}/Impact Effects/DI Power Effect Ring");
        public static Texture2D DIAshFlake => Get(ref _diAshFlake, $"{DI}/Particles/DI Ash Flake");

        // Noise
        public static Texture2D FBMNoise => MagnumTextureRegistry.GetFBMNoise();
        public static Texture2D VoronoiNoise => MagnumTextureRegistry.GetVoronoiNoise();
        public static Texture2D PerlinNoise => MagnumTextureRegistry.GetPerlinNoise();
    }

    // ═══════════════════════════════════════════════════════
    //  CONCRETE PARTICLE TYPES
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Demon ambient smoke puff — billowing dark smoke around the demon body.
    /// </summary>
    public class DemonSmokePuffParticle : DemonParticle
    {
        private readonly float _rotSpeed;

        public override bool UseAdditiveBlend => false; // Smoke uses alpha blend

        public DemonSmokePuffParticle(Vector2 pos, Vector2 vel, float scale, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            DrawColor = DiesIraePalette.CharcoalBlack;
            Scale = scale;
            Lifetime = lifetime;
            _rotSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            base.Update();
            Velocity *= 0.96f;
            Velocity.Y -= 0.02f; // Slight upward drift
            Scale *= 1.005f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = DemonTextures.SmokePuff ?? DemonTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;
            float alpha = MathF.Sin(Progress * MathF.PI) * 0.5f;
            sb.Draw(tex, pos, null, DrawColor * alpha, Rotation + Time * _rotSpeed, origin, MathHelper.Min(Scale, 0.537f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Ember spark — hot particles shedding off the demon's form.
    /// </summary>
    public class DemonEmberParticle : DemonParticle
    {
        public DemonEmberParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            Velocity.Y -= 0.04f; // Rise
            Scale *= 0.97f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = DemonTextures.PointBloom ?? DemonTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;
            float alpha = (1f - Progress) * 0.8f;
            sb.Draw(tex, pos, null, DrawColor * alpha, 0f, origin, MathHelper.Min(Scale, 0.139f), SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, DiesIraePalette.WrathWhite * alpha * 0.3f, 0f, origin, MathHelper.Min(Scale * 0.4f, 0.139f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Blood drain orb — small crimson glow flowing along the blood tether.
    /// </summary>
    public class BloodDrainOrbParticle : DemonParticle
    {
        private readonly Vector2 _start;
        private readonly Vector2 _end;
        private readonly bool _reversed; // For Blood Sacrifice reversal

        public BloodDrainOrbParticle(Vector2 start, Vector2 end, bool reversed, int lifetime)
        {
            _start = start;
            _end = end;
            _reversed = reversed;
            DrawColor = DiesIraePalette.BloodRed;
            Scale = 0.015f;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Time++;
            if (Time >= Lifetime) Active = false;
            float t = _reversed ? (1f - Progress) : Progress;
            Position = Vector2.Lerp(_start, _end, t);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = DemonTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;
            Color color = _reversed ? DiesIraePalette.JudgmentGold : DiesIraePalette.BloodRed;
            float alpha = MathF.Sin(Progress * MathF.PI) * 0.7f;
            sb.Draw(tex, pos, null, color * alpha, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Frenzy kill burst spark — radial scatter on enemy kill during Frenzy state.
    /// </summary>
    public class FrenzyKillSparkParticle : DemonParticle
    {
        public FrenzyKillSparkParticle(Vector2 pos, Vector2 vel, Color color, float scale, int lifetime)
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
            Velocity *= 0.92f;
            Scale *= 0.95f;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = DemonTextures.StarFlare ?? DemonTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;
            float alpha = (1f - Progress);
            sb.Draw(tex, pos, null, DrawColor * alpha * 0.6f, Velocity.ToRotation(), origin, MathHelper.Min(Scale, 0.293f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Blood sacrifice flash — dramatic expanding ring during sacrifice cinematic.
    /// </summary>
    public class BloodSacrificeFlashParticle : DemonParticle
    {
        private readonly float _endScale;

        public BloodSacrificeFlashParticle(Vector2 pos, float endScale, int lifetime)
        {
            Position = pos;
            DrawColor = DiesIraePalette.JudgmentGold;
            Scale = 0.02f;
            _endScale = endScale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = DemonTextures.DIPowerEffectRing ?? DemonTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float t = Progress;
            float scale = MathHelper.Min(MathHelper.Lerp(Scale, _endScale, 1f - MathF.Pow(1f - t, 3f)), 0.293f);
            float alpha = t < 0.2f ? t / 0.2f : MathF.Pow(1f - (t - 0.2f) / 0.8f, 2f);

            sb.Draw(tex, pos, null, DiesIraePalette.JudgmentGold * alpha * 0.5f, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.Draw(tex, pos, null, DiesIraePalette.WrathWhite * alpha * 0.25f, 0f, origin, scale * 0.6f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Breach warning pulse — rapid crimson ring pulse when HP drops below threshold.
    /// </summary>
    public class BreachWarningPulseParticle : DemonParticle
    {
        public BreachWarningPulseParticle(Vector2 pos, float scale, int lifetime)
        {
            Position = pos;
            DrawColor = DiesIraePalette.InfernalRed;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = DemonTextures.DIPowerEffectRing ?? DemonTextures.SoftGlow;
            if (tex == null) return;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Position - Main.screenPosition;

            float t = Progress;
            float pulse = MathF.Sin(t * MathF.PI * 4f); // Fast pulsing
            float alpha = (1f - t) * MathF.Abs(pulse) * 0.6f;

            sb.Draw(tex, pos, null, DrawColor * alpha, 0f, origin, MathHelper.Min(Scale * (1f + t * 0.5f), 0.293f), SpriteEffects.None, 0f);
        }
    }
}
