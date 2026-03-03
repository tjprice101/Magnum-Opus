using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MaskFoundation
{
    /// <summary>
    /// MaskOrbProjectile — A large homing circular projectile with a noise texture
    /// radially scrolling through it, masked to a vibrant circle via shader.
    /// 
    /// SHADER ARCHITECTURE:
    /// 
    /// 1. RADIAL NOISE MASK SHADER (RadialNoiseMaskShader.fx) — SpriteBatch pipeline
    ///    - Applied as SpriteBatch effect when drawing the orb body
    ///    - Converts sprite UVs to polar coordinates (angle + radius)
    ///    - Scrolls the noise texture radially (outward motion + rotation)
    ///    - Samples a second noise layer at different scale for detail
    ///    - Maps combined noise intensity through a gradient LUT for theme coloring
    ///    - Masks everything to a soft circle using smoothstep distance falloff
    ///    - Result: a vibrant, clearly visible, radially animated noise orb
    /// 
    /// 2. ADDITIVE BLOOM LAYER — Standard SpriteBatch (no shader)
    ///    - Multi-scale bloom stacking (outer/mid/inner) using SoftGlow and GlowOrb
    ///    - Provides ambient glow halo around the orb
    ///    - Pulsing brightness for a living feel
    /// 
    /// BEHAVIOR:
    /// - Spawns from melee swing, 1 orb per swing
    /// - 20-frame delay before homing kicks in (lets orb separate from player)
    /// - Smooth angular homing toward nearest enemy (0.05 rad/frame turn speed)
    /// - Pretty big visual size (drawn at ~80px diameter via scaled sprite quad)
    /// - 240-frame lifetime with 40-frame fade-out
    /// - 3 penetrations then death, with burst on kill
    /// 
    /// ai[0] = Noise mode index — determines which noise texture to use
    /// </summary>
    public class MaskOrbProjectile : ModProjectile
    {
        // Use vanilla Crystal Bullet sprite as a small placeholder (shader renders the visuals)
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CrystalBullet;

        // ---- CONSTANTS ----
        private const int MaxLifetime = 240;
        private const int HomingDelay = 20;
        private const float HomingStrength = 0.05f;
        private const float HomingRange = 900f;
        private const float TargetSpeed = 9f;
        private const int FadeOutFrames = 40;

        /// <summary>Scale applied to the SoftCircle texture for the shader quad. Controls orb size.</summary>
        private const float OrbDrawScale = 0.6f;

        // ---- STATE ----
        private int timer;
        private float seed;

        private NoiseMode CurrentMode => (NoiseMode)(int)Projectile.ai[0];

        // ---- CACHED SHADER ----
        private Effect orbShader;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.hide = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // ---- INIT ----
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);
            }

            timer++;

            // ---- HOMING ----
            if (timer > HomingDelay)
            {
                NPC target = FindClosestTarget();
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    float targetAngle = toTarget.ToRotation();
                    float currentAngle = Projectile.velocity.ToRotation();
                    float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                    float clampedTurn = MathHelper.Clamp(angleDiff, -HomingStrength, HomingStrength);
                    Projectile.velocity = (currentAngle + clampedTurn).ToRotationVector2() * Projectile.velocity.Length();
                }

                // Accelerate toward target speed
                if (Projectile.velocity.Length() < TargetSpeed)
                {
                    Projectile.velocity *= 1.015f;
                    if (Projectile.velocity.Length() > TargetSpeed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * TargetSpeed;
                }
            }

            // ---- ROTATION (visual spin) ----
            Projectile.rotation += 0.03f;

            // ---- LIGHTING ----
            Color[] colors = MFTextures.GetModeColors(CurrentMode);
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * 0.6f);

            // ---- AMBIENT DUST ----
            if (Main.rand.NextBool(4))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.RainbowMk2, dustVel, newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        private NPC FindClosestTarget()
        {
            NPC closest = null;
            float closestDist = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            return closest;
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Color[] modeColors = MFTextures.GetModeColors(CurrentMode);

            float lifeFade = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;

            // Fade in over first 10 frames
            float fadeIn = MathHelper.Clamp(timer / 10f, 0f, 1f);
            float alpha = lifeFade * fadeIn;

            // ---- LAYER 1: BLOOM HALO (behind the orb) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawBloomHalo(sb, modeColors, alpha);

            // ---- LAYER 2: SHADER-DRIVEN NOISE ORB (RadialNoiseMaskShader via SpriteBatch) ----
            DrawShaderOrb(sb, modeColors, alpha);

            // ---- LAYER 3: BRIGHT CORE BLOOM (on top) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawCoreBloom(sb, modeColors, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        // LAYER 1: BLOOM HALO
        // =====================================================================

        /// <summary>
        /// Draws a soft glow halo behind the orb using multi-scale bloom stacking.
        /// </summary>
        private void DrawBloomHalo(SpriteBatch sb, Color[] modeColors, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D softGlow = MFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.06f + seed);

            // Outer ambient glow — large, dim, primary color
            sb.Draw(softGlow, drawPos, null, modeColors[0] * (0.25f * alpha * pulse), 0f,
                glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);

            // Mid glow — medium, brighter, secondary color
            sb.Draw(softGlow, drawPos, null, modeColors[1] * (0.35f * alpha * pulse), 0f,
                glowOrigin, 0.4f * pulse, SpriteEffects.None, 0f);

            // Inner glow — small, intense, highlight
            sb.Draw(softGlow, drawPos, null, modeColors[2] * (0.45f * alpha * pulse), 0f,
                glowOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // LAYER 2: SHADER-DRIVEN NOISE ORB
        // =====================================================================

        /// <summary>
        /// Draws the orb body through the RadialNoiseMaskShader.
        /// Uses SoftCircle as the sprite quad — the shader does all the visual work.
        /// </summary>
        private void DrawShaderOrb(SpriteBatch sb, Color[] modeColors, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;

            // ---- LOAD SHADER ----
            if (orbShader == null)
            {
                orbShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER UNIFORMS ----
            orbShader.Parameters["uTime"]?.SetValue(time * 0.015f + seed);
            orbShader.Parameters["scrollSpeed"]?.SetValue(0.3f);
            orbShader.Parameters["rotationSpeed"]?.SetValue(0.15f);
            orbShader.Parameters["circleRadius"]?.SetValue(0.45f);
            orbShader.Parameters["edgeSoftness"]?.SetValue(0.08f);
            orbShader.Parameters["intensity"]?.SetValue(2.0f);
            orbShader.Parameters["primaryColor"]?.SetValue(modeColors[0].ToVector3());
            orbShader.Parameters["coreColor"]?.SetValue(modeColors[2].ToVector3());

            // Set the noise texture for the current mode
            orbShader.Parameters["noiseTex"]?.SetValue(MFTextures.GetNoiseForMode(CurrentMode));

            // Set the gradient LUT for theme coloring
            orbShader.Parameters["gradientTex"]?.SetValue(MFTextures.GetGradientForMode(CurrentMode));

            // ---- DRAW WITH SHADER ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, orbShader,
                Main.GameViewMatrix.TransformationMatrix);

            // Use the SoftCircle mask texture as the quad — shader replaces its visual content
            Texture2D orbTex = MFTextures.SoftCircle.Value;
            Vector2 orbOrigin = orbTex.Size() / 2f;

            sb.Draw(orbTex, drawPos, null, Color.White * alpha, 0f,
                orbOrigin, OrbDrawScale, SpriteEffects.None, 0f);

            // ---- END SHADER BATCH ----
            sb.End();

            // Restart in additive (no shader) for subsequent layers
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        // LAYER 3: BRIGHT CORE BLOOM
        // =====================================================================

        /// <summary>
        /// Draws a concentrated core glow on top of the orb for extra vibrance.
        /// </summary>
        private void DrawCoreBloom(SpriteBatch sb, Color[] modeColors, float alpha)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowOrb = MFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;

            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + seed * 2f);

            // Core glow — bright center point
            sb.Draw(glowOrb, drawPos, null, modeColors[2] * (0.4f * alpha * pulse), 0f,
                orbOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

            // Secondary glow layer
            sb.Draw(glowOrb, drawPos, null, modeColors[0] * (0.2f * alpha * pulse), 0f,
                orbOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // COMBAT
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] modeColors = MFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Color col = modeColors[Main.rand.Next(modeColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Color[] modeColors = MFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = modeColors[Main.rand.Next(modeColors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.65f));
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
