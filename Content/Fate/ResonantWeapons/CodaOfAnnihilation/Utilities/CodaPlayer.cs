using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities
{
    /// <summary>
    /// Per-player state for Coda of Annihilation.
    /// Tracks the weapon cycle index that increments mod 14 per swing.
    /// Also tracks Annihilation Stacks (doc mechanic) and Coda Finale timer.
    /// </summary>
    public class CodaPlayer : ModPlayer
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

        public override void ResetEffects()
        {
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
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            WeaponCycleIndex = 0;
            ContinuousUseTimer = 0;
            UseIntensity = 0f;
            CodaFinaleTriggered = false;
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
