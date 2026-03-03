using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities
{
    /// <summary>
    /// Per-player tracking for IgnitionOfTheBell.
    /// Tracks thrust combo phase and Chimequake counter (every 3rd cyclone detonation).
    /// </summary>
    public class IgnitionOfTheBellPlayer : ModPlayer
    {
        #region Thrust Combo

        /// <summary>Combo 0=Ignition Strike, 1=Tolling Frenzy, 2=Chime Cyclone</summary>
        public int ThrustCombo;
        public int ComboResetTimer;
        private const int ComboResetDelay = 50;

        public void AdvanceCombo()
        {
            ThrustCombo = (ThrustCombo + 1) % 3;
            ComboResetTimer = ComboResetDelay;
        }

        #endregion

        #region Chimequake Tracker

        /// <summary>
        /// Counts Chime Cyclone detonations. Every 3rd triggers Chimequake.
        /// </summary>
        public int CycloneDetonationCount;
        public const int ChimequakeThreshold = 3;

        /// <summary>Register a cyclone detonation. Returns true if Chimequake triggers.</summary>
        public bool RegisterCycloneDetonation()
        {
            CycloneDetonationCount++;
            if (CycloneDetonationCount >= ChimequakeThreshold)
            {
                CycloneDetonationCount = 0;
                return true;
            }
            return false;
        }

        #endregion

        public override void PostUpdate()
        {
            // Combo timeout
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ThrustCombo = 0;
            }
        }

        public override void OnRespawn()
        {
            ThrustCombo = 0;
            CycloneDetonationCount = 0;
        }
    }

    public static class IgnitionOfTheBellPlayerExtensions
    {
        public static IgnitionOfTheBellPlayer IgnitionOfTheBell(this Player player) =>
            player.GetModPlayer<IgnitionOfTheBellPlayer>();
    }
}
