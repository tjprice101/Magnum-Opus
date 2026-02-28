using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities
{
    /// <summary>
    /// Per-player tracking for SymphonicBellfireAnnihilator:
    /// - VolleyTracker: 5+5 volley system. First 5 shots are rapid, next 5 enhanced, then resets.
    /// - CrescendoCounter: Counts total volleys for Grand Crescendo (every 3rd full volley).
    /// </summary>
    public class SymphonicBellfirePlayer : ModPlayer
    {
        public int VolleyShot; // 0-9 within current volley
        public int VolleyCount; // Total completed volleys
        public int DecayTimer; // Resets volley if not firing

        private const int DecayThreshold = 60; // 1s idle resets volley
        public const int VolleySize = 10; // 5+5 volley
        public const int CrescendoVolleyThreshold = 3; // Grand Crescendo every 3rd volley

        /// <summary>
        /// Call when SymphonicBellfireAnnihilator fires. Returns:
        /// 0 = normal rocket, 1 = enhanced rocket (shots 6-10), 2 = Grand Crescendo (3rd volley completion)
        /// </summary>
        public int RegisterShot()
        {
            VolleyShot++;
            DecayTimer = DecayThreshold;

            bool isEnhanced = VolleyShot > 5;

            if (VolleyShot >= VolleySize)
            {
                VolleyShot = 0;
                VolleyCount++;

                if (VolleyCount >= CrescendoVolleyThreshold)
                {
                    VolleyCount = 0;
                    return 2; // Grand Crescendo
                }

                return isEnhanced ? 1 : 0;
            }

            return isEnhanced ? 1 : 0;
        }

        public override void PostUpdate()
        {
            if (DecayTimer > 0)
                DecayTimer--;
            else if (VolleyShot > 0)
            {
                VolleyShot = 0; // Reset if idle too long
            }
        }

        /// <summary>
        /// Gets the volley progress (0-1) for visual feedback.
        /// </summary>
        public float GetVolleyProgress() => (float)VolleyShot / VolleySize;
    }
}
