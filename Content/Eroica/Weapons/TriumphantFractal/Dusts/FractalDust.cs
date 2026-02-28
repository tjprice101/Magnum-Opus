using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal.Dusts
{
    public class FractalDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= Main.rand.NextFloat(0.4f, 0.8f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;

            // Gentle drift with slight drag
            dust.velocity *= 0.94f;
            dust.rotation += dust.velocity.X * 0.06f;
            dust.scale -= 0.008f;

            // Fade out as it shrinks
            dust.color = Color.Lerp(dust.color, Color.Transparent, 0.03f);

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Golden radiant light from fractal energy
            float brightness = dust.scale * 0.4f;
            Lighting.AddLight(dust.position, new Vector3(brightness * 1.0f, brightness * 0.85f, brightness * 0.4f));
            return false;
        }
    }
}
