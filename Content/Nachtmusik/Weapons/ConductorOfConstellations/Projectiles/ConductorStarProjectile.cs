using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Projectiles
{
    /// <summary>
    /// Stellar Conductor's star attack projectile.
    /// Uses 4PointedStarSoft texture, has trail cache, light homing after 30 ticks,
    /// spawns music notes and sparkle trail. Explodes into golden flare + notes on kill.
    /// </summary>
    public class ConductorStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;

            // Light homing after 30 ticks (0.04 lerp)
            if (Projectile.timeLeft < 120)
            {
                NPC target = FindClosestEnemy(500f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
                }
            }

            // Star trail particles
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    NachtmusikPalette.StarWhite * 0.5f, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Musical notation — occasional music notes + star sparkle
            if (Main.rand.NextBool(6))
            {
                NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 1f, 0.7f, 0.7f, 28);

                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.4f, NachtmusikPalette.RadianceGold * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.StarWhite.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            NachtmusikVFXLibrary.ProjectileImpact(target.Center, 0.6f);

            // Musical impact — chord notes
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 4, 3f, 0.7f, 0.9f, 25);

            // Star sparkle burst
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(3f, 3f),
                    NachtmusikPalette.RadianceGold * 0.6f, 0.22f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Golden flare
            CustomParticles.GenericFlare(Projectile.Center, NachtmusikPalette.RadianceGold, 0.4f, 12);

            // Burst particles with celestial gradient
            for (int i = 0; i < 5; i++)
            {
                var burst = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                    NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat()) * 0.6f, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Musical finale — burst of notes
            NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 5, 3.5f, 0.7f, 0.9f, 25);

            // Finale sparkle cascade
            for (int i = 0; i < 4; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                    NachtmusikPalette.StarWhite * 0.6f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Nachtmusik VFX — star projectile rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.3f);

            // Nachtmusik theme star flare accent
            NachtmusikShaderManager.BeginAdditive(Main.spriteBatch);
            NachtmusikVFXLibrary.DrawThemeStarFlare(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            NachtmusikShaderManager.RestoreSpriteBatch(Main.spriteBatch);

            return false;
        }

        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
