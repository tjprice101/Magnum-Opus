using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Shaders
{
    /// <summary>
    /// Loads and registers all shaders for Call of the Black Swan.
    /// Completely self-contained — no crossover with shared mod shader systems.
    /// 
    /// Registers 3 shader keys:
    ///   • MagnumOpus:BlackSwanSlash     — Voronoi-noise swing arc trail (MiscShaderData)
    ///   • MagnumOpus:BlackSwanFlareTrail — Energy streak trail for flare projectiles (MiscShaderData)
    ///   • MagnumOpus:BlackSwanSwingSprite — Blade sprite UV rotation filter (ScreenShaderData)
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class BlackSwanShaderLoader : ModSystem
    {
        // Shader availability flags
        public static bool HasSlashShader { get; private set; }
        public static bool HasFlareTrailShader { get; private set; }
        public static bool HasSwingSpriteShader { get; private set; }

        public override void PostSetupContent()
        {
            // Load DualPolaritySlash shader → swing arc trail
            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/CalloftheBlackSwan/DualPolaritySwing",
                "P0", "MagnumOpus:BlackSwanSlash"))
            {
                HasSlashShader = true;
            }

            // Load SwanFlareTrail shader → flare projectile trail
            if (TryLoadMiscShader("MagnumOpus/Effects/SwanLake/CalloftheBlackSwan/SwanFlareTrail",
                "P0", "MagnumOpus:BlackSwanFlareTrail"))
            {
                HasFlareTrailShader = true;
            }

            // Load SwingSprite shader → blade sprite rotation filter
            // Reuse the Exoblade's SwingSprite shader pattern if available, or load our own
            if (TryLoadScreenShader("MagnumOpus/Effects/SwanLake/CalloftheBlackSwan/DualPolaritySwing",
                "P0", "MagnumOpus:BlackSwanSwingSprite"))
            {
                HasSwingSpriteShader = true;
            }
        }

        private static bool TryLoadMiscShader(string effectPath, string passName, string key)
        {
            try
            {
                var effect = ModContent.Request<Effect>(effectPath, AssetRequestMode.ImmediateLoad);
                if (effect != null && effect.Value != null)
                {
                    GameShaders.Misc[key] = new MiscShaderData(effect, passName);
                    return true;
                }
            }
            catch { }
            return false;
        }

        private static bool TryLoadScreenShader(string effectPath, string passName, string key)
        {
            try
            {
                var effect = ModContent.Request<Effect>(effectPath, AssetRequestMode.ImmediateLoad);
                if (effect != null && effect.Value != null)
                {
                    Filters.Scene[key] = new Filter(new ScreenShaderData(effect, passName), EffectPriority.VeryHigh);
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>Get the slash trail shader if available, or a fallback.</summary>
        public static MiscShaderData GetSlashShader()
        {
            if (HasSlashShader)
                return GameShaders.Misc["MagnumOpus:BlackSwanSlash"];

            // Fallback to standard primitive shader if it exists
            if (GameShaders.Misc.ContainsKey("MagnumOpus:ExobladeStandardPrimitive"))
                return GameShaders.Misc["MagnumOpus:ExobladeStandardPrimitive"];

            return null;
        }

        /// <summary>Get the flare trail shader if available.</summary>
        public static MiscShaderData GetFlareTrailShader()
        {
            if (HasFlareTrailShader)
                return GameShaders.Misc["MagnumOpus:BlackSwanFlareTrail"];
            return null;
        }
    }
}
