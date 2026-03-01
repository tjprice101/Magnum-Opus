using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal.Utilities
{
    public class TriumphantFractalPlayer : ModPlayer
    {
        /// <summary>
        /// Accumulates as fractal projectiles hit enemies.
        /// Can be used for escalating VFX intensity or special attacks.
        /// </summary>
        public int FractalCharge;

        public override void ResetEffects()
        {
            // FractalCharge persists across frames and is managed by projectile logic.
            // Only clear transient per-frame state here.
        }
    }

    public static class TriumphantFractalPlayerExtensions
    {
        public static TriumphantFractalPlayer TriumphantFractal(this Player player) => player.GetModPlayer<TriumphantFractalPlayer>();
    }
}
