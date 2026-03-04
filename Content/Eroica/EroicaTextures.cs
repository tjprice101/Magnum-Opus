using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica
{
    /// <summary>
    /// Centralized texture registry for ALL Eroica-themed VFX.
    /// Follows the SLPCommonTextures / MoonlightSonataTextures pattern.
    /// 
    /// Every Eroica weapon, projectile, dust, and VFX helper references
    /// textures through this class instead of inline ModContent.Request calls.
    /// Textures are loaded once at mod init via ImmediateLoad.
    /// 
    /// Implemented as a ModSystem so tModLoader calls Load()/Unload() automatically.
    /// </summary>
    public class EroicaTexturesSystem : ModSystem
    {
        public override void OnModLoad()
        {
            EroicaTextures.Load();
        }

        public override void Unload()
        {
            EroicaTextures.Unload();
        }
    }

    public static class EroicaTextures
    {
        // ══════════════════════════════════════════════════════╁E
        //  SHARED  Eused across ALL Eroica weapons
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Soft circular glow for tip blooms and ambient light.</summary>
        public static Asset<Texture2D> SoftGlow { get; private set; }

        /// <summary>Energy flare for blade tip and impact bursts.</summary>
        public static Asset<Texture2D> EnergyFlare { get; private set; }

        /// <summary>4-pointed star for heroic sparkles.</summary>
        public static Asset<Texture2D> Star4Point { get; private set; }

        /// <summary>Glowing halo ring for expanding shockwaves.</summary>
        public static Asset<Texture2D> HaloRing { get; private set; }

        /// <summary>Circular mask for radial effects.</summary>
        public static Asset<Texture2D> CircularMask { get; private set; }

        /// <summary>Bloom orb for soft layered bloom.</summary>
        public static Asset<Texture2D> BloomOrb { get; private set; }

        /// <summary>Full slash arc overlay for finisher strikes.</summary>
        public static Asset<Texture2D> FullSlashArc { get; private set; }

        /// <summary>Flaming arc for heroic flame sword arcs.</summary>
        public static Asset<Texture2D> FlamingArc { get; private set; }

        /// <summary>Music note particle sprite for musical VFX.</summary>
        public static Asset<Texture2D> MusicNote { get; private set; }

        /// <summary>Cursive music note for elegant note particles.</summary>
        public static Asset<Texture2D> CursiveMusicNote { get; private set; }

        /// <summary>Flame impact explosion for heavy hits.</summary>
        public static Asset<Texture2D> FlameImpact { get; private set; }

        /// <summary>Radial god rays for dramatic impact bursts.</summary>
        public static Asset<Texture2D> GodRays { get; private set; }

        /// <summary>Horizontal anamorphic streak for bloom flares.</summary>
        public static Asset<Texture2D> AnamorphicStreak { get; private set; }

        /// <summary>Combo finisher impact arc slash.</summary>
        public static Asset<Texture2D> ComboFinisherArc { get; private set; }

        /// <summary>Expanding shockwave ring texture.</summary>
        public static Asset<Texture2D> ShockwaveRing { get; private set; }

        /// <summary>Directional hit slash mark for melee impacts.</summary>
        public static Asset<Texture2D> HitSlashMark { get; private set; }

        /// <summary>Rose's bud particle for sakura effects.</summary>
        public static Asset<Texture2D> RoseBud { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  NOISE TEXTURES  Efor shader sampling
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Perlin noise for flame distortion.</summary>
        public static Asset<Texture2D> PerlinNoise { get; private set; }

        /// <summary>Cosmic energy vortex noise for heroic aura.</summary>
        public static Asset<Texture2D> CosmicEnergyNoise { get; private set; }

        /// <summary>Musical wave pattern noise for harmonic effects.</summary>
        public static Asset<Texture2D> MusicalWaveNoise { get; private set; }

        /// <summary>Sparkly noise for valor shimmer.</summary>
        public static Asset<Texture2D> SparklyNoise { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  TRAIL TEXTURES
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Comet trail gradient for projectile tails.</summary>
        public static Asset<Texture2D> CometTrailGradient { get; private set; }

        /// <summary>Ember particle scatter trail texture.</summary>
        public static Asset<Texture2D> EmberScatter { get; private set; }

        /// <summary>Energy trail UV map for shader-driven trails.</summary>
        public static Asset<Texture2D> EnergyTrailUV { get; private set; }

        /// <summary>Sparkle particle field for trail sparkles.</summary>
        public static Asset<Texture2D> SparkleField { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  LUT TEXTURES
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Eroica gradient LUT for color grading.</summary>
        public static Asset<Texture2D> EroicaLUT { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  CELESTIAL VALOR  EHeroic flame blade VFX
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Celestial Valor blade sprite for custom swing drawing.</summary>
        public static Asset<Texture2D> CelestialValorBlade { get; private set; }

        /// <summary>Celestial Valor swing texture (arc shape).</summary>
        public static Asset<Texture2D> CelestialValorSwingTex { get; private set; }

        /// <summary>Celestial Valor projectile sprite.</summary>
        public static Asset<Texture2D> CelestialValorProjectileTex { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  SAKURA'S BLOSSOM  EPetal storm blade VFX
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Sakura's Blossom blade sprite.</summary>
        public static Asset<Texture2D> SakurasBlossomBlade { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  SMEAR / ARC TEXTURES (from Assets/Particles Asset Library/)
        // ══════════════════════════════════════════════════════╁E

        /// <summary>Sword arc 1  Ethin elegant slash.</summary>
        public static Asset<Texture2D> SwordArc1 { get; private set; }
        /// <summary>Sword arc 2  Estandard slash arc.</summary>
        public static Asset<Texture2D> SwordArc2 { get; private set; }
        /// <summary>Sword arc 3  Ewide sweeping slash.</summary>
        public static Asset<Texture2D> SwordArc3 { get; private set; }
        /// <summary>Sword arc 4  Eheavy overhead arc.</summary>
        public static Asset<Texture2D> SwordArc4 { get; private set; }
        /// <summary>Sword arc 5  Ecrescent moon shape.</summary>
        public static Asset<Texture2D> SwordArc5 { get; private set; }
        /// <summary>Sword arc 6  Edual crossing arcs.</summary>
        public static Asset<Texture2D> SwordArc6 { get; private set; }
        /// <summary>Sword arc 7  Erising flame arc.</summary>
        public static Asset<Texture2D> SwordArc7 { get; private set; }
        /// <summary>Sword arc 8  Efull rotation sweep.</summary>
        public static Asset<Texture2D> SwordArc8 { get; private set; }

        /// <summary>Flaming arc sword slash  Eheroic fire style.</summary>
        public static Asset<Texture2D> FlamingArcSwordSlash { get; private set; }
        /// <summary>Sword arc slash wave  Eenergy wave style.</summary>
        public static Asset<Texture2D> SwordArcSlashWave { get; private set; }
        /// <summary>Simple arc sword slash  Eclean slash.</summary>
        public static Asset<Texture2D> SimpleArcSwordSlash { get; private set; }
        /// <summary>Curved sword slash  Ecurved motion trail.</summary>
        public static Asset<Texture2D> CurvedSwordSlash { get; private set; }

        // ══════════════════════════════════════════════════════╁E
        //  INITIALIZATION
        // ══════════════════════════════════════════════════════╁E

        private static bool _loaded;

        /// <summary>
        /// Call once during mod Load to pre-cache all textures.
        /// Safe to call multiple times (idempotent).
        /// </summary>
        public static void Load()
        {
            if (_loaded) return;
            _loaded = true;

            // Shared — remapped to existing VFX Asset Library + SandboxLastPrism assets
            // SoftGlow: radial bloom orb for tip glows, ambient bloom
            SoftGlow = Req("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            // EnergyFlare: bright point flare for blade tips and impact bursts
            EnergyFlare = Req("MagnumOpus/Assets/SandboxLastPrism/Pixel/Flare");
            // Star4Point: crisp 4-pointed star for heroic sparkles
            Star4Point = Req("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard");
            // HaloRing: soft circle used as expanding shockwave ring via scale animation
            HaloRing = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle");
            // CircularMask: hard-edge circular mask for radial shader effects
            CircularMask = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/HardCircleMask");
            // BloomOrb: small soft glow for layered bloom stacking
            BloomOrb = Req("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64");
            // FullSlashArc: wide soft ellipse stretched + rotated creates convincing slash arc overlay
            FullSlashArc = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse");
            // FlamingArc: impact ellipse drawn with additive blend + scale creates flame arc effect
            FlamingArc = Req("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse");
            // Music notes — these exist in the Particles Asset Library
            MusicNote = Req("MagnumOpus/Assets/Particles Asset Library/MusicNote");
            CursiveMusicNote = Req("MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote");
            // FlameImpact: SoftRadialBloom scaled up with additive orange tint = fire explosion effect
            FlameImpact = Req("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            // GodRays: radial flow noise used as god rays via radial scroll shader
            GodRays = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/UniversalRadialFlowNoise");
            // AnamorphicStreak: horizontal beam streak segment stretched = anamorphic flare
            AnamorphicStreak = Req("MagnumOpus/Assets/VFX Asset Library/BeamTextures/HorizontalBeamStreakSegment");
            // ComboFinisherArc: impact ellipse for finisher arc VFX
            ComboFinisherArc = Req("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse");
            // ShockwaveRing: soft circle scaled rapidly outward = expanding shockwave
            ShockwaveRing = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle");
            // HitSlashMark: vertical ellipse rotated to match hit angle = directional slash mark
            HitSlashMark = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse");
            // RoseBud: 4-pointed soft star tinted pink = sakura petal substitute
            RoseBud = Req("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft");

            // Noise — remapped to VFX Asset Library/NoiseTextures/
            PerlinNoise = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise");
            CosmicEnergyNoise = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/CosmicEnergyVortex");
            MusicalWaveNoise = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern");
            // SparklyNoise: SimplexNoise provides fine-grain sparkle-like noise pattern
            SparklyNoise = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/SimplexNoise");

            // Trails — remapped to existing trail + VFX assets
            // CometTrailGradient: Eroica gradient LUT used as comet trail color ramp
            CometTrailGradient = Req("MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientLUTandRAMP");
            // EmberScatter: star field scatter provides scattered point pattern similar to ember scatter
            EmberScatter = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/StarFieldScatter");
            // EnergyTrailUV: EnergyTex provides clean energy UV map for shader trails
            EnergyTrailUV = Req("MagnumOpus/Assets/SandboxLastPrism/Trails/EnergyTex");
            // SparkleField: Voronoi cell noise makes excellent sparkle/shimmer field
            SparkleField = Req("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiCellNoise");

            // LUT — remapped to ColorGradients
            EroicaLUT = Req("MagnumOpus/Assets/VFX Asset Library/ColorGradients/EroicaGradientLUTandRAMP");

            // Celestial Valor
            CelestialValorBlade = Req("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor");
            CelestialValorSwingTex = Req("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValorSwing");
            CelestialValorProjectileTex = Req("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValorProjectile");

            // Sakura's Blossom
            SakurasBlossomBlade = Req("MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom");

            // Smears / Arcs — remapped to available mask and shape textures.
            // Sword arcs use ImpactEllipse, WideSoftEllipse, VerticalEllipse, and BasicTrail
            // rotated/scaled/tinted differently per draw call for visual variety.
            // The arc "shape" comes from the elliptical masks combined with additive blending.
            SwordArc1 = Req("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse");
            SwordArc2 = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse");
            SwordArc3 = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse");
            SwordArc4 = Req("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse");
            SwordArc5 = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse");
            SwordArc6 = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse");
            SwordArc7 = Req("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse");
            SwordArc8 = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse");
            // FlamingArcSwordSlash: impact ellipse with flame tint
            FlamingArcSwordSlash = Req("MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse");
            // SwordArcSlashWave: BasicTrail stretched wide = wave slash
            SwordArcSlashWave = Req("MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/BasicTrail");
            // SimpleArcSwordSlash: clean soft ellipse
            SimpleArcSwordSlash = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse");
            // CurvedSwordSlash: vertical ellipse for curved motion trail
            CurvedSwordSlash = Req("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse");
        }

        public static void Unload()
        {
            _loaded = false;
            // Asset<T> references are managed by tModLoader  Ejust null out refs
            SoftGlow = null;
            EnergyFlare = null;
            Star4Point = null;
            HaloRing = null;
            CircularMask = null;
            BloomOrb = null;
            FullSlashArc = null;
            FlamingArc = null;
            MusicNote = null;
            CursiveMusicNote = null;
            FlameImpact = null;
            GodRays = null;
            AnamorphicStreak = null;
            ComboFinisherArc = null;
            ShockwaveRing = null;
            HitSlashMark = null;
            RoseBud = null;

            PerlinNoise = null;
            CosmicEnergyNoise = null;
            MusicalWaveNoise = null;
            SparklyNoise = null;

            CometTrailGradient = null;
            EmberScatter = null;
            EnergyTrailUV = null;
            SparkleField = null;

            EroicaLUT = null;

            CelestialValorBlade = null;
            CelestialValorSwingTex = null;
            CelestialValorProjectileTex = null;
            SakurasBlossomBlade = null;

            SwordArc1 = null;
            SwordArc2 = null;
            SwordArc3 = null;
            SwordArc4 = null;
            SwordArc5 = null;
            SwordArc6 = null;
            SwordArc7 = null;
            SwordArc8 = null;
            FlamingArcSwordSlash = null;
            SwordArcSlashWave = null;
            SimpleArcSwordSlash = null;
            CurvedSwordSlash = null;
        }

        private static Asset<Texture2D> Req(string path)
        {
            return ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad);
        }
    }
}
