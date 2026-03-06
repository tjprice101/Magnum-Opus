using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Shaders
{
    /// <summary>
    /// Shader loader stub for Harmony of Judgement.
    /// Previously loaded 6 shaders into GameShaders.Misc that no rendering code referenced.
    /// HarmonyOfJudgementUtils loads its own shader copies via GetMaskShader(), GetThinBeamShader(), etc.
    /// This stub is retained so the file isn't orphaned; wire shader usage here if/when
    /// rendering code transitions to the GameShaders.Misc access pattern.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class HarmonyOfJudgementShaderLoader : ModSystem
    {
        // Stub — shaders are loaded on-demand by HarmonyOfJudgementUtils.
    }
}
