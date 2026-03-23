using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities
{
    public class ChronologicalityPlayer : ModPlayer, IResonantOverdrive
    {
        // Temporal charge accumulates; time slow bends the flow of combat
        public int temporalCharge;
        public bool timeSlowActive;
        public int comboCounter;
        public bool isActive;
        public int activeTimer;

        // === Damage Replay Tracking ===
        private readonly List<(int tick, int damage)> _recentDamage = new();

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingChronologicality = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        public void AddCharge(float amount)
        {
            Charge = System.Math.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public override void ResetEffects()
        {
            IsHoldingChronologicality = false;
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    temporalCharge = 0;
                    timeSlowActive = false;
                    comboCounter = 0;
                }
            }
            isActive = false;

            // Clean up damage records older than 5 seconds
            int currentTick = (int)Main.GameUpdateCount;
            _recentDamage.RemoveAll(r => currentTick - r.tick > 300);
        }

        public void IncrementCombo()
        {
            comboCounter = (comboCounter + 1) % 4;
            activeTimer = 60;
        }

        public void AddTemporalCharge(int amount = 1)
        {
            temporalCharge = System.Math.Min(temporalCharge + amount, 10);
            activeTimer = 120;
        }

        public void ActivateTimeSlow()
        {
            timeSlowActive = true;
            activeTimer = 180;
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
            temporalCharge = 0;
            timeSlowActive = false;
        }

        public float GetTemporalIntensity()
        {
            return temporalCharge / 10f;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == Terraria.ModLoader.ModContent.ProjectileType<Projectiles.ChronologicalitySwing>())
            {
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
                _recentDamage.Add(((int)Main.GameUpdateCount, Math.Max(1, damageDone)));
            }
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingChronologicality;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(95, 85, 150);
        Color IResonantOverdrive.OverdriveHighColor => new Color(175, 220, 255);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            int baseDamage = Math.Max(1, player.HeldItem.damage);
            int total = 0;
            for (int i = 0; i < _recentDamage.Count; i++)
                total += _recentDamage[i].damage;

            total = Math.Max(total * 3, baseDamage * 5);
            List<NPC> targets = NpcTargetingUtils.CollectHostiles(player.Center, 1000f);
            if (targets.Count == 0)
                targets = NpcTargetingUtils.CollectHostiles(player.Center, 1700f);

            if (targets.Count > 0)
            {
                int each = Math.Max(1, total / targets.Count);
                for (int i = 0; i < targets.Count; i++)
                    targets[i].SimpleStrikeNPC(each, 0, false, 0f, DamageClass.Melee, false, 0f, true);
            }

            _recentDamage.Clear();
            ConsumeCharge();
            return true;
        }
    }

    public static class ChronologicalityPlayerExtensions
    {
        public static ChronologicalityPlayer Chronologicality(this Player player)
            => player.GetModPlayer<ChronologicalityPlayer>();
    }
}
