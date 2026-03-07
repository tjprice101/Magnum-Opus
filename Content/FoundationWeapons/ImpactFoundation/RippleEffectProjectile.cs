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
    /// RippleEffectProjectile — Expanding concentric ring ripple at the impact point.
    /// 
    /// VISUAL ARCHITECTURE:
    /// 1. RIPPLE SHADER (RippleShader.fx) — Draws animated concentric rings that
    ///    expand outward from center using SDF ring math + noise distortion.
    ///    Applied via SpriteBatch pipeline to a large SoftCircle quad.
    /// 2. BLOOM STACKING — Multi-layer additive bloom for impact flash and ambient glow.
    /// 3. DUST — Scattered ring-shaped sparkle particles.
    /// 
    /// COLOR THEMES (via Projectile.ai[0]):
    ///   0 = Default blue/cyan/white (foundation)
    ///   1 = La Campanella infernal orange/gold/warm-white
    ///   2 = Eroica scarlet/gold/white
    /// 
    /// Behaviour:
    /// - Spawns at impact position, does NOT move
    /// - Purely visual (0 damage)
    /// - Rings expand outward over 45 frames then fade out over 15 frames
    /// - Total lifetime: 60 frames (~1 second)
    /// </summary>
    public class RippleEffectProjectile : ModProjectile
    {
        /// <summary>Color theme constants for ai[0].</summary>
        public const float ThemeDefault = 0f;
        public const float ThemeLaCampanella = 1f;
        public const float ThemeEroica = 2f;
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 60;
        private const int FadeOutFrames = 15;
        private const float MaxDrawScale = 0.5f; // Full expansion scale

        private int timer;
        private float seed;
        private Effect rippleShader;

        private static Asset<Texture2D> _gradientLUT;
        private static string _cachedGradientPath;

        /// <summary>
        /// Returns the LUT gradient path for the current theme.
        /// </summary>
        private string GetGradientLUTPath()
        {
            float theme = Projectile.ai[0];
            if (theme >= 0.9f && theme <= 1.1f)
                return "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP";
            if (theme >= 1.9f && theme <= 2.1f)
                return "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientLUTandRAMP";
            return "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP";
        }

        /// <summary>
        /// Returns themed color array based on Projectile.ai[0].
        /// Index 0 = primary, 1 = secondary, 2 = core/highlight.
        /// </summary>
        private Color[] GetThemedColors()
        {
            float theme = Projectile.ai[0];
            if (theme >= 0.9f && theme <= 1.1f) // La Campanella
                return new[] { new Color(255, 110, 15), new Color(255, 185, 50), new Color(255, 250, 220) };
            if (theme >= 1.9f && theme <= 2.1f) // Eroica
                return new[] { new Color(200, 50, 50), new Color(255, 160, 60), new Color(255, 240, 200) };
            return IFTextures.GetModeColors(ImpactMode.Ripple); // Default blue
        }

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
                seed = Main.rand.NextFloat(100f);

            timer++;

            // Lighting
            float progress = timer / (float)MaxLifetime;
            float brightness = 1f - progress;
            Color[] colors = GetThemedColors();
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * brightness * 0.8f);

            // Ring-shaped dust emission — particles spread outward in a circle
            if (timer < MaxLifetime - FadeOutFrames && Main.rand.NextBool(2))
            {
                float ringRadius = progress * 50f; // Expand with the ripple
                float dustAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * ringRadius;
                Vector2 dustVel = dustAngle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            Projectile.velocity = Vector2.Zero;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color[] colors = GetThemedColors();

            float progress = timer / (float)MaxLifetime;
            float fadeAlpha = timer > (MaxLifetime - FadeOutFrames)
                ? (MaxLifetime - timer) / (float)FadeOutFrames
                : MathHelper.Clamp(timer / 5f, 0f, 1f);

            float expandScale = MathHelper.Lerp(0.08f, MaxDrawScale, EaseOutQuad(progress));

            // ---- LAYER 1: INITIAL IMPACT FLASH ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            if (timer < 12)
            {
                float flashAlpha = 1f - (timer / 12f);
                flashAlpha *= flashAlpha; // Quadratic falloff for sharp flash

                Texture2D softGlow = IFTextures.SoftGlow.Value;
                Vector2 glowOrigin = softGlow.Size() / 2f;

                // Bright center flash
                sb.Draw(softGlow, drawPos, null, colors[2] * (flashAlpha * 0.8f),
                    0f, glowOrigin, 0.12f, SpriteEffects.None, 0f);
                // Wider dim flash
                sb.Draw(softGlow, drawPos, null, colors[0] * (flashAlpha * 0.4f),
                    0f, glowOrigin, 0.25f, SpriteEffects.None, 0f);
            }

            // ---- LAYER 2: SHADER-DRIVEN RIPPLE RINGS ----
            DrawShaderRipple(sb, drawPos, colors, progress, fadeAlpha, expandScale);

            // ---- LAYER 3: SOFT OUTER BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D glowOrb = IFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;
            float outerPulse = 0.9f + 0.1f * MathF.Sin(timer * 0.2f);

            sb.Draw(glowOrb, drawPos, null, colors[0] * (0.15f * fadeAlpha * outerPulse),
                0f, orbOrigin, expandScale * 0.5f, SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws concentric expanding rings using the RippleShader.
        /// </summary>
        private void DrawShaderRipple(SpriteBatch sb, Vector2 drawPos, Color[] colors,
            float progress, float fadeAlpha, float expandScale)
        {
            float time = (float)Main.timeForVisualEffects;

            // ---- LOAD SHADER ----
            if (rippleShader == null)
            {
                rippleShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER ----
            rippleShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            rippleShader.Parameters["progress"]?.SetValue(progress);
            rippleShader.Parameters["ringCount"]?.SetValue(4f);
            rippleShader.Parameters["ringThickness"]?.SetValue(0.04f);
            rippleShader.Parameters["primaryColor"]?.SetValue(colors[0].ToVector3());
            rippleShader.Parameters["secondaryColor"]?.SetValue(colors[1].ToVector3());
            rippleShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3());
            rippleShader.Parameters["fadeAlpha"]?.SetValue(fadeAlpha);

            // Provide noise texture for subtle ring distortion
            rippleShader.Parameters["noiseTex"]?.SetValue(IFTextures.NoisePerlin.Value);

            // Bind theme-specific LUT gradient for color ramping
            string lutPath = GetGradientLUTPath();
            if (_gradientLUT == null || _cachedGradientPath != lutPath)
            {
                _gradientLUT = ModContent.Request<Texture2D>(lutPath);
                _cachedGradientPath = lutPath;
            }
            rippleShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            rippleShader.Parameters["useGradient"]?.SetValue(1f);

            // ---- DRAW WITH SHADER ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, rippleShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = IFTextures.SoftCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * fadeAlpha,
                0f, circleOrigin, expandScale, SpriteEffects.None, 0f);

            // End shader batch
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
