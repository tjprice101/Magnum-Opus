using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>Chronologicality special: replay all damage from past 5s at 3x. Placeholder visual marker.</summary>
    public class ChronologicalitySpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 40; Projectile.height = 40;
            Projectile.friendly = true; Projectile.penetrate = -1;
            Projectile.tileCollide = false; Projectile.timeLeft = 60;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 10;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.scale += 0.03f; Projectile.Opacity -= 0.016f;
            Lighting.AddLight(Projectile.Center, 0.3f, 0.4f, 0.5f);
        }
    }
}
