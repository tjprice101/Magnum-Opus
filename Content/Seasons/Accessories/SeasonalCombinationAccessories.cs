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
            player.lifeRegen += 6;
            player.GetDamage(DamageClass.Generic) += 0.20f;
            
            // Autumn bonuses
            player.GetCritChance(DamageClass.Generic) += 10;
            player.statDefense += 13;
            
            // Equinox unique: life steal + thorns
            player.thorns = 0.8f;
            player.GetModPlayer<VivaldiPlayer>().equinoxEquipped = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springGreen = new Color(144, 238, 144);
            Color autumnOrange = new Color(255, 100, 30);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Spring + Autumn Combination") { OverrideColor = Color.Lerp(springGreen, autumnOrange, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Spring", "+6 life regen, +20% damage") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Autumn", "+10% crit chance, +13 defense") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "25% chance to steal 4% of damage dealt as HP (max 50 HP)") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Thorns", "Attackers take 80% damage dealt back") { OverrideColor = autumnOrange });
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
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.magmaStone = true; // On Fire
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            
            // Winter bonuses
            player.frostBurn = true; // Frostburn
            player.statDefense += 13;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            
            // Solstice unique: extreme damage when at full HP or low HP
            float hpPercent = (float)player.statLife / player.statLifeMax2;
            if (hpPercent > 0.9f || hpPercent < 0.25f)
            {
                player.GetDamage(DamageClass.Generic) += 0.30f;
                player.GetCritChance(DamageClass.Generic) += 10;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color summerOrange = new Color(255, 140, 0);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "Summer + Winter Combination") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "Summer", "+15% damage, +15% attack speed, inflicts On Fire!") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Winter", "+13 defense, inflicts Frostburn") { OverrideColor = winterCyan });
            tooltips.Add(new TooltipLine(Mod, "Extreme", "At 90%+ or 25%- HP: +30% additional damage, +10% crit") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
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
            player.GetDamage(DamageClass.Generic) += 0.18f;
            player.GetCritChance(DamageClass.Generic) += 17;
            player.GetAttackSpeed(DamageClass.Generic) += 0.12f;
            player.statDefense += 17;
            player.lifeRegen += 7;
            player.endurance += 0.10f; // 10% DR
            player.maxMinions += 7;
            
            // Elemental effects
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 1f;
            
            // Immunities
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            
            // Life steal
            player.GetModPlayer<VivaldiPlayer>().cycleEquipped = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color springGreen = new Color(144, 238, 144);
            Color summerOrange = new Color(255, 140, 0);
            Color autumnBrown = new Color(139, 69, 19);
            Color winterCyan = new Color(0, 255, 255);
            
            tooltips.Add(new TooltipLine(Mod, "Combo", "All Four Seasons Combined") { OverrideColor = Main.hslToRgb((Main.GameUpdateCount * 0.01f) % 1f, 0.8f, 0.7f) });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+18% damage, +17% crit chance, +12% attack speed") { OverrideColor = summerOrange });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+17 defense, +10% damage reduction") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+7 life regeneration, +7 minion slots") { OverrideColor = springGreen });
            tooltips.Add(new TooltipLine(Mod, "Elements", "Inflicts On Fire! and Frostburn, applies Thorns") { OverrideColor = Color.Lerp(summerOrange, winterCyan, 0.5f) });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to steal 4% of damage dealt as HP (max 30 HP)") { OverrideColor = autumnBrown });
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% damage, +18% critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+15% attack speed, +23 defense, +20% movement speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "+15% damage reduction, +10 life regen, +8 mana regen"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Immunity to Frozen, On Fire, Frostburn, Chilled, and Poisoned"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Melee attacks inflict fire and frostburn, 150% thorns damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "33% chance on hit to lifesteal 8% of damage dealt (max 35 HP)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal cycle of Four Seasons united in perfect harmony'") 
            { 
                OverrideColor = new Color(200, 150, 255) 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // MASSIVE bonuses - end-game accessory
            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.GetCritChance(DamageClass.Generic) += 18;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            player.statDefense += 23;
            player.lifeRegen += 10;
            player.manaRegenBonus += 40; // ~+8 mana/s
            player.endurance += 0.15f; // 15% DR
            player.moveSpeed += 0.20f;
            
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
        public bool equinoxEquipped = false;
        public bool cycleEquipped = false;
        public bool vivaldiEquipped = false;
        
        public override void ResetEffects()
        {
            equinoxEquipped = false;
            cycleEquipped = false;
            vivaldiEquipped = false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Vivaldi's Masterwork: 33% chance, 8% lifesteal, max 35 HP
            if (vivaldiEquipped && Main.rand.NextBool(3))
            {
                int healAmount = (int)(damageDone * 0.08f);
                healAmount = Math.Max(1, Math.Min(healAmount, 35));
                Player.Heal(healAmount);
                return;
            }
            
            // Relic of the Equinox: 25% chance, 4% lifesteal, max 50 HP
            if (equinoxEquipped && Main.rand.NextBool(4))
            {
                int healAmount = (int)(damageDone * 0.04f);
                healAmount = Math.Max(1, Math.Min(healAmount, 50));
                Player.Heal(healAmount);
                return;
            }
            
            // Cycle of Seasons: 20% chance, 4% lifesteal, max 30 HP
            if (cycleEquipped && Main.rand.NextBool(5))
            {
                int healAmount = (int)(damageDone * 0.04f);
                healAmount = Math.Max(1, Math.Min(healAmount, 30));
                Player.Heal(healAmount);
            }
        }
    }
}
