using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Shaders
{
    /// <summary>
    /// Centralized shader loading and management system.
    /// Loads custom HLSL shaders from Assets/Shaders/ and provides easy access.
    /// 
    /// Usage:
    ///   Effect shader = ShaderLoader.GetShader("TrailShader");
    ///   shader.Parameters["uColor"]?.SetValue(color.ToVector3());
    /// </summary>
    public class ShaderLoader : ModSystem
    {
        private static Dictionary<string, Effect> _shaders;
        private static bool _initialized;

        // Shader names (without extension)
        public const string TrailShader = "TrailShader";
        public const string BloomShader = "BloomShader";
        public const string ScreenEffects = "ScreenEffectsShader";

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _shaders = new Dictionary<string, Effect>();
            _initialized = false;
        }

        public override void Unload()
        {
            if (_shaders != null)
            {
                foreach (var shader in _shaders.Values)
                {
                    shader?.Dispose();
                }
                _shaders.Clear();
            }
            _shaders = null;
            _initialized = false;
        }

        /// <summary>
        /// Initializes and loads all shaders. Called lazily on first use.
        /// </summary>
        private static void Initialize()
        {
            if (_initialized || Main.dedServ)
                return;

            try
            {
                LoadShader(TrailShader);
                LoadShader(BloomShader);
                LoadShader(ScreenEffects);
                
                _initialized = true;
                ModContent.GetInstance<MagnumOpus>()?.Logger.Info("ShaderLoader: Successfully loaded all shaders.");
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<MagnumOpus>()?.Logger.Error($"ShaderLoader: Failed to load shaders - {ex.Message}");
                _initialized = true; // Prevent retry spam
            }
        }

        /// <summary>
        /// Loads a single shader by name.
        /// </summary>
        private static void LoadShader(string shaderName)
        {
            try
            {
                string path = $"MagnumOpus/Assets/Shaders/{shaderName}";
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
        /// Gets a loaded shader by name. Returns null if not found.
        /// </summary>
        /// <param name="shaderName">Name of the shader (e.g., "TrailShader")</param>
        /// <returns>The Effect object or null</returns>
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

        /// <summary>
        /// Gets the Trail shader if available.
        /// </summary>
        public static Effect Trail => GetShader(TrailShader);

        /// <summary>
        /// Gets the Bloom shader if available.
        /// </summary>
        public static Effect Bloom => GetShader(BloomShader);

        /// <summary>
        /// Gets the Screen Effects shader if available.
        /// </summary>
        public static Effect Screen => GetShader(ScreenEffects);
    }
}
