using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// UNIQUE WEAPON VFX STYLES PER THEME
    /// 
    /// Every weapon gets:
    /// 1. UNIQUE FOG VISUALS on attack - Theme-specific nebula/mist effects
    /// 2. LIGHT BEAM IMPACTS - Stretching rays on tile/enemy hits
    /// 3. UNIQUE PROJECTILE RENDERS - Interpolated, theme-specific drawing
    /// 4. UNIQUE TRAIL STYLES - Per-score visual identity
    /// 
    /// All effects use SUB-PIXEL INTERPOLATION for 144Hz+ smoothness.
    /// </summary>
    public static class UniqueWeaponVFXStyles
    {
        #region Theme Style Definitions
        
        /// <summary>
        /// Complete VFX style configuration for a theme.
        /// </summary>
        public class ThemeVFXStyle
        {
            public string ThemeName;
            
            // Fog properties
            public FogStyle Fog;
            
            // Light beam impact properties
            public LightBeamStyle LightBeams;
            
            // Projectile render properties
            public ProjectileRenderStyle ProjectileRender;
            
            // Trail properties
            public TrailRenderStyle Trail;
        }
        
        public class FogStyle
        {
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float Density;
            public float Turbulence; // How much the fog swirls
            public float SparkleIntensity; // Constellation/star effects in fog
            public FogType Type;
            public float ExpansionSpeed;
            public int Lifetime;
        }
        
        public enum FogType
        {
            SoftCloud,          // Gentle, billowing clouds
            CosmicNebula,       // Star-filled cosmic fog
            InfernalSmoke,      // Heavy, dark smoke with embers
            EtherealMist,       // Wispy, ghostly fog
            VoidRift,           // Dark, swirling void energy
            CrystallineHaze,    // Sparkly, prismatic mist
            SakuraPetals,       // Flower petal fog
            MoonlightGlow,      // Soft lunar radiance
            ElectricalStorm,    // Crackling energy fog
            FrostCloud          // Icy, crystalline fog
        }
        
        public class LightBeamStyle
        {
            public Color CoreColor;
            public Color EdgeColor;
            public int RayCount;
            public float BaseLength;
            public float BaseWidth;
            public float StretchSpeed;
            public float ShimmerIntensity;
            public bool IncludeMusicNotes;
            public LightBeamType Type;
        }
        
        public enum LightBeamType
        {
            Radial,             // Even spread outward
            Directional,        // Aligned with impact direction
            Starburst,          // Sharp, star-shaped
            Crescent,           // Arc-shaped spread
            Constellation,      // Connected star points
            FireBurst,          // Flame-like rays
            VoidTendrils,       // Dark, reaching rays
            PrismaticSpray      // Rainbow-colored beams
        }
        
        public class ProjectileRenderStyle
        {
            public Color CoreGlow;
            public Color OuterGlow;
            public Color TrailColor;
            public float CoreScale;
            public float GlowLayers;
            public float PulseSpeed;
            public float RotationSpeed;
            public ProjectileVisualType VisualType;
            public bool HasOrbitingElements;
            public int OrbitCount;
            public float OrbitRadius;
        }
        
        public enum ProjectileVisualType
        {
            EnergyOrb,          // Glowing sphere
            CosmicStar,         // Star-shaped with trails
            FlameBolt,          // Fire projectile
            VoidShard,          // Dark crystal shard
            MoonBeam,           // Lunar crescent
            FeatherBolt,        // Feather-shaped
            MusicNote,          // Note-shaped projectile
            CrystalSpear,       // Crystalline spear
            SmokeWisp,          // Smoke-trailing orb
            PrismaticGem        // Rainbow gem
        }
        
        public class TrailRenderStyle
        {
            public Color StartColor;
            public Color EndColor;
            public float StartWidth;
            public float EndWidth;
            public int TrailLength;
            public float FadeSpeed;
            public TrailType Type;
            public bool HasParticles;
            public float ParticleDensity;
        }
        
        public enum TrailType
        {
            Smooth,             // Clean gradient trail
            Flame,              // Flickering fire trail
            Constellation,      // Star-connected trail
            Feather,            // Drifting feather trail
            Smoke,              // Billowing smoke
            Electric,           // Crackling lightning
            Prismatic,          // Rainbow shifting
            Void,               // Dark energy trail
            Musical,            // Music notes scattered
            Crystalline         // Ice crystal trail
        }
        
        #endregion
        
        #region Theme Style Registry
        
        private static Dictionary<string, ThemeVFXStyle> _themeStyles = new Dictionary<string, ThemeVFXStyle>();
        
        static UniqueWeaponVFXStyles()
        {
            InitializeThemeStyles();
        }
        
        private static void InitializeThemeStyles()
        {
            // === EROICA - Heroic Scarlet & Gold ===
            _themeStyles["Eroica"] = new ThemeVFXStyle
            {
                ThemeName = "Eroica",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(200, 50, 50),
                    SecondaryColor = new Color(255, 200, 80),
                    Density = 0.6f,
                    Turbulence = 0.4f,
                    SparkleIntensity = 0.5f,
                    Type = FogType.SakuraPetals,
                    ExpansionSpeed = 1.2f,
                    Lifetime = 35
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(255, 215, 0),
                    EdgeColor = new Color(200, 50, 50),
                    RayCount = 6,
                    BaseLength = 80f,
                    BaseWidth = 8f,
                    StretchSpeed = 1.3f,
                    ShimmerIntensity = 0.4f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Starburst
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(255, 220, 100),
                    OuterGlow = new Color(200, 50, 50, 150),
                    TrailColor = new Color(255, 150, 180),
                    CoreScale = 0.5f,
                    GlowLayers = 4,
                    PulseSpeed = 0.08f,
                    RotationSpeed = 0.03f,
                    VisualType = ProjectileVisualType.EnergyOrb,
                    HasOrbitingElements = true,
                    OrbitCount = 3,
                    OrbitRadius = 15f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(255, 215, 0),
                    EndColor = new Color(200, 50, 50, 100),
                    StartWidth = 20f,
                    EndWidth = 2f,
                    TrailLength = 15,
                    FadeSpeed = 0.08f,
                    Type = TrailType.Flame,
                    HasParticles = true,
                    ParticleDensity = 0.6f
                }
            };
            
            // === FATE - Cosmic Celestial ===
            _themeStyles["Fate"] = new ThemeVFXStyle
            {
                ThemeName = "Fate",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(15, 5, 20),
                    SecondaryColor = new Color(180, 50, 100),
                    Density = 0.8f,
                    Turbulence = 0.7f,
                    SparkleIntensity = 1.0f, // Maximum stars
                    Type = FogType.CosmicNebula,
                    ExpansionSpeed = 0.8f,
                    Lifetime = 50
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = Color.White,
                    EdgeColor = new Color(255, 60, 80),
                    RayCount = 8,
                    BaseLength = 120f,
                    BaseWidth = 6f,
                    StretchSpeed = 1.8f,
                    ShimmerIntensity = 0.8f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Constellation
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = Color.White,
                    OuterGlow = new Color(180, 40, 80, 180),
                    TrailColor = new Color(255, 60, 80),
                    CoreScale = 0.4f,
                    GlowLayers = 5,
                    PulseSpeed = 0.06f,
                    RotationSpeed = 0.02f,
                    VisualType = ProjectileVisualType.CosmicStar,
                    HasOrbitingElements = true,
                    OrbitCount = 4,
                    OrbitRadius = 18f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(255, 60, 80),
                    EndColor = new Color(120, 30, 140, 80),
                    StartWidth = 25f,
                    EndWidth = 3f,
                    TrailLength = 20,
                    FadeSpeed = 0.05f,
                    Type = TrailType.Constellation,
                    HasParticles = true,
                    ParticleDensity = 0.8f
                }
            };
            
            // === SWAN LAKE - Monochrome + Rainbow ===
            _themeStyles["SwanLake"] = new ThemeVFXStyle
            {
                ThemeName = "SwanLake",
                Fog = new FogStyle
                {
                    PrimaryColor = Color.White,
                    SecondaryColor = new Color(30, 30, 40),
                    Density = 0.5f,
                    Turbulence = 0.3f,
                    SparkleIntensity = 0.7f,
                    Type = FogType.CrystallineHaze,
                    ExpansionSpeed = 1.0f,
                    Lifetime = 40
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = Color.White,
                    EdgeColor = new Color(30, 30, 40),
                    RayCount = 5,
                    BaseLength = 90f,
                    BaseWidth = 10f,
                    StretchSpeed = 1.1f,
                    ShimmerIntensity = 0.9f, // Rainbow shimmer
                    IncludeMusicNotes = true,
                    Type = LightBeamType.PrismaticSpray
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = Color.White,
                    OuterGlow = new Color(200, 200, 220, 150),
                    TrailColor = Color.White,
                    CoreScale = 0.45f,
                    GlowLayers = 4,
                    PulseSpeed = 0.07f,
                    RotationSpeed = 0.04f,
                    VisualType = ProjectileVisualType.FeatherBolt,
                    HasOrbitingElements = true,
                    OrbitCount = 3,
                    OrbitRadius = 14f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = Color.White,
                    EndColor = new Color(30, 30, 40, 100),
                    StartWidth = 18f,
                    EndWidth = 4f,
                    TrailLength = 18,
                    FadeSpeed = 0.06f,
                    Type = TrailType.Prismatic,
                    HasParticles = true,
                    ParticleDensity = 0.7f
                }
            };
            
            // === LA CAMPANELLA - Infernal Bell ===
            _themeStyles["LaCampanella"] = new ThemeVFXStyle
            {
                ThemeName = "LaCampanella",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(30, 20, 25),
                    SecondaryColor = new Color(255, 140, 40),
                    Density = 0.9f,
                    Turbulence = 0.8f,
                    SparkleIntensity = 0.3f, // Ember sparks
                    Type = FogType.InfernalSmoke,
                    ExpansionSpeed = 0.7f,
                    Lifetime = 55
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(255, 200, 50),
                    EdgeColor = new Color(255, 100, 0),
                    RayCount = 7,
                    BaseLength = 100f,
                    BaseWidth = 9f,
                    StretchSpeed = 1.4f,
                    ShimmerIntensity = 0.5f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.FireBurst
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(255, 200, 80),
                    OuterGlow = new Color(255, 100, 0, 180),
                    TrailColor = new Color(200, 50, 30),
                    CoreScale = 0.55f,
                    GlowLayers = 4,
                    PulseSpeed = 0.1f,
                    RotationSpeed = 0.05f,
                    VisualType = ProjectileVisualType.FlameBolt,
                    HasOrbitingElements = true,
                    OrbitCount = 2,
                    OrbitRadius = 12f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(255, 140, 40),
                    EndColor = new Color(30, 20, 25, 120),
                    StartWidth = 22f,
                    EndWidth = 5f,
                    TrailLength = 16,
                    FadeSpeed = 0.07f,
                    Type = TrailType.Smoke,
                    HasParticles = true,
                    ParticleDensity = 0.9f
                }
            };
            
            // === MOONLIGHT SONATA - Lunar Purple ===
            _themeStyles["MoonlightSonata"] = new ThemeVFXStyle
            {
                ThemeName = "MoonlightSonata",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(75, 0, 130),
                    SecondaryColor = new Color(135, 206, 250),
                    Density = 0.4f,
                    Turbulence = 0.2f,
                    SparkleIntensity = 0.6f,
                    Type = FogType.MoonlightGlow,
                    ExpansionSpeed = 0.9f,
                    Lifetime = 45
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(200, 180, 255),
                    EdgeColor = new Color(100, 60, 180),
                    RayCount = 5,
                    BaseLength = 85f,
                    BaseWidth = 7f,
                    StretchSpeed = 1.0f,
                    ShimmerIntensity = 0.6f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Crescent
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(180, 160, 255),
                    OuterGlow = new Color(100, 60, 180, 150),
                    TrailColor = new Color(135, 206, 250),
                    CoreScale = 0.4f,
                    GlowLayers = 4,
                    PulseSpeed = 0.05f,
                    RotationSpeed = 0.02f,
                    VisualType = ProjectileVisualType.MoonBeam,
                    HasOrbitingElements = true,
                    OrbitCount = 2,
                    OrbitRadius = 16f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(180, 160, 255),
                    EndColor = new Color(75, 0, 130, 80),
                    StartWidth = 16f,
                    EndWidth = 2f,
                    TrailLength = 22,
                    FadeSpeed = 0.04f,
                    Type = TrailType.Smooth,
                    HasParticles = true,
                    ParticleDensity = 0.5f
                }
            };
            
            // === ENIGMA VARIATIONS - Void Mystery ===
            _themeStyles["EnigmaVariations"] = new ThemeVFXStyle
            {
                ThemeName = "EnigmaVariations",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(15, 10, 20),
                    SecondaryColor = new Color(50, 220, 100),
                    Density = 0.7f,
                    Turbulence = 0.9f,
                    SparkleIntensity = 0.4f,
                    Type = FogType.VoidRift,
                    ExpansionSpeed = 0.6f,
                    Lifetime = 60
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(50, 220, 100),
                    EdgeColor = new Color(140, 60, 200),
                    RayCount = 6,
                    BaseLength = 95f,
                    BaseWidth = 6f,
                    StretchSpeed = 1.6f,
                    ShimmerIntensity = 0.7f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.VoidTendrils
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(50, 220, 100),
                    OuterGlow = new Color(140, 60, 200, 180),
                    TrailColor = new Color(80, 20, 120),
                    CoreScale = 0.35f,
                    GlowLayers = 5,
                    PulseSpeed = 0.09f,
                    RotationSpeed = 0.06f,
                    VisualType = ProjectileVisualType.VoidShard,
                    HasOrbitingElements = true,
                    OrbitCount = 3,
                    OrbitRadius = 14f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(140, 60, 200),
                    EndColor = new Color(15, 10, 20, 100),
                    StartWidth = 18f,
                    EndWidth = 3f,
                    TrailLength = 17,
                    FadeSpeed = 0.06f,
                    Type = TrailType.Void,
                    HasParticles = true,
                    ParticleDensity = 0.6f
                }
            };
            
            // === CLAIR DE LUNE - Celestial Soft Blue ===
            _themeStyles["ClairDeLune"] = new ThemeVFXStyle
            {
                ThemeName = "ClairDeLune",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(100, 120, 160),
                    SecondaryColor = new Color(240, 240, 250),
                    Density = 0.35f,
                    Turbulence = 0.15f,
                    SparkleIntensity = 0.8f,
                    Type = FogType.EtherealMist,
                    ExpansionSpeed = 0.8f,
                    Lifetime = 42
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(240, 240, 250),
                    EdgeColor = new Color(140, 170, 220),
                    RayCount = 4,
                    BaseLength = 70f,
                    BaseWidth = 8f,
                    StretchSpeed = 0.9f,
                    ShimmerIntensity = 0.5f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Radial
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(240, 240, 250),
                    OuterGlow = new Color(140, 170, 220, 140),
                    TrailColor = new Color(180, 200, 235),
                    CoreScale = 0.38f,
                    GlowLayers = 3,
                    PulseSpeed = 0.04f,
                    RotationSpeed = 0.015f,
                    VisualType = ProjectileVisualType.MoonBeam,
                    HasOrbitingElements = false,
                    OrbitCount = 0,
                    OrbitRadius = 0f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(200, 210, 240),
                    EndColor = new Color(100, 120, 160, 60),
                    StartWidth = 14f,
                    EndWidth = 2f,
                    TrailLength = 20,
                    FadeSpeed = 0.035f,
                    Type = TrailType.Smooth,
                    HasParticles = true,
                    ParticleDensity = 0.4f
                }
            };
            
            // === SPRING - Pastel Pink/Green ===
            _themeStyles["Spring"] = new ThemeVFXStyle
            {
                ThemeName = "Spring",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(255, 180, 200),
                    SecondaryColor = new Color(150, 220, 150),
                    Density = 0.3f,
                    Turbulence = 0.25f,
                    SparkleIntensity = 0.5f,
                    Type = FogType.SakuraPetals,
                    ExpansionSpeed = 1.1f,
                    Lifetime = 32
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(255, 200, 220),
                    EdgeColor = new Color(180, 255, 180),
                    RayCount = 5,
                    BaseLength = 65f,
                    BaseWidth = 7f,
                    StretchSpeed = 1.2f,
                    ShimmerIntensity = 0.4f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Radial
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(255, 200, 220),
                    OuterGlow = new Color(180, 255, 180, 130),
                    TrailColor = new Color(255, 180, 200),
                    CoreScale = 0.35f,
                    GlowLayers = 3,
                    PulseSpeed = 0.06f,
                    RotationSpeed = 0.025f,
                    VisualType = ProjectileVisualType.EnergyOrb,
                    HasOrbitingElements = true,
                    OrbitCount = 2,
                    OrbitRadius = 10f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(255, 180, 200),
                    EndColor = new Color(150, 220, 150, 80),
                    StartWidth = 12f,
                    EndWidth = 2f,
                    TrailLength = 14,
                    FadeSpeed = 0.08f,
                    Type = TrailType.Smooth,
                    HasParticles = true,
                    ParticleDensity = 0.5f
                }
            };
            
            // === SUMMER - Warm Orange/Gold ===
            _themeStyles["Summer"] = new ThemeVFXStyle
            {
                ThemeName = "Summer",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(255, 200, 100),
                    SecondaryColor = new Color(255, 140, 50),
                    Density = 0.35f,
                    Turbulence = 0.35f,
                    SparkleIntensity = 0.6f,
                    Type = FogType.SoftCloud,
                    ExpansionSpeed = 1.3f,
                    Lifetime = 30
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(255, 220, 120),
                    EdgeColor = new Color(255, 160, 60),
                    RayCount = 6,
                    BaseLength = 75f,
                    BaseWidth = 8f,
                    StretchSpeed = 1.4f,
                    ShimmerIntensity = 0.5f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Starburst
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(255, 220, 120),
                    OuterGlow = new Color(255, 160, 60, 150),
                    TrailColor = new Color(255, 180, 80),
                    CoreScale = 0.4f,
                    GlowLayers = 3,
                    PulseSpeed = 0.08f,
                    RotationSpeed = 0.035f,
                    VisualType = ProjectileVisualType.EnergyOrb,
                    HasOrbitingElements = true,
                    OrbitCount = 2,
                    OrbitRadius = 11f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(255, 200, 100),
                    EndColor = new Color(255, 140, 50, 90),
                    StartWidth = 14f,
                    EndWidth = 2f,
                    TrailLength = 12,
                    FadeSpeed = 0.09f,
                    Type = TrailType.Flame,
                    HasParticles = true,
                    ParticleDensity = 0.55f
                }
            };
            
            // === AUTUMN - Amber/Crimson ===
            _themeStyles["Autumn"] = new ThemeVFXStyle
            {
                ThemeName = "Autumn",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(200, 120, 60),
                    SecondaryColor = new Color(180, 60, 40),
                    Density = 0.5f,
                    Turbulence = 0.5f,
                    SparkleIntensity = 0.3f,
                    Type = FogType.SoftCloud,
                    ExpansionSpeed = 0.9f,
                    Lifetime = 38
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = new Color(220, 150, 80),
                    EdgeColor = new Color(180, 60, 40),
                    RayCount = 5,
                    BaseLength = 70f,
                    BaseWidth = 7f,
                    StretchSpeed = 1.1f,
                    ShimmerIntensity = 0.35f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Radial
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = new Color(220, 150, 80),
                    OuterGlow = new Color(180, 60, 40, 140),
                    TrailColor = new Color(200, 100, 50),
                    CoreScale = 0.42f,
                    GlowLayers = 3,
                    PulseSpeed = 0.06f,
                    RotationSpeed = 0.03f,
                    VisualType = ProjectileVisualType.EnergyOrb,
                    HasOrbitingElements = true,
                    OrbitCount = 2,
                    OrbitRadius = 12f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(200, 120, 60),
                    EndColor = new Color(140, 50, 30, 80),
                    StartWidth = 15f,
                    EndWidth = 3f,
                    TrailLength = 14,
                    FadeSpeed = 0.07f,
                    Type = TrailType.Smoke,
                    HasParticles = true,
                    ParticleDensity = 0.5f
                }
            };
            
            // === WINTER - Ice Blue/White ===
            _themeStyles["Winter"] = new ThemeVFXStyle
            {
                ThemeName = "Winter",
                Fog = new FogStyle
                {
                    PrimaryColor = new Color(180, 220, 255),
                    SecondaryColor = new Color(220, 240, 255),
                    Density = 0.45f,
                    Turbulence = 0.2f,
                    SparkleIntensity = 0.9f, // Ice crystals
                    Type = FogType.FrostCloud,
                    ExpansionSpeed = 0.7f,
                    Lifetime = 48
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = Color.White,
                    EdgeColor = new Color(150, 200, 255),
                    RayCount = 6,
                    BaseLength = 80f,
                    BaseWidth = 6f,
                    StretchSpeed = 0.85f,
                    ShimmerIntensity = 0.8f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Starburst
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = Color.White,
                    OuterGlow = new Color(150, 200, 255, 150),
                    TrailColor = new Color(180, 220, 255),
                    CoreScale = 0.38f,
                    GlowLayers = 4,
                    PulseSpeed = 0.04f,
                    RotationSpeed = 0.02f,
                    VisualType = ProjectileVisualType.CrystalSpear,
                    HasOrbitingElements = true,
                    OrbitCount = 3,
                    OrbitRadius = 13f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = new Color(200, 230, 255),
                    EndColor = new Color(150, 200, 255, 70),
                    StartWidth = 14f,
                    EndWidth = 2f,
                    TrailLength = 18,
                    FadeSpeed = 0.045f,
                    Type = TrailType.Crystalline,
                    HasParticles = true,
                    ParticleDensity = 0.65f
                }
            };
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Gets the VFX style for a theme.
        /// </summary>
        public static ThemeVFXStyle GetStyle(string theme)
        {
            if (_themeStyles.TryGetValue(theme, out var style))
                return style;
            
            // Default fallback
            return _themeStyles.GetValueOrDefault("Eroica") ?? CreateDefaultStyle();
        }
        
        private static ThemeVFXStyle CreateDefaultStyle()
        {
            return new ThemeVFXStyle
            {
                ThemeName = "Default",
                Fog = new FogStyle
                {
                    PrimaryColor = Color.White,
                    SecondaryColor = Color.Gray,
                    Density = 0.4f,
                    Turbulence = 0.3f,
                    SparkleIntensity = 0.5f,
                    Type = FogType.SoftCloud,
                    ExpansionSpeed = 1f,
                    Lifetime = 35
                },
                LightBeams = new LightBeamStyle
                {
                    CoreColor = Color.White,
                    EdgeColor = Color.Gray,
                    RayCount = 5,
                    BaseLength = 70f,
                    BaseWidth = 7f,
                    StretchSpeed = 1f,
                    ShimmerIntensity = 0.5f,
                    IncludeMusicNotes = true,
                    Type = LightBeamType.Radial
                },
                ProjectileRender = new ProjectileRenderStyle
                {
                    CoreGlow = Color.White,
                    OuterGlow = new Color(200, 200, 200, 150),
                    TrailColor = Color.White,
                    CoreScale = 0.4f,
                    GlowLayers = 3,
                    PulseSpeed = 0.06f,
                    RotationSpeed = 0.03f,
                    VisualType = ProjectileVisualType.EnergyOrb,
                    HasOrbitingElements = false,
                    OrbitCount = 0,
                    OrbitRadius = 0f
                },
                Trail = new TrailRenderStyle
                {
                    StartColor = Color.White,
                    EndColor = new Color(200, 200, 200, 80),
                    StartWidth = 15f,
                    EndWidth = 2f,
                    TrailLength = 15,
                    FadeSpeed = 0.06f,
                    Type = TrailType.Smooth,
                    HasParticles = true,
                    ParticleDensity = 0.5f
                }
            };
        }
        
        #endregion
    }
}
