using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking combo state, phase timing, and
    /// spectral blade cooldown for Requiem of Reality.
    /// </summary>
    public class RequiemPlayer : ModPlayer
    {
        /// <summary>Current swing count for combo tracking (0-3, resets on 4th).</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 120 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 120;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Whether the player just triggered a spectral blade combo.</summary>
        public bool JustTriggeredCombo;

        /// <summary>Cooldown ticks before another spectral blade can spawn.</summary>
        public int SpectralBladeCooldown;

        /// <summary>Total attacks performed (for escalating effects).</summary>
        public int TotalAttacks;

        /// <summary>Current musical "movement" (cycles 0-3 for visual variety).</summary>
        public int MusicalMovement => TotalAttacks % 4;

        public override void ResetEffects()
        {
            JustTriggeredCombo = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    SwingCounter = 0;
                    ComboIntensity = 0f;
                }
            }

            if (SpectralBladeCooldown > 0)
                SpectralBladeCooldown--;

            // Decay combo intensity slowly
            ComboIntensity *= 0.995f;
        }

        /// <summary>Called on each swing. Returns true if this was a combo trigger swing.</summary>
        public bool OnSwing()
        {
            SwingCounter++;
            TotalAttacks++;
            ComboResetTimer = ComboResetDelay;

            // Build intensity with each swing
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.25f, 0f, 1f);

            if (SwingCounter >= 4 && SpectralBladeCooldown <= 0)
            {
                SwingCounter = 0;
                JustTriggeredCombo = true;
                SpectralBladeCooldown = 60; // 1 second cooldown
                ComboIntensity = 1f; // Max intensity on combo
                return true;
            }

            return false;
        }
    }

    public static class RequiemPlayerExtensions
    {
        public static RequiemPlayer Requiem(this Player player)
            => player.GetModPlayer<RequiemPlayer>();
    }
}
