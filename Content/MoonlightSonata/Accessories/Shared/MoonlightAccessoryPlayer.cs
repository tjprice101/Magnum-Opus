using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// ModPlayer class that handles Moonlight Sonata weapon-related state.
    /// Class-accessory effects (MoonlitEngine, MoonlitGyre, FractalOfMoonlight, EmberOfTheMoon)
    /// are now handled by the shared MelodicAttunementPlayer system.
    /// </summary>
    public class MoonlightAccessoryPlayer : ModPlayer
    {
        // Weapon synergy flags (set by accessories each frame)
        public bool hasMoonlitGyre = false;

        // Resurrection of the Moon reload state
        public int resurrectionReloadTimer = 0;
        public const int ResurrectionReloadTime = 90;
        public bool resurrectionIsReloaded = true;
        public bool resurrectionPlayedReadySound = false;
        public int resurrectionActiveChamber = 0;

        // Staff of the Lunar Phases 窶・Conductor Mode state
        public bool staffConductorMode = false;
        public int conductorPulseTimer = 0;

        // Floating visual tracking
        public float floatAngle = 0f;

        public override void ResetEffects()
        {
            hasMoonlitGyre = false;
        }

        public override void PostUpdate()
        {
            floatAngle += 0.03f;
            if (floatAngle > MathHelper.TwoPi)
                floatAngle -= MathHelper.TwoPi;
        }
    }
}
