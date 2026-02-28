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
    // TEXTURE HELPER — lazy-loaded texture references for particles
    // =========================================================================
    internal static class SerenadeTextures
    {
        private static Texture2D _pointBloom;
        private static Texture2D _softRadialBloom;
        private static Texture2D _starSoft;
        private static Texture2D[] _noteTextures;

        public static Texture2D PointBloom => _pointBloom ??= LoadTex("Assets/VFX Asset Library/GlowAndBloom/PointBloom");
        public static Texture2D SoftRadialBloom => _softRadialBloom ??= LoadTex("Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
        public static Texture2D StarSoft => _starSoft ??= LoadTex("Assets/Particles Asset Library/Stars/4PointedStarSoft");

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
}
