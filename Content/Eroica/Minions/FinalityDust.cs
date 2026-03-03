using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Custom dust for Finality of the Sakura minion — dark crimson-violet flame motes.
    /// </summary>
    public class FinalityDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale *= 0.7f;
            dust.alpha = 80;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.velocity.Y -= 0.02f;
            dust.scale *= 0.97f;
            dust.alpha += 3;

            if (dust.alpha >= 255 || dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            Lighting.AddLight(dust.position, dust.color.R / 255f * 0.3f, dust.color.G / 255f * 0.15f, dust.color.B / 255f * 0.15f);
            return false;
        }
    }
}
