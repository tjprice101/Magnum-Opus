using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom sky background effect for Eroica boss fight.
    /// Black at surface, gradient up to deep scarlet red with golden flares and dust particles.
    /// Enhanced with Calamity-style animated background elements.
    /// </summary>
    public class EroicaSkyEffect : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int animationTimer = 0;
        
        // Background star/ember particles
        private List<SkyParticle> backgroundParticles = new List<SkyParticle>();
        private const int MaxBackgroundParticles = 120;
        
        // Animated pulse effect for dramatic moments
        private float pulseIntensity = 0f;
        private float pulseTimer = 0f;
        
        // Lightning flash effect
        private float flashIntensity = 0f;
        
        private struct SkyParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Opacity;
            public float OpacityDecay;
            public Color ParticleColor;
            public int Type; // 0 = ember, 1 = sparkle, 2 = dust mote
            public int Lifetime;
            public int TimeAlive;
        }
        
        public override void OnLoad()
        {
            backgroundParticles = new List<SkyParticle>();
        }

        public override void Update(GameTime gameTime)
        {
            animationTimer++;
            
            if (isActive && intensity < 1f)
            {
                intensity += 0.01f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.02f;
            }
            
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
            
            // Update pulse effect - creates breathing/pulsing atmosphere
            pulseTimer += 0.02f;
            pulseIntensity = (float)Math.Sin(pulseTimer) * 0.15f + 0.85f;
            
            // Decay flash intensity
            flashIntensity *= 0.92f;
            if (flashIntensity < 0.01f)
                flashIntensity = 0f;
            
            // Update and spawn background particles
            UpdateBackgroundParticles();
            
            // Spawn new particles while active
            if (isActive && intensity > 0.3f)
            {
                SpawnBackgroundParticles();
            }
        }
        
        private void UpdateBackgroundParticles()
        {
            for (int i = backgroundParticles.Count - 1; i >= 0; i--)
            {
                var particle = backgroundParticles[i];
                particle.Position += particle.Velocity;
                particle.Rotation += particle.RotationSpeed;
                particle.TimeAlive++;
                
                // Fade based on lifetime
                float lifeProgress = (float)particle.TimeAlive / particle.Lifetime;
                if (lifeProgress < 0.2f)
                    particle.Opacity = lifeProgress / 0.2f;
                else if (lifeProgress > 0.7f)
                    particle.Opacity = 1f - (lifeProgress - 0.7f) / 0.3f;
                
                particle.Opacity *= particle.OpacityDecay;
                
                backgroundParticles[i] = particle;
                
                // Remove dead particles
                if (particle.TimeAlive >= particle.Lifetime || particle.Opacity <= 0f)
                {
                    backgroundParticles.RemoveAt(i);
                }
            }
        }
        
        private void SpawnBackgroundParticles()
        {
            if (backgroundParticles.Count >= MaxBackgroundParticles)
                return;
                
            // Spawn rate based on intensity
            int spawnChance = (int)MathHelper.Lerp(8, 2, intensity);
            if (Main.rand.NextBool(spawnChance))
            {
                float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-200, Main.screenWidth + 200);
                float spawnY = Main.screenPosition.Y + Main.rand.NextFloat(-100, Main.screenHeight * 0.8f);
                
                int particleType = Main.rand.Next(3);
                Color particleColor;
                float scale;
                
                switch (particleType)
                {
                    case 0: // Golden ember
                        particleColor = Color.Lerp(new Color(255, 200, 50), new Color(255, 150, 30), Main.rand.NextFloat());
                        scale = Main.rand.NextFloat(0.5f, 1.5f);
                        break;
                    case 1: // Red sparkle
                        particleColor = Color.Lerp(new Color(200, 50, 50), new Color(150, 30, 60), Main.rand.NextFloat());
                        scale = Main.rand.NextFloat(0.3f, 1.0f);
                        break;
                    default: // White dust mote
                        particleColor = Color.Lerp(new Color(255, 220, 200), new Color(200, 180, 170), Main.rand.NextFloat());
                        scale = Main.rand.NextFloat(0.2f, 0.6f);
                        break;
                }
                
                backgroundParticles.Add(new SkyParticle
                {
                    Position = new Vector2(spawnX, spawnY),
                    Velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.2f, 1.5f)),
                    Scale = scale,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.05f, 0.05f),
                    Opacity = 0f,
                    OpacityDecay = 1f,
                    ParticleColor = particleColor,
                    Type = particleType,
                    Lifetime = Main.rand.Next(180, 400),
                    TimeAlive = 0
                });
            }
        }
        
        /// <summary>
        /// Triggers a lightning flash effect. Call this from boss attacks for dramatic effect.
        /// </summary>
        public void TriggerFlash(float flashStrength = 1f)
        {
            flashIntensity = Math.Max(flashIntensity, flashStrength);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                
                // Calculate player's surface position for gradient
                float surfaceY = (float)Main.worldSurface * 16f;
                
                // Draw gradient background with pulse effect
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float worldY = Main.screenPosition.Y + y;
                    float relativeY = worldY - surfaceY;
                    
                    float gradientFactor = MathHelper.Clamp(-relativeY / 2000f, 0f, 1f);
                    
                    // Animated gradient with slight color shifting
                    float colorShift = (float)Math.Sin(animationTimer * 0.01f + y * 0.002f) * 0.1f;
                    
                    Color baseColor = Color.Lerp(
                        new Color(5, 0, 5), // Near black with slight purple
                        new Color((int)(120 + 20 * colorShift), (int)(20 + 10 * colorShift), (int)(30 + 15 * colorShift)), // Animated scarlet red
                        gradientFactor
                    );
                    
                    // Apply pulse intensity for breathing effect
                    baseColor = Color.Lerp(baseColor, baseColor * 1.3f, pulseIntensity - 0.85f);
                    
                    // Apply flash effect
                    if (flashIntensity > 0f)
                    {
                        baseColor = Color.Lerp(baseColor, new Color(255, 220, 180), flashIntensity * 0.5f);
                    }
                    
                    Color finalColor = baseColor * intensity;
                    
                    Rectangle rect = new Rectangle(0, y, Main.screenWidth, 4);
                    spriteBatch.Draw(pixel, rect, finalColor);
                }
                
                // Draw background particles
                DrawBackgroundParticles(spriteBatch, pixel);
                
                // Draw vignette effect for cinematic feel
                DrawVignette(spriteBatch, pixel);
            }
        }
        
        private void DrawBackgroundParticles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var particle in backgroundParticles)
            {
                Vector2 drawPos = particle.Position - Main.screenPosition;
                
                // Skip if off screen
                if (drawPos.X < -50 || drawPos.X > Main.screenWidth + 50 ||
                    drawPos.Y < -50 || drawPos.Y > Main.screenHeight + 50)
                    continue;
                
                Color drawColor = particle.ParticleColor * particle.Opacity * intensity;
                
                switch (particle.Type)
                {
                    case 0: // Ember - draw as soft glow
                        DrawSoftGlow(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 4f);
                        break;
                        
                    case 1: // Sparkle - draw as cross
                        DrawSparkle(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 8f, particle.Rotation);
                        break;
                        
                    case 2: // Dust mote - draw as small square
                        Rectangle dustRect = new Rectangle((int)(drawPos.X - particle.Scale), (int)(drawPos.Y - particle.Scale), 
                            (int)(particle.Scale * 2), (int)(particle.Scale * 2));
                        spriteBatch.Draw(pixel, dustRect, drawColor * 0.7f);
                        break;
                }
            }
        }
        
        private void DrawSoftGlow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size)
        {
            // Draw multiple layers for soft glow effect
            for (int i = 3; i >= 0; i--)
            {
                float layerSize = size * (1f + i * 0.5f);
                float layerOpacity = 1f / (i + 1);
                Rectangle rect = new Rectangle((int)(center.X - layerSize / 2), (int)(center.Y - layerSize / 2), 
                    (int)layerSize, (int)layerSize);
                spriteBatch.Draw(pixel, rect, color * layerOpacity * 0.3f);
            }
        }
        
        private void DrawSparkle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size, float rotation)
        {
            // Draw 4-pointed star
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation + MathHelper.PiOver2 * i;
                Vector2 direction = angle.ToRotationVector2();
                
                // Elongated diamond shape
                float length = (i % 2 == 0) ? size : size * 0.5f;
                Vector2 start = center - direction * length * 0.5f;
                
                for (int j = 0; j < (int)length; j++)
                {
                    float progress = j / length;
                    float thickness = (1f - Math.Abs(progress - 0.5f) * 2f) * 2f;
                    Vector2 pos = start + direction * j;
                    Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)thickness, (int)thickness);
                    spriteBatch.Draw(pixel, rect, color * (1f - Math.Abs(progress - 0.5f) * 1.5f));
                }
            }
        }
        
        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Draw dark vignette around screen edges for cinematic effect
            int vignetteSize = 200;
            float vignetteStrength = 0.4f * intensity;
            
            // Top and bottom
            for (int i = 0; i < vignetteSize; i++)
            {
                float opacity = (1f - (float)i / vignetteSize) * vignetteStrength;
                Color vignetteColor = Color.Black * opacity;
                
                // Top
                spriteBatch.Draw(pixel, new Rectangle(0, i, Main.screenWidth, 1), vignetteColor);
                // Bottom
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - i - 1, Main.screenWidth, 1), vignetteColor);
            }
            
            // Left and right
            for (int i = 0; i < vignetteSize; i++)
            {
                float opacity = (1f - (float)i / vignetteSize) * vignetteStrength * 0.7f;
                Color vignetteColor = Color.Black * opacity;
                
                // Left
                spriteBatch.Draw(pixel, new Rectangle(i, 0, 1, Main.screenHeight), vignetteColor);
                // Right
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - i - 1, 0, 1, Main.screenHeight), vignetteColor);
            }
        }

        public override float GetCloudAlpha()
        {
            return 1f - intensity * 0.9f; // Further reduce cloud visibility
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            backgroundParticles.Clear();
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
            pulseIntensity = 0f;
            pulseTimer = 0f;
            flashIntensity = 0f;
            backgroundParticles.Clear();
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }
    }
    
    /// <summary>
    /// ModSystem that manages the Eroica sky effect and spawns golden flare particles.
    /// Enhanced with Calamity-style visual effects and new particle system integration.
    /// </summary>
    public class EroicaSkySystem : ModSystem
    {
        private static bool skyRegistered = false;
        private static bool lastEroicaActive = false;
        private static EroicaSkyEffect skyEffectInstance;
        private static int bossAttackFlashCooldown = 0;
        
        // Intensity scaling based on boss health
        private static float bossHealthFactor = 1f;
        
        public override void Load()
        {
            if (!Main.dedServ)
            {
                // Register the sky effect and store instance
                skyEffectInstance = new EroicaSkyEffect();
                SkyManager.Instance["MagnumOpus:EroicaSky"] = skyEffectInstance;
                skyRegistered = true;
            }
        }
        
        public override void Unload()
        {
            skyRegistered = false;
            skyEffectInstance = null;
        }

        public override void PostUpdateWorld()
        {
            if (Main.dedServ || !skyRegistered) return;
            
            // Decay cooldowns
            if (bossAttackFlashCooldown > 0)
                bossAttackFlashCooldown--;
            
            bool eroicaActive = IsEroicaActive(out NPC boss);
            
            // Update boss health factor for intensity scaling
            if (boss != null)
            {
                bossHealthFactor = (float)boss.life / boss.lifeMax;
            }
            
            // Activate/deactivate sky
            if (eroicaActive && !lastEroicaActive)
            {
                SkyManager.Instance.Activate("MagnumOpus:EroicaSky");
            }
            else if (!eroicaActive && lastEroicaActive)
            {
                SkyManager.Instance.Deactivate("MagnumOpus:EroicaSky");
                bossHealthFactor = 1f;
            }
            
            lastEroicaActive = eroicaActive;
            
            // Spawn visual effects during fight
            if (eroicaActive)
            {
                SpawnGoldenFlares();
                SpawnCustomParticles();
                
                // Intensify effects at low health
                if (bossHealthFactor < 0.3f)
                {
                    SpawnEnragedEffects();
                }
            }
        }
        
        /// <summary>
        /// Triggers a screen flash effect. Call this from boss attacks.
        /// </summary>
        public static void TriggerAttackFlash(float strength = 0.8f)
        {
            if (bossAttackFlashCooldown <= 0 && skyEffectInstance != null)
            {
                skyEffectInstance.TriggerFlash(strength);
                bossAttackFlashCooldown = 10; // Cooldown to prevent flash spam
            }
        }
        
        private bool IsEroicaActive(out NPC boss)
        {
            boss = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<EroicasRetribution>())
                {
                    boss = Main.npc[i];
                    return true;
                }
            }
            return false;
        }
        
        private void SpawnGoldenFlares()
        {
            Player player = Main.LocalPlayer;
            
            // Spawn golden flares and dust across the screen
            // Increased spawn rate at lower boss health
            int spawnChance = bossHealthFactor < 0.5f ? 1 : 2;
            
            if (Main.rand.NextBool(spawnChance))
            {
                float spawnX = player.Center.X + Main.rand.NextFloat(-1500, 1500);
                float spawnY = player.Center.Y - 400 + Main.rand.NextFloat(-400, 400);
                
                Dust flare = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.GoldFlame, 0f, 0f, 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                flare.noGravity = true;
                flare.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 2f));
                flare.fadeIn = 1.5f;
            }
            
            // Occasional larger sparkle
            if (Main.rand.NextBool(4))
            {
                float spawnX = player.Center.X + Main.rand.NextFloat(-1200, 1200);
                float spawnY = player.Center.Y - 300 + Main.rand.NextFloat(-300, 300);
                
                Dust sparkle = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.GoldCoin, 0f, 0f, 0, default, Main.rand.NextFloat(1f, 1.8f));
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // Red dust mixed in - more at low health
            int redChance = bossHealthFactor < 0.3f ? 2 : 5;
            if (Main.rand.NextBool(redChance))
            {
                float spawnX = player.Center.X + Main.rand.NextFloat(-1400, 1400);
                float spawnY = player.Center.Y - 200 + Main.rand.NextFloat(-400, 400);
                
                Dust red = Dust.NewDustDirect(new Vector2(spawnX, spawnY), 1, 1, DustID.CrimsonTorch, 0f, 0f, 150, default, Main.rand.NextFloat(1.2f, 2f));
                red.noGravity = true;
                red.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
            }
        }
        
        private void SpawnCustomParticles()
        {
            // Using new particle system for higher quality effects
            Player player = Main.LocalPlayer;
            
            // Bloom particles - golden energy orbs
            if (Main.rand.NextBool(8))
            {
                Vector2 spawnPos = player.Center + new Vector2(Main.rand.NextFloat(-800, 800), Main.rand.NextFloat(-600, 200));
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.3f, 1.2f));
                Color bloomColor = Color.Lerp(new Color(255, 200, 80), new Color(255, 150, 50), Main.rand.NextFloat());
                
                var bloom = new Particles.BloomParticle(spawnPos, velocity, bloomColor, Main.rand.NextFloat(0.3f, 0.8f), Main.rand.Next(90, 180));
                Particles.MagnumParticleHandler.SpawnParticle(bloom);
            }
            
            // Sparkle particles - twinkling stars
            if (Main.rand.NextBool(12))
            {
                Vector2 spawnPos = player.Center + new Vector2(Main.rand.NextFloat(-1000, 1000), Main.rand.NextFloat(-800, 100));
                Vector2 velocity = Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color sparkleColor = Color.Lerp(Color.Gold, Color.White, Main.rand.NextFloat(0.3f, 0.7f));
                
                var sparkle = new Particles.SparkleParticle(spawnPos, velocity, sparkleColor, Main.rand.NextFloat(0.5f, 1.2f), Main.rand.Next(60, 120));
                Particles.MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }
        
        private void SpawnEnragedEffects()
        {
            Player player = Main.LocalPlayer;
            
            // More intense red particles at low health
            if (Main.rand.NextBool(3))
            {
                Vector2 spawnPos = player.Center + new Vector2(Main.rand.NextFloat(-600, 600), Main.rand.NextFloat(-400, 200));
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 1.5f));
                Color glowColor = Color.Lerp(new Color(200, 50, 50), new Color(255, 100, 80), Main.rand.NextFloat());
                
                var glow = new Particles.GlowSparkParticle(spawnPos, velocity, glowColor, Main.rand.NextFloat(0.4f, 1f), Main.rand.Next(40, 80));
                Particles.MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Screen shake occasionally at very low health
            if (bossHealthFactor < 0.15f && Main.rand.NextBool(60))
            {
                // Trigger subtle screen shake (if screen shake system exists)
                TriggerAttackFlash(0.3f);
            }
        }
    }
}
