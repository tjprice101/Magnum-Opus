using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer;
using System.Collections.Generic;
using System;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Large ricochet beam that bounces between enemies 5-6 times.
    /// All VFX delegated to FuneralPrayerVFX module.
    /// </summary>
    public class FuneralPrayerRicochetBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/LightningStreak";

        private int currentTarget = -1;
        private List<int> hitEnemies = new List<int>();
        private int ricochetsRemaining = 5;
        private bool isRicocheting = false;
        private const float MaxRicochetRange = 500f;
        private Vector2 lastHitPosition;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            Projectile.light = 0f; // Lighting handled by VFX module
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Initialize ricochet count
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                ricochetsRemaining = Main.rand.Next(5, 7);
            }

            // Find next target if needed
            if (currentTarget < 0 || !Main.npc[currentTarget].active || isRicocheting)
            {
                FindNextTarget();
                isRicocheting = false;
            }

            // Track towards current target
            if (currentTarget >= 0 && Main.npc[currentTarget].active)
            {
                Vector2 direction = (Main.npc[currentTarget].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = direction * 20f;

                if (Vector2.Distance(Projectile.Center, Main.npc[currentTarget].Center) < 50f)
                {
                    HitCurrentTarget();
                }
            }
            else if (ricochetsRemaining <= 0)
            {
                Projectile.Kill();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ══╁EALL VFX delegated to module ══╁E
            FuneralPrayerVFX.RicochetBeamTrailVFX(Projectile);
        }

        private void FindNextTarget()
        {
            if (ricochetsRemaining <= 0)
            {
                currentTarget = -1;
                return;
            }

            int nextTarget = -1;
            float minDistance = MaxRicochetRange;
            Vector2 searchFrom = lastHitPosition != Vector2.Zero ? lastHitPosition : Projectile.Center;

            // Prioritize bosses
            bool foundBoss = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage && !hitEnemies.Contains(i))
                {
                    float distance = Vector2.Distance(searchFrom, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextTarget = i;
                        foundBoss = true;
                    }
                }
            }

            if (!foundBoss)
            {
                minDistance = MaxRicochetRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && !hitEnemies.Contains(i))
                    {
                        float distance = Vector2.Distance(searchFrom, npc.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nextTarget = i;
                        }
                    }
                }
            }

            currentTarget = nextTarget;
        }

        private void HitCurrentTarget()
        {
            if (currentTarget < 0 || !Main.npc[currentTarget].active)
                return;

            NPC target = Main.npc[currentTarget];

            // Game logic: damage + debuff
            target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 360);

            // Delegate hit VFX to module  Eheroic impact, requiem burst, halos, music notes
            FuneralPrayerVFX.RicochetBeamHitVFX(target.Center);

            // Massive explosion dust
            for (int i = 0; i < 30; i++)
            {
                Dust shock = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 3.5f);
                shock.noGravity = true;
                shock.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            for (int i = 0; i < 20; i++)
            {
                Dust energy = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 2.5f);
                energy.noGravity = true;
                energy.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            for (int i = 0; i < 15; i++)
            {
                Dust fire = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.Torch, 0f, 0f, 100, new Color(255, 80, 30), 2.0f);
                fire.noGravity = true;
                fire.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.8f, Pitch = -0.2f }, target.position);

            hitEnemies.Add(currentTarget);
            lastHitPosition = target.Center;
            ricochetsRemaining--;
            isRicocheting = true;
            currentTarget = -1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Delegate rendering to VFX module
            return FuneralPrayerVFX.DrawRicochetBeam(Main.spriteBatch, Projectile, ref lightColor);
        }

        public override void OnKill(int timeLeft)
        {
            // Delegate death VFX to module  Eheroic flash, radial dust, mourning flames, ash
            FuneralPrayerVFX.RicochetBeamDeathVFX(Projectile.Center);
        }
    }
}
