using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackFoundation
{
    /// <summary>
    /// ThrowSlamProjectile — Mode 1: Throw the sword upward, it spins in the air,
    /// then homes onto the nearest enemy and slams down onto them.
    ///
    /// Behaviour:
    /// Phase 0 (frames 0-20):   Sword is thrown upward, spinning rapidly
    /// Phase 1 (frames 21-35):  Sword hovers at apex, locks onto nearest target
    /// Phase 2 (frames 36+):    Sword dives toward target at high speed, dealing damage on hit
    ///
    /// ai[0] = mode index (always 0 for ThrowSlam)
    /// </summary>
    public class ThrowSlamProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        // ---- TIMING ----
        private const int RiseFrames = 20;
        private const int HoverFrames = 15;
        private const int DiveMaxFrames = 60;
        private const int MaxLifetime = RiseFrames + HoverFrames + DiveMaxFrames + 10;

        // ---- MOVEMENT ----
        private const float RiseSpeed = 10f;
        private const float DiveSpeed = 22f;
        private const float HomingRange = 900f;
        private const float SpinRate = 0.4f;

        private int timer;
        private float spinRotation;
        private int phase; // 0=rise, 1=hover, 2=dive
        private Vector2 apexPosition;
        private NPC diveTarget;
        private bool hasDealtDamage;
        private float trailAlpha;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            timer++;
            Color[] colors = AFTextures.GetModeColors(AttackMode.ThrowSlam);

            switch (phase)
            {
                case 0: // RISE PHASE — sword flies upward while spinning
                    PhaseRise(colors);
                    break;

                case 1: // HOVER PHASE — sword pauses at apex, acquires target
                    PhaseHover(colors);
                    break;

                case 2: // DIVE PHASE — sword slams toward target
                    PhaseDive(colors);
                    break;
            }

            // Spin the sword continuously
            spinRotation += SpinRate * (phase == 2 ? 2.5f : 1f);
            Projectile.rotation = spinRotation;

            // Lighting
            Lighting.AddLight(Projectile.Center, colors[1].ToVector3() * 0.6f);

            // Trail dust
            if (Main.rand.NextBool(2))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        private void PhaseRise(Color[] colors)
        {
            // Move upward with some deceleration
            float riseProgress = timer / (float)RiseFrames;
            float speed = RiseSpeed * (1f - riseProgress * 0.6f);
            Projectile.velocity = new Vector2(0, -speed);

            trailAlpha = MathHelper.Lerp(0f, 0.8f, riseProgress);

            if (timer >= RiseFrames)
            {
                phase = 1;
                apexPosition = Projectile.Center;
                Projectile.velocity = Vector2.Zero;

                // Flash at apex
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
            }
        }

        private void PhaseHover(Color[] colors)
        {
            int hoverTimer = timer - RiseFrames;

            // Gentle float at apex
            Projectile.Center = apexPosition + new Vector2(0, MathF.Sin(hoverTimer * 0.15f) * 3f);
            Projectile.velocity = Vector2.Zero;

            trailAlpha = 0.8f + 0.2f * MathF.Sin(hoverTimer * 0.2f);

            // Acquire target
            if (diveTarget == null || !diveTarget.active)
            {
                diveTarget = FindClosestTarget();
            }

            if (hoverTimer >= HoverFrames)
            {
                phase = 2;
                hasDealtDamage = false;

                if (diveTarget != null && diveTarget.active)
                {
                    // Aim at target
                    Vector2 toTarget = (diveTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = toTarget * DiveSpeed;
                }
                else
                {
                    // No target — slam straight down
                    Projectile.velocity = new Vector2(0, DiveSpeed);
                }

                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = -0.3f }, Projectile.Center);
            }
        }

        private void PhaseDive(Color[] colors)
        {
            int diveTimer = timer - RiseFrames - HoverFrames;
            trailAlpha = 1f;

            // Slight homing correction during dive
            if (diveTarget != null && diveTarget.active && diveTimer < 30)
            {
                Vector2 toTarget = (diveTarget.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float targetAngle = toTarget.ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                float clampedTurn = MathHelper.Clamp(angleDiff, -0.08f, 0.08f);
                Projectile.velocity = (currentAngle + clampedTurn).ToRotationVector2() * Projectile.velocity.Length();
            }

            // Accelerate slightly
            if (Projectile.velocity.Length() < DiveSpeed * 1.3f)
                Projectile.velocity *= 1.02f;

            // Enable tile collision during dive for impact
            if (diveTimer > 10)
                Projectile.tileCollide = true;

            // Kill if too old
            if (diveTimer > DiveMaxFrames)
                Projectile.Kill();
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact burst VFX
            Color[] colors = AFTextures.GetModeColors(AttackMode.ThrowSlam);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.2f }, target.Center);

            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.9f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }

            // Radial spark burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: colors[2], Scale: 0.6f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst
            Color[] colors = AFTextures.GetModeColors(AttackMode.ThrowSlam);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);

            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Color[] colors = AFTextures.GetModeColors(AttackMode.ThrowSlam);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);

            // ---- ADDITIVE BLOOM LAYERS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = AFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Outer spin glow (SoftGlow 1024px — target ~80px)
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.35f * trailAlpha * pulse),
                0f, glowOrigin, 0.08f * pulse, SpriteEffects.None, 0f);

            // Mid glow (~50px)
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.5f * trailAlpha * pulse),
                0f, glowOrigin, 0.05f * pulse, SpriteEffects.None, 0f);

            // Core glow (~25px)
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.7f * trailAlpha),
                0f, glowOrigin, 0.025f, SpriteEffects.None, 0f);

            // Velocity line during dive (~100×10px)
            if (phase == 2)
            {
                float velAngle = Projectile.velocity.ToRotation();
                sb.Draw(softGlow, drawPos, null, colors[1] * 0.4f,
                    velAngle, glowOrigin,
                    new Vector2(0.1f, 0.01f), SpriteEffects.None, 0f);
            }

            // Star flare at center (StarFlare 1024px — target ~40px)
            Texture2D starFlare = AFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            sb.Draw(starFlare, drawPos, null, colors[2] * (0.3f * trailAlpha),
                spinRotation * 0.5f, flareOrigin, 0.04f * pulse, SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Draw the sword sprite itself
            Texture2D swordTex = Terraria.GameContent.TextureAssets.Item[ItemID.Katana].Value;
            Vector2 swordOrigin = swordTex.Size() / 2f;
            sb.Draw(swordTex, drawPos, null, Color.White, spinRotation,
                swordOrigin, 1f, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
