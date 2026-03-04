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
    /// TidalXSlashEffect — A blazing tidal X-shaped cross rendered at the detonation point.
    /// Adapted from XSlashFoundation's XSlashEffect with lunar-themed re-coloring.
    /// 
    /// VISUAL ARCHITECTURE (Foundation: XSlashFoundation + XSlashShader):
    /// 1. BLOOM FOUNDATION — Multi-scale additive glow behind the X
    /// 2. BLAZING X SHADER — X-ShapedImpactCross with noise-driven tidal distortion,
    ///    UV scrolling along both arms, gradient LUT coloring (purple → blue → white foam)
    /// 3. DIRECTIONAL ARM BLOOM — Elongated glow along each arm
    /// 4. CENTER FLASH — Bright starburst at the intersection
    /// 5. EMBER DUST — Tidal sparks along both diagonal axes
    /// 
    /// ai[0] = impact angle
    /// ai[1] = tidal phase multiplier
    /// 
    /// Spawned by: EternalMoonTidalDetonation on first frame
    /// </summary>
    public class TidalXSlashEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 50;
        private const int FadeInFrames = 6;
        private const int HoldFrames = 16;
        private const int FadeOutFrames = 28;
        private const float BaseShaderDrawScale = 0.22f;

        // Lunar tidal palette
        private static readonly Color OuterColor = new(75, 0, 130);       // DarkPurple
        private static readonly Color MainColor = new(135, 206, 250);     // IceBlue
        private static readonly Color CoreColor = new(230, 235, 255);     // MoonWhite
        private static readonly Color FoamColor = new(170, 225, 255);     // CrescentGlow

        private int timer;
        private float seed;
        private float impactAngle;
        private float tidalMult;
        private Effect xSlashShader;

        // Shared texture references
        private static Asset<Texture2D> _xImpactCross;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _noiseFBM;
        private static Asset<Texture2D> _gradientLUT;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 800;
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
                tidalMult = MathHelper.Clamp(Projectile.ai[1], 1f, 2f);
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            float alpha = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, MainColor.ToVector3() * alpha * 0.8f * tidalMult);

            // Tidal ember dust along both X arms
            if (timer <= FadeInFrames + HoldFrames && Main.rand.NextBool(2))
            {
                float arm1Angle = impactAngle + MathHelper.PiOver4;
                float arm2Angle = impactAngle - MathHelper.PiOver4;
                float chosenArm = Main.rand.NextBool() ? arm1Angle : arm2Angle;

                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 dustVel = chosenArm.ToRotationVector2() * Main.rand.NextFloat(2f, 6f) * side * tidalMult;

                float armOffset = Main.rand.NextFloat(-30f, 30f) * tidalMult;
                Vector2 dustPos = Projectile.Center + chosenArm.ToRotationVector2() * armOffset;

                Color col = Color.Lerp(MainColor, FoamColor, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f));
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
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: BLOOM FOUNDATION ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawBloomFoundation(sb, drawPos, alpha);

            // ---- LAYER 2: BLAZING TIDAL X VIA SHADER ----
            DrawShaderX(sb, drawPos, alpha);

            // ---- LAYER 3: DIRECTIONAL ARM BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawArmBloom(sb, drawPos, alpha);

            // ---- LAYER 4: CENTER FLASH ----
            DrawCenterFlash(sb, drawPos, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawBloomFoundation(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            Texture2D softGlow = _softGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f + seed);
            float scaleMult = tidalMult * 1.2f;

            // Wide outer halo — DarkPurple
            sb.Draw(softGlow, drawPos, null, OuterColor * (0.18f * alpha * pulse),
                0f, glowOrigin, 0.25f * pulse * scaleMult, SpriteEffects.None, 0f);

            // Mid glow — IceBlue
            sb.Draw(softGlow, drawPos, null, MainColor * (0.22f * alpha * pulse),
                0f, glowOrigin, 0.13f * pulse * scaleMult, SpriteEffects.None, 0f);

            // Core — MoonWhite
            sb.Draw(softGlow, drawPos, null, CoreColor * (0.15f * alpha * pulse),
                0f, glowOrigin, 0.07f * pulse * scaleMult, SpriteEffects.None, 0f);
        }

        private void DrawShaderX(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            if (xSlashShader == null)
            {
                xSlashShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/XSlashFoundation/Shaders/XSlashShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Water-like UV flow (slower, gentler than fire)
            xSlashShader.Parameters["uTime"]?.SetValue(time * 0.015f + seed);
            xSlashShader.Parameters["edgeColor"]?.SetValue(OuterColor.ToVector3());
            xSlashShader.Parameters["midColor"]?.SetValue(MainColor.ToVector3());
            xSlashShader.Parameters["coreColor"]?.SetValue(CoreColor.ToVector3());
            xSlashShader.Parameters["fadeAlpha"]?.SetValue(alpha);
            xSlashShader.Parameters["fireIntensity"]?.SetValue(0.08f);   // Doc: 0.08 for water-like
            xSlashShader.Parameters["scrollSpeed"]?.SetValue(0.4f);       // Doc: 0.4 for tidal

            _noiseFBM ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise");
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP");

            xSlashShader.Parameters["noiseTex"]?.SetValue(_noiseFBM.Value);
            xSlashShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, xSlashShader,
                Main.GameViewMatrix.TransformationMatrix);

            _xImpactCross ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/X-ShapedImpactCross");
            Texture2D xTex = _xImpactCross.Value;
            Vector2 xOrigin = xTex.Size() / 2f;

            // Doc: ShaderDrawScale = 0.28f for massive detonation
            float drawScale = BaseShaderDrawScale * tidalMult * 1.25f;
            float scaleBoost = timer < FadeInFrames ? 1f + (1f - timer / (float)FadeInFrames) * 0.35f : 1f;

            sb.Draw(xTex, drawPos, null, Color.White * alpha,
                impactAngle, xOrigin, drawScale * scaleBoost, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawArmBloom(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            Texture2D softGlow = _softGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float arm1Angle = impactAngle + MathHelper.PiOver4;
            float arm2Angle = impactAngle - MathHelper.PiOver4;
            float armScale = tidalMult * 1.1f;

            // Wide outer glow along arms — IceBlue
            sb.Draw(softGlow, drawPos, null, MainColor * (0.22f * alpha),
                arm1Angle, glowOrigin, new Vector2(0.12f * armScale, 0.012f), SpriteEffects.None, 0f);
            sb.Draw(softGlow, drawPos, null, MainColor * (0.22f * alpha),
                arm2Angle, glowOrigin, new Vector2(0.12f * armScale, 0.012f), SpriteEffects.None, 0f);

            // Tight core along arms — MoonWhite
            sb.Draw(softGlow, drawPos, null, CoreColor * (0.3f * alpha),
                arm1Angle, glowOrigin, new Vector2(0.07f * armScale, 0.005f), SpriteEffects.None, 0f);
            sb.Draw(softGlow, drawPos, null, CoreColor * (0.3f * alpha),
                arm2Angle, glowOrigin, new Vector2(0.07f * armScale, 0.005f), SpriteEffects.None, 0f);
        }

        private void DrawCenterFlash(SpriteBatch sb, Vector2 drawPos, float alpha)
        {
            if (timer > 20) return;

            float flashProgress = timer / 20f;
            float flashAlpha = (1f - flashProgress * flashProgress) * alpha;

            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare");
            Texture2D starFlare = _starFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;

            // Primary starburst aligned to impact
            sb.Draw(starFlare, drawPos, null, CoreColor * (flashAlpha * 0.55f),
                impactAngle, flareOrigin, 0.1f * tidalMult * (1f + flashProgress * 0.4f), SpriteEffects.None, 0f);

            // Secondary rotated starburst
            sb.Draw(starFlare, drawPos, null, FoamColor * (flashAlpha * 0.35f),
                impactAngle + MathHelper.PiOver4, flareOrigin, 0.06f * tidalMult, SpriteEffects.None, 0f);

            // Bright center point
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            Texture2D pointBloom = _pointBloom.Value;
            Vector2 bloomOrigin = pointBloom.Size() / 2f;
            sb.Draw(pointBloom, drawPos, null, CoreColor * (flashAlpha * 0.45f),
                0f, bloomOrigin, 0.05f * tidalMult, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
