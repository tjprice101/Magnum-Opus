using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Vestigial slash creator - kept for backwards compatibility / localization.
    /// The new ValorBeam spawns ValorSlash directly on hit; this type is no longer used.
    /// Immediately kills itself on first tick.
    /// </summary>
    public class ValorSlashCreator : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
