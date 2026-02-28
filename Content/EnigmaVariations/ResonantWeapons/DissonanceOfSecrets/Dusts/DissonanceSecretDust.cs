using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.DissonanceOfSecrets.Dusts
{
    /// <summary>
    /// Custom dust for DissonanceOfSecrets — arcane secret motes that drift and fade.
    /// Emits soft purple-green light representing hidden knowledge dissipating.
    /// </summary>
    public class DissonanceSecretDust : ModDust
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
            dust.velocity *= 0.95f;
            dust.scale -= 0.012f;
            dust.rotation += dust.velocity.X * 0.06f;

            if (dust.scale < 0.1f)
            {
                dust.active = false;
                return false;
            }

            // Purple-green glow — secrets flickering between concealment and revelation
            float pulse = Main.rand.NextFloat(0.25f, 0.45f);
            Color glowColor = Color.Lerp(new Color(130, 45, 190), new Color(35, 210, 85), dust.scale);
            Lighting.AddLight(dust.position, glowColor.ToVector3() * pulse);

            return false;
        }
    }
}
