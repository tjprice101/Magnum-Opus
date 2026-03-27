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
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+5% magic damage")
            {
                OverrideColor = new Color(255, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+20 max mana")
            {
                OverrideColor = new Color(200, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The first step into harmonic resonance'")
            {
                OverrideColor = BasePurple * 0.8f
            });
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
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10% magic damage")
            {
                OverrideColor = new Color(255, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "5% chance to spawn healing petal on spell cast")
            {
                OverrideColor = SpringPink
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring's renewal flows through arcane channels'")
            {
                OverrideColor = SpringPink * 0.8f
            });
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
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "While holding Resonance Seared weapon:")
            {
                OverrideColor = new Color(255, 150, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+50 mana regen when any enemy has Resonant Burn")
            {
                OverrideColor = new Color(150, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "+4% magic damage per burning enemy (max 20% at 5)")
            {
                OverrideColor = new Color(255, 200, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Burning essence fuels the arcane fires within'")
            {
                OverrideColor = SummerOrange * 0.8f
            });
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
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Magic attacks consume Resonant Burn for burst damage")
            {
                OverrideColor = new Color(255, 150, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "BurstNote", "Burst deals 150% of remaining burn DoT as instant damage")
            {
                OverrideColor = AutumnOrange
            });

            tooltips.Add(new TooltipLine(Mod, "ManaNote", "Gain mana equal to 15% of burst damage (cap 30 per hit)")
            {
                OverrideColor = new Color(150, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The catalyst transforms slow burns to sudden devastation'")
            {
                OverrideColor = AutumnOrange * 0.8f
            });
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
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% magic damage")
            {
                OverrideColor = new Color(255, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+50 max mana")
            {
                OverrideColor = new Color(200, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's void holds immense power'")
            {
                OverrideColor = WinterBlue * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestSoulVessel>(1)
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
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color cyclingColor = Main.hslToRgb(hue, 0.8f, 0.6f);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% magic damage")
            {
                OverrideColor = new Color(255, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Magic attacks inflict biome-based debuffs")
            {
                OverrideColor = cyclingColor
            });

            tooltips.Add(new TooltipLine(Mod, "BiomeNote", "Snow: Frostburn | Desert: On Fire! | Jungle: Poisoned | Other: Confused")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons harmonize in arcane unity'")
            {
                OverrideColor = cyclingColor * 0.8f
            });
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
