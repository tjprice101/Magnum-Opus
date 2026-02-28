using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities
{
    /// <summary>
    /// Tracks per-player state for the Eternal Moon weapon system.
    /// Manages lunar combo phase, dash state, and mouse targeting.
    /// </summary>
    public class EternalMoonPlayer : ModPlayer
    {
        /// <summary>Current lunar combo phase: 0=New Moon, 1=Waxing, 2=Half, 3=Waning, 4=Full Moon.</summary>
        public int LunarPhase;

        /// <summary>Timer that resets the combo if the player stops swinging.</summary>
        public int ComboResetTimer;

        /// <summary>Whether the player is currently surging (dash attack).</summary>
        public bool SurgingForward;

        /// <summary>Right-click listener state for alt-fire.</summary>
        public bool RightClickListener;

        /// <summary>Tracked mouse position for targeting.</summary>
        public bool MouseWorldListener;

        /// <summary>Maximum ticks before combo resets.</summary>
        public const int ComboResetTime = 120;

        public override void ResetEffects()
        {
            if (!RightClickListener)
                SurgingForward = false;

            RightClickListener = false;
            MouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    LunarPhase = 0;
            }
        }

        /// <summary>
        /// Advances the lunar combo phase by 1, wrapping at 5 (back to 0 after Full Moon).
        /// Resets the combo timer.
        /// </summary>
        public void AdvancePhase()
        {
            LunarPhase = (LunarPhase + 1) % 5;
            ComboResetTimer = ComboResetTime;
        }

        /// <summary>
        /// Returns whether the current phase is Full Moon (final combo step).
        /// </summary>
        public bool IsFullMoon => LunarPhase == 4;

        /// <summary>
        /// Returns whether the current phase spawns ghost reflections (Half Moon).
        /// </summary>
        public bool IsHalfMoon => LunarPhase == 2;
    }

    /// <summary>
    /// Extension methods for accessing EternalMoonPlayer data.
    /// </summary>
    public static class EternalMoonPlayerExtensions
    {
        public static EternalMoonPlayer EternalMoon(this Player player) =>
            player.GetModPlayer<EternalMoonPlayer>();
    }
}
