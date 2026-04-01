using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Shaders
{
    /// <summary>
    /// Self-contained shader loader for Opus Ultima.
    /// Loads 5 unique shaders from Effects/Fate/OpusUltima/:
    ///
    ///   1. OpusSwingTrail    -- Main swing arc trail with cosmic fire + gold intensity
    ///   2. OpusSwingGlow     -- Wide bloom glow underlayer (alt pass of OpusSwingTrail)
    ///   3. OpusEnergyBall    -- Swirling cosmic energy orb
    ///   4. OpusSeekerTrail   -- Seeker homing trail
    ///   5. OpusExplosion     -- Supernova explosion effect
    ///
    /// Keys: "MagnumOpus:Opus<Purpose>"
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class OpusShaderLoader : ModSystem
    {
        internal static Asset<Effect> SwingTrailShader;
        internal static Asset<Effect> EnergyBallShader;
        internal static Asset<Effect> SeekerTrailShader;
        internal static Asset<Effect> ExplosionShader;

        public static bool HasSwingTrail { get; private set; }
        public static bool HasEnergyBall { get; private set; }
        public static bool HasSeekerTrail { get; private set; }
        public static bool HasExplosion { get; private set; }

        private const string BasePath = "MagnumOpus/Effects/Fate/OpusUltima/";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasSwingTrail = TryLoad(BasePath + "OpusSwingTrail", "OpusSwingMain",
                "MagnumOpus:OpusSwingTrail", out SwingTrailShader);

            HasEnergyBall = TryLoad(BasePath + "OpusEnergyBall", "OpusEnergyBallMain",
                "MagnumOpus:OpusEnergyBall", out EnergyBallShader);

            HasSeekerTrail = TryLoad(BasePath + "OpusSeekerTrail", "OpusSeekerTrailMain",
                "MagnumOpus:OpusSeekerTrail", out SeekerTrailShader);

            HasExplosion = TryLoad(BasePath + "OpusExplosion", "OpusExplosionMain",
                "MagnumOpus:OpusExplosion", out ExplosionShader);

            // Register alternate glow pass from the swing trail shader
            if (HasSwingTrail)
            {
                GameShaders.Misc["MagnumOpus:OpusSwingGlow"] =
                    new MiscShaderData(SwingTrailShader, "P0");
            }
        }

        private static bool TryLoad(string path, string passName, string key, out Asset<Effect> asset)
        {
            asset = null;
            try
            {
                asset = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
                if (asset?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(asset, passName);
                    return true;
                }
            }
            catch { }

            // Fallback: try shared scrolling trail shader
            try
            {
                var fallback = ModContent.Request<Effect>("MagnumOpus/Effects/ScrollingTrailShader", AssetRequestMode.ImmediateLoad);
                if (fallback?.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(fallback, "P0");
                    return true;
                }
            }
            catch { }

            return false;
        }

        /// <summary>Get the swing trail shader (main pass) or null.</summary>
        public static MiscShaderData GetSwingTrail()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:OpusSwingTrail", out var s);
            return s;
        }

        /// <summary>Get the swing glow underlayer shader or null.</summary>
        public static MiscShaderData GetSwingGlow()
        {
            if (!HasSwingTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:OpusSwingGlow", out var s);
            return s;
        }

        /// <summary>Get the energy ball shader or null.</summary>
        public static MiscShaderData GetEnergyBall()
        {
            if (!HasEnergyBall) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:OpusEnergyBall", out var s);
            return s;
        }

        /// <summary>Get the seeker trail shader or null.</summary>
        public static MiscShaderData GetSeekerTrail()
        {
            if (!HasSeekerTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:OpusSeekerTrail", out var s);
            return s;
        }

        /// <summary>Get the explosion shader or null.</summary>
        public static MiscShaderData GetExplosion()
        {
            if (!HasExplosion) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:OpusExplosion", out var s);
            return s;
        }
    }
}
