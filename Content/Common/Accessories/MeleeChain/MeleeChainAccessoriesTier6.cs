using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region T7 - Nocturnal Symphony Band

    /// <summary>
    /// T7: Nocturnal Symphony Band - Post-Fate Nachtmusik tier.
    /// Max Resonance 70. +2 per hit at night, constellation trails at 50+, Starfall Slash at 60 cost.
    /// </summary>
    public class NocturnalSymphonyBand : ModItem
    {
        private static readonly Color NachtmusikDeepPurple = new Color(45, 27, 78);
        private static readonly Color NachtmusikGold = new Color(255, 215, 0);
        private static readonly Color NachtmusikViolet = new Color(123, 104, 238);
        private static readonly Color NachtmusikStarWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 20);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasNocturnalSymphonyBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% melee damage at night, +10% during day")
            {
                OverrideColor = NachtmusikGold
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Crits grant Notturno for 2s (+8% damage, +5% crit)")
            {
                OverrideColor = NachtmusikGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night sky conducts its eternal symphony'")
            {
                OverrideColor = new Color(45, 27, 78) * 1.5f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicSymphony>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T8 - Infernal Fortissimo Band

    /// <summary>
    /// T8: Infernal Fortissimo Band - Post-Fate Dies Irae tier.
    /// Max Resonance 80. Judgment Burn at 60+, no decay during bosses, Hellfire Crescendo at 70 cost.
    /// </summary>
    public class InfernalFortissimoBandT8 : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MeleeChain/InfernalFortissimoBand";
        
        private static readonly Color DiesIraeCrimson = new Color(180, 40, 40);
        private static readonly Color DiesIraeBlack = new Color(30, 20, 25);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);
        private static readonly Color DiesIraeGold = new Color(255, 200, 80);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasInfernalFortissimoBandT8 = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% melee damage during bosses, +12% otherwise")
            {
                OverrideColor = new Color(255, 200, 80)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Crits inflict Wrathfire (fire damage + defense shred)")
            {
                OverrideColor = DiesIraeCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The day of wrath burns with righteous fury'")
            {
                OverrideColor = DiesIraeCrimson * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalSymphonyBand>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T9 - Jubilant Crescendo Band

    /// <summary>
    /// T9: Jubilant Crescendo Band - Post-Fate Ode to Joy tier.
    /// Max Resonance 90. 2% lifesteal at 70+, +5 resonance on kill, Blooming Fury at 80 cost.
    /// </summary>
    public class JubilantCrescendoBand : ModItem
    {
        private static readonly Color OdeWhite = new Color(255, 255, 255);
        private static readonly Color OdeBlack = new Color(30, 30, 35);
        private static readonly Color OdeIridescent = new Color(255, 220, 255);
        private static readonly Color OdeRose = new Color(255, 200, 220);
        private static readonly Color OdeGold = new Color(255, 215, 0);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasJubilantCrescendoBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% melee damage, lifesteal capped at 20 HP/s")
            {
                OverrideColor = new Color(180, 255, 180)
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 10th kill: Triumphant Crescendo 5s (+12% dmg, +8% crit, heals 8% max HP)")
            {
                OverrideColor = OdeGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Freude, schoener Goetterfunken - Joy, beautiful spark of divinity'")
            {
                OverrideColor = OdeGold * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalFortissimoBandT8>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T10 - Eternal Resonance Band

    /// <summary>
    /// T10: Eternal Resonance Band - Post-Fate Clair de Lune tier (ULTIMATE).
    /// Max Resonance 100. Never decays, temporal echoes at 80+, time slow at 100, Temporal Finale at 90 cost.
    /// </summary>
    public class EternalResonanceBand : ModItem
    {
        private static readonly Color ClairGray = new Color(80, 75, 90);
        private static readonly Color ClairIridescent = new Color(200, 180, 220);
        private static readonly Color ClairBrass = new Color(205, 170, 125);
        private static readonly Color ClairCrimson = new Color(180, 60, 80);
        private static readonly Color ClairWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasEternalResonanceBand = true;

            // T10 stat bonus
            player.GetDamage(DamageClass.Melee) += 0.15f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% melee damage, +8% melee speed")
            {
                OverrideColor = ClairIridescent
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time bends to the rhythm of eternity itself'")
            {
                OverrideColor = ClairBrass * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantCrescendoBand>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 1 - Starfall Judgment Gauntlet

    /// <summary>
    /// Starfall Judgment Gauntlet - Fusion of Nachtmusik + Dies Irae.
    /// Combines stellar and infernal resonance powers.
    /// </summary>
    public class StarfallJudgmentGauntlet : ModItem
    {
        private static readonly Color StarfallPurple = new Color(100, 50, 150);
        private static readonly Color StarfallCrimson = new Color(200, 60, 60);
        private static readonly Color StarfallGold = new Color(255, 200, 100);

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasStarfallJudgmentGauntlet = true;

            // Combined night bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Melee) += 0.20f;
            }

            player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
            player.GetArmorPenetration(DamageClass.Melee) += 12f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float blend = (float)System.Math.Sin(Main.GameUpdateCount * 0.02f) * 0.5f + 0.5f;
            Color titleColor = Color.Lerp(StarfallPurple, StarfallCrimson, blend);

            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses the power of Nachtmusik and Dies Irae")
            {
                OverrideColor = new Color(255, 200, 100)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% melee damage at night")
            {
                OverrideColor = new Color(255, 215, 0)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect4", "+8% melee speed and +12 armor penetration")
            {
                OverrideColor = StarfallGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'When stars fall, judgment follows'")
            {
                OverrideColor = titleColor * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalSymphonyBand>(1)
                .AddIngredient<InfernalFortissimoBandT8>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 2 - Triumphant Cosmos Gauntlet

    /// <summary>
    /// Triumphant Cosmos Gauntlet - Fusion of Nachtmusik + Dies Irae + Ode to Joy.
    /// Three-theme cosmic gauntlet of celestial might.
    /// </summary>
    public class TriumphantCosmosGauntlet : ModItem
    {
        private static readonly Color TriumphPurple = new Color(120, 80, 180);
        private static readonly Color TriumphCrimson = new Color(220, 80, 100);
        private static readonly Color TriumphRose = new Color(255, 200, 220);
        private static readonly Color TriumphGold = new Color(255, 220, 120);

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasTriumphantCosmosGauntlet = true;

            // Combined night bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Melee) += 0.20f;
            }

            player.GetAttackSpeed(DamageClass.Melee) += 0.10f;
            player.GetArmorPenetration(DamageClass.Melee) += 18f;
            player.statLifeMax2 += 30;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.012f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.6f, 0.75f);

            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses Nachtmusik, Dies Irae, and Ode to Joy")
            {
                OverrideColor = new Color(255, 220, 120)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% melee damage at night")
            {
                OverrideColor = new Color(255, 215, 0)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "3% lifesteal on melee attacks")
            {
                OverrideColor = new Color(180, 255, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect5", "+10% melee speed, +18 armor penetration, +30 max life")
            {
                OverrideColor = TriumphGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three symphonies become one cosmic triumph'")
            {
                OverrideColor = titleColor * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallJudgmentGauntlet>(1)
                .AddIngredient<JubilantCrescendoBand>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 3 - Gauntlet of the Eternal Symphony

    /// <summary>
    /// Gauntlet of the Eternal Symphony - Ultimate fusion of all four Post-Fate themes.
    /// The pinnacle of melee resonance power.
    /// </summary>
    public class GauntletOfTheEternalSymphony : ModItem
    {
        private static readonly Color EternalPurple = new Color(140, 100, 200);
        private static readonly Color EternalCrimson = new Color(200, 80, 120);
        private static readonly Color EternalRose = new Color(255, 210, 230);
        private static readonly Color EternalBrass = new Color(205, 170, 125);
        private static readonly Color EternalWhite = new Color(255, 255, 255);

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasGauntletOfTheEternalSymphony = true;

            player.GetDamage(DamageClass.Melee) += 0.30f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
            player.GetArmorPenetration(DamageClass.Melee) += 25f;
            player.GetKnockback(DamageClass.Melee) += 0.20f;
            player.statLifeMax2 += 50;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f * 0.2f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.7f, 0.85f);

            tooltips.Add(new TooltipLine(Mod, "UltimateDesc", "The ultimate fusion of all four Post-Fate themes")
            {
                OverrideColor = EternalWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+30% melee damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect3", "2% lifesteal on melee attacks")
            {
                OverrideColor = new Color(180, 255, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect5", "+12% melee speed, +25 armor penetration, +20% knockback, +50 max life")
            {
                OverrideColor = EternalWhite
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Four movements become one eternal masterpiece'")
            {
                OverrideColor = new Color(205, 170, 125) * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantCosmosGauntlet>(1)
                .AddIngredient<EternalResonanceBand>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
