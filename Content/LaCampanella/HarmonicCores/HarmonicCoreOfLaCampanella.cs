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
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 4 Harmonic Core]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 220, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 220, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassBonus", "All Classes: +10% Damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "ChromaticHeader", "◆ CHROMATIC (Offensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 150, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticBuff", "  Bell's Toll: +8% damage, crits trigger echo damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet", "  Bell Resonance: 12 hit stacks, then crit =")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet2", "  Triple damage echo + AoE blast")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DiatonicHeader", "◇ DIATONIC (Defensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 150, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicBuff", "  Bell's Ward: +12 DEF, +12% DR")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet", "  Sanctuary Bells: Taking damage hurts nearby enemies")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet2", "  Heals 6 HP every 2.5 seconds")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
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
