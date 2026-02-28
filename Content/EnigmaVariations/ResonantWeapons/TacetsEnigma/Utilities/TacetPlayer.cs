using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Utilities
{
    /// <summary>
    /// Per-player state tracking for TacetsEnigma.
    /// Tracks shot counter toward paradox bolt, paradox stacks, and visual intensity.
    /// </summary>
    public sealed class TacetPlayer : ModPlayer
    {
        /// <summary>Tracks shots fired toward the 5th (paradox bolt). Range 0-4.</summary>
        public int ShotCounter;

        /// <summary>Current paradox stacks accumulated on the weapon. 5 stacks triggers AoE explosion + chain lightning.</summary>
        public int ParadoxStacks;

        /// <summary>Visual buildup intensity (0-1), ramps with paradox stacks.</summary>
        public float ParadoxIntensity;

        /// <summary>Set to true on the frame when 5 paradox stacks are reached and the explosion fires.</summary>
        public bool ParadoxExplosionThisFrame;

        public override void ResetEffects()
        {
            ParadoxExplosionThisFrame = false;

            // Ramp visual intensity based on current stack count
            float targetIntensity = ParadoxStacks / 5f;
            ParadoxIntensity = MathHelper.Lerp(ParadoxIntensity, targetIntensity, 0.1f);
        }
    }

    public static class TacetPlayerExtensions
    {
        public static TacetPlayer Tacet(this Player player) => player.GetModPlayer<TacetPlayer>();
    }
}
