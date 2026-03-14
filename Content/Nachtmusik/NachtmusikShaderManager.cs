using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Nachtmusik
{
    /// <summary>
    /// Compartmentalized shader manager for all Nachtmusik weapon VFX.
    /// Provides availability checks, noise texture binding, generic Apply methods,
    /// and weapon-specific presets for each Nachtmusik shader.
    ///
    /// Shared theme-wide shaders:
    ///   NachtmusikStarTrail, NachtmusikSerenade
    ///
    /// Fully-implemented weapons (presets NOT provided here — see weapon files):
    ///   NocturnalExecutioner:  ExecutionDecree
    ///   MidnightsCrescendo:   CrescendoRise
    ///   TwilightSeverance:    DimensionalRift
    ///   ConstellationPiercer: StarChainBeam
    ///
    /// Gutted weapons (presets provided here for VFX restoration):
    ///   NebulasWhisper:              NebulaScatter (trail)
    ///   SerenadeOfDistantStars:      StarHomingTrail (trail)
    ///   StarweaversGrimoire:         ConstellationWeave (trail)
    ///   RequiemOfTheCosmos:          CosmicRequiem (trail)
    ///   CelestialChorusBaton:        ChorusSummonAura (radial)
    ///   GalacticOverture:            OvertureAura (radial)
    ///   ConductorOfConstellations:   StellarConductorAura (radial)
    ///
    /// Usage (in PreDraw):
    ///   NachtmusikShaderManager.BeginShaderAdditive(sb);
    ///   NachtmusikShaderManager.BindNoiseTexture(device);
    ///   NachtmusikShaderManager.ApplyNebulaScatterTrail(time, glow: false);
    ///   // ... draw trail geometry ...
    ///   NachtmusikShaderManager.RestoreSpriteBatch(sb);
    ///
    /// All Apply* methods gracefully return false if the shader is null,
    /// allowing VFX code to fall back to particle-based rendering.
    /// </summary>
    public static class NachtmusikShaderManager
    {
        // =====================================================================
        //  Shader Availability — Theme-Wide Shared
        // =====================================================================

        public static bool HasStarTrail => ShaderLoader.HasShader(ShaderLoader.NachtmusikStarTrailShader);
        public static bool HasSerenade => ShaderLoader.HasShader(ShaderLoader.NachtmusikSerenadeShader);

        // =====================================================================
        //  Shader Availability — Fully Implemented Weapons (no presets here)
        // =====================================================================

        // NocturnalExecutioner
        public static bool HasExecutionDecree => ShaderLoader.HasShader(ShaderLoader.ExecutionDecreeShader);

        // MidnightsCrescendo
        public static bool HasCrescendoRise => ShaderLoader.HasShader(ShaderLoader.CrescendoRiseShader);

        // TwilightSeverance
        public static bool HasDimensionalRift => ShaderLoader.HasShader(ShaderLoader.DimensionalRiftShader);

        // ConstellationPiercer
        public static bool HasStarChainBeam => ShaderLoader.HasShader(ShaderLoader.StarChainBeamShader);

        // =====================================================================
        //  Shader Availability — Gutted Weapons (presets below)
        // =====================================================================

        // NebulasWhisper
        public static bool HasNebulaScatter => ShaderLoader.HasShader(ShaderLoader.NebulaScatterShader);

        // SerenadeOfDistantStars
        public static bool HasStarHomingTrail => ShaderLoader.HasShader(ShaderLoader.StarHomingTrailShader);

        // StarweaversGrimoire
        public static bool HasConstellationWeave => ShaderLoader.HasShader(ShaderLoader.ConstellationWeaveShader);

        // RequiemOfTheCosmos
        public static bool HasCosmicRequiem => ShaderLoader.HasShader(ShaderLoader.CosmicRequiemShader);

        // CelestialChorusBaton
        public static bool HasChorusSummonAura => ShaderLoader.HasShader(ShaderLoader.ChorusSummonAuraShader);

        // GalacticOverture
        public static bool HasOvertureAura => ShaderLoader.HasShader(ShaderLoader.OvertureAuraShader);

        // ConductorOfConstellations
        public static bool HasStellarConductorAura => ShaderLoader.HasShader(ShaderLoader.StellarConductorAuraShader);

        /// <summary>True if any Nachtmusik shader is available.</summary>
        public static bool IsAvailable =>
            HasStarTrail || HasSerenade ||
            HasExecutionDecree || HasCrescendoRise ||
            HasDimensionalRift || HasStarChainBeam ||
            HasNebulaScatter || HasStarHomingTrail ||
            HasConstellationWeave || HasCosmicRequiem ||
            HasChorusSummonAura || HasOvertureAura ||
            HasStellarConductorAura;

        /// <summary>True if any trail shader is usable (dedicated or shared fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);
        public static bool CanRenderTrails => HasNebulaScatter || HasStarHomingTrail || HasStarTrail || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds CosmicEnergyVortex to sampler slot 1 — Nachtmusik's primary noise.
        /// Swirling cosmic vortex ideal for stellar trails and nebula effects.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SimplexNoise to sampler slot 1 for weaving constellation patterns.
        /// Used by StarweaversGrimoire and intricate star-thread effects.
        /// </summary>
        public static void BindSimplexNoise(GraphicsDevice device)
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

        /// <summary>
        /// Binds RealityCrackPattern to sampler slot 1 for dimensional rift shattering.
        /// Used by void-piercing and reality-tearing effects.
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
        /// Apply a radial/aura shader with standard uniforms. Returns true if shader was applied.
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
        //  WEAPON PRESETS — NebulasWhisper (Ranged — Nebula Scatter Gun)
        // =====================================================================

        /// <summary>
        /// Apply NebulaScatter trail for NebulasWhisper projectiles.
        /// Soft cosmic whisper through nebula mist — gentle purple-pink dissipating trails.
        /// Colors: NebulasWhisperShot palette (deep nebula -> nebula pink -> whisper white).
        /// </summary>
        public static bool ApplyNebulaScatterTrail(float time, bool glow = false)
        {
            string technique = glow ? "NebulaScatterGlow" : "NebulaScatterMain";
            if (HasNebulaScatter)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.NebulaScatterShader, technique,
                    time, NachtmusikPalette.NebulaPink, NachtmusikPalette.SerenadeGlow,
                    scrollSpeed: 1.0f, distortionAmt: 0.06f, overbrightMult: 2.5f,
                    noiseScale: 3.5f, noiseScroll: 0.4f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, NachtmusikPalette.NebulaPink, NachtmusikPalette.SerenadeGlow,
                scrollSpeed: 1.0f, overbrightMult: 2.5f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — SerenadeOfDistantStars (Ranged — Homing Bow)
        // =====================================================================

        /// <summary>
        /// Apply StarHomingTrail for SerenadeOfDistantStars homing star projectiles.
        /// Warm starlight melody through night sky — romantic sweeping trails.
        /// Colors: SerenadeOfDistantStarsShot palette (midnight -> starlit blue -> warm starlight).
        /// </summary>
        public static bool ApplyStarHomingTrail(float time, bool glow = false)
        {
            string technique = glow ? "StarHomingGlow" : "StarHomingMain";
            if (HasStarHomingTrail)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.StarHomingTrailShader, technique,
                    time, NachtmusikPalette.StarlitBlue, NachtmusikPalette.MoonlitSilver,
                    scrollSpeed: 1.4f, distortionAmt: 0.05f, overbrightMult: 2.8f,
                    noiseScale: 3f, noiseScroll: 0.6f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, NachtmusikPalette.StarlitBlue, NachtmusikPalette.MoonlitSilver,
                scrollSpeed: 1.4f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — StarweaversGrimoire (Magic — Star-Weaving Tome)
        // =====================================================================

        /// <summary>
        /// Apply ConstellationWeave trail for StarweaversGrimoire projectile orbits.
        /// Intricate star-weaving through arcane night — violet threads binding stars.
        /// Colors: StarweaversGrimoireCast palette (arcane void -> violet thread -> star pattern white).
        /// </summary>
        public static bool ApplyConstellationWeaveTrail(float time, bool glow = false)
        {
            string technique = glow ? "ConstellationWeaveGlow" : "ConstellationWeaveMain";
            if (HasConstellationWeave)
            {
                BindSimplexNoise(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.ConstellationWeaveShader, technique,
                    time, NachtmusikPalette.Violet, NachtmusikPalette.SerenadeGlow,
                    scrollSpeed: 0.9f, distortionAmt: 0.10f, overbrightMult: 2.6f,
                    noiseScale: 2.5f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, NachtmusikPalette.Violet, NachtmusikPalette.SerenadeGlow,
                scrollSpeed: 0.9f, overbrightMult: 2.6f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — RequiemOfTheCosmos (Magic — Cosmic Finale Staff)
        // =====================================================================

        /// <summary>
        /// Apply CosmicRequiem trail for RequiemOfTheCosmos channeled projectiles.
        /// Somber cosmic finale from void to supernova — grand echoing trails.
        /// Colors: RequiemOfTheCosmosCast palette (cosmic void -> radiance gold -> cosmic white).
        /// </summary>
        public static bool ApplyCosmicRequiemTrail(float time, bool glow = false)
        {
            string technique = glow ? "CosmicRequiemGlow" : "CosmicRequiemMain";
            if (HasCosmicRequiem)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.CosmicRequiemShader, technique,
                    time, NachtmusikPalette.StarlitBlue, NachtmusikPalette.RadianceGold,
                    scrollSpeed: 1.1f, distortionAmt: 0.08f, overbrightMult: 3.0f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, NachtmusikPalette.StarlitBlue, NachtmusikPalette.RadianceGold,
                scrollSpeed: 1.1f, overbrightMult: 3.0f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — CelestialChorusBaton (Summoner — Choral Baton)
        // =====================================================================

        /// <summary>
        /// Apply ChorusSummonAura for CelestialChorusBaton minion formation aura.
        /// Graceful celestial harmony — the baton commands celestial voices.
        /// Colors: CelestialChorusMinion palette (night void -> violet harmony -> chorus white).
        /// </summary>
        public static bool ApplyChorusSummonAura(float time, float activeSummons = 1f, bool glow = false)
        {
            string technique = glow ? "ChorusAuraGlow" : "ChorusAuraMain";
            if (HasChorusSummonAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.ChorusSummonAuraShader, technique,
                    time, NachtmusikPalette.Violet, NachtmusikPalette.StarWhite,
                    activeSummons, intensity: 1.0f, overbrightMult: 2.2f,
                    noiseScale: 2.5f, noiseScroll: 0.3f, hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — GalacticOverture (Summoner — Grand Opening)
        // =====================================================================

        /// <summary>
        /// Apply OvertureAura for GalacticOverture minion formation aura.
        /// Sweeping dramatic overture — the grand opening that announces the queen.
        /// Colors: GalacticOvertureMinion palette (galactic void -> radiance gold -> overture white).
        /// </summary>
        public static bool ApplyOvertureAura(float time, float activeSummons = 1f, bool glow = false)
        {
            string technique = glow ? "OvertureAuraGlow" : "OvertureAuraMain";
            if (HasOvertureAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.OvertureAuraShader, technique,
                    time, NachtmusikPalette.StarlitBlue, NachtmusikPalette.RadianceGold,
                    activeSummons, intensity: 1.2f, overbrightMult: 2.5f,
                    noiseScale: 3f, noiseScroll: 0.35f, hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — ConductorOfConstellations (Summoner — Star Commander)
        // =====================================================================

        /// <summary>
        /// Apply StellarConductorAura for ConductorOfConstellations minion formation aura.
        /// Precise commanding authority — stars bow to the conductor's will.
        /// Colors: ConductorOfConstellationsMinion palette (midnight void -> star gold baton -> conductor's white).
        /// </summary>
        public static bool ApplyStellarConductorAura(float time, float activeSummons = 1f, bool glow = false)
        {
            string technique = glow ? "StellarConductorGlow" : "StellarConductorMain";
            if (HasStellarConductorAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.StellarConductorAuraShader, technique,
                    time, NachtmusikPalette.ConstellationBlue, NachtmusikPalette.StarGold,
                    activeSummons, intensity: 1.1f, overbrightMult: 2.4f,
                    noiseScale: 2.5f, noiseScroll: 0.3f, hasNoiseBound: true);
            }
            return false;
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
