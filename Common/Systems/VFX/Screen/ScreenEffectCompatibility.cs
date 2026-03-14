using Terraria;

namespace MagnumOpus.Common.Systems.VFX.Screen
{
    /// <summary>
    /// Backward-compatible facade for older call sites that still reference ScreenEffectSystem.
    /// </summary>
    public static class ScreenEffectSystem
    {
        public static void AddScreenShake(float power)
        {
            MagnumScreenEffects.AddScreenShake(power);
        }

        public static void AddScreenShake(float power, int frames)
        {
            MagnumScreenEffects.AddScreenShake(power, frames);
        }
    }
}
