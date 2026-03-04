using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Shaders
{
    /// <summary>
    /// Self-contained shader loader for Light of the Future.
    /// Loads 5 unique shaders from Effects/Fate/LightOfTheFuture/:
    ///
    ///   1. LightBulletTrail  窶・Accelerating bullet trail with speed lines
    ///   2. LightRocketTrail  窶・Spiraling rocket smoke/fire trail
    ///   3. LightMuzzleFlash  窶・Cosmic muzzle burst shader
    ///   4. LightImpactBloom  窶・Impact shockwave/bloom
    ///   5. LightAccelGlow    窶・Accelerating glow aura on bullet core
    ///
    /// Keys: "MagnumOpus:Light<Purpose>"
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class LightShaderLoader : ModSystem
    {
        internal static Asset<Effect> BulletTrailShader;
        internal static Asset<Effect> RocketTrailShader;
        internal static Asset<Effect> MuzzleFlashShader;
        internal static Asset<Effect> ImpactBloomShader;

        public static bool HasBulletTrail { get; private set; }
        public static bool HasRocketTrail { get; private set; }
        public static bool HasMuzzleFlash { get; private set; }
        public static bool HasImpactBloom { get; private set; }

        private const string BasePath = "MagnumOpus/Effects/Fate/LightOfTheFuture/";

        public override void PostSetupContent()
        {
            if (Main.dedServ) return;

            HasBulletTrail = TryLoad(BasePath + "LightBulletTrail", "BulletTrailMain",
                "MagnumOpus:LightBulletTrail", out BulletTrailShader);

            HasRocketTrail = TryLoad(BasePath + "LightRocketTrail", "RocketTrailMain",
                "MagnumOpus:LightRocketTrail", out RocketTrailShader);

            HasMuzzleFlash = TryLoad(BasePath + "LightMuzzleFlash", "MuzzleFlashMain",
                "MagnumOpus:LightMuzzleFlash", out MuzzleFlashShader);

            HasImpactBloom = TryLoad(BasePath + "LightImpactBloom", "ImpactBloomMain",
                "MagnumOpus:LightImpactBloom", out ImpactBloomShader);

            // LightAccelGlow uses an alternate pass from LightBulletTrail
            if (HasBulletTrail)
            {
                GameShaders.Misc["MagnumOpus:LightAccelGlow"] =
                    new MiscShaderData(BulletTrailShader, "P0");
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

        /// <summary>Get the bullet trail shader or null.</summary>
        public static MiscShaderData GetBulletTrail()
        {
            if (!HasBulletTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:LightBulletTrail", out var s);
            return s;
        }

        /// <summary>Get the rocket trail shader or null.</summary>
        public static MiscShaderData GetRocketTrail()
        {
            if (!HasRocketTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:LightRocketTrail", out var s);
            return s;
        }

        /// <summary>Get the muzzle flash shader or null.</summary>
        public static MiscShaderData GetMuzzleFlash()
        {
            if (!HasMuzzleFlash) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:LightMuzzleFlash", out var s);
            return s;
        }

        /// <summary>Get the impact bloom shader or null.</summary>
        public static MiscShaderData GetImpactBloom()
        {
            if (!HasImpactBloom) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:LightImpactBloom", out var s);
            return s;
        }

        /// <summary>Get the accelerating glow shader or null.</summary>
        public static MiscShaderData GetAccelGlow()
        {
            if (!HasBulletTrail) return null;
            GameShaders.Misc.TryGetValue("MagnumOpus:LightAccelGlow", out var s);
            return s;
        }
    }
}
