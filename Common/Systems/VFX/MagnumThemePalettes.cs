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
        #region La Campanella - Infernal Bell (Black → Orange → Gold)
        
        /// <summary>La Campanella gradient palette: Deep black → Ember → Orange → Gold</summary>
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
        
        /// <summary>Gets a La Campanella gradient color (black → orange → gold)</summary>
        public static Color GetLaCampanella(float progress) => VFXUtilities.PaletteLerp(LaCampanella, progress);
        
        #endregion
        
        #region Eroica - Heroic Symphony (Scarlet → Crimson → Gold)
        
        /// <summary>Eroica gradient palette: Deep scarlet → Crimson → Orange-gold → Bright gold</summary>
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
        
        /// <summary>Gets an Eroica gradient color (scarlet → gold)</summary>
        public static Color GetEroica(float progress) => VFXUtilities.PaletteLerp(Eroica, progress);
        
        #endregion
        
        #region Moonlight Sonata - Lunar Mystique (Deep Purple → Light Blue)
        
        /// <summary>Moonlight Sonata gradient palette: Deep purple → Violet → Lavender → Ice blue</summary>
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
        public static Color MoonlightLavender => new Color(180, 150, 255);
        public static Color MoonlightSilver => new Color(220, 220, 235);
        public static Color MoonlightIceBlue => new Color(135, 206, 250);
        public static Color MoonlightMoonWhite => new Color(240, 235, 255);
        
        /// <summary>Gets a Moonlight Sonata gradient color (purple → blue)</summary>
        public static Color GetMoonlightSonata(float progress) => VFXUtilities.PaletteLerp(MoonlightSonata, progress);
        
        #endregion
        
        #region Swan Lake - Graceful Monochrome (White ↔ Black + Rainbow Shimmer)
        
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
        
        /// <summary>Gets a Swan Lake gradient color (white → black)</summary>
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
        
        #region Enigma Variations - Mysterious Arcane (Black → Purple → Green Flame)
        
        /// <summary>Enigma Variations gradient palette: Void black → Deep purple → Eerie green</summary>
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
        public static Color EnigmaDarkGreen => new Color(30, 100, 50);
        
        /// <summary>Gets an Enigma Variations gradient color (black → purple → green)</summary>
        public static Color GetEnigmaVariations(float progress) => VFXUtilities.PaletteLerp(EnigmaVariations, progress);
        
        #endregion
        
        #region Fate - Celestial Cosmic (Black → Dark Pink → Bright Red + White Stars)
        
        /// <summary>Fate celestial gradient palette: Cosmic void → Dark pink → Bright red</summary>
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
        public static Color FatePurple => new Color(120, 30, 140);
        public static Color FateBrightRed => new Color(255, 60, 80);
        public static Color FateWhite => Color.White;
        public static Color FateStarGold => new Color(255, 230, 180);
        
        /// <summary>Gets a Fate gradient color (black → pink → red)</summary>
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
        
        #region Clair de Lune - Celestial Dreamscape (Night Mist → Pearl)
        
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
        
        #region Dies Irae - Infernal Wrath (Dark Crimson → Hellfire)
        
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
        
        #region Utility Methods
        
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
                _ => Color.White
            };
        }
        
        #endregion
    }
}
