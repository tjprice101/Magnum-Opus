using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    /// <summary>
    /// Light pillar projectile for The Gardener's Fury charged right-click.
    /// Stationary pillar that grows in scale and fades, emitting green/gold light.
    /// </summary>
    public class GardenerFurySpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            // Stationary
            Projectile.velocity = Vector2.Zero;

            // Scale grows over lifetime
            float progress = 1f - (Projectile.timeLeft / 60f);
            Projectile.scale = MathHelper.Lerp(0.3f, 1.5f, progress);

            // Opacity fades in the second half
            Projectile.Opacity = progress < 0.5f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.5f) * 2f);

            // Green and gold light
            float pulse = 0.8f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 6f);
            float greenIntensity = 0.4f * Projectile.Opacity * pulse;
            float goldIntensity = 0.3f * Projectile.Opacity * pulse;
            Lighting.AddLight(Projectile.Center, new Vector3(goldIntensity, greenIntensity + 0.2f, goldIntensity * 0.2f));
            Lighting.AddLight(Projectile.Top, new Vector3(goldIntensity * 0.5f, greenIntensity * 0.8f, 0.05f));
        }
    }
}
