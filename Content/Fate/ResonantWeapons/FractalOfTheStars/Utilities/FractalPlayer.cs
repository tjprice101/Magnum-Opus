using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking 3-phase combo state, swing counter,
    /// star fracture cooldown, and orbit blade tracking for Fractal of the Stars.
    /// </summary>
    public class FractalPlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current combo phase (0 = Horizontal Sweep, 1 = Rising Uppercut, 2 = Gravity Slam).</summary>
        public int ComboPhase;

        /// <summary>Total swing counter for visual escalation.</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 90 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 90;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Whether this swing triggered the Star Fracture (3rd combo hit).</summary>
        public bool JustTriggeredStarFracture;

        /// <summary>Cooldown before the next Star Fracture can trigger.</summary>
        public int StarFractureCooldown;

        /// <summary>Number of orbiting blades currently active.</summary>
        public int OrbitBladeCount;

        // === FRACTAL RECURSION ===
        /// <summary>Current fractal recursion depth (0 = normal, 1 = sub-fracture, 2 = micro-fracture).</summary>
        public int MaxRecursionDepth = 2;

        /// <summary>Total Star Fractures triggered this combat (for visual escalation).</summary>
        public int TotalFracturesTriggered;

        /// <summary>Direction alternator for swing variety.</summary>
        public int SwingDirection => SwingCounter % 2 == 0 ? 1 : -1;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingFractalOfTheStars = false;
        public bool IsChargeFull => Charge >= MaxCharge;

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
            IsHoldingFractalOfTheStars = false;
            JustTriggeredStarFracture = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    ComboPhase = 0;
                    ComboIntensity = 0f;
                }
            }

            if (StarFractureCooldown > 0)
                StarFractureCooldown--;

            // Decay combo intensity slowly
            ComboIntensity *= 0.993f;
        }

        /// <summary>
        /// Called on each swing. Returns the current combo phase before advancing.
        /// Triggers Star Fracture on phase 2 (Gravity Slam).
        /// </summary>
        public int OnSwing()
        {
            int currentPhase = ComboPhase;
            SwingCounter++;
            ComboResetTimer = ComboResetDelay;

            // Build intensity with each swing
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.3f, 0f, 1f);

            // Advance combo phase
            ComboPhase++;
            if (ComboPhase >= 3)
            {
                ComboPhase = 0;

                // Star Fracture triggers on the 3rd hit completion
                if (StarFractureCooldown <= 0)
                {
                    JustTriggeredStarFracture = true;
                    StarFractureCooldown = 45; // 0.75 second cooldown
                    ComboIntensity = 1f;
                    TotalFracturesTriggered++;
                }
            }

            return currentPhase;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.FractalSwingProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingFractalOfTheStars;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(100, 50, 140);
        Color IResonantOverdrive.OverdriveHighColor => new Color(240, 220, 255);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return true;

            int baseDamage = Math.Max(1, player.HeldItem.damage);
            foreach (NPC npc in NpcTargetingUtils.EnumerateHostiles(player.Center, 1800f))
            {
                npc.SimpleStrikeNPC(baseDamage * 2, 0, false, 0f, DamageClass.Melee, false, 0f, true);
                npc.GetGlobalNPC<ResonantOverdriveGlobalNpc>().ApplyFractalDot(baseDamage);
            }

            ConsumeCharge();
            return true;
        }
    }

    public static class FractalPlayerExtensions
    {
        public static FractalPlayer Fractal(this Player player)
            => player.GetModPlayer<FractalPlayer>();
    }
}
