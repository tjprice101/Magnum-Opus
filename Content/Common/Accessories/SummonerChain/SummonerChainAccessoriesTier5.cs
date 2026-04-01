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

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    /// <summary>
    /// Moonlit Symphony Wand - Moonlight Sonata themed summoner accessory.
    /// Simple effect: +10% summon damage at night.
    /// </summary>
    public class MoonlitSymphonyWand : ModItem
    {
        private static readonly Color MoonlightPurple = new Color(140, 100, 200);

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
            conductor.HasMoonlitSymphonyWand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10% summon damage at night")
            {
                OverrideColor = new Color(200, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon rises, and your symphony begins'")
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
    /// Heroic General's Baton - Eroica themed summoner accessory.
    /// Simple effect: +15% summon damage, +5% crit.
    /// </summary>
    public class HeroicGeneralsBaton : ModItem
    {
        private static readonly Color EroicaGold = new Color(200, 80, 80);

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
            conductor.HasHeroicGeneralsBaton = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% summon damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+5% summon critical strike chance")
            {
                OverrideColor = new Color(255, 220, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Rally your troops! Victory awaits!'")
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
    /// Infernal Choir Master's Rod - La Campanella themed summoner accessory.
    /// Simple effect: Minions inflict burn on hit.
    /// </summary>
    public class InfernalChoirMastersRod : ModItem
    {
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);

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
            conductor.HasInfernalChoirMastersRod = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Minions inflict burn on hit")
            {
                OverrideColor = CampanellaOrange
            });

            tooltips.Add(new TooltipLine(Mod, "BurnNote", "Inflicts On Fire! for 5 seconds")
            {
                OverrideColor = new Color(255, 180, 80)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let the inferno sing through your servants'")
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
    /// Enigma's Hivemind Link - Enigma Variations themed summoner accessory.
    /// Simple effect: Minions can phase through walls.
    /// </summary>
    public class EnigmasHivemindLink : ModItem
    {
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);

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
            conductor.HasEnigmasHivemindLink = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Minions can phase through walls")
            {
                OverrideColor = EnigmaPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Your minions exist beyond the boundaries of reality'")
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
    /// Swan's Graceful Direction - Swan Lake themed summoner accessory.
    /// Simple effect: When hit, gain +25% summon damage for 5s (30s cooldown).
    /// </summary>
    public class SwansGracefulDirection : ModItem
    {
        private static readonly Color SwanWhite = new Color(240, 245, 255);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<SwanRarity>();
            Item.value = Item.sellPrice(gold: 65);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasSwansGracefulDirection = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var conductor = player.GetModPlayer<ConductorPlayer>();

            tooltips.Add(new TooltipLine(Mod, "Effect1", "When hit, gain 'Grace' buff")
            {
                OverrideColor = SwanWhite
            });

            tooltips.Add(new TooltipLine(Mod, "GraceBuff", "Grace: +25% summon damage for 5 seconds")
            {
                OverrideColor = new Color(220, 225, 235)
            });

            tooltips.Add(new TooltipLine(Mod, "Cooldown", "30 second cooldown")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            // Show cooldown status if equipped
            if (conductor.HasSwansGracefulDirection && conductor.gracefulDodgeCooldown > 0)
            {
                int secondsLeft = conductor.gracefulDodgeCooldown / 60;
                tooltips.Add(new TooltipLine(Mod, "CooldownStatus", $"Cooldown: {secondsLeft}s remaining")
                {
                    OverrideColor = new Color(255, 150, 150)
                });
            }
            else if (conductor.HasSwansGracefulDirection)
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "Ready!")
                {
                    OverrideColor = new Color(150, 255, 150)
                });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Elegance in command, grace under fire'")
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
    /// Fate's Cosmic Dominion - Fate themed summoner accessory.
    /// Simple effect: +20% summon damage.
    /// </summary>
    public class FatesCosmicDominion : ModItem
    {
        private static readonly Color FateCrimson = new Color(180, 40, 80);

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.value = Item.sellPrice(gold: 80);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var conductor = player.GetModPlayer<ConductorPlayer>();
            conductor.HasFatesCosmicDominion = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% summon damage")
            {
                OverrideColor = new Color(255, 150, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Destiny itself bows to your orchestra'")
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
