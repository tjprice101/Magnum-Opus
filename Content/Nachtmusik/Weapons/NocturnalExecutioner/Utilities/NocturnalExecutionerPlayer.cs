using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities
{
    public class NocturnalExecutionerPlayer : ModPlayer, IResonantOverdrive
    {
        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingNocturnalExecutioner = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        private int _overdriveOrbTimer;

        public void AddCharge(float amount)
        {
            Charge = System.Math.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
        }

        public override void ResetEffects()
        {
            IsHoldingNocturnalExecutioner = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.NocturnalExecutionerSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingNocturnalExecutioner;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(60, 55, 120);
        Color IResonantOverdrive.OverdriveHighColor => new Color(185, 130, 255);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            int baseDamage = Math.Max(1, (int)(player.HeldItem.damage * 2.0f));

            if (!Main.dayTime)
            {
                // Nighttime: instakill all non-boss enemies on screen
                foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 1200f))
                {
                    if (!npc.boss)
                        npc.SimpleStrikeNPC(npc.lifeMax + npc.defense, 0, false, 0f, DamageClass.Melee, false, 0f, true);
                }
            }
            else
            {
                // Daytime: start orb timer
                _overdriveOrbTimer = 300;
            }

            ConsumeCharge();
            return true;
        }

        public override void PostUpdate()
        {
            if (_overdriveOrbTimer > 0)
            {
                _overdriveOrbTimer--;
                if (_overdriveOrbTimer % 20 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, (int)(Player.HeldItem.damage * 1.6f));
                    for (int i = 0; i < 5; i++)
                    {
                        NPC target = NpcTargetingUtils.FindClosestNpc(Player.Center, 1200f);
                        if (target == null)
                            break;

                        Vector2 spawn = Player.Center + Main.rand.NextVector2Circular(55f, 55f);
                        Vector2 velocity = spawn.DirectionTo(target.Center) * Main.rand.NextFloat(7f, 10f);
                        Projectile.NewProjectile(Player.GetSource_FromThis(), spawn, velocity, ProjectileID.InfernoFriendlyBolt, damage, 2f, Player.whoAmI);

                        foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(target.Center, 120f))
                            npc.SimpleStrikeNPC(damage / 2, 0, false, 0f, DamageClass.Melee, false, 0f, true);
                    }
                }
            }
        }
    }

    public static class NocturnalExecutionerPlayerExtensions
    {
        public static NocturnalExecutionerPlayer NocturnalExecutioner(this Player player)
            => player.GetModPlayer<NocturnalExecutionerPlayer>();
    }
}
