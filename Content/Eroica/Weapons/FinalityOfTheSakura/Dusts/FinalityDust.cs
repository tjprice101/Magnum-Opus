using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.FinalityOfTheSakura.Dusts
{
    public class FinalityDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.velocity *= 0.2f;
            dust.scale *= Main.rand.NextFloat(0.5f, 0.9f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;

            // Slow drift — dark flame wisps curling lazily
            dust.velocity.X *= 0.95f;
            dust.velocity.Y -= 0.01f; // gentle rise
            dust.velocity.Y *= 0.96f;

            dust.rotation += dust.velocity.X * 0.04f;
            dust.scale -= 0.01f;

            // Fade toward abyssal black
            dust.color = Color.Lerp(dust.color, new Color(10, 5, 15, 0), 0.03f);

            if (dust.scale < 0.2f)
            {
                dust.active = false;
                return false;
            }

            // Dark crimson light — faint smoldering glow
            float brightness = dust.scale * 0.25f;
            Lighting.AddLight(dust.position, new Vector3(brightness * 0.7f, brightness * 0.1f, brightness * 0.15f));
            return false;
        }
    }
}
