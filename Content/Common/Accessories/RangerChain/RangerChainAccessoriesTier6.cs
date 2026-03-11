using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    #region T7: Nocturnal Predator's Sight (Nachtmusik Theme)
    
    /// <summary>
    /// T7 Ranger accessory - Nachtmusik theme (post-Fate).
    /// Starlight guides marks through the darkness.
    /// Max 12 marks, visible through walls, +5% damage at night, star shower on kill.
    /// </summary>
    public class NocturnalPredatorsSight : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);
        private static readonly Color NachtmusikSilver = new Color(200, 210, 230);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 85);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            
            // T7 flag
            markingPlayer.hasNocturnalPredatorsSight = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - STELLAR PREDATOR")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 12 enemies at once")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks are visible through walls at any distance")
            {
                OverrideColor = NachtmusikSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At night: Marked enemies take +5% additional damage")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Killing marked enemy triggers star shower on nearby foes")
            {
                OverrideColor = NachtmusikGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/12")
                {
                    OverrideColor = markedCount > 0 ? NachtmusikPurple : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars reveal all that hides in darkness'")
            {
                OverrideColor = new Color(140, 120, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicVerdict>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T8: Infernal Executioner's Sight (Dies Irae Theme)
    
    /// <summary>
    /// T8 Ranger accessory - Dies Irae theme (post-Fate).
    /// Hellfire brands targets for destruction.
    /// Max 14 marks, burning DoT, +100% explosion damage, 20% spread, judgment stacks.
    /// </summary>
    public class InfernalExecutionersSight : ModItem
    {
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);
        private static readonly Color DiesIraeBlack = new Color(30, 20, 25);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 95);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            
            // T8 flag
            markingPlayer.hasInfernalExecutionersSight = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - INFERNAL EXECUTIONER")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 14 enemies at once")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies take burning damage (2% max HP/s)")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Death explosions deal +100% damage and leave burning ground")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marks have 20% chance to spread to nearby enemies on hit")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Judgment Stacks: +3% damage per hit on marked enemy (max +30%)")
            {
                OverrideColor = DiesIraeOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hellfire brands those condemned to oblivion'")
            {
                OverrideColor = new Color(180, 100, 80)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalPredatorsSight>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T9: Jubilant Hunter's Sight (Ode to Joy Theme)
    
    /// <summary>
    /// T9 Ranger accessory - Ode to Joy theme (post-Fate).
    /// Nature's blessing guides your aim.
    /// Max 16 marks, healing orbs, +8% damage buff on kill, vine entangle, Nature's Bounty.
    /// </summary>
    public class JubilantHuntersSight : ModItem
    {
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color OdeToJoyBlack = new Color(30, 30, 40);
        private static readonly Color OdeToJoyIridescent = new Color(220, 200, 255);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 105);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            
            // T9 flag
            markingPlayer.hasJubilantHuntersSight = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - JUBILANT HUNTER")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 16 enemies at once")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies drop healing orbs when hit (5% chance, heals 10 HP)")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Killing marked enemies grants +8% damage buff for 10s (stacks)")
            {
                OverrideColor = OdeToJoyIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marks cause vines to entangle enemies, slowing them 20%")
            {
                OverrideColor = new Color(150, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Nature's Bounty: Kill 5 marked enemies within 10s to spawn homing projectile")
            {
                OverrideColor = OdeToJoyRose
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Nature's blessing flows through the hunt'")
            {
                OverrideColor = new Color(200, 220, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalExecutionersSight>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region T10: Eternal Verdict Sight (Clair de Lune Theme)
    
    /// <summary>
    /// T10 Ranger accessory - Clair de Lune theme (post-Fate).
    /// Time marks prey across all moments.
    /// Max 20 marks, persist after death, triple hit chance, linked damage, Temporal Judgment.
    /// </summary>
    public class EternalVerdictSight : ModItem
    {
        private static readonly Color ClairDeLuneGray = new Color(120, 110, 130);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color ClairDeLuneCrimson = new Color(180, 80, 100);
        private static readonly Color ClairDeLuneIridescent = new Color(180, 170, 200);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            markingPlayer.hasJubilantHuntersSight = true;
            
            // T10 flag
            markingPlayer.hasEternalVerdictSight = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - ETERNAL VERDICT")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ULTIMATE RANGER ACCESSORY ✦✦✦")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Mark up to 20 enemies at once")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks persist after death and transfer to respawned enemies")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Shots hit marked enemies in past and future positions (triple hit chance)")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "At 15+ marks: All marked enemies are linked (25% shared damage)")
            {
                OverrideColor = ClairDeLuneIridescent
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Temporal Judgment: Killing a marked boss rewinds 5s of damage")
            {
                OverrideColor = ClairDeLuneCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time itself marks your prey across all moments'")
            {
                OverrideColor = new Color(160, 140, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantHuntersSight>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 1: Starfall Executioner's Scope (Nachtmusik + Dies Irae)
    
    /// <summary>
    /// Fusion Tier 1 Ranger accessory - combines Nachtmusik and Dies Irae.
    /// Combines stellar precision with hellfire judgment.
    /// </summary>
    public class StarfallExecutionersScope : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color FusionGold = new Color(255, 180, 80);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 130);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            
            // Fusion flag
            markingPlayer.hasStarfallExecutionersScope = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ STARFALL EXECUTIONER FUSION ⚔")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - COSMIC JUDGMENT")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Nocturnal Predator's Sight and Infernal Executioner's Sight")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark up to 14 enemies with stellar-infernal marks")
            {
                OverrideColor = FusionGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Star showers trigger hellfire explosions on impact")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Judgment stacks build faster at night (+50%)")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from both component accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire unite in cosmic judgment'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalPredatorsSight>(1)
                .AddIngredient<InfernalExecutionersSight>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 2: Triumphant Verdict Scope (+ Ode to Joy)
    
    /// <summary>
    /// Fusion Tier 2 Ranger accessory - adds Ode to Joy to the fusion.
    /// Combines stellar precision, hellfire judgment, and nature's blessing.
    /// </summary>
    public class TriumphantVerdictScope : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color FusionTriumph = new Color(255, 220, 160);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 160);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            markingPlayer.hasJubilantHuntersSight = true;
            markingPlayer.hasStarfallExecutionersScope = true;
            
            // Fusion flag
            markingPlayer.hasTriumphantVerdictScope = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Fusion", "⚔ TRIUMPHANT VERDICT FUSION ⚔")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - TRIPLE SYMPHONY")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines Starfall Executioner's Scope with Jubilant Hunter's Sight")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark up to 16 enemies with triple-symphony marks")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Healing orbs explode into star showers and hellfire")
            {
                OverrideColor = FusionTriumph
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Nature's Bounty projectiles leave burning starlight trails")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL abilities from all three theme accessories")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three symphonies unite in triumphant harmony'")
            {
                OverrideColor = new Color(220, 200, 180)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallExecutionersScope>(1)
                .AddIngredient<JubilantHuntersSight>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
    
    #region Fusion Tier 3: Scope of the Eternal Verdict (Ultimate - + Clair de Lune)
    
    /// <summary>
    /// Ultimate Fusion Ranger accessory - all four Post-Fate themes combined.
    /// The pinnacle of the ranger marking system.
    /// </summary>
    public class ScopeOfTheEternalVerdict : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color UltimatePrismatic = new Color(255, 230, 200);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 200);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Enable all previous tiers
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
            markingPlayer.hasMoonlitPredatorsGaze = true;
            markingPlayer.hasHeroicDeadeye = true;
            markingPlayer.hasInfernalExecutionersBrand = true;
            markingPlayer.hasEnigmasParadoxMark = true;
            markingPlayer.hasSwansGracefulHunt = true;
            markingPlayer.hasFatesCosmicVerdict = true;
            markingPlayer.hasNocturnalPredatorsSight = true;
            markingPlayer.hasInfernalExecutionersSight = true;
            markingPlayer.hasJubilantHuntersSight = true;
            markingPlayer.hasEternalVerdictSight = true;
            markingPlayer.hasStarfallExecutionersScope = true;
            markingPlayer.hasTriumphantVerdictScope = true;
            
            // Ultimate flag
            markingPlayer.hasScopeOfTheEternalVerdict = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦✦ ETERNAL VERDICT - ULTIMATE FUSION ✦✦✦")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - GRAND SYMPHONY")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Combines ALL four Post-Fate theme accessories")
            {
                OverrideColor = NachtmusikPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mark up to 20 enemies with eternal symphony marks")
            {
                OverrideColor = DiesIraeCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Temporal marks create starfall-hellfire-nature chains")
            {
                OverrideColor = OdeToJoyWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Temporal Judgment triggers all theme death effects simultaneously")
            {
                OverrideColor = ClairDeLuneBrass
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Marks persist across dimensions and through time itself")
            {
                OverrideColor = UltimatePrismatic
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Masters ALL abilities from the complete Post-Fate arsenal")
            {
                OverrideColor = new Color(220, 200, 240)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The eternal verdict echoes through all of existence'")
            {
                OverrideColor = new Color(200, 180, 160)
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantVerdictScope>(1)
                .AddIngredient<EternalVerdictSight>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
    
    #endregion
}
