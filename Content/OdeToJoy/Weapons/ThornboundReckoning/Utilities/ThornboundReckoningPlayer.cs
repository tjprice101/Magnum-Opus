using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities
{
    public class ThornboundReckoningPlayer : ModPlayer
    {
        // Thorn stacks build with each strike, escalating reckoning
        public int thornStacks;
        public int comboCounter;
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
                    thornStacks = 0;
                    comboCounter = 0;
                }
            }
            isActive = false;
        }

        public void IncrementCombo()
        {
            comboCounter = (comboCounter + 1) % 4;
            activeTimer = 60;
        }

        public void AddThorns(int amount = 1)
        {
            thornStacks = System.Math.Min(thornStacks + amount, 10);
            activeTimer = 120;
        }

        public void ConsumeThorns()
        {
            thornStacks = 0;
        }

        public float GetThornIntensity()
        {
            return thornStacks / 10f;
        }
    }

    public static class ThornboundReckoningPlayerExtensions
    {
        public static ThornboundReckoningPlayer ThornboundReckoning(this Player player)
            => player.GetModPlayer<ThornboundReckoningPlayer>();
    }
}
