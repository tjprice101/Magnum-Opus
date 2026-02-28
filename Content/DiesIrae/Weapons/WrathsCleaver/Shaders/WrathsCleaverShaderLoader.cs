using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Shaders
{
    /// <summary>
    /// Loads and registers Wrath's Cleaver weapon-specific shaders.
    /// Two dedicated shaders: WrathCleaverSlash (slash arc) and InfernoTrail (afterswing).
    /// Falls back to the shared HeroicFlameTrail if custom shaders fail to load.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class WrathsCleaverShaderLoader : ModSystem
    {
        internal static Asset<Effect> WrathSlashShader;
        internal static Asset<Effect> InfernoTrailShader;

        // Shader keys for GameShaders.Misc registration
        public const string SlashKey = "MagnumOpus:WrathCleaverSlash";
        public const string SlashGlowKey = "MagnumOpus:WrathCleaverSlashGlow";
        public const string InfernoKey = "MagnumOpus:WrathCleaverInferno";
        public const string InfernoEmbersKey = "MagnumOpus:WrathCleaverInfernoEmbers";
        public const string StandardPrimitiveKey = "MagnumOpus:WrathCleaverStandardPrimitive";

        public static bool HasSlash => WrathSlashShader?.IsLoaded == true;
        public static bool HasInferno => InfernoTrailShader?.IsLoaded == true;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;

            // Load Wrath Cleaver Slash shader
            try
            {
                WrathSlashShader = assets.Request<Effect>("Effects/DiesIrae/WrathsCleaver/WrathCleaverSlash", AssetRequestMode.ImmediateLoad);
                if (WrathSlashShader?.Value != null)
                {
                    GameShaders.Misc[SlashKey] = new MiscShaderData(WrathSlashShader, "WrathSlashMain");
                    GameShaders.Misc[SlashGlowKey] = new MiscShaderData(WrathSlashShader, "WrathSlashGlow");
                    GameShaders.Misc[StandardPrimitiveKey] = new MiscShaderData(WrathSlashShader, "WrathSlashMain");
                }
            }
            catch
            {
                WrathSlashShader = null;
            }

            // Load Inferno Trail shader
            try
            {
                InfernoTrailShader = assets.Request<Effect>("Effects/DiesIrae/WrathsCleaver/InfernoTrail", AssetRequestMode.ImmediateLoad);
                if (InfernoTrailShader?.Value != null)
                {
                    GameShaders.Misc[InfernoKey] = new MiscShaderData(InfernoTrailShader, "InfernoMain");
                    GameShaders.Misc[InfernoEmbersKey] = new MiscShaderData(InfernoTrailShader, "InfernoEmbers");
                }
            }
            catch
            {
                InfernoTrailShader = null;
            }
        }
    }
}
