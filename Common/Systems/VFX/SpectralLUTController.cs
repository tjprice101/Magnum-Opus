using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// SPECTRAL LUT CONTROLLER - Fraunhofer-Style Chromatic Effects
    /// 
    /// Implements spectral sampling with chromatic diffraction patterns.
    /// Based on how real light spectra work - wavelength-based color mapping
    /// with absorption lines (Fraunhofer lines) for authentic optical effects.
    /// 
    /// Key Features:
    /// - Wavelength-to-RGB conversion (physical spectrum)
    /// - Absorption line masking (dark bands in spectrum)
    /// - Chromatic dispersion (rainbow edge effects)
    /// - Temperature-based color mapping (black body radiation)
    /// - Gradient LUT generation for shader consumption
    /// </summary>
    public class SpectralLUTController : ModSystem
    {
        #region Static Instance
        
        private static SpectralLUTController _instance;
        public static SpectralLUTController Instance => _instance;
        
        #endregion
        
        #region LUT Textures
        
        // Generated LUT textures (1D gradient textures)
        private Texture2D _spectrumLUT;           // Physical visible spectrum
        private Texture2D _fraunhoferLUT;         // Spectrum with absorption lines
        private Texture2D _blackBodyLUT;          // Temperature-based colors
        private Texture2D _chromaticLUT;          // Rainbow for diffraction
        
        // Theme-specific LUTs
        private Dictionary<string, Texture2D> _themeLUTs;
        
        // LUT dimensions
        private const int LUT_WIDTH = 256;
        private const int LUT_HEIGHT = 1;
        
        #endregion
        
        #region Physical Constants
        
        // Visible light wavelength range (in nanometers)
        private const float WAVELENGTH_MIN = 380f;  // Violet
        private const float WAVELENGTH_MAX = 780f;  // Red
        
        // Fraunhofer absorption lines (wavelength in nm, normalized intensity reduction)
        private static readonly (float wavelength, float strength)[] FraunhoferLines = new[]
        {
            (393.4f, 0.8f),   // K line (Calcium)
            (396.8f, 0.7f),   // H line (Calcium)
            (410.2f, 0.5f),   // h line (Hydrogen delta)
            (434.0f, 0.6f),   // G line (Hydrogen gamma)
            (486.1f, 0.7f),   // F line (Hydrogen beta)
            (516.7f, 0.3f),   // b1 line (Magnesium)
            (589.0f, 0.9f),   // D lines (Sodium)
            (656.3f, 0.8f),   // C line (Hydrogen alpha)
            (686.7f, 0.4f),   // B line (Oxygen)
            (759.4f, 0.5f),   // A line (Oxygen)
        };
        
        #endregion
        
        #region Lifecycle
        
        public override void Load()
        {
            _instance = this;
            _themeLUTs = new Dictionary<string, Texture2D>();
            
            Main.QueueMainThreadAction(GenerateAllLUTs);
        }
        
        public override void Unload()
        {
            _instance = null;
            
            // Capture references for disposal on main thread
            var spectrumLUT = _spectrumLUT;
            var fraunhoferLUT = _fraunhoferLUT;
            var blackBodyLUT = _blackBodyLUT;
            var chromaticLUT = _chromaticLUT;
            var themeLUTs = _themeLUTs;
            
            // Clear references immediately
            _spectrumLUT = null;
            _fraunhoferLUT = null;
            _blackBodyLUT = null;
            _chromaticLUT = null;
            _themeLUTs = null;
            
            // Queue disposal to main thread (Unload can be called from background thread)
            Main.QueueMainThreadAction(() =>
            {
                try
                {
                    spectrumLUT?.Dispose();
                    fraunhoferLUT?.Dispose();
                    blackBodyLUT?.Dispose();
                    chromaticLUT?.Dispose();
                    
                    if (themeLUTs != null)
                    {
                        foreach (var lut in themeLUTs.Values)
                        {
                            lut?.Dispose();
                        }
                        themeLUTs.Clear();
                    }
                }
                catch { /* Ignore disposal errors during unload */ }
            });
        }
        
        private void GenerateAllLUTs()
        {
            if (Main.dedServ) return;
            
            _spectrumLUT = GenerateSpectrumLUT();
            _fraunhoferLUT = GenerateFraunhoferLUT();
            _blackBodyLUT = GenerateBlackBodyLUT();
            _chromaticLUT = GenerateChromaticLUT();
            
            // Generate theme LUTs
            GenerateThemeLUT("Eroica", 
                new Color(139, 0, 0),      // Deep scarlet
                new Color(220, 50, 50),    // Crimson
                new Color(255, 100, 50),   // Flame
                new Color(255, 200, 80),   // Gold
                new Color(255, 215, 0)     // Pure gold
            );
            
            GenerateThemeLUT("Fate",
                new Color(15, 5, 20),      // Void black
                new Color(120, 30, 140),   // Deep purple
                new Color(180, 50, 100),   // Dark pink
                new Color(255, 60, 80),    // Bright red
                new Color(255, 255, 255)   // Star white
            );
            
            GenerateThemeLUT("SwanLake",
                new Color(255, 255, 255),  // Pure white
                new Color(220, 225, 235),  // Silver
                new Color(180, 180, 200),  // Pale gray
                new Color(80, 80, 100),    // Dark gray
                new Color(20, 20, 30)      // Near black
            );
            
            GenerateThemeLUT("LaCampanella",
                new Color(20, 15, 20),     // Black
                new Color(100, 40, 20),    // Dark ember
                new Color(255, 100, 0),    // Orange
                new Color(255, 180, 50),   // Light orange
                new Color(255, 220, 100)   // Golden flame
            );
            
            GenerateThemeLUT("MoonlightSonata",
                new Color(75, 0, 130),     // Indigo
                new Color(138, 43, 226),   // Violet
                new Color(150, 100, 220),  // Medium purple
                new Color(135, 206, 250),  // Light blue
                new Color(220, 220, 235)   // Silver
            );
            
            GenerateThemeLUT("EnigmaVariations",
                new Color(15, 10, 20),     // Void black
                new Color(80, 20, 120),    // Deep purple
                new Color(140, 60, 200),   // Purple
                new Color(50, 220, 100),   // Green flame
                new Color(30, 100, 50)     // Dark green
            );
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Gets the physical visible spectrum LUT.
        /// </summary>
        public static Texture2D GetSpectrumLUT() => _instance?._spectrumLUT;
        
        /// <summary>
        /// Gets the spectrum with Fraunhofer absorption lines.
        /// </summary>
        public static Texture2D GetFraunhoferLUT() => _instance?._fraunhoferLUT;
        
        /// <summary>
        /// Gets the black body radiation LUT (temperature colors).
        /// </summary>
        public static Texture2D GetBlackBodyLUT() => _instance?._blackBodyLUT;
        
        /// <summary>
        /// Gets the chromatic dispersion LUT (clean rainbow).
        /// </summary>
        public static Texture2D GetChromaticLUT() => _instance?._chromaticLUT;
        
        /// <summary>
        /// Gets a theme-specific gradient LUT.
        /// </summary>
        public static Texture2D GetThemeLUT(string theme)
        {
            if (_instance?._themeLUTs == null) return null;
            return _instance._themeLUTs.TryGetValue(theme, out var lut) ? lut : _instance?._chromaticLUT;
        }
        
        /// <summary>
        /// Samples the spectrum at a given wavelength (380-780nm).
        /// </summary>
        public static Color SampleWavelength(float wavelengthNm)
        {
            return WavelengthToRGB(wavelengthNm);
        }
        
        /// <summary>
        /// Samples the Fraunhofer spectrum with absorption lines.
        /// </summary>
        public static Color SampleFraunhofer(float t)
        {
            float wavelength = MathHelper.Lerp(WAVELENGTH_MIN, WAVELENGTH_MAX, t);
            Color baseColor = WavelengthToRGB(wavelength);
            float absorption = GetFraunhoferAbsorption(wavelength);
            return baseColor * absorption;
        }
        
        /// <summary>
        /// Samples black body color at temperature (in Kelvin).
        /// </summary>
        public static Color SampleBlackBody(float temperatureKelvin)
        {
            return BlackBodyToRGB(temperatureKelvin);
        }
        
        /// <summary>
        /// Creates a chromatic dispersion effect (rainbow split).
        /// </summary>
        public static Color GetDispersedColor(float t, float dispersion)
        {
            // Sample multiple wavelengths for spectral separation
            Color r = SampleWavelength(MathHelper.Lerp(620, 700, t + dispersion));
            Color g = SampleWavelength(MathHelper.Lerp(495, 570, t));
            Color b = SampleWavelength(MathHelper.Lerp(450, 495, t - dispersion));
            
            return new Color(r.R, g.G, b.B, 255);
        }
        
        #endregion
        
        #region LUT Generation
        
        private Texture2D GenerateSpectrumLUT()
        {
            Color[] data = new Color[LUT_WIDTH * LUT_HEIGHT];
            
            for (int i = 0; i < LUT_WIDTH; i++)
            {
                float t = i / (float)(LUT_WIDTH - 1);
                float wavelength = MathHelper.Lerp(WAVELENGTH_MIN, WAVELENGTH_MAX, t);
                data[i] = WavelengthToRGB(wavelength);
            }
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, LUT_WIDTH, LUT_HEIGHT);
            texture.SetData(data);
            return texture;
        }
        
        private Texture2D GenerateFraunhoferLUT()
        {
            Color[] data = new Color[LUT_WIDTH * LUT_HEIGHT];
            
            for (int i = 0; i < LUT_WIDTH; i++)
            {
                float t = i / (float)(LUT_WIDTH - 1);
                float wavelength = MathHelper.Lerp(WAVELENGTH_MIN, WAVELENGTH_MAX, t);
                Color baseColor = WavelengthToRGB(wavelength);
                float absorption = GetFraunhoferAbsorption(wavelength);
                
                data[i] = new Color(
                    (byte)(baseColor.R * absorption),
                    (byte)(baseColor.G * absorption),
                    (byte)(baseColor.B * absorption),
                    255
                );
            }
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, LUT_WIDTH, LUT_HEIGHT);
            texture.SetData(data);
            return texture;
        }
        
        private Texture2D GenerateBlackBodyLUT()
        {
            Color[] data = new Color[LUT_WIDTH * LUT_HEIGHT];
            
            // Temperature range: 1000K (red) to 10000K (blue-white)
            for (int i = 0; i < LUT_WIDTH; i++)
            {
                float t = i / (float)(LUT_WIDTH - 1);
                float temperature = MathHelper.Lerp(1000f, 10000f, t);
                data[i] = BlackBodyToRGB(temperature);
            }
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, LUT_WIDTH, LUT_HEIGHT);
            texture.SetData(data);
            return texture;
        }
        
        private Texture2D GenerateChromaticLUT()
        {
            Color[] data = new Color[LUT_WIDTH * LUT_HEIGHT];
            
            for (int i = 0; i < LUT_WIDTH; i++)
            {
                float t = i / (float)(LUT_WIDTH - 1);
                // HSL to RGB for smooth rainbow
                data[i] = Main.hslToRgb(t, 1f, 0.5f);
            }
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, LUT_WIDTH, LUT_HEIGHT);
            texture.SetData(data);
            return texture;
        }
        
        private void GenerateThemeLUT(string theme, params Color[] colors)
        {
            if (colors.Length == 0) return;
            
            Color[] data = new Color[LUT_WIDTH * LUT_HEIGHT];
            
            for (int i = 0; i < LUT_WIDTH; i++)
            {
                float t = i / (float)(LUT_WIDTH - 1);
                data[i] = SampleGradient(colors, t);
            }
            
            Texture2D texture = new Texture2D(Main.graphics.GraphicsDevice, LUT_WIDTH, LUT_HEIGHT);
            texture.SetData(data);
            _themeLUTs[theme] = texture;
        }
        
        #endregion
        
        #region Color Science
        
        /// <summary>
        /// Converts wavelength (in nm) to RGB color.
        /// Based on CIE 1931 color matching functions approximation.
        /// </summary>
        private static Color WavelengthToRGB(float wavelength)
        {
            float gamma = 0.8f;
            float intensityMax = 1f;
            
            float r, g, b;
            
            if (wavelength >= 380 && wavelength < 440)
            {
                r = -(wavelength - 440) / (440 - 380);
                g = 0;
                b = 1;
            }
            else if (wavelength >= 440 && wavelength < 490)
            {
                r = 0;
                g = (wavelength - 440) / (490 - 440);
                b = 1;
            }
            else if (wavelength >= 490 && wavelength < 510)
            {
                r = 0;
                g = 1;
                b = -(wavelength - 510) / (510 - 490);
            }
            else if (wavelength >= 510 && wavelength < 580)
            {
                r = (wavelength - 510) / (580 - 510);
                g = 1;
                b = 0;
            }
            else if (wavelength >= 580 && wavelength < 645)
            {
                r = 1;
                g = -(wavelength - 645) / (645 - 580);
                b = 0;
            }
            else if (wavelength >= 645 && wavelength <= 780)
            {
                r = 1;
                g = 0;
                b = 0;
            }
            else
            {
                r = 0;
                g = 0;
                b = 0;
            }
            
            // Intensity falloff at edges
            float factor;
            if (wavelength >= 380 && wavelength < 420)
                factor = 0.3f + 0.7f * (wavelength - 380) / (420 - 380);
            else if (wavelength >= 420 && wavelength < 700)
                factor = 1f;
            else if (wavelength >= 700 && wavelength <= 780)
                factor = 0.3f + 0.7f * (780 - wavelength) / (780 - 700);
            else
                factor = 0f;
            
            // Apply gamma correction
            r = (float)Math.Pow(r * factor * intensityMax, gamma);
            g = (float)Math.Pow(g * factor * intensityMax, gamma);
            b = (float)Math.Pow(b * factor * intensityMax, gamma);
            
            return new Color(
                (byte)Math.Min(255, r * 255),
                (byte)Math.Min(255, g * 255),
                (byte)Math.Min(255, b * 255),
                255
            );
        }
        
        /// <summary>
        /// Calculates the absorption factor at a given wavelength due to Fraunhofer lines.
        /// Returns 1.0 for no absorption, lower values indicate absorption.
        /// </summary>
        private static float GetFraunhoferAbsorption(float wavelength)
        {
            float absorption = 1f;
            
            foreach (var (lineWavelength, strength) in FraunhoferLines)
            {
                // Gaussian absorption profile
                float dist = wavelength - lineWavelength;
                float lineWidth = 2.5f; // nm
                float lineAbsorption = strength * (float)Math.Exp(-(dist * dist) / (2f * lineWidth * lineWidth));
                absorption -= lineAbsorption;
            }
            
            return Math.Max(0f, absorption);
        }
        
        /// <summary>
        /// Converts black body temperature (Kelvin) to RGB color.
        /// Based on Planckian locus approximation.
        /// </summary>
        private static Color BlackBodyToRGB(float temperatureKelvin)
        {
            float temp = temperatureKelvin / 100f;
            float r, g, b;
            
            // Red
            if (temp <= 66)
                r = 255;
            else
            {
                r = temp - 60;
                r = 329.698727446f * (float)Math.Pow(r, -0.1332047592f);
            }
            
            // Green
            if (temp <= 66)
            {
                g = temp;
                g = 99.4708025861f * (float)Math.Log(g) - 161.1195681661f;
            }
            else
            {
                g = temp - 60;
                g = 288.1221695283f * (float)Math.Pow(g, -0.0755148492f);
            }
            
            // Blue
            if (temp >= 66)
                b = 255;
            else if (temp <= 19)
                b = 0;
            else
            {
                b = temp - 10;
                b = 138.5177312231f * (float)Math.Log(b) - 305.0447927307f;
            }
            
            return new Color(
                (byte)MathHelper.Clamp(r, 0, 255),
                (byte)MathHelper.Clamp(g, 0, 255),
                (byte)MathHelper.Clamp(b, 0, 255),
                255
            );
        }
        
        /// <summary>
        /// Samples a multi-color gradient at position t (0-1).
        /// </summary>
        private static Color SampleGradient(Color[] colors, float t)
        {
            if (colors.Length == 1) return colors[0];
            
            float scaledT = t * (colors.Length - 1);
            int index = (int)Math.Floor(scaledT);
            float localT = scaledT - index;
            
            index = Math.Clamp(index, 0, colors.Length - 2);
            
            return Color.Lerp(colors[index], colors[index + 1], localT);
        }
        
        #endregion
        
        #region Shader Integration
        
        /// <summary>
        /// Applies spectral LUT parameters to a shader.
        /// </summary>
        public static void ApplyToShader(Effect shader, string theme = null)
        {
            if (_instance == null || shader == null) return;
            
            Texture2D lut = theme != null ? GetThemeLUT(theme) : _instance._chromaticLUT;
            shader.Parameters["uPaletteLUT"]?.SetValue(lut);
            
            // Additional spectral parameters
            shader.Parameters["uSpectrumOffset"]?.SetValue(0f);
            shader.Parameters["uSpectrumScale"]?.SetValue(1f);
        }
        
        /// <summary>
        /// Applies chromatic dispersion effect parameters.
        /// </summary>
        public static void ApplyChromaticDispersion(Effect shader, float dispersionStrength = 0.02f)
        {
            if (shader == null) return;
            
            shader.Parameters["uChromaticStrength"]?.SetValue(dispersionStrength);
            shader.Parameters["uChromaticOffset"]?.SetValue(new Vector2(dispersionStrength, 0));
        }
        
        #endregion
    }
}
