using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities
{
    public class AnthemOfGloryPlayer : ModPlayer
    {
        // Glory stacks and anthem level rise with triumphant strikes
        public int gloryStacks;
        public int anthemLevel;
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
                    gloryStacks = 0;
                    anthemLevel = 0;
                }
            }
            isActive = false;
        }

        public void AddGlory(int amount = 1)
        {
            gloryStacks += amount;
            activeTimer = 120;

            // Every 5 glory stacks raises the anthem level
            if (gloryStacks >= 5)
            {
                anthemLevel = System.Math.Min(anthemLevel + 1, 3);
                gloryStacks = 0;
            }
        }

        public void ConsumeAnthem()
        {
            anthemLevel = 0;
            gloryStacks = 0;
        }

        public float GetGloryIntensity()
        {
            return anthemLevel / 3f;
        }

        public bool IsFullAnthem()
        {
            return anthemLevel >= 3;
        }
    }

    public static class AnthemOfGloryPlayerExtensions
    {
        public static AnthemOfGloryPlayer AnthemOfGlory(this Player player)
            => player.GetModPlayer<AnthemOfGloryPlayer>();
    }
}
