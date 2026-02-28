using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.FugueOfTheUnknown.Dusts
{
    /// <summary>
    /// Custom dust for FugueOfTheUnknown — ethereal echo motes that drift and fade.
    /// Emits soft teal-purple light representing fading polyphonic voices.
    /// </summary>
    public class FugueEchoDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 16, 16);
            dust.scale = Main.rand.NextFloat(0.3f, 0.8f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;
            dust.scale -= 0.01f;
            dust.rotation += dust.velocity.X * 0.05f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Teal-purple glow — voice echoes fading between registers
            float pulse = Main.rand.NextFloat(0.2f, 0.4f);
            Color glowColor = Color.Lerp(new Color(30, 190, 120), new Color(155, 55, 205), dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
