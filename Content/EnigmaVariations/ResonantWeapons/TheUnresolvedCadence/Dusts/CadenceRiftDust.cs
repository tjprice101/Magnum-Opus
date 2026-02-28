using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Dusts
{
    /// <summary>
    /// Custom dust for TheUnresolvedCadence — dimensional rift motes that drift and fade.
    /// Emits deep violet glow representing tears between dimensions dissipating.
    /// </summary>
    public class CadenceRiftDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Pixel/PartiGlow";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 16, 16);
            dust.scale = Main.rand.NextFloat(0.4f, 1.0f);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;
            dust.scale -= 0.014f;
            dust.rotation += dust.velocity.X * 0.07f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Deep violet glow — dimensional rift energy dissipating
            float pulse = Main.rand.NextFloat(0.2f, 0.4f);
            Color glowColor = Color.Lerp(new Color(140, 50, 210), new Color(45, 215, 95), dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
