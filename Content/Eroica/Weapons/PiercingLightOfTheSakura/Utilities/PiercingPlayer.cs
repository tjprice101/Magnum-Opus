using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Utilities
{
    public class PiercingPlayer : ModPlayer
    {
        /// <summary>Current shot number in the crescendo cycle (1–10).</summary>
        public int ShotCounter;

        /// <summary>True when ShotCounter has reached the crescendo threshold.</summary>
        public bool CrescendoReady => ShotCounter >= 10;

        /// <summary>Shot counter as a 0-1 progress for gradient / VFX intensity.</summary>
        public float CrescendoProgress => ShotCounter <= 0 ? 0f : (float)ShotCounter / 10f;

        public override void ResetEffects()
        {
            // Counter persists across frames; nothing to reset per-frame.
        }

        /// <summary>Advance the shot counter by one. Clamps at 10.</summary>
        public void IncrementShot()
        {
            ShotCounter = System.Math.Min(ShotCounter + 1, 10);
        }

        /// <summary>Reset the counter back to 0 after a crescendo burst.</summary>
        public void ResetCounter()
        {
            ShotCounter = 0;
        }
    }

    public static class PiercingPlayerExtensions
    {
        public static PiercingPlayer PiercingLight(this Player player) => player.GetModPlayer<PiercingPlayer>();
    }
}
