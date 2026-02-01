using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata.HarmonicCores
{
    /// <summary>
    /// Harmonic Core of Moonlight Sonata - Tier 1
    /// Unique Effect: Lunar Aura - soft purple glow damages nearby enemies
    /// </summary>
    public class HarmonicCoreOfMoonlightSonata : ModItem
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
            Item.value = Item.sellPrice(gold: 15);
            Item.rare = ItemRarityID.Expert;
            Item.maxStack = 1;
            Item.useStyle = ItemUseStyleID.None;
            Item.UseSound = null;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 25)
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HarmonicCoreType", "[Tier 1 Harmonic Core]")
            {
                OverrideColor = new Color(180, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "HarmonicCore", "Equip in the Harmonic Core UI (opens with inventory)")
            {
                OverrideColor = new Color(180, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer1", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+4% All Damage")
            {
                OverrideColor = new Color(120, 200, 120)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer2", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "UniqueHeader", "◁ELunar Aura")
            {
                OverrideColor = new Color(180, 150, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect1", "  Emanates a soft lunar glow around you")
            {
                OverrideColor = new Color(200, 180, 255)
            });
            tooltips.Add(new TooltipLine(Mod, "UniqueEffect2", "  Enemies within the aura take periodic damage")
            {
                OverrideColor = new Color(200, 180, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Spacer3", " ") { OverrideColor = Color.Transparent });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The moon's gentle light, made manifest'")
            {
                OverrideColor = new Color(140, 120, 180)
            });
        }
        
        public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.4f, 0.2f, 0.6f);
            
            if (Main.GameUpdateCount % 30 == 0)
            {
                CustomParticles.GenericFlare(Item.Center, new Color(180, 150, 255) * 0.5f, 0.3f, 25);
            }
            
            if (Main.rand.NextBool(25))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.PurpleTorch, 0f, -0.5f, 100, default, 0.9f);
                dust.noGravity = true;
            }
        }
    }
}
