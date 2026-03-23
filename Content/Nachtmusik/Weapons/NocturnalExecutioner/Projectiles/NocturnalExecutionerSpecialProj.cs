using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>Nocturnal Executioner special: seeking void orb. Placeholder.</summary>
    public class NocturnalExecutionerSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 20; Projectile.height = 20;
            Projectile.friendly = true; Projectile.penetrate = 2;
            Projectile.tileCollide = false; Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            // Weak homing
            NPC target = Projectile.FindTargetWithinRange(600f);
            if (target != null)
            {
                Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.04f);
            }
            Projectile.rotation += 0.1f;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 0.4f);
        }
    }
}
