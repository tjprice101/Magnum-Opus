using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Eroica.Minions
{
    #region FinalityUtils

    /// <summary>
    /// Color constants and shader region helpers for Finality of the Sakura VFX.
    /// </summary>
    public static class FinalityUtils
    {
        public static readonly Color AbyssalCrimson = new Color(120, 20, 30);
        public static readonly Color FateViolet = new Color(100, 40, 120);
        public static readonly Color EmberGold = new Color(255, 180, 60);
        public static readonly Color SakuraFlame = new Color(220, 80, 100);
        public static readonly Color AshGray = new Color(130, 120, 115);
        public static readonly Color SummonGlow = new Color(255, 150, 180);
        public static readonly Color CoreWhite = new Color(255, 250, 240);

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

    #region FinalityParticleHandler

    /// <summary>
    /// Particle spawner for Finality of the Sakura effects. Delegates to MagnumParticleHandler.
    /// </summary>
    public static class FinalityParticleHandler
    {
        public static void SpawnParticle(Particle particle)
        {
            MagnumParticleHandler.SpawnParticle(particle);
        }
    }

    #endregion

    #region FinalityTrailSettings & Renderer

    public class FinalityTrailSettings
    {
        public Func<float, float> WidthFunction;
        public Func<float, Color> ColorFunction;
        public bool Smoothen;

        public FinalityTrailSettings(Func<float, float> widthFunction, Func<float, Color> colorFunction, bool smoothen = false)
        {
            WidthFunction = widthFunction;
            ColorFunction = colorFunction;
            Smoothen = smoothen;
        }
    }

    public static class FinalityTrailRenderer
    {
        /// <summary>
        /// Renders a simple bloom-based trail from position array using FinalityTrailSettings.
        /// Falls back to bloom dot rendering when shader trail infrastructure isn't available.
        /// </summary>
        public static void RenderTrail(Vector2[] positions, FinalityTrailSettings settings)
        {
            if (positions == null || positions.Length < 2) return;

            SpriteBatch sb = Main.spriteBatch;
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloom = MagnumTextureRegistry.GetBloom();
            if (bloom == null)
            {
                sb.End();
                return;
            }

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

            sb.End();
        }
    }

    #endregion

    #region Finality Particles

    /// <summary>Dark bloom flash particle — additive, quick fade.</summary>
    public class DarkBloomParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public DarkBloomParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Scale *= 0.95f;
            Color *= 0.92f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            Color drawColor = Color with { A = 0 } * (1f - LifetimeCompletion);
            spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Dark flame wisp — drifts and fades.</summary>
    public class DarkFlameParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public DarkFlameParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.96f;
            Velocity.Y -= 0.02f; // Slight rise
            Scale *= 0.97f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetSmallBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = 1f - LifetimeCompletion;
            Color drawColor = Color with { A = 0 } * (fade * fade);
            spriteBatch.Draw(tex, drawPos, null, drawColor, Rotation, origin, Scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Ash mote — slow drift downward, fading grey.</summary>
    public class SummonAshParticle : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => false;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public SummonAshParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.98f;
            Velocity.Y += 0.01f; // Gravity
            Rotation += 0.02f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetSmallBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = 1f - LifetimeCompletion;
            spriteBatch.Draw(tex, drawPos, null, Color * fade, Rotation, origin, Scale * 0.5f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Finality music note — rises with rotation.</summary>
    public class FinalityNoteParticle : Particle
    {
        public override string Texture => "MusicNoteQuarter";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;

        public FinalityNoteParticle(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
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
            Velocity.X += MathF.Sin(Time * 0.15f) * 0.05f;
            Rotation += 0.03f * Math.Sign(Velocity.X + 0.01f);
            Color *= 0.98f;
        }
    }

    /// <summary>Fate spark — fast outward burst, deceleration.</summary>
    public class FateSpark : Particle
    {
        public override string Texture => "BloomCircle";
        public override bool UseAdditiveBlend => true;
        public override bool SetLifetime => true;
        public override bool UseCustomDraw => true;

        public FateSpark(Vector2 position, Vector2 velocity, Color color, float scale, int lifetime)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
        }

        public override void Update()
        {
            Velocity *= 0.9f;
            Scale *= 0.94f;
        }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            Texture2D tex = MagnumTextureRegistry.GetPointBloom();
            if (tex == null) return;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;
            float fade = 1f - LifetimeCompletion;
            Color drawColor = Color with { A = 0 } * fade;
            spriteBatch.Draw(tex, drawPos, null, drawColor, 0f, origin, Scale * 0.3f, SpriteEffects.None, 0f);
        }
    }

    #endregion
}
