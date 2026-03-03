using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.XSlashFoundation
{
    /// <summary>
    /// XSlashEffect — A blazing X-shaped impact rendered at the hit point.
    /// 
    /// VISUAL ARCHITECTURE:
    /// 1. BLAZING X SHADER (XSlashShader.fx) — Renders the X-ShapedImpactCross texture
    ///    with noise-driven fire distortion, UV scrolling along both arms of the X,
    ///    and gradient LUT coloring. Creates a dynamic, burning cross-slash effect.
    /// 2. BLOOM FOUNDATION — Multi-scale additive glow behind the X for ambient light.
    /// 3. DIRECTIONAL ARM BLOOM — Elongated glow along each arm of the X.
    /// 4. CENTER FLASH — Bright starburst at the intersection point.
    /// 5. EMBER DUST — Sparks flying outward along both diagonal axes.
    /// 
    /// ai[0] = impact angle (incoming projectile direction)
    /// ai[1] = XSlashStyle index (determines colors/gradient)
    /// 
    /// Timing: 6-frame fade in, 15-frame hold, 25-frame fade out.
    /// Total lifetime: 46 frames.
    /// </summary>
    public class XSlashEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 46;
        private const int FadeInFrames = 6;
        private const int HoldFrames = 15;
        private const int FadeOutFrames = 25;
        private const float ShaderDrawScale = 0.22f;

        private int timer;
        private float seed;
        private float impactAngle;
        private XSlashStyle style;
        private Effect xSlashShader;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
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
                impactAngle = Projectile.ai[0];
                style = (XSlashStyle)(int)Projectile.ai[1];
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            Color[] colors = XSFTextures.GetStyleColors(style);
            float alpha = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, colors[2].ToVector3() * alpha * 0.7f);

            // ---- EMBER DUST ALONG BOTH X ARMS ----
            if (timer <= FadeInFrames + HoldFrames && Main.rand.NextBool(2))
            {
                // X has two arms: 45° offset from impact angle
                float arm1Angle = impactAngle + MathHelper.PiOver4;
                float arm2Angle = impactAngle - MathHelper.PiOver4;
                float chosenArm = Main.rand.NextBool() ? arm1Angle : arm2Angle;

                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 dustVel = chosenArm.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * side;

                float armOffset = Main.rand.NextFloat(-25f, 25f);
                Vector2 dustPos = Projectile.Center + chosenArm.ToRotationVector2() * armOffset;

                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.5f));
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color[] colors = XSFTextures.GetStyleColors(style);
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: BLOOM FOUNDATION ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawBloomFoundation(sb, drawPos, colors, alpha);

            // ---- LAYER 2: BLAZING X VIA SHADER ----
            DrawShaderX(sb, drawPos, colors, alpha);

            // ---- LAYER 3: DIRECTIONAL ARM BLOOM + CENTER FLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawArmBloom(sb, drawPos, colors, alpha);
            DrawCenterFlash(sb, drawPos, colors, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawBloomFoundation(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            Texture2D softGlow = XSFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f + seed);

            // Wide ambient glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.15f * alpha * pulse),
                0f, glowOrigin, 0.2f * pulse, SpriteEffects.None, 0f);

            // Mid glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.2f * alpha * pulse),
                0f, glowOrigin, 0.1f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Renders the blazing X using the XSlashShader applied to the X-ShapedImpactCross texture.
        /// The shader applies noise-based fire distortion, UV scrolling, and gradient LUT coloring
        /// to make the X appear to burn and flow with energy.
        /// </summary>
        private void DrawShaderX(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            // ---- LOAD SHADER ----
            if (xSlashShader == null)
            {
                xSlashShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/XSlashFoundation/Shaders/XSlashShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER ----
            xSlashShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            xSlashShader.Parameters["edgeColor"]?.SetValue(colors[0].ToVector3());
            xSlashShader.Parameters["midColor"]?.SetValue(colors[1].ToVector3());
            xSlashShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3());
            xSlashShader.Parameters["fadeAlpha"]?.SetValue(alpha);
            xSlashShader.Parameters["fireIntensity"]?.SetValue(0.06f);
            xSlashShader.Parameters["scrollSpeed"]?.SetValue(0.3f);
            xSlashShader.Parameters["noiseTex"]?.SetValue(XSFTextures.NoiseFBM.Value);
            xSlashShader.Parameters["gradientTex"]?.SetValue(XSFTextures.GetGradientForStyle(style));

            // ---- DRAW WITH SHADER ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, xSlashShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D xTex = XSFTextures.XImpactCross.Value;
            Vector2 xOrigin = xTex.Size() / 2f;

            // Slight scale-up during fade-in for a punchy entrance
            float scaleBoost = timer < FadeInFrames ? 1f + (1f - timer / (float)FadeInFrames) * 0.3f : 1f;

            sb.Draw(xTex, drawPos, null, Color.White * alpha,
                impactAngle, xOrigin, ShaderDrawScale * scaleBoost, SpriteEffects.None, 0f);

            // End shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws elongated glow along both arms of the X for directional emphasis.
        /// </summary>
        private void DrawArmBloom(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            Texture2D softGlow = XSFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float arm1Angle = impactAngle + MathHelper.PiOver4;
            float arm2Angle = impactAngle - MathHelper.PiOver4;

            // Arm 1 — elongated glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.2f * alpha),
                arm1Angle, glowOrigin, new Vector2(0.1f, 0.01f), SpriteEffects.None, 0f);

            // Arm 2 — elongated glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.2f * alpha),
                arm2Angle, glowOrigin, new Vector2(0.1f, 0.01f), SpriteEffects.None, 0f);

            // Tighter core lines on each arm
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.3f * alpha),
                arm1Angle, glowOrigin, new Vector2(0.06f, 0.004f), SpriteEffects.None, 0f);

            sb.Draw(softGlow, drawPos, null, colors[2] * (0.3f * alpha),
                arm2Angle, glowOrigin, new Vector2(0.06f, 0.004f), SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws a bright starburst at the X intersection point.
        /// </summary>
        private void DrawCenterFlash(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            if (timer > 18) return;

            float flashProgress = timer / 18f;
            float flashAlpha = (1f - flashProgress * flashProgress) * alpha;

            Texture2D starFlare = XSFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;

            // Main starburst at impact angle
            sb.Draw(starFlare, drawPos, null, colors[2] * (flashAlpha * 0.5f),
                impactAngle, flareOrigin, 0.08f * (1f + flashProgress * 0.4f), SpriteEffects.None, 0f);

            // Cross flare at 45° offset
            sb.Draw(starFlare, drawPos, null, colors[1] * (flashAlpha * 0.3f),
                impactAngle + MathHelper.PiOver4, flareOrigin, 0.05f, SpriteEffects.None, 0f);

            // Core point bloom
            Texture2D pointBloom = XSFTextures.PointBloom.Value;
            Vector2 bloomOrigin = pointBloom.Size() / 2f;
            sb.Draw(pointBloom, drawPos, null, colors[2] * (flashAlpha * 0.4f),
                0f, bloomOrigin, 0.04f, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
