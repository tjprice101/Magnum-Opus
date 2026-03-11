using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Dusts
{
    /// <summary>
    /// Custom dust for TheSilentMeasure — hushed violet motes that drift and fade.
    /// Emits soft violet glow and decelerates gently.
    /// </summary>
    public class SilentMeasureDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Projectiles/BrightStarProjectile1";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale = Main.rand.NextFloat(0.12f, 0.35f);
            // Black and white sparkles per user request
            dust.color = Main.rand.NextBool() ? Color.White : new Color(40, 20, 60);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;

            // Gentle upward drift
            dust.velocity.Y -= 0.01f;

            dust.scale -= 0.012f;
            dust.rotation += dust.velocity.X * 0.04f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Violet glow — blends between Question Violet and Enigma Emerald based on remaining scale
            float pulse = Main.rand.NextFloat(0.3f, 0.5f);
            Color glowColor = Color.Lerp(new Color(110, 35, 170), new Color(25, 185, 80), 1f - dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
