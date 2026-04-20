using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles
{
    public class CelestialMuseMinion : ModProjectile
    {
        private float hoverAngle;
        private int attackCooldown;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/GalacticOverture/CelestialMuseMinion";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            hoverAngle += 0.02f;
            attackCooldown = Math.Max(0, attackCooldown - 1);

            float hoverOffset = (float)Math.Sin(hoverAngle) * 30f;
            Vector2 idealPos = owner.Center + new Vector2(owner.direction * -60f, -50f + hoverOffset);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.08f);
            Projectile.spriteDirection = owner.direction;

            NPC target = FindTarget(owner, 700f);
            if (target != null && attackCooldown == 0)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 14f,
                    ModContent.ProjectileType<MuseNoteProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                attackCooldown = 20;
            }

            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    -Projectile.velocity * 0.1f, 0, default, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.1f, 0.12f, 0.2f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GalacticOvertureBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<GalacticOvertureBuff>()))
                Projectile.timeLeft = 2;
            return true;
        }

        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
