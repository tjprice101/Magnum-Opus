using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Dusts
{
    /// <summary>
    /// Prismatic dust for Moonlight's Calling — shifts hue over its lifetime
    /// to create a spectral shimmer effect on beams and impacts.
    /// </summary>
    public class PrismaticDust : ModDust
    {
        // Uses SoftCircle for neutral base — hue cycling applied in Update()
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = 0.8f + Main.rand.NextFloat(0.5f);
            dust.alpha = 80;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.scale -= 0.015f;
            dust.alpha += 3;
            dust.rotation += 0.08f;

            // Shift hue over lifetime via color cycling
            float hue = (Main.GameUpdateCount * 0.02f + dust.dustIndex * 0.1f) % 1f;
            Color spectral = Main.hslToRgb(hue, 0.7f, 0.6f);
            dust.color = spectral;

            Lighting.AddLight(dust.position, spectral.ToVector3() * 0.3f * dust.scale);

            if (dust.scale < 0.1f || dust.alpha >= 255)
            {
                dust.active = false;
            }

            return false;
        }
    }
}
