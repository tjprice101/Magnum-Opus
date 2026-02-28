using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.Shaders
{
    /// <summary>
    /// Centralized shader and VFX texture loading system.
    /// Loads auto-compiled shaders from Effects/ and noise/trail
    /// textures from Assets/VFX/ for use as secondary samplers (uImage1).
    /// 
    /// tModLoader auto-compiles .fx files placed in the Effects/ folder
    /// into FNA-compatible effect bytecode at build time. Do NOT place
    /// pre-compiled .fxc files here — they use DirectX bytecode that is
    /// incompatible with FNA's MojoShader runtime.
    /// 
    /// Usage:
    ///   Effect shader = ShaderLoader.GetShader("SimpleTrailShader");
    ///   shader.Parameters["uColor"]?.SetValue(color.ToVector3());
    ///   
    ///   // Bind noise texture to sampler slot 1 (uImage1)
    ///   Texture2D noise = ShaderLoader.GetNoiseTexture("PerlinNoise");
    ///   device.Textures[1] = noise;
    ///   device.SamplerStates[1] = SamplerState.LinearWrap;
    ///   
    ///   // Or get the default texture for a trail style
    ///   Texture2D tex = ShaderLoader.GetDefaultTrailTexture(TrailStyle.Flame);
    /// </summary>
    public class ShaderLoader : ModSystem
    {
        private static Dictionary<string, Effect> _shaders;
        private static Dictionary<string, Texture2D> _noiseTextures;
        private static Dictionary<string, Texture2D> _trailTextures;
        private static bool _initialized;
        private static bool _shadersEnabled;

        // Shader names (without extension) - must match .fx filenames in Effects/
        public const string TrailShader = "SimpleTrailShader";
        public const string BloomShader = "SimpleBloomShader";
        public const string ScrollingTrailShader = "ScrollingTrailShader";
        public const string CelestialValorTrailShader = "Eroica/CelestialValor/CelestialValorTrail";
        public const string MotionBlurBloomShader = "MotionBlurBloom";
        public const string TerraBladeSwingVFXShader = "TerraBladeSwingVFX";
        public const string RadialScrollShaderName = "RadialScrollShader";
        public const string BeamGradientFlowShader = "BeamGradientFlow";
        public const string MoonlightTrailShader = "MoonlightSonata/MoonlightTrail";
        public const string LunarBeamShader = "MoonlightSonata/LunarBeam";
        public const string CrescentAuraShader = "MoonlightSonata/CrescentAura";
        public const string HeroicFlameTrailShader = "Eroica/HeroicFlameTrail";
        public const string SakuraBloomShader = "Eroica/SakuraBloom";
        public const string EroicaFuneralTrailShader = "Eroica/FuneralPrayer/EroicaFuneralTrail";
        public const string TriumphantFractalShaderName = "Eroica/TriumphantFractal/TriumphantFractalShader";
        public const string NachtmusikStarTrailShader = "Nachtmusik/NachtmusikStarTrail";
        public const string NachtmusikSerenadeShader = "Nachtmusik/NachtmusikSerenade";
        public const string TerraBladeFlareBeamShaderName = "TerraBladeFlareBeamShader";

        // Nachtmusik — NocturnalExecutioner weapon-specific shaders
        public const string ExecutionDecreeShader = "Nachtmusik/NocturnalExecutioner/ExecutionDecree";

        // Nachtmusik — MidnightsCrescendo weapon-specific shaders
        public const string CrescendoRiseShader = "Nachtmusik/MidnightsCrescendo/CrescendoRise";

        // Nachtmusik — TwilightSeverance weapon-specific shaders
        public const string DimensionalRiftShader = "Nachtmusik/TwilightSeverance/DimensionalRift";

        // Nachtmusik — ConstellationPiercer weapon-specific shaders
        public const string StarChainBeamShader = "Nachtmusik/ConstellationPiercer/StarChainBeam";

        // Nachtmusik — NebulasWhisper weapon-specific shaders
        public const string NebulaScatterShader = "Nachtmusik/NebulasWhisper/NebulaScatter";

        // Nachtmusik — SerenadeOfDistantStars weapon-specific shaders
        public const string StarHomingTrailShader = "Nachtmusik/SerenadeOfDistantStars/StarHomingTrail";

        // Nachtmusik — StarweaversGrimoire weapon-specific shaders
        public const string ConstellationWeaveShader = "Nachtmusik/StarweaversGrimoire/ConstellationWeave";

        // Nachtmusik — RequiemOfTheCosmos weapon-specific shaders
        public const string CosmicRequiemShader = "Nachtmusik/RequiemOfTheCosmos/CosmicRequiem";

        // Nachtmusik — CelestialChorusBaton weapon-specific shaders
        public const string ChorusSummonAuraShader = "Nachtmusik/CelestialChorusBaton/ChorusSummonAura";

        // Nachtmusik — GalacticOverture weapon-specific shaders
        public const string OvertureAuraShader = "Nachtmusik/GalacticOverture/OvertureAura";

        // Nachtmusik — ConductorOfConstellations weapon-specific shaders
        public const string StellarConductorAuraShader = "Nachtmusik/ConductorOfConstellations/StellarConductorAura";
        public const string IncisorResonanceShader = "MoonlightSonata/IncisorOfMoonlight/IncisorResonance";
        public const string ConstellationFieldShader = "MoonlightSonata/IncisorOfMoonlight/ConstellationField";

        // Eroica — SakurasBlossom weapon-specific shaders
        public const string SakuraSwingTrailShader = "Eroica/SakurasBlossom/SakuraSwingTrail";
        public const string PetalDissolveShader = "Eroica/SakurasBlossom/PetalDissolve";

        // Eroica — CelestialValor weapon-specific shaders
        public const string ValorAuraShader = "Eroica/CelestialValor/ValorAura";

        // Eroica — BlossomOfTheSakura weapon-specific shaders
        public const string HeatDistortionShader = "Eroica/BlossomOfTheSakura/HeatDistortion";
        public const string TracerTrailShader = "Eroica/BlossomOfTheSakura/TracerTrail";

        // Eroica — PiercingLightOfTheSakura weapon-specific shaders
        public const string CrescendoChargeShader = "Eroica/PiercingLightOfTheSakura/CrescendoCharge";
        public const string SakuraLightningTrailShader = "Eroica/PiercingLightOfTheSakura/SakuraLightningTrail";

        // Eroica — FuneralPrayer weapon-specific shaders
        public const string RequiemBeamShader = "Eroica/FuneralPrayer/RequiemBeam";
        public const string PrayerConvergenceShader = "Eroica/FuneralPrayer/PrayerConvergence";

        // Eroica — TriumphantFractal weapon-specific shaders
        public const string SacredGeometryShader = "Eroica/TriumphantFractal/SacredGeometry";

        // Eroica — FinalityOfTheSakura weapon-specific shaders
        public const string FateSummonCircleShader = "Eroica/FinalityOfTheSakura/FateSummonCircle";
        public const string DarkFlameAuraShader = "Eroica/FinalityOfTheSakura/DarkFlameAura";

        // Moonlight Sonata — EternalMoon weapon-specific shaders
        public const string TidalTrailShader = "MoonlightSonata/EternalMoon/TidalTrail";
        public const string CrescentBloomShader = "MoonlightSonata/EternalMoon/CrescentBloom";
        public const string LunarPhaseAuraShader = "MoonlightSonata/EternalMoon/LunarPhaseAura";

        // Moonlight Sonata — MoonlightsCalling weapon-specific shaders
        public const string PrismaticBeamShader = "MoonlightSonata/MoonlightsCalling/PrismaticBeam";
        public const string RefractionRippleShader = "MoonlightSonata/MoonlightsCalling/RefractionRipple";

        // Moonlight Sonata — ResurrectionOfTheMoon weapon-specific shaders
        public const string CometTrailShader = "MoonlightSonata/ResurrectionOfTheMoon/CometTrail";
        public const string SupernovaBlastShader = "MoonlightSonata/ResurrectionOfTheMoon/SupernovaBlast";

        // Moonlight Sonata — StaffOfTheLunarPhases weapon-specific shaders
        public const string GravitationalRiftShader = "MoonlightSonata/StaffOfTheLunarPhases/GravitationalRift";
        public const string SummonCircleShader = "MoonlightSonata/StaffOfTheLunarPhases/SummonCircle";

        // Swan Lake — CalloftheBlackSwan weapon-specific shaders
        public const string DualPolaritySwingShader = "SwanLake/CalloftheBlackSwan/DualPolaritySwing";
        public const string SwanFlareTrailShader = "SwanLake/CalloftheBlackSwan/SwanFlareTrail";

        // Swan Lake — CallofthePearlescentLake weapon-specific shaders
        public const string PearlescentRocketTrailShader = "SwanLake/CallofthePearlescentLake/PearlescentRocketTrail";
        public const string LakeExplosionShader = "SwanLake/CallofthePearlescentLake/LakeExplosion";

        // Swan Lake — ChromaticSwanSong weapon-specific shaders
        public const string ChromaticTrailShader = "SwanLake/ChromaticSwanSong/ChromaticTrail";
        public const string AriaExplosionShader = "SwanLake/ChromaticSwanSong/AriaExplosion";

        // Swan Lake — FeatheroftheIridescentFlock weapon-specific shaders
        public const string CrystalOrbitTrailShader = "SwanLake/FeatheroftheIridescentFlock/CrystalOrbitTrail";
        public const string FlockAuraShader = "SwanLake/FeatheroftheIridescentFlock/FlockAura";

        // Swan Lake — IridescentWingspan weapon-specific shaders
        public const string EtherealWingShader = "SwanLake/IridescentWingspan/EtherealWing";
        public const string WingspanFlareTrailShader = "SwanLake/IridescentWingspan/WingspanFlareTrail";

        // Swan Lake — TheSwansLament weapon-specific shaders
        public const string LamentBulletTrailShader = "SwanLake/TheSwansLament/LamentBulletTrail";
        public const string DestructionRevelationShader = "SwanLake/TheSwansLament/DestructionRevelation";

        // Clair de Lune — theme-wide shaders
        public const string ClairDeLuneMoonlitShader = "ClairDeLune/ClairDeLuneMoonlit";
        public const string ClairDeLunePearlGlowShader = "ClairDeLune/ClairDeLunePearlGlow";

        // Clair de Lune — Chronologicality weapon-specific shaders
        public const string TemporalDrillShader = "ClairDeLune/Chronologicality/TemporalDrill";

        // Clair de Lune — TemporalPiercer weapon-specific shaders
        public const string CrystalLanceShader = "ClairDeLune/TemporalPiercer/CrystalLance";

        // Clair de Lune — ClockworkHarmony weapon-specific shaders
        public const string GearSwingShader = "ClairDeLune/ClockworkHarmony/GearSwing";

        // Clair de Lune — ClockworkGrimoire weapon-specific shaders
        public const string ArcanePagesShader = "ClairDeLune/ClockworkGrimoire/ArcanePages";

        // Clair de Lune — OrreryOfDreams weapon-specific shaders
        public const string CelestialOrbitShader = "ClairDeLune/OrreryOfDreams/CelestialOrbit";

        // Clair de Lune — RequiemOfTime weapon-specific shaders
        public const string TimeFreezeSlashShader = "ClairDeLune/RequiemOfTime/TimeFreezeSlash";

        // Clair de Lune — StarfallWhisper weapon-specific shaders
        public const string StarfallTrailShader = "ClairDeLune/StarfallWhisper/StarfallTrail";

        // Clair de Lune — MidnightMechanism weapon-specific shaders
        public const string GatlingBlurShader = "ClairDeLune/MidnightMechanism/GatlingBlur";

        // Clair de Lune — CogAndHammer weapon-specific shaders
        public const string SingularityPullShader = "ClairDeLune/CogAndHammer/SingularityPull";

        // Clair de Lune — LunarPhylactery weapon-specific shaders
        public const string SoulBeamShader = "ClairDeLune/LunarPhylactery/SoulBeam";

        // Clair de Lune — GearDrivenArbiter weapon-specific shaders
        public const string JudgmentMarkShader = "ClairDeLune/GearDrivenArbiter/JudgmentMark";

        // Clair de Lune — AutomatonsTuningFork weapon-specific shaders
        public const string ResonanceFieldShader = "ClairDeLune/AutomatonsTuningFork/ResonanceField";

        // Enigma Variations — VariationsOfTheVoid weapon-specific shaders
        public const string VoidSwingTrailShader = "EnigmaVariations/VariationsOfTheVoid/VoidVariationSwingTrail";
        public const string VoidBeamShader = "EnigmaVariations/VariationsOfTheVoid/VoidVariationBeam";

        // Enigma Variations — TheUnresolvedCadence weapon-specific shaders
        public const string CadenceSwingTrailShader = "EnigmaVariations/TheUnresolvedCadence/CadenceSwingTrail";
        public const string CadenceCollapseShader = "EnigmaVariations/TheUnresolvedCadence/CadenceCollapse";

        // Enigma Variations — CipherNocturne weapon-specific shaders
        public const string CipherBeamTrailShader = "EnigmaVariations/CipherNocturne/CipherBeamTrail";
        public const string CipherSnapBackShader = "EnigmaVariations/CipherNocturne/CipherSnapBack";

        // Enigma Variations — TheSilentMeasure weapon-specific shaders
        public const string SilentSeekerTrailShader = "EnigmaVariations/TheSilentMeasure/SilentSeekerTrail";
        public const string SilentQuestionBurstShader = "EnigmaVariations/TheSilentMeasure/SilentQuestionBurst";

        // Enigma Variations — TacetsEnigma weapon-specific shaders
        public const string TacetBulletTrailShader = "EnigmaVariations/TacetsEnigma/TacetBulletTrail";
        public const string TacetParadoxExplosionShader = "EnigmaVariations/TacetsEnigma/TacetParadoxExplosion";

        // Enigma Variations — TheWatchingRefrain weapon-specific shaders
        public const string WatchingPhantomAuraShader = "EnigmaVariations/TheWatchingRefrain/WatchingPhantomAura";
        public const string WatchingMysteryZoneShader = "EnigmaVariations/TheWatchingRefrain/WatchingMysteryZone";

        // Enigma Variations — DissonanceOfSecrets weapon-specific shaders
        public const string DissonanceOrbAuraShader = "EnigmaVariations/DissonanceOfSecrets/DissonanceOrbAura";
        public const string DissonanceRiddleTrailShader = "EnigmaVariations/DissonanceOfSecrets/DissonanceRiddleTrail";

        // Enigma Variations — FugueOfTheUnknown weapon-specific shaders
        public const string FugueVoiceTrailShader = "EnigmaVariations/FugueOfTheUnknown/FugueVoiceTrail";
        public const string FugueConvergenceShader = "EnigmaVariations/FugueOfTheUnknown/FugueConvergence";

        // Noise texture names (without extension) - in Assets/VFX/Noise/
        private static readonly string[] NoiseTextureNames = new[]
        {
            "PerlinNoise",
            "VoronoiNoise",
            "SimplexNoise",
            "TileableFBMNoise",
            "TileableMarbleNoise",
            "CosmicNebulaClouds",
            "CosmicEnergyVortex",
            "DestinyThreadPattern",
            "HorizontalBlackCoreCenterEnergyGradient",
            "HorizontalEnergyGradient",
            "MusicalWavePattern",
            "NebulaWispNoise",
            "NoiseSmoke",
            "RealityCrackPattern",
            "SoftCircularCaustics",
            "SparklyNoiseTexture",
            "StarFieldScatter",
            "UniversalRadialFlowNoise"
        };

        // Trail texture names (without extension) - in Assets/VFX/Trails/
        private static readonly string[] TrailTextureNames = new[]
        {
            "Comet Trail Gradient Fade",
            "Dissolving Particle Trail",
            "Ember Particle Scatter",
            "Full Rotation Spiral Trail",
            "Sparkle Particle Field"
        };

        /// <summary>
        /// True if shaders loaded successfully and are available for use.
        /// </summary>
        public static bool ShadersEnabled => _shadersEnabled;

        /// <summary>
        /// Number of noise textures successfully loaded.
        /// </summary>
        public static int LoadedNoiseTextureCount => _noiseTextures?.Count ?? 0;

        /// <summary>
        /// Number of trail textures successfully loaded.
        /// </summary>
        public static int LoadedTrailTextureCount => _trailTextures?.Count ?? 0;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            _shaders = new Dictionary<string, Effect>();
            _noiseTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _trailTextures = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            _initialized = false;
            _shadersEnabled = false;
        }

        public override void Unload()
        {
            if (_shaders != null)
            {
                // Cache references for main thread disposal
                var shadersCopy = new List<Effect>(_shaders.Values);
                _shaders.Clear();
                _shaders = null;

                // Queue shader disposal on main thread to avoid ThreadStateException
                Main.QueueMainThreadAction(() =>
                {
                    try
                    {
                        foreach (var shader in shadersCopy)
                        {
                            shader?.Dispose();
                        }
                    }
                    catch { }
                });
            }

            // Textures are managed by tModLoader's asset system; just clear references
            _noiseTextures?.Clear();
            _noiseTextures = null;
            _trailTextures?.Clear();
            _trailTextures = null;

            _initialized = false;
            _shadersEnabled = false;
        }

        /// <summary>
        /// Initializes and loads all shaders and textures.
        /// Called lazily on first use.
        /// </summary>
        private static void Initialize()
        {
            if (_initialized || Main.dedServ)
                return;

            _initialized = true;
            _shadersEnabled = false;

            var logger = ModContent.GetInstance<MagnumOpus>()?.Logger;

            // --- Load Shaders ---
            logger?.Info("ShaderLoader: Loading pre-compiled shaders from Effects/ ...");

            try
            {
                LoadShader(TrailShader);
                LoadShader(BloomShader);
                LoadShader(ScrollingTrailShader);
                LoadShader(CelestialValorTrailShader);
                LoadShader(MotionBlurBloomShader);
                LoadShader(TerraBladeSwingVFXShader);
                LoadShader(RadialScrollShaderName);
                LoadShader(BeamGradientFlowShader);
                LoadShader(MoonlightTrailShader);
                LoadShader(LunarBeamShader);
                LoadShader(CrescentAuraShader);
                LoadShader(HeroicFlameTrailShader);
                LoadShader(SakuraBloomShader);
                LoadShader(EroicaFuneralTrailShader);
                LoadShader(TriumphantFractalShaderName);
                LoadShader(NachtmusikStarTrailShader);
                LoadShader(NachtmusikSerenadeShader);
                LoadShader(TerraBladeFlareBeamShaderName);

                // Nachtmusik — per-weapon shaders
                LoadShader(ExecutionDecreeShader);
                LoadShader(CrescendoRiseShader);
                LoadShader(DimensionalRiftShader);
                LoadShader(StarChainBeamShader);
                LoadShader(NebulaScatterShader);
                LoadShader(StarHomingTrailShader);
                LoadShader(ConstellationWeaveShader);
                LoadShader(CosmicRequiemShader);
                LoadShader(ChorusSummonAuraShader);
                LoadShader(OvertureAuraShader);
                LoadShader(StellarConductorAuraShader);
                LoadShader(IncisorResonanceShader);
                LoadShader(ConstellationFieldShader);

                // Eroica — SakurasBlossom shaders
                LoadShader(SakuraSwingTrailShader);
                LoadShader(PetalDissolveShader);

                // Eroica — CelestialValor shaders
                LoadShader(ValorAuraShader);

                // Eroica — BlossomOfTheSakura shaders
                LoadShader(HeatDistortionShader);
                LoadShader(TracerTrailShader);

                // Eroica — PiercingLightOfTheSakura shaders
                LoadShader(CrescendoChargeShader);
                LoadShader(SakuraLightningTrailShader);

                // Eroica — FuneralPrayer shaders
                LoadShader(RequiemBeamShader);
                LoadShader(PrayerConvergenceShader);

                // Eroica — TriumphantFractal shaders
                LoadShader(SacredGeometryShader);

                // Eroica — FinalityOfTheSakura shaders
                LoadShader(FateSummonCircleShader);
                LoadShader(DarkFlameAuraShader);

                // Moonlight Sonata — EternalMoon shaders
                LoadShader(TidalTrailShader);
                LoadShader(CrescentBloomShader);
                LoadShader(LunarPhaseAuraShader);

                // Moonlight Sonata — MoonlightsCalling shaders
                LoadShader(PrismaticBeamShader);
                LoadShader(RefractionRippleShader);

                // Moonlight Sonata — ResurrectionOfTheMoon shaders
                LoadShader(CometTrailShader);
                LoadShader(SupernovaBlastShader);

                // Moonlight Sonata — StaffOfTheLunarPhases shaders
                LoadShader(GravitationalRiftShader);
                LoadShader(SummonCircleShader);

                // Swan Lake — CalloftheBlackSwan shaders
                LoadShader(DualPolaritySwingShader);
                LoadShader(SwanFlareTrailShader);

                // Swan Lake — CallofthePearlescentLake shaders
                LoadShader(PearlescentRocketTrailShader);
                LoadShader(LakeExplosionShader);

                // Swan Lake — ChromaticSwanSong shaders
                LoadShader(ChromaticTrailShader);
                LoadShader(AriaExplosionShader);

                // Swan Lake — FeatheroftheIridescentFlock shaders
                LoadShader(CrystalOrbitTrailShader);
                LoadShader(FlockAuraShader);

                // Swan Lake — IridescentWingspan shaders
                LoadShader(EtherealWingShader);
                LoadShader(WingspanFlareTrailShader);

                // Swan Lake — TheSwansLament shaders
                LoadShader(LamentBulletTrailShader);
                LoadShader(DestructionRevelationShader);

                // Clair de Lune — theme-wide shaders
                LoadShader(ClairDeLuneMoonlitShader);
                LoadShader(ClairDeLunePearlGlowShader);

                // Clair de Lune — per-weapon shaders
                LoadShader(TemporalDrillShader);
                LoadShader(CrystalLanceShader);
                LoadShader(GearSwingShader);
                LoadShader(ArcanePagesShader);
                LoadShader(CelestialOrbitShader);
                LoadShader(TimeFreezeSlashShader);
                LoadShader(StarfallTrailShader);
                LoadShader(GatlingBlurShader);
                LoadShader(SingularityPullShader);
                LoadShader(SoulBeamShader);
                LoadShader(JudgmentMarkShader);
                LoadShader(ResonanceFieldShader);

                // Enigma Variations — per-weapon shaders
                LoadShader(VoidSwingTrailShader);
                LoadShader(VoidBeamShader);
                LoadShader(CadenceSwingTrailShader);
                LoadShader(CadenceCollapseShader);
                LoadShader(CipherBeamTrailShader);
                LoadShader(CipherSnapBackShader);
                LoadShader(SilentSeekerTrailShader);
                LoadShader(SilentQuestionBurstShader);
                LoadShader(TacetBulletTrailShader);
                LoadShader(TacetParadoxExplosionShader);
                LoadShader(WatchingPhantomAuraShader);
                LoadShader(WatchingMysteryZoneShader);
                LoadShader(DissonanceOrbAuraShader);
                LoadShader(DissonanceRiddleTrailShader);
                LoadShader(FugueVoiceTrailShader);
                LoadShader(FugueConvergenceShader);

                _shadersEnabled = _shaders.Count > 0;

                if (_shadersEnabled)
                    logger?.Info($"ShaderLoader: {_shaders.Count} shader(s) loaded. VFX shaders ENABLED.");
                else
                    logger?.Warn("ShaderLoader: No shaders loaded. Using particle-based VFX fallback.");
            }
            catch (Exception ex)
            {
                logger?.Warn($"ShaderLoader: Shader init failed - {ex.Message}. Falling back to particles.");
                _shadersEnabled = false;
            }

            // --- Load VFX Textures ---
            logger?.Info("ShaderLoader: Loading VFX textures from Assets/VFX/ ...");

            int noiseLoaded = 0;
            foreach (string name in NoiseTextureNames)
            {
                if (LoadTexture($"MagnumOpus/Assets/VFX Asset Library/NoiseTextures/{name}", name, _noiseTextures))
                    noiseLoaded++;
            }

            int trailLoaded = 0;
            foreach (string name in TrailTextureNames)
            {
                // Spaces in filenames work fine with ModContent.Request
                if (LoadTexture($"MagnumOpus/Assets/SandboxLastPrism/Trails/{name}", name, _trailTextures))
                    trailLoaded++;
            }

            logger?.Info($"ShaderLoader: Loaded {noiseLoaded} noise texture(s), {trailLoaded} trail texture(s).");
        }

        /// <summary>
        /// Loads a single shader by name from the Effects/ folder.
        /// </summary>
        private static void LoadShader(string shaderName)
        {
            try
            {
                string path = $"MagnumOpus/Effects/{shaderName}";

                // Check existence BEFORE requesting to avoid tModLoader's
                // internal AssetRepository error dialog on missing assets.
                if (!ModContent.HasAsset(path))
                {
                    ModContent.GetInstance<MagnumOpus>()?.Logger.Warn(
                        $"ShaderLoader: Shader '{shaderName}' not found at '{path}' — skipping.");
                    return;
                }

                var effect = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad).Value;

                if (effect != null)
                {
                    _shaders[shaderName] = effect;
                }
                else
                {
                    ModContent.GetInstance<MagnumOpus>()?.Logger.Warn($"ShaderLoader: Shader '{shaderName}' loaded as null.");
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<MagnumOpus>()?.Logger.Warn($"ShaderLoader: Could not load shader '{shaderName}' - {ex.Message}");
            }
        }

        /// <summary>
        /// Loads a single texture by asset path and stores it in the given dictionary.
        /// Returns true on success.
        /// </summary>
        private static bool LoadTexture(string assetPath, string key, Dictionary<string, Texture2D> target)
        {
            try
            {
                var tex = ModContent.Request<Texture2D>(assetPath, AssetRequestMode.ImmediateLoad).Value;
                if (tex != null)
                {
                    target[key] = tex;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<MagnumOpus>()?.Logger.Warn($"ShaderLoader: Could not load texture '{key}' - {ex.Message}");
            }
            return false;
        }

        // =====================================================================
        //  Shader Accessors
        // =====================================================================

        /// <summary>
        /// Gets a loaded shader by name. Returns null if not found.
        /// </summary>
        public static Effect GetShader(string shaderName)
        {
            if (Main.dedServ)
                return null;

            if (!_initialized)
                Initialize();

            if (_shaders != null && _shaders.TryGetValue(shaderName, out Effect shader))
                return shader;

            return null;
        }

        /// <summary>
        /// Checks if a shader is available.
        /// </summary>
        public static bool HasShader(string shaderName)
        {
            if (!_initialized)
                Initialize();

            return _shaders != null && _shaders.ContainsKey(shaderName);
        }

        /// <summary>Gets the Trail shader if available.</summary>
        public static Effect Trail => GetShader(TrailShader);

        /// <summary>Gets the Bloom shader if available.</summary>
        public static Effect Bloom => GetShader(BloomShader);

        /// <summary>Gets the Scrolling Trail shader if available.</summary>
        public static Effect ScrollingTrail => GetShader(ScrollingTrailShader);

        /// <summary>Gets the Celestial Valor trail shader if available.</summary>
        public static Effect CelestialValorTrail => GetShader(CelestialValorTrailShader);

        /// <summary>Gets the Motion Blur Bloom shader if available.</summary>
        public static Effect MotionBlurBloom => GetShader(MotionBlurBloomShader);

        /// <summary>Gets the Terra Blade Swing VFX shader if available.</summary>
        public static Effect TerraBladeSwingVFX => GetShader(TerraBladeSwingVFXShader);

        /// <summary>Gets the Radial Scroll shader for orbs/explosions/portals if available.</summary>
        public static Effect RadialScroll => GetShader(RadialScrollShaderName);

        /// <summary>Gets the Beam Gradient Flow shader for flowing energy beams if available.</summary>
        public static Effect BeamGradientFlow => GetShader(BeamGradientFlowShader);

        /// <summary>Gets the Moonlight Trail shader for flowing lunar trails if available.</summary>
        public static Effect MoonlightTrail => GetShader(MoonlightTrailShader);

        /// <summary>Gets the Lunar Beam shader for crescent-shaped beam bodies if available.</summary>
        public static Effect LunarBeam => GetShader(LunarBeamShader);

        /// <summary>Gets the Crescent Aura shader for procedural crescent moon overlays if available.</summary>
        public static Effect CrescentAura => GetShader(CrescentAuraShader);

        /// <summary>Gets the Heroic Flame Trail shader for burning valor fire trails if available.</summary>
        public static Effect HeroicFlameTrail => GetShader(HeroicFlameTrailShader);

        /// <summary>Gets the Sakura Bloom shader for procedural petal bloom overlays if available.</summary>
        public static Effect SakuraBloom => GetShader(SakuraBloomShader);

        /// <summary>Gets the Eroica Funeral Trail shader for somber smoky flame trails if available.</summary>
        public static Effect EroicaFuneralTrail => GetShader(EroicaFuneralTrailShader);

        /// <summary>Gets the Triumphant Fractal shader for geometric hexagonal pattern trails if available.</summary>
        public static Effect TriumphantFractal => GetShader(TriumphantFractalShaderName);

        /// <summary>Gets the Nachtmusik Star Trail shader for twinkling star trails with playful nocturnal sparkle if available.</summary>
        public static Effect NachtmusikStarTrail => GetShader(NachtmusikStarTrailShader);

        /// <summary>Gets the Nachtmusik Serenade shader for soft starlit bloom with twinkling point-light sparkles if available.</summary>
        public static Effect NachtmusikSerenade => GetShader(NachtmusikSerenadeShader);

        /// <summary>Gets the Execution Decree shader for heavy void-rip slash trails on NocturnalExecutioner.</summary>
        public static Effect ExecutionDecree => GetShader(ExecutionDecreeShader);

        /// <summary>Gets the Crescendo Rise shader for intensity-building trails on MidnightsCrescendo.</summary>
        public static Effect CrescendoRise => GetShader(CrescendoRiseShader);

        /// <summary>Gets the Dimensional Rift shader for ultra-sharp dimensional tear trails on TwilightSeverance.</summary>
        public static Effect DimensionalRift => GetShader(DimensionalRiftShader);

        /// <summary>Gets the Star Chain Beam shader for precision constellation bullet trails on ConstellationPiercer.</summary>
        public static Effect StarChainBeam => GetShader(StarChainBeamShader);

        /// <summary>Gets the Nebula Scatter shader for gaseous nebula cloud trails on NebulasWhisper.</summary>
        public static Effect NebulaScatter => GetShader(NebulaScatterShader);

        /// <summary>Gets the Star Homing Trail shader for graceful arcing star ribbon trails on SerenadeOfDistantStars.</summary>
        public static Effect StarHomingTrail => GetShader(StarHomingTrailShader);

        /// <summary>Gets the Constellation Weave shader for charge-building star map pattern on StarweaversGrimoire.</summary>
        public static Effect ConstellationWeave => GetShader(ConstellationWeaveShader);

        /// <summary>Gets the Cosmic Requiem shader for channeled nebula-swirl beam on RequiemOfTheCosmos.</summary>
        public static Effect CosmicRequiem => GetShader(CosmicRequiemShader);

        /// <summary>Gets the Chorus Summon Aura shader for musical note constellation aura on CelestialChorusBaton.</summary>
        public static Effect ChorusSummonAura => GetShader(ChorusSummonAuraShader);

        /// <summary>Gets the Overture Aura shader for orchestral wave aura on GalacticOverture.</summary>
        public static Effect OvertureAura => GetShader(OvertureAuraShader);

        /// <summary>Gets the Stellar Conductor Aura shader for orbiting constellation ring aura on ConductorOfConstellations.</summary>
        public static Effect StellarConductorAura => GetShader(StellarConductorAuraShader);

        /// <summary>Gets the Terra Blade Flare Beam shader for wave-distorted motion-blur energy beams if available.</summary>
        public static Effect TerraBladeFlareBeam => GetShader(TerraBladeFlareBeamShaderName);

        /// <summary>Gets the Incisor Resonance shader for standing-wave resonance trails with constellation nodes if available.</summary>
        public static Effect IncisorResonance => GetShader(IncisorResonanceShader);

        /// <summary>Gets the Constellation Field shader for parallax starfield overlays on Incisor of Moonlight.</summary>
        public static Effect ConstellationField => GetShader(ConstellationFieldShader);

        /// <summary>Gets the Sakura Swing Trail shader for flowing petal-energy melee swing trails on SakurasBlossom.</summary>
        public static Effect SakuraSwingTrail => GetShader(SakuraSwingTrailShader);

        /// <summary>Gets the Petal Dissolve shader for noise-driven petal-shaped dissolution on Sakura's Blossom spectral copies.</summary>
        public static Effect PetalDissolve => GetShader(PetalDissolveShader);

        /// <summary>Gets the Valor Aura shader for concentric ember ring aura on Celestial Valor hold phase.</summary>
        public static Effect ValorAura => GetShader(ValorAuraShader);

        /// <summary>Gets the Heat Distortion shader for barrel heat mirage on Blossom of the Sakura.</summary>
        public static Effect HeatDistortion => GetShader(HeatDistortionShader);

        /// <summary>Gets the Tracer Trail shader for heat-reactive bullet trails on Blossom of the Sakura.</summary>
        public static Effect TracerTrail => GetShader(TracerTrailShader);

        /// <summary>Gets the Crescendo Charge shader for orbiting charge indicator on Piercing Light of the Sakura.</summary>
        public static Effect CrescendoCharge => GetShader(CrescendoChargeShader);

        /// <summary>Gets the Sakura Lightning Trail shader for zigzag lightning bolt trails on Piercing Light of the Sakura.</summary>
        public static Effect SakuraLightningTrail => GetShader(SakuraLightningTrailShader);

        /// <summary>Gets the Requiem Beam shader for electric tracking beam body on Funeral Prayer.</summary>
        public static Effect RequiemBeam => GetShader(RequiemBeamShader);

        /// <summary>Gets the Prayer Convergence shader for 5-beam convergence burst on Funeral Prayer.</summary>
        public static Effect PrayerConvergence => GetShader(PrayerConvergenceShader);

        /// <summary>Gets the Sacred Geometry shader for hexagram burst on Triumphant Fractal cast/impact.</summary>
        public static Effect SacredGeometry => GetShader(SacredGeometryShader);

        /// <summary>Gets the Fate Summon Circle shader for dark summoning ritual on Finality of the Sakura.</summary>
        public static Effect FateSummonCircle => GetShader(FateSummonCircleShader);

        /// <summary>Gets the Dark Flame Aura shader for inverted dark fire halo on Finality minion.</summary>
        public static Effect DarkFlameAura => GetShader(DarkFlameAuraShader);

        /// <summary>Gets the Tidal Trail shader for flowing water-like trail effects on EternalMoon.</summary>
        public static Effect TidalTrail => GetShader(TidalTrailShader);

        /// <summary>Gets the Crescent Bloom shader for procedural crescent moon bloom overlays on EternalMoon.</summary>
        public static Effect CrescentBloom => GetShader(CrescentBloomShader);

        /// <summary>Gets the Lunar Phase Aura shader for expanding concentric ring auras on EternalMoon.</summary>
        public static Effect LunarPhaseAura => GetShader(LunarPhaseAuraShader);

        /// <summary>Gets the Prismatic Beam shader for spectral color-splitting beam trails on MoonlightsCalling.</summary>
        public static Effect PrismaticBeam => GetShader(PrismaticBeamShader);

        /// <summary>Gets the Refraction Ripple shader for prismatic expanding ring effects on MoonlightsCalling bounces.</summary>
        public static Effect RefractionRipple => GetShader(RefractionRippleShader);

        /// <summary>Gets the Comet Trail shader for burning ember tail trails on Resurrection of the Moon.</summary>
        public static Effect CometTrail => GetShader(CometTrailShader);

        /// <summary>Gets the Supernova Blast shader for radial crater explosions on Resurrection of the Moon.</summary>
        public static Effect SupernovaBlast => GetShader(SupernovaBlastShader);

        /// <summary>Gets the Gravitational Rift shader for spiral gravity well distortion on Staff of the Lunar Phases.</summary>
        public static Effect GravitationalRift => GetShader(GravitationalRiftShader);

        /// <summary>Gets the Summon Circle shader for rotating lunar phase sigil on Staff of the Lunar Phases.</summary>
        public static Effect SummonCircle => GetShader(SummonCircleShader);

        /// <summary>Gets the Dual Polarity Swing shader for black/white melee trail on Call of the Black Swan.</summary>
        public static Effect DualPolaritySwing => GetShader(DualPolaritySwingShader);

        /// <summary>Gets the Swan Flare Trail shader for homing flare projectile trails on Call of the Black Swan.</summary>
        public static Effect SwanFlareTrail => GetShader(SwanFlareTrailShader);

        /// <summary>Gets the Pearlescent Rocket Trail shader for opal-shimmer projectile trails on Call of the Pearlescent Lake.</summary>
        public static Effect PearlescentRocketTrail => GetShader(PearlescentRocketTrailShader);

        /// <summary>Gets the Lake Explosion shader for concentric water-ripple explosions on Call of the Pearlescent Lake.</summary>
        public static Effect LakeExplosion => GetShader(LakeExplosionShader);

        /// <summary>Gets the Chromatic Trail shader for rainbow-banded projectile trails on Chromatic Swan Song.</summary>
        public static Effect ChromaticTrail => GetShader(ChromaticTrailShader);

        /// <summary>Gets the Aria Explosion shader for full-spectrum prismatic detonation on Chromatic Swan Song.</summary>
        public static Effect AriaExplosion => GetShader(AriaExplosionShader);

        /// <summary>Gets the Crystal Orbit Trail shader for faceted prismatic orbit trails on Feather of the Iridescent Flock.</summary>
        public static Effect CrystalOrbitTrail => GetShader(CrystalOrbitTrailShader);

        /// <summary>Gets the Flock Aura shader for concentric prismatic formation aura on Feather of the Iridescent Flock.</summary>
        public static Effect FlockAura => GetShader(FlockAuraShader);

        /// <summary>Gets the Ethereal Wing shader for procedural wing silhouette overlay on Iridescent Wingspan.</summary>
        public static Effect EtherealWing => GetShader(EtherealWingShader);

        /// <summary>Gets the Wingspan Flare Trail shader for feather-dissolve homing trails on Iridescent Wingspan.</summary>
        public static Effect WingspanFlareTrail => GetShader(WingspanFlareTrailShader);

        /// <summary>Gets the Lament Bullet Trail shader for muted sorrowful bullet trails on The Swan's Lament.</summary>
        public static Effect LamentBulletTrail => GetShader(LamentBulletTrailShader);

        /// <summary>Gets the Destruction Revelation shader for monochrome-to-prismatic explosion on The Swan's Lament.</summary>
        public static Effect DestructionRevelation => GetShader(DestructionRevelationShader);

        // =====================================================================
        //  Clair de Lune Shader Properties
        // =====================================================================

        /// <summary>Gets the Clair de Lune theme-wide moonlit pearl trail shader.</summary>
        public static Effect ClairDeLuneMoonlit => GetShader(ClairDeLuneMoonlitShader);

        /// <summary>Gets the Clair de Lune theme-wide pearl bloom/glow overlay shader.</summary>
        public static Effect ClairDeLunePearlGlow => GetShader(ClairDeLunePearlGlowShader);

        /// <summary>Gets the Temporal Drill shader for spiraling time-bore trails on Chronologicality.</summary>
        public static Effect TemporalDrill => GetShader(TemporalDrillShader);

        /// <summary>Gets the Crystal Lance shader for frost-crystal pierce trails on Temporal Piercer.</summary>
        public static Effect CrystalLance => GetShader(CrystalLanceShader);

        /// <summary>Gets the Gear Swing shader for brass pendulum arc on Clockwork Harmony.</summary>
        public static Effect GearSwing => GetShader(GearSwingShader);

        /// <summary>Gets the Arcane Pages shader for flowing script channel on Clockwork Grimoire.</summary>
        public static Effect ArcanePages => GetShader(ArcanePagesShader);

        /// <summary>Gets the Celestial Orbit shader for dream planetarium orbits on Orrery Of Dreams.</summary>
        public static Effect CelestialOrbit => GetShader(CelestialOrbitShader);

        /// <summary>Gets the Time Freeze Slash shader for reality-fracture sweeps on Requiem Of Time.</summary>
        public static Effect TimeFreezeSlash => GetShader(TimeFreezeSlashShader);

        /// <summary>Gets the Starfall Trail shader for falling star bolt trails on Starfall Whisper.</summary>
        public static Effect StarfallTrail => GetShader(StarfallTrailShader);

        /// <summary>Gets the Gatling Blur shader for clockwork barrel motion blur on Midnight Mechanism.</summary>
        public static Effect GatlingBlur => GetShader(GatlingBlurShader);

        /// <summary>Gets the Singularity Pull shader for gravitational vortex on Cog And Hammer.</summary>
        public static Effect SingularityPull => GetShader(SingularityPullShader);

        /// <summary>Gets the Soul Beam shader for moonlit soul tether beams on Lunar Phylactery.</summary>
        public static Effect SoulBeam => GetShader(SoulBeamShader);

        /// <summary>Gets the Judgment Mark shader for clockwork sigil branding on Gear-Driven Arbiter.</summary>
        public static Effect JudgmentMark => GetShader(JudgmentMarkShader);

        /// <summary>Gets the Resonance Field shader for harmonic pulse rings on Automaton's Tuning Fork.</summary>
        public static Effect ResonanceField => GetShader(ResonanceFieldShader);

        // =====================================================================
        //  Enigma Variations Shader Properties
        // =====================================================================

        /// <summary>Gets the Void Variation Swing Trail shader for Voronoi cellular fracture melee trails on Variations Of The Void.</summary>
        public static Effect VoidSwingTrail => GetShader(VoidSwingTrailShader);

        /// <summary>Gets the Void Variation Beam shader for tri-stream chromatic convergence beams on Variations Of The Void.</summary>
        public static Effect VoidBeam => GetShader(VoidBeamShader);

        /// <summary>Gets the Cadence Swing Trail shader for dimensional tear crack melee trails on The Unresolved Cadence.</summary>
        public static Effect CadenceSwingTrail => GetShader(CadenceSwingTrailShader);

        /// <summary>Gets the Cadence Collapse shader for geometric mandala implosion on The Unresolved Cadence.</summary>
        public static Effect CadenceCollapse => GetShader(CadenceCollapseShader);

        /// <summary>Gets the Cipher Beam Trail shader for digital data stream beam trails on Cipher Nocturne.</summary>
        public static Effect CipherBeamTrail => GetShader(CipherBeamTrailShader);

        /// <summary>Gets the Cipher SnapBack shader for clock-face sector starburst on Cipher Nocturne.</summary>
        public static Effect CipherSnapBack => GetShader(CipherSnapBackShader);

        /// <summary>Gets the Silent Seeker Trail shader for phantom echo multi-ghost trails on The Silent Measure.</summary>
        public static Effect SilentSeekerTrail => GetShader(SilentSeekerTrailShader);

        /// <summary>Gets the Silent Question Burst shader for question-mark silhouette explosion on The Silent Measure.</summary>
        public static Effect SilentQuestionBurst => GetShader(SilentQuestionBurstShader);

        /// <summary>Gets the Tacet Bullet Trail shader for crystalline fracture shard trails on Tacet's Enigma.</summary>
        public static Effect TacetBulletTrail => GetShader(TacetBulletTrailShader);

        /// <summary>Gets the Tacet Paradox Explosion shader for multi-ring moiré cascade on Tacet's Enigma.</summary>
        public static Effect TacetParadoxExplosion => GetShader(TacetParadoxExplosionShader);

        /// <summary>Gets the Watching Phantom Aura shader for procedural watching eye patterns on The Watching Refrain.</summary>
        public static Effect WatchingPhantomAura => GetShader(WatchingPhantomAuraShader);

        /// <summary>Gets the Watching Mystery Zone shader for panopticon surveillance grid on The Watching Refrain.</summary>
        public static Effect WatchingMysteryZone => GetShader(WatchingMysteryZoneShader);

        /// <summary>Gets the Dissonance Orb Aura shader for counter-rotating arcane circle aura on Dissonance Of Secrets.</summary>
        public static Effect DissonanceOrbAura => GetShader(DissonanceOrbAuraShader);

        /// <summary>Gets the Dissonance Riddle Trail shader for encrypted/decrypted segmented trail on Dissonance Of Secrets.</summary>
        public static Effect DissonanceRiddleTrail => GetShader(DissonanceRiddleTrailShader);

        /// <summary>Gets the Fugue Voice Trail shader for polyphonic audio spectrum trails on Fugue Of The Unknown.</summary>
        public static Effect FugueVoiceTrail => GetShader(FugueVoiceTrailShader);

        /// <summary>Gets the Fugue Convergence shader for standing wave interference pattern on Fugue Of The Unknown.</summary>
        public static Effect FugueConvergence => GetShader(FugueConvergenceShader);

        // =====================================================================
        //  Texture Accessors
        // =====================================================================

        /// <summary>
        /// Gets a noise texture by name (case-insensitive). Returns null if not found.
        /// Names match filenames without extension in Assets/VFX/Noise/.
        /// </summary>
        public static Texture2D GetNoiseTexture(string name)
        {
            if (Main.dedServ)
                return null;

            if (!_initialized)
                Initialize();

            if (_noiseTextures != null && _noiseTextures.TryGetValue(name, out Texture2D tex))
                return tex;

            return null;
        }

        /// <summary>
        /// Gets a trail texture by name (case-insensitive). Returns null if not found.
        /// Names match filenames without extension in Assets/VFX/Trails/.
        /// </summary>
        public static Texture2D GetTrailTexture(string name)
        {
            if (Main.dedServ)
                return null;

            if (!_initialized)
                Initialize();

            if (_trailTextures != null && _trailTextures.TryGetValue(name, out Texture2D tex))
                return tex;

            return null;
        }

        // =====================================================================
        //  Style → Default Texture Mapping
        // =====================================================================

        /// <summary>
        /// Returns the recommended default noise texture for a given SimpleTrailShader
        /// TrailStyle. Bind the result to device.Textures[1] before drawing.
        /// Returns null if the texture is not loaded (shader will use fallback float4(0.5)).
        /// </summary>
        /// <param name="style">Trail style enum value (Flame, Ice, Lightning, Nature, Cosmic).</param>
        public static Texture2D GetDefaultTrailStyleTexture(int style)
        {
            // Matches CalamityStyleTrailRenderer.TrailStyle enum order:
            // 0=Flame, 1=Ice, 2=Lightning, 3=Nature, 4=Cosmic
            string name = style switch
            {
                0 => "PerlinNoise",                // Flame — organic swirls
                1 => "SoftCircularCaustics",       // Ice — smooth caustic patterns
                2 => "SparklyNoiseTexture",        // Lightning — sharp sparkle patterns
                3 => "TileableFBMNoise",           // Nature — layered natural noise
                4 => "CosmicNebulaClouds",         // Cosmic — nebula cloud patterns
                _ => "PerlinNoise"                 // Fallback
            };
            return GetNoiseTexture(name);
        }

        /// <summary>
        /// Returns the recommended default noise texture for a given ScrollingTrailShader
        /// ScrollStyle. Bind the result to device.Textures[1] before drawing.
        /// Returns null if the texture is not loaded (shader will use fallback float4(0.5)).
        /// </summary>
        /// <param name="scrollStyle">Scroll style enum value (Flame, Cosmic, Energy, Void, Holy).</param>
        public static Texture2D GetDefaultScrollStyleTexture(int scrollStyle)
        {
            // Matches CalamityStyleTrailRenderer.ScrollStyle enum order:
            // 0=Flame, 1=Cosmic, 2=Energy, 3=Void, 4=Holy
            string name = scrollStyle switch
            {
                0 => "NoiseSmoke",                 // Flame — wispy smoke noise
                1 => "CosmicEnergyVortex",         // Cosmic — swirling vortex energy
                2 => "HorizontalEnergyGradient",   // Energy — horizontal flow gradient
                3 => "NebulaWispNoise",            // Void — dark nebula wisps
                4 => "UniversalRadialFlowNoise",   // Holy — radial emanation
                _ => "PerlinNoise"                 // Fallback
            };
            return GetNoiseTexture(name);
        }

        /// <summary>
        /// Checks whether any noise/trail textures are loaded for secondary sampler use.
        /// </summary>
        public static bool HasVFXTextures =>
            (_noiseTextures != null && _noiseTextures.Count > 0) ||
            (_trailTextures != null && _trailTextures.Count > 0);
    }
}
