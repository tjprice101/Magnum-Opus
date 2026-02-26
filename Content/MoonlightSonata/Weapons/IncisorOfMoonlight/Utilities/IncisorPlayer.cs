using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities
{
    /// <summary>
    /// Per-player state tracker for the Incisor of Moonlight.
    /// Manages dash lunging and input listeners.
    /// </summary>
    public class IncisorPlayer : ModPlayer
    {
        public bool LungingDown = false;
        public bool rightClickListener = false;
        public bool mouseWorldListener = false;
        public Vector2 mouseWorld => Main.MouseWorld;

        public override void ResetEffects()
        {
            LungingDown = false;
            rightClickListener = false;
            mouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (!LungingDown)
                return;
            Player.fullRotation = 0f;
        }
    }

    public static class IncisorPlayerExtensions
    {
        public static IncisorPlayer Incisor(this Player player) => player.GetModPlayer<IncisorPlayer>();
    }
}
