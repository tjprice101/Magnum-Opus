using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities
{
    /// <summary>
    /// Per-player state tracking for TheUnresolvedCadence.
    /// Tracks combo phase, Inevitability stacks, and visual intensity.
    /// </summary>
    public sealed class CadencePlayer : ModPlayer
    {
        /// <summary>Current combo phase: 0=VoidCleave, 1=ParadoxSlash, 2=DimensionalSeverance.</summary>
        public int ComboPhase;

        /// <summary>Stacks toward Paradox Collapse (0–10). At 10 → triggers 3× damage ultimate.</summary>
        public int InevitabilityStacks;

        /// <summary>Visual buildup intensity (0–1), drives VFX brightness/scale.</summary>
        public float CadenceIntensity;

        /// <summary>Set true on the frame Paradox Collapse triggers (10 stacks consumed).</summary>
        public bool ParadoxCollapseThisFrame;

        public override void ResetEffects()
        {
            ParadoxCollapseThisFrame = false;
        }

        public override void PostUpdate()
        {
            // CadenceIntensity decays naturally when not actively swinging
            if (CadenceIntensity > 0f && Player.itemAnimation <= 0)
            {
                CadenceIntensity = MathHelper.Clamp(CadenceIntensity - 0.015f, 0f, 1f);
            }
        }
    }

    public static class CadencePlayerExtensions
    {
        public static CadencePlayer Cadence(this Player player) => player.GetModPlayer<CadencePlayer>();
    }
}
