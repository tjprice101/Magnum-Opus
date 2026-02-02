using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    /// <summary>
    /// Moonlit Overflow Star - Post-Moon Lord Theme Chain T1 (Moonlight Sonata)
    /// At exactly 0 mana: next spell costs 0. Precision timing reward
    /// </summary>
    public class MoonlitOverflowStar : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Inherits all previous tier effects
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            
            // This tier's unique effect
            overflowPlayer.hasMoonlitOverflowStar = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Includes all seasonal overflow effects"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -120 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At exactly 0 mana: your next spell costs 0 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Precision timing rewards the patient mage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon hangs motionless at the edge of silence'") { OverrideColor = new Color(140, 100, 200) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisHarmonicCore>()
                .AddIngredient<ResonantCoreOfMoonlightSonata>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Heroic Arcane Surge - Post-Moon Lord Theme Chain T2 (Eroica)
    /// Going negative triggers brief invincibility (1s). 30s cooldown
    /// </summary>
    public class HeroicArcaneSurge : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 35, 0, 0);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Inherits all previous tier effects
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            
            // This tier's unique effect
            overflowPlayer.hasHeroicArcaneSurge = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Includes all previous overflow effects"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -120 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Entering negative mana triggers 1 second of invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "30 second cooldown between invincibility triggers"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's courage shines brightest at the brink of exhaustion'") { OverrideColor = new Color(200, 80, 80) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitOverflowStar>()
                .AddIngredient<ResonantCoreOfEroica>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Infernal Mana Inferno - Post-Moon Lord Theme Chain T3 (La Campanella)
    /// While negative: leave fire trail. Enemies in trail take DoT
    /// </summary>
    public class InfernalManaInferno : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 40, 0, 0);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Inherits all previous tier effects
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            
            // This tier's unique effect
            overflowPlayer.hasInfernalManaInferno = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Includes all previous overflow effects"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -120 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "While in negative mana: leave a fire trail that damages enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for those who dance with the inferno'") { OverrideColor = new Color(255, 140, 40) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicArcaneSurge>()
                .AddIngredient<ResonantCoreOfLaCampanella>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Enigma's Negative Space - Post-Moon Lord Theme Chain T4 (Enigma Variations)
    /// Overflow to -150. At -100 or below: spells hit twice but you take 5% max HP/s
    /// </summary>
    public class EnigmasNegativeSpace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 45, 0, 0);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Inherits all previous tier effects
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            
            // This tier's unique effect
            overflowPlayer.hasEnigmasNegativeSpace = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Includes all previous overflow effects"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -150 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At -100 mana or below: spells hit enemies twice"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "While below -100 mana: take 5% of max HP per second"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the negative space between questions, answers multiply'") { OverrideColor = new Color(140, 60, 200) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalManaInferno>()
                .AddIngredient<ResonantCoreOfEnigma>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Swan's Balanced Flow - Post-Moon Lord Theme Chain T5 (Swan Lake)
    /// Gain "Grace" buff when recovering from negative. Grace: +20% damage for 5s
    /// </summary>
    public class SwansBalancedFlow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 50, 0, 0);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Inherits all previous tier effects
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            
            // This tier's unique effect
            overflowPlayer.hasSwansBalancedFlow = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Includes all previous overflow effects"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -150 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Recovering from negative mana grants 'Grace' buff"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Grace: +20% damage for 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The swan finds balance in the space between falling and flight'") { OverrideColor = new Color(240, 245, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasNegativeSpace>()
                .AddIngredient<ResonantCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Fate's Cosmic Reservoir - Post-Moon Lord Theme Chain T6 (Fate) - FINAL TIER
    /// Overflow to -200. At -150: spells bend reality, hitting enemies through walls
    /// </summary>
    public class FatesCosmicReservoir : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(1, 0, 0, 0);
            Item.rare = ModContent.RarityType<FateRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            
            // Inherits all previous tier effects
            overflowPlayer.hasResonantOverflowGem = true;
            overflowPlayer.hasSpringArcaneConduit = true;
            overflowPlayer.hasSolarManaCrucible = true;
            overflowPlayer.hasHarvestSoulVessel = true;
            overflowPlayer.hasPermafrostVoidHeart = true;
            overflowPlayer.hasVivaldisHarmonicCore = true;
            overflowPlayer.hasMoonlitOverflowStar = true;
            overflowPlayer.hasHeroicArcaneSurge = true;
            overflowPlayer.hasInfernalManaInferno = true;
            overflowPlayer.hasEnigmasNegativeSpace = true;
            overflowPlayer.hasSwansBalancedFlow = true;
            
            // This tier's unique effect
            overflowPlayer.hasFatesCosmicReservoir = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Includes all previous overflow effects"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Can overflow to -200 mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At -150 mana or below: spells bend reality"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Reality-bent spells can hit enemies through walls"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Destiny itself bows to those who master the cosmic void'") { OverrideColor = new Color(180, 40, 80) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansBalancedFlow>()
                .AddIngredient<ResonantCoreOfFate>(30)
                .AddIngredient<FateResonantEnergy>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
