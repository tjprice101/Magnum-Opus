using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom;
using static MagnumOpus.Common.Systems.Particles.Particle;

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
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
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

            // Per-frame VFX trail — delegated to VFX module
            SakurasBlossomVFX.SpectralCopyTrailVFX(Projectile);

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

            // Dynamic palette-based lighting
            Color lightColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold,
                (float)Math.Sin(AgeTimer * 0.08f) * 0.5f + 0.5f);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // VFX: delegated to VFX module for consistent impact rendering
            SakurasBlossomVFX.SpectralCopyHitVFX(target.Center);

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
            // VFX: delegated to VFX module for consistent death rendering
            SakurasBlossomVFX.SpectralCopyDeathVFX(Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Full rendering delegated to VFX module for consistent
            // 5-layer pipeline: bloom trail → afterimage → shimmer → bloom stack → main
            return SakurasBlossomVFX.DrawSpectralCopy(Main.spriteBatch, Projectile, ref lightColor);
        }
    }
}
