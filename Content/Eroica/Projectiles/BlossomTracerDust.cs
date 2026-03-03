using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Heat-reactive tracer dust for Blossom of the Sakura bullets.
    /// Warm sakura-crimson glowing motes that linger briefly.
    /// </summary>
    public class BlossomTracerDust : ModDust
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.5f;
            dust.alpha = 40;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.93f;
            dust.scale *= 0.95f;
            dust.alpha += 6;

            if (dust.alpha >= 255 || dust.scale < 0.1f)
            {
                dust.active = false;
            }

            float brightness = dust.scale * 0.4f;
            Lighting.AddLight(dust.position, dust.color.R / 255f * brightness, dust.color.G / 255f * brightness * 0.6f, dust.color.B / 255f * brightness * 0.4f);

            return false;
        }
    }
}
