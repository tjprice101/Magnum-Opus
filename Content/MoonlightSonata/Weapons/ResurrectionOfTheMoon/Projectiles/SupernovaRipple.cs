using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Projectiles
{
    /// <summary>
    /// SupernovaRipple — Expanding concentric ring shockwave at the supernova
    /// detonation point. Adapted from ImpactFoundation's RippleEffectProjectile
    /// with the Resurrection's comet palette.
    ///
    /// VISUAL ARCHITECTURE (Foundation: ImpactFoundation + RippleShader):
    /// 1. RIPPLE SHADER — 5 concentric SDF rings expanding outward with noise distortion
    /// 2. IMPACT FLASH — White-hot center flash (CometCoreWhite) fading over 12 frames
    /// 3. OUTER BLOOM — GlowOrb accent at the expansion edge
    ///
    /// Colors: CometCoreWhite center → LunarShine mid rings → DeepSpaceViolet outer edge.
    /// Faster expansion than TidalRippleEffect (75% of MaxLifetime), brighter center.
    /// VFX-only: friendly=false, 0 damage.
    ///
    /// ai[0] = lunar phase multiplier (0.7–1.3) scaling ring count and scale
    /// </summary>
    public class SupernovaRipple : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 50;
        private const int FadeOutFrames = 12;
        private const float BaseMaxDrawScale = 0.55f;

        // Comet palette for supernova shockwave
        private static readonly Color EdgeColor = new(50, 20, 100);      // DeepSpaceViolet
        private static readonly Color MidColor = new(120, 190, 255);     // LunarShine
        private static readonly Color CoreColor = new(210, 225, 255);    // CometCoreWhite

        private int timer;
        private float seed;
        private float lunarMult;
        private Effect rippleShader;

        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _gradientLUT;

        private static readonly string GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 700;
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
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                lunarMult = MathHelper.Clamp(Projectile.ai[0], 0.5f, 2f);
            }

            timer++;

            float progress = timer / (float)MaxLifetime;
            float brightness = 1f - progress;
            Lighting.AddLight(Projectile.Center, CoreColor.ToVector3() * brightness * 0.7f * lunarMult);

            // Shockwave ring dust expanding rapidly
            if (timer < MaxLifetime - FadeOutFrames && Main.rand.NextBool(2))
            {
                float ringRadius = progress * 65f * lunarMult;
                float dustAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * ringRadius;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = Color.Lerp(MidColor, CoreColor, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }

            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float progress = timer / (float)MaxLifetime;
            float fadeAlpha = timer > (MaxLifetime - FadeOutFrames)
                ? (MaxLifetime - timer) / (float)FadeOutFrames
                : MathHelper.Clamp(timer / 4f, 0f, 1f);

            float maxScale = BaseMaxDrawScale * lunarMult;
            float expandScale = MathHelper.Lerp(0.08f, maxScale, EaseOutQuad(progress));

            // ---- LAYER 1: WHITE-HOT IMPACT FLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (timer < 12)
            {
                _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
                Texture2D softGlow = _softGlow.Value;
                Vector2 glowOrigin = softGlow.Size() / 2f;

                float flashAlpha = 1f - (timer / 12f);
                flashAlpha *= flashAlpha;

                // Bright white-hot core
                sb.Draw(softGlow, drawPos, null, CoreColor * (flashAlpha * 0.85f),
                    0f, glowOrigin, 0.06f * lunarMult, SpriteEffects.None, 0f);
                // Blue mid glow
                sb.Draw(softGlow, drawPos, null, MidColor * (flashAlpha * 0.5f),
                    0f, glowOrigin, 0.12f * lunarMult, SpriteEffects.None, 0f);
                // Wide purple fringe (SoftGlow 1024px — cap to 300px max)
                sb.Draw(softGlow, drawPos, null, EdgeColor * (flashAlpha * 0.2f),
                    0f, glowOrigin, MathHelper.Min(0.2f * lunarMult, 0.293f), SpriteEffects.None, 0f);
            }

            // ---- LAYER 2: SHADER-DRIVEN RIPPLE RINGS ----
            DrawShaderRipple(sb, drawPos, progress, fadeAlpha, expandScale);

            // ---- LAYER 3: OUTER BLOOM EDGE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            _glowOrb ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/GlowOrb");
            Texture2D glowOrb = _glowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;

            float outerPulse = 0.85f + 0.15f * MathF.Sin(timer * 0.25f);
            sb.Draw(glowOrb, drawPos, null, EdgeColor * (0.15f * fadeAlpha * outerPulse),
                0f, orbOrigin, MathHelper.Min(expandScale * 0.55f, 0.293f), SpriteEffects.None, 0f);

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

        private void DrawShaderRipple(SpriteBatch sb, Vector2 drawPos,
            float progress, float fadeAlpha, float expandScale)
        {
            float time = (float)Main.timeForVisualEffects;

            if (rippleShader == null)
            {
                rippleShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // 5 rings base, +2 at high lunar phase
            int ringCount = 5 + (int)((lunarMult - 1f) * 3f);

            rippleShader.Parameters["uTime"]?.SetValue(time * 0.025f + seed);
            rippleShader.Parameters["progress"]?.SetValue(progress);
            rippleShader.Parameters["ringCount"]?.SetValue((float)ringCount);
            rippleShader.Parameters["ringThickness"]?.SetValue(0.05f);
            rippleShader.Parameters["primaryColor"]?.SetValue(EdgeColor.ToVector3());
            rippleShader.Parameters["secondaryColor"]?.SetValue(MidColor.ToVector3());
            rippleShader.Parameters["coreColor"]?.SetValue(CoreColor.ToVector3());
            rippleShader.Parameters["fadeAlpha"]?.SetValue(fadeAlpha);

            _noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise");
            rippleShader.Parameters["noiseTex"]?.SetValue(_noisePerlin.Value);

            // Bind Moonlight Sonata LUT gradient for theme-consistent ring coloring
            _gradientLUT ??= ModContent.Request<Texture2D>(GradientLUTPath);
            rippleShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            rippleShader.Parameters["useGradient"]?.SetValue(1f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, rippleShader,
                Main.GameViewMatrix.TransformationMatrix);

            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle");
            Texture2D circleTex = _softCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * fadeAlpha,
                0f, circleOrigin, expandScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
