using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Particles
{
    // =========================================================================
    //  VOID WHISPER MOTE  -- Subtle dark motes that drift during VoidWhisper
    //  phase, like whispers made visible. Additive, custom draw 2-layer
    //  (SoftCircleBloom outer + white center), low opacity, slow drift
    // =========================================================================
    public class VoidWhisperMote : VoidVariationParticle
    {
        private readonly float _baseScale;
        private readonly float _driftAngle;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public VoidWhisperMote(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _driftAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Slow sinusoidal drift  -- whispers wandering aimlessly through the void
            float driftX = MathF.Sin(Time * 0.04f + _driftAngle) * 0.3f;
            float driftY = MathF.Cos(Time * 0.03f + _driftAngle * 1.7f) * 0.2f;
            Velocity *= 0.97f;
            Position += Velocity + new Vector2(driftX, driftY);
            Rotation += 0.01f;
            Scale = _baseScale * (1f - LifetimeCompletion * 0.5f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            // Low opacity  -- these are whispers, barely perceptible
            float alpha = VoidVariationUtils.SineBump(LifetimeCompletion) * 0.35f;
            if (alpha <= 0.01f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Outer dark halo 竜・ the void's breath (capped to 300px on 2160px texture)
            sb.Draw(tex, drawPos, null, Color * alpha * 0.3f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 2.2f, 0.139f), SpriteEffects.None, 0f);
            // Layer 2: Brighter white center 竜・ a faint ghost of substance
            sb.Draw(tex, drawPos, null, VoidVariationUtils.SunderingWhite * alpha * 0.5f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 0.4f, 0.139f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  ABYSSAL ECHO RING  -- Expanding echo rings during AbyssalEcho phase,
    //  like sonar pulses from the void. Additive, custom draw rendering
    //  rings from triangle-fan geometry, expanding and fading
    // =========================================================================
    public class AbyssalEchoRing : VoidVariationParticle
    {
        private readonly float _baseScale;
        private readonly float _expandRate;
        private const int RingSegments = 32;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public AbyssalEchoRing(Vector2 position, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _expandRate = Main.rand.NextFloat(3.0f, 5.5f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Expand outward  -- sonar echo rippling into the abyss
            Scale = _baseScale + LifetimeCompletion * _expandRate;
            Rotation += 0.005f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float fade = VoidVariationUtils.SineBump(LifetimeCompletion);
            float alpha = fade * 0.5f;
            if (alpha <= 0.01f) return;

            Vector2 drawPos = Position - Main.screenPosition;

            // Draw ring as a series of line segments approximating a circle
            float radius = Scale * 20f;
            float innerRadius = radius * 0.85f;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;

            for (int i = 0; i < RingSegments; i++)
            {
                float angle = Rotation + MathHelper.TwoPi * i / RingSegments;
                Vector2 ringPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;

                // Each segment is a small bloom dot along the ring circumference
                float segmentAlpha = alpha * (0.7f + 0.3f * MathF.Sin(angle * 3f + Time * 0.1f));
                float segmentScale = MathHelper.Min(Scale * 0.12f, 0.139f);

                sb.Draw(tex, ringPos, null, Color * segmentAlpha, 0f, tex.Size() / 2f, segmentScale, SpriteEffects.None, 0f);
            }

            // Inner glow ring  -- slightly different color for depth
            for (int i = 0; i < RingSegments / 2; i++)
            {
                float angle = Rotation + MathHelper.TwoPi * i / (RingSegments / 2) + 0.1f;
                Vector2 ringPos = drawPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * innerRadius;

                float segmentScale = MathHelper.Min(Scale * 0.08f, 0.139f);
                sb.Draw(tex, ringPos, null, VoidVariationUtils.RiftTeal * alpha * 0.4f, 0f, tex.Size() / 2f, segmentScale, SpriteEffects.None, 0f);
            }
        }
    }

    // =========================================================================
    //  RIFT SUNDER SPARK  -- Sharp directional sparks during the RiftSunder
    //  finisher, cracking reality. Additive, custom draw with SparkleFlare1,
    //  velocity-stretched, bright green-white
    // =========================================================================
    public class RiftSunderSpark : VoidVariationParticle
    {
        private readonly float _baseScale;
        private readonly float _stretchMultiplier;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public RiftSunderSpark(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            // Bright green-white gradient  -- the rift cracking open
            float colorT = Main.rand.NextFloat();
            Color = Color.Lerp(VoidVariationUtils.VoidSurge, VoidVariationUtils.SunderingWhite, colorT);
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _stretchMultiplier = Main.rand.NextFloat(2.0f, 4.5f);
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.91f;
            Position += Velocity;
            Rotation = Velocity.ToRotation();
            Scale = _baseScale * (1f - LifetimeCompletion * 0.8f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = MathF.Pow(1f - LifetimeCompletion, 0.5f);
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-stretched  -- the spark is a jagged crack in reality
            float speed = MathHelper.Clamp(Velocity.Length() * 0.2f, 1f, _stretchMultiplier);
            Vector2 sparkScale = new(Scale * speed * 1.8f, Scale * 0.3f);

            // Bright core spark  -- the rift's cutting edge
            sb.Draw(tex, drawPos, null, Color * alpha * 0.9f, Rotation, tex.Size() / 2f, sparkScale, SpriteEffects.None, 0f);
            // Wider soft glow behind
            sb.Draw(tex, drawPos, null, VoidVariationUtils.RiftTeal * alpha * 0.35f, Rotation, tex.Size() / 2f, sparkScale * new Vector2(1.4f, 2.5f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  TRI-BEAM CONVERGENCE MOTE  -- Glowing motes that spiral along the three
    //  beams toward their convergence point. Additive, 3-layer bloom draw,
    //  follows beam path with slight offset wobble
    // =========================================================================
    public class TriBeamConvergenceMote : VoidVariationParticle
    {
        private readonly float _baseScale;
        private readonly float _wobbleOffset;
        private readonly float _wobbleSpeed;
        private readonly Vector2 _beamDirection;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TriBeamConvergenceMote(Vector2 position, Vector2 beamDirection, Color color, float scale, int lifetime)
        {
            Position = position;
            _beamDirection = beamDirection.SafeNormalize(Vector2.UnitX);
            Velocity = _beamDirection * Main.rand.NextFloat(3f, 7f);
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _wobbleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            _wobbleSpeed = Main.rand.NextFloat(0.08f, 0.15f);
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Accelerate along beam direction  -- drawn toward the convergence point
            Velocity += _beamDirection * 0.15f;
            Velocity *= 0.96f;

            // Perpendicular wobble  -- slight offset from beam axis for organic feel
            Vector2 perp = new(-_beamDirection.Y, _beamDirection.X);
            float wobble = MathF.Sin(Time * _wobbleSpeed + _wobbleOffset) * 2.5f;
            Position += Velocity + perp * wobble * 0.3f;

            Rotation += 0.04f;
            Scale = _baseScale * VoidVariationUtils.SineBump(LifetimeCompletion);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = VoidVariationUtils.SineBump(LifetimeCompletion);
            if (alpha <= 0.01f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Outermost halo 竜・ ambient convergence energy (capped to 300px on 2160px texture)
            sb.Draw(tex, drawPos, null, VoidVariationUtils.AbyssPurple * alpha * 0.15f, Rotation, tex.Size() / 2f, MathHelper.Min(Scale * 3.5f, 0.139f), SpriteEffects.None, 0f);
            // Layer 2: Mid bloom 竜・ the mote's visible body
            sb.Draw(tex, drawPos, null, Color * alpha * 0.5f, Rotation * 0.6f, tex.Size() / 2f, MathHelper.Min(Scale * 1.2f, 0.139f), SpriteEffects.None, 0f);
            // Layer 3: White-hot core 竜・ the concentrated beam energy
            sb.Draw(tex, drawPos, null, VoidVariationUtils.SunderingWhite * alpha * 0.7f, Rotation * 0.3f, tex.Size() / 2f, MathHelper.Min(Scale * 0.3f, 0.139f), SpriteEffects.None, 0f);
        }
    }
}