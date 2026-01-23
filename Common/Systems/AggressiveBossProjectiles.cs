using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// AGGRESSIVE BOSS PROJECTILES - UNIQUE, VISUALLY STUNNING projectile types
    /// 
    /// DESIGN PHILOSOPHY:
    /// - EVERY projectile must have UNIQUE visual identity with layered effects
    /// - NO boring halos-only visuals - use FLARES, SPARKLES, DUST, unique particles
    /// - Each projectile type has DISTINCT visual language
    /// - Colors are vibrant, effects are layered, explosions are spectacular
    /// </summary>
    
    #region Hostile Orb - Ethereal Soul Tracker
    
    /// <summary>
    /// A hostile ethereal orb with swirling internal energy and magic sparkle trail.
    /// Visual: Pulsing core with orbiting spark points, sparkle dust trail, magic rune accents.
    /// </summary>
    public class HostileOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public Color PrimaryColor { get => new Color((int)Projectile.ai[0], (int)Projectile.ai[1], (int)(Projectile.localAI[0]), 255); }
        public Color SecondaryColor => Color.Lerp(PrimaryColor, Color.White, 0.4f);
        public Color AccentColor => Color.Lerp(PrimaryColor, new Color(255, 200, 255), 0.3f);
        public float HomingStrength { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        
        private float pulseTimer = 0f;
        private float orbitAngle = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds - balanced lifetime
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.5f; // Player-sized projectile
        }
        
        public override void AI()
        {
            pulseTimer += 0.12f;
            orbitAngle += 0.18f;
            
            // Homing behavior
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            if (target.active && !target.dead)
            {
                float homingFactor = HomingStrength > 0 ? HomingStrength : 0.03f;
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingFactor);
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === UNIQUE EFFECT: Orbiting spark points - scaled for player-sized projectile ===
            if (Projectile.timeLeft % 6 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 10f;
                    EnhancedParticles.BloomFlare(sparkPos, AccentColor, 0.12f, 8, 2, 0.4f);
                }
            }
            
            // === UNIQUE EFFECT: Magic sparkle dust trail ===
            if (Main.rand.NextBool(2))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
                var sparkle = new SparkleParticle(Projectile.Center + dustOffset, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    SecondaryColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                // Extra magic dust
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.MagicMirror, -Projectile.velocity * 0.1f, 0, PrimaryColor, 0.8f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Trailing glow particles with gradient ===
            if (Main.rand.NextBool(3))
            {
                float gradientProgress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(PrimaryColor, AccentColor, gradientProgress);
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Pulsing light
            float pulse = 0.5f + (float)Math.Sin(pulseTimer) * 0.15f;
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * pulse);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.15f;
            
            // Draw sparkle trail with alternating colors - scaled for player-sized projectile
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float scale = 0.2f * (1f - progress) * pulse;
                Color trailColor = Color.Lerp(PrimaryColor, AccentColor, progress) * (1f - progress) * 0.65f;
                trailColor.A = 0;
                
                // Alternating flare/glow for visual interest
                Texture2D trailTex = i % 2 == 0 ? tex : glowTex;
                Vector2 trailOrigin = i % 2 == 0 ? origin : glowOrigin;
                
                Main.spriteBatch.Draw(trailTex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null, trailColor, Projectile.oldRot[i], trailOrigin, scale, SpriteEffects.None, 0f);
            }
            
            // === UNIQUE: Multi-layer bloom with color gradient ===
            Color outerGlow = PrimaryColor with { A = 0 };
            Color midGlow = SecondaryColor with { A = 0 };
            Color innerGlow = Color.White with { A = 0 };
            
            // Outer ethereal layer - scaled for player-sized projectile
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerGlow * 0.25f, 0f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            // Middle energy layer
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, midGlow * 0.4f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            // Core flare layer
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, outerGlow * 0.55f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            // White-hot center
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, innerGlow * 0.75f, Projectile.rotation, origin, 0.12f * pulse, SpriteEffects.None, 0f);
            
            // Draw orbiting spark points - scaled for player-sized projectile
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 10f - Main.screenPosition;
                Color sparkColor = Color.Lerp(AccentColor, Color.White, 0.3f) with { A = 0 };
                Main.spriteBatch.Draw(tex, sparkPos, null, sparkColor * 0.7f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH: Ethereal soul burst ===
            // Central flash - reduced scale for readability
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.35f, 14, 3, 0.7f);
            EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 0.28f, 16, 2, 0.6f);
            
            // Spiraling sparkle burst - fewer particles
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + orbitAngle;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color burstColor = Color.Lerp(PrimaryColor, AccentColor, (float)i / 6f);
                
                var sparkle = new SparkleParticle(Projectile.Center, burstVel, burstColor, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                // Magic dust explosion
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, burstVel * 1.1f, 0, burstColor, 0.8f);
                dust.noGravity = true;
            }
            
            // Fading glow particles - fewer
            for (int i = 0; i < 3; i++)
            {
                var glow = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    SecondaryColor * 0.6f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // === UNIQUE HIT: Soul impact - compact flash ===
            EnhancedParticles.BloomFlare(target.Center, PrimaryColor, 0.32f, 12, 2, 0.65f);
            
            // Magic dust burst on hit - fewer
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.MagicMirror, Main.rand.NextVector2Circular(4f, 4f), 0, AccentColor, 0.9f);
                dust.noGravity = true;
            }
        }
    }
    
    #endregion
    
    #region Accelerating Bolt - Comet Strike
    
    /// <summary>
    /// A comet-like projectile that starts slow and accelerates with building intensity.
    /// Visual: Elongated flare with fire/spark trail that intensifies, trailing embers, comet tail.
    /// </summary>
    public class AcceleratingBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public Color PrimaryColor { get => new Color((int)Projectile.ai[0], (int)Projectile.ai[1], (int)(Projectile.localAI[0]), 255); }
        public Color SecondaryColor => Color.Lerp(PrimaryColor, Color.Orange, 0.35f);
        public Color HotCoreColor => Color.Lerp(Color.White, PrimaryColor, 0.2f);
        public float MaxSpeed { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        
        private float currentSpeed = 3f;
        private float intensityTimer = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 25;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150; // 2.5 seconds - fast bolt
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.45f; // Player-sized bolt
        }
        
        public override void AI()
        {
            intensityTimer++;
            
            // Accelerate over time
            float maxSpd = MaxSpeed > 0 ? MaxSpeed : 25f;
            currentSpeed = MathHelper.Lerp(currentSpeed, maxSpd, 0.02f);
            float intensity = currentSpeed / maxSpd;
            
            // Lock direction but increase speed
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * currentSpeed;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === UNIQUE EFFECT: Ember trail with increasing density ===
            int emberFrequency = Math.Max(1, 4 - (int)(intensity * 3));
            if (Projectile.timeLeft % emberFrequency == 0)
            {
                // Fire sparks/embers
                for (int i = 0; i < (int)(2 + intensity * 3); i++)
                {
                    Vector2 emberVel = -Projectile.velocity * Main.rand.NextFloat(0.08f, 0.18f) + Main.rand.NextVector2Circular(2f, 2f);
                    
                    var spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                        emberVel, false, 20, 0.25f + intensity * 0.15f, SecondaryColor, new Vector2(0.03f, 1.8f));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Fire dust that floats
                Dust fireDust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.3f + intensity * 0.5f);
                fireDust.noGravity = true;
                fireDust.fadeIn = 1.2f;
            }
            
            // === UNIQUE EFFECT: Comet core glow particles ===
            if (Main.rand.NextBool(2))
            {
                Color coreGlowColor = Color.Lerp(PrimaryColor, HotCoreColor, intensity);
                var coreTrail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    coreGlowColor * (0.4f + intensity * 0.4f), 0.2f + 0.2f * intensity, 20, true);
                MagnumParticleHandler.SpawnParticle(coreTrail);
            }
            
            // === UNIQUE EFFECT: Speed-based flare burst ===
            if (intensity > 0.7f && Main.rand.NextBool(4))
            {
                EnhancedParticles.BloomFlare(Projectile.Center, HotCoreColor, 0.25f, 8, 2, 0.5f);
            }
            
            // Intense lighting
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * (0.4f + intensity * 0.5f));
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.6f, 0.2f) * intensity * 0.3f); // Fire glow
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            float intensity = currentSpeed / (MaxSpeed > 0 ? MaxSpeed : 25f);
            float stretchX = 1.2f + intensity * 0.8f; // Elongates with speed
            
            // Draw comet tail - elongated gradient trail - scaled for player-sized projectile
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float tailScale = (0.15f + 0.15f * intensity) * (1f - progress);
                Color tailColor = Color.Lerp(HotCoreColor, SecondaryColor, progress) * (1f - progress) * 0.55f;
                tailColor.A = 0;
                
                // Stretched flare for tail
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null, tailColor, Projectile.oldRot[i], origin, new Vector2(tailScale * 1.8f, tailScale * 0.7f), SpriteEffects.None, 0f);
            }
            
            // === UNIQUE: Layered comet head - scaled for player-sized projectile ===
            Color outerFlame = SecondaryColor with { A = 0 };
            Color midFlame = PrimaryColor with { A = 0 };
            Color hotCore = HotCoreColor with { A = 0 };
            
            // Outer flame corona
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerFlame * 0.35f, Projectile.rotation, glowOrigin, new Vector2(stretchX * 0.5f, 0.35f), SpriteEffects.None, 0f);
            // Mid energy layer
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, midFlame * 0.5f, Projectile.rotation, origin, new Vector2(stretchX * 0.4f, 0.25f), SpriteEffects.None, 0f);
            // Hot compressed core
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, hotCore * 0.75f, Projectile.rotation, origin, new Vector2(stretchX * 0.25f, 0.15f), SpriteEffects.None, 0f);
            // White-hot tip
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.85f, Projectile.rotation, origin, new Vector2(0.15f, 0.1f), SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnKill(int timeLeft)
        {
            float intensity = currentSpeed / (MaxSpeed > 0 ? MaxSpeed : 25f);
            
            // === UNIQUE DEATH: Comet impact - compact and readable ===
            // Central flash - reduced scale
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.4f, 14, 3, 0.7f);
            EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 0.32f, 16, 2, 0.6f);
            
            // Radial fire burst - fewer particles
            int burstCount = 6 + (int)(intensity * 3);
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                
                // Fire sparks - smaller
                var spark = new GlowSparkParticle(Projectile.Center, burstVel, false, 20, 0.25f, SecondaryColor, new Vector2(0.03f, 1.5f));
                MagnumParticleHandler.SpawnParticle(spark);
                
                // Fire dust - smaller
                Dust fireDust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, burstVel * 0.7f, 0, default, 1.0f);
                fireDust.noGravity = true;
            }
            
            // Fractal flare points - fewer and tighter
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * 12f;
                Color flareColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / 4f);
                EnhancedParticles.BloomFlare(flarePos, flareColor, 0.18f, 10, 2, 0.5f);
            }
        }
    }
    
    #endregion
    
    #region Explosive Orb - Arcane Bomb
    
    /// <summary>
    /// A pulsing arcane bomb that builds energy before exploding into multiple projectiles.
    /// Visual: Spinning rune symbols, charging energy wisps, arcane glyph accents, dramatic detonation.
    /// </summary>
    public class ExplosiveOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public Color PrimaryColor { get => new Color((int)Projectile.ai[0], (int)Projectile.ai[1], (int)(Projectile.localAI[0]), 255); }
        public Color SecondaryColor => Color.Lerp(PrimaryColor, new Color(200, 150, 255), 0.35f);
        public Color ArcaneAccent => Color.Lerp(PrimaryColor, new Color(255, 180, 255), 0.5f);
        public int SplitCount { get => (int)Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        
        private float runeRotation = 0f;
        private float chargeGlow = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120; // Explodes after 2 seconds
            Projectile.scale = 0.6f;
        }
        
        public override void AI()
        {
            runeRotation += 0.08f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            float pulse = 1f + (float)Math.Sin(Projectile.timeLeft * 0.35f) * 0.25f * lifeProgress;
            Projectile.scale = pulse;
            chargeGlow = lifeProgress;
            
            // === UNIQUE EFFECT: Orbiting rune symbols - scaled for player-sized projectile ===
            if (Projectile.timeLeft % 8 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float runeAngle = runeRotation + MathHelper.TwoPi * i / 4f;
                    float runeRadius = 14f + lifeProgress * 5f;
                    Vector2 runePos = Projectile.Center + runeAngle.ToRotationVector2() * runeRadius;
                    
                    // Glyph particles orbiting
                    CustomParticles.Glyph(runePos, ArcaneAccent * (0.5f + lifeProgress * 0.5f), 0.2f + lifeProgress * 0.1f, -1);
                }
            }
            
            // === UNIQUE EFFECT: Converging energy wisps as it charges ===
            if (Projectile.timeLeft < 90 && Main.rand.NextBool((int)(8 - lifeProgress * 6)))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 50f + Main.rand.NextFloat(30f);
                Vector2 wispStart = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 wispVel = (Projectile.Center - wispStart).SafeNormalize(Vector2.Zero) * (4f + lifeProgress * 3f);
                
                Color wispColor = Color.Lerp(SecondaryColor, ArcaneAccent, Main.rand.NextFloat());
                var wisp = new GenericGlowParticle(wispStart, wispVel, wispColor * 0.7f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(wisp);
                
                // Magic dust converging
                Dust dust = Dust.NewDustPerfect(wispStart, DustID.PurpleTorch, wispVel * 0.8f, 0, default, 1.1f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Warning sparkles near explosion ===
            if (Projectile.timeLeft < 45 && Main.rand.NextBool((int)(6 - lifeProgress * 4)))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(25f * pulse, 25f * pulse);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), ArcaneAccent, 0.35f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                EnhancedParticles.BloomFlare(sparklePos, PrimaryColor, 0.2f, 10, 2, 0.5f);
            }
            
            // === UNIQUE EFFECT: Pulsing core flares - scaled for player-sized ===
            if (Projectile.timeLeft % 12 == 0)
            {
                EnhancedParticles.BloomFlare(Projectile.Center, SecondaryColor, 0.2f * pulse, 10, 2, 0.45f);
            }
            
            Projectile.rotation = runeRotation;
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * (0.5f + lifeProgress * 0.5f) * pulse);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH: Arcane detonation - compact and readable ===
            
            // Central arcane flash - significantly reduced
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.5f, 16, 3, 0.75f);
            EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 0.4f, 18, 2, 0.65f);
            
            // Glyph explosion burst - fewer
            CustomParticles.GlyphBurst(Projectile.Center, SecondaryColor, 4, 4f);
            
            // Fractal arcane pattern - single ring only, tighter
            int pointsInRing = 6;
            float ringRadius = 16f;
            for (int i = 0; i < pointsInRing; i++)
            {
                float angle = MathHelper.TwoPi * i / pointsInRing + runeRotation;
                Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * ringRadius;
                Color flareColor = Color.Lerp(PrimaryColor, ArcaneAccent, (float)i / pointsInRing);
                
                EnhancedParticles.BloomFlare(flarePos, flareColor, 0.18f, 12, 2, 0.5f);
            }
            
            // Particle explosion - fewer and smaller
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                
                var sparkle = new SparkleParticle(Projectile.Center, burstVel, ArcaneAccent, 0.25f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
                
                // Purple magic dust - smaller
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, burstVel * 1f, 0, default, 1.0f);
                dust.noGravity = true;
            }
            
            // Spawn split projectiles
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = SplitCount > 0 ? SplitCount : 8;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);
                    
                    int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, vel,
                        ModContent.ProjectileType<HostileOrbProjectile>(), Projectile.damage / 2, 0f, Main.myPlayer);
                    
                    if (proj < Main.maxProjectiles)
                    {
                        Main.projectile[proj].ai[0] = PrimaryColor.R;
                        Main.projectile[proj].ai[1] = PrimaryColor.G;
                        Main.projectile[proj].localAI[0] = PrimaryColor.B;
                        Main.projectile[proj].localAI[1] = 0.015f; // Low homing
                    }
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f }, Projectile.Center);
            MagnumScreenEffects.AddScreenShake(6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            float pulse = Projectile.scale;
            
            // === UNIQUE: Layered arcane orb - scaled for player-sized projectile ===
            Color outerArcane = ArcaneAccent with { A = 0 };
            Color midArcane = SecondaryColor with { A = 0 };
            Color coreArcane = PrimaryColor with { A = 0 };
            
            // Outer mystical glow
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerArcane * (0.25f + lifeProgress * 0.35f), runeRotation, glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            // Middle charging layer
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, midArcane * (0.35f + lifeProgress * 0.25f), -runeRotation * 0.5f, glowOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
            // Core energy flare
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, coreArcane * 0.6f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            // Hot inner core
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * (0.65f + lifeProgress * 0.25f), Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);
            
            // Draw orbiting rune points - scaled for player-sized projectile
            for (int i = 0; i < 4; i++)
            {
                float runeAngle = runeRotation + MathHelper.TwoPi * i / 4f;
                float runeRadius = 12f + lifeProgress * 4f;
                Vector2 runePos = Projectile.Center + runeAngle.ToRotationVector2() * runeRadius - Main.screenPosition;
                Color runeColor = Color.Lerp(PrimaryColor, ArcaneAccent, (float)i / 4f) with { A = 0 };
                
                Main.spriteBatch.Draw(tex, runePos, null, runeColor * 0.7f, runeAngle, origin, 0.1f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Wave Projectile - Serpentine Energy
    
    /// <summary>
    /// A serpentine energy wave that slithers through the air in sine wave motion.
    /// Visual: Flowing ribbon-like trail with electric sparks, prismatic shimmer accents.
    /// </summary>
    public class WaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public Color PrimaryColor { get => new Color((int)Projectile.ai[0], (int)Projectile.ai[1], (int)(Projectile.localAI[0]), 255); }
        public Color SecondaryColor => Color.Lerp(PrimaryColor, new Color(150, 255, 255), 0.4f);
        public Color ElectricAccent => Color.Lerp(PrimaryColor, Color.Cyan, 0.5f);
        public float WaveAmplitude { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        
        private float waveTimer = 0f;
        private Vector2 baseDirection;
        private float baseSpeed;
        private float sparkTimer = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds - balanced wave lifetime
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.45f; // Player-sized wave
        }
        
        public override void AI()
        {
            if (waveTimer == 0)
            {
                baseDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);
                baseSpeed = Projectile.velocity.Length();
            }
            
            waveTimer += 0.15f;
            sparkTimer += 0.1f;
            
            float amplitude = WaveAmplitude > 0 ? WaveAmplitude : 4f;
            float waveOffset = (float)Math.Sin(waveTimer) * amplitude;
            
            Vector2 perpendicular = new Vector2(-baseDirection.Y, baseDirection.X);
            Projectile.velocity = baseDirection * baseSpeed + perpendicular * waveOffset;
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === UNIQUE EFFECT: Flowing ribbon trail with electric sparks ===
            if (Main.rand.NextBool(2))
            {
                // Ribbon-like glow trail
                Vector2 trailOffset = perpendicular * (float)Math.Sin(waveTimer + 1f) * 4f;
                Color ribbonColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)Math.Sin(waveTimer) * 0.5f + 0.5f);
                var ribbon = new GenericGlowParticle(Projectile.Center + trailOffset, -Projectile.velocity * 0.08f,
                    ribbonColor * 0.6f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(ribbon);
            }
            
            // === UNIQUE EFFECT: Electric spark accents ===
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkOffset = Main.rand.NextVector2Circular(12f, 12f);
                var spark = new SparkleParticle(Projectile.Center + sparkOffset, 
                    perpendicular * Main.rand.NextFloat(-3f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                    ElectricAccent, 0.3f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
                
                // Electric dust
                Dust dust = Dust.NewDustPerfect(Projectile.Center + sparkOffset, DustID.Electric, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.9f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Prismatic shimmer at wave peaks ===
            if (Math.Abs((float)Math.Sin(waveTimer)) > 0.9f && Main.rand.NextBool(3))
            {
                float hue = (waveTimer * 0.1f) % 1f;
                Color prismColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                EnhancedParticles.BloomFlare(Projectile.Center, prismColor, 0.22f, 10, 2, 0.5f);
            }
            
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.45f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            // === UNIQUE: Ribbon-like flowing trail with gradient - scaled for player-sized projectile ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float wavePhase = (float)Math.Sin(waveTimer - i * 0.15f);
                float scale = 0.18f * (1f - progress) * (1f + wavePhase * 0.15f);
                
                // Gradient along ribbon
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, progress) * (1f - progress) * 0.6f;
                trailColor.A = 0;
                
                // Alternate between flare and soft glow for ribbon texture
                if (i % 3 == 0)
                {
                    Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                        null, trailColor, Projectile.oldRot[i], origin, scale * 1.2f, SpriteEffects.None, 0f);
                }
                else
                {
                    Main.spriteBatch.Draw(glowTex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                        null, trailColor * 0.7f, Projectile.oldRot[i], glowOrigin, scale, SpriteEffects.None, 0f);
                }
            }
            
            // === UNIQUE: Layered serpent head - scaled for player-sized projectile ===
            Color outerGlow = SecondaryColor with { A = 0 };
            Color midGlow = PrimaryColor with { A = 0 };
            Color coreGlow = Color.White with { A = 0 };
            
            // Outer flowing glow
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerGlow * 0.35f, Projectile.rotation, glowOrigin, 0.4f, SpriteEffects.None, 0f);
            // Middle energy
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, midGlow * 0.55f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            // Core
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, coreGlow * 0.75f, Projectile.rotation, origin, 0.12f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH: Serpentine energy dissipation ===
            // Flash
            EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 0.5f, 18, 3, 0.8f);
            
            // Wave pattern death - sparks follow the sine wave outward
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float wavePhase = (float)Math.Sin(waveTimer + angle * 2f);
                Vector2 deathVel = angle.ToRotationVector2() * (5f + wavePhase * 2f);
                
                Color sparkColor = Color.Lerp(PrimaryColor, ElectricAccent, (float)i / 10f);
                var spark = new SparkleParticle(Projectile.Center, deathVel, sparkColor, 0.35f, 22);
                MagnumParticleHandler.SpawnParticle(spark);
                
                // Electric dust burst
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, deathVel * 1.1f, 0, default, 1f);
                dust.noGravity = true;
            }
        }
    }
    
    #endregion
    
    #region Delayed Detonation - Arcane Mine
    
    /// <summary>
    /// An arcane mine that charges energy before detonating in a devastating explosion.
    /// Visual: Pulsing magic circle, converging star points, glyph symbols, spectacular detonation.
    /// </summary>
    public class DelayedDetonationProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public Color PrimaryColor { get => new Color((int)Projectile.ai[0], (int)Projectile.ai[1], (int)(Projectile.localAI[0]), 255); }
        public Color SecondaryColor => Color.Lerp(PrimaryColor, new Color(255, 220, 180), 0.4f);
        public Color WarningColor => Color.Lerp(PrimaryColor, Color.Red, 0.5f);
        public int DetonationDelay { get => (int)Projectile.localAI[1]; set => Projectile.localAI[1] = value; }
        
        private int timer = 0;
        private float circleRotation = 0f;
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = false; // Only hostile on explosion
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.scale = 0.55f;
        }
        
        public override void AI()
        {
            int delay = DetonationDelay > 0 ? DetonationDelay : 60;
            timer++;
            circleRotation += 0.04f + (float)timer / delay * 0.08f; // Speeds up as it charges
            
            Projectile.velocity = Vector2.Zero;
            
            float progress = (float)timer / delay;
            float warningScale = 0.3f + progress * 0.7f;
            float pulseRate = 0.15f + progress * 0.2f;
            float pulse = 1f + (float)Math.Sin(timer * pulseRate) * 0.15f * progress;
            
            // === UNIQUE EFFECT: Magic circle with rotating star points - scaled for player-sized ===
            if (timer % 10 == 0)
            {
                int starPoints = 6;
                for (int i = 0; i < starPoints; i++)
                {
                    float starAngle = circleRotation + MathHelper.TwoPi * i / starPoints;
                    float radius = 20f * warningScale;
                    Vector2 starPos = Projectile.Center + starAngle.ToRotationVector2() * radius;
                    
                    Color starColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / starPoints);
                    EnhancedParticles.BloomFlare(starPos, starColor, 0.12f + progress * 0.08f, 12, 2, 0.4f);
                }
            }
            
            // === UNIQUE EFFECT: Glyph symbols appear and orbit - scaled for player-sized ===
            if (timer % 20 == 0 && progress > 0.3f)
            {
                float glyphAngle = circleRotation * 0.7f;
                Vector2 glyphPos = Projectile.Center + glyphAngle.ToRotationVector2() * 16f * warningScale;
                CustomParticles.Glyph(glyphPos, PrimaryColor * 0.8f, 0.2f, -1);
            }
            
            // === UNIQUE EFFECT: Converging energy particles - scaled for player-sized ===
            if (timer % 4 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 32f * (1f - progress * 0.5f) + Main.rand.NextFloat(12f);
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 particleVel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * (2.5f + progress * 2f);
                
                Color convergeColor = Color.Lerp(SecondaryColor, WarningColor, progress);
                var converge = new GenericGlowParticle(particlePos, particleVel, convergeColor * 0.65f, 0.15f, 16, true);
                MagnumParticleHandler.SpawnParticle(converge);
                
                // Magical dust
                Dust dust = Dust.NewDustPerfect(particlePos, DustID.GoldFlame, particleVel * 0.7f, 0, default, 0.9f + progress * 0.4f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Warning sparkles intensify - scaled for player-sized ===
            if (progress > 0.5f && Main.rand.NextBool((int)(6 - progress * 4)))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(18f * warningScale, 18f * warningScale);
                Color sparkleColor = Color.Lerp(SecondaryColor, WarningColor, progress);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1.5f, 1.5f), sparkleColor, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === UNIQUE EFFECT: Pulsing core flare - scaled for player-sized ===
            if (timer % 15 == 0)
            {
                EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 0.2f * pulse, 12, 2, 0.45f);
            }
            
            // Explosion
            if (timer >= delay)
            {
                Projectile.hostile = true;
                Explode();
                Projectile.Kill();
            }
            
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * (0.4f + progress * 0.5f) * pulse);
        }
        
        private void Explode()
        {
            // === UNIQUE DEATH: Grand arcane detonation ===
            
            // Central flash cascade
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 1.4f, 28, 4, 1f);
            EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 1.1f, 30, 4, 0.9f);
            EnhancedParticles.BloomFlare(Projectile.Center, SecondaryColor, 0.85f, 25, 3, 0.75f);
            
            // Glyph explosion
            CustomParticles.GlyphBurst(Projectile.Center, PrimaryColor, 10, 7f);
            
            // Fractal star pattern explosion
            for (int ring = 0; ring < 4; ring++)
            {
                int pointsInRing = 6 + ring * 2;
                float ringRadius = 20f + ring * 25f;
                float ringOffset = circleRotation + ring * 0.4f;
                
                for (int i = 0; i < pointsInRing; i++)
                {
                    float angle = MathHelper.TwoPi * i / pointsInRing + ringOffset;
                    Vector2 flarePos = Projectile.Center + angle.ToRotationVector2() * ringRadius;
                    Color ringColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)ring / 4f);
                    
                    EnhancedParticles.BloomFlare(flarePos, ringColor, 0.4f - ring * 0.06f, 18 - ring * 2, 2, 0.7f);
                    
                    // Sparkle at each point
                    var sparkle = new SparkleParticle(flarePos, angle.ToRotationVector2() * (2f + ring), ringColor, 0.35f, 22);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Radial particle burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                
                Color burstColor = Color.Lerp(PrimaryColor, WarningColor, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.8f, 0.35f, 28, true);
                MagnumParticleHandler.SpawnParticle(glow);
                
                // Explosion dust
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, burstVel * 1.1f, 0, default, 1.5f);
                dust.noGravity = true;
            }
            
            // Deal damage in area
            float explosionRadius = 80f;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead && Vector2.Distance(player.Center, Projectile.Center) < explosionRadius)
                {
                    player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByProjectile(i, Projectile.whoAmI), Projectile.damage, 0);
                }
            }
            
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            MagnumScreenEffects.AddScreenShake(8f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            int delay = DetonationDelay > 0 ? DetonationDelay : 60;
            float progress = (float)timer / delay;
            float pulse = 1f + (float)Math.Sin(timer * (0.15f + progress * 0.2f)) * 0.15f * progress;
            float warningScale = 0.3f + progress * 0.7f;
            
            // === UNIQUE: Layered arcane mine - scaled for player-sized projectile ===
            Color outerGlow = SecondaryColor with { A = 0 };
            Color midGlow = PrimaryColor with { A = 0 };
            Color warningGlow = Color.Lerp(PrimaryColor, WarningColor, progress) with { A = 0 };
            
            // Outer warning aura (grows with charge)
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerGlow * (0.2f + progress * 0.35f), circleRotation, glowOrigin, 0.7f * warningScale * pulse, SpriteEffects.None, 0f);
            // Middle energy layer
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, warningGlow * 0.4f, -circleRotation * 0.6f, glowOrigin, 0.5f * warningScale * pulse, SpriteEffects.None, 0f);
            // Core flare
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, midGlow * 0.55f, circleRotation, origin, 0.3f * warningScale * pulse, SpriteEffects.None, 0f);
            // Hot center
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * (0.5f + progress * 0.35f), 0f, origin, 0.12f * pulse, SpriteEffects.None, 0f);
            
            // Draw star point indicators - scaled for player-sized projectile
            int starPoints = 6;
            for (int i = 0; i < starPoints; i++)
            {
                float starAngle = circleRotation + MathHelper.TwoPi * i / starPoints;
                float radius = 18f * warningScale;
                Vector2 starPos = Projectile.Center + starAngle.ToRotationVector2() * radius - Main.screenPosition;
                Color starColor = Color.Lerp(PrimaryColor, SecondaryColor, (float)i / starPoints) with { A = 0 };
                
                Main.spriteBatch.Draw(tex, starPos, null, starColor * (0.4f + progress * 0.35f), starAngle, origin, 0.08f * pulse, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
    
    #endregion
    
    #region Boomerang Projectile - Phantom Scythe
    
    /// <summary>
    /// A spectral scythe that sweeps out and returns, creating pincer attacks.
    /// Visual: Spinning blade with ghost trail, spectral wisps, slashing arc effects.
    /// </summary>
    public class BoomerangProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        public Color PrimaryColor { get => new Color((int)Projectile.ai[0], (int)Projectile.ai[1], (int)(Projectile.localAI[0]), 255); }
        public Color SecondaryColor => Color.Lerp(PrimaryColor, new Color(180, 200, 255), 0.4f);
        public Color GhostColor => Color.Lerp(PrimaryColor, new Color(200, 220, 255), 0.6f);
        
        private Vector2 returnTarget;
        private bool returning = false;
        private int outTimer = 0;
        private float spinAngle = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds - balanced boomerang lifetime
        }
        
        public override void AI()
        {
            if (outTimer == 0)
            {
                returnTarget = Projectile.Center; // Store starting position
            }
            
            outTimer++;
            spinAngle += 0.25f; // Fast spin for blade effect
            
            if (!returning)
            {
                // Travel outward with deceleration
                Projectile.velocity *= 0.98f;
                
                // Start return after slowing enough or 45 frames
                if (Projectile.velocity.Length() < 4f || outTimer > 45)
                {
                    returning = true;
                    
                    // === UNIQUE: Turn-around flash ===
                    EnhancedParticles.BloomFlare(Projectile.Center, GhostColor, 0.45f, 15, 3, 0.7f);
                    
                    // Spectral burst at turn point
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        var wisp = new GenericGlowParticle(Projectile.Center, angle.ToRotationVector2() * 3f, GhostColor * 0.6f, 0.25f, 18, true);
                        MagnumParticleHandler.SpawnParticle(wisp);
                    }
                }
            }
            else
            {
                // Return to source
                Vector2 toReturn = (returnTarget - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toReturn * 18f, 0.08f);
                
                // Kill when returned
                if (Vector2.Distance(Projectile.Center, returnTarget) < 30f)
                {
                    Projectile.Kill();
                }
            }
            
            Projectile.rotation = spinAngle;
            
            // === UNIQUE EFFECT: Slashing arc trail - scaled for player-sized ===
            if (Projectile.timeLeft % 3 == 0)
            {
                // Blade edge particles
                Vector2 bladeOffset = spinAngle.ToRotationVector2() * 8f;
                EnhancedParticles.BloomFlare(Projectile.Center + bladeOffset, SecondaryColor, 0.1f, 10, 2, 0.35f);
                EnhancedParticles.BloomFlare(Projectile.Center - bladeOffset, SecondaryColor, 0.1f, 10, 2, 0.35f);
            }
            
            // === UNIQUE EFFECT: Spectral wisp trail ===
            if (Main.rand.NextBool(2))
            {
                Vector2 wispOffset = Main.rand.NextVector2Circular(10f, 10f);
                Color wispColor = Color.Lerp(GhostColor, SecondaryColor, Main.rand.NextFloat());
                var wisp = new GenericGlowParticle(Projectile.Center + wispOffset, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    wispColor * 0.55f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(wisp);
                
                // Ghost dust
                Dust dust = Dust.NewDustPerfect(Projectile.Center + wispOffset, DustID.Frost, -Projectile.velocity * 0.08f, 50, default, 0.9f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Sparkle accents ===
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                    Main.rand.NextVector2Circular(1f, 1f), GhostColor, 0.28f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.45f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            // === UNIQUE: Ghost blade trail with alternating opacity - scaled for player-sized projectile ===
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float scale = 0.2f * (1f - progress);
                float ghostAlpha = (1f - progress) * (i % 2 == 0 ? 0.55f : 0.35f); // Alternating for ghostly effect
                
                Color trailColor = Color.Lerp(GhostColor, SecondaryColor, progress) * ghostAlpha;
                trailColor.A = 0;
                
                // Stretched blade shape
                Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition,
                    null, trailColor, Projectile.oldRot[i], origin, new Vector2(scale * 1.8f, scale * 0.6f), SpriteEffects.None, 0f);
            }
            
            // === UNIQUE: Layered spinning blade - scaled for player-sized projectile ===
            Color outerBlade = GhostColor with { A = 0 };
            Color midBlade = SecondaryColor with { A = 0 };
            Color coreBlade = PrimaryColor with { A = 0 };
            
            // Outer spectral glow
            Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerBlade * 0.35f, spinAngle, glowOrigin, 0.45f, SpriteEffects.None, 0f);
            // Blade shape - stretched horizontally
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, midBlade * 0.5f, spinAngle, origin, new Vector2(0.4f, 0.15f), SpriteEffects.None, 0f);
            // Perpendicular blade edge
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, coreBlade * 0.55f, spinAngle + MathHelper.PiOver2, origin, new Vector2(0.3f, 0.12f), SpriteEffects.None, 0f);
            // Center core
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.7f, spinAngle, origin, 0.12f, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void OnKill(int timeLeft)
        {
            // === UNIQUE DEATH: Phantom blade dissipation ===
            // Central flash
            EnhancedParticles.BloomFlare(Projectile.Center, GhostColor, 0.55f, 18, 3, 0.8f);
            
            // Spinning blade shatter pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + spinAngle;
                Vector2 shatterVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                Color shatterColor = Color.Lerp(GhostColor, SecondaryColor, (float)i / 8f);
                var shard = new GenericGlowParticle(Projectile.Center, shatterVel, shatterColor * 0.7f, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(shard);
                
                // Frost dust for ghostly effect
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Frost, shatterVel * 1.1f, 50, default, 1.1f);
                dust.noGravity = true;
            }
            
            // Sparkle burst
            for (int i = 0; i < 6; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), GhostColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Helper class for spawning boss projectiles with theme colors.
    /// </summary>
    public static class BossProjectileHelper
    {
        public static void SpawnHostileOrb(Vector2 position, Vector2 velocity, int damage, Color color, float homing = 0.03f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int proj = Projectile.NewProjectile(null, position, velocity, 
                ModContent.ProjectileType<HostileOrbProjectile>(), damage, 0f, Main.myPlayer);
            
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = color.R;
                Main.projectile[proj].ai[1] = color.G;
                Main.projectile[proj].localAI[0] = color.B;
                Main.projectile[proj].localAI[1] = homing;
            }
        }
        
        public static void SpawnAcceleratingBolt(Vector2 position, Vector2 velocity, int damage, Color color, float maxSpeed = 25f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int proj = Projectile.NewProjectile(null, position, velocity, 
                ModContent.ProjectileType<AcceleratingBoltProjectile>(), damage, 0f, Main.myPlayer);
            
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = color.R;
                Main.projectile[proj].ai[1] = color.G;
                Main.projectile[proj].localAI[0] = color.B;
                Main.projectile[proj].localAI[1] = maxSpeed;
            }
        }
        
        public static void SpawnExplosiveOrb(Vector2 position, Vector2 velocity, int damage, Color color, int splitCount = 8)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int proj = Projectile.NewProjectile(null, position, velocity, 
                ModContent.ProjectileType<ExplosiveOrbProjectile>(), damage, 0f, Main.myPlayer);
            
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = color.R;
                Main.projectile[proj].ai[1] = color.G;
                Main.projectile[proj].localAI[0] = color.B;
                Main.projectile[proj].localAI[1] = splitCount;
            }
        }
        
        public static void SpawnWaveProjectile(Vector2 position, Vector2 velocity, int damage, Color color, float amplitude = 4f)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int proj = Projectile.NewProjectile(null, position, velocity, 
                ModContent.ProjectileType<WaveProjectile>(), damage, 0f, Main.myPlayer);
            
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = color.R;
                Main.projectile[proj].ai[1] = color.G;
                Main.projectile[proj].localAI[0] = color.B;
                Main.projectile[proj].localAI[1] = amplitude;
            }
        }
        
        public static void SpawnDelayedDetonation(Vector2 position, int damage, Color color, int delay = 60)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int proj = Projectile.NewProjectile(null, position, Vector2.Zero, 
                ModContent.ProjectileType<DelayedDetonationProjectile>(), damage, 0f, Main.myPlayer);
            
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = color.R;
                Main.projectile[proj].ai[1] = color.G;
                Main.projectile[proj].localAI[0] = color.B;
                Main.projectile[proj].localAI[1] = delay;
            }
        }
        
        public static void SpawnBoomerang(Vector2 position, Vector2 velocity, int damage, Color color)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            int proj = Projectile.NewProjectile(null, position, velocity, 
                ModContent.ProjectileType<BoomerangProjectile>(), damage, 0f, Main.myPlayer);
            
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = color.R;
                Main.projectile[proj].ai[1] = color.G;
                Main.projectile[proj].localAI[0] = color.B;
            }
        }
    }
    
    #endregion
}
