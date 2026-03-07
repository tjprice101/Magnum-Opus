using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Particles
{
    // =========================================================================
    //  QUESTION MARK PARTICLE 窶・Floating "?" shaped glyph particles
    //  Appear at bullet split points and impact sites. AlphaBlend.
    //  Slowly rotate and fade with pulsing scale.
    // =========================================================================
    public class QuestionMarkParticle : SilentParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private readonly float _baseScale;
        private readonly float _pulseSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftRadialBloom halo has black bg
        public override bool UseCustomDraw => true;

        public QuestionMarkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _pulseSpeed = Main.rand.NextFloat(0.06f, 0.12f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Position += Velocity;
            Rotation += 0.015f;

            // Pulsing scale
            float pulse = MathF.Sin(Time * _pulseSpeed) * 0.15f + 1f;
            float fadeScale = 1f - LifetimeCompletion * 0.5f;
            Scale = _baseScale * pulse * fadeScale;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha = MathF.Pow(alpha, 0.6f);
            if (alpha <= 0f) return;

            // Use a glyph texture for the "?" shape
            int glyphIndex = (int)(Position.X + Position.Y) % NoteTextureNames.Length;
            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[Math.Abs(glyphIndex)]}";
            var tex = ModContent.Request<Texture2D>(glyphPath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;
            Color drawColor = Color * alpha;

            // Soft glow behind the glyph
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(glowTex, drawPos, null, drawColor * 0.25f, 0f, glowTex.Size() / 2f, MathHelper.Min(Scale * 2.5f, 0.139f), SpriteEffects.None, 0f);

            // The glyph itself
            sb.Draw(tex, drawPos, null, drawColor * 0.9f, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  SEEKER TRAIL DOT 窶・Small dot particles left behind by homing seekers
    //  Forms dotted trail lines. Additive, very quick fade.
    // =========================================================================
    public class SeekerTrailDot : SilentParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public SeekerTrailDot(Vector2 position, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            // Stationary dot, just fades
            Scale *= 0.97f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha *= alpha; // Quick quadratic fade
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Outer soft glow
            sb.Draw(tex, drawPos, null, Color * alpha * 0.3f, 0f, tex.Size() / 2f, MathHelper.Min(Scale * 1.8f, 0.139f), SpriteEffects.None, 0f);
            // Layer 2: Bright core
            sb.Draw(tex, drawPos, null, Color * alpha * 0.8f, 0f, tex.Size() / 2f, MathHelper.Min(Scale * 0.6f, 0.139f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  CHAIN LIGHTNING PARTICLE 窶・Bright flash particles along lightning arcs
    //  Additive, 3-layer bloom, velocity-stretched in direction of lightning.
    // =========================================================================
    public class ChainLightningParticle : SilentParticle
    {
        private readonly float _stretchDirection;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public ChainLightningParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _stretchDirection = velocity.ToRotation();
            Rotation = _stretchDirection;
        }

        public override void Update()
        {
            Velocity *= 0.9f;
            Position += Velocity;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha = MathF.Pow(alpha, 0.7f);
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-stretched scale for directional feel
            float stretch = MathHelper.Clamp(Velocity.Length() * 0.15f, 1f, 3.5f);
            Vector2 wideScale = new(Scale * stretch * 2.5f, Scale * 1.2f);
            float chainCap = wideScale.X > 0.139f ? 0.139f / wideScale.X : 1f;
            wideScale *= chainCap;
            Vector2 midScale = new Vector2(Scale * stretch * 1.5f, Scale * 0.8f) * chainCap;
            Vector2 sharpScale = new Vector2(Scale * stretch * 0.7f, Scale * 0.35f) * chainCap;

            // Layer 1: Wide outer bloom (faint)
            sb.Draw(tex, drawPos, null, Color * alpha * 0.2f, Rotation, tex.Size() / 2f, wideScale, SpriteEffects.None, 0f);
            // Layer 2: Medium middle bloom
            sb.Draw(tex, drawPos, null, Color * alpha * 0.5f, Rotation, tex.Size() / 2f, midScale, SpriteEffects.None, 0f);
            // Layer 3: Sharp center (near-white)
            sb.Draw(tex, drawPos, null, SilentUtils.AnswerWhite * alpha * 0.7f, Rotation, tex.Size() / 2f, sharpScale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  MEASURE IMPACT RING 窶・Expanding ring at the "?" explosion impact
    //  Additive, renders a circle from triangle fan with fade-out.
    // =========================================================================
    public class MeasureImpactRing : SilentParticle
    {
        private readonly float _expandSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public MeasureImpactRing(Vector2 position, Color color, float initialScale, int lifetime, float expandSpeed = 2.5f)
        {
            Position = position;
            Color = color;
            Scale = initialScale;
            Lifetime = lifetime;
            _expandSpeed = expandSpeed;
        }

        public override void Update()
        {
            Scale += _expandSpeed;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha *= alpha;
            if (alpha <= 0.01f) return;

            // Render a ring as a series of line-like quads forming a circle (triangle fan approximation)
            int segments = 32;
            float outerRadius = Scale;
            float innerRadius = outerRadius * 0.85f;
            Color ringColor = Color * alpha;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 dir = angle.ToRotationVector2();
                Vector2 midPoint = drawPos + dir * (outerRadius + innerRadius) * 0.5f;
                float thickness = (outerRadius - innerRadius) * 0.5f;

                sb.Draw(tex, midPoint, null, ringColor * 0.5f, angle, tex.Size() / 2f,
                    new Vector2(thickness / tex.Width * 2f, thickness / tex.Height * 2f), SpriteEffects.None, 0f);
            }

            // Central glow fade (capped to 300px on 2160px texture)
            sb.Draw(tex, drawPos, null, ringColor * 0.15f, 0f, tex.Size() / 2f, MathHelper.Min(outerRadius / tex.Width * 2f, 0.139f), SpriteEffects.None, 0f);
        }
    }
}