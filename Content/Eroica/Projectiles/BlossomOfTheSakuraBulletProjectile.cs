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
using MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Bullet projectile for Blossom of the Sakura — heat-reactive homing tracer.
    /// 
    /// Enhanced: AI state tracking for heat propagation, acceleration-based homing,
    /// pulsating scale, all VFX delegated to BlossomOfTheSakuraVFX module.
    /// 
    /// ai[0] = heatProgress (0-1, set by weapon item via Shoot)
    /// ai[1] = age timer
    /// </summary>
    public class BlossomOfTheSakuraBulletProjectile : ModProjectile
    {
        private ref float HeatProgress => ref Projectile.ai[0];
        private ref float AgeTimer => ref Projectile.ai[1];

        private int targetNPC = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            AgeTimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Pulsating visual scale — subtle heat shimmer
            float pulse = (float)Math.Sin(AgeTimer * 0.15f) * 0.06f;
            Projectile.scale = 1f + pulse + HeatProgress * 0.1f;

            // ── Per-frame VFX — delegated to VFX module ──
            BlossomOfTheSakuraVFX.BulletTrailVFX(Projectile, HeatProgress);

            // ── HOMING AI — boss-priority with gentle tracking ──
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

            // Gentle homing — tighter when hot, looser when cool
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = (Main.npc[targetNPC].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = Projectile.velocity.Length();
                float turnWeight = 28f - HeatProgress * 6f; // hotter = tighter turns
                turnWeight = MathHelper.Clamp(turnWeight, 20f, 30f);
                Projectile.velocity = (Projectile.velocity * turnWeight + direction * speed) / (turnWeight + 1f);
            }

            // Dynamic palette-based lighting
            Color lightColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, HeatProgress);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * (0.4f + HeatProgress * 0.35f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // VFX: delegated to VFX module
            BlossomOfTheSakuraVFX.BulletHitVFX(target.Center, HeatProgress);

            // Seeking crystals — 25% chance
            if (Main.rand.NextBool(4) && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 0.55f }, Projectile.position);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            BlossomOfTheSakuraVFX.BulletDeathVFX(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.4f }, Projectile.position);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            BlossomOfTheSakuraVFX.BulletDeathVFX(Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Full rendering delegated to VFX module:
            // {A=0} bloom trail → afterimage → heat-reactive bloom stack → main sprite
            return BlossomOfTheSakuraVFX.DrawBulletProjectile(Main.spriteBatch, Projectile, HeatProgress, ref lightColor);
        }
    }
}
