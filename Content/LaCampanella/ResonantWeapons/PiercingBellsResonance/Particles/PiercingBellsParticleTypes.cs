using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles
{
    /// <summary>
    /// Muzzle flash streak 遯ｶ繝ｻelongated directional flash spawned when firing.
    /// </summary>
    public class MuzzleFlashParticle : PiercingBellsParticle
    {
        private float direction;
        private float length;

        public MuzzleFlashParticle(Vector2 pos, float angle, float flashLength, int lifetime)
        {
            Position = pos;
            direction = angle;
            length = flashLength;
            Scale = Main.rand.NextFloat(0.3f, 0.5f);
            DrawColor = PiercingBellsResonanceUtils.StaccatoPalette[3]; // Muzzle flash white-gold
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Scale *= 0.88f;
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float alpha = fade * fade;
            // Draw elongated flash
            Vector2 stretch = new Vector2(length * 0.02f * Scale, Scale * 0.15f);
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * alpha, direction, tex.Size() / 2f, stretch, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Bullet tracer spark 遯ｶ繝ｻtiny bright particle that trails behind each bullet.
    /// </summary>
    public class BulletTracerParticle : PiercingBellsParticle
    {
        public BulletTracerParticle(Vector2 pos, Vector2 vel, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            Scale = Main.rand.NextFloat(0.15f, 0.3f);
            DrawColor = PiercingBellsResonanceUtils.MulticolorLerp(Main.rand.NextFloat(), PiercingBellsResonanceUtils.StaccatoPalette);
            SetLifetime(lifetime);
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * fade, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Resonant blast flash 遯ｶ繝ｻlarge radial burst for the 20th shot detonation.
    /// </summary>
    public class ResonantBlastFlashParticle : PiercingBellsParticle
    {
        private float maxScale;

        public ResonantBlastFlashParticle(Vector2 pos, float maxScale, int lifetime)
        {
            Position = pos;
            this.maxScale = maxScale;
            Scale = 0f;
            DrawColor = PiercingBellsResonanceUtils.ResonancePalette[3]; // Resonant white
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
                DrawColor * (fade * 0.5f), 0f, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Homing music note 遯ｶ繝ｻdrifts toward enemies after resonant blast, musical identity particle.
    /// </summary>
    public class ResonantNoteParticle : PiercingBellsParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private float sineOffset;
        private int noteVariant;

        public override bool UseAdditiveBlend => false;

        public ResonantNoteParticle(Vector2 pos, Vector2 vel, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            noteVariant = Main.rand.Next(NoteTextureNames.Length);
            Scale = Main.rand.NextFloat(0.3f, 0.5f);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            DrawColor = PiercingBellsResonanceUtils.ResonancePalette[Main.rand.Next(3)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity;
            Position.X += (float)Math.Sin(Time * 0.08f + sineOffset) * 0.6f;
            Velocity *= 0.98f;
            Rotation += 0.02f;
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            string path = $"MagnumOpus/Assets/Particles Asset Library/{NoteTextureNames[noteVariant]}";
            var tex = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float alpha = fade > 0.5f ? 1f : fade * 2f;
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
}