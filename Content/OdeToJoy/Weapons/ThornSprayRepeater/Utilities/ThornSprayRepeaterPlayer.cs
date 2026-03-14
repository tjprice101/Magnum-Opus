using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities
{
    public class ThornSprayRepeaterPlayer : ModPlayer
    {
        // Thorn accumulation builds with sustained fire
        public int thornAccumulation;
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
                    thornAccumulation = 0;
                }
            }
            isActive = false;
        }

        public void AccumulateThorns(int amount = 1)
        {
            thornAccumulation += amount;
            activeTimer = 90;
        }

        public void ConsumeThorns()
        {
            thornAccumulation = 0;
        }

        public float GetAccumulationIntensity()
        {
            return System.Math.Min(thornAccumulation / 30f, 1f);
        }

        public bool IsThornOverflow()
        {
            return thornAccumulation >= 30;
        }
    }

    public static class ThornSprayRepeaterPlayerExtensions
    {
        public static ThornSprayRepeaterPlayer ThornSprayRepeater(this Player player)
            => player.GetModPlayer<ThornSprayRepeaterPlayer>();
    }
}
