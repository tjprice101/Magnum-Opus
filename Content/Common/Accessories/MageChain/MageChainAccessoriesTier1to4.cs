using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Materials.Foundation;
using MagnumOpus.Content.Spring.Materials;
using MagnumOpus.Content.Summer.Materials;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Content.Winter.Materials;
using MagnumOpus.Content.Seasons.Accessories;

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    /// <summary>
    /// Resonant Overflow Gem - Base tier magic chain accessory.
    /// Simple effect: +5% magic damage, +20 max mana.
    /// </summary>
    public class ResonantOverflowGem : ModItem
    {
        private static readonly Color BasePurple = new Color(100, 150, 255);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OverflowPlayer>();
            modPlayer.hasResonantOverflowGem = true;
            
            player.GetDamage(DamageClass.Magic) += 0.05f;
            player.statManaMax2 += 20;
            player.manaRegenBonus += 10; // ~+2 mana/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+5% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+20 maximum mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Increased mana regeneration (+2 per second)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The first step into harmonic resonance'") { OverrideColor = new Color(100, 150, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ManaRegenerationBand)
                .AddIngredient<ResonantCrystalShard>(10)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    /// <summary>
    /// Spring Arcane Conduit - Spring tier magic chain accessory.
    /// Simple effect: +10% magic damage, 5% chance to spawn healing petal on spell cast.
    /// </summary>
    public class SpringArcaneConduit : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OverflowPlayer>();
            modPlayer.hasSpringArcaneConduit = true;
            
            player.GetDamage(DamageClass.Magic) += 0.08f;
            player.statManaMax2 += 25;
            player.manaRegenBonus += 20; // ~+4 mana/s
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+8% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+25 maximum mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Increased mana regeneration (+4 per second)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "5% chance to heal 20 HP on magic cast"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring's renewal flows through arcane channels'") { OverrideColor = new Color(255, 183, 197) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantOverflowGem>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    /// <summary>
    /// Seared Mana Conduit - Summer tier magic chain accessory.
    /// Synergizes with Resonance Seared weapons:
    /// - While holding Resonance Seared: +50 mana regen when any enemy has Resonant Burn
    /// - +4% magic damage per burning enemy (max 20% at 5 enemies)
    /// </summary>
    public class SearedManaConduit : ModItem
    {
        private static readonly Color SummerOrange = new Color(255, 140, 0);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OverflowPlayer>();
            modPlayer.hasSearedManaConduit = true;
            
            player.GetDamage(DamageClass.Magic) += 0.10f;
            player.statManaMax2 += 30;
            player.manaRegenBonus += 35; // ~+7 mana/s
            player.manaCost -= 0.03f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30 maximum mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Increased mana regeneration (+7 per second)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-3% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "5% chance to heal 25 HP on magic cast"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "All magic attacks apply Burning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Burning essence fuels the arcane fires within'") { OverrideColor = new Color(255, 140, 0) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringArcaneConduit>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Arcane Resonance Catalyst - Autumn tier magic chain accessory.
    /// Synergizes with Resonant Burn:
    /// - Magic attacks consume Resonant Burn to deal 150% of remaining DoT as instant burst damage
    /// - Gain mana equal to 15% of burst damage dealt (cap 30 per hit)
    /// </summary>
    public class ArcaneResonanceCatalyst : ModItem
    {
        private static readonly Color AutumnOrange = new Color(255, 100, 30);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 8);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OverflowPlayer>();
            modPlayer.hasArcaneResonanceCatalyst = true;
            
            player.GetDamage(DamageClass.Magic) += 0.12f;
            player.statManaMax2 += 35;
            player.manaRegenBonus += 45; // ~+9 mana/s
            player.manaCost -= 0.05f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+12% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35 maximum mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Increased mana regeneration (+9 per second)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-5% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "5% chance to heal 30 HP on magic cast"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "All magic attacks apply Burning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The catalyst transforms slow burns to sudden devastation'") { OverrideColor = new Color(255, 100, 30) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SearedManaConduit>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Permafrost Void Heart - Winter tier magic chain accessory.
    /// Simple effect: +15% magic damage, +50 max mana.
    /// </summary>
    public class PermafrostVoidHeart : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 12);
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OverflowPlayer>();
            modPlayer.hasPermafrostVoidHeart = true;
            
            player.GetDamage(DamageClass.Magic) += 0.15f;
            player.statManaMax2 += 40;
            player.manaRegenBonus += 55; // ~+11 mana/s
            player.manaCost -= 0.05f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+40 maximum mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Increased mana regeneration (+11 per second)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-5% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Every 10th magic cast is free"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "5% chance to heal 32 HP on magic cast"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "All magic attacks apply Burning on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "5% chance to slow enemies for 1 second on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's void holds immense power'") { OverrideColor = new Color(150, 220, 255) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ArcaneResonanceCatalyst>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Harmonic Core - Vivaldi (all seasons) tier magic chain accessory.
    /// Simple effect: +20% magic damage, biome-dependent debuff on magic hit.
    /// </summary>
    public class VivaldisHarmonicCore : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Lime;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OverflowPlayer>();
            modPlayer.hasVivaldisHarmonicCore = true;
            
            player.GetDamage(DamageClass.Magic) += 0.17f;
            player.statManaMax2 += 45;
            player.manaRegenBonus += 65; // ~+13 mana/s
            player.manaCost -= 0.07f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+17% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+45 maximum mana"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Increased mana regeneration (+13 per second)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-7% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Every 8th magic cast is free"));
            tooltips.Add(new TooltipLine(Mod, "Effect6", "5% chance to heal 35 HP on magic cast"));
            tooltips.Add(new TooltipLine(Mod, "Effect7", "All magic attacks apply Burning, Frostburn, Poison, and Bleeding"));
            tooltips.Add(new TooltipLine(Mod, "Effect8", "8% chance to slow enemies for 1 second on hit"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons harmonize in arcane unity'") { OverrideColor = new Color(150, 200, 100) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostVoidHeart>(1)
                .AddIngredient<CycleOfSeasons>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
