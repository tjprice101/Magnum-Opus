using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Utilities
{
    /// <summary>
    /// Per-player state tracking for FugueOfTheUnknown.
    /// Tracks orbiting voice count, harmonic convergence buildup, and chain detonation state.
    /// </summary>
    public sealed class FuguePlayer : ModPlayer
    {
        /// <summary>Number of voice projectiles currently orbiting the player (0-5).</summary>
        public int ActiveVoices;

        /// <summary>Harmonic convergence buildup (0-1), increases as echo marks stack on enemies.</summary>
        public float HarmonicConvergence;

        /// <summary>Set true on the frame a chain detonation triggers from stacked echo marks.</summary>
        public bool ConvergenceThisFrame;

        public override void ResetEffects()
        {
            ConvergenceThisFrame = false;
        }

        public override void PostUpdate()
        {
            // Harmonic convergence decays naturally when no voices are active
            if (ActiveVoices <= 0 && HarmonicConvergence > 0f)
            {
                HarmonicConvergence = MathHelper.Clamp(HarmonicConvergence - 0.015f, 0f, 1f);
            }
        }
    }

    public static class FuguePlayerExtensions
    {
        public static FuguePlayer Fugue(this Player player) => player.GetModPlayer<FuguePlayer>();
    }
}
