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
    /// Icicle Bolt - Main projectile for Frostbite Repeater
    /// Piercing ice bolt that inflicts Hypothermia stacking debuff
    /// TRUE_VFX_STANDARDS: Dense dust, orbiting music notes, layered spinning flares
    /// </summary>
    public class IcicleBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ConstellationStyleSparkle";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color DeepFrost = new Color(80, 160, 220);
        
        // Ice/Winter hue range for color oscillation (cyan-blue)
        private const float HueMin = 0.52f;
        private const float HueMax = 0.62f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === DENSE ICE DUST TRAIL - 2+ per frame with hslToRgb color oscillation ===
            for (int i = 0; i < 2; i++)
            {
                float hue = HueMin + ((Main.GameUpdateCount * 0.02f + i * 0.25f) % 1f) * (HueMax - HueMin);
                Color oscillatingIce = Main.hslToRgb(hue, 0.7f, 0.65f);
                
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Frost, dustVel, 0, oscillatingIce, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // === CONTRASTING SPARKLES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                var sparkle = new SparkleParticle(sparklePos, -Projectile.velocity * 0.1f, FrostWhite * 0.8f, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === FREQUENT FLARES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.75f, 0.7f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(7f, 7f);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 14);
            }

            // === ENHANCED SNOWFLAKE STREAM - constant falling crystals ===
            if (Main.rand.NextBool(2))
            {
                Vector2 snowOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 snowVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f));
                Color snowColor = FrostWhite * 0.6f;
                var snow = new GenericGlowParticle(Projectile.Center + snowOffset, snowVel, snowColor, 0.2f, 25, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            // === FROST GLINT SPARKLES - ice catching light ===
            if (Main.rand.NextBool(3))
            {
                Vector2 glintPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                CustomParticles.GenericFlare(glintPos, Color.White, 0.5f, 6);
                CustomParticles.GenericFlare(glintPos, CrystalCyan, 0.38f, 8);
            }

            // === ORBITING MUSIC NOTES - Locked to projectile (TRUE_VFX_STANDARDS) ===
            float musicOrbitAngle = Main.GameUpdateCount * 0.1f;
            float musicOrbitRadius = 14f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 3f;
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
            
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 noteVel = Projectile.velocity * 0.7f + noteAngle.ToRotationVector2() * 0.5f;
                    float noteHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.8f, 0.7f);
                    
                    ThemedParticles.MusicNote(Projectile.Center + noteOffset, noteVel, noteColor, 0.8f * shimmer, 36);
                    
                    // Sparkle companion
                    var notSparkle = new SparkleParticle(Projectile.Center + noteOffset, noteVel * 0.4f, FrostWhite * 0.5f, 0.25f, 14);
                    MagnumParticleHandler.SpawnParticle(notSparkle);
                }
            }

            // === CRYSTALLINE SHARD ORBIT - ice crystals circling ===
            if (Main.GameUpdateCount % 3 == 0)
            {
                float crystalOrbitAngle = Main.GameUpdateCount * 0.12f;
                for (int c = 0; c < 4; c++)
                {
                    float crystalAngle = crystalOrbitAngle + MathHelper.TwoPi * c / 4f;
                    float crystalRadius = 11f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + c) * 3f;
                    Vector2 crystalPos = Projectile.Center + crystalAngle.ToRotationVector2() * crystalRadius;
                    float crystalHue = HueMin + (c / 4f) * (HueMax - HueMin);
                    Color crystalColor = Main.hslToRgb(crystalHue, 0.75f, 0.65f);
                    CustomParticles.GenericFlare(crystalPos, crystalColor, 0.22f, 10);
                }
            }

            // === DYNAMIC PARTICLE EFFECTS - Pulsing frost aura ===
            if (Main.GameUpdateCount % 6 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, IceBlue, CrystalCyan, 0.26f, 18, 0.12f, 0.2f);
            }
            if (Main.rand.NextBool(4))
            {
                TwinklingSparks(Projectile.Center, FrostWhite, 2, 12f, 0.2f, 22);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.65f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hypothermia debuff
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(BuffID.Frostburn2, 180);

            // === CENTRAL FLASH CASCADE (3 layers) ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.7f, 18);
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.55f, 16);

            // === MUSIC NOTE IMPACT RING - 6 notes with hslToRgb gradient ===
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 4f;
                float noteHue = HueMin + (n / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.7f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 38);
            }

            // === 6-POINT ICE CRYSTAL BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 crystalPos = target.Center + angle.ToRotationVector2() * 20f;
                float crystalHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color crystalColor = Main.hslToRgb(crystalHue, 0.7f, 0.6f);
                CustomParticles.GenericFlare(crystalPos, crystalColor, 0.45f, 14);
            }

            // === EXPANDING HALO RINGS ===
            CustomParticles.HaloRing(target.Center, CrystalCyan, 0.4f, 16);
            CustomParticles.HaloRing(target.Center, IceBlue, 0.3f, 14);

            // === SPARKLE BURST ===
            for (int s = 0; s < 8; s++)
            {
                float sAngle = MathHelper.TwoPi * s / 8f;
                var sparkle = new SparkleParticle(target.Center, sAngle.ToRotationVector2() * 3.5f, FrostWhite * 0.8f, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === DENSE ICE DUST ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.7f, 0.6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Frost, sparkVel, 0, dustColor, 1.4f);
                d.noGravity = true;
            }

            // === DYNAMIC PARTICLE EFFECTS - Winter theme impact ===
            WinterImpact(target.Center, 1f);
            DramaticImpact(target.Center, CrystalCyan, IceBlue, 0.5f, 20);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system instead of PNG textures
            // This replaces loading of: ConstellationStyleSparkle, EnergyFlare, EnergyFlare4, SoftGlow2
            ProceduralProjectileVFX.DrawWinterProjectile(Main.spriteBatch, Projectile, 1.2f);
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === 4-LAYER CENTRAL GLIMMER CASCADE ===
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.35f + layer * 0.15f;
                float layerAlpha = 0.85f - layer * 0.15f;
                float layerHue = HueMin + (layer / 4f) * (HueMax - HueMin);
                Color layerColor = Color.Lerp(Color.White, Main.hslToRgb(layerHue, 0.75f, 0.7f), layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
            }

            // === 4 EXPANDING GLOW RINGS with hslToRgb ===
            for (int ring = 0; ring < 4; ring++)
            {
                float ringHue = HueMin + (ring / 4f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.7f, 0.65f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.12f, 14 + ring * 3);
            }

            // === 6-POINT ICE CRYSTAL PATTERN ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 crystalPos = Projectile.Center + angle.ToRotationVector2() * 25f;
                float crystalHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color crystalColor = Main.hslToRgb(crystalHue, 0.75f, 0.65f);
                CustomParticles.GenericFlare(crystalPos, crystalColor, 0.4f, 16);
            }

            // === MUSIC NOTE FINALE - 8 notes burst ===
            for (int n = 0; n < 8; n++)
            {
                float angle = MathHelper.TwoPi * n / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * 4f;
                float noteHue = HueMin + (n / 8f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.72f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.85f, 40);
            }

            // === RADIAL SPARKLE BURST ===
            for (int s = 0; s < 10; s++)
            {
                float angle = MathHelper.TwoPi * s / 10f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5.5f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, FrostWhite * 0.75f, 0.38f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW PARTICLE BURST ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.7f, 0.6f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // === DENSE FROST DUST ===
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.7f, 0.6f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Frost, dustVel, 0, dustColor, 1.3f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.8f);
        }
    }

    /// <summary>
    /// Blizzard Shard - Homing projectile for Frostbite Repeater's right-click
    /// TRUE_VFX_STANDARDS: Dense dust, orbiting music notes, layered spinning flares, homing trail
    /// </summary>
    public class BlizzardShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle14";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color StormPurple = new Color(120, 140, 200);
        
        // Blizzard hue range (slightly more purple for storm effect)
        private const float HueMin = 0.55f;
        private const float HueMax = 0.68f;

        private bool hasTarget = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Homing after initial delay
            if (Projectile.timeLeft < 135)
            {
                float homingRange = 400f;
                float homingStrength = 0.08f;

                NPC target = null;
                float closestDist = homingRange;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            target = npc;
                        }
                    }
                }

                if (target != null)
                {
                    hasTarget = true;
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
                }
            }

            // === DENSE BLIZZARD DUST TRAIL - 2+ per frame with hslToRgb ===
            for (int i = 0; i < 2; i++)
            {
                float hue = HueMin + ((Main.GameUpdateCount * 0.02f + i * 0.3f) % 1f) * (HueMax - HueMin);
                Color oscillatingBlizzard = Main.hslToRgb(hue, 0.65f, 0.6f);
                
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(7f, 7f);
                Vector2 dustVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(2f, 2f);
                
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Frost, dustVel, 0, oscillatingBlizzard, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // === CONTRASTING SPARKLES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Color sparkleColor = hasTarget ? FrostWhite : StormPurple * 0.8f;
                var sparkle = new SparkleParticle(sparklePos, -Projectile.velocity * 0.1f, sparkleColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === FREQUENT FLARES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.7f, 0.65f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.42f, 15);
            }

            // === ENHANCED SNOWFLAKE STREAM ===
            if (Main.rand.NextBool(2))
            {
                Vector2 snowVel = -Projectile.velocity * 0.2f + new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f));
                var snow = new GenericGlowParticle(Projectile.Center, snowVel, FrostWhite * 0.55f, 0.22f, 22, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            // === ORBITING MUSIC NOTES - Locked to projectile (TRUE_VFX_STANDARDS) ===
            float musicOrbitAngle = Main.GameUpdateCount * 0.11f;
            float musicOrbitRadius = 16f + (float)Math.Sin(Main.GameUpdateCount * 0.09f) * 4f;
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.14f) * 0.12f;
            
            if (Main.rand.NextBool(5))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 noteVel = Projectile.velocity * 0.65f + noteAngle.ToRotationVector2() * 0.6f;
                    float noteHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color noteColor = hasTarget ? Main.hslToRgb(noteHue, 0.85f, 0.75f) : Main.hslToRgb(noteHue, 0.7f, 0.6f);
                    
                    ThemedParticles.MusicNote(Projectile.Center + noteOffset, noteVel, noteColor, 0.82f * shimmer, 38);
                    
                    // Sparkle companion
                    var noteSparkle = new SparkleParticle(Projectile.Center + noteOffset, noteVel * 0.4f, FrostWhite * 0.5f, 0.22f, 14);
                    MagnumParticleHandler.SpawnParticle(noteSparkle);
                }
            }

            // === STORM CRYSTAL ORBIT - crystals intensify when homing ===
            if (Main.GameUpdateCount % 3 == 0)
            {
                float crystalOrbitAngle = Main.GameUpdateCount * 0.14f;
                int crystalCount = hasTarget ? 5 : 3;
                for (int c = 0; c < crystalCount; c++)
                {
                    float crystalAngle = crystalOrbitAngle + MathHelper.TwoPi * c / crystalCount;
                    float crystalRadius = 13f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + c) * 4f;
                    Vector2 crystalPos = Projectile.Center + crystalAngle.ToRotationVector2() * crystalRadius;
                    float crystalHue = HueMin + (c / (float)crystalCount) * (HueMax - HueMin);
                    Color crystalColor = Main.hslToRgb(crystalHue, hasTarget ? 0.8f : 0.65f, 0.7f);
                    float crystalScale = hasTarget ? 0.25f : 0.18f;
                    CustomParticles.GenericFlare(crystalPos, crystalColor, crystalScale, 10);
                }
            }

            Lighting.AddLight(Projectile.Center, (hasTarget ? CrystalCyan : DeepBlue).ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hypothermia x2
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(BuffID.Frostburn2, 240);

            // === CENTRAL FLASH CASCADE (4 layers - homing impact is stronger) ===
            CustomParticles.GenericFlare(target.Center, Color.White, 1.1f, 22);
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.85f, 20);
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.65f, 18);
            CustomParticles.GenericFlare(target.Center, DeepBlue, 0.5f, 16);

            // === MUSIC NOTE IMPACT RING - 8 notes with hslToRgb gradient ===
            for (int n = 0; n < 8; n++)
            {
                float angle = MathHelper.TwoPi * n / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * 4.5f;
                float noteHue = HueMin + (n / 8f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.72f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.9f, 40);
            }

            // === 8-POINT BLIZZARD BURST ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstPos = target.Center + angle.ToRotationVector2() * 22f;
                float burstHue = HueMin + (i / 8f) * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.75f, 0.65f);
                CustomParticles.GenericFlare(burstPos, burstColor, 0.5f, 16);
            }

            // === EXPANDING HALO RINGS ===
            CustomParticles.HaloRing(target.Center, CrystalCyan, 0.5f, 18);
            CustomParticles.HaloRing(target.Center, IceBlue, 0.38f, 16);
            CustomParticles.HaloRing(target.Center, DeepBlue, 0.28f, 14);

            // === SPARKLE BURST ===
            for (int s = 0; s < 10; s++)
            {
                float sAngle = MathHelper.TwoPi * s / 10f;
                var sparkle = new SparkleParticle(target.Center, sAngle.ToRotationVector2() * 4f, FrostWhite * 0.85f, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW BURST ===
            for (int i = 0; i < 12; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(hue, 0.7f, 0.6f);
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // === FROST DUST ===
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Frost, dustVel, 0, IceBlue, 1.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system for Winter homing projectile
            ProceduralProjectileVFX.DrawWinterHomingProjectile(Main.spriteBatch, Projectile, hasTarget, 1f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === 4-LAYER CENTRAL GLIMMER CASCADE ===
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.4f + layer * 0.18f;
                float layerAlpha = 0.9f - layer * 0.15f;
                float layerHue = HueMin + (layer / 4f) * (HueMax - HueMin);
                Color layerColor = Color.Lerp(Color.White, Main.hslToRgb(layerHue, 0.75f, 0.7f), layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 20 - layer * 2);
            }

            // === 4 EXPANDING GLOW RINGS with hslToRgb ===
            for (int ring = 0; ring < 4; ring++)
            {
                float ringHue = HueMin + (ring / 4f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.75f, 0.68f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.35f + ring * 0.14f, 16 + ring * 3);
            }

            // === 8-POINT BLIZZARD CRYSTAL PATTERN ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 crystalPos = Projectile.Center + angle.ToRotationVector2() * 28f;
                float crystalHue = HueMin + (i / 8f) * (HueMax - HueMin);
                Color crystalColor = Main.hslToRgb(crystalHue, 0.75f, 0.65f);
                CustomParticles.GenericFlare(crystalPos, crystalColor, 0.45f, 18);
            }

            // === MUSIC NOTE FINALE - 10 notes burst ===
            for (int n = 0; n < 10; n++)
            {
                float angle = MathHelper.TwoPi * n / 10f;
                Vector2 noteVel = angle.ToRotationVector2() * 4.5f;
                float noteHue = HueMin + (n / 10f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.75f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.9f, 42);
            }

            // === RADIAL SPARKLE BURST ===
            for (int s = 0; s < 12; s++)
            {
                float angle = MathHelper.TwoPi * s / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 6f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, FrostWhite * 0.8f, 0.42f, 24);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW PARTICLE BURST ===
            for (int i = 0; i < 12; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.72f, 0.62f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // === DENSE FROST DUST ===
            for (int i = 0; i < 14; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.7f, 0.6f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Frost, dustVel, 0, dustColor, 1.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, CrystalCyan.ToVector3() * 0.9f);
        }
    }

    /// <summary>
    /// Hypothermia Debuff - Stacking slow, at 5 stacks freezes enemy
    /// </summary>
    public class HypothermiaBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Frostburn;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Count stacks (multiple instances of this buff)
            int stacks = 0;
            for (int i = 0; i < npc.buffType.Length; i++)
            {
                if (npc.buffType[i] == Type && npc.buffTime[i] > 0)
                    stacks++;
            }

            // Apply increasing slow
            float slowMult = 1f - (stacks * 0.1f); // 10% slow per stack
            slowMult = Math.Max(0.3f, slowMult);

            // At 5 stacks, freeze
            if (stacks >= 5)
            {
                npc.AddBuff(BuffID.Frozen, 90);
                
                // Clear Hypothermia stacks
                for (int i = 0; i < npc.buffType.Length; i++)
                {
                    if (npc.buffType[i] == Type)
                        npc.buffTime[i] = 0;
                }

                // Freeze VFX
                CustomParticles.GenericFlare(npc.Center, new Color(100, 255, 255), 0.8f, 25);
                // Frost sparkle burst 
                var frostSparkle = new SparkleParticle(npc.Center, Vector2.Zero, new Color(150, 220, 255), 0.6f * 0.6f, 20);
                MagnumParticleHandler.SpawnParticle(frostSparkle);
            }

            // Visual frost particles on debuffed enemies
            if (Main.rand.NextBool(12))
            {
                Vector2 frostPos = npc.Center + Main.rand.NextVector2Circular(npc.width / 2f, npc.height / 2f);
                Vector2 frostVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 0.5f));
                var frost = new GenericGlowParticle(frostPos, frostVel, new Color(150, 220, 255) * 0.4f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(frost);
            }
        }
    }
}
