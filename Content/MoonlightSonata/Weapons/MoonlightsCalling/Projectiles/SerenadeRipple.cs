using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities.SerenadeUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles
{
    /// <summary>
    /// SerenadeRipple — Expanding concentric ring ripple at bounce points.
    /// Adapted from ImpactFoundation's RippleEffectProjectile with prismatic
    /// spectral colors matching the Moonlight's Calling identity.
    ///
    /// VISUAL ARCHITECTURE (Foundation: ImpactFoundation + RippleShader):
    /// 1. RIPPLE SHADER — Animated concentric SDF rings expanding with noise distortion
    /// 2. IMPACT FLASH — Spectral flash fading over 8 frames
    /// 3. OUTER BLOOM — GlowOrb accent at expansion edge
    ///
    /// Colors shift through the prismatic spectrum based on bounce count.
    /// Ring count scales with bounce count (more bounces = more refractive rings).
    /// VFX-only: friendly=false, 0 damage.
    ///
    /// ai[0] = bounce count (0-5) for color and ring scaling
    /// </summary>
    public class SerenadeRipple : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 45;
        private const int FadeOutFrames = 12;
        private const float BaseMaxDrawScale = 0.35f;

        private int timer;
        private float seed;
        private int bounceCount;
        private Effect rippleShader;

        // Prismatic palette — shifts based on bounce count
        private Color EdgeColor => GetSpectralColor(bounceCount % SpectralColors.Length);
        private Color MidColor => GetBeamGradient(bounceCount / 5f);
        private static readonly Color CoreColor = MoonWhite;

        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _gradientLUT;

        private static readonly string GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP";

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
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                bounceCount = (int)MathHelper.Clamp(Projectile.ai[0], 0, 5);
            }

            timer++;

            float progress = timer / (float)MaxLifetime;
            float brightness = 1f - progress;
            Lighting.AddLight(Projectile.Center, MidColor.ToVector3() * brightness * 0.5f);

            // Spectral dust ring expanding with the ripple
            if (timer < MaxLifetime - FadeOutFrames && Main.rand.NextBool(3))
            {
                float ringRadius = progress * 40f * (1f + bounceCount * 0.15f);
                float dustAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * ringRadius;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: GetSpectralColor(Main.rand.Next(7)), Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float progress = timer / (float)MaxLifetime;
            float fadeAlpha = timer > (MaxLifetime - FadeOutFrames)
                ? (MaxLifetime - timer) / (float)FadeOutFrames
                : MathHelper.Clamp(timer / 4f, 0f, 1f);

            float scaleMult = 1f + bounceCount * 0.12f;
            float maxScale = BaseMaxDrawScale * scaleMult;
            float expandScale = MathHelper.Lerp(0.06f, maxScale, EaseOutQuad(progress));

            // ---- LAYER 1: SPECTRAL FLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (timer < 8)
            {
                _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
                Texture2D softGlow = _softGlow.Value;
                Vector2 glowOrigin = softGlow.Size() / 2f;
                float flashAlpha = 1f - (timer / 8f);
                flashAlpha *= flashAlpha;

                sb.Draw(softGlow, drawPos, null, CoreColor * (flashAlpha * 0.6f),
                    0f, glowOrigin, 0.1f * scaleMult, SpriteEffects.None, 0f);
                sb.Draw(softGlow, drawPos, null, MidColor * (flashAlpha * 0.3f),
                    0f, glowOrigin, 0.18f * scaleMult, SpriteEffects.None, 0f);
            }

            // ---- LAYER 2: SHADER-DRIVEN RIPPLE RINGS ----
            DrawShaderRipple(sb, drawPos, progress, fadeAlpha, expandScale);

            // ---- LAYER 3: OUTER BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            _glowOrb ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/GlowOrb");
            Texture2D orbTex = _glowOrb.Value;
            Vector2 orbOrigin = orbTex.Size() / 2f;
            sb.Draw(orbTex, drawPos, null, EdgeColor * (0.1f * fadeAlpha),
                0f, orbOrigin, expandScale * 0.5f, SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

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

            // Ring count scales with bounce count — more refractive rings at higher bounces
            int ringCount = 3 + bounceCount;

            rippleShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            rippleShader.Parameters["progress"]?.SetValue(progress);
            rippleShader.Parameters["ringCount"]?.SetValue((float)ringCount);
            rippleShader.Parameters["ringThickness"]?.SetValue(0.04f);
            rippleShader.Parameters["primaryColor"]?.SetValue(EdgeColor.ToVector3());
            rippleShader.Parameters["secondaryColor"]?.SetValue(MidColor.ToVector3());
            rippleShader.Parameters["coreColor"]?.SetValue(CoreColor.ToVector3());

            // Bind Moonlight Sonata LUT gradient for theme-consistent ring coloring
            _gradientLUT ??= ModContent.Request<Texture2D>(GradientLUTPath);
            rippleShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            rippleShader.Parameters["useGradient"]?.SetValue(1f);
            rippleShader.Parameters["fadeAlpha"]?.SetValue(fadeAlpha);

            _noisePerlin ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise");
            rippleShader.Parameters["noiseTex"]?.SetValue(_noisePerlin.Value);

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
