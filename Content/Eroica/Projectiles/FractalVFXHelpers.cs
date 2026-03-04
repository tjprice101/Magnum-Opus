using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    #region FractalUtils

    /// <summary>
    /// Color constants and shader region helpers for Triumphant Fractal VFX.
    /// </summary>
    public static class FractalUtils
    {
        public static readonly Color FractalGold = new Color(255, 200, 50);
        public static readonly Color FractalViolet = new Color(140, 60, 180);
        public static readonly Color LightningBlue = new Color(120, 180, 255);
        public static readonly Color CrystalWhite = new Color(255, 245, 235);
        public static readonly Color GeometryPink = new Color(255, 140, 180);

        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
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

    #region TriumphantFractalParticleHandler

    /// <summary>
    /// Particle spawner for Triumphant Fractal effects. Delegates to MagnumParticleHandler.
    /// </summary>
    public static class TriumphantFractalParticleHandler
    {
        public static void SpawnParticle(Particle particle)
        {
            MagnumParticleHandler.SpawnParticle(particle);
        }
    }

    #endregion

    #region FractalTrailSettings & Renderer

    public class FractalTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public bool Smoothen;

        public FractalTrailSettings(Func<float, float> widthFunction, Func<float, Color> colorFunction, bool smoothen = false)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            Smoothen = smoothen;
        }
    }

    public static class FractalTrailRenderer
    {
        /// <summary>
        /// Renders a simple bloom-based trail from position array using FractalTrailSettings.
        /// </summary>
        public static void RenderTrail(Vector2[] positions, FractalTrailSettings settings)
        {
            if (positions == null || positions.Length < 2) return;

            SpriteBatch sb = Main.spriteBatch;
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            try
            {
                Texture2D bloom = MagnumTextureRegistry.GetBloom();
                if (bloom == null) return;

                Vector2 origin = bloom.Size() * 0.5f;

                for (int i = 0; i < positions.Length - 1; i++)
                {
                    float completion = (float)i / (positions.Length - 1);
                    float width = settings.WidthFunction(completion);
                    Color col = settings.ColorFunction(completion);
                    col.A = 0;

                    float scale = width / 32f;
                    Vector2 drawPos = positions[i] - Main.screenPosition;
                    sb.Draw(bloom, drawPos, null, col, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            finally
            {
                sb.End();
            }
        }
    }

    #endregion

    #region Fractal Particles

    /// <summary>Lightning arc particle — quick-fading electric spark.</summary>
    public class LightningArcParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public LightningArcParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.85f;
            Scale *= 0.92f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetPointBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = 1f - LifetimeCompletion;
            Color drawColor = Color with { A = 0 } * fade;
            spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, origin, Scale * 0.4f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Fractal spark — gold-crimson, fast deceleration.</summary>
    public class FractalSparkParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public FractalSparkParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.91f;
            Scale *= 0.95f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetPointBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = 1f - LifetimeCompletion;
            fade *= fade;
            Color drawColor = Color with { A = 0 } * fade;
            spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, origin, Scale * 0.35f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Fractal music note — rises with gentle sine wobble.</summary>
    public class FractalNoteParticle : Particle
    {
        public override string Texture => "MusicNoteQuarter";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;

        public FractalNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.97f;
            Velocity.X += MathF.Sin(Time * 0.12f) * 0.04f;
            Rotation += 0.025f * Math.Sign(Velocity.X + 0.01f);
            Color *= 0.985f;
        }
    }

    /// <summary>Geometry flash — bright white burst that fades fast.</summary>
    public class GeometryFlashParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public GeometryFlashParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale *= 0.88f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = MathF.Max(0f, 1f - LifetimeCompletion * 1.5f);
            Color drawColor = Color with { A = 0 } * fade;
            spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Fractal bloom — expanding golden bloom sphere.</summary>
    public class FractalBloomParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public FractalBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale += 0.03f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = 1f - LifetimeCompletion;
            fade *= fade;
            Color drawColor = Color with { A = 0 } * (fade * 0.5f);
            spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    #endregion
}
