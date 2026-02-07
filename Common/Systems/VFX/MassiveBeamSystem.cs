using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE MASSIVE BEAM RENDERING SYSTEM
    /// 
    /// Implements the sophisticated rendering techniques used in Calamity Mod:
    /// 
    /// 1. PRIMITIVE MESH VERTEX STRIPS
    ///    - Beams are rendered as vertex strips, not flat sprites
    ///    - Dynamic vertices along the beam path
    ///    - Allows real-time manipulation of shape
    /// 
    /// 2. DYNAMIC WIDTH FUNCTIONS
    ///    - WidthFunction determines thickness at any point
    ///    - Supports tapering, pulsing, bulging
    ///    - QuadraticBump for energy beams (thin→thick→thin)
    /// 
    /// 3. TEXTURE SCROLLING
    ///    - UV coordinates scroll along the beam
    ///    - Creates flowing energy/plasma effect
    ///    - Noise textures for organic feel
    /// 
    /// 4. MULTI-LAYER VISUAL STACKING
    ///    - Core beam (brightest, narrowest)
    ///    - Glow aura (semi-transparent, additive blurred)
    ///    - Outer bloom (large, dim)
    ///    - Particle emission (dust for impact weight)
    /// 
    /// 5. SUB-PIXEL INTERPOLATION
    ///    - 144Hz+ smoothness via partialTicks
    ///    - Eliminates micro-stutter on high refresh displays
    /// 
    /// 6. TICK-BASED MANAGEMENT
    ///    - Frame-independent timing
    ///    - Consistent speed regardless of framerate
    /// </summary>
    public static class MassiveBeamSystem
    {
        #region Vertex Buffer Management
        
        private static VertexPositionColorTexture[] _beamVertices;
        private static short[] _beamIndices;
        private static BasicEffect _beamEffect;
        
        private const int MaxBeamVertices = 4096;
        private const int MaxBeamIndices = 8192;
        
        private static bool _initialized = false;
        
        /// <summary>
        /// Initialize the beam rendering system. Called automatically on first use.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized || Main.dedServ) return;
            
            _beamVertices = new VertexPositionColorTexture[MaxBeamVertices];
            _beamIndices = new short[MaxBeamIndices];
            
            _beamEffect = new BasicEffect(Main.instance.GraphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false
            };
            
            _initialized = true;
        }
        
        #endregion
        
        #region Width Function Presets (Calamity-Style)
        
        /// <summary>
        /// Delegate for beam width calculation.
        /// </summary>
        /// <param name="completionRatio">0 at beam start, 1 at beam end</param>
        public delegate float BeamWidthFunction(float completionRatio);
        
        /// <summary>
        /// Quadratic bump - thin at both ends, thick in middle.
        /// The signature "energy beam" profile.
        /// </summary>
        public static BeamWidthFunction QuadraticBump(float maxWidth)
        {
            return ratio => maxWidth * MathF.Sin(ratio * MathHelper.Pi);
        }
        
        /// <summary>
        /// Constant width with soft taper at ends.
        /// Good for laser beams.
        /// </summary>
        public static BeamWidthFunction ConstantWithSoftEnds(float width, float taperStart = 0.1f, float taperEnd = 0.9f)
        {
            return ratio =>
            {
                if (ratio < taperStart)
                    return width * (ratio / taperStart);
                if (ratio > taperEnd)
                    return width * (1f - (ratio - taperEnd) / (1f - taperEnd));
                return width;
            };
        }
        
        /// <summary>
        /// Pulsing width that oscillates over time.
        /// For living/breathing beam effects.
        /// </summary>
        public static BeamWidthFunction PulsingWidth(float baseWidth, float pulseAmount, float pulseSpeed)
        {
            return ratio =>
            {
                float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * pulseSpeed + ratio * MathHelper.Pi) * pulseAmount;
                return baseWidth * pulse * MathF.Sin(ratio * MathHelper.Pi);
            };
        }
        
        /// <summary>
        /// Universe Splitter style - thick at source, tapers to thin at end.
        /// </summary>
        public static BeamWidthFunction SourceTaper(float sourceWidth, float endWidth = 0f)
        {
            return ratio =>
            {
                float linear = MathHelper.Lerp(sourceWidth, endWidth, ratio);
                // Smooth step for organic feel
                float smoothRatio = ratio * ratio * (3f - 2f * ratio);
                return MathHelper.Lerp(sourceWidth, endWidth, smoothRatio);
            };
        }
        
        /// <summary>
        /// Devourer of Gods style - oscillating wave pattern along beam.
        /// </summary>
        public static BeamWidthFunction WaveWidth(float baseWidth, float waveAmplitude, float waveFrequency)
        {
            return ratio =>
            {
                float wave = MathF.Sin(ratio * waveFrequency * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly * 8f);
                float taperMultiplier = MathF.Sin(ratio * MathHelper.Pi); // Taper at ends
                return (baseWidth + wave * waveAmplitude) * taperMultiplier;
            };
        }
        
        #endregion
        
        #region Color Function Presets
        
        /// <summary>
        /// Delegate for beam color calculation.
        /// </summary>
        public delegate Color BeamColorFunction(float completionRatio, float uvScrollOffset);
        
        /// <summary>
        /// Solid color with opacity fade at ends.
        /// </summary>
        public static BeamColorFunction SolidBeamColor(Color color, float coreOpacity = 1f)
        {
            return (ratio, _) =>
            {
                float opacity = MathF.Sin(ratio * MathHelper.Pi) * coreOpacity;
                return color * opacity;
            };
        }
        
        /// <summary>
        /// Theme-based gradient beam.
        /// </summary>
        public static BeamColorFunction ThemeBeamColor(string themeName, float opacity = 1f)
        {
            Color[] palette = MagnumThemePalettes.GetThemePalette(themeName) ?? new[] { Color.White };
            
            return (ratio, uvScroll) =>
            {
                // Scroll through palette based on UV offset for flowing effect
                float scrolledRatio = (ratio + uvScroll) % 1f;
                Color color = VFXUtilities.PaletteLerp(palette, scrolledRatio);
                
                // Fade at ends
                float fade = MathF.Sin(ratio * MathHelper.Pi);
                return color * opacity * fade;
            };
        }
        
        /// <summary>
        /// Hot core with cooler edges - plasma/fire effect.
        /// </summary>
        public static BeamColorFunction PlasmaBeamColor(Color coreColor, Color edgeColor)
        {
            return (ratio, uvScroll) =>
            {
                // Core is brightest, edges fade to secondary color
                float coreness = MathF.Sin(ratio * MathHelper.Pi);
                Color lerped = Color.Lerp(edgeColor, coreColor, coreness);
                
                // Add scrolling shimmer
                float shimmer = 1f + MathF.Sin(uvScroll * MathHelper.TwoPi * 3f + ratio * 10f) * 0.15f;
                
                return lerped * coreness * shimmer;
            };
        }
        
        /// <summary>
        /// Rainbow cycling beam (Swan Lake style).
        /// </summary>
        public static BeamColorFunction RainbowBeamColor(float saturation = 1f, float luminosity = 0.7f)
        {
            return (ratio, uvScroll) =>
            {
                float hue = (ratio * 0.5f + uvScroll + Main.GlobalTimeWrappedHourly * 0.3f) % 1f;
                Color rainbow = Main.hslToRgb(hue, saturation, luminosity);
                float fade = MathF.Sin(ratio * MathHelper.Pi);
                return rainbow * fade;
            };
        }
        
        #endregion
        
        #region Core Beam Rendering
        
        /// <summary>
        /// Configuration for a massive beam.
        /// </summary>
        public struct BeamSettings
        {
            public BeamWidthFunction WidthFunc;
            public BeamColorFunction ColorFunc;
            public float TextureScrollSpeed;
            public int SegmentCount;
            public bool UseInterpolation;
            public float BloomMultiplier;
            public float CoreMultiplier;
            public string ThemeName;
            
            public static BeamSettings Default => new BeamSettings
            {
                WidthFunc = QuadraticBump(40f),
                ColorFunc = SolidBeamColor(Color.White, 1f),
                TextureScrollSpeed = 2f,
                SegmentCount = 50,
                UseInterpolation = true,
                BloomMultiplier = 2.5f,
                CoreMultiplier = 0.3f,
                ThemeName = ""
            };
        }
        
        /// <summary>
        /// Renders a massive beam from start to end point.
        /// Uses multi-pass rendering: outer bloom → main beam → bright core.
        /// </summary>
        public static void RenderMassiveBeam(Vector2 start, Vector2 end, BeamSettings settings)
        {
            if (!_initialized) Initialize();
            if (Main.dedServ) return;
            
            // Generate control points along beam
            Vector2[] controlPoints = GenerateBeamControlPoints(start, end, settings.SegmentCount);
            
            // Calculate UV scroll offset based on time
            float uvScroll = Main.GlobalTimeWrappedHourly * settings.TextureScrollSpeed;
            
            // Multi-pass rendering for Calamity-style layering
            
            // Pass 1: Outer bloom (large, dim)
            RenderBeamPass(controlPoints, settings, uvScroll, 
                widthMult: settings.BloomMultiplier, 
                opacityMult: 0.25f,
                isBloomPass: true);
            
            // Pass 2: Middle glow
            RenderBeamPass(controlPoints, settings, uvScroll,
                widthMult: 1.6f,
                opacityMult: 0.5f,
                isBloomPass: true);
            
            // Pass 3: Main beam
            RenderBeamPass(controlPoints, settings, uvScroll,
                widthMult: 1f,
                opacityMult: 0.9f,
                isBloomPass: false);
            
            // Pass 4: Bright core
            RenderBeamPass(controlPoints, settings, uvScroll,
                widthMult: settings.CoreMultiplier,
                opacityMult: 1f,
                isBloomPass: false,
                forceWhite: true);
        }
        
        /// <summary>
        /// Renders a massive beam following a projectile's trail.
        /// Uses interpolation for buttery-smooth 144Hz+ rendering.
        /// </summary>
        public static void RenderProjectileBeam(Projectile projectile, BeamSettings settings)
        {
            if (!_initialized) Initialize();
            if (Main.dedServ || projectile == null) return;
            
            // Build control points from projectile trail with interpolation
            Vector2[] controlPoints = BuildInterpolatedTrailPoints(projectile, settings.SegmentCount);
            
            if (controlPoints == null || controlPoints.Length < 2)
                return;
            
            float uvScroll = Main.GlobalTimeWrappedHourly * settings.TextureScrollSpeed;
            
            // Multi-pass rendering
            RenderBeamPass(controlPoints, settings, uvScroll, 
                widthMult: settings.BloomMultiplier, opacityMult: 0.2f, isBloomPass: true);
            RenderBeamPass(controlPoints, settings, uvScroll,
                widthMult: 1.5f, opacityMult: 0.45f, isBloomPass: true);
            RenderBeamPass(controlPoints, settings, uvScroll,
                widthMult: 1f, opacityMult: 0.85f, isBloomPass: false);
            RenderBeamPass(controlPoints, settings, uvScroll,
                widthMult: settings.CoreMultiplier, opacityMult: 1f, isBloomPass: false, forceWhite: true);
        }
        
        /// <summary>
        /// Renders a single pass of the beam with specified multipliers.
        /// </summary>
        private static void RenderBeamPass(
            Vector2[] controlPoints, 
            BeamSettings settings,
            float uvScroll,
            float widthMult,
            float opacityMult,
            bool isBloomPass,
            bool forceWhite = false)
        {
            if (controlPoints.Length < 2) return;
            
            // End SpriteBatch for raw primitive rendering
            try { Main.spriteBatch.End(); } catch { }
            
            try
            {
                int vertexCount = controlPoints.Length * 2;
                int triangleCount = (controlPoints.Length - 1) * 2;
                
                if (vertexCount > MaxBeamVertices) return;
                
                // Build vertex strip
                for (int i = 0; i < controlPoints.Length; i++)
                {
                    float ratio = (float)i / (controlPoints.Length - 1);
                    
                    // Width with multiplier
                    float width = settings.WidthFunc(ratio) * widthMult;
                    
                    // Color with multiplier
                    Color color;
                    if (forceWhite)
                    {
                        float fade = MathF.Sin(ratio * MathHelper.Pi);
                        color = Color.White * fade * opacityMult;
                    }
                    else
                    {
                        color = settings.ColorFunc(ratio, uvScroll) * opacityMult;
                    }
                    
                    // CRITICAL: Remove alpha for proper additive blending
                    color = color.WithoutAlpha();
                    
                    // Calculate perpendicular direction for width
                    Vector2 direction;
                    if (i == 0)
                        direction = (controlPoints[1] - controlPoints[0]).SafeNormalize(Vector2.UnitY);
                    else if (i == controlPoints.Length - 1)
                        direction = (controlPoints[i] - controlPoints[i - 1]).SafeNormalize(Vector2.UnitY);
                    else
                        direction = (controlPoints[i + 1] - controlPoints[i - 1]).SafeNormalize(Vector2.UnitY);
                    
                    Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                    Vector2 screenPos = controlPoints[i] - Main.screenPosition;
                    
                    // UV coordinates for texture scrolling
                    float u = ratio + uvScroll;
                    
                    _beamVertices[i * 2] = new VertexPositionColorTexture(
                        new Vector3(screenPos + perpendicular * width * 0.5f, 0),
                        color,
                        new Vector2(u, 0));
                    
                    _beamVertices[i * 2 + 1] = new VertexPositionColorTexture(
                        new Vector3(screenPos - perpendicular * width * 0.5f, 0),
                        color,
                        new Vector2(u, 1));
                }
                
                // Build indices
                int idx = 0;
                for (int i = 0; i < controlPoints.Length - 1; i++)
                {
                    int baseVertex = i * 2;
                    _beamIndices[idx++] = (short)baseVertex;
                    _beamIndices[idx++] = (short)(baseVertex + 1);
                    _beamIndices[idx++] = (short)(baseVertex + 2);
                    _beamIndices[idx++] = (short)(baseVertex + 1);
                    _beamIndices[idx++] = (short)(baseVertex + 3);
                    _beamIndices[idx++] = (short)(baseVertex + 2);
                }
                
                // Draw primitives
                DrawBeamPrimitives(vertexCount, triangleCount);
            }
            finally
            {
                // Restart SpriteBatch
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        
        /// <summary>
        /// Draws the beam primitives to the GPU.
        /// </summary>
        private static void DrawBeamPrimitives(int vertexCount, int triangleCount)
        {
            GraphicsDevice device = Main.instance.GraphicsDevice;
            
            var prevBlend = device.BlendState;
            var prevRasterizer = device.RasterizerState;
            var prevDepth = device.DepthStencilState;
            var prevSampler = device.SamplerStates[0];
            
            device.BlendState = BlendState.Additive;
            device.RasterizerState = RasterizerState.CullNone;
            device.DepthStencilState = DepthStencilState.None;
            device.SamplerStates[0] = SamplerState.LinearClamp;
            
            Matrix view = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            
            _beamEffect.View = view;
            _beamEffect.Projection = projection;
            _beamEffect.World = Matrix.Identity;
            
            try
            {
                foreach (EffectPass pass in _beamEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                }
                
                device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _beamVertices, 0, vertexCount,
                    _beamIndices, 0, triangleCount);
            }
            finally
            {
                device.BlendState = prevBlend;
                device.RasterizerState = prevRasterizer;
                device.DepthStencilState = prevDepth;
                device.SamplerStates[0] = prevSampler;
            }
        }
        
        #endregion
        
        #region Control Point Generation
        
        /// <summary>
        /// Generates control points for a straight beam with Catmull-Rom smoothing.
        /// </summary>
        private static Vector2[] GenerateBeamControlPoints(Vector2 start, Vector2 end, int segmentCount)
        {
            Vector2[] points = new Vector2[segmentCount];
            
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                points[i] = Vector2.Lerp(start, end, t);
            }
            
            return points;
        }
        
        /// <summary>
        /// Builds interpolated trail points from a projectile for smooth beam rendering.
        /// Uses sub-pixel interpolation for 144Hz+ smoothness.
        /// </summary>
        private static Vector2[] BuildInterpolatedTrailPoints(Projectile projectile, int segmentCount)
        {
            if (projectile.oldPos == null || projectile.oldPos.Length < 2)
                return null;
            
            List<Vector2> rawPositions = new List<Vector2>();
            
            // Add interpolated current position (for 144Hz+ smoothness)
            Vector2 currentInterpolated = InterpolatedRenderer.GetInterpolatedCenter(projectile);
            rawPositions.Add(currentInterpolated);
            
            // Add valid old positions
            foreach (var oldPos in projectile.oldPos)
            {
                if (oldPos != Vector2.Zero)
                {
                    rawPositions.Add(oldPos + projectile.Size * 0.5f);
                }
            }
            
            if (rawPositions.Count < 2) return null;
            
            // Smooth with Catmull-Rom splines
            return SmoothPositionsWithCatmullRom(rawPositions, segmentCount);
        }
        
        /// <summary>
        /// Applies Catmull-Rom spline smoothing to raw positions.
        /// </summary>
        private static Vector2[] SmoothPositionsWithCatmullRom(List<Vector2> rawPositions, int outputCount)
        {
            Vector2[] result = new Vector2[outputCount];
            
            for (int i = 0; i < outputCount; i++)
            {
                float t = (float)i / (outputCount - 1) * (rawPositions.Count - 1);
                int segment = (int)t;
                float segmentT = t - segment;
                
                int p0 = Math.Max(0, segment - 1);
                int p1 = segment;
                int p2 = Math.Min(rawPositions.Count - 1, segment + 1);
                int p3 = Math.Min(rawPositions.Count - 1, segment + 2);
                
                result[i] = CatmullRom(rawPositions[p0], rawPositions[p1], rawPositions[p2], rawPositions[p3], segmentT);
            }
            
            return result;
        }
        
        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
        
        #endregion
        
        #region Particle Effects
        
        /// <summary>
        /// Spawns impact particles along a beam for visual "weight".
        /// Call this in AI or PostDraw.
        /// </summary>
        public static void SpawnBeamParticles(Vector2 start, Vector2 end, string themeName, float density = 1f)
        {
            Color[] palette = MagnumThemePalettes.GetThemePalette(themeName) ?? new[] { Color.White };
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            float length = (end - start).Length();
            
            int particleCount = (int)(length / 30f * density);
            
            for (int i = 0; i < particleCount; i++)
            {
                if (!Main.rand.NextBool(3)) continue;
                
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(start, end, t);
                Vector2 perpOffset = new Vector2(-direction.Y, direction.X) * Main.rand.NextFloat(-15f, 15f);
                
                Color dustColor = palette[Main.rand.Next(palette.Length)];
                
                // Dense glowing dust
                Dust dust = Dust.NewDustPerfect(
                    pos + perpOffset,
                    DustID.Enchanted_Gold,
                    direction * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustColor, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
                
                // Occasional flares
                if (Main.rand.NextBool(4))
                {
                    CustomParticles.GenericFlare(pos + perpOffset, dustColor, 0.35f, 12);
                }
            }
        }
        
        /// <summary>
        /// Spawns impact particles at beam endpoint.
        /// </summary>
        public static void SpawnBeamImpactParticles(Vector2 impactPoint, string themeName, float intensity = 1f)
        {
            Color[] palette = MagnumThemePalettes.GetThemePalette(themeName) ?? new[] { Color.White };
            
            // Central flash
            CustomParticles.GenericFlare(impactPoint, Color.White, 0.8f * intensity, 18);
            CustomParticles.GenericFlare(impactPoint, palette[0], 0.6f * intensity, 20);
            
            // Cascading halos
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = palette[Math.Min(i, palette.Length - 1)];
                CustomParticles.HaloRing(impactPoint, ringColor, (0.3f + i * 0.12f) * intensity, 12 + i * 3);
            }
            
            // Radial spark burst
            int sparkCount = (int)(12 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = palette[Main.rand.Next(palette.Length)];
                
                Dust spark = Dust.NewDustPerfect(impactPoint, DustID.Enchanted_Gold, sparkVel, 0, sparkColor, 1.3f);
                spark.noGravity = true;
            }
            
            // Music notes for musical theme
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.MusicNote(impactPoint + Main.rand.NextVector2Circular(10f, 10f),
                    Main.rand.NextVector2Circular(2f, 2f), palette[0], 0.75f, 25);
            }
        }
        
        #endregion
        
        #region Convenience Methods
        
        /// <summary>
        /// Creates beam settings for a specific theme.
        /// </summary>
        public static BeamSettings CreateThemedBeamSettings(string themeName, float width = 40f)
        {
            return new BeamSettings
            {
                WidthFunc = QuadraticBump(width),
                ColorFunc = ThemeBeamColor(themeName, 1f),
                TextureScrollSpeed = 2.5f,
                SegmentCount = 50,
                UseInterpolation = true,
                BloomMultiplier = 2.5f,
                CoreMultiplier = 0.3f,
                ThemeName = themeName
            };
        }
        
        /// <summary>
        /// Creates DoG-style wave beam settings.
        /// </summary>
        public static BeamSettings CreateWaveBeamSettings(Color primaryColor, Color secondaryColor, float width = 35f)
        {
            return new BeamSettings
            {
                WidthFunc = WaveWidth(width, width * 0.3f, 3f),
                ColorFunc = PlasmaBeamColor(primaryColor, secondaryColor),
                TextureScrollSpeed = 4f,
                SegmentCount = 60,
                UseInterpolation = true,
                BloomMultiplier = 2.8f,
                CoreMultiplier = 0.25f,
                ThemeName = ""
            };
        }
        
        /// <summary>
        /// Creates Universe Splitter-style source taper beam.
        /// </summary>
        public static BeamSettings CreateSourceTaperBeamSettings(Color color, float sourceWidth = 60f)
        {
            return new BeamSettings
            {
                WidthFunc = SourceTaper(sourceWidth, 5f),
                ColorFunc = SolidBeamColor(color, 1f),
                TextureScrollSpeed = 3f,
                SegmentCount = 50,
                UseInterpolation = true,
                BloomMultiplier = 2.2f,
                CoreMultiplier = 0.35f,
                ThemeName = ""
            };
        }
        
        /// <summary>
        /// Creates pulsing energy beam settings.
        /// </summary>
        public static BeamSettings CreatePulsingBeamSettings(string themeName, float width = 30f)
        {
            return new BeamSettings
            {
                WidthFunc = PulsingWidth(width, 0.25f, 6f),
                ColorFunc = ThemeBeamColor(themeName, 1f),
                TextureScrollSpeed = 1.5f,
                SegmentCount = 45,
                UseInterpolation = true,
                BloomMultiplier = 3f,
                CoreMultiplier = 0.28f,
                ThemeName = themeName
            };
        }
        
        #endregion
    }
}
