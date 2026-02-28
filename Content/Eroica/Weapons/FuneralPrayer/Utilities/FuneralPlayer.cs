using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Utilities
{
    public class FuneralPlayer : ModPlayer
    {
        /// <summary>
        /// Tracks how many unique beams from a single volley have hit an enemy.
        /// When all 5 converge on the target, a ricochet beam is spawned.
        /// </summary>
        public int BeamHitCount;

        public override void ResetEffects()
        {
            // BeamHitCount is reset by the projectile volley logic, not per-frame,
            // so we only clear transient per-frame state here.
        }
    }

    public static class FuneralPlayerExtensions
    {
        public static FuneralPlayer FuneralPrayer(this Player player) => player.GetModPlayer<FuneralPlayer>();
    }
}
