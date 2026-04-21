using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Buffs;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.CelestialChorusBaton.Projectiles
{
    /// <summary>
    /// Nocturnal Guardian Minion — fires orbs in 3-hit combo patterns.
    /// Hit 1: straight orb. Hit 2: homing orb. Hit 3: homing orb + zone on kill.
    /// Contact hits apply CelestialHarmony.
    /// </summary>
    public class NocturnalGuardianMinion : ModProjectile
    {
        private float orbitAngle;
        private int attackCooldown;
        private bool isAttacking;
        private Vector2 attackTarget;
        private int attackTimer;
        private int comboPhase; // 0, 1, 2

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/CelestialChorusBaton/NocturnalGuardianMinion";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner))
                return;

            orbitAngle += 0.04f;
            attackCooldown = Math.Max(0, attackCooldown - 1);

            NPC target = FindTarget(owner, 800f);

            if (!isAttacking)
            {
                // Orbit owner
                float orbitRadius = 80f + 30f * (float)Math.Sin(orbitAngle * 0.5f);
                Vector2 idealPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.15f, 0.1f);

                if (target != null && attackCooldown == 0)
                {
                    // Fire orb based on combo phase instead of dashing
                    FireComboOrb(target);
                    attackCooldown = 45;
                }
            }
            else
            {
                // Brief lunge for contact damage
                attackTimer++;
                if (attackTimer < 15)
                {
                    Vector2 toTarget = (attackTarget - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toTarget * 22f;
                }
                else
                {
                    isAttacking = false;
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.02f;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;

            // Nachtmusik dust trail
            if (Main.rand.NextBool(4))
            {
                Color dustCol = Main.rand.NextBool() ? NachtmusikPalette.CosmicPurple : NachtmusikPalette.RadianceGold;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0, dustCol, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.15f, 0.35f);
        }

        private void FireComboOrb(NPC target)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            switch (comboPhase)
            {
                case 0: // Straight orb
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 14f,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        0.0f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK, 0.8f, 60);
                    break;

                case 1: // Homing orb
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 12f,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        0.08f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK, 1f, 90);
                    break;

                case 2: // Homing orb + zone on kill
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 10f,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        0.10f, GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                        GenericHomingOrbChild.THEME_NACHTMUSIK, 1.2f, 120);

                    // Also lunge for contact
                    isAttacking = true;
                    attackTarget = target.Center;
                    attackTimer = 0;
                    break;
            }

            comboPhase = (comboPhase + 1) % 3;
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CelestialChorusBatonBuff>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<CelestialChorusBatonBuff>()))
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
