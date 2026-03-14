using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Compartmentalized shader manager for all Dies Irae weapon VFX.
    /// Provides availability checks, noise texture binding, generic Apply methods,
    /// and weapon-specific presets for each Dies Irae shader.
    ///
    /// Shared theme-wide shaders:
    ///   HellfireBloom, JudgmentAura
    ///
    /// Weapon-dedicated shaders:
    ///   WrathsCleaver:       InfernoTrail, WrathCleaverSlash
    ///   ExecutionersVerdict: GuillotineBlade
    ///   DeathTollingBell:    BellToll
    ///   EclipseOfWrath:      EclipseOrb
    ///
    /// Weapons using shared theme shaders (presets provided here):
    ///   ChainOfJudgment:          JudgmentAura fallback (trail)
    ///   ArbitersSentence:         HellfireBloom fallback (trail)
    ///   SinCollector:             HellfireBloom fallback (trail)
    ///   DamnationsCannon:         HellfireBloom fallback (trail)
    ///   GrimoireOfCondemnation:   JudgmentAura fallback (trail)
    ///   StaffOfFinalJudgement:    JudgmentAura fallback (trail)
    ///   HarmonyOfJudgement:       JudgmentAura fallback (radial)
    ///   WrathfulContract:         HellfireBloom fallback (radial)
    ///
    /// 12 Dies Irae weapons supported (4 melee, 2 ranged, 3 magic, 3 summoner):
    ///   Melee:    WrathsCleaver, ExecutionersVerdict, ChainOfJudgment, ArbitersSentence
    ///   Ranged:   SinCollector, DamnationsCannon
    ///   Magic:    GrimoireOfCondemnation, StaffOfFinalJudgement, EclipseOfWrath
    ///   Summoner: DeathTollingBell, HarmonyOfJudgement, WrathfulContract
    ///
    /// Usage (in PreDraw):
    ///   DiesIraeShaderManager.BeginShaderAdditive(sb);
    ///   DiesIraeShaderManager.BindNoiseTexture(device);
    ///   DiesIraeShaderManager.ApplyInfernoTrail(time, glow: false);
    ///   // ... draw trail geometry ...
    ///   DiesIraeShaderManager.RestoreSpriteBatch(sb);
    ///
    /// All Apply* methods gracefully return false if the shader is null,
    /// allowing VFX code to fall back to particle-based rendering.
    /// </summary>
    public static class DiesIraeShaderManager
    {
        // =====================================================================
        //  Shader Availability — Shared Theme-Wide
        // =====================================================================

        public static bool HasHellfireBloom => ShaderLoader.HasShader(ShaderLoader.HellfireBloomShader);
        public static bool HasJudgmentAura => ShaderLoader.HasShader(ShaderLoader.JudgmentAuraShader);

        // =====================================================================
        //  Shader Availability — Weapon-Dedicated
        // =====================================================================

        // WrathsCleaver
        public static bool HasInfernoTrail => ShaderLoader.HasShader(ShaderLoader.InfernoTrailShader);
        public static bool HasWrathCleaverSlash => ShaderLoader.HasShader(ShaderLoader.WrathCleaverSlashShader);

        // ExecutionersVerdict
        public static bool HasGuillotineBlade => ShaderLoader.HasShader(ShaderLoader.GuillotineBladeShader);

        // DeathTollingBell
        public static bool HasBellToll => ShaderLoader.HasShader(ShaderLoader.BellTollShader);

        // EclipseOfWrath
        public static bool HasEclipseOrb => ShaderLoader.HasShader(ShaderLoader.EclipseOrbShader);

        /// <summary>True if any Dies Irae shader is available.</summary>
        public static bool IsAvailable =>
            HasHellfireBloom || HasJudgmentAura ||
            HasInfernoTrail || HasWrathCleaverSlash ||
            HasGuillotineBlade || HasBellToll || HasEclipseOrb;

        /// <summary>True if any trail shader is usable (dedicated or shared fallback).</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);
        public static bool CanRenderTrails => HasInfernoTrail || HasWrathCleaverSlash || HasGuillotineBlade || HasHellfireBloom || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds TileableFBMNoise to sampler slot 1 — Dies Irae's primary noise.
        /// Fiery turbulence ideal for hellfire trails and infernal distortion.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
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

        /// <summary>
        /// Binds TileableMarbleNoise to sampler slot 1 for blood/lava flow patterns.
        /// Used by ChainOfJudgment and GrimoireOfCondemnation for veined, directional flow.
        /// </summary>
        public static void BindMarbleNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("TileableMarbleNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds VoronoiNoise to sampler slot 1 for cracked/fractured effects.
        /// Used by DamnationsCannon for explosive fragmentation patterns.
        /// </summary>
        public static void BindVoronoiNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("VoronoiNoise");
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
        //  WEAPON PRESETS — WrathsCleaver (Melee — Infernal Greatsword)
        // =====================================================================

        /// <summary>
        /// Apply InfernoTrail for WrathsCleaver projectile/swing trails.
        /// FBM noise-driven fiery turbulence — blood red ascending to hellfire gold.
        /// Colors: BloodRed -> HellfireGold, scroll 1.2, distortion 0.08, overbright 3.0.
        /// </summary>
        public static bool ApplyInfernoTrail(float time, bool glow = false)
        {
            string technique = glow ? "InfernoTrailGlow" : "InfernoTrailMain";
            if (HasInfernoTrail)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.InfernoTrailShader, technique,
                    time, DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                    scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 3.0f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                    scrollSpeed: 1.2f, distortionAmt: 0.08f, overbrightMult: 3.0f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                scrollSpeed: 1.2f, overbrightMult: 3.0f);
            return HasFallbackTrail;
        }

        /// <summary>
        /// Apply WrathCleaverSlash for WrathsCleaver melee swing arc.
        /// Uses the WrathsCleaverBlade palette with fiery scroll — every swing is a sentence.
        /// Colors: WrathsCleaverBlade palette, scroll 1.5.
        /// </summary>
        public static bool ApplyWrathCleaverSlash(float time, float comboPhase = 0f, bool glow = false)
        {
            string technique = glow ? "WrathCleaverSlashGlow" : "WrathCleaverSlashMain";
            if (HasWrathCleaverSlash)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                Color primary = DiesIraePalette.PaletteLerp(DiesIraePalette.WrathsCleaverBlade, 0.3f);
                Color secondary = DiesIraePalette.PaletteLerp(DiesIraePalette.WrathsCleaverBlade, 0.8f);
                return ApplyTrailShader(ShaderLoader.WrathCleaverSlashShader, technique,
                    time, primary, secondary,
                    scrollSpeed: 1.5f, distortionAmt: 0.10f, overbrightMult: 3.2f,
                    phase: comboPhase, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                Color primary = DiesIraePalette.PaletteLerp(DiesIraePalette.WrathsCleaverBlade, 0.3f);
                Color secondary = DiesIraePalette.PaletteLerp(DiesIraePalette.WrathsCleaverBlade, 0.8f);
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, primary, secondary,
                    scrollSpeed: 1.5f, distortionAmt: 0.10f, overbrightMult: 3.2f,
                    phase: comboPhase, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.InfernalRed, DiesIraePalette.HellfireGold,
                scrollSpeed: 1.5f, overbrightMult: 3.2f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — ExecutionersVerdict (Melee — Guillotine Blade)
        // =====================================================================

        /// <summary>
        /// Apply GuillotineBlade trail for ExecutionersVerdict.
        /// FBM-driven descending blade — from dark blood to bone white verdict.
        /// Colors: DarkBlood -> BoneWhite, scroll 1.3, overbright 2.8.
        /// </summary>
        public static bool ApplyGuillotineBladeTrail(float time, bool glow = false)
        {
            string technique = glow ? "GuillotineBladeGlow" : "GuillotineBladeMain";
            if (HasGuillotineBlade)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.GuillotineBladeShader, technique,
                    time, DiesIraePalette.DarkBlood, DiesIraePalette.BoneWhite,
                    scrollSpeed: 1.3f, distortionAmt: 0.07f, overbrightMult: 2.8f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, DiesIraePalette.DarkBlood, DiesIraePalette.BoneWhite,
                    scrollSpeed: 1.3f, distortionAmt: 0.07f, overbrightMult: 2.8f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.DarkBlood, DiesIraePalette.BoneWhite,
                scrollSpeed: 1.3f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — ChainOfJudgment (Melee — Judgment Chain Whip)
        // =====================================================================

        /// <summary>
        /// Apply ChainOfJudgment trail using marble noise for flowing chain links.
        /// Slow, methodical, binding — every strike chains the condemned.
        /// Colors: BloodRed -> JudgmentGold, scroll 0.8 (slow chains).
        /// </summary>
        public static bool ApplyChainOfJudgmentTrail(float time, bool glow = false)
        {
            string technique = glow ? "JudgmentAuraGlow" : "JudgmentAuraMain";

            // Try shared JudgmentAura with marble noise for chain-like flow
            if (HasJudgmentAura)
            {
                BindMarbleNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.JudgmentAuraShader, technique,
                    time, DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold,
                    scrollSpeed: 0.8f, distortionAmt: 0.06f, overbrightMult: 2.6f,
                    noiseScale: 3.5f, noiseScroll: 0.4f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindMarbleNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold,
                    scrollSpeed: 0.8f, distortionAmt: 0.06f, overbrightMult: 2.6f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.BloodRed, DiesIraePalette.JudgmentGold,
                scrollSpeed: 0.8f, overbrightMult: 2.6f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — ArbitersSentence (Melee — Judgment Spear)
        // =====================================================================

        /// <summary>
        /// Apply ArbitersSentence trail using FBM noise for infernal thrust.
        /// Swift, absolute — every thrust delivers a sentence.
        /// Colors: InfernalRed -> JudgmentGold, scroll 1.1.
        /// </summary>
        public static bool ApplyArbitersSentenceTrail(float time, bool glow = false)
        {
            string technique = glow ? "HellfireBloomGlow" : "HellfireBloomMain";

            // Try shared HellfireBloom with weapon-specific colors
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, technique,
                    time, DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold,
                    scrollSpeed: 1.1f, distortionAmt: 0.07f, overbrightMult: 2.7f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold,
                scrollSpeed: 1.1f, overbrightMult: 2.7f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — SinCollector (Ranged — Sin-Seeking Rifle)
        // =====================================================================

        /// <summary>
        /// Apply SinCollector trail using FBM noise for fast bullet tracers.
        /// Precise, seeking — every shot collects a sin.
        /// Colors: BloodRed -> HellfireGold, scroll 1.6 (fast bullets).
        /// </summary>
        public static bool ApplySinCollectorTrail(float time, bool glow = false)
        {
            string technique = glow ? "HellfireBloomGlow" : "HellfireBloomMain";

            // Try shared HellfireBloom with fast scroll for bullet speed
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, technique,
                    time, DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                    scrollSpeed: 1.6f, distortionAmt: 0.05f, overbrightMult: 2.5f,
                    noiseScale: 2.5f, noiseScroll: 0.6f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.BloodRed, DiesIraePalette.HellfireGold,
                scrollSpeed: 1.6f, overbrightMult: 2.5f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — DamnationsCannon (Ranged — Explosive Launcher)
        // =====================================================================

        /// <summary>
        /// Apply DamnationsCannon trail using Voronoi noise for explosive fragmentation.
        /// Heavy, devastating, apocalyptic — every shell is damnation incarnate.
        /// Colors: SmolderingEmber -> HellfireGold, scroll 0.9.
        /// </summary>
        public static bool ApplyDamnationsCannonTrail(float time, bool glow = false)
        {
            string technique = glow ? "HellfireBloomGlow" : "HellfireBloomMain";

            // Try shared HellfireBloom with Voronoi noise for cracked explosive pattern
            if (HasHellfireBloom)
            {
                BindVoronoiNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, technique,
                    time, DiesIraePalette.SmolderingEmber, DiesIraePalette.HellfireGold,
                    scrollSpeed: 0.9f, distortionAmt: 0.09f, overbrightMult: 2.8f,
                    noiseScale: 3.5f, noiseScroll: 0.4f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.SmolderingEmber, DiesIraePalette.HellfireGold,
                scrollSpeed: 0.9f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — GrimoireOfCondemnation (Magic — Condemnation Tome)
        // =====================================================================

        /// <summary>
        /// Apply GrimoireOfCondemnation trail using marble noise for dark ecclesiastical flow.
        /// Deliberate, ominous — every page condemns.
        /// Colors: DoomPurple -> JudgmentGold, scroll 1.0.
        /// </summary>
        public static bool ApplyGrimoireTrail(float time, bool glow = false)
        {
            string technique = glow ? "JudgmentAuraGlow" : "JudgmentAuraMain";

            // Try shared JudgmentAura with marble noise for dark tome flow
            if (HasJudgmentAura)
            {
                BindMarbleNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.JudgmentAuraShader, technique,
                    time, DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                    scrollSpeed: 1.0f, distortionAmt: 0.07f, overbrightMult: 2.5f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindMarbleNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                    scrollSpeed: 1.0f, distortionAmt: 0.07f, overbrightMult: 2.5f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                scrollSpeed: 1.0f, overbrightMult: 2.5f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — StaffOfFinalJudgement (Magic — Divine Staff)
        // =====================================================================

        /// <summary>
        /// Apply StaffOfFinalJudgement trail using the StaffOfFinalJudgementCast palette.
        /// Absolute, ecclesiastical — every cast is a final sentence.
        /// Colors: StaffOfFinalJudgementCast palette (charcoal -> doom purple -> infernal -> judgment gold).
        /// </summary>
        public static bool ApplyStaffOfFinalJudgementTrail(float time, bool glow = false)
        {
            Color primary = DiesIraePalette.PaletteLerp(DiesIraePalette.StaffOfFinalJudgementCast, 0.3f);
            Color secondary = DiesIraePalette.PaletteLerp(DiesIraePalette.StaffOfFinalJudgementCast, 0.8f);
            string technique = glow ? "JudgmentAuraGlow" : "JudgmentAuraMain";

            // Try shared JudgmentAura with weapon-specific palette
            if (HasJudgmentAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.JudgmentAuraShader, technique,
                    time, primary, secondary,
                    scrollSpeed: 1.1f, distortionAmt: 0.08f, overbrightMult: 2.8f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, primary, secondary,
                    scrollSpeed: 1.1f, distortionAmt: 0.08f, overbrightMult: 2.8f,
                    hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, primary, secondary,
                scrollSpeed: 1.1f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — EclipseOfWrath (Magic — Eclipse Orb Staff)
        // =====================================================================

        /// <summary>
        /// Apply EclipseOrb aura for EclipseOfWrath orbiting projectiles.
        /// Radial eclipse pulse — infernal core fading to wrath white corona.
        /// Colors: InfernalRed -> WrathWhite.
        /// </summary>
        public static bool ApplyEclipseOrbAura(float time, float age = 0f, bool glow = false)
        {
            string technique = glow ? "EclipseOrbGlow" : "EclipseOrbMain";
            if (HasEclipseOrb)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.EclipseOrbShader, technique,
                    time, DiesIraePalette.InfernalRed, DiesIraePalette.WrathWhite,
                    age, intensity: 1.3f, overbrightMult: 3.0f,
                    noiseScale: 3f, noiseScroll: 0.4f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, DiesIraePalette.InfernalRed, DiesIraePalette.WrathWhite,
                    age, intensity: 1.3f, overbrightMult: 3.0f,
                    hasNoiseBound: true);
            }

            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — DeathTollingBell (Summoner — Funeral Bell)
        // =====================================================================

        /// <summary>
        /// Apply BellToll aura for DeathTollingBell minion formation.
        /// Solemn radial pulse — blood red tolling to parchment resonance.
        /// Colors: BloodRed -> Parchment.
        /// </summary>
        public static bool ApplyBellTollAura(float time, float summons = 1f, bool glow = false)
        {
            string technique = glow ? "BellTollGlow" : "BellTollMain";
            if (HasBellToll)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.BellTollShader, technique,
                    time, DiesIraePalette.BloodRed, DiesIraePalette.Parchment,
                    summons, intensity: 1.0f, overbrightMult: 2.4f,
                    noiseScale: 2.5f, noiseScroll: 0.3f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasJudgmentAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.JudgmentAuraShader, "JudgmentAuraMain",
                    time, DiesIraePalette.BloodRed, DiesIraePalette.Parchment,
                    summons, intensity: 1.0f, overbrightMult: 2.4f,
                    hasNoiseBound: true);
            }

            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — HarmonyOfJudgement (Summoner — Judgment Sigil)
        // =====================================================================

        /// <summary>
        /// Apply JudgmentAura for HarmonyOfJudgement minion formation.
        /// Ecclesiastical radial sigil — doom purple ascending to judgment gold.
        /// Colors: DoomPurple -> JudgmentGold.
        /// </summary>
        public static bool ApplyJudgmentAura(float time, float summons = 1f, bool glow = false)
        {
            string technique = glow ? "JudgmentAuraGlow" : "JudgmentAuraMain";

            // Try shared JudgmentAura
            if (HasJudgmentAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.JudgmentAuraShader, technique,
                    time, DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                    summons, intensity: 1.1f, overbrightMult: 2.5f,
                    noiseScale: 2.5f, noiseScroll: 0.3f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.HellfireBloomShader, "HellfireBloomMain",
                    time, DiesIraePalette.DoomPurple, DiesIraePalette.JudgmentGold,
                    summons, intensity: 1.1f, overbrightMult: 2.5f,
                    hasNoiseBound: true);
            }

            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — WrathfulContract (Summoner — Demon Contract)
        // =====================================================================

        /// <summary>
        /// Apply HellfireBloom for WrathfulContract minion formation.
        /// Hellfire radial bloom — smoldering ember ascending to wrathful flame.
        /// Colors: SmolderingEmber -> WrathfulFlame.
        /// </summary>
        public static bool ApplyHellfireBloom(float time, float summons = 1f, bool glow = false)
        {
            string technique = glow ? "HellfireBloomGlow" : "HellfireBloomMain";

            // Try shared HellfireBloom
            if (HasHellfireBloom)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.HellfireBloomShader, technique,
                    time, DiesIraePalette.SmolderingEmber, DiesIraePalette.WrathfulFlame,
                    summons, intensity: 1.2f, overbrightMult: 2.6f,
                    noiseScale: 3f, noiseScroll: 0.35f, hasNoiseBound: true);
            }

            // shared fallback
            if (HasJudgmentAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.JudgmentAuraShader, "JudgmentAuraMain",
                    time, DiesIraePalette.SmolderingEmber, DiesIraePalette.WrathfulFlame,
                    summons, intensity: 1.2f, overbrightMult: 2.6f,
                    hasNoiseBound: true);
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
