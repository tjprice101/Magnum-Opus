using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities
{
    /// <summary>
    /// Per-player state tracking for Dual Fated Chime.
    /// Tracks 5-phase inferno waltz combo and Bell Resonance state.
    /// </summary>
    public class DualFatedChimePlayer : ModPlayer, IResonantOverdrive
    {
        private int _overdriveSpinTimer;

        #region Combo Tracking (5-Phase Inferno Waltz)

        /// <summary>Current combo step:
        /// 0 = Opening Peal (right horizontal)
        /// 1 = Answer (left diagonal, faster) 
        /// 2 = Escalation (right upward arc + flame wave)
        /// 3 = Resonance (left downward slam + double shockwave + ground fire)
        /// 4 = Grand Toll (cross-slash + 12 directional flame waves)
        /// </summary>
        public int ComboStep;

        /// <summary>Number of combo phases.</summary>
        public const int ComboPhaseCount = 5;

        /// <summary>Ticks since last swing, for combo reset.</summary>
        public int ComboResetTimer;

        /// <summary>Frames of inactivity before combo resets.</summary>
        public const int ComboResetDelay = 60;

        /// <summary>Flame Waltz Dodge: i-frames granted after completing all 5 combo phases.</summary>
        public int WaltzBuffTimer;

        #endregion

        #region Charge Meter
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingDualFatedChime = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        public void AddCharge(float amount)
        {
            Charge = MathHelper.Clamp(Charge + amount, 0f, MaxCharge);
        }

        public void ConsumeCharge()
        {
            Charge = 0f;
        }
        #endregion

        public override void ResetEffects()
        {
            IsHoldingDualFatedChime = false;

            // Tick combo reset
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ComboStep = 0;
            }

            // Flame Waltz Dodge: brief invulnerability
            if (WaltzBuffTimer > 0)
            {
                WaltzBuffTimer--;
                Player.immune = true;
                Player.immuneTime = 2;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Accept all DualFatedChime projectile hits
            if (proj.type == ModContent.ProjectileType<Projectiles.DualFatedChimeSwingProj>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        #region IResonantOverdrive

        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingDualFatedChime;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(255, 95, 20);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 220, 90);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;
            _overdriveSpinTimer = 180;
            ConsumeCharge();
            return true;
        }

        #endregion

        public override void PostUpdate()
        {
            if (_overdriveSpinTimer > 0)
            {
                _overdriveSpinTimer--;
                if (_overdriveSpinTimer % 8 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, Player.HeldItem.damage * 2);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = Main.GameUpdateCount * 0.14f + i * MathHelper.PiOver2;
                        Vector2 origin = Player.Center + angle.ToRotationVector2() * 80f;
                        NPC target = NpcTargetingUtils.FindClosestNpc(origin, 900f);
                        Vector2 vel = target != null ? origin.DirectionTo(target.Center) * 14f : angle.ToRotationVector2() * 14f;
                        Projectile.NewProjectile(Player.GetSource_FromThis(), origin, vel, ProjectileID.MagicDagger, damage, 2f, Player.whoAmI);
                    }
                }
            }
        }

        /// <summary>Advance the combo step (0-4) and reset the timer.</summary>
        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % ComboPhaseCount;
            ComboResetTimer = ComboResetDelay;
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class DualFatedChimePlayerExtensions
    {
        public static DualFatedChimePlayer DualFatedChime(this Player player)
            => player.GetModPlayer<DualFatedChimePlayer>();
    }
}
