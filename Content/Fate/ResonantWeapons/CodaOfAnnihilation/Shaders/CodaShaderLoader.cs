using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Shaders
{
    /// <summary>
    /// Loads and registers all shaders for Coda of Annihilation.
    /// Self-contained — no crossover with shared mod shader systems.
    /// 
    /// Registers 5 shader keys from 4 .fx files:
    ///   • MagnumOpus:CodaZenithTrail      — Flying sword trail (ZenithMain technique)
    ///   • MagnumOpus:CodaZenithGlow       — Flying sword trail glow (ZenithGlow technique)
    ///   • MagnumOpus:CodaSwingArc         — Held swing arc/smear
    ///   • MagnumOpus:CodaImpactBurst      — Impact explosion
    ///   • MagnumOpus:CodaAnnihilationBloom— Ultimate bloom/flash
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class CodaShaderLoader : ModSystem
    {
        public static bool HasZenithTrail { get; private set; }
        public static bool HasZenithGlow { get; private set; }
        public static bool HasSwingArc { get; private set; }
        public static bool HasImpactBurst { get; private set; }
        public static bool HasAnnihilationBloom { get; private set; }

        public override void PostSetupContent()
        {
            // CodaZenithTrail.fx — two techniques
            HasZenithTrail = TryLoadMiscShader(
                "Effects/Fate/CodaOfAnnihilation/CodaZenithTrail",
                "ZenithMain", "MagnumOpus:CodaZenithTrail");

            HasZenithGlow = TryLoadMiscShader(
                "Effects/Fate/CodaOfAnnihilation/CodaZenithTrail",
                "ZenithGlow", "MagnumOpus:CodaZenithGlow");

            // CodaSwingArc.fx
            HasSwingArc = TryLoadMiscShader(
                "Effects/Fate/CodaOfAnnihilation/CodaSwingArc",
                "SwingArcMain", "MagnumOpus:CodaSwingArc");

            // CodaImpactBurst.fx
            HasImpactBurst = TryLoadMiscShader(
                "Effects/Fate/CodaOfAnnihilation/CodaImpactBurst",
                "ImpactBurstMain", "MagnumOpus:CodaImpactBurst");

            // CodaAnnihilationBloom.fx
            HasAnnihilationBloom = TryLoadMiscShader(
                "Effects/Fate/CodaOfAnnihilation/CodaAnnihilationBloom",
                "AnnihilationBloomMain", "MagnumOpus:CodaAnnihilationBloom");
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
    }
}
