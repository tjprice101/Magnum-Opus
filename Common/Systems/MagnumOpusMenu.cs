using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom main menu for MagnumOpus with cosmic effects and flowing music notes.
    /// Creates an ethereal, musical atmosphere befitting the mod's musical theme.
    /// </summary>
    public class MagnumOpusMenu : ModMenu
    {
        // Menu particles for ambient effects
        private static List<MenuParticle> particles = new List<MenuParticle>();
        private static List<MenuMusicNote> musicNotes = new List<MenuMusicNote>();
        private static List<CosmicStream> cosmicStreams = new List<CosmicStream>();
        
        private static float globalTimer = 0f;
        private static bool initialized = false;
        
        public override string DisplayName => "Magnum Opus";
        
        public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>("MagnumOpus/icon");
        
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/Pure Resonance");
        
        public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
        {
            // Initialize particles if needed
            if (!initialized)
            {
                InitializeEffects();
                initialized = true;
            }
            
            // Update timer
            globalTimer += 0.016f;
            
            // Draw the custom background FIRST - covers the vanilla background
            DrawBackground(spriteBatch);
            
            // Update and draw cosmic streams
            UpdateAndDrawCosmicStreams(spriteBatch);
            
            // Update and draw ambient particles  
            UpdateAndDrawParticles(spriteBatch);
            
            // Update and draw music notes
            UpdateAndDrawMusicNotes(spriteBatch);
            
            // Draw subtle vignette overlay
            DrawVignette(spriteBatch);
            
            // Modify logo appearance
            logoScale = 1.1f + (float)Math.Sin(globalTimer * 1.5f) * 0.05f;
            drawColor = Color.White * 0.95f;
            
            return true; // Continue drawing the logo
        }
        
        private static void InitializeEffects()
        {
            particles.Clear();
            musicNotes.Clear();
            cosmicStreams.Clear();
            
            // Create cosmic streams
            for (int i = 0; i < 8; i++)
            {
                cosmicStreams.Add(new CosmicStream
                {
                    StartX = Main.rand.NextFloat(0f, Main.screenWidth),
                    Phase = Main.rand.NextFloat(0f, MathHelper.TwoPi),
                    Speed = Main.rand.NextFloat(0.3f, 0.8f),
                    Amplitude = Main.rand.NextFloat(100f, 300f),
                    Thickness = Main.rand.NextFloat(2f, 6f),
                    HueOffset = Main.rand.NextFloat(0f, 1f),
                    Length = Main.rand.NextFloat(200f, 500f)
                });
            }
            
            // Create initial particles
            for (int i = 0; i < 60; i++)
            {
                SpawnParticle();
            }
            
            // Create initial music notes
            for (int i = 0; i < 15; i++)
            {
                SpawnMusicNote();
            }
        }
        
        private static void SpawnParticle()
        {
            particles.Add(new MenuParticle
            {
                Position = new Vector2(Main.rand.NextFloat(0, Main.screenWidth), Main.rand.NextFloat(0, Main.screenHeight)),
                Velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.5f, -0.1f)),
                Scale = Main.rand.NextFloat(0.3f, 1.2f),
                Alpha = Main.rand.NextFloat(0.2f, 0.6f),
                Lifetime = Main.rand.Next(300, 600),
                Age = Main.rand.Next(0, 200),
                HueOffset = Main.rand.NextFloat(0f, 1f),
                Type = Main.rand.Next(3) // 0 = star, 1 = glow, 2 = sparkle
            });
        }
        
        private static void SpawnMusicNote()
        {
            int side = Main.rand.NextBool() ? 0 : 1; // 0 = left, 1 = right
            float startX = side == 0 ? -50f : Main.screenWidth + 50f;
            float dirX = side == 0 ? 1f : -1f;
            
            musicNotes.Add(new MenuMusicNote
            {
                Position = new Vector2(startX, Main.rand.NextFloat(100f, Main.screenHeight - 100f)),
                Velocity = new Vector2(dirX * Main.rand.NextFloat(0.5f, 1.5f), Main.rand.NextFloat(-0.3f, 0.3f)),
                Scale = Main.rand.NextFloat(0.4f, 0.8f),
                Alpha = 0f,
                FadeIn = true,
                Rotation = Main.rand.NextFloat(-0.2f, 0.2f),
                RotationSpeed = Main.rand.NextFloat(-0.01f, 0.01f),
                SineOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi),
                SineAmplitude = Main.rand.NextFloat(20f, 60f),
                NoteType = Main.rand.Next(4), // Different note symbols
                HueOffset = Main.rand.NextFloat(0f, 1f)
            });
        }
        
        private void DrawBackground(SpriteBatch spriteBatch)
        {
            Texture2D bgTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/MagnumOpusBG", AssetRequestMode.ImmediateLoad).Value;
            
            // Calculate scale to fill screen while maintaining aspect ratio
            float scaleX = (float)Main.screenWidth / bgTexture.Width;
            float scaleY = (float)Main.screenHeight / bgTexture.Height;
            float scale = Math.Max(scaleX, scaleY);
            
            Vector2 origin = new Vector2(bgTexture.Width / 2f, bgTexture.Height / 2f);
            Vector2 position = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            
            // Draw with slight pulse
            float pulse = 1f + (float)Math.Sin(globalTimer * 0.5f) * 0.01f;
            
            spriteBatch.Draw(bgTexture, position, null, Color.White, 0f, origin, scale * pulse, SpriteEffects.None, 0f);
        }
        
        private void UpdateAndDrawCosmicStreams(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad).Value;
            
            foreach (var stream in cosmicStreams)
            {
                stream.Phase += stream.Speed * 0.02f;
                
                // Draw stream as series of connected glows
                int segments = (int)(stream.Length / 10f);
                for (int i = 0; i < segments; i++)
                {
                    float progress = (float)i / segments;
                    float yBase = (globalTimer * stream.Speed * 50f + i * 15f) % (Main.screenHeight + stream.Length) - stream.Length * 0.5f;
                    float xOffset = (float)Math.Sin(stream.Phase + progress * 3f) * stream.Amplitude;
                    
                    Vector2 pos = new Vector2(stream.StartX + xOffset, yBase);
                    
                    // Color based on position in stream
                    float hue = (stream.HueOffset + progress * 0.3f + globalTimer * 0.1f) % 1f;
                    Color streamColor = Main.hslToRgb(hue, 0.6f, 0.7f) * (0.15f * (1f - progress * 0.5f));
                    streamColor.A = 0; // Additive blending
                    
                    float segmentScale = stream.Thickness * 0.02f * (1f - progress * 0.3f);
                    
                    spriteBatch.Draw(glowTex, pos, null, streamColor, 0f, 
                        new Vector2(glowTex.Width / 2f, glowTex.Height / 2f), 
                        segmentScale, SpriteEffects.None, 0f);
                }
            }
        }
        
        private void UpdateAndDrawParticles(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad).Value;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad).Value;
            
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Age++;
                
                if (p.Age >= p.Lifetime)
                {
                    particles.RemoveAt(i);
                    SpawnParticle();
                    continue;
                }
                
                // Update position with gentle drift
                p.Position += p.Velocity;
                p.Position.X += (float)Math.Sin(globalTimer * 2f + p.HueOffset * 10f) * 0.2f;
                
                // Wrap around screen
                if (p.Position.Y < -50) p.Position.Y = Main.screenHeight + 50;
                if (p.Position.X < -50) p.Position.X = Main.screenWidth + 50;
                if (p.Position.X > Main.screenWidth + 50) p.Position.X = -50;
                
                // Calculate alpha with fade in/out
                float lifeProgress = (float)p.Age / p.Lifetime;
                float fadeAlpha = p.Alpha;
                if (lifeProgress < 0.1f) fadeAlpha *= lifeProgress / 0.1f;
                if (lifeProgress > 0.8f) fadeAlpha *= (1f - lifeProgress) / 0.2f;
                
                // Pulsing
                float pulse = 1f + (float)Math.Sin(globalTimer * 3f + p.HueOffset * MathHelper.TwoPi) * 0.2f;
                
                // Color - cosmic purple/blue/pink
                float hue = (0.75f + p.HueOffset * 0.2f + globalTimer * 0.02f) % 1f; // Purple-ish range
                Color particleColor = Main.hslToRgb(hue, 0.5f, 0.8f) * fadeAlpha;
                particleColor.A = 0;
                
                Texture2D tex = p.Type == 1 ? flareTex : glowTex;
                float scale = p.Scale * pulse * 0.15f;
                
                spriteBatch.Draw(tex, p.Position, null, particleColor, 0f,
                    new Vector2(tex.Width / 2f, tex.Height / 2f), scale, SpriteEffects.None, 0f);
                
                particles[i] = p;
            }
        }
        
        private void UpdateAndDrawMusicNotes(SpriteBatch spriteBatch)
        {
            Texture2D noteTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNote", AssetRequestMode.ImmediateLoad).Value;
            
            for (int i = musicNotes.Count - 1; i >= 0; i--)
            {
                var note = musicNotes[i];
                
                // Update position
                note.Position += note.Velocity;
                note.Position.Y += (float)Math.Sin(globalTimer * 2f + note.SineOffset) * 0.5f;
                
                // Update rotation
                note.Rotation += note.RotationSpeed;
                
                // Fade in/out
                if (note.FadeIn)
                {
                    note.Alpha = Math.Min(1f, note.Alpha + 0.02f);
                    if (note.Alpha >= 0.6f) note.FadeIn = false;
                }
                
                // Check if off screen
                bool offScreen = note.Position.X < -100 || note.Position.X > Main.screenWidth + 100;
                if (offScreen)
                {
                    note.Alpha -= 0.02f;
                }
                
                if (note.Alpha <= 0f)
                {
                    musicNotes.RemoveAt(i);
                    SpawnMusicNote();
                    continue;
                }
                
                // Color - rainbow cycle
                float hue = (note.HueOffset + globalTimer * 0.05f) % 1f;
                Color noteColor = Main.hslToRgb(hue, 0.7f, 0.85f) * (note.Alpha * 0.7f);
                noteColor.A = 0;
                
                float scale = note.Scale * (1f + (float)Math.Sin(globalTimer * 2f + note.SineOffset) * 0.1f);
                
                // Draw glow behind
                spriteBatch.Draw(noteTex, note.Position, null, noteColor * 0.5f, note.Rotation,
                    new Vector2(noteTex.Width / 2f, noteTex.Height / 2f), scale * 1.5f, SpriteEffects.None, 0f);
                
                // Draw note
                spriteBatch.Draw(noteTex, note.Position, null, noteColor, note.Rotation,
                    new Vector2(noteTex.Width / 2f, noteTex.Height / 2f), scale, SpriteEffects.None, 0f);
                
                musicNotes[i] = note;
            }
        }
        
        private void DrawVignette(SpriteBatch spriteBatch)
        {
            // Simple corner darkening effect
            Texture2D pixel = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare", AssetRequestMode.ImmediateLoad).Value;
            
            // Top and bottom gradients
            for (int i = 0; i < 5; i++)
            {
                float alpha = (5 - i) * 0.03f;
                Color vignetteColor = Color.Black * alpha;
                
                // Top
                spriteBatch.Draw(pixel, new Rectangle(0, i * 30, Main.screenWidth, 30), vignetteColor);
                // Bottom
                spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - (i + 1) * 30, Main.screenWidth, 30), vignetteColor);
            }
        }
        
        public override void OnDeselected()
        {
            initialized = false;
        }
        
        // Particle data structures
        private struct MenuParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Alpha;
            public int Lifetime;
            public int Age;
            public float HueOffset;
            public int Type;
        }
        
        private struct MenuMusicNote
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Alpha;
            public bool FadeIn;
            public float Rotation;
            public float RotationSpeed;
            public float SineOffset;
            public float SineAmplitude;
            public int NoteType;
            public float HueOffset;
        }
        
        private class CosmicStream
        {
            public float StartX;
            public float Phase;
            public float Speed;
            public float Amplitude;
            public float Thickness;
            public float HueOffset;
            public float Length;
        }
    }
}
