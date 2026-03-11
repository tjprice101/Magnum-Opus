using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Dusts
{
    /// <summary>
    /// Custom dust for TheWatchingRefrain — ghostly phantom motes that drift upward
    /// and emit a spectral green-purple glow. Like ethereal breath from the watcher.
    /// </summary>
    public class WatchingPhantomDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Projectiles/BrightStarProjectile1";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale = Main.rand.NextFloat(0.15f, 0.45f);
            // Apply black or white tint for enigma sparkles
            dust.color = Main.rand.NextBool() ? Color.White : new Color(40, 20, 60);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;

            // Gentle upward drift
            dust.velocity.Y -= 0.015f;
            dust.velocity *= 0.97f;

            // Fade and shrink
            dust.scale -= 0.012f;
            dust.rotation += dust.velocity.X * 0.04f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Spectral green-purple glow — shifts based on remaining scale
            float t = MathHelper.Clamp(dust.scale / 0.8f, 0f, 1f);
            Color glowColor = Color.Lerp(new Color(35, 200, 85), new Color(125, 45, 185), t);
            float pulse = Main.rand.NextFloat(0.25f, 0.45f);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
