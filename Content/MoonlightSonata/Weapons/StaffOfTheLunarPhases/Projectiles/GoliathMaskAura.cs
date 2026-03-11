using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities;
using MagnumOpus.Content.MoonlightSonata.Minions;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Projectiles
{
    /// <summary>
    /// GoliathMaskAura — Persistent aura overlay on the Goliath of Moonlight entity.
    /// Adapted from MaskFoundation's RadialNoiseMaskShader for an ambient cosmic aura
    /// that cycles color with the current Lunar Phase.
    ///
    /// VISUAL ARCHITECTURE (Foundation: MaskFoundation + RadialNoiseMaskShader):
    /// 1. BLOOM HALO — Soft outer glow pulsing with lunar phase coloring
    /// 2. SHADER AURA — RadialNoiseMaskShader with nebula noise, phase-cycling gradient
    /// 3. CORE BLOOM — Inner glow orb for depth
    ///
    /// Follows the Goliath minion projectile (ai[0] = owner index of Goliath).
    /// Refreshed periodically by the Goliath; kills itself if Goliath is gone.
    ///
    /// ai[0] = Goliath projectile index to follow
    /// ai[1] = lunar phase mode at spawn time (0-3)
    ///
    /// VFX-only: friendly=false, 0 damage.
    /// </summary>
    public class GoliathMaskAura : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 120; // 2 second aura cycle, refreshed by Goliath

        // Texture paths
        private static readonly string BloomPath = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
        private static readonly string MaskPath = "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/";
        private static readonly string NoisePath = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
        private static readonly string GradientPath = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

        private int timer;
        private float seed;
        private Effect maskShader;

        // Cached textures
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _glowOrb;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _nebulaWisp;
        private static Asset<Texture2D> _gradientLUT;

        /// <summary>Index of the Goliath projectile to follow.</summary>
        private int GoliathIndex => (int)Projectile.ai[0];

        /// <summary>Lunar phase at spawn (used for color blending).</summary>
        private int SpawnPhase => (int)Projectile.ai[1];

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

            // Follow the Goliath — if it's gone, kill the aura
            int goliathIdx = GoliathIndex;
            if (goliathIdx < 0 || goliathIdx >= Main.maxProjectiles ||
                !Main.projectile[goliathIdx].active ||
                Main.projectile[goliathIdx].type != ModContent.ProjectileType<GoliathOfMoonlight>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = Main.projectile[goliathIdx].Center;
            Projectile.velocity = Vector2.Zero;

            // Phase-colored lighting
            Color lightCol = GetCurrentPhaseColor(0.5f);
            float lightIntensity = GetPulseIntensity() * 0.4f;
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * lightIntensity);
        }

        private void EnsureTextures()
        {
            _softGlow ??= ModContent.Request<Texture2D>(BloomPath + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _glowOrb ??= ModContent.Request<Texture2D>(BloomPath + "GlowOrb", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>(MaskPath + "SoftCircle", AssetRequestMode.ImmediateLoad);
            _nebulaWisp ??= ModContent.Request<Texture2D>(NoisePath + "NebulaWispNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>(GradientPath + "MoonlightSonataGradientLUTandRAMP",
                AssetRequestMode.ImmediateLoad);
        }

        /// <summary>
        /// Get the current lunar phase color, blending between spawn-time phase and real-time phase.
        /// </summary>
        private Color GetCurrentPhaseColor(float intensity)
        {
            // Try to get real-time phase from the Goliath's owner
            int goliathIdx = GoliathIndex;
            int currentPhase = SpawnPhase;

            if (goliathIdx >= 0 && goliathIdx < Main.maxProjectiles && Main.projectile[goliathIdx].active)
            {
                int ownerIdx = Main.projectile[goliathIdx].owner;
                if (ownerIdx >= 0 && ownerIdx < Main.maxPlayers && Main.player[ownerIdx].active)
                {
                    GoliathPlayer gp = Main.player[ownerIdx].GetModPlayer<GoliathPlayer>();
                    currentPhase = gp.LunarPhaseMode;
                }
            }

            currentPhase = Math.Clamp(currentPhase, 0, 3);
            Color phaseColor = GoliathPlayer.LunarPhaseColors[currentPhase];
            Color cosmicBase = GoliathUtils.GetCosmicGradient(intensity);
            return Color.Lerp(cosmicBase, phaseColor, 0.5f);
        }

        /// <summary>
        /// Smooth pulsing intensity for the aura.
        /// </summary>
        private float GetPulseIntensity()
        {
            float t = timer / (float)MaxLifetime;
            // Fade in over first 15 frames, hold, fade out last 20 frames
            float fadeIn = MathHelper.Clamp(timer / 15f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((MaxLifetime - timer) / 20f, 0f, 1f);
            // Gentle oscillation
            float pulse = 1.0f;
            return fadeIn * fadeOut * pulse;
        }

        // =================================================================
        // RENDERING
        // =================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = GetPulseIntensity();

            // Switch to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // ---- LAYER 1: BLOOM HALO ----
            DrawBloomHalo(sb, drawPos, pulse);

            // ---- LAYER 2: SHADER AURA ----
            DrawShaderAura(sb, drawPos, pulse);

            // ---- LAYER 3: CORE BLOOM ----
            DrawCoreBloom(sb, drawPos, pulse);

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

        private void DrawBloomHalo(SpriteBatch sb, Vector2 drawPos, float pulse)
        {
            Texture2D glow = _softGlow.Value;
            Vector2 origin = glow.Size() / 2f;

            Color outerColor = GetCurrentPhaseColor(0.3f);
            Color midColor = GetCurrentPhaseColor(0.5f);

            // Outer halo — wide, faint (SoftGlow 1024px — cap to 300px max)
            sb.Draw(glow, drawPos, null, outerColor * (0.07f * pulse), 0f,
                origin, 0.24f, SpriteEffects.None, 0f);

            // Mid halo — moderate
            sb.Draw(glow, drawPos, null, midColor * (0.11f * pulse), 0f,
                origin, 0.2f, SpriteEffects.None, 0f);
        }

        private void DrawShaderAura(SpriteBatch sb, Vector2 drawPos, float pulse)
        {
            // Load shader
            if (maskShader == null)
            {
                maskShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            float scrollSpeed = 0.15f;
            float rotationSpeed = 0.08f;
            float time = (float)Main.timeForVisualEffects * 0.01f + seed;

            Color coreColor = GetCurrentPhaseColor(0.8f);
            Color edgeColor = GetCurrentPhaseColor(0.3f);

            // Configure shader
            maskShader.Parameters["uTime"]?.SetValue(time);
            maskShader.Parameters["noiseStrength"]?.SetValue(0.5f);
            maskShader.Parameters["scrollSpeed"]?.SetValue(scrollSpeed);
            maskShader.Parameters["rotationSpeed"]?.SetValue(rotationSpeed);
            maskShader.Parameters["coreColor"]?.SetValue(coreColor.ToVector3());
            maskShader.Parameters["edgeColor"]?.SetValue(edgeColor.ToVector3());
            maskShader.Parameters["noiseTex"]?.SetValue(_nebulaWisp.Value);
            maskShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            // Shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, maskShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circle = _softCircle.Value;
            Vector2 circleOrigin = circle.Size() / 2f;
            float orbDrawScale = 0.25f;
            sb.Draw(circle, drawPos, null, Color.White * (pulse * 0.6f), 0f,
                circleOrigin, orbDrawScale, SpriteEffects.None, 0f);

            // End shader, return to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCoreBloom(SpriteBatch sb, Vector2 drawPos, float pulse)
        {
            Texture2D glowOrb = _glowOrb.Value;
            Vector2 origin = glowOrb.Size() / 2f;

            Color coreColor = GetCurrentPhaseColor(0.7f);
            Color innerColor = GoliathUtils.StarCore;

            // Inner core glow
            sb.Draw(glowOrb, drawPos, null, innerColor * (0.09f * pulse), 0f,
                origin, 0.12f, SpriteEffects.None, 0f);

            // Phase-tinted core
            sb.Draw(glowOrb, drawPos, null, coreColor * (0.06f * pulse), 0f,
                origin, 0.17f, SpriteEffects.None, 0f);
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
