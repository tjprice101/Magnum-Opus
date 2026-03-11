using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Summer.Accessories
{
    /// <summary>
    /// Sunfire Pendant - Offensive Summer accessory
    /// Post-Mech tier, provides fire damage boost and inflicts On Fire
    /// </summary>
    public class SunfirePendant : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.08f; // 8% damage boost
            player.magmaStone = true; // Inflicts On Fire on melee hits
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Summer Accessory") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+8% damage") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "OnFire", "Melee attacks inflict On Fire!") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun's fury burns within'") { OverrideColor = Color.Lerp(summerOrange, Color.Yellow, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 4)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 12)
                .AddIngredient(ItemID.MagmaStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Zenith Band - Speed/Attack Summer accessory
    /// Post-Mech tier, boosts attack speed and movement
    /// </summary>
    public class ZenithBand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f; // 10% attack speed
            player.moveSpeed += 0.12f; // 12% movement speed
            player.accRunSpeed += 1.5f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Summer Accessory") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Speed", "+10% attack speed") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "MoveSpeed", "+12% movement speed") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "RunSpeed", "Increased run acceleration") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'At the zenith of summer, speed becomes light'") { OverrideColor = Color.Lerp(summerGold, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 4)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 12)
                .AddIngredient(ItemID.Aglet, 1)
                .AddIngredient(ItemID.AnkletoftheWind, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Radiant Crown - Ultimate Summer accessory
    /// Post-Mech tier, provides significant combat bonuses during daytime
    /// </summary>
    public class RadiantCrown : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(gold: 6);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Base stats
            player.GetDamage(DamageClass.Generic) += 0.06f;
            player.statDefense += 5;
            
            // Daytime bonus
            if (Main.dayTime)
            {
                player.GetDamage(DamageClass.Generic) += 0.06f; // +6% more during day (12% total)
                player.GetCritChance(DamageClass.Generic) += 8;
                player.lifeRegen += 2;
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color summerGold = new Color(255, 215, 0);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Summer Accessory") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Base", "+6% damage, +5 defense") { OverrideColor = summerGold });
            tooltips.Add(new TooltipLine(Mod, "DayBonus", "During daytime: +6% additional damage, +8% crit, +2 life regen") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The crown blazes with the power of the midday sun'") { OverrideColor = Color.Lerp(summerGold, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SummerResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<SolarEssence>(), 6)
                .AddIngredient(ModContent.ItemType<EmberOfIntensity>(), 15)
                .AddIngredient(ItemID.SunStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
