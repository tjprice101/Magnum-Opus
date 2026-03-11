using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MagicOrbFoundation
{
    /// <summary>
    /// OrbBolt — A small, shiny, bloom-based projectile fired by the MagicOrb.
    /// 
    /// VISUAL ARCHITECTURE:
    /// Pure bloom rendering — no shader needed. This projectile is intentionally
    /// simple and sparkly: multi-layered additive bloom sprites stacked at different
    /// scales create a bright, shiny bolt that feels like a concentrated spark
    /// of the parent orb's energy.
    /// 
    /// Layers:
    /// 1. Outer soft glow (large, dim, primary color)
    /// 2. Mid glow (medium, brighter, secondary color)
    /// 3. Core point (small, very bright, core color)
    /// 4. Star flare (tiny directional cross for "shiny" feel)
    /// 5. Trailing dust particles in the bolt's wake
    /// 
    /// BEHAVIOUR:
    /// - Gentle homing after 8-frame delay
    /// - 1 penetration, 180 frame lifetime
    /// - Burst dust on hit/death
    /// 
    /// ai[0] = OrbNoiseStyle index (determines colors to match parent orb)
    /// </summary>
    public class OrbBolt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 180;
        private const int HomingDelay = 8;
        private const float HomingStrength = 0.06f;
        private const float HomingRange = 600f;

        private int timer;
        private float seed;

        private OrbNoiseStyle CurrentStyle => (OrbNoiseStyle)(int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 300;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

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
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ---- LIGHTING ----
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);
            Lighting.AddLight(Projectile.Center, colors[2].ToVector3() * 0.35f);

            // ---- TRAILING DUST ----
            if (Main.rand.NextBool(2))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.15f, 0.3f));
                dust.noGravity = true;
                dust.fadeIn = 0.25f;
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
        // RENDERING — Pure bloom, no shader
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;
            float alpha = fadeIn * fadeOut;
            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f + seed);

            // ---- BEGIN ADDITIVE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = MOFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Layer 1: Outer soft glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.25f * alpha * pulse),
                0f, glowOrigin, 0.09f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.4f * alpha * pulse),
                0f, glowOrigin, 0.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Core point
            Texture2D glowOrb = MOFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;
            sb.Draw(glowOrb, drawPos, null, colors[2] * (0.6f * alpha * pulse),
                0f, orbOrigin, 0.03f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Star flare for "shiny" sparkle feel
            Texture2D starFlare = MOFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            float flickerAlpha = 0.3f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.25f + seed * 3f);
            sb.Draw(starFlare, drawPos, null, colors[2] * (flickerAlpha * alpha),
                Projectile.rotation, flareOrigin, 0.025f, SpriteEffects.None, 0f);

            // Perpendicular cross flare for extra sparkle
            sb.Draw(starFlare, drawPos, null, colors[1] * (flickerAlpha * alpha * 0.5f),
                Projectile.rotation + MathHelper.PiOver4, flareOrigin, 0.018f, SpriteEffects.None, 0f);

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

        // =====================================================================
        // COMBAT
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.55f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Color[] colors = MOFTextures.GetStyleColors(CurrentStyle);
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
