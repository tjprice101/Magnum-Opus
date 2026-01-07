using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Fate.HarmonicCores
{
    public class HarmonicCoreOfFate : ModItem
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
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ItemRarityID.Red;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 6 Harmonic Core - Ultimate]"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc1", "Equip in the Harmonic Core slot (HC button)"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc2", "Base Effect: +12 defense, +60 max life/mana, +5% all damage"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc3", "Upgrade using Fate Resonant Energy"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc4", "Features 4 class trees: Melee, Ranged, Magic, Summon"));
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreDesc5", "The strongest Harmonic Core, forged by destiny itself"));
        }

        public override void PostUpdate()
        {
            // Powerful crimson/pink destiny glow
            Lighting.AddLight(Item.Center, 0.7f, 0.3f, 0.5f);
            
            if (Main.rand.NextBool(12))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.8f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.6f;
            }
            
            if (Main.rand.NextBool(25))
            {
                Dust dust2 = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.RedTorch, 0f, 0f, 100, default, 1.0f);
                dust2.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfFate>(25)
                .AddIngredient<FateResonantEnergy>(25)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
    }
    
    public class ResonantCoreOfFate : ModItem
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
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Red;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.6f, 0.2f, 0.4f);
            
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, -0.3f, 100, default, 0.9f);
                dust.noGravity = true;
            }
        }
    }
}
