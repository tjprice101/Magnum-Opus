using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Utilities
{
    public class MidnightMechanismPlayer : ModPlayer
    {
        // Mechanism heat builds with sustained fire; overdrive unleashes full power
        public int mechanismHeat;
        public bool overdriveActive;
        public int burstCounter;
        public bool isActive;
        public int activeTimer;

        public override void ResetEffects()
        {
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    mechanismHeat = 0;
                    overdriveActive = false;
                    burstCounter = 0;
                }
            }
            isActive = false;
        }

        public void AddHeat(int amount = 1)
        {
            mechanismHeat = System.Math.Min(mechanismHeat + amount, 100);
            activeTimer = 90;

            if (mechanismHeat >= 100)
                overdriveActive = true;
        }

        public void IncrementBurst()
        {
            burstCounter++;
            activeTimer = 60;
        }

        public void CoolDown()
        {
            mechanismHeat = 0;
            overdriveActive = false;
            burstCounter = 0;
        }

        public float GetHeatIntensity()
        {
            return mechanismHeat / 100f;
        }
    }

    public static class MidnightMechanismPlayerExtensions
    {
        public static MidnightMechanismPlayer MidnightMechanism(this Player player)
            => player.GetModPlayer<MidnightMechanismPlayer>();
    }
}
