using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Projectiles
{
    /// <summary>
    /// Massive energy wave projectile for Variations of the Void's charged right-click.
    /// Grows in scale, fades over its short lifetime, emits intense purple light.
    /// </summary>
    public class VoidSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
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
            // Slow to a stop
            Projectile.velocity *= 0.96f;

            // Scale grows over lifetime
            float progress = 1f - (Projectile.timeLeft / 60f);
            Projectile.scale = MathHelper.Lerp(0.5f, 3.0f, progress);

            // Opacity fades
            Projectile.Opacity = MathHelper.Lerp(1f, 0f, progress * progress);

            // Rotation
            Projectile.rotation += 0.05f;

            // Intense purple light
            float pulse = 0.8f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f);
            Vector3 lightColor = EnigmaPurple.ToVector3() * Projectile.Opacity * pulse * 0.8f;
            Lighting.AddLight(Projectile.Center, lightColor);
        }
    }
}
