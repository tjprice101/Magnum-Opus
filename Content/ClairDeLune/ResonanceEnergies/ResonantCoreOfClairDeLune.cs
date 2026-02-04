using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.ClairDeLune.Projectiles;

namespace MagnumOpus.Content.ClairDeLune.ResonanceEnergies
{
    /// <summary>
    /// Resonant Core of Clair de Lune - Refined temporal crystal used for crafting Clair de Lune equipment.
    /// Crafted from 5 Remnants of Clair de Lune's Harmony at a Moonlight Furnace.
    /// Theme: Crystallized time fragments, clockwork precision energy
    /// </summary>
    public class ResonantCoreOfClairDeLune : ModItem
    {
        // Use existing texture in this folder
        public override string Texture => "MagnumOpus/Content/ClairDeLune/ResonanceEnergies/ClairDeLuneResonantCore";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(gold: 3); // Higher than Ode to Joy (2.5g)
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time crystallized into perfect clockwork harmony'") 
            { 
                OverrideColor = ClairDeLuneColors.Crystal
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RemnantOfClairDeLunesHarmony>(), 5)
                .AddTile(ModContent.TileType<MoonlightFurnaceTile>())
                .Register();
        }

        public override void PostUpdate()
        {
            // Temporal glow with mechanical pulse
            float mechanicalPulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.18f;
            
            // Dark gray base with crimson pulse
            Lighting.AddLight(Item.Center, ClairDeLuneColors.DarkGray.ToVector3() * 0.3f * mechanicalPulse);
            Lighting.AddLight(Item.Center, ClairDeLuneColors.Crimson.ToVector3() * 0.4f * mechanicalPulse);
            
            // Clockwork dust
            if (Main.rand.NextBool(15))
            {
                Dust gear = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Silver, 0f, 0f, 100, ClairDeLuneColors.DarkGray, 0.8f);
                gear.noGravity = true;
                gear.velocity *= 0.25f;
            }
            
            // Crystal shimmer
            if (Main.rand.NextBool(20))
            {
                Dust crystal = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GemDiamond, 0f, 0f, 100, ClairDeLuneColors.Crystal, 0.7f);
                crystal.noGravity = true;
                crystal.velocity *= 0.2f;
            }
            
            // Crimson spark
            if (Main.rand.NextBool(25))
            {
                Dust crimson = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.GemRuby, 0f, 0f, 50, ClairDeLuneColors.Crimson, 0.6f);
                crimson.noGravity = true;
                crimson.velocity *= 0.3f;
            }
            
            // Brass accent
            if (Main.rand.NextBool(30))
            {
                Dust brass = Dust.NewDustDirect(Item.position, Item.width, Item.height, DustID.Enchanted_Gold, 0f, 0f, 80, ClairDeLuneColors.Brass, 0.5f);
                brass.noGravity = true;
                brass.velocity *= 0.15f;
            }
        }
    }
}
