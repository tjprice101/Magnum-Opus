using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// TidalThinSlash — A razor-thin slash line rendered at the impact point.
    /// Adapted from ThinSlashFoundation's ThinSlashEffect with Ice Cyan / Violet Cut
    /// style for the Eternal Moon's tidal identity.
    /// 
    /// VISUAL ARCHITECTURE (Foundation: ThinSlashFoundation + ThinSlashShader):
    /// 1. THIN SLASH SHADER — SDF-based razor line with edge glow
    /// 2. DIRECTIONAL BLOOM — Elongated glow along slash direction
    /// 3. ENDPOINT FLASHES — Small flares at both tips
    /// 4. SPARK DUST — Perpendicular sparks off the cut
    /// 
    /// ai[0] = slash angle
    /// ai[1] = style: 0 = Ice Cyan, 1 = Violet Cut
    /// 
    /// Spawned by: EternalMoonSwing on hit (heavy hits), EternalMoonCrescentSlash
    /// </summary>
    public class TidalThinSlash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 32;
        private const int FadeInFrames = 4;
        private const int HoldFrames = 8;
        private const int FadeOutFrames = 20;
        private const float ShaderDrawScale = 0.05f;

        // Ice Cyan style (default for most tidal slashes)
        private static readonly Color IceCyanEdge = new(75, 0, 130);      // DarkPurple
        private static readonly Color IceCyanMid = new(135, 206, 250);    // IceBlue
        private static readonly Color IceCyanCore = new(230, 235, 255);   // MoonWhite

        // Violet Cut style (for higher phases / crescent slashes)
        private static readonly Color VioletEdge = new(40, 10, 60);       // NightPurple
        private static readonly Color VioletMid = new(138, 43, 226);      // Violet
        private static readonly Color VioletCore = new(180, 150, 255);    // Lavender

        private int timer;
        private float seed;
        private float slashAngle;
        private int slashStyle;
        private Effect thinSlashShader;

        // Shared texture references
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _glowOrb;

        private Color EdgeColor => slashStyle == 0 ? IceCyanEdge : VioletEdge;
        private Color MidColor => slashStyle == 0 ? IceCyanMid : VioletMid;
        private Color CoreColor => slashStyle == 0 ? IceCyanCore : VioletCore;

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
                slashStyle = (int)Projectile.ai[1];
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            float alpha = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, MidColor.ToVector3() * alpha * 0.4f);

            // Sparse perpendicular spark dust
            if (timer <= FadeInFrames + HoldFrames && Main.rand.NextBool(4))
            {
                float perpAngle = slashAngle + MathHelper.PiOver2;
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 dustVel = perpAngle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f) * side;

                float slashOffset = Main.rand.NextFloat(-18f, 18f);
                Vector2 dustPos = Projectile.Center + slashAngle.ToRotationVector2() * slashOffset;

                Color col = Color.Lerp(MidColor, CoreColor, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel,
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
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: SHADER-DRIVEN THIN SLASH LINE ----
            DrawShaderSlash(sb, drawPos, alpha);

            // ---- LAYER 2: DIRECTIONAL BLOOM ALONG SLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawDirectionalBloom(sb, drawPos, alpha);

            // ---- LAYER 3: ENDPOINT FLASHES ----
            DrawEndpointFlashes(sb, drawPos, alpha);

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

        private void DrawShaderSlash(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            if (thinSlashShader == null)
            {
                thinSlashShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ThinSlashFoundation/Shaders/ThinSlashShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Doc: lineWidth = 0.018, lineLength = 0.45 for Eternal Moon
            thinSlashShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            thinSlashShader.Parameters["slashAngle"]?.SetValue(slashAngle);
            thinSlashShader.Parameters["edgeColor"]?.SetValue(EdgeColor.ToVector3());
            thinSlashShader.Parameters["midColor"]?.SetValue(MidColor.ToVector3());
            thinSlashShader.Parameters["coreColor"]?.SetValue(CoreColor.ToVector3());
            thinSlashShader.Parameters["fadeAlpha"]?.SetValue(alpha);
            thinSlashShader.Parameters["lineWidth"]?.SetValue(0.018f);
            thinSlashShader.Parameters["lineLength"]?.SetValue(0.45f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, thinSlashShader,
                Main.GameViewMatrix.TransformationMatrix);

            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle");
            Texture2D circleTex = _softCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * alpha,
                0f, circleOrigin, ShaderDrawScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawDirectionalBloom(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            Texture2D softGlow = _softGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Thin elongated glow along slash axis
            sb.Draw(softGlow, drawPos, null, MidColor * (0.3f * alpha),
                slashAngle, glowOrigin, new Vector2(0.045f, 0.002f), SpriteEffects.None, 0f);

            // Tighter bright center line
            sb.Draw(softGlow, drawPos, null, CoreColor * (0.5f * alpha),
                slashAngle, glowOrigin, new Vector2(0.03f, 0.001f), SpriteEffects.None, 0f);

            // Faint wide glow
            sb.Draw(softGlow, drawPos, null, EdgeColor * (0.12f * alpha),
                slashAngle, glowOrigin, new Vector2(0.05f, 0.004f), SpriteEffects.None, 0f);
        }

        private void DrawEndpointFlashes(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            if (timer > 16) return;

            float flashProgress = timer / 16f;
            float flashAlpha = (1f - flashProgress * flashProgress) * alpha;

            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            _glowOrb ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/GlowOrb");
            Texture2D pointBloom = _pointBloom.Value;
            Vector2 bloomOrigin = pointBloom.Size() / 2f;
            Texture2D glowOrb = _glowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;

            Vector2 direction = slashAngle.ToRotationVector2();

            float tipDistance = 16f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 tipPos = drawPos + direction * tipDistance * side;

                sb.Draw(pointBloom, tipPos, null, CoreColor * (flashAlpha * 0.35f),
                    0f, bloomOrigin, 0.006f * (1f + flashProgress * 0.3f), SpriteEffects.None, 0f);

                sb.Draw(glowOrb, tipPos, null, MidColor * (flashAlpha * 0.2f),
                    0f, orbOrigin, 0.008f, SpriteEffects.None, 0f);
            }

            // Center starburst flash
            if (timer < 7)
            {
                _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");
                float centerFlash = (1f - timer / 7f) * alpha;
                Texture2D starFlare = _starFlare.Value;
                Vector2 flareOrigin = starFlare.Size() / 2f;

                sb.Draw(starFlare, drawPos, null, CoreColor * (centerFlash * 0.3f),
                    slashAngle, flareOrigin, 0.013f, SpriteEffects.None, 0f);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
