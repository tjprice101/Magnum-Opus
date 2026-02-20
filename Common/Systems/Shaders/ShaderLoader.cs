using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Shaders
{
    /// <summary>
    /// Centralized shader and VFX texture loading system.
    /// Loads auto-compiled shaders from Effects/ and noise/trail
    /// textures from Assets/VFX/ for use as secondary samplers (uImage1).
    /// 
    /// tModLoader auto-compiles .fx files placed in the Effects/ folder
    /// into FNA-compatible effect bytecode at build time. Do NOT place
    /// pre-compiled .fxc files here — they use DirectX bytecode that is
    /// incompatible with FNA's MojoShader runtime.
    /// 
    /// Usage:
    ///   Effect shader = ShaderLoader.GetShader("SimpleTrailShader");
    ///   shader.Parameters["uColor"]?.SetValue(color.ToVector3());
    ///   
    ///   // Bind noise texture to sampler slot 1 (uImage1)
    ///   Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
    ///   device.Textures[1] = noise;
    ///   device.SamplerStates[1] = SamplerState.LinearWrap;
    ///   
    ///   // Or get the default texture for a trail style
    ///   Texture2D tex = ShaderLoader.GetDefaultTrailTexture(TrailStyle.Flame);
    /// </summary>
    public class ShaderLoader : ModSystem
    {
        private static Dictionary<string, Effect> _shaders;
        private static Dictionary<string, Texture2D> _noiseTextures;
        private static Dictionary<string, Texture2D> _trailTextures;
        private static bool _initialized;
        private static bool _shadersEnabled;

        // Shader names (without extension) - must match .fx filenames in Effects/
        public const string TrailShader = "SimpleTrailShader";
        public const string BloomShader = "SimpleBloomShader";
        public const string ScrollingTrailShader = "ScrollingTrailShader";
        public const string CelestialValorTrailShader = "CelestialValorTrail";
        public const string MotionBlurBloomShader = "MotionBlurBloom";
        public const string TerraBladeSwingVFXShader = "TerraBladeSwingVFX";

        // Noise texture names (without extension) - in Assets/VFX/Noise/
        private static readonly string[] NoiseTextureNames = new[]
        {
            "PerlinNoise",
            "VoronoiNoise",
            "SimplexNoise",
            "TileableFBMNoise",
            "TileableMarbleNoise",
            "CosmicNebulaClouds",
            "CosmicEnergyVortex",
            "DestinyThreadPattern",
            "HorizontalBlackCoreCenterEnergyGradient",
            "HorizontalEnergyGradient",
            "MusicalWavePattern",
            "NebulaWispNoise",
            "NoiseSmoke",
            "RealityCrackPattern",
            "SoftCircularCaustics",
            "SparklyNoiseTexture",
            "StarFieldScatter",
            "UniversalRadialFlowNoise"
        };

        // Trail texture names (without extension) - in Assets/VFX/Trails/
        private static readonly string[] TrailTextureNames = new[]
        {
            "Comet Trail Gradient Fade",
            "Dissolving Particle Trail",
            "Ember Particle Scatter",
            "Full Rotation Spiral Trail",
            "Sparkle Particle Field"
        };

        /// <summary>
        /// True if shaders loaded successfully and are available for use.
        /// </summary>
        public static bool ShadersEnabled => _shadersEnabled;

        /// <summary>
        /// Number of noise textures successfully loaded.
        /// </summary>
        public static int LoadedNoiseTextureCount => _noiseTextures?.Count ?? 0;

        /// <summary>
        /// Number of trail textures successfully loaded.
        /// </summary>
        public static int LoadedTrailTextureCount => _trailTextures?.Count ?? 0;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _shaders = new Dictionary<string, Effect>();
            _noiseTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _trailTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _initialized = false;
            _shadersEnabled = false;
        }

        public override void Unload()
        {
            if (_shaders != null)
            {
                // Cache references for main thread disposal
                var shadersCopy = new List<Effect>(_shaders.Values);
                _shaders.Clear();
                _shaders = null;

                // Queue shader disposal on main thread to avoid ThreadStateException
                Main.QueueMainThreadAction(() =>
                {
                    try
                    {
                        foreach (var shader in shadersCopy)
                        {
                            shader?.Dispose();
                        }
                    }
                    catch { }
                });
            }

            // Textures are managed by tModLoader's asset system; just clear references
            _noiseTextures?.Clear();
            _noiseTextures = null;
            _trailTextures?.Clear();
            _trailTextures = null;

            _initialized = false;
            _shadersEnabled = false;
        }

        /// <summary>
        /// Initializes and loads all shaders and textures.
        /// Called lazily on first use.
        /// </summary>
        private static void Initialize()
        {
            if (_initialized || Main.dedServ)
                return;

            _initialized = true;
            _shadersEnabled = false;

            var logger = ModContent.GetInstance<MagnumOpus>()?.Logger;

            // --- Load Shaders ---
            logger?.Info("ShaderLoader: Loading pre-compiled shaders from Effects/ ...");

            try
            {
                LoadShader(TrailShader);
                LoadShader(BloomShader);
                LoadShader(ScrollingTrailShader);
                LoadShader(CelestialValorTrailShader);
                LoadShader(MotionBlurBloomShader);
                LoadShader(TerraBladeSwingVFXShader);

                _shadersEnabled = _shaders.Count > 0;

                if (_shadersEnabled)
                    logger?.Info($"ShaderLoader: {_shaders.Count} shader(s) loaded. VFX shaders ENABLED.");
                else
                    logger?.Warn("ShaderLoader: No shaders loaded. Using particle-based VFX fallback.");
            }
            catch (Exception ex)
            {
                logger?.Warn($"ShaderLoader: Shader init failed - {ex.Message}. Falling back to particles.");
                _shadersEnabled = false;
            }

            // --- Load VFX Textures ---
            logger?.Info("ShaderLoader: Loading VFX textures from Assets/VFX/ ...");

            int noiseLoaded = 0;
            foreach (string name in NoiseTextureNames)
            {
                if (LoadTexture($"MagnumOpus/Assets/VFX/Noise/{name}", name, _noiseTextures))
                    noiseLoaded++;
            }

            int trailLoaded = 0;
            foreach (string name in TrailTextureNames)
            {
                // Spaces in filenames work fine with ModContent.Request
                if (LoadTexture($"MagnumOpus/Assets/VFX/Trails/{name}", name, _trailTextures))
                    trailLoaded++;
            }

            logger?.Info($"ShaderLoader: Loaded {noiseLoaded} noise texture(s), {trailLoaded} trail texture(s).");
        }

        /// <summary>
        /// Loads a single shader by name from the Effects/ folder.
        /// </summary>
        private static void LoadShader(string shaderName)
        {
            try
            {
                string path = $"MagnumOpus/Effects/{shaderName}";

                // Check existence BEFORE requesting to avoid tModLoader's
                // internal AssetRepository error dialog on missing assets.
                if (!ModContent.HasAsset(path))
                {
                    ModContent.GetInstance<MagnumOpus>()?.Logger.Warn(
                        $"ShaderLoader: Shader '{shaderName}' not found at '{path}' — skipping.");
                    return;
                }

                var effect = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;

                if (effect != null)
                {
                    _shaders[shaderName] = effect;
                }
                else
                {
                    ModContent.GetInstance<MagnumOpus>()?.Logger.Warn($"ShaderLoader: Shader '{shaderName}' loaded as null.");
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<MagnumOpus>()?.Logger.Warn($"ShaderLoader: Could not load shader '{shaderName}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a single texture by asset path and stores it in the given dictionary.
        /// Returns true on success.
        /// </summary>
        private static bool LoadTexture(string assetPath, string key, Dictionary<string, Texture2D> target)
        {
            try
            {
                var tex = ModContent.Request<Texture2D>(assetPath, AssetRequestMode.ImmediateLoad).Value;
                if (tex != null)
                {
                    target[key] = tex;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<MagnumOpus>()?.Logger.Warn($"ShaderLoader: Could not load texture '{key}' - {ex.Message}");
            }
            return false;
        }

        // =====================================================================
        //  Shader Accessors
        // =====================================================================

        /// <summary>
        /// Gets a loaded shader by name. Returns null if not found.
        /// </summary>
        public static Effect GetShader(string shaderName)
        {
            if (Main.dedServ)
                return null;

            if (!_initialized)
                Initialize();

            if (_shaders != null && _shaders.TryGetValue(shaderName, out Effect shader))
                return shader;

            return null;
        }

        /// <summary>
        /// Checks if a shader is available.
        /// </summary>
        public static bool HasShader(string shaderName)
        {
            if (!_initialized)
                Initialize();

            return _shaders != null && _shaders.ContainsKey(shaderName);
        }

        /// <summary>Gets the Trail shader if available.</summary>
        public static Effect Trail => GetShader(TrailShader);

        /// <summary>Gets the Bloom shader if available.</summary>
        public static Effect Bloom => GetShader(BloomShader);

        /// <summary>Gets the Scrolling Trail shader if available.</summary>
        public static Effect ScrollingTrail => GetShader(ScrollingTrailShader);

        /// <summary>Gets the Celestial Valor trail shader if available.</summary>
        public static Effect CelestialValorTrail => GetShader(CelestialValorTrailShader);

        /// <summary>Gets the Motion Blur Bloom shader if available.</summary>
        public static Effect MotionBlurBloom => GetShader(MotionBlurBloomShader);

        /// <summary>Gets the Terra Blade Swing VFX shader if available.</summary>
        public static Effect TerraBladeSwingVFX => GetShader(TerraBladeSwingVFXShader);

        // =====================================================================
        //  Texture Accessors
        // =====================================================================

        /// <summary>
        /// Gets a noise texture by name (case-insensitive). Returns null if not found.
        /// Names match filenames without extension in Assets/VFX/Noise/.
        /// </summary>
        public static Texture2D GetNoiseTexture(string name)
        {
            if (Main.dedServ)
                return null;

            if (!_initialized)
                Initialize();

            if (_noiseTextures != null && _noiseTextures.TryGetValue(name, out Texture2D tex))
                return tex;

            return null;
        }

        /// <summary>
        /// Gets a trail texture by name (case-insensitive). Returns null if not found.
        /// Names match filenames without extension in Assets/VFX/Trails/.
        /// </summary>
        public static Texture2D GetTrailTexture(string name)
        {
            if (Main.dedServ)
                return null;

            if (!_initialized)
                Initialize();

            if (_trailTextures != null && _trailTextures.TryGetValue(name, out Texture2D tex))
                return tex;

            return null;
        }

        // =====================================================================
        //  Style → Default Texture Mapping
        // =====================================================================

        /// <summary>
        /// Returns the recommended default noise texture for a given SimpleTrailShader
        /// TrailStyle. Bind the result to device.Textures[1] before drawing.
        /// Returns null if the texture is not loaded (shader will use fallback float4(0.5)).
        /// </summary>
        /// <param name="style">Trail style enum value (Flame, Ice, Lightning, Nature, Cosmic).</param>
        public static Texture2D GetDefaultTrailStyleTexture(int style)
        {
            // Matches CalamityStyleTrailRenderer.TrailStyle enum order:
            // 0=Flame, 1=Ice, 2=Lightning, 3=Nature, 4=Cosmic
            string name = style switch
            {
                0 => "PerlinNoise",                // Flame — organic swirls
                1 => "SoftCircularCaustics",       // Ice — smooth caustic patterns
                2 => "SparklyNoiseTexture",        // Lightning — sharp sparkle patterns
                3 => "TileableFBMNoise",           // Nature — layered natural noise
                4 => "CosmicNebulaClouds",         // Cosmic — nebula cloud patterns
                _ => "PerlinNoise"                 // Fallback
            };
            return GetNoiseTexture(name);
        }

        /// <summary>
        /// Returns the recommended default noise texture for a given ScrollingTrailShader
        /// ScrollStyle. Bind the result to device.Textures[1] before drawing.
        /// Returns null if the texture is not loaded (shader will use fallback float4(0.5)).
        /// </summary>
        /// <param name="scrollStyle">Scroll style enum value (Flame, Cosmic, Energy, Void, Holy).</param>
        public static Texture2D GetDefaultScrollStyleTexture(int scrollStyle)
        {
            // Matches CalamityStyleTrailRenderer.ScrollStyle enum order:
            // 0=Flame, 1=Cosmic, 2=Energy, 3=Void, 4=Holy
            string name = scrollStyle switch
            {
                0 => "NoiseSmoke",                 // Flame — wispy smoke noise
                1 => "CosmicEnergyVortex",         // Cosmic — swirling vortex energy
                2 => "HorizontalEnergyGradient",   // Energy — horizontal flow gradient
                3 => "NebulaWispNoise",            // Void — dark nebula wisps
                4 => "UniversalRadialFlowNoise",   // Holy — radial emanation
                _ => "PerlinNoise"                 // Fallback
            };
            return GetNoiseTexture(name);
        }

        /// <summary>
        /// Checks whether any noise/trail textures are loaded for secondary sampler use.
        /// </summary>
        public static bool HasVFXTextures =>
            (_noiseTextures != null && _noiseTextures.Count > 0) ||
            (_trailTextures != null && _trailTextures.Count > 0);
    }
}
