using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ImpactFoundation
{
    /// <summary>
    /// ImpactProjectile — Homing projectile that spawns impact effects on hit.
    /// 
    /// Behaviour:
    /// - Gentle homing toward nearest enemy after 15-frame delay
    /// - Leaves a soft glow trail of dust
    /// - On hitting an enemy or tile, spawns the appropriate impact effect
    ///   based on the current impact mode stored in ai[0]
    /// - 1 penetration then death
    /// 
    /// ai[0] = ImpactMode index (Ripple=0, DamageZone=1, SlashMark=2)
    /// </summary>
    public class ImpactProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 300;
        private const int HomingDelay = 15;
        private const float HomingStrength = 0.04f;
        private const float HomingRange = 800f;
        private const float TargetSpeed = 14f;

        private int timer;
        private float seed;

        private ImpactMode CurrentMode => (ImpactMode)(int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
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

            // ---- ROTATION ----
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ---- LIGHTING ----
            Color[] colors = IFTextures.GetModeColors(CurrentMode);
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * 0.5f);

            // ---- TRAIL DUST ----
            if (Main.rand.NextBool(2))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
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
            Color[] modeColors = IFTextures.GetModeColors(CurrentMode);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(timer / 8f, 0f, 1f);
            float alpha = fadeIn;
            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f + seed);

            // ---- ADDITIVE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = IFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Outer glow
            sb.Draw(softGlow, drawPos, null, modeColors[0] * (0.3f * alpha * pulse),
                0f, glowOrigin, 0.35f * pulse, SpriteEffects.None, 0f);

            // Core glow
            sb.Draw(softGlow, drawPos, null, modeColors[2] * (0.5f * alpha * pulse),
                0f, glowOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

            // Direction flare
            Texture2D starFlare = IFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            sb.Draw(starFlare, drawPos, null, modeColors[1] * (0.25f * alpha),
                Projectile.rotation, flareOrigin, 0.12f, SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        // IMPACT SPAWNING
        // =====================================================================

        /// <summary>
        /// Spawns the appropriate impact effect at the given position.
        /// Called from OnHitNPC and OnKill (tile collision).
        /// </summary>
        private void SpawnImpactEffect(Vector2 position)
        {
            switch (CurrentMode)
            {
                case ImpactMode.Ripple:
                    // Spawn a ripple effect projectile
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), position, Vector2.Zero,
                        ModContent.ProjectileType<RippleEffectProjectile>(),
                        0, 0f, Projectile.owner);
                    break;

                case ImpactMode.DamageZone:
                    // Spawn a lasting damage zone projectile
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), position, Vector2.Zero,
                        ModContent.ProjectileType<DamageZoneProjectile>(),
                        Projectile.damage / 3, 0f, Projectile.owner);
                    break;

                case ImpactMode.SlashMark:
                    // Spawn a slash mark at the impact point with the projectile's velocity as direction
                    float angle = Projectile.velocity.ToRotation();
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), position, Vector2.Zero,
                        ModContent.ProjectileType<SlashMarkProjectile>(),
                        0, 0f, Projectile.owner, ai0: angle);
                    break;
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.3f }, position);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnImpactEffect(target.Center);

            // Burst dust on hit
            Color[] modeColors = IFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = modeColors[Main.rand.Next(modeColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SpawnImpactEffect(Projectile.Center);

            // Burst dust on death
            Color[] modeColors = IFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = modeColors[Main.rand.Next(modeColors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
