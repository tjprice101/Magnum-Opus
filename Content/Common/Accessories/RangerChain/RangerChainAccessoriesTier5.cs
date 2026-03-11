using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    #region Tier 5: Post-Moon Lord Theme Chain

    /// <summary>
    /// Moonlit Predator's Gaze - Post-Moonlight Sonata boss.
    /// Can mark up to 8 enemies. Marked enemies visible through walls.
    /// </summary>
    public class MoonlitPredatorsGaze : ModItem
    {
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightSilver = new Color(200, 200, 230);
        private static readonly Color MoonlightBlue = new Color(135, 206, 250);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
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
            // Enable this tier
            markingPlayer.hasMoonlitPredatorsGaze = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Moonlit Enhancement")
            {
                OverrideColor = MoonlightPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Can mark up to 8 enemies simultaneously")
            {
                OverrideColor = MoonlightSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies glow through walls")
            {
                OverrideColor = MoonlightBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits all seasonal mark effects")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? MoonlightPurple : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Under the moonlight, no prey can hide'")
            {
                OverrideColor = new Color(150, 150, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<VivaldisSeSonalSight>(1)
                .AddIngredient<ResonantCoreOfMoonlightSonata>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Heroic Deadeye - Post-Eroica boss.
    /// Marked enemies take +8% damage. First shot on marked enemy is auto-crit.
    /// </summary>
    public class HeroicDeadeye : ModItem
    {
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaCrimson = new Color(180, 30, 60);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRarity>();
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
            // Enable this tier
            markingPlayer.hasHeroicDeadeye = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Heroic Enhancement")
            {
                OverrideColor = EroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies take +8% damage from your attacks")
            {
                OverrideColor = EroicaScarlet
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "First ranged hit on a newly marked enemy is guaranteed critical")
            {
                OverrideColor = EroicaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Moonlit Predator's abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? EroicaGold : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's aim never wavers'")
            {
                OverrideColor = new Color(180, 150, 100)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MoonlitPredatorsGaze>(1)
                .AddIngredient<ResonantCoreOfEroica>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Infernal Executioner's Brand - Post-La Campanella boss.
    /// Marked enemies burn (fire DoT). Death explosion radius +50%.
    /// </summary>
    public class InfernalExecutionersBrand : ModItem
    {
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color CampanellaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaBlack = new Color(30, 20, 25);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
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
            // Enable this tier
            markingPlayer.hasInfernalExecutionersBrand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Infernal Enhancement")
            {
                OverrideColor = CampanellaOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies burn with infernal fire")
            {
                OverrideColor = CampanellaGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Death explosion radius increased by 50%")
            {
                OverrideColor = CampanellaOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Heroic Deadeye's abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? CampanellaOrange : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for those who bear the brand'")
            {
                OverrideColor = new Color(180, 130, 80)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HeroicDeadeye>(1)
                .AddIngredient<ResonantCoreOfLaCampanella>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Enigma's Paradox Mark - Post-Enigma boss.
    /// Marks can spread to unmarked enemies on hit (15% chance). Dimensional marks.
    /// </summary>
    public class EnigmasParadoxMark : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
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
            // Enable this tier
            markingPlayer.hasEnigmasParadoxMark = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Paradox Enhancement")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Hitting marked enemies has 15% chance to spread marks")
            {
                OverrideColor = EnigmaGreen
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks transcend dimensions, touching the void")
            {
                OverrideColor = EnigmaPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Infernal Executioner's abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? EnigmaPurple : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The mark spreads like questions without answers'")
            {
                OverrideColor = new Color(120, 80, 160)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalExecutionersBrand>(1)
                .AddIngredient<ResonantCoreOfEnigma>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Swan's Graceful Hunt - Post-Swan Lake boss.
    /// Perfect shots (no damage taken for 3s) apply "Swan Mark" — +15% crit chance against target.
    /// </summary>
    public class SwansGracefulHunt : ModItem
    {
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color SwanSilver = new Color(220, 225, 235);
        private static readonly Color SwanBlack = new Color(20, 20, 30);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<SwanRarity>();
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
            // Enable this tier
            markingPlayer.hasSwansGracefulHunt = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Graceful Enhancement")
            {
                OverrideColor = SwanWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Perfect shots apply 'Swan Mark'")
            {
                OverrideColor = SwanSilver
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Perfect shot: No damage taken for 3 seconds")
            {
                OverrideColor = new Color(200, 200, 210)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Swan Marked enemies: +15% crit chance")
            {
                OverrideColor = SwanWhite
            });
            
            // Perfect shot ready indicator
            if (markingPlayer.hasSwansGracefulHunt)
            {
                if (markingPlayer.IsPerfectShot)
                {
                    tooltips.Add(new TooltipLine(Mod, "Ready", "✦ Perfect Shot READY")
                    {
                        OverrideColor = SwanWhite
                    });
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "Charging", "Perfect Shot: Avoid damage to charge")
                    {
                        OverrideColor = Color.Gray
                    });
                }
            }
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits Enigma's Paradox Mark abilities")
            {
                OverrideColor = new Color(180, 180, 200)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? SwanWhite : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Grace in the hunt, elegance in the kill'")
            {
                OverrideColor = new Color(200, 200, 210)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EnigmasParadoxMark>(1)
                .AddIngredient<ResonantCoreOfSwanLake>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Fate's Cosmic Verdict - Ultimate tier (Post-Fate boss).
    /// Marked enemies take +12% damage. Killing marked boss drops bonus loot bag.
    /// </summary>
    public class FatesCosmicVerdict : ModItem
    {
        private static readonly Color FateCrimson = new Color(200, 80, 120);
        private static readonly Color FatePink = new Color(255, 150, 200);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color FatePurple = new Color(140, 50, 160);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<FateRarity>();
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
            // Enable this tier
            markingPlayer.hasFatesCosmicVerdict = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - COSMIC VERDICT")
            {
                OverrideColor = FateCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies take +12% damage")
            {
                OverrideColor = FatePink
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Killing a marked boss drops bonus treasure")
            {
                OverrideColor = FateWhite
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate", "✦✦ ULTIMATE RANGER ACCESSORY ✦✦")
            {
                OverrideColor = FateCrimson
            });
            
            tooltips.Add(new TooltipLine(Mod, "Inherit", "Inherits ALL previous mark abilities")
            {
                OverrideColor = new Color(200, 180, 220)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? FateCrimson : Color.Gray
                });
            }
            
            // Perfect shot indicator
            if (markingPlayer.hasSwansGracefulHunt && markingPlayer.IsPerfectShot)
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "✦ Perfect Shot READY")
                {
                    OverrideColor = FateWhite
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos itself judges those marked for death'")
            {
                OverrideColor = new Color(180, 120, 160)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SwansGracefulHunt>(1)
                .AddIngredient<ResonantCoreOfFate>(30)
                .AddIngredient<FateResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
