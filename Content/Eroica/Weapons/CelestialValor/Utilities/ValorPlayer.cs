using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities
{
    public class ValorPlayer : ModPlayer
    {
        public int ComboStep;
        public int ComboResetTimer;
        public bool IsLunging;
        public bool LungingDown;
        public bool RightClickListener;
        public bool MouseWorldListener;

        private const int MaxComboResetTime = 120;

        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 3;
            ComboResetTimer = MaxComboResetTime;
        }

        public override void ResetEffects()
        {
            IsLunging = false;
            LungingDown = false;
            RightClickListener = false;
            MouseWorldListener = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ComboStep = 0;
            }

            if (!LungingDown) return;
            Player.fullRotation = 0f;
        }
    }

    public static class ValorPlayerExtensions
    {
        public static ValorPlayer CelestialValor(this Player player) => player.GetModPlayer<ValorPlayer>();
    }
}
