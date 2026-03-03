using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.SymphonicBellfireAnnihilator.Utilities
{
    /// <summary>
    /// Per-player tracking for SymphonicBellfireAnnihilator.
    /// Dual buff stacking system:
    ///   Grand Crescendo Stacks (0-5): from wave kills, +10% wave size, +8% dmg per stack.
    ///   Bellfire Crescendo Stacks (0-3): from rocket kills, rockets burst 2→3→4.
    /// Symphonic Overture: Both at max → next wave is massive overture.
    /// </summary>
    public class SymphonicBellfirePlayer : ModPlayer
    {
        #region Grand Crescendo (Wave Kills)

        public int GrandCrescendoStacks;
        public const int MaxGrandCrescendoStacks = 5;
        public int GrandCrescendoDecayTimer;
        private const int GrandCrescendoDecay = 300; // 5s no wave kills to start decay

        /// <summary>Called when an enemy is killed by a Crescendo Wave.</summary>
        public void RegisterWaveKill()
        {
            if (GrandCrescendoStacks < MaxGrandCrescendoStacks)
                GrandCrescendoStacks++;
            GrandCrescendoDecayTimer = GrandCrescendoDecay;

            // Apply/refresh buff
            Player.AddBuff(ModContent.BuffType<GrandCrescendoBuff>(), GrandCrescendoDecay);
        }

        /// <summary>Get wave size multiplier: 1.0 + stacks * 0.10</summary>
        public float GetWaveSizeMultiplier() => 1f + GrandCrescendoStacks * 0.10f;

        #endregion

        #region Bellfire Crescendo (Rocket Kills)

        public int BellfireCrescendoStacks;
        public const int MaxBellfireCrescendoStacks = 3;
        public int BellfireCrescendoDecayTimer;
        private const int BellfireCrescendoDecay = 300;

        /// <summary>Called when an enemy is killed by a Bellfire Rocket.</summary>
        public void RegisterRocketKill()
        {
            if (BellfireCrescendoStacks < MaxBellfireCrescendoStacks)
                BellfireCrescendoStacks++;
            BellfireCrescendoDecayTimer = BellfireCrescendoDecay;

            // Apply/refresh buff
            Player.AddBuff(ModContent.BuffType<BellfireCrescendoBuff>(), BellfireCrescendoDecay);
        }

        /// <summary>Get rocket burst count: 1 base + stacks (so 1, 2, 3, 4).</summary>
        public int GetRocketBurstCount() => 1 + BellfireCrescendoStacks;

        #endregion

        #region Symphonic Overture

        /// <summary>Returns true if both buff stacks are at maximum.</summary>
        public bool IsSymphonicOvertureReady() =>
            GrandCrescendoStacks >= MaxGrandCrescendoStacks &&
            BellfireCrescendoStacks >= MaxBellfireCrescendoStacks;

        /// <summary>Consume all stacks when firing Symphonic Overture.</summary>
        public void ConsumeSymphonicOverture()
        {
            GrandCrescendoStacks = 0;
            BellfireCrescendoStacks = 0;
            GrandCrescendoDecayTimer = 0;
            BellfireCrescendoDecayTimer = 0;
            Player.ClearBuff(ModContent.BuffType<GrandCrescendoBuff>());
            Player.ClearBuff(ModContent.BuffType<BellfireCrescendoBuff>());
        }

        #endregion

        public override void PostUpdate()
        {
            // Grand Crescendo decay
            if (GrandCrescendoDecayTimer > 0)
                GrandCrescendoDecayTimer--;
            else if (GrandCrescendoStacks > 0)
            {
                GrandCrescendoStacks--;
                if (GrandCrescendoStacks > 0)
                    GrandCrescendoDecayTimer = 60; // 1s between stack drops
            }

            // Bellfire crescendo decay
            if (BellfireCrescendoDecayTimer > 0)
                BellfireCrescendoDecayTimer--;
            else if (BellfireCrescendoStacks > 0)
            {
                BellfireCrescendoStacks--;
                if (BellfireCrescendoStacks > 0)
                    BellfireCrescendoDecayTimer = 60;
            }
        }

        public override void OnRespawn()
        {
            GrandCrescendoStacks = 0;
            BellfireCrescendoStacks = 0;
            GrandCrescendoDecayTimer = 0;
            BellfireCrescendoDecayTimer = 0;
        }
    }
}
