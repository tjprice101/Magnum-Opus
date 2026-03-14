using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities
{
    public class ExecutionersVerdictPlayer : ModPlayer
    {
        // Verdict stacks mark sentenced enemies; execution triggers when ready
        public int verdictStacks;
        public bool executionReady;
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
                    verdictStacks = 0;
                    executionReady = false;
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

        public void AddVerdict(int amount = 1)
        {
            verdictStacks += amount;
            activeTimer = 120;

            if (verdictStacks >= 5)
                executionReady = true;
        }

        public void ExecuteVerdict()
        {
            verdictStacks = 0;
            executionReady = false;
        }

        public float GetVerdictIntensity()
        {
            return System.Math.Min(verdictStacks / 5f, 1f);
        }
    }

    public static class ExecutionersVerdictPlayerExtensions
    {
        public static ExecutionersVerdictPlayer ExecutionersVerdict(this Player player)
            => player.GetModPlayer<ExecutionersVerdictPlayer>();
    }
}
