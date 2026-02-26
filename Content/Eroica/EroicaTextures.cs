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
    /// </summary>
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

            // Shared
            SoftGlow = Req("MagnumOpus/Assets/Particles Asset Library/SoftGlow2");
            EnergyFlare = Req("MagnumOpus/Assets/Particles Asset Library/EnergyFlare");
            Star4Point = Req("MagnumOpus/Assets/Particles Asset Library/CrispStar4");
            HaloRing = Req("MagnumOpus/Assets/Particles Asset Library/GlowingHalo1");
            CircularMask = Req("MagnumOpus/Assets/Particles Asset Library/CircularMask");
            BloomOrb = Req("MagnumOpus/Assets/Particles Asset Library/SoftGlow4");
            FullSlashArc = Req("MagnumOpus/Assets/VFX/Smears/FullSlashArc");
            FlamingArc = Req("MagnumOpus/Assets/Particles Asset Library/FlamingArcSwordSlash");
            MusicNote = Req("MagnumOpus/Assets/Particles Asset Library/MusicNote");
            CursiveMusicNote = Req("MagnumOpus/Assets/Particles Asset Library/CursiveMusicNote");
            FlameImpact = Req("MagnumOpus/Assets/Particles Asset Library/FlameImpactExplosion");
            GodRays = Req("MagnumOpus/Assets/VFX/LightRays/Radial God Rays Full Circle");
            AnamorphicStreak = Req("MagnumOpus/Assets/VFX/Blooms/Horizontal Anamorphic Streak");
            ComboFinisherArc = Req("MagnumOpus/Assets/VFX/Impacts/Combo Finisher Impact Arc");
            ShockwaveRing = Req("MagnumOpus/Assets/VFX/Impacts/Expanding Shockwave Ring");
            HitSlashMark = Req("MagnumOpus/Assets/VFX/Impacts/Directional Hit Slash Mark");
            RoseBud = Req("MagnumOpus/Assets/Particles Asset Library/RosesBud");

            // Noise
            PerlinNoise = Req("MagnumOpus/Assets/VFX/Noise/PerlinNoise");
            CosmicEnergyNoise = Req("MagnumOpus/Assets/VFX/Noise/CosmicEnergyVortex");
            MusicalWaveNoise = Req("MagnumOpus/Assets/VFX/Noise/MusicalWavePattern");
            SparklyNoise = Req("MagnumOpus/Assets/VFX/Noise/SparklyNoiseTexture");

            // Trails
            CometTrailGradient = Req("MagnumOpus/Assets/VFX/Trails/Comet Trail Gradient Fade");
            EmberScatter = Req("MagnumOpus/Assets/VFX/Trails/Ember Particle Scatter");
            EnergyTrailUV = Req("MagnumOpus/Assets/VFX/Trails/EnergyTrailUV");
            SparkleField = Req("MagnumOpus/Assets/VFX/Trails/Sparkle Particle Field");

            // LUT
            EroicaLUT = Req("MagnumOpus/Assets/VFX/LUT/EroicaGradientLUT");

            // Celestial Valor
            CelestialValorBlade = Req("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor");
            CelestialValorSwingTex = Req("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor_Swing");
            CelestialValorProjectileTex = Req("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValorProjectile");

            // Sakura's Blossom
            SakurasBlossomBlade = Req("MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom");

            // Smears / Arcs
            SwordArc1 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc1");
            SwordArc2 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc2");
            SwordArc3 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc3");
            SwordArc4 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc4");
            SwordArc5 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc5");
            SwordArc6 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc6");
            SwordArc7 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc7");
            SwordArc8 = Req("MagnumOpus/Assets/Particles Asset Library/SwordArc8");
            FlamingArcSwordSlash = Req("MagnumOpus/Assets/Particles Asset Library/FlamingArcSwordSlash");
            SwordArcSlashWave = Req("MagnumOpus/Assets/Particles Asset Library/SwordArcSlashWave");
            SimpleArcSwordSlash = Req("MagnumOpus/Assets/Particles Asset Library/SimpleArcSwordSlash");
            CurvedSwordSlash = Req("MagnumOpus/Assets/Particles Asset Library/CurvedSwordSlash");
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
