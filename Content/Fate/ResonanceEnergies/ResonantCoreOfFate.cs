using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Fate - refined crystal used for crafting powerful Fate equipment.
    /// Crafted from 5 Remnants of the Galaxy's Harmony at a Moonlight Furnace.
    /// </summary>
    public class ResonantCoreOfFate : ModItem
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
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The celestial heart of inevitability'") { OverrideColor = new Color(180, 40, 80) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfTheGalaxysHarmony>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Cosmic pink glow when dropped
            float pulse = 0.9f + (float)System.Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f;
            Lighting.AddLight(Item.Center, 0.6f * pulse, 0.2f * pulse, 0.4f * pulse);
            
            // Pink cosmic particles
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Pink, 0f, 0f, 100, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }

            // Occasional shimmer
            if (Main.rand.NextBool(35))
            {
                Dust sparkle = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, 0f, 0, default, 0.6f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Cosmic pink tint
            return new Color(255, 150, 180, 200);
        }
    }
}
