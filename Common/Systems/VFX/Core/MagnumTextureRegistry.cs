using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Centralized VFX texture registry for MagnumOpus.
    /// Loads ALL textures from:
    ///   - Assets/VFX Asset Library/ (beams, gradients, glows, impacts, masks, noise, trails)
    ///   - Assets/SandboxLastPrism/ (orbs, flares, trails, gradients)
    ///   - Assets/Particles Asset Library/ (music notes, stars)
    /// Every content file should use this registry instead of vanilla TextureAssets placeholders.
    /// </summary>
    public class MagnumTextureRegistry : ModSystem
    {
        private const string VFX = "MagnumOpus/Assets/VFX Asset Library";
        private const string SLP = "MagnumOpus/Assets/SandboxLastPrism";
        private const string CE  = "MagnumOpus/Assets/SandboxChromaticEruption";
        private const string PV  = "MagnumOpus/Assets/SandboxPhotoviscerator";
        private const string PAL = "MagnumOpus/Assets/Particles Asset Library";

        // ═══════════════════════════════════════════════════════
        //  GLOW & BLOOM
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> SoftGlow          { get; private set; }
        public static Asset<Texture2D> SoftGlow64        { get; private set; }
        public static Asset<Texture2D> PointBloom        { get; private set; }
        public static Asset<Texture2D> SoftRadialBloom   { get; private set; }
        public static Asset<Texture2D> FeatherCircle     { get; private set; }
        public static Asset<Texture2D> WhiteFireEye      { get; private set; }
        public static Asset<Texture2D> EnergyFlare       { get; private set; }
        public static Asset<Texture2D> PartiGlow         { get; private set; }
        public static Asset<Texture2D> BloomCircle       { get; private set; }
        public static Asset<Texture2D> SmallBloom        { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  MASKS & SHAPES
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> HardCircleMask    { get; private set; }
        public static Asset<Texture2D> SmallHardCircle   { get; private set; }
        public static Asset<Texture2D> SoftCircle        { get; private set; }
        public static Asset<Texture2D> SquareMask        { get; private set; }
        public static Asset<Texture2D> VerticalEllipse   { get; private set; }
        public static Asset<Texture2D> WideSoftEllipse   { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  BEAMS & TRAILS
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> HorizontalBeam    { get; private set; }
        public static Asset<Texture2D> BasicTrail        { get; private set; }
        public static Asset<Texture2D> EnergyTex         { get; private set; }
        public static Asset<Texture2D> Extra196Black     { get; private set; }
        public static Asset<Texture2D> Spark06           { get; private set; }
        public static Asset<Texture2D> ThinGlowLine      { get; private set; }
        public static Asset<Texture2D> Trail5Loop        { get; private set; }
        public static Asset<Texture2D> GlowTrailClear    { get; private set; }
        public static Asset<Texture2D> ThinLineGlowClear { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  IMPACTS
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> ImpactEllipse     { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  NOISE TEXTURES (for shaders)
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> CosmicEnergyVortex     { get; private set; }
        public static Asset<Texture2D> CosmicNebulaClouds     { get; private set; }
        public static Asset<Texture2D> DestinyThreadPattern   { get; private set; }
        public static Asset<Texture2D> MusicalWavePattern     { get; private set; }
        public static Asset<Texture2D> NebulaWispNoise        { get; private set; }
        public static Asset<Texture2D> NoiseSmoke             { get; private set; }
        public static Asset<Texture2D> PerlinNoise            { get; private set; }
        public static Asset<Texture2D> RealityCrackPattern    { get; private set; }
        public static Asset<Texture2D> SimplexNoise           { get; private set; }
        public static Asset<Texture2D> SoftCircularCaustics   { get; private set; }
        public static Asset<Texture2D> StarFieldScatter       { get; private set; }
        public static Asset<Texture2D> TileableFBMNoise       { get; private set; }
        public static Asset<Texture2D> TileableMarbleNoise    { get; private set; }
        public static Asset<Texture2D> UniversalRadialFlow    { get; private set; }
        public static Asset<Texture2D> UVDistortionMap        { get; private set; }
        public static Asset<Texture2D> VoronoiEdgeNoise       { get; private set; }
        public static Asset<Texture2D> VoronoiCellNoise       { get; private set; }
        public static Asset<Texture2D> VoronoiNoise           { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  COLOR GRADIENTS (theme-specific LUT ramps)
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> EnigmaGradient         { get; private set; }
        public static Asset<Texture2D> EroicaGradient         { get; private set; }
        public static Asset<Texture2D> EroicaPaleGradient     { get; private set; }
        public static Asset<Texture2D> FateGradient           { get; private set; }
        public static Asset<Texture2D> LaCampanellaGradient   { get; private set; }
        public static Asset<Texture2D> MoonlightGradient      { get; private set; }
        public static Asset<Texture2D> SwanLakeGradient       { get; private set; }
        public static Asset<Texture2D> RainbowGradient        { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  FLARES (SandboxLastPrism)
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> Flare16               { get; private set; }
        public static Asset<Texture2D> SimpleLensFlare       { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  SMOKE / MIST (SandboxChromaticEruption)
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> HeavySmokeParticle    { get; private set; }
        public static Asset<Texture2D> MediumMist            { get; private set; }
        public static Asset<Texture2D> SmallGreyscaleCircle  { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  MUSIC NOTES (Particles Asset Library)
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> MusicNote             { get; private set; }
        public static Asset<Texture2D> CursiveMusicNote      { get; private set; }
        public static Asset<Texture2D> MusicNoteWithSlashes  { get; private set; }
        public static Asset<Texture2D> QuarterNote           { get; private set; }
        public static Asset<Texture2D> TallMusicNote         { get; private set; }
        public static Asset<Texture2D> WholeNote             { get; private set; }

        // ═══════════════════════════════════════════════════════
        //  STARS (Particles Asset Library)
        // ═══════════════════════════════════════════════════════
        public static Asset<Texture2D> Star4PointHard        { get; private set; }
        public static Asset<Texture2D> Star4PointSoft        { get; private set; }
        public static Asset<Texture2D> StarThinTall          { get; private set; }

        // ═══════════════════════════════════════════════════════

        private static Texture2D _heavySmokeTexture;
        private static Texture2D _pixelTexture;

        public static bool TexturesLoaded { get; private set; }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            // ── Glow & Bloom ──
            SoftGlow        = SafeLoad($"{SLP}/Orbs/SoftGlow");
            SoftGlow64      = SafeLoad($"{SLP}/Orbs/SoftGlow64");
            PointBloom      = SafeLoad($"{VFX}/GlowAndBloom/PointBloom");
            SoftRadialBloom = SafeLoad($"{VFX}/GlowAndBloom/SoftRadialBloom");
            FeatherCircle   = SafeLoad($"{SLP}/Orbs/feather_circle128PMA");
            WhiteFireEye    = SafeLoad($"{SLP}/Orbs/whiteFireEyeA");
            EnergyFlare     = SafeLoad($"{SLP}/Pixel/Flare");
            PartiGlow       = SafeLoad($"{SLP}/Pixel/PartiGlow");
            BloomCircle     = SafeLoad($"{PV}/Particle_BloomCircle");
            SmallBloom      = SafeLoad($"{PV}/Particle_SmallBloom");

            // ── Masks & Shapes ──
            HardCircleMask  = SafeLoad($"{VFX}/MasksAndShapes/HardCircleMask");
            SmallHardCircle = SafeLoad($"{VFX}/MasksAndShapes/SmallHardCircleMask");
            SoftCircle      = SafeLoad($"{VFX}/MasksAndShapes/SoftCircle");
            SquareMask      = SafeLoad($"{VFX}/MasksAndShapes/SquareMask");
            VerticalEllipse = SafeLoad($"{VFX}/MasksAndShapes/VerticalEllipse");
            WideSoftEllipse = SafeLoad($"{VFX}/MasksAndShapes/WideSoftEllipse");

            // ── Beams & Trails ──
            HorizontalBeam    = SafeLoad($"{VFX}/BeamTextures/HorizontalBeamStreakSegment");
            BasicTrail        = SafeLoad($"{VFX}/TrailsAndRibbons/BasicTrail");
            EnergyTex         = SafeLoad($"{SLP}/Trails/EnergyTex");
            Extra196Black     = SafeLoad($"{SLP}/Trails/Extra_196_Black");
            Spark06           = SafeLoad($"{SLP}/Trails/spark_06");
            ThinGlowLine      = SafeLoad($"{SLP}/Trails/ThinGlowLine");
            Trail5Loop        = SafeLoad($"{SLP}/Trails/Trail5Loop");
            GlowTrailClear    = SafeLoad($"{SLP}/Trails/Clear/GlowTrailClear");
            ThinLineGlowClear = SafeLoad($"{SLP}/Trails/Clear/ThinLineGlowClear");

            // ── Impacts ──
            ImpactEllipse = SafeLoad($"{VFX}/ImpactEffects/ImpactEllipse");

            // ── Noise ──
            CosmicEnergyVortex   = SafeLoad($"{VFX}/NoiseTextures/CosmicEnergyVortex");
            CosmicNebulaClouds   = SafeLoad($"{VFX}/NoiseTextures/CosmicNebulaClouds");
            DestinyThreadPattern = SafeLoad($"{VFX}/NoiseTextures/DestinyThreadPattern");
            MusicalWavePattern   = SafeLoad($"{VFX}/NoiseTextures/MusicalWavePattern");
            NebulaWispNoise      = SafeLoad($"{VFX}/NoiseTextures/NebulaWispNoise");
            NoiseSmoke           = SafeLoad($"{VFX}/NoiseTextures/NoiseSmoke");
            PerlinNoise          = SafeLoad($"{VFX}/NoiseTextures/PerlinNoise");
            RealityCrackPattern  = SafeLoad($"{VFX}/NoiseTextures/RealityCrackPattern");
            SimplexNoise         = SafeLoad($"{VFX}/NoiseTextures/SimplexNoise");
            SoftCircularCaustics = SafeLoad($"{VFX}/NoiseTextures/SoftCircularCaustics");
            StarFieldScatter     = SafeLoad($"{VFX}/NoiseTextures/StarFieldScatter");
            TileableFBMNoise     = SafeLoad($"{VFX}/NoiseTextures/TileableFBMNoise");
            TileableMarbleNoise  = SafeLoad($"{VFX}/NoiseTextures/TileableMarbleNoise");
            UniversalRadialFlow  = SafeLoad($"{VFX}/NoiseTextures/UniversalRadialFlowNoise");
            UVDistortionMap      = SafeLoad($"{VFX}/NoiseTextures/UVDistortionMap");
            VoronoiEdgeNoise     = SafeLoad($"{VFX}/NoiseTextures/VornoiEdgeNoise");
            VoronoiCellNoise     = SafeLoad($"{VFX}/NoiseTextures/VoronoiCellNoise");
            VoronoiNoise         = SafeLoad($"{VFX}/NoiseTextures/VoronoiNoise");

            // ── Color Gradients ──
            EnigmaGradient       = SafeLoad($"{VFX}/ColorGradients/EnigmaGradientLUTandRAMP");
            EroicaGradient       = SafeLoad($"{VFX}/ColorGradients/EroicaGradientLUTandRAMP");
            EroicaPaleGradient   = SafeLoad($"{VFX}/ColorGradients/EroicaGradientPALELUTandRAMP");
            FateGradient         = SafeLoad($"{VFX}/ColorGradients/FateGradientLUTandRAMP");
            LaCampanellaGradient = SafeLoad($"{VFX}/ColorGradients/LaCampanellaGradientLUTandRAMP");
            MoonlightGradient    = SafeLoad($"{VFX}/ColorGradients/MoonlightSonataGradientLUTandRAMP");
            SwanLakeGradient     = SafeLoad($"{VFX}/ColorGradients/SwanLakeGradient");
            RainbowGradient      = SafeLoad($"{SLP}/Gradients/RainbowGrad1");

            // ── Flares ──
            Flare16        = SafeLoad($"{SLP}/Flare/flare_16");
            SimpleLensFlare = SafeLoad($"{SLP}/Flare/Simple Lens Flare_11");

            // ── Smoke / Mist ──
            HeavySmokeParticle  = SafeLoad($"{CE}/Particle_HeavySmoke");
            MediumMist          = SafeLoad($"{CE}/Particle_MediumMist");
            SmallGreyscaleCircle = SafeLoad($"{CE}/SmallGreyscaleCircle");

            // ── Music Notes ──
            MusicNote           = SafeLoad($"{PAL}/MusicNote");
            CursiveMusicNote    = SafeLoad($"{PAL}/CursiveMusicNote");
            MusicNoteWithSlashes = SafeLoad($"{PAL}/MusicNoteWithSlashes");
            QuarterNote         = SafeLoad($"{PAL}/QuarterNote");
            TallMusicNote       = SafeLoad($"{PAL}/TallMusicNote");
            WholeNote           = SafeLoad($"{PAL}/WholeNote");

            // ── Stars ──
            Star4PointHard = SafeLoad($"{PAL}/Stars/4PointedStarHard");
            Star4PointSoft = SafeLoad($"{PAL}/Stars/4PointedStarSoft");
            StarThinTall   = SafeLoad($"{PAL}/Stars/ThinTall4PointedStar");

            TexturesLoaded = true;
        }

        public override void Unload()
        {
            // Null all asset references (GC handles Asset<T>)
            SoftGlow = SoftGlow64 = PointBloom = SoftRadialBloom = null;
            FeatherCircle = WhiteFireEye = EnergyFlare = PartiGlow = null;
            BloomCircle = SmallBloom = null;
            HardCircleMask = SmallHardCircle = SoftCircle = SquareMask = null;
            VerticalEllipse = WideSoftEllipse = null;
            HorizontalBeam = BasicTrail = EnergyTex = Extra196Black = null;
            Spark06 = ThinGlowLine = Trail5Loop = GlowTrailClear = ThinLineGlowClear = null;
            ImpactEllipse = null;
            CosmicEnergyVortex = CosmicNebulaClouds = DestinyThreadPattern = null;
            MusicalWavePattern = NebulaWispNoise = NoiseSmoke = PerlinNoise = null;
            RealityCrackPattern = SimplexNoise = SoftCircularCaustics = null;
            StarFieldScatter = TileableFBMNoise = TileableMarbleNoise = null;
            UniversalRadialFlow = UVDistortionMap = VoronoiEdgeNoise = null;
            VoronoiCellNoise = VoronoiNoise = null;
            EnigmaGradient = EroicaGradient = EroicaPaleGradient = FateGradient = null;
            LaCampanellaGradient = MoonlightGradient = SwanLakeGradient = RainbowGradient = null;
            Flare16 = SimpleLensFlare = null;
            HeavySmokeParticle = MediumMist = SmallGreyscaleCircle = null;
            MusicNote = CursiveMusicNote = MusicNoteWithSlashes = QuarterNote = null;
            TallMusicNote = WholeNote = null;
            Star4PointHard = Star4PointSoft = StarThinTall = null;

            _heavySmokeTexture?.Dispose();
            _heavySmokeTexture = null;
            _pixelTexture?.Dispose();
            _pixelTexture = null;
            TexturesLoaded = false;
        }

        private Asset<Texture2D> SafeLoad(string path)
        {
            try
            {
                return ModContent.Request<Texture2D>(path, AssetRequestMode.AsyncLoad);
            }
            catch
            {
                Mod.Logger.Warn($"MagnumTextureRegistry: Texture not found: {path}");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  HELPER ACCESSORS (backward-compatible + new)
        // ═══════════════════════════════════════════════════════

        private static Texture2D Get(Asset<Texture2D> asset)
            => asset?.IsLoaded == true ? asset.Value : null;

        // ── Glow ──
        public static Texture2D GetBloom()           => Get(PointBloom) ?? Get(SoftRadialBloom) ?? Get(SoftGlow);
        public static Texture2D GetSoftGlow()        => Get(SoftGlow) ?? GetBloom();
        public static Texture2D GetPointBloom()      => Get(PointBloom) ?? GetBloom();
        public static Texture2D GetRadialBloom()     => Get(SoftRadialBloom) ?? GetBloom();
        public static Texture2D GetFeatherGlow()     => Get(FeatherCircle) ?? GetBloom();
        public static Texture2D GetFireEye()         => Get(WhiteFireEye) ?? GetBloom();
        public static Texture2D GetBloomCircle()     => Get(BloomCircle) ?? GetBloom();
        public static Texture2D GetSmallBloom()      => Get(SmallBloom) ?? GetBloom();

        // ── Flares ──
        public static Texture2D GetFlare()           => Get(EnergyFlare) ?? GetBloom();
        public static Texture2D GetEnergyFlare()     => GetFlare();
        public static Texture2D GetShineFlare4Point() => Get(Star4PointHard) ?? GetFlare();
        public static Texture2D GetFlare16()         => Get(Flare16) ?? GetFlare();
        public static Texture2D GetLensFlare()       => Get(SimpleLensFlare) ?? GetFlare();
        public static Texture2D GetPartiGlow()       => Get(PartiGlow) ?? GetFlare();

        // ── Masks ──
        public static Texture2D GetHaloRing()        => Get(SoftCircle);
        public static Texture2D GetHardCircle()      => Get(HardCircleMask) ?? Get(SoftCircle);
        public static Texture2D GetSmallCircle()     => Get(SmallHardCircle) ?? Get(HardCircleMask);
        public static Texture2D GetEllipse()         => Get(VerticalEllipse) ?? Get(SoftCircle);
        public static Texture2D GetWideEllipse()     => Get(WideSoftEllipse) ?? Get(VerticalEllipse);
        public static Texture2D GetImpactEllipse()   => Get(ImpactEllipse) ?? GetEllipse();

        // ── Beams & Trails ──
        public static Texture2D GetBeamStreak()      => Get(HorizontalBeam) ?? Get(ThinGlowLine);
        public static Texture2D GetBasicTrail()      => Get(BasicTrail) ?? Get(ThinGlowLine);
        public static Texture2D GetEnergyTrail()     => Get(EnergyTex) ?? GetBasicTrail();
        public static Texture2D GetSparkTrail()      => Get(Spark06) ?? GetBasicTrail();
        public static Texture2D GetThinGlow()        => Get(ThinGlowLine);
        public static Texture2D GetTrailLoop()       => Get(Trail5Loop) ?? GetBasicTrail();
        public static Texture2D GetClearTrail()      => Get(GlowTrailClear) ?? GetBasicTrail();
        public static Texture2D GetThinClearTrail()  => Get(ThinLineGlowClear) ?? GetClearTrail();

        // ── Noise (for shader sampler parameters) ──
        public static Texture2D GetPerlinNoise()     => Get(PerlinNoise);
        public static Texture2D GetSimplexNoise()    => Get(SimplexNoise);
        public static Texture2D GetFBMNoise()        => Get(TileableFBMNoise) ?? GetPerlinNoise();
        public static Texture2D GetVoronoiNoise()    => Get(VoronoiNoise);
        public static Texture2D GetSmokeNoise()      => Get(NoiseSmoke) ?? GetPerlinNoise();
        public static Texture2D GetCosmicVortex()    => Get(CosmicEnergyVortex);
        public static Texture2D GetNebulaClouds()    => Get(CosmicNebulaClouds);
        public static Texture2D GetMusicalWave()     => Get(MusicalWavePattern);
        public static Texture2D GetDistortionMap()   => Get(UVDistortionMap) ?? GetPerlinNoise();

        // ── Color Gradients (theme LUT ramps for shader sampling) ──
        public static Texture2D GetGradient(string theme) => theme?.ToLower() switch
        {
            "enigma"       => Get(EnigmaGradient),
            "eroica"       => Get(EroicaGradient),
            "eroicapale"   => Get(EroicaPaleGradient),
            "fate"         => Get(FateGradient),
            "lacampanella" => Get(LaCampanellaGradient),
            "moonlight"    => Get(MoonlightGradient),
            "swanlake"     => Get(SwanLakeGradient),
            "rainbow"      => Get(RainbowGradient),
            _ => Get(RainbowGradient)
        };

        // ── Smoke / Mist ──
        public static Texture2D GetCloudSmoke()      => Get(HeavySmokeParticle) ?? Get(MediumMist) ?? GetSoftGlow();
        public static Texture2D GetMediumMist()      => Get(MediumMist) ?? GetCloudSmoke();
        public static Texture2D GetSmallSmoke()      => Get(SmallGreyscaleCircle) ?? GetSoftGlow();

        public static Texture2D GetHeavySmoke()
        {
            if (_heavySmokeTexture == null || _heavySmokeTexture.IsDisposed)
                _heavySmokeTexture = ParticleTextureGenerator.HeavySmoke;
            return _heavySmokeTexture ?? GetCloudSmoke();
        }

        // ── Music Notes ──
        public static Texture2D GetMusicNote()       => Get(MusicNote);
        public static Texture2D GetCursiveNote()     => Get(CursiveMusicNote) ?? GetMusicNote();
        public static Texture2D GetQuarterNote()     => Get(QuarterNote) ?? GetMusicNote();
        public static Texture2D GetWholeNote()       => Get(WholeNote) ?? GetMusicNote();
        public static Texture2D GetRandomNote()
        {
            return Main.rand.Next(6) switch
            {
                0 => Get(MusicNote),
                1 => Get(CursiveMusicNote),
                2 => Get(MusicNoteWithSlashes),
                3 => Get(QuarterNote),
                4 => Get(TallMusicNote),
                _ => Get(WholeNote)
            } ?? Get(MusicNote);
        }

        // ── Stars ──
        public static Texture2D GetStar4Hard()       => Get(Star4PointHard);
        public static Texture2D GetStar4Soft()       => Get(Star4PointSoft) ?? GetStar4Hard();
        public static Texture2D GetStarThin()        => Get(StarThinTall) ?? GetStar4Hard();

        // ── Pixel (primitive drawing) ──
        public static Texture2D GetPixelTexture()
        {
            if (_pixelTexture == null || _pixelTexture.IsDisposed)
            {
                _pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });
            }
            return _pixelTexture;
        }
    }
}
