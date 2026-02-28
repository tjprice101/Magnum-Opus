using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TacetsEnigma.Dusts
{
    /// <summary>
    /// Custom dust for TacetsEnigma — silence-themed motes that drift and dissolve.
    /// Emits soft purple glow, decelerates gently, no gravity.
    /// </summary>
    public class TacetSilenceDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 16, 16);
            dust.scale = Main.rand.NextFloat(0.3f, 0.9f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.scale -= 0.012f;
            dust.rotation += dust.velocity.X * 0.04f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Purple glow — silence fading into void
            float pulse = Main.rand.NextFloat(0.2f, 0.45f);
            Color glowColor = Color.Lerp(new Color(120, 40, 180), new Color(30, 10, 80), 1f - dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
