using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Centralized registry for all VFX textures used throughout MagnumOpus.
    /// Inspired by FargosSoulsDLC's MiscTexturesRegistry and NoiseTexturesRegistry.
    /// 
    /// Provides easy access to bloom circles, shine flares, noise textures, and other
    /// commonly used VFX assets. All textures are loaded once and cached for performance.
    /// </summary>
    public class MagnumTextureRegistry : ModSystem
    {
        #region Bloom Textures
        
        /// <summary>
        /// Small circular bloom texture. The workhorse of all glow effects.
        /// Use with { A = 0 } for proper additive blending.
        /// </summary>
        public static Asset<Texture2D> BloomCircleSmall { get; private set; }
        
        /// <summary>
        /// Medium circular bloom texture for larger glow effects.
        /// </summary>
        public static Asset<Texture2D> BloomCircleMedium { get; private set; }
        
        /// <summary>
        /// Large, soft circular bloom for background glows and auras.
        /// </summary>
        public static Asset<Texture2D> BloomCircleLarge { get; private set; }
        
        /// <summary>
        /// Soft feathered glow with smooth falloff, ideal for ambient effects.
        /// </summary>
        public static Asset<Texture2D> SoftGlow { get; private set; }
        
        #endregion
        
        #region Flare Textures
        
        /// <summary>
        /// 4-pointed star/cross flare for impact highlights and charge-up gleams.
        /// </summary>
        public static Asset<Texture2D> ShineFlare4Point { get; private set; }
        
        /// <summary>
        /// 6-pointed star flare for more dramatic effects.
        /// </summary>
        public static Asset<Texture2D> ShineFlare6Point { get; private set; }
        
        /// <summary>
        /// Directional streak flare for motion-based effects.
        /// </summary>
        public static Asset<Texture2D> StreakFlare { get; private set; }
        
        /// <summary>
        /// Lens flare texture for bright light sources.
        /// </summary>
        public static Asset<Texture2D> LensFlare { get; private set; }
        
        #endregion
        
        #region Line and Trail Textures
        
        /// <summary>
        /// Horizontal line texture for beam/laser effects. Origin at left-center.
        /// </summary>
        public static Asset<Texture2D> BloomLine { get; private set; }
        
        /// <summary>
        /// Tapered line that fades toward one end for trail particles.
        /// </summary>
        public static Asset<Texture2D> TaperedLine { get; private set; }
        
        /// <summary>
        /// Generic white pixel for shader targets and trail rendering.
        /// </summary>
        public static Asset<Texture2D> Pixel { get; private set; }
        
        /// <summary>
        /// Transparent pixel for invisible projectile bases (shader-only rendering).
        /// </summary>
        public static Asset<Texture2D> InvisiblePixel { get; private set; }
        
        #endregion
        
        #region Noise Textures
        
        /// <summary>
        /// Smooth perlin noise for organic effects, fire distortion.
        /// </summary>
        public static Asset<Texture2D> PerlinNoise { get; private set; }
        
        /// <summary>
        /// Wavy blotch noise for flame jets and plasma effects.
        /// </summary>
        public static Asset<Texture2D> WavyBlotchNoise { get; private set; }
        
        /// <summary>
        /// Dendritic (branching) noise for lightning, cracks, and organic patterns.
        /// </summary>
        public static Asset<Texture2D> DendriticNoise { get; private set; }
        
        /// <summary>
        /// Cellular/voronoi noise for crystalline and magical effects.
        /// </summary>
        public static Asset<Texture2D> CellularNoise { get; private set; }
        
        /// <summary>
        /// Turbulent noise for smoke and cloud effects.
        /// </summary>
        public static Asset<Texture2D> TurbulenceNoise { get; private set; }
        
        #endregion
        
        #region Gradient Textures
        
        /// <summary>
        /// Horizontal linear gradient from white to black.
        /// </summary>
        public static Asset<Texture2D> LinearGradient { get; private set; }
        
        /// <summary>
        /// Radial gradient from white center to black edge.
        /// </summary>
        public static Asset<Texture2D> RadialGradient { get; private set; }
        
        #endregion
        
        #region Special Effect Textures
        
        /// <summary>
        /// Ring/halo texture for shockwave and expansion effects.
        /// </summary>
        public static Asset<Texture2D> HaloRing { get; private set; }
        
        /// <summary>
        /// Smoke puff texture for HeavySmokeParticle.
        /// </summary>
        public static Asset<Texture2D> SmokePuff { get; private set; }
        
        /// <summary>
        /// Spark texture for GlowySquareParticle-style effects.
        /// </summary>
        public static Asset<Texture2D> GlowySpark { get; private set; }
        
        #endregion
        
        /// <summary>
        /// Whether all textures have been successfully loaded.
        /// </summary>
        public static bool TexturesLoaded { get; private set; }
        
        // Fallback textures cache
        private static Texture2D _fallbackWhitePixel;
        private static Texture2D _fallbackBloom;
        
        public override void Load()
        {
            if (Main.dedServ)
                return;
                
            try
            {
                // Create fallback textures first
                CreateFallbackTextures();
                
                // Load bloom textures
                BloomCircleSmall = SafeLoad("MagnumOpus/Assets/VFX/BloomCircleSmall");
                BloomCircleMedium = SafeLoad("MagnumOpus/Assets/VFX/BloomCircleMedium");
                BloomCircleLarge = SafeLoad("MagnumOpus/Assets/VFX/BloomCircleLarge");
                SoftGlow = SafeLoad("MagnumOpus/Assets/Particles/SoftGlow");
                
                // Load flare textures
                ShineFlare4Point = SafeLoad("MagnumOpus/Assets/VFX/ShineFlare4Point");
                ShineFlare6Point = SafeLoad("MagnumOpus/Assets/VFX/ShineFlare6Point");
                StreakFlare = SafeLoad("MagnumOpus/Assets/VFX/StreakFlare");
                LensFlare = SafeLoad("MagnumOpus/Assets/VFX/LensFlare");
                
                // Load line/trail textures
                BloomLine = SafeLoad("MagnumOpus/Assets/VFX/BloomLine");
                TaperedLine = SafeLoad("MagnumOpus/Assets/VFX/TaperedLine");
                Pixel = SafeLoad("MagnumOpus/Assets/VFX/Pixel");
                InvisiblePixel = SafeLoad("MagnumOpus/Assets/VFX/InvisiblePixel");
                
                // Load noise textures
                PerlinNoise = SafeLoad("MagnumOpus/Assets/VFX/Noise/PerlinNoise");
                WavyBlotchNoise = SafeLoad("MagnumOpus/Assets/VFX/Noise/WavyBlotchNoise");
                DendriticNoise = SafeLoad("MagnumOpus/Assets/VFX/Noise/DendriticNoise");
                CellularNoise = SafeLoad("MagnumOpus/Assets/VFX/Noise/CellularNoise");
                TurbulenceNoise = SafeLoad("MagnumOpus/Assets/VFX/Noise/TurbulenceNoise");
                
                // Load gradient textures
                LinearGradient = SafeLoad("MagnumOpus/Assets/VFX/LinearGradient");
                RadialGradient = SafeLoad("MagnumOpus/Assets/VFX/RadialGradient");
                
                // Load special effect textures
                HaloRing = SafeLoad("MagnumOpus/Assets/Particles/GlowingHalo1");
                SmokePuff = SafeLoad("MagnumOpus/Assets/VFX/SmokePuff");
                GlowySpark = SafeLoad("MagnumOpus/Assets/VFX/GlowySpark");
                
                TexturesLoaded = true;
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"MagnumTextureRegistry failed to load: {ex.Message}");
                TexturesLoaded = false;
            }
        }
        
        public override void Unload()
        {
            BloomCircleSmall = null;
            BloomCircleMedium = null;
            BloomCircleLarge = null;
            SoftGlow = null;
            ShineFlare4Point = null;
            ShineFlare6Point = null;
            StreakFlare = null;
            LensFlare = null;
            BloomLine = null;
            TaperedLine = null;
            Pixel = null;
            InvisiblePixel = null;
            PerlinNoise = null;
            WavyBlotchNoise = null;
            DendriticNoise = null;
            CellularNoise = null;
            TurbulenceNoise = null;
            LinearGradient = null;
            RadialGradient = null;
            HaloRing = null;
            SmokePuff = null;
            GlowySpark = null;
            
            _fallbackWhitePixel?.Dispose();
            _fallbackBloom?.Dispose();
            _fallbackWhitePixel = null;
            _fallbackBloom = null;
            
            TexturesLoaded = false;
        }
        
        /// <summary>
        /// Creates procedural fallback textures for when assets aren't available.
        /// </summary>
        private void CreateFallbackTextures()
        {
            var device = Main.instance.GraphicsDevice;
            
            // 1x1 white pixel
            _fallbackWhitePixel = new Texture2D(device, 1, 1);
            _fallbackWhitePixel.SetData(new[] { Color.White });
            
            // Simple radial bloom gradient (64x64)
            int size = 64;
            _fallbackBloom = new Texture2D(device, size, size);
            Color[] bloomData = new Color[size * size];
            Vector2 center = new Vector2(size / 2f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                    float alpha = Math.Max(0, 1f - dist);
                    alpha = alpha * alpha; // Squared falloff for softer edge
                    bloomData[y * size + x] = Color.White * alpha;
                }
            }
            _fallbackBloom.SetData(bloomData);
        }
        
        /// <summary>
        /// Safely loads a texture, returning a fallback if the asset doesn't exist.
        /// </summary>
        private Asset<Texture2D> SafeLoad(string path)
        {
            try
            {
                return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
            }
            catch
            {
                // Return the existing SoftGlow as fallback if available
                if (SoftGlow != null && SoftGlow.IsLoaded)
                    return SoftGlow;
                    
                // Create a wrapper for the fallback
                Mod.Logger.Warn($"Texture not found: {path}, using fallback");
                return null;
            }
        }
        
        #region Helper Methods
        
        /// <summary>
        /// Gets the bloom texture, with fallback to generated texture if asset not loaded.
        /// </summary>
        public static Texture2D GetBloom()
        {
            if (BloomCircleSmall?.IsLoaded == true)
                return BloomCircleSmall.Value;
            if (SoftGlow?.IsLoaded == true)
                return SoftGlow.Value;
            return _fallbackBloom ?? CreateEmergencyBloom();
        }
        
        /// <summary>
        /// Gets a pixel texture, with fallback to generated texture.
        /// </summary>
        public static Texture2D GetPixel()
        {
            if (Pixel?.IsLoaded == true)
                return Pixel.Value;
            return _fallbackWhitePixel ?? CreateEmergencyPixel();
        }
        
        /// <summary>
        /// Gets the 4-point shine flare texture, with fallback to bloom texture.
        /// </summary>
        public static Texture2D GetShineFlare4Point()
        {
            if (ShineFlare4Point?.IsLoaded == true)
                return ShineFlare4Point.Value;
            // Fallback to bloom if shine flare not available
            return GetBloom();
        }
        
        /// <summary>
        /// Emergency fallback bloom creation if all else fails.
        /// </summary>
        private static Texture2D CreateEmergencyBloom()
        {
            if (Main.dedServ)
                return null;
                
            var device = Main.instance.GraphicsDevice;
            int size = 32;
            var tex = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Vector2 center = new Vector2(size / 2f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / (size / 2f);
                    float alpha = Math.Max(0, 1f - dist);
                    data[y * size + x] = Color.White * (alpha * alpha);
                }
            }
            tex.SetData(data);
            return tex;
        }
        
        /// <summary>
        /// Emergency fallback pixel creation if all else fails.
        /// </summary>
        private static Texture2D CreateEmergencyPixel()
        {
            if (Main.dedServ)
                return null;
                
            var device = Main.instance.GraphicsDevice;
            var tex = new Texture2D(device, 1, 1);
            tex.SetData(new[] { Color.White });
            return tex;
        }
        
        #endregion
    }
}
