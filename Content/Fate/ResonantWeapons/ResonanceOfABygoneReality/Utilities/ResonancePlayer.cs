using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Per-player data for Resonance of a Bygone Reality.
    /// Tracks hit counter for spectral blade spawning (every 5th hit).
    /// PER PLAYER instance — NOT static!
    /// </summary>
    public class ResonancePlayer : ModPlayer
    {
        /// <summary>
        /// Hit counter for Resonance bullets. Every 5th hit spawns a spectral blade.
        /// Per-player, per-instance — never static.
        /// </summary>
        public int HitCounter;

        public override void ResetEffects()
        {
            // HitCounter persists across frames; only reset on death.
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            HitCounter = 0;
        }

        public override void OnRespawn()
        {
            HitCounter = 0;
        }
    }

    /// <summary>
    /// Extension method for convenient access to ResonancePlayer.
    /// Usage: player.Resonance().HitCounter
    /// </summary>
    public static class ResonancePlayerExtensions
    {
        public static ResonancePlayer Resonance(this Player player)
        {
            return player.GetModPlayer<ResonancePlayer>();
        }
    }
}
