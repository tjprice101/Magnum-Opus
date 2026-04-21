using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Buffs;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.GalacticOverture.Projectiles
{
    /// <summary>
    /// Celestial Muse Minion — fires 3 note types in sequence.
    /// Quarter note: fast, straight, short life.
    /// Half note: medium, gentle homing, longer life.
    /// Whole note: slow, strong homing, zone on kill.
    /// Completing a 3-note measure grants +50% damage to the next note.
    /// </summary>
    public class CelestialMuseMinion : ModProjectile
    {
        private float hoverAngle;
        private int attackCooldown;
        private int notePhase; // 0=quarter, 1=half, 2=whole
        private bool measureBonus; // true after completing a cycle

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
                FireNote(target);
                attackCooldown = 20;
            }

            // Nachtmusik dust trail
            if (Main.rand.NextBool(4))
            {
                Color dustCol = Main.rand.NextBool() ? NachtmusikPalette.StarGold : NachtmusikPalette.StarlitBlue;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f, 0, dustCol, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.18f, 0.3f);
        }

        private void FireNote(NPC target)
        {
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            int damage = measureBonus ? (int)(Projectile.damage * 1.5f) : Projectile.damage;

            switch (notePhase)
            {
                case 0: // Quarter note: fast straight, short life
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 18f,
                        damage, Projectile.knockBack, Projectile.owner,
                        0.0f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK, 0.7f, 60);
                    break;

                case 1: // Half note: medium, gentle homing
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 12f,
                        damage, Projectile.knockBack, Projectile.owner,
                        0.06f, 0, GenericHomingOrbChild.THEME_NACHTMUSIK, 0.9f, 120);
                    break;

                case 2: // Whole note: slow, strong homing, zone on kill
                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 6f,
                        damage, Projectile.knockBack, Projectile.owner,
                        0.12f, GenericHomingOrbChild.FLAG_ZONE_ON_KILL,
                        GenericHomingOrbChild.THEME_NACHTMUSIK, 1.1f, 180);
                    break;
            }

            // Advance note phase
            notePhase++;
            if (notePhase >= 3)
            {
                notePhase = 0;
                measureBonus = true; // Grant bonus for completing measure
            }
            else
            {
                measureBonus = false;
            }
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
