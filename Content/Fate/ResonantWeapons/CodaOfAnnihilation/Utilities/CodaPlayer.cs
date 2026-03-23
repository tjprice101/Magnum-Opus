using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.UI;
using MagnumOpus.Common.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities
{
    /// <summary>
    /// Per-player state for Coda of Annihilation.
    /// Tracks the weapon cycle index that increments mod 14 per swing.
    /// Also tracks Annihilation Stacks (doc mechanic) and Coda Finale timer.
    /// </summary>
    public class CodaPlayer : ModPlayer, IResonantOverdrive
    {
        /// <summary>Current weapon index (0-13), increments each swing.</summary>
        public int WeaponCycleIndex;

        // === ANNIHILATION STACKS (Doc mechanic) ===
        /// <summary>Ticks of continuous weapon use. At 600 (10s), triggers Coda Finale.</summary>
        public int ContinuousUseTimer;

        /// <summary>Whether Coda Finale has been triggered this use cycle.</summary>
        public bool CodaFinaleTriggered;

        /// <summary>Cooldown before next Coda Finale can trigger.</summary>
        public int FinaleCooldown;

        /// <summary>Intensity level 0-1 that ramps up during continuous use (VFX scaling).</summary>
        public float UseIntensity;

        /// <summary>Whether the weapon is currently being used (set per frame by item).</summary>
        public bool IsActivelyUsing;

        // === Charge Meter ===
        public float Charge = 0f;
        public const float ChargePerHit = 0.05f;
        public const float MaxCharge = 1.0f;
        public bool IsHoldingCodaOfAnnihilation = false;
        public bool IsChargeFull => Charge >= MaxCharge;

        private int _overdriveSpiralTimer;

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
            IsHoldingCodaOfAnnihilation = false;
            // WeaponCycleIndex persists across swings — only reset on respawn
            IsActivelyUsing = false;
        }

        public override void PostUpdate()
        {
            if (IsActivelyUsing)
            {
                ContinuousUseTimer++;
                UseIntensity = MathHelper.Clamp(UseIntensity + 0.005f, 0f, 1f);

                // Coda Finale at 10 seconds continuous use
                if (ContinuousUseTimer >= 600 && !CodaFinaleTriggered && FinaleCooldown <= 0)
                {
                    CodaFinaleTriggered = true;
                    FinaleCooldown = 300; // 5s cooldown between finales
                }
            }
            else
            {
                // Rapidly decay when not using
                ContinuousUseTimer = (int)(ContinuousUseTimer * 0.9f);
                UseIntensity *= 0.95f;
                CodaFinaleTriggered = false;
            }

            if (FinaleCooldown > 0)
                FinaleCooldown--;

            // === Overdrive Timer ===
            if (_overdriveSpiralTimer > 0)
            {
                _overdriveSpiralTimer--;
                if (_overdriveSpiralTimer % 8 == 0 && Player.whoAmI == Main.myPlayer)
                {
                    int damage = Math.Max(1, (int)(Player.HeldItem.damage * 1.8f));
                    float spin = Main.GameUpdateCount * 0.18f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = spin + i * MathHelper.TwoPi / 8f;
                        Vector2 velocity = angle.ToRotationVector2() * 13f;
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, velocity, ProjectileID.MagicDagger, damage, 2f, Player.whoAmI);
                    }
                }
            }
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            WeaponCycleIndex = 0;
            ContinuousUseTimer = 0;
            UseIntensity = 0f;
            CodaFinaleTriggered = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.type == ModContent.ProjectileType<Projectiles.CodaZenithSword>()
                || proj.type == ModContent.ProjectileType<Projectiles.CodaHeldSwing>())
                AddCharge(target.life <= 0 ? 0.15f : ChargePerHit);
        }

        // === IResonantOverdrive ===
        bool IResonantOverdrive.IsHoldingOverdriveWeapon => IsHoldingCodaOfAnnihilation;
        float IResonantOverdrive.OverdriveCharge => Charge;
        bool IResonantOverdrive.IsOverdriveReady => IsChargeFull;
        Color IResonantOverdrive.OverdriveLowColor => new Color(110, 40, 100);
        Color IResonantOverdrive.OverdriveHighColor => new Color(255, 160, 210);

        bool IResonantOverdrive.ActivateOverdrive(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return true;

            _overdriveSpiralTimer = 240;

            ConsumeCharge();
            return true;
        }
    }

    /// <summary>
    /// Extension method for convenient access.
    /// </summary>
    public static class CodaPlayerExtensions
    {
        /// <summary>Get the Coda player data from a Player.</summary>
        public static CodaPlayer Coda(this Player player)
            => player.GetModPlayer<CodaPlayer>();
    }
}
