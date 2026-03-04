using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector
{
    /// <summary>
    /// Tracks Sin Counter per player for the Sin Collector weapon.
    /// Sin is collected on enemy hits and spent on alt-fire expenditure.
    /// </summary>
    public class SinCollectorPlayer : ModPlayer
    {
        /// <summary>Current sin count (0–30).</summary>
        public int SinCount;

        /// <summary>Maximum sin that can be collected.</summary>
        public const int MaxSin = 30;

        /// <summary>Cooldown between sin increment to prevent rapid stacking.</summary>
        private int sinCooldown;

        public override void ResetEffects()
        {
            if (sinCooldown > 0) sinCooldown--;
        }

        /// <summary>
        /// Add sin from an enemy hit. Returns new sin count.
        /// </summary>
        public int CollectSin(int amount = 1)
        {
            if (sinCooldown > 0) return SinCount;
            sinCooldown = 4;

            SinCount = System.Math.Min(SinCount + amount, MaxSin);
            return SinCount;
        }

        /// <summary>
        /// Try to spend sin for an expenditure shot.
        /// Returns the tier consumed:
        ///   0 = not enough (less than 10)
        ///   1 = Penance (10-19)
        ///   2 = Absolution (20-29)
        ///   3 = Damnation (30)
        /// </summary>
        public int TryExpendSin()
        {
            if (SinCount >= MaxSin)
            {
                SinCount = 0;
                return 3;
            }
            if (SinCount >= 20)
            {
                SinCount = 0;
                return 2;
            }
            if (SinCount >= 10)
            {
                SinCount = 0;
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Get the expenditure tier without spending.
        /// </summary>
        public int GetExpendTier()
        {
            if (SinCount >= MaxSin) return 3;
            if (SinCount >= 20) return 2;
            if (SinCount >= 10) return 1;
            return 0;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, Terraria.DataStructures.PlayerDeathReason damageSource)
        {
            SinCount = 0;
        }
    }
}
