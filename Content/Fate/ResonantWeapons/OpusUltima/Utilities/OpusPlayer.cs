using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking combo state for Opus Ultima.
    /// 3-movement combo cycle: Exposition → Development → Recapitulation.
    /// Extension method: player.Opus()
    /// </summary>
    public class OpusPlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current swing count for combo tracking (0-2, cycles through 3 movements).</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 120 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 120;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Total attacks performed (for escalating effects).</summary>
        public int TotalAttacks;

        /// <summary>Current musical movement (0=Exposition, 1=Development, 2=Recapitulation).</summary>
        public int CurrentMovement => SwingCounter % 3;

        /// <summary>Whether the current swing just triggered the Recapitulation (Movement III).</summary>
        public bool JustTriggeredRecap;

        /// <summary>Whether energy balls have been fired this swing.</summary>
        public bool EnergyBallsFired;

        // === OPUS RESONANCE PASSIVE ===
        /// <summary>Opus Resonance stacks (0-9). Each completed movement grants +1 stack. +5% all damage per stack.</summary>
        public int OpusResonanceStacks;

        /// <summary>Maximum Opus Resonance stacks (3 full movement cycles × 3 movements).</summary>
        public const int MaxResonanceStacks = 9;

        /// <summary>Ticks since last resonance gain. Stacks decay after 600 ticks (10s) out of combat.</summary>
        public int ResonanceDecayTimer;
        private const int ResonanceDecayDelay = 600;

        /// <summary>Damage bonus multiplier from Opus Resonance (1.0 + stacks * 0.05).</summary>
        public float ResonanceDamageMultiplier => 1f + OpusResonanceStacks * 0.05f;

        /// <summary>Whether the Grand Finale was just triggered this frame.</summary>
        public bool GrandFinaleTriggered;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingOpusUltima = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        private int _overdriveBarrageTimer;

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
            IsHoldingOpusUltima = false;
            JustTriggeredRecap = false;
            GrandFinaleTriggered = false;
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

            // Decay combo intensity slowly
            ComboIntensity *= 0.995f;

            // Opus Resonance decay timer
            if (ResonanceDecayTimer > 0)
            {
                ResonanceDecayTimer--;
                if (ResonanceDecayTimer <= 0 && OpusResonanceStacks > 0)
                {
                    OpusResonanceStacks--;
                    ResonanceDecayTimer = 120; // Lose 1 stack every 2s after decay starts
                }
            }

            // === Overdrive Timer ===
            if (_overdriveBarrageTimer > 0)
            {
                _overdriveBarrageTimer--;
                if (_overdriveBarrageTimer % 10 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, (int)(Player.HeldItem.damage * 2.3f));
                    for (int i = 0; i < 7; i++)
                    {
                        Vector2 spawn = Player.Center + Main.rand.NextVector2Circular(100f, 100f);
                        Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f));
                        Projectile.NewProjectile(Player.GetSource_FromThis(), spawn, velocity, ProjectileID.Grenade, damage, 4f, Player.whoAmI);
                    }
                }
            }
        }

        /// <summary>
        /// Called on each swing. Returns the movement index (0-2) for this swing.
        /// Also sets JustTriggeredRecap if this is Movement III.
        /// </summary>
        public int OnSwing()
        {
            int movement = CurrentMovement;
            SwingCounter++;
            TotalAttacks++;
            ComboResetTimer = ComboResetDelay;
            EnergyBallsFired = false;

            // Build intensity with each swing
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.33f, 0f, 1f);

            if (movement == 2)
            {
                JustTriggeredRecap = true;
                ComboIntensity = 1f; // Max intensity on Recapitulation

                // Grant Opus Resonance stack on each completed movement cycle
                if (OpusResonanceStacks < MaxResonanceStacks)
                    OpusResonanceStacks++;
                ResonanceDecayTimer = ResonanceDecayDelay;
            }

            return movement;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.OpusSwingProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingOpusUltima;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(130, 45, 110);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 165, 225);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            int baseDamage = Math.Max(1, (int)(player.HeldItem.damage * 3f));

            // Slam nearby enemies
            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 360f))
                npc.SimpleStrikeNPC(baseDamage, 0, false, 0f, DamageClass.Melee, false, 0f, true);

            _overdriveBarrageTimer = 120;

            ConsumeCharge();
            return true;
        }
    }

    public static class OpusPlayerExtensions
    {
        public static OpusPlayer Opus(this Player player)
            => player.GetModPlayer<OpusPlayer>();
    }
}
