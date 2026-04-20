using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities
{
    public class ThornSprayRepeaterPlayer : ModPlayer
    {
        // Thorn accumulation builds with sustained fire
        public int thornAccumulation;
        public int shotCounter;
        public bool isActive;
        public int activeTimer;

        /// <summary>Shots 30-35 in a 36-shot cycle are enhanced Bloom Reload shots.</summary>
        public bool bloomReloadActive => (shotCounter % 36) >= 30;

        public override void ResetEffects()
        {
            if (!isActive)
            {
                if (activeTimer > 0)
                    activeTimer--;
                if (activeTimer <= 0)
                {
                    thornAccumulation = 0;
                    shotCounter = 0;
                }
            }
            isActive = false;
        }

        public void AccumulateThorns(int amount = 1)
        {
            thornAccumulation = System.Math.Min(thornAccumulation + amount, 25);
            activeTimer = 90;
        }

        public void ConsumeThorns()
        {
            thornAccumulation = 0;
        }

        public float GetAccumulationIntensity()
        {
            return System.Math.Min(thornAccumulation / 25f, 1f);
        }

        public bool IsThornOverflow()
        {
            return thornAccumulation >= 25;
        }
    }

    public static class ThornSprayRepeaterPlayerExtensions
    {
        public static ThornSprayRepeaterPlayer ThornSprayRepeater(this Player player)
            => player.GetModPlayer<ThornSprayRepeaterPlayer>();
    }
}
