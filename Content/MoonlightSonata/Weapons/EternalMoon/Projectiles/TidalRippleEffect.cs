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
    /// TidalRippleEffect — Expanding concentric ring ripple at impact points.
    /// Adapted from ImpactFoundation's RippleEffectProjectile with lunar-themed colors
    /// and parameters matching the Eternal Moon's tidal identity.
    /// 
    /// VISUAL ARCHITECTURE (Foundation: ImpactFoundation + RippleShader):
    /// 1. RIPPLE SHADER — Animated concentric SDF rings expanding outward with noise distortion
    /// 2. BLOOM STACKING — Impact flash + ambient glow
    /// 3. DUST — Ring-shaped sparkle particles matching the expansion
    /// 
    /// ai[0] = tidal phase multiplier (1.0–2.0) from EternalMoonPlayer
    /// 
    /// Spawned by: EternalMoonWave on hit, EternalMoonSwing on heavy hits
    /// </summary>
    public class TidalRippleEffect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 55;
        private const int FadeOutFrames = 15;
        private const float BaseMaxDrawScale = 0.4f;

        // Lunar palette
        private static readonly Color EdgeColor = new(75, 0, 130);       // DarkPurple
        private static readonly Color MidColor = new(135, 206, 250);     // IceBlue
        private static readonly Color CoreColor = new(230, 235, 255);    // MoonWhite

        private int timer;
        private float seed;
        private float tidalMult;
        private Effect rippleShader;

        // Shared texture references
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _gradientLUT;

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
                tidalMult = MathHelper.Clamp(Projectile.ai[0], 1f, 2f);
            }

            timer++;

            float progress = timer / (float)MaxLifetime;
            float brightness = 1f - progress;
            Lighting.AddLight(Projectile.Center, MidColor.ToVector3() * brightness * 0.6f * tidalMult);

            // Ring-shaped dust expanding with the ripple
            if (timer < MaxLifetime - FadeOutFrames && Main.rand.NextBool(3))
            {
                float ringRadius = progress * 45f * tidalMult;
                float dustAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * ringRadius;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                Color col = Color.Lerp(MidColor, CoreColor, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f));
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
                : MathHelper.Clamp(timer / 5f, 0f, 1f);

            float maxScale = BaseMaxDrawScale * tidalMult;
            float expandScale = MathHelper.Lerp(0.06f, maxScale, EaseOutQuad(progress));

            // ---- LAYER 1: IMPACT FLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (timer < 10)
            {
                _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
                Texture2D softGlow = _softGlow.Value;
                Vector2 glowOrigin = softGlow.Size() / 2f;

                float flashAlpha = 1f - (timer / 10f);
                flashAlpha *= flashAlpha;

                sb.Draw(softGlow, drawPos, null, CoreColor * (flashAlpha * 0.7f),
                    0f, glowOrigin, 0.1f * tidalMult, SpriteEffects.None, 0f);
                sb.Draw(softGlow, drawPos, null, MidColor * (flashAlpha * 0.35f),
                    0f, glowOrigin, 0.2f * tidalMult, SpriteEffects.None, 0f);
            }

            // ---- LAYER 2: SHADER-DRIVEN RIPPLE RINGS ----
            DrawShaderRipple(sb, drawPos, progress, fadeAlpha, expandScale);

            // ---- LAYER 3: SOFT OUTER BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            _glowOrb ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/GlowOrb");
            Texture2D glowOrb = _glowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;
            float outerPulse = 0.9f + 0.1f * MathF.Sin(timer * 0.2f);

            sb.Draw(glowOrb, drawPos, null, EdgeColor * (0.12f * fadeAlpha * outerPulse),
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

            // Scale ring count with tidal phase — more rings at higher tide
            int ringCount = 3 + (int)(tidalMult - 1f) * 2;

            rippleShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            rippleShader.Parameters["progress"]?.SetValue(progress);
            rippleShader.Parameters["ringCount"]?.SetValue((float)ringCount);
            rippleShader.Parameters["ringThickness"]?.SetValue(0.05f);
            rippleShader.Parameters["primaryColor"]?.SetValue(EdgeColor.ToVector3());
            rippleShader.Parameters["secondaryColor"]?.SetValue(MidColor.ToVector3());
            rippleShader.Parameters["coreColor"]?.SetValue(CoreColor.ToVector3());
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
