using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    #region T7: Nocturnal Predator's Sight (Nachtmusik Theme)

    /// <summary>
    /// T7 Ranger accessory - Nachtmusik theme (post-Fate).
    /// Simple effect: +20% ranged damage at night.
    /// </summary>
    public class NocturnalPredatorsSight : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 85);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasNocturnalPredatorsSight = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% ranged damage at night")
            {
                OverrideColor = NachtmusikGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars reveal all that hides in darkness'")
            {
                OverrideColor = NachtmusikPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T8: Infernal Executioner's Sight (Dies Irae Theme)

    /// <summary>
    /// T8 Ranger accessory - Dies Irae theme (post-Fate).
    /// Simple effect: +25% ranged damage during boss fights.
    /// </summary>
    public class InfernalExecutionersSight : ModItem
    {
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 95);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasInfernalExecutionersSight = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% ranged damage during boss fights")
            {
                OverrideColor = DiesIraeCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hellfire brands those condemned to oblivion'")
            {
                OverrideColor = DiesIraeCrimson * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T9: Jubilant Hunter's Sight (Ode to Joy Theme)

    /// <summary>
    /// T9 Ranger accessory - Ode to Joy theme (post-Fate).
    /// Simple effect: Killing enemies with ranged attacks restores 2 HP.
    /// </summary>
    public class JubilantHuntersSight : ModItem
    {
        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 105);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasJubilantHuntersSight = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Killing enemies with ranged attacks restores 2 HP")
            {
                OverrideColor = OdeToJoyRose
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Nature's blessing flows through the hunt'")
            {
                OverrideColor = OdeToJoyRose * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T10: Eternal Verdict Sight (Clair de Lune Theme)

    /// <summary>
    /// T10 Ranger accessory - Clair de Lune theme (post-Fate).
    /// Simple effect: Ranged attacks hit twice (50% second hit).
    /// </summary>
    public class EternalVerdictSight : ModItem
    {
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color ClairDeLuneCrimson = new Color(180, 80, 100);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasEternalVerdictSight = true;

            // T10 stat bonus
            player.GetDamage(DamageClass.Ranged) += 0.15f;
            player.GetCritChance(DamageClass.Ranged) += 8f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% ranged damage, +8% ranged crit")
            {
                OverrideColor = ClairDeLuneCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time itself marks your prey across all moments'")
            {
                OverrideColor = ClairDeLuneBrass * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 1: Starfall Executioner's Scope (Nachtmusik + Dies Irae)

    /// <summary>
    /// Fusion Tier 1 Ranger accessory - combines Nachtmusik and Dies Irae.
    /// Fuses stellar precision with hellfire judgment.
    /// </summary>
    public class StarfallExecutionersScope : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color FusionGold = new Color(255, 180, 80);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 130);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasStarfallExecutionersScope = true;

            player.GetCritChance(DamageClass.Ranged) += 10f;
            player.GetArmorPenetration(DamageClass.Ranged) += 12f;
            player.ammoCost80 = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses the power of Nachtmusik and Dies Irae")
            {
                OverrideColor = FusionGold
            });

            tooltips.Add(new TooltipLine(Mod, "Effect4", "+10% ranged crit, +12 armor penetration, 20% chance to not consume ammo")
            {
                OverrideColor = FusionGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire unite in cosmic judgment'")
            {
                OverrideColor = new Color(180, 120, 160) * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalPredatorsSight>(1)
                .AddIngredient<InfernalExecutionersSight>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 2: Triumphant Verdict Scope (+ Ode to Joy)

    /// <summary>
    /// Fusion Tier 2 Ranger accessory - adds Ode to Joy to the fusion.
    /// Combines stellar precision, hellfire judgment, and nature's blessing.
    /// </summary>
    public class TriumphantVerdictScope : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        private static readonly Color FusionTriumph = new Color(255, 220, 160);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 160);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasTriumphantVerdictScope = true;

            player.GetCritChance(DamageClass.Ranged) += 12f;
            player.GetArmorPenetration(DamageClass.Ranged) += 18f;
            player.ammoCost75 = true;
            player.scope = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.012f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.6f, 0.75f);

            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses Nachtmusik, Dies Irae, and Ode to Joy")
            {
                OverrideColor = FusionTriumph
            });

            tooltips.Add(new TooltipLine(Mod, "Effect5", "+12% ranged crit, +18 armor penetration, 25% chance to not consume ammo")
            {
                OverrideColor = FusionTriumph
            });

            tooltips.Add(new TooltipLine(Mod, "Effect6", "Scope (extended zoom)")
            {
                OverrideColor = FusionTriumph
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three symphonies unite in triumphant harmony'")
            {
                OverrideColor = titleColor * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallExecutionersScope>(1)
                .AddIngredient<JubilantHuntersSight>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 3: Scope of the Eternal Verdict (Ultimate - + Clair de Lune)

    /// <summary>
    /// Ultimate Fusion Ranger accessory - all four Post-Fate themes combined.
    /// The pinnacle of the ranger accessory system.
    /// </summary>
    public class ScopeOfTheEternalVerdict : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color UltimatePrismatic = new Color(255, 230, 200);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 200);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            markingPlayer.hasScopeOfTheEternalVerdict = true;

            player.GetDamage(DamageClass.Ranged) += 0.30f;
            player.GetCritChance(DamageClass.Ranged) += 12f;
            player.GetArmorPenetration(DamageClass.Ranged) += 20f;
            player.ammoBox = true;
            player.scope = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f * 0.2f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.7f, 0.85f);

            tooltips.Add(new TooltipLine(Mod, "UltimateDesc", "The ultimate fusion of all four Post-Fate themes")
            {
                OverrideColor = UltimatePrismatic
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+30% ranged damage")
            {
                OverrideColor = NachtmusikPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect5", "+12% ranged crit, +20 armor penetration, Ammo Box effect")
            {
                OverrideColor = UltimatePrismatic
            });

            tooltips.Add(new TooltipLine(Mod, "Effect6", "Scope (extended zoom)")
            {
                OverrideColor = UltimatePrismatic
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every note finds its mark'")
            {
                OverrideColor = ClairDeLuneBrass * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantVerdictScope>(1)
                .AddIngredient<EternalVerdictSight>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
