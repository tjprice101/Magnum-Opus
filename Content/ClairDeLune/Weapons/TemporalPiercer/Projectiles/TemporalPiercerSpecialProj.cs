using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles
{
    /// <summary>Temporal Piercer special: teleport strike marker. Placeholder.</summary>
    public class TemporalPiercerSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 40; Projectile.height = 40;
            Projectile.friendly = true; Projectile.penetrate = -1;
            Projectile.tileCollide = false; Projectile.timeLeft = 20;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = -1;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.Opacity -= 0.05f;
            Lighting.AddLight(Projectile.Center, 0.3f, 0.4f, 0.5f);
        }
    }
}
