using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.ClairDeLune
{
    /// <summary>
    /// Per-weapon shader manager for all Clair de Lune VFX.
    /// Each weapon has its own dedicated shader with unique visual identity.
    /// Wraps ShaderLoader for easy Apply* calls from weapon VFX files.
    ///
    /// Core shaders (shared):
    ///   ClairDeLuneMoonlit   — Moonlit ripple trail (melee/ranged base)
    ///   ClairDeLunePearlGlow — Pearl shimmer aura/bloom (all weapons)
    ///
    /// Per-weapon shaders (unique):
    ///   TemporalDrill        — Chronologicality: time-rending drill trail
    ///   CrystalLance         — TemporalPiercer: crystalline frozen lance
    ///   GearSwing            — ClockworkHarmony: brass pendulum swing
    ///   ArcanePages          — ClockworkGrimoire: turning arcane page trail
    ///   CelestialOrbit       — OrreryOfDreams: orbiting dream sphere trail
    ///   TimeFreezeSlash      — RequiemOfTime: frozen time sweep
    ///   StarfallTrail        — StarfallWhisper: falling star ribbon
    ///   GatlingBlur          — MidnightMechanism: rapid mechanism blur
    ///   SingularityPull      — CogAndHammer: gravitational pull trail
    ///   SoulBeam             — LunarPhylactery: moonlit soul beam aura
    ///   JudgmentMark         — GearDrivenArbiter: clockwork verdict aura
    ///   ResonanceField       — AutomatonsTuningFork: harmonic field aura
    /// </summary>
    public static class ClairDeLuneShaderManager
    {
        // =====================================================================
        //  Shader Availability — Per-Weapon
        // =====================================================================

        public static bool HasMoonlit => ShaderLoader.HasShader(ShaderLoader.ClairDeLuneMoonlitShader);
        public static bool HasPearlGlow => ShaderLoader.HasShader(ShaderLoader.ClairDeLunePearlGlowShader);
        public static bool HasTemporalDrill => ShaderLoader.HasShader(ShaderLoader.TemporalDrillShader);
        public static bool HasCrystalLance => ShaderLoader.HasShader(ShaderLoader.CrystalLanceShader);
        public static bool HasGearSwing => ShaderLoader.HasShader(ShaderLoader.GearSwingShader);
        public static bool HasArcanePages => ShaderLoader.HasShader(ShaderLoader.ArcanePagesShader);
        public static bool HasCelestialOrbit => ShaderLoader.HasShader(ShaderLoader.CelestialOrbitShader);
        public static bool HasTimeFreezeSlash => ShaderLoader.HasShader(ShaderLoader.TimeFreezeSlashShader);
        public static bool HasStarfallTrail => ShaderLoader.HasShader(ShaderLoader.StarfallTrailShader);
        public static bool HasGatlingBlur => ShaderLoader.HasShader(ShaderLoader.GatlingBlurShader);
        public static bool HasSingularityPull => ShaderLoader.HasShader(ShaderLoader.SingularityPullShader);
        public static bool HasSoulBeam => ShaderLoader.HasShader(ShaderLoader.SoulBeamShader);
        public static bool HasJudgmentMark => ShaderLoader.HasShader(ShaderLoader.JudgmentMarkShader);
        public static bool HasResonanceField => ShaderLoader.HasShader(ShaderLoader.ResonanceFieldShader);

        /// <summary>True if any Clair de Lune shader is available.</summary>
        public static bool IsAvailable => HasMoonlit || HasPearlGlow || HasTemporalDrill;

        /// <summary>True if the shared scrolling trail shader is available (fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any trail-capable shader is loaded.</summary>
        public static bool CanRenderTrails => HasTemporalDrill || HasCrystalLance || HasGearSwing ||
            HasTimeFreezeSlash || HasStarfallTrail || HasGatlingBlur || HasMoonlit || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics noise to sampler slot 1 for moonlit ripple distortion.
        /// Primary noise for Clair de Lune — soft, dreamy, water-like caustics.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SimplexNoise to sampler slot 1 for clockwork mechanism patterns.
        /// Crystalline shimmer used by temporal effects and brass mechanism overlays.
        /// </summary>
        public static void BindSimplexNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SimplexNoise")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds TileableMarbleNoise to sampler slot 1 for temporal flow effects.
        /// Veined directional noise ideal for time-freeze and flowing temporal arcs.
        /// </summary>
        public static void BindMarbleNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("TileableMarbleNoise")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        // =====================================================================
        //  Common Uniform Setter
        // =====================================================================

        private static void SetCommonUniforms(Effect shader, float time, Color primary, Color secondary,
            float opacity = 1f, float intensity = 1.5f, float overbrightMult = 2.5f,
            float scrollSpeed = 1f, float distortionAmt = 0.06f,
            float hasSecondaryTex = 1f, float secondaryTexScale = 3f, float secondaryTexScroll = 0.5f)
        {
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uTime"]?.SetValue(time);
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
            shader.Parameters["uScrollSpeed"]?.SetValue(scrollSpeed);
            shader.Parameters["uDistortionAmt"]?.SetValue(distortionAmt);
            shader.Parameters["uHasSecondaryTex"]?.SetValue(hasSecondaryTex);
            shader.Parameters["uSecondaryTexScale"]?.SetValue(secondaryTexScale);
            shader.Parameters["uSecondaryTexScroll"]?.SetValue(secondaryTexScroll);
        }

        private static void SetPhaseUniform(Effect shader, float phase)
        {
            shader.Parameters["uPhase"]?.SetValue(phase);
        }

        // =====================================================================
        //  Generic Apply Methods
        // =====================================================================

        /// <summary>
        /// Generic trail shader application. Used internally by per-weapon Apply methods.
        /// Binds uniforms, selects technique, applies the first pass.
        /// </summary>
        private static void ApplyTrailShader(Effect shader, string techniqueName, float time,
            Color primary, Color secondary, float scrollSpeed, float glowIntensity,
            float distortionAmt = 0.06f, float overbrightMult = 2.5f,
            float secondaryTexScale = 3f, float secondaryTexScroll = 0.5f)
        {
            if (shader == null) return;

            SetCommonUniforms(shader, time, primary, secondary,
                intensity: glowIntensity, overbrightMult: overbrightMult,
                scrollSpeed: scrollSpeed, distortionAmt: distortionAmt,
                secondaryTexScale: secondaryTexScale, secondaryTexScroll: secondaryTexScroll);

            if (shader.Techniques[techniqueName] != null)
                shader.CurrentTechnique = shader.Techniques[techniqueName];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// Generic radial/aura shader application. Used internally by summoner Apply methods.
        /// Binds uniforms with phase for summon intensity, selects technique, applies the first pass.
        /// </summary>
        private static void ApplyRadialShader(Effect shader, string techniqueName, float time,
            Color primary, Color secondary, float summonPhase, float glowIntensity,
            float overbrightMult = 2.5f, float secondaryTexScale = 3f, float secondaryTexScroll = 0.3f)
        {
            if (shader == null) return;

            SetCommonUniforms(shader, time, primary, secondary,
                intensity: glowIntensity, overbrightMult: overbrightMult,
                scrollSpeed: 1f, secondaryTexScale: secondaryTexScale,
                secondaryTexScroll: secondaryTexScroll);
            SetPhaseUniform(shader, summonPhase);

            if (shader.Techniques[techniqueName] != null)
                shader.CurrentTechnique = shader.Techniques[techniqueName];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  FALLBACK: Use shared ScrollingTrailShader
        // =====================================================================

        /// <summary>
        /// Fallback trail rendering using the shared ScrollingTrailShader.
        /// Used when weapon-specific shaders are unavailable.
        /// </summary>
        public static void ApplyFallbackTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ScrollingTrail;
            if (shader == null) shader = ShaderLoader.Trail;
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
        //  MELEE: Chronologicality — TemporalDrill
        // =====================================================================

        /// <summary>Time-rending drill trail for Chronologicality. ClockworkBrass -> PearlWhite, fast 1.8 scroll.</summary>
        public static void ApplyTemporalDrillTrail(float time, float glowIntensity = 1.8f)
        {
            Effect shader = ShaderLoader.TemporalDrill;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.PearlWhite, 1.8f); return; }

            BindSimplexNoise(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "TemporalDrillTrail", time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.PearlWhite,
                scrollSpeed: 1.8f, glowIntensity: glowIntensity,
                distortionAmt: 0.08f, overbrightMult: 3.0f);
        }

        // =====================================================================
        //  MELEE: TemporalPiercer — CrystalLance
        // =====================================================================

        /// <summary>Crystalline frozen lance trail for TemporalPiercer. SoftBlue -> WhiteHot, 1.4 scroll.</summary>
        public static void ApplyCrystalLanceTrail(float time, float glowIntensity = 1.6f)
        {
            Effect shader = ShaderLoader.CrystalLance;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.SoftBlue, ClairDeLunePalette.WhiteHot, 1.4f); return; }

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "CrystalLanceTrail", time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.WhiteHot,
                scrollSpeed: 1.4f, glowIntensity: glowIntensity,
                distortionAmt: 0.04f, overbrightMult: 3.0f);
        }

        // =====================================================================
        //  MELEE: ClockworkHarmony — GearSwing
        // =====================================================================

        /// <summary>Brass pendulum swing trail for ClockworkHarmony. ClockworkBrass -> MoonbeamGold, steady 1.0 scroll.</summary>
        public static void ApplyGearSwingTrail(float time, float glowIntensity = 1.5f)
        {
            Effect shader = ShaderLoader.GearSwing;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, 1.0f); return; }

            BindSimplexNoise(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "GearSwingTrail", time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                scrollSpeed: 1.0f, glowIntensity: glowIntensity,
                distortionAmt: 0.05f, overbrightMult: 2.8f);
        }

        // =====================================================================
        //  MELEE: RequiemOfTime — TimeFreezeSlash
        // =====================================================================

        /// <summary>Frozen time sweep trail for RequiemOfTime. MidnightBlue -> PearlWhite, slow 0.6 scroll.</summary>
        public static void ApplyTimeFreezeSlashTrail(float time, float glowIntensity = 1.6f)
        {
            Effect shader = ShaderLoader.TimeFreezeSlash;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlWhite, 0.6f); return; }

            BindMarbleNoise(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "TimeFreezeSlashTrail", time,
                ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlWhite,
                scrollSpeed: 0.6f, glowIntensity: glowIntensity,
                distortionAmt: 0.10f, overbrightMult: 3.2f);
        }

        // =====================================================================
        //  RANGED: StarfallWhisper — StarfallTrail
        // =====================================================================

        /// <summary>Falling star ribbon trail for StarfallWhisper. SoftBlue -> PearlBlue, 1.5 scroll.</summary>
        public static void ApplyStarfallTrail(float time, float glowIntensity = 1.6f)
        {
            Effect shader = ShaderLoader.StarfallTrail;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlBlue, 1.5f); return; }

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "StarfallTrail", time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlBlue,
                scrollSpeed: 1.5f, glowIntensity: glowIntensity,
                distortionAmt: 0.05f, overbrightMult: 2.8f);
        }

        // =====================================================================
        //  RANGED: MidnightMechanism — GatlingBlur
        // =====================================================================

        /// <summary>Rapid mechanism blur trail for MidnightMechanism. StarlightSilver -> WhiteHot, fast 2.0 scroll.</summary>
        public static void ApplyGatlingBlurTrail(float time, float glowIntensity = 1.8f)
        {
            Effect shader = ShaderLoader.GatlingBlur;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.WhiteHot, 2.0f); return; }

            BindSimplexNoise(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "GatlingBlurTrail", time,
                ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.WhiteHot,
                scrollSpeed: 2.0f, glowIntensity: glowIntensity,
                distortionAmt: 0.03f, overbrightMult: 3.0f);
        }

        // =====================================================================
        //  RANGED: CogAndHammer — SingularityPull
        // =====================================================================

        /// <summary>Gravitational pull trail for CogAndHammer. ClockworkBrass -> PearlBlue, heavy 0.8 scroll.</summary>
        public static void ApplySingularityPullTrail(float time, float glowIntensity = 1.5f)
        {
            Effect shader = ShaderLoader.SingularityPull;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.PearlBlue, 0.8f); return; }

            BindMarbleNoise(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "SingularityPullTrail", time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.PearlBlue,
                scrollSpeed: 0.8f, glowIntensity: glowIntensity,
                distortionAmt: 0.09f, overbrightMult: 2.8f);
        }

        // =====================================================================
        //  MAGIC: ClockworkGrimoire — ArcanePages
        // =====================================================================

        /// <summary>Arcane page turning trail for ClockworkGrimoire. NightMist -> MoonbeamGold, gentle 0.9 scroll.</summary>
        public static void ApplyArcanePagesTrail(float time, float glowIntensity = 1.5f)
        {
            Effect shader = ShaderLoader.ArcanePages;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.NightMist, ClairDeLunePalette.MoonbeamGold, 0.9f); return; }

            BindMarbleNoise(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "ArcanePagesTrail", time,
                ClairDeLunePalette.NightMist, ClairDeLunePalette.MoonbeamGold,
                scrollSpeed: 0.9f, glowIntensity: glowIntensity,
                distortionAmt: 0.06f, overbrightMult: 2.6f);
        }

        // =====================================================================
        //  MAGIC: OrreryOfDreams — CelestialOrbit
        // =====================================================================

        /// <summary>Orbiting dream sphere trail for OrreryOfDreams. SoftBlue -> PearlWhite, dreamy 0.7 scroll.</summary>
        public static void ApplyCelestialOrbitTrail(float time, float glowIntensity = 1.4f)
        {
            Effect shader = ShaderLoader.CelestialOrbit;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, 0.7f); return; }

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "CelestialOrbitTrail", time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                scrollSpeed: 0.7f, glowIntensity: glowIntensity,
                distortionAmt: 0.07f, overbrightMult: 2.5f);
        }

        // =====================================================================
        //  SUMMONER: LunarPhylactery — SoulBeam
        // =====================================================================

        /// <summary>Moonlit soul beam aura for LunarPhylactery. MidnightBlue -> PearlBlue.</summary>
        public static void ApplySoulBeamAura(float time, float summonPhase = 1f, float glowIntensity = 1.5f)
        {
            Effect shader = ShaderLoader.SoulBeam;
            if (shader == null) return;

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyRadialShader(shader, "SoulBeamAura", time,
                ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlBlue,
                summonPhase: summonPhase, glowIntensity: glowIntensity,
                overbrightMult: 2.5f);
        }

        // =====================================================================
        //  SUMMONER: GearDrivenArbiter — JudgmentMark
        // =====================================================================

        /// <summary>Clockwork verdict aura for GearDrivenArbiter. ClockworkBrass -> WhiteHot.</summary>
        public static void ApplyJudgmentMarkAura(float time, float summonPhase = 1f, float glowIntensity = 1.6f)
        {
            Effect shader = ShaderLoader.JudgmentMark;
            if (shader == null) return;

            BindSimplexNoise(Main.graphics.GraphicsDevice);
            ApplyRadialShader(shader, "JudgmentMarkAura", time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.WhiteHot,
                summonPhase: summonPhase, glowIntensity: glowIntensity,
                overbrightMult: 3.0f);
        }

        // =====================================================================
        //  SUMMONER: AutomatonsTuningFork — ResonanceField
        // =====================================================================

        /// <summary>Harmonic resonance field aura for AutomatonsTuningFork. SoftBlue -> MoonbeamGold.</summary>
        public static void ApplyResonanceFieldAura(float time, float summonPhase = 1f, float glowIntensity = 1.4f)
        {
            Effect shader = ShaderLoader.ResonanceField;
            if (shader == null) return;

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyRadialShader(shader, "ResonanceFieldAura", time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonbeamGold,
                summonPhase: summonPhase, glowIntensity: glowIntensity,
                overbrightMult: 2.5f);
        }

        // =====================================================================
        //  SHARED: ClairDeLuneMoonlit — Moonlit Trail
        // =====================================================================

        /// <summary>Shared moonlit ripple trail for Clair de Lune theme. MidnightBlue -> PearlBlue.</summary>
        public static void ApplyMoonlitTrail(float time, float glowIntensity = 1.4f)
        {
            Effect shader = ShaderLoader.ClairDeLuneMoonlit;
            if (shader == null) { ApplyFallbackTrail(time, ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlBlue, 1.0f); return; }

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyTrailShader(shader, "ClairDeLuneMoonlitTrail", time,
                ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlBlue,
                scrollSpeed: 1.0f, glowIntensity: glowIntensity,
                distortionAmt: 0.06f, overbrightMult: 2.5f);
        }

        // =====================================================================
        //  SHARED: ClairDeLunePearlGlow — Pearl Glow Aura
        // =====================================================================

        /// <summary>Shared pearl shimmer aura for Clair de Lune theme. SoftBlue -> PearlWhite, phase-driven bloom.</summary>
        public static void ApplyPearlGlowAura(float time, float age = 1f, float glowIntensity = 1.3f)
        {
            Effect shader = ShaderLoader.ClairDeLunePearlGlow;
            if (shader == null) return;

            BindNoiseTexture(Main.graphics.GraphicsDevice);
            ApplyRadialShader(shader, "ClairDeLunePearlGlowAura", time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                summonPhase: age, glowIntensity: glowIntensity,
                overbrightMult: 2.2f);
        }

        // =====================================================================
        //  SpriteBatch State Helpers
        // =====================================================================

        /// <summary>Begins SpriteBatch in Immediate + Additive mode for shader drawing.</summary>
        public static void BeginShaderAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Restores SpriteBatch to normal deferred alpha-blend mode.</summary>
        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>Begins SpriteBatch in Deferred + Additive mode (no shader, for bloom stacking).</summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
