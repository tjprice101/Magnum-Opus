using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom sky background effect for La Campanella boss fight.
    /// Features massive black and orange wavy heat distortion effect,
    /// Yharon-style OnTileColor tinting, health-driven intensity,
    /// distance-based falloff, and attack flash integration.
    /// </summary>
    public class LaCampanellaSkyEffect : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private float animationTimer = 0f;
        
        // Background ember particles
        private List<InfernalParticle> backgroundParticles = new List<InfernalParticle>();
        private const int MaxBackgroundParticles = 200;
        
        // Heat wave distortion parameters
        private float heatWavePhase = 0f;
        private float heatIntensity = 0f;
        
        // Flash effect for boss attacks
        private float flashIntensity = 0f;
        private Color flashColor = Color.Orange;
        
        // Pulse effect for dramatic moments
        private float pulseIntensity = 0f;
        private float pulseTimer = 0f;
        
        // Boss state tracking — Yharon-style health-driven intensity
        private float bossLifeRatio = 1f;
        private Vector2 bossCenter = Vector2.Zero;
        private bool bossIsEnraged = false;
        
        private struct InfernalParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Opacity;
            public Color ParticleColor;
            public int Type; // 0 = ember, 1 = spark, 2 = smoke wisp, 3 = bell shard
            public int Lifetime;
            public int TimeAlive;
            public float WaveOffset; // For wavy motion
        }
        
        public override void OnLoad()
        {
            backgroundParticles = new List<InfernalParticle>();
        }
        
        /// <summary>
        /// Feed boss state each frame for health-driven visuals.
        /// </summary>
        public void UpdateBossState(float lifeRatio, Vector2 center, bool enraged)
        {
            bossLifeRatio = lifeRatio;
            bossCenter = center;
            bossIsEnraged = enraged;
        }
        
        /// <summary>
        /// Yharon-style distance-based falloff — effect weakens far from boss.
        /// </summary>
        private float GetEffectiveIntensity()
        {
            if (bossCenter == Vector2.Zero) return intensity;
            float dist = Vector2.Distance(Main.LocalPlayer.Center, bossCenter);
            float distanceFade = 1f - MathHelper.SmoothStep(2500f, 5500f, dist);
            return intensity * distanceFade;
        }
        
        /// <summary>
        /// Yharon-style world lighting tint — infernal orange/crimson based on HP.
        /// </summary>
        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            if (eff <= 0f) return inColor;
            
            // High HP: warm amber tint. Low HP: deep crimson suffocation.
            float hpDrive = 1f - bossLifeRatio;
            Color tintHigh = new Color(255, 180, 100); // warm amber
            Color tintLow = new Color(200, 60, 20);    // deep crimson
            Color tint = Color.Lerp(tintHigh, tintLow, hpDrive);
            
            float tintStrength = eff * MathHelper.Lerp(0.15f, 0.4f, hpDrive);
            if (bossIsEnraged) tintStrength *= 1.4f;
            
            return Color.Lerp(inColor, tint, tintStrength);
        }

        public override void Update(GameTime gameTime)
        {
            animationTimer += 0.016f;
            heatWavePhase += 0.03f;
            
            if (isActive && intensity < 1f)
            {
                intensity += 0.008f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.015f;
            }
            
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
            
            // Heat intensity scales with boss HP loss
            float hpDrive = 1f - bossLifeRatio;
            heatIntensity = intensity * MathHelper.Lerp(0.6f, 1.3f, hpDrive);
            
            // Pulse speeds up as HP drops
            pulseTimer += MathHelper.Lerp(0.02f, 0.04f, hpDrive);
            pulseIntensity = (float)Math.Sin(pulseTimer) * 0.2f + 0.8f;
            
            // Decay flash intensity
            flashIntensity *= 0.88f;
            if (flashIntensity < 0.01f)
                flashIntensity = 0f;
            
            // Update and spawn background particles
            UpdateBackgroundParticles();
            
            if (isActive && intensity > 0.2f)
            {
                SpawnBackgroundParticles();
            }
        }
        
        private void UpdateBackgroundParticles()
        {
            for (int i = backgroundParticles.Count - 1; i >= 0; i--)
            {
                var particle = backgroundParticles[i];
                
                // Apply wavy motion to simulate heat distortion
                float waveX = (float)Math.Sin(animationTimer * 2f + particle.WaveOffset) * 0.5f;
                particle.Position += particle.Velocity + new Vector2(waveX, 0);
                particle.Rotation += particle.RotationSpeed;
                particle.TimeAlive++;
                
                // Fade based on lifetime
                float lifeProgress = (float)particle.TimeAlive / particle.Lifetime;
                if (lifeProgress < 0.15f)
                    particle.Opacity = lifeProgress / 0.15f;
                else if (lifeProgress > 0.6f)
                    particle.Opacity = 1f - (lifeProgress - 0.6f) / 0.4f;
                
                backgroundParticles[i] = particle;
                
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
                
            int spawnChance = (int)MathHelper.Lerp(6, 2, intensity);
            if (Main.rand.NextBool(spawnChance))
            {
                float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-100, Main.screenWidth + 100);
                float spawnY = Main.screenPosition.Y + Main.screenHeight + 50; // Spawn below screen
                
                int particleType = Main.rand.Next(10);
                Color particleColor;
                float scale;
                Vector2 velocity;
                
                if (particleType < 4) // Ember (40%)
                {
                    particleColor = Color.Lerp(new Color(255, 120, 20), new Color(255, 80, 0), Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(0.8f, 2.0f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-3f, -1.5f));
                }
                else if (particleType < 7) // Dark smoke (30%)
                {
                    particleColor = Color.Lerp(new Color(30, 20, 25), new Color(60, 40, 50), Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(1.5f, 4f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-2f, -0.8f));
                }
                else if (particleType < 9) // Orange spark (20%)
                {
                    particleColor = Color.Lerp(new Color(255, 180, 50), new Color(255, 220, 100), Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(0.4f, 1.0f);
                    velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-4f, -2f));
                }
                else // Yellow-white core (10%)
                {
                    particleColor = Color.Lerp(new Color(255, 240, 200), new Color(255, 255, 220), Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(0.3f, 0.7f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-2.5f, -1f));
                }
                
                backgroundParticles.Add(new InfernalParticle
                {
                    Position = new Vector2(spawnX, spawnY),
                    Velocity = velocity,
                    Scale = scale,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                    Opacity = 0f,
                    ParticleColor = particleColor,
                    Type = particleType < 4 ? 0 : (particleType < 7 ? 2 : 1),
                    Lifetime = Main.rand.Next(200, 500),
                    TimeAlive = 0,
                    WaveOffset = Main.rand.NextFloat(MathHelper.TwoPi)
                });
            }
        }
        
        /// <summary>
        /// Triggers a flash effect. Call from boss attacks for dramatic emphasis.
        /// </summary>
        public void TriggerFlash(float strength = 1f, Color? color = null)
        {
            flashIntensity = Math.Max(flashIntensity, strength);
            flashColor = color ?? Color.Orange;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                Texture2D pixel = MagnumTextureRegistry.GetPixelTexture();
                if (pixel == null) return;
                
                // Draw wavy heat distortion gradient background
                DrawHeatDistortionBackground(spriteBatch, pixel);
                
                // Draw background particles
                DrawBackgroundParticles(spriteBatch, pixel);
                
                // Draw vignette effect
                DrawVignette(spriteBatch, pixel);
                
                // Draw flash overlay
                if (flashIntensity > 0f)
                {
                    Rectangle fullScreen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                    spriteBatch.Draw(pixel, fullScreen, flashColor * flashIntensity * 0.4f);
                }
            }
        }
        
        private void DrawHeatDistortionBackground(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Draw gradient with animated heat wave distortion
            for (int y = 0; y < Main.screenHeight; y += 3)
            {
                // Calculate heat wave offset for this row
                float waveOffset1 = (float)Math.Sin(heatWavePhase + y * 0.008f) * 15f * heatIntensity;
                float waveOffset2 = (float)Math.Sin(heatWavePhase * 1.3f + y * 0.012f) * 10f * heatIntensity;
                float waveOffset3 = (float)Math.Sin(heatWavePhase * 0.7f + y * 0.005f) * 8f * heatIntensity;
                float totalWave = waveOffset1 + waveOffset2 + waveOffset3;
                
                // Gradient from black at top to dark orange at bottom
                float gradientFactor = (float)y / Main.screenHeight;
                
                // Add wave influence to color
                float colorWave = (float)Math.Sin(heatWavePhase * 0.8f + y * 0.01f) * 0.15f;
                
                // Base colors: black -> dark gray -> dark orange -> orange
                Color baseColor;
                if (gradientFactor < 0.3f)
                {
                    // Top: pure black to dark gray
                    float t = gradientFactor / 0.3f;
                    baseColor = Color.Lerp(
                        new Color(5, 3, 8),
                        new Color(25, 18, 28),
                        t
                    );
                }
                else if (gradientFactor < 0.6f)
                {
                    // Middle: dark gray to dark orange with heat shimmer
                    float t = (gradientFactor - 0.3f) / 0.3f;
                    baseColor = Color.Lerp(
                        new Color(25, 18, 28),
                        new Color((int)(80 + colorWave * 40), (int)(35 + colorWave * 20), 15),
                        t
                    );
                }
                else
                {
                    // Bottom: dark orange to orange glow
                    float t = (gradientFactor - 0.6f) / 0.4f;
                    baseColor = Color.Lerp(
                        new Color((int)(80 + colorWave * 40), (int)(35 + colorWave * 20), 15),
                        new Color((int)(160 + colorWave * 60), (int)(70 + colorWave * 30), 20),
                        t
                    );
                }
                
                // Apply pulse for breathing effect
                baseColor = Color.Lerp(baseColor, baseColor * 1.2f, (pulseIntensity - 0.8f) * 2f);
                
                Color finalColor = baseColor * intensity;
                
                // Draw the row with horizontal wave offset for heat shimmer
                int drawX = (int)totalWave;
                Rectangle rect = new Rectangle(drawX - 20, y, Main.screenWidth + 40, 3);
                spriteBatch.Draw(pixel, rect, finalColor);
            }
        }
        
        private void DrawBackgroundParticles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var particle in backgroundParticles)
            {
                Vector2 drawPos = particle.Position - Main.screenPosition;
                
                if (drawPos.X < -100 || drawPos.X > Main.screenWidth + 100 ||
                    drawPos.Y < -100 || drawPos.Y > Main.screenHeight + 100)
                    continue;
                
                Color drawColor = particle.ParticleColor * particle.Opacity * intensity;
                
                switch (particle.Type)
                {
                    case 0: // Ember - soft glow
                        DrawSoftGlow(spriteBatch, pixel, drawPos, drawColor, Math.Min(particle.Scale * 5f, 88f));
                        break;
                        
                    case 1: // Spark - cross shape
                        DrawSparkle(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 10f, particle.Rotation);
                        break;
                        
                    case 2: // Smoke wisp - larger soft blob
                        DrawSmokeWisp(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 12f, particle.Rotation);
                        break;
                }
            }
        }
        
        private void DrawSoftGlow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size)
        {
            for (int i = 4; i >= 0; i--)
            {
                float layerSize = size * (1f + i * 0.6f);
                float layerOpacity = 1f / (i + 1.5f);
                Rectangle rect = new Rectangle(
                    (int)(center.X - layerSize / 2), 
                    (int)(center.Y - layerSize / 2), 
                    (int)layerSize, 
                    (int)layerSize);
                spriteBatch.Draw(pixel, rect, color * layerOpacity * 0.35f);
            }
        }
        
        private void DrawSparkle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size, float rotation)
        {
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation + MathHelper.PiOver2 * i;
                Vector2 direction = angle.ToRotationVector2();
                float length = (i % 2 == 0) ? size : size * 0.5f;
                
                for (int j = 0; j < (int)length; j++)
                {
                    float progress = j / length;
                    float thickness = (1f - Math.Abs(progress - 0.5f) * 2f) * 3f;
                    Vector2 pos = center + direction * (j - length / 2);
                    Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)Math.Max(1, thickness), (int)Math.Max(1, thickness));
                    spriteBatch.Draw(pixel, rect, color * (1f - Math.Abs(progress - 0.5f) * 1.2f));
                }
            }
        }
        
        private void DrawSmokeWisp(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size, float rotation)
        {
            // Draw as elongated soft blob
            for (int i = 3; i >= 0; i--)
            {
                float layerSize = size * (1f + i * 0.4f);
                float layerOpacity = 1f / (i + 2f);
                
                // Elongated shape
                float width = layerSize * 1.5f;
                float height = layerSize * 0.8f;
                
                Rectangle rect = new Rectangle(
                    (int)(center.X - width / 2), 
                    (int)(center.Y - height / 2), 
                    (int)width, 
                    (int)height);
                spriteBatch.Draw(pixel, rect, color * layerOpacity * 0.25f);
            }
        }
        
        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel)
        {
            float eff = GetEffectiveIntensity();
            float hpDrive = 1f - bossLifeRatio;
            int vignetteSize = (int)MathHelper.Lerp(200, 350, hpDrive);
            float vignetteStrength = MathHelper.Lerp(0.4f, 0.7f, hpDrive) * eff;
            if (bossIsEnraged) vignetteStrength *= 1.3f;
            
            // Infernal orange tint bleeding in from edges at low HP
            Color vignetteBase = new Color(0, 0, 0);
            Color infernalTint = new Color(80, 30, 5);
            Color vignetteColor = Color.Lerp(vignetteBase, infernalTint, hpDrive * 0.5f);
            
            for (int i = 0; i < vignetteSize; i++)
            {
                float opacity = (1f - (float)i / vignetteSize) * vignetteStrength;
                Color vc = vignetteColor * opacity;
                
                // Top
                spriteBatch.Draw(pixel, new Rectangle(0, i, Main.screenWidth, 1), vc * 0.6f);
                // Bottom — heavier (fire rises)
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - i - 1, Main.screenWidth, 1), vc * 1.2f);
                // Left
                spriteBatch.Draw(pixel, new Rectangle(i, 0, 1, Main.screenHeight), vc * 0.5f);
                // Right
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - i - 1, 0, 1, Main.screenHeight), vc * 0.5f);
            }
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
        }

        public override bool IsActive() => isActive || intensity > 0f;
    }
    
    /// <summary>
    /// ModSystem companion — manages sky activation, boss state feeding,
    /// and provides static flash/particle APIs for attack VFX integration.
    /// </summary>
    public class LaCampanellaSkySystem : ModSystem
    {
        // Boss state for feeding to sky
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; } = Vector2.Zero;
        public static bool BossEnraged { get; set; } = false;
        public static int BossPhaseIndex { get; set; } = 0;
        
        // Flash tracking
        private static float pendingFlashStrength = 0f;
        private static Color pendingFlashColor = Color.Orange;
        
        // Ambient world particle tracking
        private static int ambientTimer = 0;
        
        /// <summary>Trigger an infernal orange flash — bell slam impacts.</summary>
        public static void TriggerInfernalFlash(float strength = 0.6f)
        {
            pendingFlashStrength = Math.Max(pendingFlashStrength, strength);
            pendingFlashColor = new Color(255, 140, 40);
        }
        
        /// <summary>Trigger a white-hot flash — grand finale / phase transition.</summary>
        public static void TriggerWhiteFlash(float strength = 0.8f)
        {
            pendingFlashStrength = Math.Max(pendingFlashStrength, strength);
            pendingFlashColor = new Color(255, 240, 220);
        }
        
        /// <summary>Trigger a crimson flash — enrage attacks.</summary>
        public static void TriggerCrimsonFlash(float strength = 0.5f)
        {
            pendingFlashStrength = Math.Max(pendingFlashStrength, strength);
            pendingFlashColor = new Color(200, 50, 30);
        }
        
        /// <summary>Trigger a custom bell toll flash — visible bell chime effect.</summary>
        public static void TriggerBellTollFlash(float strength = 0.4f)
        {
            pendingFlashStrength = Math.Max(pendingFlashStrength, strength);
            pendingFlashColor = new Color(220, 180, 80);
        }
        
        public override void PostUpdateEverything()
        {
            if (Main.dedServ) return;
            
            var skyObj = SkyManager.Instance["MagnumOpus:LaCampanellaSky"];
            if (skyObj is LaCampanellaSkyEffect sky && sky.IsActive())
            {
                // Feed boss state to sky
                sky.UpdateBossState(BossLifeRatio, BossCenter, BossEnraged);
                
                // Deliver pending flash
                if (pendingFlashStrength > 0f)
                {
                    sky.TriggerFlash(pendingFlashStrength, pendingFlashColor);
                    pendingFlashStrength = 0f;
                }
                
                // Ambient world-space particles near boss
                ambientTimer++;
                if (BossCenter != Vector2.Zero && ambientTimer % 3 == 0)
                {
                    float hpDrive = 1f - BossLifeRatio;
                    int emberChance = (int)MathHelper.Lerp(8, 2, hpDrive);
                    
                    if (Main.rand.NextBool(Math.Max(1, emberChance)))
                    {
                        Vector2 dustPos = BossCenter + Main.rand.NextVector2Circular(300f, 200f);
                        Dust ember = Dust.NewDustDirect(dustPos, 4, 4, Terraria.ID.DustID.Torch, 0f, -2f, 100, default, 1.5f);
                        ember.noGravity = true;
                        ember.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 3f));
                        ember.color = Color.Lerp(new Color(255, 140, 40), new Color(255, 80, 20), Main.rand.NextFloat());
                    }
                    
                    // Heavy smoke below boss at low HP
                    if (hpDrive > 0.4f && Main.rand.NextBool(4))
                    {
                        Vector2 smokePos = BossCenter + new Vector2(Main.rand.NextFloat(-150f, 150f), 60f);
                        Dust smoke = Dust.NewDustDirect(smokePos, 8, 8, Terraria.ID.DustID.Smoke, 0f, -1f, 200, new Color(30, 20, 15), 2f);
                        smoke.noGravity = true;
                        smoke.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 1.5f));
                    }
                    
                    // Bloom particles during enrage
                    if (BossEnraged && ambientTimer % 8 == 0)
                    {
                        Vector2 bloomPos = BossCenter + Main.rand.NextVector2Circular(80f, 80f);
                        Vector2 bloomVel = new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(1f, 0.5f);
                        Color bloomColor = Color.Lerp(new Color(255, 140, 40), new Color(200, 50, 30), Main.rand.NextFloat());
                        MagnumParticleHandler.SpawnParticle(new BloomParticle(bloomPos, bloomVel, bloomColor, 0.4f, 35));
                    }
                }
            }
            else
            {
                ambientTimer = 0;
            }
        }
    }

    /// <summary>
    /// ModSystem to register the La Campanella sky effect.
    /// </summary>
    public class LaCampanellaSkyEffectLoader : ModSystem
    {
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance["MagnumOpus:LaCampanellaSky"] = new LaCampanellaSkyEffect();
            }
        }
    }
}
