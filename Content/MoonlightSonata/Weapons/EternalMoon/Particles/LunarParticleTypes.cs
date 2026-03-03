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

    // =================================================================
    // NEW PARTICLES — TIDAL PHASE OVERHAUL
    // =================================================================

    /// <summary>
    /// Tidal droplet — small water-like droplets that fall with gravity and slight horizontal drift.
    /// Spawned during higher tidal phases for a "crashing wave" feel.
    /// </summary>
    public class TidalDropletParticle : LunarParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _drift;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TidalDropletParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _drift = Main.rand.NextFloat(-0.3f, 0.3f);
        }

        public override void Update()
        {
            Velocity.Y += 0.15f; // Gravity
            Velocity.X += _drift * 0.1f;
            Velocity *= 0.99f;
            Scale *= 0.995f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            Texture2D tex = _texture.Value;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 1.5f);
            Color color = Color.Lerp(DrawColor, EternalMoonUtils.MoonWhite, LifetimeCompletion * 0.3f);
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, 0f, tex.Size() / 2f, Scale * 0.08f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Wave spray particle — burst radially from impact with high velocity and quick fade.
    /// Creates the foam/spray effect of a tidal crash.
    /// </summary>
    public class WaveSprayParticle : LunarParticle
    {
        private static Asset<Texture2D> _texture;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public WaveSprayParticle(Vector2 position, Vector2 velocity, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = velocity.ToRotation();
        }

        public override void Update()
        {
            Velocity *= 0.92f;
            Velocity.Y += 0.08f; // Slight arc trajectory
            Rotation = Velocity.ToRotation();
            Scale *= 0.97f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            Texture2D tex = _texture.Value;

            float opacity = 1f - (float)Math.Pow(LifetimeCompletion, 0.8f);
            Color color = Color.Lerp(EternalMoonUtils.MoonWhite, DrawColor, LifetimeCompletion * 0.5f);
            color.A = 0;

            Vector2 stretchScale = new Vector2(1f + Velocity.Length() * 0.15f, 1f) * Scale * 0.12f;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, Rotation, tex.Size() / 2f, stretchScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Moon glint — stationary sparkle at blade tip with slow rotation and gentle pulse.
    /// 4-pointed silver star that accents the blade's cutting edge.
    /// </summary>
    public class MoonGlintParticle : LunarParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _pulseSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public MoonGlintParticle(Vector2 position, float scale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            _pulseSpeed = 0.15f + Main.rand.NextFloat(0.1f);
        }

        public override void Update()
        {
            Rotation += 0.03f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft");
            Texture2D tex = _texture.Value;

            float pulse = 0.7f + 0.3f * (float)Math.Sin(Time * _pulseSpeed);
            float opacity = (1f - (float)Math.Pow(LifetimeCompletion, 2f)) * pulse;
            Color color = DrawColor;
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, Rotation, tex.Size() / 2f, Scale * 0.3f * pulse, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Gravity well mote — spirals inward toward a gravitational center, consumed on arrival.
    /// Used for gravitational pull VFX on hit.
    /// </summary>
    public class GravityWellMoteParticle : LunarParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly Vector2 _center;
        private float _orbitAngle;
        private float _orbitRadius;
        private readonly float _orbitSpeed;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public GravityWellMoteParticle(Vector2 position, Vector2 center, float scale, Color color, int lifetime)
        {
            Position = position;
            _center = center;
            Velocity = Vector2.Zero;
            Scale = scale;
            DrawColor = color;
            Lifetime = lifetime;
            _orbitRadius = Vector2.Distance(position, center);
            _orbitAngle = (position - center).ToRotation();
            _orbitSpeed = 0.08f + Main.rand.NextFloat(0.04f);
        }

        public override void Update()
        {
            // Logarithmic spiral inward
            _orbitAngle += _orbitSpeed;
            _orbitRadius *= 0.96f;
            Position = _center + _orbitAngle.ToRotationVector2() * _orbitRadius;
            Scale *= 0.99f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            Texture2D tex = _texture.Value;

            float opacity = (1f - (float)Math.Pow(LifetimeCompletion, 1.5f)) * 0.8f;
            Color color = Color.Lerp(DrawColor, EternalMoonUtils.MoonWhite, LifetimeCompletion * 0.4f);
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, 0f, tex.Size() / 2f, Scale * 0.1f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>
    /// Tidal phase indicator — a gentle glow ring that represents the current tidal phase level.
    /// Renders as concentric expanding rings around the player during active combo.
    /// </summary>
    public class TidalPhaseRingParticle : LunarParticle
    {
        private static Asset<Texture2D> _texture;
        private readonly float _maxScale;

        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        public TidalPhaseRingParticle(Vector2 position, float maxScale, Color color, int lifetime)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Scale = 0f;
            DrawColor = color;
            Lifetime = lifetime;
            _maxScale = maxScale;
        }

        public override void Update()
        {
            float t = LifetimeCompletion;
            // Fast expansion with smooth settling
            Scale = _maxScale * (float)(1.0 - Math.Pow(1.0 - t, 3));
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            _texture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle");
            Texture2D tex = _texture.Value;

            float opacity = (1f - LifetimeCompletion) * 0.35f;
            Color color = DrawColor;
            color.A = 0;

            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
                color * opacity, 0f, tex.Size() / 2f, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }
}
