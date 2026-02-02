using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Dies Irae - Refined crystal used for crafting powerful Dies Irae equipment.
    /// Crafted from 5 Remnants of Dies Irae's Wrath at a Moonlight Furnace.
    /// Theme: Day of Wrath - crystallized judgment
    /// </summary>
    public class ResonantCoreOfDiesIrae : ModItem
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
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crystallized essence of final judgment'") 
            { 
                OverrideColor = new Color(139, 0, 0) // Blood red
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfDiesIraesWrath>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Infernal glow with pulsing intensity
            float pulse = 0.85f + (float)System.Math.Sin(Main.GameUpdateCount * 0.07f) * 0.18f;
            Lighting.AddLight(Item.Center, 0.75f * pulse, 0.2f * pulse, 0.1f * pulse);
            
            // Fire particles
            if (Main.rand.NextBool(18))
            {
                Dust fire = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Torch, 0f, 0f, 100, default, 0.9f);
                fire.noGravity = true;
                fire.velocity *= 0.25f;
            }

            // Occasional blood red shimmer
            if (Main.rand.NextBool(30))
            {
                Dust shimmer = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.CrimsonTorch, 0f, 0f, 0, default, 0.7f);
                shimmer.noGravity = true;
                shimmer.velocity = Main.rand.NextVector2Circular(0.4f, 0.4f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Infernal red tint
            return new Color(255, 100, 70, 210);
        }
    }
}
