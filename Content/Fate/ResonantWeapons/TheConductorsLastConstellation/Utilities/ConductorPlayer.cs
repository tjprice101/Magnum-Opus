using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking 3-phase combo state (3 orchestral movements),
    /// swing counter, convergence cooldown, and beam tracking for
    /// The Conductor's Last Constellation.
    ///
    /// Phase 0 = Downbeat (descending sweep)
    /// Phase 1 = Crescendo (rising sweep)
    /// Phase 2 = Forte (wide horizontal + convergence)
    /// </summary>
    public class ConstellationConductorPlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current combo phase (0=Downbeat, 1=Crescendo, 2=Forte).</summary>
        public int ComboPhase;

        /// <summary>Total swing counter for visual escalation.</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 90 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 90;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Whether this swing triggered the Convergence (3rd combo hit).</summary>
        public bool JustTriggeredConvergence;

        /// <summary>Cooldown before the next Convergence can trigger.</summary>
        public int ConvergenceCooldown;

        /// <summary>Number of active conductor beams currently alive.</summary>
        public int ActiveBeamCount;

        /// <summary>Direction alternator for swing variety.</summary>
        public int SwingDirection => SwingCounter % 2 == 0 ? 1 : -1;

        // === STAR MAP CHARGE SYSTEM ===
        /// <summary>Ticks the weapon has been held (for Star Map Overlay charge). Max 120 (2s).</summary>
        public int ChargeTimer;

        /// <summary>Charge level 0.0-1.0, derived from ChargeTimer / 120.</summary>
        public float ChargeLevel => MathHelper.Clamp(ChargeTimer / 120f, 0f, 1f);

        /// <summary>Beam damage multiplier: 1x at 0.5s, 1.5x at 1s, 2.5x at 2s.</summary>
        public float BeamDamageMultiplier => ChargeLevel < 0.25f ? 1f :
            ChargeLevel < 0.5f ? 1f + (ChargeLevel - 0.25f) * 2f :
            1.5f + (ChargeLevel - 0.5f) * 2f;

        /// <summary>Number of star points visible in the Star Map Overlay (scales with charge).</summary>
        public int StarMapStarCount => (int)(ChargeLevel * 12f);

        /// <summary>Whether the last full-charge beam killed an enemy (triggers Constellation Shatter).</summary>
        public bool ConstellationShatterTriggered;

        /// <summary>Cooldown for Constellation Shatter (prevents spam).</summary>
        public int ShatterCooldown;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingConductorsConstellation = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        private int _overdriveConductorTimer;

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
            IsHoldingConductorsConstellation = false;
            JustTriggeredConvergence = false;
            ConstellationShatterTriggered = false;
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

            if (ConvergenceCooldown > 0)
                ConvergenceCooldown--;

            if (ShatterCooldown > 0)
                ShatterCooldown--;

            // Charge timer decays when not actively swinging
            if (ComboResetTimer <= 0 && ChargeTimer > 0)
                ChargeTimer = (int)(ChargeTimer * 0.95f);

            // Decay combo intensity slowly
            ComboIntensity *= 0.993f;

            // === Overdrive Timer ===
            if (_overdriveConductorTimer > 0)
            {
                _overdriveConductorTimer--;
                if (_overdriveConductorTimer % 12 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, (int)(Player.HeldItem.damage * 1.2f));
                    NPC target = NpcTargetingUtils.FindClosestNpc(Player.Center, 1400f);
                    if (target != null)
                    {
                        Vector2 spawn = Player.Center + Main.rand.NextVector2Circular(80f, 80f);
                        Vector2 velocity = spawn.DirectionTo(target.Center) * 11f;
                        Projectile.NewProjectile(Player.GetSource_FromThis(), spawn, velocity, ProjectileID.HallowStar, damage, 2f, Player.whoAmI);
                        target.AddBuff(BuffID.OnFire3, 120);
                    }
                }
            }
        }

        /// <summary>
        /// Called on each swing. Returns the current combo phase before advancing.
        /// Triggers Convergence on phase 2 (Forte).
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

                // Convergence triggers on the 3rd hit completion
                if (ConvergenceCooldown <= 0)
                {
                    JustTriggeredConvergence = true;
                    ConvergenceCooldown = 45; // 0.75 second cooldown
                    ComboIntensity = 1f;
                }
            }

            return currentPhase;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.ConductorSwingProjectile>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingConductorsConstellation;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(125, 40, 90);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 175, 220);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            int baseDamage = Math.Max(1, (int)(player.HeldItem.damage * 1.5f));

            // Fire aimed dagger toward mouse
            Vector2 velToMouse = player.Center.DirectionTo(Main.MouseWorld) * 15f;
            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, velToMouse, ProjectileID.MagicDagger, (int)(baseDamage * 1.5f), 2f, player.whoAmI);

            _overdriveConductorTimer = 240;

            ConsumeCharge();
            return true;
        }
    }

    public static class ConstellationConductorPlayerExtensions
    {
        public static ConstellationConductorPlayer Conductor(this Player player)
            => player.GetModPlayer<ConstellationConductorPlayer>();
    }
}
