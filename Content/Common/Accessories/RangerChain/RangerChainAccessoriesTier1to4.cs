using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    /// <summary>
    /// Resonant Spotter - Base tier ranger accessory.
    /// Simple effect: Ranged attacks mark enemies (visual effect only).
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ranged attacks mark enemies with a glowing indicator")
            {
                OverrideColor = new Color(255, 180, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hunt begins with a single mark'")
            {
                OverrideColor = BaseRed * 0.8f
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
    /// Spring Hunter's Lens - Spring tier ranger accessory.
    /// Simple effect: 10% chance to drop hearts on ranged hit.
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
            markingPlayer.hasSpringHuntersLens = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "10% chance to drop hearts on ranged hit")
            {
                OverrideColor = SpringPink
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring's renewal brings vitality to the hunter'")
            {
                OverrideColor = SpringGreen * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantSpotter>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Solar Tracker's Badge - Summer tier ranger accessory.
    /// Simple effect: +5% ranged damage.
    /// </summary>
    public class SolarTrackersBadge : ModItem
    {
        private static readonly Color SummerOrange = new Color(255, 140, 0);

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
            markingPlayer.hasSolarTrackersBadge = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+5% ranged damage")
            {
                OverrideColor = new Color(255, 200, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The summer sun reveals all prey'")
            {
                OverrideColor = SummerOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringHuntersLens>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    /// <summary>
    /// Harvest Reaper's Mark - Autumn tier ranger accessory.
    /// Simple effect: Enemies killed with ranged attacks explode.
    /// </summary>
    public class HarvestReapersMark : ModItem
    {
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color AutumnOrange = new Color(210, 120, 50);

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
            markingPlayer.hasHarvestReapersMark = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enemies killed with ranged attacks explode")
            {
                OverrideColor = AutumnOrange
            });

            tooltips.Add(new TooltipLine(Mod, "ExplosionNote", "Explosion deals 50% of weapon damage in an area")
            {
                OverrideColor = new Color(200, 150, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest reaps what was sown in blood'")
            {
                OverrideColor = AutumnBrown * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarTrackersBadge>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Permafrost Hunter's Eye - Winter tier ranger accessory.
    /// Simple effect: Ranged attacks slow enemies by 15%.
    /// </summary>
    public class PermafrostHuntersEye : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);
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
            markingPlayer.hasPermafrostHuntersEye = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ranged attacks slow enemies by 15%")
            {
                OverrideColor = WinterCyan
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold eye of winter sees all and freezes hope'")
            {
                OverrideColor = WinterBlue * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestReapersMark>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Seasonal Sight - Vivaldi (all seasons) tier ranger accessory.
    /// Simple effect: +10% ranged damage, biome-dependent debuffs on ranged hit.
    /// </summary>
    public class VivaldisSeSonalSight : ModItem
    {
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
            markingPlayer.hasVivaldisSeSonalSight = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color cyclingColor = Main.hslToRgb(hue, 0.8f, 0.6f);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10% ranged damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Ranged attacks inflict biome-based debuffs")
            {
                OverrideColor = cyclingColor
            });

            tooltips.Add(new TooltipLine(Mod, "BiomeNote", "Snow: Frostburn | Desert: On Fire! | Jungle: Poisoned | Other: Confused")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons dance eternal in the hunter's gaze'")
            {
                OverrideColor = cyclingColor * 0.8f
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
}
