using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Particles
{
    #region ArcaneOrbParticle — Swirling magic fire orb fragment

    public class ArcaneOrbParticle : FangOfTheInfiniteBellParticle
    {
        private float _heat, _baseScale;

        public ArcaneOrbParticle(Vector2 pos, Vector2 vel, float heat, int lifetime, float scale)
        {
            Position = pos; Velocity = vel; _heat = heat; _baseScale = scale; Scale = scale;
            DrawColor = FangOfTheInfiniteBellUtils.GetArcaneGradient(heat);
            Rotation = vel.ToRotation(); SetLifetime(lifetime);
        }

        public override void Update()
        {
            base.Update();
            float t = LifetimeCompletion;
            Scale = _baseScale * (1f - t * t);
            DrawColor = FangOfTheInfiniteBellUtils.GetArcaneGradient(MathHelper.Lerp(_heat, 0.1f, t));
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (tex == null) return;
            Vector2 origin = new(tex.Width / 2f, tex.Height / 2f);
            float alpha = 1f - LifetimeCompletion;
            sb.Draw(tex, Position - Main.screenPosition, null,
                FangOfTheInfiniteBellUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, origin, MathHelper.Min(Scale, 0.586f), SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region EmpoweredSparkParticle — Electric gold sparks during empowerment

    public class EmpoweredSparkParticle : FangOfTheInfiniteBellParticle
    {
        private float _baseScale;

        public EmpoweredSparkParticle(Vector2 pos, Vector2 vel, int lifetime, float scale)
        {
            Position = pos; Velocity = vel; _baseScale = scale; Scale = scale;
            DrawColor = FangOfTheInfiniteBellUtils.GetEmpoweredGradient(Main.rand.NextFloat(0.4f, 0.8f));
            Rotation = vel.ToRotation(); SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity; Velocity *= 0.92f; Time++;
            float t = LifetimeCompletion;
            Scale = _baseScale * (1f - t);
            DrawColor = FangOfTheInfiniteBellUtils.GetEmpoweredGradient(MathHelper.Lerp(0.7f, 0.2f, t));
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (tex == null) return;
            Vector2 origin = new(tex.Width / 2f, tex.Height / 2f);
            float alpha = 1f - LifetimeCompletion;
            sb.Draw(tex, Position - Main.screenPosition, null,
                FangOfTheInfiniteBellUtils.Additive(DrawColor, alpha * 0.8f),
                Rotation, origin, new Vector2(Scale * 1.5f, Scale * 0.4f), SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region ArcaneFlashParticle — Impact flash

    public class ArcaneFlashParticle : FangOfTheInfiniteBellParticle
    {
        private float _baseScale;
        private bool _empowered;

        public ArcaneFlashParticle(Vector2 pos, int lifetime, float scale, bool empowered = false)
        {
            Position = pos; Velocity = Vector2.Zero; _baseScale = scale; Scale = scale;
            _empowered = empowered;
            DrawColor = empowered ? new Color(255, 220, 50) : new Color(255, 140, 30);
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Time++;
            Scale = _baseScale * (1f + LifetimeCompletion * 0.3f);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (tex == null) return;
            Vector2 origin = new(tex.Width / 2f, tex.Height / 2f);
            float alpha = 1f - LifetimeCompletion; alpha *= alpha;
            Vector2 screenPos = Position - Main.screenPosition;

            // 1024px SoftGlow → max 200px; tighter cap for less screen fill
            float arcaneCap = Scale * 1.0f > 0.195f ? 0.195f / (Scale * 1.0f) : 1f;

            Color outer = _empowered
                ? FangOfTheInfiniteBellUtils.Additive(new Color(200, 150, 0), alpha * 0.25f)
                : FangOfTheInfiniteBellUtils.Additive(new Color(180, 50, 20), alpha * 0.25f);
            sb.Draw(tex, screenPos, null, outer, 0f, origin, Scale * 1.0f * arcaneCap, SpriteEffects.None, 0f);

            Color mid = FangOfTheInfiniteBellUtils.Additive(DrawColor, alpha * 0.4f);
            sb.Draw(tex, screenPos, null, mid, 0f, origin, Scale * 0.6f * arcaneCap, SpriteEffects.None, 0f);

            Color core = FangOfTheInfiniteBellUtils.Additive(new Color(255, 245, 210), alpha * 0.5f);
            sb.Draw(tex, screenPos, null, core, 0f, origin, Scale * 0.2f * arcaneCap, SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region MusicalBellNoteParticle — Floating music note with arcane glow

    public class MusicalBellNoteParticle : FangOfTheInfiniteBellParticle
    {
        private float _baseScale;
        private bool _empowered;

        public MusicalBellNoteParticle(Vector2 pos, Vector2 vel, int lifetime, float scale, bool empowered = false)
        {
            Position = pos; Velocity = vel; _baseScale = scale; Scale = scale;
            _empowered = empowered;
            DrawColor = empowered
                ? FangOfTheInfiniteBellUtils.GetEmpoweredGradient(0.5f)
                : FangOfTheInfiniteBellUtils.GetArcaneGradient(0.6f);
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            SetLifetime(lifetime);
        }

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.96f;
            Velocity.Y -= 0.03f; // Drift upward
            Rotation += 0.02f;
            Time++;
            float t = LifetimeCompletion;
            Scale = _baseScale * (float)Math.Sin(t * Math.PI);
        }

        public override void Draw(SpriteBatch sb)
        {
            Texture2D tex = null;
            try { tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/MusicNote", ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value; } catch { }
            if (tex == null) return;
            Vector2 origin = new(tex.Width / 2f, tex.Height / 2f);
            float alpha = (float)Math.Sin(LifetimeCompletion * Math.PI);
            sb.Draw(tex, Position - Main.screenPosition, null,
                FangOfTheInfiniteBellUtils.Additive(DrawColor, alpha * 0.7f),
                Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    #endregion
}
