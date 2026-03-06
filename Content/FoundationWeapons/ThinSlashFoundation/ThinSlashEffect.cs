using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ThinSlashFoundation
{
    /// <summary>
    /// ThinSlashEffect — A razor-thin slash line rendered at the impact point.
    /// 
    /// VISUAL ARCHITECTURE:
    /// 1. THIN SLASH SHADER (ThinSlashShader.fx) — Renders a very thin, sharp line
    ///    using SDF math. The line is extremely narrow with a bright hot core and
    ///    minimal edge glow, creating a clean sword-cut look.
    /// 2. DIRECTIONAL BLOOM — Elongated glow sprites along the slash direction.
    /// 3. ENDPOINT FLASHES — Small flares at the tips of the slash line.
    /// 4. SPARK DUST — A few tiny sparks fly off perpendicular to the slash.
    /// 
    /// ai[0] = slash angle (incoming projectile direction)
    /// ai[1] = SlashStyle index (determines colors)
    /// 
    /// Timing: Quick 6-frame fade in, brief hold, 20-frame fade out.
    /// Total lifetime: 35 frames (slashes should feel instant and sharp).
    /// </summary>
    public class ThinSlashEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 35;
        private const int FadeInFrames = 5;
        private const int HoldFrames = 8;
        private const int FadeOutFrames = 22;
        private const float ShaderDrawScale = 0.045f;

        private int timer;
        private float seed;
        private float slashAngle;
        private SlashStyle style;
        private Effect thinSlashShader;

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
                style = (SlashStyle)(int)Projectile.ai[1];
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            Color[] colors = TSFTextures.GetStyleColors(style);
            float alpha = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, colors[2].ToVector3() * alpha * 0.5f);

            // ---- SPARSE PERPENDICULAR SPARK DUST ----
            if (timer <= FadeInFrames + HoldFrames && Main.rand.NextBool(3))
            {
                // Sparks fly perpendicular to the slash direction
                float perpAngle = slashAngle + MathHelper.PiOver2;
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 dustVel = perpAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f) * side;

                // Spawn along the slash line
                float slashOffset = Main.rand.NextFloat(-18f, 18f);
                Vector2 dustPos = Projectile.Center + slashAngle.ToRotationVector2() * slashOffset;

                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.15f, 0.35f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
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
            Color[] colors = TSFTextures.GetStyleColors(style);
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: SHADER-DRIVEN THIN SLASH LINE ----
            DrawShaderSlash(sb, drawPos, colors, alpha);

            // ---- LAYER 2: DIRECTIONAL BLOOM ALONG SLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawDirectionalBloom(sb, drawPos, colors, alpha);

            // ---- LAYER 3: ENDPOINT FLASHES ----
            DrawEndpointFlashes(sb, drawPos, colors, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Renders the thin slash line via ThinSlashShader.
        /// The shader draws an extremely narrow, bright, crisp line using SDF math.
        /// </summary>
        private void DrawShaderSlash(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            // ---- LOAD SHADER ----
            if (thinSlashShader == null)
            {
                thinSlashShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ThinSlashFoundation/Shaders/ThinSlashShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER ----
            thinSlashShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            thinSlashShader.Parameters["slashAngle"]?.SetValue(slashAngle);
            thinSlashShader.Parameters["edgeColor"]?.SetValue(colors[0].ToVector3());
            thinSlashShader.Parameters["midColor"]?.SetValue(colors[1].ToVector3());
            thinSlashShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3());
            thinSlashShader.Parameters["fadeAlpha"]?.SetValue(alpha);
            // Very thin line: width of ~0.015 in normalized UV space
            thinSlashShader.Parameters["lineWidth"]?.SetValue(0.018f);
            // Long line: extends most of the quad
            thinSlashShader.Parameters["lineLength"]?.SetValue(0.45f);

            // ---- DRAW WITH SHADER ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, thinSlashShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = TSFTextures.SoftCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * alpha,
                0f, circleOrigin, ShaderDrawScale, SpriteEffects.None, 0f);

            // End shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws a very thin, elongated glow along the slash direction.
        /// Uses a soft glow texture stretched heavily in the slash direction
        /// and compressed perpendicular to it.
        /// </summary>
        private void DrawDirectionalBloom(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            Texture2D softGlow = TSFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Thin elongated glow across the full slash length
            // SoftGlow is 1024px — target ~45px length, ~2px width
            // ScaleX (along slash) is large, ScaleY (perpendicular) is tiny
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.3f * alpha),
                slashAngle, glowOrigin, new Vector2(0.045f, 0.002f), SpriteEffects.None, 0f);

            // Tighter bright center line (~30px × 1px)
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.5f * alpha),
                slashAngle, glowOrigin, new Vector2(0.03f, 0.001f), SpriteEffects.None, 0f);

            // Very faint wide glow for ambient light feel (~50px × 4px)
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.12f * alpha),
                slashAngle, glowOrigin, new Vector2(0.05f, 0.004f), SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws small flare effects at both endpoints of the slash line.
        /// Creates endpoint emphasis, making the line feel like it has entry/exit points.
        /// </summary>
        private void DrawEndpointFlashes(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            if (timer > 18) return; // Only during the early phase

            float flashProgress = timer / 18f;
            float flashAlpha = (1f - flashProgress * flashProgress) * alpha;

            Texture2D pointBloom = TSFTextures.PointBloom.Value;
            Vector2 bloomOrigin = pointBloom.Size() / 2f;
            Texture2D glowOrb = TSFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;

            Vector2 direction = slashAngle.ToRotationVector2();

            // Endpoint offsets — place flashes at the tips of the slash
            float tipDistance = 15f; // Matches the slash visual length roughly
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 tipPos = drawPos + direction * tipDistance * side;

                // Small point bloom (PointBloom is 2160px — target ~10px)
                sb.Draw(pointBloom, tipPos, null, colors[2] * (flashAlpha * 0.35f),
                    0f, bloomOrigin, 0.005f * (1f + flashProgress * 0.3f), SpriteEffects.None, 0f);

                // Tiny glow orb (GlowOrb is 1024px — target ~8px)
                sb.Draw(glowOrb, tipPos, null, colors[1] * (flashAlpha * 0.2f),
                    0f, orbOrigin, 0.008f, SpriteEffects.None, 0f);
            }

            // Center flash — very small and brief
            if (timer < 8)
            {
                float centerFlash = (1f - timer / 8f) * alpha;
                Texture2D starFlare = TSFTextures.StarFlare.Value;
                Vector2 flareOrigin = starFlare.Size() / 2f;

                // StarFlare is 1024px — target ~12px
                sb.Draw(starFlare, drawPos, null, colors[2] * (centerFlash * 0.3f),
                    slashAngle, flareOrigin, 0.012f, SpriteEffects.None, 0f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
