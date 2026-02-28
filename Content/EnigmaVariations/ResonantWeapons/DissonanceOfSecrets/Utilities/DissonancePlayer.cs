using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Utilities
{
    /// <summary>
    /// Per-player state tracking for DissonanceOfSecrets.
    /// Tracks orb charge growth, riddlebolt launches, cascade state.
    /// </summary>
    public sealed class DissonancePlayer : ModPlayer
    {
        /// <summary>How large the current orb has grown (increases while channeling).</summary>
        public int OrbChargeLevel;

        /// <summary>Count of riddlebolt sub-projectiles launched from the current orb.</summary>
        public int RiddleboltsFired;

        /// <summary>Visual intensity scaling (0-1), used for VFX brightness/size.</summary>
        public float SecretIntensity;

        /// <summary>Set true on the frame the orb detonates in a cascade explosion.</summary>
        public bool CascadeThisFrame;

        public override void ResetEffects()
        {
            CascadeThisFrame = false;
        }

        public override void PostUpdate()
        {
            // SecretIntensity decays naturally when not actively maintained
            if (SecretIntensity > 0f && !Player.channel)
            {
                SecretIntensity = MathHelper.Clamp(SecretIntensity - 0.02f, 0f, 1f);
            }
        }
    }

    public static class DissonancePlayerExtensions
    {
        public static DissonancePlayer Dissonance(this Player player) => player.GetModPlayer<DissonancePlayer>();
    }
}
