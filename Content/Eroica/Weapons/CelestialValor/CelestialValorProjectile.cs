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
    /// Projectile fired by Celestial Valor greatsword on swing.
    /// A powerful energy slash with red and gold effects.
    /// Collides with walls and creates gold/red AOE explosions on impact.
    /// </summary>
    public class CelestialValorProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
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
            Projectile.tileCollide = true; // Now collides with walls
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.7f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Face direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Slight homing toward nearby enemies
            float homingRange = 150f;
            float homingStrength = 0.02f;
            
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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }
            
            // Trail VFX (consolidated in CelestialValorVFX)
            CelestialValorVFX.ProjectileTrailVFX(Projectile);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // AOE explosion + VFX (consolidated in CelestialValorVFX)
            CreateAOEExplosion(target.Center);
            CelestialValorVFX.ProjectileHitVFX(target.Center);
            
            // === SEEKING CRYSTALS - Celestial valor burst ===
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
            
            // Deal 5% bonus explosion damage to nearby enemies
            int explosionDamage = (int)(damageDone * 0.05f);
            float explosionRadius = 100f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && npc.whoAmI != target.whoAmI)
                {
                    if (Vector2.Distance(npc.Center, target.Center) <= explosionRadius)
                    {
                        Player player = Main.player[Projectile.owner];
                        npc.SimpleStrikeNPC(explosionDamage, 0, false, 0f, DamageClass.Melee);
                    }
                }
            }
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Create AOE explosion on wall collision
            CreateAOEExplosion(Projectile.Center);
            return true; // Destroy projectile
        }

        private void CreateAOEExplosion(Vector2 position)
        {
            CelestialValorVFX.AOEExplosion(position);
        }
        
        // Lightning VFX consolidated in CelestialValorVFX.SpawnLightning

        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 0)
                return;
            CelestialValorVFX.DeathFlash(Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return CelestialValorVFX.DrawProjectile(Main.spriteBatch, Projectile, ref lightColor);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 240, 200, 180);
        }
    }
}
