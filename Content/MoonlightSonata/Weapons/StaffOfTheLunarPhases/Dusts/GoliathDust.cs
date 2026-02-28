using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Dusts
{
    /// <summary>
    /// Goliath cosmic dust — shifts from ice-blue-white through nebula purple
    /// to deep cosmic void over its lifetime, matching the GoliathCosmic gradient.
    /// Used for ambient motes around the Goliath and beam impact debris.
    /// </summary>
    public class GoliathDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = 0.8f + Main.rand.NextFloat(0.5f);
            dust.alpha = 50;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.93f;
            dust.scale -= 0.015f;
            dust.alpha += 3;
            dust.rotation += 0.08f;

            // Cosmic gradient: start ice-blue, shift through nebula purple to void
            float life = dust.alpha / 255f;
            Color iceBlue = new(210, 225, 255);
            Color nebulaPurple = new(150, 80, 220);
            Color cosmicVoid = new(20, 8, 40);

            Color current;
            if (life < 0.5f)
                current = Color.Lerp(iceBlue, nebulaPurple, life * 2f);
            else
                current = Color.Lerp(nebulaPurple, cosmicVoid, (life - 0.5f) * 2f);

            dust.color = current;
            Lighting.AddLight(dust.position, current.ToVector3() * 0.3f * dust.scale);

            if (dust.scale < 0.1f || dust.alpha >= 255)
            {
                dust.active = false;
            }

            return false;
        }
    }
}
