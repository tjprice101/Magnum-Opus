using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Eroica - Tier 2
    /// Unique Effect: Heroic Rally - killing enemies creates healing bursts
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
            Item.scale = 1.25f;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.EroicaRarity>();
            Item.maxStack = 1;
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
                OverrideColor = new Color(255, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(255, 180, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+6% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◆ Heroic Rally")
            {
                OverrideColor = new Color(255, 150, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  Defeating enemies releases a healing burst")
            {
                OverrideColor = new Color(255, 200, 210)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Restores 15 HP per enemy slain")
            {
                OverrideColor = new Color(255, 200, 210)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The hero's triumph echoes in victory'")
            {
                OverrideColor = new Color(180, 120, 140)
            });
        }
        
        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.6f, 0.3f, 0.2f);
            
            if (Main.GameUpdateCount % 35 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, new Color(255, 150, 180) * 0.5f, 0.35f, 25);
            }
            
            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, -0.5f, 100, default, 1.0f);
                dust.noGravity = true;
            }
        }
    }
}
