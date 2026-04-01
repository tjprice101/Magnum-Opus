using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Particles
{
    // =========================================================================
    //  RIDDLE ECHO  -- Soft glow motes that spiral outward from the orb
    //  Like whispered secrets escaping from a sealed vault
    // =========================================================================
    public class RiddleEchoParticle : DissonanceParticle
    {
        private readonly float _baseScale;
        private float _spiralAngle;
        private readonly float _spiralSpeed;
        private readonly float _driftSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public RiddleEchoParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _spiralAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            _spiralSpeed = Main.rand.NextFloat(0.04f, 0.09f) * (Main.rand.NextBool() ? 1 : -1);
            _driftSpeed = Main.rand.NextFloat(0.3f, 0.8f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            _spiralAngle += _spiralSpeed;
            Velocity *= 0.96f;
            // Spiral outward drift
            Vector2 spiralOffset = _spiralAngle.ToRotationVector2() * _driftSpeed;
            Position += Velocity + spiralOffset;
            Rotation += 0.015f;
            Scale = _baseScale * (1f - LifetimeCompletion * 0.5f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha = MathF.Pow(alpha, 0.4f); // Slow fade, linger like whispers
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;
            Color drawColor = Color * alpha;

            // Layer 1: Outer halo bloom 竜・ the secret's aura (capped to 300px on 2160px texture)
            sb.Draw(tex, drawPos, null, drawColor * 0.25f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 2.5f, 0.139f), SpriteEffects.None, 0f);
            // Layer 2: Colored core 竜・ the riddle itself
            sb.Draw(tex, drawPos, null, drawColor * 0.65f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale, 0.139f), SpriteEffects.None, 0f);
            // Layer 3: White-hot center 竜・ a flicker of truth
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.35f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 0.35f, 0.139f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  SECRET GLYPH  -- Orbiting arcane glyphs around the cascade orb
    //  Slowly rotating with pulsing visibility, like forbidden runes
    // =========================================================================
    public class SecretGlyphParticle : DissonanceParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private readonly Vector2 _orbitCenter;
        private float _orbitAngle;
        private readonly float _orbitRadius;
        private readonly float _orbitSpeed;
        private readonly int _glyphVariant;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftRadialBloom halo has black bg
        public override bool UseCustomDraw => true;

        public SecretGlyphParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, Color color, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.015f, 0.04f) * (Main.rand.NextBool() ? 1 : -1);
            _glyphVariant = Main.rand.Next(NoteTextureNames.Length);
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * orbitRadius;
        }

        public override void Update()
        {
            _orbitAngle += _orbitSpeed;
            Position = _orbitCenter + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Rotation += 0.025f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            // Pulsing visibility  -- glyphs flicker in and out like half-remembered secrets
            float pulse = DissonanceUtils.SineBump(LifetimeCompletion);
            float flicker = MathF.Sin(Time * 0.12f) * 0.2f + 0.8f;
            float alpha = pulse * flicker;
            if (alpha <= 0.01f) return;

            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[_glyphVariant]}";
            var tex = ModContent.Request<Texture2D>(glyphPath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Soft glow halo behind the glyph
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(glowTex, drawPos, null, Color * alpha * 0.25f, 0f, glowTex.Size() / 2f, MathHelper.Min(Scale * 3.5f, 0.139f), SpriteEffects.None, 0f);
            // The glyph itself
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  CASCADE SPARK  -- Fast directional sparks for the cascade explosion
    //  Reality splitting with sharp bright sparks, motion-blur stretched
    // =========================================================================
    public class CascadeSparkParticle : DissonanceParticle
    {
        private readonly float _gravity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public CascadeSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _gravity = Main.rand.NextFloat(0.01f, 0.04f);
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Velocity.Y += _gravity;
            Position += Velocity;
            Rotation = Velocity.ToRotation();
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = (1f - LifetimeCompletion);
            alpha *= alpha; // Quadratic falloff
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Stretch by velocity for motion-blur feel
            float stretch = MathHelper.Clamp(Velocity.Length() * 0.12f, 1f, 3.5f);
            Vector2 scale = new(Scale * stretch, Scale * 0.45f);

            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.4f, Rotation, tex.Size() / 2f, scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  RIDDLEBOLT TRAIL MOTE  -- Tiny trail motes left behind by riddlebolts
    //  Fading quickly like the echo of a whispered answer
    // =========================================================================
    public class RiddleboltTrailMote : DissonanceParticle
    {
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public RiddleboltTrailMote(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            Position += Velocity;
            Scale = _baseScale * (1f - LifetimeCompletion);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Soft glow
            sb.Draw(tex, drawPos, null, Color * alpha * 0.5f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 1.5f, 0.139f), SpriteEffects.None, 0f);
            // Layer 2: Bright core
            sb.Draw(tex, drawPos, null, Color * alpha * 0.9f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 0.5f, 0.139f), SpriteEffects.None, 0f);
        }
    }
}