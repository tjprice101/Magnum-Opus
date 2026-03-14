using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities
{
    public class TemporalPiercerPlayer : ModPlayer
    {
        // Pierce stacks build toward a time freeze moment
        public int pierceStacks;
        public bool timeFreezeReady;
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
                    pierceStacks = 0;
                    timeFreezeReady = false;
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

        public void AddPierceStack(int amount = 1)
        {
            pierceStacks = System.Math.Min(pierceStacks + amount, 8);
            activeTimer = 120;

            if (pierceStacks >= 8)
                timeFreezeReady = true;
        }

        public void TriggerTimeFreeze()
        {
            pierceStacks = 0;
            timeFreezeReady = false;
        }

        public float GetPierceIntensity()
        {
            return pierceStacks / 8f;
        }
    }

    public static class TemporalPiercerPlayerExtensions
    {
        public static TemporalPiercerPlayer TemporalPiercer(this Player player)
            => player.GetModPlayer<TemporalPiercerPlayer>();
    }
}
