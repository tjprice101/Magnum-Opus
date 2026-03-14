using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities
{
    public class WrathsCleaverPlayer : ModPlayer
    {
        // Wrath stacks build from 0-5 as the cleaver feeds on fury
        public int wrathStacks;
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
                    wrathStacks = 0;
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

        public void AddWrath(int amount = 1)
        {
            wrathStacks = System.Math.Min(wrathStacks + amount, 5);
            activeTimer = 120;
        }

        public void ConsumeWrath()
        {
            wrathStacks = 0;
        }

        public float GetWrathIntensity()
        {
            return wrathStacks / 5f;
        }
    }

    public static class WrathsCleaverPlayerExtensions
    {
        public static WrathsCleaverPlayer WrathsCleaver(this Player player)
            => player.GetModPlayer<WrathsCleaverPlayer>();
    }
}
