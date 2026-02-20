using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// TEMPORARY STUB — Unblocks compilation while weapons are being converted
    /// to the Calamity-style held-projectile combo system (MeleeSwingBase).
    /// 
    /// DELETE THIS FILE once all 16 weapon conversions are complete.
    /// </summary>
    public static class SpectacularMeleeSwing
    {
        public enum SwingTier
        {
            Basic,
            Mid,
            High,
            Endgame,
            Ultimate
        }

        public enum WeaponTheme
        {
            Spring,
            Summer,
            Autumn,
            Winter,
            Seasons,
            MoonlightSonata,
            Nachtmusik,
            Fate,
            Eroica,
            Enigma
        }

        /// <summary>
        /// No-op stub. Previously attempted to apply generic VFX to melee swings.
        /// All weapons are being converted to per-weapon held-projectile combos instead.
        /// </summary>
        public static void OnSwing(Player player, Rectangle hitbox,
            Color primaryColor, Color secondaryColor,
            SwingTier tier, WeaponTheme theme)
        {
            // Intentionally empty — global swing VFX are disabled.
            // Each weapon will implement its own VFX via MeleeSwingBase subclass.
        }
    }
}
