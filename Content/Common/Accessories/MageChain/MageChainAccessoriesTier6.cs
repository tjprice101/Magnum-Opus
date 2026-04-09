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

namespace MagnumOpus.Content.Common.Accessories.MageChain
{
    #region T7: Nocturnal Harmonic Overflow (Nachtmusik Theme)

    /// <summary>
    /// T7 Mage accessory - Nachtmusik theme (post-Fate).
    /// Simple effect: +20% magic damage at night.
    /// </summary>
    public class NocturnalHarmonicOverflow : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/NocturnalOverflowStar";

        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color NachtmusikGold = new Color(255, 215, 140);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 85);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasNocturnalHarmonicOverflow = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% magic damage at night, +10% during day")
            {
                OverrideColor = NachtmusikGold
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+15 mana regen at night")
            {
                OverrideColor = NachtmusikGold
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Kills grant Serenade's Refrain 3s (restores 5% max mana)")
            {
                OverrideColor = NachtmusikGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The night sky sings through your spellwork'")
            {
                OverrideColor = NachtmusikPurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FatesCosmicReservoir>(1)
                .AddIngredient<NachtmusikResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T8: Infernal Mana Cataclysm (Dies Irae Theme)

    /// <summary>
    /// T8 Mage accessory - Dies Irae theme (post-Fate).
    /// Simple effect: +25% magic damage during boss fights.
    /// </summary>
    public class InfernalManaCataclysm : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/InfernalManaInferno";

        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color DiesIraeOrange = new Color(255, 120, 40);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 95);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasInfernalManaCataclysm = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+25% magic damage during bosses, +12% otherwise")
            {
                OverrideColor = DiesIraeCrimson
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Kills grant Tuba Mirum 3s (restores 8% max mana, +8% magic damage)")
            {
                OverrideColor = DiesIraeCrimson
            });
            tooltips.Add(new TooltipLine(Mod, "Effect3", "5% on hit: Lacrimosa — enemy takes increased magic damage for 4s")
            {
                OverrideColor = DiesIraeCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Hellfire consumes the void, forging destruction anew'")
            {
                OverrideColor = DiesIraeCrimson * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalHarmonicOverflow>(1)
                .AddIngredient<DiesIraeResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T9: Jubilant Arcane Celebration (Ode to Joy Theme)

    /// <summary>
    /// T9 Mage accessory - Ode to Joy theme (post-Fate).
    /// Simple effect: Casting spells heals 1 HP per 20 mana spent.
    /// </summary>
    public class JubilantArcaneCelebration : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/JubilantOverflowBlossom";

        private static readonly Color OdeToJoyWhite = new Color(255, 255, 255);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 105);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasJubilantArcaneCelebration = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% magic damage, heals 2 HP per hit (max 8 HP/s)")
            {
                OverrideColor = OdeToJoyRose
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "20% on kill: Arcane Jubilee 3s (restores 10 mana, +5% magic damage)")
            {
                OverrideColor = OdeToJoyRose
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Joy flows through the very fabric of magic'")
            {
                OverrideColor = OdeToJoyRose * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<InfernalManaCataclysm>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region T10: Eternal Overflow Mastery (Clair de Lune Theme)

    /// <summary>
    /// T10 Mage accessory - Clair de Lune theme (post-Fate).
    /// Simple effect: Magic attacks hit twice (50% second hit).
    /// </summary>
    public class EternalOverflowMastery : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/EternalOverflowNexus";

        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color ClairDeLuneCrimson = new Color(180, 80, 100);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 120);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasEternalOverflowMastery = true;

            // T10 stat bonus
            player.GetDamage(DamageClass.Magic) += 0.15f;
            player.manaCost -= 0.08f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+15% magic damage, 8% reduced mana cost")
            {
                OverrideColor = ClairDeLuneCrimson
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Time itself flows through the eternal void'")
            {
                OverrideColor = ClairDeLuneBrass * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<JubilantArcaneCelebration>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 1: Starfall Harmonic Pendant (Nachtmusik + Dies Irae)

    /// <summary>
    /// Fusion Tier 1 Mage accessory - combines Nachtmusik and Dies Irae.
    /// Fuses stellar and infernal power.
    /// </summary>
    public class StarfallHarmonicPendant : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Common/Accessories/MageChain/StarfallCruciblePendant";

        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color FusionGold = new Color(255, 180, 80);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 130);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasStarfallHarmonicPendant = true;

            // Night bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Magic) += 0.20f;
            }

            player.statManaMax2 += 80;
            player.manaCost -= 0.10f;
            player.manaRegenBonus += 20;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses the power of Nachtmusik and Dies Irae")
            {
                OverrideColor = FusionGold
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% magic damage at night")
            {
                OverrideColor = NachtmusikPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+80 max mana, 10% reduced mana cost, accelerated mana regen")
            {
                OverrideColor = FusionGold
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Starfall and hellfire merge in harmonic destruction'")
            {
                OverrideColor = new Color(180, 120, 160) * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<NocturnalHarmonicOverflow>(1)
                .AddIngredient<InfernalManaCataclysm>(1)
                .AddIngredient<NachtmusikResonantEnergy>(10)
                .AddIngredient<DiesIraeResonantEnergy>(10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 2: Triumphant Overflow Pendant (+ Ode to Joy)

    /// <summary>
    /// Fusion Tier 2 Mage accessory - adds Ode to Joy to the fusion.
    /// Triple harmony of stellar, infernal, and jubilant power.
    /// </summary>
    public class TriumphantOverflowPendant : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        private static readonly Color FusionTriumph = new Color(255, 220, 160);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 160);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasTriumphantOverflowPendant = true;

            // Night bonus
            if (!Main.dayTime)
            {
                player.GetDamage(DamageClass.Magic) += 0.20f;
            }

            player.statManaMax2 += 120;
            player.manaCost -= 0.14f;
            player.manaRegenBonus += 30;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.012f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.6f, 0.75f);

            tooltips.Add(new TooltipLine(Mod, "FusionDesc", "Fuses Nachtmusik, Dies Irae, and Ode to Joy")
            {
                OverrideColor = FusionTriumph
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+20% magic damage at night")
            {
                OverrideColor = NachtmusikPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+120 max mana, 14% reduced mana cost, major mana regeneration")
            {
                OverrideColor = FusionTriumph
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Three harmonies unite in triumphant overflow'")
            {
                OverrideColor = titleColor * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<StarfallHarmonicPendant>(1)
                .AddIngredient<JubilantArcaneCelebration>(1)
                .AddIngredient<OdeToJoyResonantEnergy>(15)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion

    #region Fusion Tier 3: Pendant of the Eternal Overflow (Ultimate - + Clair de Lune)

    /// <summary>
    /// Ultimate Fusion Mage accessory - all four Post-Fate themes combined.
    /// The pinnacle of the mage accessory system.
    /// </summary>
    public class PendantOfTheEternalOverflow : ModItem
    {
        private static readonly Color NachtmusikPurple = new Color(100, 80, 180);
        private static readonly Color DiesIraeCrimson = new Color(200, 50, 50);
        private static readonly Color OdeToJoyRose = new Color(255, 180, 200);
        private static readonly Color ClairDeLuneBrass = new Color(200, 170, 100);
        private static readonly Color UltimatePrismatic = new Color(255, 230, 200);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 200);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var overflowPlayer = player.GetModPlayer<OverflowPlayer>();
            overflowPlayer.hasPendantOfTheEternalOverflow = true;

            player.statManaMax2 += 120;
            player.manaCost -= 0.15f;
            player.manaRegenBonus += 30;
            player.magicCuffs = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f * 0.2f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.7f, 0.85f);

            tooltips.Add(new TooltipLine(Mod, "UltimateDesc", "The ultimate fusion of all four Post-Fate themes")
            {
                OverrideColor = UltimatePrismatic
            });

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+30% magic damage")
            {
                OverrideColor = NachtmusikPurple
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+120 max mana, 15% reduced mana cost, damage taken restores mana")
            {
                OverrideColor = UltimatePrismatic
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Mana flows like an endless river of song'")
            {
                OverrideColor = ClairDeLuneBrass * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<TriumphantOverflowPendant>(1)
                .AddIngredient<EternalOverflowMastery>(1)
                .AddIngredient<ClairDeLuneResonantEnergy>(20)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    #endregion
}
