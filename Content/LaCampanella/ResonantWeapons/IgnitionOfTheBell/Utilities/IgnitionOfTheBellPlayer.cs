using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities
{
    /// <summary>
    /// Per-player tracking for IgnitionOfTheBell.
    /// Tracks Chime Cyclone hit-count per NPC, thrust combo phase, and charge level.
    /// </summary>
    public class IgnitionOfTheBellPlayer : ModPlayer
    {
        #region Cyclone Tracker

        /// <summary>Hit count per NPC whoAmI. Every 3rd hit triggers Chime Cyclone explosion.</summary>
        private int[] _cycloneHits = new int[Main.maxNPCs];

        public const int CycloneThreshold = 3;

        public int GetCycloneHits(int npcWhoAmI) =>
            npcWhoAmI >= 0 && npcWhoAmI < _cycloneHits.Length ? _cycloneHits[npcWhoAmI] : 0;

        /// <summary>Increment hit counter. Returns true when cyclone threshold is reached.</summary>
        public bool RegisterHit(int npcWhoAmI)
        {
            if (npcWhoAmI < 0 || npcWhoAmI >= _cycloneHits.Length) return false;
            _cycloneHits[npcWhoAmI]++;
            if (_cycloneHits[npcWhoAmI] >= CycloneThreshold)
            {
                _cycloneHits[npcWhoAmI] = 0;
                return true;
            }
            return false;
        }

        public void ResetCycloneHits(int npcWhoAmI)
        {
            if (npcWhoAmI >= 0 && npcWhoAmI < _cycloneHits.Length)
                _cycloneHits[npcWhoAmI] = 0;
        }

        #endregion

        #region Thrust Combo

        /// <summary>Combo 0=Jab, 1=Cross, 2=Infernal Lunge</summary>
        public int ThrustCombo;
        public int ComboResetTimer;
        private const int ComboResetDelay = 50;

        public void AdvanceCombo()
        {
            ThrustCombo = (ThrustCombo + 1) % 3;
            ComboResetTimer = ComboResetDelay;
        }

        #endregion

        #region Charge

        /// <summary>Charge level for alt-fire geyser (0-60 ticks).</summary>
        public float ChargeLevel;
        public const float MaxCharge = 60f;
        public bool IsCharging;

        public float ChargePercent => ChargeLevel / MaxCharge;

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

            // Charge decay when not charging
            if (!IsCharging && ChargeLevel > 0)
                ChargeLevel = System.Math.Max(0f, ChargeLevel - 2f);
            IsCharging = false;
        }

        public override void OnRespawn()
        {
            ThrustCombo = 0;
            ChargeLevel = 0;
            for (int i = 0; i < _cycloneHits.Length; i++)
                _cycloneHits[i] = 0;
        }
    }

    public static class IgnitionOfTheBellPlayerExtensions
    {
        public static IgnitionOfTheBellPlayer IgnitionOfTheBell(this Player player) =>
            player.GetModPlayer<IgnitionOfTheBellPlayer>();
    }
}
