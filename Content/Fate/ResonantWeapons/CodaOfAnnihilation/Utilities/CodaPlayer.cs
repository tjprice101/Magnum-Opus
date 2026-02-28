using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Utilities
{
    /// <summary>
    /// Per-player state for Coda of Annihilation.
    /// Tracks the weapon cycle index that increments mod 14 per swing.
    /// </summary>
    public class CodaPlayer : ModPlayer
    {
        /// <summary>Current weapon index (0-13), increments each swing.</summary>
        public int WeaponCycleIndex;

        public override void ResetEffects()
        {
            // WeaponCycleIndex persists across swings — only reset on respawn
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            WeaponCycleIndex = 0;
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
