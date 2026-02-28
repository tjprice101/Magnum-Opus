using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura.Dusts
{
    public class PiercingLightDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= Main.rand.NextFloat(0.5f, 1.0f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.90f;
            dust.rotation += dust.velocity.X * 0.08f;
            dust.scale -= 0.015f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Bright gold-white light
            float brightness = dust.scale * 0.6f;
            Lighting.AddLight(dust.position, new Vector3(brightness * 1f, brightness * 0.92f, brightness * 0.55f));
            return false;
        }
    }
}
