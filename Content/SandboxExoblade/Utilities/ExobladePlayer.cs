using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Utilities
{
    public class ExobladePlayer : ModPlayer
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

    public static class ExobladePlayerExtensions
    {
        public static ExobladePlayer ExoBlade(this Player player) => player.GetModPlayer<ExobladePlayer>();
    }
}
