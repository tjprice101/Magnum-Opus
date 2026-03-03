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
    /// DamageZoneProjectile — A lasting circular damage area (5 seconds) that uses
    /// MaskFoundation-style radial noise masking to create a sparkling damage zone.
    /// 
    /// VISUAL ARCHITECTURE:
    /// 1. DAMAGE ZONE SHADER (DamageZoneShader.fx) — Radial noise pattern masked to
    ///    a circle, similar to MaskFoundation's RadialNoiseMaskShader but with:
    ///    - Breathing/pulsing animation to feel alive
    ///    - Edge shimmer with sparkle highlights
    ///    - Gradual fade in/out on spawn and expiry
    /// 2. BLOOM STACKING — Ambient glow underneath the zone
    /// 3. SPARKLE PARTICLES — CPU-side dust particles that sparkle within the zone
    /// 
    /// Behaviour:
    /// - Spawns at impact, does NOT move
    /// - Deals 1/3 of weapon damage to enemies standing in the zone
    /// - Lasts 300 frames (5 seconds)
    /// - Fade in over 15 frames, fade out over 30 frames
    /// - Sparkle dust particles scattered inside the circle
    /// </summary>
    public class DamageZoneProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 300; // 5 seconds
        private const int FadeInFrames = 15;
        private const int FadeOutFrames = 30;
        private const float ZoneRadius = 40f; // Pixel radius for damage detection
        private const float DrawScale = 0.3f; // Scale of the shader quad

        private int timer;
        private float seed;
        private Effect zoneShader;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 500;
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)(ZoneRadius * 2);
            Projectile.height = (int)(ZoneRadius * 2);
            Projectile.friendly = true;
            Projectile.penetrate = -1; // Infinite penetration — zone persists
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30; // Hit every 0.5 seconds
            Projectile.hide = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;

            // Keep velocity at zero — zone doesn't move
            Projectile.velocity = Vector2.Zero;

            // ---- LIGHTING ----
            Color[] colors = IFTextures.GetModeColors(ImpactMode.DamageZone);
            float alphaMult = GetAlphaMultiplier();
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * alphaMult * 0.5f);

            // ---- SPARKLE DUST ----
            if (Main.rand.NextBool(3))
            {
                // Spawn sparkle dust within the zone radius
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(ZoneRadius * 0.9f);
                Vector2 dustPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 dustVel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1.5f)); // Gentle upward float

                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.RainbowMk2, dustVel,
                    newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }

            // Occasional larger sparkle burst
            if (Main.rand.NextBool(12))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(ZoneRadius * 0.5f);
                Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * radius;

                for (int i = 0; i < 3; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f);
                    Color col = colors[Main.rand.Next(colors.Length)];
                    Dust dust = Dust.NewDustPerfect(burstPos, DustID.RainbowMk2, vel,
                        newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.4f;
                }
            }
        }

        /// <summary>
        /// Returns a 0-1 alpha multiplier accounting for fade in and fade out.
        /// </summary>
        private float GetAlphaMultiplier()
        {
            int framesRemaining = Projectile.timeLeft;
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = framesRemaining < FadeOutFrames
                ? framesRemaining / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision — check if any point of the target is within zone radius
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

            float dist = Vector2.Distance(Projectile.Center, closestPoint);
            return dist <= ZoneRadius;
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color[] colors = IFTextures.GetModeColors(ImpactMode.DamageZone);
            float alpha = GetAlphaMultiplier();

            // ---- LAYER 1: AMBIENT BLOOM (behind) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawAmbientBloom(sb, drawPos, colors, alpha);

            // ---- LAYER 2: SHADER-DRIVEN NOISE ZONE ----
            DrawShaderZone(sb, drawPos, colors, alpha);

            // ---- LAYER 3: EDGE SPARKLE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawEdgeSparkles(sb, drawPos, colors, alpha);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws ambient soft glow beneath the damage zone.
        /// </summary>
        private void DrawAmbientBloom(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            Texture2D softGlow = IFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.04f + seed);

            // Wide ambient glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.2f * alpha * pulse),
                0f, glowOrigin, 0.2f * pulse, SpriteEffects.None, 0f);

            // Medium glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.25f * alpha * pulse),
                0f, glowOrigin, 0.12f * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Draws the main zone body via DamageZoneShader — radial noise masked to a circle.
        /// </summary>
        private void DrawShaderZone(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;

            // ---- LOAD SHADER ----
            if (zoneShader == null)
            {
                zoneShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/DamageZoneShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER ----
            zoneShader.Parameters["uTime"]?.SetValue(time * 0.012f + seed);
            zoneShader.Parameters["scrollSpeed"]?.SetValue(0.2f);
            zoneShader.Parameters["rotationSpeed"]?.SetValue(0.1f);
            zoneShader.Parameters["circleRadius"]?.SetValue(0.44f);
            zoneShader.Parameters["edgeSoftness"]?.SetValue(0.06f);
            zoneShader.Parameters["intensity"]?.SetValue(1.8f);
            zoneShader.Parameters["primaryColor"]?.SetValue(colors[0].ToVector3());
            zoneShader.Parameters["coreColor"]?.SetValue(colors[2].ToVector3());
            zoneShader.Parameters["fadeAlpha"]?.SetValue(alpha);

            // Breathing pulse
            float breathe = 0.85f + 0.15f * MathF.Sin(time * 0.05f + seed);
            zoneShader.Parameters["breathe"]?.SetValue(breathe);

            // Set noise + gradient textures
            zoneShader.Parameters["noiseTex"]?.SetValue(IFTextures.NoiseFBM.Value);
            zoneShader.Parameters["gradientTex"]?.SetValue(IFTextures.GetGradientForMode(ImpactMode.DamageZone));

            // ---- DRAW WITH SHADER ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, zoneShader,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D circleTex = IFTextures.SoftCircle.Value;
            Vector2 circleOrigin = circleTex.Size() / 2f;

            sb.Draw(circleTex, drawPos, null, Color.White * alpha,
                0f, circleOrigin, DrawScale, SpriteEffects.None, 0f);

            // End shader batch
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Draws sparkle highlights around the zone edge for extra visual shimmer.
        /// </summary>
        private void DrawEdgeSparkles(SpriteBatch sb, Vector2 drawPos, Color[] colors, float alpha)
        {
            float time = (float)Main.timeForVisualEffects;
            Texture2D pointBloom = IFTextures.PointBloom.Value;
            Vector2 bloomOrigin = pointBloom.Size() / 2f;

            // Draw several sparkle points orbiting the zone edge
            int sparkleCount = 6;
            for (int i = 0; i < sparkleCount; i++)
            {
                float baseAngle = (i / (float)sparkleCount) * MathHelper.TwoPi;
                float animAngle = baseAngle + time * 0.01f + seed;
                float radiusOffset = 0.85f + 0.1f * MathF.Sin(time * 0.03f + i * 1.5f);

                Vector2 sparkleOffset = animAngle.ToRotationVector2() * (ZoneRadius * radiusOffset * 0.8f);
                float sparkleAlpha = 0.3f + 0.2f * MathF.Sin(time * 0.06f + i * 2.1f);

                Color sparkleColor = i % 2 == 0 ? colors[2] : colors[1];
                float sparkleScale = 0.08f + 0.04f * MathF.Sin(time * 0.08f + i * 1.3f);

                sb.Draw(pointBloom, drawPos + sparkleOffset, null,
                    sparkleColor * (sparkleAlpha * alpha),
                    0f, bloomOrigin, sparkleScale, SpriteEffects.None, 0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Small sparkle burst on damage tick
            Color[] colors = IFTextures.GetModeColors(ImpactMode.DamageZone);
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f));
                dust.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
