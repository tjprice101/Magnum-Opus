using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Utilities
{
    public class TheGardenersFuryPlayer : ModPlayer
    {
        // Seeds planted grow fury; fury level escalates the gardener's wrath
        public int seedsPlanted;
        public int furyLevel;
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
                    seedsPlanted = 0;
                    furyLevel = 0;
                }
            }
            isActive = false;
        }

        public void PlantSeed()
        {
            seedsPlanted++;
            activeTimer = 150;
        }

        public void AddFury(int amount = 1)
        {
            furyLevel = System.Math.Min(furyLevel + amount, 10);
            activeTimer = 120;
        }

        public void ConsumeSeeds()
        {
            seedsPlanted = 0;
        }

        public float GetFuryIntensity()
        {
            return furyLevel / 10f;
        }
    }

    public static class TheGardenersFuryPlayerExtensions
    {
        public static TheGardenersFuryPlayer TheGardenersFury(this Player player)
            => player.GetModPlayer<TheGardenersFuryPlayer>();
    }
}
