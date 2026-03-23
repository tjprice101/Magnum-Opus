using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles
{
    /// <summary>Twilight Severance special: severance line/detonation marker. Placeholder.</summary>
    public class TwilightSeveranceSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 30; Projectile.height = 30;
            Projectile.friendly = true; Projectile.penetrate = -1;
            Projectile.tileCollide = false; Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = -1;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.Opacity -= 0.005f;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.25f, 0.4f);
        }
    }
}
