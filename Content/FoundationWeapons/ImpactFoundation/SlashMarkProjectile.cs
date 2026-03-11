using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ImpactFoundation
{
    /// <summary>
    /// SlashMarkProjectile — A fluid directional slash mark rendered at the impact point.
    /// 
    /// VISUAL ARCHITECTURE:
    /// 1. SLASH SHADER (SlashMarkShader.fx) — Draws a directional slash arc using
    ///    SDF math with noise distortion for organic, fluid edges. The slash
    ///    appears to carve through space with a hot bright core and cooler outer glow.
    /// 2. BLOOM LAYERS — Directional glow along the slash direction and center point bloom.
    /// 3. SLASH DUST — Directional sparkle particles flying outward along the slash.
    /// 
    /// Behaviour:
    /// - Spawns at impact, does NOT move
    /// - Purely visual (0 damage)
    /// - ai[0] = slash angle (direction the projectile was traveling on hit)
    /// - Quick appearance (10 frame fade in), holds briefly, then fades out
    /// - Total lifetime: 50 frames
    /// </summary>
    public class SlashMarkProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 50;
        private const int FadeInFrames = 8;
        private const int HoldFrames = 20;
        private const int FadeOutFrames = 22;
        private const float DrawScale = 0.35f;

        private int timer;
        private float seed;
        private float slashAngle; // Direction of the slash
        private Effect slashShader;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxLifetime;
            Projectile.hide = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                slashAngle = Projectile.ai[0]; // Angle passed from ImpactProjectile
            }

            timer++;

            Projectile.velocity = Vector2.Zero;

            // Lighting along slash direction
            Color[] colors = IFTextures.GetModeColors(ImpactMode.SlashMark);
            float alpha = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * alpha * 0.6f);

            // Directional slash dust — particles fly out along the slash angle
            if (timer < FadeInFrames + HoldFrames && Main.rand.NextBool(2))
            {
                float dustAngle = slashAngle + Main.rand.NextFloat(-0.4f, 0.4f);
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * side;
                Vector2 dustPos = Projectile.Center + dustVel.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(5f, 30f);

                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        private float GetAlphaMultiplier()
        {
            if (timer < FadeInFrames)
                return timer / (float)FadeInFrames;
            else if (timer < FadeInFrames + HoldFrames)
                return 1f;
            else
                return MathHelper.Clamp((MaxLifetime - timer) / (float)FadeOutFrames, 0f, 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color[] colors = IFTextures.GetModeColors(ImpactMode.SlashMark);
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: DIRECTIONAL BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawDirectionalBloom(sb, drawPos, colors, alpha);

            // ---- LAYER 2: SHADER-DRIVEN SLASH MARK ----
            DrawShaderSlash(sb, drawPos, colors, alpha);

            // ---- LAYER 3: CENTER FLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCenterFlash(sb, drawPos, colors, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        /// <summary>
        /// Draws elongated bloom along the slash direction.
        /// </summary>
        private void DrawDirectionalBloom(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            Texture2D softGlow = IFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Draw stretched glow along the slash direction for each side
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 offset = slashAngle.ToRotationVector2() * 15f * side;
                sb.Draw(softGlow, drawPos + offset, null, colors[0] * (0.2f * alpha),
                    0f, glowOrigin, new Vector2(0.12f, 0.04f), SpriteEffects.None, 0f);
            }

            // Central glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.25f * alpha),
                0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the slash mark via SlashMarkShader — SDF-based directional slash arc.
        /// </summary>
        private void DrawShaderSlash(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            // ---- LOAD SHADER ----
            if (slashShader == null)
            {
                slashShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/SlashMarkShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER ----
            slashShader.Parameters["uTime"]?.SetValue(time * 0.015f + seed);
            slashShader.Parameters["slashAngle"]?.SetValue(slashAngle);
            slashShader.Parameters["primaryColor"]?.SetValue(colors[0].ToVector3());
            slashShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3());
            slashShader.Parameters["fadeAlpha"]?.SetValue(alpha);
            slashShader.Parameters["slashWidth"]?.SetValue(0.06f);
            slashShader.Parameters["slashLength"]?.SetValue(0.35f);

            // Noise for fluid/organic edge distortion
            slashShader.Parameters["noiseTex"]?.SetValue(IFTextures.NoiseCosmicVortex.Value);
            slashShader.Parameters["gradientTex"]?.SetValue(IFTextures.GetGradientForMode(ImpactMode.SlashMark));

            // ---- DRAW WITH SHADER ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, slashShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = IFTextures.SoftCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * alpha,
                0f, circleOrigin, DrawScale, SpriteEffects.None, 0f);

            // End shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws a flash at the center of the slash that fades quickly.
        /// </summary>
        private void DrawCenterFlash(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            if (timer > 15) return; // Only show flash early on

            float flashProgress = timer / 15f;
            float flashAlpha = (1f - flashProgress) * alpha;

            Texture2D starFlare = IFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;

            // Directional flare along slash angle
            sb.Draw(starFlare, drawPos, null, colors[2] * (flashAlpha * 0.6f),
                slashAngle, flareOrigin, 0.2f * (1f + flashProgress * 0.5f), SpriteEffects.None, 0f);

            // Perpendicular flare
            sb.Draw(starFlare, drawPos, null, colors[1] * (flashAlpha * 0.3f),
                slashAngle + MathHelper.PiOver2, flareOrigin, 0.1f, SpriteEffects.None, 0f);

            // Core point
            Texture2D glowOrb = IFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;
            sb.Draw(glowOrb, drawPos, null, colors[2] * (flashAlpha * 0.5f),
                0f, orbOrigin, 0.15f, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
