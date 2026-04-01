using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    #region FuneralUtils

    /// <summary>
    /// Color constants and shader region helpers for Funeral Prayer VFX.
    /// </summary>
    public static class FuneralUtils
    {
        public static readonly Color PrayerFlame = new Color(200, 60, 40);
        public static readonly Color DeepCrimson = new Color(140, 20, 30);
        public static readonly Color SmolderingAmber = new Color(220, 140, 50);
        public static readonly Color SoulWhite = new Color(255, 245, 230);
        public static readonly Color RequiemViolet = new Color(120, 50, 100);
        public static readonly Color AshGray = new Color(130, 120, 115);
        public static readonly Color FuneralBlack = new Color(30, 20, 25);
        public static readonly Color EmberCore = new Color(255, 200, 80);

        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void ExitShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    #endregion

    #region FuneralParticleHandler

    /// <summary>
    /// Particle spawner for Funeral Prayer effects. Delegates to MagnumParticleHandler.
    /// </summary>
    public static class FuneralParticleHandler
    {
        public static void SpawnParticle(Particle particle)
        {
            MagnumParticleHandler.SpawnParticle(particle);
        }
    }

    #endregion

    #region FuneralTrailSettings & Renderer

    public class FuneralTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public bool Smoothen;

        public FuneralTrailSettings(Func<float, float> widthFunction, Func<float, Color> colorFunction, bool smoothen = false)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            Smoothen = smoothen;
        }
    }

    public static class FuneralTrailRenderer
    {
        /// <summary>
        /// Renders a bloom-based trail from position array using FuneralTrailSettings.
        /// </summary>
        public static void RenderTrail(Vector2[] positions, FuneralTrailSettings settings)
        {
            if (positions == null || positions.Length < 2) return;

            SpriteBatch sb = Main.spriteBatch;
            var bloomTex = MagnumTextureRegistry.GetSmallBloom();
            if (bloomTex == null) return;

            Vector2 origin = bloomTex.Size() / 2f;

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            try
            {
                for (int i = 0; i < positions.Length - 1; i++)
                {
                    if (positions[i] == Vector2.Zero || positions[i + 1] == Vector2.Zero) continue;

                    float completion = (float)i / (positions.Length - 1);
                    float width = settings.WidthFunction(completion);
                    Color color = settings.ColorFunction(completion);
                    color.A = 0;

                    float scale = width / bloomTex.Width * 2f;
                    Vector2 drawPos = positions[i] - Main.screenPosition;

                    sb.Draw(bloomTex, drawPos, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            finally
            {
                sb.End();
            }
        }
    }

    #endregion

    #region FuneralFlameParticle

    /// <summary>
    /// Rising flame particle for Funeral Prayer — drifts upward with flickering.
    /// </summary>
    public class FuneralFlameParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;

        public FuneralFlameParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            initialScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.94f;
            Velocity.Y -= 0.03f; // Rising
            Scale = initialScale * (1f - LifetimeCompletion);
            Rotation += 0.08f;
        }
    }

    #endregion

    #region RequiemSparkParticle

    /// <summary>
    /// Quick burst spark for Funeral requiem impacts — fast deceleration.
    /// </summary>
    public class FuneralSparkParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;

        public FuneralSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            initialScale = scale;
        }

        public override void Update()
        {
            Velocity *= 0.87f;
            Scale = initialScale * (1f - LifetimeCompletion);
        }
    }

    #endregion

    #region PrayerAshParticle

    /// <summary>
    /// Slow drifting ash particle — non-additive, falls gently with gravity.
    /// </summary>
    public class PrayerAshParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => false;

        private float initialScale;

        public PrayerAshParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            initialScale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.98f;
            Velocity.Y += 0.01f; // Gentle gravity
            Rotation += 0.02f;
            float progress = LifetimeCompletion;
            Scale = initialScale * (1f - progress * progress);
        }
    }

    #endregion

    #region ConvergenceBloomParticle

    /// <summary>
    /// Expanding bloom flash for convergence/impact moments.
    /// </summary>
    public class ConvergenceBloomParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float initialScale;

        public ConvergenceBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            initialScale = scale;
        }

        public override void Update()
        {
            float progress = LifetimeCompletion;
            float flashCurve = progress < 0.2f ? progress / 0.2f : (1f - progress) / 0.8f;
            Scale = initialScale * flashCurve * 2.5f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex == null) return;

            Vector2 origin = bloomTex.Size() / 2f;
            Vector2 drawPos = Position - Main.screenPosition;
            Color drawColor = Color;
            drawColor.A = 0;
            float fade = 1f - LifetimeCompletion;
            float maxScale = 300f / bloomTex.Width;

            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.4f * fade, 0f, origin, MathF.Min(Scale * 1.5f, maxScale), SpriteEffects.None, 0f);
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White * 0.5f * fade, 0f, origin, MathF.Min(Scale * 0.5f, maxScale), SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region FuneralNoteParticle

    /// <summary>
    /// Music note particle — rises with gentle sine wobble, representing the funeral hymn.
    /// </summary>
    public class FuneralNoteParticle : Particle
    {
        public override string Texture => "MusicNoteQuarter";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;
        private float wobbleOffset;

        public FuneralNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            initialScale = scale;
            wobbleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.Y -= 0.02f;
            float sine = (float)Math.Sin(Time * 0.1f + wobbleOffset) * 0.35f;
            Position += new Vector2(sine, 0);
            Rotation = Velocity.X * 0.06f;

            float progress = LifetimeCompletion;
            float alpha = progress < 0.1f ? progress / 0.1f : (1f - progress) / 0.9f;
            Scale = initialScale * alpha;
        }
    }

    #endregion
}
