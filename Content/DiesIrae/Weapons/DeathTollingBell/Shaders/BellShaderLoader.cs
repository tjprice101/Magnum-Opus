using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Shaders
{
    /// <summary>
    /// Loads and registers shaders for the Death Tolling Bell weapon system.
    /// BellToll.fx: BellTollTechnique (expanding ring), DeathKnellTechnique (final explosion)
    /// </summary>
    public class BellShaderLoader : ModSystem
    {
        // Shader keys unique to DeathTollingBell
        public const string BellTollKey = "MagnumOpus:BellToll";
        public const string DeathKnellKey = "MagnumOpus:DeathKnell";
        public const string HellfireBloomKey = "MagnumOpus:HellfireBloom_Bell";
        public const string JudgmentAuraKey = "MagnumOpus:JudgmentAura_Bell";

        public override void Load()
        {
            if (Main.dedServ) return;

            // Load Bell Toll shader (weapon-specific)
            var bellToll = Mod.Assets.Request<Effect>("Effects/DiesIrae/DeathTollingBell/BellToll", ReLogic.Content.AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[BellTollKey] = new MiscShaderData(bellToll, "BellTollTechnique");
            GameShaders.Misc[DeathKnellKey] = new MiscShaderData(bellToll, "DeathKnellTechnique");

            // Load shared Dies Irae shaders (separate registration so Bell doesn't depend on other weapons)
            var hellfireBloom = Mod.Assets.Request<Effect>("Effects/DiesIrae/HellfireBloom", ReLogic.Content.AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[HellfireBloomKey] = new MiscShaderData(hellfireBloom, "HellfireBloomTechnique");

            var judgmentAura = Mod.Assets.Request<Effect>("Effects/DiesIrae/JudgmentAura", ReLogic.Content.AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[JudgmentAuraKey] = new MiscShaderData(judgmentAura, "JudgmentAuraTechnique");
        }

        public override void Unload()
        {
            // Shader data cleaned up automatically
        }
    }
}
