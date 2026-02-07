using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// THEMED NEBULA FOG EFFECTS
    /// 
    /// Each musical score theme has its own unique fog cloud effect:
    /// - Winter: Icy crystalline fog with frost sparkles and snowflake patterns
    /// - Nachtmusik: Starry night sky fog with twinkling stars and moon glow
    /// - La Campanella: Infernal smoke with ember particles and bell-wave ripples
    /// - Eroica: Heroic golden mist with sakura petal swirls
    /// - Swan Lake: Elegant monochrome fog with feather wisps and prismatic edges
    /// - Moonlight Sonata: Lunar mist with purple gradients and silver highlights
    /// - Enigma Variations: Void fog with watching eyes and mystery particles
    /// - Fate: Cosmic nebula with constellation patterns and reality distortions
    /// - Clair de Lune: Soft moonbeam fog with gentle ripples
    /// - Dies Irae: Wrathful storm fog with lightning crackles
    /// - Ode to Joy: Celebratory golden fog with jubilant sparkles
    /// 
    /// All fog effects incorporate floating music notes unique to each theme!
    /// </summary>
    public class ThemedNebulaFogEffects : ModSystem
    {
        #region Fog Cloud Types
        
        public enum ThemedFogType
        {
            // Seasonal
            Winter,
            Spring,
            Summer,
            Autumn,
            
            // Main Themes
            LaCampanella,
            Eroica,
            SwanLake,
            MoonlightSonata,
            EnigmaVariations,
            Fate,
            ClairDeLune,
            DiesIrae,
            OdeToJoy,
            Nachtmusik
        }
        
        #endregion
        
        #region Themed Fog Cloud Class
        
        public class ThemedFogCloud
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public int Lifetime;
            public int MaxLifetime;
            public ThemedFogType FogType;
            public float Opacity;
            public bool Active;
            
            // Theme-specific properties
            public float PulsePhase;
            public float SecondaryScale;
            public List<FogMusicNote> MusicNotes;
            public List<FogParticle> ThemeParticles;
            public int Seed;
            
            public ThemedFogCloud()
            {
                MusicNotes = new List<FogMusicNote>();
                ThemeParticles = new List<FogParticle>();
            }
            
            public void Reset()
            {
                Active = false;
                MusicNotes.Clear();
                ThemeParticles.Clear();
            }
        }
        
        public class FogMusicNote
        {
            public Vector2 LocalOffset;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public float RotationSpeed;
            public int NoteVariant; // 1-6 for different note shapes
            public float Opacity;
            public int Lifetime;
            public int MaxLifetime;
            public Color TintColor;
            public float OrbitAngle;
            public float OrbitRadius;
            public float OrbitSpeed;
            public bool Orbiting;
        }
        
        public class FogParticle
        {
            public Vector2 LocalOffset;
            public Vector2 Velocity;
            public float Scale;
            public float Rotation;
            public int ParticleType; // Theme-specific particle type
            public float Opacity;
            public int Lifetime;
            public Color Color;
        }
        
        #endregion
        
        #region Object Pooling
        
        private static List<ThemedFogCloud> _activeFogClouds = new List<ThemedFogCloud>();
        private static Queue<ThemedFogCloud> _fogPool = new Queue<ThemedFogCloud>();
        private const int MAX_THEMED_FOGS = 50;
        
        private static ThemedFogCloud GetPooledFog()
        {
            if (_fogPool.Count > 0)
                return _fogPool.Dequeue();
            return new ThemedFogCloud();
        }
        
        private static void ReturnToPool(ThemedFogCloud fog)
        {
            fog.Reset();
            _fogPool.Enqueue(fog);
        }
        
        #endregion
        
        #region Theme Color Definitions
        
        public static class ThemeColors
        {
            // Winter - Icy blues and whites
            public static readonly Color WinterPrimary = new Color(200, 230, 255);
            public static readonly Color WinterSecondary = new Color(150, 200, 255);
            public static readonly Color WinterAccent = new Color(255, 255, 255);
            public static readonly Color WinterFrost = new Color(180, 220, 255, 180);
            
            // Nachtmusik - Deep night sky
            public static readonly Color NachtmusikPrimary = new Color(20, 30, 60);
            public static readonly Color NachtmusikSecondary = new Color(60, 80, 140);
            public static readonly Color NachtmusikStars = new Color(255, 255, 220);
            public static readonly Color NachtmusikMoon = new Color(255, 250, 200);
            
            // La Campanella - Infernal fire
            public static readonly Color CampanellaPrimary = new Color(30, 20, 25);
            public static readonly Color CampanellaSecondary = new Color(255, 140, 40);
            public static readonly Color CampanellaEmber = new Color(255, 100, 20);
            public static readonly Color CampanellaSmoke = new Color(60, 40, 45, 200);
            
            // Eroica - Heroic gold and scarlet
            public static readonly Color EroicaPrimary = new Color(200, 50, 50);
            public static readonly Color EroicaSecondary = new Color(255, 200, 80);
            public static readonly Color EroicaSakura = new Color(255, 180, 200);
            public static readonly Color EroicaGlow = new Color(255, 220, 150, 150);
            
            // Swan Lake - Monochrome elegance
            public static readonly Color SwanPrimary = new Color(255, 255, 255);
            public static readonly Color SwanSecondary = new Color(30, 30, 40);
            public static readonly Color SwanPrismatic = new Color(220, 200, 255);
            public static readonly Color SwanFeather = new Color(250, 250, 255, 180);
            
            // Moonlight Sonata - Lunar purple
            public static readonly Color MoonlightPrimary = new Color(100, 80, 160);
            public static readonly Color MoonlightSecondary = new Color(150, 130, 200);
            public static readonly Color MoonlightSilver = new Color(220, 220, 240);
            public static readonly Color MoonlightGlow = new Color(180, 160, 220, 150);
            
            // Enigma Variations - Mysterious void
            public static readonly Color EnigmaPrimary = new Color(40, 20, 60);
            public static readonly Color EnigmaSecondary = new Color(120, 60, 180);
            public static readonly Color EnigmaGreen = new Color(80, 200, 120);
            public static readonly Color EnigmaVoid = new Color(20, 10, 30, 220);
            
            // Fate - Cosmic celestial
            public static readonly Color FatePrimary = new Color(20, 10, 30);
            public static readonly Color FateSecondary = new Color(200, 80, 140);
            public static readonly Color FateCrimson = new Color(255, 80, 100);
            public static readonly Color FateStars = new Color(255, 255, 255);
            
            // Clair de Lune - Soft moonbeam
            public static readonly Color ClairPrimary = new Color(140, 160, 200);
            public static readonly Color ClairSecondary = new Color(200, 210, 240);
            public static readonly Color ClairPearl = new Color(250, 250, 255);
            
            // Dies Irae - Wrathful storm
            public static readonly Color DiesIraePrimary = new Color(60, 20, 30);
            public static readonly Color DiesIraeSecondary = new Color(180, 40, 60);
            public static readonly Color DiesIraeLightning = new Color(255, 200, 220);
            
            // Ode to Joy - Jubilant gold
            public static readonly Color OdePrimary = new Color(255, 220, 100);
            public static readonly Color OdeSecondary = new Color(255, 180, 50);
            public static readonly Color OdeSparkle = new Color(255, 255, 200);
            
            // Spring - Fresh green
            public static readonly Color SpringPrimary = new Color(150, 220, 150);
            public static readonly Color SpringSecondary = new Color(200, 255, 180);
            public static readonly Color SpringPetal = new Color(255, 200, 220);
            
            // Summer - Warm orange
            public static readonly Color SummerPrimary = new Color(255, 200, 100);
            public static readonly Color SummerSecondary = new Color(255, 160, 80);
            public static readonly Color SummerGlow = new Color(255, 240, 180);
            
            // Autumn - Rich amber
            public static readonly Color AutumnPrimary = new Color(200, 120, 60);
            public static readonly Color AutumnSecondary = new Color(180, 80, 40);
            public static readonly Color AutumnLeaf = new Color(220, 100, 50);
        }
        
        #endregion
        
        #region Spawning Methods
        
        /// <summary>
        /// Spawns a themed fog cloud with unique visual characteristics.
        /// </summary>
        public static ThemedFogCloud SpawnThemedFog(Vector2 position, ThemedFogType fogType, float scale = 1f, int lifetime = 120)
        {
            if (_activeFogClouds.Count >= MAX_THEMED_FOGS)
                return null;
            
            var fog = GetPooledFog();
            fog.Position = position;
            fog.Velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            fog.Scale = scale;
            fog.SecondaryScale = scale * 0.8f;
            fog.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            fog.RotationSpeed = Main.rand.NextFloat(-0.02f, 0.02f);
            fog.Lifetime = lifetime;
            fog.MaxLifetime = lifetime;
            fog.FogType = fogType;
            fog.Opacity = 0f;
            fog.Active = true;
            fog.PulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            fog.Seed = Main.rand.Next(10000);
            
            // Spawn theme-specific music notes
            SpawnFogMusicNotes(fog);
            
            // Spawn theme-specific particles
            SpawnThemeParticles(fog);
            
            _activeFogClouds.Add(fog);
            return fog;
        }
        
        /// <summary>
        /// Spawns music notes embedded in the fog cloud.
        /// </summary>
        private static void SpawnFogMusicNotes(ThemedFogCloud fog)
        {
            int noteCount = GetNoteCountForTheme(fog.FogType);
            
            for (int i = 0; i < noteCount; i++)
            {
                var note = new FogMusicNote
                {
                    LocalOffset = Main.rand.NextVector2Circular(fog.Scale * 30f, fog.Scale * 30f),
                    Velocity = Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Scale = Main.rand.NextFloat(0.5f, 0.9f) * GetNoteScaleForTheme(fog.FogType),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.05f, 0.05f),
                    NoteVariant = Main.rand.Next(1, 7),
                    Opacity = Main.rand.NextFloat(0.4f, 0.8f),
                    Lifetime = fog.MaxLifetime,
                    MaxLifetime = fog.MaxLifetime,
                    TintColor = GetNoteColorForTheme(fog.FogType),
                    OrbitAngle = Main.rand.NextFloat(MathHelper.TwoPi),
                    OrbitRadius = Main.rand.NextFloat(15f, 40f) * fog.Scale,
                    OrbitSpeed = Main.rand.NextFloat(0.02f, 0.06f) * (Main.rand.NextBool() ? 1 : -1),
                    Orbiting = Main.rand.NextBool(3) // 1 in 3 notes orbit
                };
                
                fog.MusicNotes.Add(note);
            }
        }
        
        /// <summary>
        /// Spawns theme-specific particles within the fog.
        /// </summary>
        private static void SpawnThemeParticles(ThemedFogCloud fog)
        {
            int particleCount = GetParticleCountForTheme(fog.FogType);
            
            for (int i = 0; i < particleCount; i++)
            {
                var particle = new FogParticle
                {
                    LocalOffset = Main.rand.NextVector2Circular(fog.Scale * 40f, fog.Scale * 40f),
                    Velocity = GetParticleVelocityForTheme(fog.FogType),
                    Scale = Main.rand.NextFloat(0.2f, 0.6f),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    ParticleType = Main.rand.Next(GetParticleTypeCountForTheme(fog.FogType)),
                    Opacity = Main.rand.NextFloat(0.3f, 0.7f),
                    Lifetime = fog.MaxLifetime,
                    Color = GetParticleColorForTheme(fog.FogType)
                };
                
                fog.ThemeParticles.Add(particle);
            }
        }
        
        #endregion
        
        #region Theme-Specific Helpers
        
        private static int GetNoteCountForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Nachtmusik => 8,      // Night music - lots of notes
                ThemedFogType.MoonlightSonata => 6, // Soft piano notes
                ThemedFogType.LaCampanella => 5,    // Bell chimes
                ThemedFogType.Eroica => 4,          // Heroic fanfare
                ThemedFogType.SwanLake => 5,        // Ballet music
                ThemedFogType.Fate => 7,            // Cosmic symphony
                ThemedFogType.Winter => 3,          // Gentle winter song
                ThemedFogType.OdeToJoy => 8,        // Jubilant chorus
                _ => 4
            };
        }
        
        private static float GetNoteScaleForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Fate => 1.2f,         // Large cosmic notes
                ThemedFogType.OdeToJoy => 1.1f,     // Bold notes
                ThemedFogType.LaCampanella => 1.0f, // Standard bell notes
                ThemedFogType.Winter => 0.8f,       // Delicate ice notes
                _ => 0.9f
            };
        }
        
        private static Color GetNoteColorForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Winter => new Color(200, 230, 255, 200),
                ThemedFogType.Nachtmusik => new Color(255, 255, 220, 220),
                ThemedFogType.LaCampanella => new Color(255, 180, 80, 200),
                ThemedFogType.Eroica => new Color(255, 220, 150, 200),
                ThemedFogType.SwanLake => Color.White * 0.9f,
                ThemedFogType.MoonlightSonata => new Color(200, 180, 255, 200),
                ThemedFogType.EnigmaVariations => new Color(150, 100, 220, 180),
                ThemedFogType.Fate => new Color(255, 150, 200, 220),
                ThemedFogType.ClairDeLune => new Color(220, 230, 255, 200),
                ThemedFogType.DiesIrae => new Color(255, 100, 120, 200),
                ThemedFogType.OdeToJoy => new Color(255, 240, 150, 220),
                ThemedFogType.Spring => new Color(200, 255, 200, 200),
                ThemedFogType.Summer => new Color(255, 220, 150, 200),
                ThemedFogType.Autumn => new Color(255, 180, 100, 200),
                _ => Color.White * 0.8f
            };
        }
        
        private static int GetParticleCountForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Winter => 15,          // Snowflakes
                ThemedFogType.Nachtmusik => 20,      // Stars
                ThemedFogType.LaCampanella => 12,    // Embers
                ThemedFogType.SwanLake => 10,        // Feathers
                ThemedFogType.Fate => 18,            // Cosmic dust
                ThemedFogType.EnigmaVariations => 8, // Mystery particles
                _ => 8
            };
        }
        
        private static int GetParticleTypeCountForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Winter => 3,           // Snowflake, ice crystal, frost
                ThemedFogType.Nachtmusik => 2,       // Star, twinkle
                ThemedFogType.LaCampanella => 3,     // Ember, smoke, spark
                ThemedFogType.SwanLake => 2,         // Feather white, feather black
                ThemedFogType.Fate => 4,             // Star, glyph, cosmic dust, sparkle
                _ => 2
            };
        }
        
        private static Vector2 GetParticleVelocityForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Winter => new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.3f, 0.8f)), // Falling snow
                ThemedFogType.Nachtmusik => Main.rand.NextVector2Circular(0.1f, 0.1f), // Gentle twinkle
                ThemedFogType.LaCampanella => new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.3f)), // Rising embers
                ThemedFogType.SwanLake => new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.2f, 0.3f)), // Floating feathers
                ThemedFogType.Fate => Main.rand.NextVector2Circular(0.3f, 0.3f), // Cosmic drift
                _ => Main.rand.NextVector2Circular(0.2f, 0.2f)
            };
        }
        
        private static Color GetParticleColorForTheme(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Winter => ThemeColors.WinterFrost,
                ThemedFogType.Nachtmusik => ThemeColors.NachtmusikStars,
                ThemedFogType.LaCampanella => ThemeColors.CampanellaEmber,
                ThemedFogType.Eroica => ThemeColors.EroicaSakura,
                ThemedFogType.SwanLake => Main.rand.NextBool() ? ThemeColors.SwanPrimary : ThemeColors.SwanSecondary,
                ThemedFogType.MoonlightSonata => ThemeColors.MoonlightSilver,
                ThemedFogType.EnigmaVariations => Main.rand.NextBool() ? ThemeColors.EnigmaSecondary : ThemeColors.EnigmaGreen,
                ThemedFogType.Fate => Main.rand.NextBool() ? ThemeColors.FateStars : ThemeColors.FateSecondary,
                ThemedFogType.ClairDeLune => ThemeColors.ClairPearl,
                ThemedFogType.DiesIrae => ThemeColors.DiesIraeLightning,
                ThemedFogType.OdeToJoy => ThemeColors.OdeSparkle,
                ThemedFogType.Spring => ThemeColors.SpringPetal,
                ThemedFogType.Summer => ThemeColors.SummerGlow,
                ThemedFogType.Autumn => ThemeColors.AutumnLeaf,
                _ => Color.White
            };
        }
        
        #endregion
        
        #region Update Logic
        
        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;
            
            for (int i = _activeFogClouds.Count - 1; i >= 0; i--)
            {
                var fog = _activeFogClouds[i];
                
                if (!fog.Active || fog.Lifetime <= 0)
                {
                    ReturnToPool(fog);
                    _activeFogClouds.RemoveAt(i);
                    continue;
                }
                
                UpdateFogCloud(fog);
                fog.Lifetime--;
            }
        }
        
        private static void UpdateFogCloud(ThemedFogCloud fog)
        {
            // Update position
            fog.Position += fog.Velocity;
            fog.Rotation += fog.RotationSpeed;
            fog.PulsePhase += 0.03f;
            
            // Fade in/out
            float lifeProgress = 1f - (fog.Lifetime / (float)fog.MaxLifetime);
            if (lifeProgress < 0.15f)
                fog.Opacity = lifeProgress / 0.15f;
            else if (lifeProgress > 0.7f)
                fog.Opacity = (1f - lifeProgress) / 0.3f;
            else
                fog.Opacity = 1f;
            
            // Update theme-specific behavior
            UpdateThemeBehavior(fog);
            
            // Update music notes
            foreach (var note in fog.MusicNotes)
            {
                if (note.Orbiting)
                {
                    note.OrbitAngle += note.OrbitSpeed;
                    note.LocalOffset = note.OrbitAngle.ToRotationVector2() * note.OrbitRadius;
                }
                else
                {
                    note.LocalOffset += note.Velocity;
                }
                note.Rotation += note.RotationSpeed;
                
                // Shimmer effect
                note.Scale = note.Scale * 0.98f + (Main.rand.NextFloat(0.5f, 0.9f) * GetNoteScaleForTheme(fog.FogType)) * 0.02f;
            }
            
            // Update particles
            foreach (var particle in fog.ThemeParticles)
            {
                particle.LocalOffset += particle.Velocity;
                particle.Rotation += 0.02f;
                
                // Keep particles within fog bounds
                float maxDist = fog.Scale * 50f;
                if (particle.LocalOffset.Length() > maxDist)
                {
                    particle.LocalOffset = Main.rand.NextVector2Circular(maxDist * 0.3f, maxDist * 0.3f);
                }
            }
        }
        
        private static void UpdateThemeBehavior(ThemedFogCloud fog)
        {
            switch (fog.FogType)
            {
                case ThemedFogType.Winter:
                    // Gentle downward drift with slight horizontal sway
                    fog.Velocity.Y += 0.01f;
                    fog.Velocity.X += (float)Math.Sin(fog.PulsePhase) * 0.005f;
                    fog.Velocity.Y = Math.Min(fog.Velocity.Y, 0.5f);
                    break;
                    
                case ThemedFogType.Nachtmusik:
                    // Very slow drift with twinkling scale
                    fog.SecondaryScale = fog.Scale * (0.9f + (float)Math.Sin(fog.PulsePhase * 2f) * 0.1f);
                    break;
                    
                case ThemedFogType.LaCampanella:
                    // Rising smoke with turbulence
                    fog.Velocity.Y -= 0.02f;
                    fog.Velocity.X += (float)Math.Sin(fog.PulsePhase * 1.5f) * 0.01f;
                    fog.Velocity.Y = Math.Max(fog.Velocity.Y, -1f);
                    break;
                    
                case ThemedFogType.SwanLake:
                    // Graceful floating with gentle rotation
                    fog.RotationSpeed = (float)Math.Sin(fog.PulsePhase) * 0.01f;
                    break;
                    
                case ThemedFogType.Fate:
                    // Cosmic swirl with scale pulsing
                    fog.RotationSpeed = 0.02f;
                    fog.SecondaryScale = fog.Scale * (0.85f + (float)Math.Sin(fog.PulsePhase * 3f) * 0.15f);
                    break;
                    
                case ThemedFogType.DiesIrae:
                    // Stormy turbulence
                    fog.Velocity += Main.rand.NextVector2Circular(0.05f, 0.05f);
                    fog.Velocity *= 0.98f;
                    break;
            }
        }
        
        #endregion
        
        #region Drawing
        
        public static void DrawAllThemedFogs(SpriteBatch spriteBatch)
        {
            if (_activeFogClouds.Count == 0)
                return;
            
            // Load textures
            Texture2D fogTexture = NebulaFogSystem.GetFogTexture();
            Texture2D[] noteTextures = GetMusicNoteTextures();
            
            foreach (var fog in _activeFogClouds)
            {
                if (!fog.Active || fog.Opacity <= 0)
                    continue;
                
                DrawThemedFogCloud(spriteBatch, fog, fogTexture, noteTextures);
            }
        }
        
        private static void DrawThemedFogCloud(SpriteBatch spriteBatch, ThemedFogCloud fog, Texture2D fogTexture, Texture2D[] noteTextures)
        {
            Vector2 screenPos = fog.Position - Main.screenPosition;
            
            // Get theme colors
            var (primary, secondary, accent) = GetThemeColors(fog.FogType);
            
            // Draw fog layers with theme-specific blend
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer glow layer
            float outerScale = fog.Scale * 1.4f * (1f + (float)Math.Sin(fog.PulsePhase) * 0.1f);
            spriteBatch.Draw(fogTexture, screenPos, null, primary * fog.Opacity * 0.3f, 
                fog.Rotation, fogTexture.Size() / 2f, outerScale, SpriteEffects.None, 0f);
            
            // Main fog layer
            spriteBatch.Draw(fogTexture, screenPos, null, secondary * fog.Opacity * 0.5f, 
                fog.Rotation * 0.7f, fogTexture.Size() / 2f, fog.Scale, SpriteEffects.None, 0f);
            
            // Inner bright layer
            float innerScale = fog.SecondaryScale * 0.6f;
            spriteBatch.Draw(fogTexture, screenPos, null, accent * fog.Opacity * 0.4f, 
                -fog.Rotation * 0.5f, fogTexture.Size() / 2f, innerScale, SpriteEffects.None, 0f);
            
            // Draw theme-specific particles
            DrawThemeParticles(spriteBatch, fog, screenPos);
            
            // Draw music notes
            DrawFogMusicNotes(spriteBatch, fog, screenPos, noteTextures);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        
        private static void DrawThemeParticles(SpriteBatch spriteBatch, ThemedFogCloud fog, Vector2 screenPos)
        {
            Texture2D sparkleTexture = null;
            
            try
            {
                sparkleTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/TwinkleSparkle", 
                    ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
            catch { return; }
            
            foreach (var particle in fog.ThemeParticles)
            {
                Vector2 particlePos = screenPos + particle.LocalOffset;
                float particleOpacity = particle.Opacity * fog.Opacity;
                
                // Scale based on particle type for variety
                float scale = particle.Scale * (0.8f + particle.ParticleType * 0.2f);
                
                // Draw with theme-specific coloring
                Color particleColor = particle.Color * particleOpacity;
                
                spriteBatch.Draw(sparkleTexture, particlePos, null, particleColor,
                    particle.Rotation, sparkleTexture.Size() / 2f, scale, SpriteEffects.None, 0f);
                
                // Add extra glow for stars in Nachtmusik
                if (fog.FogType == ThemedFogType.Nachtmusik && particle.ParticleType == 0)
                {
                    spriteBatch.Draw(sparkleTexture, particlePos, null, particleColor * 0.5f,
                        particle.Rotation, sparkleTexture.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0f);
                }
            }
        }
        
        private static void DrawFogMusicNotes(SpriteBatch spriteBatch, ThemedFogCloud fog, Vector2 screenPos, Texture2D[] noteTextures)
        {
            if (noteTextures == null || noteTextures.Length == 0)
                return;
            
            foreach (var note in fog.MusicNotes)
            {
                int textureIndex = Math.Clamp(note.NoteVariant - 1, 0, noteTextures.Length - 1);
                Texture2D noteTex = noteTextures[textureIndex];
                
                if (noteTex == null)
                    continue;
                
                Vector2 notePos = screenPos + note.LocalOffset;
                float noteOpacity = note.Opacity * fog.Opacity;
                
                // Shimmer effect
                float shimmer = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f + note.OrbitAngle) * 0.15f;
                float finalScale = note.Scale * shimmer;
                
                // Multi-layer bloom for visibility
                Color bloomColor = note.TintColor with { A = 0 };
                
                // Outer bloom
                spriteBatch.Draw(noteTex, notePos, null, bloomColor * noteOpacity * 0.3f,
                    note.Rotation, noteTex.Size() / 2f, finalScale * 1.6f, SpriteEffects.None, 0f);
                
                // Middle bloom
                spriteBatch.Draw(noteTex, notePos, null, bloomColor * noteOpacity * 0.5f,
                    note.Rotation, noteTex.Size() / 2f, finalScale * 1.3f, SpriteEffects.None, 0f);
                
                // Core note
                spriteBatch.Draw(noteTex, notePos, null, note.TintColor * noteOpacity,
                    note.Rotation, noteTex.Size() / 2f, finalScale, SpriteEffects.None, 0f);
            }
        }
        
        private static (Color primary, Color secondary, Color accent) GetThemeColors(ThemedFogType type)
        {
            return type switch
            {
                ThemedFogType.Winter => (ThemeColors.WinterPrimary, ThemeColors.WinterSecondary, ThemeColors.WinterAccent),
                ThemedFogType.Nachtmusik => (ThemeColors.NachtmusikPrimary, ThemeColors.NachtmusikSecondary, ThemeColors.NachtmusikMoon),
                ThemedFogType.LaCampanella => (ThemeColors.CampanellaSmoke, ThemeColors.CampanellaSecondary, ThemeColors.CampanellaEmber),
                ThemedFogType.Eroica => (ThemeColors.EroicaPrimary, ThemeColors.EroicaSecondary, ThemeColors.EroicaGlow),
                ThemedFogType.SwanLake => (ThemeColors.SwanSecondary, ThemeColors.SwanPrimary, ThemeColors.SwanPrismatic),
                ThemedFogType.MoonlightSonata => (ThemeColors.MoonlightPrimary, ThemeColors.MoonlightSecondary, ThemeColors.MoonlightGlow),
                ThemedFogType.EnigmaVariations => (ThemeColors.EnigmaVoid, ThemeColors.EnigmaSecondary, ThemeColors.EnigmaGreen),
                ThemedFogType.Fate => (ThemeColors.FatePrimary, ThemeColors.FateSecondary, ThemeColors.FateCrimson),
                ThemedFogType.ClairDeLune => (ThemeColors.ClairPrimary, ThemeColors.ClairSecondary, ThemeColors.ClairPearl),
                ThemedFogType.DiesIrae => (ThemeColors.DiesIraePrimary, ThemeColors.DiesIraeSecondary, ThemeColors.DiesIraeLightning),
                ThemedFogType.OdeToJoy => (ThemeColors.OdePrimary, ThemeColors.OdeSecondary, ThemeColors.OdeSparkle),
                ThemedFogType.Spring => (ThemeColors.SpringPrimary, ThemeColors.SpringSecondary, ThemeColors.SpringPetal),
                ThemedFogType.Summer => (ThemeColors.SummerPrimary, ThemeColors.SummerSecondary, ThemeColors.SummerGlow),
                ThemedFogType.Autumn => (ThemeColors.AutumnPrimary, ThemeColors.AutumnSecondary, ThemeColors.AutumnLeaf),
                _ => (Color.Gray, Color.White, Color.LightGray)
            };
        }
        
        private static Texture2D[] GetMusicNoteTextures()
        {
            try
            {
                return new Texture2D[]
                {
                    ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNote", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/CursiveMusicNote", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MusicNoteWithSlashes", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/QuarterNote", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/TallMusicNote", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                    ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/WholeNote", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value
                };
            }
            catch
            {
                return new Texture2D[0];
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Spawns a Winter-themed icy fog with frost sparkles and snowflake patterns.
        /// </summary>
        public static ThemedFogCloud SpawnWinterFog(Vector2 position, float scale = 1f, int lifetime = 150)
        {
            var fog = SpawnThemedFog(position, ThemedFogType.Winter, scale, lifetime);
            if (fog != null)
            {
                fog.Velocity.Y = 0.3f; // Gentle falling
            }
            return fog;
        }
        
        /// <summary>
        /// Spawns a Nachtmusik starry night fog with twinkling stars.
        /// </summary>
        public static ThemedFogCloud SpawnNachtmusikFog(Vector2 position, float scale = 1.5f, int lifetime = 180)
        {
            return SpawnThemedFog(position, ThemedFogType.Nachtmusik, scale, lifetime);
        }
        
        /// <summary>
        /// Spawns a La Campanella infernal smoke fog with rising embers.
        /// </summary>
        public static ThemedFogCloud SpawnCampanellaFog(Vector2 position, float scale = 1f, int lifetime = 120)
        {
            var fog = SpawnThemedFog(position, ThemedFogType.LaCampanella, scale, lifetime);
            if (fog != null)
            {
                fog.Velocity.Y = -0.5f; // Rising smoke
            }
            return fog;
        }
        
        /// <summary>
        /// Spawns an Eroica heroic golden mist with sakura petals.
        /// </summary>
        public static ThemedFogCloud SpawnEroicaFog(Vector2 position, float scale = 1f, int lifetime = 120)
        {
            return SpawnThemedFog(position, ThemedFogType.Eroica, scale, lifetime);
        }
        
        /// <summary>
        /// Spawns a Swan Lake elegant monochrome fog with feathers.
        /// </summary>
        public static ThemedFogCloud SpawnSwanLakeFog(Vector2 position, float scale = 1.2f, int lifetime = 150)
        {
            return SpawnThemedFog(position, ThemedFogType.SwanLake, scale, lifetime);
        }
        
        /// <summary>
        /// Spawns a Moonlight Sonata lunar mist with silver highlights.
        /// </summary>
        public static ThemedFogCloud SpawnMoonlightFog(Vector2 position, float scale = 1f, int lifetime = 140)
        {
            return SpawnThemedFog(position, ThemedFogType.MoonlightSonata, scale, lifetime);
        }
        
        /// <summary>
        /// Spawns an Enigma void fog with mystery particles.
        /// </summary>
        public static ThemedFogCloud SpawnEnigmaFog(Vector2 position, float scale = 1f, int lifetime = 130)
        {
            return SpawnThemedFog(position, ThemedFogType.EnigmaVariations, scale, lifetime);
        }
        
        /// <summary>
        /// Spawns a Fate cosmic nebula fog with constellation patterns.
        /// </summary>
        public static ThemedFogCloud SpawnFateFog(Vector2 position, float scale = 1.5f, int lifetime = 160)
        {
            return SpawnThemedFog(position, ThemedFogType.Fate, scale, lifetime);
        }
        
        /// <summary>
        /// Spawns fog based on theme name string.
        /// </summary>
        public static ThemedFogCloud SpawnFogByThemeName(string themeName, Vector2 position, float scale = 1f, int lifetime = 120)
        {
            ThemedFogType type = themeName.ToLower() switch
            {
                "winter" => ThemedFogType.Winter,
                "spring" => ThemedFogType.Spring,
                "summer" => ThemedFogType.Summer,
                "autumn" => ThemedFogType.Autumn,
                "lacampanella" or "campanella" => ThemedFogType.LaCampanella,
                "eroica" => ThemedFogType.Eroica,
                "swanlake" or "swan" => ThemedFogType.SwanLake,
                "moonlightsonata" or "moonlight" => ThemedFogType.MoonlightSonata,
                "enigmavariations" or "enigma" => ThemedFogType.EnigmaVariations,
                "fate" => ThemedFogType.Fate,
                "clairdelune" or "clair" => ThemedFogType.ClairDeLune,
                "diesirae" or "dies" => ThemedFogType.DiesIrae,
                "odetojoy" or "ode" => ThemedFogType.OdeToJoy,
                "nachtmusik" or "nacht" => ThemedFogType.Nachtmusik,
                _ => ThemedFogType.MoonlightSonata
            };
            
            return SpawnThemedFog(position, type, scale, lifetime);
        }
        
        #endregion
    }
}
