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
    /// Custom sky background effect for Enigma, The Hollow Mystery boss fight.
    /// Features dark void background with swirling purple mists, watching eyes, and eerie green accents.
    /// A mysterious void that questions reality itself.
    /// </summary>
    public class EnigmaSkyEffect : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private float animationTimer = 0f;
        
        // Background particles (void wisps, mystery motes)
        private List<MysteryParticle> backgroundParticles = new List<MysteryParticle>();
        private const int MaxBackgroundParticles = 150;
        
        // Watching eyes in the void
        private List<VoidEye> watchingEyes = new List<VoidEye>();
        private const int MaxEyes = 12;
        
        // Swirling void vortexes
        private List<VoidVortex> vortexes = new List<VoidVortex>();
        private const int MaxVortexes = 4;
        
        // Flash effect for boss attacks
        private float flashIntensity = 0f;
        private Color flashColor = new Color(80, 40, 120);
        
        // Pulse effect - void breathing
        private float pulseIntensity = 0f;
        private float pulseTimer = 0f;
        
        // Enigma colors
        private static readonly Color EnigmaBlack = new Color(8, 5, 12);
        private static readonly Color EnigmaDeepVoid = new Color(15, 10, 25);
        private static readonly Color EnigmaDarkPurple = new Color(40, 15, 60);
        private static readonly Color EnigmaPurple = new Color(100, 40, 160);
        private static readonly Color EnigmaGreenFlame = new Color(50, 180, 90);
        private static readonly Color EnigmaDarkGreen = new Color(30, 90, 50);
        private static readonly Color EnigmaEyeGlow = new Color(120, 200, 100);
        
        private struct MysteryParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Opacity;
            public Color ParticleColor;
            public int Type; // 0 = void wisp, 1 = mystery mote, 2 = glyph, 3 = green flame
            public int Lifetime;
            public int TimeAlive;
            public float WaveOffset;
        }
        
        private struct VoidEye
        {
            public Vector2 Position;
            public float Scale;
            public float BlinkPhase;
            public float BlinkSpeed;
            public float LookAngle;
            public float LookSpeed;
            public Color IrisColor;
            public float Opacity;
            public int BlinkCooldown;
        }
        
        private struct VoidVortex
        {
            public Vector2 Position;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public float Opacity;
            public Color VortexColor;
        }
        
        public override void OnLoad()
        {
            backgroundParticles = new List<MysteryParticle>();
            watchingEyes = new List<VoidEye>();
            vortexes = new List<VoidVortex>();
        }

        public override void Update(GameTime gameTime)
        {
            animationTimer += 0.016f;
            
            if (isActive && intensity < 1f)
            {
                intensity += 0.005f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.012f;
            }
            
            intensity = MathHelper.Clamp(intensity, 0f, 1f);
            
            // Slow, ominous pulse
            pulseTimer += 0.012f;
            pulseIntensity = (float)Math.Sin(pulseTimer) * 0.2f + 0.8f;
            
            // Decay flash
            flashIntensity *= 0.9f;
            if (flashIntensity < 0.01f)
                flashIntensity = 0f;
            
            // Initialize elements
            if (isActive && intensity > 0.1f)
            {
                if (watchingEyes.Count < MaxEyes && Main.rand.NextBool(60))
                    SpawnWatchingEye();
                    
                if (vortexes.Count < MaxVortexes && Main.rand.NextBool(180))
                    SpawnVortex();
            }
            
            // Update all elements
            UpdateBackgroundParticles();
            UpdateWatchingEyes();
            UpdateVortexes();
            
            if (isActive && intensity > 0.15f)
            {
                SpawnBackgroundParticles();
            }
        }
        
        private void SpawnWatchingEye()
        {
            watchingEyes.Add(new VoidEye
            {
                Position = new Vector2(Main.rand.NextFloat(50, Main.screenWidth - 50), 
                                      Main.rand.NextFloat(50, Main.screenHeight - 50)),
                Scale = Main.rand.NextFloat(15f, 35f),
                BlinkPhase = Main.rand.NextFloat(MathHelper.TwoPi),
                BlinkSpeed = Main.rand.NextFloat(0.02f, 0.05f),
                LookAngle = Main.rand.NextFloat(MathHelper.TwoPi),
                LookSpeed = Main.rand.NextFloat(-0.01f, 0.01f),
                IrisColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, Main.rand.NextFloat()),
                Opacity = 0f,
                BlinkCooldown = Main.rand.Next(120, 300)
            });
        }
        
        private void SpawnVortex()
        {
            vortexes.Add(new VoidVortex
            {
                Position = new Vector2(Main.rand.NextFloat(100, Main.screenWidth - 100),
                                      Main.rand.NextFloat(100, Main.screenHeight - 100)),
                Scale = Main.rand.NextFloat(60f, 120f),
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f),
                Opacity = 0f,
                VortexColor = Color.Lerp(EnigmaDarkPurple, EnigmaPurple, Main.rand.NextFloat())
            });
        }
        
        private void UpdateWatchingEyes()
        {
            for (int i = watchingEyes.Count - 1; i >= 0; i--)
            {
                var eye = watchingEyes[i];
                
                // Fade in/out
                if (eye.Opacity < 0.7f && eye.BlinkCooldown > 60)
                    eye.Opacity = Math.Min(eye.Opacity + 0.015f, 0.7f);
                else if (eye.BlinkCooldown <= 60)
                    eye.Opacity = Math.Max(eye.Opacity - 0.02f, 0f);
                
                eye.BlinkPhase += eye.BlinkSpeed;
                eye.LookAngle += eye.LookSpeed;
                eye.BlinkCooldown--;
                
                // Change look direction occasionally
                if (Main.rand.NextBool(120))
                    eye.LookSpeed = Main.rand.NextFloat(-0.015f, 0.015f);
                
                watchingEyes[i] = eye;
                
                // Remove when faded and blink done
                if (eye.BlinkCooldown <= 0 && eye.Opacity <= 0f)
                {
                    watchingEyes.RemoveAt(i);
                }
            }
        }
        
        private void UpdateVortexes()
        {
            for (int i = vortexes.Count - 1; i >= 0; i--)
            {
                var vortex = vortexes[i];
                
                vortex.Rotation += vortex.RotationSpeed;
                
                // Fade lifecycle
                if (vortex.Opacity < 0.4f && vortex.Scale > 30f)
                    vortex.Opacity = Math.Min(vortex.Opacity + 0.005f, 0.4f);
                else
                    vortex.Scale -= 0.1f;
                
                if (vortex.Scale <= 20f)
                    vortex.Opacity -= 0.008f;
                
                vortexes[i] = vortex;
                
                if (vortex.Opacity <= 0f || vortex.Scale <= 0f)
                {
                    vortexes.RemoveAt(i);
                }
            }
        }
        
        private void UpdateBackgroundParticles()
        {
            for (int i = backgroundParticles.Count - 1; i >= 0; i--)
            {
                var particle = backgroundParticles[i];
                
                // Swirling drift
                float waveX = (float)Math.Sin(animationTimer * 0.8f + particle.WaveOffset) * 0.5f;
                float waveY = (float)Math.Cos(animationTimer * 0.6f + particle.WaveOffset * 1.3f) * 0.4f;
                particle.Position += particle.Velocity + new Vector2(waveX, waveY);
                particle.Rotation += particle.RotationSpeed;
                particle.TimeAlive++;
                
                float lifeProgress = (float)particle.TimeAlive / particle.Lifetime;
                if (lifeProgress < 0.15f)
                    particle.Opacity = lifeProgress / 0.15f;
                else if (lifeProgress > 0.75f)
                    particle.Opacity = 1f - (lifeProgress - 0.75f) / 0.25f;
                
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
                
            if (Main.rand.NextBool(3))
            {
                float spawnX = Main.rand.NextFloat(-50, Main.screenWidth + 50);
                float spawnY = Main.rand.NextFloat(-50, Main.screenHeight + 50);
                
                int particleType = Main.rand.Next(12);
                Color particleColor;
                float scale;
                Vector2 velocity;
                int type;
                
                if (particleType < 5) // Void wisps (42%)
                {
                    particleColor = Color.Lerp(EnigmaDarkPurple, EnigmaPurple, Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(1.5f, 3.5f);
                    velocity = Main.rand.NextVector2Circular(0.3f, 0.3f);
                    type = 0;
                }
                else if (particleType < 9) // Mystery motes (33%)
                {
                    particleColor = Color.Lerp(EnigmaBlack, EnigmaDarkPurple, Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(0.5f, 1.2f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.3f, 0.3f));
                    type = 1;
                }
                else if (particleType < 11) // Glyphs (17%)
                {
                    particleColor = EnigmaPurple;
                    scale = Main.rand.NextFloat(0.4f, 0.9f);
                    velocity = Main.rand.NextVector2Circular(0.15f, 0.15f);
                    type = 2;
                }
                else // Green flame wisps (8%)
                {
                    particleColor = Color.Lerp(EnigmaDarkGreen, EnigmaGreenFlame, Main.rand.NextFloat());
                    scale = Main.rand.NextFloat(0.8f, 1.8f);
                    velocity = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.8f, -0.3f));
                    type = 3;
                }
                
                backgroundParticles.Add(new MysteryParticle
                {
                    Position = new Vector2(spawnX, spawnY),
                    Velocity = velocity,
                    Scale = scale,
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                    Opacity = 0f,
                    ParticleColor = particleColor,
                    Type = type,
                    Lifetime = Main.rand.Next(250, 500),
                    TimeAlive = 0,
                    WaveOffset = Main.rand.NextFloat(MathHelper.TwoPi)
                });
            }
        }
        
        public void TriggerFlash(float strength = 1f, Color? color = null)
        {
            flashIntensity = Math.Max(flashIntensity, strength);
            flashColor = color ?? EnigmaPurple;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                
                // Draw void black base with subtle purple gradient
                DrawVoidBackground(spriteBatch, pixel);
                
                // Draw swirling vortexes (background)
                DrawVortexes(spriteBatch, pixel);
                
                // Draw animated void particles
                DrawBackgroundParticles(spriteBatch, pixel);
                
                // Draw watching eyes
                DrawWatchingEyes(spriteBatch, pixel);
                
                // Draw mystery fog overlay
                DrawMysteryFog(spriteBatch, pixel);
                
                // Draw vignette
                DrawVignette(spriteBatch, pixel);
                
                // Draw flash overlay
                if (flashIntensity > 0f)
                {
                    Rectangle fullScreen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
                    spriteBatch.Draw(pixel, fullScreen, flashColor * flashIntensity * 0.4f);
                }
            }
        }
        
        private void DrawVoidBackground(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Deep void with subtle purple undertones
            for (int y = 0; y < Main.screenHeight; y += 4)
            {
                float gradientFactor = (float)y / Main.screenHeight;
                
                // Subtle wave distortion
                float voidWave = (float)Math.Sin(animationTimer * 0.4f + y * 0.004f) * 0.15f;
                
                Color baseColor;
                if (gradientFactor < 0.3f)
                {
                    float t = gradientFactor / 0.3f;
                    baseColor = Color.Lerp(EnigmaBlack, new Color(10, 6, 16), t);
                }
                else if (gradientFactor < 0.6f)
                {
                    float t = (gradientFactor - 0.3f) / 0.3f;
                    baseColor = Color.Lerp(new Color(10, 6, 16), new Color(14, 8, 22), t);
                }
                else
                {
                    float t = (gradientFactor - 0.6f) / 0.4f;
                    baseColor = Color.Lerp(new Color(14, 8, 22), EnigmaDeepVoid, t);
                }
                
                // Apply wave distortion with purple tint
                baseColor = Color.Lerp(baseColor, EnigmaDarkPurple * 0.2f, Math.Max(0, voidWave) * 0.4f);
                
                Color finalColor = baseColor * intensity;
                Rectangle rect = new Rectangle(0, y, Main.screenWidth, 4);
                spriteBatch.Draw(pixel, rect, finalColor);
            }
        }
        
        private void DrawVortexes(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var vortex in vortexes)
            {
                DrawSwirlVortex(spriteBatch, pixel, vortex.Position, vortex.Scale, vortex.Rotation,
                    vortex.VortexColor, vortex.Opacity * intensity * pulseIntensity);
            }
        }
        
        private void DrawSwirlVortex(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float size, float rotation, Color color, float opacity)
        {
            // Draw concentric swirling rings
            int rings = 8;
            for (int r = 0; r < rings; r++)
            {
                float ringRadius = size * ((float)r / rings);
                float ringOpacity = (1f - (float)r / rings) * opacity * 0.3f;
                float ringRotation = rotation + r * 0.3f;
                
                int points = 24;
                for (int p = 0; p < points; p++)
                {
                    float angle = ringRotation + MathHelper.TwoPi * p / points;
                    Vector2 point = center + angle.ToRotationVector2() * ringRadius;
                    
                    float pointSize = 2f + (rings - r) * 0.3f;
                    Rectangle pointRect = new Rectangle(
                        (int)(point.X - pointSize / 2), (int)(point.Y - pointSize / 2),
                        (int)pointSize, (int)pointSize);
                    spriteBatch.Draw(pixel, pointRect, color * ringOpacity);
                }
            }
            
            // Center core
            for (int i = 3; i >= 0; i--)
            {
                float coreSize = 8f * (1f + i * 0.6f);
                float coreOpacity = 1f / (i + 1.5f);
                Rectangle coreRect = new Rectangle(
                    (int)(center.X - coreSize / 2), (int)(center.Y - coreSize / 2),
                    (int)coreSize, (int)coreSize);
                spriteBatch.Draw(pixel, coreRect, color * coreOpacity * opacity * 0.6f);
            }
        }
        
        private void DrawWatchingEyes(SpriteBatch spriteBatch, Texture2D pixel)
        {
            foreach (var eye in watchingEyes)
            {
                if (eye.Opacity <= 0f) continue;
                
                float finalOpacity = eye.Opacity * intensity * pulseIntensity;
                
                // Blink effect (eyelid closing)
                float blink = (float)Math.Sin(eye.BlinkPhase);
                float eyeOpenness = blink > 0.7f ? (1f - (blink - 0.7f) / 0.3f) : 1f;
                
                DrawVoidEye(spriteBatch, pixel, eye.Position, eye.Scale, eye.LookAngle, 
                    eye.IrisColor, eyeOpenness, finalOpacity);
            }
        }
        
        private void DrawVoidEye(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float size, float lookAngle, Color irisColor, float openness, float opacity)
        {
            if (openness < 0.1f) return;
            
            // Outer glow (dark purple)
            for (int i = 4; i >= 0; i--)
            {
                float glowSize = size * (1.3f + i * 0.3f);
                float glowOpacity = 1f / (i + 2f);
                
                // Ellipse for eye shape
                float eyeHeight = glowSize * 0.5f * openness;
                Rectangle glowRect = new Rectangle(
                    (int)(center.X - glowSize / 2), (int)(center.Y - eyeHeight / 2),
                    (int)glowSize, (int)eyeHeight);
                spriteBatch.Draw(pixel, glowRect, EnigmaDarkPurple * glowOpacity * opacity * 0.5f);
            }
            
            // Eye white (dark)
            float eyeWhiteWidth = size * 0.8f;
            float eyeWhiteHeight = size * 0.4f * openness;
            Rectangle whiteRect = new Rectangle(
                (int)(center.X - eyeWhiteWidth / 2), (int)(center.Y - eyeWhiteHeight / 2),
                (int)eyeWhiteWidth, (int)eyeWhiteHeight);
            spriteBatch.Draw(pixel, whiteRect, new Color(20, 15, 30) * opacity);
            
            // Iris
            Vector2 lookOffset = lookAngle.ToRotationVector2() * size * 0.15f;
            Vector2 irisCenter = center + lookOffset;
            float irisSize = size * 0.3f;
            
            // Iris glow
            for (int i = 2; i >= 0; i--)
            {
                float irisGlowSize = irisSize * (1f + i * 0.5f);
                float irisGlowOpacity = 1f / (i + 1.5f);
                Rectangle irisGlowRect = new Rectangle(
                    (int)(irisCenter.X - irisGlowSize / 2), (int)(irisCenter.Y - irisGlowSize * 0.5f * openness / 2),
                    (int)irisGlowSize, (int)(irisGlowSize * 0.5f * openness));
                spriteBatch.Draw(pixel, irisGlowRect, irisColor * irisGlowOpacity * opacity * 0.7f);
            }
            
            // Iris core
            Rectangle irisRect = new Rectangle(
                (int)(irisCenter.X - irisSize / 2), (int)(irisCenter.Y - irisSize * 0.4f * openness / 2),
                (int)irisSize, (int)(irisSize * 0.4f * openness));
            spriteBatch.Draw(pixel, irisRect, irisColor * opacity);
            
            // Pupil (black with green glow)
            float pupilSize = irisSize * 0.4f;
            Rectangle pupilRect = new Rectangle(
                (int)(irisCenter.X - pupilSize / 2), (int)(irisCenter.Y - pupilSize * 0.4f * openness / 2),
                (int)pupilSize, (int)(pupilSize * 0.4f * openness));
            spriteBatch.Draw(pixel, pupilRect, Color.Black * opacity);
            
            // Green inner glow
            Rectangle innerGlowRect = new Rectangle(
                (int)(irisCenter.X - pupilSize * 0.3f), (int)(irisCenter.Y - pupilSize * 0.15f * openness),
                (int)(pupilSize * 0.6f), (int)(pupilSize * 0.3f * openness));
            spriteBatch.Draw(pixel, innerGlowRect, EnigmaGreenFlame * opacity * 0.5f);
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
                    case 0: // Void wisp
                        DrawVoidWisp(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 8f);
                        break;
                        
                    case 1: // Mystery mote
                        DrawMysteryMote(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 3f);
                        break;
                        
                    case 2: // Glyph
                        DrawMysteryGlyph(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 10f, particle.Rotation);
                        break;
                        
                    case 3: // Green flame wisp
                        DrawGreenFlameWisp(spriteBatch, pixel, drawPos, drawColor, particle.Scale * 6f);
                        break;
                }
            }
        }
        
        private void DrawVoidWisp(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size)
        {
            for (int i = 3; i >= 0; i--)
            {
                float layerSize = size * (1f + i * 0.5f);
                float layerOpacity = 1f / (i + 1.5f);
                Rectangle rect = new Rectangle(
                    (int)(center.X - layerSize / 2), (int)(center.Y - layerSize * 0.6f),
                    (int)layerSize, (int)(layerSize * 0.8f));
                spriteBatch.Draw(pixel, rect, color * layerOpacity * 0.3f);
            }
        }
        
        private void DrawMysteryMote(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size)
        {
            Rectangle rect = new Rectangle((int)(center.X - size / 2), (int)(center.Y - size / 2), 
                (int)Math.Max(1, size), (int)Math.Max(1, size));
            spriteBatch.Draw(pixel, rect, color * 0.6f);
        }
        
        private void DrawMysteryGlyph(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size, float rotation)
        {
            // Simple arcane rune shape - diamond with inner lines
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation + MathHelper.PiOver2 * i;
                Vector2 point = center + angle.ToRotationVector2() * size * 0.5f;
                
                // Connect points
                Vector2 nextPoint = center + (angle + MathHelper.PiOver2).ToRotationVector2() * size * 0.5f;
                DrawLine(spriteBatch, pixel, point, nextPoint, color * 0.5f, 1f);
                
                // Lines to center
                DrawLine(spriteBatch, pixel, point, center, color * 0.3f, 1f);
            }
            
            // Center dot
            Rectangle centerRect = new Rectangle((int)(center.X - 1), (int)(center.Y - 1), 2, 2);
            spriteBatch.Draw(pixel, centerRect, color);
        }
        
        private void DrawGreenFlameWisp(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Color color, float size)
        {
            // Flame shape - narrower at top
            for (int i = 4; i >= 0; i--)
            {
                float layerY = center.Y - i * size * 0.2f;
                float layerWidth = size * (1f - i * 0.15f) * (1f - (float)i / 8f);
                float layerOpacity = 1f / (i + 1.2f);
                
                Rectangle rect = new Rectangle(
                    (int)(center.X - layerWidth / 2), (int)layerY,
                    (int)Math.Max(1, layerWidth), (int)(size * 0.25f));
                spriteBatch.Draw(pixel, rect, color * layerOpacity * 0.4f);
            }
        }
        
        private void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 diff = end - start;
            int length = (int)diff.Length();
            if (length < 1) return;
            
            diff.Normalize();
            for (int i = 0; i < length; i += 2)
            {
                Vector2 pos = start + diff * i;
                Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, (int)thickness, (int)thickness);
                spriteBatch.Draw(pixel, rect, color);
            }
        }
        
        private void DrawMysteryFog(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Layers of drifting purple fog
            int fogLayers = 6;
            for (int l = 0; l < fogLayers; l++)
            {
                float fogPhase = animationTimer * 0.15f + l * 1.2f;
                float fogX = (float)Math.Sin(fogPhase) * Main.screenWidth * 0.2f + Main.screenWidth * 0.5f;
                float fogY = Main.screenHeight * (0.2f + l * 0.12f) + (float)Math.Cos(fogPhase * 0.7f) * 50f;
                
                float fogWidth = Main.screenWidth * 0.6f + (float)Math.Sin(fogPhase * 0.5f) * 100f;
                float fogHeight = 80f + l * 15f;
                float fogOpacity = 0.06f * intensity * pulseIntensity;
                
                Color fogColor = Color.Lerp(EnigmaDarkPurple, EnigmaPurple, l / (float)fogLayers);
                
                Rectangle fogRect = new Rectangle(
                    (int)(fogX - fogWidth / 2), (int)(fogY - fogHeight / 2),
                    (int)fogWidth, (int)fogHeight);
                spriteBatch.Draw(pixel, fogRect, fogColor * fogOpacity);
            }
        }
        
        private void DrawVignette(SpriteBatch spriteBatch, Texture2D pixel)
        {
            int vignetteSize = 350;
            float vignetteStrength = 0.7f * intensity;
            
            for (int i = 0; i < vignetteSize; i++)
            {
                float opacity = (1f - (float)i / vignetteSize) * vignetteStrength;
                Color vignetteColor = EnigmaBlack * opacity;
                
                // All sides - darker on bottom for ominous feel
                spriteBatch.Draw(pixel, new Rectangle(0, i, Main.screenWidth, 1), vignetteColor * 0.6f);
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - i - 1, Main.screenWidth, 1), vignetteColor * 0.9f);
                spriteBatch.Draw(pixel, new Rectangle(i, 0, 1, Main.screenHeight), vignetteColor * 0.6f);
                spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - i - 1, 0, 1, Main.screenHeight), vignetteColor * 0.6f);
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
            watchingEyes.Clear();
            vortexes.Clear();
            backgroundParticles.Clear();
        }

        public override bool IsActive() => isActive || intensity > 0f;
    }
    
    /// <summary>
    /// ModSystem to register the Enigma sky effect.
    /// </summary>
    public class EnigmaSkyEffectLoader : ModSystem
    {
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance["MagnumOpus:EnigmaSky"] = new EnigmaSkyEffect();
            }
        }
    }
}
