using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles
{
    #region FlameJetParticle — Concentrated directional fire stream

    /// <summary>
    /// Elongated fire particle that stretches in movement direction.
    /// Used for thrust jets and geyser plumes. Magma-crimson tones.
    /// </summary>
    public class FlameJetParticle : IgnitionOfTheBellParticle
    {
        private float _heat;
        private float _baseScale;

        public FlameJetParticle(Vector2 pos, Vector2 vel, float heat, int lifetime, float scale)
        {
            Position = pos;
            Velocity = vel;
            _heat = MathHelper.Clamp(heat, 0f, 1f);
            _baseScale = scale;
            Scale = scale;
            DrawColor = IgnitionOfTheBellUtils.GetThrustGradient(heat);
            Rotation = vel.ToRotation();
            SetLifetime(lifetime);
        }

        public override bool UseAdditiveBlend => true;

        public override void Update()
        {
            base.Update();
            float t = LifetimeCompletion;
            Scale = _baseScale * (1f - t * t);
            Rotation = Velocity.Length() > 0.1f ? Velocity.ToRotation() : Rotation;
            DrawColor = IgnitionOfTheBellUtils.GetThrustGradient(MathHelper.Lerp(_heat, 0.1f, t));
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try
            {
                tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (tex == null) return;

            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 screenPos = Position - Main.screenPosition;
            float alpha = 1f - LifetimeCompletion;
            Color col = IgnitionOfTheBellUtils.Additive(DrawColor, alpha * 0.7f);

            // Stretched along velocity direction
            Vector2 stretch = new Vector2(Scale * 1.8f, Scale * 0.5f);
            sb.Draw(tex, screenPos, null, col, Rotation, origin, stretch, SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region ThrustEmberParticle — Volcanic sparks from stab impacts

    /// <summary>
    /// Sharp angular sparks that scatter from thrust impacts.
    /// Slightly gravity-affected, velocity-squished rendering.
    /// </summary>
    public class ThrustEmberParticle : IgnitionOfTheBellParticle
    {
        private float _heat;
        private float _baseScale;

        public ThrustEmberParticle(Vector2 pos, Vector2 vel, float heat, int lifetime, float scale)
        {
            Position = pos;
            Velocity = vel;
            _heat = heat;
            _baseScale = scale;
            Scale = scale;
            DrawColor = IgnitionOfTheBellUtils.GetThrustGradient(heat);
            Rotation = vel.ToRotation();
            SetLifetime(lifetime);
        }

        public override bool UseAdditiveBlend => true;

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.94f;
            Velocity.Y += 0.04f; // Light gravity
            Time++;

            float t = LifetimeCompletion;
            Scale = _baseScale * (1f - t);
            Rotation = Velocity.Length() > 0.1f ? Velocity.ToRotation() : Rotation;
            DrawColor = IgnitionOfTheBellUtils.GetThrustGradient(MathHelper.Lerp(_heat, 0f, t));
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try
            {
                tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (tex == null) return;

            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 screenPos = Position - Main.screenPosition;
            float alpha = 1f - LifetimeCompletion;
            Color col = IgnitionOfTheBellUtils.Additive(DrawColor, alpha * 0.8f);

            float stretch = 1f + Velocity.Length() * 0.15f;
            sb.Draw(tex, screenPos, null, col, Rotation, origin, new Vector2(Scale * stretch, Scale * 0.5f), SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region CycloneFlameParticle — Swirling fire for cyclone explosions

    /// <summary>
    /// Flame particle that orbits outward in a spiral pattern.
    /// Used when Chime Cyclone triggers. Cherry-crimson cyclone tones.
    /// </summary>
    public class CycloneFlameParticle : IgnitionOfTheBellParticle
    {
        private float _angle;
        private float _angularSpeed;
        private float _radius;
        private float _radiusExpand;
        private Vector2 _center;
        private float _baseScale;

        public CycloneFlameParticle(Vector2 center, float startAngle, float angularSpeed, float startRadius, float expandRate, int lifetime, float scale)
        {
            _center = center;
            _angle = startAngle;
            _angularSpeed = angularSpeed;
            _radius = startRadius;
            _radiusExpand = expandRate;
            _baseScale = scale;
            Scale = scale;
            DrawColor = IgnitionOfTheBellUtils.GetCycloneGradient(0.6f);
            SetLifetime(lifetime);
        }

        public override bool UseAdditiveBlend => true;

        public override void Update()
        {
            Time++;
            _angle += _angularSpeed;
            _radius += _radiusExpand;
            Position = _center + _angle.ToRotationVector2() * _radius;

            float t = LifetimeCompletion;
            Scale = _baseScale * (float)Math.Sin(t * Math.PI);
            float gradientT = MathHelper.Lerp(0.7f, 0.1f, t);
            DrawColor = IgnitionOfTheBellUtils.GetCycloneGradient(gradientT);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try
            {
                tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (tex == null) return;

            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 screenPos = Position - Main.screenPosition;
            float alpha = (float)Math.Sin(LifetimeCompletion * Math.PI);
            Color col = IgnitionOfTheBellUtils.Additive(DrawColor, alpha * 0.6f);

            sb.Draw(tex, screenPos, null, col, _angle, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region BellIgnitionFlashParticle — Radial burst on thrust impact

    /// <summary>
    /// Quick radial flash with crimson-gold tones for thrust impacts.
    /// </summary>
    public class BellIgnitionFlashParticle : IgnitionOfTheBellParticle
    {
        private float _baseScale;

        public BellIgnitionFlashParticle(Vector2 pos, int lifetime, float scale)
        {
            Position = pos;
            Velocity = Vector2.Zero;
            _baseScale = scale;
            Scale = scale;
            DrawColor = new Color(255, 140, 30);
            SetLifetime(lifetime);
        }

        public override bool UseAdditiveBlend => true;

        public override void Update()
        {
            Time++;
            float t = LifetimeCompletion;
            Scale = _baseScale * (1f + t * 0.5f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try
            {
                tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (tex == null) return;

            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Vector2 screenPos = Position - Main.screenPosition;
            float alpha = 1f - LifetimeCompletion;
            alpha *= alpha;

            // Crimson outer
            Color outer = IgnitionOfTheBellUtils.Additive(new Color(200, 40, 0), alpha * 0.4f);
            sb.Draw(tex, screenPos, null, outer, 0f, origin, Scale * 1.5f, SpriteEffects.None, 0f);

            // Gold mid
            Color mid = IgnitionOfTheBellUtils.Additive(new Color(255, 160, 40), alpha * 0.6f);
            sb.Draw(tex, screenPos, null, mid, 0f, origin, Scale, SpriteEffects.None, 0f);

            // White core
            Color core = IgnitionOfTheBellUtils.Additive(new Color(255, 240, 210), alpha * 0.8f);
            sb.Draw(tex, screenPos, null, core, 0f, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    #endregion
}
