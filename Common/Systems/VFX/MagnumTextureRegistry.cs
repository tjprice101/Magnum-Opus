using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

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
        
        /// <summary>
        /// Whether textures have been loaded.
        /// </summary>
        public static bool TexturesLoaded { get; private set; }
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            // Load only textures that ACTUALLY EXIST in Assets/Particles/
            SoftGlow = SafeLoad("MagnumOpus/Assets/Particles/SoftGlow");
            EnergyFlare = SafeLoad("MagnumOpus/Assets/Particles/EnergyFlare");
            HaloRing = SafeLoad("MagnumOpus/Assets/Particles/GlowingHalo1");
            
            TexturesLoaded = true;
        }
        
        public override void Unload()
        {
            SoftGlow = null;
            EnergyFlare = null;
            HaloRing = null;
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
        /// Gets the energy flare texture.
        /// </summary>
        public static Texture2D GetFlare()
        {
            if (EnergyFlare?.IsLoaded == true)
                return EnergyFlare.Value;
            return GetBloom(); // Fallback to soft glow
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
        
        #endregion
    }
}
