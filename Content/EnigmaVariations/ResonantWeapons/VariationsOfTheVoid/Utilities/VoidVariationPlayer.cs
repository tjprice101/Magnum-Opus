using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities
{
    /// <summary>
    /// Per-player state tracking for VariationsOfTheVoid.
    /// Tracks combo phase, resonance stacks, visual intensity, and convergence state.
    /// </summary>
    public sealed class VoidVariationPlayer : ModPlayer
    {
        /// <summary>Current combo phase: 0=VoidWhisper, 1=AbyssalEcho, 2=RiftSunderFinisher.</summary>
        public int VoidComboPhase;

        /// <summary>Resonance stacks accumulated across swings, powering the finisher tri-beam.</summary>
        public int VariationStack;

        /// <summary>Visual buildup intensity (0–1), drives VFX brightness/scale.</summary>
        public float VoidIntensity;

        /// <summary>Set true on the frame the tri-beam convergence fires.</summary>
        public bool ConvergenceThisFrame;

        public override void ResetEffects()
        {
            ConvergenceThisFrame = false;
        }

        public override void PostUpdate()
        {
            // VoidIntensity decays naturally when not actively swinging
            if (VoidIntensity > 0f && Player.itemAnimation <= 0)
            {
                VoidIntensity = MathHelper.Clamp(VoidIntensity - 0.015f, 0f, 1f);
            }
        }
    }

    public static class VoidVariationPlayerExtensions
    {
        public static VoidVariationPlayer VoidVariation(this Player player) => player.GetModPlayer<VoidVariationPlayer>();
    }
}
