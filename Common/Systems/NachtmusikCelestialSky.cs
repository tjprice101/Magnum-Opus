using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Bosses;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// COSMIC PURPLE AND GOLDEN STAR DAZZLING BACKGROUND
    /// A magnificent celestial sky for the Queen of Radiance fight.
    /// Features: Swirling nebula, golden stars, cosmic streaks, phase-based color shifting
    /// </summary>
    public class NachtmusikCelestialSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int animationTimer = 0;
        
        // Phase 2 enhanced visuals
        private bool isPhase2 = false;
        private float phase2Transition = 0f;
        
        // Celestial particles - stars, nebula wisps, cosmic streaks
        private List<CelestialParticle> celestialParticles = new List<CelestialParticle>();
        private const int MaxCelestialParticles = 200;
        
        // Static star field (background constellations)
        private List<StaticStar> starField = new List<StaticStar>();
        private const int StarFieldCount = 80;
        private bool starFieldInitialized = false;
        
        // Swirling nebula effect
        private float nebulaRotation = 0f;
        private float nebulaScale = 1f;
        
        // Lightning/flash effects
        private float flashIntensity = 0f;
        private Color flashColor = Color.White;
        
        // Color cycling for Phase 2
        private float colorCycleTimer = 0f;
        
        // Theme colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);      // #2D1B4E
        private static readonly Color CosmicPurple = new Color(90, 50, 150);
        private static readonly Color Gold = new Color(255, 215, 0);           // #FFD700
        private static readonly Color StarGold = new Color(255, 230, 130);
        private static readonly Color Violet = new Color(123, 104, 238);       // #7B68EE
        private static readonly Color NightBlue = new Color(25, 25, 80);
        private static readonly Color NebulaPink = new Color(200, 100, 180);
        private static readonly Color StarWhite = new Color(255, 250, 245);
        
        private struct CelestialParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Opacity;
            public Color ParticleColor;
            public int Type; // 0 = star twinkle, 1 = nebula wisp, 2 = cosmic streak, 3 = golden flare, 4 = shooting star
            public int Lifetime;
            public int TimeAlive;
            public float TwinklePhase; // For twinkling stars
            public float Depth; // Parallax depth
        }
        
        private struct StaticStar
        {
            public Vector2 BasePosition; // Relative to screen center (0,0 - 1,1)
            public float Scale;
            public float TwinkleOffset;
            public Color BaseColor;
            public float Depth;
        }
        
        public override void OnLoad()
        {
            celestialParticles = new List<CelestialParticle>();
            starField = new List<StaticStar>();
            starFieldInitialized = false;
        }
        
        private void InitializeStarField()
        {
            starField.Clear();
            
            for (int i = 0; i < StarFieldCount; i++)
            {
                // Distribute stars across the screen
                float x = Main.rand.NextFloat(0f, 1f);
                float y = Main.rand.NextFloat(0f, 0.7f); // More stars in upper portion
                
                // Vary star colors between gold, white, and soft purple
                Color starColor;
                float colorRoll = Main.rand.NextFloat();
                if (colorRoll < 0.4f)
                    starColor = Color.Lerp(StarGold, Gold, Main.rand.NextFloat());
                else if (colorRoll < 0.7f)
                    starColor = Color.Lerp(StarWhite, new Color(200, 200, 255), Main.rand.NextFloat());
                else
                    starColor = Color.Lerp(Violet, new Color(180, 150, 220), Main.rand.NextFloat());
                
                starField.Add(new StaticStar
                {
                    BasePosition = new Vector2(x, y),
                    Scale = Main.rand.NextFloat(0.3f, 1.2f),
                    TwinkleOffset = Main.rand.NextFloat(MathHelper.TwoPi),
                    BaseColor = starColor,
                    Depth = Main.rand.NextFloat(0.3f, 1f)
                });
            }
            
            starFieldInitialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            animationTimer++;
            
            if (!starFieldInitialized)
                InitializeStarField();
            
            // Activation/deactivation intensity
            if (isActive && intensity < 1f)
            {
                intensity += 0.015f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.025f;
            }
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
            
            // Phase 2 transition
            if (isPhase2 && phase2Transition < 1f)
            {
                phase2Transition += 0.02f;
            }
            phase2Transition = MathHelper.Clamp(phase2Transition, 0f, 1f);
            
            // Color cycling for Phase 2 FADING EFFECTS
            if (isPhase2)
            {
                colorCycleTimer += 0.03f;
            }
            
            // Nebula rotation
            nebulaRotation += 0.002f * (1f + phase2Transition * 0.5f);
            nebulaScale = 1f + (float)Math.Sin(animationTimer * 0.01f) * 0.1f;
            
            // Flash decay
            flashIntensity *= 0.88f;
            if (flashIntensity < 0.01f)
                flashIntensity = 0f;
            
            // Update particles
            UpdateCelestialParticles();
            
            // Spawn particles
            if (isActive && intensity > 0.2f)
            {
                SpawnCelestialParticles();
            }
        }
        
        private void UpdateCelestialParticles()
        {
            for (int i = celestialParticles.Count - 1; i >= 0; i--)
            {
                var particle = celestialParticles[i];
                particle.Position += particle.Velocity;
                particle.Rotation += particle.RotationSpeed;
                particle.TimeAlive++;
                particle.TwinklePhase += 0.1f;
                
                // Lifetime-based opacity
                float lifeProgress = (float)particle.TimeAlive / particle.Lifetime;
                if (lifeProgress < 0.15f)
                    particle.Opacity = lifeProgress / 0.15f;
                else if (lifeProgress > 0.75f)
                    particle.Opacity = 1f - (lifeProgress - 0.75f) / 0.25f;
                else
                    particle.Opacity = 1f;
                
                // Twinkling for stars
                if (particle.Type == 0 || particle.Type == 3)
                {
                    particle.Opacity *= 0.6f + (float)Math.Sin(particle.TwinklePhase) * 0.4f;
                }
                
                celestialParticles[i] = particle;
                
                if (particle.TimeAlive >= particle.Lifetime || particle.Opacity <= 0f)
                {
                    celestialParticles.RemoveAt(i);
                }
            }
        }
        
        private void SpawnCelestialParticles()
        {
            if (celestialParticles.Count >= MaxCelestialParticles)
                return;
            
            // Spawn rate increases in Phase 2
            int baseSpawnChance = isPhase2 ? 2 : 4;
            
            // === TWINKLING STARS ===
            if (Main.rand.NextBool(baseSpawnChance))
            {
                SpawnTwinklingStar();
            }
            
            // === NEBULA WISPS ===
            if (Main.rand.NextBool(baseSpawnChance + 3))
            {
                SpawnNebulaWisp();
            }
            
            // === COSMIC STREAKS ===
            if (Main.rand.NextBool(isPhase2 ? 5 : 12))
            {
                SpawnCosmicStreak();
            }
            
            // === GOLDEN FLARES ===
            if (Main.rand.NextBool(baseSpawnChance + 2))
            {
                SpawnGoldenFlare();
            }
            
            // === SHOOTING STARS (rare, dramatic) ===
            if (Main.rand.NextBool(isPhase2 ? 30 : 60))
            {
                SpawnShootingStar();
            }
        }
        
        private void SpawnTwinklingStar()
        {
            float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-100, Main.screenWidth + 100);
            float spawnY = Main.screenPosition.Y + Main.rand.NextFloat(-50, Main.screenHeight * 0.6f);
            
            // Phase 2 color cycling
            Color starColor;
            if (isPhase2)
            {
                float hueShift = (float)Math.Sin(colorCycleTimer + Main.rand.NextFloat(MathHelper.TwoPi)) * 0.5f + 0.5f;
                starColor = Color.Lerp(Gold, CosmicPurple, hueShift);
            }
            else
            {
                starColor = Main.rand.NextBool() ? StarGold : StarWhite;
            }
            
            celestialParticles.Add(new CelestialParticle
            {
                Position = new Vector2(spawnX, spawnY),
                Velocity = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(0.05f, 0.2f)),
                Scale = Main.rand.NextFloat(0.4f, 1.2f),
                Rotation = 0f,
                RotationSpeed = 0f,
                Opacity = 0f,
                ParticleColor = starColor,
                Type = 0,
                Lifetime = Main.rand.Next(200, 500),
                TimeAlive = 0,
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                Depth = Main.rand.NextFloat(0.4f, 1f)
            });
        }
        
        private void SpawnNebulaWisp()
        {
            float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-200, Main.screenWidth + 200);
            float spawnY = Main.screenPosition.Y + Main.rand.NextFloat(0, Main.screenHeight * 0.8f);
            
            // Phase 2 fading color effect - cycle through purple, gold, pink
            Color wispColor;
            if (isPhase2)
            {
                float cyclePhase = (colorCycleTimer * 0.3f + Main.rand.NextFloat(MathHelper.TwoPi)) % MathHelper.TwoPi;
                if (cyclePhase < MathHelper.TwoPi / 3f)
                    wispColor = Color.Lerp(CosmicPurple, Gold, (cyclePhase / (MathHelper.TwoPi / 3f)));
                else if (cyclePhase < MathHelper.TwoPi * 2f / 3f)
                    wispColor = Color.Lerp(Gold, NebulaPink, ((cyclePhase - MathHelper.TwoPi / 3f) / (MathHelper.TwoPi / 3f)));
                else
                    wispColor = Color.Lerp(NebulaPink, CosmicPurple, ((cyclePhase - MathHelper.TwoPi * 2f / 3f) / (MathHelper.TwoPi / 3f)));
            }
            else
            {
                wispColor = Main.rand.NextBool(3) ? NebulaPink : Color.Lerp(CosmicPurple, Violet, Main.rand.NextFloat());
            }
            
            celestialParticles.Add(new CelestialParticle
            {
                Position = new Vector2(spawnX, spawnY),
                Velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.2f, 0.3f)),
                Scale = Main.rand.NextFloat(2f, 5f),
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.01f, 0.01f),
                Opacity = 0f,
                ParticleColor = wispColor * 0.4f,
                Type = 1,
                Lifetime = Main.rand.Next(300, 600),
                TimeAlive = 0,
                TwinklePhase = 0f,
                Depth = Main.rand.NextFloat(0.2f, 0.5f)
            });
        }
        
        private void SpawnCosmicStreak()
        {
            float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-50, Main.screenWidth + 50);
            float spawnY = Main.screenPosition.Y + Main.rand.NextFloat(-20, Main.screenHeight * 0.5f);
            
            Color streakColor = isPhase2 
                ? Color.Lerp(Gold, StarWhite, Main.rand.NextFloat()) 
                : Color.Lerp(Violet, StarWhite, Main.rand.NextFloat());
            
            celestialParticles.Add(new CelestialParticle
            {
                Position = new Vector2(spawnX, spawnY),
                Velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 2f)),
                Scale = Main.rand.NextFloat(0.5f, 1.5f),
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f),
                Opacity = 0f,
                ParticleColor = streakColor,
                Type = 2,
                Lifetime = Main.rand.Next(100, 200),
                TimeAlive = 0,
                TwinklePhase = 0f,
                Depth = Main.rand.NextFloat(0.6f, 1f)
            });
        }
        
        private void SpawnGoldenFlare()
        {
            float spawnX = Main.screenPosition.X + Main.rand.NextFloat(0, Main.screenWidth);
            float spawnY = Main.screenPosition.Y + Main.rand.NextFloat(0, Main.screenHeight * 0.7f);
            
            celestialParticles.Add(new CelestialParticle
            {
                Position = new Vector2(spawnX, spawnY),
                Velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, 0.5f)),
                Scale = Main.rand.NextFloat(0.8f, 2f),
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                Opacity = 0f,
                ParticleColor = Color.Lerp(Gold, StarGold, Main.rand.NextFloat()),
                Type = 3,
                Lifetime = Main.rand.Next(150, 350),
                TimeAlive = 0,
                TwinklePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                Depth = Main.rand.NextFloat(0.5f, 0.9f)
            });
        }
        
        private void SpawnShootingStar()
        {
            // Spawn from top-left moving to bottom-right
            float spawnX = Main.screenPosition.X + Main.rand.NextFloat(-100, Main.screenWidth * 0.5f);
            float spawnY = Main.screenPosition.Y - 50f;
            
            float speed = Main.rand.NextFloat(8f, 15f);
            float angle = Main.rand.NextFloat(MathHelper.PiOver4 * 0.5f, MathHelper.PiOver4 * 1.5f);
            
            celestialParticles.Add(new CelestialParticle
            {
                Position = new Vector2(spawnX, spawnY),
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed,
                Scale = Main.rand.NextFloat(1.5f, 3f),
                Rotation = angle,
                RotationSpeed = 0f,
                Opacity = 1f,
                ParticleColor = isPhase2 ? Gold : StarWhite,
                Type = 4,
                Lifetime = Main.rand.Next(40, 80),
                TimeAlive = 0,
                TwinklePhase = 0f,
                Depth = 1f
            });
        }
        
        /// <summary>
        /// Triggers a dramatic flash effect
        /// </summary>
        public void TriggerFlash(float strength = 1f, Color? color = null)
        {
            flashIntensity = Math.Max(flashIntensity, strength);
            flashColor = color ?? StarWhite;
        }
        
        /// <summary>
        /// Activates Phase 2 enhanced visuals - call when boss enters Phase 2
        /// </summary>
        public void ActivatePhase2()
        {
            isPhase2 = true;
        }
        
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (intensity <= 0f) return;
            
            if (maxDepth >= 0 && minDepth < 0)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                
                // === DRAW COSMIC GRADIENT BACKGROUND ===
                DrawCosmicGradient(spriteBatch, pixel);
                
                // === DRAW SWIRLING NEBULA ===
                DrawNebulaEffect(spriteBatch, pixel);
                
                // === DRAW STATIC STAR FIELD ===
                DrawStaticStarField(spriteBatch, pixel);
                
                // === DRAW CELESTIAL PARTICLES ===
                DrawCelestialParticles(spriteBatch, pixel);
                
                // === DRAW FLASH OVERLAY ===
                if (flashIntensity > 0f)
                {
                    Color flashOverlay = flashColor * flashIntensity * intensity * 0.6f;
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashOverlay);
                }
            }
        }
        
        private void DrawCosmicGradient(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Phase 2: Gradient colors pulse and shift
            float pulsePhase = (float)Math.Sin(animationTimer * 0.015f) * 0.15f;
            float colorShift = isPhase2 ? (float)Math.Sin(colorCycleTimer * 0.5f) * 0.3f : 0f;
            
            for (int y = 0; y < Main.screenHeight; y += 3)
            {
                float gradientProgress = (float)y / Main.screenHeight;
                
                // Base colors shift based on phase
                Color bottomColor = NightBlue;
                Color topColor;
                
                if (isPhase2)
                {
                    // Phase 2: Cycling between deep purple, gold tint, and cosmic purple
                    Color phase2TopA = new Color(
                        (int)(70 + 30 * colorShift), 
                        (int)(40 + 20 * Math.Sin(colorCycleTimer * 0.3f)), 
                        (int)(120 + 30 * colorShift)
                    );
                    Color phase2TopB = new Color(
                        (int)(100 + 50 * pulsePhase), 
                        (int)(80 + 40 * pulsePhase), 
                        (int)(40 + 20 * Math.Sin(colorCycleTimer))
                    );
                    topColor = Color.Lerp(phase2TopA, phase2TopB, (float)Math.Sin(colorCycleTimer * 0.2f) * 0.5f + 0.5f);
                }
                else
                {
                    topColor = new Color(
                        (int)(60 + 15 * pulsePhase), 
                        (int)(35 + 10 * pulsePhase), 
                        (int)(100 + 20 * pulsePhase)
                    );
                }
                
                // Gradient from dark bottom to colored top
                Color gradientColor = Color.Lerp(bottomColor, topColor, 1f - gradientProgress);
                gradientColor *= intensity;
                
                spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), gradientColor);
            }
        }
        
        private void DrawNebulaEffect(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Multiple nebula "clouds" at different positions
            int nebulaCount = isPhase2 ? 5 : 3;
            
            for (int n = 0; n < nebulaCount; n++)
            {
                float offsetAngle = nebulaRotation + MathHelper.TwoPi * n / nebulaCount;
                float offsetRadius = 200f + n * 100f;
                
                Vector2 nebulaCenter = new Vector2(
                    Main.screenWidth * 0.5f + (float)Math.Cos(offsetAngle) * offsetRadius * 0.5f,
                    Main.screenHeight * 0.3f + (float)Math.Sin(offsetAngle * 0.7f) * offsetRadius * 0.3f
                );
                
                // Draw soft circular nebula glow
                int glowLayers = 15;
                for (int layer = glowLayers; layer > 0; layer--)
                {
                    float layerProgress = (float)layer / glowLayers;
                    float radius = 80f * nebulaScale * layerProgress * (1f + n * 0.2f);
                    
                    // Color varies by nebula and phase
                    Color nebulaColor;
                    if (isPhase2)
                    {
                        float hue = (colorCycleTimer * 0.1f + n * 0.3f) % 1f;
                        if (hue < 0.33f)
                            nebulaColor = Color.Lerp(CosmicPurple, Gold, hue * 3f);
                        else if (hue < 0.66f)
                            nebulaColor = Color.Lerp(Gold, NebulaPink, (hue - 0.33f) * 3f);
                        else
                            nebulaColor = Color.Lerp(NebulaPink, CosmicPurple, (hue - 0.66f) * 3f);
                    }
                    else
                    {
                        nebulaColor = n % 2 == 0 ? CosmicPurple : NebulaPink;
                    }
                    
                    nebulaColor *= (1f - layerProgress) * 0.08f * intensity;
                    
                    Rectangle nebulaRect = new Rectangle(
                        (int)(nebulaCenter.X - radius),
                        (int)(nebulaCenter.Y - radius),
                        (int)(radius * 2),
                        (int)(radius * 2)
                    );
                    
                    spriteBatch.Draw(pixel, nebulaRect, nebulaColor);
                }
            }
        }
        
        private void DrawStaticStarField(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var star in starField)
            {
                // Calculate star position with parallax
                float parallaxOffset = (float)Math.Sin(animationTimer * 0.005f * star.Depth) * 5f * star.Depth;
                Vector2 starPos = new Vector2(
                    star.BasePosition.X * Main.screenWidth + parallaxOffset,
                    star.BasePosition.Y * Main.screenHeight + parallaxOffset * 0.5f
                );
                
                // Twinkling
                float twinkle = (float)Math.Sin(animationTimer * 0.05f + star.TwinkleOffset) * 0.4f + 0.6f;
                
                // Phase 2: Stars pulse with color
                Color starColor = star.BaseColor;
                if (isPhase2)
                {
                    float colorPulse = (float)Math.Sin(colorCycleTimer + star.TwinkleOffset) * 0.5f + 0.5f;
                    starColor = Color.Lerp(star.BaseColor, Gold, colorPulse * 0.3f);
                }
                
                starColor *= twinkle * intensity;
                
                float size = star.Scale * 2f + twinkle;
                Rectangle starRect = new Rectangle(
                    (int)(starPos.X - size * 0.5f),
                    (int)(starPos.Y - size * 0.5f),
                    (int)size,
                    (int)size
                );
                
                spriteBatch.Draw(pixel, starRect, starColor);
                
                // Draw cross flare for brighter stars
                if (star.Scale > 0.8f)
                {
                    float flareSize = star.Scale * 4f * twinkle;
                    Color flareColor = starColor * 0.5f;
                    
                    // Horizontal
                    spriteBatch.Draw(pixel, new Rectangle(
                        (int)(starPos.X - flareSize),
                        (int)(starPos.Y - 0.5f),
                        (int)(flareSize * 2),
                        1
                    ), flareColor);
                    
                    // Vertical
                    spriteBatch.Draw(pixel, new Rectangle(
                        (int)(starPos.X - 0.5f),
                        (int)(starPos.Y - flareSize),
                        1,
                        (int)(flareSize * 2)
                    ), flareColor);
                }
            }
        }
        
        private void DrawCelestialParticles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var particle in celestialParticles)
            {
                Vector2 drawPos = particle.Position - Main.screenPosition;
                Color drawColor = particle.ParticleColor * particle.Opacity * intensity;
                
                switch (particle.Type)
                {
                    case 0: // Star twinkle
                    case 3: // Golden flare
                        float starSize = particle.Scale * 3f;
                        Rectangle starRect = new Rectangle(
                            (int)(drawPos.X - starSize * 0.5f),
                            (int)(drawPos.Y - starSize * 0.5f),
                            (int)starSize,
                            (int)starSize
                        );
                        spriteBatch.Draw(pixel, starRect, drawColor);
                        
                        // Cross flare
                        float crossSize = particle.Scale * 6f;
                        spriteBatch.Draw(pixel, new Rectangle(
                            (int)(drawPos.X - crossSize),
                            (int)(drawPos.Y - 0.5f),
                            (int)(crossSize * 2), 1
                        ), drawColor * 0.6f);
                        spriteBatch.Draw(pixel, new Rectangle(
                            (int)(drawPos.X - 0.5f),
                            (int)(drawPos.Y - crossSize),
                            1, (int)(crossSize * 2)
                        ), drawColor * 0.6f);
                        break;
                        
                    case 1: // Nebula wisp
                        float wispSize = particle.Scale * 20f;
                        for (int i = 0; i < 5; i++)
                        {
                            float layerSize = wispSize * (1f - i * 0.15f);
                            Color layerColor = drawColor * (0.3f - i * 0.05f);
                            Rectangle wispRect = new Rectangle(
                                (int)(drawPos.X - layerSize * 0.5f),
                                (int)(drawPos.Y - layerSize * 0.5f),
                                (int)layerSize,
                                (int)layerSize
                            );
                            spriteBatch.Draw(pixel, wispRect, layerColor);
                        }
                        break;
                        
                    case 2: // Cosmic streak
                        float streakLength = particle.Scale * 15f;
                        Vector2 streakDir = particle.Velocity.SafeNormalize(Vector2.UnitY);
                        Vector2 streakEnd = drawPos - streakDir * streakLength;
                        
                        // Draw as series of points
                        for (int i = 0; i < 8; i++)
                        {
                            float t = i / 8f;
                            Vector2 point = Vector2.Lerp(drawPos, streakEnd, t);
                            Color pointColor = drawColor * (1f - t);
                            float pointSize = particle.Scale * (1f - t * 0.5f);
                            Rectangle pointRect = new Rectangle(
                                (int)(point.X - pointSize * 0.5f),
                                (int)(point.Y - pointSize * 0.5f),
                                (int)pointSize,
                                (int)pointSize
                            );
                            spriteBatch.Draw(pixel, pointRect, pointColor);
                        }
                        break;
                        
                    case 4: // Shooting star
                        float trailLength = particle.Scale * 40f;
                        Vector2 trailDir = particle.Velocity.SafeNormalize(Vector2.UnitY);
                        
                        // Bright head
                        float headSize = particle.Scale * 4f;
                        Rectangle headRect = new Rectangle(
                            (int)(drawPos.X - headSize * 0.5f),
                            (int)(drawPos.Y - headSize * 0.5f),
                            (int)headSize,
                            (int)headSize
                        );
                        spriteBatch.Draw(pixel, headRect, drawColor);
                        
                        // Fading trail
                        for (int i = 0; i < 20; i++)
                        {
                            float t = i / 20f;
                            Vector2 point = drawPos - trailDir * trailLength * t;
                            Color pointColor = drawColor * (1f - t) * 0.8f;
                            float pointSize = particle.Scale * (1f - t * 0.7f);
                            Rectangle pointRect = new Rectangle(
                                (int)(point.X - pointSize * 0.5f),
                                (int)(point.Y - pointSize * 0.5f),
                                (int)Math.Max(1, pointSize),
                                (int)Math.Max(1, pointSize)
                            );
                            spriteBatch.Draw(pixel, pointRect, pointColor);
                        }
                        break;
                }
            }
        }
        
        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
            isPhase2 = false;
            phase2Transition = 0f;
        }

        public override void Reset()
        {
            isActive = false;
            intensity = 0f;
            isPhase2 = false;
            phase2Transition = 0f;
            celestialParticles.Clear();
            starFieldInitialized = false;
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }
    }
    
    /// <summary>
    /// Registers the Nachtmusik celestial sky effect with tModLoader
    /// </summary>
    public class NachtmusikCelestialSkyLoader : ModSystem
    {
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] = new NachtmusikCelestialSky();
            }
        }
        
        public override void Unload()
        {
            if (!Main.dedServ)
            {
                // SkyManager handles cleanup
            }
        }
    }
}
