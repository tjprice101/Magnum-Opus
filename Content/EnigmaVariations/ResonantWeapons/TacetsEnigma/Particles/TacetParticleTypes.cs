using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Particles
{
    // =========================================================================
    //  SILENCE BURST 窶・Muzzle flash angular burst of silence-themed shards
    //  Like breaking glass made of compressed silence, sharp and brief
    // =========================================================================
    public class SilenceBurstParticle : TacetParticle
    {
        private readonly float _baseScale;
        private readonly float _stretchFactor;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public SilenceBurstParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _stretchFactor = Main.rand.NextFloat(1.5f, 3.0f);
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            Position += Velocity;
            Rotation = Velocity.ToRotation();
            Scale = _baseScale * (1f - LifetimeCompletion * 0.8f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha = MathF.Pow(alpha, 0.7f);
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Velocity-stretched scale for motion blur shard feel
            float stretch = MathHelper.Clamp(Velocity.Length() * 0.15f, 1f, _stretchFactor);
            Vector2 drawScale = new(Scale * stretch, Scale * 0.4f);

            // Rotated shard with bright core
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, drawScale, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, TacetUtils.FlashWhite * alpha * 0.5f, Rotation, tex.Size() / 2f, drawScale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  PARADOX BOLT GLOW 窶・Glowing orb traveling with the paradox bolt
    //  Leaves afterimages, multi-layered bloom: outer soft 竊・halo middle 竊・white center
    // =========================================================================
    public class ParadoxBoltGlowParticle : TacetParticle
    {
        private readonly Vector2[] _afterimagePositions;
        private int _afterimageIndex;
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public ParadoxBoltGlowParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            _baseScale = scale;
            _afterimagePositions = new Vector2[6];
            for (int i = 0; i < _afterimagePositions.Length; i++)
                _afterimagePositions[i] = position;
        }

        public override void Update()
        {
            // Shift afterimage ring buffer
            _afterimagePositions[_afterimageIndex % _afterimagePositions.Length] = Position;
            _afterimageIndex++;

            Position += Velocity;
            Velocity *= 0.99f;
            Scale = _baseScale * (1f - LifetimeCompletion * 0.3f);
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            if (alpha <= 0f) return;

            var softBloom = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            var haloTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;

            // Draw afterimages (faded copies trailing behind)
            for (int i = 0; i < _afterimagePositions.Length; i++)
            {
                int ringIdx = (_afterimageIndex - 1 - i + _afterimagePositions.Length * 2) % _afterimagePositions.Length;
                Vector2 afterPos = _afterimagePositions[ringIdx] - Main.screenPosition;
                float afterAlpha = alpha * (1f - (float)i / _afterimagePositions.Length) * 0.3f;
                float afterScale = Scale * (0.6f + 0.4f * (1f - (float)i / _afterimagePositions.Length));

                sb.Draw(softBloom, afterPos, null, Color * afterAlpha, 0f, softBloom.Size() / 2f, MathHelper.Min(afterScale * 1.5f, 0.139f), SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Outer soft bloom (large, dim) (capped to 300px on 2160px texture)
            sb.Draw(softBloom, drawPos, null, Color * alpha * 0.35f, 0f, softBloom.Size() / 2f, MathHelper.Min(Scale * 2.5f, 0.139f), SpriteEffects.None, 0f);

            // Layer 2: Halo middle (medium, paradox green tint) — capped to 300px on 2160px SoftCircle
            Color haloColor = Color.Lerp(Color, TacetUtils.ParadoxGreen, 0.4f);
            sb.Draw(haloTex, drawPos, null, haloColor * alpha * 0.6f, Time * 0.02f, haloTex.Size() / 2f, MathHelper.Min(Scale * 1.2f, 0.139f), SpriteEffects.None, 0f);

            // Layer 3: White-hot center
            sb.Draw(softBloom, drawPos, null, Color.White * alpha * 0.7f, 0f, softBloom.Size() / 2f, MathHelper.Min(Scale * 0.4f, 0.139f), SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  PARADOX STACK 窶・Small pulsing glyphs / ? marks orbiting the player
    //  Shows current stack count, AlphaBlend, uses Glyph textures
    // =========================================================================
    public class ParadoxStackParticle : TacetParticle
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
        private readonly int _glyphIndex;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true; // SoftRadialBloom halo has black bg
        public override bool UseCustomDraw => true;

        public ParadoxStackParticle(Vector2 orbitCenter, float orbitRadius, float startAngle, Color color, float scale, int lifetime)
        {
            _orbitCenter = orbitCenter;
            _orbitRadius = orbitRadius;
            _orbitAngle = startAngle;
            _orbitSpeed = Main.rand.NextFloat(0.03f, 0.06f) * (Main.rand.NextBool() ? 1 : -1);
            _glyphIndex = Main.rand.Next(NoteTextureNames.Length);
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

            // Pulse scale
            float pulse = MathF.Sin(Time * 0.15f) * 0.15f + 1f;
            Scale *= pulse > 0 ? 1f : 0.99f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = TacetUtils.SineBump(LifetimeCompletion);
            if (alpha <= 0f) return;

            string glyphPath = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[_glyphIndex]}";
            var tex = ModContent.Request<Texture2D>(glyphPath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            float pulse = MathF.Sin(Time * 0.15f) * 0.15f + 1f;
            float pulseScale = Scale * pulse;

            // Glow behind the glyph
            var glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            sb.Draw(glowTex, drawPos, null, Color * alpha * 0.25f, 0f, glowTex.Size() / 2f, MathHelper.Min(pulseScale * 3f, 0.139f), SpriteEffects.None, 0f);

            // Glyph itself
            sb.Draw(tex, drawPos, null, Color * alpha, Rotation, tex.Size() / 2f, pulseScale, SpriteEffects.None, 0f);
        }
    }

    // =========================================================================
    //  CHAIN LIGHTNING MOTE 窶・Tiny bright motes scattered along lightning paths
    //  Additive, simple 2-layer, very short lifetime (10-15 frames)
    // =========================================================================
    public class ChainLightningMoteParticle : TacetParticle
    {
        private readonly float _baseScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public ChainLightningMoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = (int)MathHelper.Clamp(lifetime, 10, 15);
            _baseScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.9f;
            Position += Velocity;
            Scale = _baseScale * (1f - LifetimeCompletion);
            Rotation += 0.05f;
        }

        public override void CustomDraw(SpriteBatch sb)
        {
            float alpha = 1f - LifetimeCompletion;
            alpha *= alpha; // Rapid falloff
            if (alpha <= 0f) return;

            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;

            // Layer 1: Colored glow
            sb.Draw(tex, drawPos, null, Color * alpha * 0.6f, 0f, tex.Size() / 2f, MathHelper.Min(Scale * 1.5f, 0.139f), SpriteEffects.None, 0f);

            // Layer 2: White-hot center
            sb.Draw(tex, drawPos, null, Color.White * alpha * 0.8f, 0f, tex.Size() / 2f, MathHelper.Min(Scale * 0.4f, 0.139f), SpriteEffects.None, 0f);
        }
    }
}