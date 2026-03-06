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
    /// PrismaticMaskOrb — A radial noise-masked orb that blooms outward at the
    /// center of a PrismaticDetonation (5th bounce explosion).
    ///
    /// VISUAL ARCHITECTURE (Foundation: MaskFoundation + RadialNoiseMaskShader):
    /// 1. BLOOM HALO — 3-layer soft glow behind the orb (prismatic gradient colors)
    /// 2. SHADER ORB — RadialNoiseMaskShader rendering a swirling noise sphere
    ///    masked to a circle, with prismatic coloring via Moonlight Sonata LUT
    /// 3. CORE BLOOM — Concentrated white-hot center point
    ///
    /// The orb scales up rapidly on spawn (expo ease-out), stays briefly at
    /// full size with gentle pulsing, then fades out. CosmicNebulaClouds noise
    /// creates a swirling, refractive feel matching the prismatic detonation.
    ///
    /// VFX-only: friendly=false, 0 damage.
    ///
    /// ai[0] = spectral phase offset (0-1) for color shifting
    /// </summary>
    public class PrismaticMaskOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 55;
        private const float OrbDrawScale = 0.85f;

        // Texture paths — sourced from VFX Asset Library
        private static readonly string BloomPath = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string MaskPath = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string NoisePath = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string GradPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        private int timer;
        private float seed;
        private Effect orbShader;

        // Cached textures
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _noiseCosmicNebula;
        private static Asset<Texture2D> _gradMoonlightSonata;

        private float SpectralPhase => Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
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

            // Lighting — prismatic gradient shifts over time
            float t = timer / (float)MaxLifetime;
            Color lightCol = GetBeamGradient(MathHelper.Clamp(SpectralPhase + t * 0.3f, 0f, 1f));
            float lightInt = GetAlpha() * 1.2f;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * lightInt);
        }

        private void EnsureTextures()
        {
            _softGlow ??= ModContent.Request<Texture2D>(BloomPath + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _glowOrb ??= ModContent.Request<Texture2D>(BloomPath + "GlowOrb", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>(MaskPath + "SoftCircle", AssetRequestMode.ImmediateLoad);
            _noiseCosmicNebula ??= ModContent.Request<Texture2D>(NoisePath + "CosmicNebulaClouds", AssetRequestMode.ImmediateLoad);
            _gradMoonlightSonata ??= ModContent.Request<Texture2D>(GradPath + "MoonlightSonataGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
        }

        /// <summary>
        /// Scale-up entrance, hold, then fade-out.
        /// </summary>
        private float GetScale()
        {
            float t = timer / (float)MaxLifetime;
            // Expo ease-out entrance over first 20% of life
            float entrance = t < 0.2f ? (1f - MathF.Pow(2f, -10f * (t / 0.2f))) : 1f;
            // Gentle scale pulse during hold
            float pulse = 1f + 0.06f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f + seed);
            // Slight shrink as it fades
            float shrink = t > 0.7f ? 1f - (t - 0.7f) / 0.3f * 0.25f : 1f;
            return entrance * pulse * shrink;
        }

        /// <summary>
        /// Fade in quickly, hold, fade out over last 30%.
        /// </summary>
        private float GetAlpha()
        {
            float t = timer / (float)MaxLifetime;
            float fadeIn = MathHelper.Clamp(t / 0.1f, 0f, 1f);
            float fadeOut = t > 0.7f ? 1f - (t - 0.7f) / 0.3f : 1f;
            return fadeIn * fadeOut;
        }

        /// <summary>
        /// Get 3-color palette from prismatic gradient: edge / mid / core.
        /// </summary>
        private void GetPrismaticColors(out Color edgeColor, out Color midColor, out Color coreColor)
        {
            float phase = SpectralPhase;
            edgeColor = GetBeamGradient(MathHelper.Clamp(phase * 0.3f, 0f, 1f));
            midColor = GetBeamGradient(MathHelper.Clamp(phase * 0.6f + 0.2f, 0f, 1f));
            coreColor = MoonWhite;
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();
            float scale = GetScale();

            // Ensure textures are loaded (may not have run AI yet on first frame)
            EnsureTextures();
            if (_softGlow == null || !_softGlow.IsLoaded || _softGlow.Value == null)
                return false;

            GetPrismaticColors(out Color edgeColor, out Color midColor, out Color coreColor);

            // Switch to additive for all layers
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- LAYER 1: BLOOM HALO ----
            DrawBloomHalo(sb, drawPos, edgeColor, midColor, coreColor, alpha, scale);

            // ---- LAYER 2: SHADER-DRIVEN NOISE ORB ----
            DrawShaderOrb(sb, drawPos, edgeColor, coreColor, alpha, scale);

            // ---- LAYER 3: CORE BLOOM ----
            DrawCoreBloom(sb, drawPos, midColor, coreColor, alpha, scale);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =================================================================
        // LAYER 1: BLOOM HALO
        // =================================================================

        private void DrawBloomHalo(SpriteBatch sb, Vector2 drawPos,
            Color edge, Color mid, Color core, float alpha, float scale)
        {
            Texture2D softGlow = _softGlow?.Value;
            if (softGlow == null) return;
            Vector2 origin = softGlow.Size() / 2f;
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f + seed);

            // Outer ambient — wide, dim, prismatic edge color
            sb.Draw(softGlow, drawPos, null, edge * (0.22f * alpha * pulse), 0f,
                origin, 0.65f * scale * pulse, SpriteEffects.None, 0f);

            // Mid glow — brighter, mid gradient
            sb.Draw(softGlow, drawPos, null, mid * (0.3f * alpha * pulse), 0f,
                origin, 0.4f * scale * pulse, SpriteEffects.None, 0f);

            // Inner glow — intense, warm core
            sb.Draw(softGlow, drawPos, null, core * (0.4f * alpha * pulse), 0f,
                origin, 0.2f * scale * pulse, SpriteEffects.None, 0f);
        }

        // =================================================================
        // LAYER 2: SHADER-DRIVEN NOISE ORB
        // =================================================================

        private void DrawShaderOrb(SpriteBatch sb, Vector2 drawPos,
            Color primary, Color core, float alpha, float scale)
        {
            float time = (float)Main.timeForVisualEffects;

            // Load shader
            if (orbShader == null)
            {
                orbShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // Configure uniforms — faster scroll and more intensity for explosive feel
            orbShader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            orbShader.Parameters["scrollSpeed"]?.SetValue(0.45f);
            orbShader.Parameters["rotationSpeed"]?.SetValue(0.25f);
            orbShader.Parameters["circleRadius"]?.SetValue(0.42f);
            orbShader.Parameters["edgeSoftness"]?.SetValue(0.07f);
            orbShader.Parameters["intensity"]?.SetValue(2.2f);
            orbShader.Parameters["primaryColor"]?.SetValue(primary.ToVector3());
            orbShader.Parameters["coreColor"]?.SetValue(core.ToVector3());

            // Cosmic nebula noise for swirling prismatic refraction
            orbShader.Parameters["noiseTex"]?.SetValue(_noiseCosmicNebula.Value);
            // Moonlight Sonata gradient LUT for theme-consistent coloring
            orbShader.Parameters["gradientTex"]?.SetValue(_gradMoonlightSonata.Value);

            // Begin shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, orbShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = _softCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * alpha, 0f,
                circleOrigin, OrbDrawScale * scale, SpriteEffects.None, 0f);

            // End shader batch, return to additive (no shader)
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // LAYER 3: CORE BLOOM
        // =================================================================

        private void DrawCoreBloom(SpriteBatch sb, Vector2 drawPos,
            Color mid, Color core, float alpha, float scale)
        {
            Texture2D glowOrb = _glowOrb?.Value;
            if (glowOrb == null) return;
            Vector2 origin = glowOrb.Size() / 2f;
            float pulse = 0.92f + 0.08f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + seed * 2f);

            // Bright prismatic core
            sb.Draw(glowOrb, drawPos, null, mid * (0.35f * alpha * pulse), 0f,
                origin, 0.18f * scale * pulse, SpriteEffects.None, 0f);

            // White-hot pinpoint
            sb.Draw(glowOrb, drawPos, null, core * (0.5f * alpha * pulse), 0f,
                origin, 0.1f * scale * pulse, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
