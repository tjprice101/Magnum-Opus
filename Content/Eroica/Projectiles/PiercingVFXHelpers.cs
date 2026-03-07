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
    #region PiercingUtils

    /// <summary>
    /// Color constants and shader region helpers for Piercing Light of the Sakura VFX.
    /// </summary>
    public static class PiercingUtils
    {
        public static readonly Color LightningCore = new Color(255, 240, 180);
        public static readonly Color LightningEdge = new Color(220, 170, 80);
        public static readonly Color LightGold = new Color(255, 210, 100);
        public static readonly Color BrilliantWhite = new Color(255, 255, 240);
        public static readonly Color SakuraGlow = new Color(255, 180, 200);
        public static readonly Color CrescendoPink = new Color(240, 140, 170);

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

    #region PiercingParticleHandler

    /// <summary>
    /// Particle spawner for Piercing Light effects. Delegates to MagnumParticleHandler.
    /// </summary>
    public static class PiercingParticleHandler
    {
        public static void SpawnParticle(Particle particle)
        {
            MagnumParticleHandler.SpawnParticle(particle);
        }
    }

    #endregion

    #region PiercingTrailSettings & Renderer

    public class PiercingTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public bool Smoothen;

        public PiercingTrailSettings(Func<float, float> widthFunction, Func<float, Color> colorFunction, bool smoothen = false)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            Smoothen = smoothen;
        }
    }

    public static class PiercingTrailRenderer
    {
        /// <summary>
        /// Renders a bloom-based trail from position array using PiercingTrailSettings.
        /// Falls back to bloom dot rendering when shader trail infrastructure isn't available.
        /// </summary>
        public static void RenderTrail(Vector2[] positions, PiercingTrailSettings settings)
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

    #region LightningSparkParticle

    /// <summary>
    /// Electric spark particle — fast burst that decelerates rapidly.
    /// </summary>
    public class LightningSparkParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;

        public LightningSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.88f;
            Scale = initialScale * (1f - LifetimeCompletion);
            Rotation += 0.15f;
        }
    }

    #endregion

    #region CrescendoFlashParticle

    /// <summary>
    /// Bright flash burst particle for crescendo/impact moments.
    /// </summary>
    public class CrescendoFlashParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float initialScale;

        public CrescendoFlashParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            // Quick flash that fades: peak at 20%, then fade
            float flashCurve = progress < 0.2f ? progress / 0.2f : (1f - progress) / 0.8f;
            Scale = initialScale * flashCurve * 2f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex == null) return;

            Vector2 origin = bloomTex.Size() / 2f;
            Vector2 drawPos = Position - Main.screenPosition;
            Color drawColor = Color;
            drawColor.A = 0;
            float maxScale = 300f / bloomTex.Width;

            // Outer glow
            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.4f, 0f, origin, MathF.Min(Scale * 1.5f, maxScale), SpriteEffects.None, 0f);
            // Hot core
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White * 0.6f * (1f - LifetimeCompletion), 0f, origin, MathF.Min(Scale * 0.6f, maxScale), SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region SniperTrailParticle

    /// <summary>
    /// Lingering energy wisp that drifts slowly along the projectile trail.
    /// </summary>
    public class SniperTrailParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;

        public SniperTrailParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.96f;
            float fade = 1f - LifetimeCompletion;
            Scale = initialScale * fade;
        }
    }

    #endregion

    #region CrescendoNoteParticle

    /// <summary>
    /// Music note particle that rises and drifts with a gentle sine wobble.
    /// </summary>
    public class CrescendoNoteParticle : Particle
    {
        public override string Texture => "MusicNoteQuarter";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;
        private float wobbleOffset;

        public CrescendoNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity.Y -= 0.02f; // Gentle rise
            float sine = (float)Math.Sin(Time * 0.1f + wobbleOffset) * 0.4f;
            Position += new Vector2(sine, 0);
            Rotation = Velocity.X * 0.08f;

            float progress = LifetimeCompletion;
            // Fade in briefly then fade out
            float alpha = progress < 0.1f ? progress / 0.1f : (1f - progress) / 0.9f;
            Scale = initialScale * alpha;
        }
    }

    #endregion

    #region PiercingImpactParticle

    /// <summary>
    /// Radial star burst impact particle — expands quickly then fades.
    /// </summary>
    public class PiercingImpactParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float initialScale;

        public PiercingImpactParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            // Fast expand then slow fade
            float expandCurve = progress < 0.15f ? progress / 0.15f : 1f;
            float fadeCurve = 1f - progress;
            Scale = initialScale * expandCurve * 1.5f;
            Color *= fadeCurve > 0.01f ? 1f : 0f;
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

            // Multi-layer star burst
            // Outer glow sphere
            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.3f * fade, 0f, origin, MathF.Min(Scale * 2f, maxScale), SpriteEffects.None, 0f);
            // Mid layer
            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.5f * fade, 0f, origin, MathF.Min(Scale * 1.2f, maxScale), SpriteEffects.None, 0f);
            // Core
            Color coreColor = Color.White;
            coreColor.A = 0;
            spriteBatch.Draw(bloomTex, drawPos, null, coreColor * 0.7f * fade, 0f, origin, MathF.Min(Scale * 0.5f, maxScale), SpriteEffects.None, 0f);
        }
    }

    #endregion
}
