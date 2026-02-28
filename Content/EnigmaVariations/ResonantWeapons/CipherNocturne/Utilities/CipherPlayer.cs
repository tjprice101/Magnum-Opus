using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Utilities
{
    /// <summary>
    /// Per-player state tracking for CipherNocturne.
    /// Tracks beam channel duration, unravel intensity, snap-back state.
    /// </summary>
    public sealed class CipherPlayer : ModPlayer
    {
        /// <summary>How many ticks the player has been channeling the beam.</summary>
        public int ChannelTime;
        
        /// <summary>Current unravel intensity (0-1), ramps up while channeling.</summary>
        public float UnravelIntensity;
        
        /// <summary>Whether the beam just snapped back this frame (for screen effects).</summary>
        public bool SnapBackThisFrame;
        
        /// <summary>Accumulates damage dealt for visual scaling.</summary>
        public float AccumulatedDamage;

        public override void ResetEffects()
        {
            if (!Player.channel)
            {
                ChannelTime = 0;
                UnravelIntensity = 0f;
                AccumulatedDamage = 0f;
            }
            SnapBackThisFrame = false;
        }

        public override void PostUpdate()
        {
            if (Player.channel && Player.HeldItem?.ModItem is CipherNocturne)
            {
                ChannelTime++;
                UnravelIntensity = MathHelper.Clamp(ChannelTime / 180f, 0f, 1f);
            }
        }
    }

    public static class CipherPlayerExtensions
    {
        public static CipherPlayer Cipher(this Player player) => player.GetModPlayer<CipherPlayer>();
    }
}
