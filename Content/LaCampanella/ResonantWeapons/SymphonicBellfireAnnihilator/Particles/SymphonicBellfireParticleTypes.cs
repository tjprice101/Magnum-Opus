using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Particles
{
    /// <summary>
    /// Rocket exhaust ember 遯ｶ繝ｻelongated fire particle trailing behind rocket.
    /// </summary>
    public class RocketExhaustParticle : SymphonicBellfireParticle
    {
        private float stretchFactor;

        public RocketExhaustParticle(Vector2 pos, Vector2 vel, float stretch, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            stretchFactor = stretch;
            Scale = Main.rand.NextFloat(0.2f, 0.45f);
            Rotation = vel.ToRotation();
            DrawColor = SymphonicBellfireUtils.RocketPalette[Main.rand.Next(2, 4)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.94f;
            Scale *= 0.97f;
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            Vector2 stretch = new Vector2(stretchFactor * Scale * 0.1f, Scale * 0.08f);
            // 1024px SoftGlow → max 300px per axis
            float rocketMaxDim = Math.Max(stretch.X, stretch.Y);
            float rocketCap = rocketMaxDim > 0.293f ? 0.293f / rocketMaxDim : 1f;
            stretch *= rocketCap;
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * (fade * fade), Rotation, tex.Size() / 2f, stretch, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Explosion fireball 遯ｶ繝ｻlarge expanding fire sphere on rocket detonation.
    /// </summary>
    public class ExplosionFireballParticle : SymphonicBellfireParticle
    {
        private float maxScale;

        public ExplosionFireballParticle(Vector2 pos, float maxScale, int lifetime)
        {
            Position = pos;
            this.maxScale = maxScale;
            DrawColor = SymphonicBellfireUtils.RocketPalette[2]; // Fire orange
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            Scale = maxScale * (float)Math.Sin(t * MathHelper.Pi);
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float fadeSq = fade * fade;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Position - Main.screenPosition;

            // 1024px SoftGlow → max 300px; cap proportionally
            float fireCap = Scale * 0.55f > 0.293f ? 0.293f / (Scale * 0.55f) : 1f;

            // Outer fire shell — dark orange, wide, soft
            Color outerColor = SymphonicBellfireUtils.RocketPalette[0] * (fadeSq * 0.35f);
            sb.Draw(tex, drawPos, null, outerColor, 0f, origin, Scale * 0.55f * fireCap, SpriteEffects.None, 0f);

            // Mid body — fire orange
            Color midColor = DrawColor * (fadeSq * 0.5f);
            sb.Draw(tex, drawPos, null, midColor, 0f, origin, Scale * 0.4f * fireCap, SpriteEffects.None, 0f);

            // White-hot core — bright center
            Color coreColor = new Color(255, 240, 200) * (fadeSq * 0.7f);
            sb.Draw(tex, drawPos, null, coreColor, 0f, origin, Scale * 0.18f * fireCap, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Crescendo wave ring 遯ｶ繝ｻexpanding golden ring for Grand Crescendo detonation.
    /// </summary>
    public class CrescendoWaveParticle : SymphonicBellfireParticle
    {
        private float maxRadius;
        private float currentRadius;

        public CrescendoWaveParticle(Vector2 center, float maxRad, int lifetime)
        {
            Position = center;
            maxRadius = maxRad;
            DrawColor = SymphonicBellfireUtils.CrescendoPalette[1]; // Blazing gold
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            currentRadius = maxRadius * (float)Math.Sqrt(t);
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float scale = MathHelper.Min(currentRadius / (tex.Width * 0.5f), 0.139f);
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * (fade * 0.6f), 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Symphonic note 遯ｶ繝ｻmusical identity particle for the annihilator, fiery notes.
    /// </summary>
    public class SymphonicNoteParticle : SymphonicBellfireParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private int noteVariant;
        private float sineOffset;
        public override bool UseAdditiveBlend => false;

        public SymphonicNoteParticle(Vector2 pos, Vector2 vel, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            noteVariant = Main.rand.Next(NoteTextureNames.Length);
            sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            Scale = Main.rand.NextFloat(0.35f, 0.55f);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            DrawColor = SymphonicBellfireUtils.CrescendoPalette[Main.rand.Next(3)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity;
            Position.X += (float)Math.Sin(Time * 0.07f + sineOffset) * 0.5f;
            Velocity *= 0.97f;
            Rotation += 0.02f;
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>($"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[noteVariant]}", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float alpha = fade > 0.5f ? 1f : fade * 2f;
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
}