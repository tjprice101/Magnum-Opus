using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxLastPrism
{
    public class SLPScreenShakePlayer : ModPlayer
    {
        public float ScreenShakePower;

        public override void ModifyScreenPosition()
        {
            if (ScreenShakePower > 0.1f)
            {
                //This runs less often at lower frame rates (and vice versa) so this normalizes that
                float adjustedValue = ScreenShakePower * (Main.frameRate / 144f);

                float totalIntensity = adjustedValue * 1f;

                if (totalIntensity > 0)
                    Main.screenPosition += new Vector2(Main.rand.NextFloat(totalIntensity), Main.rand.NextFloat(totalIntensity));
                ScreenShakePower *= 0.9f;
            }
        }
    }
}
