using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

// Rarity imports
using MagnumOpus.Common;

// Theme core imports (CORRECT NAMESPACES)
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    // ==========================================
    // TIER 5: POST-MOON LORD THEME CHAIN
    // ==========================================
    
    /// <summary>
    /// Theme Chain T1: Moonlit Symphony Wand
    /// Conducting at night: +10% minion damage globally for duration
    /// </summary>
    public class MoonlitSymphonyWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
            Item.value = Item.sellPrice(gold: 25);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            
            // New ability
            conductor.HasMoonlitSymphonyWand = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives Broken Armor (halved defense) and 25% slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Conducting at night: +10% minion damage globally"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon rises, and your symphony begins'") { OverrideColor = new Color(140, 100, 200) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisOrchestraBaton>()
                .AddIngredient<ResonantCoreOfMoonlightSonata>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T2: Heroic General's Baton
    /// Conduct grants minions brief invincibility (1s). Rally your troops!
    /// </summary>
    public class HeroicGeneralsBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EroicaRarity>();
            Item.value = Item.sellPrice(gold: 35);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            
            // New ability
            conductor.HasHeroicGeneralsBaton = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives Broken Armor (halved defense) and 25% slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Conducting at night: +10% minion damage globally"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Conduct grants minions 1 second of invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Rally your troops! Victory awaits!'") { OverrideColor = new Color(200, 80, 80) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitSymphonyWand>()
                .AddIngredient<ResonantCoreOfEroica>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T3: Infernal Choir Master's Rod
    /// Conducted minions explode on hit (doesn't kill them). +50% damage as AoE
    /// </summary>
    public class InfernalChoirMastersRod : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
            Item.value = Item.sellPrice(gold: 45);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            
            // New ability
            conductor.HasInfernalChoirMastersRod = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives Broken Armor (halved defense) and 25% slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Conducting at night: +10% minion damage globally"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Conduct grants minions 1 second of invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "Conducted minions explode on hit for +50% damage as AoE"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let the inferno sing through your servants'") { OverrideColor = new Color(255, 140, 40) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicGeneralsBaton>()
                .AddIngredient<ResonantCoreOfLaCampanella>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T4: Enigma's Hivemind Link
    /// Minions can phase through blocks during Conduct. Ambush from anywhere
    /// </summary>
    public class EnigmasHivemindLink : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
            Item.value = Item.sellPrice(gold: 55);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            
            // New ability
            conductor.HasEnigmasHivemindLink = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives Broken Armor (halved defense) and 25% slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Conducting at night: +10% minion damage globally"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Conduct grants minions 1 second of invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "Conducted minions explode on hit for +50% damage as AoE"));
            tooltips.Add(new TooltipLine(Mod, "Effect10", "Minions can phase through blocks during Conduct"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hive knows no boundaries'") { OverrideColor = new Color(140, 60, 200) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalChoirMastersRod>()
                .AddIngredient<ResonantCoreOfEnigma>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T5: Swan's Graceful Direction
    /// Perfect Conduct (full HP): minions deal double damage for focus duration
    /// </summary>
    public class SwansGracefulDirection : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 70);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            
            // New ability
            conductor.HasSwansGracefulDirection = true;
            
            if (Main.mouseRight && Main.mouseRightRelease && player.whoAmI == Main.myPlayer)
            {
                conductor.TryConduct();
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Focused target receives Broken Armor (halved defense) and 25% slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Conducting at night: +10% minion damage globally"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Conduct grants minions 1 second of invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "Conducted minions explode on hit for +50% damage as AoE"));
            tooltips.Add(new TooltipLine(Mod, "Effect10", "Minions can phase through blocks during Conduct"));
            tooltips.Add(new TooltipLine(Mod, "Effect11", "Perfect Conduct (full HP): minions deal double damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Grace in motion, death in stillness'") { OverrideColor = new Color(240, 245, 255) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasHivemindLink>()
                .AddIngredient<ResonantCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    /// <summary>
    /// Theme Chain T6: Fate's Cosmic Dominion
    /// Conduct cooldown 5s. "Finale": hold Conduct 2s to sacrifice all minions for massive single hit
    /// </summary>
    public class FatesCosmicDominion : ModItem
    {
        private static readonly Color FateCrimson = new Color(180, 40, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.value = Item.sellPrice(gold: 100);
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            
            // Inherit all previous tier abilities
            conductor.HasConductorsWand = true;
            conductor.HasSpringMaestrosBadge = true;
            conductor.HasSolarDirectorsCrest = true;
            conductor.HasHarvestBeastlordsHorn = true;
            conductor.HasPermafrostCommandersCrown = true;
            conductor.HasVivaldisOrchestraBaton = true;
            conductor.HasMoonlitSymphonyWand = true;
            conductor.HasHeroicGeneralsBaton = true;
            conductor.HasInfernalChoirMastersRod = true;
            conductor.HasEnigmasHivemindLink = true;
            conductor.HasSwansGracefulDirection = true;
            
            // New ability
            conductor.HasFatesCosmicDominion = true;
            
            // Right-click to conduct (hold for Finale)
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.mouseRight && Main.mouseRightRelease)
                {
                    conductor.TryConduct();
                }
                else if (!Main.mouseRight && conductor.IsChargingFinale)
                {
                    conductor.ReleaseConductButton();
                }
            }
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Right-click to Conduct: focus all minions on one target"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Double-tap Conduct to Scatter: spread minions to all nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "5 second cooldown"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Conducted minions deal +30% damage during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Conducted minions heal you 1 HP per hit during focus"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Focused target receives Broken Armor (halved defense) and 25% slow"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Killing conducted target extends focus duration by 2 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "Conducting at night: +10% minion damage globally"));
            tooltips.Add(new TooltipLine(Mod, "Effect9", "Conduct grants minions 1 second of invincibility"));
            tooltips.Add(new TooltipLine(Mod, "Effect10", "Conducted minions explode on hit for +50% damage as AoE"));
            tooltips.Add(new TooltipLine(Mod, "Effect11", "Minions can phase through blocks during Conduct"));
            tooltips.Add(new TooltipLine(Mod, "Effect12", "Perfect Conduct (full HP): minions deal double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect13", "Hold Conduct for 2 seconds to perform 'Finale': sacrifice all minions for a massive single hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos bends to your final symphony'") { OverrideColor = new Color(180, 40, 80) });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansGracefulDirection>()
                .AddIngredient<ResonantCoreOfFate>(30)
                .AddIngredient<FateResonantEnergy>()
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
