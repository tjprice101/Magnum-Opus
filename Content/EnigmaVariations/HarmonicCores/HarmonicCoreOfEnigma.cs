using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;

namespace MagnumOpus.Content.EnigmaVariations.HarmonicCores
{
    public class HarmonicCoreOfEnigma : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.scale = 1.25f; // Display 25% larger
            Item.maxStack = 1;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ItemRarityID.Lime;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 5 Harmonic Core]"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc1", "Equip in the Harmonic Core slot (HC button)"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc2", "Base Effect: +4% damage to ALL classes"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc3", "Upgrade using Enigma Resonant Energy"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc4", "Features 4 class trees: Melee, Ranged, Magic, Summon"));
        }

        public override void PostUpdate()
        {
            // Mysterious green glow
            Lighting.AddLight(Item.Center, 0.2f, 0.6f, 0.3f);
            
            if (Main.rand.NextBool(18))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GreenTorch, 0f, -0.5f, 100, default, 1.3f);
                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEnigma>(25)
                .AddIngredient<EnigmaResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
    
    public class ResonantCoreOfEnigma : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 99;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.Lime;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.2f, 0.5f, 0.3f);
        }
    }
}
