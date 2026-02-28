using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Utilities
{
    /// <summary>
    /// Per-player tracking for FangOfTheInfiniteBell.
    /// Empowerment cycle: 3 consecutive hits → empowered state (InfiniteBellDamageBuff + InfiniteBellEmpoweredBuff).
    /// 20-second cooldown after empowerment ends.
    /// </summary>
    public class FangOfTheInfiniteBellPlayer : ModPlayer
    {
        #region Empowerment Tracking

        public int ConsecutiveHits;
        public const int EmpowermentThreshold = 3;

        public bool IsEmpowered;
        public int EmpoweredTimer;
        public const int EmpoweredDuration = 600; // 10 seconds

        public int CooldownTimer;
        public const int CooldownDuration = 1200; // 20 seconds

        public bool CanEmpowerment => !IsEmpowered && CooldownTimer <= 0;

        /// <summary>Register a hit. Returns true when empowerment triggers.</summary>
        public bool RegisterHit()
        {
            if (!CanEmpowerment) return false;
            ConsecutiveHits++;
            if (ConsecutiveHits >= EmpowermentThreshold)
            {
                ConsecutiveHits = 0;
                ActivateEmpowerment();
                return true;
            }
            return false;
        }

        private void ActivateEmpowerment()
        {
            IsEmpowered = true;
            EmpoweredTimer = EmpoweredDuration;

            // Buffs will be applied from the projectile's OnHitNPC
            Player.AddBuff(ModContent.BuffType<InfiniteBellDamageBuff>(), EmpoweredDuration);
            Player.AddBuff(ModContent.BuffType<InfiniteBellEmpoweredBuff>(), EmpoweredDuration);
        }

        #endregion

        #region Hit Decay

        private int _hitDecayTimer;
        private const int HitDecayDelay = 120; // 2 seconds to continue combo

        #endregion

        public override void PostUpdate()
        {
            // Empowerment timer
            if (IsEmpowered)
            {
                EmpoweredTimer--;
                if (EmpoweredTimer <= 0)
                {
                    IsEmpowered = false;
                    CooldownTimer = CooldownDuration;
                }
            }

            // Cooldown timer
            if (CooldownTimer > 0)
                CooldownTimer--;

            // Hit combo decay
            if (ConsecutiveHits > 0)
            {
                _hitDecayTimer++;
                if (_hitDecayTimer >= HitDecayDelay)
                {
                    ConsecutiveHits = 0;
                    _hitDecayTimer = 0;
                }
            }

            // Infinite mana during empowerment
            if (IsEmpowered)
                Player.statMana = Player.statManaMax2;
        }

        public override void OnRespawn()
        {
            ConsecutiveHits = 0;
            IsEmpowered = false;
            EmpoweredTimer = 0;
            CooldownTimer = 0;
        }

        public void ResetHitDecay() { _hitDecayTimer = 0; }
    }

    public static class FangOfTheInfiniteBellPlayerExtensions
    {
        public static FangOfTheInfiniteBellPlayer FangOfTheInfiniteBell(this Player player) =>
            player.GetModPlayer<FangOfTheInfiniteBellPlayer>();
    }
}
