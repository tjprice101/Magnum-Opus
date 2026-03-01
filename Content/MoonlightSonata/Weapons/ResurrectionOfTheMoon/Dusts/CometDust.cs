using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Dusts
{
    /// <summary>
    /// Comet dust for Resurrection of the Moon — shifts from gold-white to deep violet
    /// over its lifetime to match the comet's cooling gradient.
    /// </summary>
    public class CometDust : ModDust
    {
        // Uses PointBloom for bright comet core — gradient coloring in Update()
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = 0.9f + Main.rand.NextFloat(0.6f);
            dust.alpha = 60;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;
            dust.scale -= 0.018f;
            dust.alpha += 3;
            dust.rotation += 0.1f;

            // Cooling gradient: start white-gold, shift through violet to deep space
            float life = dust.alpha / 255f;
            Color goldWhite = new(235, 230, 255);
            Color cometViolet = new(180, 120, 255);
            Color deepSpace = new(50, 20, 100);

            Color current;
            if (life < 0.5f)
                current = Color.Lerp(goldWhite, cometViolet, life * 2f);
            else
                current = Color.Lerp(cometViolet, deepSpace, (life - 0.5f) * 2f);

            dust.color = current;
            Lighting.AddLight(dust.position, current.ToVector3() * 0.35f * dust.scale);

            if (dust.scale < 0.1f || dust.alpha >= 255)
            {
                dust.active = false;
            }

            return false;
        }
    }
}
