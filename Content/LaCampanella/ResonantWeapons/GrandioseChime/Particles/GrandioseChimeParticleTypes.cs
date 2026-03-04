using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles
{
    /// <summary>
    /// Beam muzzle flash 遯ｶ繝ｻelongated directional burst from the weapon barrel.
    /// </summary>
    public class GrandioseBeamFlashParticle : GrandioseChimeParticle
    {
        private float direction;
        private float flashLength;

        public GrandioseBeamFlashParticle(Vector2 pos, float angle, float len, int lifetime)
        {
            Position = pos;
            direction = angle;
            flashLength = len;
            Scale = Main.rand.NextFloat(0.35f, 0.55f);
            DrawColor = GrandioseChimeUtils.BeamPalette[3];
            SetLifetime(lifetime);
        }

        public override void Update() { Scale *= 0.85f; Time++; }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            Vector2 stretch = new Vector2(flashLength * 0.02f * Scale, Scale * 0.12f);
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * (fade * fade), direction, tex.Size() / 2f, stretch, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Barrage burning note particle 遯ｶ繝ｻfiery note that arcs outward from main beam.
    /// </summary>
    public class BurningNoteParticle : GrandioseChimeParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private int noteVariant;
        public override bool UseAdditiveBlend => false;

        public BurningNoteParticle(Vector2 pos, Vector2 vel, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            noteVariant = Main.rand.Next(NoteTextureNames.Length);
            Scale = Main.rand.NextFloat(0.3f, 0.5f);
            Rotation = Main.rand.NextFloat(-0.4f, 0.4f);
            DrawColor = GrandioseChimeUtils.BarragePalette[Main.rand.Next(3)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.96f;
            Velocity.Y += 0.05f; // Gentle gravity
            Rotation += 0.03f;
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

    /// <summary>
    /// Mine pulse particle 遯ｶ繝ｻconcentric ring expanding from note mine detonation.
    /// </summary>
    public class MineDetonationPulseParticle : GrandioseChimeParticle
    {
        private float maxRadius;
        private float currentRadius;

        public MineDetonationPulseParticle(Vector2 center, float maxRad, int lifetime)
        {
            Position = center;
            maxRadius = maxRad;
            DrawColor = GrandioseChimeUtils.MinePalette[2]; // Detonation gold
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
            float scale = currentRadius / (tex.Width * 0.5f);
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * (fade * 0.5f), 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Kill echo particle 遯ｶ繝ｻspectral afterimage that marks where an enemy died.
    /// </summary>
    public class KillEchoParticle : GrandioseChimeParticle
    {
        private float maxScale;

        public KillEchoParticle(Vector2 pos, float maxScale, int lifetime)
        {
            Position = pos;
            this.maxScale = maxScale;
            DrawColor = GrandioseChimeUtils.EchoPalette[Main.rand.Next(3)];
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
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * (fade * 0.4f), 0f, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }
}