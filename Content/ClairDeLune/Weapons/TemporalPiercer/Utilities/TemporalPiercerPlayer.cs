using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities
{
    public class TemporalPiercerPlayer : ModPlayer, IResonantOverdrive
    {
        // Pierce stacks build toward a time freeze moment
        public int pierceStacks;
        public bool timeFreezeReady;
        public int comboCounter;
        public bool isActive;
        public int activeTimer;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingTemporalPiercer = false;
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
            IsHoldingTemporalPiercer = false;
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    pierceStacks = 0;
                    timeFreezeReady = false;
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

        public void AddPierceStack(int amount = 1)
        {
            pierceStacks = System.Math.Min(pierceStacks + amount, 8);
            activeTimer = 120;

            if (pierceStacks >= 8)
                timeFreezeReady = true;
        }

        public void TriggerTimeFreeze()
        {
            pierceStacks = 0;
            timeFreezeReady = false;
        }

        public float GetPierceIntensity()
        {
            return pierceStacks / 8f;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == Terraria.ModLoader.ModContent.ProjectileType<Projectiles.TemporalThrustProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingTemporalPiercer;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(100, 120, 180);
        Color IResonantOverdrive.OverdriveHighColor => new Color(165, 220, 255);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            NPC nearest = NpcTargetingUtils.FindClosestNpc(player.Center, 2200f);
            if (nearest != null)
            {
                Vector2 offset = new Vector2(-player.direction * 64f, 0f);
                player.Teleport(nearest.Center + offset, TeleportationStyleID.RodOfDiscord);
                int burst = Math.Max(1, (int)(nearest.lifeMax * 0.10f));
                nearest.SimpleStrikeNPC(burst, player.direction, true, 0f, DamageClass.Melee, false, 0f, true);

                int heal = Math.Max(1, (int)(player.statLifeMax2 * 0.15f));
                player.statLife = Math.Min(player.statLifeMax2, player.statLife + heal);
                player.HealEffect(heal, true);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class TemporalPiercerPlayerExtensions
    {
        public static TemporalPiercerPlayer TemporalPiercer(this Player player)
            => player.GetModPlayer<TemporalPiercerPlayer>();
    }
}
