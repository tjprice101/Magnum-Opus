using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Metaballs
{
    /// <summary>
    /// MAGNUM OPUS METABALL SHADER SYSTEM
    /// 
    /// Following Calamity's CalamityShaders.cs pattern exactly:
    /// 1. Use [Autoload(Side = ModSide.Client)] to skip on servers
    /// 2. Load shaders in PostSetupContent() using Mod.Assets.Request&lt;Effect&gt;
    /// 3. Store as Asset&lt;Effect&gt; for lazy loading
    /// 4. Register with GameShaders.Misc using standard prefix naming
    /// 
    /// tModLoader 1.4.4+ auto-compiles .fx files to .xnb at build time.
    /// The .fx files must be in the Assets/Shaders/ folder.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class MagnumMetaballShaders : ModSystem
    {
        private const string ShaderPath = "Effects/";
        private const string ShaderPrefix = "MagnumOpus:";
        
        /// <summary>
        /// Standard metaball edge detection shader.
        /// Detects edges where alpha transitions to 0 and applies EdgeColor.
        /// Used by most metaball types for the iconic edge glow effect.
        /// </summary>
        internal static Asset<Effect> MetaballEdgeShader;
        
        /// <summary>
        /// Additive metaball shader for fire/plasma effects.
        /// Averages neighboring pixels and pushes toward white for intense glow.
        /// Used by RancorLava, Dragons Breath, and similar fire metaballs.
        /// </summary>
        internal static Asset<Effect> AdditiveMetaballEdgeShader;
        
        /// <summary>
        /// Whether both metaball shaders loaded successfully.
        /// </summary>
        public static bool ShadersAvailable { get; private set; }
        
        /// <summary>
        /// Helper to safely load a shader using the mod's asset system.
        /// Returns null if the shader doesn't exist or fails to load.
        /// </summary>
        private Asset<Effect> LoadShader(string path)
        {
            try
            {
                // Check if the compiled shader asset exists BEFORE requesting it.
                // Mod.Assets.Request registers a tracked asset; if it's missing,
                // Mod.TransferAllAssets() throws MissingResourceException and
                // fatally disables the entire mod — even if we catch the exception here.
                string fullPath = $"{ShaderPath}{path}";
                if (!Mod.HasAsset(fullPath))
                {
                    Mod.Logger.Info($"MagnumMetaballShaders: {fullPath} not found — skipping.");
                    return null;
                }

                var asset = Mod.Assets.Request<Effect>(fullPath, AssetRequestMode.AsyncLoad);
                
                if (asset != null && asset.State != AssetState.NotLoaded)
                {
                    asset.Wait();
                    if (asset.Value != null)
                        return asset;
                }
            }
            catch
            {
                // Shader not available - this is fine, we'll use fallback
            }
            return null;
        }
        
        /// <summary>
        /// Helper to register a shader with GameShaders.Misc.
        /// </summary>
        private void RegisterMiscShader(Asset<Effect> shader, string passName, string registrationName)
        {
            if (shader?.Value == null)
                return;
                
            var shaderData = new MiscShaderData(shader, passName);
            GameShaders.Misc[$"{ShaderPrefix}{registrationName}"] = shaderData;
        }
        
        public override void Load()
        {
            // Shaders are loaded in PostSetupContent after all content is available
        }
        
        public override void PostSetupContent()
        {
            // Skip on dedicated servers
            if (Main.dedServ)
                return;
            
            bool success = true;
            
            try
            {
                // Load MetaballEdgeShader
                MetaballEdgeShader = LoadShader("MetaballEdgeShader");
                if (MetaballEdgeShader?.Value != null)
                {
                    RegisterMiscShader(MetaballEdgeShader, "ParticlePass", "MetaballEdge");
                    Mod.Logger.Info("MagnumMetaballShaders: Loaded MetaballEdgeShader successfully.");
                }
                else
                {
                    Mod.Logger.Warn("MagnumMetaballShaders: MetaballEdgeShader not found or loaded as null.");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Mod.Logger.Error($"MagnumMetaballShaders: Failed to load MetaballEdgeShader - {ex.Message}");
                success = false;
            }
            
            try
            {
                // Load AdditiveMetaballEdgeShader
                AdditiveMetaballEdgeShader = LoadShader("AdditiveMetaballEdgeShader");
                if (AdditiveMetaballEdgeShader?.Value != null)
                {
                    RegisterMiscShader(AdditiveMetaballEdgeShader, "ParticlePass", "AdditiveMetaballEdge");
                    Mod.Logger.Info("MagnumMetaballShaders: Loaded AdditiveMetaballEdgeShader successfully.");
                }
                else
                {
                    Mod.Logger.Warn("MagnumMetaballShaders: AdditiveMetaballEdgeShader not found or loaded as null.");
                    success = false;
                }
            }
            catch (System.Exception ex)
            {
                Mod.Logger.Error($"MagnumMetaballShaders: Failed to load AdditiveMetaballEdgeShader - {ex.Message}");
                success = false;
            }
            
            ShadersAvailable = success;
            
            if (ShadersAvailable)
            {
                Mod.Logger.Info("MagnumMetaballShaders: All metaball shaders loaded successfully!");
            }
            else
            {
                Mod.Logger.Info("MagnumMetaballShaders: Some shaders failed to load — using particle-based fallback.");
            }
        }
        
        public override void Unload()
        {
            MetaballEdgeShader = null;
            AdditiveMetaballEdgeShader = null;
            ShadersAvailable = false;
        }
    }
}
