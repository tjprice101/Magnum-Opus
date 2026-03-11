using Microsoft.Xna.Framework;

namespace MagnumOpus.Common.Systems.VFX.Sparkle
{
    /// <summary>
    /// Static color palettes for each theme's sparkle explosion.
    /// Each palette follows the Foundation4PointSparkle convention:
    /// Index 0 = primary, 1 = secondary, 2 = core/highlight, 3 = accent, 4 = warm accent
    ///
    /// These colors are specifically tuned for additive blending sparkle VFX —
    /// they're brighter and more saturated than the theme's base palette to
    /// ensure sparkles pop visually against any background.
    /// </summary>
    public static class ThemeSparkleColors
    {
        /// <summary>Returns the 5-color sparkle palette for the given theme.</summary>
        public static Color[] GetColors(SparkleTheme theme) => theme switch
        {
            SparkleTheme.MoonlightSonata => MoonlightSparkle,
            SparkleTheme.Eroica => EroicaSparkle,
            SparkleTheme.SwanLake => SwanLakeSparkle,
            SparkleTheme.LaCampanella => LaCampanellaSparkle,
            SparkleTheme.EnigmaVariations => EnigmaSparkle,
            SparkleTheme.Fate => FateSparkle,
            _ => MoonlightSparkle,
        };

        /// <summary>
        /// Moonlight Sonata — Deep purple to ice blue, lunar silver highlights.
        /// Soft, mystical, like moonbeams through clouds.
        /// </summary>
        public static readonly Color[] MoonlightSparkle = new[]
        {
            new Color(138, 43, 226),   // Violet (primary)
            new Color(135, 206, 250),  // Ice Blue (secondary)
            new Color(240, 235, 255),  // Moon White (core/highlight)
            new Color(180, 150, 255),  // Lavender (accent)
            new Color(170, 225, 255),  // Crescent Glow (warm accent)
        };

        /// <summary>
        /// Eroica — Scarlet to gold with sakura pink accents.
        /// Heroic fire and cherry blossom glory.
        /// </summary>
        public static readonly Color[] EroicaSparkle = new[]
        {
            new Color(220, 50, 50),    // Crimson (primary)
            new Color(255, 215, 0),    // Gold (secondary)
            new Color(255, 240, 200),  // Hot Core (core/highlight)
            new Color(255, 150, 180),  // Sakura (accent)
            new Color(255, 180, 60),   // Orange Gold (warm accent)
        };

        /// <summary>
        /// Swan Lake — Pure whites and silvers with prismatic rainbow shimmer edges.
        /// Monochromatic elegance with fleeting rainbow beauty.
        /// </summary>
        public static readonly Color[] SwanLakeSparkle = new[]
        {
            new Color(220, 225, 235),  // Swan Silver (primary)
            new Color(240, 240, 250),  // Pure White (secondary)
            new Color(255, 255, 255),  // Rainbow Flash white (core/highlight)
            new Color(220, 230, 255),  // Prismatic Shimmer (accent)
            new Color(240, 230, 245),  // Pearlescent (warm accent)
        };

        /// <summary>
        /// La Campanella — Infernal orange to bell gold with white-hot cores.
        /// Ringing bells of fire, intense burning virtuosity.
        /// </summary>
        public static readonly Color[] LaCampanellaSparkle = new[]
        {
            new Color(255, 100, 0),    // Infernal Orange (primary)
            new Color(218, 165, 32),   // Bell Gold (secondary)
            new Color(255, 240, 200),  // White Hot (core/highlight)
            new Color(255, 200, 50),   // Flame Yellow (accent)
            new Color(200, 50, 20),    // Ember Red (warm accent)
        };

        /// <summary>
        /// Enigma Variations — Void purple to eerie green flame with arcane flashes.
        /// Mysterious, dreadful, unknowable.
        /// </summary>
        public static readonly Color[] EnigmaSparkle = new[]
        {
            new Color(140, 60, 200),   // Purple (primary)
            new Color(50, 220, 100),   // Green Flame (secondary)
            new Color(220, 255, 230),  // White-Green Flash (core/highlight)
            new Color(80, 255, 130),   // Eye Green (accent)
            new Color(120, 50, 180),   // Glyph Purple (warm accent)
        };

        /// <summary>
        /// Fate — Dark pink to bright crimson to celestial gold with cosmic white.
        /// The weight of destiny, cosmic inevitability.
        /// </summary>
        public static readonly Color[] FateSparkle = new[]
        {
            new Color(255, 60, 80),    // Bright Crimson (primary)
            new Color(180, 50, 100),   // Dark Pink (secondary)
            new Color(255, 255, 255),  // Celestial White (core/highlight)
            new Color(255, 230, 180),  // Star Gold (accent)
            new Color(160, 80, 200),   // Nebula Purple (warm accent)
        };
    }
}
