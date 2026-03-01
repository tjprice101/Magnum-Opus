using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Shaders
{
    /// <summary>
    /// Loads and registers shaders for the Death Tolling Bell weapon system.
    /// BellToll.fx: BellTollTechnique (expanding concentric rings), DeathKnellTechnique (dark death ring)
    /// Also loads shared HellfireBloom and JudgmentAura for bell-specific registration.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class BellShaderLoader : ModSystem
    {
        internal static Asset<Effect> BellTollShader;
        internal static Asset<Effect> HellfireBloomShader;
        internal static Asset<Effect> JudgmentAuraShader;

        // Shader keys unique to DeathTollingBell
        public const string BellTollKey = "MagnumOpus:BellToll";
        public const string DeathKnellKey = "MagnumOpus:DeathKnell";
        public const string HellfireBloomKey = "MagnumOpus:HellfireBloom_Bell";
        public const string JudgmentAuraKey = "MagnumOpus:JudgmentAura_Bell";

        public static bool HasBellToll => BellTollShader?.IsLoaded == true;
        public static bool HasHellfireBloom => HellfireBloomShader?.IsLoaded == true;
        public static bool HasJudgmentAura => JudgmentAuraShader?.IsLoaded == true;

        public override void PostSetupContent()
        {
            var assets = Mod.Assets;

            // Load Bell Toll shader (weapon-specific)
            try
            {
                BellTollShader = assets.Request<Effect>("Effects/DiesIrae/DeathTollingBell/BellToll", AssetRequestMode.ImmediateLoad);
                if (BellTollShader?.Value != null)
                {
                    GameShaders.Misc[BellTollKey] = new MiscShaderData(BellTollShader, "BellTollMain");
                    GameShaders.Misc[DeathKnellKey] = new MiscShaderData(BellTollShader, "DeathKnellMain");
                }
            }
            catch { BellTollShader = null; }

            // Load shared Dies Irae shaders (separate registration so Bell doesn't depend on other weapons)
            try
            {
                HellfireBloomShader = assets.Request<Effect>("Effects/DiesIrae/HellfireBloom", AssetRequestMode.ImmediateLoad);
                if (HellfireBloomShader?.Value != null)
                {
                    GameShaders.Misc[HellfireBloomKey] = new MiscShaderData(HellfireBloomShader, "HellfireBloomMain");
                }
            }
            catch { HellfireBloomShader = null; }

            try
            {
                JudgmentAuraShader = assets.Request<Effect>("Effects/DiesIrae/JudgmentAura", AssetRequestMode.ImmediateLoad);
                if (JudgmentAuraShader?.Value != null)
                {
                    GameShaders.Misc[JudgmentAuraKey] = new MiscShaderData(JudgmentAuraShader, "JudgmentAuraMain");
                }
            }
            catch { JudgmentAuraShader = null; }
        }
    }
}
