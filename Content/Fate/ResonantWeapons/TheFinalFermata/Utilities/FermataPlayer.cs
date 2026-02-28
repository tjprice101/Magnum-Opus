using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Projectiles;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities
{
    /// <summary>
    /// Per-player Fermata state. Tracks active swords, global sync timer,
    /// and the 90-frame synchronized slash mechanic.
    /// Access via player.Fermata() extension method.
    /// </summary>
    public class FermataModPlayer : ModPlayer
    {
        /// <summary>Current number of active Fermata swords owned by this player.</summary>
        public int ActiveSwordCount;

        /// <summary>Global sync timer — drives the 90-frame synchronized slash.</summary>
        public int SyncTimer;

        /// <summary>Set to true on the frame when a synchronized slash fires.</summary>
        public bool SyncSlashTriggered;

        /// <summary>Total casts performed (for stagger offset of second group).</summary>
        public int TotalCasts;

        public override void ResetEffects()
        {
            SyncSlashTriggered = false;
        }

        public override void PostUpdate()
        {
            // Count active fermata spectral swords
            ActiveSwordCount = Player.ownedProjectileCounts[
                ModContent.ProjectileType<FermataSpectralSwordNew>()];

            // Advance the global sync timer when swords are active
            if (ActiveSwordCount > 0)
            {
                SyncTimer++;
                if (SyncTimer >= 90)
                {
                    SyncSlashTriggered = true;
                    SyncTimer = 0;
                }
            }
            else
            {
                SyncTimer = 0;
                TotalCasts = 0;
            }
        }

        /// <summary>Whether this player has the maximum (6) fermata swords active.</summary>
        public bool AtMaxSwords => ActiveSwordCount >= 6;

        /// <summary>Damage multiplier: 1.5x at max swords, 1.0x otherwise.</summary>
        public float DamageMultiplier => AtMaxSwords ? 1.5f : 1.0f;
    }

    /// <summary>
    /// Extension: player.Fermata() for convenient access.
    /// </summary>
    public static class FermataPlayerExtensions
    {
        public static FermataModPlayer Fermata(this Player player)
        {
            return player.GetModPlayer<FermataModPlayer>();
        }
    }
}
