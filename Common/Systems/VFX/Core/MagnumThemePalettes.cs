using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Theme color palettes for MagnumOpus, organized for easy gradient lerping.
    /// Each theme has a primary array for PaletteLerp() usage.
    /// 
    /// Based on FargosSoulsDLC's color system patterns.
    /// </summary>
    public static class MagnumThemePalettes
    {
        #region La Campanella - Infernal Bell (Black ↁEOrange ↁEGold)
        
        /// <summary>La Campanella gradient palette: Deep black ↁEEmber ↁEOrange ↁEGold</summary>
        public static readonly Color[] LaCampanella = new Color[]
        {
            new Color(20, 15, 20),     // Deep smoky black
            new Color(80, 30, 20),     // Dark ember
            new Color(150, 60, 20),    // Ember
            new Color(255, 100, 0),    // Bright orange
            new Color(255, 200, 50),   // Golden flame tip
        };
        
        public static Color LaCampanellaBlack => new Color(20, 15, 20);
        public static Color LaCampanellaEmber => new Color(150, 60, 20);
        public static Color LaCampanellaOrange => new Color(255, 100, 0);
        public static Color LaCampanellaGold => new Color(255, 200, 50);
        public static Color LaCampanellaYellow => new Color(255, 240, 180);
        
        // Aliases for shorter access
        public static Color CampanellaBlack => LaCampanellaBlack;
        public static Color CampanellaEmber => LaCampanellaEmber;
        public static Color CampanellaOrange => LaCampanellaOrange;
        public static Color CampanellaGold => LaCampanellaGold;
        public static Color CampanellaYellow => LaCampanellaYellow;
                // Weapon-specific colors (DualFatedChime infernal blade)
        public static Color InfernalBlack => LaCampanellaBlack;
        public static Color InfernalDeepOrange => new Color(200, 60, 10);
        public static Color InfernalOrange => new Color(255, 120, 20);
        public static Color InfernalBright => new Color(255, 180, 40);
        public static Color InfernalGold => new Color(255, 220, 100);
        public static Color InfernalWhiteHot => new Color(255, 250, 220);
                /// <summary>Gets a La Campanella gradient color (black ↁEorange ↁEgold)</summary>
        public static Color GetLaCampanella(float progress) => VFXUtilities.PaletteLerp(LaCampanella, progress);
        
        #endregion
        
        #region Eroica - Heroic Symphony (Scarlet ↁECrimson ↁEGold)
        
        /// <summary>Eroica gradient palette: Deep scarlet ↁECrimson ↁEOrange-gold ↁEBright gold</summary>
        public static readonly Color[] Eroica = new Color[]
        {
            new Color(100, 20, 20),    // Deep scarlet
            new Color(180, 40, 40),    // Rich red
            new Color(220, 80, 50),    // Red-orange
            new Color(255, 150, 50),   // Orange-gold
            new Color(255, 215, 100),  // Bright gold
        };
        
        public static Color EroicaScarlet => new Color(139, 0, 0);
        public static Color EroicaCrimson => new Color(220, 50, 50);
        public static Color EroicaFlame => new Color(255, 100, 50);
        public static Color EroicaGold => new Color(255, 215, 0);
        public static Color EroicaSakura => new Color(255, 150, 180);
        public static Color EroicaHotCore => new Color(255, 230, 180);
                // Weapon-specific colors (CelestialValor, SakurasBlossom blade rendering)
        public static Color EroicaBladeScarlet => new Color(200, 50, 50);
        public static Color EroicaBladeCrimson => new Color(180, 30, 60);
        public static Color SakuraPale => new Color(255, 200, 220);
        public static Color SakuraPollenGold => new Color(255, 230, 140);
                /// <summary>Gets an Eroica gradient color (scarlet ↁEgold)</summary>
        public static Color GetEroica(float progress) => VFXUtilities.PaletteLerp(Eroica, progress);
        
        #endregion
        
        #region Moonlight Sonata - Lunar Mystique (Deep Purple ↁELight Blue)
        
        /// <summary>Moonlight Sonata gradient palette: Deep purple ↁEViolet ↁELavender ↁEIce blue</summary>
        public static readonly Color[] MoonlightSonata = new Color[]
        {
            new Color(75, 0, 130),     // Deep purple (indigo)
            new Color(120, 50, 180),   // Medium purple
            new Color(160, 100, 220),  // Violet
            new Color(180, 150, 255),  // Lavender
            new Color(135, 206, 250),  // Light sky blue
        };
        
        public static Color MoonlightDarkPurple => new Color(75, 0, 130);
        public static Color MoonlightViolet => new Color(138, 43, 226);
        public static Color MoonlightPurple => new Color(138, 43, 226);  // Alias for Violet
        public static Color MoonlightLavender => new Color(180, 150, 255);
        public static Color MoonlightSilver => new Color(220, 220, 235);
        public static Color MoonlightIceBlue => new Color(135, 206, 250);
        public static Color MoonlightMoonWhite => new Color(240, 235, 255);
        
        // Aliases for compatibility
        public static Color MoonlightLightBlue => MoonlightIceBlue;
                // Weapon-specific colors (EternalMoon, IncisorOfMoonlight blade rendering)
        public static Color MoonlightWeaponLavender => new Color(200, 170, 255);
        public static Color MoonlightLightPurple => new Color(240, 220, 255);
                /// <summary>Gets a Moonlight Sonata gradient color (purple ↁEblue)</summary>
        public static Color GetMoonlightSonata(float progress) => VFXUtilities.PaletteLerp(MoonlightSonata, progress);
        
        #endregion
        
        #region Swan Lake - Graceful Monochrome (White ↁEBlack + Rainbow Shimmer)
        
        /// <summary>Swan Lake monochrome palette</summary>
        public static readonly Color[] SwanLake = new Color[]
        {
            new Color(255, 255, 255),  // Pure white
            new Color(220, 225, 235),  // Silver
            new Color(180, 185, 200),  // Gray-blue
            new Color(100, 105, 120),  // Dark gray
            new Color(30, 30, 40),     // Near black
        };
        
        public static Color SwanWhite => new Color(255, 255, 255);
        public static Color SwanSilver => new Color(220, 225, 235);
        public static Color SwanIcyBlue => new Color(180, 220, 255);
        public static Color SwanBlack => new Color(20, 20, 30);
                // Weapon-specific colors (CalloftheBlackSwan monochrome palette)
        public static Color SwanDarkGray => new Color(60, 60, 70);
        public static Color SwanMidGray => new Color(140, 140, 150);
        public static Color SwanLightGray => new Color(200, 200, 210);
                /// <summary>Gets a Swan Lake gradient color (white ↁEblack)</summary>
        public static Color GetSwanLake(float progress) => VFXUtilities.PaletteLerp(SwanLake, progress);
        
        /// <summary>Gets a Swan Lake rainbow shimmer color</summary>
        public static Color GetSwanRainbow(float offset = 0f)
        {
            float hue = (Main.GlobalTimeWrappedHourly * 0.3f + offset) % 1f;
            Color rainbow = Main.hslToRgb(hue, 1f, 0.8f);
            // Blend with white for ethereal effect
            return Color.Lerp(Color.White, rainbow, 0.6f);
        }
        
        #endregion
        
        #region Enigma Variations - Mysterious Arcane (Black ↁEPurple ↁEGreen Flame)
        
        /// <summary>Enigma Variations gradient palette: Void black ↁEDeep purple ↁEEerie green</summary>
        public static readonly Color[] EnigmaVariations = new Color[]
        {
            new Color(15, 10, 20),     // Void darkness
            new Color(60, 20, 80),     // Dark purple
            new Color(100, 40, 140),   // Medium purple
            new Color(80, 140, 100),   // Purple-green transition
            new Color(50, 220, 100),   // Eerie green flame
        };
        
        public static Color EnigmaBlack => new Color(15, 10, 20);
        public static Color EnigmaDeepPurple => new Color(80, 20, 120);
        public static Color EnigmaPurple => new Color(140, 60, 200);
        public static Color EnigmaGreenFlame => new Color(50, 220, 100);
        public static Color EnigmaGreen => new Color(50, 220, 100);  // Alias for GreenFlame
        public static Color EnigmaDarkGreen => new Color(30, 100, 50);
                // Weapon-specific colors (TheUnresolvedCadence, VariationsOfTheVoid)
        public static Color EnigmaVoid => new Color(30, 15, 40);
                /// <summary>Gets an Enigma Variations gradient color (black ↁEpurple ↁEgreen)</summary>
        public static Color GetEnigmaVariations(float progress) => VFXUtilities.PaletteLerp(EnigmaVariations, progress);
        
        #endregion
        
        #region Fate - Celestial Cosmic (Black ↁEDark Pink ↁEBright Red + White Stars)
        
        /// <summary>Fate celestial gradient palette: Cosmic void ↁEDark pink ↁEBright red</summary>
        public static readonly Color[] Fate = new Color[]
        {
            new Color(15, 5, 20),      // Cosmic void
            new Color(80, 20, 60),     // Dark cosmic
            new Color(140, 40, 90),    // Purple-pink
            new Color(180, 50, 100),   // Dark pink
            new Color(255, 60, 80),    // Bright crimson
        };
        
        public static Color FateBlack => new Color(15, 5, 20);
        public static Color FateDarkPink => new Color(180, 50, 100);
        public static Color FatePink => new Color(180, 50, 100);  // Alias for DarkPink
        public static Color FatePurple => new Color(120, 30, 140);
        public static Color FateBrightRed => new Color(255, 60, 80);
        public static Color FateWhite => Color.White;
        public static Color FateStarGold => new Color(255, 230, 180);
        
        /// <summary>Gets a Fate gradient color (black ↁEpink ↁEred)</summary>
        public static Color GetFate(float progress) => VFXUtilities.PaletteLerp(Fate, progress);
        
        /// <summary>Gets the cosmic Fate gradient with white star highlight at the end</summary>
        public static Color GetFateCosmic(float progress)
        {
            if (progress < 0.8f)
                return VFXUtilities.PaletteLerp(Fate, progress / 0.8f);
            else
            {
                // Transition to white star at the end
                float starProgress = (progress - 0.8f) / 0.2f;
                return Color.Lerp(FateBrightRed, FateWhite, starProgress);
            }
        }
        
        #endregion
        
        #region Clair de Lune - Celestial Dreamscape (Night Mist ↁEPearl)
        
        /// <summary>Clair de Lune gradient palette</summary>
        public static readonly Color[] ClairDeLune = new Color[]
        {
            new Color(80, 100, 140),   // Night mist
            new Color(120, 150, 190),  // Soft blue
            new Color(160, 185, 220),  // Dreamy blue
            new Color(200, 210, 240),  // Moonbeam
            new Color(240, 240, 250),  // Pearl white
        };
        
        public static Color ClairNightMist => new Color(100, 120, 160);
        public static Color ClairSoftBlue => new Color(140, 170, 220);
        public static Color ClairMoonbeam => new Color(200, 210, 240);
        public static Color ClairPearl => new Color(240, 240, 250);
        
        /// <summary>Gets a Clair de Lune gradient color</summary>
        public static Color GetClairDeLune(float progress) => VFXUtilities.PaletteLerp(ClairDeLune, progress);
        
        #endregion
        
        #region Dies Irae - Infernal Wrath (Dark Crimson ↁEHellfire)
        
        /// <summary>Dies Irae gradient palette</summary>
        public static readonly Color[] DiesIrae = new Color[]
        {
            new Color(40, 10, 10),     // Blood darkness
            new Color(80, 10, 10),     // Dark crimson
            new Color(120, 20, 20),    // Blood red
            new Color(180, 60, 30),    // Ember
            new Color(255, 100, 50),   // Hellfire
        };
        
        public static Color DiesBloodRed => new Color(120, 20, 20);
        public static Color DiesDarkCrimson => new Color(80, 10, 10);
        public static Color DiesEmber => new Color(200, 80, 40);
        public static Color DiesAsh => new Color(60, 50, 50);
        public static Color DiesHellfire => new Color(255, 100, 50);
        
        /// <summary>Gets a Dies Irae gradient color</summary>
        public static Color GetDiesIrae(float progress) => VFXUtilities.PaletteLerp(DiesIrae, progress);
        
        #endregion
        
        #region Spring - Cherry Blossom Awakening (Rose → Pink → Green → Vibrant → Cream)
        
        /// <summary>Spring gradient palette — fresh, floral, awakening</summary>
        public static readonly Color[] Spring = new Color[]
        {
            new Color(180, 120, 150),  // Dusk rose
            new Color(255, 180, 200),  // Cherry blossom pink
            new Color(200, 255, 200),  // Fresh spring green
            new Color(150, 230, 130),  // Vibrant green
            new Color(255, 245, 200),  // Warm cream sunshine
        };
        
        public static Color SpringRose => new Color(180, 120, 150);
        public static Color SpringBlossom => new Color(255, 180, 200);
        public static Color SpringGreen => new Color(200, 255, 200);
        public static Color SpringVibrant => new Color(150, 230, 130);
        public static Color SpringCream => new Color(255, 245, 200);
        
        // Weapon-specific colors (BlossomsEdge blade rendering)
        public static Color SpringPink => new Color(255, 183, 197);
        public static Color SpringWhite => new Color(255, 250, 250);
        public static Color SpringLightGreen => new Color(144, 238, 144);
        
        /// <summary>Gets a Spring gradient color</summary>
        public static Color GetSpring(float progress) => VFXUtilities.PaletteLerp(Spring, progress);
        
        #endregion
        
        #region Summer - Blazing Passion (Gold → Orange → Flame → Ember → Crimson)
        
        /// <summary>Summer gradient palette — hot, passionate, blazing</summary>
        public static readonly Color[] Summer = new Color[]
        {
            new Color(255, 200, 80),   // Molten gold
            new Color(255, 150, 40),   // Hot orange
            new Color(255, 100, 20),   // Bright flame
            new Color(230, 60, 20),    // Deep ember
            new Color(180, 30, 10),    // Smoldering crimson
        };
        
        public static Color SummerGold => new Color(255, 200, 80);
        public static Color SummerOrange => new Color(255, 150, 40);
        public static Color SummerFlame => new Color(255, 100, 20);
        public static Color SummerEmber => new Color(230, 60, 20);
        public static Color SummerCrimson => new Color(180, 30, 10);
        
        // Weapon-specific colors (ZenithCleaver solar blade)
        public static Color SunGold => new Color(255, 215, 0);
        public static Color SunOrange => new Color(255, 140, 0);
        public static Color SunWhite => new Color(255, 250, 240);
        public static Color SunRed => new Color(255, 100, 50);
        
        /// <summary>Gets a Summer gradient color</summary>
        public static Color GetSummer(float progress) => VFXUtilities.PaletteLerp(Summer, progress);
        
        #endregion
        
        #region Autumn - Harvest Melancholy (Sienna → Amber → Gold → Oak → Earth)
        
        /// <summary>Autumn gradient palette — rich, warm, melancholic harvest</summary>
        public static readonly Color[] Autumn = new Color[]
        {
            new Color(80, 40, 20),     // Deep earth
            new Color(140, 80, 30),    // Dark oak
            new Color(200, 120, 40),   // Warm amber
            new Color(210, 160, 60),   // Harvest gold
            new Color(180, 60, 20),    // Burnt sienna
        };
        
        public static Color AutumnEarth => new Color(80, 40, 20);
        public static Color AutumnOak => new Color(140, 80, 30);
        public static Color AutumnAmber => new Color(200, 120, 40);
        public static Color AutumnGold => new Color(210, 160, 60);
        public static Color AutumnSienna => new Color(180, 60, 20);
        
        // Weapon-specific colors (HarvestReaper scythe blade)
        public static Color AutumnOrange => new Color(255, 140, 50);
        public static Color AutumnBrown => new Color(139, 90, 43);
        public static Color AutumnRed => new Color(178, 34, 34);
        public static Color AutumnHarvestGold => new Color(218, 165, 32);
        public static Color AutumnDecayPurple => new Color(100, 50, 120);
        
        /// <summary>Gets an Autumn gradient color</summary>
        public static Color GetAutumn(float progress) => VFXUtilities.PaletteLerp(Autumn, progress);
        
        #endregion
        
        #region Winter - Crystalline Serenity (Frost → Blue → Ice → White → Crystal)
        
        /// <summary>Winter gradient palette — cold, crystalline, serene</summary>
        public static readonly Color[] Winter = new Color[]
        {
            new Color(40, 60, 100),    // Deep frost
            new Color(80, 140, 200),   // Winter blue
            new Color(140, 200, 240),  // Ice blue
            new Color(200, 230, 250),  // Frost white
            new Color(240, 248, 255),  // Crystal white
        };
        
        public static Color WinterFrost => new Color(40, 60, 100);
        public static Color WinterBlue => new Color(80, 140, 200);
        public static Color WinterIce => new Color(140, 200, 240);
        public static Color WinterFrostWhite => new Color(200, 230, 250);
        public static Color WinterCrystal => new Color(240, 248, 255);
        
        // Weapon-specific colors (GlacialExecutioner ice blade)
        public static Color WinterIceBlue => new Color(150, 220, 255);
        public static Color WinterDeepBlue => new Color(60, 100, 180);
        public static Color WinterCrystalCyan => new Color(100, 255, 255);
        public static Color WinterFrostPure => new Color(240, 250, 255);
        
        /// <summary>Gets a Winter gradient color</summary>
        public static Color GetWinter(float progress) => VFXUtilities.PaletteLerp(Winter, progress);
        
        #endregion
        
        #region Ode to Joy - Triumphant Celebration (Deep Gold → Bright → Sunlight → Cream → Light)
        
        /// <summary>Ode to Joy gradient palette — joyful, triumphant, celebratory</summary>
        public static readonly Color[] OdeToJoy = new Color[]
        {
            new Color(160, 100, 30),   // Deep gold
            new Color(255, 180, 50),   // Bright gold
            new Color(255, 220, 100),  // Sunlight
            new Color(255, 240, 170),  // Warm cream
            new Color(255, 250, 230),  // Pure light
        };
        
        public static Color OdeDeepGold => new Color(160, 100, 30);
        public static Color OdeBrightGold => new Color(255, 180, 50);
        public static Color OdeSunlight => new Color(255, 220, 100);
        public static Color OdeCream => new Color(255, 240, 170);
        public static Color OdePureLight => new Color(255, 250, 230);
        
        /// <summary>Gets an Ode to Joy gradient color</summary>
        public static Color GetOdeToJoy(float progress) => VFXUtilities.PaletteLerp(OdeToJoy, progress);
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Gets a theme's color palette array by name.
        /// Returns the full gradient array for flexible usage.
        /// </summary>
        public static Color[] GetThemePalette(string themeName)
        {
            return GetPalette(themeName);
        }
        
        /// <summary>
        /// Gets a theme's color palette array by name (alias for GetThemePalette).
        /// Returns the full gradient array for flexible usage.
        /// </summary>
        public static Color[] GetPalette(string themeName)
        {
            return themeName?.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => LaCampanella,
                "eroica" => Eroica,
                "moonlight" or "moonlightsonata" => MoonlightSonata,
                "swanlake" or "swan" => SwanLake,
                "enigma" or "enigmavariations" => EnigmaVariations,
                "fate" => Fate,
                "clair" or "clairdelune" => ClairDeLune,
                "dies" or "diesirae" => DiesIrae,
                "spring" => Spring,
                "summer" => Summer,
                "autumn" or "fall" => Autumn,
                "winter" => Winter,
                "odetojoy" or "ode" or "joy" => OdeToJoy,
                _ => new Color[] { Color.White, Color.Gray }
            };
        }
        
        /// <summary>
        /// Gets a theme gradient by name.
        /// </summary>
        public static Color GetThemeColor(string themeName, float progress)
        {
            return themeName.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => GetLaCampanella(progress),
                "eroica" => GetEroica(progress),
                "moonlight" or "moonlightsonata" => GetMoonlightSonata(progress),
                "swanlake" or "swan" => GetSwanLake(progress),
                "enigma" or "enigmavariations" => GetEnigmaVariations(progress),
                "fate" => GetFate(progress),
                "clair" or "clairdelune" => GetClairDeLune(progress),
                "dies" or "diesirae" => GetDiesIrae(progress),
                "spring" => GetSpring(progress),
                "summer" => GetSummer(progress),
                "autumn" or "fall" => GetAutumn(progress),
                "winter" => GetWinter(progress),
                "odetojoy" or "ode" or "joy" => GetOdeToJoy(progress),
                _ => Color.White
            };
        }
        
        /// <summary>
        /// Gets a theme's primary color (first in gradient).
        /// </summary>
        public static Color GetThemePrimary(string themeName)
        {
            return themeName.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => LaCampanellaOrange,
                "eroica" => EroicaScarlet,
                "moonlight" or "moonlightsonata" => MoonlightDarkPurple,
                "swanlake" or "swan" => SwanWhite,
                "enigma" or "enigmavariations" => EnigmaPurple,
                "fate" => FateDarkPink,
                "clair" or "clairdelune" => ClairSoftBlue,
                "dies" or "diesirae" => DiesBloodRed,
                "spring" => SpringBlossom,
                "summer" => SummerOrange,
                "autumn" or "fall" => AutumnAmber,
                "winter" => WinterBlue,
                "odetojoy" or "ode" or "joy" => OdeBrightGold,
                _ => Color.White
            };
        }
        
        /// <summary>
        /// Gets a theme's secondary/accent color.
        /// </summary>
        public static Color GetThemeSecondary(string themeName)
        {
            return themeName.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => LaCampanellaGold,
                "eroica" => EroicaGold,
                "moonlight" or "moonlightsonata" => MoonlightIceBlue,
                "swanlake" or "swan" => SwanIcyBlue,
                "enigma" or "enigmavariations" => EnigmaGreenFlame,
                "fate" => FateBrightRed,
                "clair" or "clairdelune" => ClairPearl,
                "dies" or "diesirae" => DiesHellfire,
                "spring" => SpringGreen,
                "summer" => SummerCrimson,
                "autumn" or "fall" => AutumnGold,
                "winter" => WinterCrystal,
                "odetojoy" or "ode" or "joy" => OdeSunlight,
                _ => Color.White
            };
        }
        
        #endregion
    }
}
