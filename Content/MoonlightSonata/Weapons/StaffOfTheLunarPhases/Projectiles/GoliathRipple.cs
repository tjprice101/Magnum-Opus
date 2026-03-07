using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Projectiles
{
    /// <summary>
    /// GoliathRipple — Expanding concentric ring ripple at beam impact points.
    /// Adapted from ImpactFoundation's RippleEffectProjectile with lunar phase
    /// coloring matching the Goliath's cosmic/cast palette.
    ///
    /// VISUAL ARCHITECTURE (Foundation: ImpactFoundation + RippleShader):
    /// 1. RIPPLE SHADER — Animated concentric SDF rings expanding with noise distortion
    /// 2. IMPACT FLASH — Cosmic flash fading over 10 frames
    /// 3. OUTER BLOOM — GlowOrb accent at expansion edge
    ///
    /// Colors shift based on lunar phase (ai[0]):
    /// 0=New Moon (void dark), 1=Waxing (purple), 2=Full Moon (ice blue), 3=Waning (lavender)
    /// Devastating beam hits (ai[1]=1) spawn larger, more intense ripples.
    ///
    /// VFX-only: friendly=false, 0 damage.
    ///
    /// ai[0] = lunar phase mode (0-3)
    /// ai[1] = devastating flag (0 or 1)
    /// </summary>
    public class GoliathRipple : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 50;

        // Texture paths
        private static readonly string BloomPath = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string MaskPath = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string NoisePath = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";

        private int timer;
        private float seed;
        private Effect rippleShader;

        // Cached textures
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _noisePerlin;
        private static Asset<Texture2D> _gradientLUT;

        private static readonly string GradientLUTPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/MoonlightSonataGradientLUTandRAMP";

        private int LunarPhase => (int)Projectile.ai[0];
        private bool IsDevastating => Projectile.ai[1] >= 0.5f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = MaxLifetime;
            Projectile.hide = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
                EnsureTextures();
            }

            timer++;
            Projectile.velocity = Vector2.Zero;

            // Phase-colored lighting
            Color lightCol = GetPhaseColor(0.6f);
            float lightIntensity = (1f - timer / (float)MaxLifetime) * (IsDevastating ? 0.8f : 0.5f);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * lightIntensity);
        }

        private void EnsureTextures()
        {
            _softGlow ??= ModContent.Request<Texture2D>(BloomPath + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _glowOrb ??= ModContent.Request<Texture2D>(BloomPath + "GlowOrb", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>(MaskPath + "SoftCircle", AssetRequestMode.ImmediateLoad);
            _noisePerlin ??= ModContent.Request<Texture2D>(NoisePath + "PerlinNoise", AssetRequestMode.ImmediateLoad);
        }

        /// <summary>
        /// Get a phase-tinted color along the cosmic gradient.
        /// Blends the Goliath cosmic palette with the current lunar phase color.
        /// </summary>
        private Color GetPhaseColor(float intensity)
        {
            Color cosmic = GoliathUtils.GetCosmicGradient(intensity);
            Color phase = GoliathPlayer.LunarPhaseColors[Math.Clamp(LunarPhase, 0, 3)];
            return Color.Lerp(cosmic, phase, 0.4f);
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float t = timer / (float)MaxLifetime;
            float expansionEase = 1f - MathF.Pow(2f, -10f * t); // expo ease-out
            float alpha = (1f - t) * (1f - t); // quadratic fade-out

            float devastatingMult = IsDevastating ? 1.4f : 1f;

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- LAYER 1: IMPACT FLASH (first 10 frames) ----
            if (timer < 10)
            {
                DrawImpactFlash(sb, drawPos, t, devastatingMult);
            }

            // ---- LAYER 2: RIPPLE SHADER ----
            DrawRippleShader(sb, drawPos, t, expansionEase, alpha, devastatingMult);

            // ---- LAYER 3: OUTER BLOOM ----
            DrawOuterBloom(sb, drawPos, expansionEase, alpha, devastatingMult);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawImpactFlash(SpriteBatch sb, Vector2 drawPos, float t, float devMult)
        {
            float flashAlpha = 1f - t * 10f; // fades over first 10 frames
            Texture2D glow = _softGlow.Value;
            Vector2 origin = glow.Size() / 2f;

            Color phaseColor = GetPhaseColor(0.8f);
            Color coreColor = GoliathUtils.StarCore;

            // Core flash — white-hot (SoftGlow 1024px — cap to 300px max)
            sb.Draw(glow, drawPos, null, coreColor * (0.7f * flashAlpha * devMult), 0f,
                origin, MathHelper.Min(0.3f * devMult, 0.293f), SpriteEffects.None, 0f);

            // Mid flash — ice blue
            sb.Draw(glow, drawPos, null, GoliathUtils.IceBlueBrilliance * (0.5f * flashAlpha * devMult), 0f,
                origin, MathHelper.Min(0.5f * devMult, 0.293f), SpriteEffects.None, 0f);

            // Outer flash — phase-tinted
            sb.Draw(glow, drawPos, null, phaseColor * (0.3f * flashAlpha * devMult), 0f,
                origin, MathHelper.Min(0.7f * devMult, 0.293f), SpriteEffects.None, 0f);
        }

        private void DrawRippleShader(SpriteBatch sb, Vector2 drawPos, float t,
            float expansion, float alpha, float devMult)
        {
            // Load shader
            if (rippleShader == null)
            {
                rippleShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Ring count: 4 base + phase scaling, devastating = more rings
            int ringCount = 4 + LunarPhase + (IsDevastating ? 2 : 0);
            float drawScale = (0.45f + expansion * 0.4f) * devMult;

            Color edgeColor = GetPhaseColor(0.3f);
            Color midColor = GetPhaseColor(0.6f);
            Color coreColor = GoliathUtils.StarCore;

            // Configure shader — using correct parameter names matching RippleShader.fx
            rippleShader.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.03f + seed);
            rippleShader.Parameters["progress"]?.SetValue(expansion);
            rippleShader.Parameters["ringCount"]?.SetValue((float)ringCount);
            rippleShader.Parameters["ringThickness"]?.SetValue(0.035f);
            rippleShader.Parameters["primaryColor"]?.SetValue(edgeColor.ToVector3());
            rippleShader.Parameters["secondaryColor"]?.SetValue(midColor.ToVector3());
            rippleShader.Parameters["coreColor"]?.SetValue(coreColor.ToVector3());
            rippleShader.Parameters["fadeAlpha"]?.SetValue(alpha);
            rippleShader.Parameters["noiseTex"]?.SetValue(_noisePerlin.Value);

            // Bind Moonlight Sonata LUT gradient for theme-consistent ring coloring
            _gradientLUT ??= ModContent.Request<Texture2D>(GradientLUTPath);
            rippleShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            rippleShader.Parameters["useGradient"]?.SetValue(1f);

            // Shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, rippleShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circle = _softCircle.Value;
            Vector2 circleOrigin = circle.Size() / 2f;
            sb.Draw(circle, drawPos, null, Color.White * alpha, 0f,
                circleOrigin, drawScale, SpriteEffects.None, 0f);

            // End shader, return to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawOuterBloom(SpriteBatch sb, Vector2 drawPos, float expansion, float alpha, float devMult)
        {
            Texture2D glowOrb = _glowOrb.Value;
            Vector2 origin = glowOrb.Size() / 2f;

            Color phaseOuter = GetPhaseColor(0.4f);
            float bloomScale = MathHelper.Min((0.15f + expansion * 0.25f) * devMult, 0.293f); // GlowOrb 1024px — cap to 300px max
            float bloomAlpha = alpha * 0.3f * devMult;

            sb.Draw(glowOrb, drawPos, null, phaseOuter * bloomAlpha, 0f,
                origin, bloomScale, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
