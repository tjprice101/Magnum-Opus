using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    /// <summary>
    /// Moonlit Overflow Star - Moonlight Sonata themed mage accessory.
    /// Simple effect: At <50 mana, next spell costs 0 mana.
    /// </summary>
    public class MoonlitOverflowStar : ModItem
    {
        private static readonly Color MoonlightPurple = new Color(140, 100, 200);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasMoonlitOverflowStar = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "At <50 mana: your next spell costs 0 mana")
            {
                OverrideColor = new Color(200, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon hangs motionless at the edge of silence'")
            {
                OverrideColor = MoonlightPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfMoonlightSonata>(20)
                .AddIngredient<MoonlightsResonantEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Heroic Arcane Surge - Eroica themed mage accessory.
    /// Simple effect: Taking damage grants brief invincibility (30s cooldown).
    /// </summary>
    public class HeroicArcaneSurge : ModItem
    {
        private static readonly Color EroicaGold = new Color(200, 80, 80);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasHeroicArcaneSurge = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "When hit, gain 1 second of invulnerability")
            {
                OverrideColor = new Color(255, 220, 220)
            });

            tooltips.Add(new TooltipLine(Mod, "Cooldown", "30 second cooldown")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's courage shines brightest at the brink of exhaustion'")
            {
                OverrideColor = EroicaGold * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEroica>(20)
                .AddIngredient<EroicasResonantEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Infernal Mana Inferno - La Campanella themed mage accessory.
    /// Simple effect: Magic attacks leave fire trails that damage enemies.
    /// </summary>
    public class InfernalManaInferno : ModItem
    {
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasInfernalManaInferno = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Magic attacks create fire sparks on hit")
            {
                OverrideColor = CampanellaOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for those who dance with the inferno'")
            {
                OverrideColor = CampanellaOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfLaCampanella>(20)
                .AddIngredient<LaCampanellaResonantEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Enigma's Negative Space - Enigma Variations themed mage accessory.
    /// Simple effect: Magic attacks hit enemies twice (50% second hit).
    /// </summary>
    public class EnigmasNegativeSpace : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasEnigmasNegativeSpace = true;

            // Enigma grants magic crit bonus
            player.GetCritChance(DamageClass.Magic) += 10;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10% magic critical strike chance")
            {
                OverrideColor = EnigmaPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the negative space between questions, answers multiply'")
            {
                OverrideColor = EnigmaPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEnigma>(20)
                .AddIngredient<EnigmaResonantEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Swan's Balanced Flow - Swan Lake themed mage accessory.
    /// Simple effect: Killing enemies grants +20% magic damage for 5 seconds.
    /// </summary>
    public class SwansBalancedFlow : ModItem
    {
        private static readonly Color SwanWhite = new Color(240, 245, 255);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasSwansBalancedFlow = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Killing enemies grants 'Grace' buff")
            {
                OverrideColor = SwanWhite
            });

            tooltips.Add(new TooltipLine(Mod, "GraceBuff", "Grace: +20% magic damage for 5 seconds")
            {
                OverrideColor = new Color(200, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The swan finds balance in the space between falling and flight'")
            {
                OverrideColor = SwanWhite * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfSwanLake>(20)
                .AddIngredient<SwansResonanceEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    /// <summary>
    /// Fate's Cosmic Reservoir - Fate themed mage accessory.
    /// Simple effect: Magic attacks ignore 25% of enemy defense.
    /// </summary>
    public class FatesCosmicReservoir : ModItem
    {
        private static readonly Color FateCrimson = new Color(180, 40, 80);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasFatesCosmicReservoir = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Magic attacks ignore 25% of enemy defense")
            {
                OverrideColor = new Color(255, 200, 220)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Destiny itself bows to those who master the cosmic void'")
            {
                OverrideColor = FateCrimson * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfFate>(30)
                .AddIngredient<FateResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
