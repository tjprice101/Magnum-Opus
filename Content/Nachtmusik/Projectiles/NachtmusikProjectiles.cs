using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik.Debuffs;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Nachtmusik.Projectiles
{
    #region Melee Projectiles
    
    /// <summary>
    /// Nocturnal Executioner's massive spectral blade projectile.
    /// Homes toward enemies and explodes on contact.
    /// TRUE_VFX_STANDARDS: Layered spinning flares, orbiting music notes, hslToRgb oscillation
    /// </summary>
    public class NocturnalBladeProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/Glyphs10";
        
        // Nachtmusik hue range - violet/purple spectrum (0.75-0.85)
        private const float HueMin = 0.75f;
        private const float HueMax = 0.85f;
        
        private float orbitAngle;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            orbitAngle += 0.12f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Homing toward enemies
            NPC target = FindClosestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 18f, 0.06f);
            }
            
            // === DENSE DUST TRAIL - 2+ per frame GUARANTEED (TRUE_VFX_STANDARDS) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, -Projectile.velocity * 0.1f, 0, NachtmusikCosmicVFX.Gold, 1.3f);
                gold.noGravity = true;
            }
            
            // === FREQUENT FLARES with hslToRgb oscillation - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color oscillatedColor = Main.hslToRgb(hue, 0.88f, 0.72f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, oscillatedColor, 0.42f, 16);
            }
            
            // === GLOW TRAIL with gradient ===
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.68f);
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === 3 ORBITING MUSIC NOTES - LOCKED TO PROJECTILE (TRUE_VFX_STANDARDS) ===
            float noteOrbitAngle = Main.GameUpdateCount * 0.08f;
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 18f;
                    Vector2 noteVel = Projectile.velocity * 0.85f + noteAngle.ToRotationVector2() * 0.5f;
                    float noteHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.75f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.78f, 32);
                }
            }
            
            // === SPARKLE ACCENT ===
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), -Projectile.velocity * 0.1f, NachtmusikCosmicVFX.StarWhite, 0.38f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === DYNAMIC PARTICLE EFFECTS - Celestial Pulsing Core ===
            if (Main.GameUpdateCount % 5 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, 0.3f, 22, 0.16f, 0.25f);
            }
            
            // === DYNAMIC: Concentric cosmic orbits for celestial aura ===
            if (Main.GameUpdateCount % 35 == 0)
            {
                ConcentricOrbits(Projectile.Center, NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, 2, 3, 12f, 8f, 0.1f, 0.18f, 26);
            }
            
            // === DYNAMIC: Twinkling cosmic sparkles ===
            if (Main.rand.NextBool(6))
            {
                TwinklingSparks(Projectile.Center, NachtmusikCosmicVFX.StarWhite, 2, 10f, 0.22f, 20);
            }

            // === 4-ELEMENT CELESTIAL ORBIT ===
            if (Projectile.timeLeft % 4 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 16f;
                    float sparkHue = HueMin + (i / 4f) * (HueMax - HueMin);
                    Color sparkColor = Main.hslToRgb(sparkHue, 0.88f, 0.78f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor, 0.28f, 12);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.8f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            
            // === 3-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.7f, 22);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Gold, 0.55f, 20);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Violet, 0.45f, 18);
            
            // === 6 MUSIC NOTES WITH hslToRgb GRADIENT ===
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float noteHue = HueMin + (n / 6f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.72f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.8f, 35);
            }
            
            // === STARBURST IMPACT ===
            NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 0.8f, 2);
            
            // === DYNAMIC IMPACT: STYLE 1 - Constellation Cascade (cosmic blade) ===
            NachtConstellationCascade(target.Center, 1.1f);
            
            // === DYNAMIC: Celestial burst with spiraling particles ===
            CelestialBurst(target.Center, 0.9f);
            
            // === 3 HALO RINGS ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringHue = HueMin + (ring / 3f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.82f, 0.7f);
                CustomParticles.HaloRing(target.Center, ringColor, 0.35f + ring * 0.12f, 15 + ring * 2);
            }
            
            // === 8 SPARKLE BURST ===
            for (int s = 0; s < 8; s++)
            {
                float angle = MathHelper.TwoPi * s / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                float sparkHue = HueMin + (s / 8f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.85f, 0.75f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, sparkColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === SEEKING CRYSTALS ===
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH STYLE 1: CONSTELLATION IMPLOSION ===
            // Stars converge to center then explode outward as connected constellation
            DynamicParticleEffects.NachtDeathConstellation(Projectile.Center, 1.0f);
            
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Nachtmusik rendering - violet/gold cosmic glyph
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.6f);
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
    
    /// <summary>
    /// Midnight's Crescendo's crescendo wave projectile.
    /// Expands as it travels, dealing more damage at larger sizes.
    /// TRUE_VFX_STANDARDS: Layered spinning flares, orbiting music notes, hslToRgb oscillation
    /// </summary>
    public class CrescendoWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc6";
        
        // Nachtmusik hue range - violet/purple spectrum (0.75-0.85)
        private const float HueMin = 0.75f;
        private const float HueMax = 0.85f;
        
        private float growthFactor = 1f;
        private float orbitAngle = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }
        
        public override void AI()
        {
            // Grow over time
            growthFactor = 1f + (120 - Projectile.timeLeft) / 60f;
            Projectile.scale = growthFactor;
            orbitAngle += 0.1f;
            
            // Slow down as it grows
            Projectile.velocity *= 0.985f;
            
            // Rotation
            Projectile.rotation += 0.08f;
            
            // === DENSE DUST TRAIL - 2+ per frame GUARANTEED (TRUE_VFX_STANDARDS) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(18f * growthFactor, 18f * growthFactor);
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.85f, 0.7f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, dustColor, 1.4f * growthFactor);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(15f * growthFactor, 15f * growthFactor);
                Dust gold = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, NachtmusikCosmicVFX.Gold, 1.2f);
                gold.noGravity = true;
            }
            
            // === FREQUENT FLARES with hslToRgb oscillation - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color oscillatedColor = Main.hslToRgb(hue, 0.88f, 0.72f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(20f * growthFactor, 20f * growthFactor);
                CustomParticles.GenericFlare(flarePos, oscillatedColor, 0.35f * growthFactor, 14);
            }
            
            // === GLOW TRAIL with gradient ===
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f * growthFactor, 20f * growthFactor);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.68f);
                var trail = new GenericGlowParticle(Projectile.Center + offset, Main.rand.NextVector2Circular(2f, 2f),
                    trailColor * 0.6f, 0.28f * growthFactor, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === 3 ORBITING MUSIC NOTES - LOCKED TO PROJECTILE (TRUE_VFX_STANDARDS) ===
            float noteOrbitAngle = Main.GameUpdateCount * 0.08f;
            if (Projectile.timeLeft % 8 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * (22f * growthFactor);
                    Vector2 noteVel = noteAngle.ToRotationVector2() * 2.5f;
                    float noteHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.75f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.78f, 28);
                }
            }
            
            // === SPARKLE ACCENT ===
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(18f * growthFactor, 18f * growthFactor);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), NachtmusikCosmicVFX.StarWhite, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === 5-ELEMENT CRESCENDO ORBIT ===
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 5f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * (15f * growthFactor);
                    float sparkHue = HueMin + (i / 5f) * (HueMax - HueMin);
                    Color sparkColor = Main.hslToRgb(sparkHue, 0.88f, 0.78f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor, 0.25f * growthFactor, 12);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.9f * growthFactor);
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= growthFactor;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            // === 3-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.65f * growthFactor, 20);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Gold, 0.5f * growthFactor, 18);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Violet, 0.4f * growthFactor, 16);
            
            // === 5 MUSIC NOTES WITH hslToRgb GRADIENT ===
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                float noteHue = HueMin + (n / 5f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.72f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.78f, 32);
            }
            
            // === 2 HALO RINGS ===
            for (int ring = 0; ring < 2; ring++)
            {
                float ringHue = HueMin + (ring / 2f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.82f, 0.7f);
                CustomParticles.HaloRing(target.Center, ringColor, (0.35f + ring * 0.12f) * growthFactor, 14 + ring * 2);
            }
            
            // === 6 SPARKLE BURST ===
            for (int s = 0; s < 6; s++)
            {
                float angle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 6f);
                float sparkHue = HueMin + (s / 6f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.85f, 0.75f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, sparkColor, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === DYNAMIC IMPACT: STYLE 2 - Crescent Wave (crescendo wave) ===
            NachtCrescentWave(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 0.95f * growthFactor);
            DramaticImpact(target.Center, NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, 0.45f * growthFactor, 18);
            
            // === SEEKING CRYSTALS ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH STYLE 2: CRESCENT SHATTER ===
            // Crescent moon shape shatters into shards that fly outward in arc pattern
            DynamicParticleEffects.NachtDeathCrescentShatter(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), growthFactor);
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 1.1f * growthFactor);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Procedural Nachtmusik rendering - growing sword arc slash
            float scale = growthFactor * (1f + (float)Math.Sin(Main.GameUpdateCount * 0.055f * 2.2f) * 0.18f);
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, scale * 0.7f);
            return false;
        }
    }
    
    #endregion
    
    #region Ranged Projectiles
    
    /// <summary>
    /// Constellation Piercer's star bolt that chains to nearby enemies.
    /// TRUE_VFX_STANDARDS: Layered spinning flares, orbiting music notes, hslToRgb oscillation
    /// </summary>
    public class ConstellationBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";
        
        // Nachtmusik hue range - violet/purple spectrum (0.75-0.85)
        private const float HueMin = 0.75f;
        private const float HueMax = 0.85f;
        
        private int chainCount = 0;
        private const int MaxChains = 4;
        private List<int> hitEnemies = new List<int>();
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // === DENSE DUST TRAIL - 2+ per frame GUARANTEED (TRUE_VFX_STANDARDS) ===
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                float dustHue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color dustColor = Main.hslToRgb(dustHue, 0.88f, 0.72f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, dustColor, 1.35f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // === CONTRASTING SPARKLES - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Dust gold = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, -Projectile.velocity * 0.1f, 0, NachtmusikCosmicVFX.Gold, 1.15f);
                gold.noGravity = true;
            }
            
            // === FREQUENT FLARES with hslToRgb oscillation - 1-in-2 ===
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color oscillatedColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, oscillatedColor, 0.32f, 14);
            }
            
            // === GLOW TRAIL with gradient ===
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.68f);
                var trail = new GenericGlowParticle(Projectile.Center + offset, -Projectile.velocity * 0.08f,
                    trailColor * 0.6f, 0.24f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === 3 ORBITING MUSIC NOTES - LOCKED TO PROJECTILE (TRUE_VFX_STANDARDS) ===
            float noteOrbitAngle = Main.GameUpdateCount * 0.1f;
            if (Projectile.timeLeft % 6 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = noteOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 14f;
                    Vector2 noteVel = noteAngle.ToRotationVector2() * 2f + Projectile.velocity * 0.3f;
                    float noteHue = HueMin + (i / 3f) * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.78f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 28);
                }
            }
            
            // === SPARKLE ACCENT - Star twinkle ===
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    -Projectile.velocity * 0.05f, NachtmusikCosmicVFX.StarWhite, 0.3f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === 4-ELEMENT CONSTELLATION ORBIT ===
            if (Projectile.timeLeft % 5 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                for (int i = 0; i < 4; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 12f;
                    float sparkHue = HueMin + (i / 4f) * (HueMax - HueMin);
                    Color sparkColor = Main.hslToRgb(sparkHue, 0.88f, 0.78f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor, 0.2f, 10);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.7f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            // === 3-LAYER FLASH CASCADE ===
            CustomParticles.GenericFlare(target.Center, Color.White, 0.55f, 18);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Gold, 0.45f, 16);
            CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Violet, 0.35f, 14);
            
            // === 5 MUSIC NOTES WITH hslToRgb GRADIENT ===
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                float noteHue = HueMin + (n / 5f) * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.88f, 0.75f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.75f, 30);
            }
            
            // === 2 HALO RINGS ===
            for (int ring = 0; ring < 2; ring++)
            {
                float ringHue = HueMin + (ring / 2f) * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(ringHue, 0.82f, 0.7f);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.1f, 14 + ring * 2);
            }
            
            // === 6 SPARKLE BURST ===
            for (int s = 0; s < 6; s++)
            {
                float angle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5.5f);
                float sparkHue = HueMin + (s / 6f) * (HueMax - HueMin);
                Color sparkColor = Main.hslToRgb(sparkHue, 0.85f, 0.75f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === DYNAMIC IMPACT: STYLE 1 - Constellation Cascade (star bolt) ===
            NachtConstellationCascade(target.Center, 0.9f);
            DramaticImpact(target.Center, NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.Violet, 0.4f, 16);
            
            hitEnemies.Add(target.whoAmI);
            
            // Chain to next target
            if (chainCount < MaxChains)
            {
                NPC nextTarget = FindNextChainTarget(target.Center, 300f);
                if (nextTarget != null)
                {
                    chainCount++;
                    Vector2 toNext = (nextTarget.Center - target.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = toNext * Projectile.velocity.Length() * 0.95f;
                    Projectile.Center = target.Center;
                    
                    // === CHAIN LIGHTNING VFX ===
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 linePos = Vector2.Lerp(target.Center, target.Center + toNext * 60f, i / 8f);
                        float lineHue = HueMin + (i / 8f) * (HueMax - HueMin);
                        Color lineColor = Main.hslToRgb(lineHue, 0.88f, 0.75f);
                        var line = new GenericGlowParticle(linePos, Vector2.Zero, lineColor * 0.8f, 0.22f, 12, true);
                        MagnumParticleHandler.SpawnParticle(line);
                    }
                }
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH STYLE 1: CONSTELLATION IMPLOSION ===
            // This chaining bolt uses constellation death - stars connected by threads
            DynamicParticleEffects.NachtDeathConstellation(Projectile.Center, 0.9f);
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 1f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 90+ lines of PNG-based rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.45f);
            return false;
        }
        
        private NPC FindNextChainTarget(Vector2 from, float range)
        {
            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (hitEnemies.Contains(i)) continue;
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(from, npc.Center);
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
    
    /// <summary>
    /// Nebula's Whisper's ethereal arrow - UNIQUE NEBULA CLOUD AESTHETIC
    /// A swirling vortex of cosmic gas with orbiting star motes that leaves 
    /// a billowing nebula trail and rains down starfall on impact.
    /// </summary>
    public class NebulaArrowProjectile : ModProjectile
    {
        // Use StarBurst texture for unique nebula core look
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst2";
        
        private int splitCount = 0;
        private float nebulaRotation = 0f;
        private float[] starMoteAngles = new float[5];
        private float[] starMoteDistances = new float[5];
        
        // Nebula color palette - distinct pinks and purples
        private static readonly Color NebulaCore = new Color(255, 120, 200);
        private static readonly Color NebulaOuter = new Color(180, 80, 220);
        private static readonly Color NebulaGas = new Color(120, 60, 180);
        private static readonly Color StarMote = new Color(255, 255, 220);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
            
            // Initialize orbiting star motes with varied distances
            for (int i = 0; i < starMoteAngles.Length; i++)
            {
                starMoteAngles[i] = MathHelper.TwoPi * i / starMoteAngles.Length;
                starMoteDistances[i] = 12f + Main.rand.NextFloat(6f);
            }
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            nebulaRotation += 0.08f;
            
            // Update orbiting star motes
            for (int i = 0; i < starMoteAngles.Length; i++)
            {
                starMoteAngles[i] += 0.12f + i * 0.02f;
                starMoteDistances[i] += (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 0.3f;
            }
            
            // BILLOWING NEBULA GAS TRAIL - multiple layers of cosmic clouds
            if (Main.rand.NextBool(2))
            {
                for (int layer = 0; layer < 3; layer++)
                {
                    float layerOffset = layer * 0.15f;
                    Color gasColor = Color.Lerp(NebulaCore, NebulaGas, layerOffset + Main.rand.NextFloat(0.3f));
                    Vector2 gasOffset = Main.rand.NextVector2Circular(6f + layer * 3f, 6f + layer * 3f);
                    Vector2 gasVel = -Projectile.velocity * (0.06f + layer * 0.02f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                    var cloud = new GenericGlowParticle(Projectile.Center + gasOffset, gasVel, gasColor * (0.4f - layer * 0.1f), 
                        0.28f - layer * 0.05f, 22 + layer * 4, true);
                    MagnumParticleHandler.SpawnParticle(cloud);
                }
            }
            
            // STAR MOTE SPARKLE TRAIL - tiny stars left behind from orbiting motes
            if (Projectile.timeLeft % 3 == 0)
            {
                int moteIndex = Projectile.timeLeft % starMoteAngles.Length;
                Vector2 motePos = Projectile.Center + starMoteAngles[moteIndex].ToRotationVector2() * starMoteDistances[moteIndex];
                var starTrail = new SparkleParticle(motePos, -Projectile.velocity * 0.05f, StarMote * 0.7f, 0.18f, 15);
                MagnumParticleHandler.SpawnParticle(starTrail);
            }
            
            // MUSIC NOTES - Nebula whisper melody floating upward
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), Main.rand.NextFloat(-1.2f, -0.5f));
                Color noteColor = Color.Lerp(NebulaCore, StarMote, Main.rand.NextFloat(0.3f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel, noteColor, 0.72f, 35);
            }
            
            // PRISMATIC SPARKLE accents
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    Vector2.Zero, Color.Lerp(NebulaCore, StarMote, Main.rand.NextFloat()), 0.32f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dynamic lighting that pulses
            float lightPulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            Lighting.AddLight(Projectile.Center, NebulaCore.ToVector3() * lightPulse);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 300);
            
            // STARFALL RAIN - Split creates falling stars that impact ground
            if (splitCount == 0 && Projectile.ai[0] == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.ToRadians(-150f + i * 30f); // Arc above
                    Vector2 splitVel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);
                    int split = Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center + new Vector2(0, -30f), splitVel,
                        ModContent.ProjectileType<NebulaStarfallProjectile>(), Projectile.damage / 2, Projectile.knockBack / 2, Projectile.owner);
                }
            }
            
            // NEBULA BURST VFX - Expanding gas cloud explosion
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(NebulaCore, NebulaGas, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor * 0.6f, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Star sparkle explosion
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(25f, 25f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), StarMote, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music chord burst
            ThemedParticles.MusicNoteBurst(target.Center, NebulaCore, 6, 4f);
            
            // Cascading halos
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = Color.Lerp(NebulaCore, NebulaOuter, i / 4f);
                CustomParticles.HaloRing(target.Center, haloColor * (0.6f - i * 0.1f), 0.3f + i * 0.15f, 15 + i * 3);
            }
            
            // === DYNAMIC IMPACT: STYLE 3 - Serenade Resonance (nebula magic) ===
            NachtSerenadeResonance(target.Center, 1.15f);
            DramaticImpact(target.Center, NebulaCore, StarMote, 0.5f, 20);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH STYLE 3: NEBULA BLOOM ===
            // Soft nebula cloud expands with embedded stars - perfect for ranged
            DynamicParticleEffects.NachtDeathNebulaBoom(Projectile.Center, 1.0f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 45+ lines of PNG-based nebula rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.5f);
            return false;
        }
    }
    
    /// <summary>
    /// Starfall projectile spawned by Nebula Arrow on impact - falls and explodes on ground
    /// </summary>
    public class NebulaStarfallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle14";
        
        private static readonly Color StarCore = new Color(255, 255, 220);
        private static readonly Color StarTrail = new Color(255, 180, 220);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            // Gravity - falls like a star
            Projectile.velocity.Y += 0.25f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            
            // Sparkling trail
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.1f, StarCore * 0.6f, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, StarCore.ToVector3() * 0.5f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH STYLE 3: NEBULA BLOOM (variant for starfall) ===
            // Starfall uses nebula bloom but with velocity-aware direction
            DynamicParticleEffects.NachtDeathNebulaBoom(Projectile.Center, 0.7f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 20+ lines of PNG-based starfall rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.2f);
            return false;
        }
    }
    
    /// <summary>
    /// Serenade of Distant Stars' homing star projectile.
    /// TRUE_VFX_STANDARDS: 6-layer spinning flares, dense dust, orbiting music notes, hslToRgb oscillation
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle13";
        
        // TRUE_VFX_STANDARDS: Hue range for Nachtmusik violet/purple spectrum
        private const float HueMin = 0.75f;
        private const float HueMax = 0.85f;
        
        // Serenade star colors - starlight theme
        private static readonly Color StarCore = new Color(255, 255, 220);
        private static readonly Color StarGold = new Color(255, 230, 160);
        private static readonly Color StarViolet = new Color(180, 140, 255);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.15f;
            
            // Strong homing after initial delay
            if (Projectile.timeLeft < 270)
            {
                NPC target = FindClosestEnemy(800f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.08f);
                }
            }
            
            // ===== TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) =====
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1.2f, 1.2f);
                
                Dust main = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, StarViolet, 1.4f);
                main.noGravity = true;
                main.fadeIn = 1.2f;
            }
            
            // Contrasting sparkle dust (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Dust contrast = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, 
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, StarGold, 1.1f);
                contrast.noGravity = true;
            }
            
            // ===== TRUE_VFX_STANDARDS: COLOR OSCILLATION with hslToRgb =====
            if (Main.rand.NextBool(3))
            {
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color oscillatingColor = Main.hslToRgb(hue, 0.85f, 0.75f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(flarePos, oscillatingColor, 0.35f, 14);
            }
            
            // ===== TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES LOCKED TO PROJECTILE =====
            float orbitAngle = Main.GameUpdateCount * 0.1f;
            if (Main.rand.NextBool(6))
            {
                for (int n = 0; n < 3; n++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * n / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * 14f;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.85f + noteAngle.ToRotationVector2() * 0.4f;
                    
                    float noteHue = HueMin + ((n / 3f) * (HueMax - HueMin));
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.8f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.78f, 32);
                    
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.3f, StarCore * 0.6f, 0.22f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // ===== ORBITING STAR MOTES - 4-point constellation =====
            float moteAngle = Main.GameUpdateCount * 0.07f;
            float moteRadius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 4f;
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int m = 0; m < 4; m++)
                {
                    float angle = moteAngle + MathHelper.TwoPi * m / 4f;
                    Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * moteRadius;
                    float moteHue = HueMin + ((m / 4f + Main.GameUpdateCount * 0.005f) % 1f) * (HueMax - HueMin);
                    Color moteColor = Main.hslToRgb(moteHue, 0.9f, 0.85f);
                    CustomParticles.GenericFlare(motePos, moteColor, 0.2f, 10);
                }
            }
            
            // Glow particles for trailing
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    -Projectile.velocity * 0.1f, trailColor * 0.65f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sparkle accents
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 
                    Main.rand.NextVector2Circular(1.2f, 1.2f), StarCore, 0.32f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, StarCore.ToVector3() * 0.8f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            
            // ===== TRUE_VFX_STANDARDS: 3-LAYER FLASH CASCADE =====
            CustomParticles.GenericFlare(target.Center, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(target.Center, StarCore, 0.8f, 20);
            CustomParticles.GenericFlare(target.Center, StarViolet, 0.6f, 18);
            
            // ===== MUSIC NOTES WITH GRADIENT =====
            for (int i = 0; i < 5; i++)
            {
                float noteProgress = i / 5f;
                float noteHue = HueMin + noteProgress * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.8f);
                float noteAngle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2.5f, 4.5f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.8f, 30);
            }
            
            // ===== HALO RINGS (2 layers) =====
            CustomParticles.HaloRing(target.Center, StarCore * 0.7f, 0.4f, 16);
            CustomParticles.HaloRing(target.Center, StarViolet * 0.5f, 0.3f, 14);
            
            // ===== SPARKLE BURST (6 radial) =====
            for (int s = 0; s < 6; s++)
            {
                float sparkleAngle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkleVel = sparkleAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 5.5f);
                var sparkle = new SparkleParticle(target.Center, sparkleVel, StarCore, 0.38f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === DYNAMIC IMPACT: STYLE 1 - Constellation Cascade (serenade star) ===
            NachtConstellationCascade(target.Center, 1f);
            DramaticImpact(target.Center, StarCore, StarViolet, 0.48f, 20);
            
            // === SEEKING CRYSTALS - Star burst ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Serenade Spiral - Musical notes spiral outward elegantly (fits "serenade" theme)
            DynamicParticleEffects.NachtDeathSerenadeSpiral(Projectile.Center, 1.0f);
            Lighting.AddLight(Projectile.Center, StarCore.ToVector3() * 1.2f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 60+ lines of PNG-based serenade star rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.45f);
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
    
    #endregion
    
    #region Magic Projectiles
    
    /// <summary>
    /// Starweaver's Grimoire's cosmic orb projectile.
    /// TRUE_VFX_STANDARDS: 6-layer spinning flares, dense dust, orbiting music notes, hslToRgb oscillation
    /// Creates mini-explosions along its path.
    /// </summary>
    public class StarweaverOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField8";
        
        // TRUE_VFX_STANDARDS: Hue range for Nachtmusik violet/purple spectrum
        private const float HueMin = 0.75f;
        private const float HueMax = 0.85f;
        
        // Starweaver orb colors
        private static readonly Color OrbCore = new Color(255, 255, 230);
        private static readonly Color OrbViolet = new Color(160, 100, 220);
        private static readonly Color OrbGold = new Color(255, 220, 140);
        
        private int explosionTimer = 0;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.08f;
            explosionTimer++;
            
            // Slow homing
            NPC target = FindClosestEnemy(600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.04f);
            }
            
            // ===== TRUE_VFX_STANDARDS: DENSE DUST TRAIL (2+ per frame guaranteed) =====
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 dustVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                Dust main = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, OrbViolet, 1.5f);
                main.noGravity = true;
                main.fadeIn = 1.3f;
            }
            
            // Contrasting sparkle dust (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Dust contrast = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, 
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f), 0, OrbGold, 1.2f);
                contrast.noGravity = true;
            }
            
            // ===== TRUE_VFX_STANDARDS: COLOR OSCILLATION with hslToRgb =====
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + (Main.rand.NextFloat() * (HueMax - HueMin));
                Color oscillatingColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                Vector2 flarePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                CustomParticles.GenericFlare(flarePos, oscillatingColor, 0.38f, 14);
            }
            
            // ===== TRUE_VFX_STANDARDS: ORBITING MUSIC NOTES LOCKED TO PROJECTILE =====
            float orbitAngle = Main.GameUpdateCount * 0.09f;
            if (Main.rand.NextBool(6))
            {
                for (int n = 0; n < 3; n++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * n / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * 16f;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    Vector2 noteVel = Projectile.velocity * 0.8f + noteAngle.ToRotationVector2() * 0.5f;
                    
                    float noteHue = HueMin + ((n / 3f) * (HueMax - HueMin));
                    Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.8f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.8f, 34);
                    
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.3f, OrbCore * 0.6f, 0.24f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // ===== ORBITING MAGIC MOTES - 5-point arcane orbit =====
            float moteAngle = Main.GameUpdateCount * 0.07f;
            float moteRadius = 15f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 5f;
            if (Projectile.timeLeft % 4 == 0)
            {
                for (int m = 0; m < 5; m++)
                {
                    float angle = moteAngle + MathHelper.TwoPi * m / 5f;
                    Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * moteRadius;
                    float moteHue = HueMin + ((m / 5f + Main.GameUpdateCount * 0.004f) % 1f) * (HueMax - HueMin);
                    Color moteColor = Main.hslToRgb(moteHue, 0.85f, 0.8f);
                    CustomParticles.GenericFlare(motePos, moteColor, 0.22f, 12);
                }
            }
            
            // Periodic mini-explosions (enhanced)
            if (explosionTimer % 20 == 0)
            {
                // Mini star burst with gradient
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * 22f;
                    float burstHue = HueMin + (i / 8f) * (HueMax - HueMin);
                    Color burstColor = Main.hslToRgb(burstHue, 0.9f, 0.75f);
                    CustomParticles.GenericFlare(burstPos, burstColor, 0.35f, 14);
                }
                
                // Halo ring on explosion
                CustomParticles.HaloRing(Projectile.Center, OrbViolet * 0.6f, 0.35f, 12);
                
                // Damage enemies in radius
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < 80f)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage / 3, 0, false, 0f, null, false, 0, false);
                    }
                }
            }
            
            // Glow particle trail
            if (Main.rand.NextBool(2))
            {
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color trailColor = Main.hslToRgb(hue, 0.85f, 0.7f);
                var glow = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(1.2f, 1.2f), trailColor * 0.6f, 0.24f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Sparkle accents
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    Main.rand.NextVector2Circular(1.5f, 1.5f), OrbGold, 0.28f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, OrbViolet.ToVector3() * 0.9f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 420);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 2);
            
            // ===== TRUE_VFX_STANDARDS: 3-LAYER FLASH CASCADE =====
            CustomParticles.GenericFlare(target.Center, Color.White, 1.1f, 22);
            CustomParticles.GenericFlare(target.Center, OrbCore, 0.85f, 20);
            CustomParticles.GenericFlare(target.Center, OrbViolet, 0.65f, 18);
            
            // ===== MUSIC NOTES WITH GRADIENT =====
            for (int i = 0; i < 6; i++)
            {
                float noteProgress = i / 6f;
                float noteHue = HueMin + noteProgress * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(noteHue, 0.9f, 0.8f);
                float noteAngle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2.8f, 4.8f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.82f, 32);
            }
            
            // ===== HALO RINGS (2 layers) =====
            CustomParticles.HaloRing(target.Center, OrbCore * 0.7f, 0.45f, 18);
            CustomParticles.HaloRing(target.Center, OrbViolet * 0.55f, 0.35f, 15);
            
            // ===== SPARKLE BURST (8 radial) =====
            for (int s = 0; s < 8; s++)
            {
                float sparkleAngle = MathHelper.TwoPi * s / 8f;
                Vector2 sparkleVel = sparkleAngle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 6f);
                var sparkle = new SparkleParticle(target.Center, sparkleVel, OrbCore, 0.4f, 24);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === DYNAMIC IMPACT: STYLE 3 - Serenade Resonance (starweaver orb) ===
            NachtSerenadeResonance(target.Center, 1.2f);
            DramaticImpact(target.Center, OrbCore, OrbViolet, 0.55f, 22);
        }
        
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
            
            // UNIQUE: Cosmic Nova - Blinding flash with radial light rays (powerful orb deserves dramatic explosion)
            DynamicParticleEffects.NachtDeathCosmicNova(Projectile.Center, 1.2f);
            Lighting.AddLight(Projectile.Center, OrbCore.ToVector3() * 1.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 65+ lines of PNG-based starweaver orb rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.55f);
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
    
    /// <summary>
    /// Requiem of the Cosmos' cosmic beam that pierces everything.
    /// </summary>
    /// Requiem of the Cosmos - UNIQUE COSMIC BEAM AESTHETIC
    /// A flowing river of cosmic energy with orbiting galaxy motes, 
    /// constellation trail connections, and reality-warping visual effects.
    /// </summary>
    public class CosmicRequiemBeamProjectile : ModProjectile
    {
        // Use unique textures for cosmic beam look
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField7";
        
        private float cosmicRotation = 0f;
        private float[] galaxyMoteAngles = new float[4];
        private float warpPulse = 0f;
        
        // Cosmic Requiem color palette - deep space
        private static readonly Color CosmicCore = new Color(255, 255, 255);
        private static readonly Color CosmicMid = new Color(160, 120, 255);
        private static readonly Color CosmicOuter = new Color(80, 40, 160);
        private static readonly Color CosmicVoid = new Color(20, 10, 40);
        private static readonly Color GalaxyMote = new Color(255, 200, 255);
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.extraUpdates = 2;
            
            // Initialize galaxy mote orbit angles
            for (int i = 0; i < galaxyMoteAngles.Length; i++)
            {
                galaxyMoteAngles[i] = MathHelper.TwoPi * i / galaxyMoteAngles.Length;
            }
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            cosmicRotation += 0.15f;
            warpPulse += 0.2f;
            
            // Update orbiting galaxy motes
            for (int i = 0; i < galaxyMoteAngles.Length; i++)
            {
                galaxyMoteAngles[i] += 0.18f + i * 0.03f;
            }
            
            // COSMIC RIVER TRAIL - Flowing energy particles
            for (int layer = 0; layer < 2; layer++)
            {
                if (Main.rand.NextBool(2 - layer))
                {
                    float offset = (layer == 0) ? 0f : Main.rand.NextFloat(-4f, 4f);
                    Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * offset;
                    Color streamColor = Color.Lerp(CosmicMid, CosmicOuter, layer * 0.5f + Main.rand.NextFloat(0.2f));
                    Vector2 streamVel = -Projectile.velocity * (0.03f + layer * 0.02f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                    var stream = new GenericGlowParticle(Projectile.Center + perpendicular, streamVel, streamColor * (0.5f - layer * 0.15f), 
                        0.25f - layer * 0.06f, 16 + layer * 4, true);
                    MagnumParticleHandler.SpawnParticle(stream);
                }
            }
            
            // GALAXY MOTE SPARKLE TRAIL - Tiny galaxies shed from orbiting motes
            if (Projectile.timeLeft % 2 == 0)
            {
                int moteIndex = (Projectile.timeLeft / 2) % galaxyMoteAngles.Length;
                float moteRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + moteIndex) * 3f;
                Vector2 motePos = Projectile.Center + galaxyMoteAngles[moteIndex].ToRotationVector2() * moteRadius;
                var galaxyTrail = new SparkleParticle(motePos, -Projectile.velocity * 0.04f, GalaxyMote * 0.6f, 0.15f, 12);
                MagnumParticleHandler.SpawnParticle(galaxyTrail);
            }
            
            // GLYPHS - Ancient cosmic runes occasionally appear
            if (Main.rand.NextBool(12))
            {
                Vector2 glyphPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                CustomParticles.Glyph(glyphPos, CosmicMid * 0.7f, 0.35f, Main.rand.Next(1, 13));
            }
            
            // MUSIC NOTES - The requiem's eternal melody
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * -1.2f;
                Color noteColor = Color.Lerp(CosmicMid, GalaxyMote, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor, 0.75f, 28);
            }
            
            // PRISMATIC SPARKLES - Cosmic shimmer
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), 
                    -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(1.5f, 1.5f), CosmicCore * 0.8f, 0.28f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Pulsing cosmic light
            float lightIntensity = 0.8f + (float)Math.Sin(warpPulse) * 0.2f;
            Lighting.AddLight(Projectile.Center, CosmicMid.ToVector3() * lightIntensity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 3);
            
            // COSMIC IMPACT - Reality ripple effect
            CustomParticles.GenericFlare(target.Center, CosmicCore, 0.9f, 18);
            CustomParticles.GenericFlare(target.Center, CosmicMid, 0.7f, 16);
            
            // Glyph burst on impact
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = target.Center + angle.ToRotationVector2() * 25f;
                CustomParticles.Glyph(glyphPos, CosmicMid * 0.6f, 0.4f, Main.rand.Next(1, 13));
            }
            
            // Cascading halos
            for (int i = 0; i < 3; i++)
            {
                Color haloColor = Color.Lerp(CosmicCore, CosmicOuter, i / 3f);
                CustomParticles.HaloRing(target.Center, haloColor * (0.6f - i * 0.15f), 0.25f + i * 0.12f, 12 + i * 2);
            }
            
            // Music chord impact
            ThemedParticles.MusicNoteBurst(target.Center, CosmicMid, 4, 3f);
            
            // === DYNAMIC IMPACT: STYLE 2 - Crescent Wave (cosmic requiem beam) ===
            NachtCrescentWave(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 0.95f);
            DramaticImpact(target.Center, CosmicCore, CosmicMid, 0.45f, 16);
        }
        
        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Cosmic Nova - Dramatic beam dissipation with radial light rays (cosmic beam finale)
            DynamicParticleEffects.NachtDeathCosmicNova(Projectile.Center, 0.9f);
            Lighting.AddLight(Projectile.Center, CosmicCore.ToVector3() * 1.3f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 60+ lines of PNG-based cosmic beam rendering
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, 0.6f);
            return false;
        }
    }
    
    /// <summary>
    /// Twilight Severance's fast dimension-cutting slash projectile.
    /// Ultra-fast, short-range, high damage slashes.
    /// </summary>
    public class TwilightSlashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc3";
        
        private bool isDimensionSever => Projectile.ai[0] == 1f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 40; // Short-lived fast slash
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 2;
        }
        
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Faster for dimension sever
            if (isDimensionSever)
            {
                Projectile.velocity *= 1.02f;
                
                // More intense trail
                if (Main.rand.NextBool(2))
                {
                    Color trailColor = Color.Lerp(NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.StarWhite, Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                        trailColor, 0.4f, 15, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // 笘・MUSICAL NOTATION - Dimension sever crescendo (VISIBLE SCALE 0.78f+)
                if (Main.rand.NextBool(4))
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), -1.3f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Gold, 0.78f, 25);
                }
                
                // 笘・SPARKLE ACCENT - Dimension shimmer
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.1f, NachtmusikCosmicVFX.StarWhite, 0.4f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            else
            {
                // Normal slash trail
                if (Main.rand.NextBool(3))
                {
                    Color trailColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                    var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.05f,
                        trailColor * 0.7f, 0.25f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // 笘・MUSICAL NOTATION - Twilight melody (VISIBLE SCALE 0.7f+)
                if (Main.rand.NextBool(6))
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                    ThemedParticles.MusicNote(Projectile.Center, noteVel, NachtmusikCosmicVFX.Violet, 0.7f, 28);
                }
                
                // 笘・SPARKLE ACCENT - Twilight gleam
                if (Main.rand.NextBool(5))
                {
                    var sparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, NachtmusikCosmicVFX.Gold, 0.25f, 15);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            Lighting.AddLight(Projectile.Center, NachtmusikCosmicVFX.Violet.ToVector3() * (isDimensionSever ? 0.6f : 0.35f));
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Celestial Harmony
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 240);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
            {
                harmonyNPC.AddStack(target, isDimensionSever ? 2 : 1);
            }
            
            // Hit VFX
            if (isDimensionSever)
            {
                NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, 0.8f);
                // Starburst 
                var hitBurst = new StarBurstParticle(target.Center, Vector2.Zero, NachtmusikCosmicVFX.Gold, 0.35f, 12);
                MagnumParticleHandler.SpawnParticle(hitBurst);
                
                // 笘・MUSICAL IMPACT - Dimension sever chord
                ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Gold, 5, 3.5f);
                
                // === DYNAMIC IMPACT: STYLE 2 - Crescent Wave (Dimension Sever) ===
                NachtCrescentWave(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 1.2f);
                DramaticImpact(target.Center, NachtmusikCosmicVFX.Gold, NachtmusikCosmicVFX.Violet, 0.55f, 20);
            }
            else
            {
                CustomParticles.GenericFlare(target.Center, NachtmusikCosmicVFX.Violet, 0.4f, 10);
                
                // 笘・MUSICAL IMPACT - Twilight chord
                ThemedParticles.MusicNoteBurst(target.Center, NachtmusikCosmicVFX.Violet, 3, 2.5f);
                
                // === DYNAMIC IMPACT: STYLE 2 - Crescent Wave (Normal slash) ===
                NachtCrescentWave(target.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), 0.8f);
                DramaticImpact(target.Center, NachtmusikCosmicVFX.Violet, NachtmusikCosmicVFX.Gold, 0.35f, 15);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // UNIQUE: Twilight Fade - Elegant dissolve into rising motes (melee slash fades gracefully)
            DynamicParticleEffects.NachtDeathTwilightFade(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX), isDimensionSever ? 1.2f : 0.8f);
            Lighting.AddLight(Projectile.Center, (isDimensionSever ? NachtmusikCosmicVFX.Gold : NachtmusikCosmicVFX.Violet).ToVector3() * 0.8f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Use procedural VFX system - replaces 25+ lines of PNG-based slash rendering
            float scale = isDimensionSever ? 0.55f : 0.4f;
            ProceduralProjectileVFX.DrawNachtmusikProjectile(Main.spriteBatch, Projectile, scale);
            return false;
        }
    }
    
    #endregion
}
