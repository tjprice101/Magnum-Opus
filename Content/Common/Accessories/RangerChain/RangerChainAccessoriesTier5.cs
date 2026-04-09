using Microsoft.Xna.Framework;
using System.Collections.Generic;
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

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    /// <summary>
    /// Moonlit Predator's Gaze - Moonlight Sonata themed ranger accessory.
    /// Simple effect: See marked enemies through walls.
    /// </summary>
    public class MoonlitPredatorsGaze : ModItem
    {
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);

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
            markingPlayer.hasMoonlitPredatorsGaze = true;
            player.nightVision = true;
            player.detectCreature = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Night vision and see enemies through walls")
            {
                OverrideColor = new Color(200, 200, 230)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Under the moonlight, no prey can hide'")
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
    /// Heroic Deadeye - Eroica themed ranger accessory.
    /// Simple effect: +12% ranged damage, +8% ranged crit.
    /// </summary>
    public class HeroicDeadeye : ModItem
    {
        private static readonly Color EroicaGold = new Color(255, 200, 80);

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
            markingPlayer.hasHeroicDeadeye = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+12% ranged damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+8% ranged critical strike chance")
            {
                OverrideColor = new Color(255, 220, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's aim never wavers'")
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
    /// Infernal Executioner's Brand - La Campanella themed ranger accessory.
    /// Simple effect: Ranged attacks inflict burn damage over time.
    /// </summary>
    public class InfernalExecutionersBrand : ModItem
    {
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);

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
            markingPlayer.hasInfernalExecutionersBrand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Ranged attacks inflict infernal burn")
            {
                OverrideColor = CampanellaOrange
            });

            tooltips.Add(new TooltipLine(Mod, "BurnNote", "Inflicts On Fire! for 5 seconds")
            {
                OverrideColor = new Color(255, 180, 80)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell tolls for those who bear the brand'")
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
    /// Enigma's Paradox Mark - Enigma Variations themed ranger accessory.
    /// Simple effect: 15% chance to spawn bonus projectile on ranged hit.
    /// </summary>
    public class EnigmasParadoxMark : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);

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
            markingPlayer.hasEnigmasParadoxMark = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "15% chance for visual spark effects on ranged hit")
            {
                OverrideColor = new Color(80, 180, 120)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The mark spreads like questions without answers'")
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
    /// Swan's Graceful Hunt - Swan Lake themed ranger accessory.
    /// Simple effect: When hit, gain 5s of +20% ranged damage (30s cooldown).
    /// </summary>
    public class SwansGracefulHunt : ModItem
    {
        private static readonly Color SwanWhite = new Color(255, 255, 255);

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
            markingPlayer.hasSwansGracefulHunt = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();

            tooltips.Add(new TooltipLine(Mod, "Effect1", "When hit, gain 'Grace' buff")
            {
                OverrideColor = SwanWhite
            });

            tooltips.Add(new TooltipLine(Mod, "GraceBuff", "Grace: +20% ranged damage for 5 seconds")
            {
                OverrideColor = new Color(220, 225, 235)
            });

            tooltips.Add(new TooltipLine(Mod, "Cooldown", "30 second cooldown")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            // Show cooldown status if equipped
            if (markingPlayer.hasSwansGracefulHunt && markingPlayer.gracefulDodgeCooldown > 0)
            {
                int secondsLeft = markingPlayer.gracefulDodgeCooldown / 60;
                tooltips.Add(new TooltipLine(Mod, "CooldownStatus", $"Cooldown: {secondsLeft}s remaining")
                {
                    OverrideColor = new Color(255, 150, 150)
                });
            }
            else if (markingPlayer.hasSwansGracefulHunt)
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "Ready!")
                {
                    OverrideColor = new Color(150, 255, 150)
                });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Grace in the hunt, elegance in the kill'")
            {
                OverrideColor = new Color(220, 225, 235) * 0.8f
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
    /// Fate's Cosmic Verdict - Fate themed ranger accessory.
    /// Simple effect: +15% ranged damage.
    /// </summary>
    public class FatesCosmicVerdict : ModItem
    {
        private static readonly Color FateCrimson = new Color(200, 80, 120);

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
            markingPlayer.hasFatesCosmicVerdict = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% ranged damage")
            {
                OverrideColor = new Color(255, 150, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cosmos itself judges those marked for death'")
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
