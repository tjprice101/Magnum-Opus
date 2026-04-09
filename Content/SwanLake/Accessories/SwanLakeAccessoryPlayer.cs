using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Minimal shared state for Swan Lake accessories.
    /// All accessory-specific mechanics are now in per-item ModPlayers.
    /// </summary>
    public class SwanLakeAccessoryPlayer : ModPlayer
    {
        // Shared visual state
        public float floatAngle = 0f;

        // Swan Lake color palette
        public static readonly Color SwanWhite = new Color(240, 245, 255);
        public static readonly Color SwanBlack = new Color(30, 30, 40);
        public static readonly Color SwanPearl = new Color(255, 240, 245);

        public override void PostUpdate()
        {
            floatAngle += 0.025f;
            if (floatAngle > MathHelper.TwoPi)
                floatAngle -= MathHelper.TwoPi;
        }
    }
}
