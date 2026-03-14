using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities
{
    public class ChronologicalityPlayer : ModPlayer
    {
        // Temporal charge accumulates; time slow bends the flow of combat
        public int temporalCharge;
        public bool timeSlowActive;
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
                    temporalCharge = 0;
                    timeSlowActive = false;
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

        public void AddTemporalCharge(int amount = 1)
        {
            temporalCharge = System.Math.Min(temporalCharge + amount, 10);
            activeTimer = 120;
        }

        public void ActivateTimeSlow()
        {
            timeSlowActive = true;
            activeTimer = 180;
        }

        public void ConsumeCharge()
        {
            temporalCharge = 0;
            timeSlowActive = false;
        }

        public float GetTemporalIntensity()
        {
            return temporalCharge / 10f;
        }
    }

    public static class ChronologicalityPlayerExtensions
    {
        public static ChronologicalityPlayer Chronologicality(this Player player)
            => player.GetModPlayer<ChronologicalityPlayer>();
    }
}
