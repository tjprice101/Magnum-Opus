using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Projectiles
{
    /// <summary>
    /// Opus Ultima Special Projectile — Explosive sparkle.
    /// Decelerates after spawn, then explodes into a dust burst at end of life.
    /// Spawned in a spread on right-click charge release.
    /// </summary>
    public class OpusUltimaSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            // Decelerate over time
            Projectile.velocity *= 0.96f;

            // Rotate visually
            Projectile.rotation += 0.08f;

            // Sparkle dust trail
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.FireworkFountain_Red, 0f, 0f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 1.0f);
                d.velocity *= 0.2f;
            }

            // Pulsing crimson-gold light
            float pulse = 0.5f + 0.3f * MathHelper.Clamp(
                (float)System.Math.Sin(Projectile.localAI[0]++ * 0.15f), 0f, 1f);
            Lighting.AddLight(Projectile.Center, 0.9f * pulse, 0.3f * pulse, 0.2f * pulse);
        }

        public override void OnKill(int timeLeft)
        {
            // Explosion dust burst
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.FireworkFountain_Red,
                    dustVel.X, dustVel.Y);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 2.0f);
            }

            // Gold accent dust
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame,
                    dustVel.X, dustVel.Y);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            // Brief bright light on explosion
            Lighting.AddLight(Projectile.Center, 1.2f, 0.5f, 0.3f);
        }
    }
}
