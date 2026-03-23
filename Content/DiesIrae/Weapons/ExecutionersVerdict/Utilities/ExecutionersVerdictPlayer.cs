using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities
{
    public class ExecutionersVerdictPlayer : ModPlayer, IResonantOverdrive
    {
        // Verdict stacks mark sentenced enemies; execution triggers when ready
        public int verdictStacks;
        public bool executionReady;
        public int comboCounter;
        public bool isActive;
        public int activeTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingExecutionersVerdict = false;
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
            IsHoldingExecutionersVerdict = false;

            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    verdictStacks = 0;
                    executionReady = false;
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

        public void AddVerdict(int amount = 1)
        {
            verdictStacks += amount;
            activeTimer = 120;

            if (verdictStacks >= 5)
                executionReady = true;
        }

        public void ExecuteVerdict()
        {
            verdictStacks = 0;
            executionReady = false;
        }

        public float GetVerdictIntensity()
        {
            return System.Math.Min(verdictStacks / 5f, 1f);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.ExecutionersVerdictSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingExecutionersVerdict;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(130, 20, 20);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 100, 100);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 1800f))
            {
                if (npc.life <= Math.Max(1, (int)(npc.lifeMax * 0.05f)) && !npc.boss)
                {
                    npc.SimpleStrikeNPC(npc.life + 1, 0, true, 0f, DamageClass.Melee, false, 0f, true);
                }
                else
                {
                    int damage = Math.Max(1, (int)(npc.lifeMax * 0.05f));
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, DamageClass.Melee, false, 0f, true);
                }
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class ExecutionersVerdictPlayerExtensions
    {
        public static ExecutionersVerdictPlayer ExecutionersVerdict(this Player player)
            => player.GetModPlayer<ExecutionersVerdictPlayer>();
    }
}
