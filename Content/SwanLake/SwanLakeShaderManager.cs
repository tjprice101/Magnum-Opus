using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.SwanLake
{
    /// <summary>
    /// Compartmentalized shader manager for all Swan Lake VFX.
    /// Provides availability checks, noise texture binding, generic Apply methods,
    /// and weapon-specific presets for each of the 12 Swan Lake shaders.
    ///
    /// Each weapon gets 2 dedicated shaders with 2 techniques each (Main + Glow):
    ///   CalloftheBlackSwan:       DualPolaritySwing, SwanFlareTrail
    ///   CallofthePearlescentLake: PearlescentRocketTrail, LakeExplosion
    ///   ChromaticSwanSong:        ChromaticTrail, AriaExplosion
    ///   FeatheroftheIridescentFlock: CrystalOrbitTrail, FlockAura
    ///   IridescentWingspan:       EtherealWing, WingspanFlareTrail
    ///   TheSwansLament:           LamentBulletTrail, DestructionRevelation
    ///
    /// Usage (in PreDraw):
    ///   SwanLakeShaderManager.BeginShaderAdditive(sb);
    ///   SwanLakeShaderManager.BindNoiseTexture(device);
    ///   SwanLakeShaderManager.ApplyBlackSwanSwingTrail(time, comboPhase, glow: false);
    ///   // ... draw trail geometry ...
    ///   SwanLakeShaderManager.RestoreSpriteBatch(sb);
    ///
    /// All Apply* methods gracefully return false if the shader is null,
    /// allowing VFX code to fall back to particle-based rendering.
    /// </summary>
    public static class SwanLakeShaderManager
    {
        // =====================================================================
        //  Legacy Shared Shader Constants (kept for backward compatibility)
        // =====================================================================

        public const string SwanLakeTrailShaderName = "SwanLakeTrail";
        public const string SwanLakeBloomShaderName = "SwanLakeBloom";
        public const string SwanLakePrismaticShaderName = "SwanLakePrismatic";

        // =====================================================================
        //  Shader Availability — Weapon-Specific
        // =====================================================================

        // CalloftheBlackSwan
        public static bool HasDualPolaritySwing => ShaderLoader.HasShader(ShaderLoader.DualPolaritySwingShader);
        public static bool HasSwanFlareTrail => ShaderLoader.HasShader(ShaderLoader.SwanFlareTrailShader);

        // CallofthePearlescentLake
        public static bool HasPearlescentRocketTrail => ShaderLoader.HasShader(ShaderLoader.PearlescentRocketTrailShader);
        public static bool HasLakeExplosion => ShaderLoader.HasShader(ShaderLoader.LakeExplosionShader);

        // ChromaticSwanSong
        public static bool HasChromaticTrail => ShaderLoader.HasShader(ShaderLoader.ChromaticTrailShader);
        public static bool HasAriaExplosion => ShaderLoader.HasShader(ShaderLoader.AriaExplosionShader);

        // FeatheroftheIridescentFlock
        public static bool HasCrystalOrbitTrail => ShaderLoader.HasShader(ShaderLoader.CrystalOrbitTrailShader);
        public static bool HasFlockAura => ShaderLoader.HasShader(ShaderLoader.FlockAuraShader);

        // IridescentWingspan
        public static bool HasEtherealWing => ShaderLoader.HasShader(ShaderLoader.EtherealWingShader);
        public static bool HasWingspanFlareTrail => ShaderLoader.HasShader(ShaderLoader.WingspanFlareTrailShader);

        // TheSwansLament
        public static bool HasLamentBulletTrail => ShaderLoader.HasShader(ShaderLoader.LamentBulletTrailShader);
        public static bool HasDestructionRevelation => ShaderLoader.HasShader(ShaderLoader.DestructionRevelationShader);

        // Legacy shared shader checks
        public static bool HasSwanTrail => ShaderLoader.HasShader(SwanLakeTrailShaderName);
        public static bool HasSwanBloom => ShaderLoader.HasShader(SwanLakeBloomShaderName);
        public static bool HasSwanPrismatic => ShaderLoader.HasShader(SwanLakePrismaticShaderName);

        /// <summary>True if any Swan Lake shader is available.</summary>
        public static bool IsAvailable =>
            HasDualPolaritySwing || HasSwanFlareTrail ||
            HasPearlescentRocketTrail || HasLakeExplosion ||
            HasChromaticTrail || HasAriaExplosion ||
            HasCrystalOrbitTrail || HasFlockAura ||
            HasEtherealWing || HasWingspanFlareTrail ||
            HasLamentBulletTrail || HasDestructionRevelation ||
            HasSwanTrail || HasSwanBloom || HasSwanPrismatic;

        /// <summary>True if any trail shader is usable (dedicated or shared fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);
        public static bool CanRenderTrails => HasDualPolaritySwing || HasSwanTrail || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics to sampler slot 1 for graceful flow distortion.
        /// Swan Lake's signature noise: soft, circular, elegant.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 for prismatic swirl effects.
        /// Used by crystal orbit trails and flock aura.
        /// </summary>
        public static void BindPrismaticNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SparklyNoiseTexture to sampler slot 1 for chromatic shimmer highlights.
        /// Used by Chromatic Swan Song trail and Aria Explosion.
        /// </summary>
        public static void BindSparklyNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds RealityCrackPattern to sampler slot 1 for shattering monochrome effects.
        /// Used by Destruction Revelation explosion.
        /// </summary>
        public static void BindCrackNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("RealityCrackPattern");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds TileableFBMNoise to sampler slot 1 for organic dissolve patterns.
        /// Used by Wingspan Flare Trail feather-dissolve.
        /// </summary>
        public static void BindFBMNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  Generic Apply Methods — Trail Shaders
        // =====================================================================

        /// <summary>
        /// Apply a trail shader with standard uniforms. Returns true if shader was applied.
        /// </summary>
        private static bool ApplyTrailShader(string shaderName, string technique,
            float time, Color primary, Color secondary,
            float scrollSpeed, float distortionAmt, float overbrightMult,
            float phase = 0f, float noiseScale = 3f, float noiseScroll = 0.5f,
            bool hasNoiseBound = false)
        {
            Effect shader = ShaderLoader.GetShader(shaderName);
            if (shader == null) return false;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uPhase"]?.SetValue(phase);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(hasNoiseBound ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
            return true;
        }

        /// <summary>
        /// Apply a radial/explosion shader with standard uniforms. Returns true if shader was applied.
        /// </summary>
        private static bool ApplyRadialShader(string shaderName, string technique,
            float time, Color primary, Color secondary,
            float explosionAge, float intensity, float overbrightMult,
            float noiseScale = 3f, float noiseScroll = 0.3f,
            bool hasNoiseBound = false)
        {
            Effect shader = ShaderLoader.GetShader(shaderName);
            if (shader == null) return false;

            shader.Parameters["uColor"]?.SetValue(new Vector4(primary.ToVector3(), 1f));
            shader.Parameters["uSecondaryColor"]?.SetValue(new Vector4(secondary.ToVector3(), 1f));
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(1f);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.1f);
            shader.Parameters["uPhase"]?.SetValue(explosionAge);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(hasNoiseBound);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
            return true;
        }

        // =====================================================================
        //  Legacy Shared Shader Methods (kept for backward compatibility)
        // =====================================================================

        /// <summary>
        /// Fallback trail rendering using the shared ScrollingTrailShader.
        /// </summary>
        public static void ApplyFallbackTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null)
                shader = ShaderLoader.Trail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);

            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON PRESETS — Call of the Black Swan (Melee Greatsword)
        // =====================================================================

        /// <summary>
        /// Apply DualPolaritySwing trail for Black Swan melee swing.
        /// Falls back through SwanLakeTrail -> ScrollingTrailShader.
        /// </summary>
        public static bool ApplyBlackSwanSwingTrail(float time, float comboPhase = 0f, bool glow = false)
        {
            string technique = glow ? "DualPolarityGlow" : "DualPolarityFlow";
            if (HasDualPolaritySwing)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.DualPolaritySwingShader, technique,
                    time, SwanLakePalette.ObsidianBlack, SwanLakePalette.PureWhite,
                    scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 2.8f,
                    phase: comboPhase, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, SwanLakePalette.ObsidianBlack, SwanLakePalette.PureWhite,
                scrollSpeed: 1.2f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        /// <summary>
        /// Apply SwanFlareTrail for Black Swan homing flare projectiles.
        /// flareType: 0 = black flare, 1 = white flare.
        /// </summary>
        public static bool ApplyBlackSwanFlareTrail(float time, float flareType = 0f, bool glow = false)
        {
            string technique = glow ? "SwanFlareGlow" : "SwanFlareMain";
            if (HasSwanFlareTrail)
            {
                Color primary = flareType < 0.5f ? SwanLakePalette.ObsidianBlack : SwanLakePalette.PureWhite;
                Color secondary = flareType < 0.5f ? SwanLakePalette.Silver : SwanLakePalette.DarkSilver;
                return ApplyTrailShader(ShaderLoader.SwanFlareTrailShader, technique,
                    time, primary, secondary,
                    scrollSpeed: 2.0f, distortionAmt: 0.04f, overbrightMult: 2.5f,
                    phase: flareType, hasNoiseBound: false);
            }

            Color fbPrimary = flareType < 0.5f ? SwanLakePalette.ObsidianBlack : SwanLakePalette.PureWhite;
            Color fbSecondary = flareType < 0.5f ? SwanLakePalette.Silver : SwanLakePalette.DarkSilver;
            ApplyFallbackTrail(time, fbPrimary, fbSecondary, scrollSpeed: 2.0f, overbrightMult: 2.5f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — Call of the Pearlescent Lake (Ranged Assault Rifle)
        // =====================================================================

        /// <summary>
        /// Apply PearlescentRocketTrail for Pearlescent Lake rocket projectiles.
        /// </summary>
        public static bool ApplyPearlescentLakeRocketTrail(float time, bool glow = false)
        {
            string technique = glow ? "PearlescentTrailGlow" : "PearlescentTrailMain";
            if (HasPearlescentRocketTrail)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.PearlescentRocketTrailShader, technique,
                    time, SwanLakePalette.LakeSurface, SwanLakePalette.Pearlescent,
                    scrollSpeed: 1.3f, distortionAmt: 0.06f, overbrightMult: 2.6f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, SwanLakePalette.LakeSurface, SwanLakePalette.Pearlescent,
                scrollSpeed: 1.3f, overbrightMult: 2.6f);
            return HasFallbackTrail;
        }

        /// <summary>
        /// Apply LakeExplosion for Pearlescent Lake rocket detonations.
        /// ringOnly: true for ring-only stacking pass.
        /// </summary>
        public static bool ApplyPearlescentLakeExplosion(float time, float explosionAge, bool ringOnly = false)
        {
            string technique = ringOnly ? "LakeExplosionRing" : "LakeExplosionMain";
            if (HasLakeExplosion)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.LakeExplosionShader, technique,
                    time, SwanLakePalette.PureWhite, SwanLakePalette.DarkSilver,
                    explosionAge, intensity: 1.5f, overbrightMult: 2.5f,
                    hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — Chromatic Swan Song (Magic Pistol)
        // =====================================================================

        /// <summary>
        /// Apply ChromaticTrail for Chromatic Swan Song tracking projectiles.
        /// comboPhase: 0.0 = shot 1 (desaturated), 0.5 = shot 2, 1.0 = shot 3 (vivid rainbow).
        /// </summary>
        public static bool ApplySwanSongProjectileTrail(float time, float comboPhase = 0f, bool glow = false)
        {
            string technique = glow ? "ChromaticTrailGlow" : "ChromaticTrailMain";
            if (HasChromaticTrail)
            {
                BindSparklyNoiseTexture(Main.graphics.GraphicsDevice);
                Color rainbow = SwanLakePalette.GetRainbow(time * 0.1f);
                return ApplyTrailShader(ShaderLoader.ChromaticTrailShader, technique,
                    time, SwanLakePalette.Silver, rainbow,
                    scrollSpeed: 1.5f, distortionAmt: 0.10f, overbrightMult: 3.0f,
                    phase: comboPhase, hasNoiseBound: true);
            }

            Color fbRainbow = SwanLakePalette.GetRainbow(time * 0.1f);
            ApplyFallbackTrail(time, SwanLakePalette.Silver, fbRainbow,
                scrollSpeed: 1.5f, overbrightMult: 3.0f);
            return HasFallbackTrail;
        }

        /// <summary>
        /// Apply AriaExplosion for Chromatic Swan Song 3-hit combo detonation.
        /// ringOnly: true for ring-only stacking pass.
        /// </summary>
        public static bool ApplySwanSongComboExplosion(float time, float explosionAge, bool ringOnly = false)
        {
            string technique = ringOnly ? "AriaExplosionRing" : "AriaExplosionMain";
            if (HasAriaExplosion)
            {
                BindSparklyNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.AriaExplosionShader, technique,
                    time, SwanLakePalette.PureWhite, SwanLakePalette.ObsidianBlack,
                    explosionAge, intensity: 2.0f, overbrightMult: 3.5f,
                    hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — Feather of the Iridescent Flock (Summoner)
        // =====================================================================

        /// <summary>
        /// Apply CrystalOrbitTrail for Iridescent Flock crystal minion orbits.
        /// crystalIndex: 0.0/0.33/0.66 for hue offset per crystal.
        /// </summary>
        public static bool ApplyIridescentFlockOrbitTrail(float time, float crystalIndex = 0f, bool glow = false)
        {
            string technique = glow ? "CrystalOrbitGlow" : "CrystalOrbitMain";
            if (HasCrystalOrbitTrail)
            {
                BindPrismaticNoiseTexture(Main.graphics.GraphicsDevice);
                Color rainbow = SwanLakePalette.GetVividRainbow(time * 0.15f + crystalIndex);
                return ApplyTrailShader(ShaderLoader.CrystalOrbitTrailShader, technique,
                    time, SwanLakePalette.Pearlescent, rainbow,
                    scrollSpeed: 1.0f, distortionAmt: 0.12f, overbrightMult: 3.2f,
                    phase: crystalIndex, noiseScale: 2.5f, noiseScroll: 0.7f,
                    hasNoiseBound: true);
            }

            Color fbRainbow = SwanLakePalette.GetVividRainbow(time * 0.15f + crystalIndex);
            ApplyFallbackTrail(time, SwanLakePalette.Pearlescent, fbRainbow,
                scrollSpeed: 1.8f, overbrightMult: 3.2f);
            return HasFallbackTrail;
        }

        /// <summary>
        /// Apply FlockAura for Iridescent Flock player-centered formation aura.
        /// activeCrystals: normalized crystal count (0.33/0.66/1.0).
        /// </summary>
        public static bool ApplyIridescentFlockAura(float time, float activeCrystals = 1f, bool glow = false)
        {
            string technique = glow ? "FlockAuraGlow" : "FlockAuraMain";
            if (HasFlockAura)
            {
                BindPrismaticNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.FlockAuraShader, technique,
                    time, SwanLakePalette.Silver, SwanLakePalette.PrismaticShimmer,
                    activeCrystals, intensity: 1.0f, overbrightMult: 2.0f,
                    hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — Iridescent Wingspan (Magic Staff)
        // =====================================================================

        /// <summary>
        /// Apply EtherealWing for Iridescent Wingspan hold-item wing silhouette.
        /// unfurlPhase: 0 = subtle idle, 1 = full cast burst.
        /// </summary>
        public static bool ApplyIridescentWingspanWings(float time, float unfurlPhase = 0f, bool glow = false)
        {
            string technique = glow ? "EtherealWingGlow" : "EtherealWingMain";
            if (HasEtherealWing)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.EtherealWingShader, technique,
                    time, SwanLakePalette.PureWhite, SwanLakePalette.PrismaticShimmer,
                    unfurlPhase, intensity: 1.2f, overbrightMult: 2.5f,
                    hasNoiseBound: true);
            }
            return false;
        }

        /// <summary>
        /// Apply WingspanFlareTrail for Iridescent Wingspan homing projectiles.
        /// projectileIndex: 0.0/0.33/0.66 for hue offset per projectile.
        /// </summary>
        public static bool ApplyIridescentWingspanFlareTrail(float time, float projectileIndex = 0f, bool glow = false)
        {
            string technique = glow ? "WingspanFlareGlow" : "WingspanFlareMain";
            if (HasWingspanFlareTrail)
            {
                BindFBMNoiseTexture(Main.graphics.GraphicsDevice);
                Color rainbow = SwanLakePalette.GetRainbow(time * 0.12f + projectileIndex);
                return ApplyTrailShader(ShaderLoader.WingspanFlareTrailShader, technique,
                    time, SwanLakePalette.PureWhite, rainbow,
                    scrollSpeed: 1.0f, distortionAmt: 0.07f, overbrightMult: 3.0f,
                    phase: projectileIndex, hasNoiseBound: true);
            }

            Color fbRainbow = SwanLakePalette.GetRainbow(time * 0.12f + projectileIndex);
            ApplyFallbackTrail(time, SwanLakePalette.PureWhite, fbRainbow,
                scrollSpeed: 1.0f, overbrightMult: 3.0f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — The Swan's Lament (Ranged Shotgun)
        // =====================================================================

        /// <summary>
        /// Apply LamentBulletTrail for Swan's Lament muted sorrowful bullet trails.
        /// </summary>
        public static bool ApplySwansLamentBulletTrail(float time, bool glow = false)
        {
            string technique = glow ? "LamentTrailGlow" : "LamentTrailMain";
            if (HasLamentBulletTrail)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.LamentBulletTrailShader, technique,
                    time, SwanLakePalette.SwanDarkGray, SwanLakePalette.FeatherWhite,
                    scrollSpeed: 0.8f, distortionAmt: 0.05f, overbrightMult: 2.0f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, SwanLakePalette.SwanDarkGray, SwanLakePalette.FeatherWhite,
                scrollSpeed: 0.8f, overbrightMult: 2.0f);
            return HasFallbackTrail;
        }

        /// <summary>
        /// Apply DestructionRevelation for Swan's Lament destruction halo explosions.
        /// ringOnly: true for ring-only stacking pass.
        /// </summary>
        public static bool ApplySwansLamentDestructionHalo(float time, float explosionAge, bool ringOnly = false)
        {
            string technique = ringOnly ? "RevelationBlastRing" : "RevelationBlastMain";
            if (HasDestructionRevelation)
            {
                BindCrackNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.DestructionRevelationShader, technique,
                    time, SwanLakePalette.Silver, SwanLakePalette.PureWhite,
                    explosionAge, intensity: 1.8f, overbrightMult: 3.0f,
                    hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  LEGACY WEAPON PRESETS (updated with dedicated shader priority)
        // =====================================================================

        /// <summary>Legacy preset: CalloftheBlackSwan trail. Now delegates to dedicated shader.</summary>
        public static void ApplyBlackSwanTrail(float time)
        {
            ApplyBlackSwanSwingTrail(time, comboPhase: 0f, glow: false);
        }

        /// <summary>Legacy preset: ChromaticSwanSong trail. Now delegates to dedicated shader.</summary>
        public static void ApplySwanSongTrail(float time)
        {
            ApplySwanSongProjectileTrail(time, comboPhase: 0f, glow: false);
        }

        /// <summary>Legacy preset: TheSwansLament trail. Now delegates to dedicated shader.</summary>
        public static void ApplySwansLamentTrail(float time)
        {
            ApplySwansLamentBulletTrail(time, glow: false);
        }

        /// <summary>Legacy preset: FeatheroftheIridescentFlock trail. Now delegates to dedicated shader.</summary>
        public static void ApplyIridescentFlockTrail(float time)
        {
            ApplyIridescentFlockOrbitTrail(time, crystalIndex: 0f, glow: false);
        }

        /// <summary>Legacy preset: IridescentWingspan trail. Now delegates to dedicated shader.</summary>
        public static void ApplyWingspanTrail(float time)
        {
            ApplyIridescentWingspanFlareTrail(time, projectileIndex: 0f, glow: false);
        }

        /// <summary>Legacy preset: CallofthePearlescentLake trail. Now delegates to dedicated shader.</summary>
        public static void ApplyPearlescentLakeTrail(float time)
        {
            ApplyPearlescentLakeRocketTrail(time, glow: false);
        }

        /// <summary>Legacy preset: Boss trail.</summary>
        public static void ApplyMonochromaticFractalTrail(float time)
        {
            ApplyFallbackTrail(time,
                SwanLakePalette.ShadowCore, SwanLakePalette.MonochromaticFlash,
                scrollSpeed: 2.0f, overbrightMult: 3.5f);
        }

        // =====================================================================
        //  SpriteBatch State Helpers
        // =====================================================================

        /// <summary>
        /// Begins a SpriteBatch in Immediate + Additive mode for shader drawing.
        /// </summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restores the SpriteBatch to normal deferred alpha-blend mode.
        /// </summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Begins a SpriteBatch in Deferred + Additive mode (no shader, for bloom stacking).
        /// </summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
