using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Shaders
{
    /// <summary>
    /// Loads and registers all shaders for Dual Fated Chime.
    /// Self-contained — no crossover with shared mod shader systems.
    /// 
    /// Registers 3 shader keys:
    ///   • MagnumOpus:DualFatedChimeSlash    — Infernal flame swing arc trail
    ///   • MagnumOpus:DualFatedChimeFlame    — Flame wave projectile trail
    ///   • MagnumOpus:DualFatedChimeWaltz    — Inferno Waltz spinning aura
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class DualFatedChimeShaderLoader : ModSystem
    {
        public static bool HasSlashShader { get; private set; }
        public static bool HasFlameShader { get; private set; }
        public static bool HasWaltzShader { get; private set; }

        public override void PostSetupContent()
        {
            // Try to load from per-weapon shader path first, fallback to shared La Campanella shaders
            if (TryLoadMiscShader("MagnumOpus/Effects/LaCampanella/DualFatedChime/InfernalFlameSlash",
                "InfernalFlameMain", "MagnumOpus:DualFatedChimeSlash"))
            {
                HasSlashShader = true;
            }
            else if (TryLoadMiscShader("MagnumOpus/Effects/HeroicFlameTrail",
                "HeroicFlameTrailPass", "MagnumOpus:DualFatedChimeSlash"))
            {
                HasSlashShader = true;
            }

            if (TryLoadMiscShader("MagnumOpus/Effects/LaCampanella/DualFatedChime/BellFlameTrail",
                "BellFlameMain", "MagnumOpus:DualFatedChimeFlame"))
            {
                HasFlameShader = true;
            }
            else if (TryLoadMiscShader("MagnumOpus/Effects/ScrollingTrailShader",
                "ScrollingTrailPass", "MagnumOpus:DualFatedChimeFlame"))
            {
                HasFlameShader = true;
            }

            if (TryLoadMiscShader("MagnumOpus/Effects/LaCampanella/DualFatedChime/InfernoWaltzAura",
                "WaltzAuraMain", "MagnumOpus:DualFatedChimeWaltz"))
            {
                HasWaltzShader = true;
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

        /// <summary>Get the slash trail shader if available.</summary>
        public static MiscShaderData GetSlashShader()
        {
            if (!HasSlashShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:DualFatedChimeSlash"];
            }
            catch
            {
                HasSlashShader = false;
                return null;
            }
        }

        /// <summary>Get the flame trail shader if available.</summary>
        public static MiscShaderData GetFlameShader()
        {
            if (!HasFlameShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:DualFatedChimeFlame"];
            }
            catch
            {
                HasFlameShader = false;
                return null;
            }
        }

        /// <summary>Get the waltz aura shader if available.</summary>
        public static MiscShaderData GetWaltzShader()
        {
            if (!HasWaltzShader) return null;
            try
            {
                return GameShaders.Misc["MagnumOpus:DualFatedChimeWaltz"];
            }
            catch
            {
                HasWaltzShader = false;
                return null;
            }
        }
    }
}
