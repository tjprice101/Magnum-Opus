using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

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

        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";

        private float orbitTimer = 0f;

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            orbitTimer += 0.12f;

            // ═══════════════════════════════════════════════════════════════
            // DENSE DUST TRAIL - 2+ particles per frame! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // CONTRASTING SPARKLES - 1-in-2 for visual pop
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust contrast = Dust.NewDustPerfect(sparklePos, DustID.WhiteTorch, 
                    -Projectile.velocity * 0.12f, 0, SunWhite, 1.3f);
                contrast.noGravity = true;
            }

            // ═══════════════════════════════════════════════════════════════
            // FREQUENT FLARES littering the air - 1-in-2
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(10f, 10f);
                float hue = HueMin + Main.rand.NextFloat() * (HueMax - HueMin);
                Color flareColor = Main.hslToRgb(hue, 1f, 0.7f);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, flareColor, 0.45f, 16);
            }

            // ═══════════════════════════════════════════════════════════════
            // SOLAR CORONA ORBIT - Enhanced with more particles
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // ORBITING MUSIC NOTES - Locked to projectile! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // RADIANT PULSE RINGS - Enhanced
            // ═══════════════════════════════════════════════════════════════
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
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER IMPACT - Layered cascade (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
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
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER CASCADE - NOT a puff! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
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
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load MULTIPLE flare textures for layered spinning effect
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float time = Main.GameUpdateCount * 0.06f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.18f;
            
            // Colors with alpha removed for proper additive blending (Fargos pattern)
            Color goldBloom = SunGold with { A = 0 };
            Color orangeBloom = SunOrange with { A = 0 };
            Color whiteBloom = SunWhite with { A = 0 };
            Color redBloom = SunRed with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══════════════════════════════════════════════════════════════
            // TRAIL RENDERING with gradient
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.6f;
                float trailScale = 0.4f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(goldBloom, orangeBloom, progress) * trailAlpha;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], 
                    origin, trailScale, SpriteEffects.None, 0f);
            }

            // ═══════════════════════════════════════════════════════════════
            // 4+ LAYERED SPINNING FLARES (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Layer 1: Soft glow base (large, dim)
            spriteBatch.Draw(softGlow, drawPos, null, orangeBloom * 0.35f, 0f, 
                glowOrigin, 0.7f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise (gold)
            spriteBatch.Draw(flare1, drawPos, null, goldBloom * 0.55f, time, 
                flareOrigin1, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise (red accent)
            spriteBatch.Draw(flare2, drawPos, null, redBloom * 0.4f, -time * 0.8f, 
                flareOrigin2, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different rotation speed (orange)
            spriteBatch.Draw(flare1, drawPos, null, orangeBloom * 0.5f, time * 1.4f, 
                flareOrigin1, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Main halo texture
            spriteBatch.Draw(texture, drawPos, null, goldBloom * 0.6f, Projectile.rotation, 
                origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: Bright white core
            spriteBatch.Draw(texture, drawPos, null, whiteBloom * 0.85f, Projectile.rotation, 
                origin, 0.2f, SpriteEffects.None, 0f);

            // ═══════════════════════════════════════════════════════════════
            // ORBITING SPARK POINTS around the orb (solar corona visual)
            // ═══════════════════════════════════════════════════════════════
            float sparkOrbitAngle = time * 1.8f;
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = sparkOrbitAngle + MathHelper.TwoPi * i / 4f;
                float sparkRadius = 12f + (float)Math.Sin(time * 3f + i) * 3f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * sparkRadius;
                Color sparkColor = Color.Lerp(goldBloom, redBloom, i / 4f);
                spriteBatch.Draw(texture, sparkPos, null, sparkColor * 0.65f, 0f, 
                    origin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

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

        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField11";

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // ═══════════════════════════════════════════════════════════════
            // DENSE DUST TRAIL - 3 particles per frame! (intense beam)
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // CONTRASTING SPARKLES - 1-in-2 for visual pop
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Dust contrast = Dust.NewDustPerfect(sparklePos, DustID.WhiteTorch, 
                    -Projectile.velocity * 0.1f, 0, SunWhite, 1.5f);
                contrast.noGravity = true;
            }

            // ═══════════════════════════════════════════════════════════════
            // FREQUENT FLARES littering the air - 1-in-2
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // SIDE SPARKS - Enhanced with oscillating colors
            // ═══════════════════════════════════════════════════════════════
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

            // ═══════════════════════════════════════════════════════════════
            // ORBITING MUSIC NOTES - Locked to projectile! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
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
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER IMPACT - Layered cascade (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
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
            // ═══════════════════════════════════════════════════════════════
            // GLIMMER CASCADE - NOT a puff! (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
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

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Load MULTIPLE flare textures for layered spinning effect
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare4").Value;
            Texture2D softGlow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 origin = texture.Size() / 2f;
            Vector2 flareOrigin1 = flare1.Size() / 2f;
            Vector2 flareOrigin2 = flare2.Size() / 2f;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float time = Main.GameUpdateCount * 0.08f;
            float pulse = 1f + (float)Math.Sin(time * 2f) * 0.2f;
            
            // Colors with alpha removed for proper additive blending (Fargos pattern)
            Color goldBloom = SunGold with { A = 0 };
            Color orangeBloom = SunOrange with { A = 0 };
            Color whiteBloom = SunWhite with { A = 0 };
            Color redBloom = SunRed with { A = 0 };

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══════════════════════════════════════════════════════════════
            // TRAIL RENDERING with gradient - Long intense beam trail
            // ═══════════════════════════════════════════════════════════════
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.7f;
                float trailScale = 0.55f * (1f - progress * 0.4f);
                Color trailColor = Color.Lerp(whiteBloom, orangeBloom, progress) * trailAlpha;
                
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], 
                    origin, trailScale, SpriteEffects.None, 0f);
            }

            // ═══════════════════════════════════════════════════════════════
            // 5+ LAYERED SPINNING FLARES (TRUE_VFX_STANDARDS)
            // ═══════════════════════════════════════════════════════════════
            
            // Layer 1: Soft glow base (large, dim)
            spriteBatch.Draw(softGlow, drawPos, null, orangeBloom * 0.4f, 0f, 
                glowOrigin, 0.85f * pulse, SpriteEffects.None, 0f);
            
            // Layer 2: First flare spinning clockwise (gold)
            spriteBatch.Draw(flare1, drawPos, null, goldBloom * 0.6f, time, 
                flareOrigin1, 0.55f * pulse, SpriteEffects.None, 0f);
            
            // Layer 3: Second flare spinning counter-clockwise (red accent)
            spriteBatch.Draw(flare2, drawPos, null, redBloom * 0.45f, -time * 0.75f, 
                flareOrigin2, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // Layer 4: Third flare different rotation speed (orange)
            spriteBatch.Draw(flare1, drawPos, null, orangeBloom * 0.55f, time * 1.5f, 
                flareOrigin1, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // Layer 5: Main sparkle texture
            spriteBatch.Draw(texture, drawPos, null, goldBloom * 0.65f, Projectile.rotation, 
                origin, 0.55f * pulse, SpriteEffects.None, 0f);
            
            // Layer 6: Bright white core
            spriteBatch.Draw(texture, drawPos, null, whiteBloom * 0.9f, Projectile.rotation, 
                origin, 0.32f, SpriteEffects.None, 0f);

            // ═══════════════════════════════════════════════════════════════
            // ORBITING SPARK POINTS around the beam (solar flare corona)
            // ═══════════════════════════════════════════════════════════════
            float sparkOrbitAngle = time * 2.2f;
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = sparkOrbitAngle + MathHelper.TwoPi * i / 5f;
                float sparkRadius = 16f + (float)Math.Sin(time * 3.5f + i) * 4f;
                Vector2 sparkPos = drawPos + sparkAngle.ToRotationVector2() * sparkRadius;
                Color sparkColor = Color.Lerp(goldBloom, redBloom, (float)i / 5f);
                spriteBatch.Draw(texture, sparkPos, null, sparkColor * 0.7f, 0f, 
                    origin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
