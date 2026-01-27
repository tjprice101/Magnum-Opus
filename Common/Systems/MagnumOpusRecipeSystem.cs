using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MagnumOpus.Common.Systems
{
    // Registers recipe groups for MagnumOpus
    public class MagnumOpusRecipeSystem : ModSystem
    {
        public override void AddRecipeGroups()
        {
            // Register the AnyResonantEnergy group for Phase 6 crafting
            RecipeGroup group = new RecipeGroup(() => "Any Resonant Energy", new int[]
            {
                ModContent.ItemType<Content.Materials.Foundation.MinorMusicNote>(),
                ModContent.ItemType<Content.Materials.Foundation.DullResonator>(),
                ModContent.ItemType<Content.Materials.Foundation.ResonantCrystalShard>(),
                ModContent.ItemType<Content.Summer.Materials.SummerResonantEnergy>(),
                ModContent.ItemType<Content.Winter.Materials.WinterResonantEnergy>(),
                ModContent.ItemType<Content.Spring.Materials.SpringResonantEnergy>(),
                ModContent.ItemType<Content.Autumn.Materials.AutumnResonantEnergy>(),
                ModContent.ItemType<Content.MoonlightSonata.ResonanceEnergies.MoonlightsResonantEnergy>(),
                ModContent.ItemType<Content.Eroica.ResonanceEnergies.EroicasResonantEnergy>(),
                ModContent.ItemType<Content.SwanLake.ResonanceEnergies.SwansResonanceEnergy>(),
                ModContent.ItemType<Content.LaCampanella.ResonanceEnergies.LaCampanellaResonantEnergy>(),
                ModContent.ItemType<Content.EnigmaVariations.ResonanceEnergies.EnigmaResonantEnergy>(),
                ModContent.ItemType<Content.Fate.ResonanceEnergies.FateResonantEnergy>()
            });
            RecipeGroup.RegisterGroup("MagnumOpus:AnyResonantEnergy", group);
        }
    }
}
