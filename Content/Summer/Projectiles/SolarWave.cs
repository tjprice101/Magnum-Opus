using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Summer.Projectiles
{
    /// <summary>
    /// Solar Wave - Standard energy wave from Zenith Cleaver swings
    /// </summary>
    public class SolarWave : ModProjectile
    {
        private VertexStrip _strip;
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle";

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Fire dust
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, -Projectile.velocity * 0.1f, 0, SunOrange, 1.0f);
                dust.noGravity = true;
            }

            // ☁EMUSICAL NOTATION - VISIBLE notes blaze! (scale 0.7f+)
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = -Projectile.velocity * 0.05f + new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-1f, -0.3f));
                // Scale 0.7f makes notes VISIBLE!
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, SunGold, 0.7f, 35);
            }
            
            // Sparkle accents for magical shimmer
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.1f, SunWhite * 0.6f, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Slowing down slightly
            Projectile.velocity *= 0.98f;

            // === DYNAMIC PARTICLE EFFECTS - Solar wave aura ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, SunGold, SunOrange, 0.3f, 18, 0.14f, 0.24f);
            }
            if (Main.rand.NextBool(4))
            {
                TwinklingSparks(Projectile.Center, SunWhite, 2, 12f, 0.2f, 20);
            }

            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - VISIBLE notes scatter! (scale 0.8f)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SunGold, 0.8f, 38);
            }
            
            CustomParticles.GenericFlare(Projectile.Center, SunWhite, 0.6f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.5f, 16);
            
            // Sparkle ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                var sparkle = new SparkleParticle(Projectile.Center, angle.ToRotationVector2() * 3f, SunWhite * 0.7f, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.65f, 0.26f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // === DYNAMIC PARTICLE EFFECTS - Solar finale ===
            MagicalImpact(Projectile.Center, SunGold, SunOrange, 0.55f);
            SpiralBurst(Projectile.Center, SunWhite, SunGold, 6, 0.2f, 4f, 0.3f, 18);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Summer, ref _strip);
            return false;
        }
    }

    /// <summary>
    /// Zenith Flare - Massive solar projectile from Zenith Strike
    /// </summary>
    public class ZenithFlare : ModProjectile
    {
        private VertexStrip _strip;
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        private float orbitAngle = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.light = 0.8f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";

        public override void AI()
        {
            orbitAngle += 0.15f;
            Projectile.rotation += 0.12f;

            // Intense trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat()) * 0.75f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Orbiting flare points
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.PiOver2 * i;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 20f;
                    Color sparkColor = i % 2 == 0 ? SunGold : SunOrange;
                    CustomParticles.GenericFlare(sparkPos, sparkColor * 0.7f, 0.28f, 10);
                }
            }

            // Fire dust constantly
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.SolarFlare, -Projectile.velocity * 0.15f, 0, SunOrange, 1.2f);
            dust.noGravity = true;

            // ☁EMUSICAL NOTATION - Blazing notes orbit the zenith flare! - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(3))
            {
                float noteAngle = orbitAngle + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 15f;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.2f, -0.4f));
                ThemedParticles.MusicNote(notePos, noteVel, Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()), 0.75f, 40);
                
                // Solar sparkle
                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, SunGold * 0.5f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * pulse * 0.9f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - Solar symphony explodes on hit!
            ThemedParticles.MusicNoteBurst(target.Center, SunGold, 10, 5f);
            ThemedParticles.MusicNoteRing(target.Center, SunOrange, 50f, 6);
            
            // Apply heavy burning
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 180);

            // Big impact VFX - layered bloom
            CustomParticles.GenericFlare(target.Center, SunWhite, 0.9f, 22);
            CustomParticles.GenericFlare(target.Center, SunGold, 0.75f, 20);
            CustomParticles.GenericFlare(target.Center, SunOrange * 0.7f, 0.55f, 18);
            CustomParticles.GenericFlare(target.Center, SunOrange * 0.4f, 0.4f, 15);
            // Solar ray burst - 6-point star
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = target.Center + rayAngle.ToRotationVector2() * 16f;
                Color rayColor = ray % 2 == 0 ? SunGold : SunOrange;
                CustomParticles.GenericFlare(rayPos, rayColor * 0.7f, 0.2f, 12);
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 emberVel = Main.rand.NextVector2Circular(8f, 8f);
                Color emberColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat());
                var ember = new GenericGlowParticle(target.Center, emberVel, emberColor * 0.8f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Big solar explosion - layered bloom cascade
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.1f, 25);
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.9f, 22);
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.7f, 0.7f, 20);
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.5f, 0.55f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SunRed * 0.5f, 0.4f, 16);
            // Intense solar ray burst - 8-point star
            for (int ray = 0; ray < 8; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 8f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 22f;
                Color rayColor = Color.Lerp(SunGold, SunRed, ray / 8f);
                CustomParticles.GenericFlare(rayPos, rayColor * 0.8f, 0.25f, 14);
            }

            // Radial burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 16f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.75f, 0.38f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Fire dust explosion
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(8f, 8f), 0, SunOrange, 1.3f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Summer, ref _strip);
            return false;
        }
    }
}
