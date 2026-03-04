using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities
{
    /// <summary>
    /// Tracks Gardener's Fury combo state and seed pod management.
    /// 3-Phase combo: Sow → Cultivate → Harvest
    /// Botanical Barrage after 3 full combo cycles.
    /// </summary>
    public class GardenerPlayer : ModPlayer
    {
        /// <summary>Current combo phase (0 = Sow, 1 = Cultivate, 2 = Harvest)</summary>
        public int ComboPhase { get; set; }

        /// <summary>Full combo cycles completed (triggers barrage at 3)</summary>
        public int ComboCyclesCompleted { get; set; }

        /// <summary>Number of active seed pods in the world belonging to this player</summary>
        public int ActivePodCount { get; set; }

        /// <summary>Timer to reset combo if player stops attacking</summary>
        private int comboResetTimer;
        private const int ComboResetDelay = 90; // 1.5 seconds

        public override void PostUpdate()
        {
            if (comboResetTimer > 0)
            {
                comboResetTimer--;
                if (comboResetTimer <= 0)
                {
                    ComboPhase = 0;
                    ComboCyclesCompleted = 0;
                }
            }
        }

        /// <summary>Advance to next combo phase</summary>
        public void AdvanceCombo()
        {
            comboResetTimer = ComboResetDelay;
            ComboPhase++;
            if (ComboPhase > 2)
            {
                ComboPhase = 0;
                ComboCyclesCompleted++;
            }
        }

        /// <summary>Check if Botanical Barrage is ready (3 full cycles)</summary>
        public bool BarrageReady => ComboCyclesCompleted >= 3;

        /// <summary>Consume barrage charges</summary>
        public void ConsumeBarrage()
        {
            ComboCyclesCompleted = 0;
        }

        /// <summary>Pod type cycles: Bloom(0) → Thorn(1) → Pollen(2)</summary>
        public int GetCurrentPodType()
        {
            return ComboCyclesCompleted % 3;
        }
    }
}
