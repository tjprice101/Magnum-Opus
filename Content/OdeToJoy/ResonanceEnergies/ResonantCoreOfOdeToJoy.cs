using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.OdeToJoy.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Ode to Joy - Refined crystal used for crafting powerful Ode to Joy equipment.
    /// Crafted from 5 Remnants of Ode to Joy's Bloom at a Moonlight Furnace.
    /// Theme: Joy and celebration - crystallized nature energy
    /// </summary>
    public class ResonantCoreOfOdeToJoy : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 2, silver: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crystallized essence of triumphant celebration'") 
            { 
                OverrideColor = new Color(76, 175, 80) // Verdant green
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfOdeToJoysBloom>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Joyous glow with pulsing intensity
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.07f) * 0.18f;
            Lighting.AddLight(Item.Center, 0.4f * pulse, 0.85f * pulse, 0.45f * pulse);
            
            // Nature particles
            if (Main.rand.NextBool(18))
            {
                Dust nature = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.JungleGrass, 0f, 0f, 100, default, 0.9f);
                nature.noGravity = true;
                nature.velocity *= 0.25f;
            }
            
            // Golden shimmer
            if (Main.rand.NextBool(25))
            {
                Dust gold = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GoldCoin, 0f, 0f, 100, default, 0.7f);
                gold.noGravity = true;
                gold.velocity *= 0.2f;
            }
        }
    }
}
