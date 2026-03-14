using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities
{
    public class TheStandingOvationPlayer : ModPlayer
    {
        // Ovation level builds toward an encore performance
        public int ovationLevel;
        public bool encoreReady;
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
                    ovationLevel = 0;
                    encoreReady = false;
                }
            }
            isActive = false;
        }

        public void AddOvation(int amount = 1)
        {
            ovationLevel = System.Math.Min(ovationLevel + amount, 10);
            activeTimer = 120;

            if (ovationLevel >= 10)
                encoreReady = true;
        }

        public void TriggerEncore()
        {
            ovationLevel = 0;
            encoreReady = false;
        }

        public float GetOvationIntensity()
        {
            return ovationLevel / 10f;
        }
    }

    public static class TheStandingOvationPlayerExtensions
    {
        public static TheStandingOvationPlayer TheStandingOvation(this Player player)
            => player.GetModPlayer<TheStandingOvationPlayer>();
    }
}
