using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles
{
    /// <summary>Dual Fated Chime special: spectral spinning blade that fires beams. Placeholder.</summary>
    public class DualFatedChimeSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 30; Projectile.height = 30;
            Projectile.friendly = true; Projectile.penetrate = -1;
            Projectile.tileCollide = false; Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = 20;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }
            // Orbit around player
            float angle = Projectile.ai[0] * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly * 3f;
            Projectile.Center = owner.Center + angle.ToRotationVector2() * 80f;
            Projectile.rotation += 0.1f;
            Lighting.AddLight(Projectile.Center, 0.5f, 0.3f, 0.1f);
        }
    }
}
