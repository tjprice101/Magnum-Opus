using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Particles
{
    // =========================================================================
    //  PHANTOM WISP  -- Ghostly wisps that drift around the phantom minion,
    //  like ethereal breath. Additive, 3-layer glow, velocity-squished, orbiting.
    // =========================================================================
    public class PhantomWispParticle : WatchingParticle
    {
        private readonly float _baseScale;
        private readonly Vector2 _orbitCenter;
        private float _orbitAngle;
        private readonly float _orbitRadius;
        private readonly float _orbitSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public PhantomWispParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, Color color, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.015f, 0.04f) * (Main.rand.NextBool() ? 1 : -1);
            Color = color;
            Scale = scale;
            _baseScale = scale;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Position = orbitCenter + _orbitAngle.ToRotationVector2() * orbitRadius;
            Velocity = Main.rand.NextVector2Circular(0.3f, 0.3f);
        }

        public override void Update()
        {
            _orbitAngle += _orbitSpeed;
            Vector2 targetPos = _orbitCenter + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Position = Vector2.Lerp(Position, targetPos, 0.08f);
            Position += Velocity;
            Velocity *= 0.95f;
            Rotation += 0.01f;
            Scale = _baseScale * (1f - LifetimeCompletion * 0.5f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = WatchingUtils.SineBump(LifetimeCompletion) * 0.8f;
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-squish: stretch along velocity direction
            float speed = Velocity.Length();
            float squish = 1f + MathHelper.Clamp(speed * 0.3f, 0f, 0.6f);
            float rot = speed > 0.01f ? Velocity.ToRotation() : Rotation;

            // Cap bloom so no dimension exceeds 300px on 2160px texture
            float outerDim = Scale * 2.5f * squish;
            float bloomCap = outerDim > 0.139f ? 0.139f / outerDim : 1f;

            // Layer 1: Wide outer halo
            sb.Draw(tex, drawPos, null, Color * alpha * 0.2f, rot, tex.Size() / 2f,
                new Vector2(Scale * 2.5f * squish, Scale * 2.5f / squish) * bloomCap, SpriteEffects.None, 0f);
            // Layer 2: Colored mid glow
            sb.Draw(tex, drawPos, null, Color * alpha * 0.5f, rot, tex.Size() / 2f,
                new Vector2(Scale * 1.2f * squish, Scale * 1.2f / squish) * bloomCap, SpriteEffects.None, 0f);
            // Layer 3: White-hot core
            sb.Draw(tex, drawPos, null, WatchingUtils.PhantomWhite * alpha * 0.3f, rot, tex.Size() / 2f,
                new Vector2(Scale * 0.4f * squish, Scale * 0.4f / squish) * bloomCap, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  WATCHING EYE  -- Floating eye-like glyph particles that blink/pulse
    //  when the minion shifts phase. AlphaBlend, uses Glyph textures,
    //  scale pulses in/out like an eye blinking.
    // =========================================================================
    public class WatchingEyeParticle : WatchingParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private readonly float _blinkSpeed;
        private readonly int _glyphIndex;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftRadialBloom halo has black bg
        public override bool UseCustomDraw => true;

        public WatchingEyeParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _blinkSpeed = Main.rand.NextFloat(0.06f, 0.12f);
            _glyphIndex = Main.rand.Next(NoteTextureNames.Length);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.97f;
            Rotation += 0.015f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            // Blink effect: scale Y oscillates like an eye opening/closing
            float blinkPhase = MathF.Sin(Time * _blinkSpeed);
            float scaleY = MathHelper.Clamp(0.3f + blinkPhase * 0.7f, 0.05f, 1f);
            float alpha = WatchingUtils.SineBump(LifetimeCompletion);
            if (alpha <= 0f) return;

            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[_glyphIndex]}";
            var tex = ModContent.Request<Texture2D>(glyphPath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Soft glow behind the eye glyph
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(glowTex, drawPos, null, WatchingUtils.GazeGreen * alpha * 0.25f, 0f,
                glowTex.Size() / 2f, MathHelper.Min(Scale * 3f, 0.139f), SpriteEffects.None, 0f);

            // Glyph with Y-squish blink
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f,
                new Vector2(Scale, Scale * scaleY), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  MYSTERY ZONE RIPPLE  -- Expanding ripple rings at mystery zone boundaries,
    //  like gravitational waves. Additive, custom draw rendering concentric rings
    //  from triangle-fan, fading outward.
    // =========================================================================
    public class MysteryZoneRipple : WatchingParticle
    {
        private readonly float _maxRadius;
        private readonly int _ringCount;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public MysteryZoneRipple(Vector2 position, Color color, float maxRadius, int lifetime, int ringCount = 3)
        {
            Position = position;
            Color = color;
            Scale = 0f;
            _maxRadius = maxRadius;
            Lifetime = lifetime;
            _ringCount = ringCount;
        }

        public override void Update()
        {
            Scale = WatchingUtils.PolyOut(LifetimeCompletion) * _maxRadius;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha *= alpha; // Quadratic fade
            if (alpha <= 0f) return;

            Vector2 drawPos = Position - Main.screenPosition;
            int segments = 32;

            for (int ring = 0; ring < _ringCount; ring++)
            {
                float ringRatio = (ring + 1f) / _ringCount;
                float radius = Scale * ringRatio;
                float ringAlpha = alpha * (1f - ringRatio * 0.5f) * 0.4f;
                Color ringColor = Color.Lerp(Color, WatchingUtils.SpectralMint, ringRatio * 0.5f) * ringAlpha;

                // Draw ring as connected line segments with bloom texture
                var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
                for (int s = 0; s < segments; s++)
                {
                    float angle = MathHelper.TwoPi * s / segments;
                    Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                    sb.Draw(tex, drawPos + offset, null, ringColor, 0f, tex.Size() / 2f,
                        MathHelper.Min(0.15f + ring * 0.05f, 0.139f), SpriteEffects.None, 0f);
                }
            }
        }
    }

    // =========================================================================
    //  PHANTOM BOLT TRAIL MOTE  -- Small streaking motes behind phantom bolt
    //  projectiles. Additive, simple 2-layer with SparkleFlare1, short lifetime,
    //  velocity-stretched.
    // =========================================================================
    public class PhantomBoltTrailMote : WatchingParticle
    {
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public PhantomBoltTrailMote(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.92f;
            Rotation = Velocity.ToRotation();
            Scale *= 0.97f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = (1f - LifetimeCompletion);
            alpha *= alpha; // Quadratic fade
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-stretch for motion blur
            float speed = Velocity.Length();
            float stretch = MathHelper.Clamp(speed * 0.15f, 1f, 3.5f);
            Vector2 drawScale = new(Scale * stretch, Scale * 0.5f);

            // Layer 1: Colored streak
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, drawScale, SpriteEffects.None, 0f);
            // Layer 2: White-hot core
            sb.Draw(tex, drawPos, null, WatchingUtils.PhantomWhite * alpha * 0.4f, Rotation, tex.Size() / 2f,
                drawScale * 0.35f, SpriteEffects.None, 0f);
        }
    }
}