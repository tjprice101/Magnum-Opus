using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Fading ash dust for Funeral Prayer effects — dark smoke that rises and dissipates.
    /// </summary>
    public class FuneralAshDust : ModDust
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.6f;
            dust.alpha = 80;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.velocity.Y -= 0.02f; // Rises like ash
            dust.scale *= 0.97f;
            dust.alpha += 4;

            if (dust.alpha >= 255 || dust.scale < 0.08f)
            {
                dust.active = false;
            }

            float brightness = dust.scale * 0.25f;
            Lighting.AddLight(dust.position, brightness * 0.8f, brightness * 0.3f, brightness * 0.2f);

            return false;
        }
    }
}
