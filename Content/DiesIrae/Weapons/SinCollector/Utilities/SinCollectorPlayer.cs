using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities
{
    public class SinCollectorPlayer : ModPlayer
    {
        // Sin stacks accumulate from kills, fueling the charge level
        public int sinsCollected;
        public int sinChargeLevel;
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
                    sinsCollected = 0;
                    sinChargeLevel = 0;
                }
            }
            isActive = false;
        }

        public void CollectSin(int amount = 1)
        {
            sinsCollected += amount;
            activeTimer = 180;
        }

        public void AddCharge(int amount = 1)
        {
            sinChargeLevel = System.Math.Min(sinChargeLevel + amount, 10);
            activeTimer = 120;
        }

        public void ConsumeSins()
        {
            sinsCollected = 0;
            sinChargeLevel = 0;
        }

        public float GetSinIntensity()
        {
            return System.Math.Min(sinsCollected / 20f, 1f);
        }
    }

    public static class SinCollectorPlayerExtensions
    {
        public static SinCollectorPlayer SinCollector(this Player player)
            => player.GetModPlayer<SinCollectorPlayer>();
    }
}
