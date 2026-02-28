using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Utilities
{
    public class SakuraPlayer : ModPlayer
    {
        public int ComboStep;
        public int ComboResetTimer;
        public bool IsLunging;
        public bool LungingDown;
        public bool RightClickListener;
        public Vector2 MouseWorldListener;
        private const int MaxComboResetTime = 150;

        public override void ResetEffects()
        {
            if (!IsLunging)
                LungingDown = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer == 0)
                    ComboStep = 0;
            }

            if (IsLunging && !LungingDown)
            {
                Player.fullRotation = Player.velocity.X * 0.04f;
                Player.fullRotationOrigin = Player.Size / 2f;
                Player.maxFallSpeed = 50f;
                if (!Player.mount.Active)
                    Player.gravity = 0f;
            }
        }

        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 4;
            ComboResetTimer = MaxComboResetTime;
        }
    }

    public static class SakuraPlayerExtensions
    {
        public static SakuraPlayer SakuraBlossom(this Player player) => player.GetModPlayer<SakuraPlayer>();
    }
}
