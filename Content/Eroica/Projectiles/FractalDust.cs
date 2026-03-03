using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Custom dust for Triumphant Fractal projectiles — golden geometric energy motes.
    /// </summary>
    public class FractalDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.8f;
            dust.alpha = 60;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;
            dust.scale *= 0.96f;
            dust.alpha += 4;

            if (dust.alpha >= 255 || dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            Lighting.AddLight(dust.position, dust.color.R / 255f * 0.35f, dust.color.G / 255f * 0.3f, dust.color.B / 255f * 0.1f);
            return false;
        }
    }
}
