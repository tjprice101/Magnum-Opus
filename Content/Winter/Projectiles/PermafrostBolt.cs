using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Winter.Projectiles
{
    /// <summary>
    /// Permafrost Bolt - Main projectile for Permafrost Codex
    /// Frost magic bolt that applies stacking frostbite
    /// </summary>
    public class PermafrostBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/TwilightSparkle";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color DeepBlue = new Color(60, 90, 180);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
        }

        private float frostOrbitAngle = 0f;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            frostOrbitAngle += 0.12f;

            // === VFX: GLACIAL AURORA TRAIL ===
            // Shimmering aurora-like particles trail behind
            if (Main.rand.NextBool(2))
            {
                float auroraHue = (Main.GameUpdateCount * 0.01f + Main.rand.NextFloat() * 0.2f) % 0.3f + 0.5f; // Blue-purple range
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(7f, 7f);
                Vector2 trailVel = -Projectile.velocity * 0.11f + Main.rand.NextVector2Circular(1.8f, 1.8f);
                Color trailColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.24f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === VFX: ARCANE FROST RUNES ===
            // Magical runes orbit the frost bolt
            if (Main.GameUpdateCount % 8 == 0)
            {
                for (int r = 0; r < 2; r++)
                {
                    float runeAngle = frostOrbitAngle + MathHelper.Pi * r;
                    float runeRadius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + r) * 3f;
                    Vector2 runePos = Projectile.Center + runeAngle.ToRotationVector2() * runeRadius;
                    Color runeColor = r == 0 ? GlacialPurple * 0.6f : DeepBlue * 0.55f;
                    CustomParticles.GenericFlare(runePos, runeColor, 0.2f, 10);
                }
            }

            // === VFX: FROST MIST CLOUD ===
            // Cold mist surrounds the bolt
            if (Main.rand.NextBool(3))
            {
                float mistAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float mistRadius = Main.rand.NextFloat(8f, 16f);
                Vector2 mistPos = Projectile.Center + mistAngle.ToRotationVector2() * mistRadius;
                Vector2 mistVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.8f));
                Color mistColor = FrostWhite * 0.35f;
                var mist = new GenericGlowParticle(mistPos, mistVel, mistColor, 0.15f, 25, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Arcane rune sparkles - enhanced
            if (Main.rand.NextBool(5))
            {
                CustomParticles.GenericFlare(Projectile.Center, GlacialPurple * 0.55f, 0.2f, 12);
            }

            // Frost crystals sing - VISIBLE (scale 0.78f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, IceBlue * 0.75f, 0.78f, 42);
            }
            
            // Prismatic sparkle for magical shimmer - enhanced
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.12f, FrostWhite * 0.55f, 0.25f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === DYNAMIC PARTICLE EFFECTS - Arcane frost aura ===
            if (Main.GameUpdateCount % 7 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, GlacialPurple, IceBlue, 0.24f, 18, 0.1f, 0.18f);
            }
            if (Main.rand.NextBool(5))
            {
                TwinklingSparks(Projectile.Center, FrostWhite, 2, 10f, 0.18f, 20);
            }

            Lighting.AddLight(Projectile.Center, GlacialPurple.ToVector3() * 0.45f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 240);
            target.AddBuff(BuffID.Slow, 180);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, GlacialPurple, 0.45f, 16);

            // ☁EMUSICAL IMPACT - VISIBLE chilling note burst (scale 0.75f)
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                ThemedParticles.MusicNote(target.Center, noteVel, IceBlue * 0.8f, 0.75f, 35);
            }
            
            // Sparkle burst for magical impact
            for (int s = 0; s < 4; s++)
            {
                float sAngle = MathHelper.TwoPi * s / 4f;
                var sparkle = new SparkleParticle(target.Center, sAngle.ToRotationVector2() * 3f, FrostWhite * 0.6f, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === DYNAMIC PARTICLE EFFECTS - Winter theme impact ===
            WinterImpact(target.Center, 0.85f);
            DramaticImpact(target.Center, GlacialPurple, IceBlue, 0.45f, 18);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - Winter frost bolt effect
            ProceduralProjectileVFX.DrawWinterProjectile(Main.spriteBatch, Projectile, 0.35f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, GlacialPurple, 0.45f, 16);

            // ☁EMUSICAL FINALE - VISIBLE frost melody fades (scale 0.8f)
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, GlacialPurple * 0.7f, 0.8f, 38);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // === DYNAMIC PARTICLE EFFECTS - Magical death burst ===
            MagicalImpact(Projectile.Center, GlacialPurple, IceBlue, 0.55f);
            SpiralBurst(Projectile.Center, FrostWhite, IceBlue, 6, 0.2f, 4f, 0.3f, 18);
        }
    }

    /// <summary>
    /// Ice Storm Projectile - Charged attack for Permafrost Codex
    /// Creates a devastating blizzard that damages all enemies in range
    /// </summary>
    [AllowLargeHitbox("Blizzard storm requires large hitbox for AoE damage")]
    public class IceStormProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo1";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);

        private const float StormRadius = 180f;

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // Slow movement
            Projectile.velocity *= 0.97f;

            // Growing blizzard
            float lifeProgress = 1f - Projectile.timeLeft / 180f;
            float currentRadius = StormRadius * (0.5f + lifeProgress * 0.5f);
            
            Projectile.scale = 1f + lifeProgress * 0.3f;

            // Blizzard particle storm
            int particleCount = 4 + (int)(lifeProgress * 6);
            for (int i = 0; i < particleCount; i++)
            {
                // Swirling snow and ice
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(0.3f, 1f) * currentRadius;
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;
                
                // Spiral velocity
                float spiralAngle = angle + MathHelper.PiOver2;
                Vector2 particleVel = spiralAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f) + new Vector2(0, Main.rand.NextFloat(-2f, 0.5f));
                
                Color particleColor = Main.rand.NextBool(3) ? FrostWhite : Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                particleColor *= Main.rand.NextFloat(0.4f, 0.7f);
                
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(15, 30), true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Icicle spawns
            if (Projectile.timeLeft % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, currentRadius * 0.8f);
                    Vector2 icicleVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(8f, 14f));
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPos, icicleVel,
                        ModContent.ProjectileType<StormIcicle>(), Projectile.damage / 4, Projectile.knockBack * 0.3f, Projectile.owner);
                }
            }

            // Central vortex flare
            if (Projectile.timeLeft % 8 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, GlacialPurple * 0.6f, 0.4f + lifeProgress * 0.3f, 12);
            }

            // Frost sparkles 
            if (Projectile.timeLeft % 20 == 0)
            {
                var frostSparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.4f, (currentRadius / 200f) * 0.6f, 25);
                MagnumParticleHandler.SpawnParticle(frostSparkle);
            }

            // ☁EMUSICAL NOTATION - Blizzard symphonic swirl - VISIBLE SCALE 0.72f+
            if (Projectile.timeLeft % 10 == 0)
            {
                float noteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * Main.rand.NextFloat(20f, currentRadius * 0.6f);
                Vector2 noteVel = (noteAngle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                ThemedParticles.MusicNote(notePos, noteVel, CrystalCyan * 0.6f, 0.72f, 35);
                
                // Storm sparkle accent
                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, IceBlue * 0.4f, 0.25f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * (0.8f + lifeProgress * 0.4f));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float lifeProgress = 1f - Projectile.timeLeft / 180f;
            float currentRadius = StormRadius * (0.5f + lifeProgress * 0.5f);
            
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            return Vector2.Distance(Projectile.Center, targetCenter) < currentRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frozen, 60);
            target.AddBuff(BuffID.Frostburn2, 300);
            target.AddBuff(BuffID.Slow, 240);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.5f, 16);

            // ☁EMUSICAL IMPACT - Ice storm chord
            ThemedParticles.MusicNoteBurst(target.Center, IceBlue * 0.7f, 4, 3f);

            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                var spark = new GenericGlowParticle(target.Center, sparkVel, IceBlue * 0.5f, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - Winter ice storm halo effect
            ProceduralProjectileVFX.DrawWinterProjectile(Main.spriteBatch, Projectile, Projectile.scale * 1.2f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final explosion
            CustomParticles.GenericFlare(Projectile.Center, FrostWhite, 1.0f, 30);

            // ☁EMUSICAL FINALE - Grand blizzard crescendo
            ThemedParticles.MusicNoteBurst(Projectile.Center, CrystalCyan * 0.8f, 10, 5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, IceBlue * 0.7f, 60f, 8);

            // Frost sparkle burst 
            var frostSparkle1 = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.7f, 0.8f * 0.6f, 25);
            MagnumParticleHandler.SpawnParticle(frostSparkle1);
            var frostSparkle2 = new SparkleParticle(Projectile.Center, Vector2.Zero, CrystalCyan * 0.5f, 0.6f * 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(frostSparkle2);
            var frostSparkle3 = new SparkleParticle(Projectile.Center, Vector2.Zero, GlacialPurple * 0.4f, 0.4f * 0.6f, 18);
            MagnumParticleHandler.SpawnParticle(frostSparkle3);

            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color burstColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Storm Icicle - Secondary projectile spawned by Ice Storm
    /// Falls down and shatters on impact
    /// </summary>
    public class StormIcicle : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SmallTwilightSparkle";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.25f; // Gravity

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, IceBlue * 0.4f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Falling icicle hum - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1f, -0.3f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, IceBlue * 0.5f, 0.68f, 30);
                
                // Tiny frost sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.4f, CrystalCyan * 0.4f, 0.18f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.35f, 12);

            // ☁EMUSICAL IMPACT - Icicle shatter note
            ThemedParticles.MusicNoteBurst(target.Center, IceBlue * 0.6f, 3, 2.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - Winter icicle effect
            ProceduralProjectileVFX.DrawWinterProjectile(Main.spriteBatch, Projectile, 0.18f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Icicle final note
            ThemedParticles.MusicNoteBurst(Projectile.Center, IceBlue * 0.5f, 4, 2.5f);

            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.45f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }
}
