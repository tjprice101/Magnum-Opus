using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities
{
    /// <summary>
    /// Per-player tracking for PiercingBellsResonance:
    /// - ShotCounter: tracks consecutive shots for Scorching Staccato acceleration and 20th-shot resonant blast
    /// - StaccatoSpeed: current fire rate boost (0→0.6 over sustained fire)
    /// - DecayTimer: resets combo after brief pause
    /// </summary>
    public class PiercingBellsResonancePlayer : ModPlayer
    {
        public int ShotCounter;
        public float StaccatoSpeed; // 0 to 0.6 (60% faster)
        public int DecayTimer;

        private const int DecayThreshold = 30; // ~0.5s without firing resets acceleration
        private const float MaxSpeedBoost = 0.6f;
        private const float AccelPerShot = 0.03f;
        public const int ResonantBlastThreshold = 20;

        public override void PostUpdate()
        {
            if (DecayTimer > 0)
                DecayTimer--;
            else
            {
                // Decay acceleration when not firing
                StaccatoSpeed *= 0.92f;
                if (StaccatoSpeed < 0.01f)
                {
                    StaccatoSpeed = 0f;
                    ShotCounter = 0;
                }
            }
        }

        /// <summary>
        /// Called when PiercingBellsResonance fires a shot. Returns true if this is the 20th (resonant blast) shot.
        /// </summary>
        public bool RegisterShot()
        {
            ShotCounter++;
            StaccatoSpeed = MathHelper.Clamp(StaccatoSpeed + AccelPerShot, 0f, MaxSpeedBoost);
            DecayTimer = DecayThreshold;

            if (ShotCounter >= ResonantBlastThreshold)
            {
                ShotCounter = 0;
                StaccatoSpeed = MaxSpeedBoost; // Peak speed on resonant shot
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the current fire rate multiplier (1.0 = normal, up to 1.6x faster).
        /// Applied by reducing useTime/useAnimation.
        /// </summary>
        public float GetFireRateMultiplier() => 1f + StaccatoSpeed;
    }
}
