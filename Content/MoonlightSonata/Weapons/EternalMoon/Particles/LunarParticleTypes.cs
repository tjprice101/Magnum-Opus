using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles
{
    /// <summary>
    /// Tidal mote particle — a soft, luminous mote that drifts like moonlight on water.
    /// Floats gently with a sinusoidal wave motion, fading from ice blue to deep purple.
    /// Used during swings, trailing from the blade tip.
    /// </summary>
    public class TidalMoteParticle : LunarParticle
    {
        private readonly float _waveAmplitude;
        private readonly float _waveFrequency;
        private readonly float _initialAngle;
        private static Asset<Texture2D> _texture;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TidalMoteParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime,
            float waveAmplitude = 1.5f, float waveFrequency = 0.08f)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _waveAmplitude = waveAmplitude;
            _waveFrequency = waveFrequency;
            _initialAngle = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            // Sinusoidal drift perpendicular to velocity — like floating on water
            float waveOffset = (float)Math.Sin(_initialAngle + Time * _waveFrequency) * _waveAmplitude;
            Vector2 perpendicular = new Vector2(-Velocity.Y, Velocity.X);
            if (perpendicular.LengthSquared() > 0.001f)
                perpendicular.Normalize();
            Position += perpendicular * waveOffset * 0.3f;

            // Slow deceleration
            Velocity *= 0.97f;

            // Gentle rotation
            Rotation += 0.02f;

            // Fade and shrink over lifetime
            Scale *= 0.995f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            Texture2D tex = _texture.Value;

            float opacity = 1f - LifetimeCompletion;
            opacity *= opacity; // Quadratic fade for gentle disappearance
            Color color = Color.Lerp(DrawColor, EternalMoonUtils.DarkPurple, LifetimeCompletion * 0.6f);
            color.A = 0; // Additive needs zero alpha channel

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, Rotation, tex.Size() / 2f, Scale * 0.15f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Crescent spark — a bright, directional flash shaped like a crescent sliver.
    /// Spawns on impacts and phase transitions, conveying the moon's sharp edge.
    /// </summary>
    public class CrescentSparkParticle : LunarParticle
    {
        private readonly float _lengthScale;
        private static Asset<Texture2D> _texture;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public CrescentSparkParticle(Vector2 position, Vector2 velocity, float scale, Color color,
            int lifetime, float lengthScale = 3f)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation();
            _lengthScale = lengthScale;
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Rotation = Velocity.ToRotation();
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar");
            Texture2D tex = _texture.Value;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 0.5f);
            Color color = Color.Lerp(EternalMoonUtils.MoonWhite, DrawColor, LifetimeCompletion * 0.8f);
            color.A = 0;

            Vector2 scaleVec = new Vector2(_lengthScale, 1f) * Scale * 0.4f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, Rotation, tex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Lunar bloom particle — a large, soft bloom circle that expands and fades.
    /// Used for impact bursts, phase transitions, and the Full Moon crescendo.
    /// </summary>
    public class LunarBloomParticle : LunarParticle
    {
        private readonly float _expansionRate;
        private static Asset<Texture2D> _texture;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public LunarBloomParticle(Vector2 position, float scale, Color color, int lifetime,
            float expansionRate = 0.03f)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _expansionRate = expansionRate;
        }

        public override void Update()
        {
            Scale += _expansionRate;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            Texture2D tex = _texture.Value;

            float opacity = (1f - LifetimeCompletion);
            opacity = (float)Math.Pow(opacity, 1.5f);
            Color color = DrawColor;
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, 0f, tex.Size() / 2f, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Music note particle — a floating music note that drifts upward with gentle oscillation.
    /// Colored in the Moonlight Sonata palette (purple → ice blue).
    /// </summary>
    public class LunarNoteParticle : LunarParticle
    {
        private readonly float _wavePhase;
        private static Asset<Texture2D>[] _noteTextures;

        private readonly int _noteVariant;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;
        public override bool UseCustomDraw => true;

        private static readonly string[] NoteTexturePaths = new[]
        {
            "MagnumOpus/Assets/Particles Asset Library/MusicNote",
            "MagnumOpus/Assets/Particles Asset Library/QuarterNote",
            "MagnumOpus/Assets/Particles Asset Library/TallMusicNote",
            "MagnumOpus/Assets/Particles Asset Library/WholeNote",
            "MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote",
        };

        public LunarNoteParticle(Vector2 position, Vector2 velocity, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            Lifetime = lifetime;
            _wavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            _noteVariant = Main.rand.Next(NoteTexturePaths.Length);

            // Color from moonlight palette with variation
            float hue = Main.rand.NextFloat(0.3f, 0.8f);
            DrawColor = EternalMoonUtils.GetLunarGradient(hue);
        }

        public override void Update()
        {
            // Float upward with gentle horizontal oscillation
            Velocity.Y -= 0.02f;
            Position.X += (float)Math.Sin(_wavePhase + Time * 0.05f) * 0.5f;
            Velocity *= 0.98f;
            Rotation += Velocity.X * 0.02f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            if (_noteTextures == null)
            {
                _noteTextures = new Asset<Texture2D>[NoteTexturePaths.Length];
                for (int i = 0; i < NoteTexturePaths.Length; i++)
                    _noteTextures[i] = ModContent.Request<Texture2D>(NoteTexturePaths[i]);
            }

            Texture2D tex = _noteTextures[_noteVariant].Value;
            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 2f);
            float scale = Scale * (0.8f + 0.2f * (float)Math.Sin(Time * 0.1f));

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                DrawColor * opacity, Rotation, tex.Size() / 2f, scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Heavy tidal smoke — dense, slow-moving smoke clouds that linger after the Full Moon detonation.
    /// Deep purple with moonlight highlights at the edges.
    /// </summary>
    public class TidalSmokeParticle : LunarParticle
    {
        private static Asset<Texture2D> _texture;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TidalSmokeParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Rotation += 0.005f;
            Scale += 0.01f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke");
            Texture2D tex = _texture.Value;

            float opacity = (1f - LifetimeCompletion) * 0.4f;
            Color color = Color.Lerp(DrawColor, EternalMoonUtils.NightPurple, LifetimeCompletion);
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, Rotation, tex.Size() / 2f, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }
}
