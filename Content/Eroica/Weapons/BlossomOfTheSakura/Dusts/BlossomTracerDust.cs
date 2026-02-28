using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.BlossomOfTheSakura.Dusts
{
    public class BlossomTracerDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= Main.rand.NextFloat(0.5f, 0.9f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.93f;
            dust.rotation += dust.velocity.X * 0.1f;
            dust.scale -= 0.012f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Pink-gold tracer light
            float brightness = dust.scale * 0.5f;
            Lighting.AddLight(dust.position, new Vector3(brightness * 1f, brightness * 0.7f, brightness * 0.5f));
            return false;
        }
    }
}
