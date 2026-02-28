using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura.Utilities
{
    public class BlossomPlayer : ModPlayer
    {
        /// <summary>Current barrel heat level (0 = cool, MaxHeat = white-hot).</summary>
        public int HeatLevel;

        /// <summary>Ticks remaining before heat begins decaying.</summary>
        public int HeatDecayCooldown;

        public const int MaxHeat = 40;

        /// <summary>Heat as a 0-1 progress for gradient lookups.</summary>
        public float HeatProgress => MaxHeat <= 0 ? 0f : (float)HeatLevel / MaxHeat;

        public override void ResetEffects()
        {
            // Heat decay cooldown counts down each frame in PostUpdate; nothing to reset per-frame here.
        }

        public override void PostUpdate()
        {
            if (HeatDecayCooldown > 0)
            {
                HeatDecayCooldown--;
            }
            else if (HeatLevel > 0)
            {
                // Barrel cools down 1 unit every 3 ticks when not firing
                if (Player.miscCounter % 3 == 0)
                    HeatLevel--;
            }
        }

        /// <summary>Add heat from firing a round. Resets the decay cooldown.</summary>
        public void AddHeat(int amount = 1, int cooldownTicks = 12)
        {
            HeatLevel = System.Math.Min(HeatLevel + amount, MaxHeat);
            HeatDecayCooldown = cooldownTicks;
        }
    }

    public static class BlossomPlayerExtensions
    {
        public static BlossomPlayer BlossomOfSakura(this Player player) => player.GetModPlayer<BlossomPlayer>();
    }
}
