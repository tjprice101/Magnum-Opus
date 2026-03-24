using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Configuration for the MagnumOpus full-rotation swing system.
    /// Stub implementation — to be expanded when global swing system is built out.
    /// </summary>
    public class MagnumSwingConfig
    {
        public Color PrimaryColor { get; set; } = Color.White;
        public Color SecondaryColor { get; set; } = Color.Gray;
    }

    /// <summary>
    /// Preset theme configs for MagnumSwingConfig.
    /// </summary>
    public static class MagnumSwingConfigs
    {
        public static readonly MagnumSwingConfig SwanLake = new MagnumSwingConfig
        {
            PrimaryColor = new Color(240, 240, 255),
            SecondaryColor = new Color(30, 30, 40)
        };

        public static readonly MagnumSwingConfig LaCampanella = new MagnumSwingConfig
        {
            PrimaryColor = new Color(255, 140, 40),
            SecondaryColor = new Color(30, 20, 25)
        };

        public static readonly MagnumSwingConfig Eroica = new MagnumSwingConfig
        {
            PrimaryColor = new Color(200, 50, 50),
            SecondaryColor = new Color(255, 215, 0)
        };

        public static readonly MagnumSwingConfig MoonlightSonata = new MagnumSwingConfig
        {
            PrimaryColor = new Color(138, 43, 226),
            SecondaryColor = new Color(135, 206, 250)
        };

        public static readonly MagnumSwingConfig EnigmaVariations = new MagnumSwingConfig
        {
            PrimaryColor = new Color(140, 60, 200),
            SecondaryColor = new Color(50, 220, 100)
        };

        public static readonly MagnumSwingConfig Fate = new MagnumSwingConfig
        {
            PrimaryColor = new Color(180, 50, 100),
            SecondaryColor = new Color(255, 60, 80)
        };

        public static readonly MagnumSwingConfig Spring = new MagnumSwingConfig
        {
            PrimaryColor = new Color(255, 183, 197),
            SecondaryColor = new Color(144, 238, 144)
        };

        public static readonly MagnumSwingConfig Summer = new MagnumSwingConfig
        {
            PrimaryColor = new Color(255, 140, 40),
            SecondaryColor = new Color(255, 200, 80)
        };

        public static readonly MagnumSwingConfig Autumn = new MagnumSwingConfig
        {
            PrimaryColor = new Color(215, 120, 55),
            SecondaryColor = new Color(255, 190, 90)
        };

        public static readonly MagnumSwingConfig Winter = new MagnumSwingConfig
        {
            PrimaryColor = new Color(140, 190, 255),
            SecondaryColor = new Color(200, 235, 255)
        };

        public static readonly MagnumSwingConfig Seasons = new MagnumSwingConfig
        {
            PrimaryColor = new Color(170, 205, 255),
            SecondaryColor = new Color(255, 205, 140)
        };
    }

    /// <summary>
    /// ModPlayer for the MagnumOpus global melee swing system.
    /// Stub implementation — to be expanded when global swing system is built out.
    /// </summary>
    public class MagnumMeleePlayer : ModPlayer
    {
        private MagnumSwingConfig currentConfig;
        public bool IsSwinging { get; private set; }

        public void SetHeldWeaponConfig(Item item, MagnumSwingConfig config)
        {
            currentConfig = config;
        }

        public float GetCurrentSwingAngle()
        {
            return 0f;
        }
    }
}
