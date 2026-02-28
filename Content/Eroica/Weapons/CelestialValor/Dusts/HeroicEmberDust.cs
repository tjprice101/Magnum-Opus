using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts
{
    public class HeroicEmberDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noLight = false;
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            dust.alpha = 60;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;
            dust.velocity.Y -= 0.04f;
            dust.scale -= 0.018f;
            dust.alpha += 4;
            dust.rotation += dust.velocity.X * 0.1f;

            if (dust.scale < 0.3f || dust.alpha >= 255)
                dust.active = false;

            float brightness = dust.scale * 0.6f;
            Lighting.AddLight(dust.position, new Vector3(0.9f, 0.35f, 0.1f) * brightness);

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(220, 80, 30) * ((255 - dust.alpha) / 255f);
        }
    }
}
