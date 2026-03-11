using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Dusts
{
    /// <summary>
    /// Custom dust for VariationsOfTheVoid — void motes that drift and fade.
    /// Emits purple-to-teal glow representing void energy dissipating into the abyss.
    /// </summary>
    public class VoidVariationDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Projectiles/BrightStarProjectile1";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale = Main.rand.NextFloat(0.12f, 0.4f);
            // Black and white sparkles
            dust.color = Main.rand.NextBool() ? Color.White : new Color(40, 20, 60);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.93f;

            // Slight drift — void motes wander aimlessly
            dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
            dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

            dust.scale -= 0.012f;
            dust.rotation += dust.velocity.X * 0.06f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Void purple-teal glow — abyssal energy dissipating
            float pulse = Main.rand.NextFloat(0.2f, 0.4f);
            Color glowColor = Color.Lerp(new Color(135, 40, 200), new Color(30, 195, 110), dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
