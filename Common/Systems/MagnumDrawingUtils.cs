using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Drawing utility extensions inspired by Calamity Mod.
    /// Provides helper methods for afterimages, shader regions, backglow effects, and more.
    /// </summary>
    public static class MagnumDrawingUtils
    {
        #region SpriteBatch Shader Region Helpers
        
        /// <summary>
        /// Enters a shader-compatible SpriteBatch region.
        /// Use before applying shaders or special blend modes.
        /// </summary>
        public static void EnterShaderRegion(this SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Enters a shader-compatible SpriteBatch region with a custom blend state.
        /// </summary>
        public static void EnterShaderRegion(this SpriteBatch spriteBatch, BlendState blendState)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Exits a shader region and returns to normal deferred drawing.
        /// </summary>
        public static void ExitShaderRegion(this SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Sets the blend state for additive blending (glows, energy effects).
        /// </summary>
        public static void SetAdditiveBlend(this SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Returns to normal alpha blend mode.
        /// </summary>
        public static void SetAlphaBlend(this SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Afterimage Drawing

        /// <summary>
        /// Draws afterimages for a projectile centered on its position.
        /// Supports multiple trailing modes for different visual styles.
        /// </summary>
        /// <param name="proj">The projectile to draw afterimages for.</param>
        /// <param name="mode">Trailing mode (0 = standard, 1 = fading, 2 = with rotation).</param>
        /// <param name="lightColor">Base light color for the afterimages.</param>
        /// <param name="typeOneIncrement">For mode 1, controls how many afterimages to skip.</param>
        /// <param name="texture">Custom texture to use (null = projectile's default texture).</param>
        public static void DrawAfterimagesCentered(Projectile proj, int mode, Color lightColor, 
            int typeOneIncrement = 1, Texture2D texture = null)
        {
            if (texture == null)
                texture = TextureAssets.Projectile[proj.type].Value;

            int frameHeight = texture.Height / Main.projFrames[proj.type];
            int frameY = frameHeight * proj.frame;
            Rectangle rectangle = new Rectangle(0, frameY, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;

            SpriteEffects spriteEffects = proj.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color alphaColor = proj.GetAlpha(lightColor);
            int trailLength = ProjectileID.Sets.TrailCacheLength[proj.type];
            
            if (trailLength <= 0)
            {
                // No trail cache, just draw the projectile normally
                Vector2 drawPos = proj.Center - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                Main.spriteBatch.Draw(texture, drawPos, rectangle, alphaColor, proj.rotation, origin, proj.scale, spriteEffects, 0f);
                return;
            }

            switch (mode)
            {
                // Standard afterimages - linear opacity falloff
                case 0:
                    for (int i = 0; i < proj.oldPos.Length; i++)
                    {
                        if (proj.oldPos[i] == Vector2.Zero)
                            continue;

                        Vector2 drawPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                        Color color = alphaColor * ((float)(proj.oldPos.Length - i) / proj.oldPos.Length);
                        Main.spriteBatch.Draw(texture, drawPos, rectangle, color, proj.rotation, origin, proj.scale, spriteEffects, 0f);
                    }
                    break;

                // Fading afterimages with increment control
                case 1:
                    int increment = Math.Max(1, typeOneIncrement);
                    float afterimageColorCount = trailLength * 1.5f;
                    
                    for (int i = 0; i < trailLength; i += increment)
                    {
                        if (proj.oldPos[i] == Vector2.Zero)
                            continue;

                        float colorMult = (afterimageColorCount - i) / afterimageColorCount;
                        Color color = alphaColor * colorMult;
                        Vector2 drawPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                        Main.spriteBatch.Draw(texture, drawPos, rectangle, color, proj.oldRot[i], origin, proj.scale, spriteEffects, 0f);
                    }
                    break;

                // Afterimages with individual rotations
                case 2:
                    for (int i = 0; i < proj.oldPos.Length; i++)
                    {
                        if (proj.oldPos[i] == Vector2.Zero)
                            continue;

                        float afterimageRot = proj.oldRot[i];
                        SpriteEffects sfxForThisAfterimage = proj.oldSpriteDirection[i] == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        Vector2 drawPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition + new Vector2(0f, proj.gfxOffY);
                        Color color = alphaColor * ((float)(proj.oldPos.Length - i) / proj.oldPos.Length);
                        Main.spriteBatch.Draw(texture, drawPos, rectangle, color, afterimageRot, origin, proj.scale, sfxForThisAfterimage, 0f);
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws afterimages for an NPC with optional glow texture.
        /// </summary>
        public static void DrawNPCAfterimages(NPC npc, Color drawColor, int numAfterimages = 5, 
            Texture2D texture = null, Texture2D glowTexture = null)
        {
            if (texture == null)
                texture = TextureAssets.Npc[npc.type].Value;

            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / Main.npcFrameCount[npc.type] / 2f);
            SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 1; i < numAfterimages; i += 2)
            {
                Color afterimageColor = Color.Lerp(drawColor, Color.White, 0.5f);
                afterimageColor = npc.GetAlpha(afterimageColor);
                afterimageColor *= (numAfterimages - i) / 15f;

                Vector2 afterimageCenter = npc.oldPos[i] + new Vector2(npc.width, npc.height) / 2f - Main.screenPosition;
                afterimageCenter += origin * npc.scale + new Vector2(0f, npc.gfxOffY);

                Main.spriteBatch.Draw(texture, afterimageCenter, npc.frame, afterimageColor, 
                    npc.oldRot[i], origin, npc.scale, spriteEffects, 0f);

                if (glowTexture != null)
                {
                    Main.spriteBatch.Draw(glowTexture, afterimageCenter, npc.frame, afterimageColor, 
                        npc.oldRot[i], origin, npc.scale, spriteEffects, 0f);
                }
            }
        }

        #endregion

        #region Backglow and Aura Effects

        /// <summary>
        /// Draws a glowing backglow effect behind a projectile.
        /// </summary>
        public static void DrawBackglow(this Projectile proj, Color backglowColor, float backglowArea, 
            Texture2D texture = null, Rectangle? frame = null)
        {
            if (texture == null)
                texture = TextureAssets.Projectile[proj.type].Value;

            Rectangle sourceRect = frame ?? texture.Frame(1, Main.projFrames[proj.type], 0, proj.frame);
            Vector2 origin = sourceRect.Size() / 2f;
            Vector2 drawPos = proj.Center - Main.screenPosition;

            // Draw multiple offset copies for glow effect
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * backglowArea;
                Main.spriteBatch.Draw(texture, drawPos + offset, sourceRect, backglowColor * 0.5f, 
                    proj.rotation, origin, proj.scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draws a projectile with a backglow effect.
        /// </summary>
        public static void DrawProjectileWithBackglow(this Projectile proj, Color backglowColor, Color lightColor, 
            float backglowArea, Texture2D texture = null, Rectangle? frame = null)
        {
            proj.DrawBackglow(backglowColor, backglowArea, texture, frame);

            if (texture == null)
                texture = TextureAssets.Projectile[proj.type].Value;

            Rectangle sourceRect = frame ?? texture.Frame(1, Main.projFrames[proj.type], 0, proj.frame);
            Vector2 origin = sourceRect.Size() / 2f;
            Vector2 drawPos = proj.Center - Main.screenPosition;

            Main.spriteBatch.Draw(texture, drawPos, sourceRect, proj.GetAlpha(lightColor), 
                proj.rotation, origin, proj.scale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws an outline effect around a sprite.
        /// Useful for boss enrage states or powered-up weapons.
        /// </summary>
        public static void DrawOutline(Vector2 position, Texture2D texture, Rectangle? frame, Color outlineColor, 
            float rotation, Vector2 origin, float scale, float outlineThickness = 2f)
        {
            Rectangle sourceRect = frame ?? texture.Bounds;

            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * outlineThickness;
                Main.spriteBatch.Draw(texture, position + offset, sourceRect, outlineColor, 
                    rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }

        #endregion

        #region Trail Drawing

        /// <summary>
        /// Draws a simple star-shaped trail effect for a projectile.
        /// </summary>
        public static void DrawStarTrail(this Projectile proj, Color outerColor, Color innerColor, float auraHeight = 10f)
        {
            for (int i = 0; i < proj.oldPos.Length; i++)
            {
                if (proj.oldPos[i] == Vector2.Zero)
                    continue;

                float completion = (float)i / proj.oldPos.Length;
                float scale = MathHelper.Lerp(1f, 0.3f, completion);
                Color color = Color.Lerp(outerColor, innerColor, completion) * (1f - completion);

                Vector2 drawPos = proj.oldPos[i] + proj.Size * 0.5f - Main.screenPosition;
                
                // Draw a simple diamond/star shape using the magic pixel
                for (int j = 0; j < 4; j++)
                {
                    Vector2 offset = (MathHelper.PiOver2 * j + proj.oldRot[i]).ToRotationVector2() * auraHeight * scale;
                    Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawPos + offset, 
                        new Rectangle(0, 0, 1, 1), color, 0f, Vector2.Zero, 
                        new Vector2(2f, auraHeight * scale), SpriteEffects.None, 0f);
                }
            }
        }

        #endregion

        #region Pulse and Flash Effects

        /// <summary>
        /// Calculates a pulsing value between 0 and 1 for breathing/pulsing effects.
        /// </summary>
        public static float GetPulse(float speed = 1f, float offset = 0f)
        {
            return (float)(Math.Sin(Main.GlobalTimeWrappedHourly * speed * MathHelper.TwoPi + offset) * 0.5f + 0.5f);
        }

        /// <summary>
        /// Calculates an intensity value that peaks and fades for flash effects.
        /// </summary>
        public static float GetFlashIntensity(int currentTime, int totalTime, float peakTime = 0.3f)
        {
            float progress = (float)currentTime / totalTime;
            if (progress < peakTime)
                return progress / peakTime;
            else
                return 1f - (progress - peakTime) / (1f - peakTime);
        }

        #endregion

        #region Line Drawing

        /// <summary>
        /// Draws a line efficiently using a scaled texture.
        /// Much more efficient than Utils.DrawLine.
        /// </summary>
        public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 2f)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float rotation = direction.ToRotation();

            // Use magic pixel stretched to make a line
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, start - Main.screenPosition,
                new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f),
                new Vector2(length, thickness), SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a glowing line with inner and outer colors.
        /// </summary>
        public static void DrawGlowingLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, 
            Color innerColor, Color outerColor, float innerThickness = 2f, float outerThickness = 6f)
        {
            // Draw outer glow
            DrawLine(spriteBatch, start, end, outerColor * 0.5f, outerThickness);
            // Draw inner core
            DrawLine(spriteBatch, start, end, innerColor, innerThickness);
        }

        #endregion
    }
}
