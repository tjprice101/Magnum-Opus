using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Utilities
{
    public class FinalityPlayer : ModPlayer
    {
        /// <summary>
        /// Counts total flame projectiles spawned since the minion was summoned.
        /// Used for escalating VFX intensity over time.
        /// </summary>
        public int SummonFlameCount;

        /// <summary>
        /// Tracks the dark aura visual intensity (0-1) around the summoned minion.
        /// Drives bloom and particle density for the black fire aura effect.
        /// </summary>
        public float DarkAuraIntensity;

        public override void ResetEffects()
        {
            // Reset per-frame transient visual state
            DarkAuraIntensity = 0f;
        }
    }

    public static class FinalityPlayerExtensions
    {
        public static FinalityPlayer FinalityOfTheSakura(this Player player) => player.GetModPlayer<FinalityPlayer>();
    }
}
