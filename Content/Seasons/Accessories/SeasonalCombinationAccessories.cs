using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Spring.Accessories;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Summer.Accessories;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Autumn.Accessories;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Winter.Accessories;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;
using System;

namespace MagnumOpus.Content.Seasons.Accessories
{
    /// <summary>
    /// Relic of the Equinox - Spring + Autumn combination
    /// Combines growth and decay powers - balanced life and death
    /// </summary>
    public class RelicOfTheEquinox : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 20);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Spring bonuses
            player.lifeRegen += 4;
            player.GetDamage(DamageClass.Generic) += 0.08f;
            
            // Autumn bonuses
            player.GetCritChance(DamageClass.Generic) += 8;
            player.statDefense += 10;
            
            // Equinox unique: life steal + thorns
            player.thorns = 0.8f;
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springGreen = new Color(144, 238, 144);
            Color autumnOrange = new Color(255, 100, 30);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Spring + Autumn Combination") { OverrideColor = Color.Lerp(springGreen, autumnOrange, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Spring", "+4 life regen, +8% damage") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Autumn", "+8% crit chance, +10 defense") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to steal 4% of damage dealt as HP (max 8 HP)") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Thorns", "Attackers take 80% damage back") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Balance between life and death, growth and decay'") { OverrideColor = Color.Lerp(springGreen, autumnOrange, 0.5f) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BloomCrest>(), 1)
                .AddIngredient(ModContent.ItemType<GrowthBand>(), 1)
                .AddIngredient(ModContent.ItemType<ReapersCharm>(), 1)
                .AddIngredient(ModContent.ItemType<TwilightRing>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSpringCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantAutumnCore>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Solstice Ring - Summer + Winter combination
    /// Combines fire and ice powers - extreme temperature
    /// </summary>
    public class SolsticeRing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 20);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Summer bonuses
            player.GetDamage(DamageClass.Generic) += 0.1f;
            player.magmaStone = true; // On Fire
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
            
            // Winter bonuses
            player.frostBurn = true; // Frostburn
            player.statDefense += 10;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            
            // Solstice unique: extreme damage when at full HP or low HP
            float hpPercent = (float)player.statLife / player.statLifeMax2;
            if (hpPercent > 0.9f || hpPercent < 0.25f)
            {
                player.GetDamage(DamageClass.Generic) += 0.08f; // +8% more at extremes
                player.GetCritChance(DamageClass.Generic) += 6;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Summer + Winter Combination") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Summer", "+10% damage, +10% attack speed, inflicts On Fire!") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Winter", "+10 defense, inflicts Frostburn") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Extreme", "At 90%+ or 25%- HP: +8% additional damage, +6% crit") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to Frozen and On Fire!") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fire and ice united in perfect opposition'") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SunfirePendant>(), 1)
                .AddIngredient(ModContent.ItemType<RadiantCrown>(), 1)
                .AddIngredient(ModContent.ItemType<FrostbiteAmulet>(), 1)
                .AddIngredient(ModContent.ItemType<GlacialHeart>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSummerCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantWinterCore>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Cycle of Seasons - All 4 seasons combined
    /// The penultimate seasonal accessory
    /// </summary>
    public class CycleOfSeasons : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(gold: 40);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Combined bonuses from all seasons
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.GetCritChance(DamageClass.Generic) += 12;
            player.GetAttackSpeed(DamageClass.Generic) += 0.08f;
            player.statDefense += 15;
            player.lifeRegen += 5;
            player.endurance += 0.08f; // 8% DR
            
            // Elemental effects
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1f;
            
            // Immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            
            // Life steal
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springGreen = new Color(144, 238, 144);
            Color summerOrange = new Color(255, 140, 0);
            Color autumnBrown = new Color(139, 69, 19);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "All Four Seasons Combined") { OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f) % 1f, 0.8f, 0.7f) });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+15% damage, +12% crit chance, +8% attack speed") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+15 defense, +8% damage reduction") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+5 life regeneration") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Elements", "Inflicts On Fire! and Frostburn, applies Thorns") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to steal 4% of damage dealt as HP (max 8 HP)") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to Frozen, On Fire!, and Frostburn") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle turns, granting power over all seasons'") { OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f + 0.5f) % 1f, 0.8f, 0.7f) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<RelicOfTheEquinox>(), 1)
                .AddIngredient(ModContent.ItemType<SolsticeRing>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Vivaldi's Masterwork - The ultimate seasonal accessory
    /// Requires defeating all 4 seasonal bosses
    /// </summary>
    public class VivaldisMasterwork : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(platinum: 1);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% damage, +15% critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+12% attack speed, +20 defense, +15% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+12% damage reduction, +8 life regen, +4 mana regen"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Immunity to Frozen, On Fire, Frostburn, Chilled, and Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Melee attacks inflict fire and frostburn, 150% thorns damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "33% chance on hit to lifesteal 6% of damage dealt (max 15 HP)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle of Four Seasons united in perfect harmony'") 
            { 
                OverrideColor = new Color(200, 150, 255) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // MASSIVE bonuses - end-game accessory
            player.GetDamage(DamageClass.Generic) += 0.2f; // 20% damage
            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetAttackSpeed(DamageClass.Generic) += 0.12f; // 12% attack speed
            player.statDefense += 20;
            player.lifeRegen += 8;
            player.manaRegen += 4;
            player.endurance += 0.12f; // 12% DR
            player.moveSpeed += 0.15f;
            
            // All elemental effects
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1.5f;
            
            // All immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Poisoned] = true;
            
            // Enhanced life steal
            player.GetModPlayer<VivaldiPlayer>().vivaldiEquipped = true;
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<CycleOfSeasons>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSpringCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantSummerCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantAutumnCore>(), 1)
                .AddIngredient(ModContent.ItemType<DormantWinterCore>(), 1)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    public class VivaldiPlayer : ModPlayer
    {
        public bool vivaldiEquipped = false;
        
        public override void ResetEffects()
        {
            vivaldiEquipped = false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (vivaldiEquipped && Main.rand.NextBool(3)) // 33% chance
            {
                int healAmount = (int)(damageDone * 0.06f); // 6% lifesteal
                healAmount = Math.Max(1, Math.Min(healAmount, 15)); // Cap at 15 HP
                Player.Heal(healAmount);
                
                // Grand lifesteal VFX with music note!
                float hue = Main.rand.NextFloat();
                Color vfxColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                
                // ☁ESPARKLE accent
                
                // ☁EMUSICAL NOTATION - Lifesteal note! - VISIBLE SCALE 0.7f+
            }
        }
    }
}
