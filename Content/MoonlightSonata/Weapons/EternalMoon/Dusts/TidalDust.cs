using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Dusts
{
    /// <summary>
    /// Tidal Dust — the primary dust particle for the Eternal Moon weapon.
    /// A soft, luminous mote that drifts with gentle deceleration, colored in the lunar palette.
    /// </summary>
    public class TidalDust : ModDust
    {
        // Uses PointBloom for soft luminous mote — tinted violet-blue in GetAlpha
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.alpha = 80;
            dust.scale *= 0.6f;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.scale -= 0.015f;
            dust.alpha += 3;
            dust.rotation += dust.velocity.X * 0.04f;

            if (dust.scale < 0.1f || dust.alpha > 250)
            {
                dust.active = false;
            }

            // Moonlight palette lighting: soft violet/blue glow
            float brightness = dust.scale * 0.4f;
            Lighting.AddLight(dust.position, new Vector3(0.35f, 0.15f, 0.55f) * brightness);

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(138, 43, 226, dust.alpha);
        }
    }
}
