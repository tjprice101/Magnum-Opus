using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ThinSlashFoundation
{
    /// <summary>
    /// ThinSlashProjectile — Homing projectile that spawns a thin slash effect on hit.
    /// 
    /// Behaviour:
    /// - Gentle homing toward nearest enemy after 12-frame delay
    /// - Leaves a soft glow trail
    /// - On hitting an enemy or tile, spawns ThinSlashEffect at the hit point
    /// - 1 penetration then death
    /// 
    /// ai[0] = SlashStyle index (determines slash color)
    /// </summary>
    public class ThinSlashProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 300;
        private const int HomingDelay = 12;
        private const float HomingStrength = 0.05f;
        private const float HomingRange = 900f;
        private const float TargetSpeed = 16f;

        private int timer;
        private float seed;

        private SlashStyle CurrentStyle => (SlashStyle)(int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
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

                if (Projectile.velocity.Length() < TargetSpeed)
                {
                    Projectile.velocity *= 1.01f;
                    if (Projectile.velocity.Length() > TargetSpeed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * TargetSpeed;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ---- LIGHTING ----
            Color[] colors = TSFTextures.GetStyleColors(CurrentStyle);
            Lighting.AddLight(Projectile.Center, colors[2].ToVector3() * 0.4f);

            // ---- TRAIL DUST ----
            if (Main.rand.NextBool(2))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
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
            Color[] styleColors = TSFTextures.GetStyleColors(CurrentStyle);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(timer / 6f, 0f, 1f);
            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + seed);

            // ---- ADDITIVE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = TSFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Outer glow
            sb.Draw(softGlow, drawPos, null, styleColors[0] * (0.25f * fadeIn * pulse),
                0f, glowOrigin, 0.12f * pulse, SpriteEffects.None, 0f);

            // Core glow
            sb.Draw(softGlow, drawPos, null, styleColors[2] * (0.45f * fadeIn * pulse),
                0f, glowOrigin, 0.05f * pulse, SpriteEffects.None, 0f);

            // Direction flare
            Texture2D starFlare = TSFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            sb.Draw(starFlare, drawPos, null, styleColors[1] * (0.2f * fadeIn),
                Projectile.rotation, flareOrigin, 0.04f, SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        // IMPACT — SPAWN THIN SLASH EFFECT
        // =====================================================================

        private void SpawnSlashEffect(Vector2 position)
        {
            float angle = Projectile.velocity.ToRotation();

            // Spawn a thin slash mark at impact
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<ThinSlashEffect>(),
                0, 0f, Projectile.owner,
                ai0: angle, ai1: (float)CurrentStyle);

            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.5f, Pitch = 0.5f }, position);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnSlashEffect(target.Center);

            Color[] styleColors = TSFTextures.GetStyleColors(CurrentStyle);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = styleColors[Main.rand.Next(styleColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SpawnSlashEffect(Projectile.Center);

            Color[] styleColors = TSFTextures.GetStyleColors(CurrentStyle);
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = styleColors[Main.rand.Next(styleColors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
