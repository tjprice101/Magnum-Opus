using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Self-contained shader loader for Resonance of a Bygone Reality.
    /// Loads 5 shader keys from 4 .fx files:
    ///   ResonanceBulletTrail  (technique: BulletMain)
    ///   ResonanceBulletGlow   (technique: BulletGlow — same .fx file)
    ///   ResonanceBladeTrail
    ///   ResonanceMuzzleFlash
    ///   ResonanceEchoBloom
    /// Zero references to global ShaderLoader.
    /// </summary>
    public class ResonanceShaderLoader : ModSystem
    {
        private static Dictionary<string, Effect> _shaders;
        private static bool _initialized;

        public const string BulletTrailKey = "ResonanceBulletTrail";
        public const string BulletGlowKey = "ResonanceBulletGlow";
        public const string BladeTrailKey = "ResonanceBladeTrail";
        public const string MuzzleFlashKey = "ResonanceMuzzleFlash";
        public const string EchoBloomKey = "ResonanceEchoBloom";

        private const string ShaderBasePath = "MagnumOpus/Effects/Fate/ResonanceOfABygoneReality/";

        public static bool ShadersAvailable => _initialized && _shaders != null && _shaders.Count > 0;

        public override void Load()
        {
            if (Main.dedServ) return;
            _shaders = new Dictionary<string, Effect>();
            _initialized = false;
        }

        public override void Unload()
        {
            _shaders?.Clear();
            _shaders = null;
            _initialized = false;
        }

        private static void Initialize()
        {
            if (_initialized || Main.dedServ) return;
            _initialized = true;

            // ResonanceBulletTrail.fx — two techniques (BulletMain / BulletGlow)
            TryLoadShader(BulletTrailKey, "ResonanceBulletTrail");
            // Store same Effect under second key for BulletGlow technique access
            if (_shaders.ContainsKey(BulletTrailKey))
                _shaders[BulletGlowKey] = _shaders[BulletTrailKey];

            TryLoadShader(BladeTrailKey, "ResonanceBladeTrail");
            TryLoadShader(MuzzleFlashKey, "ResonanceMuzzleFlash");
            TryLoadShader(EchoBloomKey, "ResonanceEchoBloom");
        }

        private static void TryLoadShader(string key, string fileName)
        {
            try
            {
                string path = ShaderBasePath + fileName;
                if (!ModContent.HasAsset(path))
                    return;

                var effect = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
                if (effect != null)
                    _shaders[key] = effect;
            }
            catch (Exception)
            {
                // Silently skip — shader not critical for gameplay
            }
        }

        /// <summary>
        /// Get a loaded shader by key. Returns null if unavailable.
        /// </summary>
        public static Effect GetShader(string key)
        {
            if (!_initialized) Initialize();
            return _shaders != null && _shaders.TryGetValue(key, out var effect) ? effect : null;
        }

        /// <summary>
        /// Set a shader's technique by name. Returns false if shader or technique not found.
        /// </summary>
        public static bool SetTechnique(string key, string techniqueName)
        {
            var shader = GetShader(key);
            if (shader == null) return false;

            foreach (var tech in shader.Techniques)
            {
                if (tech.Name == techniqueName)
                {
                    shader.CurrentTechnique = tech;
                    return true;
                }
            }
            return false;
        }
    }
}
