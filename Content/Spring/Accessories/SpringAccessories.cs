using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Spring.Accessories
{
    /// <summary>
    /// Petal Shield - Defensive Spring accessory
    /// Post-WoF tier, provides defense and damage reduction when hit
    /// </summary>
    public class PetalShield : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 8;
            player.endurance += 0.06f; // 6% damage reduction
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+8 defense") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "DR", "+6% damage reduction") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Petals form an impenetrable shield of renewal'") { OverrideColor = Color.Lerp(springPink, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 10)
                .AddIngredient(ItemID.CobaltShield, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Growth Band - Regeneration Spring accessory
    /// Post-WoF tier, provides life regen and mana regen
    /// </summary>
    public class GrowthBand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.lifeRegen += 3;
            player.manaRegen += 2;
            player.GetDamage(DamageClass.Generic) += 0.04f; // 4% damage boost
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+3 life regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "+2 mana regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+4% damage") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Life flows through the wearer like sap through a tree'") { OverrideColor = Color.Lerp(springGreen, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 10)
                .AddIngredient(ItemID.BandofRegeneration, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Bloom Crest - Offensive Spring accessory
    /// Post-WoF tier, boosts damage and crit when moving
    /// </summary>
    public class BloomCrest : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 3, silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.06f; // 6% base damage
            
            // Bonus when moving
            if (player.velocity.Length() > 3f)
            {
                player.GetDamage(DamageClass.Generic) += 0.04f; // +4% when moving fast
                player.GetCritChance(DamageClass.Generic) += 5;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springPink = new Color(255, 183, 197);
            Color springGreen = new Color(144, 238, 144);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Spring Accessory") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+6% damage (base)") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Moving", "While moving: +4% additional damage, +5% crit chance") { OverrideColor = springPink });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crest blooms brighter with each step forward'") { OverrideColor = Color.Lerp(springPink, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SpringResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<BlossomEssence>(), 4)
                .AddIngredient(ModContent.ItemType<PetalOfRebirth>(), 12)
                .AddIngredient(ItemID.WarriorEmblem, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
