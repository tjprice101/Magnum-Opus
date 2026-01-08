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
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 6 Harmonic Core - Ultimate]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 100, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 100, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassBonus", "All Classes: +15% Damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "ChromaticHeader", "◆ CHROMATIC (Offensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 150, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticBuff", "  Fate's Wrath: +14% damage, execute enemies <10% HP")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet", "  Mark of Fate: First hit marks bosses")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet2", "  Marked targets take +35% damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DiatonicHeader", "◇ DIATONIC (Defensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 150, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicBuff", "  Fate's Shield: +18 DEF, +16% DR")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet", "  Cheat death once per minute (set to 25% HP)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet2", "  Destiny's Weave: Each save = permanent +4% DMG")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The strongest Harmonic Core, forged by destiny itself'")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(180, 120, 140)
            });
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
