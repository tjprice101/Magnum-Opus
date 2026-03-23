using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities
{
    public class ThornboundReckoningPlayer : ModPlayer, IResonantOverdrive
    {
        // Thorn stacks build with each strike, escalating reckoning
        public int thornStacks;
        public int comboCounter;
        public bool isActive;
        public int activeTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingThornboundReckoning = false;
        public bool IsChargeFull => Charge >= MaxCharge;

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
            IsHoldingThornboundReckoning = false;
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    thornStacks = 0;
                    comboCounter = 0;
                }
            }
            isActive = false;
        }

        public void IncrementCombo()
        {
            comboCounter = (comboCounter + 1) % 4;
            activeTimer = 60;
        }

        public void AddThorns(int amount = 1)
        {
            thornStacks = System.Math.Min(thornStacks + amount, 10);
            activeTimer = 120;
        }

        public void ConsumeThorns()
        {
            thornStacks = 0;
        }

        public float GetThornIntensity()
        {
            return thornStacks / 10f;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == Terraria.ModLoader.ModContent.ProjectileType<Projectiles.ThornboundSwingProj>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingThornboundReckoning;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(65, 120, 60);
        Color IResonantOverdrive.OverdriveHighColor => new Color(175, 255, 145);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            int baseDamage = Math.Max(1, player.HeldItem.damage);
            HashSet<int> hit = new();
            NPC current = NpcTargetingUtils.FindClosestNpc(player.Center, 900f);
            for (int jump = 0; jump < 8 && current != null; jump++)
            {
                hit.Add(current.whoAmI);
                current.SimpleStrikeNPC(baseDamage * 2, 0, false, 0f, DamageClass.Melee, false, 0f, true);

                foreach (NPC splash in NpcTargetingUtils.EnumerateHostiles(current.Center, 90f))
                {
                    if (splash.whoAmI != current.whoAmI)
                        splash.SimpleStrikeNPC((int)(baseDamage * 0.8f), 0, false, 0f, DamageClass.Melee, false, 0f, true);
                }

                current = NpcTargetingUtils.FindClosestNpc(current.Center, 560f, hit);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class ThornboundReckoningPlayerExtensions
    {
        public static ThornboundReckoningPlayer ThornboundReckoning(this Player player)
            => player.GetModPlayer<ThornboundReckoningPlayer>();
    }
}
