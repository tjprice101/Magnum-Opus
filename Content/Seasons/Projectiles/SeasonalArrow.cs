using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Seasonal Arrow - Main projectile for Seasonal Bow
    /// Changes effects based on the season (ai[0])
    /// TRUE_VFX_STANDARDS: Dense dust, orbiting music notes, layered spinning flares per season
    /// </summary>
    public class SeasonalArrow : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ParticleTrail2";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        // Season-specific hue ranges for color oscillation
        private float HueMin => SeasonIndex switch
        {
            0 => 0.92f,  // Spring - pink/magenta
            1 => 0.08f,  // Summer - orange/gold
            2 => 0.06f,  // Autumn - orange/brown
            _ => 0.52f   // Winter - cyan/blue
        };
        
        private float HueMax => SeasonIndex switch
        {
            0 => 0.98f,  // Spring - pink/magenta
            1 => 0.14f,  // Summer - orange/gold
            2 => 0.12f,  // Autumn - orange/brown
            _ => 0.62f   // Winter - cyan/blue
        };

        private int SeasonIndex => (int)Projectile.ai[0];

        private Color PrimaryColor => SeasonIndex switch
        {
            0 => SpringPink,
            1 => SummerGold,
            2 => AutumnOrange,
            _ => WinterBlue
        };

        private Color SecondaryColor => SeasonIndex switch
        {
            0 => SpringGreen,
            1 => SummerOrange,
            2 => AutumnBrown,
            _ => WinterWhite
        };
        
        private int SeasonDustType => SeasonIndex switch
        {
            0 => DustID.PinkFairy,
            1 => DustID.OrangeTorch,
            2 => DustID.Torch,
            _ => DustID.Frost
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Light gravity
            Projectile.velocity.Y += 0.05f;

            // === DENSE SEASONAL DUST TRAIL - 2+ per frame with hslToRgb color oscillation ===
            for (int i = 0; i < 2; i++)
            {
                float hue = HueMin + ((Main.GameUpdateCount * 0.025f + i * 0.25f) % 1f) * (HueMax - HueMin);
                Color oscillatingColor = Main.hslToRgb(hue, 0.75f, 0.6f);
                
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust d = Dust.NewDustPerfect(dustPos, SeasonDustType, dustVel, 0, oscillatingColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // === CONTRASTING SPARKLES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                var sparkle = new SparkleParticle(sparklePos, -Projectile.velocity * 0.08f, SecondaryColor * 0.75f, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === FREQUENT FLARES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.75f, 0.65f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.4f, 14);
            }

            // === SEASON-SPECIFIC GLOW TRAIL ===
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                float trailHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(trailHue, 0.7f, 0.55f);
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.26f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === ORBITING MUSIC NOTES - Locked to projectile (TRUE_VFX_STANDARDS) ===
            float musicOrbitAngle = Main.GameUpdateCount * 0.1f;
            float musicOrbitRadius = 13f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 3f;
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
            
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 noteVel = Projectile.velocity * 0.65f + noteAngle.ToRotationVector2() * 0.5f;
                    float noteHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.7f);
                    
                    ThemedParticles.MusicNote(Projectile.Center + noteOffset, noteVel, noteColor, 0.82f * shimmer, 36);
                    
                    // Sparkle companion
                    var noteSparkle = new SparkleParticle(Projectile.Center + noteOffset, noteVel * 0.4f, SecondaryColor * 0.55f, 0.24f, 14);
                    MagnumParticleHandler.SpawnParticle(noteSparkle);
                }
            }

            // === SEASONAL ELEMENT ORBIT ===
            if (Main.GameUpdateCount % 3 == 0)
            {
                float elementOrbitAngle = Main.GameUpdateCount * 0.12f;
                for (int c = 0; c < 4; c++)
                {
                    float elementAngle = elementOrbitAngle + MathHelper.TwoPi * c / 4f;
                    float elementRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + c) * 3f;
                    Vector2 elementPos = Projectile.Center + elementAngle.ToRotationVector2() * elementRadius;
                    float elementHue = HueMin + (c / 4f) * (HueMax - HueMin);
                    Color elementColor = Main.hslToRgb(elementHue, 0.8f, 0.65f);
                    CustomParticles.GenericFlare(elementPos, elementColor, 0.2f, 10);
                }
            }

            // Autumn-specific: Leave decay zones
            if (SeasonIndex == 2 && Projectile.timeLeft % 15 == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<DecayZoneProjectile>(), Projectile.damage / 4, 0f, Projectile.owner);
            }

            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.55f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Season-specific on-hit effects
            switch (SeasonIndex)
            {
                case 0: // Spring - Split into homing petals
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 5f;
                        Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, petalVel,
                            ModContent.ProjectileType<HomingPetalProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                    target.AddBuff(BuffID.Poisoned, 180);
                    break;

                case 1: // Summer - Solar explosion
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 flareVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, flareVel,
                            ModContent.ProjectileType<SummerArrowFlareProjectile>(), Projectile.damage / 4, Projectile.knockBack * 0.2f, Projectile.owner);
                    }
                    target.AddBuff(BuffID.OnFire3, 300);
                    target.AddBuff(BuffID.Daybreak, 120);
                    break;

                case 2: // Autumn - Life steal, debuffs
                    Player owner = Main.player[Projectile.owner];
                    owner.Heal(Math.Max(1, damageDone / 20));
                    target.AddBuff(BuffID.CursedInferno, 240);
                    target.AddBuff(BuffID.ShadowFlame, 180);
                    break;

                case 3: // Winter - Freeze and shatter
                    target.AddBuff(BuffID.Frostburn2, 300);
                    if (Main.rand.NextFloat() < 0.35f)
                    {
                        target.AddBuff(BuffID.Frozen, 90);
                        
                        // Shatter damage to nearby
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.dontTakeDamage)
                            {
                                float dist = Vector2.Distance(target.Center, npc.Center);
                                if (dist < 100f)
                                {
                                    npc.SimpleStrikeNPC(Projectile.damage / 3, hit.HitDirection, false, Projectile.knockBack * 0.3f, DamageClass.Ranged);
                                    npc.AddBuff(BuffID.Frostburn2, 120);
                                }
                            }
                        }
                    }
                    break;
            }

            // === CENTRAL FLASH CASCADE (3 layers) ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.85f, 20);
            CustomParticles.GenericFlare(target.Center, PrimaryColor, 0.65f, 18);
            CustomParticles.GenericFlare(target.Center, SecondaryColor, 0.5f, 16);

            // === MUSIC NOTE IMPACT RING - 6 notes with hslToRgb gradient ===
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 4f;
                float noteHue = HueMin + (n / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.7f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 38);
            }

            // === 6-POINT SEASONAL BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 burstPos = target.Center + angle.ToRotationVector2() * 20f;
                float burstHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.75f, 0.6f);
                CustomParticles.GenericFlare(burstPos, burstColor, 0.45f, 15);
            }

            // === EXPANDING HALO RINGS ===
            CustomParticles.HaloRing(target.Center, PrimaryColor, 0.45f, 16);
            CustomParticles.HaloRing(target.Center, SecondaryColor * 0.7f, 0.35f, 14);

            // === SPARKLE BURST ===
            for (int s = 0; s < 8; s++)
            {
                float sAngle = MathHelper.TwoPi * s / 8f;
                var sparkle = new SparkleParticle(target.Center, sAngle.ToRotationVector2() * 3.5f, SecondaryColor * 0.8f, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW BURST ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(hue, 0.7f, 0.55f);
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // === SEEKING CRYSTALS - Season-based burst ===
            if (Main.rand.NextBool(3))
            {
                int crystalDamage = (int)(damageDone * 0.18f);
                switch (SeasonIndex)
                {
                    case 0: // Spring
                        SeekingCrystalHelper.SpawnSpringCrystals(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, crystalDamage, Projectile.knockBack, Projectile.owner, 4);
                        break;
                    case 1: // Summer
                        SeekingCrystalHelper.SpawnSummerCrystals(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, crystalDamage, Projectile.knockBack, Projectile.owner, 4);
                        break;
                    case 2: // Autumn
                        SeekingCrystalHelper.SpawnAutumnCrystals(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, crystalDamage, Projectile.knockBack, Projectile.owner, 4);
                        break;
                    case 3: // Winter
                        SeekingCrystalHelper.SpawnWinterCrystals(Projectile.GetSource_FromThis(), target.Center, Projectile.velocity, crystalDamage, Projectile.knockBack, Projectile.owner, 4);
                        break;
                }
            }
            
            // === DYNAMIC: Season-based Impact ===
            switch (SeasonIndex)
            {
                case 0: SpringImpact(target.Center, 1f); SpringPetalBloom(target.Center, 1.1f); break;
                case 1: SummerImpact(target.Center, 1f); SummerSolarFlare(target.Center, 1.2f); break;
                case 2: AutumnImpact(target.Center, 1f); AutumnLeafCascade(target.Center, 1f); break;
                case 3: WinterImpact(target.Center, 1f); WinterCrystallineShatter(target.Center, 1.1f); break;
            }
            DramaticImpact(target.Center, PrimaryColor, SecondaryColor, 0.5f, 20);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural seasonal rendering - dynamically selects season based on ai[0]
            ProceduralProjectileVFX.DrawSeasonalProjectile(Main.spriteBatch, Projectile, SeasonIndex, 0.55f);
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
                Color layerColor = Color.Lerp(Color.White, Main.hslToRgb(layerHue, 0.75f, 0.68f), layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 18 - layer * 2);
            }

            // === 4 EXPANDING GLOW RINGS with hslToRgb ===
            for (int ring = 0; ring < 4; ring++)
            {
                float ringHue = HueMin + (ring / 4f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.7f, 0.62f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.12f, 14 + ring * 3);
            }

            // === 6-POINT SEASONAL PATTERN ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 patternPos = Projectile.Center + angle.ToRotationVector2() * 22f;
                float patternHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color patternColor = Main.hslToRgb(patternHue, 0.75f, 0.62f);
                CustomParticles.GenericFlare(patternPos, patternColor, 0.4f, 16);
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
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, SecondaryColor * 0.75f, 0.38f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW PARTICLE BURST ===
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.7f, 0.58f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.7f);
        }
    }

    /// <summary>
    /// Homing Petal - Spring arrow split projectile
    /// TRUE_VFX_STANDARDS: Dense dust, orbiting music notes, layered spinning flares
    /// </summary>
    public class HomingPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle13";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color DeepPink = new Color(255, 120, 180);
        
        // Spring pink/magenta hue range
        private const float HueMin = 0.92f;
        private const float HueMax = 0.98f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;

            // Homing
            float homingRange = 350f;
            float homingStrength = 0.06f;

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
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 10f, homingStrength);
            }

            // === DENSE SPRING DUST TRAIL - 2+ per frame with hslToRgb ===
            for (int i = 0; i < 2; i++)
            {
                float hue = HueMin + ((Main.GameUpdateCount * 0.025f + i * 0.25f) % 1f) * (HueMax - HueMin);
                Color oscillatingSpring = Main.hslToRgb(hue, 0.78f, 0.65f);
                
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 dustVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.2f, 1.2f);
                
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PinkFairy, dustVel, 0, oscillatingSpring, 1.35f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // === CONTRASTING SPARKLES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                var sparkle = new SparkleParticle(sparklePos, -Projectile.velocity * 0.06f, SpringGreen * 0.7f, 0.26f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === FREQUENT FLARES - 1 in 2 ===
            if (Main.rand.NextBool(2))
            {
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.8f, 0.68f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                CustomParticles.GenericFlare(flarePos, flareColor, 0.32f, 12);
            }

            // === PETAL GLOW TRAIL ===
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                float trailHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(trailHue, 0.72f, 0.55f);
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === ORBITING MUSIC NOTES - Locked to projectile (TRUE_VFX_STANDARDS) ===
            float musicOrbitAngle = Main.GameUpdateCount * 0.12f;
            float musicOrbitRadius = 9f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 2f;
            float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
            
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 2f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 noteVel = Projectile.velocity * 0.5f + noteAngle.ToRotationVector2() * 0.4f;
                    float noteHue = HueMin + (i / 2f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.72f);
                    
                    ThemedParticles.MusicNote(Projectile.Center + noteOffset, noteVel, noteColor, 0.72f * shimmer, 28);
                }
            }

            // === ORBITING PETAL POINTS ===
            if (Main.GameUpdateCount % 4 == 0)
            {
                float petalOrbitAngle = Main.GameUpdateCount * 0.15f;
                for (int p = 0; p < 3; p++)
                {
                    float petalAngle = petalOrbitAngle + MathHelper.TwoPi * p / 3f;
                    float petalRadius = 6f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + p) * 2f;
                    Vector2 petalPos = Projectile.Center + petalAngle.ToRotationVector2() * petalRadius;
                    float petalHue = HueMin + (p / 3f) * (HueMax - HueMin);
                    Color petalColor = Main.hslToRgb(petalHue, 0.8f, 0.68f);
                    CustomParticles.GenericFlare(petalPos, petalColor, 0.15f, 8);
                }
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);
            
            // === CENTRAL FLASH CASCADE (3 layers) ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.55f, 16);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.42f, 14);
            CustomParticles.GenericFlare(target.Center, SpringGreen, 0.32f, 12);

            // === MUSIC NOTE IMPACT - 4 notes ===
            for (int n = 0; n < 4; n++)
            {
                float angle = MathHelper.TwoPi * n / 4f;
                Vector2 noteVel = angle.ToRotationVector2() * 2.5f;
                float noteHue = HueMin + (n / 4f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.7f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.72f, 28);
            }

            // === HALO RING ===
            CustomParticles.HaloRing(target.Center, SpringPink, 0.3f, 12);

            // === GLOW BURST ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(hue, 0.72f, 0.55f);
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // === DYNAMIC SPRING IMPACT ===
            SpringImpact(target.Center, 0.6f);
            SpringSunshower(target.Center, 0.5f); // Unique sunlit particles
            DramaticImpact(target.Center, SpringPink, SpringGreen, 0.35f, 15);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Spring rendering - cherry blossom pink and green
            ProceduralProjectileVFX.DrawSpringProjectile(Main.spriteBatch, Projectile, 0.4f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === 3-LAYER CENTRAL GLIMMER CASCADE ===
            for (int layer = 0; layer < 3; layer++)
            {
                float layerScale = 0.25f + layer * 0.1f;
                float layerAlpha = 0.78f - layer * 0.15f;
                float layerHue = HueMin + (layer / 3f) * (HueMax - HueMin);
                Color layerColor = Color.Lerp(Color.White, Main.hslToRgb(layerHue, 0.75f, 0.68f), layer / 3f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 14 - layer * 2);
            }

            // === 3 EXPANDING GLOW RINGS ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringHue = HueMin + (ring / 3f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.72f, 0.6f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.2f + ring * 0.08f, 10 + ring * 2);
            }

            // === MUSIC NOTE FINALE - 5 notes ===
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 2.5f;
                float noteHue = HueMin + (n / 5f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.85f, 0.72f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.72f, 30);
            }

            // === SPARKLE BURST ===
            for (int s = 0; s < 6; s++)
            {
                float angle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, SpringGreen * 0.7f, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW BURST ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.7f, 0.55f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.5f);
        }
    }

    /// <summary>
    /// Solar Flare - Summer arrow explosion projectile
    /// TRUE_VFX_STANDARDS: 6-layer spinning flares, dense dust, orbiting music notes, glimmer cascade
    /// </summary>
    public class SummerArrowFlareProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField8";
        
        // Summer hue range: orange/gold (0.08-0.14)
        private const float HueMin = 0.08f;
        private const float HueMax = 0.14f;
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerYellow = new Color(255, 240, 100);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.95f;

            // === DENSE DUST TRAIL - 2+ per frame GUARANTEED ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.SolarFlare, dustVel, 80, SummerGold, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // === CONTRASTING SPARKLES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Dust contrast = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 0, SummerYellow, 1.2f);
                contrast.noGravity = true;
            }

            // === FREQUENT FLARES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(6f, 6f);
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.95f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.35f, 12);
            }

            // === SOLAR GLOW TRAIL ===
            if (Main.rand.NextBool(2))
            {
                Vector2 glowVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f);
                float glowHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color glowColor = Main.hslToRgb(glowHue, 0.85f, 0.65f);
                var glow = new GenericGlowParticle(Projectile.Center, glowVel, glowColor * 0.7f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // === ORBITING MUSIC NOTES - LOCKED TO PROJECTILE ===
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            if (Main.rand.NextBool(6))
            {
                for (int n = 0; n < 2; n++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * n / 2f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * 12f;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.7f + noteAngle.ToRotationVector2() * 0.4f;
                    float noteHue = HueMin + (n / 2f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.72f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.72f, 28);

                    var sparkle = new SparkleParticle(notePos, noteVel * 0.4f, SummerYellow * 0.6f, 0.22f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // === 3-ELEMENT ORBIT (like sun rays) ===
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 3; i++)
                {
                    float rayAngle = orbitAngle * 1.3f + MathHelper.TwoPi * i / 3f;
                    Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 10f;
                    float rayHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color rayColor = Main.hslToRgb(rayHue, 0.92f, 0.78f);
                    CustomParticles.GenericFlare(rayPos, rayColor * 0.55f, 0.18f, 10);
                }
            }

            // === SPARKLE ACCENT ===
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 
                    Main.rand.NextVector2Circular(1.5f, 1.5f), SummerOrange * 0.8f, 0.28f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === COLOR OSCILLATION - hslToRgb ===
            if (Main.rand.NextBool(3))
            {
                float hue = (Main.GameUpdateCount * 0.025f + Main.rand.NextFloat(0.1f)) % 1f;
                hue = HueMin + (hue * (HueMax - HueMin));
                Color shiftColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center, shiftColor, 0.28f, 10);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.65f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // === 3-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.55f, 18);
            CustomParticles.GenericFlare(target.Center, SummerYellow, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, SummerGold, 0.38f, 14);

            // === 4 MUSIC NOTES WITH GRADIENT ===
            for (int n = 0; n < 4; n++)
            {
                float angle = MathHelper.TwoPi * n / 4f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float noteHue = HueMin + (n / 4f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.72f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.75f, 28);
            }

            // === HALO RING ===
            CustomParticles.HaloRing(target.Center, SummerOrange, 0.35f, 14);

            // === GLOW PARTICLES ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(4f, 4f);
                float glowHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color glowColor = Main.hslToRgb(glowHue, 0.8f, 0.65f);
                var glow = new GenericGlowParticle(target.Center, glowVel, glowColor * 0.7f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // === DYNAMIC SUMMER IMPACT ===
            SummerImpact(target.Center, 0.65f);
            SummerZenithStrike(target.Center, 0.6f); // Unique zenith strike
            DramaticImpact(target.Center, SummerOrange, SummerGold, 0.4f, 16);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Summer rendering - solar flare gold and orange
            ProceduralProjectileVFX.DrawSummerProjectile(Main.spriteBatch, Projectile, 0.5f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === 4-LAYER CENTRAL GLIMMER CASCADE ===
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.3f + layer * 0.12f;
                float layerAlpha = 0.8f - layer * 0.15f;
                float layerHue = HueMin + (layer / 4f) * (HueMax - HueMin);
                Color layerColor = Color.Lerp(Color.White, Main.hslToRgb(layerHue, 0.85f, 0.72f), layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 16 - layer * 2);
            }

            // === 4 EXPANDING GLOW RINGS WITH hslToRgb ===
            for (int ring = 0; ring < 4; ring++)
            {
                float ringHue = HueMin + (ring / 4f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.82f, 0.68f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.25f + ring * 0.1f, 12 + ring * 2);
            }

            // === 6-POINT SOLAR BURST PATTERN ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 22f;
                float patternHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color patternColor = Main.hslToRgb(patternHue, 0.88f, 0.72f);
                CustomParticles.GenericFlare(Projectile.Center + offset, patternColor, 0.3f, 14);
            }

            // === MUSIC NOTE FINALE - 6 notes ===
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                float noteHue = HueMin + (n / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.75f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.78f, 32);
            }

            // === SPARKLE BURST ===
            for (int s = 0; s < 8; s++)
            {
                float angle = MathHelper.TwoPi * s / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float sparkHue = HueMin + (s / 8f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.85f, 0.75f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, sparkColor, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW BURST ===
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.82f, 0.65f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.75f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.7f);
        }
    }

    /// <summary>
    /// Decay Zone - Autumn arrow damage zone
    /// TRUE_VFX_STANDARDS: 6-layer spinning flares, dense dust, orbiting music notes, glimmer cascade
    /// </summary>
    public class DecayZoneProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField9";
        
        // Autumn hue range: orange/brown (0.06-0.12)
        private const float HueMin = 0.06f;
        private const float HueMax = 0.12f;
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            float lifeProgress = Projectile.timeLeft / 90f;
            Projectile.alpha = (int)(100 + (155 * (1f - lifeProgress)));

            // === DENSE DUST TRAIL - 2+ per frame GUARANTEED ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(28f, 28f);
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2.5f, -0.5f));
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.75f, 0.55f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.AmberBolt, dustVel, 60, dustColor, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // === CONTRASTING SPARKLES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                Dust contrast = Dust.NewDustPerfect(sparkPos, DustID.GoldFlame,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.3f)), 0, AutumnGold, 1.1f);
                contrast.noGravity = true;
            }

            // === FREQUENT FLARES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(22f, 22f);
                float flareHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(flareHue, 0.82f, 0.62f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor * lifeProgress, 0.28f, 10);
            }

            // === DECAY GLOW PARTICLES (rising) ===
            if (Main.rand.NextBool(2))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 particleVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-2f, -0.5f));
                float particleHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color particleColor = Main.hslToRgb(particleHue, 0.72f, 0.55f);
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor * 0.6f * lifeProgress, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // === ORBITING MUSIC NOTES - LOCKED TO ZONE CENTER ===
            float orbitAngle = Main.GameUpdateCount * 0.06f;
            if (Main.rand.NextBool(8))
            {
                for (int n = 0; n < 3; n++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * n / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * 25f;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = (noteAngle + MathHelper.PiOver2).ToRotationVector2() * 0.8f + new Vector2(0, -0.5f);
                    float noteHue = HueMin + (n / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.82f, 0.65f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor * lifeProgress, 0.72f, 28);

                    var sparkle = new SparkleParticle(notePos, noteVel * 0.4f, AutumnGold * 0.5f * lifeProgress, 0.2f, 14);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // === 4-ELEMENT DECAY ORBIT (leaf-like swirl) ===
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 4; i++)
                {
                    float leafAngle = orbitAngle * 1.2f + MathHelper.TwoPi * i / 4f;
                    float leafRadius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 6f;
                    Vector2 leafPos = Projectile.Center + leafAngle.ToRotationVector2() * leafRadius;
                    float leafHue = HueMin + (i / 4f) * (HueMax - HueMin);
                    Color leafColor = Main.hslToRgb(leafHue, 0.78f, 0.58f);
                    CustomParticles.GenericFlare(leafPos, leafColor * 0.5f * lifeProgress, 0.15f, 8);
                }
            }

            // === GLYPH ACCENT - Decay runes ===
            if (Main.rand.NextBool(10))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(22f, 22f), AutumnOrange * 0.55f * lifeProgress, 0.28f, -1);
            }

            // === COLOR OSCILLATION - hslToRgb ===
            if (Main.rand.NextBool(4))
            {
                float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat(0.1f)) % 1f;
                hue = HueMin + (hue * (HueMax - HueMin));
                Color shiftColor = Main.hslToRgb(hue, 0.75f, 0.6f);
                Vector2 shiftPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.GenericFlare(shiftPos, shiftColor * lifeProgress, 0.22f, 10);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.35f * lifeProgress);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 90);

            // === 3-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White * 0.7f, 0.4f, 14);
            CustomParticles.GenericFlare(target.Center, AutumnGold, 0.35f, 12);
            CustomParticles.GenericFlare(target.Center, AutumnOrange, 0.3f, 10);

            // === 3 MUSIC NOTES WITH GRADIENT ===
            for (int n = 0; n < 3; n++)
            {
                float angle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3f);
                float noteHue = HueMin + (n / 3f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.8f, 0.62f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.7f, 25);
            }

            // === HALO RING ===
            CustomParticles.HaloRing(target.Center, AutumnBrown * 0.7f, 0.28f, 12);

            // === GLOW PARTICLES ===
            for (int i = 0; i < 4; i++)
            {
                Vector2 glowVel = Main.rand.NextVector2Circular(3f, 3f);
                float glowHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color glowColor = Main.hslToRgb(glowHue, 0.72f, 0.55f);
                var glow = new GenericGlowParticle(target.Center, glowVel, glowColor * 0.6f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // === DYNAMIC AUTUMN IMPACT ===
            AutumnImpact(target.Center, 0.55f);
            AutumnHarvestMoon(target.Center, 0.5f); // Unique harvest glow
            DramaticImpact(target.Center, AutumnOrange, AutumnGold, 0.35f, 14);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Autumn rendering - decay zone with harvest colors
            ProceduralProjectileVFX.DrawAutumnProjectile(Main.spriteBatch, Projectile, 0.65f);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // === 3-LAYER CENTRAL GLIMMER CASCADE ===
            for (int layer = 0; layer < 3; layer++)
            {
                float layerScale = 0.35f + layer * 0.12f;
                float layerAlpha = 0.7f - layer * 0.15f;
                float layerHue = HueMin + (layer / 3f) * (HueMax - HueMin);
                Color layerColor = Color.Lerp(AutumnGold, Main.hslToRgb(layerHue, 0.75f, 0.58f), layer / 3f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 14 - layer * 2);
            }

            // === 3 EXPANDING GLOW RINGS WITH hslToRgb ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringHue = HueMin + (ring / 3f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.72f, 0.55f);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + ring * 0.1f, 12 + ring * 2);
            }

            // === 6-POINT DECAY PATTERN ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                float patternHue = HueMin + (i / 6f) * (HueMax - HueMin);
                Color patternColor = Main.hslToRgb(patternHue, 0.75f, 0.58f);
                CustomParticles.GenericFlare(Projectile.Center + offset, patternColor * 0.65f, 0.25f, 12);
            }

            // === MUSIC NOTE FINALE - 5 notes ===
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                float noteHue = HueMin + (n / 5f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.8f, 0.65f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.75f, 30);
            }

            // === SPARKLE BURST ===
            for (int s = 0; s < 6; s++)
            {
                float angle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                float sparkHue = HueMin + (s / 6f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.75f, 0.62f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, sparkColor, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === GLOW BURST ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                float burstHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color burstColor = Main.hslToRgb(burstHue, 0.72f, 0.55f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.65f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.5f);
        }
    }
}
