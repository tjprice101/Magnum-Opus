using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Particles
{
    // =========================================================================
    //  UNRAVEL MOTE — Soft glow particles that drift along the beam
    //  Like reality fraying at the seams, small void motes orbit and drift
    // =========================================================================
    public class UnravelMoteParticle : CipherParticle
    {
        private readonly float _hueShift;
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public UnravelMoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _hueShift = Main.rand.NextFloat(-0.1f, 0.1f);
            _baseScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Position += Velocity;
            Rotation += 0.02f;
            Scale = _baseScale * (1f - LifetimeCompletion * 0.6f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha = MathF.Pow(alpha, 0.5f); // Slow fade
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            Vector2 drawPos = Position - Main.screenPosition;
            Color drawColor = Color * alpha;

            // Layer 1: Outer halo bloom
            sb.Draw(tex, drawPos, null, drawColor * 0.3f, Rotation, tex.Size() / 2f, Scale * 2f, SpriteEffects.None, 0f);
            // Layer 2: Colored core
            sb.Draw(tex, drawPos, null, drawColor * 0.7f, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            // Layer 3: White-hot center
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.4f, Rotation, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  CIPHER GLYPH — Floating enigma glyphs that orbit the beam
    //  Each glyph is a slowly rotating arcane symbol
    // =========================================================================
    public class CipherGlyphParticle : CipherParticle
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

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public CipherGlyphParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, Color color, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.02f, 0.05f) * (Main.rand.NextBool() ? 1 : -1);
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * orbitRadius;
        }

        public override void Update()
        {
            _orbitAngle += _orbitSpeed;
            Position = _orbitCenter + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Rotation += 0.03f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = CipherUtils.SineBump(LifetimeCompletion);
            if (alpha <= 0f) return;

            int glyphIndex = (int)(Position.X * 3 + Position.Y) % NoteTextureNames.Length;
            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[Math.Abs(glyphIndex)]}";
            var tex = ModContent.Request<Texture2D>(glyphPath).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Glyph with soft glow behind
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            sb.Draw(glowTex, drawPos, null, Color * alpha * 0.3f, 0f, glowTex.Size() / 2f, Scale * 3f, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  SNAP-BACK SPARK — Fast directional sparks for the snap-back explosion
    //  Reality collapsing inward with sharp bright sparks
    // =========================================================================
    public class SnapBackSparkParticle : CipherParticle
    {
        private readonly float _gravity;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public SnapBackSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _gravity = Main.rand.NextFloat(0.02f, 0.06f);
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.95f;
            Velocity.Y += _gravity;
            Position += Velocity;
            Rotation = Velocity.ToRotation();
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = (1f - LifetimeCompletion);
            alpha *= alpha;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow").Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Stretched by velocity for motion-blur feel
            float stretch = MathHelper.Clamp(Velocity.Length() * 0.1f, 1f, 3f);
            Vector2 scale = new(Scale * stretch, Scale * 0.5f);

            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.5f, Rotation, tex.Size() / 2f, scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  VOID DISTORTION RING — Expanding ring that fades, for beam impacts
    // =========================================================================
    public class VoidDistortionRingParticle : CipherParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public VoidDistortionRingParticle(Vector2 position, Color color, float scale, int lifetime)
        {
            Position = position;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.08f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle").Value;
            Vector2 drawPos = Position - Main.screenPosition;

            sb.Draw(tex, drawPos, null, Color * alpha * 0.6f, 0f, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.2f, 0f, tex.Size() / 2f, Scale * 0.7f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  BEAM CORE PULSE — Pulsing glow at the beam origin
    // =========================================================================
    public class BeamCorePulseParticle : CipherParticle
    {
        private readonly float _pulseSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public BeamCorePulseParticle(Vector2 position, Color color, float scale, int lifetime)
        {
            Position = position;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _pulseSpeed = Main.rand.NextFloat(0.08f, 0.15f);
        }

        public override void Update()
        {
            float pulse = MathF.Sin(Time * _pulseSpeed) * 0.3f + 0.7f;
            Scale *= pulse > 0 ? 1f : 0.98f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = CipherUtils.SineBump(LifetimeCompletion);
            float pulse = MathF.Sin(Time * _pulseSpeed) * 0.3f + 0.7f;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;
            Vector2 drawPos = Position - Main.screenPosition;

            sb.Draw(tex, drawPos, null, Color * alpha * 0.4f, 0f, tex.Size() / 2f, Scale * 3f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, Color * alpha * 0.8f, 0f, tex.Size() / 2f, Scale * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, CipherUtils.WhiteRevelation * alpha * 0.5f, 0f, tex.Size() / 2f, Scale * 0.5f * pulse, SpriteEffects.None, 0f);
        }
    }
}
