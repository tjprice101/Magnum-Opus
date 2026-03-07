using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Particles
{
    /// <summary>
    /// Glowing ember that orbits outward from minion, used during idle and attack warm-ups.
    /// </summary>
    public class ChoirEmberParticle : InfernalChimesParticle
    {
        private float orbitAngle;
        private float orbitRadius;
        private float orbitSpeed;
        private Vector2 origin;

        public ChoirEmberParticle(Vector2 origin, float startAngle, float radius, float speed, int lifetime)
        {
            this.origin = origin;
            orbitAngle = startAngle;
            orbitRadius = radius;
            orbitSpeed = speed;
            Position = origin + new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * orbitRadius;
            Scale = Main.rand.NextFloat(0.3f, 0.6f);
            DrawColor = InfernalChimesCallingUtils.ChoirPalette[Main.rand.Next(3)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            orbitAngle += orbitSpeed;
            orbitRadius += 0.3f;
            Position = origin + new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * orbitRadius;
            float fade = 1f - LifetimeCompletion;
            DrawColor *= (0.95f + fade * 0.05f);
            Scale *= 0.995f;
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float alpha = fade * fade;
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * alpha, 0f, tex.Size() / 2f, Scale * 0.15f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Expanding ring pulse emitted on bell-ring attacks 遯ｶ繝ｻconcentric circle that grows and fades.
    /// </summary>
    public class BellRingPulseParticle : InfernalChimesParticle
    {
        private float maxRadius;
        private float currentRadius;

        public BellRingPulseParticle(Vector2 center, float maxRadius, int lifetime)
        {
            Position = center;
            this.maxRadius = maxRadius;
            currentRadius = 0f;
            Scale = 1f;
            DrawColor = InfernalChimesCallingUtils.ChoirPalette[2]; // resonant white
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            currentRadius = MathHelper.Lerp(0f, maxRadius, (float)Math.Sqrt(t));
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            float alpha = fade * 0.6f;
            float scale = MathHelper.Min(currentRadius / (tex.Width * 0.5f), 0.139f);
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * alpha, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Radial shockwave burst particle spawned on the 5th-hit shockwave detonation.
    /// </summary>
    public class ShockwavePulseParticle : InfernalChimesParticle
    {
        private float maxScale;

        public ShockwavePulseParticle(Vector2 center, float maxScale, int lifetime)
        {
            Position = center;
            this.maxScale = maxScale;
            Scale = 0f;
            DrawColor = InfernalChimesCallingUtils.ShockwavePalette[Main.rand.Next(3)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            // Quick expand, slow fade
            Scale = maxScale * (float)Math.Sin(t * MathHelper.Pi);
            Time++;
        }

        public override void Draw(SpriteBatch sb)
        {
            var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            float fade = 1f - LifetimeCompletion;
            sb.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * (fade * 0.7f), 0f, tex.Size() / 2f, MathHelper.Min(Scale * 0.5f, 0.293f), SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Musical note particle that drifts upward from minion during choir performance 遯ｶ繝ｻthe musical identity particle.
    /// </summary>
    public class MusicalChoirNoteParticle : InfernalChimesParticle
    {
        private static readonly string[] NoteTextureNames = new[]
        {
            "MusicNote", "CursiveMusicNote", "MusicNoteWithSlashes",
            "QuarterNote", "TallMusicNote", "WholeNote"
        };

        private float sineOffset;
        private float sineFreq;
        private int noteVariant;

        public override bool UseAdditiveBlend => false;

        public MusicalChoirNoteParticle(Vector2 pos, Vector2 vel, int lifetime)
        {
            Position = pos;
            Velocity = vel;
            sineOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            sineFreq = Main.rand.NextFloat(0.06f, 0.12f);
            noteVariant = Main.rand.Next(NoteTextureNames.Length);
            Scale = Main.rand.NextFloat(0.25f, 0.45f);
            Rotation = Main.rand.NextFloat(-0.3f, 0.3f);
            DrawColor = InfernalChimesCallingUtils.ChoirPalette[Main.rand.Next(3)];
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity;
            Position.X += (float)Math.Sin(Time * sineFreq + sineOffset) * 0.5f;
            Velocity.Y -= 0.01f; // gentle rise
            Velocity *= 0.98f;
            Rotation += 0.01f;
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