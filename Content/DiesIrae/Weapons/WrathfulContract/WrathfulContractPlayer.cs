using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract
{
    /// <summary>
    /// ModPlayer that handles the Wrathful Contract HP drain mechanic.
    /// While a WrathDemonMinion is active, drains player HP per second.
    /// Rate: 1 HP/s normal, 3 HP/s during Frenzy.
    /// Also tracks Blood Sacrifice state and Breach of Contract threshold.
    /// </summary>
    public class WrathfulContractPlayer : ModPlayer
    {
        /// <summary>Whether the player currently has an active demon minion.</summary>
        public bool HasActiveDemon;

        /// <summary>Whether any active demon is in Frenzy state (drain 3x).</summary>
        public bool DemonInFrenzy;

        /// <summary>Blood Sacrifice active timer (frames remaining).</summary>
        public int BloodSacrificeTimer;

        /// <summary>Breach of Contract cooldown (frames since last breach).</summary>
        public int BreachCooldown;

        private int _drainTimer;

        public override void ResetEffects()
        {
            HasActiveDemon = false;
            DemonInFrenzy = false;
        }

        public override void PostUpdate()
        {
            if (!HasActiveDemon) 
            {
                _drainTimer = 0;
                return;
            }

            // HP drain: 1 HP/s normal, 3 HP/s during Frenzy
            _drainTimer++;
            int drainInterval = DemonInFrenzy ? 20 : 60; // 3/s vs 1/s at 60fps

            if (_drainTimer >= drainInterval)
            {
                _drainTimer = 0;
                if (Player.statLife > 1)
                {
                    Player.statLife--;
                    // Don't kill the player from drain alone — minimum 1 HP
                }
            }

            // Blood Sacrifice countdown
            if (BloodSacrificeTimer > 0)
                BloodSacrificeTimer--;

            // Breach cooldown
            if (BreachCooldown > 0)
                BreachCooldown--;
        }

        /// <summary>
        /// Check if the player is below Breach of Contract threshold (10% HP).
        /// </summary>
        public bool IsBelowBreachThreshold()
        {
            return Player.statLife < Player.statLifeMax2 * 0.10f;
        }

        /// <summary>
        /// Activate Blood Sacrifice: costs 20% max HP, returns 3x demon damage for 5s.
        /// Returns true if sacrifice was successful (player has enough HP).
        /// </summary>
        public bool TryBloodSacrifice()
        {
            int cost = (int)(Player.statLifeMax2 * 0.20f);
            if (Player.statLife <= cost + 1) return false; // Don't kill player

            Player.statLife -= cost;
            BloodSacrificeTimer = 300; // 5 seconds
            return true;
        }
    }
}
