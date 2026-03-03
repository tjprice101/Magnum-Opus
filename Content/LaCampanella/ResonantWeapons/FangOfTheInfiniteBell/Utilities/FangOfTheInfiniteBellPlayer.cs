using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities
{
    /// <summary>
    /// Per-player tracking for FangOfTheInfiniteBell.
    /// Bounce stacking: +3% magic damage per bounce, max 20 stacks = +60%.
    /// Stacks decay 1 per second after 3 seconds of no bouncing.
    /// At 10+: airborne orbs chain lightning. At 20: orbs explode on final bounce.
    /// </summary>
    public class FangOfTheInfiniteBellPlayer : ModPlayer
    {
        public int BounceStacks;
        public const int MaxStacks = 20;
        public const float DamagePerStack = 0.03f; // +3% per stack

        private int _bounceTimer;
        private int _decayTimer;
        private const int DecayGracePeriod = 180; // 3 seconds before decay starts
        private const int DecayInterval = 60;     // Lose 1 stack per second

        public bool HasLightningArcs => BounceStacks >= 10;
        public bool HasFinalBounceExplosion => BounceStacks >= MaxStacks;
        public bool CanInfiniteCrescendo => BounceStacks >= MaxStacks;

        /// <summary>Register a bounce. Adds a stack (up to max).</summary>
        public void RegisterBounce()
        {
            if (BounceStacks < MaxStacks)
                BounceStacks++;

            _bounceTimer = DecayGracePeriod;
            _decayTimer = 0;

            // Apply/refresh the damage buff
            Player.AddBuff(ModContent.BuffType<InfiniteBellDamageBuff>(), 600);

            // At 10+, add empowered indicator buff
            if (HasLightningArcs)
                Player.AddBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>(), 600);
        }

        /// <summary>Consume all stacks for Infinite Crescendo. Returns the stacks consumed.</summary>
        public int ConsumeAllStacks()
        {
            int consumed = BounceStacks;
            BounceStacks = 0;
            _bounceTimer = 0;
            _decayTimer = 0;
            Player.ClearBuff(ModContent.BuffType<InfiniteBellDamageBuff>());
            Player.ClearBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>());
            return consumed;
        }

        public override void PostUpdate()
        {
            if (BounceStacks > 0)
            {
                if (_bounceTimer > 0)
                {
                    _bounceTimer--;
                }
                else
                {
                    // Decay: lose 1 stack per second
                    _decayTimer++;
                    if (_decayTimer >= DecayInterval)
                    {
                        _decayTimer = 0;
                        BounceStacks--;
                        if (BounceStacks <= 0)
                        {
                            BounceStacks = 0;
                            Player.ClearBuff(ModContent.BuffType<InfiniteBellDamageBuff>());
                            Player.ClearBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>());
                        }
                    }
                }
            }
        }

        public override void OnRespawn()
        {
            BounceStacks = 0;
            _bounceTimer = 0;
            _decayTimer = 0;
        }
    }

    public static class FangOfTheInfiniteBellPlayerExtensions
    {
        public static FangOfTheInfiniteBellPlayer FangOfTheInfiniteBell(this Player player) =>
            player.GetModPlayer<FangOfTheInfiniteBellPlayer>();
    }
}
