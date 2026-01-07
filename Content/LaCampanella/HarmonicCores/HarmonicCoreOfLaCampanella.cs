using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;

namespace MagnumOpus.Content.LaCampanella.HarmonicCores
{
    public class HarmonicCoreOfLaCampanella : ModItem
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
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 4 Harmonic Core]"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc1", "Equip in the Harmonic Core slot (HC button)"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc2", "Base Effect: +5% crit to all damage types"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc3", "Upgrade using La Campanella Resonant Energy"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc4", "Features 4 class trees: Melee, Ranged, Magic, Summon"));
        }

        public override void PostUpdate()
        {
            // Golden bell glow
            Lighting.AddLight(Item.Center, 0.6f, 0.5f, 0.2f);
            
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldFlame, 0f, -0.5f, 100, default, 1.2f);
                dust.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfLaCampanella>(25)
                .AddIngredient<LaCampanellaResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
    
    public class ResonantCoreOfLaCampanella : ModItem
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
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.5f, 0.4f, 0.2f);
        }
    }
}
