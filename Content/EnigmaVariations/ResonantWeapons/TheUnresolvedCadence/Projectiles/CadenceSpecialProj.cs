using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Projectiles
{
    /// <summary>
    /// Detonation projectile for The Unresolved Cadence's charged right-click.
    /// Expanding circle with intense purple/green light, high damage, short lifetime.
    /// </summary>
    public class CadenceSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Stationary detonation
            Projectile.velocity = Vector2.Zero;

            // Expanding circle
            float progress = 1f - (Projectile.timeLeft / 30f);
            Projectile.scale = MathHelper.Lerp(0.5f, 4.0f, progress);

            // Opacity: bright then fades rapidly
            Projectile.Opacity = progress < 0.3f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.3f) / 0.7f);

            // Rotation
            Projectile.rotation += 0.1f;

            // Intense purple/green light that shifts
            float shift = MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, shift);
            Vector3 light = lightColor.ToVector3() * Projectile.Opacity * 1.2f;
            Lighting.AddLight(Projectile.Center, light);

            // Burst dust on first frame
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0,
                        DustID.PurpleTorch, vel.X, vel.Y, 100, default, 1.5f);
                    dust.noGravity = true;
                }
            }
        }
    }
}
