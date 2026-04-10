using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Graphics;
using ReLogic.Content;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

// Dynamic particle effects for aesthetically pleasing animations
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Summer.Projectiles
{
    /// <summary>
    /// Solar Orb Projectile - Rapid fire orb from Solstice Tome
    /// TRUE_VFX_STANDARDS: Layered spinning flares, dense dust, orbiting music notes, hslToRgb color oscillation
    /// </summary>
    public class SolarOrbProjectile : ModProjectile
    {
        // Summer color palette - warm hues (0.08-0.14 on hue wheel for orange/gold)
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 80, 40);
        
        // Hue range for color oscillation (orange-gold range)
        private const float HueMin = 0.08f;
        private const float HueMax = 0.14f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.4f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        private float orbitTimer = 0f;

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            orbitTimer += 0.12f;

            // ══════════════════════════════════════════════════════════════╁E
            // DENSE DUST TRAIL - 2+ particles per frame! (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                
                // COLOR OSCILLATION with Main.hslToRgb
                float hue = HueMin + ((Main.GameUpdateCount * 0.025f + i * 0.15f) % 1f) * (HueMax - HueMin);
                Color oscillatingColor = Main.hslToRgb(hue, 1f, 0.65f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.SolarFlare, dustVel, 0, oscillatingColor, 1.6f);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }

            // ══════════════════════════════════════════════════════════════╁E
            // CONTRASTING SPARKLES - 1-in-2 for visual pop
            // ══════════════════════════════════════════════════════════════╁E
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust contrast = Dust.NewDustPerfect(sparklePos, DustID.WhiteTorch, 
                    -Projectile.velocity * 0.12f, 0, SunWhite, 1.3f);
                contrast.noGravity = true;
            }

            // ══════════════════════════════════════════════════════════════╁E
            // FREQUENT FLARES littering the air - 1-in-2
            // ══════════════════════════════════════════════════════════════╁E
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(10f, 10f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.45f, 16);
            }

            // ══════════════════════════════════════════════════════════════╁E
            // SOLAR CORONA ORBIT - Enhanced with more particles
            // ══════════════════════════════════════════════════════════════╁E
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int c = 0; c < 5; c++)
                {
                    float coronaAngle = orbitTimer + MathHelper.TwoPi * c / 5f;
                    float coronaRadius = 14f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + c) * 4f;
                    Vector2 coronaPos = Projectile.Center + coronaAngle.ToRotationVector2() * coronaRadius;
                    Vector2 coronaVel = coronaAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 1f;
                    
                    float hue = HueMin + (float)c / 5f * (HueMax - HueMin);
                    Color coronaColor = Main.hslToRgb(hue, 1f, 0.7f) * 0.7f;
                    
                    var corona = new GenericGlowParticle(coronaPos, coronaVel, coronaColor, 0.25f, 14, true);
                    MagnumParticleHandler.SpawnParticle(corona);
                }
            }

            // ══════════════════════════════════════════════════════════════╁E
            // ORBITING MUSIC NOTES - Locked to projectile! (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            float musicOrbitAngle = Main.GameUpdateCount * 0.1f;
            float musicOrbitRadius = 16f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 5f;
            
            if (Main.rand.NextBool(5))
            {
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    
                    // Note velocity matches projectile + slight outward drift
                    Vector2 noteVel = Projectile.velocity * 0.65f + noteAngle.ToRotationVector2() * 0.4f;
                    
                    // VISIBLE SCALE (0.8f+) with shimmer
                    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.18f + i) * 0.12f;
                    float hue = HueMin + (float)i / 3f * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 1f, 0.75f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.8f * shimmer, 38);
                    
                    // Sparkle companion for bloom effect
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.4f, SunWhite * 0.6f, 0.28f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // ══════════════════════════════════════════════════════════════╁E
            // RADIANT PULSE RINGS - Enhanced
            // ══════════════════════════════════════════════════════════════╁E
            if (Main.GameUpdateCount % 12 == 0)
            {
                for (int r = 0; r < 8; r++)
                {
                    float ringAngle = MathHelper.TwoPi * r / 8f;
                    Vector2 ringVel = ringAngle.ToRotationVector2() * 4f;
                    float hue = HueMin + (float)r / 8f * (HueMax - HueMin);
                    Color ringColor = Main.hslToRgb(hue, 1f, 0.7f) * 0.6f;
                    var ring = new GenericGlowParticle(Projectile.Center, ringVel, ringColor, 0.22f, 18, true);
                    MagnumParticleHandler.SpawnParticle(ring);
                }
            }

            // === DYNAMIC PARTICLE EFFECTS - Pulsing Solar Core ===
            // Creates a living, fiery glow at the projectile center
            if (Main.GameUpdateCount % 5 == 0)
            {
                PulsingGlow(Projectile.Center, Vector2.Zero, SunGold, SunOrange, 0.32f, 22, 0.18f, 0.3f);
            }

            // === DYNAMIC: Rainbow hue burst for prismatic heat effect ===
            if (Main.rand.NextBool(8))
            {
                RainbowBurst(Projectile.Center, 4, 3f, 0.2f, 18);
            }

            // Glow particle trail
            if (Main.rand.NextBool(3))
            {
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.75f;
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.12f, 
                    trailColor, 0.32f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // BRIGHT lighting (1.0f+ intensity)
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f + 0.9f;
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ══════════════════════════════════════════════════════════════╁E
            // GLIMMER IMPACT - Layered cascade (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            
            // Central flash cascade
            CustomParticles.GenericFlare(target.Center, Color.White, 0.8f, 22);
            CustomParticles.GenericFlare(target.Center, SunGold, 0.65f, 20);
            CustomParticles.GenericFlare(target.Center, SunOrange, 0.5f, 18);
            
            // Music notes BURST outward with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + (float)i / 8f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 1f, 0.8f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.85f, 38);
            }
            
            // Halo ring expansion
            CustomParticles.HaloRing(target.Center, SunGold * 0.65f, 0.5f, 18);
            CustomParticles.HaloRing(target.Center, SunOrange * 0.5f, 0.35f, 16);
            
            // === DYNAMIC IMPACT: Summer Theme Solar Burst ===
            SummerImpact(target.Center, 1.1f);
            
            // === DYNAMIC: Dramatic flare for fiery impact ===
            DramaticImpact(target.Center, SunWhite, SunOrange, 0.65f, 24);
            
            // Sparkle burst ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, SunWhite * 0.85f, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust burst for density
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.SolarFlare, burstVel, 0, SunOrange, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            target.AddBuff(BuffID.OnFire3, 90);
            
            // Bright impact lighting
            Lighting.AddLight(target.Center, SunGold.ToVector3() * 1.3f);
        }

        public override void OnKill(int timeLeft)
        {
            // ══════════════════════════════════════════════════════════════╁E
            // GLIMMER CASCADE - NOT a puff! (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            
            // Central glimmer - multiple layered spinning flares
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = 0.35f + layer * 0.15f;
                float layerAlpha = 0.85f - layer * 0.15f;
                Color layerColor = Color.Lerp(SunWhite, SunOrange, layer / 4f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 20 - layer * 2);
            }
            
            // Expanding glow rings
            for (int ring = 0; ring < 4; ring++)
            {
                float hue = HueMin + (float)ring / 4f * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.7f, 0.35f + ring * 0.12f, 16 + ring * 3);
            }
            
            // Music note finale - burst outward with gradient
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                float hue = HueMin + (float)i / 10f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 1f, 0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.85f, 40);
            }
            
            // Radial sparkle burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, 
                    Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()), 0.4f, 24);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Dust explosion for density
            for (int i = 0; i < 14; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, dustVel, 0, SunOrange, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.1f;
            }
            
            // Bright lighting flash
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * 1.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            // Use procedural VFX system for Summer theme with solar corona effects
            ProceduralProjectileVFX.DrawSummerProjectile(Main.spriteBatch, Projectile, 1f);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }

    /// <summary>
    /// Sunbeam Projectile - Charged beam from Solstice Tome
    /// TRUE_VFX_STANDARDS: Layered spinning flares, dense dust, orbiting music notes, hslToRgb color oscillation
    /// </summary>
    public class SunbeamProjectile : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);
        
        // Hue range for color oscillation (warm orange-red range)
        private const float HueMin = 0.06f;
        private const float HueMax = 0.12f;

        // --- InfernalBeamFoundation scaffolding: shader + texture caching ---
        private static Effect _sunbeamShader;
        private static Asset<Texture2D> _sunbeamAlphaMask;
        private static Asset<Texture2D> _sunbeamGradientLUT;
        private static Asset<Texture2D> _sunbeamBodyTex;
        private static Asset<Texture2D> _sunbeamDetail1;
        private static Asset<Texture2D> _sunbeamDetail2;
        private static Asset<Texture2D> _sunbeamNoise;
        private static Asset<Texture2D> _sunbeamSoftGlow;
        private static Asset<Texture2D> _sunbeamPointBloom;
        private VertexStrip _sunbeamStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 10;
            Projectile.timeLeft = 90;
            Projectile.light = 0.9f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 2;
        }

        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // ══════════════════════════════════════════════════════════════╁E
            // DENSE DUST TRAIL - 3 particles per frame! (intense beam)
            // ══════════════════════════════════════════════════════════════╁E
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 dustVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(2.5f, 2.5f);
                
                // COLOR OSCILLATION with Main.hslToRgb
                float hue = HueMin + ((Main.GameUpdateCount * 0.03f + i * 0.12f) % 1f) * (HueMax - HueMin);
                Color oscillatingColor = Main.hslToRgb(hue, 1f, 0.7f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.SolarFlare, dustVel, 0, oscillatingColor, 1.7f);
                dust.noGravity = true;
                dust.fadeIn = 1.4f;
            }

            // ══════════════════════════════════════════════════════════════╁E
            // CONTRASTING SPARKLES - 1-in-2 for visual pop
            // ══════════════════════════════════════════════════════════════╁E
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Dust contrast = Dust.NewDustPerfect(sparklePos, DustID.WhiteTorch, 
                    -Projectile.velocity * 0.1f, 0, SunWhite, 1.5f);
                contrast.noGravity = true;
            }

            // ══════════════════════════════════════════════════════════════╁E
            // FREQUENT FLARES littering the air - 1-in-2
            // ══════════════════════════════════════════════════════════════╁E
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(14f, 14f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.5f, 16);
            }

            // Glow particles for trail
            Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
            Vector2 trailVel = -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(2f, 2f);
            Color trailColor = Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()) * 0.75f;
            var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.45f, 22, true);
            MagnumParticleHandler.SpawnParticle(trail);

            // ══════════════════════════════════════════════════════════════╁E
            // SIDE SPARKS - Enhanced with oscillating colors
            // ══════════════════════════════════════════════════════════════╁E
            if (Projectile.timeLeft % 2 == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 perpDir = Projectile.velocity.RotatedBy(MathHelper.PiOver2 * side).SafeNormalize(Vector2.Zero);
                    Vector2 sparkPos = Projectile.Center + perpDir * 14f;
                    float hue = HueMin + ((Main.GameUpdateCount * 0.04f + side * 0.3f) % 1f) * (HueMax - HueMin);
                    Color sparkColor = Main.hslToRgb(hue, 1f, 0.7f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor * 0.65f, 0.28f, 10);
                }
            }

            // ══════════════════════════════════════════════════════════════╁E
            // ORBITING MUSIC NOTES - Locked to projectile! (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            float musicOrbitAngle = Main.GameUpdateCount * 0.12f;
            float musicOrbitRadius = 20f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 6f;
            
            if (Main.rand.NextBool(4))
            {
                for (int i = 0; i < 4; i++)
                {
                    float noteAngle = musicOrbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * musicOrbitRadius;
                    Vector2 notePos = Projectile.Center + noteOffset;
                    
                    // Note velocity matches projectile + slight outward drift
                    Vector2 noteVel = Projectile.velocity * 0.5f + noteAngle.ToRotationVector2() * 0.6f;
                    
                    // VISIBLE SCALE (0.85f+) with shimmer
                    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f + i) * 0.15f;
                    float hue = HueMin + (float)i / 4f * (HueMax - HueMin);
                    Color noteColor = Main.hslToRgb(hue, 1f, 0.8f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.85f * shimmer, 42);
                    
                    // Sparkle companion for bloom effect
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.35f, SunWhite * 0.65f, 0.32f, 20);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // BRIGHT lighting (intense beam)
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.25f + 1.1f;
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ══════════════════════════════════════════════════════════════╁E
            // GLIMMER IMPACT - Layered cascade (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            
            // Central flash cascade
            CustomParticles.GenericFlare(target.Center, Color.White, 0.9f, 24);
            CustomParticles.GenericFlare(target.Center, SunGold, 0.7f, 22);
            CustomParticles.GenericFlare(target.Center, SunOrange, 0.55f, 20);
            
            // Music notes BURST outward with gradient
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                float hue = HueMin + (float)i / 10f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 1f, 0.85f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.9f, 42);
            }
            
            // Halo ring expansion
            CustomParticles.HaloRing(target.Center, SunGold * 0.7f, 0.55f, 20);
            CustomParticles.HaloRing(target.Center, SunOrange * 0.55f, 0.4f, 18);
            
            // Solar ray burst - 8-point star
            for (int ray = 0; ray < 8; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 8f;
                Vector2 rayPos = target.Center + rayAngle.ToRotationVector2() * 20f;
                float hue = HueMin + (float)ray / 8f * (HueMax - HueMin);
                Color rayColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.GenericFlare(rayPos, rayColor * 0.7f, 0.25f, 12);
            }
            
            // Sparkle burst ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, SunWhite * 0.85f, 0.38f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Dust burst for density
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                Color sparkColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat());
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor * 0.75f, 0.36f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Heavy burning debuffs
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 180);
            
            // Bright impact lighting
            Lighting.AddLight(target.Center, SunGold.ToVector3() * 1.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // ══════════════════════════════════════════════════════════════╁E
            // GLIMMER CASCADE - NOT a puff! (TRUE_VFX_STANDARDS)
            // ══════════════════════════════════════════════════════════════╁E
            
            // Central glimmer - multiple layered spinning flares
            for (int layer = 0; layer < 5; layer++)
            {
                float layerScale = 0.4f + layer * 0.18f;
                float layerAlpha = 0.9f - layer * 0.15f;
                Color layerColor = Color.Lerp(SunWhite, SunOrange, layer / 5f);
                CustomParticles.GenericFlare(Projectile.Center, layerColor * layerAlpha, layerScale, 22 - layer * 2);
            }
            
            // Expanding glow rings
            for (int ring = 0; ring < 5; ring++)
            {
                float hue = HueMin + (float)ring / 5f * (HueMax - HueMin);
                Color ringColor = Main.hslToRgb(hue, 1f, 0.75f);
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.75f, 0.4f + ring * 0.14f, 18 + ring * 3);
            }
            
            // Music note finale - burst outward with gradient
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                float hue = HueMin + (float)i / 12f * (HueMax - HueMin);
                Color noteColor = Main.hslToRgb(hue, 1f, 0.85f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.9f, 45);
            }
            
            // Solar ray burst - 8-point star pattern
            for (int ray = 0; ray < 8; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 8f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 22f;
                float hue = HueMin + (float)ray / 8f * (HueMax - HueMin);
                Color rayColor = Main.hslToRgb(hue, 1f, 0.8f);
                CustomParticles.GenericFlare(rayPos, rayColor * 0.8f, 0.3f, 14);
            }
            
            // Radial sparkle burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                var sparkle = new SparkleParticle(Projectile.Center, sparkleVel, 
                    Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()), 0.42f, 26);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Radial glow burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 11f);
                Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 16f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.75f, 0.4f, 26, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Dust explosion for density
            for (int i = 0; i < 16; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, dustVel, 0, SunOrange, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
            
            // Bright lighting flash
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * 1.8f);
        }

        private void LoadSunbeamTextures()
        {
            const string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
            const string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
            const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
            const string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
            const string Gradients = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

            _sunbeamAlphaMask ??= ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);
            _sunbeamGradientLUT ??= ModContent.Request<Texture2D>(Gradients + "OdeToJoyGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _sunbeamBodyTex ??= ModContent.Request<Texture2D>(Beams + "SoundWaveBeam", AssetRequestMode.ImmediateLoad);
            _sunbeamDetail1 ??= ModContent.Request<Texture2D>(Beams + "EnergyMotion", AssetRequestMode.ImmediateLoad);
            _sunbeamDetail2 ??= ModContent.Request<Texture2D>(Beams + "EnergySurgeBeam", AssetRequestMode.ImmediateLoad);
            _sunbeamNoise ??= ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);
            _sunbeamSoftGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _sunbeamPointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            LoadSunbeamTextures();

            // Build VertexStrip from trail cache (oldPos[0] = newest/head)
            int count = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }

            sb.End(); // End current SpriteBatch for raw vertex drawing

            // === LAYER 1: Shader-driven beam body via VertexStrip (InfernalBeamFoundation) ===
            if (count >= 2)
            {
                Vector2[] positions = new Vector2[count];
                float[] rotations = new float[count];
                float totalLength = 0f;

                for (int i = 0; i < count; i++)
                {
                    positions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    rotations[i] = Projectile.oldRot[i];
                    if (i > 0) totalLength += Vector2.Distance(positions[i - 1], positions[i]);
                }

                _sunbeamStrip ??= new VertexStrip();
                _sunbeamStrip.PrepareStrip(positions, rotations,
                    (float progress) => Color.White * (1f - progress * 0.75f),
                    (float progress) => MathHelper.Lerp(36f, 4f, progress),
                    -Main.screenPosition, includeBacksides: true);

                _sunbeamShader ??= ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/InfernalBeamFoundation/Shaders/InfernalBeamBodyShader",
                    AssetRequestMode.ImmediateLoad).Value;

                if (_sunbeamShader != null)
                {
                    float repVal = MathHelper.Max(totalLength / 600f, 0.3f);
                    float time = (float)Main.timeForVisualEffects * -0.03f;

                    _sunbeamShader.Parameters["WorldViewProjection"].SetValue(
                        Main.GameViewMatrix.NormalizedTransformationmatrix);
                    _sunbeamShader.Parameters["onTex"].SetValue(_sunbeamAlphaMask.Value);
                    _sunbeamShader.Parameters["gradientTex"].SetValue(_sunbeamGradientLUT.Value);
                    _sunbeamShader.Parameters["bodyTex"].SetValue(_sunbeamBodyTex.Value);
                    _sunbeamShader.Parameters["detailTex1"].SetValue(_sunbeamDetail1.Value);
                    _sunbeamShader.Parameters["detailTex2"].SetValue(_sunbeamDetail2.Value);
                    _sunbeamShader.Parameters["noiseTex"].SetValue(_sunbeamNoise.Value);

                    _sunbeamShader.Parameters["bodyReps"].SetValue(2.0f * repVal);
                    _sunbeamShader.Parameters["detail1Reps"].SetValue(2.5f * repVal);
                    _sunbeamShader.Parameters["detail2Reps"].SetValue(1.5f * repVal);
                    _sunbeamShader.Parameters["gradientReps"].SetValue(1.0f * repVal);
                    _sunbeamShader.Parameters["bodyScrollSpeed"].SetValue(1.2f);
                    _sunbeamShader.Parameters["detail1ScrollSpeed"].SetValue(1.6f);
                    _sunbeamShader.Parameters["detail2ScrollSpeed"].SetValue(-0.8f);
                    _sunbeamShader.Parameters["noiseDistortion"].SetValue(0.04f);
                    _sunbeamShader.Parameters["totalMult"].SetValue(1.5f);
                    _sunbeamShader.Parameters["uTime"].SetValue(time);

                    _sunbeamShader.CurrentTechnique.Passes["MainPS"].Apply();
                    _sunbeamStrip.DrawTrail();
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                }
            }

            // === LAYER 2: Multi-layer bloom head (Summer solar palette) ===
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowTex = _sunbeamSoftGlow?.Value;
            Texture2D bloomTex = _sunbeamPointBloom?.Value;

            if (glowTex != null && bloomTex != null)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f + 1f;
                // Wide solar outer glow (capped 300px max)
                sb.Draw(glowTex, drawPos, null, SunOrange * 0.4f, 0f,
                    glowTex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
                // Mid golden bloom (capped 300px max)
                sb.Draw(bloomTex, drawPos, null, SunGold * 0.55f, 0f,
                    bloomTex.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
                // White-hot center
                sb.Draw(bloomTex, drawPos, null, SunWhite * 0.7f, 0f,
                    bloomTex.Size() / 2f, 0.08f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
