using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Compartmentalized shader manager for all Ode to Joy weapon VFX.
    /// Provides availability checks, noise texture binding, generic Apply methods,
    /// and weapon-specific presets for each Ode to Joy shader.
    ///
    /// Shared theme-wide shaders (6 shaders mapped to 12 weapons):
    ///   OdeToJoyTriumphantTrail   — general trail for melee/ranged
    ///   OdeToJoyGardenBloom       — aura for summoner/magic
    ///   OdeToJoyCelebrationAura   — radial effects
    ///   OdeToJoyVerdantSlash      — melee swings
    ///   OdeToJoyPollenDrift       — ranged projectile trails
    ///   OdeToJoyJubilantHarmony   — summoner harmony aura
    ///
    /// Weapon presets:
    ///   Melee (VerdantSlash for swing, TriumphantTrail for projectile):
    ///     ThornboundReckoning  — VerdantSlash  (LeafGreen -> GoldenPollen, scroll 1.3)
    ///     ElysianVerdict       — VerdantSlash  (SunlightYellow -> WhiteBloom, scroll 1.5)
    ///     RoseThornChainsaw    — TriumphantTrail (RosePink -> GoldenPollen, scroll 2.0)
    ///     HymnOfTheVictorious  — VerdantSlash  (GoldenPollen -> WhiteBloom, scroll 1.2)
    ///
    ///   Ranged (PollenDrift for trails):
    ///     ThornSprayRepeater   — PollenDrift     (LeafGreen -> PollenGold, scroll 1.8)
    ///     PetalStormCannon     — PollenDrift     (PetalPink -> WhiteBloom, scroll 1.4)
    ///     AnthemOfGlory        — TriumphantTrail (GoldenPollen -> WhiteBloom, scroll 1.6)
    ///     ThePollinator        — PollenDrift     (PollenGold -> GoldenPollen, scroll 1.0)
    ///
    ///   Magic (GardenBloom radial):
    ///     FountainOfJoyousHarmony — GardenBloom (PetalPink -> WhiteBloom)
    ///     TheGardenersFury        — GardenBloom (LeafGreen -> PollenGold)
    ///
    ///   Summoner (JubilantHarmony + CelebrationAura):
    ///     TheStandingOvation   — JubilantHarmony  (GoldenPollen -> WhiteBloom)
    ///     TriumphantChorus     — CelebrationAura  (SunlightYellow -> WhiteBloom)
    ///
    /// Usage (in PreDraw):
    ///   OdeToJoyShaderManager.BeginShaderAdditive(sb);
    ///   OdeToJoyShaderManager.BindNoiseTexture(device);
    ///   OdeToJoyShaderManager.ApplyThornboundSlash(time, comboPhase, glow: false);
    ///   // ... draw trail geometry ...
    ///   OdeToJoyShaderManager.RestoreSpriteBatch(sb);
    ///
    /// All Apply* methods gracefully return false if the shader is null,
    /// allowing VFX code to fall back to particle-based rendering.
    /// </summary>
    public static class OdeToJoyShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        public static bool HasTriumphantTrail => ShaderLoader.HasShader(ShaderLoader.OdeToJoyTriumphantTrailShader);
        public static bool HasGardenBloom => ShaderLoader.HasShader(ShaderLoader.OdeToJoyGardenBloomShader);
        public static bool HasCelebrationAura => ShaderLoader.HasShader(ShaderLoader.OdeToJoyCelebrationAuraShader);
        public static bool HasVerdantSlash => ShaderLoader.HasShader(ShaderLoader.OdeToJoyVerdantSlashShader);
        public static bool HasPollenDrift => ShaderLoader.HasShader(ShaderLoader.OdeToJoyPollenDriftShader);
        public static bool HasJubilantHarmony => ShaderLoader.HasShader(ShaderLoader.OdeToJoyJubilantHarmonyShader);

        /// <summary>True if any Ode to Joy shader is available.</summary>
        public static bool IsAvailable =>
            HasTriumphantTrail || HasGardenBloom ||
            HasCelebrationAura || HasVerdantSlash ||
            HasPollenDrift || HasJubilantHarmony;

        /// <summary>True if the shared ScrollingTrail fallback shader is usable.</summary>
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any trail-capable shader is available (dedicated or fallback).</summary>
        public static bool CanRenderTrails =>
            HasTriumphantTrail || HasVerdantSlash || HasPollenDrift || HasFallbackTrail;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds PerlinNoise to sampler slot 1 — Ode to Joy's primary noise.
        /// Smooth organic flow ideal for garden trails and verdant effects.
        /// Falls back to SimplexNoise if unavailable.
        /// </summary>
        public static void BindNoiseTexture(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise == null)
                noise = ShaderLoader.GetNoiseTexture("SimplexNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds TileableFBMNoise to sampler slot 1 for dense foliage effects.
        /// Layered turbulence ideal for overgrown, tangled garden VFX.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindFBMNoise(GraphicsDevice device)
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
        /// Binds VoronoiNoise to sampler slot 1 for petal and cell patterns.
        /// Crystalline cell structure ideal for petal separation and bloom effects.
        /// Falls back to PerlinNoise if unavailable.
        /// </summary>
        public static void BindVoronoiNoise(GraphicsDevice device)
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
        //  WEAPON PRESETS — ThornboundReckoning (Melee — Thorn Whip Sword)
        // =====================================================================

        /// <summary>
        /// Apply VerdantSlash for ThornboundReckoning melee swings.
        /// Vicious thorned strikes tearing through verdant energy — green to gold.
        /// Colors: LeafGreen -> GoldenPollen. Scroll 1.3.
        /// </summary>
        public static bool ApplyThornboundSlash(float time, float comboPhase = 0f, bool glow = false)
        {
            string technique = glow ? "VerdantSlashGlow" : "VerdantSlashMain";
            if (HasVerdantSlash)
            {
                BindFBMNoise(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyVerdantSlashShader, technique,
                    time, OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen,
                    scrollSpeed: 1.3f, distortionAmt: 0.08f, overbrightMult: 2.6f,
                    phase: comboPhase, noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen,
                scrollSpeed: 1.3f, overbrightMult: 2.6f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — ElysianVerdict (Melee — Radiant Judgment Blade)
        // =====================================================================

        /// <summary>
        /// Apply VerdantSlash for ElysianVerdict melee swings.
        /// Brilliant sunbeam arcs of golden justice — sunlight to pure white.
        /// Colors: SunlightYellow -> WhiteBloom. Scroll 1.5.
        /// </summary>
        public static bool ApplyElysianVerdictSlash(float time, float comboPhase = 0f, bool glow = false)
        {
            string technique = glow ? "VerdantSlashGlow" : "VerdantSlashMain";
            if (HasVerdantSlash)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyVerdantSlashShader, technique,
                    time, OdeToJoyPalette.SunlightYellow, OdeToJoyPalette.WhiteBloom,
                    scrollSpeed: 1.5f, distortionAmt: 0.06f, overbrightMult: 3.0f,
                    phase: comboPhase, noiseScale: 2.5f, noiseScroll: 0.6f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.SunlightYellow, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 1.5f, overbrightMult: 3.0f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — RoseThornChainsaw (Melee — Thorned Chainsaw)
        // =====================================================================

        /// <summary>
        /// Apply TriumphantTrail for RoseThornChainsaw continuous cutting trail.
        /// Fast whirring rose-thorned teeth — warm rose to golden sparks.
        /// Colors: RosePink -> GoldenPollen. Scroll 2.0 for rapid chainsaw motion.
        /// </summary>
        public static bool ApplyRoseThornChainsawTrail(float time, bool glow = false)
        {
            string technique = glow ? "TriumphantTrailGlow" : "TriumphantTrailMain";
            if (HasTriumphantTrail)
            {
                BindFBMNoise(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyTriumphantTrailShader, technique,
                    time, OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen,
                    scrollSpeed: 2.0f, distortionAmt: 0.10f, overbrightMult: 2.8f,
                    noiseScale: 3.5f, noiseScroll: 0.7f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen,
                scrollSpeed: 2.0f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — HymnOfTheVictorious (Melee — Triumphant Hymnal Blade)
        // =====================================================================

        /// <summary>
        /// Apply VerdantSlash for HymnOfTheVictorious melee swings.
        /// Stately golden hymnal arcs of triumph — golden pollen to jubilant white.
        /// Colors: GoldenPollen -> WhiteBloom. Scroll 1.2.
        /// </summary>
        public static bool ApplyHymnSlash(float time, float comboPhase = 0f, bool glow = false)
        {
            string technique = glow ? "VerdantSlashGlow" : "VerdantSlashMain";
            if (HasVerdantSlash)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyVerdantSlashShader, technique,
                    time, OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom,
                    scrollSpeed: 1.2f, distortionAmt: 0.05f, overbrightMult: 2.4f,
                    phase: comboPhase, noiseScale: 3f, noiseScroll: 0.4f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 1.2f, overbrightMult: 2.4f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — ThornSprayRepeater (Ranged — Thorn Spray Gun)
        // =====================================================================

        /// <summary>
        /// Apply PollenDrift trail for ThornSprayRepeater projectiles.
        /// Scattering thorned pollen shot — verdant green dissolving into gold.
        /// Colors: LeafGreen -> PollenGold. Scroll 1.8.
        /// </summary>
        public static bool ApplyThornSprayTrail(float time, bool glow = false)
        {
            string technique = glow ? "PollenDriftGlow" : "PollenDriftMain";
            if (HasPollenDrift)
            {
                BindFBMNoise(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyPollenDriftShader, technique,
                    time, OdeToJoyPalette.LeafGreen, OdeToJoyPalette.PollenGold,
                    scrollSpeed: 1.8f, distortionAmt: 0.07f, overbrightMult: 2.5f,
                    noiseScale: 3f, noiseScroll: 0.6f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.LeafGreen, OdeToJoyPalette.PollenGold,
                scrollSpeed: 1.8f, overbrightMult: 2.5f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — PetalStormCannon (Ranged — Petal Barrage Launcher)
        // =====================================================================

        /// <summary>
        /// Apply PollenDrift trail for PetalStormCannon projectiles.
        /// Swirling petal storm dissolving into light — soft pink to radiant white.
        /// Colors: PetalPink -> WhiteBloom. Scroll 1.4.
        /// </summary>
        public static bool ApplyPetalStormTrail(float time, bool glow = false)
        {
            string technique = glow ? "PollenDriftGlow" : "PollenDriftMain";
            if (HasPollenDrift)
            {
                BindVoronoiNoise(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyPollenDriftShader, technique,
                    time, OdeToJoyPalette.PetalPink, OdeToJoyPalette.WhiteBloom,
                    scrollSpeed: 1.4f, distortionAmt: 0.06f, overbrightMult: 2.6f,
                    noiseScale: 3.5f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.PetalPink, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 1.4f, overbrightMult: 2.6f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — AnthemOfGlory (Ranged — Triumphant Anthem Bow)
        // =====================================================================

        /// <summary>
        /// Apply TriumphantTrail for AnthemOfGlory projectile trails.
        /// Glorious golden streaks of triumph — warm gold to jubilant white.
        /// Colors: GoldenPollen -> WhiteBloom. Scroll 1.6.
        /// </summary>
        public static bool ApplyAnthemTrail(float time, bool glow = false)
        {
            string technique = glow ? "TriumphantTrailGlow" : "TriumphantTrailMain";
            if (HasTriumphantTrail)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyTriumphantTrailShader, technique,
                    time, OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom,
                    scrollSpeed: 1.6f, distortionAmt: 0.05f, overbrightMult: 2.8f,
                    noiseScale: 3f, noiseScroll: 0.5f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom,
                scrollSpeed: 1.6f, overbrightMult: 2.8f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — ThePollinator (Ranged — Pollen Spread Gun)
        // =====================================================================

        /// <summary>
        /// Apply PollenDrift trail for ThePollinator projectiles.
        /// Gentle drifting pollen motes — warm gold dissolving into bloom gold.
        /// Colors: PollenGold -> GoldenPollen. Scroll 1.0.
        /// </summary>
        public static bool ApplyPollinatorTrail(float time, bool glow = false)
        {
            string technique = glow ? "PollenDriftGlow" : "PollenDriftMain";
            if (HasPollenDrift)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyTrailShader(ShaderLoader.OdeToJoyPollenDriftShader, technique,
                    time, OdeToJoyPalette.PollenGold, OdeToJoyPalette.GoldenPollen,
                    scrollSpeed: 1.0f, distortionAmt: 0.04f, overbrightMult: 2.2f,
                    noiseScale: 3f, noiseScroll: 0.3f, hasNoiseBound: true);
            }

            ApplyFallbackTrail(time, OdeToJoyPalette.PollenGold, OdeToJoyPalette.GoldenPollen,
                scrollSpeed: 1.0f, overbrightMult: 2.2f);
            return HasFallbackTrail;
        }

        // =====================================================================
        //  WEAPON PRESETS — FountainOfJoyousHarmony (Magic — Joyous Fountain)
        // =====================================================================

        /// <summary>
        /// Apply GardenBloom radial aura for FountainOfJoyousHarmony channeled effects.
        /// Overflowing joyous energy blossoming outward — petal pink to radiant white.
        /// Colors: PetalPink -> WhiteBloom.
        /// </summary>
        public static bool ApplyFountainBloom(float time, float age = 0f, bool glow = false)
        {
            string technique = glow ? "GardenBloomGlow" : "GardenBloomMain";
            if (HasGardenBloom)
            {
                BindVoronoiNoise(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.OdeToJoyGardenBloomShader, technique,
                    time, OdeToJoyPalette.PetalPink, OdeToJoyPalette.WhiteBloom,
                    age, intensity: 1.2f, overbrightMult: 2.6f,
                    noiseScale: 3f, noiseScroll: 0.4f, hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — TheGardenersFury (Magic — Furious Garden Staff)
        // =====================================================================

        /// <summary>
        /// Apply GardenBloom radial aura for TheGardenersFury channeled effects.
        /// Wrathful overgrowth erupting in verdant fury — leaf green to pollen gold.
        /// Colors: LeafGreen -> PollenGold.
        /// </summary>
        public static bool ApplyGardenerFuryBloom(float time, float age = 0f, bool glow = false)
        {
            string technique = glow ? "GardenBloomGlow" : "GardenBloomMain";
            if (HasGardenBloom)
            {
                BindFBMNoise(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.OdeToJoyGardenBloomShader, technique,
                    time, OdeToJoyPalette.LeafGreen, OdeToJoyPalette.PollenGold,
                    age, intensity: 1.4f, overbrightMult: 2.8f,
                    noiseScale: 3.5f, noiseScroll: 0.5f, hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — TheStandingOvation (Summoner — Ovation Conductor)
        // =====================================================================

        /// <summary>
        /// Apply JubilantHarmony aura for TheStandingOvation minion formation.
        /// Jubilant harmonious resonance around summoned performers — golden to white.
        /// Colors: GoldenPollen -> WhiteBloom.
        /// </summary>
        public static bool ApplyStandingOvationAura(float time, float activeSummons = 1f, bool glow = false)
        {
            string technique = glow ? "JubilantHarmonyGlow" : "JubilantHarmonyMain";
            if (HasJubilantHarmony)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.OdeToJoyJubilantHarmonyShader, technique,
                    time, OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom,
                    activeSummons, intensity: 1.0f, overbrightMult: 2.2f,
                    noiseScale: 2.5f, noiseScroll: 0.3f, hasNoiseBound: true);
            }
            return false;
        }

        // =====================================================================
        //  WEAPON PRESETS — TriumphantChorus (Summoner — Choral Triumph)
        // =====================================================================

        /// <summary>
        /// Apply CelebrationAura for TriumphantChorus minion formation.
        /// Radiant celebration rippling through the chorus — sunlight to jubilant white.
        /// Colors: SunlightYellow -> WhiteBloom.
        /// </summary>
        public static bool ApplyTriumphantChorusAura(float time, float activeSummons = 1f, bool glow = false)
        {
            string technique = glow ? "CelebrationAuraGlow" : "CelebrationAuraMain";
            if (HasCelebrationAura)
            {
                BindNoiseTexture(Main.graphics.GraphicsDevice);
                return ApplyRadialShader(ShaderLoader.OdeToJoyCelebrationAuraShader, technique,
                    time, OdeToJoyPalette.SunlightYellow, OdeToJoyPalette.WhiteBloom,
                    activeSummons, intensity: 1.1f, overbrightMult: 2.4f,
                    noiseScale: 3f, noiseScroll: 0.35f, hasNoiseBound: true);
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
