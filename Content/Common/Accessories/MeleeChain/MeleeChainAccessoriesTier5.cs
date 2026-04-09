using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region Moonlight Sonata Theme

    /// <summary>
    /// Moonlit Sonata Band - Moonlight Sonata themed melee accessory.
    /// Simple effect: Melee critical hits spawn homing moonbeams.
    /// </summary>
    public class MoonlitSonataBand : ModItem
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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasMoonlitSonataBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Melee critical hits create lunar sparkle effects")
            {
                OverrideColor = new Color(220, 220, 235)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The moon's soft melody guides your blade through the darkness'")
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

    #endregion

    #region Eroica Theme

    /// <summary>
    /// Heroic Crescendo - Eroica themed melee accessory.
    /// Simple effect: +15% melee damage, +10% melee crit chance.
    /// </summary>
    public class HeroicCrescendo : ModItem
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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasHeroicCrescendo = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% melee damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+10% melee critical strike chance")
            {
                OverrideColor = new Color(255, 255, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'A hero's strength crescendos when hope is needed most'")
            {
                OverrideColor = EroicaGold * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEroica>(1)
                .AddIngredient<EroicasResonantEnergy>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region La Campanella Theme

    /// <summary>
    /// Infernal Fortissimo - La Campanella themed melee accessory.
    /// Simple effect: Enemies killed by melee attacks explode, damaging nearby enemies.
    /// </summary>
    public class InfernalFortissimo : ModItem
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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasInfernalFortissimo = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Enemies killed with melee attacks create a fire burst")
            {
                OverrideColor = CampanellaOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bell's infernal toll echoes through eternity'")
            {
                OverrideColor = CampanellaOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfLaCampanella>(1)
                .AddIngredient<LaCampanellaResonantEnergy>(5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Enigma Variations Theme

    /// <summary>
    /// Enigma's Dissonance - Enigma Variations themed melee accessory.
    /// Simple effect: Hit enemies take delayed burst damage after 2 seconds.
    /// </summary>
    public class EnigmasDissonance : ModItem
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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasEnigmasDissonance = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Melee hits mark enemies with Paradox")
            {
                OverrideColor = EnigmaPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Paradox", "Paradox: Deals damage over 3 seconds")
            {
                OverrideColor = new Color(80, 180, 120)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The answer to the enigma... is another question'")
            {
                OverrideColor = EnigmaPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfEnigma>(1)
                .AddIngredient<EnigmaResonantEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Swan Lake Theme

    /// <summary>
    /// Swan's Perfect Measure - Swan Lake themed melee accessory.
    /// Simple effect: When hit, gain 2 seconds of invulnerability (30 second cooldown).
    /// </summary>
    public class SwansPerfectMeasure : ModItem
    {
        private static readonly Color SwanWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasSwansPerfectMeasure = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();

            tooltips.Add(new TooltipLine(Mod, "Effect1", "When hit, gain 2 seconds of invulnerability")
            {
                OverrideColor = SwanWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Cooldown", "30 second cooldown")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            // Show cooldown status if equipped
            if (modPlayer.hasSwansPerfectMeasure && modPlayer.perfectDodgeCooldown > 0)
            {
                int secondsLeft = modPlayer.perfectDodgeCooldown / 60;
                tooltips.Add(new TooltipLine(Mod, "CooldownStatus", $"Cooldown: {secondsLeft}s remaining")
                {
                    OverrideColor = new Color(255, 150, 150)
                });
            }
            else if (modPlayer.hasSwansPerfectMeasure)
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "Ready!")
                {
                    OverrideColor = new Color(150, 255, 150)
                });
            }

            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the swan's final dance, every measure is perfect'")
            {
                OverrideColor = new Color(220, 225, 235) * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfSwanLake>(1)
                .AddIngredient<SwansResonanceEnergy>(5)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fate Theme

    /// <summary>
    /// Fate's Cosmic Symphony - Fate themed melee accessory.
    /// Simple effect: +25% melee damage at night, +15% during day. Attacks leave cosmic trails.
    /// </summary>
    public class FatesCosmicSymphony : ModItem
    {
        private static readonly Color FateDarkPink = new Color(200, 80, 120);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasFatesCosmicSymphony = true;

            // Day/night scaling damage
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Melee) += 0.25f;
            }
            else
            {
                player.GetDamage(DamageClass.Melee) += 0.15f;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% melee damage at night")
            {
                OverrideColor = new Color(180, 100, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+15% melee damage during day")
            {
                OverrideColor = new Color(255, 200, 150)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Fate's melody rings through every strike, steady and sure'")
            {
                OverrideColor = FateDarkPink * 0.8f
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

    #endregion
}
