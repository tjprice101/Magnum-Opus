using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    #region BlossomUtils

    /// <summary>
    /// Color constants and shader region helpers for Blossom of the Sakura bullet VFX.
    /// Heat-reactive palette: cool sakura pink → warm crimson → white-hot.
    /// </summary>
    public static class BlossomUtils
    {
        public static readonly Color CoolPetal = new Color(255, 180, 200);
        public static readonly Color SakuraBody = new Color(230, 130, 160);
        public static readonly Color WarmCrimson = new Color(200, 60, 50);
        public static readonly Color WhiteHot = new Color(255, 250, 230);
        public static readonly Color MuzzleFlash = new Color(255, 220, 150);

        /// <summary>
        /// Returns a color along the heat gradient: cool petal (0) → warm crimson (0.5) → white-hot (1).
        /// </summary>
        public static Color GetHeatGradient(float heatProgress)
        {
            heatProgress = MathHelper.Clamp(heatProgress, 0f, 1f);
            if (heatProgress < 0.5f)
                return Color.Lerp(CoolPetal, WarmCrimson, heatProgress * 2f);
            else
                return Color.Lerp(WarmCrimson, WhiteHot, (heatProgress - 0.5f) * 2f);
        }

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

    #region BlossomParticleHandler

    /// <summary>
    /// Particle spawner for Blossom effects. Delegates to MagnumParticleHandler.
    /// </summary>
    public static class BlossomParticleHandler
    {
        public static void SpawnParticle(Particle particle)
        {
            MagnumParticleHandler.SpawnParticle(particle);
        }
    }

    #endregion

    #region BlossomTrailSettings & Renderer

    public class BlossomTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public bool Smoothen;

        public BlossomTrailSettings(Func<float, float> widthFunction, Func<float, Color> colorFunction, bool smoothen = false)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            Smoothen = smoothen;
        }
    }

    public static class BlossomTrailRenderer
    {
        /// <summary>
        /// Renders a bloom-based trail from position array using BlossomTrailSettings.
        /// </summary>
        public static void RenderTrail(Vector2[] positions, BlossomTrailSettings settings)
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

    #region TracerSparkParticle

    /// <summary>
    /// Fast tracer spark — bursts out from bullets then decelerates.
    /// </summary>
    public class TracerSparkParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;

        public TracerSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.89f;
            Scale = initialScale * (1f - LifetimeCompletion);
        }
    }

    #endregion

    #region HeatShimmerParticle

    /// <summary>
    /// Rising heat shimmer — translucent wispy particle for hot barrel effects.
    /// </summary>
    public class HeatShimmerParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;

        private float initialScale;

        public HeatShimmerParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity *= 0.97f;
            Velocity.Y -= 0.01f; // Rises gently
            float progress = LifetimeCompletion;
            float fade = progress < 0.2f ? progress / 0.2f : (1f - progress) / 0.8f;
            Scale = initialScale * fade;
        }
    }

    #endregion

    #region BulletPetalParticle

    /// <summary>
    /// Floating sakura petal particle — drifts with gravity and gentle rotation.
    /// Uses the ER Sakura Petal texture for authentic petal shape.
    /// </summary>
    public class BulletPetalParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot"; // Fallback only
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float initialScale;
        private float wobbleOffset;

        public BulletPetalParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity.Y += 0.015f; // Gentle gravity for petal drifting
            float sine = (float)Math.Sin(Time * 0.08f + wobbleOffset) * 0.3f;
            Position += new Vector2(sine, 0);
            Rotation += 0.04f;

            float progress = LifetimeCompletion;
            float alpha = progress < 0.1f ? progress / 0.1f : (1f - progress) / 0.9f;
            Scale = initialScale * alpha;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            // Try ER Sakura Petal texture first, fall back to GlowDot
            Texture2D tex = EroicaThemeTextures.ERSakuraPetal;
            if (tex == null)
            {
                tex = MagnumTextureRegistry.GetPixelTexture();
                if (tex == null) return;
            }

            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Position - Main.screenPosition;
            float progress = LifetimeCompletion;
            float fade = progress < 0.1f ? progress / 0.1f : (1f - progress) / 0.9f;
            Color drawColor = Color;
            drawColor.A = 0;

            spriteBatch.Draw(tex, drawPos, null, drawColor * fade, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region MuzzleFlashParticle

    /// <summary>
    /// Bright directional muzzle flash — quick pop then fade.
    /// </summary>
    public class MuzzleFlashParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float initialScale;

        public MuzzleFlashParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            float flashCurve = progress < 0.15f ? progress / 0.15f : (1f - progress) / 0.85f;
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
            float fade = 1f - LifetimeCompletion;
            float maxScale = 300f / bloomTex.Width;

            // Outer flash
            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.5f * fade, 0f, origin, MathF.Min(Scale * 1.8f, maxScale), SpriteEffects.None, 0f);
            // Hot core
            spriteBatch.Draw(bloomTex, drawPos, null, Color.White * 0.7f * fade, 0f, origin, MathF.Min(Scale * 0.5f, maxScale), SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region ImpactBloomParticle

    /// <summary>
    /// Expanding bloom burst for bullet impact moments.
    /// </summary>
    public class ImpactBloomParticle : Particle
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";
        public override bool SetLifetime => true;
        public override bool UseAdditiveBlend => true;
        public override bool UseCustomDraw => true;

        private float initialScale;

        public ImpactBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            float expandCurve = progress < 0.2f ? progress / 0.2f : 1f;
            Scale = initialScale * expandCurve * 1.8f;
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

            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.4f * fade, 0f, origin, MathF.Min(Scale * 2f, maxScale), SpriteEffects.None, 0f);
            spriteBatch.Draw(bloomTex, drawPos, null, drawColor * 0.6f * fade, 0f, origin, MathF.Min(Scale * 1.0f, maxScale), SpriteEffects.None, 0f);
            Color coreColor = Color.White;
            coreColor.A = 0;
            spriteBatch.Draw(bloomTex, drawPos, null, coreColor * 0.5f * fade, 0f, origin, MathF.Min(Scale * 0.4f, maxScale), SpriteEffects.None, 0f);
        }
    }

    #endregion
}
