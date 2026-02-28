using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities
{
    /// <summary>
    /// Tracks Executioner's Verdict combo state and execution counter.
    /// 3-phase combo: Slash → Overhead → GUILLOTINE DROP.
    /// Execution counter builds with kills for visual escalation.
    /// </summary>
    public sealed class ExecutionersVerdictPlayer : ModPlayer
    {
        /// <summary>Current combo step (0 = Slash, 1 = Overhead, 2 = Guillotine Drop)</summary>
        public int ComboStep;

        /// <summary>Timer before combo resets to 0. Reset on 120 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetThreshold = 120; // 2 seconds

        /// <summary>Number of executions performed this session. Drives visual escalation.</summary>
        public int ExecutionCount;

        /// <summary>Frames since last execution — used for lingering blood VFX.</summary>
        public int ExecutionCooldown;

        /// <summary>True when the guillotine drop is in progress.</summary>
        public bool IsGuillotineDropping;

        /// <summary>Screen shake intensity from execution kills.</summary>
        public float ScreenShakeIntensity;

        public override void ResetEffects()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    ComboStep = 0;
                }
            }

            if (ExecutionCooldown > 0)
                ExecutionCooldown--;

            if (ScreenShakeIntensity > 0f)
                ScreenShakeIntensity *= 0.9f;
        }

        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 3;
            ComboResetTimer = ComboResetThreshold;
        }

        public void RegisterExecution()
        {
            ExecutionCount++;
            ExecutionCooldown = 60; // 1 second of lingering blood VFX
            ScreenShakeIntensity = 12f; // Heavy screen shake on execution
        }

        public override void ModifyScreenPosition()
        {
            if (ScreenShakeIntensity > 0.5f)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(ScreenShakeIntensity, ScreenShakeIntensity);
            }
        }
    }

    public static class ExecutionersVerdictPlayerExt
    {
        public static ExecutionersVerdictPlayer ExecutionersVerdict(this Player player)
            => player.GetModPlayer<ExecutionersVerdictPlayer>();
    }
}
