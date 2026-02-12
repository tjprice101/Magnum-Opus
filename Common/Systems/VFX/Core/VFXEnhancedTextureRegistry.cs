using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ENHANCED VFX TEXTURE REGISTRY
    /// ==============================
    /// 
    /// Extended texture registry for all new VFX+ enhancement textures.
    /// These textures are designed for additive blending (white on black).
    /// 
    /// Categories:
    /// - Impact: Hit effects, explosions, shockwaves
    /// - Beam: Beam core, muzzle, impact, glow
    /// - Screen: Chromatic aberration, distortion, speed lines
    /// - Smear: Weapon swing trails, arc slashes
    /// - Trail: Comet trails, particle effects, ember scatter
    /// - LightRay: God rays, directional shafts, light cones
    /// - Bloom: Bokeh, anamorphic streaks, concentric glow
    /// - Afterimage: Motion blur streaks
    /// - Lightning: Arc patterns
    /// 
    /// All textures are white/grayscale on black for tinting via shader or color multiplication.
    /// </summary>
    public class VFXEnhancedTextureRegistry : ModSystem
    {
        #region Singleton Access
        
        public static VFXEnhancedTextureRegistry Instance { get; private set; }
        
        #endregion

        #region Impact Textures
        
        /// <summary>
        /// Impact hit effect textures for melee, projectile, and explosion effects.
        /// </summary>
        public static class Impact
        {
            /// <summary>4-point star burst for directional impacts (128x128)</summary>
            public static Texture2D StarBurst { get; internal set; }
            
            /// <summary>Curved arc for combo finisher effects (128x64)</summary>
            public static Texture2D ComboArc { get; internal set; }
            
            /// <summary>Concentric ripple rings for shockwaves (128x128)</summary>
            public static Texture2D RippleRings { get; internal set; }
            
            /// <summary>Directional slash mark for hit effects (64x32)</summary>
            public static Texture2D SlashMark { get; internal set; }
            
            /// <summary>Expanding shockwave ring (128x128)</summary>
            public static Texture2D Shockwave { get; internal set; }
            
            /// <summary>Explosion flash for instant bursts (128x128)</summary>
            public static Texture2D ExplosionFlash { get; internal set; }
            
            /// <summary>X-shaped slash cross for crossed impacts (128x128)</summary>
            public static Texture2D SlashCross { get; internal set; }
        }
        
        #endregion

        #region Beam Textures
        
        /// <summary>
        /// Beam component textures for segmented beam rendering.
        /// </summary>
        public static class BeamEnhanced
        {
            /// <summary>Tileable beam core segment (64x16)</summary>
            public static Texture2D CoreSegment { get; internal set; }
            
            /// <summary>Beam impact splash at endpoints (64x64)</summary>
            public static Texture2D ImpactSplash { get; internal set; }
            
            /// <summary>Muzzle flare origin point (64x64)</summary>
            public static Texture2D MuzzleFlare { get; internal set; }
            
            /// <summary>Tileable outer glow for beam edges (64x32)</summary>
            public static Texture2D OuterGlow { get; internal set; }
        }
        
        #endregion

        #region Screen Effect Textures
        
        /// <summary>
        /// Screen-space effect textures for post-processing.
        /// </summary>
        public static class Screen
        {
            /// <summary>Chromatic ring distortion map for circular aberration (256x256)</summary>
            public static Texture2D ChromaticRing { get; internal set; }
            
            /// <summary>Chromatic separation layer for RGB split (256x128)</summary>
            public static Texture2D ChromaticSeparation { get; internal set; }
            
            /// <summary>Radial speed lines for zoom blur (256x256)</summary>
            public static Texture2D SpeedLines { get; internal set; }
        }
        
        #endregion

        #region Smear Textures
        
        /// <summary>
        /// Swing smear textures for melee weapon trails.
        /// </summary>
        public static class Smear
        {
            /// <summary>Double-layer smear effect (256x64)</summary>
            public static Texture2D DoubleLaye { get; internal set; }
            
            /// <summary>Wide crescent arc slash (256x128)</summary>
            public static Texture2D CrescentArc { get; internal set; }
        }
        
        #endregion

        #region Trail Textures
        
        /// <summary>
        /// Particle trail textures for projectile effects.
        /// </summary>
        public static class Trail
        {
            /// <summary>Comet trail with gradient fade (256x32)</summary>
            public static Texture2D CometTrail { get; internal set; }
            
            /// <summary>Dissolving particle trail (128x32)</summary>
            public static Texture2D DissolvingTrail { get; internal set; }
            
            /// <summary>Ember particle scatter (128x128)</summary>
            public static Texture2D EmberScatter { get; internal set; }
            
            /// <summary>Full rotation spiral trail (128x128)</summary>
            public static Texture2D SpiralTrail { get; internal set; }
            
            /// <summary>Sparkle particle field (128x128)</summary>
            public static Texture2D SparkleField { get; internal set; }
        }
        
        #endregion

        #region Light Ray Textures
        
        /// <summary>
        /// God ray and directional light textures.
        /// </summary>
        public static class LightRay
        {
            /// <summary>Concentrated light cone (64x256)</summary>
            public static Texture2D Cone { get; internal set; }
            
            /// <summary>Radial god rays full circle (256x256)</summary>
            public static Texture2D RadialGodRays { get; internal set; }
            
            /// <summary>Single directional light shaft (64x256)</summary>
            public static Texture2D DirectionalShaft { get; internal set; }
        }
        
        #endregion

        #region Bloom Textures
        
        /// <summary>
        /// Bloom and glow effect textures.
        /// </summary>
        public static class Bloom
        {
            /// <summary>Hexagonal bokeh array (128x128)</summary>
            public static Texture2D HexBokeh { get; internal set; }
            
            /// <summary>Horizontal anamorphic streak (256x32)</summary>
            public static Texture2D AnamorphicStreak { get; internal set; }
            
            /// <summary>Multi-ring concentric glow stack (128x128)</summary>
            public static Texture2D ConcentricGlow { get; internal set; }
            
            /// <summary>Perfect soft color bloom (128x128)</summary>
            public static Texture2D SoftBloom { get; internal set; }
        }
        
        #endregion

        #region Afterimage Textures
        
        /// <summary>
        /// Motion blur and afterimage textures.
        /// </summary>
        public static class Afterimage
        {
            /// <summary>Horizontal motion blur streak (128x32)</summary>
            public static Texture2D MotionBlurStreak { get; internal set; }
        }
        
        #endregion

        #region Lightning Textures
        
        /// <summary>
        /// Lightning and electrical arc textures.
        /// </summary>
        public static class Lightning
        {
            /// <summary>Lightning arc branch pattern (128x256)</summary>
            public static Texture2D ArcBranch { get; internal set; }
        }
        
        #endregion

        #region Lifecycle
        
        public override void Load()
        {
            Instance = this;
            
            if (Main.dedServ) return;
        }
        
        public override void PostSetupContent()
        {
            if (Main.dedServ) return;
            
            Main.QueueMainThreadAction(LoadAllTextures);
        }
        
        public override void Unload()
        {
            Instance = null;
            
            // Clear all texture references
            Impact.StarBurst = null;
            Impact.ComboArc = null;
            Impact.RippleRings = null;
            Impact.SlashMark = null;
            Impact.Shockwave = null;
            Impact.ExplosionFlash = null;
            Impact.SlashCross = null;
            
            BeamEnhanced.CoreSegment = null;
            BeamEnhanced.ImpactSplash = null;
            BeamEnhanced.MuzzleFlare = null;
            BeamEnhanced.OuterGlow = null;
            
            Screen.ChromaticRing = null;
            Screen.ChromaticSeparation = null;
            Screen.SpeedLines = null;
            
            Smear.DoubleLaye = null;
            Smear.CrescentArc = null;
            
            Trail.CometTrail = null;
            Trail.DissolvingTrail = null;
            Trail.EmberScatter = null;
            Trail.SpiralTrail = null;
            Trail.SparkleField = null;
            
            LightRay.Cone = null;
            LightRay.RadialGodRays = null;
            LightRay.DirectionalShaft = null;
            
            Bloom.HexBokeh = null;
            Bloom.AnamorphicStreak = null;
            Bloom.ConcentricGlow = null;
            Bloom.SoftBloom = null;
            
            Afterimage.MotionBlurStreak = null;
            
            Lightning.ArcBranch = null;
        }
        
        #endregion

        #region Texture Loading
        
        private void LoadAllTextures()
        {
            // Note: Filenames have spaces - they're loaded with the exact names
            
            // ==========================================
            // IMPACT TEXTURES
            // ==========================================
            Impact.StarBurst = LoadTexture("Assets/VFX/Impacts/4-Point Star Impact Burst");
            Impact.ComboArc = LoadTexture("Assets/VFX/Impacts/Combo Finisher Impact Arc");
            Impact.RippleRings = LoadTexture("Assets/VFX/Impacts/Concentric Impact Ripple Rings");
            Impact.SlashMark = LoadTexture("Assets/VFX/Impacts/Directional Hit Slash Mark");
            Impact.Shockwave = LoadTexture("Assets/VFX/Impacts/Expanding Shockwave Ring");
            Impact.ExplosionFlash = LoadTexture("Assets/VFX/Impacts/Explosion Flash Burst");
            Impact.SlashCross = LoadTexture("Assets/VFX/Impacts/X-Shaped Slash Impact Cross");
            
            // ==========================================
            // BEAM TEXTURES
            // ==========================================
            BeamEnhanced.CoreSegment = LoadTexture("Assets/VFX/Beams/Beam Core Segment (Tileable)");
            BeamEnhanced.ImpactSplash = LoadTexture("Assets/VFX/Beams/Beam Impact Splash");
            BeamEnhanced.MuzzleFlare = LoadTexture("Assets/VFX/Beams/Beam Muzzle Flare Origin");
            BeamEnhanced.OuterGlow = LoadTexture("Assets/VFX/Beams/Beam Outer Glow (Tileable)");
            
            // ==========================================
            // SCREEN EFFECT TEXTURES
            // ==========================================
            Screen.ChromaticRing = LoadTexture("Assets/VFX/Screen/Chromatic Ring Distortion Map");
            Screen.ChromaticSeparation = LoadTexture("Assets/VFX/Screen/Chromatic Separation Base Layer");
            Screen.SpeedLines = LoadTexture("Assets/VFX/Screen/Radial Speed Lines Zoom Blur");
            
            // ==========================================
            // SMEAR TEXTURES
            // ==========================================
            Smear.DoubleLaye = LoadTexture("Assets/VFX/Smears/Double-Layer Smear Effect");
            Smear.CrescentArc = LoadTexture("Assets/VFX/Smears/Wide Crescent Arc Slash");
            
            // ==========================================
            // TRAIL TEXTURES
            // ==========================================
            Trail.CometTrail = LoadTexture("Assets/VFX/Trails/Comet Trail Gradient Fade");
            Trail.DissolvingTrail = LoadTexture("Assets/VFX/Trails/Dissolving Particle Trail");
            Trail.EmberScatter = LoadTexture("Assets/VFX/Trails/Ember Particle Scatter");
            Trail.SpiralTrail = LoadTexture("Assets/VFX/Trails/Full Rotation Spiral Trail");
            Trail.SparkleField = LoadTexture("Assets/VFX/Trails/Sparkle Particle Field");
            
            // ==========================================
            // LIGHT RAY TEXTURES
            // ==========================================
            LightRay.Cone = LoadTexture("Assets/VFX/LightRays/Concentrated Light Cone");
            LightRay.RadialGodRays = LoadTexture("Assets/VFX/LightRays/Radial God Rays Full Circle");
            LightRay.DirectionalShaft = LoadTexture("Assets/VFX/LightRays/Single Directional Light Shaft");
            
            // ==========================================
            // BLOOM TEXTURES
            // ==========================================
            Bloom.HexBokeh = LoadTexture("Assets/VFX/Blooms/Hexagonal Bokeh Array");
            Bloom.AnamorphicStreak = LoadTexture("Assets/VFX/Blooms/Horizontal Anamorphic Streak");
            Bloom.ConcentricGlow = LoadTexture("Assets/VFX/Blooms/Multi-Ring Concentric Glow Stack");
            Bloom.SoftBloom = LoadTexture("Assets/VFX/Blooms/Perfect Soft Color Bloom");
            
            // ==========================================
            // AFTERIMAGE TEXTURES
            // ==========================================
            Afterimage.MotionBlurStreak = LoadTexture("Assets/VFX/Afterimages/Horizontal Motion Blur Streak");
            
            // ==========================================
            // LIGHTNING TEXTURES
            // ==========================================
            Lightning.ArcBranch = LoadTexture("Assets/VFX/Lightning/Lightning Arc Branch Pattern");
            
            LogLoadStatus();
        }
        
        private Texture2D LoadTexture(string path)
        {
            try
            {
                string fullPath = $"MagnumOpus/{path}";
                if (ModContent.HasAsset(fullPath))
                {
                    return ModContent.Request<Texture2D>(fullPath, AssetRequestMode.ImmediateLoad).Value;
                }
                else
                {
                    Mod.Logger.Warn($"[VFXEnhanced] Asset not found: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"[VFXEnhanced] Failed to load {path}: {ex.Message}");
            }
            
            return null;
        }
        
        private void LogLoadStatus()
        {
            int loaded = 0;
            int failed = 0;
            
            void Check(Texture2D tex, string name)
            {
                if (tex != null)
                {
                    loaded++;
                    Mod.Logger.Info($"[VFXEnhanced] ✓ {name}: {tex.Width}x{tex.Height}");
                }
                else
                {
                    failed++;
                    Mod.Logger.Warn($"[VFXEnhanced] ✗ {name}: NOT LOADED");
                }
            }
            
            // Impacts
            Check(Impact.StarBurst, "Impact.StarBurst");
            Check(Impact.ComboArc, "Impact.ComboArc");
            Check(Impact.RippleRings, "Impact.RippleRings");
            Check(Impact.SlashMark, "Impact.SlashMark");
            Check(Impact.Shockwave, "Impact.Shockwave");
            Check(Impact.ExplosionFlash, "Impact.ExplosionFlash");
            Check(Impact.SlashCross, "Impact.SlashCross");
            
            // Beams
            Check(BeamEnhanced.CoreSegment, "BeamEnhanced.CoreSegment");
            Check(BeamEnhanced.ImpactSplash, "BeamEnhanced.ImpactSplash");
            Check(BeamEnhanced.MuzzleFlare, "BeamEnhanced.MuzzleFlare");
            Check(BeamEnhanced.OuterGlow, "BeamEnhanced.OuterGlow");
            
            // Screen
            Check(Screen.ChromaticRing, "Screen.ChromaticRing");
            Check(Screen.ChromaticSeparation, "Screen.ChromaticSeparation");
            Check(Screen.SpeedLines, "Screen.SpeedLines");
            
            // Smears
            Check(Smear.DoubleLaye, "Smear.DoubleLayer");
            Check(Smear.CrescentArc, "Smear.CrescentArc");
            
            // Trails
            Check(Trail.CometTrail, "Trail.CometTrail");
            Check(Trail.DissolvingTrail, "Trail.DissolvingTrail");
            Check(Trail.EmberScatter, "Trail.EmberScatter");
            Check(Trail.SpiralTrail, "Trail.SpiralTrail");
            Check(Trail.SparkleField, "Trail.SparkleField");
            
            // Light Rays
            Check(LightRay.Cone, "LightRay.Cone");
            Check(LightRay.RadialGodRays, "LightRay.RadialGodRays");
            Check(LightRay.DirectionalShaft, "LightRay.DirectionalShaft");
            
            // Blooms
            Check(Bloom.HexBokeh, "Bloom.HexBokeh");
            Check(Bloom.AnamorphicStreak, "Bloom.AnamorphicStreak");
            Check(Bloom.ConcentricGlow, "Bloom.ConcentricGlow");
            Check(Bloom.SoftBloom, "Bloom.SoftBloom");
            
            // Afterimage
            Check(Afterimage.MotionBlurStreak, "Afterimage.MotionBlurStreak");
            
            // Lightning
            Check(Lightning.ArcBranch, "Lightning.ArcBranch");
            
            Mod.Logger.Info($"[VFXEnhanced] SUMMARY: {loaded}/30 textures loaded, {failed} failed");
        }
        
        #endregion

        #region Public Convenience Methods
        
        /// <summary>
        /// Gets the best available impact texture.
        /// </summary>
        public static Texture2D GetImpactTexture(ImpactStyle style = ImpactStyle.StarBurst)
        {
            return style switch
            {
                ImpactStyle.StarBurst => Impact.StarBurst,
                ImpactStyle.ComboArc => Impact.ComboArc,
                ImpactStyle.RippleRings => Impact.RippleRings,
                ImpactStyle.SlashMark => Impact.SlashMark,
                ImpactStyle.Shockwave => Impact.Shockwave,
                ImpactStyle.ExplosionFlash => Impact.ExplosionFlash,
                ImpactStyle.SlashCross => Impact.SlashCross,
                _ => Impact.StarBurst
            } ?? VFXTextureRegistry.Mask.RadialGradient; // Fallback
        }
        
        /// <summary>
        /// Gets the best available bloom texture.
        /// </summary>
        public static Texture2D GetBloomTexture(BloomStyle style = BloomStyle.SoftBloom)
        {
            return style switch
            {
                BloomStyle.SoftBloom => Bloom.SoftBloom,
                BloomStyle.HexBokeh => Bloom.HexBokeh,
                BloomStyle.AnamorphicStreak => Bloom.AnamorphicStreak,
                BloomStyle.ConcentricGlow => Bloom.ConcentricGlow,
                _ => Bloom.SoftBloom
            } ?? VFXTextureRegistry.Mask.RadialGradient; // Fallback
        }
        
        /// <summary>
        /// Gets the best available trail texture.
        /// </summary>
        public static Texture2D GetTrailTexture(TrailStyle style = TrailStyle.CometTrail)
        {
            return style switch
            {
                TrailStyle.CometTrail => Trail.CometTrail,
                TrailStyle.DissolvingTrail => Trail.DissolvingTrail,
                TrailStyle.EmberScatter => Trail.EmberScatter,
                TrailStyle.SpiralTrail => Trail.SpiralTrail,
                TrailStyle.SparkleField => Trail.SparkleField,
                _ => Trail.CometTrail
            } ?? VFXTextureRegistry.Beam.TaperedLine; // Fallback
        }
        
        /// <summary>
        /// Gets the best available light ray texture.
        /// </summary>
        public static Texture2D GetLightRayTexture(LightRayStyle style = LightRayStyle.RadialGodRays)
        {
            return style switch
            {
                LightRayStyle.Cone => LightRay.Cone,
                LightRayStyle.RadialGodRays => LightRay.RadialGodRays,
                LightRayStyle.DirectionalShaft => LightRay.DirectionalShaft,
                _ => LightRay.RadialGodRays
            } ?? VFXTextureRegistry.Mask.RadialGradient; // Fallback
        }
        
        #endregion
    }
    
    #region Style Enums
    
    /// <summary>Impact effect styles</summary>
    public enum ImpactStyle
    {
        StarBurst,
        ComboArc,
        RippleRings,
        SlashMark,
        Shockwave,
        ExplosionFlash,
        SlashCross
    }
    
    /// <summary>Bloom effect styles</summary>
    public enum BloomStyle
    {
        SoftBloom,
        HexBokeh,
        AnamorphicStreak,
        ConcentricGlow
    }
    
    /// <summary>Trail effect styles</summary>
    public enum TrailStyle
    {
        CometTrail,
        DissolvingTrail,
        EmberScatter,
        SpiralTrail,
        SparkleField
    }
    
    /// <summary>Light ray styles</summary>
    public enum LightRayStyle
    {
        Cone,
        RadialGodRays,
        DirectionalShaft
    }
    
    #endregion
}
