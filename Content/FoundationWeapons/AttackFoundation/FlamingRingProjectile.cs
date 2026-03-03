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
    /// FlamingRingProjectile — Mode 4: Summoner weapon that creates a flaming ring
    /// around the player.
    ///
    /// Behaviour:
    /// - Ring of fire orbs orbits the player at a set radius
    /// - Each orb emits flame particles and light
    /// - Enemies within the ring take continuous fire damage
    /// - Ring expands on spawn, holds for duration, then contracts and fades
    /// - Orbs accelerate over time, creating a spinning vortex of flame
    ///
    /// ai[0] = mode index (always 3 for FlamingRing)
    /// </summary>
    public class FlamingRingProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        // ---- CONFIGURATION ----
        private const int OrbCount = 12;
        private const float MaxRadius = 160f;
        private const float MinRadius = 10f;
        private const int ExpandFrames = 18;
        private const int HoldFrames = 50;
        private const int ContractFrames = 15;
        private const int TotalFrames = ExpandFrames + HoldFrames + ContractFrames;
        private const float BaseOrbitSpeed = 0.03f;
        private const float MaxOrbitSpeed = 0.08f;
        private const float DamageTickRate = 10; // frames between damage ticks

        // ---- STATE ----
        private int timer;
        private float currentRadius;
        private float orbitAngle;
        private float currentOrbitSpeed;
        private float overallAlpha;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.timeLeft = TotalFrames + 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = (int)DamageTickRate;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            timer++;

            // Follow the player
            Projectile.Center = owner.Center;
            Projectile.velocity = Vector2.Zero;

            Color[] colors = AFTextures.GetModeColors(AttackMode.FlamingRing);

            // Phase logic
            if (timer <= ExpandFrames)
            {
                float progress = timer / (float)ExpandFrames;
                float eased = EaseOutQuad(progress);
                currentRadius = MathHelper.Lerp(MinRadius, MaxRadius, eased);
                currentOrbitSpeed = MathHelper.Lerp(BaseOrbitSpeed * 0.5f, BaseOrbitSpeed, eased);
                overallAlpha = MathHelper.Clamp(progress * 2.5f, 0f, 1f);
            }
            else if (timer <= ExpandFrames + HoldFrames)
            {
                int holdTimer = timer - ExpandFrames;
                float holdProgress = holdTimer / (float)HoldFrames;

                // Gentle radius pulse
                float pulse = MathF.Sin(holdTimer * 0.06f) * 8f;
                currentRadius = MaxRadius + pulse;

                // Gradually accelerate orbit
                currentOrbitSpeed = MathHelper.Lerp(BaseOrbitSpeed, MaxOrbitSpeed, holdProgress);
                overallAlpha = 1f;
            }
            else
            {
                int contractTimer = timer - ExpandFrames - HoldFrames;
                float progress = contractTimer / (float)ContractFrames;
                float eased = EaseInQuad(progress);
                currentRadius = MathHelper.Lerp(MaxRadius, 0f, eased);
                currentOrbitSpeed = MaxOrbitSpeed * (1f - eased);
                overallAlpha = 1f - eased;
            }

            // Orbit rotation
            orbitAngle += currentOrbitSpeed;

            // Deal damage to enemies inside the ring
            if (timer % (int)DamageTickRate == 0 && overallAlpha > 0.3f)
            {
                DealRingDamage(owner, colors);
            }

            // Spawn flame particles at orb positions
            if (overallAlpha > 0.1f)
            {
                for (int n = 0; n < OrbCount; n++)
                {
                    if (!Main.rand.NextBool(4)) continue;

                    float orbAngle = orbitAngle + (MathHelper.TwoPi / OrbCount) * n;
                    Vector2 orbPos = owner.Center + orbAngle.ToRotationVector2() * currentRadius;

                    // Flame dust
                    Vector2 dustVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                    Color col = colors[Main.rand.Next(colors.Length)];
                    Dust dust = Dust.NewDustPerfect(
                        orbPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Torch, dustVel,
                        newColor: col,
                        Scale: Main.rand.NextFloat(1.0f, 1.8f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.3f;

                    // Light at each orb
                    Lighting.AddLight(orbPos, colors[0].ToVector3() * 0.4f * overallAlpha);
                }
            }

            // Center fire dust (ring fill effect)
            if (Main.rand.NextBool(3) && overallAlpha > 0.5f)
            {
                float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float randomDist = Main.rand.NextFloat(currentRadius * 0.3f, currentRadius * 0.9f);
                Vector2 dustPos = owner.Center + randomAngle.ToRotationVector2() * randomDist;

                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    new Vector2(0, Main.rand.NextFloat(-1.5f, -0.3f)),
                    newColor: colors[1], Scale: Main.rand.NextFloat(0.8f, 1.3f));
                dust.noGravity = true;
            }

            // Center lighting
            Lighting.AddLight(owner.Center, colors[0].ToVector3() * 0.5f * overallAlpha);

            if (timer >= TotalFrames)
                Projectile.Kill();
        }

        private void DealRingDamage(Player owner, Color[] colors)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(npc.Center, owner.Center);
                // Damage enemies within the ring radius (with some tolerance)
                if (dist < currentRadius + 30f)
                {
                    int dir = (npc.Center.X > owner.Center.X) ? 1 : -1;
                    npc.StrikeNPC(npc.CalculateHitInfo(Projectile.damage, dir, false,
                        Projectile.knockBack, Projectile.DamageType, true));

                    // Fire on hit
                    npc.AddBuff(BuffID.OnFire, 180);

                    // Hit VFX
                    for (int d = 0; d < 5; d++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Torch, vel,
                            newColor: colors[2], Scale: Main.rand.NextFloat(1.0f, 1.5f));
                        dust.noGravity = true;
                    }
                }
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (overallAlpha < 0.01f)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            Player owner = Main.player[Projectile.owner];
            Color[] colors = AFTextures.GetModeColors(AttackMode.FlamingRing);
            Vector2 center = owner.Center - Main.screenPosition;

            float time = (float)Main.timeForVisualEffects;

            // ---- ADDITIVE PASS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = AFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Texture2D glowOrb = AFTextures.GlowOrb.Value;
            Vector2 orbOrigin = glowOrb.Size() / 2f;

            // Draw the ring of fire orbs
            for (int n = 0; n < OrbCount; n++)
            {
                float orbAngle = orbitAngle + (MathHelper.TwoPi / OrbCount) * n;
                Vector2 orbScreenPos = center + orbAngle.ToRotationVector2() * currentRadius;

                float orbPulse = 0.7f + 0.3f * MathF.Sin(time * 0.08f + n * 0.8f);

                // Wide flame glow per orb (SoftGlow 1024px — target ~30px)
                sb.Draw(softGlow, orbScreenPos, null,
                    colors[0] * (overallAlpha * 0.4f * orbPulse),
                    0f, glowOrigin, 0.03f * orbPulse, SpriteEffects.None, 0f);

                // Bright orb core (GlowOrb 1024px — target ~15px)
                sb.Draw(glowOrb, orbScreenPos, null,
                    colors[2] * (overallAlpha * 0.6f * orbPulse),
                    0f, orbOrigin, 0.015f * orbPulse, SpriteEffects.None, 0f);

                // Mid fire glow (SoftGlow — target ~20px)
                sb.Draw(softGlow, orbScreenPos, null,
                    colors[1] * (overallAlpha * 0.3f),
                    0f, glowOrigin, 0.02f, SpriteEffects.None, 0f);
            }

            // Draw glow arcs between adjacent orbs (simulating the ring)
            for (int n = 0; n < OrbCount; n++)
            {
                int next = (n + 1) % OrbCount;
                float angle1 = orbitAngle + (MathHelper.TwoPi / OrbCount) * n;
                float angle2 = orbitAngle + (MathHelper.TwoPi / OrbCount) * next;
                Vector2 from = center + angle1.ToRotationVector2() * currentRadius;
                Vector2 to = center + angle2.ToRotationVector2() * currentRadius;

                DrawFireArc(sb, softGlow, glowOrigin, from, to, colors[0] * (overallAlpha * 0.2f));
            }

            // Central ambient heat glow (SoftGlow 1024px — scale to ~80px at max)
            float centerPulse = 0.8f + 0.2f * MathF.Sin(time * 0.04f);
            float centerScale = (currentRadius / MaxRadius) * 80f / softGlow.Width;
            sb.Draw(softGlow, center, null,
                colors[0] * (overallAlpha * 0.15f * centerPulse),
                0f, glowOrigin, centerScale, SpriteEffects.None, 0f);

            // Ring outline glow (PowerEffectRing)
            Texture2D ringTex = AFTextures.PowerEffectRing.Value;
            Vector2 ringOrigin = ringTex.Size() / 2f;
            float ringScale = currentRadius / (ringTex.Width / 2f);
            sb.Draw(ringTex, center, null,
                colors[1] * (overallAlpha * 0.25f),
                orbitAngle * 0.5f, ringOrigin, ringScale, SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws a glowing arc between two points using a stretched texture.
        /// </summary>
        private void DrawFireArc(SpriteBatch sb, Texture2D tex, Vector2 origin,
            Vector2 from, Vector2 to, Color color)
        {
            Vector2 diff = to - from;
            float length = diff.Length();
            float angle = diff.ToRotation();
            float scaleX = length / tex.Width;

            sb.Draw(tex, from, null, color, angle, new Vector2(0, tex.Height / 2f),
                new Vector2(scaleX, 0.01f), SpriteEffects.None, 0f);
        }

        // ---- EASING ----
        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;

        public override Color? GetAlpha(Color lightColor) => Color.Transparent;
    }
}
