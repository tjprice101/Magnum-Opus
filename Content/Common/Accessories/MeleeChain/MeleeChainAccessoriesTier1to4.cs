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

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    #region Tier 1: Pre-Hardmode Foundation

    /// <summary>
    /// Resonant Rhythm Band - Entry-level melee accessory.
    /// Simple effect: +5% melee damage, +3% melee speed.
    /// </summary>
    public class ResonantRhythmBand : ModItem
    {
        private static readonly Color BasePurple = new Color(180, 130, 255);

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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasResonantRhythmBand = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+5% melee damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "+3% melee attack speed")
            {
                OverrideColor = new Color(200, 255, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The rhythm of battle begins with a single beat'")
            {
                OverrideColor = BasePurple * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCrystalShard>(10)
                .AddIngredient(ItemID.BandofRegeneration, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 2: Spring

    /// <summary>
    /// Spring Tempo Charm - Post-Primavera accessory.
    /// Simple effect: +10% melee speed, 5% chance to heal 1 HP on hit.
    /// No longer requires Tier 1.
    /// </summary>
    public class SpringTempoCharm : ModItem
    {
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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasSpringTempoCharm = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+10% melee attack speed")
            {
                OverrideColor = new Color(200, 255, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "5% chance to heal 1 HP on melee hit")
            {
                OverrideColor = SpringPink
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Spring awakens the tempo of new beginnings'")
            {
                OverrideColor = SpringPink * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantRhythmBand>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<VernalBar>(15)
                .AddIngredient<SpringResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 3: Summer

    /// <summary>
    /// Solar Crescendo Ring - Post-L'Estate accessory.
    /// Simple effect: Melee attacks inflict On Fire! for 5 seconds.
    /// No longer requires Tier 2.
    /// </summary>
    public class SolarCrescendoRing : ModItem
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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasSolarCrescendoRing = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Melee attacks inflict On Fire! for 5 seconds")
            {
                OverrideColor = SummerOrange
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The summer sun fuels an ever-rising crescendo'")
            {
                OverrideColor = SummerOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SpringTempoCharm>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<SolsticeBar>(15)
                .AddIngredient<SummerResonantEnergy>(1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    #endregion

    #region Tier 4: Autumn

    /// <summary>
    /// Harvest Rhythm Signet - Post-Autunno accessory.
    /// Simple effect: 2% lifesteal on melee attacks.
    /// No longer requires Tier 3.
    /// </summary>
    public class HarvestRhythmSignet : ModItem
    {
        private static readonly Color AutumnOrange = new Color(255, 100, 30);

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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasHarvestRhythmSignet = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "2% lifesteal on melee attacks")
            {
                OverrideColor = new Color(180, 255, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest reaps what the rhythm sows'")
            {
                OverrideColor = AutumnOrange * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<SolarCrescendoRing>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<HarvestBar>(20)
                .AddIngredient<AutumnResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion

    #region Tier 5: Winter

    /// <summary>
    /// Permafrost Cadence Seal - Post-L'Inverno accessory.
    /// Simple effect: 10% chance to freeze enemies for 1 second on melee hit.
    /// No longer requires Tier 4.
    /// </summary>
    public class PermafrostCadenceSeal : ModItem
    {
        private static readonly Color WinterBlue = new Color(150, 220, 255);

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
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasPermafrostCadenceSeal = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "10% chance to freeze enemies for 1 second")
            {
                OverrideColor = WinterBlue
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'Winter's cadence freezes time itself'")
            {
                OverrideColor = WinterBlue * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<HarvestRhythmSignet>(1)
                .AddIngredient<ResonantCrystalShard>(5)
                .AddIngredient<PermafrostBar>(25)
                .AddIngredient<WinterResonantEnergy>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Vivaldi's Tempo Master - Post-Plantera accessory (All Seasons combined).
    /// Simple effect: +12% melee damage, biome-dependent debuff on hit.
    /// Requires CycleOfSeasons, not previous tier.
    /// </summary>
    public class VivaldisTempoMaster : ModItem
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 20);
            Item.rare = ItemRarityID.Lime;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<ResonanceComboPlayer>();
            modPlayer.hasVivaldisTempoMaster = true;
        }

        private Color GetCurrentSeasonColor()
        {
            int season = (int)((Main.time / 15000) % 4);
            return season switch
            {
                0 => SpringPink,
                1 => SummerOrange,
                2 => AutumnBrown,
                _ => WinterBlue
            };
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float hue = (Main.GameUpdateCount * 0.01f) % 1f;
            Color titleColor = Main.hslToRgb(hue, 0.8f, 0.6f);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "+12% melee damage")
            {
                OverrideColor = new Color(255, 200, 200)
            });

            tooltips.Add(new TooltipLine(Mod, "Effect2", "Melee attacks inflict biome-based debuffs")
            {
                OverrideColor = titleColor
            });

            tooltips.Add(new TooltipLine(Mod, "BiomeNote", "Snow: Frostburn | Desert: On Fire! | Jungle: Poisoned | Other: Confused")
            {
                OverrideColor = new Color(180, 180, 180)
            });

            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Four Seasons unite under the maestro's baton'")
            {
                OverrideColor = GetCurrentSeasonColor() * 0.8f
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PermafrostCadenceSeal>(1)
                .AddIngredient<CycleOfSeasons>(1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    #endregion
}
