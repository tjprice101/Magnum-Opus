using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.CipherNocturne.Dusts
{
    /// <summary>
    /// Custom dust for CipherNocturne — arcane void motes that drift and fade.
    /// Emits soft purple-green light and decelerates gently.
    /// </summary>
    public class CipherVoidDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Projectiles/BrightStarProjectile1";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale = Main.rand.NextFloat(0.15f, 0.5f);
            // Black and white sparkles
            dust.color = Main.rand.NextBool() ? Color.White : new Color(40, 20, 60);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.96f;
            dust.scale -= 0.015f;
            dust.rotation += dust.velocity.X * 0.05f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Purple-green glow
            float pulse = Main.rand.NextFloat(0.3f, 0.5f);
            Color glowColor = Color.Lerp(new Color(140, 60, 200), new Color(50, 220, 100), dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
