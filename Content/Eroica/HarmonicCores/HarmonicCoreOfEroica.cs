using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Eroica - Equippable core that provides combat-focused bonuses.
    /// Can be upgraded using Eroica's Resonant Energy.
    /// </summary>
    public class HarmonicCoreOfEroica : ModItem
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
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.maxStack = 1;
            
            // Not directly usable - must be placed in Harmonic Core slot
            Item.useStyle = ItemUseStyleID.None;
            Item.UseSound = null;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 25)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 25)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 10)
                .AddTile(ModContent.TileType<Content.MoonlightSonata.CraftingStations.MoonlightAnvilTile>())
                .Register();
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 2 Harmonic Core]")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassBonus", "All Classes: +6% Damage")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "ChromaticHeader", "◆ CHROMATIC (Offensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 150, 150)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticBuff", "  Heroic Fury: +12% damage below 50% HP (+4% else)")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet", "  Heroic Momentum: 20 consecutive hits unleashes")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            tooltips.Add(new TooltipLine(Mod, "ChromaticSet2", "  a massive AoE shockwave")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(255, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Microsoft.Xna.Framework.Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DiatonicHeader", "◇ DIATONIC (Defensive) - Right-click to toggle")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(150, 150, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicBuff", "  Heroic Resolve: +12 DEF below 50% HP (+6 else), +8% DR")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet", "  Rally Cry: Below 30% HP when hit triggers 4s of")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "DiatonicSet2", "  +18% DMG, +22% Speed, +15 DEF")
            {
                OverrideColor = new Microsoft.Xna.Framework.Color(200, 200, 255)
            });
        }
        
        public override void PostUpdate()
        {
            // Heroic red/gold glow when in world
            Lighting.AddLight(Item.Center, 0.6f, 0.3f, 0.2f);
            
            // Eroica halo effect for item in world
            if (Main.GameUpdateCount % 35 == 0)
            {
                CustomParticles.EroicaHalo(Item.Center, 0.4f);
            }
            
            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, -0.5f, 100, default, 1.0f);
                dust.noGravity = true;
            }
        }
    }
}
