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
            
            player.GetDamage(DamageClass.Ranged) += 0.05f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+5% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd ranged shot deals 5% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hunt begins with a single mark'") { OverrideColor = new Color(255, 100, 100) });
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
            
            player.GetDamage(DamageClass.Ranged) += 0.07f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+7% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd ranged shot deals 8% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "2% chance for enemies to drop hearts on ranged hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring's renewal brings vitality to the hunter'") { OverrideColor = new Color(255, 183, 197) });
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
    /// Resonant Piercing Lens - Summer tier ranger accessory.
    /// Synergizes with Resonance Pierced weapons:
    /// - While holding Resonance Pierced: +30% damage vs burning enemies
    /// - Armor penetration doubled against burning enemies
    /// </summary>
    public class ResonantPiercingLens : ModItem
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
            markingPlayer.hasResonantPiercingLens = true;
            
            player.GetDamage(DamageClass.Ranged) += 0.14f;
            player.GetCritChance(DamageClass.Ranged) += 7;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+14% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+7% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd ranged shot deals 12% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "2% chance for enemies to drop hearts on ranged hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All ranged attacks apply Burning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The flames reveal every weakness'") { OverrideColor = new Color(255, 140, 0) });
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
    /// Echoing Bolt Chamber - Autumn tier ranger accessory.
    /// Synergizes with Resonant Burn:
    /// - 15% chance on ranged hit to fire a homing resonance bolt (50% weapon damage)
    /// - Critical hits on burning enemies spread Resonant Burn to 1 nearby enemy (200 units)
    /// </summary>
    public class EchoingBoltChamber : ModItem
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
            markingPlayer.hasEchoingBoltChamber = true;
            
            player.GetDamage(DamageClass.Ranged) += 0.17f;
            player.GetCritChance(DamageClass.Ranged) += 9;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+17% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+9% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd ranged shot deals 15% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "2.5% chance for enemies to drop hearts on ranged hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All ranged attacks apply Burning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "Critical hits spread Burning to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The hunter's arrows echo through flame and foe'") { OverrideColor = new Color(210, 120, 50) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantPiercingLens>(1)
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
            
            player.GetDamage(DamageClass.Ranged) += 0.19f;
            player.GetCritChance(DamageClass.Ranged) += 11;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+19% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+11% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd ranged shot deals 20% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "3% chance for enemies to drop hearts on ranged hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All ranged attacks apply Burning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "5% chance to slow enemies for 1 second on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Critical hits spread Burning and Slow to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cold eye of winter sees all and freezes hope'") { OverrideColor = new Color(150, 220, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<EchoingBoltChamber>(1)
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
    public class VivaldisSeasonalSight : ModItem
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
            markingPlayer.hasVivaldisSeasonalSight = true;
            
            player.GetDamage(DamageClass.Ranged) += 0.22f;
            player.GetCritChance(DamageClass.Ranged) += 13;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+22% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+13% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 3rd ranged shot deals 25% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "5% chance for enemies to drop hearts on ranged hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "All ranged attacks apply Burning, Frostburn, Poison, and Bleeding"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "8% chance to slow enemies for 1 second on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "Critical hits spread all debuffs to nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons dance eternal in the hunter's gaze'") { OverrideColor = new Color(150, 200, 100) });
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
