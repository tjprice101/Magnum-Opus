using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// UNIQUE TRAIL STYLES - 107 Particle PNG Integration
    /// 
    /// This system provides distinct visual trail effects for each weapon type using
    /// MagnumOpus's comprehensive particle asset library. Every weapon gets a UNIQUE
    /// trail based on its theme and weapon class.
    /// 
    /// Trail Categories:
    /// - SwanFeather (10 variants) - Graceful, floating trails for Swan Lake theme
    /// - SwordArc (9 variants) - Melee slash arcs for all sword weapons
    /// - MusicNote (6 variants) - Musical trails for general music theming
    /// - ParticleTrail (4 variants) - Generic flowing trails
    /// - Glyphs (12 variants) - Arcane magical trails for Enigma/Fate
    /// - EnigmaEye (8 variants) - Watching eye particles for Enigma
    /// - SoftGlow (3 variants) - Ambient glow trails
    /// - EnergyFlare (2 variants) - Intense energy effects
    /// 
    /// Each theme gets a UNIQUE combination of these assets!
    /// </summary>
    public static class UniqueTrailStyles
    {
        #region Particle Texture Caches
        
        // Cached texture references for performance
        private static Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
        
        // Asset path prefix
        private const string ParticlePath = "MagnumOpus/Assets/Particles/";
        
        /// <summary>
        /// Gets a particle texture by name, with caching for performance.
        /// </summary>
        public static Texture2D GetParticleTexture(string name)
        {
            if (_textureCache.TryGetValue(name, out var cached))
                return cached;
            
            try
            {
                var tex = ModContent.Request<Texture2D>(ParticlePath + name, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                _textureCache[name] = tex;
                return tex;
            }
            catch
            {
                return null;
            }
        }
        
        #endregion
        
        #region Theme-Specific Trail Configurations
        
        /// <summary>
        /// Trail configuration for each weapon type within a theme.
        /// </summary>
        public struct TrailConfig
        {
            public string[] ParticleTextures;      // Array of texture names to mix
            public float BaseScale;                 // Base particle scale
            public float ScaleVariation;           // Random scale variation
            public int ParticlesPerFrame;          // How dense the trail is
            public float SpreadRadius;             // How wide particles spread
            public float VelocityInheritance;      // How much parent velocity affects particles
            public float OrbitRadius;              // For orbiting particles
            public int OrbitCount;                 // Number of orbiting particles
            public bool UseBloom;                  // Enable multi-layer bloom
            public int BloomLayers;                // Number of bloom layers
            public float BloomIntensity;           // Bloom brightness
            public bool UseMusicNotes;             // Include music note particles
            public float RotationSpeed;            // Particle spin speed
            public float Lifetime;                 // Base particle lifetime in frames
            public float FadeInRatio;              // How fast particles fade in
            public float FadeOutRatio;             // How fast particles fade out
            
            public static TrailConfig Default => new TrailConfig
            {
                ParticleTextures = new[] { "SoftGlow2" },
                BaseScale = 0.3f,
                ScaleVariation = 0.1f,
                ParticlesPerFrame = 2,
                SpreadRadius = 5f,
                VelocityInheritance = 0.15f,
                OrbitRadius = 0f,
                OrbitCount = 0,
                UseBloom = true,
                BloomLayers = 3,
                BloomIntensity = 0.7f,
                UseMusicNotes = true,
                RotationSpeed = 0.05f,
                Lifetime = 25f,
                FadeInRatio = 0.1f,
                FadeOutRatio = 0.3f
            };
        }
        
        /// <summary>
        /// Gets the unique trail configuration for a theme + weapon class combination.
        /// </summary>
        public static TrailConfig GetTrailConfig(string theme, DamageClass damageClass)
        {
            // Each theme-class combination gets UNIQUE particle mixes
            string themeLower = theme.ToLowerInvariant();
            
            // === SWAN LAKE ===
            if (themeLower.Contains("swan"))
            {
                if (damageClass == DamageClass.Melee)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "SwanFeather1", "SwanFeather2", "SwanFeather3", "SwordArc2", "MusicNote" },
                        BaseScale = 0.4f,
                        ScaleVariation = 0.15f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 8f,
                        VelocityInheritance = 0.2f,
                        OrbitRadius = 18f,
                        OrbitCount = 3,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 0.9f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.03f,
                        Lifetime = 35f,
                        FadeInRatio = 0.15f,
                        FadeOutRatio = 0.4f
                    };
                else if (damageClass == DamageClass.Ranged)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "SwanFeather4", "SwanFeather5", "PrismaticSparkle11", "ParticleTrail1" },
                        BaseScale = 0.35f,
                        ScaleVariation = 0.1f,
                        ParticlesPerFrame = 2,
                        SpreadRadius = 6f,
                        VelocityInheritance = 0.25f,
                        OrbitRadius = 12f,
                        OrbitCount = 2,
                        UseBloom = true,
                        BloomLayers = 3,
                        BloomIntensity = 0.85f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.02f,
                        Lifetime = 28f,
                        FadeInRatio = 0.1f,
                        FadeOutRatio = 0.35f
                    };
                else if (damageClass == DamageClass.Magic)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "SwanFeather6", "SwanFeather7", "MagicSparklField4", "CircularStarRing" },
                        BaseScale = 0.45f,
                        ScaleVariation = 0.2f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 10f,
                        VelocityInheritance = 0.15f,
                        OrbitRadius = 22f,
                        OrbitCount = 4,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.0f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.06f,
                        Lifetime = 40f,
                        FadeInRatio = 0.2f,
                        FadeOutRatio = 0.5f
                    };
                else
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "SwanFeather8", "SwanFeather9", "TwinkleSparkle" },
                        BaseScale = 0.38f,
                        ScaleVariation = 0.12f,
                        ParticlesPerFrame = 2,
                        SpreadRadius = 7f,
                        VelocityInheritance = 0.18f,
                        OrbitRadius = 15f,
                        OrbitCount = 3,
                        UseBloom = true,
                        BloomLayers = 3,
                        BloomIntensity = 0.8f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.04f,
                        Lifetime = 30f,
                        FadeInRatio = 0.12f,
                        FadeOutRatio = 0.38f
                    };
            }
            
            // === EROICA ===
            else if (themeLower.Contains("eroica"))
            {
                if (damageClass == DamageClass.Melee)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "SwordArc1", "SwordArc3", "SwordArc6", "FlameImpactExplosion", "MusicNote" },
                        BaseScale = 0.5f,
                        ScaleVariation = 0.15f,
                        ParticlesPerFrame = 4,
                        SpreadRadius = 10f,
                        VelocityInheritance = 0.3f,
                        OrbitRadius = 20f,
                        OrbitCount = 4,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.1f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.08f,
                        Lifetime = 30f,
                        FadeInRatio = 0.1f,
                        FadeOutRatio = 0.3f
                    };
                else if (damageClass == DamageClass.Ranged)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "EnergyFlare", "EnergyFlare4", "FlareSparkle", "ParticleTrail2" },
                        BaseScale = 0.4f,
                        ScaleVariation = 0.12f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 7f,
                        VelocityInheritance = 0.35f,
                        OrbitRadius = 14f,
                        OrbitCount = 3,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.0f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.05f,
                        Lifetime = 25f,
                        FadeInRatio = 0.08f,
                        FadeOutRatio = 0.25f
                    };
                else
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "StarBurst1", "GlowingHalo1", "MagicSparklField6", "TallMusicNote" },
                        BaseScale = 0.42f,
                        ScaleVariation = 0.15f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 8f,
                        VelocityInheritance = 0.22f,
                        OrbitRadius = 18f,
                        OrbitCount = 3,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 0.95f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.06f,
                        Lifetime = 32f,
                        FadeInRatio = 0.12f,
                        FadeOutRatio = 0.35f
                    };
            }
            
            // === LA CAMPANELLA ===
            else if (themeLower.Contains("campanella"))
            {
                if (damageClass == DamageClass.Melee)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "FlamingArcSwordSlash", "SwordArc8", "FlameWispImpactExplosion", "MusicNoteWithSlashes" },
                        BaseScale = 0.55f,
                        ScaleVariation = 0.18f,
                        ParticlesPerFrame = 4,
                        SpreadRadius = 12f,
                        VelocityInheritance = 0.28f,
                        OrbitRadius = 22f,
                        OrbitCount = 5,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.2f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.1f,
                        Lifetime = 28f,
                        FadeInRatio = 0.08f,
                        FadeOutRatio = 0.25f
                    };
                else
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "FlamingWispProjectileSmall", "TallFlamingWispProjectile", "LightningStreak" },
                        BaseScale = 0.45f,
                        ScaleVariation = 0.15f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 9f,
                        VelocityInheritance = 0.32f,
                        OrbitRadius = 16f,
                        OrbitCount = 4,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.1f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.07f,
                        Lifetime = 24f,
                        FadeInRatio = 0.1f,
                        FadeOutRatio = 0.28f
                    };
            }
            
            // === ENIGMA VARIATIONS ===
            else if (themeLower.Contains("enigma"))
            {
                if (damageClass == DamageClass.Melee)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "CurvedSwordSlash", "EnigmaEye1", "Glyphs1", "Glyphs2", "Glyphs3" },
                        BaseScale = 0.48f,
                        ScaleVariation = 0.16f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 10f,
                        VelocityInheritance = 0.2f,
                        OrbitRadius = 25f,
                        OrbitCount = 4,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 0.95f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.04f,
                        Lifetime = 38f,
                        FadeInRatio = 0.15f,
                        FadeOutRatio = 0.45f
                    };
                else if (damageClass == DamageClass.Magic)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "ActivatedEnigmaEye", "BurstingEye", "CircularEnigmaEye", "Glyphs4", "Glyphs5" },
                        BaseScale = 0.5f,
                        ScaleVariation = 0.2f,
                        ParticlesPerFrame = 4,
                        SpreadRadius = 14f,
                        VelocityInheritance = 0.15f,
                        OrbitRadius = 28f,
                        OrbitCount = 5,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.05f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.05f,
                        Lifetime = 42f,
                        FadeInRatio = 0.18f,
                        FadeOutRatio = 0.5f
                    };
                else
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "GodEye", "SpikeyEye", "TriangularEye", "Glyphs6", "Glyphs7" },
                        BaseScale = 0.45f,
                        ScaleVariation = 0.14f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 11f,
                        VelocityInheritance = 0.18f,
                        OrbitRadius = 22f,
                        OrbitCount = 3,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 0.9f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.045f,
                        Lifetime = 35f,
                        FadeInRatio = 0.14f,
                        FadeOutRatio = 0.4f
                    };
            }
            
            // === FATE ===
            else if (themeLower.Contains("fate"))
            {
                if (damageClass == DamageClass.Melee)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "SwordArcSlashWave", "SimpleArcSwordSlash", "Glyphs8", "Glyphs9", "StarBurst2", "ConstellationStyleSparkle" },
                        BaseScale = 0.52f,
                        ScaleVariation = 0.18f,
                        ParticlesPerFrame = 4,
                        SpreadRadius = 12f,
                        VelocityInheritance = 0.25f,
                        OrbitRadius = 24f,
                        OrbitCount = 5,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.15f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.06f,
                        Lifetime = 35f,
                        FadeInRatio = 0.1f,
                        FadeOutRatio = 0.35f
                    };
                else if (damageClass == DamageClass.Magic)
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "Glyphs10", "Glyphs11", "Glyphs12", "ShatteredStarlight", "Star", "CrescentSparkleMoon" },
                        BaseScale = 0.55f,
                        ScaleVariation = 0.22f,
                        ParticlesPerFrame = 5,
                        SpreadRadius = 16f,
                        VelocityInheritance = 0.18f,
                        OrbitRadius = 30f,
                        OrbitCount = 6,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.25f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.07f,
                        Lifetime = 45f,
                        FadeInRatio = 0.15f,
                        FadeOutRatio = 0.5f
                    };
                else
                    return new TrailConfig
                    {
                        ParticleTextures = new[] { "StarryStarburst", "CircularStarRing", "Glyphs1", "TwilightSparkle" },
                        BaseScale = 0.48f,
                        ScaleVariation = 0.16f,
                        ParticlesPerFrame = 3,
                        SpreadRadius = 10f,
                        VelocityInheritance = 0.22f,
                        OrbitRadius = 20f,
                        OrbitCount = 4,
                        UseBloom = true,
                        BloomLayers = 4,
                        BloomIntensity = 1.1f,
                        UseMusicNotes = true,
                        RotationSpeed = 0.055f,
                        Lifetime = 38f,
                        FadeInRatio = 0.12f,
                        FadeOutRatio = 0.42f
                    };
            }
            
            // === MOONLIGHT SONATA ===
            else if (themeLower.Contains("moonlight"))
            {
                return new TrailConfig
                {
                    ParticleTextures = new[] { "SoftGlow2", "SoftGlow3", "MagicSparklField7", "MagicSparklField8", "CursiveMusicNote" },
                    BaseScale = 0.4f,
                    ScaleVariation = 0.15f,
                    ParticlesPerFrame = 3,
                    SpreadRadius = 9f,
                    VelocityInheritance = 0.18f,
                    OrbitRadius = 20f,
                    OrbitCount = 4,
                    UseBloom = true,
                    BloomLayers = 4,
                    BloomIntensity = 0.85f,
                    UseMusicNotes = true,
                    RotationSpeed = 0.03f,
                    Lifetime = 40f,
                    FadeInRatio = 0.2f,
                    FadeOutRatio = 0.5f
                };
            }
            
            // === DEFAULT / GENERIC ===
            return TrailConfig.Default;
        }
        
        #endregion
        
        #region Trail Rendering Methods
        
        /// <summary>
        /// Spawns a unique theme-based trail at the given position.
        /// Uses the weapon's theme and damage class to determine particle mix.
        /// </summary>
        public static void SpawnUniqueTrail(Vector2 position, Vector2 velocity, string theme, DamageClass damageClass, Color[] palette)
        {
            var config = GetTrailConfig(theme, damageClass);
            
            if (palette == null || palette.Length == 0)
                palette = new[] { Color.White };
            
            float time = Main.GameUpdateCount * 0.05f;
            
            // === MAIN TRAIL PARTICLES ===
            for (int i = 0; i < config.ParticlesPerFrame; i++)
            {
                // Select random texture from the config's mix
                string textureName = config.ParticleTextures[Main.rand.Next(config.ParticleTextures.Length)];
                
                // Calculate position with spread
                Vector2 offset = Main.rand.NextVector2Circular(config.SpreadRadius, config.SpreadRadius);
                Vector2 spawnPos = position + offset;
                
                // Calculate velocity with inheritance
                Vector2 particleVel = -velocity * config.VelocityInheritance + Main.rand.NextVector2Circular(1f, 1f);
                
                // Calculate scale with variation
                float scale = config.BaseScale + Main.rand.NextFloat(-config.ScaleVariation, config.ScaleVariation);
                
                // Get gradient color
                float colorProgress = (time + i * 0.2f) % 1f;
                Color color = VFXUtilities.PaletteLerp(palette, colorProgress);
                
                // Calculate lifetime
                int lifetime = (int)(config.Lifetime + Main.rand.NextFloat(-5f, 5f));
                
                // Create the particle based on texture type
                SpawnStyledParticle(spawnPos, particleVel, textureName, color, scale, lifetime, config);
            }
            
            // === ORBITING PARTICLES ===
            if (config.OrbitCount > 0 && Main.rand.NextBool(4))
            {
                float orbitAngle = time * 0.08f;
                for (int i = 0; i < config.OrbitCount; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / config.OrbitCount;
                    float radius = config.OrbitRadius + (float)Math.Sin(time * 0.15f + i) * 4f;
                    Vector2 orbitPos = position + angle.ToRotationVector2() * radius;
                    
                    // Pick orbiting particle texture (prefer glows and sparkles)
                    string orbitTex = Main.rand.NextBool() ? "FlareSparkle" : "TwinkleSparkle";
                    
                    Color orbitColor = VFXUtilities.PaletteLerp(palette, (float)i / config.OrbitCount);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.7f, 0.2f, 8);
                }
            }
            
            // === MUSIC NOTES ===
            if (config.UseMusicNotes && Main.rand.NextBool(8))
            {
                string[] noteTextures = { "MusicNote", "CursiveMusicNote", "QuarterNote", "TallMusicNote", "WholeNote", "MusicNoteWithSlashes" };
                string noteTex = noteTextures[Main.rand.Next(noteTextures.Length)];
                
                Vector2 notePos = position + Main.rand.NextVector2Circular(config.SpreadRadius * 1.5f, config.SpreadRadius * 1.5f);
                Vector2 noteVel = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color noteColor = palette.Length > 2 ? palette[2] : palette[palette.Length - 1];
                
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 30);
            }
        }
        
        /// <summary>
        /// Spawns a particle with the appropriate style based on texture name.
        /// </summary>
        private static void SpawnStyledParticle(Vector2 position, Vector2 velocity, string textureName, Color color, float scale, int lifetime, TrailConfig config)
        {
            // Determine particle type based on texture name
            if (textureName.Contains("SwordArc") || textureName.Contains("Slash"))
            {
                // Use glow spark for sword arcs
                var spark = new GlowSparkParticle(position, velocity, false, lifetime, scale,
                    color, new Vector2(0.5f, 2f), false, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            else if (textureName.Contains("Feather"))
            {
                // Feathers get gentle drift
                Vector2 driftVel = velocity + new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f);
                var glow = new GenericGlowParticle(position, driftVel, color * 0.85f, scale * 1.2f, lifetime, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            else if (textureName.Contains("Eye") || textureName.Contains("Glyph"))
            {
                // Eyes and glyphs get slow rotation
                var glow = new GenericGlowParticle(position, velocity * 0.5f, color * 0.9f, scale, lifetime, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            else if (textureName.Contains("Sparkle") || textureName.Contains("Star"))
            {
                // Sparkles and stars
                var sparkle = new SparkleParticle(position, velocity, color, scale * 0.8f, lifetime);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            else if (textureName.Contains("Flame") || textureName.Contains("Fire"))
            {
                // Flames rise
                Vector2 flameVel = velocity + new Vector2(0, -2f);
                var glow = new GenericGlowParticle(position, flameVel, color, scale * 1.1f, lifetime, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            else
            {
                // Default glow particle
                var glow = new GenericGlowParticle(position, velocity, color * 0.8f, scale, lifetime, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Add bloom layers if enabled
            if (config.UseBloom && Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(position, color * config.BloomIntensity * 0.4f, scale * 0.8f, lifetime / 2);
            }
        }
        
        /// <summary>
        /// Spawns a dense impact effect using the theme's particle configuration.
        /// </summary>
        public static void SpawnUniqueImpact(Vector2 position, string theme, DamageClass damageClass, Color[] palette, float scale = 1f)
        {
            var config = GetTrailConfig(theme, damageClass);
            
            if (palette == null || palette.Length == 0)
                palette = new[] { Color.White };
            
            // === CENTRAL FLARE CASCADE ===
            for (int i = 0; i < 4; i++)
            {
                Color layerColor = Color.Lerp(Color.White, palette[0], i / 4f);
                CustomParticles.GenericFlare(position, layerColor, (0.8f - i * 0.15f) * scale, 15 - i * 2);
            }
            
            // === HALO RINGS ===
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = VFXUtilities.PaletteLerp(palette, i / 5f);
                CustomParticles.HaloRing(position, ringColor * (1f - i * 0.15f), (0.25f + i * 0.1f) * scale, 12 + i * 3);
            }
            
            // === RADIAL PARTICLE BURST ===
            int burstCount = config.ParticlesPerFrame * 4;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f) * scale;
                
                string textureName = config.ParticleTextures[Main.rand.Next(config.ParticleTextures.Length)];
                Color burstColor = VFXUtilities.PaletteLerp(palette, (float)i / burstCount);
                
                SpawnStyledParticle(position, burstVel, textureName, burstColor, config.BaseScale * 1.2f, (int)config.Lifetime, config);
            }
            
            // === MUSIC NOTE BURST ===
            if (config.UseMusicNotes)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                    Color noteColor = palette.Length > 1 ? palette[1] : palette[0];
                    ThemedParticles.MusicNote(position, noteVel, noteColor, 0.8f * scale, 35);
                }
            }
            
            // === SCREEN DISTORTION ===
            ScreenDistortionManager.TriggerThemeEffect(theme, position, 0.3f * scale, 15);
        }
        
        #endregion
    }
}
