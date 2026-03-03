using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SmokeFoundation
{
    /// <summary>
    /// Smoke cloud visual styles — right-click cycles through these,
    /// changing the color palette used for the smoke ring explosion.
    /// </summary>
    public enum SmokeCloudStyle
    {
        /// <summary>Deep indigo/purple smoke — the Supernova look.</summary>
        CosmicIndigo = 0,

        /// <summary>Infernal black-orange smoke with ember-hot cores.</summary>
        InfernalEmber,

        /// <summary>Ethereal blue-white wisps like ghostly mist.</summary>
        SpectralMist,

        /// <summary>Toxic green-purple alchemical fumes.</summary>
        AlchemicalFume,

        /// <summary>Warm golden-amber smoke like incense haze.</summary>
        GoldenHaze,

        /// <summary>Crimson blood-red smoke with dark edges.</summary>
        CrimsonVeil,

        COUNT
    }

    /// <summary>
    /// Self-contained texture registry for SmokeFoundation.
    /// Loads the 3×6 smoke spritesheet grid and bloom/glow assets.
    /// </summary>
    internal static class SKFTextures
    {
        // ---- PATHS ----
        private static readonly string VFXRoot = "MagnumOpus/Assets/VFX Asset Library/";
        private static readonly string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";

        // ---- SMOKE SPRITESHEET (3 columns × 6 rows = 18 frames) ----
        // Full image: 428×947 → each frame is ~142×157 pixels
        public const int GridColumns = 3;
        public const int GridRows = 6;
        public const int TotalFrames = GridColumns * GridRows; // 18

        /// <summary>SmokeRender3x6GRID — the 3×6 smoke puff spritesheet.</summary>
        public static readonly Asset<Texture2D> SmokeGrid =
            ModContent.Request<Texture2D>(VFXRoot + "SmokeRender3x6GRID", AssetRequestMode.ImmediateLoad);

        // ---- BLOOM / GLOW ----
        public static readonly Asset<Texture2D> SoftGlow =
            ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> GlowOrb =
            ModContent.Request<Texture2D>(Bloom + "GlowOrb", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> StarFlare =
            ModContent.Request<Texture2D>(Bloom + "StarFlare", AssetRequestMode.ImmediateLoad);

        public static readonly Asset<Texture2D> PointBloom =
            ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);

        // ---- HELPERS ----

        /// <summary>
        /// Returns the source rectangle for a specific frame in the 3×6 grid.
        /// Frames are numbered 0–17, row-major (left-to-right, top-to-bottom).
        /// </summary>
        public static Rectangle GetFrameRect(int frameIndex)
        {
            Texture2D tex = SmokeGrid.Value;
            int frameWidth = tex.Width / GridColumns;
            int frameHeight = tex.Height / GridRows;
            int col = frameIndex % GridColumns;
            int row = frameIndex / GridColumns;
            return new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
        }

        /// <summary>
        /// Returns the origin (center) of a single frame for centered drawing.
        /// </summary>
        public static Vector2 GetFrameOrigin()
        {
            Texture2D tex = SmokeGrid.Value;
            int frameWidth = tex.Width / GridColumns;
            int frameHeight = tex.Height / GridRows;
            return new Vector2(frameWidth / 2f, frameHeight / 2f);
        }

        public static string GetStyleName(SmokeCloudStyle style) => style switch
        {
            SmokeCloudStyle.CosmicIndigo => "Cosmic Indigo",
            SmokeCloudStyle.InfernalEmber => "Infernal Ember",
            SmokeCloudStyle.SpectralMist => "Spectral Mist",
            SmokeCloudStyle.AlchemicalFume => "Alchemical Fume",
            SmokeCloudStyle.GoldenHaze => "Golden Haze",
            SmokeCloudStyle.CrimsonVeil => "Crimson Veil",
            _ => "Unknown",
        };

        /// <summary>
        /// Each smoke style has a 3-color palette:
        ///  [0] = main/body color, [1] = core/hot color, [2] = edge/cool color.
        /// The smoke lifecycle color-shifts from [1] → [0] → [2].
        /// </summary>
        public static Color[] GetStyleColors(SmokeCloudStyle style) => style switch
        {
            // Calamity Supernova style — deep indigo/purple
            SmokeCloudStyle.CosmicIndigo => new[] {
                new Color(57, 46, 115),    // Body: deep indigo
                new Color(120, 90, 200),   // Core: bright purple
                new Color(30, 25, 60),     // Edge: near-black violet
            },
            // Infernal black-orange with fire cores
            SmokeCloudStyle.InfernalEmber => new[] {
                new Color(60, 30, 10),     // Body: dark brown-smoke
                new Color(255, 140, 40),   // Core: hot orange
                new Color(20, 10, 5),      // Edge: near-black soot
            },
            // Ethereal blue-white phantasmal mist
            SmokeCloudStyle.SpectralMist => new[] {
                new Color(120, 160, 200),  // Body: pale blue
                new Color(220, 240, 255),  // Core: near-white blue
                new Color(50, 70, 100),    // Edge: dark blue-gray
            },
            // Toxic green-purple alchemical fumes
            SmokeCloudStyle.AlchemicalFume => new[] {
                new Color(40, 120, 50),    // Body: dark green
                new Color(100, 220, 80),   // Core: bright toxic green
                new Color(60, 30, 90),     // Edge: dark purple
            },
            // Warm golden incense haze
            SmokeCloudStyle.GoldenHaze => new[] {
                new Color(160, 120, 40),   // Body: warm amber
                new Color(255, 220, 100),  // Core: bright gold
                new Color(80, 50, 20),     // Edge: dark brown
            },
            // Crimson blood-red smoke
            SmokeCloudStyle.CrimsonVeil => new[] {
                new Color(140, 20, 20),    // Body: dark crimson
                new Color(220, 60, 40),    // Core: bright red
                new Color(50, 10, 10),     // Edge: near-black red
            },
            _ => new[] { Color.Gray, Color.White, Color.DarkGray },
        };
    }
}
