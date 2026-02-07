using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// BEAM VARIATION SYSTEM
    /// 
    /// Provides specialized beam types with unique width profiles and behaviors:
    /// 
    /// 1. THIN BEAMS - Needle-like precision lasers (Fate, precision weapons)
    /// 2. WIDE BEAMS - Massive devastation beams (boss attacks, ultimate abilities)
    /// 3. CURVED BEAMS - Arc/sine-wave beams (homing, musical wave effects)
    /// 4. FLARE BEAMS - Start thin, expand dramatically (charging attacks, releases)
    /// 5. PULSING BEAMS - Width oscillates over time (sustained channeling)
    /// 6. TAPERED BEAMS - Thick at source, thin at tip (standard laser)
    /// 7. DOUBLE HELIX - Two intertwined beam strands (DNA-style, cosmic)
    /// 
    /// Usage:
    ///   var beam = BeamVariations.CreateThinBeam(start, end, Color.White);
    ///   beam.Draw(spriteBatch);
    ///   
    ///   // Or use width functions directly:
    ///   var settings = new PrimitiveSettings(
    ///       BeamVariations.FlareWidth(4f, 40f, 0.3f),
    ///       BeamVariations.ThemeColorGlow("Fate")
    ///   );
    /// </summary>
    public static class BeamVariations
    {
        #region Width Function Presets
        
        // ============================================================
        // THIN BEAMS - Needle-like precision
        // ============================================================
        
        /// <summary>
        /// Extremely thin beam with slight taper. Perfect for precision lasers.
        /// Width: 2-6 pixels typically.
        /// </summary>
        /// <param name="width">Base width (recommend 2-6)</param>
        /// <param name="coreRatio">How much of the beam is full-width before taper (0-1)</param>
        public static EnhancedTrailRenderer.WidthFunction ThinNeedle(float width = 3f, float coreRatio = 0.7f)
        {
            return completionRatio =>
            {
                if (completionRatio < coreRatio)
                    return width;
                
                // Sharp taper at the end
                float taperProgress = (completionRatio - coreRatio) / (1f - coreRatio);
                return width * (1f - taperProgress * taperProgress); // Quadratic falloff
            };
        }
        
        /// <summary>
        /// Hair-thin beam that's constant width with soft ends.
        /// Perfect for: Fate theme precision strikes, instant-hit weapons.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction HairThin(float width = 2f)
        {
            return completionRatio =>
            {
                // Soft start
                float startFade = Math.Min(1f, completionRatio * 10f);
                // Soft end
                float endFade = Math.Min(1f, (1f - completionRatio) * 10f);
                return width * startFade * endFade;
            };
        }
        
        /// <summary>
        /// Thread beam with subtle pulsing effect.
        /// Perfect for: Sustained thin lasers, channeling effects.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction ThreadPulse(float baseWidth = 2.5f, float pulseAmount = 0.5f)
        {
            return completionRatio =>
            {
                float pulse = 1f + MathF.Sin(completionRatio * MathHelper.TwoPi * 3f + Main.GlobalTimeWrappedHourly * 8f) * pulseAmount;
                float taper = 1f - completionRatio * 0.3f;
                return baseWidth * pulse * taper;
            };
        }
        
        // ============================================================
        // WIDE BEAMS - Massive devastation
        // ============================================================
        
        /// <summary>
        /// Massive constant-width beam. Boss-level destruction.
        /// Width: 40-120 pixels typically.
        /// </summary>
        /// <param name="width">Base width (recommend 40-120)</param>
        /// <param name="edgeSoftness">How soft the edges are (0 = sharp, 1 = very soft)</param>
        public static EnhancedTrailRenderer.WidthFunction MassiveConstant(float width = 80f, float edgeSoftness = 0.1f)
        {
            return completionRatio =>
            {
                // Soft fade at very start and end
                float edgeFade = 1f;
                if (completionRatio < edgeSoftness)
                    edgeFade = completionRatio / edgeSoftness;
                else if (completionRatio > 1f - edgeSoftness)
                    edgeFade = (1f - completionRatio) / edgeSoftness;
                
                return width * edgeFade;
            };
        }
        
        /// <summary>
        /// Wide beam with turbulent edges that wobble.
        /// Perfect for: Unstable energy beams, overloaded weapons.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction TurbulentWide(float width = 60f, float turbulence = 8f)
        {
            return completionRatio =>
            {
                // Multiple sine waves for organic turbulence
                float noise = MathF.Sin(completionRatio * 23.7f + Main.GlobalTimeWrappedHourly * 12f) * 0.5f +
                              MathF.Sin(completionRatio * 41.3f + Main.GlobalTimeWrappedHourly * 17f) * 0.3f +
                              MathF.Sin(completionRatio * 67.1f + Main.GlobalTimeWrappedHourly * 23f) * 0.2f;
                
                float taper = 1f - completionRatio * 0.15f;
                return (width + noise * turbulence) * taper;
            };
        }
        
        /// <summary>
        /// Segmented wide beam (Exo Mech style).
        /// Creates visible "segments" along the beam length.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction SegmentedWide(float width = 50f, int segments = 8, float segmentDepth = 0.3f)
        {
            return completionRatio =>
            {
                float segmentProgress = (completionRatio * segments) % 1f;
                float segmentFactor = 1f - MathF.Sin(segmentProgress * MathHelper.Pi) * segmentDepth;
                float taper = 1f - completionRatio * 0.2f;
                return width * segmentFactor * taper;
            };
        }
        
        // ============================================================
        // CURVED BEAMS - Arc and wave patterns
        // ============================================================
        
        /// <summary>
        /// Creates offset function for sine-wave curved beam.
        /// Apply to trail positions for wavy beam effect.
        /// </summary>
        /// <param name="amplitude">Wave height in pixels</param>
        /// <param name="frequency">Number of complete waves along beam</param>
        /// <param name="animationSpeed">How fast the wave animates</param>
        public static Func<float, Vector2> SineWaveOffset(float amplitude = 15f, float frequency = 2f, float animationSpeed = 3f)
        {
            return completionRatio =>
            {
                float wave = MathF.Sin(completionRatio * MathHelper.TwoPi * frequency + Main.GlobalTimeWrappedHourly * animationSpeed);
                // Perpendicular offset (will be rotated by actual beam direction)
                return new Vector2(0, wave * amplitude);
            };
        }
        
        /// <summary>
        /// Creates offset function for smooth arc (bow-shaped curve).
        /// </summary>
        /// <param name="arcHeight">Maximum height of arc at midpoint</param>
        /// <param name="arcBias">Where the arc peaks (0.5 = center)</param>
        public static Func<float, Vector2> ArcOffset(float arcHeight = 40f, float arcBias = 0.5f)
        {
            return completionRatio =>
            {
                // Parabolic arc
                float normalizedPos = (completionRatio - arcBias) / arcBias;
                float arcFactor = 1f - normalizedPos * normalizedPos;
                arcFactor = Math.Max(0, arcFactor);
                return new Vector2(0, -arcFactor * arcHeight);
            };
        }
        
        /// <summary>
        /// Creates offset function for S-curve (musical wave).
        /// Perfect for: Swan Lake, musical themes.
        /// </summary>
        public static Func<float, Vector2> SCurveOffset(float amplitude = 25f)
        {
            return completionRatio =>
            {
                // S-curve using cubic function
                float t = completionRatio * 2f - 1f; // -1 to 1
                float sCurve = t * t * t; // Cubic for S-shape
                return new Vector2(0, sCurve * amplitude);
            };
        }
        
        /// <summary>
        /// Spiral offset for helix-style beams.
        /// </summary>
        public static Func<float, Vector2> SpiralOffset(float radius = 10f, float rotations = 3f, float animationSpeed = 2f)
        {
            return completionRatio =>
            {
                float angle = completionRatio * MathHelper.TwoPi * rotations + Main.GlobalTimeWrappedHourly * animationSpeed;
                return new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius * (1f - completionRatio);
            };
        }
        
        /// <summary>
        /// Width function for curved beams - maintains visual consistency on curves.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction CurvedBeamWidth(float width = 12f)
        {
            return completionRatio =>
            {
                // Slightly thicker in middle to compensate for curve stretching
                float curveCompensation = 1f + MathF.Sin(completionRatio * MathHelper.Pi) * 0.2f;
                float taper = 1f - completionRatio * 0.25f;
                return width * curveCompensation * taper;
            };
        }
        
        // ============================================================
        // FLARE BEAMS - Start thin, expand dramatically
        // ============================================================
        
        /// <summary>
        /// Starts narrow, expands to wide. Perfect for charging attacks.
        /// The "trumpet" profile.
        /// </summary>
        /// <param name="startWidth">Width at beam origin</param>
        /// <param name="endWidth">Width at beam end</param>
        /// <param name="flarePoint">Where the flare begins (0-1)</param>
        public static EnhancedTrailRenderer.WidthFunction FlareOutward(float startWidth = 4f, float endWidth = 40f, float flarePoint = 0.3f)
        {
            return completionRatio =>
            {
                if (completionRatio < flarePoint)
                {
                    // Constant thin section
                    return startWidth;
                }
                else
                {
                    // Exponential flare
                    float flareProgress = (completionRatio - flarePoint) / (1f - flarePoint);
                    float exponentialFlare = flareProgress * flareProgress; // Quadratic expansion
                    return MathHelper.Lerp(startWidth, endWidth, exponentialFlare);
                }
            };
        }
        
        /// <summary>
        /// Dramatic flare with oscillating end (like fire/energy).
        /// Perfect for: La Campanella, fire-themed weapons.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction FlareWithFlicker(float startWidth = 4f, float endWidth = 50f, float flickerAmount = 0.2f)
        {
            return completionRatio =>
            {
                // Base flare
                float flare = MathF.Pow(completionRatio, 1.5f);
                float baseWidth = MathHelper.Lerp(startWidth, endWidth, flare);
                
                // Add flicker at the expanded end
                float flicker = 1f + MathF.Sin(completionRatio * 30f + Main.GlobalTimeWrappedHourly * 15f) * flickerAmount * completionRatio;
                
                return baseWidth * flicker;
            };
        }
        
        /// <summary>
        /// "Shotgun blast" pattern - rapid expansion then slight taper.
        /// Perfect for: Explosive releases, shotgun-style beams.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction BlastFlare(float startWidth = 6f, float maxWidth = 80f, float expansionPoint = 0.2f)
        {
            return completionRatio =>
            {
                if (completionRatio < expansionPoint)
                {
                    // Rapid expansion
                    float expandProgress = completionRatio / expansionPoint;
                    return MathHelper.Lerp(startWidth, maxWidth, MathF.Sqrt(expandProgress));
                }
                else
                {
                    // Gradual taper after expansion
                    float taperProgress = (completionRatio - expansionPoint) / (1f - expansionPoint);
                    return maxWidth * (1f - taperProgress * 0.3f);
                }
            };
        }
        
        /// <summary>
        /// Cone beam that expands linearly. Clean geometric expansion.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction ConeExpand(float startWidth = 2f, float endWidth = 60f)
        {
            return completionRatio => MathHelper.Lerp(startWidth, endWidth, completionRatio);
        }
        
        /// <summary>
        /// Inverse flare - starts wide, ends thin. Good for suction effects.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction InverseFlare(float startWidth = 50f, float endWidth = 3f, float flarePoint = 0.7f)
        {
            return completionRatio =>
            {
                if (completionRatio < flarePoint)
                {
                    // Gradual taper
                    float progress = completionRatio / flarePoint;
                    return MathHelper.Lerp(startWidth, startWidth * 0.6f, progress);
                }
                else
                {
                    // Rapid collapse
                    float collapseProgress = (completionRatio - flarePoint) / (1f - flarePoint);
                    float exponentialCollapse = MathF.Pow(collapseProgress, 0.5f);
                    return MathHelper.Lerp(startWidth * 0.6f, endWidth, exponentialCollapse);
                }
            };
        }
        
        // ============================================================
        // SPECIALIZED BEAM PROFILES
        // ============================================================
        
        /// <summary>
        /// Pulsing beam - width oscillates over time.
        /// Perfect for: Sustained channeling, charged beams.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction PulsingBeam(float baseWidth = 20f, float pulseAmount = 0.4f, float pulseSpeed = 5f)
        {
            return completionRatio =>
            {
                float timePulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * pulseSpeed) * pulseAmount;
                float spatialPulse = 1f + MathF.Sin(completionRatio * MathHelper.Pi * 2f) * 0.15f;
                float taper = 1f - completionRatio * 0.2f;
                return baseWidth * timePulse * spatialPulse * taper;
            };
        }
        
        /// <summary>
        /// Double-peak beam - thick at both ends, thin in middle.
        /// Good for: Portal beams, connection beams.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction DoublePeak(float peakWidth = 30f, float valleyWidth = 8f)
        {
            return completionRatio =>
            {
                // Two peaks at 0 and 1, valley at 0.5
                float valley = MathF.Sin(completionRatio * MathHelper.Pi);
                return MathHelper.Lerp(peakWidth, valleyWidth, valley);
            };
        }
        
        /// <summary>
        /// Multi-bulge beam - several thick sections along length.
        /// Good for: Energy transfer, magical conduits.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction MultiBulge(float baseWidth = 12f, float bulgeAmount = 10f, int bulgeCount = 4)
        {
            return completionRatio =>
            {
                float bulge = MathF.Abs(MathF.Sin(completionRatio * MathHelper.Pi * bulgeCount + Main.GlobalTimeWrappedHourly * 3f));
                float taper = 1f - completionRatio * 0.2f;
                return (baseWidth + bulge * bulgeAmount) * taper;
            };
        }
        
        /// <summary>
        /// Ribbon beam - very wide but thin depth (like a ribbon).
        /// Animates with a wave pattern.
        /// </summary>
        public static EnhancedTrailRenderer.WidthFunction RibbonWave(float width = 40f, float waveAmount = 0.5f)
        {
            return completionRatio =>
            {
                float wave = MathF.Cos(completionRatio * MathHelper.TwoPi * 2f + Main.GlobalTimeWrappedHourly * 4f);
                float waveWidth = width * (1f + wave * waveAmount);
                float taper = 1f - completionRatio * 0.15f;
                return waveWidth * taper;
            };
        }
        
        #endregion
        
        #region Color Function Presets for Beams
        
        /// <summary>
        /// Beam-optimized theme color with core brightness.
        /// </summary>
        public static EnhancedTrailRenderer.ColorFunction ThemeColorGlow(string themeName, float intensity = 1f)
        {
            Color[] palette = GetThemePalette(themeName);
            return completionRatio =>
            {
                Color baseColor = VFXUtilities.PaletteLerp(palette, completionRatio);
                // Brighter at start (source)
                float brightness = 1f - completionRatio * 0.4f;
                return baseColor * (brightness * intensity);
            };
        }
        
        /// <summary>
        /// Hot core transitioning to cooler edges.
        /// Perfect for: Fire/energy beams.
        /// </summary>
        public static EnhancedTrailRenderer.ColorFunction HotToColCore(Color coreColor, Color edgeColor)
        {
            return completionRatio =>
            {
                // Core is hot (white/bright), edges are colored
                float coreStrength = 1f - MathF.Pow(completionRatio, 0.5f);
                Color white = Color.White * coreStrength * 0.7f;
                Color colored = Color.Lerp(coreColor, edgeColor, completionRatio) * (1f - coreStrength * 0.3f);
                return new Color(
                    Math.Min(255, white.R + colored.R),
                    Math.Min(255, white.G + colored.G),
                    Math.Min(255, white.B + colored.B),
                    (byte)255
                );
            };
        }
        
        /// <summary>
        /// Rainbow beam that shifts along length.
        /// </summary>
        public static EnhancedTrailRenderer.ColorFunction RainbowBeam(float saturation = 0.9f, float brightness = 0.85f)
        {
            return completionRatio =>
            {
                float hue = (completionRatio + Main.GlobalTimeWrappedHourly * 0.5f) % 1f;
                return Main.hslToRgb(hue, saturation, brightness);
            };
        }
        
        /// <summary>
        /// Flickering beam color (for unstable energy).
        /// </summary>
        public static EnhancedTrailRenderer.ColorFunction FlickerColor(Color baseColor, float flickerIntensity = 0.3f)
        {
            return completionRatio =>
            {
                float flicker = 1f + (Main.rand.NextFloat() - 0.5f) * flickerIntensity;
                float taper = 1f - completionRatio * 0.3f;
                return baseColor * (flicker * taper);
            };
        }
        
        private static Color[] GetThemePalette(string themeName)
        {
            return themeName.ToLowerInvariant() switch
            {
                "lacampanella" or "campanella" => MagnumThemePalettes.LaCampanella,
                "eroica" => MagnumThemePalettes.Eroica,
                "moonlight" or "moonlightsonata" => MagnumThemePalettes.MoonlightSonata,
                "swanlake" or "swan" => MagnumThemePalettes.SwanLake,
                "enigma" or "enigmavariations" => MagnumThemePalettes.EnigmaVariations,
                "fate" => MagnumThemePalettes.Fate,
                "clair" or "clairdelune" => MagnumThemePalettes.ClairDeLune,
                "dies" or "diesirae" => MagnumThemePalettes.DiesIrae,
                _ => new[] { Color.White, Color.LightBlue }
            };
        }
        
        #endregion
        
        #region Beam Instance Class
        
        /// <summary>
        /// Represents a complete beam with position, width, color, and optional curve.
        /// Use for quick beam creation without manual setup.
        /// </summary>
        public class BeamInstance
        {
            public Vector2 Start { get; set; }
            public Vector2 End { get; set; }
            public EnhancedTrailRenderer.WidthFunction WidthFunc { get; set; }
            public EnhancedTrailRenderer.ColorFunction ColorFunc { get; set; }
            public Func<float, Vector2> OffsetFunc { get; set; }
            public int Segments { get; set; } = 40;
            public bool UseBloom { get; set; } = true;
            public float BloomScale { get; set; } = 2.5f;
            public float BloomOpacity { get; set; } = 0.35f;
            
            private Vector2[] _positions;
            
            /// <summary>
            /// Draws the beam with optional bloom pass.
            /// </summary>
            public void Draw(SpriteBatch spriteBatch)
            {
                if (Start == End) return;
                
                BuildPositions();
                
                var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                    WidthFunc,
                    ColorFunc,
                    OffsetFunc,
                    smoothen: true
                );
                
                // Bloom pass first
                if (UseBloom)
                {
                    var bloomSettings = new EnhancedTrailRenderer.PrimitiveSettings(
                        completionRatio => WidthFunc(completionRatio) * BloomScale,
                        completionRatio => ColorFunc(completionRatio) * BloomOpacity,
                        OffsetFunc,
                        smoothen: true
                    );
                    EnhancedTrailRenderer.RenderTrail(_positions, bloomSettings, Segments);
                }
                
                // Main pass
                EnhancedTrailRenderer.RenderTrail(_positions, settings, Segments);
            }
            
            private void BuildPositions()
            {
                _positions = new Vector2[Segments];
                Vector2 direction = End - Start;
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                if (perpendicular != Vector2.Zero)
                    perpendicular.Normalize();
                
                for (int i = 0; i < Segments; i++)
                {
                    float t = (float)i / (Segments - 1);
                    Vector2 basePos = Vector2.Lerp(Start, End, t);
                    
                    if (OffsetFunc != null)
                    {
                        Vector2 offset = OffsetFunc(t);
                        basePos += perpendicular * offset.Y; // Apply perpendicular offset
                    }
                    
                    _positions[i] = basePos;
                }
            }
        }
        
        #endregion
        
        #region Quick Beam Factory Methods
        
        /// <summary>
        /// Creates a thin precision beam.
        /// </summary>
        public static BeamInstance CreateThinBeam(Vector2 start, Vector2 end, Color color, float width = 3f)
        {
            return new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = ThinNeedle(width),
                ColorFunc = completionRatio => color * (1f - completionRatio * 0.3f),
                BloomScale = 3f,
                BloomOpacity = 0.25f
            };
        }
        
        /// <summary>
        /// Creates a massive devastation beam.
        /// </summary>
        public static BeamInstance CreateWideBeam(Vector2 start, Vector2 end, Color color, float width = 60f)
        {
            return new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = MassiveConstant(width),
                ColorFunc = HotToColCore(Color.White, color),
                Segments = 60,
                BloomScale = 2.2f,
                BloomOpacity = 0.4f
            };
        }
        
        /// <summary>
        /// Creates a curved sine-wave beam.
        /// </summary>
        public static BeamInstance CreateCurvedBeam(Vector2 start, Vector2 end, Color color, 
            float width = 15f, float amplitude = 20f, float frequency = 2f)
        {
            return new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = CurvedBeamWidth(width),
                ColorFunc = completionRatio => color * (1f - completionRatio * 0.25f),
                OffsetFunc = SineWaveOffset(amplitude, frequency),
                Segments = 60
            };
        }
        
        /// <summary>
        /// Creates an arc-shaped curved beam.
        /// </summary>
        public static BeamInstance CreateArcBeam(Vector2 start, Vector2 end, Color color,
            float width = 12f, float arcHeight = 50f)
        {
            return new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = CurvedBeamWidth(width),
                ColorFunc = completionRatio => color * (1f - completionRatio * 0.2f),
                OffsetFunc = ArcOffset(arcHeight),
                Segments = 50
            };
        }
        
        /// <summary>
        /// Creates a flare beam that expands outward.
        /// </summary>
        public static BeamInstance CreateFlareBeam(Vector2 start, Vector2 end, Color coreColor, Color flareColor,
            float startWidth = 4f, float endWidth = 50f, float flarePoint = 0.3f)
        {
            return new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = FlareOutward(startWidth, endWidth, flarePoint),
                ColorFunc = completionRatio => 
                {
                    Color c = Color.Lerp(coreColor, flareColor, completionRatio);
                    // Brighter core
                    float coreBrightness = 1f - completionRatio * 0.5f;
                    return Color.Lerp(c, Color.White, (1f - completionRatio) * 0.3f) * coreBrightness;
                },
                Segments = 50,
                BloomScale = 2f
            };
        }
        
        /// <summary>
        /// Creates a blast-style rapid expansion beam.
        /// </summary>
        public static BeamInstance CreateBlastBeam(Vector2 start, Vector2 end, Color color, 
            float startWidth = 8f, float maxWidth = 100f)
        {
            return new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = BlastFlare(startWidth, maxWidth),
                ColorFunc = HotToColCore(Color.White, color),
                Segments = 50,
                BloomScale = 2.5f,
                BloomOpacity = 0.45f
            };
        }
        
        /// <summary>
        /// Creates a theme-specific beam with all VFX.
        /// </summary>
        public static BeamInstance CreateThemedBeam(Vector2 start, Vector2 end, string theme,
            BeamType type = BeamType.Standard, float scale = 1f)
        {
            var beam = new BeamInstance
            {
                Start = start,
                End = end,
                ColorFunc = ThemeColorGlow(theme),
                Segments = 50
            };
            
            beam.WidthFunc = type switch
            {
                BeamType.Thin => ThinNeedle(3f * scale),
                BeamType.Wide => MassiveConstant(60f * scale),
                BeamType.Curved => CurvedBeamWidth(15f * scale),
                BeamType.Flare => FlareOutward(4f * scale, 40f * scale),
                BeamType.Pulsing => PulsingBeam(20f * scale),
                BeamType.Ribbon => RibbonWave(35f * scale),
                _ => EnhancedTrailRenderer.LinearTaper(20f * scale)
            };
            
            if (type == BeamType.Curved)
                beam.OffsetFunc = SineWaveOffset(20f * scale, 2f);
            
            return beam;
        }
        
        /// <summary>
        /// Creates a double helix (DNA-style) beam.
        /// </summary>
        public static void DrawDoubleHelixBeam(SpriteBatch spriteBatch, Vector2 start, Vector2 end, 
            Color color1, Color color2, float width = 8f, float helixRadius = 15f)
        {
            // First strand
            var strand1 = new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = CurvedBeamWidth(width),
                ColorFunc = completionRatio => color1 * (1f - completionRatio * 0.3f),
                OffsetFunc = SpiralOffset(helixRadius, 3f, 2f),
                Segments = 60
            };
            
            // Second strand (offset by 180 degrees)
            var strand2 = new BeamInstance
            {
                Start = start,
                End = end,
                WidthFunc = CurvedBeamWidth(width),
                ColorFunc = completionRatio => color2 * (1f - completionRatio * 0.3f),
                OffsetFunc = completionRatio =>
                {
                    var offset = SpiralOffset(helixRadius, 3f, 2f)(completionRatio);
                    return -offset; // Opposite side
                },
                Segments = 60
            };
            
            strand1.Draw(spriteBatch);
            strand2.Draw(spriteBatch);
        }
        
        #endregion
        
        #region Enums
        
        public enum BeamType
        {
            Standard,
            Thin,
            Wide,
            Curved,
            Flare,
            Pulsing,
            Ribbon
        }
        
        #endregion
    }
}
