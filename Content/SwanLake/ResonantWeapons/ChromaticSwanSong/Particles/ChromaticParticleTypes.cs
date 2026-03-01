using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Particles
{
    /// <summary>
    /// Chromatic spark — small bright spark that shifts through the full spectrum.
    /// </summary>
    public class ChromaticSparkParticle : ChromaticParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 15 + Main.rand.Next(10);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.94f;
            Scale *= 0.97f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = 1f - Progress;
            Texture2D tex = MagnumTextureRegistry.GetPointBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Color shifting = ChromaticSwanUtils.GetChromatic(Progress);

            spriteBatch.Draw(tex, drawPos, null, shifting * alpha, 0f,
                tex.Size() * 0.5f, Scale * 0.12f, SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                spriteBatch.Draw(glow, drawPos, null, shifting * alpha * 0.4f, 0f,
                    glow.Size() * 0.5f, Scale * 0.2f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Music note glyph — floats upward with gentle sway, represents harmonic resonance.
    /// </summary>
    public class HarmonicNoteParticle : ChromaticParticle
    {
        private float _swayPhase;
        public override bool UseAdditiveBlend => false;
        protected override int SetLifetime() => 45 + Main.rand.Next(25);

        public override void Update()
        {
            base.Update();
            _swayPhase += 0.08f;
            Velocity.Y -= 0.03f; // Float up
            Velocity.X += (float)Math.Sin(_swayPhase) * 0.06f;
            Velocity *= 0.98f;
            Rotation += Velocity.X * 0.05f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = MathHelper.SmoothStep(1f, 0f, Progress);
            Texture2D tex = MagnumTextureRegistry.GetPointBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;

            // Note body — white with chromatic tint
            Color noteColor = Color.Lerp(Color.White, ChromaticSwanUtils.GetChromatic(Progress * 2f), 0.4f);
            spriteBatch.Draw(tex, drawPos, null, noteColor * alpha, Rotation,
                tex.Size() * 0.5f, new Vector2(Scale * 0.08f, Scale * 0.15f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                spriteBatch.Draw(glow, drawPos, null, noteColor * alpha * 0.35f, Rotation,
                    glow.Size() * 0.5f, new Vector2(Scale * 0.12f, Scale * 0.22f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Aria burst ring — expanding chromatic ring for detonation effects.
    /// </summary>
    public class AriaBurstParticle : ChromaticParticle
    {
        private float _maxRadius;
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 25 + Main.rand.Next(10);

        public void Setup(float maxRadius) { _maxRadius = maxRadius; }

        public override void Update() { base.Update(); }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float t = Progress;
            float radius = MathHelper.Lerp(0f, _maxRadius, (float)Math.Sqrt(t));
            float alpha = (1f - t) * (1f - t);

            Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
            if (pixel == null) return;
            int segments = 64;
            float thickness = MathHelper.Lerp(4f, 1f, t);

            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                Vector2 drawPos = Position - Main.screenPosition + offset;

                // Full spectrum around the ring
                Color col = ChromaticSwanUtils.GetSpectrumColor((float)i / segments + t * 0.5f);

                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), col * alpha * Scale,
                    0f, Vector2.Zero, thickness, SpriteEffects.None, 0f);
            }

            // Bloom overlay at ring center
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Color bloomCol = ChromaticSwanUtils.GetSpectrumColor(t * 0.5f);
                spriteBatch.Draw(glow, Position - Main.screenPosition, null, bloomCol * alpha * 0.3f * Scale,
                    0f, glow.Size() * 0.5f, radius / 40f, SpriteEffects.None, 0f);
            }
        }
    }

    /// <summary>
    /// Prismatic shard — sharp triangular fragment with rainbow coloring for impacts.
    /// </summary>
    public class PrismaticShardParticle : ChromaticParticle
    {
        public override bool UseAdditiveBlend => true;
        protected override int SetLifetime() => 20 + Main.rand.Next(10);

        public override void Update()
        {
            base.Update();
            Velocity *= 0.95f;
            Velocity.Y += 0.05f;
            Rotation += 0.15f;
            Scale *= 0.98f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float alpha = 1f - Progress * Progress;
            Texture2D tex = MagnumTextureRegistry.GetEllipse();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Color col = ChromaticSwanUtils.GetChromatic(Progress);

            // Elongated shard shape
            spriteBatch.Draw(tex, drawPos, null, col * alpha, Rotation,
                tex.Size() * 0.5f, new Vector2(Scale * 0.05f, Scale * 0.2f), SpriteEffects.None, 0f);

            // Additive bloom overlay
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
                spriteBatch.Draw(glow, drawPos, null, col * alpha * 0.35f, Rotation,
                    glow.Size() * 0.5f, new Vector2(Scale * 0.08f, Scale * 0.3f), SpriteEffects.None, 0f);
        }
    }
}
