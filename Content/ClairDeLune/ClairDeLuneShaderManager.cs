using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.ClairDeLune
{
    /// <summary>
    /// Shader manager for all Clair de Lune weapon-specific and theme-wide shaders.
    /// Wraps ShaderLoader access to 14 custom Clair de Lune .fx files  E2 theme-wide
    /// (ClairDeLuneMoonlit, ClairDeLunePearlGlow) plus 12 per-weapon shaders.
    ///
    /// Each weapon has its own unique shader with two techniques (main + glow/overlay).
    /// The theme-wide shaders provide shared moonlit trail and pearl bloom effects
    /// usable by accessories, tools, wings, and any content that needs the Clair de Lune look.
    ///
    /// Usage:
    ///   ClairDeLuneShaderManager.BindMistNoise(device);
    ///   ClairDeLuneShaderManager.ApplyTemporalDrill(time); // weapon-specific
    ///   // ... draw trail geometry ...
    ///   ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
    /// </summary>
    public static class ClairDeLuneShaderManager
    {
        // =====================================================================
        //  Shader Availability
        // =====================================================================

        // Theme-wide
        public static bool HasMoonlit => ShaderLoader.HasShader(ShaderLoader.ClairDeLuneMoonlitShader);
        public static bool HasPearlGlow => ShaderLoader.HasShader(ShaderLoader.ClairDeLunePearlGlowShader);

        // Melee
        public static bool HasTemporalDrill => ShaderLoader.HasShader(ShaderLoader.TemporalDrillShader);
        public static bool HasCrystalLance => ShaderLoader.HasShader(ShaderLoader.CrystalLanceShader);
        public static bool HasGearSwing => ShaderLoader.HasShader(ShaderLoader.GearSwingShader);

        // Magic
        public static bool HasArcanePages => ShaderLoader.HasShader(ShaderLoader.ArcanePagesShader);
        public static bool HasCelestialOrbit => ShaderLoader.HasShader(ShaderLoader.CelestialOrbitShader);
        public static bool HasTimeFreezeSlash => ShaderLoader.HasShader(ShaderLoader.TimeFreezeSlashShader);

        // Ranged
        public static bool HasStarfallTrail => ShaderLoader.HasShader(ShaderLoader.StarfallTrailShader);
        public static bool HasGatlingBlur => ShaderLoader.HasShader(ShaderLoader.GatlingBlurShader);
        public static bool HasSingularityPull => ShaderLoader.HasShader(ShaderLoader.SingularityPullShader);

        // Summon
        public static bool HasSoulBeam => ShaderLoader.HasShader(ShaderLoader.SoulBeamShader);
        public static bool HasJudgmentMark => ShaderLoader.HasShader(ShaderLoader.JudgmentMarkShader);
        public static bool HasResonanceField => ShaderLoader.HasShader(ShaderLoader.ResonanceFieldShader);

        // Fallback
        public static bool HasFallbackTrail => ShaderLoader.HasShader(ShaderLoader.ScrollingTrailShader);

        /// <summary>True if any Clair de Lune shader is available.</summary>
        public static bool IsAvailable => HasMoonlit || HasPearlGlow || HasTemporalDrill ||
            HasCrystalLance || HasGearSwing || HasArcanePages || HasCelestialOrbit ||
            HasTimeFreezeSlash || HasStarfallTrail || HasGatlingBlur || HasSingularityPull ||
            HasSoulBeam || HasJudgmentMark || HasResonanceField;

        // =====================================================================
        //  Noise Texture Binding
        // =====================================================================

        /// <summary>
        /// Binds SoftCircularCaustics noise to sampler slot 1 for dreamy mist distortion.
        /// Primary noise for Clair de Lune  Eused by the theme-wide moonlit trail shader.
        /// </summary>
        public static void BindMistNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics")
                           ?? ShaderLoader.GetNoiseTexture("NoiseSmoke")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds CosmicEnergyVortex noise for temporal vortex effects (drill, singularity).
        /// </summary>
        public static void BindVortexNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex")
                           ?? ShaderLoader.GetNoiseTexture("SoftCircularCaustics")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds SimplexNoise for crystalline/starfall weapon effects.
        /// </summary>
        public static void BindSparkleNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("SimplexNoise")
                           ?? ShaderLoader.GetNoiseTexture("StarFieldScatter")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds VoronoiCellNoise for fracture/cracking weapon effects.
        /// </summary>
        public static void BindFractureNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("VoronoiCellNoise")
                           ?? ShaderLoader.GetNoiseTexture("RealityCrackPattern")
                           ?? ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (noise != null)
            {
                device.Textures[1] = noise;
                device.SamplerStates[1] = SamplerState.LinearWrap;
            }
        }

        /// <summary>
        /// Binds MusicalWavePattern noise for resonance/harmonic weapon effects.
        /// </summary>
        public static void BindHarmonicNoise(GraphicsDevice device)
        {
            Texture2D noise = ShaderLoader.GetNoiseTexture("MusicalWavePattern")
                           ?? ShaderLoader.GetNoiseTexture("SoftCircularCaustics")
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
        //  THEME-WIDE: ClairDeLuneMoonlit  Edreamy moonlit pearl trail
        // =====================================================================

        /// <summary>Apply the theme-wide moonlit pearl trail (technique: MoonlitFlow).</summary>
        public static void ApplyMoonlitTrail(float time, Color primary, Color secondary,
            float scrollSpeed = 1f, float distortionAmt = 0.06f, float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ClairDeLuneMoonlit;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time, primary, secondary,
                scrollSpeed: scrollSpeed, distortionAmt: distortionAmt, overbrightMult: overbrightMult);

            shader.CurrentTechnique = shader.Techniques["MoonlitFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply the theme-wide moonlit glow pass (technique: MoonlitGlow).</summary>
        public static void ApplyMoonlitGlow(float time, Color primary, Color secondary,
            float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ClairDeLuneMoonlit;
            if (shader == null) return;

            SetCommonUniforms(shader, time, primary, secondary, overbrightMult: overbrightMult);

            shader.CurrentTechnique = shader.Techniques["MoonlitGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  THEME-WIDE: ClairDeLunePearlGlow  Epearl bloom overlay
        // =====================================================================

        /// <summary>Apply the theme-wide pearl bloom (technique: PearlBloom).</summary>
        public static void ApplyPearlBloom(float time, Color primary, Color secondary,
            float overbrightMult = 3f)
        {
            Effect shader = ShaderLoader.ClairDeLunePearlGlow;
            if (shader == null) return;

            SetCommonUniforms(shader, time, primary, secondary, overbrightMult: overbrightMult);

            shader.CurrentTechnique = shader.Techniques["PearlBloom"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply the theme-wide pearl shimmer (technique: PearlShimmer).</summary>
        public static void ApplyPearlShimmer(float time, Color primary, Color secondary,
            float overbrightMult = 2.5f)
        {
            Effect shader = ShaderLoader.ClairDeLunePearlGlow;
            if (shader == null) return;

            SetCommonUniforms(shader, time, primary, secondary, overbrightMult: overbrightMult);

            shader.CurrentTechnique = shader.Techniques["PearlShimmer"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  MELEE: Chronologicality  ETemporal Drill
        // =====================================================================

        /// <summary>Apply TemporalDrill bore trail (technique: TemporalDrillBore).</summary>
        public static void ApplyTemporalDrill(float time, float intensity = 1.8f)
        {
            Effect shader = ShaderLoader.TemporalDrill;
            if (shader == null) return;

            BindVortexNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlWhite,
                intensity: intensity, overbrightMult: 3.2f,
                scrollSpeed: 2.2f, distortionAmt: 0.1f,
                secondaryTexScale: 2.5f);

            shader.CurrentTechnique = shader.Techniques["TemporalDrillBore"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply TemporalDrill glow overlay (technique: TemporalDrillGlow).</summary>
        public static void ApplyTemporalDrillGlow(float time)
        {
            Effect shader = ShaderLoader.TemporalDrill;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlWhite,
                overbrightMult: 3.2f);

            shader.CurrentTechnique = shader.Techniques["TemporalDrillGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  MELEE: Temporal Piercer  ECrystal Lance
        // =====================================================================

        /// <summary>Apply CrystalLance thrust trail (technique: CrystalLanceThrust).</summary>
        public static void ApplyCrystalLance(float time, float intensity = 1.8f)
        {
            Effect shader = ShaderLoader.CrystalLance;
            if (shader == null) return;

            BindSparkleNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlBlue,
                intensity: intensity, overbrightMult: 3.5f,
                scrollSpeed: 2.8f, distortionAmt: 0.03f,
                secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["CrystalLanceThrust"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply CrystalLance shatter burst (technique: CrystalLanceShatter).</summary>
        public static void ApplyCrystalLanceShatter(float time)
        {
            Effect shader = ShaderLoader.CrystalLance;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlBlue,
                overbrightMult: 3.5f);

            shader.CurrentTechnique = shader.Techniques["CrystalLanceShatter"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  MELEE: Clockwork Harmony  EGear Swing
        // =====================================================================

        /// <summary>Apply GearSwing arc (technique: GearSwingArc).</summary>
        public static void ApplyGearSwing(float time, float comboPhase = 0f, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.GearSwing;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                intensity: intensity, overbrightMult: 2.8f,
                scrollSpeed: 1f, distortionAmt: 0.08f,
                secondaryTexScale: 3f);
            SetPhaseUniform(shader, comboPhase);

            shader.CurrentTechnique = shader.Techniques["GearSwingArc"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply GearSwing trailing afterimage (technique: GearSwingTrail).</summary>
        public static void ApplyGearSwingTrail(float time)
        {
            Effect shader = ShaderLoader.GearSwing;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                overbrightMult: 2.8f);

            shader.CurrentTechnique = shader.Techniques["GearSwingTrail"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  MAGIC: Clockwork Grimoire  EArcane Pages
        // =====================================================================

        /// <summary>Apply ArcanePages flowing script (technique: ArcanePageFlow).</summary>
        public static void ApplyArcanePages(float time, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.ArcanePages;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                intensity: intensity, overbrightMult: 2.8f,
                scrollSpeed: 1.5f, distortionAmt: 0.06f,
                secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["ArcanePageFlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply ArcanePages ambient glow (technique: ArcanePageGlow).</summary>
        public static void ApplyArcanePagesGlow(float time)
        {
            Effect shader = ShaderLoader.ArcanePages;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                overbrightMult: 2.8f);

            shader.CurrentTechnique = shader.Techniques["ArcanePageGlow"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  MAGIC: Orrery Of Dreams  ECelestial Orbit
        // =====================================================================

        /// <summary>Apply CelestialOrbit orbital paths (technique: CelestialOrbitPath).</summary>
        public static void ApplyCelestialOrbit(float time, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.CelestialOrbit;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.DreamHaze, ClairDeLunePalette.StarlightSilver,
                intensity: intensity, overbrightMult: 3f,
                scrollSpeed: 1.2f, distortionAmt: 0.07f,
                secondaryTexScale: 2.5f);

            shader.CurrentTechnique = shader.Techniques["CelestialOrbitPath"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply CelestialOrbit core glow (technique: CelestialOrbitCore).</summary>
        public static void ApplyCelestialOrbitCore(float time)
        {
            Effect shader = ShaderLoader.CelestialOrbit;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.DreamHaze, ClairDeLunePalette.StarlightSilver,
                overbrightMult: 3f);

            shader.CurrentTechnique = shader.Techniques["CelestialOrbitCore"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  MAGIC: Requiem Of Time  ETime Freeze Slash
        // =====================================================================

        /// <summary>Apply TimeFreezeSlash main slash (technique: TimeFreezeSlash).</summary>
        public static void ApplyTimeFreezeSlash(float time, float intensity = 1.8f)
        {
            Effect shader = ShaderLoader.TimeFreezeSlash;
            if (shader == null) return;

            BindFractureNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.PearlBlue, ClairDeLunePalette.TemporalCrimson,
                intensity: intensity, overbrightMult: 3.2f,
                scrollSpeed: 1.8f, distortionAmt: 0.09f,
                secondaryTexScale: 2.5f);

            shader.CurrentTechnique = shader.Techniques["TimeFreezeSlash"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply TimeFreezeSlash crack network (technique: TimeFreezeCrack).</summary>
        public static void ApplyTimeFreezeCrack(float time)
        {
            Effect shader = ShaderLoader.TimeFreezeSlash;
            if (shader == null) return;

            BindFractureNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.PearlBlue, ClairDeLunePalette.TemporalCrimson,
                overbrightMult: 3.2f, secondaryTexScale: 2.5f);

            shader.CurrentTechnique = shader.Techniques["TimeFreezeCrack"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  RANGED: Starfall Whisper  EStarfall Trail
        // =====================================================================

        /// <summary>Apply StarfallTrail falling star bolt (technique: StarfallBolt).</summary>
        public static void ApplyStarfallBolt(float time, float intensity = 1.8f)
        {
            Effect shader = ShaderLoader.StarfallTrail;
            if (shader == null) return;

            BindSparkleNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlBlue,
                intensity: intensity, overbrightMult: 3.5f,
                scrollSpeed: 3f, distortionAmt: 0.02f,
                secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["StarfallBolt"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply StarfallTrail soft stardust wake (technique: StarfallWake).</summary>
        public static void ApplyStarfallWake(float time)
        {
            Effect shader = ShaderLoader.StarfallTrail;
            if (shader == null) return;

            BindSparkleNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlBlue,
                overbrightMult: 3.5f, secondaryTexScale: 2f);

            shader.CurrentTechnique = shader.Techniques["StarfallWake"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  RANGED: Midnight Mechanism  EGatling Blur
        // =====================================================================

        /// <summary>Apply GatlingBlur barrel motion blur (technique: GatlingBarrelBlur).</summary>
        public static void ApplyGatlingBlur(float time, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.GatlingBlur;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.SoftBlue,
                intensity: intensity, overbrightMult: 2.8f,
                scrollSpeed: 2.5f, distortionAmt: 5f, // Higher distortion = more blur spread
                secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["GatlingBarrelBlur"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply GatlingBlur muzzle flash (technique: GatlingMuzzle).</summary>
        public static void ApplyGatlingMuzzle(float time)
        {
            Effect shader = ShaderLoader.GatlingBlur;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                overbrightMult: 3.5f);

            shader.CurrentTechnique = shader.Techniques["GatlingMuzzle"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  RANGED: Cog And Hammer  ESingularity Pull
        // =====================================================================

        /// <summary>Apply SingularityPull gravitational vortex (technique: SingularityVortex).</summary>
        public static void ApplySingularityVortex(float time, float intensity = 2f)
        {
            Effect shader = ShaderLoader.SingularityPull;
            if (shader == null) return;

            BindVortexNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.DeepNight, ClairDeLunePalette.PearlBlue,
                intensity: intensity, overbrightMult: 3f,
                scrollSpeed: 0.8f, distortionAmt: 0.12f,
                secondaryTexScale: 2f);

            shader.CurrentTechnique = shader.Techniques["SingularityVortex"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply SingularityPull intense core (technique: SingularityCore).</summary>
        public static void ApplySingularityCore(float time)
        {
            Effect shader = ShaderLoader.SingularityPull;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.DeepNight, ClairDeLunePalette.WhiteHot,
                overbrightMult: 4f);

            shader.CurrentTechnique = shader.Techniques["SingularityCore"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  SUMMON: Lunar Phylactery  ESoul Beam
        // =====================================================================

        /// <summary>Apply SoulBeam tether beam (technique: SoulBeamTether).</summary>
        public static void ApplySoulBeam(float time, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.SoulBeam;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.PearlShimmer, ClairDeLunePalette.MoonlitFrost,
                intensity: intensity, overbrightMult: 2.5f,
                scrollSpeed: 0.8f, distortionAmt: 0.06f,
                secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["SoulBeamTether"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply SoulBeam soft aura (technique: SoulBeamAura).</summary>
        public static void ApplySoulBeamAura(float time)
        {
            Effect shader = ShaderLoader.SoulBeam;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.PearlShimmer, ClairDeLunePalette.MoonlitFrost,
                overbrightMult: 2.5f, secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["SoulBeamAura"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  SUMMON: Gear-Driven Arbiter  EJudgment Mark
        // =====================================================================

        /// <summary>Apply JudgmentMark sigil (technique: JudgmentMarkSigil).</summary>
        public static void ApplyJudgmentMark(float time, float phase = 0f, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.JudgmentMark;
            if (shader == null) return;

            BindMistNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.TemporalCrimson,
                intensity: intensity, overbrightMult: 3f,
                scrollSpeed: 2f, distortionAmt: 0.05f,
                secondaryTexScale: 3f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["JudgmentMarkSigil"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply JudgmentMark detonation blast (technique: JudgmentMarkDetonate).</summary>
        public static void ApplyJudgmentMarkDetonate(float time, float phase = 1f)
        {
            Effect shader = ShaderLoader.JudgmentMark;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.TemporalCrimson,
                overbrightMult: 4f);
            SetPhaseUniform(shader, phase);

            shader.CurrentTechnique = shader.Techniques["JudgmentMarkDetonate"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  SUMMON: Automaton's Tuning Fork  EResonance Field
        // =====================================================================

        /// <summary>Apply ResonanceField expanding pulse rings (technique: ResonanceFieldPulse).</summary>
        public static void ApplyResonanceField(float time, float intensity = 1.5f)
        {
            Effect shader = ShaderLoader.ResonanceField;
            if (shader == null) return;

            BindHarmonicNoise(Main.graphics.GraphicsDevice);
            SetCommonUniforms(shader, time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                intensity: intensity, overbrightMult: 2.5f,
                scrollSpeed: 1f, distortionAmt: 0.05f,
                secondaryTexScale: 3f);

            shader.CurrentTechnique = shader.Techniques["ResonanceFieldPulse"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>Apply ResonanceField harmonic overtone overlay (technique: ResonanceFieldHarmonic).</summary>
        public static void ApplyResonanceFieldHarmonic(float time)
        {
            Effect shader = ShaderLoader.ResonanceField;
            if (shader == null) return;

            SetCommonUniforms(shader, time,
                ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite,
                overbrightMult: 2.5f);

            shader.CurrentTechnique = shader.Techniques["ResonanceFieldHarmonic"];
            shader.CurrentTechnique.Passes[0].Apply();
        }

        // =====================================================================
        //  BOSS / SPECIAL PRESETS
        // =====================================================================

        /// <summary>
        /// Boss temporal shockwave  Emassive expanding ring with intense moonlit bloom.
        /// Uses RadialScroll as this is a screen-space effect.
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
