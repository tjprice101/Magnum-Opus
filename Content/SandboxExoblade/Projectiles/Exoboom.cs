using MagnumOpus.Content.SandboxExoblade.Buffs;
using MagnumOpus.Content.SandboxExoblade.Particles;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Projectiles
{
    public class Exoboom : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/SandboxExoblade/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 250;
            Projectile.height = 250;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.MaxUpdates = 2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < 16; i++)
                {
                    Vector2 cinderSpawnPosition = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(50f);
                    Vector2 cinderVelocity = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(5f, 18f);
                    Color cinderColor = ExobladeUtils.MulticolorLerp(Main.rand.NextFloat(), ExobladeUtils.ExoPalette);
                    ExoSquishyLightParticle cinder = new(cinderSpawnPosition, cinderVelocity, 1.1f, cinderColor, 32, 1f, 4f);
                    ExoParticleHandler.SpawnParticle(cinder);
                }
                Projectile.localAI[0] = 1f;
            }

            // Create smoke.
            for (int i = 0; i < 2; i++)
            {
                Color smokeColor = ExobladeUtils.MulticolorLerp(Main.rand.NextFloat(), ExobladeUtils.ExoPalette);
                smokeColor = Color.Lerp(smokeColor, Color.Gray, 0.55f);
                ExoHeavySmokeParticle smoke = new(Projectile.Center, Main.rand.NextVector2Circular(27f, 27f), smokeColor, 40, 1.8f, 1f, 0.03f, true, 0.075f);
                ExoParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<ExoMiracleBlight>(), 300);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExoMiracleBlight>(), 300);
        }
    }
}
