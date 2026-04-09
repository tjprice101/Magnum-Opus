using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// ModPlayer class for Enigma Variations accessory visual state.
    /// Theme: Mystery, the unknowable, arcane secrets
    /// Colors: Black -> Deep Purple -> Eerie Green Flame
    /// Class-specific accessory mechanics are now handled by MelodicAttunementPlayer.
    /// </summary>
    public class EnigmaAccessoryPlayer : ModPlayer
    {
        // Enigma color palette (kept for VFX use by other systems)
        public static readonly Color EnigmaBlack = new Color(15, 10, 20);
        public static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        public static readonly Color EnigmaPurple = new Color(140, 60, 200);
        public static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        public static readonly Color EnigmaDarkGreen = new Color(30, 100, 50);

        public float floatAngle = 0f;

        public override void PostUpdate()
        {
            floatAngle += 0.025f;
        }
    }
}
