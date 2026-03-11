using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Eroica
{
    /// <summary>
    /// Compartmentalized shader manager for all Eroica VFX.
    /// Wraps ShaderLoader access to HeroicFlameTrail.fx, CelestialValorTrail.fx,
    /// SakuraBloom.fx, EroicaFuneralTrail.fx, TriumphantFractalShader.fx,
    /// SakuraSwingTrail.fx, and all weapon-specific shaders  Eproviding preset
    /// Apply* methods for each rendering technique.
    ///
    /// Root-Level Techniques:
    ///   HeroicFlameTrail     EHeroicFlameFlow / HeroicFlameGlow
    ///   CelestialValorTrail  EHeroicTrail / ValorFlare
    ///   SakuraBloom          ESakuraPetalBloom / SakuraGlowPass
    ///   EroicaFuneralTrail   EFuneralFlameFlow / FuneralGlowPass
    ///   TriumphantFractal    EFractalEnergyTrail / FractalGlowPass
    ///   SakuraSwingTrail     ESakuraTrailFlow / SakuraTrailGlow
    ///
    /// Usage (in PreDraw):
    ///   EroicaShaderManager.BindPerlinNoise(device);
    ///   EroicaShaderManager.ApplyHeroicFlameTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   EroicaShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class EroicaShaderManager
    {
        // =====================================================================
        //  Weapon-Specific Shader Path Constants
        //  (aliases to ShaderLoader's const strings for local readability)
        // =====================================================================

        private const string ValorAuraPath = ShaderLoader.ValorAuraShader;
        private const string PetalDissolvePath = ShaderLoader.PetalDissolveShader;
        private const string HeatDistortionPath = ShaderLoader.HeatDistortionShader;
        private const string TracerTrailPath = ShaderLoader.TracerTrailShader;
        private const string CrescendoChargePath = ShaderLoader.CrescendoChargeShader;
        private const string SakuraLightningTrailPath = ShaderLoader.SakuraLightningTrailShader;
        private const string RequiemBeamPath = ShaderLoader.RequiemBeamShader;
        private const string PrayerConvergencePath = ShaderLoader.PrayerConvergenceShader;
        private const string SacredGeometryPath = ShaderLoader.SacredGeometryShader;
        private const string FateSummonCirclePath = ShaderLoader.FateSummonCircleShader;
        private const string DarkFlameAuraPath = ShaderLoader.DarkFlameAuraShader;

        // =====================================================================
        //  Shader Availability  ERoot-Level
        // =====================================================================

        /// <summary>True if the HeroicFlameTrail shader loaded successfully.</summary>
        public static bool HasHeroicFlameTrail => ShaderLoader.HasShader(ShaderLoader.HeroicFlameTrailShader);

        /// <summary>True if the CelestialValorTrail shader loaded successfully.</summary>
        public static bool HasCelestialValorTrail => ShaderLoader.HasShader(ShaderLoader.CelestialValorTrailShader);

        /// <summary>True if the SakuraSwingTrail shader loaded successfully.</summary>
        public static bool HasSakuraSwingTrail => ShaderLoader.HasShader(ShaderLoader.SakuraSwingTrailShader);

        /// <summary>True if the SakuraBloom shader loaded successfully.</summary>
        public static bool HasSakuraBloom => ShaderLoader.HasShader(ShaderLoader.SakuraBloomShader);

        /// <summary>True if the EroicaFuneralTrail shader loaded successfully.</summary>
        public static bool HasFuneralTrail => ShaderLoader.HasShader(ShaderLoader.EroicaFuneralTrailShader);

        /// <summary>True if the TriumphantFractal shader loaded successfully.</summary>
        public static bool HasTriumphantFractal => ShaderLoader.HasShader(ShaderLoader.TriumphantFractalShaderName);

        // =====================================================================
        //  Shader Availability  EWeapon-Specific
        // =====================================================================

        /// <summary>True if the ValorAura shader loaded (Celestial Valor hold aura).</summary>
        public static bool HasValorAura => ShaderLoader.HasShader(ValorAuraPath);

        /// <summary>True if the PetalDissolve shader loaded (Sakura's Blossom spectral copy).</summary>
        public static bool HasPetalDissolve => ShaderLoader.HasShader(PetalDissolvePath);

        /// <summary>True if the HeatDistortion shader loaded (Blossom of the Sakura barrel heat).</summary>
        public static bool HasHeatDistortion => ShaderLoader.HasShader(HeatDistortionPath);

        /// <summary>True if the TracerTrail shader loaded (Blossom of the Sakura bullet trail).</summary>
        public static bool HasTracerTrail => ShaderLoader.HasShader(TracerTrailPath);

        /// <summary>True if the CrescendoCharge shader loaded (Piercing Light charge orbit).</summary>
        public static bool HasCrescendoCharge => ShaderLoader.HasShader(CrescendoChargePath);

        /// <summary>True if the SakuraLightningTrail shader loaded (Piercing Light lightning bolt).</summary>
        public static bool HasSakuraLightningTrail => ShaderLoader.HasShader(SakuraLightningTrailPath);

        /// <summary>True if the RequiemBeam shader loaded (Funeral Prayer beam body).</summary>
        public static bool HasRequiemBeam => ShaderLoader.HasShader(RequiemBeamPath);

        /// <summary>True if the PrayerConvergence shader loaded (Funeral Prayer convergence burst).</summary>
        public static bool HasPrayerConvergence => ShaderLoader.HasShader(PrayerConvergencePath);

        /// <summary>True if the SacredGeometry shader loaded (Triumphant Fractal hexagram burst).</summary>
        public static bool HasSacredGeometry => ShaderLoader.HasShader(SacredGeometryPath);

        /// <summary>True if the FateSummonCircle shader loaded (Finality summoning circle).</summary>
        public static bool HasFateSummonCircle => ShaderLoader.HasShader(FateSummonCirclePath);

        /// <summary>True if the DarkFlameAura shader loaded (Finality minion dark fire).</summary>
        public static bool HasDarkFlameAura => ShaderLoader.HasShader(DarkFlameAuraPath);

        /// <summary>True if any Eroica shader is available.</summary>
        public static bool IsAvailable => HasHeroicFlameTrail || HasCelestialValorTrail ||
            HasSakuraSwingTrail || HasSakuraBloom || HasFuneralTrail || HasTriumphantFractal ||
            HasValorAura || HasPetalDissolve || HasHeatDistortion || HasTracerTrail ||
            HasCrescendoCharge || HasSakuraLightningTrail || HasRequiemBeam || HasPrayerConvergence ||
            HasSacredGeometry || HasFateSummonCircle || HasDarkFlameAura;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds PerlinNoise to sampler slot 1 for flame distortion effects.
        /// Call once per frame before any flame-based Apply* method.
        /// </summary>
        public static void BindPerlinNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds NoiseSmoke to sampler slot 1 for funeral/smoke dissolution effects.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindSmokeNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SimplexNoise to sampler slot 1 for fractal/crystal effects.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindSparklyNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SimplexNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: HeroicFlameFlow / HeroicFlameGlow (HeroicFlameTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply HeroicFlameTrail.fx for burning valor flame trail.
        /// Uses turbulent fire distortion with scarlet-to-gold gradient.
        /// </summary>
        public static void ApplyHeroicFlameTrail(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.HeroicFlameTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = glowPass ? "HeroicFlameGlow" : "HeroicFlameFlow";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure HeroicFlameTrail.fx with noise texture for richer fire distortion.
        /// </summary>
        public static void ApplyHeroicFlameTrailWithNoise(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.HeroicFlameTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            string technique = glowPass ? "HeroicFlameGlow" : "HeroicFlameFlow";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: HeroicTrail / ValorFlare (CelestialValorTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply CelestialValorTrail.fx for valor-specific trail rendering.
        /// Sharper edge profile with ember impressions and golden crest highlights.
        /// </summary>
        public static void ApplyCelestialValorTrail(float time, Color primary, Color secondary,
            bool flarePass = false,
            float scrollSpeed = 1.5f, float overbrightMult = 3f, float progress = 0f)
        {
            Effect shader = ShaderLoader.CelestialValorTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uProgress"]?.SetValue(MathHelper.Clamp(progress, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = flarePass ? "ValorFlare" : "HeroicTrail";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: SakuraTrailFlow / SakuraTrailGlow (SakuraSwingTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply SakuraSwingTrail.fx for flowing sakura petal trail.
        /// Uses dual-frequency petal shimmer with pink-to-gold gradient.
        /// </summary>
        public static void ApplySakuraSwingTrail(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float phase = 0f)
        {
            Effect shader = ShaderLoader.SakuraSwingTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 3f));

            string technique = glowPass ? "SakuraTrailGlow" : "SakuraTrailFlow";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: SakuraPetalBloom / SakuraGlowPass (SakuraBloom.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply SakuraBloom.fx for procedural petal bloom overlay.
        /// Phase controls bloom state: 0=bud, 0.5=full bloom, 1=scatter.
        /// </summary>
        public static void ApplySakuraBloom(float time, float phase,
            Color primary, Color secondary,
            bool glowPass = false,
            float overbrightMult = 2.5f, float petalCount = 5f, float rotationSpeed = 0.5f)
        {
            Effect shader = ShaderLoader.SakuraBloom;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uPetalCount"]?.SetValue(petalCount);
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.1f);

            string technique = glowPass ? "SakuraGlowPass" : "SakuraPetalBloom";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: FuneralFlameFlow / FuneralGlowPass (EroicaFuneralTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply EroicaFuneralTrail.fx for somber, smoky flame trail.
        /// Slow-scrolling fire with smoke dissolution and incense wisps.
        /// </summary>
        public static void ApplyFuneralTrail(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 0.8f, float distortionAmt = 0.05f, float overbrightMult = 2.0f,
            float smokeIntensity = 0.6f)
        {
            Effect shader = ShaderLoader.EroicaFuneralTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(2.5f);
            shader.Parameters["uSmokeIntensity"]?.SetValue(MathHelper.Clamp(smokeIntensity, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);

            string technique = glowPass ? "FuneralGlowPass" : "FuneralFlameFlow";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure EroicaFuneralTrail.fx with smoke noise texture for richer dissolution.
        /// </summary>
        public static void ApplyFuneralTrailWithNoise(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 0.8f, float distortionAmt = 0.05f, float overbrightMult = 2.0f,
            float smokeIntensity = 0.6f, float noiseScale = 2.5f, float noiseScroll = 0.4f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.EroicaFuneralTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uSmokeIntensity"]?.SetValue(MathHelper.Clamp(smokeIntensity, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            string technique = glowPass ? "FuneralGlowPass" : "FuneralFlameFlow";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: FractalEnergyTrail / FractalGlowPass (TriumphantFractalShader.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply TriumphantFractalShader.fx for geometric hexagonal trail.
        /// Procedural hex grid with golden energy flowing through crystalline structure.
        /// </summary>
        public static void ApplyFractalTrail(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 1.2f, float distortionAmt = 0.04f, float overbrightMult = 3.0f,
            float fractalDepth = 2.0f, float rotationSpeed = 0.3f)
        {
            Effect shader = ShaderLoader.TriumphantFractal;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uFractalDepth"]?.SetValue(MathHelper.Clamp(fractalDepth, 1f, 3f));
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = glowPass ? "FractalGlowPass" : "FractalEnergyTrail";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure TriumphantFractalShader.fx with sparkly noise for crystal shimmer.
        /// </summary>
        public static void ApplyFractalTrailWithNoise(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 1.2f, float distortionAmt = 0.04f, float overbrightMult = 3.0f,
            float fractalDepth = 2.0f, float rotationSpeed = 0.3f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SimplexNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.TriumphantFractal;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uFractalDepth"]?.SetValue(MathHelper.Clamp(fractalDepth, 1f, 3f));
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            string technique = glowPass ? "FractalGlowPass" : "FractalEnergyTrail";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON-SPECIFIC SHADER APPLY METHODS
        //  (Available after Phase 3+4 creates the .fx files and registers them)
        // =====================================================================

        /// <summary>
        /// Apply ValorAura.fx for Celestial Valor concentric ember ring aura.
        /// comboPhase controls ring intensity (0-1).
        /// </summary>
        public static void ApplyValorAura(float time, float comboPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 1.0f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.GetShader(ValorAuraPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(comboPhase, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(comboPhase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "ValorAuraGlow" : "ValorAuraMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply PetalDissolve.fx for Sakura's Blossom spectral copy dissolution.
        /// dissolveProgress: 0=solid, 1=fully dissolved.
        /// </summary>
        public static void ApplyPetalDissolve(float time, float dissolveProgress,
            Color primary, Color secondary, bool glowPass = false,
            float overbrightMult = 2.0f)
        {
            Effect shader = ShaderLoader.GetShader(PetalDissolvePath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uDissolveProgress"]?.SetValue(MathHelper.Clamp(dissolveProgress, 0f, 1f));
            shader.Parameters["uNoiseScale"]?.SetValue(4f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "PetalDissolveGlow" : "PetalDissolveMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply HeatDistortion.fx for Blossom of the Sakura barrel heat mirage.
        /// heatLevel: 0=cool, 1=overheated.
        /// </summary>
        public static void ApplyHeatDistortion(float time, float heatLevel,
            Color primary, Color secondary,
            float distortionAmt = 0.08f, float scrollSpeed = 1.0f, float overbrightMult = 1.5f)
        {
            Effect shader = ShaderLoader.GetShader(HeatDistortionPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(heatLevel, 0f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uHeatLevel"]?.SetValue(MathHelper.Clamp(heatLevel, 0f, 1f));
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);

            shader.CurrentTechnique = shader.Techniques["HeatShimmerMain"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply TracerTrail.fx for Blossom of the Sakura heat-reactive bullet trail.
        /// heatLevel: 0=cool sakura pink, 1=white-hot gold.
        /// </summary>
        public static void ApplyTracerTrail(float time, float heatLevel,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 2.0f, float distortionAmt = 0.04f, float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.GetShader(TracerTrailPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1f + heatLevel * 0.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uHeatLevel"]?.SetValue(MathHelper.Clamp(heatLevel, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "TracerTrailGlow" : "TracerTrailMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply CrescendoCharge.fx for Piercing Light charge orbit indicator.
        /// chargeProgress: 0=empty, 1=fully charged.
        /// </summary>
        public static void ApplyCrescendoCharge(float time, float chargeProgress,
            Color primary, Color secondary, bool glowPass = false,
            float overbrightMult = 2.5f, float orbCount = 9f)
        {
            Effect shader = ShaderLoader.GetShader(CrescendoChargePath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.5f + chargeProgress * 0.8f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uChargeProgress"]?.SetValue(MathHelper.Clamp(chargeProgress, 0f, 1f));
            shader.Parameters["uOrbCount"]?.SetValue(orbCount);
            shader.Parameters["uScrollSpeed"]?.SetValue(1.0f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(chargeProgress, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "CrescendoChargeGlow" : "CrescendoChargeMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply SakuraLightningTrail.fx for Piercing Light zigzag lightning bolt trail.
        /// chargePhase controls intensity (0=weak spark, 1=full crescendo bolt).
        /// </summary>
        public static void ApplySakuraLightningTrail(float time, float chargePhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 2.0f, float distortionAmt = 0.06f, float overbrightMult = 3.5f)
        {
            Effect shader = ShaderLoader.GetShader(SakuraLightningTrailPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.5f + chargePhase * 0.8f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(chargePhase, 0f, 1f));
            shader.Parameters["uBranchIntensity"]?.SetValue(0.3f + chargePhase * 0.5f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "LightningTrailGlow" : "LightningTrailMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply RequiemBeam.fx for Funeral Prayer electric tracking beam body.
        /// Funeral-fire colored with Tesla coil arc distortion.
        /// </summary>
        public static void ApplyRequiemBeam(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 1.5f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float arcFrequency = 8f, float arcAmplitude = 0.04f)
        {
            Effect shader = ShaderLoader.GetShader(RequiemBeamPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uArcFrequency"]?.SetValue(arcFrequency);
            shader.Parameters["uArcAmplitude"]?.SetValue(arcAmplitude);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "RequiemBeamGlow" : "RequiemBeamMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply PrayerConvergence.fx for Funeral Prayer convergence burst.
        /// phase: 0=burst start, 1=fully expanded.
        /// </summary>
        public static void ApplyPrayerConvergence(float time, float phase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 1.0f, float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.GetShader(PrayerConvergencePath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(MathHelper.Clamp(1f - phase * 0.5f, 0.1f, 1f));
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uPointCount"]?.SetValue(5f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "ConvergenceGlow" : "ConvergenceMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply SacredGeometry.fx for Triumphant Fractal hexagram burst.
        /// phase: 0=burst start, 1=fully expanded.
        /// </summary>
        public static void ApplySacredGeometry(float time, float phase,
            Color primary, Color secondary, bool glowPass = false,
            float rotationSpeed = 0.5f, float overbrightMult = 3.0f, float recursionDepth = 2f)
        {
            Effect shader = ShaderLoader.GetShader(SacredGeometryPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(MathHelper.Clamp(1f - phase * 0.4f, 0.1f, 1f));
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uRotationSpeed"]?.SetValue(rotationSpeed);
            shader.Parameters["uRecursionDepth"]?.SetValue(MathHelper.Clamp(recursionDepth, 1f, 3f));
            shader.Parameters["uScrollSpeed"]?.SetValue(1.0f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "SacredGeometryGlow" : "SacredGeometryMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply FateSummonCircle.fx for Finality of the Sakura dark summoning circle.
        /// ritualPhase: 0=dormant, 1=fully active ritual.
        /// </summary>
        public static void ApplyFateSummonCircle(float time, float ritualPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 0.6f, float distortionAmt = 0.05f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.GetShader(FateSummonCirclePath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.3f + ritualPhase * 0.7f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(ritualPhase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "FateSummonGlow" : "FateSummonMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Apply DarkFlameAura.fx for Finality minion dark fire halo.
        /// Inverted flame aura with dark core and bright crimson edges.
        /// </summary>
        public static void ApplyDarkFlameAura(float time, Color primary, Color secondary,
            bool glowPass = false,
            float scrollSpeed = 0.8f, float distortionAmt = 0.06f, float overbrightMult = 2.0f,
            float phase = 0f)
        {
            Effect shader = ShaderLoader.GetShader(DarkFlameAuraPath);
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);

            string technique = glowPass ? "DarkFlameAuraGlow" : "DarkFlameAuraMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON-SPECIFIC PRESETS
        //  Tuned palettes and parameters for each Eroica weapon.
        // =====================================================================

        /// <summary>
        /// Preset: Celestial Valor swing trail  Eheroic fire with phase-scaled intensity.
        /// comboPhase maps to EroicaPalette.CelestialValorBlade progression.
        /// </summary>
        public static void ApplyCelestialValorSwingTrail(float time, float comboPhase, bool glowPass = false)
        {
            BindPerlinNoise(Main.graphics.GraphicsDevice);
            ApplyHeroicFlameTrailWithNoise(time,
                EroicaPalette.Scarlet, EroicaPalette.Gold,
                glowPass: glowPass,
                scrollSpeed: 1.5f + comboPhase * 0.5f,
                distortionAmt: 0.06f + comboPhase * 0.04f,
                overbrightMult: 2.5f + comboPhase * 1.5f);
        }

        /// <summary>
        /// Preset: Celestial Valor impact flare  EValorFlare technique.
        /// </summary>
        public static void ApplyCelestialValorFlare(float time, float comboPhase)
        {
            ApplyCelestialValorTrail(time,
                EroicaPalette.Scarlet, EroicaPalette.Gold,
                flarePass: true,
                scrollSpeed: 2.0f,
                overbrightMult: 3.5f + comboPhase * 1.0f,
                progress: comboPhase);
        }

        /// <summary>
        /// Preset: Celestial Valor hold aura  Eexpanding ember rings.
        /// </summary>
        public static void ApplyCelestialValorHoldAura(float time, float comboPhase, bool glowPass = false)
        {
            BindPerlinNoise(Main.graphics.GraphicsDevice);
            ApplyValorAura(time, comboPhase,
                EroicaPalette.Scarlet, EroicaPalette.Gold,
                glowPass: glowPass,
                scrollSpeed: 0.8f + comboPhase * 0.4f,
                overbrightMult: 2.0f + comboPhase * 1.5f);
        }

        /// <summary>
        /// Preset: Sakura's Blossom swing trail  Eflowing petal energy.
        /// comboPhase 0-3 maps to the 4-phase combo system.
        /// </summary>
        public static void ApplySakurasBlossomSwingTrail(float time, float comboPhase, bool glowPass = false)
        {
            BindPerlinNoise(Main.graphics.GraphicsDevice);
            ApplySakuraSwingTrail(time,
                EroicaPalette.Sakura, EroicaPalette.PollenGold,
                glowPass: glowPass,
                scrollSpeed: 1.0f + comboPhase * 0.3f,
                distortionAmt: 0.05f + comboPhase * 0.02f,
                overbrightMult: 2.5f + comboPhase * 0.5f,
                phase: comboPhase);
        }

        /// <summary>
        /// Preset: Sakura's Blossom petal bloom overlay  Ephase-driven petal burst.
        /// </summary>
        public static void ApplySakurasBlossomPetalBurst(float time, float bloomPhase, bool glowPass = false)
        {
            ApplySakuraBloom(time, bloomPhase,
                EroicaPalette.Sakura, EroicaPalette.PollenGold,
                glowPass: glowPass,
                overbrightMult: 2.5f + bloomPhase * 1.0f,
                petalCount: 5f,
                rotationSpeed: 0.5f + bloomPhase * 0.3f);
        }

        /// <summary>
        /// Preset: Sakura's Blossom spectral copy dissolution.
        /// </summary>
        public static void ApplySakurasBlossomDissolve(float time, float dissolveProgress, bool glowPass = false)
        {
            ApplyPetalDissolve(time, dissolveProgress,
                EroicaPalette.Sakura, EroicaPalette.PollenGold,
                glowPass: glowPass,
                overbrightMult: 2.0f);
        }

        /// <summary>
        /// Preset: Blossom of the Sakura tracer trail  Eheat-reactive color shift.
        /// heatLevel: heatCounter/40f (0=cool, 1=overheat).
        /// </summary>
        public static void ApplyBlossomTracerTrail(float time, float heatLevel, bool glowPass = false)
        {
            if (heatLevel > 0.5f)
                BindPerlinNoise(Main.graphics.GraphicsDevice);

            Color primary = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Scarlet, heatLevel);
            Color secondary = Color.Lerp(EroicaPalette.PollenGold, EroicaPalette.HotCore, heatLevel);

            ApplyTracerTrail(time, heatLevel,
                primary, secondary,
                glowPass: glowPass,
                scrollSpeed: 2.0f + heatLevel * 1.0f,
                distortionAmt: 0.03f + heatLevel * 0.05f,
                overbrightMult: 2.5f + heatLevel * 2.0f);
        }

        /// <summary>
        /// Preset: Blossom of the Sakura barrel heat shimmer.
        /// </summary>
        public static void ApplyBlossomHeatShimmer(float time, float heatLevel)
        {
            ApplyHeatDistortion(time, heatLevel,
                EroicaPalette.Flame, EroicaPalette.HotCore,
                distortionAmt: 0.04f + heatLevel * 0.08f,
                scrollSpeed: 0.8f + heatLevel * 0.4f,
                overbrightMult: 1.5f);
        }

        /// <summary>
        /// Preset: Piercing Light crescendo charge orbit.
        /// chargeProgress: shotCount/9f (0=empty, 1=ready for crescendo).
        /// </summary>
        public static void ApplyPiercingLightCrescendoCharge(float time, float chargeProgress, bool glowPass = false)
        {
            Color primary = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Scarlet, chargeProgress);
            Color secondary = Color.Lerp(EroicaPalette.PollenGold, EroicaPalette.Gold, chargeProgress);

            ApplyCrescendoCharge(time, chargeProgress,
                primary, secondary,
                glowPass: glowPass,
                overbrightMult: 2.5f + chargeProgress * 1.5f,
                orbCount: 9f);
        }

        /// <summary>
        /// Preset: Piercing Light lightning trail  Esakura-fire zigzag bolt.
        /// </summary>
        public static void ApplyPiercingLightLightningTrail(float time, float chargeProgress, bool glowPass = false)
        {
            BindSparklyNoise(Main.graphics.GraphicsDevice);
            ApplySakuraLightningTrail(time, chargeProgress,
                EroicaPalette.Sakura, EroicaPalette.Gold,
                glowPass: glowPass,
                scrollSpeed: 2.0f + chargeProgress * 1.0f,
                distortionAmt: 0.05f + chargeProgress * 0.04f,
                overbrightMult: 3.0f + chargeProgress * 1.5f);
        }

        /// <summary>
        /// Preset: Funeral Prayer beam trail  Esomber funeral fire beam.
        /// </summary>
        public static void ApplyFuneralPrayerBeamTrail(float time, bool glowPass = false)
        {
            BindSmokeNoise(Main.graphics.GraphicsDevice);
            ApplyFuneralTrailWithNoise(time,
                EroicaPalette.PaletteLerp(EroicaPalette.FuneralPrayerBlade, 0.4f),
                EroicaPalette.PaletteLerp(EroicaPalette.FuneralPrayerBlade, 0.7f),
                glowPass: glowPass,
                scrollSpeed: 0.8f,
                distortionAmt: 0.05f,
                overbrightMult: 2.0f,
                smokeIntensity: 0.7f);
        }

        /// <summary>
        /// Preset: Funeral Prayer requiem beam  Eelectric tracking beam body.
        /// </summary>
        public static void ApplyFuneralPrayerRequiemBeam(float time, bool glowPass = false)
        {
            BindSmokeNoise(Main.graphics.GraphicsDevice);
            ApplyRequiemBeam(time,
                EroicaPalette.DeepScarlet, EroicaPalette.PaletteLerp(EroicaPalette.FuneralPrayerBlade, 0.7f),
                glowPass: glowPass,
                scrollSpeed: 1.2f,
                distortionAmt: 0.06f,
                overbrightMult: 2.5f,
                arcFrequency: 8f,
                arcAmplitude: 0.04f);
        }

        /// <summary>
        /// Preset: Funeral Prayer convergence  E"Prayer Answered" radial burst.
        /// </summary>
        public static void ApplyFuneralPrayerConvergence(float time, float expansionPhase, bool glowPass = false)
        {
            ApplyPrayerConvergence(time, expansionPhase,
                EroicaPalette.Gold, EroicaPalette.Scarlet,
                glowPass: glowPass,
                scrollSpeed: 1.5f,
                overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: Triumphant Fractal projectile trail  Egolden geometry.
        /// </summary>
        public static void ApplyTriumphantFractalProjectileTrail(float time, bool glowPass = false)
        {
            BindSparklyNoise(Main.graphics.GraphicsDevice);
            ApplyFractalTrailWithNoise(time,
                EroicaPalette.Gold, EroicaPalette.Scarlet,
                glowPass: glowPass,
                scrollSpeed: 1.2f,
                distortionAmt: 0.04f,
                overbrightMult: 3.0f,
                fractalDepth: 2.0f,
                rotationSpeed: 0.3f);
        }

        /// <summary>
        /// Preset: Triumphant Fractal sacred geometry burst  Ehexagram impact.
        /// </summary>
        public static void ApplyTriumphantFractalSacredGeometry(float time, float burstPhase, bool glowPass = false)
        {
            ApplySacredGeometry(time, burstPhase,
                EroicaPalette.Gold, EroicaPalette.Scarlet,
                glowPass: glowPass,
                rotationSpeed: 0.5f + burstPhase * 0.3f,
                overbrightMult: 3.0f + burstPhase * 1.0f,
                recursionDepth: 2f);
        }

        /// <summary>
        /// Preset: Finality of the Sakura summoning circle  Edark fate ritual.
        /// </summary>
        public static void ApplyFinalitySummonCircle(float time, float ritualPhase, bool glowPass = false)
        {
            BindSmokeNoise(Main.graphics.GraphicsDevice);
            ApplyFateSummonCircle(time, ritualPhase,
                EroicaPalette.Crimson, EroicaPalette.DeepScarlet,
                glowPass: glowPass,
                scrollSpeed: 0.5f + ritualPhase * 0.3f,
                distortionAmt: 0.04f + ritualPhase * 0.04f,
                overbrightMult: 2.0f + ritualPhase * 1.5f);
        }

        /// <summary>
        /// Preset: Finality of the Sakura dark flame aura  Eminion dark fire halo.
        /// </summary>
        public static void ApplyFinalityDarkFlameAura(float time, bool glowPass = false)
        {
            BindSmokeNoise(Main.graphics.GraphicsDevice);
            ApplyDarkFlameAura(time,
                EroicaPalette.Crimson, EroicaPalette.Black,
                glowPass: glowPass,
                scrollSpeed: 0.6f,
                distortionAmt: 0.05f,
                overbrightMult: 2.0f);
        }

        /// <summary>
        /// Preset: Finality of the Sakura dark funeral trail  Eminion dark flame stream.
        /// Uses EroicaFuneralTrail with dark palette variant.
        /// </summary>
        public static void ApplyFinalityDarkFuneralTrail(float time, bool glowPass = false)
        {
            BindSmokeNoise(Main.graphics.GraphicsDevice);
            ApplyFuneralTrailWithNoise(time,
                EroicaPalette.Crimson, EroicaPalette.DeepScarlet,
                glowPass: glowPass,
                scrollSpeed: 0.6f,
                distortionAmt: 0.05f,
                overbrightMult: 1.8f,
                smokeIntensity: 0.8f);
        }

        // =====================================================================
        //  SpriteBatch State Helpers
        // =====================================================================

        /// <summary>
        /// Begins a SpriteBatch in Immediate mode for shader-driven rendering.
        /// Uses Additive blending. Alias for BeginShaderAdditive.
        /// </summary>
        public static void BeginShaderBatch(SpriteBatch sb) => BeginShaderAdditive(sb);

        /// <summary>
        /// Restores the SpriteBatch to default deferred mode after shader drawing.
        /// Alias for RestoreSpriteBatch.
        /// </summary>
        public static void RestoreDefaultBatch(SpriteBatch sb) => RestoreSpriteBatch(sb);

        /// <summary>
        /// Begins a SpriteBatch in Immediate + Additive mode for shader drawing.
        /// Uses ShaderAdditive (SourceAlpha blend) so shader alpha masking works correctly.
        /// </summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(
                SpriteSortMode.Immediate,
                MagnumBlendStates.ShaderAdditive,
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
                MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
