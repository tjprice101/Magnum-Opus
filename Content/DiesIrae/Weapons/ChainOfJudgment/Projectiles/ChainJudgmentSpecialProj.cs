using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Projectiles
{
    /// <summary>Chain of Judgment special: empower all projectiles for 10 seconds. This is the buff aura. Placeholder.</summary>
    public class ChainJudgmentSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600; // 10 seconds
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }
            Projectile.Center = owner.Center;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.15f, 0.05f);
        }
    }
}
