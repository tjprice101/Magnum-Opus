using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    #region Tier 1: Pre-Hardmode Foundation

    /// <summary>
    /// Resonant Spotter - Base tier ranger chain accessory.
    /// Enables the Marked for Death System: ranged attacks mark enemies for 5 seconds.
    /// Marked enemies glow slightly for visibility.
    /// </summary>
    public class ResonantSpotter : ModItem
    {
        private static readonly Color BaseRed = new Color(255, 100, 100);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasResonantSpotter = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Enables the Marked for Death System")
            {
                OverrideColor = BaseRed
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ranged attacks mark enemies for 5 seconds")
            {
                OverrideColor = new Color(255, 180, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies glow, making them easier to track")
            {
                OverrideColor = new Color(220, 160, 160)
            });
            
            // Show current marked count if equipped
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? BaseRed : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hunt begins with a single mark'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.Binoculars, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Spring Hunter's Lens - Post-Primavera tier.
    /// Marks last 8 seconds. Hitting marked enemies has 10% chance to drop hearts.
    /// </summary>
    public class SpringHuntersLens : ModItem
    {
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringPink = new Color(255, 183, 197);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System")
            {
                OverrideColor = SpringGreen
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marks last 8 seconds")
            {
                OverrideColor = new Color(200, 255, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Hitting marked enemies has 10% chance to drop hearts")
            {
                OverrideColor = SpringPink
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? SpringGreen : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring's renewal brings vitality to the hunter'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantSpotter>(1)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 2: Mid Pre-Hardmode (Post-L'Estate)

    /// <summary>
    /// Solar Tracker's Badge - Post-L'Estate tier.
    /// Marks last 10 seconds. Marked enemies take +5% damage from ALL sources (team buff!).
    /// </summary>
    public class SolarTrackersBadge : ModItem
    {
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System")
            {
                OverrideColor = SummerOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marks last 10 seconds")
            {
                OverrideColor = new Color(255, 200, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies take +5% damage from ALL sources")
            {
                OverrideColor = SummerGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "TeamBuff", "✦ Great for multiplayer - helps the whole team!")
            {
                OverrideColor = new Color(255, 230, 180)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? SummerOrange : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The summer sun reveals all prey'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringHuntersLens>(1)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 3: Early Hardmode (Post-Autunno)

    /// <summary>
    /// Harvest Reaper's Mark - Post-Autunno tier.
    /// Marked enemies explode on death (50% weapon damage AoE). Chain marking to nearby enemies.
    /// </summary>
    public class HarvestReapersMark : ModItem
    {
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color AutumnOrange = new Color(210, 120, 50);
        private static readonly Color AutumnRed = new Color(180, 60, 40);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System")
            {
                OverrideColor = AutumnBrown
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies explode on death")
            {
                OverrideColor = AutumnOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Explosion deals 50% weapon damage in an area")
            {
                OverrideColor = AutumnRed
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Explosion marks nearby enemies (chain marking)")
            {
                OverrideColor = new Color(200, 150, 100)
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? AutumnBrown : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest reaps what was sown in blood'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarTrackersBadge>(1)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 4: Post-Mech (Post-L'Inverno)

    /// <summary>
    /// Permafrost Hunter's Eye - Post-L'Inverno tier.
    /// Marked enemies are slowed 15%. Killing marked enemy refreshes marks on nearby enemies.
    /// </summary>
    public class PermafrostHuntersEye : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(230, 240, 255);
        private static readonly Color WinterCyan = new Color(180, 240, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.LightPurple;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System")
            {
                OverrideColor = WinterBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marked enemies are slowed by 15%")
            {
                OverrideColor = WinterCyan
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Killing marked enemies refreshes marks on nearby enemies")
            {
                OverrideColor = WinterWhite
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? WinterBlue : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold eye of winter sees all and freezes hope'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestReapersMark>(1)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Seasonal Sight - Post-Plantera tier (requires Cycle of Seasons).
    /// Marks apply seasonal debuffs (burn/chill/wither/bloom). Mark duration 15 seconds.
    /// </summary>
    public class VivaldisSeSonalSight : ModItem
    {
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        
        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasResonantSpotter = true;
            markingPlayer.hasSpringHuntersLens = true;
            markingPlayer.hasSolarTrackersBadge = true;
            markingPlayer.hasHarvestReapersMark = true;
            markingPlayer.hasPermafrostHuntersEye = true;
            markingPlayer.hasVivaldisSeSonalSight = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // Current season indicator
            int season = (int)(Main.GameUpdateCount / 600) % 4;
            string seasonName = season switch { 0 => "Spring", 1 => "Summer", 2 => "Autumn", 3 => "Winter", _ => "Unknown" };
            Color seasonColor = season switch
            {
                0 => SpringGreen,
                1 => SummerOrange,
                2 => AutumnBrown,
                3 => WinterBlue,
                _ => Color.White
            };
            
            tooltips.Add(new TooltipLine(Mod, "System", "Marked for Death System - Vivaldi's Masterwork")
            {
                OverrideColor = seasonColor
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Marks last 15 seconds")
            {
                OverrideColor = new Color(220, 220, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marks apply seasonal debuffs that cycle:")
            {
                OverrideColor = new Color(200, 200, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "SeasonSpring", "  ♪ Spring: Bloom - Healing petals on hit")
            {
                OverrideColor = SpringGreen
            });
            
            tooltips.Add(new TooltipLine(Mod, "SeasonSummer", "  ♪ Summer: Burn - Fire damage over time")
            {
                OverrideColor = SummerOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "SeasonAutumn", "  ♪ Autumn: Wither - Life drain effect")
            {
                OverrideColor = AutumnBrown
            });
            
            tooltips.Add(new TooltipLine(Mod, "SeasonWinter", "  ♪ Winter: Chill - Slowed and minor damage")
            {
                OverrideColor = WinterBlue
            });
            
            tooltips.Add(new TooltipLine(Mod, "CurrentSeason", $"Current Season: {seasonName}")
            {
                OverrideColor = seasonColor
            });
            
            if (markingPlayer.hasResonantSpotter)
            {
                int markedCount = markingPlayer.CountMarkedEnemies();
                tooltips.Add(new TooltipLine(Mod, "Marks", $"Currently Marked: {markedCount}/{markingPlayer.maxMarkedEnemies}")
                {
                    OverrideColor = markedCount > 0 ? seasonColor : Color.Gray
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons dance eternal in the hunter's gaze'")
            {
                OverrideColor = new Color(150, 150, 150)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostHuntersEye>(1)
                .AddIngredient<CycleOfSeasons>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion
}
