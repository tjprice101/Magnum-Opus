using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// Cross-slash damage visual — a brief cutting energy line that slices through enemies.
    /// Spawned in bursts by ValorSlashCreator on beam/dash hits.
    /// Similar to ExobeamSlash — wide hitbox, short lifetime, penetrates.
    /// </summary>
    public class ValorSlash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        public override void SetDefaults()
        {
            Projectile.width = 440;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.Opacity = 1f;
            Projectile.timeLeft = 30;
            Projectile.MaxUpdates = 2;
            Projectile.scale = 0.7f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 14;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Opacity = Projectile.timeLeft / 30f;

            if (Projectile.timeLeft == 29)
            {
                Color sparkColor = (Main.rand.NextBool() ? ValorUtils.Gold : ValorUtils.Scarlet) * 0.7f;
                ValorSparkParticle spark = new(Projectile.Center,
                    new Vector2(0.1f, 0.1f).RotatedByRandom(100),
                    sparkColor,
                    Main.rand.NextFloat(0.05f, 0.09f) * 10f,
                    14);
                ValorParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 start = Projectile.Center - dir * Projectile.width * 0.5f;
            Vector2 end = Projectile.Center + dir * Projectile.width * 0.5f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, Projectile.width, ref _);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
        }

        public override bool ShouldUpdatePosition() => true;

        public override Color? GetAlpha(Color lightColor)
            => Color.Lerp(ValorUtils.Scarlet, ValorUtils.Gold, Projectile.identity / 7f % 1f) * Projectile.Opacity;

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
