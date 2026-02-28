using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Shaders
{
    /// <summary>
    /// Self-contained shader loader for The Final Fermata.
    /// Loads and caches 5 shader effects from Effects/Fate/TheFinalFermata/.
    /// Keys: FermataSwordTrail, FermataSwordGlow, FermataOrbitRing, FermataTemporalWave, FermataSyncSlash.
    ///
    /// FermataSwordTrail and FermataSwordGlow share the same .fx file (FermataSwordTrail.fx)
    /// but use different techniques.
    /// </summary>
    public class FermataShaderLoader : ModSystem
    {
        private static readonly Dictionary<string, Effect> _shaders = new();
        private static bool _loaded;

        // === SHADER KEYS ===
        public const string SwordTrailKey = "FermataSwordTrail";
        public const string SwordGlowKey = "FermataSwordGlow";
        public const string OrbitRingKey = "FermataOrbitRing";
        public const string TemporalWaveKey = "FermataTemporalWave";
        public const string SyncSlashKey = "FermataSyncSlash";

        private static readonly string BasePath = "Effects/Fate/TheFinalFermata/";

        public override void PostSetupContent()
        {
            LoadShaders();
        }

        private static void LoadShaders()
        {
            if (_loaded) return;

            var mod = ModContent.GetInstance<MagnumOpus>();
            if (mod == null) return;

            TryLoad(mod, SwordTrailKey, BasePath + "FermataSwordTrail");
            // SwordGlow shares the same file — we store it separately so the caller
            // can switch techniques by key if needed.
            TryLoad(mod, SwordGlowKey, BasePath + "FermataSwordTrail");
            TryLoad(mod, OrbitRingKey, BasePath + "FermataOrbitRing");
            TryLoad(mod, TemporalWaveKey, BasePath + "FermataTemporalWave");
            TryLoad(mod, SyncSlashKey, BasePath + "FermataSyncSlash");

            _loaded = true;
        }

        private static void TryLoad(Mod mod, string key, string path)
        {
            try
            {
                Effect fx = mod.Assets.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;
                if (fx != null)
                    _shaders[key] = fx;
            }
            catch
            {
                // Shader missing or failed to compile — silently skip.
                // Rendering code should null-check before using.
            }
        }

        /// <summary>
        /// Get a loaded shader by key. Returns null if unavailable.
        /// </summary>
        public static Effect Get(string key)
        {
            if (!_loaded)
                LoadShaders();

            return _shaders.TryGetValue(key, out var fx) ? fx : null;
        }

        /// <summary>
        /// Check if a shader is available.
        /// </summary>
        public static bool Has(string key)
        {
            return Get(key) != null;
        }

        public override void Unload()
        {
            _shaders.Clear();
            _loaded = false;
        }
    }
}
