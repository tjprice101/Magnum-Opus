using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.ClairDeLune
{
    /// <summary>
    /// Compartmentalized shader manager for all Clair de Lune VFX.
    /// Wraps ShaderLoader access to HeroicFlameTrail.fx, RadialScrollShader.fx,
    /// and ScrollingTrailShader.fx — providing preset Apply* methods for each
    /// weapon's moonlit / dreamy / clockwork rendering technique.
    ///
    /// Techniques available:
    ///   HeroicFlameTrail  — HeroicFlameFlow: moonlit dreamy trail with pearl distortion
    ///   RadialScrollShader — temporal shockwave / expanding pearl rings
    ///   ScrollingTrailShader — blue-pearl scrolling trail for general weapon use
    ///
    /// Usage (in PreDraw):
    ///   ClairDeLuneShaderManager.BindMistNoiseTexture(device);
    ///   ClairDeLuneShaderManager.ApplyMoonlitTrail(time, primary, secondary);
    ///   // ... draw trail geometry ...
    ///   ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class ClairDeLuneShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        /// <summary>True if the HeroicFlameTrail shader loaded successfully.</summary>
        public static bool HasFlameTrail => ShaderLoader.HasShader(ShaderLoader.HeroicFlameTrailShader);

        /// <summary>True if the RadialScroll shader loaded successfully.</summary>
        public static bool HasRadialScroll => ShaderLoader.HasShader(ShaderLoader.RadialScrollShaderName);

        /// <summary>True if the ScrollingTrail shader loaded successfully.</summary>
        public static bool HasScrollingTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any Clair de Lune shader is available.</summary>
        public static bool IsAvailable => HasFlameTrail || HasRadialScroll || HasScrollingTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics texture to sampler slot 1 for dreamy mist distortion.
        /// Preferred noise for Clair de Lune — softer, more impressionistic than NoiseSmoke.
        /// Falls back to NoiseSmoke if unavailable.
        /// </summary>
        public static void BindMistNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 for temporal vortex effects.
        /// Used by clockwork weapons and temporal distortion VFX.
        /// </summary>
        public static void BindVortexNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  TECHNIQUE: HeroicFlameFlow (HeroicFlameTrail.fx)
        // =====================================================================

        /// <summary>
        /// Configure and apply HeroicFlameTrail.fx for moonlit dreamy trail rendering.
        /// Uses flowing blue-to-pearl gradient with soft mist noise distortion.
        /// </summary>
        public static void ApplyMoonlitTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.05f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.HeroicFlameTrail;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(1.2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(3f);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Configure HeroicFlameTrail.fx with mist noise texture on sampler 1
        /// for richer dreamy distortion effects.
        /// </summary>
        public static void ApplyMoonlitTrailWithNoise(float time, Color primary, Color secondary,
            float scrollSpeed = 1.0f, float distortionAmt = 0.05f, float overbrightMult = 2.5f,
            float noiseScale = 3f, float noiseScroll = 0.4f)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("NoiseSmoke");
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
            shader.Parameters["uIntensity"]?.SetValue(1.2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(noiseScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(noiseScroll);

            shader.CurrentTechnique = shader.Techniques["HeroicFlameFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  WEAPON PRESETS — MELEE
        // =====================================================================

        /// <summary>
        /// Preset: Chronologicality trail — temporal drill ripping through reality.
        /// Crimson-shifted with aggressive scroll speed and vortex distortion.
        /// </summary>
        public static void ApplyChronologicalityTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlWhite,
                scrollSpeed: 2.2f, distortionAmt: 0.10f, overbrightMult: 3.2f,
                noiseScale: 2.5f, noiseScroll: 0.7f);
        }

        /// <summary>
        /// Preset: TemporalPiercer trail — crystalline lance of frozen time.
        /// Precise, sharp, frosted blue-to-white gradient.
        /// </summary>
        public static void ApplyTemporalPiercerTrail(float time)
        {
            ApplyMoonlitTrail(time,
                ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.WhiteHot,
                scrollSpeed: 2.8f, distortionAmt: 0.03f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: ClockworkHarmony trail — brass mechanism meets moonlit steel.
        /// Heavy, rhythmic, clockwork brass with soft blue highlights.
        /// </summary>
        public static void ApplyClockworkHarmonyTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.PearlBlue,
                scrollSpeed: 1.0f, distortionAmt: 0.08f, overbrightMult: 2.8f,
                noiseScale: 3f, noiseScroll: 0.5f);
        }

        // =====================================================================
        //  WEAPON PRESETS — RANGED
        // =====================================================================

        /// <summary>
        /// Preset: StarfallWhisper trail — precise sniper shot tracing starlit paths.
        /// Clean, narrow, starlight silver to pearl white.
        /// </summary>
        public static void ApplyStarfallWhisperTrail(float time)
        {
            ApplyMoonlitTrail(time,
                ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.WhiteHot,
                scrollSpeed: 3.0f, distortionAmt: 0.02f, overbrightMult: 3.5f);
        }

        /// <summary>
        /// Preset: MidnightMechanism trail — rapid-fire clockwork gatling bolts.
        /// Brass-gold with fast scroll for mechanical feel.
        /// </summary>
        public static void ApplyMidnightMechanismTrail(float time)
        {
            ApplyMoonlitTrail(time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                scrollSpeed: 2.5f, distortionAmt: 0.04f, overbrightMult: 2.8f);
        }

        /// <summary>
        /// Preset: CogAndHammer trail — explosive clockwork bomb with heavy distortion.
        /// Deep night to brass with volcanic-style distortion.
        /// </summary>
        public static void ApplyCogAndHammerTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.NightMist, ClairDeLunePalette.ClockworkBrass,
                scrollSpeed: 0.8f, distortionAmt: 0.12f, overbrightMult: 3.0f,
                noiseScale: 2f, noiseScroll: 0.5f);
        }

        // =====================================================================
        //  WEAPON PRESETS — MAGIC
        // =====================================================================

        /// <summary>
        /// Preset: ClockworkGrimoire trail — arcane pages of temporal knowledge.
        /// Soft blue to pearl with moderate dreamy distortion.
        /// </summary>
        public static void ApplyClockworkGrimoireTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                scrollSpeed: 1.5f, distortionAmt: 0.06f, overbrightMult: 2.8f,
                noiseScale: 3f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: OrreryOfDreams trail — celestial spheres in dreamy orbit.
        /// Dream haze to starlight silver with cosmic scroll.
        /// </summary>
        public static void ApplyOrreryOfDreamsTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.DreamHaze, ClairDeLunePalette.StarlightSilver,
                scrollSpeed: 1.2f, distortionAmt: 0.07f, overbrightMult: 3.0f,
                noiseScale: 2.5f, noiseScroll: 0.6f);
        }

        /// <summary>
        /// Preset: RequiemOfTime trail — time-freeze magic sword.
        /// Temporal crimson to pearl white with sharp, sweeping scroll.
        /// </summary>
        public static void ApplyRequiemOfTimeTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlBlue,
                scrollSpeed: 1.8f, distortionAmt: 0.09f, overbrightMult: 3.2f,
                noiseScale: 2.5f, noiseScroll: 0.5f);
        }

        // =====================================================================
        //  WEAPON PRESETS — SUMMON
        // =====================================================================

        /// <summary>
        /// Preset: LunarPhylactery minion trail — dreamy soul vessel channeling moonlight.
        /// Midnight blue to pearl blue with ethereal distortion.
        /// </summary>
        public static void ApplyLunarPhylacteryTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlBlue,
                scrollSpeed: 0.8f, distortionAmt: 0.06f, overbrightMult: 2.5f,
                noiseScale: 3f, noiseScroll: 0.4f);
        }

        /// <summary>
        /// Preset: GearDrivenArbiter minion trail — clockwork judge with brass judgment.
        /// Brass to moonbeam gold with mechanical precision.
        /// </summary>
        public static void ApplyGearDrivenArbiterTrail(float time)
        {
            ApplyMoonlitTrail(time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                scrollSpeed: 2.0f, distortionAmt: 0.05f, overbrightMult: 3.0f);
        }

        /// <summary>
        /// Preset: AutomatonsTuningFork minion trail — resonant support field.
        /// Dream haze to pearl shimmer with harmonic oscillation.
        /// </summary>
        public static void ApplyAutomatonsTuningForkTrail(float time)
        {
            ApplyMoonlitTrailWithNoise(time,
                ClairDeLunePalette.DreamHaze, ClairDeLunePalette.PearlShimmer,
                scrollSpeed: 1.0f, distortionAmt: 0.05f, overbrightMult: 2.5f);
        }

        // =====================================================================
        //  BOSS / SPECIAL PRESETS
        // =====================================================================

        /// <summary>
        /// Preset: Boss temporal shockwave — massive expanding ring with intense moonlit bloom.
        /// </summary>
        public static void ApplyTemporalShockwave(float time)
        {
            Effect shader = ShaderLoader.RadialScroll;
            if (shader == null) return;

            shader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.WhiteHot.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(1f);
            shader.Parameters["uIntensity"]?.SetValue(2f);
            shader.Parameters["uOverbrightMult"]?.SetValue(4f);
            shader.Parameters["uScrollSpeed"]?.SetValue(3f);

            shader.CurrentTechnique.Passes[0].Apply();
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
