using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom sky background effect for Fate boss fight.
    /// Features void black background with cosmic clouds, stars, and galaxies.
    /// A celestial cosmic void that represents the ultimate theme of destiny.
    /// </summary>
    public class FateSkyEffect : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private float animationTimer = 0f;
        
        // Background particles (stars, cosmic clouds, galaxies)
        private List<CosmicParticle> backgroundParticles = new List<CosmicParticle>();
        private const int MaxBackgroundParticles = 200;
        
        // Static star field (distant stars that don't move much)
        private List<Star> staticStars = new List<Star>();
        private const int MaxStaticStars = 150;
        private bool starsInitialized = false;
        
        // Galaxies (large spiral shapes)
        private List<Galaxy> galaxies = new List<Galaxy>();
        private const int MaxGalaxies = 5;
        
        // Cosmic nebula clouds
        private float nebulaPhase = 0f;
        
        // Flash effect for boss attacks
        private float flashIntensity = 0f;
        private Color flashColor = new Color(180, 50, 100);
        
        // Pulse effect
        private float pulseIntensity = 0f;
        private float pulseTimer = 0f;
        
        // Fate colors
        private static readonly Color FateBlack = new Color(8, 3, 12);
        private static readonly Color FateDarkPurple = new Color(25, 10, 40);
        private static readonly Color FatePurple = new Color(80, 30, 100);
        private static readonly Color FateDarkPink = new Color(140, 50, 90);
        private static readonly Color FateBrightRed = new Color(200, 60, 80);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color StarGold = new Color(255, 230, 180);
        
        private struct CosmicParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Opacity;
            public Color ParticleColor;
            public int Type; // 0 = star dust, 1 = cosmic cloud, 2 = glyph, 3 = shooting star
            public int Lifetime;
            public int TimeAlive;
            public float WaveOffset;
        }
        
        private struct Star
        {
            public Vector2 ScreenPosition; // Fixed screen position
            public float Scale;
            public float TwinklePhase;
            public float TwinkleSpeed;
            public Color BaseColor;
            public int Type; // 0 = small, 1 = medium, 2 = large with glow
        }
        
        private struct Galaxy
        {
            public Vector2 ScreenPosition;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public Color CoreColor;
            public Color ArmColor;
            public float Opacity;
        }
        
        public override void OnLoad()
        {
            backgroundParticles = new List<CosmicParticle>();
            staticStars = new List<Star>();
            galaxies = new List<Galaxy>();
        }

        public override void Update(GameTime gameTime)
        {
            animationTimer += 0.016f;
            nebulaPhase += 0.008f;
            
            if (isActive && intensity < 1f)
            {
                intensity += 0.006f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.012f;
            }
            
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
            
            // Pulse effect - cosmic breathing
            pulseTimer += 0.018f;
            pulseIntensity = (float)Math.Sin(pulseTimer) * 0.15f + 0.85f;
            
            // Decay flash
            flashIntensity *= 0.92f;
            if (flashIntensity < 0.01f)
                flashIntensity = 0f;
            
            // Initialize static stars once
            if (!starsInitialized && isActive)
            {
                InitializeStaticStars();
                InitializeGalaxies();
                starsInitialized = true;
            }
            
            // Update particles
            UpdateBackgroundParticles();
            UpdateStaticStars();
            UpdateGalaxies();
            
            if (isActive && intensity > 0.15f)
            {
                SpawnBackgroundParticles();
            }
        }
        
        private void InitializeStaticStars()
        {
            staticStars.Clear();
            for (int i = 0; i < MaxStaticStars; i++)
            {
                int starType = Main.rand.Next(10);
                Color starColor;
                float scale;
                
                if (starType < 6) // Small white stars (60%)
                {
                    starColor = Color.Lerp(FateWhite, StarGold, Main.rand.NextFloat(0.3f));
                    scale = Main.rand.NextFloat(0.5f, 1.2f);
                    starType = 0;
                }
                else if (starType < 9) // Medium colored stars (30%)
                {
                    starColor = Color.Lerp(FateDarkPink, FatePurple, Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(1.0f, 2.0f);
                    starType = 1;
                }
                else // Large bright stars with glow (10%)
                {
                    starColor = Color.Lerp(FateWhite, FateBrightRed, Main.rand.NextFloat(0.5f));
                    scale = Main.rand.NextFloat(2.0f, 3.5f);
                    starType = 2;
                }
                
                staticStars.Add(new Star
                {
                    ScreenPosition = new Vector2(Main.rand.NextFloat(Main.screenWidth), Main.rand.NextFloat(Main.screenHeight)),
                    Scale = scale,
                    TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                    TwinkleSpeed = Main.rand.NextFloat(0.02f, 0.08f),
                    BaseColor = starColor,
                    Type = starType
                });
            }
        }
        
        private void InitializeGalaxies()
        {
            galaxies.Clear();
            for (int i = 0; i < MaxGalaxies; i++)
            {
                galaxies.Add(new Galaxy
                {
                    ScreenPosition = new Vector2(
                        Main.rand.NextFloat(Main.screenWidth * 0.1f, Main.screenWidth * 0.9f),
                        Main.rand.NextFloat(Main.screenHeight * 0.1f, Main.screenHeight * 0.8f)
                    ),
                    Scale = Main.rand.NextFloat(40f, 100f),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.003f, 0.003f),
                    CoreColor = Color.Lerp(FateWhite, FateDarkPink, Main.rand.NextFloat(0.5f)),
                    ArmColor = Color.Lerp(FatePurple, FateDarkPink, Main.rand.NextFloat()),
                    Opacity = Main.rand.NextFloat(0.15f, 0.35f)
                });
            }
        }
        
        private void UpdateStaticStars()
        {
            for (int i = 0; i < staticStars.Count; i++)
            {
                var star = staticStars[i];
                star.TwinklePhase += star.TwinkleSpeed;
                staticStars[i] = star;
            }
        }
        
        private void UpdateGalaxies()
        {
            for (int i = 0; i < galaxies.Count; i++)
            {
                var galaxy = galaxies[i];
                galaxy.Rotation += galaxy.RotationSpeed;
                galaxies[i] = galaxy;
            }
        }
        
        private void UpdateBackgroundParticles()
        {
            for (int i = backgroundParticles.Count - 1; i >= 0; i--)
            {
                var particle = backgroundParticles[i];
                
                // Gentle drift
                float waveX = (float)Math.Sin(animationTimer * 1.2f + particle.WaveOffset) * 0.3f;
                float waveY = (float)Math.Cos(animationTimer * 0.8f + particle.WaveOffset) * 0.2f;
                particle.Position += particle.Velocity + new Vector2(waveX, waveY);
                particle.Rotation += particle.RotationSpeed;
                particle.TimeAlive++;
                
                float lifeProgress = (float)particle.TimeAlive / particle.Lifetime;
                if (lifeProgress < 0.2f)
                    particle.Opacity = lifeProgress / 0.2f;
                else if (lifeProgress > 0.7f)
                    particle.Opacity = 1f - (lifeProgress - 0.7f) / 0.3f;
                
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
                
            if (Main.rand.NextBool(4))
            {
                float spawnX = Main.rand.NextFloat(-50, Main.screenWidth + 50);
                float spawnY = Main.rand.NextFloat(-50, Main.screenHeight + 50);
                
                int particleType = Main.rand.Next(12);
                Color particleColor;
                float scale;
                Vector2 velocity;
                int type;
                
                if (particleType < 5) // Star dust (42%)
                {
                    particleColor = Color.Lerp(FateWhite, StarGold, Main.rand.NextFloat(0.4f));
                    scale = Main.rand.NextFloat(0.3f, 0.8f);
                    velocity = Main.rand.NextVector2Circular(0.2f, 0.2f);
                    type = 0;
                }
                else if (particleType < 9) // Cosmic cloud wisps (33%)
                {
                    particleColor = Color.Lerp(FateDarkPurple, FatePurple, Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(1.5f, 4f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.2f));
                    type = 1;
                }
                else if (particleType < 11) // Glyphs (17%)
                {
                    particleColor = Color.Lerp(FateDarkPink, FateBrightRed, Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(0.5f, 1.2f);
                    velocity = Main.rand.NextVector2Circular(0.15f, 0.15f);
                    type = 2;
                }
                else // Shooting star (8%)
                {
                    particleColor = FateWhite;
                    scale = Main.rand.NextFloat(0.4f, 0.8f);
                    velocity = new Vector2(Main.rand.NextFloat(2f, 5f), Main.rand.NextFloat(1f, 3f));
                    type = 3;
                    spawnX = -50;
                    spawnY = Main.rand.NextFloat(Main.screenHeight * 0.5f);
                }
                
                backgroundParticles.Add(new CosmicParticle
                {
                    Position = new Vector2(spawnX, spawnY),
                    Velocity = velocity,
                    Scale = scale,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f),
                    Opacity = 0f,
                    ParticleColor = particleColor,
                    Type = type,
                    Lifetime = type == 3 ? Main.rand.Next(60, 120) : Main.rand.Next(300, 600),
                    TimeAlive = 0,
                    WaveOffset = Main.rand.NextFloat(MathHelper.TwoPi)
                });
            }
        }
        
        public void TriggerFlash(float strength = 1f, Color? color = null)
        {
            flashIntensity = Math.Max(flashIntensity, strength);
            flashColor = color ?? FateDarkPink;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                
                // Draw void black base with subtle cosmic gradient
                DrawCosmicVoidBackground(spriteBatch, pixel);
                
                // Draw galaxies (background layer)
                DrawGalaxies(spriteBatch, pixel);
                
                // Draw static star field
                DrawStaticStars(spriteBatch, pixel);
                
                // Draw animated cosmic particles
                DrawBackgroundParticles(spriteBatch, pixel);
                
                // Draw cosmic nebula overlay
                DrawNebulaOverlay(spriteBatch, pixel);
                
                // Draw vignette
                DrawVignette(spriteBatch, pixel);
                
                // Draw flash overlay
                if (flashIntensity > 0f)
                {
                    Rectangle fullScreen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                    spriteBatch.Draw(pixel, fullScreen, flashColor * flashIntensity * 0.35f);
                }
            }
        }
        
        private void DrawCosmicVoidBackground(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Deep void black with subtle purple gradient
            for (int y = 0; y < Main.screenHeight; y += 4)
            {
                float gradientFactor = (float)y / Main.screenHeight;
                
                // Subtle wave in the void
                float voidWave = (float)Math.Sin(nebulaPhase * 0.5f + y * 0.003f) * 0.1f;
                
                Color baseColor;
                if (gradientFactor < 0.4f)
                {
                    // Top: pure void black
                    float t = gradientFactor / 0.4f;
                    baseColor = Color.Lerp(FateBlack, new Color(12, 5, 18), t);
                }
                else if (gradientFactor < 0.7f)
                {
                    // Middle: hint of dark purple
                    float t = (gradientFactor - 0.4f) / 0.3f;
                    baseColor = Color.Lerp(new Color(12, 5, 18), new Color(18, 8, 28), t);
                }
                else
                {
                    // Bottom: slightly lighter void
                    float t = (gradientFactor - 0.7f) / 0.3f;
                    baseColor = Color.Lerp(new Color(18, 8, 28), new Color(15, 6, 22), t);
                }
                
                // Apply subtle void wave
                baseColor = Color.Lerp(baseColor, FateDarkPurple * 0.3f, voidWave * 0.5f + 0.5f);
                
                Color finalColor = baseColor * intensity;
                Rectangle rect = new Rectangle(0, y, Main.screenWidth, 4);
                spriteBatch.Draw(pixel, rect, finalColor);
            }
        }
        
        private void DrawGalaxies(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var galaxy in galaxies)
            {
                DrawSpiral(spriteBatch, pixel, galaxy.ScreenPosition, galaxy.Scale, galaxy.Rotation, 
                    galaxy.CoreColor, galaxy.ArmColor, galaxy.Opacity * intensity * pulseIntensity);
            }
        }
        
        private void DrawSpiral(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float size, float rotation, Color coreColor, Color armColor, float opacity)
        {
            // Draw soft core glow
            for (int i = 5; i >= 0; i--)
            {
                float coreSize = size * 0.15f * (1f + i * 0.5f);
                float coreOpacity = 1f / (i + 1.5f);
                Rectangle coreRect = new Rectangle(
                    (int)(center.X - coreSize / 2),
                    (int)(center.Y - coreSize / 2),
                    (int)coreSize, (int)coreSize);
                spriteBatch.Draw(pixel, coreRect, coreColor * coreOpacity * opacity * 0.4f);
            }
            
            // Draw spiral arms
            int armCount = 2;
            int pointsPerArm = 30;
            
            for (int arm = 0; arm < armCount; arm++)
            {
                float armOffset = arm * MathHelper.Pi;
                
                for (int p = 0; p < pointsPerArm; p++)
                {
                    float t = (float)p / pointsPerArm;
                    float spiralAngle = rotation + armOffset + t * MathHelper.TwoPi * 1.5f;
                    float radius = t * size;
                    
                    Vector2 point = center + spiralAngle.ToRotationVector2() * radius;
                    
                    float pointOpacity = (1f - t) * opacity * 0.5f;
                    float pointSize = (1f - t * 0.7f) * 4f;
                    
                    Color pointColor = Color.Lerp(coreColor, armColor, t);
                    
                    Rectangle pointRect = new Rectangle(
                        (int)(point.X - pointSize / 2),
                        (int)(point.Y - pointSize / 2),
                        (int)Math.Max(1, pointSize), (int)Math.Max(1, pointSize));
                    spriteBatch.Draw(pixel, pointRect, pointColor * pointOpacity);
                }
            }
        }
        
        private void DrawStaticStars(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var star in staticStars)
            {
                float twinkle = (float)Math.Sin(star.TwinklePhase) * 0.3f + 0.7f;
                float finalOpacity = twinkle * intensity * pulseIntensity;
                
                Vector2 pos = star.ScreenPosition;
                Color starColor = star.BaseColor * finalOpacity;
                
                switch (star.Type)
                {
                    case 0: // Small star - single point
                        DrawPoint(spriteBatch, pixel, pos, starColor, star.Scale);
                        break;
                        
                    case 1: // Medium star - small cross
                        DrawSmallCross(spriteBatch, pixel, pos, starColor, star.Scale);
                        break;
                        
                    case 2: // Large star with glow
                        DrawStarWithGlow(spriteBatch, pixel, pos, starColor, star.Scale);
                        break;
                }
            }
        }
        
        private void DrawPoint(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, Color color, float size)
        {
            Rectangle rect = new Rectangle((int)(pos.X - size / 2), (int)(pos.Y - size / 2), 
                (int)Math.Max(1, size), (int)Math.Max(1, size));
            spriteBatch.Draw(pixel, rect, color);
        }
        
        private void DrawSmallCross(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, Color color, float size)
        {
            // Horizontal
            spriteBatch.Draw(pixel, new Rectangle((int)(pos.X - size), (int)pos.Y, (int)(size * 2), 1), color);
            // Vertical
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)(pos.Y - size * 0.5f), 1, (int)size), color * 0.8f);
        }
        
        private void DrawStarWithGlow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, Color color, float size)
        {
            // Glow layers
            for (int i = 3; i >= 0; i--)
            {
                float glowSize = size * (1f + i * 1.2f);
                float glowOpacity = 1f / (i + 1.5f);
                Rectangle glowRect = new Rectangle(
                    (int)(pos.X - glowSize / 2), (int)(pos.Y - glowSize / 2),
                    (int)glowSize, (int)glowSize);
                spriteBatch.Draw(pixel, glowRect, color * glowOpacity * 0.3f);
            }
            
            // Core
            DrawSmallCross(spriteBatch, pixel, pos, color, size * 0.7f);
        }
        
        private void DrawBackgroundParticles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var particle in backgroundParticles)
            {
                Vector2 drawPos = particle.Position;
                
                if (drawPos.X < -100 || drawPos.X > Main.screenWidth + 100 ||
                    drawPos.Y < -100 || drawPos.Y > Main.screenHeight + 100)
                    continue;
                
                Color drawColor = particle.ParticleColor * particle.Opacity * intensity;
                
                switch (particle.Type)
                {
                    case 0: // Star dust
                        DrawPoint(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 2f);
                        break;
                        
                    case 1: // Cosmic cloud
                        DrawCosmicCloud(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 10f);
                        break;
                        
                    case 2: // Glyph-like shape
                        DrawGlyphShape(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 8f, particle.Rotation);
                        break;
                        
                    case 3: // Shooting star
                        DrawShootingStar(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 15f, particle.Velocity);
                        break;
                }
            }
        }
        
        private void DrawCosmicCloud(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size)
        {
            for (int i = 4; i >= 0; i--)
            {
                float layerSize = size * (1f + i * 0.4f);
                float layerOpacity = 1f / (i + 2f);
                Rectangle rect = new Rectangle(
                    (int)(center.X - layerSize / 2), (int)(center.Y - layerSize * 0.4f),
                    (int)layerSize, (int)(layerSize * 0.6f));
                spriteBatch.Draw(pixel, rect, color * layerOpacity * 0.2f);
            }
        }
        
        private void DrawGlyphShape(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size, float rotation)
        {
            // Simple geometric glyph - triangle-ish
            for (int i = 0; i < 3; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / 3f;
                Vector2 point = center + angle.ToRotationVector2() * size * 0.5f;
                DrawPoint(spriteBatch, pixel, point, color, 2f);
                
                // Lines to center
                Vector2 toCenter = center - point;
                int steps = (int)(toCenter.Length() / 2f);
                for (int s = 0; s < steps; s++)
                {
                    float t = (float)s / steps;
                    Vector2 linePos = Vector2.Lerp(point, center, t);
                    DrawPoint(spriteBatch, pixel, linePos, color * (1f - t * 0.5f), 1f);
                }
            }
        }
        
        private void DrawShootingStar(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, Color color, float size, Vector2 velocity)
        {
            // Head
            DrawStarWithGlow(spriteBatch, pixel, pos, color, size * 0.15f);
            
            // Trail
            Vector2 trailDir = -velocity.SafeNormalize(Vector2.UnitX);
            int trailLength = (int)(size * 1.5f);
            for (int i = 0; i < trailLength; i++)
            {
                float t = (float)i / trailLength;
                Vector2 trailPos = pos + trailDir * i * 1.5f;
                float trailOpacity = (1f - t) * 0.7f;
                float trailSize = (1f - t) * 2f;
                DrawPoint(spriteBatch, pixel, trailPos, color * trailOpacity, trailSize);
            }
        }
        
        private void DrawNebulaOverlay(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Subtle nebula clouds across screen
            int cloudCount = 8;
            for (int c = 0; c < cloudCount; c++)
            {
                float cloudPhase = nebulaPhase + c * 0.8f;
                float cloudX = (float)Math.Sin(cloudPhase * 0.3f + c) * Main.screenWidth * 0.3f + Main.screenWidth * 0.5f;
                float cloudY = (float)Math.Cos(cloudPhase * 0.2f + c * 0.5f) * Main.screenHeight * 0.25f + Main.screenHeight * (0.3f + c * 0.08f);
                
                float cloudSize = 150f + (float)Math.Sin(cloudPhase + c) * 50f;
                float cloudOpacity = 0.08f * intensity * pulseIntensity;
                
                Color cloudColor = Color.Lerp(FatePurple, FateDarkPink, (float)Math.Sin(cloudPhase * 0.5f) * 0.5f + 0.5f);
                
                DrawCosmicCloud(spriteBatch, pixel, new Vector2(cloudX, cloudY), cloudColor, cloudSize);
            }
        }
        
        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel)
        {
            int vignetteSize = 300;
            float vignetteStrength = 0.6f * intensity;
            
            for (int i = 0; i < vignetteSize; i++)
            {
                float opacity = (1f - (float)i / vignetteSize) * vignetteStrength;
                Color vignetteColor = FateBlack * opacity;
                
                // All sides
                spriteBatch.Draw(pixel, new Rectangle(0, i, Main.screenWidth, 1), vignetteColor * 0.7f);
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - i - 1, Main.screenWidth, 1), vignetteColor * 0.8f);
                spriteBatch.Draw(pixel, new Rectangle(i, 0, 1, Main.screenHeight), vignetteColor * 0.5f);
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - i - 1, 0, 1, Main.screenHeight), vignetteColor * 0.5f);
            }
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
            if (!starsInitialized)
            {
                InitializeStaticStars();
                InitializeGalaxies();
                starsInitialized = true;
            }
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
            starsInitialized = false;
            staticStars.Clear();
            galaxies.Clear();
            backgroundParticles.Clear();
        }

        public override bool IsActive() => isActive || intensity > 0f;
    }
    
    /// <summary>
    /// ModSystem to register the Fate sky effect.
    /// </summary>
    public class FateSkyEffectLoader : ModSystem
    {
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance["MagnumOpus:FateSky"] = new FateSkyEffect();
            }
        }
    }
}
