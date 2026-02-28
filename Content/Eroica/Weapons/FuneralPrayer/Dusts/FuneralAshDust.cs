using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Dusts
{
    public class FuneralAshDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/NoiseSmoke";

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

            // Slow drift upward like rising ash from a funeral pyre
            dust.velocity.X *= 0.96f;
            dust.velocity.Y -= 0.015f; // gentle rise
            dust.velocity.Y *= 0.97f;

            dust.rotation += dust.velocity.X * 0.05f;
            dust.scale -= 0.005f;

            // Fade to dark as the ember dies
            dust.color = Color.Lerp(dust.color, new Color(30, 10, 20, 0), 0.02f);

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Ember-tinted light: warm crimson-amber glow
            float brightness = dust.scale * 0.35f;
            Lighting.AddLight(dust.position, new Vector3(brightness * 0.9f, brightness * 0.3f, brightness * 0.15f));
            return false;
        }
    }
}
