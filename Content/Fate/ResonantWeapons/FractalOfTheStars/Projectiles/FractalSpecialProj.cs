using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Projectiles
{
    /// <summary>
    /// Fractal Special Projectile — Slash-all-enemies marker.
    /// Spawned at each on-screen enemy's position on right-click charge release.
    /// Fades out quickly, deals damage on spawn, crimson light.
    /// </summary>
    public class FractalSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            // Fade out over lifetime
            float progress = 1f - (Projectile.timeLeft / 30f);
            Projectile.Opacity = 1f - progress;

            // Crimson light that fades with the projectile
            float lightIntensity = Projectile.Opacity * 0.8f;
            Lighting.AddLight(Projectile.Center, 0.9f * lightIntensity, 0.15f * lightIntensity, 0.25f * lightIntensity);

            // Spawn a few dust particles on first frame
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.FireworkFountain_Red, dustVel.X, dustVel.Y);
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.8f, 1.4f);
                }
            }
        }
    }
}
