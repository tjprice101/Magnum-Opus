using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spectral copy of Sakura's Blossom — homing phantom blade that seeks enemies
    /// and detonates in petal bursts. Enhanced with acceleration-based homing,
    /// pulsating visual scale, and full VFX module integration.
    /// 
    /// Trail cache: 20 positions for dramatic arc sweep rendering.
    /// Rendering: Delegated to SakurasBlossomVFX.DrawSpectralCopy for
    /// consistent 5-layer bloom + afterimage + perpendicular shimmer pipeline.
    /// </summary>
    public class SakurasBlossomSpectral : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom";

        // AI state tracking
        private ref float HomingAccel => ref Projectile.ai[0];
        private ref float AgeTimer => ref Projectile.ai[1];

        private int targetNPC = -1;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 80;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            AgeTimer++;

            // Align rotation with velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Pulsating visual scale — breathing blossom effect
            float scaleBase = 1.0f;
            float scalePulse = (float)Math.Sin(AgeTimer * 0.12f) * 0.08f;
            Projectile.scale = scaleBase + scalePulse;

            // ── ENHANCED HOMING — acceleration curve ──
            // Homing strengthens over time: starts gentle, becomes aggressive
            float ageSeconds = AgeTimer / 60f;
            HomingAccel = MathHelper.Lerp(0.020f, 0.048f, MathHelper.Clamp(ageSeconds, 0f, 1f));

            // Find target — prioritize bosses
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = -1;
                float maxDistance = 850f;
                bool foundBoss = false;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distance < maxDistance)
                        {
                            maxDistance = distance;
                            targetNPC = i;
                            foundBoss = true;
                        }
                    }
                }

                if (!foundBoss)
                {
                    maxDistance = 650f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float distance = Vector2.Distance(Projectile.Center, npc.Center);
                            if (distance < maxDistance)
                            {
                                maxDistance = distance;
                                targetNPC = i;
                            }
                        }
                    }
                }
            }

            // Apply homing with acceleration curve + rotation smoothing
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = (Main.npc[targetNPC].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = Projectile.velocity.Length();
                float turnWeight = 18f - HomingAccel * 80f; // tighter turns as age increases
                turnWeight = MathHelper.Clamp(turnWeight, 12f, 20f);
                Projectile.velocity = (Projectile.velocity * turnWeight + direction * speed) / (turnWeight + 1f);
            }

            // Slight speed decay after 50 ticks — gives a weighted feel
            if (AgeTimer > 50)
            {
                Projectile.velocity *= 0.998f;
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Seeking crystals — 33% chance
            if (Main.rand.NextBool(3) && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with
            {
                Pitch = 0.15f,
                Volume = 0.7f
            }, Projectile.position);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
}
