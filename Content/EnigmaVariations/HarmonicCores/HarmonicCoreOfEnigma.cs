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
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 5 Harmonic Core]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 255, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 255, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassBonus", "All Classes: +12% Damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "ChromaticHeader", "◆ CHROMATIC (Offensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 150, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticBuff", "  Enigma's Chaos: +6-12% random damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet", "  Variations: Every 10s gain random powerful buff:")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet2", "  +14% DMG / +18% Speed / +14 Pen / +10% Crit / Mana")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DiatonicHeader", "◇ DIATONIC (Defensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 150, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicBuff", "  Enigma's Mystery: +15 DEF, +15% DR")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet", "  Mystery Shield: ~17% chance to reflect")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet2", "  enemy projectiles back at them")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
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
