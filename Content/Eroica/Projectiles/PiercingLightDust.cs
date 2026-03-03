using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Golden lightning-tinted dust for Piercing Light of the Sakura effects.
    /// Bright gold energy motes that linger and fade.
    /// </summary>
    public class PiercingLightDust : ModDust
    {
        public override string Texture => "MagnumOpus/Common/Systems/Particles/Textures/GlowDot";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.6f;
            dust.alpha = 40;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.92f;
            dust.scale *= 0.96f;
            dust.alpha += 5;

            if (dust.alpha >= 255 || dust.scale < 0.1f)
            {
                dust.active = false;
            }

            float brightness = dust.scale * 0.4f;
            Lighting.AddLight(dust.position, dust.color.R / 255f * brightness, dust.color.G / 255f * brightness, dust.color.B / 255f * brightness * 0.5f);

            return false;
        }
    }
}
