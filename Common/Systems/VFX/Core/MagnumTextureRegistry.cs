using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Centralized VFX texture registry for MagnumOpus.
    /// Uses EXISTING particle textures from Assets/Particles/ folder.
    /// 
    /// This provides a standardized API for VFX systems to access common textures
    /// without needing to hard-code paths everywhere.
    /// </summary>
    public class MagnumTextureRegistry : ModSystem
    {
        #region Glow Textures
        
        /// <summary>
        /// Soft circular glow for general bloom effects.
        /// </summary>
        public static Asset<Texture2D> SoftGlow { get; private set; }
        
        /// <summary>
        /// Energy flare for impact/explosion effects.
        /// </summary>
        public static Asset<Texture2D> EnergyFlare { get; private set; }
        
        #endregion
        
        #region Halo Textures
        
        /// <summary>
        /// Ring/halo texture for shockwave and expansion effects.
        /// </summary>
        public static Asset<Texture2D> HaloRing { get; private set; }
        
        #endregion
        
        #region Cloud/Smoke Textures
        
        /// <summary>
        /// Cloud/smoke texture for fog effects (uses SoftGlow for cloud-like appearance).
        /// </summary>
        public static Asset<Texture2D> CloudSmoke { get; private set; }
        
        /// <summary>
        /// Procedurally generated heavy smoke texture with proper cloud shapes.
        /// </summary>
        private static Texture2D _heavySmokeTexture;
        
        #endregion
        
        /// <summary>
        /// Whether textures have been loaded.
        /// </summary>
        public static bool TexturesLoaded { get; private set; }
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            // Load distinct textures from Assets/Particles/
            // SoftGlow2 = softest, SoftGlow3 = medium, SoftGlow4 = most defined
            SoftGlow = SafeLoad("MagnumOpus/Assets/Particles/SoftGlow2");
            EnergyFlare = SafeLoad("MagnumOpus/Assets/Particles/EnergyFlare");
            HaloRing = SafeLoad("MagnumOpus/Assets/Particles/GlowingHalo1");
            CloudSmoke = SafeLoad("MagnumOpus/Assets/Particles/SoftGlow3"); // Softer texture for clouds
            
            TexturesLoaded = true;
        }
        
        public override void Unload()
        {
            SoftGlow = null;
            EnergyFlare = null;
            HaloRing = null;
            CloudSmoke = null;
            _heavySmokeTexture?.Dispose();
            _heavySmokeTexture = null;
            TexturesLoaded = false;
        }
        
        /// <summary>
        /// Safely loads a texture, returning null if the asset doesn't exist.
        /// </summary>
        private Asset<Texture2D> SafeLoad(string path)
        {
            try
            {
                return ModContent.Request<Texture2D>(path, AssetRequestMode.AsyncLoad);
            }
            catch
            {
                Mod.Logger.Warn($"Texture not found: {path}");
                return null;
            }
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Gets the bloom texture (SoftGlow).
        /// </summary>
        public static Texture2D GetBloom()
        {
            if (SoftGlow?.IsLoaded == true)
                return SoftGlow.Value;
            return null;
        }
        
        /// <summary>
        /// Gets the soft glow texture (alias for GetBloom).
        /// </summary>
        public static Texture2D GetSoftGlow()
        {
            return GetBloom();
        }
        
        /// <summary>
        /// Gets the energy flare texture.
        /// </summary>
        public static Texture2D GetFlare()
        {
            if (EnergyFlare?.IsLoaded == true)
                return EnergyFlare.Value;
            return GetBloom(); // Fallback to soft glow
        }
        
        /// <summary>
        /// Gets the energy flare texture (alias).
        /// </summary>
        public static Texture2D GetEnergyFlare()
        {
            return GetFlare();
        }
        
        /// <summary>
        /// Gets the 4-point shine flare texture (using EnergyFlare as substitute).
        /// </summary>
        public static Texture2D GetShineFlare4Point()
        {
            return GetFlare();
        }
        
        /// <summary>
        /// Gets the halo ring texture.
        /// </summary>
        public static Texture2D GetHaloRing()
        {
            if (HaloRing?.IsLoaded == true)
                return HaloRing.Value;
            return GetBloom(); // Fallback to soft glow
        }
        
        /// <summary>
        /// Single white pixel texture for primitive drawing (lines, rectangles, etc.).
        /// </summary>
        private static Texture2D _pixelTexture;
        
        /// <summary>
        /// Gets a 1x1 white pixel texture for line/primitive drawing.
        /// Used by TelegraphSystem and DynamicSkyboxSystem.
        /// </summary>
        public static Texture2D GetPixelTexture()
        {
            if (_pixelTexture == null || _pixelTexture.IsDisposed)
            {
                _pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            }
            return _pixelTexture;
        }
        
        /// <summary>
        /// Gets the cloud/smoke texture for fog effects.
        /// Uses a softer texture than the energy flare for proper cloud appearance.
        /// </summary>
        public static Texture2D GetCloudSmoke()
        {
            if (CloudSmoke?.IsLoaded == true)
                return CloudSmoke.Value;
            return GetBloom(); // Fallback to soft glow
        }
        
        /// <summary>
        /// Gets the procedurally generated heavy smoke texture.
        /// This creates a proper cloud-like appearance with Perlin noise.
        /// </summary>
        public static Texture2D GetHeavySmoke()
        {
            if (_heavySmokeTexture == null || _heavySmokeTexture.IsDisposed)
            {
                // Get from particle texture generator
                _heavySmokeTexture = ParticleTextureGenerator.HeavySmoke;
            }
            return _heavySmokeTexture ?? GetCloudSmoke();
        }
        
        #endregion
    }
}
