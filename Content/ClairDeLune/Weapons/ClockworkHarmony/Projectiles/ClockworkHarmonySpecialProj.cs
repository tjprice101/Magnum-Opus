using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>Clockwork Harmony special: explosive bomb. Placeholder.</summary>
    public class ClockworkHarmonySpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 20; Projectile.height = 20;
            Projectile.friendly = true; Projectile.penetrate = -1;
            Projectile.tileCollide = false; Projectile.timeLeft = 90;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.usesLocalNPCImmunity = true; Projectile.localNPCHitCooldown = -1;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.96f;
            Lighting.AddLight(Projectile.Center, 0.3f, 0.4f, 0.5f);
            if (Projectile.timeLeft == 1)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, Main.rand.NextVector2CircularEdge(5f, 5f));
                    d.noGravity = true;
                }
            }
        }
    }
}
