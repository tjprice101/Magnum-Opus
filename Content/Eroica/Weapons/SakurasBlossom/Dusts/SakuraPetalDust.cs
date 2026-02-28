using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    public class SakuraPetalDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/ThinTall4PointedStar";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.noLight = false;
            dust.scale *= Main.rand.NextFloat(0.6f, 1.1f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.X *= 0.97f;
            dust.velocity.Y += 0.04f; // gentle gravity — petals drift down
            dust.velocity.Y *= 0.98f;
            dust.rotation += dust.velocity.X * 0.08f;
            dust.scale -= 0.006f;

            if (dust.scale < 0.15f)
            {
                dust.active = false;
                return false;
            }

            float brightness = dust.scale * 0.4f;
            Lighting.AddLight(dust.position, new Vector3(brightness * 1f, brightness * 0.5f, brightness * 0.6f));
            return false;
        }
    }
}
