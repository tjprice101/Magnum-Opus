using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems.Bosses
{
    /// <summary>
    /// Centralized boss rendering utilities for shader-driven VFX.
    /// Provides reusable methods for aura rendering, trail primitives,
    /// dissolve effects, phase transition flashes, and more.
    /// 
    /// Usage: Call from boss PreDraw/PostDraw to layer shader effects.
    /// </summary>
    public static class BossRenderHelper
    {
        #region Aura / Presence Drawing

        /// <summary>
        /// Draws a pulsing shader-driven aura around the boss NPC.
        /// The aura scales with boss HP and phase.
        /// </summary>
        public static void DrawShaderAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            string shaderKey, Color primary, Color secondary, float baseRadius,
            float intensity, float time)
        {
            Effect shader = BossShaderManager.GetShader(shaderKey);
            if (shader == null)
            {
                // Fallback: draw a simple additive glow circle
                DrawFallbackAura(sb, npc, screenPos, primary, baseRadius, intensity);
                return;
            }

            // Save blend state, apply additive
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            BossShaderManager.ApplyAuraParams(shader, npc.Center - screenPos, baseRadius, intensity,
                primary, secondary, time);
            shader.CurrentTechnique.Passes[0].Apply();

            // Draw a quad covering the aura area
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow() ?? Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 drawPos = npc.Center - screenPos;
            float drawSize = baseRadius * 2.5f;
            sb.Draw(pixel, drawPos, null, Color.White, 0f,
                new Vector2(0.5f), drawSize / pixel.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void DrawFallbackAura(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Color color, float radius, float intensity)
        {
            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow() ?? Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 drawPos = npc.Center - screenPos;
            Color auraColor = color * (0.15f * intensity);
            auraColor.A = 0;

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + (float)Main.timeForVisualEffects * 0.01f;
                Vector2 offset = angle.ToRotationVector2() * (radius * 0.15f);
                sb.Draw(pixel, drawPos + offset, null, auraColor, 0f,
                    new Vector2(0.5f), radius * 2f / pixel.Width, SpriteEffects.None, 0f);
            }
        }

        #endregion

        #region Afterimage Trail Drawing

        /// <summary>
        /// Draws shader-driven afterimage trails using the NPC's oldPos/oldRot cache.
        /// Falls back to standard alpha-blended afterimages if shader unavailable.
        /// </summary>
        public static void DrawShaderTrail(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin,
            string shaderKey, Color trailColor, float width, float time,
            float velocityThreshold = 4f)
        {
            if (npc.velocity.Length() < velocityThreshold) return;

            Effect shader = BossShaderManager.GetShader(shaderKey);
            SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                BossShaderManager.ApplyTrailParams(shader, trailColor, width, 0.85f, time);
                shader.CurrentTechnique.Passes[0].Apply();
            }

            int trailLength = Math.Min(npc.oldPos.Length, 10);
            for (int i = 0; i < trailLength; i++)
            {
                if (npc.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / trailLength;
                Color c = trailColor * (1f - progress) * 0.5f;
                c.A = 0;
                float scale = npc.scale * (1f - progress * 0.2f);
                Vector2 pos = npc.oldPos[i] + npc.Size / 2f - screenPos;
                float rot = npc.oldRot.Length > i ? npc.oldRot[i] : npc.rotation;
                sb.Draw(texture, pos, sourceRect, c, rot, origin, scale, effects, 0f);
            }

            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        #endregion

        #region Dissolve / Death Effect

        /// <summary>
        /// Draws the boss sprite with a dissolve shader applied (death animation).
        /// Progress 0 = fully visible, 1 = fully dissolved.
        /// </summary>
        public static void DrawDissolve(SpriteBatch sb, NPC npc, Vector2 screenPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin,
            string shaderKey, float dissolveProgress, Color edgeColor, float edgeWidth = 0.05f)
        {
            Effect shader = BossShaderManager.GetShader(shaderKey);
            SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPos = npc.Center - screenPos;

            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                BossShaderManager.ApplyDissolveParams(shader, dissolveProgress, edgeColor, edgeWidth);
                shader.CurrentTechnique.Passes[0].Apply();
            }

            // If no shader, fade alpha manually
            float alpha = shader == null ? 1f - dissolveProgress : 1f;
            Color drawColor = Color.White * alpha;
            sb.Draw(texture, drawPos, sourceRect, drawColor, npc.rotation, origin, npc.scale, effects, 0f);

            if (shader != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        #endregion

        #region Phase Transition Flash

        /// <summary>
        /// Renders a phase transition flash effect: a brief whiteout that fades with
        /// optional shader coloring.
        /// </summary>
        public static void DrawPhaseTransition(SpriteBatch sb, NPC npc, Vector2 screenPos,
            string shaderKey, float progress, Color fromColor, Color toColor, float intensity)
        {
            if (progress <= 0f || progress >= 1f) return;

            Effect shader = BossShaderManager.GetShader(shaderKey);
            
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (shader != null)
            {
                BossShaderManager.ApplyPhaseTransitionParams(shader, progress, fromColor, toColor,
                    intensity, (float)Main.timeForVisualEffects * 0.02f);
                shader.CurrentTechnique.Passes[0].Apply();
            }

            // Draw expanding flash ring
            float flashAlpha = (float)Math.Sin(progress * MathHelper.Pi);
            Color flashColor = Color.Lerp(fromColor, toColor, progress) * flashAlpha * intensity;
            flashColor.A = 0;

            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow() ?? Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 drawPos = npc.Center - screenPos;
            float expandRadius = 200f * progress;
            
            sb.Draw(pixel, drawPos, null, flashColor, 0f,
                new Vector2(0.5f), expandRadius * 2f / pixel.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Boss Glow Overlay

        /// <summary>
        /// Draws a soft glow overlay behind/around the boss sprite.
        /// Standard for all bosses — drawn before the main sprite.
        /// </summary>
        public static void DrawGlowOverlay(SpriteBatch sb, Vector2 drawPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin,
            Color glowColor, float glowScale, SpriteEffects effects, float rotation)
        {
            Color c = glowColor;
            c.A = 0;
            sb.Draw(texture, drawPos, sourceRect, c * 0.35f, rotation, origin, glowScale, effects, 0f);
        }

        /// <summary>
        /// Draws the standard boss sprite with proper lighting.
        /// </summary>
        public static void DrawBossSprite(SpriteBatch sb, NPC npc, Vector2 drawPos,
            Texture2D texture, Rectangle sourceRect, Vector2 origin,
            SpriteEffects effects)
        {
            Color mainColor = npc.IsABestiaryIconDummy
                ? Color.White
                : Lighting.GetColor((int)(npc.Center.X / 16), (int)(npc.Center.Y / 16));
            mainColor = Color.Lerp(mainColor, Color.White, 0.35f);
            float alpha = (255 - npc.alpha) / 255f;
            sb.Draw(texture, drawPos, sourceRect, mainColor * alpha, npc.rotation,
                origin, npc.scale, effects, 0f);
        }

        #endregion

        #region Attack Flash / Screen Impact

        /// <summary>
        /// Draws a brief attack-release flash at the boss position.
        /// Call once when an attack fires.
        /// </summary>
        public static void DrawAttackFlash(SpriteBatch sb, Vector2 position, Vector2 screenPos,
            string shaderKey, Color color, float scale, float time)
        {
            Effect shader = BossShaderManager.GetShader(shaderKey);
            Vector2 drawPos = position - screenPos;
            
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            if (shader != null)
            {
                var p = shader.Parameters;
                p["uColor"]?.SetValue(color.ToVector4());
                p["uIntensity"]?.SetValue(scale);
                p["uTime"]?.SetValue(time);
                shader.CurrentTechnique.Passes[0].Apply();
            }

            Color flashColor = color;
            flashColor.A = 0;
            Texture2D pixel = MagnumTextureRegistry.GetPointBloom() ?? Terraria.GameContent.TextureAssets.MagicPixel.Value;
            sb.Draw(pixel, drawPos, null, flashColor * 0.8f, 0f,
                new Vector2(0.5f), scale * 100f / pixel.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Spritesheet Helpers

        /// <summary>
        /// Gets the source rectangle for a spritesheet animation frame.
        /// Supports both single-column and grid (NxM) spritesheets.
        /// </summary>
        public static Rectangle GetSpriteSheetFrame(Texture2D texture, int currentFrame,
            int columns, int rows)
        {
            int frameWidth = texture.Width / columns;
            int frameHeight = texture.Height / rows;
            int col = currentFrame % columns;
            int row = currentFrame / columns;
            return new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
        }

        /// <summary>Gets the origin (center) of a spritesheet frame.</summary>
        public static Vector2 GetFrameOrigin(Texture2D texture, int columns, int rows)
        {
            return new Vector2(texture.Width / columns / 2f, texture.Height / rows / 2f);
        }

        #endregion
    }
}
