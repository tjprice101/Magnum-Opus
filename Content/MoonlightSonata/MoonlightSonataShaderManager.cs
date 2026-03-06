using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.MoonlightSonata
{
    /// <summary>
    /// Compartmentalized shader manager for all Moonlight Sonata VFX.
    /// Wraps ShaderLoader access to MoonlightTrail.fx, LunarBeam.fx,
    /// and CrescentAura.fx — providing preset Apply* methods for each
    /// rendering technique.
    ///
    /// Techniques available:
    ///   MoonlightTrail  — MoonlightFlowTrail: flowing lunar trail with noise distortion
    ///   LunarBeam       — CrescentBeam: crescent-shaped beam body rendering
    ///   CrescentAura    — CrescentShape / CrescentPulse: procedural crescent overlays
    ///
    /// Usage (in PreDraw):
    ///   MoonlightSonataShaderManager.BindNoiseTexture(device);
    ///   MoonlightSonataShaderManager.ApplyMoonlightTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   MoonlightSonataShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class MoonlightSonataShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the MoonlightTrail shader loaded successfully.</summary>
        public static bool HasMoonlightTrail => ShaderLoader.HasShader(ShaderLoader.MoonlightTrailShader);

        /// <summary>True if the LunarBeam shader loaded successfully.</summary>
        public static bool HasLunarBeam => ShaderLoader.HasShader(ShaderLoader.LunarBeamShader);

        /// <summary>True if the CrescentAura shader loaded successfully.</summary>
        public static bool HasCrescentAura => ShaderLoader.HasShader(ShaderLoader.CrescentAuraShader);

        /// <summary>True if any Moonlight shader is available.</summary>
        public static bool IsAvailable => HasMoonlightTrail || HasLunarBeam || HasCrescentAura || HasTidalTrail || HasIncisorResonance || HasPrismaticBeam || HasCometTrail || HasGravitationalRift;

        // --- EternalMoon dedicated shaders ---

        /// <summary>True if the TidalTrail shader loaded successfully.</summary>
        public static bool HasTidalTrail => ShaderLoader.HasShader(ShaderLoader.TidalTrailShader);

        /// <summary>True if the CrescentBloom shader loaded successfully.</summary>
        public static bool HasCrescentBloom => ShaderLoader.HasShader(ShaderLoader.CrescentBloomShader);

        /// <summary>True if the LunarPhaseAura shader loaded successfully.</summary>
        public static bool HasLunarPhaseAura => ShaderLoader.HasShader(ShaderLoader.LunarPhaseAuraShader);

        // --- IncisorOfMoonlight dedicated shaders ---

        /// <summary>True if the IncisorResonance shader loaded successfully.</summary>
        public static bool HasIncisorResonance => ShaderLoader.HasShader(ShaderLoader.IncisorResonanceShader);

        /// <summary>True if the ConstellationField shader loaded successfully.</summary>
        public static bool HasConstellationField => ShaderLoader.HasShader(ShaderLoader.ConstellationFieldShader);

        // --- MoonlightsCalling dedicated shaders ---

        /// <summary>True if the PrismaticBeam shader loaded successfully.</summary>
        public static bool HasPrismaticBeam => ShaderLoader.HasShader(ShaderLoader.PrismaticBeamShader);

        /// <summary>True if the RefractionRipple shader loaded successfully.</summary>
        public static bool HasRefractionRipple => ShaderLoader.HasShader(ShaderLoader.RefractionRippleShader);

        // --- ResurrectionOfTheMoon dedicated shaders ---

        /// <summary>True if the CometTrail shader loaded successfully.</summary>
        public static bool HasCometTrail => ShaderLoader.HasShader(ShaderLoader.CometTrailShader);

        /// <summary>True if the SupernovaBlast shader loaded successfully.</summary>
        public static bool HasSupernovaBlast => ShaderLoader.HasShader(ShaderLoader.SupernovaBlastShader);

        // --- StaffOfTheLunarPhases dedicated shaders ---

        /// <summary>True if the GravitationalRift shader loaded successfully.</summary>
        public static bool HasGravitationalRift => ShaderLoader.HasShader(ShaderLoader.GravitationalRiftShader);

        /// <summary>True if the SummonCircle shader loaded successfully.</summary>
        public static bool HasSummonCircle => ShaderLoader.HasShader(ShaderLoader.SummonCircleShader);

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics noise to sampler slot 1 for trail distortion.
        /// Call once per frame before any Apply* method.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds NebulaWispNoise to sampler slot 1 for cosmic/void effects.
        /// Falls back to SoftCircularCaustics if unavailable.
        /// </summary>
        public static void BindCosmicNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NebulaWispNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: MoonlightFlowTrail (MoonlightTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply MoonlightTrail.fx for standard lunar trail rendering.
        /// Uses flowing purple-to-blue gradient with optional noise distortion.
        /// </summary>
        public static void ApplyMoonlightTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.MoonlightTrail;
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

            shader.CurrentTechnique = shader.Techniques["MoonlightFlowTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure MoonlightTrail.fx with noise texture on sampler 1
        /// for richer distortion effects.
        /// </summary>
        public static void ApplyMoonlightTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.5f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.MoonlightTrail;
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

            shader.CurrentTechnique = shader.Techniques["MoonlightFlowTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: EternalMoon trail — wide crescent sweep with moderate distortion.
        /// </summary>
        public static void ApplyEternalMoonTrail(float time)
        {
            ApplyMoonlightTrailWithNoise(time,
                MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue,
                scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 2.8f);
        }

        /// <summary>
        /// Preset: Incisor trail — sharp, fast, minimal distortion for precision cuts.
        /// </summary>
        public static void ApplyIncisorTrail(float time)
        {
            ApplyMoonlightTrail(time,
                MoonlightSonataPalette.GravityWell, MoonlightSonataPalette.IceBlue,
                scrollSpeed: 2.0f, distortionAmt: 0.03f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: Resurrection comet trail — heavy, impactful, strong distortion.
        /// </summary>
        public static void ApplyResurrectionTrail(float time)
        {
            ApplyMoonlightTrailWithNoise(time,
                MoonlightSonataPalette.NebulaPurple, MoonlightSonataPalette.CrescentGlow,
                scrollSpeed: 0.8f, distortionAmt: 0.10f, overbrightMult: 3.5f,
                noiseScale: 2.5f, noiseScroll: 0.7f);
        }

        /// <summary>
        /// Preset: Goliath cosmic beam trail — massive, gravitational distortion.
        /// </summary>
        public static void ApplyGoliathTrail(float time)
        {
            ApplyMoonlightTrailWithNoise(time,
                MoonlightSonataPalette.CosmicVoid, MoonlightSonataPalette.StarCore,
                scrollSpeed: 0.6f, distortionAmt: 0.12f, overbrightMult: 4.0f,
                noiseScale: 2f, noiseScroll: 0.4f);
        }

        // =====================================================================
        //  TECHNIQUE: CrescentBeam (LunarBeam.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply LunarBeam.fx for crescent-shaped beam rendering.
        /// Phase controls crescent fullness (0 = new moon, 1 = full moon).
        /// </summary>
        public static void ApplyLunarBeam(float time, float phase,
            Color primary, Color secondary,
            float scrollSpeed = 1.5f, float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.LunarBeam;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.8f);

            shader.CurrentTechnique = shader.Techniques["CrescentBeam"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: MoonlightsCalling prismatic beam — bouncing refraction beam.
        /// </summary>
        public static void ApplyMoonlightsCallingBeam(float time, float bouncePhase)
        {
            ApplyLunarBeam(time, bouncePhase,
                MoonlightSonataPalette.PrismViolet, MoonlightSonataPalette.RefractedBlue,
                scrollSpeed: 2.0f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: WaningDeer lunar beam sweep — enemy attack beam.
        /// </summary>
        public static void ApplyWaningDeerBeam(float time, float sweepProgress)
        {
            ApplyLunarBeam(time, sweepProgress,
                MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue,
                scrollSpeed: 1.0f, overbrightMult: 2.5f);
        }

        // =====================================================================
        //  TECHNIQUE: CrescentShape / CrescentPulse (CrescentAura.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply CrescentAura.fx for crescent moon overlays.
        /// Set pulsing=true for rhythmic pulse variant.
        /// </summary>
        public static void ApplyCrescentAura(float time, float phase,
            Color primary, Color secondary,
            float overbrightMult = 2f, bool pulsing = false)
        {
            Effect shader = ShaderLoader.CrescentAura;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uSharpness"]?.SetValue(2.5f);

            string technique = pulsing ? "CrescentPulse" : "CrescentShapeTechnique";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: StaffOfTheLunarPhases crescent — phase-cycling aura for conductor's baton.
        /// </summary>
        public static void ApplyLunarPhaseAura(float time, float lunarPhase)
        {
            ApplyCrescentAura(time, lunarPhase,
                MoonlightSonataPalette.Violet, MoonlightSonataPalette.CrescentGlow,
                overbrightMult: 2.5f, pulsing: true);
        }

        // =====================================================================
        //  TECHNIQUE: TidalTrailMain / TidalTrailGlow (TidalTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply TidalTrail.fx for EternalMoon flowing water trail.
        /// comboPhase controls intensity (0.25 new moon → 1.0 full moon).
        /// </summary>
        public static void ApplyTidalTrail(float time, float comboPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 1.0f, float distortionAmt = 0.06f, float overbrightMult = 2.8f)
        {
            Effect shader = ShaderLoader.TidalTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(comboPhase, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);

            string technique = glowPass ? "TidalTrailGlow" : "TidalTrailMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: EternalMoon tidal trail with noise texture bound for full caustic effect.
        /// </summary>
        public static void ApplyEternalMoonTidalTrail(float time, float comboPhase, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.TidalTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.DarkPurple.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.IceBlue.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(comboPhase, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(2.8f + comboPhase * 1.2f);
            shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.06f + comboPhase * 0.04f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);

            string technique = glowPass ? "TidalTrailGlow" : "TidalTrailMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: CrescentBloomPass / CrescentGlowPass (CrescentBloom.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply CrescentBloom.fx for procedural crescent moon overlay.
        /// phase controls crescent illumination (0 = new moon sliver, 1 = full circle).
        /// </summary>
        public static void ApplyCrescentBloom(float time, float phase,
            Color primary, Color secondary,
            float overbrightMult = 2.5f, bool glowPass = false)
        {
            Effect shader = ShaderLoader.CrescentBloom;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);

            string technique = glowPass ? "CrescentGlowPass" : "CrescentBloomPass";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: EternalMoon crescent bloom with palette colors.
        /// </summary>
        public static void ApplyEternalMoonCrescentBloom(float time, float comboPhase, bool glowPass = false)
        {
            ApplyCrescentBloom(time, comboPhase,
                MoonlightSonataPalette.Violet, MoonlightSonataPalette.CrescentGlow,
                overbrightMult: 2.5f + comboPhase * 1.5f, glowPass: glowPass);
        }

        // =====================================================================
        //  TECHNIQUE: LunarPhaseAuraPass (LunarPhaseAura.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply LunarPhaseAura.fx for expanding concentric ring aura.
        /// phase controls ring density and brightness (0 = subtle, 1 = intense).
        /// </summary>
        public static void ApplyLunarPhaseAuraShader(float time, float phase,
            Color primary, Color secondary,
            float scrollSpeed = 1.0f, float overbrightMult = 2f)
        {
            Effect shader = ShaderLoader.LunarPhaseAura;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(phase, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique = shader.Techniques["LunarPhaseAuraPass"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: EternalMoon aura with palette colors and noise.
        /// </summary>
        public static void ApplyEternalMoonAura(float time, float comboPhase)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.LunarPhaseAura;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.Violet.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.IceBlue.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(comboPhase, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(2f + comboPhase);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(comboPhase, 0f, 1f));
            shader.Parameters["uScrollSpeed"]?.SetValue(0.8f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique = shader.Techniques["LunarPhaseAuraPass"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: IncisorResonanceTrail / IncisorResonanceGlow (IncisorResonance.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply IncisorResonance.fx for standing-wave resonance trail.
        /// resonanceLevel controls frequency density (0.3 low → 1.0 full resonance).
        /// </summary>
        public static void ApplyIncisorResonance(float time, float resonanceLevel,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 2.0f, float distortionAmt = 0.04f, float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.IncisorResonance;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(resonanceLevel, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.6f);

            string technique = glowPass ? "IncisorResonanceGlow" : "IncisorResonanceTrail";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Incisor resonance trail with noise texture and palette colors.
        /// </summary>
        public static void ApplyIncisorResonanceTrail(float time, float resonanceLevel, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.IncisorResonance;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.GravityWell.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.IceBlue.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(resonanceLevel, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(3.0f + resonanceLevel * 1.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(2.0f + resonanceLevel * 0.5f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.03f + resonanceLevel * 0.03f);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.6f);

            string technique = glowPass ? "IncisorResonanceGlow" : "IncisorResonanceTrail";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: ConstellationFieldMain / ConstellationFieldGlow (ConstellationField.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply ConstellationField.fx for parallax starfield overlay.
        /// phase controls star density and constellation line visibility (0-1).
        /// </summary>
        public static void ApplyConstellationField(float time, float phase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 0.5f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ConstellationField;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(phase, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(phase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.05f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "ConstellationFieldGlow" : "ConstellationFieldMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Incisor constellation field overlay with palette colors and star scatter noise.
        /// </summary>
        public static void ApplyIncisorConstellationField(float time, float resonanceLevel, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("StarFieldScatter");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.ConstellationField;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(new Vector3(0.627f, 0.784f, 1f)); // ConstellationBlue
            shader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.922f, 0.941f, 1f)); // HarmonicWhite
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(resonanceLevel, 0.2f, 1f));
            shader.Parameters["uOverbrightMult"]?.SetValue(2.5f + resonanceLevel * 1.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(0.4f + resonanceLevel * 0.2f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(resonanceLevel, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.04f + resonanceLevel * 0.02f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "ConstellationFieldGlow" : "ConstellationFieldMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: PrismaticBeamMain / PrismaticBeamGlow (PrismaticBeam.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply PrismaticBeam.fx for spectral color-splitting beam trail.
        /// spectralPhase controls the spectral spread (0 = narrow purple, 1 = full rainbow).
        /// </summary>
        public static void ApplyPrismaticBeam(float time, float spectralPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 1.5f, float distortionAmt = 0.08f, float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.PrismaticBeam;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(0.5f + spectralPhase * 0.5f, 0.3f, 1.5f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(spectralPhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "PrismaticBeamGlow" : "PrismaticBeamMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: MoonlightsCalling prismatic beam trail with noise texture and palette colors.
        /// bouncePhase = bounceCount / MaxBounces (0 = first shot, 1 = final bounce).
        /// </summary>
        public static void ApplyMoonlightsCallingPrismaticBeam(float time, float bouncePhase, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.PrismaticBeam;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.PrismViolet.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.RefractedBlue.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.5f + bouncePhase * 0.8f);
            shader.Parameters["uOverbrightMult"]?.SetValue(3.0f + bouncePhase * 1.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(1.5f + bouncePhase * 0.5f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.06f + bouncePhase * 0.06f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(bouncePhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "PrismaticBeamGlow" : "PrismaticBeamMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: RefractionRippleMain / RefractionRippleSubtle (RefractionRipple.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply RefractionRipple.fx for prismatic expanding ring effect.
        /// rippleAge controls expansion (0 = just created, 1 = fully expanded).
        /// </summary>
        public static void ApplyRefractionRipple(float time, float rippleAge,
            Color primary, Color secondary, bool subtle = false,
            float scrollSpeed = 1.0f, float distortionAmt = 0.12f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.RefractionRipple;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(MathHelper.Clamp(1f - rippleAge * 0.8f, 0.1f, 1f));
            shader.Parameters["uIntensity"]?.SetValue(1f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(rippleAge, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);

            string technique = subtle ? "RefractionRippleSubtle" : "RefractionRippleMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: MoonlightsCalling refraction ripple at bounce point.
        /// bounceIntensity scales with bounce count (higher = more dramatic).
        /// </summary>
        public static void ApplyMoonlightsCallingRefractionRipple(float time, float rippleAge, float bounceIntensity)
        {
            Effect shader = ShaderLoader.RefractionRipple;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.PrismViolet.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.RefractedBlue.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(MathHelper.Clamp(1f - rippleAge * 0.7f, 0.1f, 1f));
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(bounceIntensity, 0.5f, 2f));
            shader.Parameters["uOverbrightMult"]?.SetValue(2.5f + bounceIntensity * 0.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(1.2f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.10f + bounceIntensity * 0.04f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(rippleAge, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);

            shader.CurrentTechnique = shader.Techniques["RefractionRippleMain"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: CometTrailMain / CometTrailGlow (CometTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply CometTrail.fx for burning comet tail trail rendering.
        /// cometPhase controls comet heat intensity (0 = cold first shot, 1 = white-hot max ricochets).
        /// </summary>
        public static void ApplyCometTrail(float time, float cometPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 1.2f, float noiseScale = 3f, float distortionAmt = 0.06f,
            float overbrightMult = 3.0f)
        {
            Effect shader = ShaderLoader.CometTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(0.5f + cometPhase * 0.8f, 0.3f, 1.5f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(cometPhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = glowPass ? "CometTrailGlow" : "CometTrailMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Resurrection comet trail with noise texture and palette colors.
        /// ricochetPhase = ricochetCount / MaxRicochets (0 = first shot, 1 = max ricochets).
        /// </summary>
        public static void ApplyResurrectionCometTrail(float time, float ricochetPhase, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.CometTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.StarCore.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.NebulaPurple.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.5f + ricochetPhase * 0.8f);
            shader.Parameters["uOverbrightMult"]?.SetValue(3.0f + ricochetPhase * 1.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(1.2f + ricochetPhase * 0.4f);
            shader.Parameters["uNoiseScale"]?.SetValue(3f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.05f + ricochetPhase * 0.05f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(ricochetPhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = glowPass ? "CometTrailGlow" : "CometTrailMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: SupernovaBlastMain / SupernovaBlastRing (SupernovaBlast.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply SupernovaBlast.fx for radial crater explosion rendering.
        /// explosionAge controls expansion (0 = just detonated, 1 = fully expanded/faded).
        /// </summary>
        public static void ApplySupernovaBlast(float time, float explosionAge,
            Color primary, Color secondary, bool ringOnly = false,
            float scrollSpeed = 1.0f, float noiseScale = 4f, float distortionAmt = 0.08f,
            float overbrightMult = 3.5f)
        {
            Effect shader = ShaderLoader.SupernovaBlast;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(MathHelper.Clamp(1f - explosionAge * 0.6f, 0.1f, 1f));
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uNoiseScale"]?.SetValue(noiseScale);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(explosionAge, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = ringOnly ? "SupernovaBlastRing" : "SupernovaBlastMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Resurrection supernova blast with noise texture and palette colors.
        /// Used for on-hit detonations, grand finale, and SupernovaShell impacts.
        /// </summary>
        public static void ApplyResurrectionSupernovaBlast(float time, float explosionAge, bool ringOnly = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.SupernovaBlast;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.StarCore.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.DeepSpaceViolet.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(MathHelper.Clamp(1f - explosionAge * 0.5f, 0.15f, 1f));
            shader.Parameters["uIntensity"]?.SetValue(1.5f);
            shader.Parameters["uOverbrightMult"]?.SetValue(3.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(1.0f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.08f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(explosionAge, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(2f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            string technique = ringOnly ? "SupernovaBlastRing" : "SupernovaBlastMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: GravitationalRiftMain / GravitationalRiftGlow (GravitationalRift.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply GravitationalRift.fx for spiral gravity well distortion.
        /// riftPhase controls vortex intensity (0 = dormant, 1 = fully active rift).
        /// </summary>
        public static void ApplyGravitationalRift(float time, float riftPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 1.0f, float distortionAmt = 0.08f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.GravitationalRift;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(0.3f + riftPhase * 0.7f, 0.2f, 1.5f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(riftPhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "GravitationalRiftGlow" : "GravitationalRiftMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Goliath gravitational rift with cosmic noise and palette colors.
        /// chargeProgress = 0..1 during beam charge, or ambient level when idle.
        /// </summary>
        public static void ApplyGoliathGravitationalRift(float time, float chargeProgress, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.GravitationalRift;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.GravityWell.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.StarCore.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.3f + chargeProgress * 0.7f);
            shader.Parameters["uOverbrightMult"]?.SetValue(2.5f + chargeProgress * 1.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(0.8f + chargeProgress * 0.5f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.06f + chargeProgress * 0.06f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(chargeProgress, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "GravitationalRiftGlow" : "GravitationalRiftMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  TECHNIQUE: SummonCircleMain / SummonCircleGlow (SummonCircle.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply SummonCircle.fx for rotating lunar phase sigil.
        /// ritualPhase controls circle visibility and ring intensity (0 = dormant, 1 = fully active ritual).
        /// </summary>
        public static void ApplySummonCircle(float time, float ritualPhase,
            Color primary, Color secondary, bool glowPass = false,
            float scrollSpeed = 0.8f, float distortionAmt = 0.05f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.SummonCircle;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(MathHelper.Clamp(0.3f + ritualPhase * 0.7f, 0.2f, 1.5f));
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(ritualPhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "SummonCircleGlow" : "SummonCircleMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Preset: Summoning ritual circle with cosmic noise and palette colors.
        /// ritualPhase = 0..1 during summoning sequence.
        /// </summary>
        public static void ApplyGoliathSummonCircle(float time, float ritualPhase, bool glowPass = false)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("NebulaWispNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = noise;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            }

            Effect shader = ShaderLoader.SummonCircle;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(MoonlightSonataPalette.NebulaPurple.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(MoonlightSonataPalette.StarCore.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(0.3f + ritualPhase * 0.7f);
            shader.Parameters["uOverbrightMult"]?.SetValue(2.5f + ritualPhase * 1.5f);
            shader.Parameters["uScrollSpeed"]?.SetValue(0.6f + ritualPhase * 0.4f);
            shader.Parameters["uDistortionAmt"]?.SetValue(0.04f + ritualPhase * 0.04f);
            shader.Parameters["uPhase"]?.SetValue(MathHelper.Clamp(ritualPhase, 0f, 1f));
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
            shader.Parameters["uNoiseScale"]?.SetValue(4f);

            string technique = glowPass ? "SummonCircleGlow" : "SummonCircleMain";
            shader.CurrentTechnique = shader.Techniques[technique];
            shader.CurrentTechnique.Passes[0].Apply();
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
