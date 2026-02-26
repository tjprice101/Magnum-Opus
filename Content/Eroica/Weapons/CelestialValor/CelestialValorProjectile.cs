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
using MagnumOpus.Content.Eroica;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor energy slash projectile — "The Hero's Echo".
    /// 
    /// A blazing arc of heroic determination that seeks nearby enemies,
    /// leaving a wake of flame dust and valor sparks. On impact, detonates
    /// in a multi-layered heroic explosion with bloom cascades and lightning.
    /// 
    /// Enhanced features:
    ///   - Stronger homing with acceleration curve (gentle at start, aggressive near death)
    ///   - Pulsating visual scale tied to projectile age
    ///   - Expanded AOE explosion radius for finisher feel
    ///   - 33% chance seeking crystals on hit
    ///   - 12-position trail cache for smooth afterimage rendering
    /// </summary>
    public class CelestialValorProjectile : ModProjectile
    {
        // AI state
        private ref float HomingAccel => ref Projectile.ai[0];
        private ref float AgeTimer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.8f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            AgeTimer++;

            // Face direction of travel with slight rotation smoothing
            float targetRot = Projectile.velocity.ToRotation();
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRot, 0.3f);

            // Pulsating visual scale — subtle breathing tied to age
            float pulse = 1f + (float)Math.Sin(AgeTimer * 0.15f) * 0.06f;
            Projectile.scale = pulse;

            // Enhanced homing: gentle at first, more aggressive as projectile ages
            float homingRange = 180f;
            float baseHoming = 0.018f;
            float ageFactor = MathHelper.Clamp(AgeTimer / 60f, 0f, 1f); // 0→1 over 1 second
            float homingStrength = baseHoming + ageFactor * 0.025f; // Ramps from 0.018 to 0.043

            NPC closestNPC = null;
            float closestDist = homingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }

            if (closestNPC != null)
            {
                Vector2 toTarget = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                    toTarget * Projectile.velocity.Length(), homingStrength);
            }

            // Slight speed decay to give "weight" to the projectile
            if (AgeTimer > 40)
            {
                Projectile.velocity *= 0.998f;
            }

            // Trail VFX — enhanced per-frame particles
            CelestialValorVFX.ProjectileTrailVFX(Projectile);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // AOE explosion VFX
            CreateAOEExplosion(target.Center);
            CelestialValorVFX.ProjectileHitVFX(target.Center);

            // Seeking crystals — 33% chance on hit
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.20f),
                    Projectile.knockBack,
                    Projectile.owner,
                    5
                );
            }

            // Splash damage to nearby enemies (5% of hit damage, 110f radius)
            int explosionDamage = (int)(damageDone * 0.05f);
            float explosionRadius = 110f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && npc.whoAmI != target.whoAmI)
                {
                    if (Vector2.Distance(npc.Center, target.Center) <= explosionRadius)
                    {
                        npc.SimpleStrikeNPC(explosionDamage, 0, false, 0f, DamageClass.Melee);
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateAOEExplosion(Projectile.Center);
            return true;
        }

        private void CreateAOEExplosion(Vector2 position)
        {
            CelestialValorVFX.AOEExplosion(position);
        }

        public override void OnKill(int timeLeft)
        {
            // Always play death flash (removed the timeLeft > 0 guard — projectiles
            // killed by penetrate exhaustion should also flash)
            CelestialValorVFX.DeathFlash(Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return CelestialValorVFX.DrawProjectile(Main.spriteBatch, Projectile, ref lightColor);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Warm heroic tint with slight transparency
            return new Color(255, 242, 210, 175);
        }
    }
}
