using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking combo state, phase timing, and
    /// spectral blade cooldown for Requiem of Reality.
    /// </summary>
    public class RequiemPlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current swing count for combo tracking (0-3, resets on 4th).</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 120 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 120;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Whether the player just triggered a spectral blade combo.</summary>
        public bool JustTriggeredCombo;

        /// <summary>Cooldown ticks before another spectral blade can spawn.</summary>
        public int SpectralBladeCooldown;

        /// <summary>Total attacks performed (for escalating effects).</summary>
        public int TotalAttacks;

        /// <summary>Current musical "movement" (cycles 0-3 for visual variety).</summary>
        public int MusicalMovement => TotalAttacks % 4;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingRequiemOfReality = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        // === Overdrive Cooldown ===
        private int _requiemCooldown;

        public void AddCharge(float amount)
        {
            Charge = MathHelper.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
        }

        public override void ResetEffects()
        {
            IsHoldingRequiemOfReality = false;
            JustTriggeredCombo = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    SwingCounter = 0;
                    ComboIntensity = 0f;
                }
            }

            if (SpectralBladeCooldown > 0)
                SpectralBladeCooldown--;

            // Decay combo intensity slowly
            ComboIntensity *= 0.995f;

            // Overdrive cooldown
            if (_requiemCooldown > 0)
                _requiemCooldown--;
        }

        /// <summary>Called on each swing. Returns true if this was a combo trigger swing.</summary>
        public bool OnSwing()
        {
            SwingCounter++;
            TotalAttacks++;
            ComboResetTimer = ComboResetDelay;

            // Build intensity with each swing
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.25f, 0f, 1f);

            if (SwingCounter >= 4 && SpectralBladeCooldown <= 0)
            {
                SwingCounter = 0;
                JustTriggeredCombo = true;
                SpectralBladeCooldown = 60; // 1 second cooldown
                ComboIntensity = 1f; // Max intensity on combo
                return true;
            }

            return false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.RequiemSwingProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingRequiemOfReality;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(90, 30, 110);
        Color IResonantOverdrive.OverdriveHighColor => new Color(230, 130, 255);
        bool IResonantOverdrive.IsOverdriveOnCooldown => _requiemCooldown > 0;
        string IResonantOverdrive.OverdriveCooldownMessage => "Requiem cooling down";

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            _requiemCooldown = 1800; // 30 seconds
            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 1900f))
            {
                if (!npc.boss)
                    npc.AddBuff(BuffID.Frozen, 180);
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.hostile)
                    proj.GetGlobalProjectile<ResonantOverdriveGlobalProjectile>().FreezeFor(180);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class RequiemPlayerExtensions
    {
        public static RequiemPlayer Requiem(this Player player)
            => player.GetModPlayer<RequiemPlayer>();
    }
}
