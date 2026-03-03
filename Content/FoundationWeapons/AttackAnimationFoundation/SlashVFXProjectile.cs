using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackAnimationFoundation
{
    /// <summary>
    /// SlashVFXProjectile — Visual-only slash effect that appears at each hit point
    /// during the attack animation.
    ///
    /// Each slash is a bright directional line/arc that flashes at the impact point.
    /// The final slash is larger and triggers additional bloom layers.
    ///
    /// ai[0] = slash angle
    /// ai[1] = packed value: slashIndex + (isFinal ? 100 : 0)
    /// </summary>
    public class SlashVFXProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 30;
        private const int FadeInFrames = 3;
        private const int HoldFrames = 8;
        private const int FadeOutFrames = 19;

        private int timer;
        private float seed;
        private float slashAngle;
        private int slashIndex;
        private bool isFinal;

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
                slashAngle = Projectile.ai[0];

                float packed = Projectile.ai[1];
                isFinal = packed >= 100f;
                slashIndex = (int)(packed - (isFinal ? 100f : 0f));
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            // Lighting
            float alpha = GetAlphaMultiplier();
            Color lightColor = isFinal ? AAFTextures.SlashColors[2] : AAFTextures.SlashColors[1];
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * alpha * 0.8f);

            // Directional dust particles
            if (timer < FadeInFrames + HoldFrames && Main.rand.NextBool(2))
            {
                float dustAngle = slashAngle + Main.rand.NextFloat(-0.3f, 0.3f);
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * side;
                Vector2 dustPos = Projectile.Center + dustVel.SafeNormalize(Vector2.UnitX)
                    * Main.rand.NextFloat(5f, 25f);

                Color col = AAFTextures.SlashColors[Main.rand.Next(3)];
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.7f));
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

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: DIRECTIONAL SLASH BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawSlashBloom(sb, drawPos, alpha);

            // ---- LAYER 2: CENTER IMPACT ----
            DrawCenterImpact(sb, drawPos, alpha);

            // ---- LAYER 3: FINAL SLASH EXTRA LAYERS ----
            if (isFinal)
            {
                DrawFinalSlashExtra(sb, drawPos, alpha);
            }

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws the main directional slash line — a stretched glow along the slash angle.
        /// </summary>
        private void DrawSlashBloom(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            Texture2D softGlow = AAFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float intensityMult = isFinal ? 1.5f : 1f;
            float lengthScale = isFinal ? 0.3f : 0.18f;
            float widthScale = isFinal ? 0.025f : 0.015f;

            // Expanding slash line
            float expand = 1f + (timer / (float)MaxLifetime) * 0.5f;

            // Outer slash glow
            sb.Draw(softGlow, drawPos, null,
                AAFTextures.SlashColors[0] * (alpha * 0.5f * intensityMult),
                slashAngle, glowOrigin,
                new Vector2(lengthScale * expand, widthScale * expand * 1.5f),
                SpriteEffects.None, 0f);

            // Mid slash
            sb.Draw(softGlow, drawPos, null,
                AAFTextures.SlashColors[1] * (alpha * 0.6f * intensityMult),
                slashAngle, glowOrigin,
                new Vector2(lengthScale * expand * 0.7f, widthScale * expand),
                SpriteEffects.None, 0f);

            // Bright core line
            sb.Draw(softGlow, drawPos, null,
                AAFTextures.SlashColors[2] * (alpha * 0.8f * intensityMult),
                slashAngle, glowOrigin,
                new Vector2(lengthScale * expand * 0.4f, widthScale * expand * 0.5f),
                SpriteEffects.None, 0f);

            // Perpendicular accent (cross slash look)
            sb.Draw(softGlow, drawPos, null,
                AAFTextures.SlashColors[0] * (alpha * 0.2f * intensityMult),
                slashAngle + MathHelper.PiOver2, glowOrigin,
                new Vector2(lengthScale * 0.3f, widthScale * 0.8f),
                SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the center impact flash — bright point bloom at the impact center.
        /// </summary>
        private void DrawCenterImpact(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            if (timer > 12) return; // Only show early

            float flashProgress = timer / 12f;
            float flashAlpha = (1f - flashProgress * flashProgress) * alpha;

            // Star flare at center
            Texture2D starFlare = AAFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            float flareScale = 0.15f + flashProgress * 0.1f;

            sb.Draw(starFlare, drawPos, null,
                AAFTextures.SlashColors[2] * (flashAlpha * 0.7f),
                slashAngle, flareOrigin, flareScale, SpriteEffects.None, 0f);

            // Perpendicular flare
            sb.Draw(starFlare, drawPos, null,
                AAFTextures.SlashColors[1] * (flashAlpha * 0.4f),
                slashAngle + MathHelper.PiOver2, flareOrigin, flareScale * 0.6f,
                SpriteEffects.None, 0f);

            // Bright core point
            Texture2D glowOrb = AAFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;
            sb.Draw(glowOrb, drawPos, null,
                Color.White * (flashAlpha * 0.6f),
                0f, orbOrigin, 0.12f + flashProgress * 0.05f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Extra bloom layers for the final slash — bigger, brighter, more dramatic.
        /// </summary>
        private void DrawFinalSlashExtra(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            Texture2D softRadial = AAFTextures.SoftRadialBloom.Value;
            Vector2 radialOrigin = softRadial.Size() / 2f;

            // Large expanding radial bloom
            float expand = 1f + (timer / (float)MaxLifetime) * 2f;
            float radialAlpha = alpha * (1f - timer / (float)MaxLifetime);

            sb.Draw(softRadial, drawPos, null,
                Color.White * (radialAlpha * 0.5f),
                0f, radialOrigin, 0.3f * expand, SpriteEffects.None, 0f);

            sb.Draw(softRadial, drawPos, null,
                AAFTextures.SlashColors[0] * (radialAlpha * 0.4f),
                0f, radialOrigin, 0.5f * expand, SpriteEffects.None, 0f);

            // Lens flare for maximum impact
            Texture2D lensFlare = AAFTextures.LensFlare.Value;
            Vector2 lfOrigin = lensFlare.Size() / 2f;
            float lfAlpha = radialAlpha * 0.6f;

            sb.Draw(lensFlare, drawPos, null,
                Color.White * lfAlpha,
                slashAngle, lfOrigin, 0.25f * expand, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
