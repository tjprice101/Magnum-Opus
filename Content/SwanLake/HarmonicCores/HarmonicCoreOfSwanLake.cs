using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;

namespace MagnumOpus.Content.SwanLake.HarmonicCores
{
    public class HarmonicCoreOfSwanLake : ModItem
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
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 3 Harmonic Core]"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc1", "Equip in the Harmonic Core slot (HC button)"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc2", "Base Effect: +12% movement speed, +4 defense"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc3", "Upgrade using Swan Lake Resonant Energy"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc4", "Features 4 class trees: Melee, Ranged, Magic, Summon"));
        }

        public override void PostUpdate()
        {
            // Graceful white/blue glow
            Lighting.AddLight(Item.Center, 0.4f, 0.5f, 0.7f);
            
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Cloud, 0f, -0.5f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfSwanLake>(25)
                .AddIngredient<SwanLakeResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
    
    // Placeholder class for the crafting ingredient
    public class ResonantCoreOfSwanLake : ModItem
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
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Cyan;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.3f, 0.4f, 0.6f);
        }
    }
}
