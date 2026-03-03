using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities
{
    /// <summary>
    /// Per-player tracking for GrandioseChime:
    /// - Kill Echo Chain: kills propagate to nearest enemy within 15 tiles at 60% damage, up to 3 chains
    /// - Grandiose Crescendo: after 5 complete 3-chain kill echoes, next beam is triple width + auto mines
    /// </summary>
    public class GrandioseChimePlayer : ModPlayer
    {
        public int FullChainKillCount; // Kills that triggered a full 3-chain echo
        public const int GrandioseCrescendoThreshold = 5;
        public bool GrandioseCrescendoReady;
        public int GrandioseCrescendoDecayTimer;
        private const int CrescendoDecayDelay = 600; // 10 seconds to use it

        /// <summary>Register a kill echo chain completion. Returns true when Grandiose Crescendo activates.</summary>
        public bool RegisterFullChainKill()
        {
            FullChainKillCount++;
            if (FullChainKillCount >= GrandioseCrescendoThreshold)
            {
                FullChainKillCount = 0;
                GrandioseCrescendoReady = true;
                GrandioseCrescendoDecayTimer = CrescendoDecayDelay;
                return true;
            }
            return false;
        }

        /// <summary>Consume Grandiose Crescendo for next beam.</summary>
        public void ConsumeGrandioseCrescendo()
        {
            GrandioseCrescendoReady = false;
            GrandioseCrescendoDecayTimer = 0;
        }

        public override void PostUpdate()
        {
            if (GrandioseCrescendoReady)
            {
                GrandioseCrescendoDecayTimer--;
                if (GrandioseCrescendoDecayTimer <= 0)
                {
                    GrandioseCrescendoReady = false;
                }
            }
        }

        public override void OnRespawn()
        {
            FullChainKillCount = 0;
            GrandioseCrescendoReady = false;
        }
    }
}
