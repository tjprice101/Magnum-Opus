using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Shaders
{
    /// <summary>
    /// Shader loader stub for Staff of Final Judgement.
    /// Previously loaded 4 shaders into GameShaders.Misc that no rendering code referenced.
    /// Retained as a stub; wire shader usage here when VFX is implemented for this weapon.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class StaffOfFinalJudgementShaderLoader : ModSystem
    {
        // Stub — no rendering code currently uses registered shaders for this weapon.
    }
}
