using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Particles
{
    // =========================================================================
    //  VOICE WISP 窶・Ethereal wisps orbiting the player alongside voice projectiles.
    //  Faint phantom faces or sound waves, like spectral choir members drifting.
    //  Additive, 3-layer soft glow on velocity-squished Halo1 texture.
    // =========================================================================
    public class VoiceWispParticle : FugueParticle
    {
        private readonly float _baseScale;
        private float _driftAngle;
        private readonly float _driftSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public VoiceWispParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _driftAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            _driftSpeed = Main.rand.NextFloat(0.03f, 0.07f) * (Main.rand.NextBool() ? 1 : -1);
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            _driftAngle += _driftSpeed;
            Velocity *= 0.95f;
            Vector2 drift = _driftAngle.ToRotationVector2() * 0.4f;
            Position += Velocity + drift;
            Rotation = Velocity.ToRotation();
            Scale = _baseScale * (1f - LifetimeCompletion * 0.6f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha = MathF.Pow(alpha, 0.5f); // Linger like ghostly voices
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-squish: stretch along movement direction
            float speed = Velocity.Length();
            float stretch = MathHelper.Clamp(speed * 0.08f, 1f, 2.2f);
            Vector2 squishScale = new(Scale * stretch, Scale / MathHelper.Max(stretch * 0.6f, 1f));

            Color drawColor = Color * alpha;

            // Layer 1: Outer spectral halo 窶・the voice's echo
            sb.Draw(tex, drawPos, null, drawColor * 0.2f, Rotation, tex.Size() / 2f, squishScale * 2.8f, SpriteEffects.None, 0f);
            // Layer 2: Mid glow 窶・the voice itself
            sb.Draw(tex, drawPos, null, drawColor * 0.55f, Rotation, tex.Size() / 2f, squishScale * 1.2f, SpriteEffects.None, 0f);
            // Layer 3: White-hot core 窶・the note's attack
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.3f, Rotation, tex.Size() / 2f, squishScale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  ECHO MARK 窶・Pulsing glyph hovering above enemies with echo mark stacks.
    //  Orbits the enemy position, growing more intense with higher stack count.
    //  AlphaBlend, uses Glyph textures.
    // =========================================================================
    public class EchoMarkParticle : FugueParticle
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
        private readonly int _stackCount;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        public EchoMarkParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, Color color, float scale, int lifetime, int stackCount)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.02f, 0.05f) * (Main.rand.NextBool() ? 1 : -1);
            _glyphVariant = Main.rand.Next(NoteTextureNames.Length);
            _stackCount = Math.Clamp(stackCount, 1, 5);
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * orbitRadius;
        }

        public override void Update()
        {
            _orbitAngle += _orbitSpeed;
            Position = _orbitCenter + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Rotation += 0.02f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            // Intensity pulses faster with more stacks
            float pulseSpeed = 0.08f + _stackCount * 0.04f;
            float pulse = FugueUtils.SineBump(LifetimeCompletion);
            float flicker = MathF.Sin(Time * pulseSpeed) * 0.25f + 0.75f;
            float alpha = pulse * flicker;

            // Stack count intensifies brightness
            float stackIntensity = 0.5f + _stackCount * 0.1f;
            alpha *= stackIntensity;
            if (alpha <= 0.01f) return;

            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[_glyphVariant]}";
            var tex = ModContent.Request<Texture2D>(glyphPath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Soft teal glow halo behind the glyph 窶・echo resonance
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Color glowColor = FugueUtils.EchoTeal * alpha * 0.3f;
            sb.Draw(glowTex, drawPos, null, glowColor, 0f, glowTex.Size() / 2f, Scale * 3f, SpriteEffects.None, 0f);
            // The glyph mark itself
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  CONVERGENCE FLASH 窶・Bright burst when chain detonation triggers.
    //  Expanding ring with center flare, like all voices reaching unison.
    //  Additive, custom draw with radial bloom + halo ring.
    // =========================================================================
    public class ConvergenceFlashParticle : FugueParticle
    {
        private readonly float _maxRingRadius;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public ConvergenceFlashParticle(Vector2 position, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _maxRingRadius = scale * 4f;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Rotation += 0.01f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float progress = LifetimeCompletion;

            // Flash alpha: bright at start, fades out
            float alpha = 1f - FugueUtils.PolyIn(progress);
            if (alpha <= 0f) return;

            Vector2 drawPos = Position - Main.screenPosition;

            // Center flare 窶・the convergence point
            var flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            float flareScale = Scale * (0.5f + progress * 0.5f);
            sb.Draw(flareTex, drawPos, null, Color * alpha * 0.8f, Rotation, flareTex.Size() / 2f, flareScale, SpriteEffects.None, 0f);
            // White-hot core
            sb.Draw(flareTex, drawPos, null, Color.White * alpha * 0.6f, Rotation, flareTex.Size() / 2f, flareScale * 0.3f, SpriteEffects.None, 0f);

            // Expanding ring 窶・the resonance wave
            var ringTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;
            float ringRadius = FugueUtils.SineOut(progress) * _maxRingRadius;
            float ringAlpha = alpha * 0.7f * (1f - progress);
            float ringScale = ringRadius / (ringTex.Width * 0.5f);
            sb.Draw(ringTex, drawPos, null, FugueUtils.FugueCyan * ringAlpha, 0f, ringTex.Size() / 2f, ringScale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  FUGUE TRAIL MOTE 窶・Small trailing motes behind flying voice projectiles.
    //  Fading teal-to-purple like an echo decaying across registers.
    //  Additive, simple 2-layer draw with SparkleFlare1.
    // =========================================================================
    public class FugueTrailMote : FugueParticle
    {
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public FugueTrailMote(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.93f;
            Position += Velocity;
            Scale = _baseScale * (1f - LifetimeCompletion);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Color fades from teal to purple as the mote dies
            Color fadeColor = Color.Lerp(FugueUtils.EchoTeal, FugueUtils.VoicePurple, LifetimeCompletion);

            // Layer 1: Soft outer glow
            sb.Draw(tex, drawPos, null, fadeColor * alpha * 0.45f, Rotation, tex.Size() / 2f, Scale * 1.8f, SpriteEffects.None, 0f);
            // Layer 2: Bright core
            sb.Draw(tex, drawPos, null, fadeColor * alpha * 0.85f, Rotation, tex.Size() / 2f, Scale * 0.6f, SpriteEffects.None, 0f);
        }
    }
}