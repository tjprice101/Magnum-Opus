using System;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Utilities;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// AoE heroic explosion — triggered when an empowered Phase 2 slam connects.
    /// Massive particle eruption with layered bloom, ember cinders, and heavy smoke.
    /// Mirrors Exoboom's invisible-hitbox area explosion pattern.
    /// </summary>
    public class ValorBoom : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/SandboxExoblade/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 280;
            Projectile.height = 280;
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
                // Initial burst — heroic cinder eruption
                for (int i = 0; i < 20; i++)
                {
                    Vector2 pos = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(55f);
                    Vector2 vel = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(5f, 20f);
                    Color color = ValorUtils.GetFireGradient(Main.rand.NextFloat());
                    ValorParticleHandler.SpawnParticle(new HeroicEmberParticle(
                        Projectile.Center + pos, vel, color, 1.2f, 35));
                }

                // Spark ring
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                    Color sparkColor = Main.rand.NextBool() ? ValorUtils.Gold : ValorUtils.Scarlet;
                    ValorParticleHandler.SpawnParticle(new ValorSparkParticle(
                        Projectile.Center, vel, sparkColor, Main.rand.NextFloat(0.06f, 0.12f), 20));
                }

                // Heroic bloom flash
                ValorParticleHandler.SpawnParticle(new HeroicBloomParticle(
                    Projectile.Center, Vector2.Zero, ValorUtils.Gold, 1.5f, 30));

                Projectile.localAI[0] = 1f;
            }

            // Ongoing smoke
            for (int i = 0; i < 2; i++)
            {
                Color smokeColor = Color.Lerp(
                    ValorUtils.GetFireGradient(Main.rand.NextFloat()), Color.Gray, 0.5f);
                ValorParticleHandler.SpawnParticle(new HeroicSmokeParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(25f, 25f),
                    smokeColor, 1.6f, 40));
            }

            Lighting.AddLight(Projectile.Center, ValorUtils.HotCore.ToVector3() * 1.5f *
                (float)Math.Sin(MathHelper.Pi * Projectile.timeLeft / 30f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);
        }
    }
}
